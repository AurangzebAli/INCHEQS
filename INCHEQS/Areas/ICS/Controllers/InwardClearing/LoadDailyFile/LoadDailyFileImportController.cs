using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Areas.ICS.Controllers.InwardClearing.LoadDailyFile.Base;
using INCHEQS.Areas.ICS.Models.LoadDailyFile;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.LoadDailyFile
{

    public class LoadDailyFileController : LoadDailyFileBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public LoadDailyFileController(IPageConfigDao pageConfigDao, ICSIDataProcessDao ICScleardataProcess,  IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadDailyFileDao LoadDailyFileDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao) : base(pageConfigDao, ICScleardataProcess, auditTrailDao, searchPageService, LoadDailyFileDao, fileManagerDao, fileInformationService, sequenceDao, systemProfileDao)
        {
        }

    }
}