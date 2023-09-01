
using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using INCHEQS.Common;

namespace INCHEQS {
    public class MvcApplication : HttpApplication {
        protected void Application_Start() {

            ViewEngines.Engines.Clear();
            //IViewEngine razorEngine = new RazorViewEngine() { FileExtensions = new string[] { "cshtml" } };
            IViewEngine razorEngine = new CSharpRazorViewEngine() { FileExtensions = new string[] { "cshtml" } };
            ViewEngines.Engines.Add(razorEngine);


            UnityConfig.RegisterComponents();
            LogConfig.RegisterLog();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
            //Application["default"] = ConfigureSetting.GetConnectionString("default");
            //Application["ics_connection_string"] = ConfigureSetting.GetConnectionString("ics_connection_string");
            //Application["ocs_connection_string"] = ConfigureSetting.GetConnectionString("ocs_connection_string");
            //Application["sds_connection_string"] = ConfigureSetting.GetConnectionString("sds_connection_string");
            //Application["rpa_connection_string"] = ConfigureSetting.GetConnectionString("rpa_connection_string");
            //Application["atv_connection_string"] = ConfigureSetting.GetConnectionString("atv_connection_string");
        }

        //ViewEngine to search for cshtml only
        public class CSharpRazorViewEngine : RazorViewEngine {
            public CSharpRazorViewEngine() {
                AreaViewLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                AreaMasterLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                AreaPartialViewLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                ViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml"
                };
                MasterLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml"
                };
                PartialViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml"
                };
            }
        }
    }
}
