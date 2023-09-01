using INCHEQS.Processes.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.Models.DataProcess;
using INCHEQS.Models.FileInformation;
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
using INCHEQS.Areas.OCS.Models.AIFImport;
using System.Data;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.AIFImport.Base
{
    public abstract class AIFImportBaseController : BaseController
    {

        protected readonly IPageConfigDaoOCS pageConfigDao;
        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IAIFImportDao aifImportDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly SequenceDao sequenceDao;
        protected readonly ISystemProfileDao systemProfileDao;

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
        protected AIFImportModel gAIFImportModel { get; private set; }

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
            gAIFImportModel = aifImportDao.GetDataFromHostFileConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

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
                systemProfileDao.Log("AIFImportBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public AIFImportBaseController(IPageConfigDaoOCS pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IAIFImportDao aifImportDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.aifImportDao = aifImportDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
        }

        public virtual async Task<ActionResult> Index()
        {
            initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.DataPosPayType = gAIFImportModel.fldPosPayType;
            ViewBag.ProcessName = gAIFImportModel.fldProcessName;

            List<AIFImportModel> result = await aifImportDao.GetDataFromFileMasterAsync();

            ViewBag.FileDetails = result;

            return View();
        }

        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection)
        {
            initializeBeforeAction();
            //ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);
            ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);
            return View();
        }

        public JsonResult Import(FormCollection col)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gAIFImportModel.fldProcessName;
            string posPayType = col["Type"];
            string clearDate = col["fldProcessDate"];
            string reUpload = "N";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";//TODO: unknown batch ID
            string folderName = systemProfileDao.GetValueFromSystemProfile(gAIFImportModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
            string fileExt = gAIFImportModel.fldFileExt;
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
            if (fileExt == ".TXT")
            {
                count = 1;
            }
            else
            {
                //Get file information
                fileList = fileInformationService.GetFileInFolder(folderName, clearDate, 0,0,fileExt, CurrentUser.Account.BankCode);
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
                    string sequenceTableName = "AIFImport";
                    int nextSequenceNo = sequenceDao.GetNextSequenceNo(sequenceTableName);
                    //modify by shamil 20170309
                    // only icl will insert date into tblinwardcleardate
                    if (fileExt == ".TXT")
                    {
                        //Insert new clear Date
                        dataProcessDao.InsertClearDate(clearDate, nextSequenceNo, CurrentUser.Account.BankCode);
                    }
                    //Delete previous data process for current bank code
                    dataProcessDao.DeleteDataProcess(processName, posPayType, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                    //Update sequence number
                    sequenceDao.UpdateSequenceNo(nextSequenceNo, sequenceTableName);

                    auditTrailDao.Log("Perform Download and Import", CurrentUser.Account);
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

    }
}