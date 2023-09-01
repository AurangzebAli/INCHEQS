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
using INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile2nd.Base;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile2nd;
using INCHEQS.Models.HostFile;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile2nd
{

    public class LoadBankHostFile2ndController : LoadBankHostFile2ndBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public LoadBankHostFile2ndController(IPageConfigDao pageConfigDao, ICSIDataProcessDao ICScleardataProcess,  IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadBankHostFile2ndDao LoadBankHostFile2ndDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao, IHostFileDao hostFileDao) : base(pageConfigDao, ICScleardataProcess, auditTrailDao, searchPageService, LoadBankHostFile2ndDao, fileManagerDao, fileInformationService, sequenceDao, systemProfileDao, hostFileDao)
        {
        }

    }
}