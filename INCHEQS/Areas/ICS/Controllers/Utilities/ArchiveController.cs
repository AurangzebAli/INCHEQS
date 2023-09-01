using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{
    public class ArchiveController : BaseController {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ArchiveController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
        }

        // GET: Archive
        [CustomAuthorize(TaskIds = TaskIds.Archive.INDEX)]
        public async Task<ActionResult> Index() {
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIds.Archive.INDEX, "View_AuditLog"));
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.Archive.INDEX)]
        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(new PageSqlConfig(TaskIds.Archive.INDEX, "View_AuditLog"),
                collection);
            auditTrailDao.Log("Retrieve audit log", CurrentUser.Account);
            return View();

        }
    }
}