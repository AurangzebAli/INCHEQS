using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.ProgressMonitoring
{
    public class BranchProgressStatusController : BaseController
    {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IDashboardConfigDao dashboardConfigDao;

        public BranchProgressStatusController(IPageConfigDao pageConfigDao, IDashboardConfigDao dashboardConfigDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dashboardConfigDao = dashboardConfigDao;
        }

        // GET: ProgressStatus
        [CustomAuthorize(TaskIds = TaskIds.BranchProgressStatus.INDEX)]
        public async Task<ActionResult> Index()
        {

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIds.BranchProgressStatus.INDEX, "View_BranchProgressStatus", "", "fldBankCode = @fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchProgressStatus.INDEX)]
        public ActionResult Progress()
        {

            ViewBag.Config = dashboardConfigDao.BranchProgressMonitoring(TaskIds.Dashboard.MAIN, TaskIds.BranchProgressStatus.INDEX);
            return View();
        }

    }
}