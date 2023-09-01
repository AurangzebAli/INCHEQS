using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Helpers;
using INCHEQS.Controllers.Reports.Base;
using INCHEQS.Models.Report;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security;

namespace INCHEQS.Areas.OCS.Controllers.Reports
{
    public class DefaultReportController : ReportBaseController
    {
        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId, "");
        }

        public DefaultReportController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IReportService reportService) : base(auditTrailDao, pageConfigDao, reportService) { }
    }
}