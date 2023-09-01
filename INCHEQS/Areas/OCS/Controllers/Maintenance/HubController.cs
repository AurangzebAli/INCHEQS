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

namespace INCHEQS.Areas.OCS.Controllers.Maintenance {


    public class HubController : BaseController {

        private readonly IHubDao hubDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;

        public HubController(IHubDao hubDao, IAuditTrailDao auditTrailDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.hubDao = hubDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.HubUserProfile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.HubUserProfile.INDEX, "View_Hub", "fldHubCode", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string hubIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string hubId = "";
            if (string.IsNullOrEmpty(hubIdParam))
            {
                hubId = filter["fldHubId"].Trim();
            }
            else
            {
                hubId = hubIdParam;
            }
          
            //ViewBag.Hub = hubDao.GetHub(hubId, CurrentUser.Account.BankCode);
            ViewBag.SelectedUser = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
            ViewBag.AvailableUser = hubDao.ListAvailableUserInHub(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<string> userIds = new List<string>();
                string hubId = col["fldHubId"].Trim();

                List<string> errorMessages = new List<string>();// hubDao.ValidateUpdate(col, CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Edit", new { hubIdParam = hubId });
                }
                else if ("N".Equals(systemProfile))
                {
                    //HubModel beforeHub = hubDao.GetHub(hubId, CurrentUser.Account.BankCode);
                  //  auditTrailDao.Log("Update Hub- Before Update=> Hub ID: " + beforeHub.fldHubId + " Hub Desc : " + beforeHub.fldHubDesc, CurrentUser.Account);
                    //hubDao.AddHubToHubMasterTempToUpdate(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

                    string beforeUser = "";
                    List<UserModel> beforeUserLists = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var beforeUserlist in beforeUserLists)
                    {
                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Hub , Before Update =>- Hub ID: " + hubId + " User : " + beforeUser, CurrentUser.Account);

                    if ((col["selectedUser"]) != null)
                    {
                        userIds = col["selectedUser"].Split(',').ToList();
                        foreach (string userId in userIds)
                        {
                            //hubDao.AddUserToHubUserTempToUpdate(userId, hubId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        }
                    }
                    else
                    {
                        //hubDao.DeleteAllUserInHubTemp(hubId);
                    }
                    
                    //hubDao.UpdateHubMaster(hubId);

                    string afterUser = "";
                    List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var afterUserlist in afterUserLists)
                    {
                        afterUser = afterUser + afterUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Hub , After Update => - Hub ID: " + hubId + " User : " + afterUser, CurrentUser.Account);

                    hubDao.DeleteInHubMasterTemp(hubId);
                   // hubDao.DeleteAllUserInHubTemp(hubId);

                    TempData["Notice"] = Locale.HubSuccessfullyUpdated;
                    return RedirectToAction("Index");
                }
                else
                {
                    //HubModel beforeHub = hubDao.GetHub(hubId, CurrentUser.Account.BankCode);
                    //auditTrailDao.Log("Update Hub- Before Update=> Hub ID: " + beforeHub.fldHubId + " Hub Desc : " + beforeHub.fldHubDesc, CurrentUser.Account);
                    //hubDao.AddHubToHubMasterTempToUpdate(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

                    string beforeUser = "";
                    List<UserModel> beforeUserLists = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
                    foreach (var beforeUserlist in beforeUserLists)
                    {
                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Hub , Before Update =>- Hub ID: " + hubId + " User : " + beforeUser, CurrentUser.Account);

                    if ((col["selectedUser"]) != null)
                    {
                        userIds = col["selectedUser"].Split(',').ToList();
                        foreach (string userId in userIds)
                        {
                            //hubDao.AddUserToHubUserTempToUpdate(userId, hubId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        }
                    }
                    else {
                        //hubDao.DeleteAllUserInHubTemp(hubId);
                    }

                    auditTrailDao.Log("Add into Temporary record to Update - Hub Id: " + hubId, CurrentUser.Account);  
                    TempData["Notice"] = Locale.HubAddedToTempForUpdate;
                    return RedirectToAction("Index");
                }                
                
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            ViewBag.AvailableUser = hubDao.ListAvailableUserInHub(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("HubChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<string> userIds = new List<string>();
                string hubId = col["fldHubId"].Trim();
                List<string> errorMessages = new List<string>();// hubDao.ValidateCreate(col, CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    if ("N".Equals(systemProfile)) {
                        //hubDao.CreateHubMaster(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        auditTrailDao.Log("Add - Hub ID: " + hubId + " Hub Desc : " + col["fldHubDesc"], CurrentUser.Account);

                        if ((col["selectedUser"]) != null)
                        {
                            userIds = col["selectedUser"].Split(',').ToList();
                            foreach (string userId in userIds)
                            {
                                //hubDao.InsertUserInHub(hubId, userId,CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                            }
                            string afterUser = "";
                            List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            auditTrailDao.Log("Add User In Hub , After Update => - Hub ID: " + hubId + " User : " + afterUser, CurrentUser.Account);
                        }
                        TempData["Notice"] = Locale.HubSuccessfullyCreated;
                    }
                    else {
                        hubDao.CreateHubMasterTemp(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                        if ((col["selectedUser"]) != null)
                        {
                            userIds = col["selectedUser"].Split(',').ToList();
                            foreach (string userId in userIds)
                            {
                                //hubDao.InsertUserInHub(hubId, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                            }
                            string afterUser = "";
                            List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(hubId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            auditTrailDao.Log("Add User In Hub , After Update => - Hub ID: " + hubId + " User : " + afterUser, CurrentUser.Account);
                        }
                        TempData["Notice"] = Locale.HubAddedToTempForCreate;
                        auditTrailDao.Log("Add into Temporary record to Create - Hub ID: " + col["fldHubDesc"], CurrentUser.Account);
                    }                    
                }
                return RedirectToAction("Create");
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.HubUserProfile.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {
            //try {
            //    List<string> arrResults = new List<string>();

            //    if ((col["deleteBox"]) != null) {
            //        if ("N".Equals(hubDao.GetMaintenanceChecker().Trim())) {
            //            arrResults = col["deleteBox"].Trim().Split(',').ToList();
            //            foreach (var arrResult in arrResults) {
            //                hubDao.DeleteAllUserInHub(arrResult, CurrentUser.Account.BankCode);
            //                hubDao.DeleteHub(arrResult, CurrentUser.Account.BankCode);
            //                auditTrailDao.Log("Delete Hub - Hub ID: " + arrResult, CurrentUser.Account);
            //            }
            //            TempData["Notice"] = Locale.HubSuccessfullyCreated;

            //        } else {
                        
            //            arrResults = col["deleteBox"].Trim().Split(',').ToList();
            //            foreach (var arrResult in arrResults)
            //            {
            //                if (hubDao.CheckPendingApproval(arrResult, CurrentUser.Account.BankCode))
            //                {
            //                    TempData["ErrorMsg"] = Locale.HubPendingApproval;
            //                }
            //                else {
            //                    hubDao.AddHubToHubMasterTempToDelete(arrResult);
            //                    auditTrailDao.Log("Add into Temporary record to Delete - Hub Id: " + col["deleteBox"], CurrentUser.Account);
            //                    TempData["Notice"] = Locale.HubAddedToTempForDelete;
            //                }                            
            //            }                 
                        
            //        }
            //    } else {
            //        TempData["Warning"] = Locale.PleaseSelectARecord;
            //    }

                return RedirectToAction("Index");
            //} catch (Exception ex) {
            //    throw ex;
            //}
        }

    }
}