using INCHEQS.Areas.ICS.Models.MICRImage;
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
using System.Xml.Linq;
using INCHEQS.Models.TaskMenu;
using System.Collections;
using ImageProcessor.Imaging.Quantizers.WuQuantizer;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Text.RegularExpressions;
using System.Drawing;
using static System.Net.WebRequestMethods;
using System.ComponentModel.DataAnnotations;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.MICRImage.Base
{
    public abstract class MICRImageBaseController : BaseController
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
        protected readonly IMICRImageDao micrImageDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IProgressStatusDao progressStatusDao;

        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected MICRImageModel gMICRImageModel { get; private set; }

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
            gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

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


        public MICRImageBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IProgressStatusDao progressStatusDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.micrImageDao = micrImageDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.progressStatusDao = progressStatusDao;
          

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.MICRImage.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {
            CurrentUser.Account.TaskId = TaskIdsICS.MICRImage.INDEX; 
            gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsICS.MICRImage.INDEX, CurrentUser.Account.BankCode);
            string processName = gMICRImageModel.fldProcessName;
            string fromFolder = micrImageDao.ListFolderPathFrom(processName, CurrentUser.Account.BankCode);
            //string fileExt = ".MMYR";
            string clearDate = progressStatusDao.getClearDate();

            string folderDate = clearDate == ""? DateTime.Now.ToString() : DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"); 
            //string clearDate = collection["fldProcessDate"];
            //string irempath = Path.   Combine(fromFolder.TrimEnd('\"'),folderDate);

            string iwnmpath = Path.Combine(fromFolder.TrimEnd('\"'), folderDate);
            if (Directory.Exists(iwnmpath))
            {
                /*ViewBag.FolderList = fileInformationService.GetGWCFolderICS(fromFolder, clearDate, fileExt);
                if (ViewBag.FolderList.Count > 0)
                {
                    ViewBag.Button = "";

                }
                else
                {*/
                    ViewBag.Button = "";
                //}
            }
            else
            {
                ViewBag.Button = "hidden";
            }

                

            //ViewBag.FolderPathFrom = micrImageDao.ListFolderPathFrom();
            //ViewBag.FolderPathTo = micrImageDao.ListFolderPathTo();
            //ViewBag.FolderPathToCompleted = micrImageDao.ListFolderPathToCompleted();

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.MICRImage.INDEX));
            //auditTrailDao.SecurityLog("Access MICR Download & Import", "", TaskIdsICS.MICRImage.INDEX, CurrentUser.Account);
            auditTrailDao.Log("Access MICR Download & Import",CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.MICRImage.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.MICRImage.INDEX, "View_MICRImageFile", "fldIssuingBank", "",
                   null),
             collection);

            }
            else if(t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.MICRImage.DOWNLOAD, "View_MICRImageProcess", "fldIssuingBank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            }


            return View();

        }


        public ActionResult GetMICRItemList(FormCollection collection)
        {
            ViewBag.InwardHistory = micrImageDao.InwardItemList(collection);
            return PartialView("Modal/_InwardItemListPopup");

        }
        
        [CustomAuthorize(TaskIds = TaskIdsICS.MICRImage.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Import(FormCollection col)
        {
            //string chkImport = "";
            
                gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsICS.MICRImage.INDEX, CurrentUser.Account.BankCode);


                string posPayType = gMICRImageModel.fldPosPayType;
                string clearDate = col["fldProcessDate"];
                string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                string processName = gMICRImageModel.fldProcessName;
                string taskId = TaskIdsICS.MICRImage.INDEX;
                CurrentUser.Account.TaskId = taskId;
                string batchId = "";
                string reUpload = "N";
                string filename = "";
                string fromFolder = micrImageDao.ListFolderPathFrom(processName, CurrentUser.Account.BankCode);
                string tofolder = micrImageDao.ListFolderPathTo(processName, CurrentUser.Account.BankCode);
                //int dateSubString = gMICRImageModel.fldDateSubString;
                //int bankCodeSubString = gMICRImageModel.fldBankCodeSubString;
                
                //string fileExt = gMICRImageModel.fldFileExt;
                string fileExt = ".zip";
                
                //int count = 0;
                string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
                List<string> errorMessages = new List<string>();
                //int Checking = 0;


                //string currentProcess = "1";
                //int ItemBatch = 10;

                        DirectoryInfo d = new DirectoryInfo(fromFolder);//Assuming Test is your Folder
                        if (Directory.Exists(fromFolder))
                        {
                                ViewBag.FolderList = fileInformationService.GetGWCFolderICS(fromFolder, clearDate, fileExt);
                                if (ViewBag.FolderList.Count > 0)
                                {
                                    bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDate, CurrentUser.Account.BankCode);

                                    //Do process
                                    if (runningProcess)
                                    {
                                        //micrImageDao.updateMICRStatusCompleted(MICRBatch);

                                        cleardataProcess.DeleteDataProcessWithSystemTypeICS(dateClearing, posPayType, CurrentUser.Account.BankCode);
                                        //Insert new data process
                                        cleardataProcess.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                                        //auditTrailDao.SecurityLog("[Download & Import] Perform Download and Import", "", TaskIdsICS.MICRImage.INDEX, CurrentUser.Account);
                                        auditTrailDao.Log("Perform Download and Import", CurrentUser.Account);
                                        TempData["Notice"] = Locale.PerformDownloadandImport; //Perform Download and Import
                                        
                                    }
                                    else
                                    {
                                        TempData["Warning"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                                    }
                                }
                                else
                                {
                                    TempData["ErrorMsg"] = "Folder not available";
                                }
                                
                        }
                        else { TempData["ErrorMsg"] = "Please Check GWC Folder is accessible"; }


            ViewBag.inwardProcessType = "ICL";
            ViewBag.dataProcessType = "ICL";



            return RedirectToAction("Index");
        }
        [CustomAuthorize(TaskIds = TaskIdsICS.MICRImage.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult GWCListing(FormCollection col)
        {
            //get list of folders in gwc  based on clearing date
            if (TempData["fldProcessDate"] != null)
            {
                col["fldProcessDate"] = col["fldProcessDate"] == null ? TempData["fldProcessDate"].ToString() : col["fldProcessDate"];
            }
            
            string clearDate = col["fldProcessDate"];
            TempData["SelectedDate"] = clearDate;
            //initializeBeforeAction();
            Clearing(clearDate);

            return View();

            
        }
        void Clearing(string clearDate)
        {
            gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsICS.MICRImage.INDEX, CurrentUser.Account.BankCode);
            string processName = gMICRImageModel.fldProcessName;
            ViewBag.FolderPathFrom = micrImageDao.ListFolderPathFrom(processName, CurrentUser.Account.BankCode);
            ViewBag.FolderPathTo = micrImageDao.ListFolderPathTo(processName, CurrentUser.Account.BankCode);

            
            clearDate = clearDate == null ? DateTime.Now.ToString("dd-MM-yyyy") : clearDate;
            string folderPath = micrImageDao.ListFolderPathFrom(processName, CurrentUser.Account.BankCode);
            //string fileExt = gMICRImageModel.fldFileExt;
            string fileExt = ".zip";


            if (Directory.Exists(folderPath))
            {

                ViewBag.FolderList = fileInformationService.GetGWCFolderICS(folderPath, clearDate, fileExt);

            }
            
        }

//        [RequestSizeLimit]
        public virtual ActionResult Upload(FormCollection col)
        {
            string clearDate = "";

            if (Request.Files.Count > 0)
            {
            //  Get all files from Request object  
                HttpFileCollectionBase files = Request.Files;
                gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsICS.MICRImage.INDEX, CurrentUser.Account.BankCode);
                string processName = gMICRImageModel.fldProcessName;
                string getdatefromaHUBnum = files[0].FileName;
                getdatefromaHUBnum = Regex.Replace(getdatefromaHUBnum, "[^0-9]", "");
                getdatefromaHUBnum = getdatefromaHUBnum.Remove(getdatefromaHUBnum.Length - 1, 1);
                getdatefromaHUBnum = getdatefromaHUBnum.Remove(0, 11);
                string day = getdatefromaHUBnum.Remove(2);
                string month = getdatefromaHUBnum.Remove(getdatefromaHUBnum.Length - 4);
                month = month.Remove(0, 2);
                string year = getdatefromaHUBnum.Remove(0, getdatefromaHUBnum.Length - 4);
                string getfoldername = year + month + day;
                clearDate = day + "-" + month + "-" + year;

                

                if (TempData["SelectedDate"].ToString() == clearDate)
                {
                    TempData["fldProcessDate"] = clearDate;
                    // Get the complete folder path and store the file inside it.  
                    string configpath = micrImageDao.ListFolderPathUpload(processName, CurrentUser.Account.BankCode);
                    //"D:/D-Voyager/Development/Pak/Banks/BoP/INCHEQS_FILES/GWC/ImageFolder/";
                    getfoldername = configpath + "\\" + getfoldername;

                    if (Directory.Exists(getfoldername))
                    {
                        DeleteDirectory(getfoldername);
                        Directory.CreateDirectory(getfoldername);
                    }
                    else
                    {
                        Directory.CreateDirectory(getfoldername);

                    }

                }
                else
                {
                        TempData["ErrorMsg"] = "Selected Date should be same with clearing file date";
                }
                


                    
                for (int i = 0; i < files.Count; i++)
                {
                    //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                    //string filename = Path.GetFileName(Request.Files[i].FileName);  

                    HttpPostedFileBase file = files[i];
                    string fname;

                    // Checking for Internet Explorer  
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }

                    //gMICRImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsICS.MICRImage.INDEX, CurrentUser.Account.BankCode);
                    //string processName = gMICRImageModel.fldProcessName;
                    //string getdatefromaHUBnum = file.FileName;
                    //getdatefromaHUBnum = Regex.Replace(getdatefromaHUBnum, "[^0-9]", "");
                    //getdatefromaHUBnum = getdatefromaHUBnum.Remove(getdatefromaHUBnum.Length - 1, 1);
                    //getdatefromaHUBnum = getdatefromaHUBnum.Remove(0, 11);
                    //string day = getdatefromaHUBnum.Remove(2);
                    //string month = getdatefromaHUBnum.Remove(getdatefromaHUBnum.Length - 4);
                    //month = month.Remove(0, 2);
                    //string year = getdatefromaHUBnum.Remove(0, getdatefromaHUBnum.Length - 4);
                    //string getfoldername = year + month + day;
                    //clearDate = day + "-" + month + "-" + year;

                    //if (TempData["SelectedDate"].ToString() == clearDate)
                    //{
                    //    TempData["fldProcessDate"] = clearDate;

                        // Get the complete folder path and store the file inside it.  
                        //string configpath = micrImageDao.ListFolderPathUpload(processName, CurrentUser.Account.BankCode);
                        ////"D:/D-Voyager/Development/Pak/Banks/BoP/INCHEQS_FILES/GWC/ImageFolder/";
                        //getfoldername = configpath + "\\" + getfoldername;
                        
                        if (Directory.Exists(getfoldername))
                        {

                            fname = Path.Combine(getfoldername, fname);
                            CreateNewDirectory(getfoldername, fname, file);
                        //  if (Directory.Exists(getfoldername))
                          //  {

                            //    fname = Path.Combine(getfoldername, fname);
   //                             DeleteDirectory(getfoldername);
 //                               CreateNewDirectory(getfoldername, fname, file);

                            //}


                        }
                        //else
                        //{
                            //CreateNewDirectory(getfoldername, fname, file);


                        //}
                    
                    

                }

                if (TempData["SelectedDate"].ToString() == clearDate)
                {
                    micrImageDao.AddClearingDate(clearDate, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                    
                }
                
            }


            return RedirectToAction("GWCListing");
                

        }
        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                //Delete all files from the Directory
                foreach (string file in Directory.GetFiles(path))
                {
                   System.IO.File.Delete(file);
                }
                //Delete all child Directories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    DeleteDirectory(directory);
                }
                //Delete a Directory
                Directory.Delete(path);
            }
        }

        public void CreateNewDirectory(string getfoldername,string fname, HttpPostedFileBase file)
        {
            if (Directory.Exists(getfoldername))
            {

                fname = Path.Combine(getfoldername, fname);

                file.SaveAs(fname);

            }
        }
        
        public async Task<JsonResult> ProgressBar(FormCollection col)
        {
           

            //ICSUpdateChequeNumber
            string progressbarvalue = micrImageDao.BroadCastProgressBar(col["Import"], "");
            progressbarvalue  = await RecursiveProgressBar(progressbarvalue, col["Import"]);
            List<string> resultList = new List<string>();
            resultList.Add(progressbarvalue);
            resultList.Add(col["Import"]);

            //if (progressbarvalue == "2")
            //{
            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            ////    progressbarvalue = "20";
            //}
            //if (progressbarvalue == "3")
            //{

            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            //    progressbarvalue = "60";
            //}
            //if (progressbarvalue == "4")
            //{
            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            //    progressbarvalue = "100";
            //}

            return Json(resultList);
        }
        public async Task<string> RecursiveProgressBar(string progressbarvalue,string processName)
        {
            //systemProfileDao.Log("RecursiveProgressBar errors :" + progressbarvalue,"SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);


            string processname = processName;

            if (progressbarvalue == "" || progressbarvalue == "1" || progressbarvalue == "2")
            {
                return await Task.Run(async () => {
                    progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                    await Task.Delay(10000);
                    return progressbarvalue;
                });

            }
            
            //if (progressbarvalue == "1")
            //{
            //    progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
            //    return progressbarvalue;

                //}
                //if (progressbarvalue == "2")
                //{
                //    progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                //    return progressbarvalue;
                //}
            else if (progressbarvalue == "4")
            {
                progressbarvalue = "4";
            }

            return progressbarvalue;

        }



        //public string RecursiveProgressBar(string progressbarvalue)
        //{

        //    string processname = "ICSUpdateChequeNumber";
        //    if (progressbarvalue == "")
        //    {
        //        System.Threading.Thread.Sleep(1000);    
        //        progressbarvalue = micrImageDao.BroadCastProgressBar(processname , "");
        //        RecursiveProgressBar(progressbarvalue);
        //    }
        //    if (progressbarvalue == "1")
        //    {
        //        System.Threading.Thread.Sleep(1000);
        //        progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
        //        progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);


        //    }
        //    if (progressbarvalue == "2")
        //    {
        //        System.Threading.Thread.Sleep(1000);
        //        progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
        //        progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
        //    }
        //    if (progressbarvalue == "3")
        //    {
        //        System.Threading.Thread.Sleep(1000);
        //        progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
        //        progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
        //    }
        //    if (progressbarvalue == "4")
        //    {
        //        progressbarvalue = "4";
        //    }

        //    return progressbarvalue;

        //}

        public virtual ActionResult Browse(FormCollection col)
        {


            return View();
        }


    }
}
