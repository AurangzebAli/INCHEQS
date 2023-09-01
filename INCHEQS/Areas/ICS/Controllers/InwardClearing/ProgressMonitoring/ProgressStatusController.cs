using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.Models.ProgressStatus;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.ProgressMonitoring
{    
    public class ProgressStatusController : BaseController {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IDashboardConfigDao dashboardConfigDao;
        private readonly IProgressStatusDao progressStatusDao;
        public string BranchMakerPendingData = "308130";
        public string BranchMakerLargeAmount;
        public string BranchCheckerPendingData = "308120";
        public string BranchCheckerLargeAmount = "308150";


        public ProgressStatusController(IPageConfigDao pageConfigDao, IDashboardConfigDao dashboardConfigDao, IProgressStatusDao progressStatusDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dashboardConfigDao = dashboardConfigDao;
            this.progressStatusDao = progressStatusDao;
        }

        // GET: ProgressStatus
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Index()
        {
            ViewBag.ClearDate = progressStatusDao.getClearDate();
            //ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIds.ProgressStatus.INDEX, "View_ProgressStatus", "", "fldBankCode = @fldBankCode", new[] {
            // new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}));

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ProgressStatus.INDEX));


            return View(); 
        }

        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Progress(FormCollection col) {
            
            if (col["branchBox"] != null)
            {
                ViewBag.ProgressStatusWithBranch = progressStatusDao.ReturnFilterBranchProgressStatus(col);
                ViewBag.ProgressStatusWithPPSBranch = progressStatusDao.ReturnFilterPPSBranchProgressStatus(col);
                return View("ProgressStatusWithBranch");
            }
            else
            {
                ViewBag.ProgressStatus = progressStatusDao.ReturnProgressStatus(col);
                return View();
            }
        }

    }
}