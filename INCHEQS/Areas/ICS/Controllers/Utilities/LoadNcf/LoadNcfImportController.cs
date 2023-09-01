using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
//using INCHEQS.Areas.ICS.Controllers.InwardClearing.MICRImage.Base;
//using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Areas.ICS.Controllers.Utilities.LoadNcf.Base;
using INCHEQS.Areas.ICS.Models.LoadNcf;
using INCHEQS.Areas.ICS.Models.ProgressStatus;

namespace INCHEQS.Areas.ICS.Controllers.Utilities.LoadNcf
{

    public class LoadNcfController : LoadNcfBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public LoadNcfController(IPageConfigDao pageConfigDao, ICSIDataProcessDao ICScleardataProcess,  IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadNcfDao loadNcfDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao, IProgressStatusDao progressStatusDao) : base(pageConfigDao, ICScleardataProcess, auditTrailDao, searchPageService, loadNcfDao, fileManagerDao, fileInformationService, sequenceDao, systemProfileDao, progressStatusDao)
        {
        }

    }
}