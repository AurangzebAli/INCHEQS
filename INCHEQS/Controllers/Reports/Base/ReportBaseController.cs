using INCHEQS.Helpers;
using INCHEQS.Models.SearchPageConfig;
using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.IO;
using INCHEQS.Models.Report;
using System.Threading.Tasks;
using INCHEQS.Security;
//using INCHEQS.Models.AuditTrail;
using INCHEQS.Resources;
//using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Common;
using System.Globalization;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Mime;
using log4net;
using System.Security.Policy;
using System.Web.Services.Description;

namespace INCHEQS.Controllers.Reports.Base {

    public abstract class ReportBaseController : BaseController {
        protected readonly IAuditTrailDao auditTrailDao;
        protected IPageConfigDao pageConfigDao;
        protected IReportService reportService;
        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }

        //Must be implemented by inheriting class
        protected abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;
        protected string searchPageHtml2;
        protected string searchPageHtml3;
        protected string searchPageHtml4;
        protected string searchPageHtml5;
        protected string searchPageHtml6;
        protected string searchPageHtml7;
        protected string searchPageHtml8;
        protected string searchPageHtml9;
        protected string searchPageHtml10;
        protected string searchPageHtml11;
        protected string searchPageHtml20;
        protected string searchPageHtml21;
        protected string generateReportHtml;
        protected string OCSSearchPageHtml;
        protected string ICSSearchPageHtml;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;

        /// <summary>
        /// This function should be called inside All Actions in ICCSBaseController and it's Inheritence Controller.
        /// This is to protect from UNAUTHORIZED ACCESS to the page through TASKID returned by setupPageSqlConfig().
        /// This function is important and have to be called or else, the page won't work
        /// Returns: PageSqlConfig set in setupPageSqlConfig();
        /// </summary>
        [NonAction]
        protected PageSqlConfig initializeBeforeAction() {

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
            try {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access         
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            } catch (HttpException) {
                throw new HttpException(Locale.AccessDenied);
            }
        }


        public ReportBaseController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IReportService reportService) {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.reportService = reportService;
            this.searchPageHtml = searchPageHtml != null ? searchPageHtml : "Report/SearchPage";
            this.searchPageHtml2 = searchPageHtml2 != null ? searchPageHtml2 : "Report/SearchPage2";
            this.searchPageHtml3 = searchPageHtml3 != null ? searchPageHtml3 : "Report/SearchPage3";
            this.searchPageHtml4 = searchPageHtml4 != null ? searchPageHtml4 : "Report/SearchPage4";
            this.searchPageHtml5 = searchPageHtml5 != null ? searchPageHtml5 : "Report/SearchPage5";
            this.searchPageHtml6 = searchPageHtml6 != null ? searchPageHtml6 : "Report/SearchPage6";
            this.searchPageHtml7 = searchPageHtml7 != null ? searchPageHtml7 : "Report/SearchPage7";
            this.searchPageHtml8 = searchPageHtml8 != null ? searchPageHtml8 : "Report/SearchPage8";
            this.searchPageHtml9 = searchPageHtml9 != null ? searchPageHtml9 : "Report/SearchPage9";
            this.searchPageHtml20 = searchPageHtml20 != null ? searchPageHtml10 : "Report/SearchPage10";
            this.searchPageHtml21 = searchPageHtml21 != null ? searchPageHtml11 : "Report/SearchPage11";
            this.generateReportHtml = generateReportHtml != null ? generateReportHtml : "Report/GenerateReport";
            this.OCSSearchPageHtml = OCSSearchPageHtml != null ? OCSSearchPageHtml : "Report/OCSSearchPage";
            this.ICSSearchPageHtml = ICSSearchPageHtml != null ? ICSSearchPageHtml : "Report/ICSSearchPage";
        }

        // GET: ICS/ReportBase
        public virtual async Task<ActionResult> Index() {
            initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.ClearingDate = CurrentUser.Account.ClearingDate;
            if (ViewBag.SearchPage.PageTitle == "Daily Inward Clearing Summary Report" || ViewBag.SearchPage.PageTitle == "Daily Outward Return Summary Report")
            {
                return View(searchPageHtml2);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily Inward Clearing Details Report")
            {
                return View(searchPageHtml3);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily Outward Return Details Report")
            {
                return View(searchPageHtml4);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily Outward Clearing Summary Report" || ViewBag.SearchPage.PageTitle == "Daily Outward Clearing Details Report" ||
                ViewBag.SearchPage.PageTitle == "Daily Inward Return Summary Report" || ViewBag.SearchPage.PageTitle == "Daily Inward Return Details Report" ||
                ViewBag.SearchPage.PageTitle == "Daily OCS Credit Posting Report" || ViewBag.SearchPage.PageTitle == "Daily Outward Clearing Summary (by Drawee Bank) Report" ||
                ViewBag.SearchPage.PageTitle == "Daily Outward Clearing Details (by Drawee Bank) Report" || ViewBag.SearchPage.PageTitle == "Daily OCS Debit Posting Report")
            {
                return View(OCSSearchPageHtml);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily ICS Credit Posting Report" || ViewBag.SearchPage.PageTitle == "Daily ICS Debit Posting Report")
            {
                return View(ICSSearchPageHtml);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily Inward Clearing Details (by Presenting Bank) Report")
            {
                return View(searchPageHtml5);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily Inward Clearing Summary (by Presenting Bank) Report")
            {
                return View(searchPageHtml6);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily ICS Summary by Date Report" || ViewBag.SearchPage.PageTitle == "Daily ICS Summary by Presenting Bank Report" || ViewBag.SearchPage.PageTitle == "Daily ICS Details by Date Report")
            {
                return View(searchPageHtml7);
            }
            else if (ViewBag.SearchPage.PageTitle == "Daily OCS Summary by Date Report" || ViewBag.SearchPage.PageTitle == "Daily OCS Summary by Drawee Bank Report" || ViewBag.SearchPage.PageTitle == "Daily OCS Details by Date Report")
            {
                return View(searchPageHtml8);
            }
            else if (ViewBag.SearchPage.TaskId.ToString().Trim() == "304770")
            {
                return View(searchPageHtml9);
            }
            else if (ViewBag.SearchPage.PageTitle == "CO/DD/DV/BA Report" || ViewBag.SearchPage.PageTitle == "Non Conformance Item Report" || ViewBag.SearchPage.PageTitle == "Summary of Total Inward Clearing Cheque Report" || ViewBag.SearchPage.PageTitle == "Duplicate Inward Item Report")
            {
                return View(searchPageHtml20);
            }
            else if (ViewBag.SearchPage.PageTitle == "User Performance Report")
            {
                return View(searchPageHtml21);
            }
            else
            {
                // xx start 20210517
                if (@ViewBag.SearchPage.PageTitle == "Report - Host Status Report" || @ViewBag.SearchPage.PageTitle == "Report - Host Status Report By Collecting Bank")
                {
                    ViewBag.HostStatus = reportService.ListBankHostStatus();
                }
                // xx end 20210517
                return View(searchPageHtml);
            }
        }

        public virtual async Task<ActionResult> PrintReport() {
            initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            return View("Report/ReportParentForm");
        }


        public virtual async Task<ActionResult> GenerateReport(string reportType, FormCollection collection) {
            initializeBeforeAction();

            try
            {
                if (pageSqlConfig.TaskId == "304760")
                {
                    pageSqlConfig.SetTaskId(reportService.checkClearDate(pageSqlConfig.TaskId, collection));
                }
                // xx start 20210528
                if (pageSqlConfig.TaskId == "304770" && (collection["chkSummary"] != null && collection["chkSummary"] != ""))
                {
                    pageSqlConfig.SetTaskId("304771");
                }
                // xx end 20210528
                ReportModel reportModel = await GetReportModel(reportType);
                string mimeType;
                byte[] renderedBytes;

                if (!pageSqlConfig.PrintReportParam.Equals("page")) {
                    renderedBytes = reportService.renderReportBasedOnConfig(reportModel, collection, reportModel.reportPath, reportType, out mimeType);
                    
                }
                else {
                    renderedBytes = reportService.renderReportBasedOnConfigForPage(reportModel, collection, reportModel.reportPath, reportType, out mimeType);
                }

                //Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, reportModel.extensionType));
                CurrentUser.Account.TaskId = reportModel.taskId;
                //auditTrailDao.Log("Generate Report '" + reportModel.extentionFilename+"' in "+ reportType, CurrentUser.Account);
                auditTrailDao.SecurityLog("Generate Report '" + reportModel.extentionFilename + "' in " + reportType, "", reportModel.taskId, CurrentUser.Account);

                return File(renderedBytes, mimeType); 



            }
            catch (Exception ex) {
                throw ex;
            }
        }




        public virtual ActionResult GetReportType() {
            initializeBeforeAction();
            return View("Report/ModalReportType");
        }

        private async Task<ReportModel> GetReportModel(string reportType) {
            //Azim start
            ReportModel reportModel = await reportService.GetReportConfigAsync(pageSqlConfig, reportType);
            //Azim End
            string reportPath = reportModel.reportPath;

            //Search Report Path in Project Or In Any Folder specified
            if (!System.IO.File.Exists(reportPath)) {
                if (reportPath.Contains("~"))
                {
                    reportPath = Server.MapPath(reportModel.reportPath);

                }

                if (!System.IO.File.Exists(reportPath)) {
                    throw new Exception("Report Path Not Found");
                }
                reportModel.reportPath = reportPath;
            }

            string fileNameExtention = "pdf";
            if (ReportType.Extensions.ContainsKey(reportType)) {
                fileNameExtention = ReportType.Extensions[reportType];
            }
            reportModel.extensionType = fileNameExtention;
            return reportModel;

        }

    }
}