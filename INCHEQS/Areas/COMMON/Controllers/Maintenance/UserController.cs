using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
//using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.Users;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class UserController : BaseController
    {

        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        private static readonly ILog _log = LogManager.GetLogger(typeof(UserController));
        public UserController(IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

        }

        [CustomAuthorize(TaskIds = TaskIds.User.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.User.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.User.INDEX, "View_ApprovedUser", "fldUserAbb", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.User.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string id = null)
        {

            try
            {

                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                if (!string.IsNullOrEmpty(id))
                {
                    ViewBag.User = userDao.CheckUserMasterByID("", id, "UserId");
                }
                else
                {
                    ViewBag.User = userDao.CheckUserMasterByID("", filter["fldUserId"], "UserId");
                }

                ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
                ViewBag.VerificationClass = userDao.ListVerificationClass(CurrentUser.Account.BankCode);
                //ViewBag.VerificationLimit = systemProfileDao.GetValueFromSystemProfile("VerificationLimit", CurrentUser.Account.BankCode).Trim();
                //ViewBag.City = userDao.ListCity();
                //ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode, @ViewBag.User.fldUserAbb);
                //ViewBag.GetOfficer = userDao.GetOfficerId(@ViewBag.User.fldUserAbb);
                //ViewBag.BranchAvailableList = userDao.ListAvailableBranch(@ViewBag.User.fldUserAbb);
                //ViewBag.BranchSelectedList = userDao.ListSelectedBranch(CurrentUser.Account.BankCode, @ViewBag.User.fldUserAbb);
                //ViewBag.VerificationClass = userDao.ListVerificationClass();
                //ViewBag.Security = securityProfileDao.GetSecurityProfile();

                //Check if LoginAD "Y" to display password
                //ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                CurrentUser.Account.Status = "U";
                CurrentUser.Account.TaskId = TaskIds.User.INDEX;

                //string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
                string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

                List<String> errorMessages = new List<string>();
                    //userDao.ValidateUser(col, "Update", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if (userDao.CheckUserMasterTempByID(col["fldUserId"], "", "UserId") == false)
                    {
                        //if ("N".Equals(systemProfile))
                        if ("N".Equals(securityProfile))
                        {
                            UserModel before = userDao.CheckUserMasterByID("", col["fldUserId"], "UserId");
                            auditTrailDao.Log("Before Update => User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType + " Disable : " + before.fldDisableLogin + " Verification Limit : " + before.fldVerificationLimit + " Verification Class : " + before.fldVerificationClass + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                            //update Process
                            userDao.UpdateUserMaster(col, col["fldUserId"]);

                            UserModel after = userDao.CheckUserMasterByID("", col["fldUserId"], "UserId");
                            auditTrailDao.Log("After Update => User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Branch Code : " + after.fldBranchCode + " User Type : " + after.userType + " Disable : " + after.fldDisableLogin + " Verification Limit : " + after.fldVerificationLimit + " Verification Class : " + after.fldVerificationClass + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);

                            string ActionDetail = SecurityAuditLogDao.UserProfile_EditTemplate(before, after, "Edit", col);
                            auditTrailDao.SecurityLog("Edit User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);
                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                        else
                        {
                            UserModel before = userDao.CheckUserMasterByID("", col["fldUserId"], "UserId");

                            userDao.CreateUserMasterTemp(col, CurrentUser.Account.BankCode, col["fldUserId"], "Update");
                            TempData["Notice"] = Locale.UserSuccessfullyAddedtoApprovedUpdate;
                            auditTrailDao.Log("Before Update => User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType + " Disable : " + before.fldDisableLogin + " Verification Limit : " + before.fldVerificationLimit + " Verification Class : " + before.fldVerificationClass + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);
                            //auditTrailDao.Log("1User Record Successfully Added to Temp Table for Check to Update . User Id : " + col["fldUserId"], CurrentUser.Account);

                            UserModel after = SecurityAuditLogDao.CheckUserMasterByTempID("", col["fldUserId"], "UserId");
                            string ActionDetail = SecurityAuditLogDao.UserProfile_EditTemplate(before, after, "Edit", col);
                            auditTrailDao.SecurityLog("Edit User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);

                        }
                    }
                    else
                    {
                        TempData["Warning"] = Locale.UserPendingApproval;
                    }
                }
                return RedirectToAction("Edit", new { id = col["fldUserId"] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                CurrentUser.Account.Status = "D";
                CurrentUser.Account.TaskId = TaskIds.User.INDEX;
                //Get value from system profile
                //string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
                //Get the Value of Dual Approval Setting
                string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

                string x = col["deleteBox"];
                string _userType = "";
                DataTable ds = new DataTable();
                if ((col["deleteBox"]) != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        UserModel CheckUserType = userDao.CheckUserMasterByID(arrResult, "", "UserAbb");
                        if (CheckUserType != null)
                        {
                            _userType = CheckUserType.userType;
                        }
                        if (!_userType.Equals("System"))
                        {
                            if (userDao.CheckUserMasterTempByID("", arrResult, "UserAbb") == false)
                            {
                                //if ("N".Equals(systemProfile))
                                if ("N".Equals(securityProfile)) // Check if Dual Approval Setting = N
                                {

                                    userDao.DeleteUserMaster(arrResult);
                                    auditTrailDao.Log("User Abb : " + CheckUserType.fldUserAbb + " User Desc : " + CheckUserType.fldUserDesc + " Branch Code : " + CheckUserType.fldBranchCode + " User Type : " + CheckUserType.userType + " Disable : " + CheckUserType.fldDisableLogin + " Verification Limit : " + CheckUserType.fldVerificationLimit + " Verification Class : " + CheckUserType.fldVerificationClass + " ID Suspended At : " + CheckUserType.fldIDExpDate, CurrentUser.Account);
                                    string ActionDetail = SecurityAuditLogDao.UserProfile_DeleteTemplate(null, null, "Deleted", "Delete", arrResult);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);
                                    //auditTrailDao.Log("Delete User - Ret :  " + col["deleteBox"], CurrentUser.Account);

                                    TempData["Notice"] = Locale.RecordsuccesfullyDeleted;


                                }
                                else
                                {

                                    userDao.CreateUserMasterTemp(col, CurrentUser.Account.BankCode, arrResult, "Delete");
                                    string ActionDetail = SecurityAuditLogDao.UserProfile_DeleteTemplate(null, null, "Deleted", "Delete", arrResult);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);
                                    auditTrailDao.Log("User Abb : " + CheckUserType.fldUserAbb + " User Desc : " + CheckUserType.fldUserDesc + " Branch Code : " + CheckUserType.fldBranchCode + " User Type : " + CheckUserType.userType + " Disable : " + CheckUserType.fldDisableLogin + " Verification Limit : " + CheckUserType.fldVerificationLimit + " Verification Class : " + CheckUserType.fldVerificationClass + " ID Suspended At : " + CheckUserType.fldIDExpDate, CurrentUser.Account);
                                    TempData["Notice"] = Locale.UserSuccessfullyAddedtoApproved;


                                }
                            }
                            else
                            {
                                TempData["Warning"] = Locale.UserPendingApproval;
                            }
                        }
                        else
                        {
                            TempData["Notice"] = Locale.SystemUserIdNotAllowedtoDelete;
                        }
                    }

                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //_log.Error(ex);
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                //ViewBag.Bank = CurrentUser.Account;
                ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
                ViewBag.VerificationClass = userDao.ListVerificationClass(CurrentUser.Account.BankCode);

                //ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode, CurrentUser.Account.UserAbbr);
                //ViewBag.BranchAvailableList = userDao.ListAvailableBranch(CurrentUser.Account.BankCode);
                //ViewBag.BranchSelectedList = userDao.ListSelectedBranch(CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                //ViewBag.VerificationClass = userDao.ListVerificationClass();
                //ViewBag.City = userDao.ListCity();
                //Check if LoginAD "Y" to display password
                //ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.User.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                CurrentUser.Account.Status = "A";
                CurrentUser.Account.TaskId = TaskIds.User.INDEX;

                //Get value from system profile
                //string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
                string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

                UserModel user = new UserModel();
                List<string> errorMessages = userDao.ValidateUser(col, "Create", CurrentUser.Account.UserId);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if (userDao.CheckUserMasterTempByID("", col["fldUserAbb"], "UserAbb") == false)
                    {
                        //if ("N".Equals(systemProfile))
                        if ("N".Equals(securityProfile))
                        {

                            userDao.CreateUserMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create");
                            UserModel CheckUser = userDao.CheckUserMasterInTempByID(col["fldUserAbb"], "", "UserAbb");
                            auditTrailDao.Log("User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);

                            
                            //userDao.MoveToUserMasterFromTemp(col["fldUserAbb"], "Create");
                            //string ActionDetail = SecurityAuditLogDao.UserProfile_AddTemplate(col, null, "Add", "Create");
                            //auditTrailDao.SecurityLog("Add User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);
                            
                            TempData["Notice"] = Locale.RecordsuccesfullyCreated;

                        }
                        else
                        {
                            userDao.CreateUserMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create");

                            //string ActionDetail = SecurityAuditLogDao.UserProfile_AddTemplate(col, null, "Add", "Create");
                            UserModel CheckUser = userDao.CheckUserMasterInTempByID(col["fldUserAbb"], "", "UserAbb");
                            auditTrailDao.Log("User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);
                            //auditTrailDao.SecurityLog("Add User", ActionDetail, CurrentUser.Account.TaskId, CurrentUser.Account);
                            //auditTrailDao.Log("Add User Record Successfully to Temp Table for Check to Create. User Abb : " + col["fldUserAbb"], CurrentUser.Account);

                            TempData["Notice"] = Locale.UserSuccessfullyCreated;
                            //auditTrailDao.Log("Add into Temporary record to Create - User Abb: " + col["fldUserAbb"], CurrentUser.Account);
                        }
                    }
                    else
                    {
                        TempData["Notice"] = Locale.UserProfileAlreadyExiststoDeleteorUpdate;
                    }

                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}