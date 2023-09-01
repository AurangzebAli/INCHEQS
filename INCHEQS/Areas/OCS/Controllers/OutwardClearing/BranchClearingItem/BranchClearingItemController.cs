using INCHEQS.Areas.OCS.Models.BranchClearingItem;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing
{
    public class BranchClearingItemController : BaseController
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(BranchClearingItemController));
        private IBranchClearingItemDao branchClearingItemDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public BranchClearingItemController(IBranchClearingItemDao branchClearingItemDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.branchClearingItemDao = branchClearingItemDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        [CustomAuthorize(TaskIds = "700101")]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig("700101"));
            auditTrailDao.SecurityLog("Branch Clearing Item", "", "700101", CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = "700101")]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string t = collection["Type"];

            string stmt = "SELECT fldBranchId FROM tblHubBranch as a left join tblHubUser as b ";
            stmt = stmt + "on a.fldHubCode = b.fldHubCode left join ";
            stmt = stmt + "tblHubMaster as c on a.fldHubId = c.fldHubId ";
            stmt = stmt + "where b.fldUserId = @fldUserID and c.fldApprovalStatus = 'Y'";

            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (t.Equals("Ready"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig("700101", "View_BranchItemReadyForSubmit", "CapturingBranch", "fldCapturingBranch IN ("+ stmt + ")", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig("700101", "View_BranchItemSubmitted", "CapturingBranch", "fldCapturingBranch IN (" + stmt + ")", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                collection);
               
            }
            return View();
            
        }

        [CustomAuthorize(TaskIds = "700101")]
        [HttpPost()]
        public ActionResult Submit(FormCollection collection)
        {

            try
            {
                branchClearingItemDao.UpdateBranchItem(collection, CurrentUser.Account);
                TempData["Notice"] = "Successfully Submitted!!!";
                auditTrailDao.Log("Add - Branch Clearing Item - ", CurrentUser.Account);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}