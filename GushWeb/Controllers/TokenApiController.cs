using GushWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace GushWeb.Controllers
{
    [AllowAnonymous]
    public class TokenApiController : BaseApiController
    {
        [HttpGet]
        public object UserLogin(string uName, string uPassword)
        {
            WebDBContext db = new WebDBContext();
            var user = db.Members.Where(x => x.Name == uName && x.Password == uPassword).FirstOrDefault();
            if (user == null)
            {
                return Json(new { ret = 0, data = "", msg = "用户名密码错误" });
            }
            return GetToken(uName, uPassword);
        }

        [HttpGet]
        public object TempLogin()
        {
            string temp = Guid.NewGuid().ToString("N");
            return GetToken(temp, temp);
        }

        private object GetToken(string uName, string uPassword)
        {
            FormsAuthenticationTicket token = new FormsAuthenticationTicket(0, uName, DateTime.Now, DateTime.Now.AddHours(12), true, $"{uName}&{uPassword}", FormsAuthentication.FormsCookiePath);
            //返回登录结果、用户信息、用户验证票据信息
            var _token = FormsAuthentication.Encrypt(token);
            //将身份信息保存在session中，验证当前请求是否是有效请求
            LoginID = uName;
            TokenValue = _token;
            HttpContext.Current.Session[LoginID] = _token;
            return Json(new { ret = 1, data = _token, msg = "登录成功！" });
        }
    }
}
