
using INCHEQS.Models.HostFile;
using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
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
using INCHEQS.Models.FileInformation;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Common;
using INCHEQS.Areas.ICS.Models.ProgressStatus;
using System.IO;
using INCHEQS.Areas.ICS.Models.TransCode;

namespace INCHEQS.Areas.ICS.Controllers.HostFileProcessing.ICSAPIDebitPosting
{
    public class ICSDebitPostingController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        //private IClearingItemDao IClearingItemDao;
        protected readonly IHostFileDao hostFileDao;
        private readonly IDataProcessDao dataProcessDao;
        private readonly IICSDebitPostingDao ICSDebitPostingDao;
        protected readonly FileInformationService fileInformationService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IProgressStatusDao progressStatusDao;

        private ITransCodeDao transCodeDao;

        public ICSDebitPostingController(ITransCodeDao transCodeDao, IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, IHostFileDao hostFileDao, IDataProcessDao dataProcessDao, IICSDebitPostingDao ICSDebitPostingDao, IFileManagerDao fileManagerDao, ISystemProfileDao systemProfileDao, FileInformationService fileInformationService, IProgressStatusDao progressStatusDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            // this.IClearingItemDao = IClearingItemDao;
            this.hostFileDao = hostFileDao;
            this.dataProcessDao = dataProcessDao;
            this.ICSDebitPostingDao = ICSDebitPostingDao;
            this.systemProfileDao = systemProfileDao;
            this.fileInformationService = fileInformationService;
            this.progressStatusDao = progressStatusDao;
            this.transCodeDao = transCodeDao;

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.FolderList = fileInformationService.getDestinationPath("ICSGenerateDebitFile");
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateDebitFile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string clearDate = progressStatusDao.getClearDate();
            string clearDateDebitFile = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string systemProfile = systemProfileDao.GetValueFromInterfaceFileMasterICS("GWCConsolidatedPath", CurrentUser.Account.BankCode).Trim();
            string GWCConsolidatedFolder = systemProfile + @"\" + clearDateDebitFile + @"\IW\";
            int bankCodeSubString = 32;

            if (Directory.Exists(GWCConsolidatedFolder))
            {
                ViewBag.FileList = fileInformationService.GetAllFileInFolderICSDP(GWCConsolidatedFolder, bankCodeSubString, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                if (ViewBag.FileList.Count > 0)
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

            //string strUserBranch = "";
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateDebitFile.INDEX, "View_GetICSItemsforDebitPosting", "IssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateDebitFile.POSTED, "View_GetICSDebitPostedtems", "IssuingBankCode", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public JsonResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            HostFileModel gHostFileModel = new HostFileModel();   
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(TaskIdsICS.GenerateDebitFile.INDEX);
            string processName = gHostFileModel.fldHostFileDesc;
            string posPayType = gHostFileModel.fldProcessName;
            string reUpload = "N";
            string taskId = TaskIdsICS.GenerateDebitFile.INDEX;
            string batchId = "";
            //string bankcodeChkbox = "";
            string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDate = collection["fldClearDate"];
            string filename = "";
            string notice = "";
            //string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            //string clearDateyyyyMMdd = DateTime.ParseExact(clearDateDDMMYYYY, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            string t = collection["Type"];
            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (collection != null)
            {
                //if (t.Equals("Ready"))
                //{
                //bankcodeChkbox = "029";
                //Delete previous data process

                //bankcodeChkbox = arrResults[0].ToString();

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



                auditTrailDao.Log("Generate Debit Posting - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateDebitFile.INDEX, CurrentUser.Account);
                notice = "Data Successfully Submitted to Perform Debit Posting.";

                //return RedirectToAction("Index");
                //}
            }
            else
            {
                notice = "No Data was selected";
            }
            //return RedirectToAction("Index");
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PostedItemHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = ICSDebitPostingDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }
        public ActionResult ReadyItemForPostingHistory(FormCollection collection)
        {

            ViewBag.PostedHistory = ICSDebitPostingDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostedPopup");

        }
        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateDebitFile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult ReadGWCConsolidatedFiles(FormCollection collection)
        {
            string clearDate = DateTime.ParseExact(collection["fldcleardate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string systemProfile = systemProfileDao.GetValueFromInterfaceFileMasterICS("GWCConsolidatedPath", CurrentUser.Account.BankCode).Trim();
            string GWCConsolidatedFolder = systemProfile + @"\" + clearDate + @"\IW\";
            int bankCodeSubString = 32;
            ViewBag.SourcePath = GWCConsolidatedFolder;
            ViewBag.FileList = fileInformationService.GetAllFileInFolderICSDP(GWCConsolidatedFolder, bankCodeSubString, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
            return PartialView("Modal/ReadGWCConsolidatedFiles");
        }
    }
}