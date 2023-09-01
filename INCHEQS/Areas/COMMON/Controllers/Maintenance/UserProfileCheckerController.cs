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
    public class UserProfileCheckerController : BaseController {
        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        private static readonly ILog _log = LogManager.GetLogger(typeof(UserController));

        public UserProfileCheckerController(IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

        }

        [CustomAuthorize(TaskIds = TaskIds.UserProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.UserProfileChecker.INDEX));
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.UserProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.UserProfileChecker.INDEX, "View_UserProfileChecker", "fldId", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.UserProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            CurrentUser.Account.Status = "Y";
            CurrentUser.Account.TaskId = TaskIds.UserProfileChecker.INDEX;
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                //string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

                if ((col["deleteBox"]) != null) 
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);
                        switch (taskId)
                        {
                            case TaskIds.UserProfileChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    UserModel CheckUser = userDao.CheckUserMasterInTempByID(id, "", "UserAbb");
                                    auditTrailDao.Log("User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);
                                    userDao.MoveToUserMasterFromTemp(id, "Create");
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerApp_AddTemplate(null, null, "Checker (Approve)", "Approve", id);                          
                                    //auditTrailDao.Log("Checker Approve Add - User Abb: " + id, CurrentUser.Account);
                                    auditTrailDao.SecurityLog("Add User", ActionDetail, taskId, CurrentUser.Account);

                                }
                                else if (action.Equals("D"))
                                {
                                    //Add Audit Trial
                                    UserModel CheckUser = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    auditTrailDao.Log("User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);
                                    //auditTrailDao.Log("Checker Approve Delete - User Abb: " + id, CurrentUser.Account);
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerApp_DeleteTemplate(null, null, "Checker (Approve)", "Delete", id);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMaster(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    UserModel before = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Edit User - Before Update=> User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Disable : " + before.fldDisableLogin + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType  + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                                    userDao.MoveToUserMasterFromTemp(id, "Update");

                                    UserModel after = userDao.CheckUserMasterByID(id, "", "UserAbb");

                                    auditTrailDao.Log("After Update => User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Branch Code : " + after.fldBranchCode + " User Type : " + after.userType + " Disable : " + after.fldDisableLogin + " Verification Limit : " + after.fldVerificationLimit + " Verification Class : " + after.fldVerificationClass + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);

                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerApp_EditTemplate(before, after, "Checker (Approve)", id);
                                    auditTrailDao.SecurityLog("Edit User", ActionDetail, taskId, CurrentUser.Account);
                                }
                                userDao.DeleteUserMasterTemp(id);
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
                //        string Action = "";

                //            if (action == "A")
                //                    {Action = "Add";}
                //            else if (action == "U") 
                //                    {Action = "Update";}    
                //            else if (action == "D")
                //                    {Action = "Delete";}


                //                //Check Method by Action
                //                userDao.ApproveInChecker(id,action);
                //                auditTrailDao.SecurityLog("Approve State Code", Action, TaskId , CurrentUser.Account);

                //    }
                //    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                //}
                //else
                //{
                //    TempData["Warning"] = Locale.PleaseSelectARecord;
                //}
                //return RedirectToAction("Index");


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.UserProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            CurrentUser.Account.Status = "R";
            CurrentUser.Account.TaskId = TaskIds.UserProfileChecker.INDEX;
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);
                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.UserProfileChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    UserModel CheckUser = userDao.CheckUserMasterInTempByID(id, "", "UserAbb");
                                    auditTrailDao.Log("User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);
                                    //auditTrailDao.Log("Checker Approve Add - User Abb: " + id, CurrentUser.Account);
                                    //auditTrailDao.Log("Reject Add - User Abb: " + id, CurrentUser.Account);
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_AddTemplate(null, null, "Checker (Reject)", "Reject", id);
                                    auditTrailDao.SecurityLog("Add User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMasterTemp(id);
                                }
                                else if (action.Equals("D"))
                                {
                                    //Add Audit Trial
                                    UserModel before = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    auditTrailDao.Log("User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType + " Disable : " + before.fldDisableLogin + " Verification Limit : " + before.fldVerificationLimit + " Verification Class : " + before.fldVerificationClass + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);
                                    //auditTrailDao.Log("Checker Reject Delete - User Abb: " + id, CurrentUser.Account);
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_DeleteTemplate(null, null, "Checker (Reject)", "Reject", id);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMasterTemp(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    UserModel CheckUser = userDao.CheckUserMasterInTempByID(id, "", "UserAbb");
                                    auditTrailDao.Log("Before Update => User Abb : " + CheckUser.fldUserAbb + " User Desc : " + CheckUser.fldUserDesc + " Branch Code : " + CheckUser.fldBranchCode + " User Type : " + CheckUser.userType + " Disable : " + CheckUser.fldDisableLogin + " Verification Limit : " + CheckUser.fldVerificationLimit + " Verification Class : " + CheckUser.fldVerificationClass + " ID Suspended At : " + CheckUser.fldIDExpDate, CurrentUser.Account);

                                    UserModel before = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Reject Update - User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Disable : " + before.fldDisableLogin + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                                    UserModel after = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Reject Update => User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Disable : " + after.fldDisableLogin + " Branch Code : " + after.fldBranchCode + " User Type : " + after.userType + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);

                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_EditTemplate(before, after, "Checker (Reject)", id);
                                    auditTrailDao.SecurityLog("Edit User", ActionDetail, taskId, CurrentUser.Account);

                                    userDao.DeleteUserMasterTemp(id);
                                }
                                userDao.DeleteUserMasterTemp(id);
                                break;
                                ////Act based on task id


                                //        userDao.RejectInChecker(id, action);
                                //        auditTrailDao.SecurityLog("Reject State Code", "", TaskId, CurrentUser.Account);

                        }
                        }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
}