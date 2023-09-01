using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;

using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.SqlClient;
using INCHEQS.Areas.OCS.Models.ProgressStatus;
using System.Collections.Generic;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Security.AuditTrail;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ProgressMonitoring
{    
    public class ProgressStatusController : BaseController {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IOCSProgressStatusDao OCSProgressStatusDao;
        private readonly IAuditTrailDao auditTrailDao;

        public ProgressStatusController(IPageConfigDao pageConfigDao , IOCSProgressStatusDao OCSProgressStatusDao, IAuditTrailDao audilTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.OCSProgressStatusDao = OCSProgressStatusDao;
            this.auditTrailDao = audilTrailDao;

        }

        // GET: ProgressStatus
        [CustomAuthorize(TaskIds = TaskIds.OCSProgressStatus.INDEX)]
        public async Task<ActionResult> Index()
        {

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIds.OCSProgressStatus.INDEX, "view_ocsprogressstatus"));
            auditTrailDao.SecurityLog("Access OCS Progress Status", "", TaskIds.OCSProgressStatus.INDEX, CurrentUser.Account);
            return View(); 
        }

        [CustomAuthorize(TaskIds = TaskIds.OCSProgressStatus.INDEX)]
        public ActionResult Progress() {

            List<OCSProgressStatus> history = OCSProgressStatusDao.ReturnProgressStatus();
            ViewBag.ProgressStatus = history;
            ViewBag.CapturingMode = OCSProgressStatusDao.GetCapturingModeDataTable();
            ViewBag.UserType = OCSProgressStatusDao.GetUserType();
            return View();
        }

    }
}