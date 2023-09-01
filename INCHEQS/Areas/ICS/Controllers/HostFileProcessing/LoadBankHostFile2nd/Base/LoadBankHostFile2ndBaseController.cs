//using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Areas.ICS.Models.LoadBankHostFile2nd;
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
using INCHEQS.Models.HostFile;
using INCHEQS.Areas.ICS.Models.HostFile;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.LoadBankHostFile2nd.Base
{
    public abstract class LoadBankHostFile2ndBaseController : BaseController
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
        protected readonly ILoadBankHostFile2ndDao LoadBankHostFile2ndDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IHostFileDao hostFileDao;


        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected LoadBankHostFile2ndModel gLoadBankHostFileModel { get; private set; }

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
                systemProfileDao.Log("LoadBankHostFile2ndBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public LoadBankHostFile2ndBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ILoadBankHostFile2ndDao LoadBankHostFile2ndDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.LoadBankHostFile2ndDao = LoadBankHostFile2ndDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.hostFileDao = hostFileDao;


        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile2nd.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {


            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX));
            auditTrailDao.SecurityLog("Access Load Non Conformance File Download & Import", "", TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile2nd.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX, "View_LoadBankHostFile2nd", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);

            }
            else if (t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LoadBankHostFile2nd.DOWNLOAD, "View_LoadBankHostFile2ndProcess", "fldStartTime", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            } else if (t.Equals("Overview")) {
                ViewBag.Type = t;
                string clearDate = collection["fldProcessDate"];
                string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                //ViewBag.ProgressStatus = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LoadBankHostFile.INDEX));

                ViewBag.ProgressStatus = LoadBankHostFile2ndDao.GetLoadBankHostSummary(clearDateDD);
                return View();
            }


            return View();

        }


        public ActionResult GetMICRItemList(FormCollection collection)
        {
            ViewBag.NcfHistory = LoadBankHostFile2ndDao.InwardItemList(collection);

            return PartialView("Modal/_InwardItemListPopup");

        }

        //[CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile2nd.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Import(FormCollection col)
        //{
        //    //string chkImport = "";
        //    string notice = "";

        //    gLoadBankHostFileModel = LoadBankHostFile2ndDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account.BankCode);

        //    HostFileModel gHostFileModel = new HostFileModel();
        //    gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX);
        //    string processName = gHostFileModel.fldProcessName;
        //    string posPayType = gHostFileModel.fldPosPayType;


        //        string clearDate = col["fldProcessDate"];
        //        string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
        //        //string processName = "Download & Import Host Status File";
        //        //string posPayType = "LoadHostStatusFile";
        //        string taskId = TaskIdsICS.LoadBankHostFile2nd.INDEX;
        //        string batchId = "";
        //        string reUpload = "N";
        //        string filename = "";




        //        string fromFolder = LoadBankHostFile2ndDao.ListFolderPathFrom();
        //        string tofolder = LoadBankHostFile2ndDao.ListFolderPathTo();



        //                DirectoryInfo d = new DirectoryInfo(tofolder);//Assuming Test is your Folder
        //    if (Directory.Exists(tofolder))
        //    {
        //        if (!(cleardataProcess.CheckDataProcessExist(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode)))
        //        {
        //            bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode);

        //            //Do process
        //            if (runningProcess)
        //            {

        //                //Insert new data process
        //                cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
        //                auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account);

        //                TempData["Notice"] = "Load Bank Host File has been triggered";
        //            }
        //            else
        //            {
        //                TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
        //            }
        //        }
        //        else
        //        {
        //            TempData["Notice"] = "File has been loaded successfully.";
        //        }
        //    }
        //    else { TempData["Notice"] = "Please Check Folder is accessible"; }


        //    return RedirectToAction("Index");
        //    //return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        //}

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile2nd.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public JsonResult Import(FormCollection col)
        {
            //string chkImport = "";
            string notice = "";

            //gLoadBankHostFileModel = LoadBankHostFile2ndDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account.BankCode);

            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;


            string clearDate = col["fldProcessDate"];
            //string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
            //string processName = "Download & Import Host Status File";
            //string posPayType = "LoadHostStatusFile";
            string taskId = TaskIdsICS.LoadBankHostFile2nd.INDEX;
            string batchId = "";
            string reUpload = "N";
            string filename = "";




            string fromFolder = LoadBankHostFile2ndDao.ListFolderPathFrom();
            string tofolder = LoadBankHostFile2ndDao.ListFolderPathTo();



            DirectoryInfo d = new DirectoryInfo(tofolder);//Assuming Test is your Folder
            if (Directory.Exists(tofolder))
            {
                //if (!(cleardataProcess.CheckDataProcessExist(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode)))
                //{
                    bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDD, CurrentUser.Account.BankCode);

                    //Do process
                    if (runningProcess)
                    {

                        //Insert new data process
                        cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account);

                        notice = "Load Processed Debit File has been triggered";
                    }
                    else
                    {
                        notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                //}
                //else
                //{
                //    notice = "File has been loaded successfully.";
                //}
            }
            else { notice = "Please Check Folder is accessible"; }


            //return RedirectToAction("Index");
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LoadBankHostFile2nd.INDEX)]
        [HttpPost()]
        public virtual ActionResult DownloadFile(FormCollection collection)
        {
            string taskId = TaskIdsICS.LoadBankHostFile2nd.DOWNLOAD;
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
                auditTrailDao.SecurityLog("[Inward Return] Download ICL File :  " + fullFileName, "", TaskIdsICS.LoadBankHostFile2nd.INDEX, CurrentUser.Account);
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
            //gLoadBankHostFileModel = LoadBankHostFile2ndDao.GetDataFromMICRImportConfig(TaskIdsICS.LoadBankHostFile.INDEX, CurrentUser.Account.BankCode);
            ViewBag.FolderPathFrom = LoadBankHostFile2ndDao.ListFolderPathFrom();
            ViewBag.FolderPathTo = LoadBankHostFile2ndDao.ListFolderPathTo();

            string clearDate = col["fldProcessDate"];
            string folderPath = LoadBankHostFile2ndDao.ListFolderPathTo();
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
                        string name = FolderList[i].fileNameOnly;
                        string folderDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                        //folderDate = folderDate.Substring(2, 8);
                        //folderPath = folderPath +"'\'";
                        //ViewBag.FolderList[i].fileName
                        name = name.Replace(folderPath + "\\", "");
                        //string index = name.Substring(0, 13);

                        //if (name.Contains(folderDate))
                        //{
                        //    if (index != "IS" && index != "SS")
                        //    {
                        //        FolderList.RemoveAt(i);
                        //        --i;
                        //    }
                        //    else
                        //    {
                        //        FolderList[i].fileName = name;
                        //    }
                        //}
                        //else
                        //{
                        //    FolderList.RemoveAt(i);
                        //    --i;
                        //}

                        if (name != "FPEN2" + folderDate + ".TXT")
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
        //public ActionResult DownloadFileFromFtp(FormCollection col)
        //{

        //    HostFileModel gHostFileModel = new HostFileModel();
        //    gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.LoadBankHostFile2nd.INDEX);
        //    string processname = gHostFileModel.fldProcessName;
        //    string bankcode = gHostFileModel.fldBankCode;

        //    string URL = LoadBankHostFile2ndDao.ListFolderPathFrom();
        //    string UserID = LoadBankHostFile2ndDao.GetFTPUserName();
        //    string Password = LoadBankHostFile2ndDao.GetFTPPassword();
        //    string fileDestPath = LoadBankHostFile2ndDao.ListFolderPathTo();

        //    string clearDate = col["fldProcessDate"].ToString();
        //    clearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

        //    try
        //    {

        //        string FileName = LoadBankHostFile2ndDao.GetInterfaceFileName("RepairedHostStatusFile");
        //        FileName = FileName.Replace("YYYYMMDD", clearDate);
        //        string Message = "";
        //        bool statusDownloadFile = LoadBankHostFile2ndDao.DownloadFile(URL, UserID, Password, fileDestPath, FileName, ref Message);
        //        if (statusDownloadFile is true)
        //        {

        //            TempData["Notice"] = "Host File Successfully Download From FTP";
        //            //    if (Directory.Exists(fileDestPath))
        //            //    {
        //            //        //List<FolderInformationModel> foldername = new List<FolderInformationModel>();
        //            //        //foldername = fileInformationService.GetGWCFolderICS(folderPath, clearDate, ".txt");
        //            //        List<FileInformationModel> FolderList = new List<FileInformationModel>();
        //            //        FolderList = fileInformationService.GetAllFileInFolderWithPath(fileDestPath, 0, ".txt");

        //            //        if (FolderList.Count > 0)
        //            //        {
        //            //            for (int i = 0; i < FolderList.Count; i++)
        //            //            {
        //            //                string name = FolderList[i].fileNameOnly;

        //            //                //folderPath = folderPath +"'\'";
        //            //                //ViewBag.FolderList[i].fileName
        //            //                //name = name.Replace(fileDestPath + "\\", "");
        //            //                //string index = name.Substring(0, 2);
        //            //                if (name != ("ODACCT" + clearDate + ".txt") && name != ("ODACCT" + clearDate + ".TXT"))
        //            //                {
        //            //                    FolderList.RemoveAt(i);
        //            //                    --i;
        //            //                }
        //            //                else
        //            //                {
        //            //                    FolderList[i].fileName = name;

        //            //                }
        //            //            }

        //            //        }
        //            //    }
        //            //}
        //            //else
        //            //{

        //        }
        //        else
        //        {
        //            TempData["Error"] = "Host File Failed Download From FTP.";
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = "Host File Failed Download From FTP: " + ex.Message;
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}
