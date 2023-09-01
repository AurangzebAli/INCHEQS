using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models.SearchPageConfig;
//using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System.Web.Mvc;

namespace INCHEQS.Areas.RPA.Controller
{
    [CustomAuthorize(TaskIds = "all")]
    public class MainController : BaseController
    {
        public MainController() { }

        private readonly IPageConfigDao pageConfigDao;
        private readonly IDashboardConfigDao dashboardConfigDao;

        public MainController(IPageConfigDao pageConfigDao, IDashboardConfigDao dashboardConfigDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dashboardConfigDao = dashboardConfigDao;
        }
    }
}