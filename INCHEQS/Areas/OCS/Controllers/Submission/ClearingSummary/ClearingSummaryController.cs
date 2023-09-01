using INCHEQS.Areas.OCS.Controllers.Submission.ClearingSummary.Base;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Areas.OCS.Models.Clearing;

namespace INCHEQS.Areas.OCS.Controllers.Submission.ClearingSummary
{

    public class ClearingSummaryController : ClearingSummaryBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public ClearingSummaryController(IClearingDao clearingDao, IPageConfigDaoOCS pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao) : base(clearingDao, pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, micrImageDao, fileInformationService, sequenceDao, systemProfileDao)
        {
        }

    }
}