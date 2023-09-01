using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.Resources;
using INCHEQS.Security;
//using INCHEQS.Security.AuditTrail;
//using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;
using INCHEQS.Security.SecurityProfile;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    
    public class ThresholdSettingController : BaseController {

        private IThresholdSettingDao threshold;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        

        public ThresholdSettingController(IThresholdSettingDao threshold, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.threshold = threshold;
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        

        }
        // GET: ThresholdSetting
        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.MAIN)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ThresholdSetting.MAIN));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.MAIN)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ThresholdSetting.MAIN, "View_ThresholdSetting",null, "fldBankCode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            }),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string thresholdSettingType1 = "", string thresholdSettingTitle1 = "", string thresholdSettingAmt1 = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string thresholdSettingType = "";
            string thresholdSettingTitle = "";
            string thresholdSettingAmt = "";

            if (string.IsNullOrEmpty(thresholdSettingType1)) {
                thresholdSettingType = filter["thresholdType"].Trim();
                thresholdSettingTitle = filter["fldTitle"].Trim();
                thresholdSettingAmt = filter["fldThresholdAmt"].Trim();
            } else {
                thresholdSettingType = thresholdSettingType1;
                if (thresholdSettingType == "A")
                {
                    thresholdSettingType = "Approve";
                }
                else if (thresholdSettingType == "R")
                {
                    thresholdSettingType = "Reject";
                }
                thresholdSettingTitle = thresholdSettingTitle1;
                if (thresholdSettingTitle  == "1")
                {
                    thresholdSettingTitle = "First Level Amount";
                }
                else
                {
                    thresholdSettingTitle = "Second Level Amount";
                }
                thresholdSettingAmt = thresholdSettingAmt1;
            }
            DataTable result = threshold.GetThreshold(thresholdSettingType, thresholdSettingTitle, thresholdSettingAmt);
            if (result.Rows.Count > 0) {
                ViewBag.GetThreshold = result.Rows[0];
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ThresholdSetting.EDIT;

            try {
                List<String> errorMessages = threshold.Validate(col, CurrentUser.Account.BankCode);
                string x = col["thresholdid_1"];
                if (threshold.NoChanges(col, x) == true)
                {
                    TempData["Warning"] = Locale.NoChanges;

                }
                else
                {
              

                    if ("N".Equals(systemProfile))
                    {
                        if ((errorMessages.Count > 0))
                        {
                    TempData["ErrorMsg"] = errorMessages;
                        }
                        else
                        {
                            if (col != null)
                            {
                                ThresholdModel before = MaintenanceAuditLogDao.GetThresholdData(col["fldType"], col["fldSequence"], CurrentUser.Account.BankCode);
                        threshold.UpdateThreshold(col);

                                ThresholdModel after = MaintenanceAuditLogDao.GetThresholdData(col["fldType"], col["fldSequence"], CurrentUser.Account.BankCode);
                        TempData["Notice"] = Locale.Recordsavesuccessfully;
                       // auditTrailDao.Log("Update Threshold Setting successfully.", CurrentUser.Account);

                                string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_EditTemplate(CurrentUser.Account.BankCode, col["fldType"], col["fldSequence"], before, after, "Edit");
                                auditTrailDao.SecurityLog("Edit Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                        }

                }
                    else
                    {
                        if (threshold.checkIfaRecordPendingforApproval(col["thresholdid_1"]) == true)
                        {
                            ThresholdModel before = MaintenanceAuditLogDao.GetThresholdData(col["fldType"],col["fldSequence"], CurrentUser.Account.BankCode);

                            threshold.AddtoThresholdSettingTempToUpdate(col);
                            ThresholdModel after = MaintenanceAuditLogDao.GetThresholdDataTemp(col["fldType"], col["fldSequence"], CurrentUser.Account.BankCode);
                            TempData["Notice"] = Locale.ThresholdSettingSuccessVerify;
                           // auditTrailDao.Log("Add Threshold Setting to Threshold Setting Checker to Update.", CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_EditTemplate(CurrentUser.Account.BankCode, col["fldType"], col["fldSequence"], before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else
                        {
                            TempData["warning"] = Locale.VerificationLimitPendingApproval;
                        }
                        
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Edit", new { thresholdSettingType1 = col["fldType"].Trim() , thresholdSettingTitle1 = col["fldSequence"].Trim(), thresholdSettingAmt1 =  col["fldOrgAmount"].Trim() });
            
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.DELETE)]
        public ActionResult Delete(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ThresholdSetting.DELETE;

            if (col != null & col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults) {

                    string codebank = arrResult.Substring(0, 3);
                    string type = arrResult.Substring(3, 1);
                    string level = arrResult.Substring(4, arrResult.Length - 4);

                    if ("N".Equals(systemProfile)) {


                        threshold.DeleteInThresholdSetting(codebank, type, level);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                        string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_DeleteTemplate(codebank, type, level, "Delete");
                        auditTrailDao.SecurityLog("Delete Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                    } else {

                        if (threshold.checkIfaRecordPendingforApproval(arrResult) == true)
                        {
                            threshold.AddtoThresholdSettingTempToDelete(codebank, type, level);
                            TempData["notice"] = Locale.ThresholdSettingDeleteVerify;
                            string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_DeleteTemplate(codebank, type, level, "Delete");
                            auditTrailDao.SecurityLog("Delete Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else
                        {
                            TempData["warning"] = Locale.VerificationLimitPendingApproval;
                        }
                        
                    }
                }

                if ("N".Equals(systemProfile)) {
                    //auditTrailDao.Log("Delete - Threshold Setting Id : " + col["deleteBox"], CurrentUser.Account);
                } else {
                   // auditTrailDao.Log("Insert into Temporary Table to Delete - Threshold Setting Id : " + col["deleteBox"], CurrentUser.Account);
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ThresholdSetting.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection) {

            //string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ThresholdSetting.SAVECREATE;
            try {
                List<string> errorMessages = threshold.ValidateCreate(collection, CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                }
                else
                {
                    //if ("N".Equals(securityProfile))
                    if ("N".Equals(systemProfile)) 
                    {
                        //threshold.CreateThresholdSettingTemp(collection, CurrentUser.Account.UserId,CurrentUser.Account.BankCode);
                        threshold.CreateThresholdSetting(collection, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        //threshold.DeleteInThresholdSettingTemp(collection);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        //auditTrailDao.Log("Add - Threshold Setting : " + collection["fldId"] + " Threshold Setting Title : " + collection["fldSequence"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_AddTemplate(CurrentUser.Account.BankCode, collection["fldType"], collection["fldSequence"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                    } else {
                        threshold.CreateThresholdSettingTemp(collection, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

                        TempData["Notice"] = Locale.ThresholdSettingCreateVerify;
                        //auditTrailDao.Log("Add Temporary Threshold Setting - Threshold Setting Id: " + collection["fldId"] + " Threshold Setting Title : " + collection["fldSequence"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.VerificationThreshold_AddTemplate(CurrentUser.Account.BankCode, collection["fldType"], collection["fldSequence"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Verification Threshold", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    return RedirectToAction("Create");
                }
            } catch (Exception ex) {
                throw ex;
            }

        }
    }
}