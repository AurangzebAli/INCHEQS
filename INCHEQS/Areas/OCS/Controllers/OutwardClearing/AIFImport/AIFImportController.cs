using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Areas.OCS.Models.AIFImport;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.AIFImport.Base
{

    public class AIFImportController : AIFImportBaseController
    {
        public AIFImportController(IPageConfigDaoOCS pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IAIFImportDao aifImportDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, aifImportDao, fileInformationService, sequenceDao, systemProfileDao)
        {
        }

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

    }
}