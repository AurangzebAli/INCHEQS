using INCHEQS.PPS.CMSAccountInfo;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class CMSAccountInfoController : BaseController {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private ICMSAccountInfoDao cmsAccountInfoDao;
        protected readonly ISearchPageService searchPageService;

        public CMSAccountInfoController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ICMSAccountInfoDao cmsAccountInfoDao) {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.cmsAccountInfoDao = cmsAccountInfoDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.CMSAccountInfo.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.CMSAccountInfo.INDEX, "View_CMSAccountInfo",null, "fldBankCode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            }),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string AccountNoParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string AccountNo = "";
            if (string.IsNullOrEmpty(AccountNoParam)) {
                AccountNo = filter["fldAccountNo"].Trim();
            } else {
                AccountNo = AccountNoParam;
            }

            ViewBag.CmsAccount = cmsAccountInfoDao.GetCMSAccountInfo(AccountNo);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.CREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            List<string> errorMsg = cmsAccountInfoDao.ValidateCreate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                cmsAccountInfoDao.CreateCMSAccountInfo(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode,CurrentUser.Account.BankDesc);
                TempData["Notice"] = Locale.SuccessfullyCreated;

                auditTrailDao.Log("CMS Account Info Created - Account No :"+col["fldAccountNo"], CurrentUser.Account);
            }
            return RedirectToAction("Create");

        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            List<String> errorMsg = cmsAccountInfoDao.ValidateUpdate(col);
            if (errorMsg.Count > 0) {
                TempData["ErrorMsg"] = errorMsg;
            } else {
                CMSAccountInfoModel before = cmsAccountInfoDao.GetCMSAccountInfo(col["fldAccountNo"]);
                auditTrailDao.Log("CMS Account Info Created - Account No : " + before.fldAccountNo+ ", Branch Code : "+before.fldBranchCode+ ", Account Holder : " + before.fldAccountHolderName, CurrentUser.Account);

                cmsAccountInfoDao.UpdateCMSAccountInfo(col);
                TempData["Notice"] = Locale.SuccessfullyUpdated;

                CMSAccountInfoModel after = cmsAccountInfoDao.GetCMSAccountInfo(col["fldAccountNo"]);
                auditTrailDao.Log("CMS Account Info Created - Account No : " + after.fldAccountNo + ", Branch Code : " + after.fldBranchCode + ", Account Holder : " + after.fldAccountHolderName, CurrentUser.Account);
            }
            return RedirectToAction("Edit", new { AccountNoParam = col["fldAccountNo"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.CMSAccountInfo.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {

            if (col != null & col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {

                    cmsAccountInfoDao.DeleteFromCMSAccountInfo(arrResult);
                }
                TempData["Notice"] = Resources.Locale.SuccessfullyDeleted;
                auditTrailDao.Log("CMS Account Info Deleted - Account No :" + col["deleteBox"], CurrentUser.Account);
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}