using GushWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace GushWeb.Controllers
{
    public class MemberController : BaseController
    {
        readonly string pwSalt = ConfigurationManager.AppSettings["passwordSalt"];
        readonly string emailAccount = ConfigurationManager.AppSettings["emailAccount"];
        readonly string emailPwd = ConfigurationManager.AppSettings["emailPassword"];
        readonly string emailSmtp = ConfigurationManager.AppSettings["emailSmtp"];

        WebDBContext db = new WebDBContext();

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register([Bind(Exclude = "RegisterOn,AuthCode")]Member member)
        {
            var chk_member = db.Members.Where(p => p.Email == member.Email).FirstOrDefault();
            if (chk_member != null)
            {
                ModelState.AddModelError("Email", "Email has been registed");
            }
            if (ModelState.IsValid)
            {
                bool.TryParse(ConfigurationManager.AppSettings["isCheckEmail"], out bool isCheckEmail);
#pragma warning disable CS0618 // 类型或成员已过时
                member.Password = FormsAuthentication.HashPasswordForStoringInConfigFile(pwSalt + member.Password, "SHA1");
#pragma warning restore CS0618 // 类型或成员已过时
                member.ConfirmPassword = member.Password;
                member.RegisterOn = DateTime.Now;
                member.Id = Guid.NewGuid();
                var authCode = Guid.NewGuid().ToString("N");
                member.AuthCode = isCheckEmail ? authCode : null;
                db.Members.Add(member);
                db.SaveChanges();
                if (!String.IsNullOrWhiteSpace(authCode))
                    TempData["LastTempMessage"] = SendAuthCodeToMember(member)?"邮件已发送":"邮件验证错误";
                return RedirectToAction("Login", "Member");
            }
            else
                return View();
        }

        public ActionResult ValidateRegister(string id)
        {
            if (String.IsNullOrEmpty(id))
                return HttpNotFound();
            var member = db.Members.Where(p => p.AuthCode == id).FirstOrDefault();
            if (member != null)
            {
                TempData["LastTempMessage"] = "验证成功";
                member.ConfirmPassword = member.Password;
                member.AuthCode = null;
                db.SaveChanges();
            }
            else
            {
                TempData["LastTempMessage"] = "查无或验证过";
            }
            return RedirectToAction("Login", "Member");
        }
        [HttpGet]
        private bool SendAuthCodeToMember(Member member)
        {
            bool isSend = false;
            string filePath = Server.MapPath("~/App_Data/MemberRegisterEMailTemplate.htm");
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    SmtpClient SmtpServer = new SmtpClient(emailSmtp)
                    {
                        Port = 587,
                        Credentials = new System.Net.NetworkCredential(emailAccount, emailPwd),
                        EnableSsl = true
                    };

                    MailMessage mail = new MailMessage
                    {
                        From = new MailAddress(emailAccount)
                    };

                    mail.To.Add(member.Email);
                    mail.Subject = "Gush.com用户验证";
                    mail.Body = MailBody(filePath, member);
                    mail.IsBodyHtml = true;

                    SmtpServer.Send(mail);
                    isSend = true;
                }
                catch (Exception)
                {
                    isSend = false;
                }
            }
            return isSend;
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            if (ValidateUser(email, password))
            {
                FormsAuthentication.SetAuthCookie(email, false);
                if (String.IsNullOrWhiteSpace(returnUrl))
                    return RedirectToAction("Index", "Home");
                else
                    return Redirect(returnUrl);
            }
            ModelState.AddModelError("", "你输入的账号或者密码有误");
            return View();
        }
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        private bool ValidateUser(string email, string password)
        {
#pragma warning disable CS0618 // 类型或成员已过时
            var hash_pw = FormsAuthentication.HashPasswordForStoringInConfigFile(pwSalt + password, "SHA1");
#pragma warning restore CS0618 // 类型或成员已过时
            var member = (from p in db.Members where p.Email == email && p.Password == hash_pw select p).FirstOrDefault();
            if (member != null)
            {
                if (member.AuthCode == null)
                    return true;
                else
                {
                    ModelState.AddModelError("", "尚未通过验证，请通过邮件验证");
                    return false;
                }
            }
            else
            {
                ModelState.AddModelError("", "账号密码错误");
                return false;
            }
        }
        [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]   //清除缓存
        [HttpPost]
        public ActionResult CheckDup(string Email)
        {
            var member = db.Members.Where(p => p.Email == Email).FirstOrDefault();
            //return Json(member==null);
            return Json(false);
        }

        // GET: Member
        public ActionResult Index()
        {
            return View();
        }

        // GET: Member/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Member/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Member/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Member/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Member/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Member/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Member/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [ChildActionOnly]
        private string MailBody(string filePath, Member member)
        {
            string mailBody = System.IO.File.ReadAllText(filePath);
            mailBody = mailBody.Replace("{{Name}}", member.Name);
            mailBody = mailBody.Replace("{{RegisterOn}}", member.RegisterOn.ToString("F"));

            var auth_url = new UriBuilder(Request.Url)
            {
                Path = Url.Action("ValidateRegister", new { id = member.AuthCode }),
                Query = ""
            };
            mailBody = mailBody.Replace("{{AUTH_URL}}", auth_url.ToString());
            return mailBody;
        }
    }
}
