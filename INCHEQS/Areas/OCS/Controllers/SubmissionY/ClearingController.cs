using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.Clearing;
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

namespace INCHEQS.Areas.OCS.Controllers.Submission {
    public class ClearingController : BaseController {
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(ClearingController));
        private IClearingDao clearingDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ClearingController(IClearingDao clearingDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.clearingDao = clearingDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Clearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.Clearing.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Clearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TCCode.INDEX, "View_TCCode", "fldTCCode"),
            collection);
            return View();
        }
    }
}