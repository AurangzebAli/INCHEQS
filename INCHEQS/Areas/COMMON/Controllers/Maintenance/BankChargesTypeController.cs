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
using INCHEQS.Areas.COMMON.Models.BankChargesType;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class BankChargesTypeController : BaseController
    {

        private readonly IBankChargesTypeDao bankchargestypeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        public BankChargesTypeController(IBankChargesTypeDao bankchargestypeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.bankchargestypeDao = bankchargestypeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.INDEX)]   //Done
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankChargesType.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.INDEX)]   //Done
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankChargesType.INDEX, "View_BankChargesType", "fldBankChargesType"),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.EDIT)]    //Done
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string bankchargestype = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                if (!string.IsNullOrEmpty(bankchargestype))
                {
                    ViewBag.bankcharges = bankchargestypeDao.GetBankChargesType(bankchargestype);
                }
                else
                {

                    ViewBag.bankcharges = bankchargestypeDao.GetBankChargesType(filter["fldBankChargesType"]);
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.UPDATE)] //Done
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim(); //Done
                string sTaskId = TaskIdsOCS.BankChargesType.UPDATE;

                //validate bank zone
                List<String> errorMessages = bankchargestypeDao.ValidateBankChargesType(col, "Update", CurrentUser.Account.BankCode); //Done

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(col["fldBankChargesType"]); //Done
                        //auditTrailDao.Log("Edit Bank Charges Type - Before Update=> Bank Charges Type : " + before.fldBankChargesType + " Bank Charges Desc : " + before.fldBankChargesDesc, CurrentUser.Account);

                        //update Process
                        bankchargestypeDao.UpdateBankChargesTypeMaster(col); //done
                        

                        BankChargesTypeModel after = bankchargestypeDao.GetBankChargesType(col["fldBankChargesType"]); //Done
                        //auditTrailDao.Log("Edit Bank Charges Type - After Update=> Bank Charges Type : " + after.fldBankChargesType + " Bank Charges Desc : " + after.fldBankChargesDesc, CurrentUser.Account);
                        TempData["Notice"] = Locale.BankChargesTypeSuccessfullyUpdated;

                        string ActionDetails = MaintenanceAuditLogDao.BankChargesType_EditTemplate(col["fldBankChargesType"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(col["fldBankChargesType"]);
                        bankchargestypeDao.CreateBankChargesTypeMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Update"); //Done
                        BankChargesTypeModel after = bankchargestypeDao.GetBankChargesTypeTemp(col["fldBankChargesType"]);
                        TempData["Notice"] = Locale.BankChargesTypeSuccessfullyAddedtoApprovedUpdate;
                        //auditTrailDao.Log("Bank Charges Type Record Successfully Added to Temp Table for Check to Approve . Bank Charges Type : " + col["fldBankChargesType"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.BankChargesType_EditTemplate(col["fldBankChargesType"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Edit", new { bankchargestype = col["fldBankChargesType"] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.DELETE)] //Done
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.BankChargesType.DELETE;
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankChargesType_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                            bankchargestypeDao.DeleteBankChargesTypeMaster(arrResult, CurrentUser.Account.BankCode); //Done
                            TempData["Notice"] = Locale.SuccessfullyDeleted;
                            //auditTrailDao.Log("Delete - Bank Charges Type table - Bank Charges Type :  " + col["deleteBox"], CurrentUser.Account); //Done

                        }
                        else
                        {
                            bool IsBankChargesTypeTempExist = bankchargestypeDao.CheckBankChargesTypeMasterTempById(arrResult, CurrentUser.Account.BankCode); //Done

                            if (IsBankChargesTypeTempExist == true)
                            {
                                TempData["Warning"] = Locale.BankChargesTypeAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                string ActionDetails = MaintenanceAuditLogDao.BankChargesType_DeleteTemplate(arrResult, "Delete");
                                auditTrailDao.SecurityLog("Delete Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                bankchargestypeDao.CreateBankChargesTypeMasterTemp(col, CurrentUser.Account.BankCode, arrResult, "Delete"); //Done
                                TempData["Notice"] = Locale.BankChargesTypeVerifyDelete;
                                //auditTrailDao.Log("Add into Bank Charges Type Temp table to Delete -  Bank Charges Type :  " + col["deleteBox"], CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.CREATE)] //Done
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesType.SAVECREATE)] //Done
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.BankChargesType.SAVECREATE;
              
                //validate bank zone
                List<String> errorMessages = bankchargestypeDao.ValidateBankChargesType(col, "Create", CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {

                        //Create Process
                        bankchargestypeDao.CreateBankChargesTypeMaster(col, CurrentUser.Account.BankCode); //Done
                        TempData["Notice"] = Locale.BankChargesTypeSuccessfullyCreated;
                        //auditTrailDao.Log("Add into Bank Charges Type Master Table -  Bank Charges Type : " + col["fldBankChargesType"] + " Bank Charges Desc : " + col["fldBankChargesDesc"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.BankChargesType_AddTemplate(col["fldBankChargesType"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bankchargestypeDao.CreateBankChargesTypeMasterTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create"); //Done
                        TempData["Notice"] = Locale.BankChargesTypeSuccessfullyAddedtoApprovedCreate;
                        //auditTrailDao.Log("Add into Bank Charges Type Temporary Table - Bank Charges Code : " + col["fldBankChargesType"] + " Bank Charges Desc : " + col["fldBankChargesDesc"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.BankChargesType_AddTemplate(col["fldBankChargesType"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
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