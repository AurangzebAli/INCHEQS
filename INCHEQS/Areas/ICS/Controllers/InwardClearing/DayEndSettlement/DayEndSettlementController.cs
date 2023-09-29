

using INCHEQS.Areas.ICS.Controllers.InwardClearing.DayEndSettlement;
using INCHEQS.Areas.ICS.Models.DayEndSettlement;
using INCHEQS.Areas.ICS.Models.HostValidation;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.EOD.DayEndProcess;
using INCHEQS.Helpers;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.DayEndSettlement
{
    public class DayEndSettlementController : DayEndSettlementBaseController
    {
        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId);
        }
        public DayEndSettlementController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMICRImageDao mICRImageDao,IDayEndSettlementDao dayEndSettlementDao) : base(pageConfigDao, cleardataProcess, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, mICRImageDao,dayEndSettlementDao) { }
    }
}