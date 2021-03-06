﻿using System.Web;
using System.Web.Optimization;

namespace GushWeb
{
    public class BundleConfig
    {
        // 有关捆绑的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备就绪，请使用 https://modernizr.com 上的生成工具仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/runoob/bootstrap_new").Include(
                "~/Scripts/runoob/jquery-3.3.1.min.js",
                "~/Scripts/runoob/bootstrap.min.js"
                ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-select.min.css",
                      "~/Content/PagedList.css",
                      "~/Content/site.css",
                      "~/Content/ownStyle.css"));

            bundles.Add(new StyleBundle("~/runoob/css").Include(
                "~/Content/runoob/bootstrap.min.css",
                "~/Content/runoob/layoutit.css"));

            bundles.Add(new StyleBundle("~/bundles/echarts").Include(
                      "~/Scripts/echarts.min.js"));

            bundles.Add(new StyleBundle("~/bundles/jquery.unobtrusive-ajax.min").Include(
                      "~/Scripts/echarts.min.js"));
        }
    }
}
