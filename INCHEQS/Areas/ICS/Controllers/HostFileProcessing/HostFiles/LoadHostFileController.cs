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
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostFiles {
    
    public class LoadHostFileController : HostFileBaseController {

        protected override PageSqlConfig setupPageSqlConfig() {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId);
        }

        public LoadHostFileController(IPageConfigDao pageConfigDao,
             IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, hostFileDao) {
        }

        public virtual JsonResult LoadHostFile(FormCollection collection) {
            initializeBeforeAction();

            string notice = "";
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string clearDate = collection["fldClearDate"];
            string fileName = processName;
            string reUpload = "Y";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";

            //List<String> errorMessages = ValidateHostFile(collection);

            //if ((errorMessages.Count > 0)) {
            //    notice = "ERROR:<br/>" + string.Join("<br/>", errorMessages.ToArray());
            //} else {

                try {
                    bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                    if (runningProcess) {

                        //Delete previous data process
                        dataProcessDao.DeleteDataProcess(processName, fileName, CurrentUser.Account.BankCode);
                        //Insert new data process
                        dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, fileName, clearDate, reUpload, taskId, batchId,  CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        fileManagerDao.UpdateFileManagerLoadFile(fileName, clearDate);

                        auditTrailDao.Log("Load Host File (Details) - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                        notice = "Loading Data";

                    } else {
                        notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                } catch (Exception ex) {
                    systemProfileDao.Log("LoadHostFileController/LoadHostFile error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw ex;
                }
           // }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }



        public virtual JsonResult UploadMultiple(IEnumerable<HttpPostedFileBase> file, FormCollection col)
        {
            initializeBeforeAction();
            HostFileModel hostFileModel = hostFileDao.GetDataFromHostFileConfig(pageSqlConfig.TaskId);

            string taskId = pageSqlConfig.TaskId;
            string clearDate = col["fldClearDate"];
            string notice = "";
            string cDate = "";

            cDate = clearDate.Replace("-", "/");
            DateTime date = DateTime.ParseExact(cDate, "dd/MM/yyyy", null);
            cDate = date.ToString("yyyyMMdd");

            //Folder section
            string path = systemProfileDao.GetValueFromSystemProfile(hostFileModel.fldSystemProfileCode, CurrentUser.Account.BankCode);

            foreach (var item in file)
            {
                if (item != null && item.ContentLength > 0)
                {

                    bool exists = System.IO.Directory.Exists(path+ @"\"+cDate);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(path + @"\" + cDate);

                    //Upload File into path
                    //item.SaveAs(Path.Combine(Path.Combine(@path + @"\" + cDate + @"\", item.FileName)));
                    item.SaveAs( item.FileName);

                    //Upload File information into database
                    bool fileExist = fileManagerDao.CheckFileExist(CurrentUser.Account, item.FileName);
                    if (!fileExist)
                    {
                        //fileManagerDao.InsertToFileManager(CurrentUser.Account, taskId, path + @"\" + item.FileName, item.FileName, clearDate);
                        fileManagerDao.InsertToFileManager(CurrentUser.Account, taskId, item.FileName, item.FileName, clearDate);
                    }
                    else
                    {
                        fileManagerDao.UpdateFileManager(CurrentUser.Account, path);
                    }

                    notice = "Successfully uploaded";

                }
                else
                {
                    notice = "No File Uploaded";
                }
            }

            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);

        }

        ////public List<string> ValidateHostFile(FormCollection col) {
        ////    List<string> err = new List<string>();
            
        ////    if (string.IsNullOrEmpty(col["radioButton"])) {
        ////        err.Add("Please select a file to load. File can be manually upload if file doesn't exist");
        ////    }

        ////    return err;
        //}



    }
}