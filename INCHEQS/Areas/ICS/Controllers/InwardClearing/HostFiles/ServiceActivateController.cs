using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.HostFile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.Models.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.HostFile;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Resources;
using INCHEQS.Security;
//using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostFiles {
    
    public class ServiceActivateController : HostFileBaseController {
        private ImageHelper imageHelper;

        protected override PageSqlConfig setupPageSqlConfig() {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId);
        }

        public ServiceActivateController(IPageConfigDao pageConfigDao,
             IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao, ImageHelper imageHelper) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, hostFileDao)
        {
            this.imageHelper = imageHelper;
        }

        public virtual JsonResult StartProcess(FormCollection collection) {
            initializeBeforeAction();

            string notice = "";
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string clearDate = collection["fldClearDate"];
            string reUpload = "";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";

            List<String> errorMessages = Validate(collection);
            if (processName == "ICSTestGif")
            {
                DataTable result = dataProcessDao.GenGif(processName, clearDate, CurrentUser.Account.BankCode);
                List<string> resultList = new List<string>();
                foreach (DataRow row in result.Rows)
                {
                    ImageHelper.ImageInfo imageInfo = getImageInfo(row["fldUIC"].ToString(), "F", row["fldimagefolder"].ToString());
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, imageInfo.sizeScale, imageInfo.angle, imageInfo.filter);
                }
            }
            if ((errorMessages.Count > 0)) {
                notice = "ERROR:<br/>" + string.Join("<br/>", errorMessages.ToArray());
            } else {
                try {
                    bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                    if (runningProcess) {

                        //Delete previous data process
                        dataProcessDao.DeleteDataProcessWithoutPosPayType(processName, CurrentUser.Account.BankCode);
                        //Insert new data process
                        dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                        auditTrailDao.Log("Activate Service Start - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                        notice = "Process Started";

                    } else {
                        notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                } catch (Exception ex) {
                    systemProfileDao.Log("ServiceActiveController/StartProcess error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw ex;
                }
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }
        public virtual JsonResult StartProcessGenerateFile(FormCollection collection)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string posPayTypeSFTP = "SFTP";
            string clearDate = collection["fldClearDate"];
            string reUpload = "";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";
            bool runningProcess;
            int sProcess = 0;

            List<String> errorMessages = Validate(collection);

            if ((errorMessages.Count > 0))
            {
                notice = "ERROR:<br/>" + string.Join("<br/>", errorMessages.ToArray());
            }
            else
            {
                try
                {

                    bool checkProcessDate = dataProcessDao.CheckProcessDateWithinRetentionPeriod(clearDate, sProcess, CurrentUser.Account.BankCode);
                    if (checkProcessDate)
                    {
                        //bool runningProcess = dataProcessDao.CheckRunningProcessGenerateFile(processName, posPayType, clearDate);
                        runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                        //runningProcess = dataProcessDao.CheckRunningProcess(processName, posPayTypeSFTP, clearDate);
                        if (runningProcess)
                        {

                            //Delete previous data process
                            dataProcessDao.DeleteDataProcessWithoutPosPayType(processName, CurrentUser.Account.BankCode);
                            //Insert new data process
                            dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                            auditTrailDao.Log("Activate Service Start - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                            notice = "Process Started";

                        }
                        else
                        {
                            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                        }
                    }
                    else
                    {
                        notice = Locale.NotAllowedForGenerate;
                    }
                }
                catch (Exception ex)
                {
                    systemProfileDao.Log("ServiceActiveController/StartProcess error :" + ex.Message,CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw ex;
                }
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult ReStartSFTPProcess(FormCollection collection)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gHostFileModel.fldProcessName;
            string posPayType = "SFTP";
            string clearDate = collection["fldClearDate"];
            string reUpload = "";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";
            int sProcess = 1;

            List<String> errorMessages = Validate(collection);

            if ((errorMessages.Count > 0))
            {
                notice = "ERROR:<br/>" + string.Join("<br/>", errorMessages.ToArray());
            }
            else
            {
                try
                {
                    bool checkProcessDate = dataProcessDao.CheckProcessDateWithinRetentionPeriod(clearDate, sProcess, CurrentUser.Account.BankCode);
                    if (checkProcessDate)
                    {
                        bool runningProcess = dataProcessDao.CheckRunningProcess(processName, posPayType, clearDate, CurrentUser.Account.BankCode);
                        if (runningProcess)
                        {

                            //Delete previous data process
                            dataProcessDao.DeleteDataProcessWithPosPayType(processName, posPayType, CurrentUser.Account.BankCode);
                            //Insert new data process
                            dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                            auditTrailDao.Log("Activate Service Re-Start SFTP - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                            notice = "Process Started";

                        }
                        else
                        {
                            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                        }
                    }
                    else
                    {
                        notice = Locale.OutOfRentionPeriod;
                    }
                }
                catch (Exception ex)
                {
                    systemProfileDao.Log("ServiceActiveController/StartProcess error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw ex;
                }
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public List<string> Validate(FormCollection col) {
            List<string> err = new List<string>();

            if (string.IsNullOrEmpty(col["fldClearDate"])) {
                err.Add("Please select clear date");
            }

            return err;
        }

        [NonAction]
        private ImageHelper.ImageInfo getImageInfo(string imageId, string imageState, string imageFolder = null)
        {
            if (imageFolder == null)
            {
                imageFolder = systemProfileDao.GetValueFromSystemProfile("InwardImageFolder", CurrentUser.Account.BankCode);
            }

            if ((string.IsNullOrEmpty(imageFolder) | string.IsNullOrEmpty(imageId)))
            {
                return null;
            }

            List<string> states = imageState.Split(',').ToList();
            ImageHelper.ImageInfo imageInfo = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);
            return imageInfo;

        }

    }
}