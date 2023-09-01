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
    public class ICCSPrintReportController : ReportBaseController
    {
        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            string printReportParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "p");
            QueueSqlConfig sqlConfig = pageConfigDao.GetQueueConfig(taskId, CurrentUser.Account);

            return new PageSqlConfig(taskId, "", sqlConfig.SqlOrderBy, sqlConfig.SqlExtraCondition, sqlConfig.SqlExtraConditionParams, "", printReportParam);
        }

        public ICCSPrintReportController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IReportService reportService) : base(auditTrailDao, pageConfigDao, reportService) { }

    }
}