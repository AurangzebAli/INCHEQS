using log4net;
using System.Web.Mvc;

namespace INCHEQS.ActionFilters {
    public class ExceptionHandlerAttribute : HandleErrorAttribute {
        
        public override void OnException(ExceptionContext filterContext) {
            // log error to NLog
            string controller = filterContext.RouteData.Values["controller"].ToString();
            string action = filterContext.RouteData.Values["action"].ToString();
            string loggerName = string.Format("{0}Controller.{1}", controller, action);

            LogManager.GetLogger(loggerName).Error(string.Empty, filterContext.Exception);
            base.OnException(filterContext);
        }
    }
}