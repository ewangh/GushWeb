using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GushWeb.Controllers
{
    public class OrderController : BaseController
    {
        // GET: Order
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Complete()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Complete(FormCollection form)
        {
            return RedirectToAction("Index", "Home");
        }
    }
}