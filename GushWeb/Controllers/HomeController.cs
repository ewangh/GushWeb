using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GushWeb.Controllers
{
    public class HomeController : BaseController
    {
        //[Authorize(Roles = "")]
        public ActionResult Index()
        {
            ViewBag.Title = "Gush Com";
            return View();
        }

        //public ActionResult NotFound()
        //{
        //    Response.Status = "404 NotFound";
        //    Response.StatusCode = 404;
        //    return View();
        //}
    }
}
