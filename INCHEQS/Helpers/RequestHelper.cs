using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace INCHEQS.Helpers {
    public class RequestHelper {

        public static void RestrictAccessToUserBasedOnTaskId(ControllerContext controllerContext, string taskId) {
            
            if (!CurrentUser.HasTask(taskId)) {
                RejectUserToLoginPage(controllerContext);
                throw new HttpException(Locale.AccessDenied);
            }
            controllerContext.Controller.ViewBag.TaskIds = taskId;
        }

        public static void RejectUserToLoginPage(ControllerContext filterContext) {

            //REVISIT
            //UserSessionDao sessionDao = new UserSessionDao();
            //sessionDao.DeleteSessionForUser(CurrentUser.Account.UserId, HttpContext.Current.Session.SessionID);
            HttpContext.Current.Session.RemoveAll();
            //If request from AJAX, return 401
            if ((filterContext.HttpContext.Request.IsAjaxRequest())) {
                filterContext.HttpContext.Response.StatusCode = 401;
                filterContext.HttpContext.Response.End();
            }
        }

        public static string PersistQueryStringForActions(ControllerContext controllerContext , string queryString) {
            string result = "";
            if (controllerContext.HttpContext.Request.QueryString[queryString] != null) {
                result = (string) controllerContext.HttpContext.Request.QueryString[queryString];
            } else if (controllerContext.Controller.TempData.ContainsKey(queryString)) {
                result = (string) controllerContext.Controller.TempData[queryString];
                controllerContext.Controller.TempData.Keep();
            }
            //ViewBag to be used in View
            controllerContext.Controller.ViewData[queryString] = result;
            //TempData to be used temporarily for the next Action by this own function
            controllerContext.Controller.TempData[queryString] = result;
            return result;
        }

        [AllowAnonymous]
        public static void RejectAccessToLoginPage(ControllerContext filterContext, string errorMessage = "Locale.AccessDenied") {

            UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);


            if (CurrentUser.Account != null) {
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();

                //AuditTrailDao.Log(string.Format(errorMessage + ": for ({0}) at ({1}/{2}) with SessionID ({3})", CurrentUser.Account.UserAbbr, controller, action, HttpContext.Current.Session.SessionID));
            }

            //If request from AJAX, return 401
            if ((filterContext.HttpContext.Request.IsAjaxRequest())) {
                if(filterContext.HttpContext.Response.StatusCode != 401) { 
                    filterContext.HttpContext.Response.StatusCode = 401;
                }
                filterContext.HttpContext.Response.End();
            } else {

                //Redirect to login page
                
                filterContext.HttpContext.Response.Redirect(urlHelper.Action("Index" , "Login", new { area = ""}) +"?error=" + errorMessage);
                
            }
            //throw new HttpException(errorMessage);
            filterContext.HttpContext.Response.End();
        }

    }
}