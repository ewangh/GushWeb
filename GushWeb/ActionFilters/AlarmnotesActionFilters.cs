using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GushWeb.Utility;

namespace GushWeb.ActionFilters
{
    public class AlarmnotesActionFilters:ActionFilterAttribute
    {
        private readonly string nodeName = "tempTokens";
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dt = DateTime.Today;
            var cookies = XmlSetting.GetNodes(nodeName,dt).ConvertAll(d=>d.Token);
            var cookie = filterContext.HttpContext.Request.Cookies[nodeName];

            bool isCheck = false;

            if (!cookies.IsNullOrEmpty() && cookie != null)
            {
                var theCookie = cookie.Value;
                if (cookies.Contains(theCookie))
                    isCheck = true;
            }

            if (!isCheck)
                filterContext.Result = new RedirectResult("/Temptoken/Login");
            
        }
    }
}