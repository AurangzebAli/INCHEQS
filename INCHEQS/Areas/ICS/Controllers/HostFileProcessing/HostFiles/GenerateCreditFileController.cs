using INCHEQS.Models.HostFile;
using INCHEQS.Areas.ICS.Models.GenerateCreditFile;
using INCHEQS.Common.Resources;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.Models.HostFile;
using System.IO;
using System.Text;
using System.Net;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing
{
    public class GenerateCreditFileController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        //private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileDao hostFileDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        private readonly IGenerateCreditFileDao GenerateCreditFileDao;


        public GenerateCreditFileController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, IHostFileDao hostFileDao, ICSIDataProcessDao cleardataProcess, IGenerateCreditFileDao GenerateCreditFileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            // this.IClearingItemDao = IClearingItemDao;
            this.hostFileDao = hostFileDao;
            this.cleardataProcess = cleardataProcess;
            this.GenerateCreditFileDao = GenerateCreditFileDao;

        }


        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateCreditFileBased.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateCreditFileBased.INDEX, "View_GetICSItemsforCreditPosting", "fldIssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateCreditFileBased.POSTED, "View_GetICSCreditPostedtems", "fldStartTime", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }

        //[CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Generate(FormCollection collection)
        //{
        //    Dictionary<string, string> errors = new Dictionary<string, string>();
        //    HostFileModel gHostFileModel = new HostFileModel();
        //    gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFileBased.INDEX);
        //    string processName = gHostFileModel.fldProcessName;
        //    string posPayType = gHostFileModel.fldPosPayType;
        //    string reUpload = "N";
        //    string taskId = TaskIdsICS.GenerateCreditFileBased.INDEX;
        //    string batchId = "";
        //    string bankcodeChkbox = "";
        //    string totalItem = "";
        //    string totalAmount = "";
        //   // string clearDateDDMMYYYY = collection["row_fldClearDate"];
        //    string clearDate = collection["fldClearDate"];
        //    string clearDateDDMMYYYY = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
        //    string filename = "";
        //    string t = collection["Type"];
        //    if (t == null)
        //    {
        //        t = "Ready";
        //    }
        //    else
        //    {
        //        t = collection["Type"];
        //    }
        //    if (collection != null && collection["deleteBox"] != null)
        //    {
        //        if (t.Equals("Ready"))
        //        {




        //                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
        //                //bankcodeChkbox = arrResults[0].ToString();
        //                //for (int i = 0; i <= arrResults.Count - 1; i++)
        //                //{
        //                    foreach (string arrResult in arrResults)
        //                    {
        //                        bankcodeChkbox = GenerateCreditFileDao.getBetween(arrResult, "ae", "bf"); 
        //                        totalItem = GenerateCreditFileDao.getBetween(arrResult, "bf", "cg");
        //                        totalAmount = GenerateCreditFileDao.getBetween(arrResult, "cg", "dh");

        //                       bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDDMMYYYY, bankcodeChkbox);
        //                        if (runningProcess)
        //                        {
        //                            GenerateCreditFileDao.GenerateNewBatches(bankcodeChkbox, gHostFileModel.fldProcessName, gHostFileModel.fldFileExt, CurrentUser.Account.UserId, clearDateDDMMYYYY, totalItem, totalAmount);

        //                            //Insert new data process
        //                            cleardataProcess.InsertToDataProcessICS(bankcodeChkbox, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
        //                            auditTrailDao.Log("Generate Credit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateCreditFileBased.INDEX, CurrentUser.Account);
        //                            TempData["Notice"] = "Generate Credit File has been triggered";
        //                        }
        //                        else
        //                        {
        //                            TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
        //                        }
        //                    }

        //                //}

        //            }

        //    }
        //    else
        //    {
        //        TempData["ErrorMsg"] = "No Data was selected";
        //    }
        //    return RedirectToAction("Index");
        //}

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public JsonResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFileBased.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsICS.GenerateCreditFileBased.INDEX;
            string batchId = "";
            string bankcodeChkbox = "";
            string totalItem = "";
            string totalAmount = "";
            // string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDate = collection["fldClearDate"];
            string clearDateDDMMYYYY = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
            string filename = "";
            string t = collection["Type"];

            string notice = "";

            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }
            if (collection != null && collection["deleteBox"] != null)
            {
                if (t.Equals("Ready"))
                {

                    List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                    //bankcodeChkbox = arrResults[0].ToString();
                    //for (int i = 0; i <= arrResults.Count - 1; i++)
                    //{
                    foreach (string arrResult in arrResults)
                    {
                        bankcodeChkbox = GenerateCreditFileDao.getBetween(arrResult, "ae", "bf");
                        totalItem = GenerateCreditFileDao.getBetween(arrResult, "bf", "cg");
                        totalAmount = GenerateCreditFileDao.getBetween(arrResult, "cg", "dh");

                        bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDDMMYYYY, bankcodeChkbox);
                        if (runningProcess)
                        {
                            GenerateCreditFileDao.GenerateNewBatches(bankcodeChkbox, gHostFileModel.fldProcessName, gHostFileModel.fldFileExt, CurrentUser.Account.UserId, clearDate, totalItem, totalAmount);

                            //Insert new data process
                            cleardataProcess.InsertToDataProcessICS(bankcodeChkbox, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                            auditTrailDao.Log("Generate Credit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateCreditFileBased.INDEX, CurrentUser.Account);
                            notice = "Generate Credit File has been triggered";
                        }
                        else
                        {
                            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                        }
                    }

                    //}

                }

            }
            else
            {
                notice = "No Data was selected";
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PostedItemHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = GenerateCreditFileDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = GenerateCreditFileDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }

       
        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
        [HttpPost()]
        public virtual ActionResult Download(FormCollection collection)
        {
            //initializeBeforeAction();
            string taskId = TaskIdsICS.GenerateCreditFileBased.INDEX;
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFileBased.INDEX);

            string filePath = collection["this_fldfilepath"];
            string fileName = collection["this_fldfilename"];
            string fullFileName = filePath + fileName;
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, ""));
                auditTrailDao.Log("Generate Credit Posting Download File : " + fullFileName + ". TaskId :" + TaskIdsICS.GenerateCreditFileBased.INDEX, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }


        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFileBased.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult ReGenerate(FormCollection collection)
        {
            string taskId = TaskIdsICS.GenerateCreditFileBased.INDEX;
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFileBased.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string batchId = "";
            string bankcodeChkbox = "";
            string totalItem = "";
            string totalAmount = "";
            string clearDate = "";
            string clearDateDDMMYYYY = "";
            string filename = "";
            string previousbatch = "";

            
          
            clearDate = collection["row_fldClearDate"];
            clearDateDDMMYYYY = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            bankcodeChkbox = collection["row_fldIssuingBankCode"];
            previousbatch = collection["row_fldPostingBatch"];
            totalItem = collection["row_fldPostingTotalItem"];
            totalAmount = collection["row_fldPostingTotalAmount"];

            bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDateDDMMYYYY, bankcodeChkbox);
                    if (runningProcess)
                    {
                        GenerateCreditFileDao.ReGenerateBatches(bankcodeChkbox, previousbatch , gHostFileModel.fldProcessName, gHostFileModel.fldFileExt, CurrentUser.Account.UserId, clearDateDDMMYYYY, totalItem, totalAmount);

                        //Insert new data process
                        cleardataProcess.InsertToDataProcessICS(bankcodeChkbox, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        auditTrailDao.Log("Generate Credit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateCreditFileBased.INDEX, CurrentUser.Account);
                        TempData["Notice"] = "Generate Credit File has been triggered";
                    }
                    else
                    {
                        TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                

                return RedirectToAction("Index");
            
        }

        //public ActionResult UploadFtpFile(FormCollection col)

        //{
        //    HostFileModel gHostFileModel = new HostFileModel();
        //    gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFileBased.INDEX);

        //    string processname = gHostFileModel.fldProcessName;
        //    string bankcode = gHostFileModel.fldBankCode;
        //    GenerateCreditFileModel GenerateCreditFileModel = GenerateCreditFileDao.GetCreditFilePath(processname, bankcode);
        //    //FTP Server URL.

        //    string ftp = GenerateCreditFileModel.fldFileDestination;
        //    string ftpUsername = GenerateCreditFileDao.GetFTPUserName();
        //    string ftpPassword = GenerateCreditFileDao.GetFTPPassword();

        //    //FTP Folder name. Leave blank if you want to upload to root folder.

        //    //string ftpFolder = System.Configuration.ConfigurationManager.AppSettings["FTPFolderName"];

        //    byte[] fileBytes = null;

        //    string fileSource = GenerateCreditFileModel.fldFileSource;
        //    string strfilename = GenerateCreditFileModel.fldFileName;
        //    string clearDate = col["fldClearDate"].ToString();

        //    //clearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

        //    //strfilename = strfilename.Replace("YYYYMMDD", DateTime.Now.ToString("yyyyMMdd"));
        //    strfilename = strfilename.Replace("YYYYMMDD", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"));

        //    //Read the FileName and convert it to Byte array.

        //    string fileName = Path.GetFileName(strfilename);

        //    //Insert new data process
        //    cleardataProcess.InsertToDataProcessICS(
        //        CurrentUser.Account.BankCode,
        //        "StatusCreditFileToFTP",
        //        "ICS",
        //        clearDate,
        //        "",
        //        "",
        //        "",
        //        CurrentUser.Account.UserId,
        //        CurrentUser.Account.UserId,
        //        "");

        //    if (System.IO.File.Exists(fileSource + strfilename))
        //    {
        //        GenerateCreditFileDao.UpdateDataProcess("2", "StatusCreditFileToFTP", "Start Send The File To FTP", "", "1", CurrentUser.Account.BankCode);

        //        using (StreamReader fileStream = new StreamReader(fileSource + strfilename))
        //        {
        //            fileBytes = Encoding.UTF8.GetBytes(fileStream.ReadToEnd());
        //            fileStream.Close();
        //        }

        //        try
        //        {
        //            //Create FTP Request.

        //            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftp + fileName);

        //            request.Method = WebRequestMethods.Ftp.UploadFile;

        //            request.Timeout = 3000000;

        //            //Enter FTP Server credentials.

        //            request.Credentials = new NetworkCredential(ftpUsername, ftpPassword);

        //            request.ContentLength = fileBytes.Length;

        //            request.UsePassive = true;

        //            request.UseBinary = true;

        //            request.ServicePoint.ConnectionLimit = fileBytes.Length;

        //            request.EnableSsl = false;



        //            using (Stream requestStream = request.GetRequestStream())

        //            {

        //                requestStream.Write(fileBytes, 0, fileBytes.Length);

        //                requestStream.Close();

        //            }



        //            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

        //            response.Close();

        //            GenerateCreditFileDao.UpdateCreditFileUploaded(col, "Y");
        //            GenerateCreditFileDao.UpdateDataProcess("4", "StatusCreditFileToFTP", "Credit File Sent Successfully", "", "2", CurrentUser.Account.BankCode);
        //            TempData["Notice"] = "Credit File Sent Successfully.";
        //        }

        //        catch (WebException ex)

        //        {
        //            TempData["ErrorMsg"] = ex.Message;
        //            GenerateCreditFileDao.UpdateDataProcess("3", "StatusCreditFileToFTP", ex.Message, "", "2", CurrentUser.Account.BankCode);
        //            throw new Exception((ex.Response as FtpWebResponse).StatusDescription);

        //        }
        //    }
        //    else
        //    {

        //        GenerateCreditFileDao.UpdateDataProcess("4", "StatusCreditFileToFTP", "File Not Found In The Local Folder", "", "1", CurrentUser.Account.BankCode);
        //        TempData["ErrorMsg"] = "File Not Found In The Local Folder";
        //    }
        //    return RedirectToAction("Index");

        //}

    }
}