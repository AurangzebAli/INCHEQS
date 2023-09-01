using INCHEQS.Areas.ICS.Models.HostReturnReason;
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
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class HostReturnReasonController : Controller {
        private readonly IHostReturnReasonDao hostReturnReasonDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected ISystemProfileDao systemProfileDao;

        public HostReturnReasonController(IHostReturnReasonDao hostReturnReasonDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.hostReturnReasonDao = hostReturnReasonDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        // GET: ICS/HostReturnReason
        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.HostReturnReason.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.HostReturnReason.INDEX, "View_HostReturnReasonCode", "fldStatusID"),
            collection);
            return View();
        }

        // GET: ReturnCode/Details?..
        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string statusIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string strStatusId = "";
            if (string.IsNullOrEmpty(statusIdParam)) {
                strStatusId = filter["fldStatusID"].Trim();
            } else {
                strStatusId = statusIdParam;
            }

            DataTable dataTable = hostReturnReasonDao.GetHostReturnReason(strStatusId);
            if ((dataTable.Rows.Count > 0)) {
                ViewBag.ReturnStatus = dataTable.Rows[0];
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection) {
            List<String> errorMessages = hostReturnReasonDao.ValidateUpdate(collection);

            if ((errorMessages.Count > 0)) {
                TempData["ErrorMsg"] = errorMessages;
            } else {
                HostReturnReasonModel before = hostReturnReasonDao.GetHostReturnReasonModel(collection["statusId"]);
                auditTrailDao.Log("Edit - Host Return Reason - Before Update => Status Id : " + before.statusId + ", Status Description : " + before.statusDesc, CurrentUser.Account);

                hostReturnReasonDao.UpdateHostReturnReason(collection, CurrentUser.Account);
                TempData["notice"] = Locale.SuccessfullyUpdated;

                HostReturnReasonModel after = hostReturnReasonDao.GetHostReturnReasonModel(collection["statusId"]);
                auditTrailDao.Log("Edit - Host Return Reason - After Update => Status Id : " + after.statusId + ", Status Description : " + after.statusDesc, CurrentUser.Account);

            }
            return RedirectToAction("Edit", new { statusIdParam = collection["statusId"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("HostReturnReasonChecker", CurrentUser.Account.BankCode).Trim();
            List<String> errorMessages = hostReturnReasonDao.ValidateCreate(collection);
            string strAutoReject = collection["autoReject"];
            string strAutoPending = collection["autoPending"];

            if (collection["autoReject"] == null) {
                strAutoReject = "0";
            } else {
                strAutoReject = "1";
            }
            if (collection["autoPending"] == null) {
                strAutoPending = "0";
            } else {
                strAutoPending = "1";
            }

            if ((errorMessages.Count > 0)) {
                TempData["ErrorMsg"] = errorMessages;
            } else {
                if ("N".Equals(systemProfile)) {
                    hostReturnReasonDao.CreateHostReturnReasonTemp(collection, strAutoPending, strAutoReject, CurrentUser.Account);
                    hostReturnReasonDao.CreateInBankHostStatusMaster(collection["statusId"]);
                    hostReturnReasonDao.DeleteInBankHostStatusMasterTemp(collection["statusId"]);

                    TempData["notice"] = Locale.SuccessfullyCreated;
                    auditTrailDao.Log("Add Host Return Reason - Status ID : " + collection["statusId"] + ", Status Description : " + collection["statusDesc"], CurrentUser.Account);

                } else {
                    hostReturnReasonDao.CreateHostReturnReasonTemp(collection, strAutoPending, strAutoReject, CurrentUser.Account);
                    TempData["notice"] = Locale.HostReturnReasonCreateVerify;

                    auditTrailDao.Log("Add Host Return Reason Temporary Record - Status Id : " + collection["statusId"] + ", Status Description : " + collection["statusDesc"], CurrentUser.Account);
                }
            }
            return RedirectToAction("Index");
        }


        [CustomAuthorize(TaskIds = TaskIds.HostReturnReason.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("HostReturnReasonChecker", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {
                        hostReturnReasonDao.DeleteInBankHostStatusMaster(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                    } else {
                        hostReturnReasonDao.AddtoBankHostStatusMasterTempToDelete(arrResult);
                        TempData["notice"] = Locale.HostReturnReasonDeleteVerify;
                    }
                }

                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete Host Return Reason - Status Id: " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Add Host Return Reason into Temporary Record to Delete - Status Id: " + collection["deleteBox"], CurrentUser.Account);
                }

            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }

    }
}