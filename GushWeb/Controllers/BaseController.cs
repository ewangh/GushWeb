using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GushLibrary.Models;
using GushWeb.Utility;

namespace GushWeb.Controllers
{
    public class BaseController : Controller
    {
        protected string LastDate => new settlementService().GetLastDate();

        protected string Today
        {
            get
            {
                if (DateTime.Now.Hour < 9)
                {
                    return LastDate ?? DateTime.Today.ToYYYYMMDD();
                }

                return DateTime.Today.ToYYYYMMDD();
            }
        }

        protected string Time => DateTime.Now.ToLongTimeString();


        protected override void HandleUnknownAction(string actionName)
        {
            try
            {
                this.View("NotFound").ExecuteResult(this.ControllerContext);
            }
            catch (InvalidOperationException ieox)
            {
                ViewData["error"] = "Unknown Action:\"" + Server.HtmlEncode(actionName) + "\"";
                ViewData["exMessage"] = ieox.Message;
                this.View("Error").ExecuteResult(this.ControllerContext);
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            ViewData["error"] = filterContext.HttpContext.Response.StatusCode;
            ViewData["exMessage"] = filterContext.Exception.Message;
            this.View("Error").ExecuteResult(this.ControllerContext);
        }


    }
}