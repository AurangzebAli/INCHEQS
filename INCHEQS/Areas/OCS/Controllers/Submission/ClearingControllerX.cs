using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Areas.OCS.Models.Clearing;
using System.Data;
using INCHEQS.Security.SystemProfile;
using System.Linq;
using log4net;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Controllers.Submission
{
    public class ClearingControllerX : ClearingBaseController
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ClearingControllerX));
        private readonly IClearingDao clearingDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;
        private IPageConfigDaoOCS pageConfigDao;

        public ClearingControllerX(IClearingDao clearingDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao, IPageConfigDaoOCS pageConfigDao)
        {
            this.clearingDao = clearingDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Clearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual async Task<ActionResult> Index()
        {
            //DataTable dt = new DataTable();

            //dt = clearingDao.GetClearingBranchByUserDataTable(CurrentUser.Account.UserId, CurrentUser.Account.BankCode);

            //return View();

            //ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TCCode.INDEX));
            //return View();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, gQueueSqlConfig.toPageSqlConfig());
            return View(searchPageHtml);

            ViewBag.SearchResult = pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);
            return View(searchResultPageHtml);

        }
    }
}