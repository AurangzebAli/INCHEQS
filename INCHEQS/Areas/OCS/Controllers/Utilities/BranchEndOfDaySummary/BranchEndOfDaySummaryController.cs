using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using System.Linq;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Areas.OCS.Models.BranchEndOfDaySummary;
using INCHEQS.Models.SearchPageConfig.Services;
using System.Data;

namespace INCHEQS.Areas.OCS.Controllers.Utilities
{

    public class BranchEndOfDaySummaryController : BaseController
    {
        private readonly IBranchEndOfDaySummaryDao branchEndOfDaySummaryDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly IPageConfigDaoOCS pageConfigDao;
        private readonly ISearchPageService searchPageService;

        public BranchEndOfDaySummaryController(IBranchEndOfDaySummaryDao branchEndOfDaySummaryDao, IPageConfigDaoOCS pageConfigDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao, ISearchPageService searchPageService)
        {
            this.branchEndOfDaySummaryDao = branchEndOfDaySummaryDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
            this.searchPageService = searchPageService;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchEndOfDaySummary.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {

            DataTable dtEOD = new DataTable();
            string strEODStatus = "";
            string strEODStatusDesc = "";
            dtEOD = branchEndOfDaySummaryDao.GetCenterEndOfDay(CurrentUser.Account.BankCode);

            if (dtEOD.Rows.Count > 0) {
                strEODStatus = dtEOD.Rows[0]["fldEODStatus"].ToString();
            }
            else {
                strEODStatus = "N";
            }

            
            if (strEODStatus == "Y") {
                strEODStatusDesc = "Done";
            }
            else {
                strEODStatusDesc = "Not Done";
            }

            ViewBag.EODStatus = strEODStatusDesc;
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BranchEndOfDaySummary.INDEX));
            auditTrailDao.SecurityLog("Access OCS Branch End of Day Summary", "", TaskIdsOCS.BranchEndOfDaySummary.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchEndOfDaySummary.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BranchEndOfDaySummary.INDEX, "View_BranchEndOfDaySummary", "fldProcessDate"),
            collection);
            return View();
        }

        public virtual ActionResult PerformEOD(FormCollection collection)
        {
            DataTable dtPendingBranch = new DataTable();
            dtPendingBranch = branchEndOfDaySummaryDao.GetPendingBranchesInfo(CurrentUser.Account.BankCode);
            if (dtPendingBranch.Rows.Count > 0)
            {

                if (dtPendingBranch.Rows[0]["fldPendingItems"].ToString() == "0")
                {

                    bool result = branchEndOfDaySummaryDao.InsertCenterEndOfDay(CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                }
                else
                {
                    TempData["ErrorMsg"] = "There are Pending Items in Branches !";

                }
            }
        
            return RedirectToAction("Index");
        }



    }
}