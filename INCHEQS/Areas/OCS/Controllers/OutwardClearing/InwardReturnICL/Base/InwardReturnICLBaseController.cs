using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Areas.OCS.Models.ClearingItem;
//using INCHEQS.Areas.OCS.Models.HostFile;
using INCHEQS.Areas.OCS.Models.InwardReturnICL;
using INCHEQS.Common;
using INCHEQS.Common.Resources;
using INCHEQS.Helpers;
using INCHEQS.Models.FileInformation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.Sequence;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
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


namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.InwardReturnICL.Base
{
    public abstract class InwardReturnICLBaseController : BaseController
    {
        protected readonly IPageConfigDaoOCS pageConfigDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;

        // protected readonly IClearingItemDao IClearingItemDao;
        //protected readonly IHostFileOCSDao hostFileDao;
        protected readonly OCSIDataProcessDao OCScleardataProcess;
        protected readonly IInwardReturnICLDao InwardReturnICLDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly SequenceDao sequenceDao;
        protected readonly IMICRImageDao micrImageDao;
        protected readonly IFileManagerDao fileManagerDao;


        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected MICRImageModel gMicrImageModel { get; private set; }
        protected InwardReturnICLModel gInwardReturnICLModel { get; private set; }

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
                systemProfileDao.Log("InwardReturnICLBaseController/initializeBeforeAction errors :" + ex.Message, "SystemLog", CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public InwardReturnICLBaseController(IPageConfigDaoOCS pageConfigDao, OCSIDataProcessDao OCScleardataProcess, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, IMICRImageDao micrImageDao, IFileManagerDao fileManagerDao, FileInformationService fileInformationService, SequenceDao sequenceDao, ISystemProfileDao systemProfileDao, IInwardReturnICLDao InwardReturnICLDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.OCScleardataProcess = OCScleardataProcess;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            // this.IClearingItemDao = IClearingItemDao;
            //this.hostFileDao = hostFileDao;
            this.micrImageDao = micrImageDao;
            this.fileManagerDao = fileManagerDao;
            this.fileInformationService = fileInformationService;
            this.sequenceDao = sequenceDao;
            this.systemProfileDao = systemProfileDao;
            this.InwardReturnICLDao = InwardReturnICLDao;

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index(FormCollection collection)
        {
            ViewBag.FolderPathFrom = InwardReturnICLDao.ListFolderPathFrom();
            ViewBag.FolderPathTo = InwardReturnICLDao.ListFolderPathTo();

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.InwardReturnICL.INDEX));
            auditTrailDao.SecurityLog("Access ICL Download & Import", "", TaskIdsOCS.InwardReturnICL.INDEX, CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string strUserBranch = "";
            string t = collection["Type"];
            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (t.Equals("Ready"))
            {

                ViewBag.Type = t;
                //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InwardReturnICL.INDEX, "View_InwardReturnFile", "fldpresentingbank", "", new[] {
                //      new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                //collection);

                //initializeBeforeAction();
                gMicrImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsOCS.InwardReturnICL.INDEX, CurrentUser.Account.BankCode);

                string clearDate = collection["fldProcessDate"];
                //string clearDate = DateTime.ParseExact(collection["fldProcessDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                string folderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
                string completefolderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode2, CurrentUser.Account.BankCode);
                int dateSubString = gMicrImageModel.fldDateSubString;
                int bankCodeSubString = gMicrImageModel.fldBankCodeSubString;

                ViewBag.FileList = fileInformationService.GetAllFileInFolderOCS(folderName, completefolderName, bankCodeSubString, CurrentUser.Account.BankCode, clearDate);

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InwardReturnICL.INDEX, "View_InwardReturnFile", "fldpresentingbank", "", new[] {
               new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                collection);
            }
            else if (t.Equals("Submitted"))
            {

                ViewBag.Type = t;

                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InwardReturnICL.DOWNLOAD, "View_GetIRDDownloadItem", "fldpresentingbank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);


            }
            else if (t.Equals("Summary"))
            {

                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InwardReturnICL.SUMMARY, "View_GetInwardICLtems", "fldpresentingbank", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);

            }

            // TempData["Notice"] = "File Imported";

            return View();

        }


        public ActionResult ICLItemList(FormCollection collection)
        {
            ViewBag.InwardHistory = InwardReturnICLDao.InwardItemList(collection);
            return PartialView("Modal/_InwardItemListPopup");

        }

        //public JsonResult Import(FormCollection col)
        [CustomAuthorize(TaskIds = TaskIdsOCS.InwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        //public ActionResult Import(FormCollection col)
        public JsonResult Import(FormCollection col)
        {
            string chkImport = "";
            string notice = "";

            // if (col["chkImport"] != null)
            if (col != null && col["deleteBox"] != null)
            {
                string clearDate = col["fldProcessDate"];
                string clearDateDD = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                gMicrImageModel = micrImageDao.GetDataFromMICRImportConfig(TaskIdsOCS.InwardReturnICL.INDEX, CurrentUser.Account.BankCode);

                //string notice = "";
                string processName = "Download & Import Inward Return File";
                string posPayType = gMicrImageModel.fldPosPayType;

                string reUpload = "N";
                string filename = "";

                string taskId = TaskIdsOCS.InwardReturnICL.INDEX;
                string batchId = "";//TODO: unknown batch ID
                string folderName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode, CurrentUser.Account.BankCode);
                string folderFileName = systemProfileDao.GetValueFromSystemProfile(gMicrImageModel.fldSystemProfileCode2, CurrentUser.Account.BankCode);
                int dateSubString = gMicrImageModel.fldDateSubString;
                int bankCodeSubString = gMicrImageModel.fldBankCodeSubString;
                string fileExt = gMicrImageModel.fldFileExt;
                int count = 0;
                string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
                List<string> errorMessages = new List<string>();
                int Checking = 0;


                string currentProcess = "1";
                int ItemBatch = 10;

                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                for (int i = 0; i <= arrResults.Count - 1; i++)
                {
                    chkImport = arrResults[i].ToString();
                    InwardReturnICLDao.GenerateNewBatches(CurrentUser.Account.BankCode, CurrentUser.Account.UserId, clearDate);

                    errorMessages = InwardReturnICLDao.GetInwardStatus(CurrentUser.Account.BankCode, currentProcess, ItemBatch);
                    Checking = errorMessages.Count;

                    if ((Checking > 0))
                    {
                        gInwardReturnICLModel = InwardReturnICLDao.GetIRDStatus(CurrentUser.Account.BankCode, currentProcess, ItemBatch);

                        string IRDBatch = gInwardReturnICLModel.fldIRDBatch;
                        ViewBag.CurrentProcess = gInwardReturnICLModel.currentProcess;

                        InwardReturnICLDao.updateIRDStatusProcessTime(IRDBatch);

                        //string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
                        string fromFolder = folderName + dateClearing;
                        string tofolder = folderFileName + dateClearing;
                        System.IO.Directory.CreateDirectory(tofolder + "\\");

                        DirectoryInfo d = new DirectoryInfo(fromFolder);//Assuming Test is your Folder
                        if (Directory.Exists(fromFolder))
                        {
                            FileInfo[] Files = d.GetFiles("*.ICL"); //Getting Text files
                            string str = "";
                            string toPath = "";
                            string fromPath = "";

                            string fileName = null;

                            // using the method 
                            fileName = Path.GetFileName(chkImport);

                            //foreach (FileInfo file in Files)
                            //{
                            str = fileName;
                            string extension = str.Substring(str.LastIndexOf('.') + 1);
                            if (extension.ToUpper() == "ICL")
                            {
                                toPath = folderFileName + dateClearing + "\\" + str;
                                fromPath = folderName + dateClearing + "\\" + str;

                                if (!System.IO.File.Exists(toPath))
                                {

                                    System.IO.File.Copy(Path.Combine(fromFolder, str), Path.Combine(tofolder, str));
                                    //  System.IO.File.Copy(file.Name, tofolder + "\\" + fromPath(file.Name));
                                    //System.IO.File.Delete(toPath);
                                    fileManagerDao.DeleteFileManagerOCS(str, tofolder, CurrentUser.Account.BankCode);

                                    fileManagerDao.InsertToFileManagerOcs(CurrentUser.Account, tofolder, str, clearDateDD);

                                    InwardReturnICLDao.updateIRDStatusCompleted(IRDBatch, str);

                                    OCScleardataProcess.DeleteDataProcess(processName, posPayType, CurrentUser.Account.BankCode);
                                    //Insert new data process
                                    OCScleardataProcess.InsertToDataProcessIRD(CurrentUser.Account.BankCode, processName, posPayType, clearDateDD, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                                    auditTrailDao.SecurityLog("[Inward Return] Perform Download and Import", "", TaskIdsOCS.InwardReturnICL.INDEX, CurrentUser.Account);
                                    //auditTrailDao.Log("Perform Download and Import", CurrentUser.Account);
                                    notice = Locale.PerformDownloadandImport; //Perform Download and Import

                                }

                                else if (System.IO.File.Exists(toPath))
                                {

                                    //TempData["Notice"]
                                    notice = "File Already Exists";

                                }

                                else
                                {
                                    InwardReturnICLDao.updateIRDStatusFail(IRDBatch);

                                }

                            }
                            else
                            {
                                notice = "Please select correct file";

                            }

                        }
                        else { notice = "Please check date file location"; }

                    }
                    else
                    {
                        notice = "Please Check Server Date";

                    }

                }
            }
            else
            {
                //TempData["Warning"]
                notice = Locale.Nodatawasselected;
            }
            //return RedirectToAction("Index");
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InwardReturnICL.INDEX)]
        [HttpPost()]
        public virtual ActionResult DownloadFile(FormCollection collection)
        {
            string taskId = TaskIdsOCS.InwardReturnICL.DOWNLOAD;
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
                auditTrailDao.SecurityLog("[Inward Return] Download ICL File :  " + fullFileName, "", TaskIdsOCS.InwardReturnICL.INDEX, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                TempData["Warning"] = Locale.Nodatawasselected;
            }

            return RedirectToAction("Index");
        }



    }
}
