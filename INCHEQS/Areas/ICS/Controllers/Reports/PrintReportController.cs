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

namespace INCHEQS.Areas.ICS.Controllers.Reports {
    public class PrintReportController : ReportBaseController {
        protected override PageSqlConfig setupPageSqlConfig() {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            string printReportParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "p");

            return new PageSqlConfig(taskId, "","","",null,"", printReportParam);
            //return new PageSqlConfig(taskId, "", sqlConfig.SqlOrderBy, sqlConfig.SqlExtraCondition, sqlConfig.SqlExtraConditionParams, "", printReportParam);
        }

        public PrintReportController(IAuditTrailDao auditTrailDao,IPageConfigDao pageConfigDao, IReportService reportService) : base(auditTrailDao,pageConfigDao, reportService) {}
        

    }
}