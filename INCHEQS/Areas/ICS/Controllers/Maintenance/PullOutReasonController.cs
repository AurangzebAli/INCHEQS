using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class PullOutReasonController : BaseController {
       
        private IPullOutReasonDao pulloutdao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;

        public PullOutReasonController(IPullOutReasonDao pulloutdao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.pulloutdao = pulloutdao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.PullOutReason.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.PullOutReason.INDEX, "View_PullOutReason"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string pullOutIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string pullout = "";
            if (string.IsNullOrEmpty(pullOutIdParam)) {
                pullout = filter["fldPullOutId"].Trim();
            } else {
                pullout = pullOutIdParam;
            }
            ViewBag.PullOutReasonId = pulloutdao.getPullOutReason(pullout);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection) {

            try {
                List<String> errorMessages = pulloutdao.Validate(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Edit", new { pullOutIdParam = collection["pullOutReasonID"] });
                } else {
                    PullOutReasonModel before = pulloutdao.getPullOutData(collection["pullOutReasonID"]);
                    auditTrailDao.Log("Edit Pull Out Reason - Before Update=> Pull Out Reason : " + before.pullOutDesc, CurrentUser.Account);
                    pulloutdao.Update(collection, CurrentUser.Account.UserId);
                    TempData["Notice"] = Locale.SuccessfullyUpdated;

                    PullOutReasonModel after = pulloutdao.getPullOutData(collection["pullOutReasonID"]);
                    auditTrailDao.Log("Edit Pull Out Reason - Before Update=> Pull Out Reason : " + after.pullOutDesc, CurrentUser.Account);
                    return RedirectToAction("Index");

                }
            } catch (Exception ex) {

                throw ex;
            }
            
        }
        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("PullOutReasonChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<String> errorMessages = pulloutdao.Validate(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                } else {
                    if ("N".Equals(systemProfile)) {
                        pulloutdao.CreatePullOutReasonTemp(collection, CurrentUser.Account.UserId);
                        pulloutdao.CreateInPullOutReason(collection["pullOutReasonID"]);
                        pulloutdao.DeleteInPullOutReasonTemp(collection["pullOutReasonID"]);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - Pull Out Reason :" + collection["pullOutDesc"], CurrentUser.Account);
                    } else {
                        pulloutdao.CreatePullOutReasonTemp(collection, CurrentUser.Account.UserId);

                        TempData["Notice"] = Locale.PullOutReasonCreateVerify;
                        auditTrailDao.Log("Add Temporary record - Pull Out Reason :" + collection["pullOutDesc"], CurrentUser.Account);
                    }
                    auditTrailDao.Log("Add - Pull Out Reason :" + collection["pullOutDesc"], CurrentUser.Account);
                    return RedirectToAction("Index");
                }

            } catch (Exception ex) {

                throw ex;
            }

        }
        [CustomAuthorize(TaskIds = TaskIds.PullOutReason.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("PullOutReasonChecker", CurrentUser.Account.BankCode).Trim();

            if (collection != null & collection["deleteBox"] != null) {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {
                        pulloutdao.DeleteInPullOutReason(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                    } else {
                        pulloutdao.AddToPullOutReasonTempToDelete(arrResult);
                        TempData["notice"] = Locale.PullOutReasonDeleteVerify;
                    }
                }
                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete - Pull Out Reason : " + collection["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Add temporary record for Delete - Pull Out Reason : " + collection["deleteBox"], CurrentUser.Account);
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }
    }
}