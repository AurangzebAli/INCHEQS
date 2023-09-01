//using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Areas.ICS.Models.LoadNcf;
using INCHEQS.Common;
using INCHEQS.Common.Resources;
using INCHEQS.Helpers;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.Sequence;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using INCHEQS.Areas.ICS.Models.ProgressStatus;


namespace INCHEQS.Areas.ICS.Controllers.Utilities.LoadNcf.Base
{
    public abstract class LoadNcfBaseController : BaseController
    {
        protected readonly IPageConfigDao pageConfigDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;

        // protected readonly IClearingItemDao IClearingItemDao;
        //protected readonly IHostFileDao hostFileDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly SequenceDao sequenceDao;
        protected readonly ILoadNcfDao loadNcfDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IProgressStatusDao progressStatusDao;


        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected LoadNcfModel gLoadNcfModel { get; private set; }

        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }

        //Must be implemented by inheriting class
        protected abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;

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
            gLoadNcfModel = loadNcfDao.GetDataFromMICRImportConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

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
                systemProfileDao.Log("LoadNcfBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public LoadNcfBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadNcfDao loadNcfDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IProgressStatusDao progressStatusDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.loadNcfDao = loadNcfDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.progressStatusDao = progressStatusDao;
          

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadNcf.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {
            CurrentUser.Account.TaskId = TaskIdsICS.LoadNcf.INDEX;
            string fromFolder = loadNcfDao.ListFolderPathFrom();
            string clearDate = progressStatusDao.getClearDate();
            string fileNameclearDate = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            if (Directory.Exists(fromFolder))
            {
                ViewBag.FolderList = fileInformationService.GetAllFileInFolderWithPathLoadNCF(fromFolder, 0, ".txt", fileNameclearDate);
                if (ViewBag.FolderList.Count > 0)
                {
                    ViewBag.Button = "";
                }
                else
                {
                    ViewBag.Button = "hidden";
                }
            }
            else
            {
                ViewBag.Button = "hidden";
            }

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadNcf.INDEX));
            //auditTrailDao.SecurityLog("Access Load Non Conformance File Download & Import", "", TaskIdsICS.LoadNcf.INDEX, CurrentUser.Account);
            auditTrailDao.Log("Access Load Non Conformance File Download & Import",  CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadNcf.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            
            string t = collection["Type"];
            if (t == null)
            {
                t = "Summary";
            }
            else
            {
                t = collection["Type"];
            }

           
             if (t.Equals("Summary"))
            {

                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadNcf.INDEX, "View_LoadNcfFile", "fldIssuingBank", "", new[] {
                    new SqlParameter("fldUserID", CurrentUser.Account.UserId)}),
             collection);

            }
            else if(t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadNcf.DOWNLOAD, "View_LoadNcfProcess", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            }


            return View();

        }


        public ActionResult GetMICRItemList(FormCollection collection)
        {
            ViewBag.NcfHistory = loadNcfDao.InwardItemList(collection);
            return PartialView("Modal/_InwardItemListPopup");

        }
        
        [CustomAuthorize(TaskIds = TaskIdsICS.LoadNcf.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Import(FormCollection col)
        {
            //string chkImport = "";
            string notice = "";

           // gLoadNcfModel = loadNcfDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadNcf.INDEX, CurrentUser.Account.BankCode);

                //string posPayType = gLoadNcfModel.fldPosPayType;
                string posPayType = "LoadClearingHouseNCFFile";
                string clearDate = col["fldProcessDate"];
                string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                string processName = "Download & Import Clearing House Nonconformance File";
                string taskId = TaskIdsICS.LoadNcf.INDEX;
                CurrentUser.Account.TaskId = taskId;
                string batchId = "";
                string reUpload = "N";
                string filename = "";
                string fileNameclearDate = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");

            

                string fromFolder = loadNcfDao.ListFolderPathFrom();
                string tofolder = loadNcfDao.ListFolderPathTo();
                //int dateSubString = gLoadNcfModel.fldDateSubString;
                //int bankCodeSubString = gLoadNcfModel.fldBankCodeSubString;
                //string fileExt = ".txt";
                ////int count = 0;
                //string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
                //List<string> errorMessages = new List<string>();
                ////int Checking = 0;


                //string currentProcess = "1";
                //int ItemBatch = 10;

          
            DirectoryInfo d = new DirectoryInfo(fromFolder);//Assuming Test is your Folder
            if (Directory.Exists(fromFolder))
            {
                ViewBag.FolderList = fileInformationService.GetAllFileInFolderWithPathLoadNCF(fromFolder, 0, ".txt", fileNameclearDate);
                if (ViewBag.FolderList.Count > 0)
                {
                    bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode);

                    //Do process
                    if (runningProcess)
                    {

                        //Insert new data process
                        cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        //auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.LoadNcf.INDEX, CurrentUser.Account);
                        auditTrailDao.Log("[Load NCF File] Perform Download and Import",  CurrentUser.Account);

                        TempData["Notice"] = Locale.PerformDownloadandImport; //Perform Download and Import
                    }
                    else
                    {
                        TempData["Warning"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }


                }
                else
                { TempData["ErrorMsg"] = "NC File is not available"; }
            }
            else
            { TempData["ErrorMsg"] = "Please Check GWC Folder is accessible"; }



            //return RedirectToAction("Index");
            //return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadNcf.INDEX)]
        [HttpPost()]
        public virtual ActionResult DownloadFile(FormCollection collection)
        {
            string taskId = TaskIdsICS.LoadNcf.INDEX;

            CurrentUser.Account.TaskId = taskId;
            //initializeBeforeAction();

            string filePath = "";
            string clearDate = collection["fldProcessDate"];
            string bankcode = collection["this_PresentingBankCode"];
            string fileName = collection["this_fldFileName"];
            string folderFileName = systemProfileDao.GetValueFromSystemProfile("GWCLocalIRFolder", bankcode);
            string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            filePath = folderFileName + dateClearing + "\\" + fileName;

            string fileBatch = collection["this_fldIRDBatch"];
            string fullFileName = string.Format("{0}", filePath);
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
                //auditTrailDao.SecurityLog("[Inward Return] Download ICL File :  " + fullFileName, "", TaskIdsICS.LoadNcf.INDEX, CurrentUser.Account);
                auditTrailDao.Log("[Load NCF File] Download NCF File :  " + fullFileName, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                TempData["Warning"] = Locale.Nodatawasselected;
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult GWCListing(FormCollection col)
        {
            //get list of folders in gwc  based on clearing date


            //initializeBeforeAction();
            //gLoadNcfModel = loadNcfDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadNcf.INDEX, CurrentUser.Account.BankCode);
            ViewBag.FolderPathFrom = loadNcfDao.ListFolderPathFrom();
            ViewBag.FolderPathTo = loadNcfDao.ListFolderPathTo();

            string clearDate = col["fldProcessDate"];
            string fileNameclearDate = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            string folderPath = loadNcfDao.ListFolderPathFrom();
            //string fileExt = gLoadNcfModel.fldFileExt;

            if (Directory.Exists(folderPath))
            {
                List<FolderInformationModel> foldername = new List<FolderInformationModel>();
                foldername = fileInformationService.GetGWCFolderICS(folderPath, clearDate, ".txt");
                ViewBag.FolderList = fileInformationService.GetAllFileInFolderWithPathLoadNCF(folderPath, 0, ".txt", fileNameclearDate);
                if (ViewBag.FolderList.Count > 0)
                {
                    for (int i = 0; i < ViewBag.FolderList.Count; i++)
                    {
                        string name = ViewBag.FolderList[i].fileName;
                        //folderPath = folderPath +"'\'";
                        ViewBag.FolderList[i].fileName = name.Replace(folderPath + "\\", "");
                    }
                }
            }
            
            return View();
        }

    }
}
