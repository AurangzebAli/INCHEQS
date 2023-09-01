using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.TransactionCode;
using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Resources;
using INCHEQS.Security;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance {

    public class TransactionCodeController : BaseController {

        private static readonly ILog _log = LogManager.GetLogger(typeof(TransactionCodeController));
        private ITransactionCodeDao transactioncodeDao;
        private ITransactionTypeDao transactiontypeDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public TransactionCodeController(ITransactionCodeDao transactioncodeDao, ITransactionTypeDao transactiontypeDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.transactioncodeDao = transactioncodeDao;
            this.transactiontypeDao = transactiontypeDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TransactionCode.INDEX));
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TransactionCode.INDEX, "View_TransactionCode", "fldTransactionCode"),
            collection);
            return View();
        }
        
        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Details(FormCollection collection, string TransCode = null) {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            if (!string.IsNullOrEmpty(TransCode)) {
                ViewBag.TransactionCodes = await transactioncodeDao.FindAsync(TransCode);

            } else {
                ViewBag.TransactionCodes = await transactioncodeDao.FindAsync(filter["fldTransactionCode"]);
            }
            ViewBag.TransactionTypes = transactiontypeDao.ListTransactionTypes();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.UPDATE)]
        public ActionResult Update(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TransactionCodeChecker", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> errorMessages = transactioncodeDao.ValidateUpdate(col);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Details", new { TransCode = col["transCode"].Trim() });
                }
                else if ("N".Equals(systemProfile))
                {
                    transactioncodeDao.AddToTransactionCodeTempToUpdate(col["transCode"]);
                    transactioncodeDao.UpdateTransactionCodeInTemp(col);
                    transactioncodeDao.UpdateTransactionCode(col["transCode"]);
                    transactioncodeDao.DeleteInTransactionCodeTemp(col["transCode"]);

                    TempData["Notice"] = Locale.TransactionCodeSuccessfullyUpdated;
                    auditTrailDao.Log("Add - Transaction Code : " + col["transCode"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                    return RedirectToAction("Index");
                }
                else
                {
                    transactioncodeDao.AddToTransactionCodeTempToUpdate(col["transCode"]);
                    transactioncodeDao.UpdateTransactionCodeInTemp(col);

                    TempData["Notice"] = Locale.TransactionCodeAddedToTempForUpdate;
                    auditTrailDao.Log("Add into Temporary Record to Update - Transaction Code : " + col["transCode"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TransactionCodeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<string> errorMessages = transactioncodeDao.ValidateCreate(col);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;

                } else if ("N".Equals(systemProfile)) {
                    transactioncodeDao.CreateTransactionCodeInTemp(col, CurrentUser.Account);
                    transactioncodeDao.CreateTransactionCode(col["transCode"]);
                    transactioncodeDao.DeleteInTransactionCodeTemp(col["transCode"]);

                    TempData["Notice"] = Locale.TransactionCodeSuccessfullyCreated;
                    auditTrailDao.Log("Add - Transaction Code : " + col["transCode"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                } else {
                    transactioncodeDao.CreateTransactionCodeInTemp(col, CurrentUser.Account);

                    TempData["Notice"] = Locale.TransactionCodeAddedToTempForCreate;
                    auditTrailDao.Log("Add into Temporary Record to Create - Transaction Code : " + col["transCode"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                }
                return RedirectToAction("Create") ;

            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TransactionCodeChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if (transactioncodeDao.CheckPendingApproval(arrResult))
                    {
                        TempData["ErrorMsg"] = Locale.TransactionCodePendingApproval;
                    }
                    else if ("N".Equals(systemProfile)) {
                        transactioncodeDao.DeleteTransactionCode(arrResult);
                        TempData["Warning"] = Locale.SuccessfullyDeleted;
                    } else {
                        transactioncodeDao.AddToTransactionCodeTempToDelete(arrResult);
                        TempData["Notice"] = Locale.TransactionCodeAddedToTempForDelete;
                    }
                }
                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete - Transaction Code : " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Insert into Temporary Table to Delete - Transaction Code : " + collection["deleteBox"], CurrentUser.Account);
                }
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}
