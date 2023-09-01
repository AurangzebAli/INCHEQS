using INCHEQS.Processes.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Web.Mvc;
using INCHEQS.Models.OCSAIFFileUploadDao;
using INCHEQS.TaskAssignment;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance {
    
    public class OCSAIFFileUploadController : Maintenance.AIFFileBaseController
    {

        protected override PageSqlConfig setupPageSqlConfig() {
            string taskId = TaskIdsOCS.AIFFileUpload.INDEX;

            return new PageSqlConfig(taskId);
        }

        public OCSAIFFileUploadController(IPageConfigDao pageConfigDao,
             IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IOCSAIFFileUploadDao AIFFileDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, AIFFileDao) {
        }

        public virtual JsonResult LoadAIFFile(FormCollection collection) {
            initializeBeforeAction();

            string notice = "";
            string processName = gAIFFileModel.fldProcessName;
            string posPayType = gAIFFileModel.fldPosPayType;
            string clearDate = collection["fldClearDate"];
            string fileName = processName;
            string reUpload = "Y";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";
            string AIFType = collection["radioAIF"].ToString();

            try {
                    bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                    if (runningProcess) {

                        //Delete previous data process
                        dataProcessDao.DeleteDataProcess(processName, AIFType, CurrentUser.Account.BankCode);
                        //Insert new data process
                        dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, AIFType, clearDate, reUpload, taskId, batchId,  CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                       
                        auditTrailDao.Log("Load AIF File (Details) - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                        notice = "Loading Data";

                    } else {
                        notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                } catch (Exception ex) {
                    systemProfileDao.Log("OCSAIFFileUploadController/LoadAIFFile error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw ex;
                }
           // }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

    }
}