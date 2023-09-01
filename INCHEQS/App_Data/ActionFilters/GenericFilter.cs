using INCHEQS.Common;
using INCHEQS.Helpers;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS {
    public class GenericFilter : ActionFilterAttribute {

        public string MainMenuTitle { get; set; }        
        public bool AllowHttpGet { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext) {
            filterContext.Controller.ViewBag.CurrentUser = CurrentUser.Account;       
            filterContext.Controller.ViewBag.MainMenuTitle = this.MainMenuTitle;
            filterContext.Controller.ViewBag.DateFormat = DateUtils.ConvertDateFormatToJQuery(ConfigureSetting.GetDateFormat());
            filterContext.Controller.ViewBag.IsExtended = true;
            //Check if action does not have CustomAuthorizeAttribute attr

           
            if (!AllowHttpGet)
            {
                if (filterContext.HttpContext.Request.HttpMethod == "GET")
                {
                    RequestHelper.RejectAccessToLoginPage(filterContext, Locale.OnlyPOSTMethodAllowed);
                }
            }
        }   

        
    }
}