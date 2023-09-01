using INCHEQS.Areas.ICS.Models.GenerateUPI;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.ICS.Models.HostValidation;
using INCHEQS.Areas.ICS.Models.MICRImage;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostValidation
{
    
        
        public class HostValidationController : HostValidationBaseController
        {
            protected override PageSqlConfig setupPageSqlConfig()
            {
                string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

                return new PageSqlConfig(taskId);
            }



        public HostValidationController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostValidationDao hostValidationDao,IMICRImageDao mICRImageDao) : base(pageConfigDao, cleardataProcess, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, hostValidationDao,mICRImageDao) { }
    }





}