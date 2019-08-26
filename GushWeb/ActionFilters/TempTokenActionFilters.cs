using GushWeb.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GushWeb.ActionFilters
{
    public class TempTokenActionFilters:ActionFilterAttribute
    {
        private readonly string nodeName = ConfigEntity.NodeName;
        private readonly string tknSalt = ConfigEntity.tknSalt;
        private readonly string redirectToAction = "/Alarmnotes/Index?date={0}";
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dt = DateTime.Today;
            var cookies = XmlSetting.GetNodes(nodeName,dt).ConvertAll(d=>d.Token.ToSalt(tknSalt));
            var cuttentCookie = filterContext.HttpContext.Request.Cookies[nodeName];

            bool isCheck = false;

            if (!cookies.IsNullOrEmpty() && cuttentCookie != null)
            {
                var ck = cuttentCookie.Value.ToSalt(tknSalt);
                if (cookies.Contains(ck))
                    isCheck = true;
            }

            if (isCheck)
                filterContext.Result = new RedirectResult(string.Format(redirectToAction,DateTime.Today.ToYYYYMMDD()));
            
        }
    }
}