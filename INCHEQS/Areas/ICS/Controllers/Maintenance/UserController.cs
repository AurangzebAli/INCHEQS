using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
   
    public class UserController : BaseController {

        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;

        public UserController(IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.User.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.User.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.User.INDEX, "View_ApprovedUser", "fldUserAbb", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string id = null) {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            if (!string.IsNullOrEmpty(id)) {
                ViewBag.User = userDao.GetUser(id);
            } else {
                ViewBag.User = userDao.GetUser(filter["fldUserId"]);
            }
            ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
            ViewBag.City = userDao.ListCity();
            ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode,@ViewBag.User.fldUserAbb);
            ViewBag.GetOfficer = userDao.GetOfficerId(@ViewBag.User.fldUserAbb);
            ViewBag.BranchAvailableList = userDao.ListAvailableBranch(@ViewBag.User.fldUserAbb);
            ViewBag.BranchSelectedList = userDao.ListSelectedBranch(CurrentUser.Account.BankCode,@ViewBag.User.fldUserAbb);
            ViewBag.VerificationClass = userDao.ListVerificationClass();
            ViewBag.Security = securityProfileDao.GetSecurityProfile();

            //Check if LoginAD "Y" to display password
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            try {
                List<String> errorMessages = userDao.ValidateUser(col, "Update", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    UserModel before = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit User - Before Update=> User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Email : " + before.fldEmail + " Branch Code : " + before.fldBranchCode + " Verification Limit : " + before.fldVerificationLimit + " City : " + before.fldCity + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                    //update Process
                    userDao.UpdateUser(col, CurrentUser.Account.BankCode);
                    userDao.InsertUserDedicatedBranch(col,CurrentUser.Account.BankCode);
                    userDao.UpdateUserDedicatedBranch(col["fldUserAbb"]);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;

                    UserModel after = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit User - After Update=> User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Email : " + after.fldEmail + " Branch Code : " + after.fldBranchCode + " Verification Limit : " + after.fldVerificationLimit + " City : " + after.fldCity + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;
                }

                return RedirectToAction("Edit", new { id = col["fldUserId"] });
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col) {
            //Get value from system profile
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
            try {

                if ((col["deleteBox"]) != null) {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults) {
                        if (userDao.CheckUserExistInTempUser(arrResult) == false) {
                            if ("N".Equals(systemProfile)) {
                                userDao.DeleteInUserMaster(arrResult);
                                TempData["Notice"] = Locale.SuccessfullyDeleted;
                            } else {
                                userDao.AddUserToUserMasterTempToDelete(arrResult);
                                TempData["Notice"] = Locale.UserSuccessfullyAddedtoApproved;
                            }
                        }
                    }

                    //audittrail
                    if ("N".Equals(systemProfile)) {
                        auditTrailDao.Log("Delete - User Id: " + col["deleteBox"], CurrentUser.Account);
                    } else {
                        auditTrailDao.Log("Add into Temporary record to Delete - User Id: " + col["deleteBox"], CurrentUser.Account);
                    }

                } else {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            String x = null;
            String y = null;
            ViewBag.Bank = CurrentUser.Account;
            ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
            ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode,CurrentUser.Account.UserAbbr);
            ViewBag.BranchAvailableList = userDao.ListAvailableBranch(CurrentUser.Account.BankCode);
            ViewBag.BranchSelectedList = userDao.ListSelectedBranch(CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
            ViewBag.VerificationClass = userDao.ListVerificationClass();
            ViewBag.City = userDao.ListCity();

            //Check if LoginAD "Y" to display password
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            //Get value from system profile
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();

            try {
                UserModel user = new UserModel();
                List<string> errorMessages = userDao.ValidateUser(col, "Create", CurrentUser.Account.UserId);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    if ("N".Equals(systemProfile)) {
                        userDao.InsertUserDedicatedBranch(col, CurrentUser.Account.BankCode);
                        userDao.UpdateUserDedicatedBranch(col["fldUserAbb"]);
                        userDao.CreateUserToUserTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                        userDao.CreateInUserMaster(col["fldUserAbb"]);
                        userDao.DeleteInUserMasterTemp(col["fldUserAbb"]);
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - User Abb: " + col["fldUserAbb"], CurrentUser.Account);
                    } else {
                        userDao.CreateUserToUserTemp(col,CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                        userDao.InsertUserDedicatedBranch(col, CurrentUser.Account.BankCode);
                        TempData["Notice"] = Locale.UserSuccessfullyCreated;
                        auditTrailDao.Log("Add into Temporary record to Create - User Abb: " + col["fldUserAbb"], CurrentUser.Account);
                    }

                }
                return RedirectToAction("Create");
            } catch (Exception ex) {
                throw ex;
            }
        }
    }
}