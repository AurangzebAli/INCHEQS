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
using INCHEQS.Areas.OCS.Models.InwardReturnICL;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturnICL.Base;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturnICL
{

    public class InwardReturnICLController : InwardReturnICLBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return new PageSqlConfig(taskId);
        }

        public InwardReturnICLController(IPageConfigDaoOCS pageConfigDao, OCSIDataProcessDao OCScleardataProcess,  IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, SystemProfileDao systemProfileDao, IInwardReturnICLDao inwardReturnICLDao) : base(pageConfigDao, OCScleardataProcess, auditTrailDao, searchPageService, micrImageDao, fileManagerDao, fileInformationService, sequenceDao, systemProfileDao, inwardReturnICLDao)
        {
        }

    }
}