using INCHEQS.Areas.ICS.Models.LoadDailyFile;
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


namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.LoadDailyFile.Base
{
    public abstract class LoadDailyFileBaseController : BaseController
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
        protected readonly ILoadDailyFileDao LoadDailyFileDao;
        protected readonly IFileManagerDao fileManagerDao;


        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected LoadDailyFileModel gLoadDailyFileModelA { get; private set; }
        protected LoadDailyFileModel gLoadDailyFileModelB { get; private set; }

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
            gLoadDailyFileModelA = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode, "DDYF");
            gLoadDailyFileModelB = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode, "DAHF");

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
                systemProfileDao.Log("LoadDailyFileBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public LoadDailyFileBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadDailyFileDao LoadDailyFileDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.LoadDailyFileDao = LoadDailyFileDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
          

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadDailyFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {
            CurrentUser.Account.TaskId = TaskIdsICS.LoadDailyFile.INDEX;
            //ViewBag.FolderPathFrom = micrImageDao.ListFolderPathFrom();
            //ViewBag.FolderPathTo = micrImageDao.ListFolderPathTo();
            //ViewBag.FolderPathToCompleted = micrImageDao.ListFolderPathToCompleted();

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadDailyFile.INDEX));
            //auditTrailDao.SecurityLog("Access Load Daily File", "", TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account);
            auditTrailDao.Log("Access Load Daily File", CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadDailyFile.INDEX)]
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

            if (t.Equals("List"))
            {

                ViewBag.Type = t;
                string clearDate = collection["fldClearingDate"];
                clearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

                gLoadDailyFileModelA = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account.BankCode, "DDYF");
                int bankSubStringDDYF = gLoadDailyFileModelA.fldBankCodeSubString;
                int dateSubStringDDYF = gLoadDailyFileModelA.fldDateSubString;
                int bankSubStringCompletedDDYF = gLoadDailyFileModelA.fldBankCodeSubStringCompleted;
                int dateSubStringCompletedDDYF = gLoadDailyFileModelA.fldDateSubStringCompleted;
                ViewBag.FolderPathFromDDYF = LoadDailyFileDao.ListFolderPathFrom("DDYF", clearDate);
                ViewBag.FolderPathToDDYF = LoadDailyFileDao.ListFolderPathTo("DDYF");

                string clearDateDDYF = collection["fldClearingDate"];
                string folderPathDDYF = LoadDailyFileDao.ListFolderPathFrom("DDYF", clearDate);
                string fileExtDDYF = gLoadDailyFileModelA.fldFileExt;

                if (Directory.Exists(folderPathDDYF))
                {

                    ViewBag.FileListA = fileInformationService.GetAllFileInFolderLoadDailyFile(folderPathDDYF, CurrentUser.Account.BankCode, clearDateDDYF, fileExtDDYF);

                }

                //from dahf

                gLoadDailyFileModelB = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account.BankCode, "DAHF");
                ViewBag.FolderPathFromDAHF = LoadDailyFileDao.ListFolderPathFrom("DAHF", clearDate);
                ViewBag.FolderPathToDAHF = LoadDailyFileDao.ListFolderPathTo("DAHF");
                int bankSubStringDAHF = gLoadDailyFileModelA.fldBankCodeSubString;
                int dateSubStringDAHF = gLoadDailyFileModelA.fldDateSubString;
                int bankSubStringCompletedDAHF = gLoadDailyFileModelA.fldBankCodeSubStringCompleted;
                int dateSubStringCompletedDAHF = gLoadDailyFileModelA.fldDateSubStringCompleted;
                string clearDateDAHF = collection["fldClearingDate"];
                string folderPathDAHF = LoadDailyFileDao.ListFolderPathFrom("DAHF", clearDate);
                string fileExtDAHF = gLoadDailyFileModelB.fldFileExt;

                if (Directory.Exists(folderPathDAHF))
                {

                    ViewBag.FileListB = fileInformationService.GetAllFileInFolderLoadDailyFile(folderPathDAHF, CurrentUser.Account.BankCode, clearDateDAHF, fileExtDAHF);

                }

                return View();

                //   ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadDailyFile.INDEX, "View_LoadDailyFileList", "", "", new[] {
                //       new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                //collection);

            }
            else if (t.Equals("Summary"))
            {

                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadDailyFile.DOWNLOAD, "View_LoadDailyFileSummary", "", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);

            }
            else if(t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadDailyFile.STATUS, "View_LoadDailyFileStatus", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            }


            return View();

        }


        public ActionResult GetLoadDailyFileList(FormCollection collection)
        {
            string results = collection["row_fldAction"];
            string FileType = results.Substring(results.Length - 2);
            string FileId = results.Substring(0, results.Length - 2);

            if (FileType == "BT")
            {
                ViewBag.FileList = LoadDailyFileDao.LoadDailyFileItemList(FileId, FileType);
                return PartialView("Modal/_BankTypeItemListPopup");

            }
            else if (FileType == "BF")
            {
                ViewBag.FileList = LoadDailyFileDao.LoadDailyFileItemList(FileId, FileType);
                return PartialView("Modal/_BankFileItemListPopup");
            }
            else if (FileType == "BR")
            {
                ViewBag.FileList = LoadDailyFileDao.LoadDailyFileItemList(FileId, FileType);
                return PartialView("Modal/_BranchItemListPopup");
            }
            else if (FileType == "TC")
            {
                ViewBag.FileList = LoadDailyFileDao.LoadDailyFileItemList(FileId, FileType);
                return PartialView("Modal/_TransCodeItemListPopup");
            }
            else if (FileType == "RC")
            {
                ViewBag.FileList = LoadDailyFileDao.LoadDailyFileItemList(FileId, FileType);
                return PartialView("Modal/_RejectCodeItemListPopup");
            }

            return null;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadDailyFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Run(FormCollection collection)
        {
            string processDate = collection["fldClearingDate"];

            processDate = DateTime.ParseExact(processDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");

            string notice = "";
            List<string> arrResults = new List<string>();
            if(collection["action"] != null)
            {
                arrResults = collection["action"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    string processName = arrResult;
                    if (processName.Contains("FIType"))
                    {
                        string filetype = "FIType";
                        LoadDailyFileDao.run(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, processDate, processName, filetype);
                    }
                    else if (processName.Contains("Bank"))
                    {
                        string filetype = "Bank";
                        LoadDailyFileDao.run(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, processDate, processName, filetype);
                    }
                    else if (processName.Contains("BRInfo"))
                    {
                        string filetype = "Branch";
                        LoadDailyFileDao.run(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, processDate, processName, filetype);
                    }
                    else if (processName.Contains("Transaction"))
                    {
                        string filetype = "Transaction";
                        LoadDailyFileDao.run(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, processDate, processName, filetype);
                    }
                    else if (processName.Contains("Return"))
                    {
                        string filetype = "Return";
                        LoadDailyFileDao.run(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, processDate, processName, filetype);
                    }
                }
                TempData["Notice"] = "Load Daily Files has been triggered";
                
            }
            else
            {
                TempData["ErrorMsg"] = "No Data was selected";
                
            }
            return RedirectToAction("Index");

        }


        [CustomAuthorize(TaskIds = TaskIdsICS.LoadDailyFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        //public JsonResult Import(FormCollection col)
        //{
        //    //string chkImport = "";
        //    string notice = "";

        //    gLoadDailyFileModelA = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account.BankCode, "DDYF");
            
        //    string posPayType = gLoadDailyFileModelA.fldPosPayType;
        //    string clearDate = col["fldProcessDate"];
        //    string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
        //    string processName = "ICSLoadDailyFile";
        //    string taskId = TaskIdsICS.MICRImage.INDEX;
        //    string batchId = "";
        //    string reUpload = "N";
        //    string filename = "";

        //    string fromFolder = LoadDailyFileDao.ListFolderPathFrom("DDYF");
        //    string tofolder = LoadDailyFileDao.ListFolderPathTo("DDYF");
        //    int dateSubString = gLoadDailyFileModelA.fldDateSubString;
        //    int bankCodeSubString = gLoadDailyFileModelA.fldBankCodeSubString;
        //    string fileExt = gLoadDailyFileModelA.fldFileExt;
        //    //int count = 0;
        //    string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
        //    List<string> errorMessages = new List<string>();
        //    //int Checking = 0;

        //    string currentProcess = "1";
        //    int ItemBatch = 10;

        //    DirectoryInfo d = new DirectoryInfo(fromFolder);//Assuming Test is your Folder
        //    if (Directory.Exists(fromFolder))
        //    {

        //        bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDate, CurrentUser.Account.BankCode);

        //        //Do process
        //        if (runningProcess)
        //        {
        //            //Insert new data process
        //            cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
        //            auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account);

        //            notice = Locale.PerformDownloadandImport; //Perform Download and Import
        //        }
        //        else
        //        {
        //            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
        //        }

        //    }
        //    else { notice = "Please Check GWC Folder is accessible"; }


        //    //return RedirectToAction("Index");
        //    return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        //}

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadDailyFile.INDEX)]
        [HttpPost()]
        //public virtual ActionResult DownloadFile(FormCollection collection)
        //{
        //    string taskId = TaskIdsICS.LoadDailyFile.DOWNLOAD;
        //    //initializeBeforeAction();

        //    string filePath = "";
        //    string clearDate = collection["fldClearingDate"];
        //    string bankcode = collection["fldIssuingBank"];
        //    string fileName = collection["this_fldFileName"];
        //    string folderFileName = systemProfileDao.GetValueFromSystemProfile("GWCLocalIRFolder", bankcode);
        //    string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
        //    filePath = folderFileName + dateClearing + "\\" + fileName;

        //    string fileBatch = collection["this_fldIRDBatch"];
        //    string fullFileName = string.Format("{0}", filePath);
        //    if (System.IO.File.Exists(fullFileName))
        //    {
        //        byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
        //        Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
        //        auditTrailDao.SecurityLog("[Inward Return] Download ICL File :  " + fullFileName, "", TaskIdsICS.MICRImage.INDEX, CurrentUser.Account);
        //        return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
        //    }
        //    else
        //    {
        //        TempData["Warning"] = Locale.Nodatawasselected;
        //    }

        //    return RedirectToAction("Index");
        //}

        public virtual ActionResult GWCListing(FormCollection col)
        {
            //get list of folders in gwc  based on clearing date
            //initializeBeforeAction();

            //from ddyf
            string clearDate = col["fldClearingDate"];
            clearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

            gLoadDailyFileModelA = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account.BankCode, "DDYF");
            int bankSubStringDDYF = gLoadDailyFileModelA.fldBankCodeSubString;
            int dateSubStringDDYF = gLoadDailyFileModelA.fldDateSubString;
            int bankSubStringCompletedDDYF = gLoadDailyFileModelA.fldBankCodeSubStringCompleted;
            int dateSubStringCompletedDDYF = gLoadDailyFileModelA.fldDateSubStringCompleted;
            ViewBag.FolderPathFromDDYF = LoadDailyFileDao.ListFolderPathFrom("DDYF", clearDate);
            ViewBag.FolderPathToDDYF = LoadDailyFileDao.ListFolderPathTo("DDYF");

            string clearDateDDYF = col["fldClearingDate"];
            string folderPathDDYF = LoadDailyFileDao.ListFolderPathFrom("DDYF", clearDate);
            string fileExtDDYF = gLoadDailyFileModelA.fldFileExt;

            if (Directory.Exists(folderPathDDYF))
            {

                ViewBag.FileListA = fileInformationService.GetAllFileInFolderLoadDailyFile(folderPathDDYF, CurrentUser.Account.BankCode, clearDateDDYF, fileExtDDYF);

            }

            //from dahf

            gLoadDailyFileModelB = LoadDailyFileDao.GetDataFromLoadDailyFileConfig(TaskIdsICS.LoadDailyFile.INDEX, CurrentUser.Account.BankCode, "DAHF");
            ViewBag.FolderPathFromDAHF = LoadDailyFileDao.ListFolderPathFrom("DAHF", clearDate);
            ViewBag.FolderPathToDAHF = LoadDailyFileDao.ListFolderPathTo("DAHF");
            int bankSubStringDAHF = gLoadDailyFileModelA.fldBankCodeSubString;
            int dateSubStringDAHF = gLoadDailyFileModelA.fldDateSubString;
            int bankSubStringCompletedDAHF = gLoadDailyFileModelA.fldBankCodeSubStringCompleted;
            int dateSubStringCompletedDAHF = gLoadDailyFileModelA.fldDateSubStringCompleted;
            string clearDateDAHF = col["fldClearingDate"];
            string folderPathDAHF = LoadDailyFileDao.ListFolderPathFrom("DAHF", clearDate);
            string fileExtDAHF = gLoadDailyFileModelB.fldFileExt;

            if (Directory.Exists(folderPathDAHF))
            {

                ViewBag.FileListB = fileInformationService.GetAllFileInFolderLoadDailyFile(folderPathDAHF, CurrentUser.Account.BankCode, clearDateDAHF, fileExtDAHF);

            }

            return View();
        }

    }
}
