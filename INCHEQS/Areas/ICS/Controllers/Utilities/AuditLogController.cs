using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{
    public class AuditLogController : BaseController {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        public AuditLogController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
        }

        // GET: AuditLog
        [CustomAuthorize(TaskIds = TaskIds.AuditLog.INDEX)]
        public async Task<ActionResult> Index() {
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIds.AuditLog.INDEX, "View_AuditLog"));
            //AuditTrail.Log("Retrieve audit log_test");
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.AuditLog.INDEX)]
        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(new PageSqlConfig(TaskIds.AuditLog.INDEX, "View_AuditLog" 
                ,null, "fldBankCode=@fldBankCode", new[] {new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}
                ),collection);
            auditTrailDao.Log("Retrieve audit log", CurrentUser.Account);
            return View();

        }
    }
}