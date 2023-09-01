using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;
//using INCHEQS.Areas.OCS.Models.HubBranch;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class HubUserController : BaseController
    {

        private readonly IHubDao hubDao;
        private readonly IHubBranchDao hubBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public HubUserController(IHubDao hubDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, IHubBranchDao hubBranchDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.HubUserProfile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.HubUserProfile.INDEX, "View_Hub", "", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string hubIdParam = "")
        {
            string taskid = TaskIdsOCS.HubUserProfile.EDIT;
            if (CurrentUser.HasTask(taskid))
            {
                CurrentUser.Account.TaskId = taskid;
            }
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string hubCode = "";
            if (string.IsNullOrEmpty(hubIdParam))
            {
                hubCode = filter["fldHubCode"].Trim();
            }
            else
            {
                hubCode = hubIdParam;
            }

            ViewBag.Hub = hubDao.CheckHubMasterByID(hubCode, "HubCode");
            ViewBag.SelectedUser = hubDao.ListSelectedUserInHub(hubCode, CurrentUser.Account.BankCode);
            ViewBag.AvailableUser = hubDao.ListAvailableUserInHub(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string sTaskID = TaskIdsOCS.HubUserProfile.UPDATE;
            if (CurrentUser.HasTask(sTaskID))
            {
                CurrentUser.Account.TaskId = sTaskID;
            }
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            try
            {
                List<string> userIds = new List<string>();
                string hubcode = col["fldHubCode"].Trim();

                List<string> errorMessages = hubDao.ValidateHubUser(col, "Update", CurrentUser.Account.UserId);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Edit", new { hubIdParam = hubcode });
                }
                else
                {
                    if (hubDao.CheckHubMasterTempByID(col["fldHubCode"].Trim(), "HubCode") == true)

                    {
                        TempData["Warning"] = Locale.HubPendingApproval;

                    }


                    else
                    {


                        if ("N".Equals(securityProfile))
                        {
                            HubModel beforeHub = hubDao.CheckHubMasterByID(hubcode, "HubCode");
                            //auditTrailDao.Log("Update Hub- Before Update=> Hub Code: " + beforeHub.fldHubId + " Hub Desc : " + beforeHub.fldHubDesc, CurrentUser.Account);

                            string beforeUser = "";
                            List<UserModel> beforeUserLists = hubDao.ListSelectedUserInHub(hubcode, CurrentUser.Account.BankCode);
                            foreach (var beforeUserlist in beforeUserLists)
                            {
                                beforeUser = beforeUser + beforeUserlist.fldUserAbb + ',';
                            }


                            //auditTrailDao.Log("Update User In Hub , Before Update =>- Hub Code: " + hubcode + " User : " + beforeUser, CurrentUser.Account);

                            if ((col["selectedUser"]) != null)
                            {
                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId in userIds)
                                {
                                    hubDao.InsertUserInHubTemp(hubcode, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Create");
                                }
                            }
                            else
                            {
                                hubDao.DeleteInHubUserTemp(hubcode);
                            }

                            hubDao.UpdateHubMaster(col, CurrentUser.Account.UserId);
                            hubDao.UpdateHubUser(hubcode);

                            HubModel afterHub = hubDao.CheckHubMasterByID(hubcode, "HubCode");


                            string afterUser = "";
                            List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(hubcode, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ',';
                            }
                            //auditTrailDao.Log("Update User In Hub , After Update => - Hub Code: " + hubcode + " User : " + afterUser, CurrentUser.Account);

                            hubDao.DeleteInHubUserTemp(hubcode);

                            string ActionDetails = SecurityAuditLogDao.HubUser_EditTemplate(beforeHub, afterHub, beforeUser, afterUser, "Edit", col);
                            auditTrailDao.SecurityLog("Edit Hub", ActionDetails, sTaskID, CurrentUser.Account);

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                            return RedirectToAction("Edit", new { hubIdParam = hubcode });
                        }
                        else
                        {
                            string beforeUser2 = "";
                            List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(hubcode, CurrentUser.Account.BankCode);
                            foreach (var beforeUserlist2 in beforeUserLists2)
                            {
                                beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                            }
                            HubModel before = hubDao.CheckHubMasterByID(hubcode, "HubCode");

                            hubDao.CreateHubMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Update");


                            if ((col["selectedUser"]) != null)
                            {
                                

                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId in userIds)
                                {
                                    hubDao.InsertUserInHubTemp(hubcode, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Create");
                                }

                                string alreadyselecteduser = "";
                                List<UserModel> alreadyselecteduserLists = hubDao.ListSelectedUserInHub(hubcode, CurrentUser.Account.BankCode);
                                foreach (var userlist in alreadyselecteduserLists)
                                {
                                    alreadyselecteduser = userlist.fldUserId;
                                    if (hubDao.CheckHubUserExistInTemp(hubcode, alreadyselecteduser, "Update") == false)
                                    {
                                        hubDao.InsertUserInHubTemp(hubcode, alreadyselecteduser, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Delete");
                                }
                            }

                            }

                            else
                            {
                                string beforeUser = "";
                                List<UserModel> beforeUserLists = hubDao.ListSelectedUserInHub(hubcode, CurrentUser.Account.BankCode);
                                foreach (var beforeUserlist in beforeUserLists)
                                {
                                    beforeUser = beforeUserlist.fldUserId;
                                    hubDao.InsertUserInHubTemp(hubcode, beforeUser, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Delete");
                                }
                            }

                            string afterUser2 = "";
                            List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(hubcode, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist2 in afterUserLists2)
                            {
                                afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            }
                            HubModel after = SecurityAuditLogDao.CheckHubMasterByIDTemp_Security(hubcode, "HubCode");

                            //auditTrailDao.Log("Add into Temporary record to Update - Hub Code: " + hubcode, CurrentUser.Account);
                            string ActionDetails = SecurityAuditLogDao.HubUser_EditTemplate(before, after, beforeUser2, afterUser2, "Edit", col);
                            auditTrailDao.SecurityLog("Edit Hub", ActionDetails, sTaskID, CurrentUser.Account);

                            TempData["Notice"] = Locale.HubAddedToTempForUpdate;
                        }
                        return RedirectToAction("Edit", new { hubIdParam = hubcode });
                    }

                }

                return RedirectToAction("Edit", new { hubIdParam = hubcode });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            string taskid = TaskIdsOCS.HubUserProfile.CREATE;
            if (CurrentUser.HasTask(taskid))
            {
                CurrentUser.Account.TaskId = taskid;
            }
            ViewBag.AvailableUser = hubDao.ListAvailableUserInHub(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.HubUserProfile.SAVECREATE;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            try
            {
                List<string> userIds = new List<string>();
                string hubCode = col["fldHubCode"].Trim();
                List<string> errorMessages = hubDao.ValidateHubUser(col, "Create", CurrentUser.Account.UserId);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if (hubDao.CheckHubMasterTempByID(col["fldHubCode"].Trim(), "HubCode") == false)
                    {

                        if ("N".Equals(securityProfile))
                        {

                            hubDao.CreateHubMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create");
                            hubDao.MoveToHubMasterFromTemp(col["fldHubCode"].Trim(), "Create");
                            //auditTrailDao.Log("Add - Hub ID: " + hubCode + " Hub Desc : " + col["fldHubDesc"], CurrentUser.Account);

                            if ((col["selectedUser"]) != null)
                            {
                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId in userIds)
                                {
                                    hubDao.InsertUserInHub(hubCode, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "New");
                                }
                                string afterUser = "";
                                List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(hubCode, CurrentUser.Account.BankCode);
                                foreach (var afterUserlist in afterUserLists)
                                {
                                    afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                                }
                                //auditTrailDao.Log("Add User In Hub , After Update => - Hub ID: " + hubCode + " User : " + afterUser, CurrentUser.Account);
                                string ActionDetails = SecurityAuditLogDao.HubUser_AddTemplate(col, afterUser, "Add");
                                auditTrailDao.SecurityLog("Add Hub", ActionDetails, sTaskId, CurrentUser.Account);

                            }
                            TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                        }
                        else
                        {
                            hubDao.CreateHubMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create");
                            if ((col["selectedUser"]) != null)
                            {
                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId in userIds)
                                {
                                    hubDao.InsertUserInHubTemp(hubCode, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Create");
                                }
                                string afterUser2 = "";
                                List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(hubCode, CurrentUser.Account.BankCode);
                                foreach (var afterUserlist2 in afterUserLists2)
                                {
                                    afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                }
                                //auditTrailDao.Log("Add User In Hub , After Update => - Hub ID: " + hubCode + " User : " + afterUser, CurrentUser.Account);
                                string ActionDetails = SecurityAuditLogDao.HubUser_AddTemplate(col, afterUser2, "Add");
                                auditTrailDao.SecurityLog("Add Hub", ActionDetails, sTaskId, CurrentUser.Account);

                            }
                            TempData["Notice"] = Locale.HubAddedToTempForCreate;
                           //auditTrailDao.Log("Add into Temporary record to Create - Hub ID: " + col["fldHubDesc"], CurrentUser.Account);
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = Locale.HubPendingApproval;
                    }
                }

                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HubUserProfile.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.HubUserProfile.DELETE;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            try
            {

                if ((col["deleteBox"]) != null)
                {
                    List<string> arrResults = new List<string>();
                    arrResults = col["deleteBox"].Trim().Split(',').ToList();
                    foreach (var arrResult in arrResults)
                    {
                        if (hubDao.CheckHubMasterTempByID(arrResult.Trim(), "HubCode") == true)
                        {
                            TempData["Warning"] = Locale.HubPendingApproval;
                        }
                        else
                        {


                            if (("N".Equals(securityProfile)))
                            {
                                string beforeUser2 = "";
                                List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(arrResult, CurrentUser.Account.BankCode);
                                foreach (var beforeUserlist2 in beforeUserLists2)
                                {
                                    beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                }


                                hubDao.DeleteInHubUserMaster(arrResult, "Both");
                                //hubBranchDao.DeleteInHubMasterBranchTemp(arrResult);
                                hubBranchDao.DeleteAllBranchInHubTemp(arrResult);
                                hubBranchDao.DeleteAllBranchInHub(arrResult, CurrentUser.Account.BankCode);
                                //auditTrailDao.Log("Delete Hub - Hub Code: " + arrResult, CurrentUser.Account);
                                string ActionDetails = SecurityAuditLogDao.HubUser_DeleteTemp(beforeUser2, "Delete", arrResult);
                                auditTrailDao.SecurityLog("Delete Hub", ActionDetails, sTaskId, CurrentUser.Account);


                                TempData["Notice"] = Locale.RecordsuccesfullyDeleted;

                            }
                            else
                            {

                                string beforeUser2 = "";
                                List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(arrResult, CurrentUser.Account.BankCode);
                                foreach (var beforeUserlist2 in beforeUserLists2)
                                {
                                    beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                }
                                

                                hubDao.CreateHubMasterTemp(col, arrResult, CurrentUser.Account.UserId, "Delete");
                                //auditTrailDao.Log("Add into Temporary record to Delete - Hub Code: " + col["deleteBox"], CurrentUser.Account);
                                string ActionDetails = SecurityAuditLogDao.HubUser_DeleteTemp(beforeUser2, "Delete", arrResult);
                                auditTrailDao.SecurityLog("Delete Hub", ActionDetails, sTaskId, CurrentUser.Account);

                                TempData["Notice"] = Locale.HubAddedToTempForDelete;



                            }

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
                throw ex;
            }
        }

    }
}