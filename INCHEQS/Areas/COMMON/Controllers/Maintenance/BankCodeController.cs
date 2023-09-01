// bank of Malaysia
//using INCHEQS.BNM.BankCode;

// bank of Philippines
//using INCHEQS.PCHC.BankCode;

//using INCHEQS.Areas.ICS.Models.SystemProfile;
//using INCHEQS.Models.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Resources;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankCodeController : BaseController
    {

        private readonly ISystemProfileDao systemProfileDao;
        private IBankCodeDao bankCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankCodeController(ISystemProfileDao systemProfileDao, IBankCodeDao bankCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.bankCodeDao = bankCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankCode.INDEX, "View_BankCode", "fldBankCode", bankCodeDao.condition()), collection);

            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.BankCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Edit(FormCollection col, string bankCodeParam = "", string bankTypeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string bankType = "";
            string bankcode = "";
            if (string.IsNullOrEmpty(bankCodeParam))
            {
                bankcode = filter["fldBankCode"].Trim();
            }
            else
            {
                bankcode = bankCodeParam;
            }

            if (string.IsNullOrEmpty(bankTypeParam))
            {
                
                  bankType = filter["fldBankType"].Trim();
            }
            else
            {
                bankType = bankTypeParam;
            }

            DataTable dataTable = await bankCodeDao.getBankCodeAsync(bankcode,bankType);

            if ((dataTable.Rows.Count > 0))
            {
                ViewBag.BankCode = dataTable.Rows[0];
                ViewBag.BankType = await bankCodeDao.getBankTypeAsync();
            }
            /*else
            {
                ViewBag.BankCode["fldBankCode"] = bankcode;
                ViewBag.BankType = bankType;
            }*/
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.UPDATE)]
        [HttpPost()]
        public async Task<ActionResult> Update(FormCollection collection)
        {
            string sTaskId = TaskIds.BankCode.INDEX;

            CurrentUser.Account.TaskId = sTaskId;
            ActionResult action;
            try
            {
                List<String> errorMessages = await bankCodeDao.ValidateUpdateAsync(collection);

                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { bankCodeParam = collection["bankCode"] });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        BankCodeModel before = bankCodeDao.GetBankCodeData(collection["bankCode"], collection["oldBankType"]);
                        auditTrailDao.Log("Edit Bank Code - Before Update=> Bank Code: " + before.fldBankCode + "Bank Type: " + before.fldBankType + "Bank Desc: " + before.fldBankDesc, CurrentUser.Account);

                        bankCodeDao.UpdateBankCodeToMain(collection);
                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;

                        BankCodeModel after = bankCodeDao.GetBankCodeData(collection["bankCode"], collection["bankTypeId"]);
                        auditTrailDao.Log("Edit Bank Code - After Update=> Bank Code: " + after.fldBankCode + "Bank Type: " + after.fldBankType + "Bank Desc: " + after.fldBankDesc, CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.BankProfile_EditTemplate(collection["bankCode"], before, after, "Edit", collection["bankTypeId"]);
                        //auditTrailDao.SecurityLog("Edit Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsBankCodeExist = bankCodeDao.CheckBankCodeDataTempById(collection["bankCode"]);
                        BankCodeModel before = bankCodeDao.GetBankCodeData(collection["bankCode"], collection["bankTypeId"]);

                        if (IsBankCodeExist == true)
                        {
                            TempData["Warning"] = Locale.BankCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            bankCodeDao.CreateInBankCodeTemp(collection, "U");

                            TempData["Notice"] = Locale.BankCodeUpdateVerify;

                            BankCodeModel after = MaintenanceAuditLogDao.GetBankCodeDataTemp(collection["bankCode"]);

                            auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Bank Code : " + collection["bankCode"], CurrentUser.Account);

                            //string ActionDetails = MaintenanceAuditLogDao.BankProfile_EditTemplate(collection["bankCode"], before, after, "Edit", collection["bankTypeId"]);
                            //auditTrailDao.SecurityLog("Edit Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return RedirectToAction("Edit", new { bankCodeParam = collection["bankCode"].Trim(), bankTypeParam = collection["bankTypeId"].Trim() });
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Create()
        {
            ViewBag.BankType = await bankCodeDao.getBankTypeAsync();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.SAVECREATE)]
        [HttpPost()]
        public async Task<ActionResult> SaveCreate(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BankCode.INDEX;

            CurrentUser.Account.TaskId = sTaskId;

            try
            {
                List<String> errorMessages = await bankCodeDao.ValidateCreateAsync(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {

                        bankCodeDao.CreateInBankMaster(collection);

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                        auditTrailDao.Log("Add into Bank Master Table -  Bank Code : " + collection["bankCode"] + "Bank Type: " + collection["bankTypeId"] + "Bank Desc: " + collection["bankDesc"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.BankProfile_AddTemplate(collection["bankCode"], "Add", "N", collection["bankTypeId"]);
                        //auditTrailDao.SecurityLog("Add Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);

                    }
                    else
                    {

                        bankCodeDao.CreateInBankCodeTemp(collection, "A");
                        TempData["Notice"] = Locale.BankCodeCreateVerify;
                        auditTrailDao.Log("Add into Bank Master Table -  Bank Code : " + collection["bankCode"] + "Bank Type: " + collection["bankTypeId"] + "Bank Desc: " + collection["bankDesc"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.BankProfile_AddTemplate(collection["bankCode"], "Add", "Y", collection["bankTypeId"]);
                        //auditTrailDao.SecurityLog("Add Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return RedirectToAction("Create");
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BankCode.INDEX;

            CurrentUser.Account.TaskId = sTaskId;

            if (collection != null & collection["deleteBox"] != null)
            {

                List<string> arrResults = collection["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        //string ActionDetails = MaintenanceAuditLogDao.BankProfile_DeleteTemplate(arrResult, "Delete");
                        //auditTrailDao.SecurityLog("Delete Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        bankCodeDao.DeleteInBankCode(arrResult);
                        TempData["notice"] = Locale.RecordsuccesfullyDeleted;
                        auditTrailDao.Log("Delete - Bank Code : " + arrResult.Substring(0,2) + ", Bank Type : " + arrResult.Substring(arrResult.Length-2), CurrentUser.Account);
                    }

                    else
                    {

                        bool IsBankCodeTempExist = bankCodeDao.CheckBankCodeDataTempById(arrResult);
                        if (IsBankCodeTempExist == true)
                        {
                            TempData["Warning"] = Locale.BankCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            //string ActionDetails = MaintenanceAuditLogDao.BankProfile_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Bank Profile", ActionDetails, sTaskId, CurrentUser.Account);

                            bankCodeDao.AddBankCodeinTemptoDelete(arrResult);
                            TempData["notice"] = Locale.BankCodeDeleteVerify;
                            auditTrailDao.Log("Add into Temporary Record to Delete - Bank Code : " + arrResult.Substring(0, 2) + ", Bank Type : " + arrResult.Substring(arrResult.Length - 2), CurrentUser.Account);
                        }
                    }
                }

            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }
}