
using INCHEQS.Models.HostFile;
using INCHEQS.Areas.ICS.Models.ICSAPICreditPosting;
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
using INCHEQS.Areas.ICS.Models.HostFile;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.ICSAPICreditPosting
{
    public class ICSCreditPostingController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        //private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileDao hostFileDao;
        private readonly IDataProcessDao dataProcessDao;
        private readonly IICSCreditPostingDao ICSCreditPostingDao;


        public ICSCreditPostingController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, IHostFileDao hostFileDao, IDataProcessDao dataProcessDao, IICSCreditPostingDao ICSCreditPostingDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            // this.IClearingItemDao = IClearingItemDao;
            this.hostFileDao = hostFileDao;
            this.dataProcessDao = dataProcessDao;
            this.ICSCreditPostingDao = ICSCreditPostingDao;

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateCreditFile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFile.INDEX)]
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateCreditFile.INDEX, "View_GetICSItemsforCreditPosting", "IssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateCreditFile.POSTED, "View_GetICSCreditPostedtems", "IssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateCreditFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateCreditFile.INDEX);
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsICS.GenerateCreditFile.INDEX;
            string batchId = "";
            string bankcodeChkbox = "";

            string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDate = collection["fldClearDate"];
            //string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            //string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            string t = collection["Type"];
            string filename = "";
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
                    bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayTypeICS(processName, clearDateDDMMYYYY, CurrentUser.Account.BankCode);
                    if (runningProcess)
                    {
                        //Delete previous data process
                        dataProcessDao.DeleteDataProcessWithoutPosPayTypeICS(processName, CurrentUser.Account.BankCode);

                        List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                        //bankcodeChkbox = arrResults[0].ToString();
                        for (int i = 0; i <= arrResults.Count - 1; i++)
                        {
                            bankcodeChkbox = arrResults[i].ToString();
                            ICSCreditPostingDao.GenerateNewBatches(bankcodeChkbox, CurrentUser.Account.UserId, clearDateDDMMYYYY, collection["row_totalitem"], collection["row_totalamount"]);
                        }

                        //Insert new data process
                        dataProcessDao.InsertToDataProcessICS(
                            CurrentUser.Account.BankCode,
                            processName,
                            posPayType,
                            clearDate,
                            reUpload,
                            taskId,
                            batchId,
                            CurrentUser.Account.UserId,
                            CurrentUser.Account.UserId,
                            filename);



                        auditTrailDao.Log("Generate Credit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateCreditFile.INDEX, CurrentUser.Account);
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

        public ActionResult PostedItemHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = ICSCreditPostingDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = ICSCreditPostingDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }
    }
}