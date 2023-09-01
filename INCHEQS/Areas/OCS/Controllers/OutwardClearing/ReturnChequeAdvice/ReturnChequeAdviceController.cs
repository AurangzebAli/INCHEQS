using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using System.Collections.Generic;
using INCHEQS.Models.Report;
using System.Threading.Tasks;
using System.IO;
using INCHEQS.Resources;
using System.Linq;

using INCHEQS.Models.Report.OCS;
using System.Web;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ReturnChequeAdvice
{

    public class ReturnChequeAdviceController : BaseController
    {

        private IPageConfigDaoOCS pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IReportServiceOCS reportServiceOCS;


        public ReturnChequeAdviceController(IPageConfigDaoOCS pageConfigDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, SystemProfileDao systemProfileDao, IReportServiceOCS reportServiceOCS)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;            
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.reportServiceOCS = reportServiceOCS;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnChequeAdvice.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ReturnChequeAdvice.INDEX));
            auditTrailDao.SecurityLog("Inward Return Document", "", TaskIdsOCS.ReturnChequeAdvice.INDEX, CurrentUser.Account);

            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnChequeAdvice.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ReturnChequeAdvice.INDEX, "View_ReturnChequeAdvice", "", "fldBankCode =@fldBankCode and SUBSTRING(TRIM(fldpresentingBankBranch),5,4) =@fldBranchCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode),new SqlParameter("@fldBranchCode", CurrentUser.Account.BranchCode)}),
             collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnChequeAdvice.GENERATE)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual async Task<ActionResult> Generate(FormCollection collection)
        {
            if (collection != null && collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    ReportModel reportModel = await reportServiceOCS.GetReportConfigByTaskIdAsync(TaskIdsOCS.ReturnChequeAdvice.PRINT);
                    string reportPath = reportModel.reportPath;

                    //Search Report Path in Project Or In Any Folder specified
                    if (!System.IO.File.Exists(reportPath))
                    {
                        reportPath = Server.MapPath(reportModel.reportPath);
                        if (!System.IO.File.Exists(reportPath))
                        {
                            return View("InwardClearing/Modal/_EmptyPrint");
                        }
                    }

                    string fileNameExtention = "pdf"; string mimeType;
                    reportServiceOCS.ReturnChequeAdviceConfig(reportModel, arrResult, reportPath, fileNameExtention, out mimeType);

                    Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
                    //auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
                    auditTrailDao.SecurityLog("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", TaskIdsOCS.ReturnChequeAdvice.INDEX, CurrentUser.Account);
                }
                TempData["Notice"] = "Processing completed successfully.";
            }
            else
            {
                TempData["ErrorMsg"] = "Radio buttons must be checked.";
            }

            

            return RedirectToAction("Index");
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnChequeAdvice.PRINT)]
        [GenericFilter(AllowHttpGet = true)]

        public virtual async Task<ActionResult> Print(FormCollection collection)
        {
            if (collection != null)
                {
                //List<string> arrResults = collection["Download"].Split(',').ToList();  && collection["Download"] != null
                //if (arrResults.Count > 1)
                //{
                //    TempData["ErrorMsg"] = "Radio buttons must be checked."; 
                //}
                //else if (arrResults.Count == 1)
                //{
                string itemId = collection["this_fldiriteminitialid"].Trim();
                    ReportModel reportModel = await reportServiceOCS.GetReportConfigByTaskIdAsync(TaskIdsOCS.ReturnChequeAdvice.PRINT);
                    string reportPath = reportModel.reportPath;

                    //Search Report Path in Project Or In Any Folder specified
                    if (!System.IO.File.Exists(reportPath))
                    {
                        reportPath = Server.MapPath(reportModel.reportPath);
                        if (!System.IO.File.Exists(reportPath))
                        {
                            return View("InwardClearing/Modal/_EmptyPrint");
                        }
                    }

                    string fileNameExtention = "pdf"; string mimeType;
                    byte[] renderedBytes = reportServiceOCS.ReturnChequeAdviceConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
                //auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
                auditTrailDao.SecurityLog("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", TaskIdsOCS.ReturnChequeAdvice.INDEX, CurrentUser.Account);
                return File(renderedBytes, mimeType);
                //}
            }
            else
            {
                TempData["Warning"] = Locale.Nodatawasselected;
            }

            return RedirectToAction("Index");
        }
    }
}