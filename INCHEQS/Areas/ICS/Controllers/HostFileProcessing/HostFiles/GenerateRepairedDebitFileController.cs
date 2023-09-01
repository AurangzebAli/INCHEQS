
using INCHEQS.Models.HostFile;
using INCHEQS.Areas.ICS.Models.GenerateRepairedDebitFile;
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
using System.Net;
using System.Text;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.ICSAPIDebitPosting
{
    public class GenerateRepairedDebitFileController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        //private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileDao hostFileDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        private readonly IGenerateRepairedDebitFileDao GenerateRepairedDebitFileDao;


        public GenerateRepairedDebitFileController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, IHostFileDao hostFileDao, ICSIDataProcessDao cleardataProcess, IGenerateRepairedDebitFileDao GenerateRepairedDebitFileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            // this.IClearingItemDao = IClearingItemDao;
            this.hostFileDao = hostFileDao;
            this.cleardataProcess = cleardataProcess;
            this.GenerateRepairedDebitFileDao = GenerateRepairedDebitFileDao;

        }


        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateRepairedDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateRepairedDebitFile.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX, "View_GetRepairedDebitItemsForPosting", "fldIssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateRepairedDebitFile.POSTED, "View_GetRepairedDebitPostedItems", "fldStartTime", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateRepairedDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsICS.GenerateRepairedDebitFile.INDEX;
            string batchId = "";
            string bankcodeChkbox = "";
            string totalItem = "";
            string totalAmount = "";
           // string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDate = collection["fldClearDate"];
            string clearDateDDMMYYYY = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
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
                        bankcodeChkbox = GenerateRepairedDebitFileDao.getBetween(arrResult, "ae", "bf");
                        totalItem = GenerateRepairedDebitFileDao.getBetween(arrResult, "bf", "cg");
                        totalAmount = GenerateRepairedDebitFileDao.getBetween(arrResult, "cg", "dh");

                        bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDate, bankcodeChkbox);
                        if (runningProcess)
                        {
                            GenerateRepairedDebitFileDao.GenerateNewBatches(bankcodeChkbox, gHostFileModel.fldProcessName, gHostFileModel.fldFileExt, CurrentUser.Account.UserId, clearDateDDMMYYYY, totalItem, totalAmount);

                            //Insert new data process
                            cleardataProcess.InsertToDataProcessICS(bankcodeChkbox, processName, posPayType, clearDate, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                            auditTrailDao.Log("Generate Debit Posting - Date : " + clearDate + ". TaskId :" + TaskIdsICS.GenerateRepairedDebitFile.INDEX, CurrentUser.Account);
                            //notice = "Generate Repaired Debit File has been triggered";
                            TempData["Notice"] = "Generate Repaired Debit File has been triggered";
                        }
                        else
                        {
                            //notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                            TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                        }
                    }
                                
                        //}
           
                }
                    
            }
            else
            {
                TempData["Warning"] = "No Data was selected";
                //notice = "No Data was selected";
            }
            return RedirectToAction("Index");
            //return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult PostedItemHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = GenerateRepairedDebitFileDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = GenerateRepairedDebitFileDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateRepairedDebitFile.INDEX)]
        [HttpPost()]
        public virtual ActionResult Download(FormCollection collection)
        {
            //initializeBeforeAction();
            string taskId = TaskIdsICS.GenerateRepairedDebitFile.INDEX;
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX);

            string filePath = collection["this_fldfilepath"];
            string fileName = collection["this_fldfilename"];
            string fullFileName = filePath + fileName;
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, ""));
                auditTrailDao.Log("Generate Debit Posting Download File : " + fullFileName + ". TaskId :" + TaskIdsICS.GenerateRepairedDebitFile.INDEX, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }


        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateRepairedDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult ReGenerate(FormCollection collection)
        {
            string taskId = TaskIdsICS.GenerateRepairedDebitFile.INDEX;
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX);
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
                        GenerateRepairedDebitFileDao.ReGenerateBatches(bankcodeChkbox, previousbatch , gHostFileModel.fldProcessName, gHostFileModel.fldFileExt, CurrentUser.Account.UserId, clearDateDDMMYYYY, totalItem, totalAmount);

                        //Insert new data process
                        cleardataProcess.InsertToDataProcessICS(bankcodeChkbox, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                        auditTrailDao.Log("Generate Debit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateRepairedDebitFile.INDEX, CurrentUser.Account);
                        TempData["Notice"] = "Generate Repaired Debit File has been triggered";
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
        //    gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateRepairedDebitFile.INDEX);

        //    string processname = gHostFileModel.fldProcessName;
        //    string bankcode = gHostFileModel.fldBankCode;
        //    GenerateRepairedDebitFileModel postingStatusModel = GenerateRepairedDebitFileDao.GetRepairedDebitFilePath(processname, bankcode);
        //    //FTP Server URL.

        //    string ftp = postingStatusModel.fldFileDestination;
        //    string ftpUsername = GenerateRepairedDebitFileDao.GetFTPUserName();
        //    string ftpPassword = GenerateRepairedDebitFileDao.GetFTPPassword();

        //    //FTP Folder name. Leave blank if you want to upload to root folder.

        //    //string ftpFolder = System.Configuration.ConfigurationManager.AppSettings["FTPFolderName"];

        //    byte[] fileBytes = null;

        //    string fileSource = postingStatusModel.fldFileSource;
        //    string strfilename = postingStatusModel.fldFileName;
        //    string clearDate = col["fldClearDate"].ToString();

        //    //clearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");

        //    //strfilename = strfilename.Replace("YYYYMMDD", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd"));
        //    strfilename = strfilename.Replace("YYYYMMDD", DateTime.Now.ToString("yyyyMMdd"));

        //    //Read the FileName and convert it to Byte array.

        //    string fileName = Path.GetFileName(strfilename);

        //    //Insert new data process
        //    cleardataProcess.InsertToDataProcessICS(
        //        CurrentUser.Account.BankCode,
        //        "StatusRepairedDebitFileToFTP",
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
        //        GenerateRepairedDebitFileDao.UpdateDataProcess("2", "StatusRepairedDebitFileToFTP", "Start Send The File To FTP", "", "1", CurrentUser.Account.BankCode);

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

        //            GenerateRepairedDebitFileDao.UpdateDebitFileUploaded(col, "Y");
        //            GenerateRepairedDebitFileDao.UpdateDataProcess("4", "StatusRepairedDebitFileToFTP", "Debit File Sent Successfully", "", "2", CurrentUser.Account.BankCode);
        //            TempData["Notice"] = "Repaired Debit File Sent Successfully.";
        //        }

        //        catch (WebException ex)

        //        {
        //            TempData["ErrorMsg"] = ex.Message;
        //            GenerateRepairedDebitFileDao.UpdateDataProcess("3", "StatusRepairedDebitFileToFTP", ex.Message, "", "2", CurrentUser.Account.BankCode);
        //            throw new Exception((ex.Response as FtpWebResponse).StatusDescription);

        //        }
        //    }
        //    else
        //    {

        //        GenerateRepairedDebitFileDao.UpdateDataProcess("4", "StatusRepairedDebitFileToFTP", "File Not Found In The Local Folder", "", "1", CurrentUser.Account.BankCode);
        //        TempData["ErrorMsg"] = "File Not Found In The Local Folder";
        //    }
        //    return RedirectToAction("Index");

        //}

    }
}