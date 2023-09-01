using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.Users;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class ApprovedUserCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IUserDao userDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public ApprovedUserCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IUserDao userDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.userDao = userDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedUserChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ApprovedUserChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedUserChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ApprovedUserChecker.INDEX, "View_ApprovedUserChecker", "", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedUserChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
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
                            case TaskIdsOCS.ApprovedUserChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    userDao.MoveToUserMasterFromTemp(id, "Create");
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerApp_AddTemplate(null, null, "Checker (Approve)", "Approve", id);
                                    auditTrailDao.SecurityLog("Add User", ActionDetail, taskId, CurrentUser.Account);

                                }
                                else if (action.Equals("D"))
                                {
                                    //Add Audit Trial
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerApp_DeleteTemplate(null, null, "Checker (Approve)", "Delete", id);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMaster(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    UserModel before = userDao.CheckUserMasterByID(id,"", "UserAbb");
                                    //auditTrailDao.Log("Edit User - Before Update=> User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Disable : " + before.fldDisableLogin + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType  + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                                    userDao.MoveToUserMasterFromTemp(id, "Update");

                                    UserModel after = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Edit User - After Update=> User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Disable : " + after.fldDisableLogin + " Branch Code : " + after.fldBranchCode + " User Type : " + after.userType + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);

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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedUserChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
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
                            case TaskIdsOCS.ApprovedUserChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_AddTemplate(null, null, "Checker (Reject)", "Reject", id);
                                    auditTrailDao.SecurityLog("Add User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMasterTemp(id);
                                }
                                else if (action.Equals("D"))
                                {
                                    //Add Audit Trial
                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_DeleteTemplate(null, null, "Checker (Reject)", "Reject", id);
                                    auditTrailDao.SecurityLog("Delete User", ActionDetail, taskId, CurrentUser.Account);
                                    userDao.DeleteUserMasterTemp(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    UserModel before = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Edit User - Before Update=> User Abb : " + before.fldUserAbb + " User Desc : " + before.fldUserDesc + " Disable : " + before.fldDisableLogin + " Branch Code : " + before.fldBranchCode + " User Type : " + before.userType + " ID Suspended At : " + before.fldIDExpDate, CurrentUser.Account);

                                    userDao.MoveToUserMasterFromTemp(id, "Update");

                                    UserModel after = userDao.CheckUserMasterByID(id, "", "UserAbb");
                                    //auditTrailDao.Log("Edit User - After Update=> User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " Disable : " + after.fldDisableLogin + " Branch Code : " + after.fldBranchCode + " User Type : " + after.userType + " ID Suspended At : " + after.fldIDExpDate, CurrentUser.Account);

                                    string ActionDetail = SecurityAuditLogDao.UserProfileCheckerRej_EditTemplate(before, after, "Checker (Reject)", id);
                                    auditTrailDao.SecurityLog("Edit User", ActionDetail, taskId, CurrentUser.Account);

                                    userDao.DeleteUserMasterTemp(id);
                                }
                                userDao.DeleteUserMasterTemp(id);
                                break;
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