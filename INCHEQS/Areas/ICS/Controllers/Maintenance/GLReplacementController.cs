using INCHEQS.Areas.ICS.Models.GLReplacement;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
//using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class GLReplacementController : BaseController{

        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IGLReplacementDao glReplacementDao;
        protected readonly ISearchPageService searchPageService;

        public GLReplacementController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IGLReplacementDao glReplacementDao) {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.glReplacementDao = glReplacementDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.GLReplacement.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.GLReplacement.INDEX, "View_GLReplacement"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string GLReplacementIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string GLReplacementId = "";
            if (string.IsNullOrEmpty(GLReplacementIdParam)) {
                GLReplacementId = filter["fldGLReplacementID"].Trim();
            } else {
                GLReplacementId = GLReplacementIdParam;
            }

            ViewBag.GLReplacement = glReplacementDao.GetGLReplacement(GLReplacementId);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.CREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            List<string> errorMsg = glReplacementDao.ValidateCreate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                glReplacementDao.CreateGLReplacement(col, CurrentUser.Account);
                TempData["Notice"] = Locale.SuccessfullyCreated;
                auditTrailDao.Log("Create GL Replacement - Account Number : "+col["fldGLAccountNumber"],CurrentUser.Account);
            }
            return RedirectToAction("Create");

        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            List<String> errorMsg = glReplacementDao.ValidateUpdate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                GLReplacementModel before = glReplacementDao.GetGLReplacement(col["fldGLReplacementID"]);
                auditTrailDao.Log("Edit GL Replacement - Account No: " + before.fldGLAccountNumber, CurrentUser.Account);

                glReplacementDao.UpdateGLReplacement(col);
                TempData["Notice"] = Locale.SuccessfullyUpdated;

                GLReplacementModel after = glReplacementDao.GetGLReplacement(col["fldGLReplacementID"]);
                auditTrailDao.Log("Edit GL Replacement - Account No: " + after.fldGLAccountNumber, CurrentUser.Account);
            }
            return RedirectToAction("Edit", new { GLReplacementIdParam = col["fldGLReplacementID"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.GLReplacement.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {

            if (col != null & col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {

                    glReplacementDao.DeleteFromGLReplacement(arrResult);
                }
                TempData["Notice"] = Resources.Locale.SuccessfullyDeleted;
                auditTrailDao.Log("Delete Gl Replacement - Account Number : "+ col["deleteBox"], CurrentUser.Account);
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}