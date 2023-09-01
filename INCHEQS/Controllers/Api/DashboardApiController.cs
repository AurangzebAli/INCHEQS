using INCHEQS.Models.DashboardConfig;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Controllers.Api
{
    public class DashboardApiController : BaseController
    {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IDashboardConfigDao dashboardConfigDao;
        public DashboardApiController(IPageConfigDao pageConfigDao, IDashboardConfigDao dashboardConfigDao) {
            this.pageConfigDao = pageConfigDao;
            this.dashboardConfigDao = dashboardConfigDao;
        }

        //Return JSON From DataTable
        [CustomAuthorize(TaskIds ="all")]
        public async Task<JsonResult> GetWidgetDetails(FormCollection collection, string taskId = null , string divId = null) {

            if (!CurrentUser.HasTask(taskId)) {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.End();
                return null;
            }

            var lst = await pageConfigDao.GetResultDashboardForDivIdAsync(divId, new PageSqlConfig(taskId,"","", "fldBankCode = @fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}), collection);
            return Json(lst, JsonRequestBehavior.AllowGet);
        }

    }
}