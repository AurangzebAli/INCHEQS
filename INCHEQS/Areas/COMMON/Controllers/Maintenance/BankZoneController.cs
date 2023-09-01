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
using INCHEQS.Areas.COMMON.Models.BankZone;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class BankZoneController : BaseController
    {

        private readonly IBankZoneDao bankzoneDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        //private static readonly ILog _log = LogManager.GetLogger(typeof(BankZoneController));
        public BankZoneController(IBankZoneDao bankzoneDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao/*, ISecurityProfileDao securityProfileDao*/ , IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.bankzoneDao = bankzoneDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            //this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankZone.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankZone.INDEX, "View_BankZone", "fldBankZoneCode"),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string bankzonecode = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                if (!string.IsNullOrEmpty(bankzonecode))
                {
                    ViewBag.bankzone = bankzoneDao.GetBankZone(bankzonecode);
                }
                else
                {

                    ViewBag.bankzone = bankzoneDao.GetBankZone(filter["fldBankZoneCode"]);
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.BankZone.UPDATE;

                //validate bank zone
                List<String> errorMessages = bankzoneDao.ValidateBankZone(col, "Update", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        BankZoneModel before = bankzoneDao.GetBankZone(col["fldBankZoneCode"]);
                        auditTrailDao.Log("Edit Bank Zone - Before Update=> Bank Zone Code : " + before.fldBankZoneCode + " Bank Zone Desc : " + before.fldBankZoneDesc, CurrentUser.Account);

                        //update Process
                        bankzoneDao.UpdateBankZoneMaster(col);
                        TempData["Notice"] = Locale.BankZoneSuccessfullyUpdated;

                        BankZoneModel after = bankzoneDao.GetBankZone(col["fldBankZoneCode"]);
                        auditTrailDao.Log("Edit Bank Zone - After Update=> Bank Zone Code : " + after.fldBankZoneCode + " Bank Zone Desc : " + after.fldBankZoneDesc, CurrentUser.Account);
                        //TempData["Notice"] = Locale.BankZoneSuccessfullyUpdated;
                        string ActionDetails = MaintenanceAuditLogDao.BankZone_EditTemplate(col["fldBankZoneCode"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        BankZoneModel before = bankzoneDao.GetBankZone(col["fldBankZoneCode"]);
                        bankzoneDao.CreateBankZoneMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Update");
                        BankZoneModel after = bankzoneDao.GetBankZoneTemp(col["fldBankZoneCode"]);
                        TempData["Notice"] = Locale.BankZoneSuccessfullyAddedtoApprovedUpdate;
                        auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Bank Zone Code : " + col["fldBankZoneCode"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.BankZone_EditTemplate(col["fldBankZoneCode"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Edit", new { bankzonecode = col["fldBankZoneCode"] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.BankZone.DELETE;
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankZone_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                            bankzoneDao.DeleteBankZoneMaster(arrResult, CurrentUser.Account.BankCode);
                            TempData["Notice"] = Locale.SuccessfullyDeleted;
                            auditTrailDao.Log("Delete - Account Profile table - Account Number :  " + col["deleteBox"], CurrentUser.Account);

                        }
                        else
                        {
                            bool IsAccountProfileTempExist = bankzoneDao.CheckBankZoneMasterTempById(arrResult, CurrentUser.Account.BankCode);

                            if (IsAccountProfileTempExist == true)
                            {
                                TempData["Warning"] = Locale.BankZoneAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                string ActionDetails = MaintenanceAuditLogDao.BankZone_DeleteTemplate(arrResult, "Delete");
                                auditTrailDao.SecurityLog("Delete Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                bankzoneDao.CreateBankZoneMasterTemp(col, CurrentUser.Account.BankCode, arrResult, "Delete");
                                TempData["Notice"] = Locale.BankZoneVerifyDelete;
                                auditTrailDao.Log("Add into Account Profile Temp table to Delete -  Account Number :  " + col["deleteBox"], CurrentUser.Account);
                            }
                        }

                    }

                }
                else

                    TempData["Warning"] = Locale.Nodatawasselected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //_log.Error(ex);
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZone.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.BankZone.SAVECREATE;
                BankZoneModel user = new BankZoneModel();
                //validate bank zone
                List<String> errorMessages = bankzoneDao.ValidateBankZone(col, "Create", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {

                        //Create Process
                        bankzoneDao.CreateBankZoneMaster(col, CurrentUser.Account.BankCode);
                        TempData["Notice"] = Locale.BankZoneSuccessfullyCreated;
                        auditTrailDao.Log("Add into Bank Zone Master Table -  Bank Zone Code : " + col["fldBankZoneCode"] + " Bank Zone Desc : " + col["fldBankZoneDesc"], CurrentUser.Account);
                        string ActionDetails = MaintenanceAuditLogDao.BankZone_AddTemplate(col["fldBankZoneCode"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bankzoneDao.CreateBankZoneMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create");
                        TempData["Notice"] = Locale.BankZoneSuccessfullyAddedtoApprovedCreate;
                        auditTrailDao.Log("Add into Bank Zone Temporary Table - Bank Zone Code : " + col["fldBankZoneCode"] + " Bank Zone Desc : " + col["fldBankZoneDesc"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.BankZone_AddTemplate(col["fldBankZoneCode"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
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