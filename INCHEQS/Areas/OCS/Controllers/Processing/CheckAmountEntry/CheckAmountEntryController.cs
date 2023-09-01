using INCHEQS.Areas.OCS.Controllers.Processing.CheckAmountEntry.Base;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Areas.OCS.Models.CheckAmountEntry;

namespace INCHEQS.Areas.OCS.Controllers.Processing.CheckAmountEntry
{

    public class CheckAmountEntryController : CheckAmountEntryBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public CheckAmountEntryController(ICheckAmountEntryDao checkAmountEntryDao, IPageConfigDaoOCS pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao) : base(checkAmountEntryDao, pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, micrImageDao, fileInformationService, sequenceDao, systemProfileDao)
        {
        }

    }
}