using INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturn.Base;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.Areas.OCS.Models.DataProcess;
//using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Areas.OCS.Models.InwardReturn;
using INCHEQS.Processes.DataProcess;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturn
{

    public class InwardReturnImportController : InwardReturnBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public InwardReturnImportController(IPageConfigDaoOCS pageConfigDao, OCSIDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao, IInwardReturnDao inwardReturnDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, micrImageDao, fileInformationService, sequenceDao, systemProfileDao, inwardReturnDao)
        {
        }

    }
}