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

namespace INCHEQS.Areas.PPS.Controllers.Reports
{
    public class PrintReportController : ReportBaseController
    {
        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            string printReportParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "p");
            if (taskId == "306280")
            {
                printReportParam = "all";
            }
            return new PageSqlConfig(taskId, "", "", "", null, "", printReportParam);
        }

        public PrintReportController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IReportService reportService) : base(auditTrailDao, pageConfigDao, reportService) { }


    }
}