﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GushWeb.Helpers;
using GushWeb.Utility;

namespace GushWeb.ActionFilters
{
    public class AlarmnotesActionFilters : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var dt = DateTime.Today;

            var cookie = filterContext.HttpContext.Request.Cookies[ConfigEntity.NodeName];
            var cookies = CacheHelper.GetCache("token", () => XmlSetting.GetNodesByDate(ConfigEntity.NodeName, dt));

            bool isCheck = false;

            if (!cookies.IsNullOrEmpty() && cookie != null)
            {
                isCheck = cookies.Exists(d => d.Token == cookie.Value);
            }

            if (!isCheck)
                filterContext.Result = new RedirectResult("/Temptoken/Login");

        }
    }
}