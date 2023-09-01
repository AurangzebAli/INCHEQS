//Bank of Malaysia
using INCHEQS.BNM.BranchCode;
using INCHEQS.BNM.BankCode;

// bank of Philipine
//using INCHEQS.PCHC.BranchCode;
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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class BranchCodeController : BaseController {
        
        private IBranchCodeDao branchcodedao;
        private IBankCodeDao bankcodedao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;

        public BranchCodeController(IBranchCodeDao branchcodedao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao,IBankCodeDao bankCodeDao) {
            this.pageConfigDao = pageConfigDao;
            this.branchcodedao = branchcodedao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.bankcodedao = bankCodeDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.BranchCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BranchCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
           
            /*Edit by Umar to filter BranchCode */
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BranchCode.INDEX, "View_BranchCode", "fldStateCode, fldBranchCode", null), collection);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.BranchCode.EDIT)]
        public ActionResult Edit(FormCollection col) {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string branchid = filter["fldBranchId"].Trim();
            if ((branchid == null)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DataTable dataTable = branchcodedao.getBranchCode(branchid);

            if ((dataTable.Rows.Count > 0)) {
                ViewBag.BranchCode = dataTable.Rows[0];

            }

            DataTable dataTable2 = bankcodedao.getBankCode(ViewBag.BranchCode["fldBankCode"]);
            if ((dataTable2.Rows.Count > 0))
            {
                ViewBag.Bank = dataTable2.Rows[0];
                ViewBag.BankCode = ViewBag.Bank["fldBankCode"];
                ViewBag.BankDesc = ViewBag.Bank["fldBankDesc"];
            }
            else
            {
                ViewBag.BankCode ="";
                ViewBag.BankDesc = "";
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.EDIT)]
        public ActionResult Update(FormCollection col,string userId) {
            //more codes
            branchcodedao.UpdateBranchMaster(col,userId);
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("BranchCodeChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {

                        branchcodedao.DeleteInBranchMasters(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                    } else {

                        branchcodedao.AddtoBranchMasterTempToDelete(arrResult);
                        TempData["notice"] = Locale.BranchCodeDeleteVerify;
                    }
                }

                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete - Branch Code : " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Insert into Temporary Table to Delete - Branch Code : " + collection["deleteBox"], CurrentUser.Account);
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
            ViewBag.Bank = branchcodedao.getBank();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCode.CREATE)]
        public async Task<ActionResult> SaveCreate(FormCollection collection,string userId)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("BranchCodeChecker", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<String> errorMessages = await branchcodedao.ValidateCreateAsync(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        branchcodedao.CreateBranchMasterTemp(collection, CurrentUser.Account.UserId);
                        branchcodedao.CreateInBranchMaster(collection["branchCode"]);
                        branchcodedao.DeleteInBranchMasterTemp(collection["branchCode"]);
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - Branch Code: " + collection["branchCode"], CurrentUser.Account);
                    }
                    else
                    {
                        branchcodedao.CreateBranchMasterTemp(collection, CurrentUser.Account.UserId);
                        TempData["Notice"] = Locale.BranchCodeCreateVerify;
                        auditTrailDao.Log("Add into Temporary record to Create - Branch Code: " + collection["branchCode"], CurrentUser.Account);
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