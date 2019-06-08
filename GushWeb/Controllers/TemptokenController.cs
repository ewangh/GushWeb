using GushWeb.ActionFilters;
using GushWeb.Models;
using GushWeb.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace GushWeb.Controllers
{

    public class TemptokenController : BaseController
    {
        readonly string pwSalt = ConfigurationManager.AppSettings["passwordSalt"];
        readonly string emailAccount = ConfigurationManager.AppSettings["emailAccount"];
        readonly string emailPwd = ConfigurationManager.AppSettings["emailPassword"];
        readonly string emailSmtp = ConfigurationManager.AppSettings["emailSmtp"];
        
        // GET: Temptoken
        //[TempTokenActionFilters]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "Token")]TempToken tokenObj)
        {
            if (ModelState.IsValid)
            {
                string nodeName = "tempTokens";
                var dt = DateTime.Today;
                var cookies = XmlSetting.GetNodes(nodeName, dt).ConvertAll(d => d.Token);
                if (!cookies.IsNullOrEmpty() && cookies.Contains(tokenObj.Token))
                {
                    Response.Cookies[nodeName].Value = tokenObj.Token;
                    return RedirectToAction("Index", "Alarmnotes", new {date = Today});
                }

                ModelState.AddModelError("token", "Error Token");
            }

            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Email")]TempToken tokenObj)
        {
            if (ModelState.IsValid)
            {
                //if (!String.IsNullOrWhiteSpace(email))
                //    TempData["LastTempMessage"] = SendAuthCodeToMember(email) ? "邮件已发送" : "邮件验证错误";
                return RedirectToAction("Login", "Temptoken");
            }
            else
                return View();
        }

        //private bool SendAuthCodeToMember(string email)
        //{
        //    bool isSend = false;
        //    string filePath = Server.MapPath("~/App_Data/TempTokenRegisterEMailTemplate.htm");
        //    if (System.IO.File.Exists(filePath))
        //    {
        //        try
        //        {
        //            SmtpClient SmtpServer = new SmtpClient(emailSmtp)
        //            {
        //                Port = 587,
        //                Credentials = new System.Net.NetworkCredential(emailAccount, emailPwd),
        //                EnableSsl = true
        //            };

        //            MailMessage mail = new MailMessage
        //            {
        //                From = new MailAddress(emailAccount)
        //            };
        //            var dt = DateTime.Today;
        //            var cookies = XmlSetting.GetNodes(nodeName, dt);
        //            if (cookies.IsNullOrEmpty())
        //                isSend = false;
        //            else
        //            {
        //                Random rd = new Random();
        //                var i = rd.Next(1, cookies.Count);

        //                var tkObj = cookies.ToArray()[i];

        //                mail.To.Add(email);
        //                mail.Subject = "Gush.com用户验证";
        //                mail.Body = MailBody(filePath, tkObj);
        //                mail.IsBodyHtml = true;

        //                SmtpServer.Send(mail);
        //                isSend = true;

        //            }

        //        }
        //        catch (Exception)
        //        {
        //            isSend = false;
        //        }
        //    }
        //    return isSend;
        //}

        //[ChildActionOnly]
        //private string MailBody(string filePath, TempToken tkObj)
        //{
        //    string mailBody = System.IO.File.ReadAllText(filePath);
        //    mailBody = mailBody.Replace("{{Token}}", tkObj.Token);
        //    mailBody = mailBody.Replace("{{ExpireDate}}", tkObj.ExpireDate.ToYYYYMMDD());

        //    var auth_url = new UriBuilder(Request.Url)
        //    {
        //        Path = Url.Action("ValidateRegister", new { id = member.AuthCode }),
        //        Query = ""
        //    };
        //    mailBody = mailBody.Replace("{{AUTH_URL}}", auth_url.ToString());
        //    return mailBody;
        //}

        //public ActionResult ValidateRegister(string id)
        //{
        //    if (String.IsNullOrEmpty(id))
        //        return HttpNotFound();
        //    var member = db.Members.Where(p => p.AuthCode == id).FirstOrDefault();
        //    if (member != null)
        //    {
        //        TempData["LastTempMessage"] = "验证成功";
        //        member.ConfirmPassword = member.Password;
        //        member.AuthCode = null;
        //        db.SaveChanges();
        //    }
        //    else
        //    {
        //        TempData["LastTempMessage"] = "查无或验证过";
        //    }
        //    return RedirectToAction("Login", "Member");
        //}
    }
}