//using INCHEQS.Areas.OCS.Models.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Resources;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.InwardReturn;
using System.Data.SqlClient;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.MICRImage;

using System;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturn.Base
{
    public abstract class InwardReturnBaseController : BaseController
    {

        protected readonly IPageConfigDaoOCS pageConfigDao;
        protected readonly OCSIDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IMICRImageDao micrImageDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly SequenceDao sequenceDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IInwardReturnDao inwardReturnDao;

        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }

        //Must be implemented by inheriting class
        protected abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;
        protected string generateReportHtml;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected MICRImageModel gMicrImageModel { get; private set; }


        /// <summary>
        /// This function should be called inside All Actions in ICCSBaseController and it's Inheritence Controller.
        /// This is to protect from UNAUTHORIZED ACCESS to the page through TASKID returned by setupPageSqlConfig().
        /// This function is important and have to be called or else, the page won't work
        /// Returns: PageSqlConfig set in setupPageSqlConfig();
        /// </summary>
        [NonAction]
        protected PageSqlConfig initializeBeforeAction()
        {

            //Expose 'currentAction' to Children Controller so that it can be intercepted and add logics accordingly
            //currentAction is action URL accessed
            //currentFormCollection is FormCollection sent to URL accessed
            //The actions are:
            // - Index
            // - GenerateReport
            currentAction = currentAction != null ? currentAction : (string)ControllerContext.RouteData.Values["action"];
            currentFormCollection = new FormCollection(ControllerContext.HttpContext.Request.Form);


            //Initializ PageSqlConfig based on: 
            // - Inherited Controller initialization of setupPageSqlConfig()
            // - Request Query String of TaskId and ViewId
            pageSqlConfig = setupPageSqlConfig();

            gQueueSqlConfig = pageConfigDao.GetQueueConfig(setupPageSqlConfig().TaskId, CurrentUser.Account);

            //Constructor for MIRCimageModel
            gMicrImageModel = micrImageDao.GetDataFromMICRImportConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

            //ViewBagForCheckerMaker
            ViewBag.TaskRole = gQueueSqlConfig.TaskRole;

            //ViewBagForAllowedAction
            ViewBag.AllowedAction = gQueueSqlConfig.AllowedActions;

            try
            {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                 RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            }
            catch (HttpException ex)
            {
                systemProfileDao.Log("MICRImageBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public InwardReturnBaseController(IPageConfigDaoOCS pageConfigDao, OCSIDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IInwardReturnDao inwardReturnDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.micrImageDao = micrImageDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.inwardReturnDao = inwardReturnDao;
        }

        public virtual async Task<ActionResult> Index()
        {
            initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.DataPosPayType = gMicrImageModel.fldPosPayType;
            ViewBag.ProcessName = gMicrImageModel.fldProcessName;
            return View();
        }

        //public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
        //    initializeBeforeAction();
        //    //ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);
        //    ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);
        //    return View();
        //}

        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(setupPageSqlConfig().TaskId, "View_InwardReturnImport", "", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        public virtual ActionResult FileListing(FormCollection col)
        {
            initializeBeforeAction();
            string clearDate = col["fldcleardate"];
            string folderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
            int dateSubString = gMicrImageModel.fldDateSubString;
            int bankCodeSubString = gMicrImageModel.fldBankCodeSubString;

            ViewBag.FileList = fileInformationService.GetAllFileInFolder(folderName, bankCodeSubString, CurrentUser.Account.BankCode);

            return View();
        }


        public virtual ActionResult MatchListing(FormCollection col)
        {
            initializeBeforeAction();
            string clearDate = col["fldClearDate"];
            string MatchStatus = "Y"; //Y for Matched items
            ViewBag.FileList = inwardReturnDao.GetItemStatusListing(CurrentUser.Account.BankCode, clearDate, MatchStatus);

            return View();
        }

        public virtual ActionResult UnmatchListing(FormCollection col)
        {
            initializeBeforeAction();
            string clearDate = col["fldClearDate"];
            string MatchStatus = "N"; //N for Matched items
            ViewBag.FileList = inwardReturnDao.GetItemStatusListing(CurrentUser.Account.BankCode, clearDate, MatchStatus);

            return View();
        }

        public virtual ActionResult ExceptionFileListing(FormCollection col)
        {
            initializeBeforeAction();
            string clearDate = col["fldClearDate"];
            ViewBag.FileList = micrImageDao.GetErrorListFromICLException(clearDate);
            return View();
        }

        public virtual ActionResult PushImport(FormCollection col)
        {
            initializeBeforeAction();
            micrImageDao.Update(CurrentUser.Account.BankCode);
            return Json(new { notice = "Data transfer will start" }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult DownloadHistory(FormCollection col)
        {
            initializeBeforeAction();
            string clearDate = col["fldClearDate"];
            ViewBag.FileList = micrImageDao.GetErrorListFromICLException(clearDate);
            ViewBag.SearchResult = pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, col);
            return View();
        }

        public JsonResult DownloadImport(FormCollection col)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gMicrImageModel.fldProcessName;
            string posPayType = gMicrImageModel.fldPosPayType;
            string clearDate = col["fldClearDate"];
            string reUpload = "N";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";//TODO: unknown batchId
            string folderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
            int dateSubString = gMicrImageModel.fldDateSubString;
            int bankCodeSubString = gMicrImageModel.fldBankCodeSubString;
            string fileExt = gMicrImageModel.fldFileExt;

            //Formatting fileExt if type is "selectList", file et should take from select collection form
            List<FileInformationModel> fileList = new List<FileInformationModel>();
            if (fileExt.ToLower().Equals("selectlist"))
            {
                fileExt = col["fileType"].Substring(col["fileType"].Length - 4, 4);
            }

            //Get file information
            fileList = fileInformationService.GetFileInFolder(folderName, clearDate, dateSubString, bankCodeSubString, fileExt, CurrentUser.Account.BankCode);


            //If file exists
            if (fileList.Count > 0)
            {
                //Formatting fldPosPayType if type is "filename"
                if (posPayType.ToLower().Equals("filename"))
                {
                    posPayType = fileList[0].fileName;
                }

                //Check running process if not "" OR "4" then dont Do process
                bool runningProcess = dataProcessDao.CheckRunningProcess(processName, posPayType, clearDate, CurrentUser.Account.BankCode);

                //Do process
                if (runningProcess)
                {
                    string sequenceTableName = "tblinwardcleardate";
                    int nextSequenceNo = sequenceDao.GetNextSequenceNo(sequenceTableName);

                    //Insert new clear Date
                    dataProcessDao.InsertClearDate(clearDate, nextSequenceNo, CurrentUser.Account.BankCode);
                    //Delete previous data process
                    dataProcessDao.DeleteDataProcess(processName, posPayType, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                    //Update sequence number
                    sequenceDao.UpdateSequenceNo(nextSequenceNo, sequenceTableName);

                    //auditTrailDao.Log("Perform Download and Import", CurrentUser.Account);
                    auditTrailDao.SecurityLog("Perform Download and Import", "", taskId, CurrentUser.Account);

                    notice = Locale.PerformDownloadandImport;
                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            }
            else
            {
                notice = Locale.FileNotFound;
            }

            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Import(FormCollection col)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gMicrImageModel.fldProcessName;
            string posPayType = gMicrImageModel.fldPosPayType;
            string clearDate = col["fldClearDate"];
            string reUpload = "N";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";//TODO: unknown batch ID
            string folderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
            int dateSubString = gMicrImageModel.fldDateSubString;
            int bankCodeSubString = gMicrImageModel.fldBankCodeSubString;
            string fileExt = gMicrImageModel.fldFileExt;
            int count = 0;

            //Formatting fileExt if type is "selectList", file et should take from select collection form
            List<FileInformationModel> fileList = new List<FileInformationModel>();
            if (fileExt.ToLower().Equals("selectlist"))
            {
                fileExt = col["fileType"].Substring(col["fileType"].Length - 4, 4);
            }
            //modify by shamil 2070309
            //improve donwload as no need for system to get all file before download. fix timeout issue
            //all task by icl import will trigger service while task by eccs import will check the if the file exist or not
            // MICR FILE MMYR
            if (fileExt == ".MMYR")
            {
                count = 1;
            }
            else
            {
                //Get file information
                fileList = fileInformationService.GetFileInFolder(folderName, clearDate, dateSubString, bankCodeSubString, fileExt, CurrentUser.Account.BankCode);
                count = fileList.Count;
            }

            //If file exists
            if (count > 0)
            {
                //Formatting fldPosPayType if type is "filename"
                if (posPayType.ToLower().Equals("filename"))
                {
                    //posPayType = fileList[3].fileName;
                    posPayType = DateUtils.formatDateToFileDate(clearDate) + "-" + CurrentUser.Account.BankCode + "PHP00" + fileExt;
                }

                //Check running process if not "" OR "4" then dont Do process
                bool runningProcess = dataProcessDao.CheckRunningProcess(processName, posPayType, clearDate, CurrentUser.Account.BankCode);

                //Do process
                if (runningProcess)
                {

                    //Delete previous data process for current bank code
                    dataProcessDao.DeleteDataProcess(processName, posPayType, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                   // auditTrailDao.Log("Perform Import Inward Return", CurrentUser.Account);
                    auditTrailDao.SecurityLog("Perform Import Inward Return", "", taskId, CurrentUser.Account);
                    notice = Locale.PerformDownloadandImport;

                    dataProcessDao.UpdateToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, "2", "1", "Matching Process in progress");
                    //inwardReturnDao.InwardReturnFileRecordWithStatus(CurrentUser.Account.BankCode, clearDate);
                    inwardReturnDao.PerformMatching(CurrentUser.Account.UserId, CurrentUser.Account.BankCode, clearDate, "0");
                    dataProcessDao.UpdateToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, "4", "2", "Matching Process is completed");
                    //inwardReturnDao.InwardReturnFileRecordWithStatus(CurrentUser.Account.BankCode, clearDate);

                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            }
            else
            {
                notice = Locale.FileNotFound;
            }

            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        private int Int32(string userId)
        {
            throw new NotImplementedException();
        }



    }
}