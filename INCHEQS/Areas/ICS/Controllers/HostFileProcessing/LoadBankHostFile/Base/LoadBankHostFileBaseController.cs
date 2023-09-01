//using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile;
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

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile.Base
{
    public abstract class LoadBankHostFileBaseController : BaseController
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
        protected readonly ILoadBankHostFileDao LoadBankHostFileDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IProgressStatusDao progressStatusDao;


        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected LoadBankHostFileModel gLoadBankHostFileModel { get; private set; }

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
            gLoadBankHostFileModel = LoadBankHostFileDao.GetDataFromMICRImportConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

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
                systemProfileDao.Log("LoadBankHostFileBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public LoadBankHostFileBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadBankHostFileDao LoadBankHostFileDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IProgressStatusDao progressStatusDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.LoadBankHostFileDao = LoadBankHostFileDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.progressStatusDao = progressStatusDao;


        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {
            string staskid = TaskIdsICS.LoadBankHostFile.INDEX;
            CurrentUser.Account.TaskId = staskid;
            string tofolder = LoadBankHostFileDao.ListFolderPathTo();
            string clearDate = progressStatusDao.getClearDate();
            string loadHostFileClearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).AddDays(1).ToString("yyyyMMdd");
            string loadHostFilePath = Path.Combine(tofolder, loadHostFileClearDate + ".txt");
               
            if (Directory.Exists(tofolder))
            {
                if (System.IO.File.Exists(loadHostFilePath))
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

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadBankHostFile.INDEX));
            auditTrailDao.Log("Access Load Non Conformance File Download & Import", CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadBankHostFile.INDEX, "View_LoadBankHostFile", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);

            }
            else if (t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadBankHostFile.DOWNLOAD, "View_LoadBankHostFileProcess", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            } else if (t.Equals("Overview")) {
                ViewBag.Type = t;
                string clearDate = collection["fldProcessDate"];
                string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                //ViewBag.ProgressStatus = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadBankHostFile.INDEX));
                ViewBag.ProgressStatus = LoadBankHostFileDao.ReturnProgressStatusConventional(clearDateDD);
                //ViewBag.ProgressStatusIslam = LoadBankHostFileDao.ReturnProgressStatusIslamic(clearDateDD);
                return View();
            }


            return View();

        }


        public ActionResult GetMICRItemList(FormCollection collection)
        {
            ViewBag.NcfHistory = LoadBankHostFileDao.InwardItemList(collection);

            //if (ViewBag.NcfHistory.Rows.Count > 0)
            //{
            //    for (int i = 0; i < ViewBag.NcfHistory.Rows.Count; i++)
            //    {
            //        string value = "hi";
            //        if (ViewBag.NcfHistory.Rows[i].ItemArray[9] == "03") {
            //            ViewBag.NcfHistory.Rows[i].ItemArray[9] = value;
            //        } 
            //        else if (ViewBag.NcfHistory.Rows[i].ItemArray[9] == "04")
            //        {
            //            ViewBag.NcfHistory.Rows[i].ItemArray[9] = "04 - First Presented Cheque";
            //        }
            //        else if (ViewBag.NcfHistory.Rows[i].ItemArray[9] == "05")
            //        {
            //            ViewBag.NcfHistory.Rows[i].ItemArray[9] = "05 - Data Correction";
            //        }
            //        else 
            //        {
            //            ViewBag.NcfHistory.Rows[i].ItemArray[9] = "06 - Unposted";
            //        }

            //    }
            //}
            return PartialView("Modal/_InwardItemListPopup");

        }
        
        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public JsonResult Import(FormCollection col)
        {
            //string chkImport = "";
            string notice = "";

            //gLoadBankHostFileModel = LoadBankHostFileDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadBankHostFile.INDEX, CurrentUser.Account.BankCode);

            //string posPayType = gLoadBankHostFileModel.fldPosPayType;
            string posPayType = "LoadHostStatusFile";
            string clearDate = col["fldProcessDate"];
            string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
            string processName = "Download & Import Host Status File";
            string taskId = TaskIdsICS.LoadBankHostFile.INDEX;
            string batchId = "";
            string reUpload = "N";
            string filename = "";
            //string staskid = TaskIdsICS.LoadBankHostFile.INDEX;
            CurrentUser.Account.TaskId = taskId;



            string fromFolder = LoadBankHostFileDao.ListFolderPathFrom();
            string tofolder = LoadBankHostFileDao.ListFolderPathTo();
            //int dateSubString = gLoadBankHostFileModel.fldDateSubString;
            //int bankCodeSubString = gLoadBankHostFileModel.fldBankCodeSubString;
            //string fileExt = ".txt";
            ////int count = 0;
            //string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            //List<string> errorMessages = new List<string>();
            ////int Checking = 0;


            //string currentProcess = "1";
            //int ItemBatch = 10;

          
            DirectoryInfo d = new DirectoryInfo(tofolder);//Assuming Test is your Folder
            if (Directory.Exists(tofolder))
            {
                if (!(cleardataProcess.CheckDataProcessExist(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode)))
                {
                    bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode);

                    //Do process
                    if (runningProcess)
                    {

                        //Insert new data process
                        cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        //auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.LoadBankHostFile.INDEX, CurrentUser.Account);
                        auditTrailDao.Log("[Download & Import] Perform Download and Import", CurrentUser.Account);
                        notice = Locale.PerformDownloadandImport; //Perform Download and Import
                    }
                    else
                    {
                        notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                }
                else
                {
                    notice = "File Already Load Success";
                }
            }
            else { notice = "Please Check GWC Folder is accessible"; }
            
           
            //return RedirectToAction("Index");
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile.INDEX)]
        [HttpPost()]
        public virtual ActionResult DownloadFile(FormCollection collection)
        {
            string taskId = TaskIdsICS.LoadBankHostFile.DOWNLOAD;
            CurrentUser.Account.TaskId = taskId;
            //initializeBeforeAction();

            string filePath = "";
            string clearDate = collection["fldProcessDate"];
            string bankcode = collection["this_PresentingBankCode"];
            string fileName = collection["this_fldFileName"];
            string folderFileName = systemProfileDao.GetValueFromSystemProfile("GWCLocalIRFolder", bankcode);
            string dateClearing = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            filePath = folderFileName + dateClearing + "\\" + fileName;

            string fileBatch = collection["this_fldIRDBatch"];
            string fullFileName = string.Format("{0}", filePath);
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
                //auditTrailDao.SecurityLog("[Inward Return] Download ICL File :  " + fullFileName, "", TaskIdsICS.LoadBankHostFile.INDEX, CurrentUser.Account);
                auditTrailDao.Log("[Inward Return] Download ICL File :  " + fullFileName,CurrentUser.Account);
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
            //gLoadBankHostFileModel = LoadBankHostFileDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadBankHostFile.INDEX, CurrentUser.Account.BankCode);
            ViewBag.FolderPathFrom = LoadBankHostFileDao.ListFolderPathFrom();
            ViewBag.FolderPathTo = LoadBankHostFileDao.ListFolderPathTo();

            string clearDate = col["fldProcessDate"];
            string folderPath = LoadBankHostFileDao.ListFolderPathFrom();
            //string fileExt = gLoadBankHostFileModel.fldFileExt;

            if (Directory.Exists(folderPath))
            {
                List<FolderInformationModel> foldername = new List<FolderInformationModel>();
                foldername = fileInformationService.GetGWCFolderICS(folderPath, clearDate, ".txt");

                List<FileInformationModel> FolderList = new List<FileInformationModel>();
                FolderList = fileInformationService.GetAllFileInFolderWithPath(folderPath, 0, ".txt");

                if (FolderList.Count > 0)
                {
                    for (int i = 0; i < FolderList.Count; i++)
                    {
                        string name = FolderList[i].fileName;
                        //folderPath = folderPath +"'\'";
                        //ViewBag.FolderList[i].fileName
                        name = name.Replace(folderPath + "\\", "");
                        string index = name.Substring(0, 2);

                        //string filename = (DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).AddDays(1)).ToString("yyyyMMdd") + ".txt";

                        if (name != (DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).AddDays(1)).ToString("yyyyMMdd") + ".txt")
                        {
                            FolderList.RemoveAt(i);
                            --i;
                        }
                        else
                        {
                            FolderList[i].fileName = name;
                        }
                    }
                    ViewBag.FolderList = FolderList;
                }
            }

            return View();
        }

    }
}
