﻿using System.Web;
using System.Web.Optimization;
using System.Web.UI.WebControls;

namespace INCHEQS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.IgnoreList.Clear();

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui.min.js",
                        "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.min.js"));

            //Use the development version of Modernizr to develop with and learn from. Then, when you're
            //ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/lodash").Include(
                        "~/Scripts/lodash.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootbox").Include(
                   "~/Scripts/bootbox.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/dropDown.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/site.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/ProgressBarWindow.css"));

            bundles.Add(new StyleBundle("~/Content/dataTable").Include(
                      "~/Content/jquery.dataTable-1.10.11.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/dataTable").Include(
                       "~/Scripts/jquery.dataTable-1.10.11.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/moment").Include(
                       "~/Scripts/moment-min.js"));

            bundles.Add(new ScriptBundle("~/bundles/AppConfig").Include(
                       "~/Scripts/App/appConfig.js"));


            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                       "~/Scripts/App/app.js",
                       "~/Scripts/App/pagination.js"));

            bundles.Add(new ScriptBundle("~/bundles/session").Include(
                       "~/Scripts/App/session.js"));

            bundles.Add(new ScriptBundle("~/bundles/menu").Include(
                       "~/Scripts/App/menu.js"));

            bundles.Add(new ScriptBundle("~/bundles/SearchPage").Include(
                       "~/Scripts/App/searchPage.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/SearchResultPage").Include(
                       "~/Scripts/App/searchResultPage.js"));

            bundles.Add(new ScriptBundle("~/bundles/SearchBtn").Include(
                       "~/Scripts/App/searchbtn.js"));

            bundles.Add(new ScriptBundle("~/bundles/FileUpload").Include(
                       "~/Scripts/App/fileUpload.js"));

            bundles.Add(new ScriptBundle("~/bundles/Report").Include(
                       "~/Scripts/App/report.js"));

            bundles.Add(new ScriptBundle("~/bundles/ChequeVerification").Include(
                       "~/Scripts/App/chequeVerification.js"));

            bundles.Add(new ScriptBundle("~/bundles/ChequeOCS").Include(
                      "~/Scripts/OCS/cheque.js"));

            bundles.Add(new ScriptBundle("~/bundles/ChequeICS").Include(
                       "~/Scripts/ICS/cheque.js"));
            bundles.Add(new ScriptBundle("~/bundles/ChequeBranch").Include(
                      "~/Scripts/ICS/chequeBranch.js"));
            bundles.Add(new ScriptBundle("~/bundles/ChequeBack").Include(
                      "~/Scripts/ICS/chequeBack.js"));

            bundles.Add(new ScriptBundle("~/bundles/BranchChequeICS").Include(
                       "~/Scripts/ICS/branchcheque.js"));

            bundles.Add(new ScriptBundle("~/bundles/BankBranchesOcs").Include(
         "~/Scripts/App/BankBranchesOcs.js"));

            bundles.Add(new ScriptBundle("~/bundles/BankBranchesOcsSW").Include(
                    "~/Scripts/App/BankBranchesOcsSW.js"));

            bundles.Add(new ScriptBundle("~/bundles/BankBranchMaintain").Include(
         "~/Scripts/App/BankBranchesOcsSWMaintain.js"));

            bundles.Add(new ScriptBundle("~/bundles/RejectCodeApi").Include(
                       "~/Scripts/App/rejectCodeApi.js"));

            bundles.Add(new ScriptBundle("~/bundles/RejectCodeBranchApi").Include(
                       "~/Scripts/App/rejectCodeBranchApi.js"));

            bundles.Add(new ScriptBundle("~/bundles/DataProcessRealTime").Include(
                       "~/Scripts/App/dataProcessRealTime.js"));
            bundles.Add(new ScriptBundle("~/bundles/DataProcessRefreshPage").Include(
                       "~/Scripts/App/dataProcessRefreshPage.js"));

            bundles.Add(new ScriptBundle("~/bundles/ez-plus").Include(
                       "~/Scripts/jquery.ez-plus.js"));


            bundles.Add(new ScriptBundle("~/bundles/pivotTable").Include(
                       "~/Scripts/pivot.min.js"));


            bundles.Add(new StyleBundle("~/Content/pivotTable").Include(
                      "~/Content/pivot.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/dashboard")
                        .Include("~/Scripts/App/session.js")
                        .Include("~/Scripts/Dashboard/dashboard-templates.js")
                        .Include("~/Scripts/Dashboard/dashboard-functions.js")
                        .Include("~/Scripts/Dashboard/dashboard-app.js"));

            bundles.Add(new ScriptBundle("~/bundles/ocsdashboard")
                       .Include("~/Scripts/App/session.js")
                       .Include("~/Scripts/OCSDashboard/ocsdashboard-templates.js")
                       .Include("~/Scripts/OCSDashboard/ocsdashboard-functions.js")
                       .Include("~/Scripts/OCSDashboard/ocsdashboard-app.js"));



            bundles.Add(new ScriptBundle("~/bundles/branchdashboard")
                       .Include("~/Scripts/App/session.js")
                       .Include("~/Scripts/BranchDashboard/branchdashboard-templates.js")
                       .Include("~/Scripts/BranchDashboard/branchdashboard-functions.js")
                       .Include("~/Scripts/BranchDashboard/branchdashboard-app.js"));

            bundles.Add(new ScriptBundle("~/bundles/moneymask")
                    .Include("~/Scripts/jquery.moneymask.js"));
            bundles.Add(new ScriptBundle("~/bundles/calendar")
                     .Include("~/Scripts/zabuto_calendar.js")
                     .Include("~/Scripts/zabuto_calendar.min.js"));
            bundles.Add(new StyleBundle("~/Content/calendar")
                .Include("~/Content/zabuto_calendar.css")
                .Include("~/Content/font-awesome.css"));


            bundles.Add(new ScriptBundle("~/bundles/OCSDataProcessRealTime").Include(
                    "~/Scripts/App/OCSdataProcessRealTime.js"));

            bundles.Add(new ScriptBundle("~/bundles/BankBranchMaintDetails").Include(
                     "~/Scripts/App/BranchCodeMaintDetails.js"));

            bundles.Add(new ScriptBundle("~/bundles/InternalBranchSelfClearingId").Include(
                         "~/Scripts/OCS/GetInternalBranchSelfClearingId.js"));
            //new
            bundles.Add(new ScriptBundle("~/bundles/InternalBranchCode")
                .Include("~/Scripts/App/InternalBranchCodeIslamic.js")
                .Include("~/Scripts/App/InternalBranchCodeConv.js"));
            bundles.Add(new ScriptBundle("~/bundles/StartBatchICR")
                    .Include("~/Scripts/App/startBatchICR.js"));
            bundles.Add(new ScriptBundle("~/bundles/ChequePPSVerification").Include(
                       "~/Scripts/App/chequePPSVerification.js"));
            
        }
    }
}
