﻿using INCHEQS.Security.SystemProfile;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
//using INCHEQS.Models.VerificationLimit;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Security.Account;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class VerificationLimitController : BaseController {
        
        private readonly IAuditTrailDao auditTrailDao;
        private IVerificationLimitDao verifyLimit;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public VerificationLimitController(IVerificationLimitDao verifyLimit, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.verifyLimit = verifyLimit;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.VerificationLimit.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.VerificationLimit.INDEX, "View_VerificationLimit"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Details(FormCollection col, string classIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string classID = "";
            if (string.IsNullOrEmpty(classIdParam)) {
                classID = filter["fldClass"].Trim();
            } else {
                classID = classIdParam;
            }

            if ((classID == null)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DataTable dataTable = verifyLimit.Find(classID);
            if ((dataTable.Rows.Count > 0)) {
                ViewBag.findVerificationLimit = dataTable.Rows[0];
            }

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection) {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.VerificationLimit.UPDATE;
            try {
                    if (collection != null) {
                    VerificationLimitModel before = verifyLimit.GetVerifyLimit(collection["txtclass"]);
                    //auditTrailDao.Log("Edit Batch Class - Before Change =>Class :" + before.verifyLimitClass + " Limit Desc : " + before.verifyLimitDesc + " UpdateTimestamp :" + DateTime.Now, CurrentUser.Account);

                    if ("N".Equals(systemProfile))
                    {
                        VerificationLimitModel beforeEdit = MaintenanceAuditLogDao.GetVerificationLimitData(collection["txtclass"]);

                        verifyLimit.Update(collection, CurrentUser.Account.UserId);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;
                        VerificationLimitModel afterEdit = MaintenanceAuditLogDao.GetVerificationLimitData(collection["txtclass"]);
                        string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_EditTemplate(collection["txtclass"], beforeEdit, afterEdit, "Edit");
                        auditTrailDao.SecurityLog("Edit Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {

                        if (verifyLimit.checkIfaRecordPendingforApproval(collection["txtclass"]) == true)
                        {
                            VerificationLimitModel beforeEdit = MaintenanceAuditLogDao.GetVerificationLimitData(collection["txtclass"]);

                            verifyLimit.AddToVerificationLimitMasterTempToUpdate(collection, collection["txtclass"], CurrentUser.Account.UserId);
                            TempData["Notice"] = Locale.VerificationLimitUpdateVerify;

                            VerificationLimitModel afterEdit = MaintenanceAuditLogDao.GetVerificationLimitDataTemp(collection["txtclass"]);
                            string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_EditTemplate(collection["txtclass"], beforeEdit, afterEdit, "Edit");
                            auditTrailDao.SecurityLog("Edit Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else
                        {
                            TempData["Warning"] = Locale.VerificationLimitPendingApproval;
                        }

                    }

                    VerificationLimitModel after = verifyLimit.GetVerifyLimit(collection["txtclass"]);
                    //auditTrailDao.Log("Edit Batch Class - Before Change =>Class :" + after.verifyLimitClass + " Limit Desc : " + after.verifyLimitDesc + " UpdateTimestamp :" + DateTime.Now, CurrentUser.Account);
                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Details", new { classIdParam = collection["txtclass"] }); 
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create(FormCollection collection) {

            return View();
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection) {
            
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.VerificationLimit.SAVECREATE;
            try {
                List<string> messages = verifyLimit.Validate(collection);

                if (messages.Count > 0) {
                    TempData["ErrorMsg"] = messages;
                    return RedirectToAction("Create");
                } else {
                    if ("N".Equals(systemProfile)) {
                        verifyLimit.CreateInVerificationLimitMasterTemp(CurrentUser.Account, collection, CurrentUser.Account.UserId);
                        verifyLimit.CreateInVerificationLimitMaster(collection["class"]);
                        verifyLimit.DeleteInVerificationLimitMasterTemp(collection["class"]);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        //auditTrailDao.Log("Add - Verification Limit Class : " + collection["class"] + " Limit Desc : " + collection["VerifyLimitDesc"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_AddTemplate(collection["class"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                    } else {
                        if (verifyLimit.checkIfaRecordPendingforApproval(collection["class"]) == true)
                        {
                            verifyLimit.CreateInVerificationLimitMasterTemp(CurrentUser.Account, collection, CurrentUser.Account.UserId);
                            TempData["Notice"] = Locale.VerificationLimitCreateVerify;
                            string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_AddTemplate(collection["class"], "Add", "Y");
                            auditTrailDao.SecurityLog("Add Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else
                        {
                            TempData["Warning"] = Locale.VerificationLimitPendingApproval;
                            return RedirectToAction("Create");
                        }

                        //auditTrailDao.Log("Add temporary record - Verification Limit Class : " + collection["class"] + " Limit Desc : " + collection["VerifyLimitDesc"], CurrentUser.Account);
                    }
                    
                    return RedirectToAction("Create", new { classIdParam = collection["txtclass"] });
                }
            } catch (Exception ex) {

                throw ex;
            }
            
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimit.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.VerificationLimit.DELETE;

           
            if (collection != null & collection["deleteBox"] != null) {
            List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {

                    if ("N".Equals(systemProfile)) {
                        string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                        verifyLimit.DeleteInVerificationLimitMaster(arrResult);

                        TempData["notice"] = Locale.SuccessfullyDeleted;

                    } else {
                        if (verifyLimit.checkIfaRecordPendingforApproval(arrResult) == true)
                        {
                            verifyLimit.AddToVerificationLimitMasterTempToDelete(arrResult);
                        TempData["Notice"] = Locale.VerificationLimitDeleteVerify;
                            string ActionDetails = MaintenanceAuditLogDao.VerificationLimit_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else
                        {
                            TempData["Warning"] = Locale.VerificationLimitPendingApproval;
                        }
                    }
                }
                if ("N".Equals(systemProfile)) {
                    //auditTrailDao.Log("Delete - Verification Limit Class : " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    //auditTrailDao.Log("Add temporary record to Delete - Verification Limit Class : " + collection["deleteBox"], CurrentUser.Account);
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }
    }
}