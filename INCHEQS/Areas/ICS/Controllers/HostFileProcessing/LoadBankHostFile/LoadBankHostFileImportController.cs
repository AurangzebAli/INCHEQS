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
using INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile.Base;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile;
using INCHEQS.Areas.ICS.Models.ProgressStatus;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile
{

    public class LoadBankHostFileController : LoadBankHostFileBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public LoadBankHostFileController(IPageConfigDao pageConfigDao, ICSIDataProcessDao ICScleardataProcess,  IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadBankHostFileDao LoadBankHostFileDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao, IProgressStatusDao progressStatusDao) : base(pageConfigDao, ICScleardataProcess, auditTrailDao, searchPageService, LoadBankHostFileDao, fileManagerDao, fileInformationService, sequenceDao, systemProfileDao, progressStatusDao)
        {
        }

    }
}