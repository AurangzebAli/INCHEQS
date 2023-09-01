using INCHEQS.Areas.OCS.Models.ClearingItem;
using INCHEQS.Areas.OCS.Models.HostFile;
using INCHEQS.Areas.OCS.Models.OCSAPIPosting;
using INCHEQS.Common.Resources;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.HostFileProcessing.OCSAPIPosting
{
    public class OCSCreditPostingController : BaseController
    {
        private IPageConfigDaoOCS pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileOCSDao hostFileDao;
        private readonly OCSIDataProcessDao OCScleardataProcess;
        private readonly IOCSCreditPostingDao OCSCreditPostingDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;



        public OCSCreditPostingController(IPageConfigDaoOCS pageConfigDao, IAuditTrailDao auditTrailDao, IClearingItemDao IClearingItemDao, IHostFileOCSDao hostFileDao, OCSIDataProcessDao OCScleardataProcess, IOCSCreditPostingDao OCSCreditPostingDao, ISystemProfileDao systemProfileDao, ISearchPageService searchPageService)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.IClearingItemDao = IClearingItemDao;
            this.hostFileDao = hostFileDao;
            this.OCScleardataProcess = OCScleardataProcess;
            this.OCSCreditPostingDao = OCSCreditPostingDao;
            this.systemProfileDao = systemProfileDao;
            this.searchPageService = searchPageService;


        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateCreditFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.GenerateCreditFile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateCreditFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateCreditFile.INDEX, "View_GetItemsforPosting", "PresentingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                string strPostingMode = systemProfileDao.GetValueFromSystemProfile("PostingMode", CurrentUser.Account.BankCode).Trim();
                ViewBag.PostingMode = strPostingMode;
                if (strPostingMode == "FILE" || strPostingMode == "File" || strPostingMode == "file")
                {
                    ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateCreditFile.FILEBASEPOSTED, "View_GetFileBasePostedtems", "PresentingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                     collection);
                }
                else
                {
                    ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateCreditFile.POSTED, "View_GetPostedtems", "PresentingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                     collection);
                }

            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateCreditFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            string t = collection["Type"];
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
                HostFileOCSModel gHostFileModel = new HostFileOCSModel();
                gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsOCS.GenerateCreditFile.INDEX);
                string processName = gHostFileModel.fldProcessName;
                string posPayType = gHostFileModel.fldPosPayType;
                string reUpload = "N";
                string taskId = TaskIdsOCS.GenerateCreditFile.INDEX;
                string batchId = "";
                string bankcodeChkbox = "";
                string clearDateDDMMYYYY = collection["row_fldcapturingdate"];
                string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                if (t.Equals("Ready"))
                {
                    //bankcodeChkbox = "029";
                    bool runningProcess = OCScleardataProcess.CheckRunningProcessWithoutPosPayType(processName, clearDateyyyyMMdd, CurrentUser.Account.BankCode);
                    if (runningProcess)
                    {
                        //Delete previous data process
                        OCScleardataProcess.DeleteDataProcessWithoutPosPayType(processName, CurrentUser.Account.BankCode);

                        List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                        //bankcodeChkbox = arrResults[0].ToString();
                        for (int i = 0; i <= arrResults.Count - 1; i++)
                        {
                            bankcodeChkbox = arrResults[i].ToString();
                            OCSCreditPostingDao.GenerateNewBatches(bankcodeChkbox, CurrentUser.Account.UserId, clearDateyyyyMMdd, collection["row_totalitem"], Convert.ToInt64(collection["row_totalamount"].Replace(",", "").Replace(".", "")));
                        }

                        //Insert new data process
                        OCScleardataProcess.InsertToDataProcess(
                            CurrentUser.Account.BankCode,
                            processName,
                            posPayType,
                            clearDateyyyyMMdd,
                            reUpload,
                            taskId,
                            batchId,
                            CurrentUser.Account.UserId,
                            CurrentUser.Account.UserId);
                        auditTrailDao.Log("Generate Credit Posting - Date : " + clearDateyyyyMMdd + ". TaskId :" + TaskIdsOCS.GenerateCreditFile.INDEX, CurrentUser.Account);
                        TempData["Notice"] = "Data Successfully Submitted to Perform Credit Posting.";
                    }
                    else
                    {
                        TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                    }
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["ErrorMsg"] = "No Data was selected";
            }
            return RedirectToAction("Index");
        }
        public virtual ActionResult Download(FormCollection collection)
        {
            string filePath = collection["this_fldFilePath"].Trim();
            string fileName = collection["this_fldFileName"].Trim();
            string fullFileName = string.Format("{0}", Path.Combine(filePath, fileName));
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                //Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, "txt"));
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
                auditTrailDao.Log("Credit Posting - Downloaded Credit Posting File : " + filePath, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(filePath));
            }
            else
            {
                return null;
            }
        }
        public virtual JsonResult RegeneratePosting(FormCollection collection)
        {
            string notice = "";
            bool flag ;
            ViewBag.Type = "Submitted";
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            Int64 NewBatchNumber = Convert.ToInt64(filter["fldPostingBatch"].Trim());
            NewBatchNumber = NewBatchNumber + 1;
            HostFileOCSModel gHostFileModel = new HostFileOCSModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsOCS.GenerateCreditFile.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsOCS.GenerateCreditFile.INDEX;
            string batchId = "";
            string clearDate = collection["row_fldPostingBatch"].Substring(0, 8);
            string clearDateyyyyMMdd = DateTime.ParseExact(clearDate, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            bool runningProcess = OCScleardataProcess.CheckRunningProcessWithoutPosPayType(processName, clearDateyyyyMMdd, CurrentUser.Account.BankCode);
            if (OCSCreditPostingDao.checkBatchExist(NewBatchNumber) == false)
            {
                if (runningProcess)
                {
                    flag = OCSCreditPostingDao.RegeneratePosting(collection);
                    if (flag == true)
                    {
                        //Insert new data process
                        OCScleardataProcess.InsertToDataProcess(
                            CurrentUser.Account.BankCode,
                            processName,
                            posPayType,
                            clearDateyyyyMMdd,
                            reUpload,
                            taskId,
                            batchId,
                            CurrentUser.Account.UserId,
                            CurrentUser.Account.UserId);
                        auditTrailDao.Log("Re-Generate Credit Posting - Date : " + clearDateyyyyMMdd + ". TaskId :" + TaskIdsOCS.GenerateCreditFile.INDEX, CurrentUser.Account);
                        notice = "Data Successfully Submitted to Perform Credit Posting.";
                    }
                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                } 
            }
            else
            {
                notice = "Batch no : '"+ NewBatchNumber + "' is already Generated.";
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PostedItemHistory(FormCollection collection)
        {
            string strPostingMode = systemProfileDao.GetValueFromSystemProfile("PostingMode", CurrentUser.Account.BankCode).Trim();
            if (strPostingMode == "FILE" || strPostingMode == "File" || strPostingMode == "file")
            {
                ViewBag.PostedHistory = OCSCreditPostingDao.PostedItemFileBaseHistory(collection);
                return PartialView("Modal/_PostedItemFileBaseHistoryPopup");
            }
            else
            {
                ViewBag.PostedHistory = OCSCreditPostingDao.PostedItemHistory(collection);
                return PartialView("Modal/_PostedItemHistoryPopup");
            }

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = OCSCreditPostingDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }
    }
}