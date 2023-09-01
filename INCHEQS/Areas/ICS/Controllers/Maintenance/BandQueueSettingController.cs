using INCHEQS.Areas.ICS.Models.BandQueueSetting;
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
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class BandQueueSettingController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IBandQueueSettingDao bandQueueSettingDao;
        protected readonly ISearchPageService searchPageService;
        private readonly ISystemProfileDao systemProfileDao;

       
        public BandQueueSettingController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, SearchPageService searchPageService, ISystemProfileDao systemProfileDao, IBandQueueSettingDao bandQueueSettingDao) {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.bandQueueSettingDao = bandQueueSettingDao;
        }
        // GET: ICS/AccountType
        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BandQueueSetting.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BandQueueSetting.INDEX, "View_BandQueueSetting"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string BandQueueIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string taskId = "";
            if (string.IsNullOrEmpty(BandQueueIdParam)) {
                taskId = filter["fldTaskId"].Trim();
            } else {
                taskId = BandQueueIdParam;
            }

            ViewBag.BandQueueSetting = bandQueueSettingDao.GetBandQueueSetting(taskId);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.CREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            List<string> errorMsg = bandQueueSettingDao.ValidateCreate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                bandQueueSettingDao.CreateBandQueueSetting(col, CurrentUser.Account);
                TempData["Notice"] = Locale.SuccessfullyCreated;

            }
            return RedirectToAction("Create");
            
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            List<String> errorMsg = bandQueueSettingDao.ValidateUpdate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                bandQueueSettingDao.UpdateBandQueueSetting(col);
                TempData["Notice"] = Locale.SuccessfullyUpdated;
            }
            return RedirectToAction("Edit", new { BandQueueIdParam = col["fldTaskId"] });
            
        }

        [CustomAuthorize(TaskIds = TaskIds.BandQueueSetting.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {

            if (col != null & col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {

                    bandQueueSettingDao.DeleteFromBandQueueSetting(arrResult);
                }
                TempData["Notice"] = Resources.Locale.SuccessfullyDeleted;
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}