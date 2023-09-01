
//using INCHEQS.Models.HostFile;
using INCHEQS.Areas.OCS.Models.OCSAPIDebitPosting;
using INCHEQS.Common.Resources;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Processes.DataProcess;
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
using INCHEQS.Areas.OCS.Models.HostFile;

namespace INCHEQS.Areas.OCS.Controllers.HostFileProcessing.ICSAPIDebitPosting
{
    public class OCSDebitPostingController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        //private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileOCSDao hostFileOCSDao;
        private readonly IDataProcessDao dataProcessDao;
        private readonly IOCSDebitPostingDao OCSDebitPostingDao;


        public OCSDebitPostingController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, IHostFileOCSDao hostFileOCSDao, IDataProcessDao dataProcessDao, IOCSDebitPostingDao OCSDebitPostingDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            // this.IClearingItemDao = IClearingItemDao;
            this.hostFileOCSDao = hostFileOCSDao;
            this.dataProcessDao = dataProcessDao;
            this.OCSDebitPostingDao = OCSDebitPostingDao;

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.GenerateDebitFile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateDebitFile.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateDebitFile.INDEX, "View_GetOCSItemsforDebitPosting", "PresentingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateDebitFile.POSTED, "View_GetOCSDebitPostedtems", "PresentingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileOCSModel gHostFileModel = new HostFileOCSModel();
            gHostFileModel = hostFileOCSDao.GetDataFromHostFileConfig(TaskIdsOCS.GenerateDebitFile.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsOCS.GenerateDebitFile.INDEX;
            string batchId = "";
            string bankcodeChkbox = "";
            string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            string filename = "";
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
                if (t.Equals("Ready"))
                {
                    //bankcodeChkbox = "029";
                    bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDateDDMMYYYY, CurrentUser.Account.BankCode);
                    if (runningProcess)
                    {
                        //Delete previous data process
                        dataProcessDao.DeleteDataProcessWithoutPosPayType(processName, CurrentUser.Account.BankCode);

                        List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                        //bankcodeChkbox = arrResults[0].ToString();
                        for (int i = 0; i <= arrResults.Count - 1; i++)
                        {
                            bankcodeChkbox = arrResults[i].ToString();
                            OCSDebitPostingDao.GenerateNewBatches(bankcodeChkbox, CurrentUser.Account.UserId, clearDateDDMMYYYY, collection["row_totalitem"], collection["row_totalamount"]);
                        }

                        //Insert new data process
                        dataProcessDao.InsertToDataProcess(
                            CurrentUser.Account.BankCode,
                            processName,
                            posPayType,
                            clearDateDDMMYYYY,
                            reUpload,
                            taskId,
                            batchId,
                            CurrentUser.Account.UserId,
                            CurrentUser.Account.UserId,
                            filename);



                        auditTrailDao.Log("Generate Debit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsOCS.GenerateDebitFile.INDEX, CurrentUser.Account);
                        TempData["Notice"] = "Data Successfully Submitted to Perform Debit Posting.";
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

        public ActionResult PostedItemHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = OCSDebitPostingDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = OCSDebitPostingDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }
    }
}