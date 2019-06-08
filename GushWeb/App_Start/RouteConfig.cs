using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GushWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",//指定路由名
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "TempToken", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
