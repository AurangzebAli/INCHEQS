using Elmah;
//using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.UserSession;
using INCHEQS.Security.Resources;
using INCHEQS.Security;
using System;
using System.Web;
using System.Web.Mvc;
//using System.Web.Routing;
using INCHEQS.Common;
using INCHEQS.Helpers;

namespace INCHEQS.Security {
    public class CustomAuthorizeAttribute : AuthorizeAttribute {

        public string TaskIds { get; set; }


        //Refer http://stackoverflow.com/questions/31694082/asp-net-mvc5-dependency-injection-and-authorizeattribute/31696909#31696909
        private IUserSessionDao sessionDao {
            get {
                return DependencyResolver.Current.GetService<IUserSessionDao>();
            }
        }        
        private IAuditTrailDao auditTrailDao {
            get {
                return DependencyResolver.Current.GetService<IAuditTrailDao>();
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext) {
            
            if ((!string.IsNullOrEmpty(TaskIds))) {
                Roles = TaskIds;
            }

            filterContext.Controller.ViewBag.TaskIds = Roles;
            if ((CurrentUser.Account == null)) {
                //RejectAccessToLoginPage(filterContext, Locale.SessionTimeoutOrAnonymousAccessDenied); 
                 RejectAccessToLoginPage(filterContext, Locale.InvalidLogonSession);
            }
            else {
                AccountModel account = CurrentUser.Account;
                CustomPrincipal principal = new CustomPrincipal(account);
                string sessionId = HttpContext.Current.Session.SessionID;
                
                if ((!principal.IsInRole(Roles))) {
                    RemoveSessionForUser(account);
                    RejectAccessToLoginPage(filterContext, Locale.AccessDenied);
                }

                else if (sessionDao.HasManyAndNotAllowedConcurrent(account.UserId, sessionId)) {
                    RemoveSessionForUser(account);
                    //RejectAccessToLoginPage(filterContext, Locale.ConcurrentConnectionDenied); 
                    RejectAccessToLoginPage(filterContext, Locale.InvalidLogonSession);
                }                

                //Check for session update session
                else if(sessionDao.isLoginSessionExceededAndRefresh(account.UserId, sessionId)) {
                    RemoveSessionForUser(account);
                    RejectAccessToLoginPage(filterContext, Locale.SessionTimeout);
                    RejectAccessToLoginPage(filterContext, Locale.InvalidLogonSession);
                }

            }
        }

        //[AllowAnonymous]
        private void RejectAccessToLoginPage(AuthorizationContext filterContext, string errorMessage) {

            if (CurrentUser.Account != null) {
                string controller = filterContext.RouteData.Values["controller"].ToString();
                string action = filterContext.RouteData.Values["action"].ToString();

                auditTrailDao.Log(string.Format(errorMessage + ": for ({0}) at ({1}/{2}) with SessionID ({3})",
                    CurrentUser.Account.UserAbbr,
                    controller, action, HttpContext.Current.Session.SessionID), CurrentUser.Account);
            }

            RequestHelper.RejectAccessToLoginPage(filterContext, errorMessage);
            
        }



        private void RemoveSessionForUser(AccountModel account) {
            sessionDao.DeleteSessionForUser(account.UserId, HttpContext.Current.Session.SessionID);
            HttpContext.Current.Session.RemoveAll();

        }

        private void LogAccess(AuthorizationContext filterContext , AccountModel account) {
            string url = filterContext.HttpContext.Request.Url.PathAndQuery;
            string method = filterContext.HttpContext.Request.HttpMethod;
            string log = string.Format("Accessing : {0} - {1}", method , url);

            auditTrailDao.Log(log, account);
        }
    }

}