//Bank of Malaysia
//using INCHEQS.BNM.BranchCode;
//using INCHEQS.BNM.BankCode;

// bank of Philipine
//using INCHEQS.PCHC.BankCode;

using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance {
    public class BranchCodeController : BaseController {
        
        private IBranchCodeDao branchcodedao;
        private IBankCodeDao bankcodedao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BranchCodeController(IBranchCodeDao branchcodedao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IBankCodeDao bankCodeDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.branchcodedao = branchcodedao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.bankcodedao = bankCodeDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.BranchCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BranchCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) 
        {
           
            /*Edit by Umar to filter BranchCode */
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BranchCode.INDEX, "View_BranchCode", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string branchIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);

            string branchid = "";

            if (string.IsNullOrEmpty(branchIdParam))
            {

                branchid = filter["fldBranchId"].Trim();
            }
            else
            {
                branchid = branchIdParam;
            }
            ViewBag.BankType = branchcodedao.getBankType();

            DataTable dataTable = branchcodedao.getBranchCode(branchid);

            if ((dataTable.Rows.Count > 0)) {
                ViewBag.BranchCode = dataTable.Rows[0];
            }

            DataTable dataTable2 = bankcodedao.getBankCode(ViewBag.BranchCode["fldBankCode"], ViewBag.BranchCode["fldBankType"]);
            if ((dataTable2.Rows.Count > 0))
            {
                ViewBag.Bank = dataTable2.Rows[0];
                ViewBag.BankCode = ViewBag.Bank["fldBankCode"];
                ViewBag.BankDesc = ViewBag.Bank["fldBankDesc"];
            }
            else
            {
                ViewBag.BankCode = "";
                ViewBag.BankDesc = "";
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.UPDATE)]
        [HttpPost()]
        //public async Task<ActionResult> Update(FormCollection collection)
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            try
            {
                List<String> errorMessages = branchcodedao.ValidateUpdate(collection);

                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.BranchCode.INDEX;
                CurrentUser.Account.TaskId = sTaskId;

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { branchIdParam = collection["branchId"] });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(collection["branchId"], "Master");
                        auditTrailDao.Log("Edit Branch ID - Before Update=> Branch ID: " + before.branchId + ", Branch Desc:" + before.branchDesc, CurrentUser.Account);

                        branchcodedao.UpdateBranchCode(collection);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;

                        BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(collection["branchId"], "Master");
                        auditTrailDao.Log("Edit Branch ID - After Update=> Branch ID: " + after.branchId + ", Branch Desc:" + after.branchDesc, CurrentUser.Account);
                        //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_EditTemplate(collection["branchId"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsBranchMasterTempExist = branchcodedao.CheckBranchCodeTempById(collection["branchId"]);

                        if (IsBranchMasterTempExist == true)
                        {
                            TempData["Warning"] = Locale.BranchCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(collection["branchId"], "Master");

                            branchcodedao.CreateBranchCodeTemp(collection,"U");
                            TempData["Notice"] = Locale.BranchCodeUpdateVerify;

                            BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(collection["branchId"], "Temp");

                            auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Branch ID : " + collection["branchId"] + ", Branch Desc:" + collection["branchDesc"], CurrentUser.Account);

                            //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_EditTemplate(collection["branchId"], before, after, "Edit");
                            //auditTrailDao.SecurityLog("Edit Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return RedirectToAction("Edit", new { branchIdParam = collection["branchId"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BranchCode.INDEX;
            CurrentUser.Account.TaskId = sTaskId;

            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults) {

                    if ("N".Equals(systemProfile)) {

                        //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_DeleteTemplate(arrResult, "Delete");
                        //auditTrailDao.SecurityLog("Delete Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        branchcodedao.DeleteBranchCode(arrResult);
                        TempData["Notice"] = Locale.SuccessfullyDeleted;
                        auditTrailDao.Log("Delete - Branch ID : " + arrResult, CurrentUser.Account);

                    }
                    else {

                        bool IsBranchCodeTempExist = branchcodedao.CheckBranchCodeTempById(arrResult);

                        if (IsBranchCodeTempExist == true)
                        {
                            TempData["Warning"] = Locale.BranchCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            branchcodedao.CreateBranchCodeTempToDelete(arrResult);
                            TempData["Notice"] = Locale.BranchCodeVerifyDelete;
                            auditTrailDao.Log("Add into Temporary Record to Delete - Branch ID : " + arrResult, CurrentUser.Account);

                            //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                }

                }

            } else

                TempData["Warning"] = Locale.Nodatawasselected;

            //auditTrailDao.Log("");
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create(FormCollection collection)
        {
            ViewBag.BankType = branchcodedao.getBankType();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection, string userId)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BranchCode.INDEX;
            CurrentUser.Account.TaskId = sTaskId;

            try
            {
                List<String> errorMessages = branchcodedao.ValidateCreate(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        branchcodedao.CreateBranchMaster(collection);
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add into Branch Master Table -  Branch ID : " + collection["branchId"] + ", Branch Desc:" + collection["branchDesc"], CurrentUser.Account);
                        //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_AddTemplate(collection["branchId"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        branchcodedao.CreateBranchCodeTemp(collection, "A");

                        TempData["Notice"] = Locale.BranchCodeCreateVerify;
                        auditTrailDao.Log("Add into Branch Master Table -  Branch ID : " + collection["branchId"] + ", Branch Desc:" + collection["branchDesc"], CurrentUser.Account);
                        //string ActionDetails = MaintenanceAuditLogDao.BranchProfile_AddTemplate(collection["branchId"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return RedirectToAction("Create");
        }

    }
}