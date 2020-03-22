using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GushWeb
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<Models.WebDBContext>());

            MvcHandler.DisableMvcResponseHeader = true;//隐藏标头MVC版本
            RouteTable.Routes.RouteExistingFiles = true;//先比较路由规则

            AreaRegistration.RegisterAllAreas();//区域路由
            
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            ViewEngines.Engines.Add(new WebFormViewEngine());
        }

        /// <summary>
        /// 重写Application_PostAuthorizeRequest
        /// </summary>
        protected void Application_PostAuthorizeRequest()
        {
            //对Session的支持，不然运行调用会直接异常
            HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
        }
    }
}
