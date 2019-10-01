using GushWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GushWeb.Models;
using GushWeb.Utility;
using GushLibrary.Models;

namespace GushWeb.Controllers
{
    [Authorize]
    public class CentreController : BaseController
    {
        // GET: Centre
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UpdateToken()
        {
            CacheHelper.Clear();
            var cookies = CacheHelper.GetCache("token", () => XmlSetting.GetNodes(ConfigEntity.NodeName));
            return PartialView("pviewTokenContent", cookies);
        }
    }
}