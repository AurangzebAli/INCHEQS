// bank of Malaysia
using INCHEQS.BNM.BankCode;

// bank of Philippines
//using INCHEQS.PCHC.BankCode;

//using INCHEQS.Areas.ICS.Models.SystemProfile;
//using INCHEQS.Models.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class BankCodeController : BaseController {

        private readonly ISystemProfileDao systemProfileDao;
        private IBankCodeDao bankCode;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;

        public BankCodeController(ISystemProfileDao systemProfileDao, IBankCodeDao bankCode, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService) {
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.bankCode = bankCode;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankCode.INDEX, "View_BankCode", null, bankCode.condition()), collection);

            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.BankCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Edit(FormCollection col, string bankCodeParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string bankcode = "";
            if (string.IsNullOrEmpty(bankCodeParam)) {
                bankcode = filter["fldBankCode"].Trim();
            } else {
                bankcode = bankCodeParam;
            }
            DataTable dataTable = await bankCode.getBankCodeAsync(bankcode);

            if ((dataTable.Rows.Count > 0)) {
                ViewBag.BankCode = dataTable.Rows[0];
                ViewBag.BankType = await bankCode.getBankTypeAsync();

            }
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.BankCode.UPDATE)]
        [HttpPost()]
        public async Task<ActionResult> Update(FormCollection collection) {
            ActionResult action;
            try {
                List<String> errorMessages = await bankCode.ValidateUpdateAsync(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { bankCodeParam = collection["bankCode"] });
                } else {
                    BankCodeModel before = bankCode.getBankId(collection["bankCode"]);
                    auditTrailDao.Log("Edit - Bank - Before Update => Bank Code : " + before.bankCode + " Bank Desc : " + before.bankDesc + " Bank Indicator : " + before.bankIndicator+ " Active : " + before.fldActive, CurrentUser.Account);

                    bankCode.Update(collection, CurrentUser.Account.UserId);
                    TempData["Notice"] = Locale.SuccessfullyUpdated;
                    BankCodeModel after = bankCode.getBankId(collection["bankCode"]);
                    auditTrailDao.Log("Edit - Bank - Before Update => Bank Code : " + after.bankCode + " Bank Desc : " + after.bankDesc + " Bank Indicator : " + after.bankIndicator+ " Active : " + after.fldActive, CurrentUser.Account);
                    action = RedirectToAction("Index");
                }
            } catch (Exception ex) {

                throw ex;
            }
            return action;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Create() {
            ViewBag.BankType = await bankCode.getBankTypeAsync();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCode.SAVECREATE)]
        [HttpPost()]
        public async Task<ActionResult> SaveCreate(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("BankCodeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<String> errorMessages = await bankCode.ValidateCreateAsync(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    if ("N".Equals(systemProfile)) {
                        bankCode.CreateBankCodeTemp(collection, CurrentUser.Account.UserId);
                        bankCode.CreateInBankMaster(collection["bankCode"]);
                        bankCode.DeleteInBankMasterTemp(collection["bankCode"]);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - Bank Code : " + collection["bankCode"] + " Bank Desc : " + collection["bankDesc"] + " Bank Type : " + collection["bankIndicator"], CurrentUser.Account);
                    } else {
                        bankCode.CreateBankCodeTemp(collection, CurrentUser.Account.UserId);

                        TempData["Notice"] = Locale.BankCodeCreateVerify;
                        auditTrailDao.Log("Add into Temporary Record to Create - Bank Code : " + collection["bankCode"] + " Bank Desc : " + collection["bankDesc"] + " Bank Type : " + collection["bankIndicator"], CurrentUser.Account);
                    }
                }

            } catch (Exception ex) {

                throw ex;
            }
            return RedirectToAction("Create");
        }
        [CustomAuthorize(TaskIds = TaskIds.BankCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("BankCodeChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {

                    List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        if ("N".Equals(systemProfile)) {
                            bankCode.DeleteBank(arrResult);
                            TempData["notice"] = Locale.SuccessfullyDeleted;
                            auditTrailDao.Log("Delete - Bank Code : " + arrResult, CurrentUser.Account);
                        } else {
                            bankCode.AddToBankMasterTempToDelete(arrResult);
                            TempData["notice"] = Locale.BankCodeDeleteVerify;
                            auditTrailDao.Log("Add into Temporary Record to Delete - Bank Code : " + arrResult, CurrentUser.Account);
                        }
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }
}