using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.TransactionType;
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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {

    public class TransactionTypeController : BaseController {

        private static readonly ILog _log = LogManager.GetLogger(typeof(TransactionTypeController));
        private ITransactionTypeDao transactiontypeDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public TransactionTypeController(ITransactionTypeDao transactiontypeDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.transactiontypeDao = transactiontypeDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionType.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TransactionType.INDEX));
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.TransactionType.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TransactionType.INDEX, "View_TransactionType", "fldTransactionDesc"),
            collection);
            return View();
        }
        // GET: TransactionType/Details?TransType=01
        [CustomAuthorize(TaskIds = TaskIds.TransactionType.DETAIL)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Details(FormCollection collection, string TransType = null) {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            if (!string.IsNullOrEmpty(TransType)) {
                DataTable result = await transactiontypeDao.FindAsync(TransType);
                if (result.Rows.Count > 0) {
                    ViewBag.TransactionTypes = result.Rows[0];

                }

            } else {
                ViewBag.TransactionTypes = await transactiontypeDao.FindAsync(filter["fldTransactionType"]);
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionType.DETAIL)]
        public ActionResult Update(FormCollection collection) {

            transactiontypeDao.UpdateTransactionType(collection);
            auditTrailDao.Log("Update Transaction Type successfully.", CurrentUser.Account);
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionType.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionCode.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TransactionTypeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<string> errorMessages = transactiontypeDao.ValidateCreate(col);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;

                } else if ("N".Equals(systemProfile)) {
                    transactiontypeDao.CreateTransactionTypeInTemp(col, CurrentUser.Account);
                    transactiontypeDao.CreateTransactionType(col["transType"]);
                    transactiontypeDao.DeleteInTransactionTypeTemp(col["transType"]);

                    TempData["Notice"] = Locale.TransactionCodeSuccessfullyCreated;
                    auditTrailDao.Log("Add - Transaction Type : " + col["transType"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                } else {
                    transactiontypeDao.CreateTransactionTypeInTemp(col, CurrentUser.Account);

                    TempData["Notice"] = Locale.TransactionTypeCreateVerify;
                    auditTrailDao.Log("Add into Temporary Record to Create - Transaction Code : " + col["transType"] + ", Transaction Desc : " + col["transDesc"], CurrentUser.Account);
                }
                return RedirectToAction("Create") ;

            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.TransactionType.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TransactionTypeChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {
                        transactiontypeDao.DeleteTransactionType(arrResult);
                        TempData["Warning"] = Locale.SuccessfullyDeleted;
                    } else {
                        transactiontypeDao.AddToTransactionTypeTempToDelete(arrResult);
                        TempData["Notice"] = Locale.TransactionTypeDeleteVerify;
                    }
                }
                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete - Transaction Type : " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Insert into Temporary Table to Delete - Transaction Type : " + collection["deleteBox"], CurrentUser.Account);
                }
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}
