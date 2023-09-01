//using ICSSecurity;
//using InsiteMYLicensing;
//using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Account;
using INCHEQS.Security.UserSession;
using INCHEQS.Security;
using INCHEQS.ViewModels;
using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using log4net;
using System.Web.Routing;
using INCHEQS.Security.User;
using System.Collections.Generic;
using INCHEQS.Security.Password;
using INCHEQS.Resources;
using System.Threading.Tasks;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Common;
using INCHEQS.TaskAssignment;
//using INCHEQS.ViewModel;

namespace INCHEQS.Controllers {

    //[RequireHttps]
    public class LoginController : Controller {
       // private static readonly ILog _log = LogManager.GetLogger(typeof(LoginController));

        private readonly IUserSessionDao userSessionDao;
        private readonly ILoginAccountService loginAccountService;
        private readonly IUserDao userDao;
        private readonly IPasswordDao passwordDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;

        public LoginController(IUserSessionDao userSessionDao, ILoginAccountService loginAccountService , 
            IUserDao userDao, IPasswordDao passwordDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao) {
            this.userSessionDao = userSessionDao;
            this.loginAccountService = loginAccountService;
            this.userDao = userDao;
            this.passwordDao = passwordDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        // GET: Login
        [AllowAnonymous()]
        public ActionResult Index( string error = null) {

            bool log4netIsConfigured = log4net.LogManager.GetRepository() .Configured;
            InsiteMYLicensing.ProductLicense InsiteMyLicense = new InsiteMYLicensing.ProductLicense();
            //long ExpDate = InsiteMyLicense.getExpiryDays("ICS");
            int ExpDate = 99;
            if (ExpDate < 15)
            {
                error = "License will expire within " + ExpDate + " Day(s) ";
            }
            else if (ExpDate == 0)
            {
                TempData["LoginError"] = "License already expired ";
            }
            //Set Culture here
            string culture = ConfigurationManager.AppSettings["ApplicationCulture"];
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

            //Check if LoginAD "Y" to display select l  ist
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            ViewBag.UserAuthMethod = loginAccountService.ListUserAuthMethod();
            ViewBag.SystemVersion = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("SystemVersion").Trim();
            if (error != null) {
                TempData["LoginError"] = error;
                TempData[@Locale.LoginError] = error;
            }

            return View();
        }

        [HttpGet()]
        [AllowAnonymous()]
        public ActionResult Login() {
            //Check if LoginAD "Y" to display select list
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            return RedirectToAction("Index?tId=999930");
        }


        [HttpPost()]
        [AllowAnonymous()]
        [ValidateAntiForgeryToken()]
        public async Task<ActionResult> Login(LoginViewModel avm) {


            //Check if LoginAD "Y" to display select list
            AccountModel accounts = await loginAccountService.setLofFilePathAsync();
            CurrentUser.SetAuthenticatedSession(accounts);
            systemProfileDao.Log(DateTime.Now +": Start log in User :" + avm.UserAbbr , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            ViewBag.SystemVersion = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("SystemVersion").Trim();
            systemProfileDao.Log(DateTime.Now + ": Get AD status User :" + avm.UserAbbr , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            string macAddress = Utils.getMacAddress();
            string remoteAddress = Utils.getRemoteAddress(this.Request);
            Dictionary<string, string> errors = new Dictionary<string, string>();
            string sessionId = Session.SessionID;
            ViewBag.UserAuthMethod = loginAccountService.ListUserAuthMethod();
            if (string.IsNullOrEmpty(avm.UserAbbr) | string.IsNullOrEmpty(avm.UserPassword)) {
                ViewBag.Error = Locale.PleasekeyinUserIDandPassword;
                TempData["LoginError"] = Locale.PleasekeyinUserIDandPassword;
                return View("Index");
            }
            UserModel user = new UserModel();

            try {
                systemProfileDao.Log(DateTime.Now +": Get product license User :" + avm.UserAbbr , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                InsiteMYLicensing.ProductLicense InsiteMyLicense = new InsiteMYLicensing.ProductLicense();
                systemProfileDao.Log(DateTime.Now +": Successfully get license User: " + avm.UserAbbr , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                int ExpDate = 99;// hardcode remember to add license dll //InsiteMyLicense.getExpiryDays("ICS");
                //long ExpDate = InsiteMyLicense.getExpiryDays("ICS");
                systemProfileDao.Log(DateTime.Now +": Validate product licencse User :" + avm.UserAbbr , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                if (ExpDate != 0)
                {
                    systemProfileDao.Log(DateTime.Now +": Get user abb User :" + avm.UserAbbr, avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    user = await loginAccountService.GetUser(avm.UserAbbr, avm.macAddress);
                    systemProfileDao.Log(DateTime.Now +": Validate user id and password User :" + avm.UserAbbr, avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    //await
                    errors =  loginAccountService.ValidateLogin(user, avm.UserPassword, sessionId, remoteAddress, avm.SelectLoginAD, avm.Domain);
                    systemProfileDao.Log(DateTime.Now +": Finish validate userid and password Error(if any):" , avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    //Login loginObject = new Login();
                   // loginObject.SetPasswordXML = ConfigurationManager.AppSettings["EncryptionPath"];
                    //loginObject.loginValidation(avm.UserAbbr, avm.UserPassword, sessionId, remoteAddress, DatabaseUtils.ConnectionString);

                    //Success
                    if (errors.Count == 0)
                    {
                        ViewBag.UserAuthMethod = loginAccountService.ListUserAuthMethod();
                        systemProfileDao.Log(DateTime.Now +": Assign user to current user", avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        AccountModel account = await loginAccountService.convertUserModelToAccountModelAsync(user);
                        CurrentUser.SetAuthenticatedSession(account);

                        //update counter to 0
                        systemProfileDao.Log(DateTime.Now + ": Update login counter", avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        userDao.updateLoginCounterForUser(user.fldUserAbb, 0, "N", true);
                        systemProfileDao.Log(DateTime.Now + ": Update last login date", avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        userDao.updateLastLoginDate(user.fldUserId);
                        systemProfileDao.Log(DateTime.Now + ": Update user session", avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        userSessionDao.updateSessionTrack(user.fldUserId, sessionId);
                        systemProfileDao.Log(DateTime.Now + ": Sucessfully log in User : " + avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        TempData["Notice"] = Locale.Youhavebeensuccessfullyloggedin;
                        auditTrailDao.Log("User " + CurrentUser.Account.UserAbbr + " Successfully log in - " + Utils.getRemoteAddress(Request) + "", CurrentUser.Account);
                        //auditTrailDao.SecurityLog("User " + CurrentUser.Account.UserAbbr + " Successfully log in - " + Utils.getRemoteAddress(Request) + "","", TaskIdsOCS.Login, CurrentUser.Account);
                        return new RedirectToRouteResult(new RouteValueDictionary(new
                        {

                            area = "COMMON",
                            controller = "Main",
                            action = "Index"
                        }));

                    }
                    else
                    {
                        ViewBag.UserAuthMethod = loginAccountService.ListUserAuthMethod();
                        TempData["LoginError"] = errors;
                        if (errors.ContainsKey("PASSWORDEXPIRED"))
                        {
                            avm.UserId = user.fldUserId;
                            passwordDao.DeleteUserSessionTrack(CurrentUser.Account.UserId);
                            return View("ChangePassword", avm);
                        }

                        //This line below throws generic error to prevent from Account Harvesting.
                        //errors = new Dictionary<string, string>() { { "GenericError", Locale.LoginError } };

                       
                        systemProfileDao.Log(DateTime.Now +": Error ValidateUser: " + errors, avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    }
                }
                else
                {
                    TempData["LoginError"] = "License already expired ";
                    systemProfileDao.Log(DateTime.Now +": Error : License already expired", avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                }
                //TempData["ErrorMsg"] = errors;
            } catch (Exception e) {
                TempData["LoginError"] = e.Message;
                systemProfileDao.Log(DateTime.Now +": LoginController/Login error:" + e.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                systemProfileDao.Log(DateTime.Now +": Error Exception: " + e, avm.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            }
            //ModelState.Clear();
           
            return View("Index");
        }

        // GET: Logout
        public ActionResult Logout() {
            auditTrailDao.Log("Successfully log out - " + Utils.getRemoteAddress(Request) + "", CurrentUser.Account);
            //auditTrailDao.SecurityLog("Successfully log out - " + Utils.getRemoteAddress(Request) + "", "", TaskIdsOCS.Logout, CurrentUser.Account);
            userSessionDao.DeleteSessionForUser(CurrentUser.Account.UserId, Session.SessionID);

            Session.RemoveAll();

            TempData["Notice"] = Locale.Youhavebeensuccessfullyloggedout;
            return new RedirectToRouteResult(new RouteValueDictionary(new {
                controller = "Login",
                action = "Index"
            }));
        }

        [HttpGet]
        [AllowAnonymous()]        
        public ActionResult UpdatePassword() {
            return RedirectToAction("Index");

        }


        [HttpPost]
        [AllowAnonymous()]
        [ValidateAntiForgeryToken()]
        public ActionResult UpdatePassword(LoginViewModel avm ,FormCollection col) {

            try {
                string userId = avm.UserId;

                if (string.IsNullOrEmpty(avm.UserAbbr) | string.IsNullOrEmpty(avm.OldPassword) | string.IsNullOrEmpty(avm.NewPassword) | string.IsNullOrEmpty(avm.ConfirmNewPassword)) {
                    TempData["LoginError"] = Locale.PleaseInsertAllFields;                    
                    return View("ChangePassword" , avm);
                }

                UserModel userModel = userDao.GetUser(avm.UserId);
                List<string> errorMessages = passwordDao.Validate(userModel.fldUserId, userModel.fldPassword, avm.OldPassword, avm.NewPassword, avm.ConfirmNewPassword);

                if ((errorMessages.Count > 0)) {
                    foreach (string inventoryItem in errorMessages)
                    {
                        TempData["LoginError"] = inventoryItem;
                        return View("ChangePassword", avm);
                    }

                } else {
                    Session.RemoveAll();
                    passwordDao.UpdatePwd(userId, userModel.fldPasswordExpDate,  avm.OldPassword, avm.NewPassword);
                    passwordDao.AddPwdHistory(userId, avm.NewPassword);
                    auditTrailDao.Log("Change Password - User ID : " + CurrentUser.Account.UserId, CurrentUser.Account);
                    //auditTrailDao.SecurityLog("Change Password - User ID : " + CurrentUser.Account.UserId, "", TaskIds.ChangePassword.INDEX, CurrentUser.Account);
                    passwordDao.DeleteUserSessionTrack(userId);
                    TempData["Notice"] = Locale.PasswordChangesuccessfully;
                }


            } catch (Exception ex) {
                throw ex;
            }
            return new RedirectToRouteResult(new RouteValueDictionary(new {
                controller = "Login",
                action = "Index",
                notice = Locale.Passwordissuccessfullyupdated
            }));
        }


    }
}