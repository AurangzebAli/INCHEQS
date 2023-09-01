using INCHEQS.Models.SearchPageConfig;
using System.Data;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;
using System;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using INCHEQS.Helpers;
using System.Threading.Tasks;
using INCHEQS.Security;
using System.Globalization;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Security.SystemProfile;
using System.Linq;

namespace INCHEQS.Models.Report
{
    public class ReportService : IReportService
    {



        private static string GetDeviceInfo(string orientation)
        {
            string deviceInfo = "";

            if (orientation.ToLower().Equals("portrait"))
            {
                deviceInfo = "<DeviceInfo>" +
                                  "  <OutputFormat>{0}</OutputFormat>" +
                                  "  <PageWidth>21cm</PageWidth>" +
                                  "  <PageHeight>29.7cm</PageHeight>" +
                                  "  <MarginTop>1cm</MarginTop>" +
                                  "  <MarginLeft>1cm</MarginLeft>" +
                                  "  <MarginRight>1cm</MarginRight>" +
                                  "  <MarginBottom>1cm</MarginBottom>" +
                              "</DeviceInfo>";
            }
            else
            {
                deviceInfo = "<DeviceInfo>" +
                                 "  <OutputFormat>{0}</OutputFormat>" +
                                 "  <PageWidth>29.7cm</PageWidth>" +
                                 "  <PageHeight>21cm</PageHeight>" +
                                 "  <MarginTop>1cm</MarginTop>" +
                                 "  <MarginLeft>1cm</MarginLeft>" +
                                 "  <MarginRight>1cm</MarginRight>" +
                                 "  <MarginBottom>1cm</MarginBottom>" +
                             "</DeviceInfo>";
            }
            return deviceInfo;
        }

        private readonly IPageConfigDao pageDao;
        private readonly ApplicationDbContext dbContext;
        private readonly ImageHelper imageHelper;
        private readonly ISystemProfileDao systemProfileDao;

        public ReportService(IPageConfigDao pageConfigDao, ApplicationDbContext dbContext, ImageHelper imageHelper, ISystemProfileDao systemProfileDao)
        {
            pageDao = pageConfigDao;
            this.dbContext = dbContext;
            this.imageHelper = imageHelper;
            this.systemProfileDao = systemProfileDao;
        }


        public async Task<ReportModel> GetReportConfigAsync(PageSqlConfig pageSqlConfig, string reportType) //Azim
        {
            return await Task.Run(() => GetReportConfig(pageSqlConfig, reportType)); //Azim
        }


        public async Task<ReportModel> GetReportConfigByTaskIdAsync(string taskId)
        {
            PageSqlConfig temp = new PageSqlConfig();
            temp.SetTaskId(taskId);
            return await Task.Run(() => GetReportConfig(temp, "")); //Azim
        }

        public async Task<DataTable> getReportBasedOnPageConfigAsync(PageSqlConfig pageSqlConfig, FormCollection collection)
        {
            return await Task.Run(() => getReportBasedOnPageConfig(pageSqlConfig, collection));
        }

        //Azim
        public ReportModel GetReportConfig(PageSqlConfig pageSqlConfig, string reportType)
        {
            ReportModel reportModel = new ReportModel();
            List<PageSqlConfig> tableAndViewNames = new List<PageSqlConfig>();
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sql = "";

            if (string.IsNullOrEmpty(pageSqlConfig.PrintReportParam))
            {

                string strPostingMode = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("PostingMode").Trim();

                if (strPostingMode == "API" && pageSqlConfig.TaskId == "305995")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyOCSCreditPostingReport_API"));
                }
                else if (strPostingMode == "FILE" && pageSqlConfig.TaskId == "305995")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyOCSCreditPostingReport"));
                }
                else if (strPostingMode == "API" && pageSqlConfig.TaskId == "305994")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyICSCreditPostingReport_API"));
                }
                else if (strPostingMode == "FILE" && pageSqlConfig.TaskId == "305994")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyICSCreditPostingReport"));
                }
                else if (strPostingMode == "API" && pageSqlConfig.TaskId == "305993")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyICSDebitPostingReport_API"));
                }
                else if (strPostingMode == "FILE" && pageSqlConfig.TaskId == "305993")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyICSDebitPostingReport"));
                }
                else if (strPostingMode == "API" && pageSqlConfig.TaskId == "305992")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyOCSDebitPostingReport_API"));
                }
                else if (strPostingMode == "FILE" && pageSqlConfig.TaskId == "305992")
                {
                    sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId and DatabaseViewId=@databaseviewid";
                    sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                    sqlParams.Add(new SqlParameter("@databaseviewid", "View_DailyOCSDebitPostingReport"));
                }
                else
                {
                sql = "SELECT * FROM [tblReportPageConfig] where TaskId = @taskId ";
                sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
            }
            }
            else
            {
                sql = "SELECT * FROM [tblReportPageConfig] WHERE taskId = @taskId AND PrintReportParam=@PrintReportParam ";
                sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                sqlParams.Add(new SqlParameter("@PrintReportParam", pageSqlConfig.PrintReportParam));
            }
            
            DataTable dt = dbContext.GetRecordsAsDataTable(sql, sqlParams.ToArray());


            foreach (DataRow row in dt.Rows)
            { 
                //Azim Start
                reportModel.reportId = row["ReportId"].ToString();
                if (reportType == "Excel")
                {
                    if (row["ReportPathExcel"].ToString() != "" && System.DBNull.Value != row["ReportPathExcel"])
                    {
                        reportModel.reportPath = row["ReportPathExcel"].ToString();
                    }
                    else {
                        reportModel.reportPath = row["ReportPath"].ToString();
                    }
                }
                else
                {
                    reportModel.reportPath = row["ReportPath"].ToString();
                }
                //Azim End
                reportModel.reportTitle = row["reportTitle"].ToString();
                reportModel.taskId = row["TaskId"].ToString();
                reportModel.viewId = row["DatabaseViewId"].ToString();
                reportModel.extentionFilename = row["exportFileName"].ToString();
                reportModel.dataSetName = row["DataSetName"].ToString();
                reportModel.orientation = row["Orientation"].ToString();
                tableAndViewNames.Add(new PageSqlConfig(row["TaskId"].ToString(), row["DatabaseViewId"].ToString()));
            }

            reportModel.sqlConfigForDataSet = tableAndViewNames;

            return reportModel;
        }

        public DataTable getReportBasedOnPageConfig(PageSqlConfig pageSqlConfig, FormCollection collection)
        {

            ConfigTable configTable = pageDao.GetConfigTable(pageSqlConfig.TaskId);
            SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(pageSqlConfig, configTable, collection);            
            return dbContext.GetRecordsAsDataTable(sql.sql, sql.sqlParams.ToArray());

        }

        //Jimuel - 20170721
        public string CHistory(ReportModel reportModel, ConfigTable configTable, FormCollection collection, string bankcode)
        {

            if (reportModel.taskId == "62010")
            {
                string fldcleardate = DateUtils.formatDateToSql(collection["fldcleardate"]);
                DataTable ds = new DataTable();
                string tableOrViewName = reportModel.viewId;
                string stmt = string.Format("SELECT top 1 fldClearDate FROM  {0}", configTable.ViewOrTableName);
                stmt = stmt + " WHERE fldcleardate=@fldClearDate and fldBankCode =  @fldBankCode";

                ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@tableOrViewName", configTable.ViewOrTableName),
                new SqlParameter("@fldClearDate",fldcleardate),
                new SqlParameter("@fldBankCode", bankcode)
            });

                if (ds.Rows.Count == 0)
                {
                    reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
                    return reportModel.viewId + "h";

                }
                else
                {
                    reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
                    return reportModel.viewId;
                }
            }
            else
            {
                string fldcleardate = DateUtils.formatDateToSql(collection["fldcleardate"]);
                DataTable ds = new DataTable();
                string tableOrViewName = reportModel.viewId;
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
                ds = dbContext.GetRecordsAsDataTableSP("spcgClearDate", sqlParameterNext.ToArray());

                // XX START 20210608
                //    string stmt = string.Format("SELECT top 1 fldClearDate FROM  {0} ", configTable.ViewOrTableName);
                //stmt = stmt + " WHERE fldcleardate=@fldClearDate ";

                //ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                //    new SqlParameter("@tableOrViewName", configTable.ViewOrTableName),
                //    new SqlParameter("@fldClearDate",fldcleardate)
                //});
                // XX END 20210608
                if (reportModel.taskId == "620100")
                {
                    if (collection["ViewUPI"].ToString().Trim() == "Y")
                    {
                        reportModel.extentionFilename = "[" + DateTime.Now.ToString("dd-MMM-yyyy") + "]" + reportModel.extentionFilename;
                        return reportModel.viewId;
                    }
                }
                //if(collection["ViewUPI"])

                if (ds.Rows.Count == 0)
                {

                    reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
                    return reportModel.viewId + "h";
                }
                else
                {
                    reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
                    return reportModel.viewId;
                }

            }
        }

        public string InwardHistory(ReportModel reportModel, ConfigTable configTable, FormCollection collection, string clearDate)
        {
            string Total = "";
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", clearDate));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgTotalCountClearingDate", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                Total = row["Total"].ToString();
            }
            if (Total == "0")
            {
                configTable.ViewOrTableName = reportModel.viewId + "H";
                return reportModel.viewId + "H";
            }
            else
            {
                configTable.ViewOrTableName = reportModel.viewId;
                return reportModel.viewId;
            }
        }
        public string checkClearDate(string pageSqlConfig, FormCollection collection)
        {
            try
            {
                string clearDate = DateUtils.formatDateToReportDate(collection["fldClearDate"]);
                DataTable dt = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldClearDate", clearDate));
                dt = dbContext.GetRecordsAsDataTableSP("spcgClearDate", sqlParameterNext.ToArray());
                if (pageSqlConfig == "304760")
                {
                    if (collection.AllKeys.Contains("fldByTime"))
                    {
                        if (dt.Rows.Count <= 0)
                        {
                            pageSqlConfig = "304762";
                        }
                        else
                        {
                            pageSqlConfig = "304761";
                        }
                    } 
                    else if (collection.AllKeys.Contains("radType") && collection["radType"] == "Monthly")
                    {
                        pageSqlConfig = "304765";
                    }
                    else if (collection.AllKeys.Contains("radType") && collection["radType"] == "Yearly")
                    {
                        pageSqlConfig = "304766";
                    }
                    // xx start 20210510
                    else if (collection.AllKeys.Contains("radType") && collection["radType"] == "Weekly")
                    {
                        pageSqlConfig = "304763";
                    }
                    // xx end 20210510
                    else
                    {
                        if (dt.Rows.Count <= 0)
                        {
                            pageSqlConfig = "304764";
                        }
                    }
                }
                return pageSqlConfig;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        

        public byte[] renderReportBasedOnConfig(ReportModel reportModel, FormCollection collection, string path,
            string reportType, out string mimeType)
        {

            string date = string.IsNullOrEmpty(collection["fldstartDate"]) ? "": DateUtils.formatDateToReportDate(collection["fldstartDate"]);
            string date2 = string.IsNullOrEmpty(collection["fldendDate"]) ? "" : DateUtils.formatDateToReportDate(collection["fldendDate"]);
            string bcode = collection["fldIssueBranchCode"];
            //string bcode = collection["txtBankBranch"];
            string bcode2 = collection["fldIssuingBankBranchId"];
            string bcode3 = collection["fldPresentingBankBranchId"];
            string accnum = collection["txtIssuingAccNum"];
            string accnum2 = collection["fldAccountNumber"];
            string cheqstatus = collection["txtChequeStatus"];
            string cheqstatus2 = collection["fldChequeStatus"];
            string returnReason = collection["txtReturnReason"];
            string returnReason2 = collection["fldReturnReason"];
            string rejectCode = collection["txtRejectCode"];
            string rejectCode2 = collection["fldRejectCode"];
            string ddd = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            string branchcode = collection["fldpresentingbranchid"];
            string branchcode1 = collection["fldissuingbranchid"];
            string fldBranchCode = collection["fldBranchCode"];
            string bankCode = collection["bankCode"];
            string transactioncode = collection["fldTransCode"];
            string dsstatus = collection["Status"];
            string PayBranchCode = collection["PayBranchCode"];
            string PayBranchFilter = collection["PayBranchFilter"];
            string collectingBank = collection["fldPreBankCode"];
            string clearDate = (string.IsNullOrEmpty(collection["fldClearDate"])  ? "" :DateUtils.formatDateToReportDate(collection["fldClearDate"].Replace(",", "")));
            string ncf = collection["fldNonConformance"];
            string fldprebankcode = collection["fldPreBankCode"];
            string total = "";
            string sDateCondition = "";
            string reportPeriod = "";
            string bankCodeForReport = collection["fldBankCode"];
            string reportTitleName = "";
            string filterBranch = collection["fldBranchDesc"];
            //string statusUPI;

            // xx start 20210601
            if (bankCodeForReport is null)
            {
                bankCodeForReport = collection["fldIssueBankCode"];
            }
            // xx end 20210601
            if (collection.AllKeys.Contains("radType"))
            {
                reportPeriod = collection["radType"].ToString();
            }
            string hostcode = "";
            string hostcode2 = "";
            string postingstatus = "";
            // XX START
            string dbClearDate = getClearDate();
            string hostStauts = collection["selectedHostStatus"];
            string fileType = collection["fldFileType"];
            string branchCode = collection["PayBranchCode"];
            string branchFilter = collection["PayBranchFilter"];
            string hostRejectView = collection["HostRejectView"];
            // xx start 20210522
            string transFilter = collection["TransFilter"];
            // xx end 20210522
            // XX END

            string branchFilterDesc = "";

            // xx start 20210602
            string status = collection["Status"];
            // xx end 20210602

            //if (branchFilter != "" && branchFilter != null)
            //{

            //}

            //if (reportModel.taskId == "304513")
            //{
            //    hostcode = collection["txtHostStatus"];
            //}

            if (reportModel.taskId == "304512" || reportModel.taskId == "304513")
            {
                hostcode2 = collection["fldBankHostStatusCode"];
            }

            if (reportModel.taskId == "305989")
            {
                date = collection["fldClearDate"];
            }

            /*if (reportModel.taskId == "305010")
            {
                statusUPI = collection["StatusUpi"];
            }*/

            //if (reportModel.taskId == "305993" || reportModel.taskId == "305994" || reportModel.taskId == "305995")
            //{
            //    hostcode = collection["txtHostStatus"];
            //}

            //DateTime temp = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            //string str = temp.ToString("yyyy-MM-dd");
            //xx 20210315
            /* Comment By Ali */
            //if ((bankCodeForReport is null || bankCodeForReport == "All") || bankCodeForReport == "") reportTitleName = "AFFIN BANK BERHAD &\n AFFIN ISLAMIC BANK BERHAD";
            if ((bankCodeForReport is null || bankCodeForReport == "All") || bankCodeForReport == "") reportTitleName = "The Bank Of Punjab";
            //if ((bankCodeForReport is null || bankCodeForReport == "All") || bankCodeForReport == "") reportTitleName = getReportName();
            else if (bankCodeForReport != null && bankCodeForReport == "32") reportTitleName = "AFFIN BANK BERHAD";
            else if (bankCodeForReport != null && bankCodeForReport == "47") reportTitleName = "AFFIN ISLAMIC BANK BERHAD";
            else if (bankCodeForReport != null && bankCodeForReport == "083") reportTitleName = "The Bank Of Punjab";
            reportTitleName = "The Bank Of Punjab";
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();
            collection.Add("ReportLogo", "Logo");//new Uri(CurrentUser.Account.LogoPath).AbsoluteUri
            collection.Add("BankName", CurrentUser.Account.BankDesc);
            collection.Add("UserAbb", CurrentUser.Account.UserAbbr);
            collection.Add("BankTitleName", reportTitleName);

            if (reportModel.taskId == "305010" || reportModel.taskId == "305020" || reportModel.taskId == "304351" ||  reportModel.taskId == "304350" || reportModel.taskId == "304420")
            {
                if (collection["StatusUPI"] is null || collection["StatusUPI"].ToString().Trim() == "")
                {
                    //collection.Remove("UpiStatus");
                    collection.Add("UpiStatus", "UPI Generated, Not Yet Generated");
                }
                else
                {
                    collection.Add("UpiStatus", collection["StatusUpi"].ToString().Trim());
                }
                //statusUPI = collection["StatusUpi"].ToString().Trim();
                //collection.Add("StatusUpi", collection["StatusUpi"].ToString().Trim());
            }

            if (reportModel.taskId == "304320")
            {
                if (collection["StatusUPI"] is null || collection["StatusUPI"].ToString().Trim() == "")
                {
                    //collection.Remove("UpiStatus");
                    collection.Add("UpiStatus", "UPI Generated, Not Yet Generated");
                }
                else
                {
                    if (collection["StatusUPI"].ToString().Trim() != "0")
                    {
                        collection.Add("UpiStatus", "UPI Generated");
                    }
                    else
                    {
                        collection.Add("UpiStatus", "Not Yet Generated");
                    }

                }
            }

            if (reportModel.taskId == "304440" || reportModel.taskId == "304450" || reportModel.taskId == "304470" || reportModel.taskId == "304480")
            {
                if (collection["IBranch"] is null || collection["IBranch"].ToString().Trim() != "")
                {
                    collection.Add("IBranch", "000");
                }
            }
            // xx end
            // xx start 20210624
            string branchDesc = "";

            // xx start 20210710
            string CBranchId = "";
            string IBranchId = "";
            // xx end 20210710

            if (CurrentUser.Account.BranchCode != "" && CurrentUser.Account.BranchCode != "000" && CurrentUser.Account.BranchCode != null)
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                DataTable dt = new DataTable();

                sqlParameterNext.Add(new SqlParameter("@CBranchCode", CurrentUser.Account.BranchCode));
                sqlParameterNext.Add(new SqlParameter("@IBranchCode", CurrentUser.Account.BranchCode3));

                dt = dbContext.GetRecordsAsDataTableSP("spcgGetBranchDesc", sqlParameterNext.ToArray());

                if (dt.Rows.Count > 0)
                {
                    branchDesc = dt.Rows[0]["BranchDesc"].ToString();
                }
            }
            // xx end 20210624

            // xx start 20210710
            if (PayBranchFilter != "" && PayBranchFilter != null)
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                DataTable dt = new DataTable();

                sqlParameterNext.Add(new SqlParameter("@branchId", PayBranchFilter));

                dt = dbContext.GetRecordsAsDataTableSP("spcgGetInternalBranchDesc", sqlParameterNext.ToArray());

                if (dt.Rows.Count > 0)
                {
                    branchDesc = dt.Rows[0]["BranchDesc"].ToString();
                    CBranchId = dt.Rows[0]["CBranchId"].ToString();
                    IBranchId = dt.Rows[0]["IBranchId"].ToString();
                }
            }
            else
            {
                string ConvBranch = CurrentUser.Account.BranchCode;
                string IslamiCBranch = CurrentUser.Account.BranchCode3;

                string FilterBranch = "";

                if (ConvBranch != "" && ConvBranch != "000" && ConvBranch != null)
                {
                    //FilterBranch = string.Concat("18", ConvBranch.Trim());
                    FilterBranch = ConvBranch.Trim();
                }
                else
                {
                    if (IslamiCBranch != "" && IslamiCBranch != "000" && IslamiCBranch != null)
                    {
                        FilterBranch = string.Concat("43", IslamiCBranch.Trim());
                    }
                    else
                    {
                        FilterBranch = "";
                    }

                }

                if (FilterBranch == "")
                {
                    branchDesc = "ALL";
                    CBranchId = "ALL";
                    IBranchId = "ALL";
                }
                else
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    DataTable dt = new DataTable();

                    sqlParameterNext.Add(new SqlParameter("@branchId", FilterBranch.Trim()));

                    dt = dbContext.GetRecordsAsDataTableSP("spcgGetInternalBranchDesc", sqlParameterNext.ToArray());

                    if (dt.Rows.Count > 0)
                    {
                        branchDesc = dt.Rows[0]["BranchDesc"].ToString();
                        CBranchId = dt.Rows[0]["CBranchId"].ToString();
                        IBranchId = dt.Rows[0]["IBranchId"].ToString();
                    }
                }


            }
            // xx end 20210710

            // xx start 20210624
            collection.Add("CBranch", CurrentUser.Account.BranchCode);
            collection.Add("IBranch", CurrentUser.Account.BranchCode3);
            collection.Add("BranchDesc", branchDesc);
            // xx end 20210624

            // xx start 20210710
            collection.Add("CBranchId", CBranchId);
            collection.Add("IBranchId", IBranchId);
            // xx end 20210710
            if (!string.IsNullOrEmpty(date))
            {
                DateTime dt1 = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            }
            if (!string.IsNullOrEmpty(date2))
            {
                DateTime dt2 = DateTime.ParseExact(date2, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            }
            collection.Add("StartDate", date);
            collection.Add("EndDate", date2);
            collection.Add("CurrentDateTime", ddd);
            //collection["fldClearDate"] = DateUtils.formatDateToReportDate(collection["fldClearDate"]);
            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);

            if (reportType == "Excel")
            {
                reportType = "Excelopenxml";
            }
            else if (reportType == "CSV")
            {
                reportType = "Excelopenxml";
            }

            if (configTable.ViewOrTableName.Equals(""))
            {
                configTable.ViewOrTableName = reportModel.viewId;
            }

            if (reportModel.taskId == "999910")
            {
                //do nothing
            }
            else if (reportModel.taskId == "304220" || reportModel.taskId == "304770" || reportModel.taskId == "304890" || reportModel.taskId == "304511" || reportModel.taskId == "304614" || reportModel.taskId == "304760" || reportModel.taskId == "304761" || reportModel.taskId == "304762" || reportModel.taskId == "304764")
            {
                reportModel.viewId = InwardHistory(reportModel, configTable, collection, clearDate);
                configTable.ViewOrTableName = reportModel.viewId;
            }

            else if (reportModel.taskId != "107030" && reportModel.taskId != "107020" && reportModel.taskId != "106010" && reportModel.taskId != "102150" && reportModel.taskId != "102110" && reportModel.taskId != "102130" && reportModel.taskId != "102340" && reportModel.taskId != "102380" && reportModel.taskId != "102160" && reportModel.taskId != "102220" && reportModel.taskId != "102230" && reportModel.taskId != "102520" && reportModel.taskId != "102680" && reportModel.taskId != "102570" && reportModel.taskId != "102690" && reportModel.taskId != "102670")
            {
                //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
            }
            //Heavy load of Datasets load
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                if (reportModel.taskId != "107060" && reportModel.taskId != "102150" && reportModel.taskId != "102110" && reportModel.taskId != "102130" && reportModel.taskId != "102340" && reportModel.taskId != "102380" && reportModel.taskId != "102570" && reportModel.taskId != "102670")
                {
                    if ((config.TaskId == "308110") || (config.TaskId == "308170") || (config.TaskId == "308180") || (config.TaskId == "308190") || (config.TaskId == "308200")  /*|| (config.TaskId == "308130")*/)
                    {
                        config.AddSqlExtraCondition("fldIssueBankBranch in (" + CurrentUser.Account.BranchCodes[0].Trim() + "," + CurrentUser.Account.BranchCodes[1].Trim() + ")");
                    }

                    else if (config.TaskId == "308160")
                    {
                        config.AddSqlExtraCondition("fldBankCode=" + CurrentUser.Account.BankCode + " and fldIssueBranchCode=" + CurrentUser.Account.BranchCodes[0]);
                    }

                    //else if (/*(config.TaskId == "308120") ||*/ (config.TaskId == "308140"))
                    //{
                    //    config.AddSqlExtraCondition("fldBankCode=" + CurrentUser.Account.BankCode /**+ " and fldBranchCode=" + CurrentUser.Account.BranchCodes[0]**/);
                    //}

                    //else if ((config.TaskId == "306220") || (config.TaskId == "306910"))
                    //{
                    //    config.AddSqlExtraCondition("  fldBRSTN in ( Select fldBranchId from tblDedicatedBranch where flduserid = '" + CurrentUser.Account.UserAbbr + "')");
                    //}
                    //else if ((config.TaskId == "306230") || (config.TaskId == "306920"))
                    //{
                    //    config.AddSqlExtraCondition("  fldBRSTN in ( Select fldBranchId from tblDedicatedBranch where fldofficerid = '" + CurrentUser.Account.UserAbbr + "')");
                    //}
                    else if ((config.TaskId == "306240") || (config.TaskId == "306930"))
                    {
                        config.AddSqlExtraCondition("  fldBRSTN not in ( Select fldBranchId from tblDedicatedBranch where fldofficerid = '" + CurrentUser.Account.UserAbbr + "')");
                    }
                    else if (config.TaskId == "306510" || config.TaskId == "308210")
                    {
                        config.AddSqlExtraCondition("fldInvolvedUserName = '" + CurrentUser.Account.UserAbbr + "'");
                    }
                    else if (config.TaskId == "304640" || config.TaskId == "304650" || config.TaskId == "304660" || config.TaskId == "304670" || config.TaskId == "304680")
                    {
                        config.AddSqlExtraCondition("fldBankCode=" + CurrentUser.Account.BankCode + " and fldBRSTN=" + CurrentUser.Account.BranchCodes[0]);
                    }
                    else if (config.TaskId == "305999" || config.TaskId == "305998" || config.TaskId == "305997" || config.TaskId == "305996" || config.TaskId == "305995" || config.TaskId == "305991" || config.TaskId == "305990" || config.TaskId == "305992")
                    {
                        //config.AddSqlExtraCondition("flduserabb='" + CurrentUser.Account.UserAbbr + "'");
                        //do nothing
                        //config.AddSqlExtraCondition("fldbankcode='" + CurrentUser.Account.BankCode + "'");;
                        if (branchcode.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldCapturingDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldpresentingbranchid in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");

                        }
                        else
                        {
                            config.AddSqlExtraCondition("fldCapturingDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)");
                        }
                    }

                    else if (config.TaskId == "305983" || config.TaskId == "305984" || config.TaskId == "305987" || config.TaskId == "305988")
                    {

                        config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)");

                    }
                    else if (config.TaskId == "305985" || config.TaskId == "305986")
                    {

                        config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) or isnull(fldClearDate,'')=''");

                    }

                    else if (config.TaskId == "305994" || config.TaskId == "305993")
                    {
                        //config.AddSqlExtraCondition("flduserabb='" + CurrentUser.Account.UserAbbr + "'");
                        //do nothing
                        //config.AddSqlExtraCondition("fldbankcode='" + CurrentUser.Account.BankCode + "'");;
                        if (branchcode1.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldchequeclearingdate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldissuingbranchid in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else
                        {
                            config.AddSqlExtraCondition("fldchequeclearingdate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)");

                        }
                    }

                    else if (config.TaskId == "304601")
                    {
                        if (bcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "'");
                        }

                    }
                    else if (config.TaskId == "304791")
                    {
                        if (bcode2.Equals("") && string.IsNullOrEmpty(cheqstatus2))
                        {
                            //config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (bcode2.Equals("") && !cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && !cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldChequeStatus = '" + cheqstatus2 + "'");
                        }
                        else if (!bcode2.Equals("") && cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' ");
                        }
                    }
                    else if (config.TaskId == "304792")
                    {
                        if (bcode3.Equals("") && cheqstatus2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (bcode3.Equals("") && !cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode3.Equals("") && !cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldChequeStatus = '" + cheqstatus2 + "'");
                        }
                        else if (!bcode3.Equals("") && cheqstatus2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' ");
                        }
                    }
                    //else if (config.TaskId == "305989")
                    //{
                    //    if (bcode.Equals("All"))
                    //    {
                    //        config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)");
                    //    }
                    //    else
                    //    {

                    //        config.AddSqlExtraCondition("fldclearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode + "'");
                    //    }
                    //}
                    else if (config.TaskId == "304512")
                    {
                        if (bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%'");
                        }
                        else if (bcode2.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)  and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "'  and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%'");
                        }
                        else if (!bcode2.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%'");
                        }
                        else if (!bcode2.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "'");
                        }
                        else if (!bcode2.Equals("ll") && accnum2.Equals("") && cheqstatus2.Equals("ll") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode + "'");
                        }

                        else if (bcode2.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)  and fldIssuingBankBranchId = '" + bcode2 + "' and fldChequeStatus = '" + cheqstatus2 + "'");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%'");
                        }


                        else if (!bcode2.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "'");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldChequeStatus = '" + cheqstatus2 + "'");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%'");
                        }

                        else if (bcode2.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (bcode2.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("All") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "'");
                        }
                        else if (bcode2.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (bcode2.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "' )");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' ");
                        }


                    }

                    else if (config.TaskId == "304513")
                    {
                        if (bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (bcode3.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)  and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "'  and fldChequeStatus = '" + cheqstatus2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldAccountNumber = '" + accnum2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }

                        else if (bcode3.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("All") && !hostcode2.Equals("All"))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }


                        else if (!bcode3.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldAccountNumber = '" + accnum2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }

                        else if (bcode3.Equals("") && !accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (bcode3.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (!bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldPresentingBankBranchId = '" + bcode3 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (bcode3.Equals("") && !accnum2.Equals("") && cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (bcode3.Equals("") && accnum2.Equals("") && !cheqstatus2.Equals("") && hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldChequeStatus = '" + cheqstatus2 + "' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }
                        else if (bcode3.Equals("") && accnum2.Equals("") && cheqstatus2.Equals("") && !hostcode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldbankHostStatusCode like '%" + hostcode2 + "%' and fldIssuingBankBranchId in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId =  '" + CurrentUser.Account.UserId + "')");
                        }


                    }


                    else if (config.TaskId == "304602")
                    {
                        if (bcode2.Equals("") && accnum2.Equals("") && rejectCode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113)");
                        }
                        else if (!bcode2.Equals("") && !accnum2.Equals("") && !rejectCode2.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "' and fldRejectCode = '" + rejectCode2 + "'");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "'");
                        }
                        else if (bcode2.Equals("") && !accnum2.Equals("") && rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldAccountNumber = '" + accnum2 + "'");
                        }
                        else if (bcode2.Equals("") && accnum2.Equals("") && !rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldRejectCode = '" + rejectCode2 + "'");
                        }
                        //
                        else if (!bcode2.Equals("") && !accnum2.Equals("") && rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldAccountNumber = '" + accnum2 + "'");
                        }
                        else if (!bcode2.Equals("") && accnum2.Equals("") && !rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldIssuingBankBranchId = '" + bcode2 + "' and fldRejectCode = '" + rejectCode2 + "'");
                        }
                        else if (bcode2.Equals("") && !accnum2.Equals("") && !rejectCode2.Equals(""))
                        {

                            config.AddSqlExtraCondition("fldClearDate between CONVERT(nvarchar(30),'" + date + "',113) and CONVERT(nvarchar(30), '" + date2 + "',113) and fldRejectCode = '" + rejectCode2 + "' and fldAccountNumber = '" + accnum2 + "'");
                        }
                    }
                    else if (config.TaskId == "102690" /*|| config.TaskId == "308140" || config.TaskId == "308110"*/ || config.TaskId == "106010" || config.TaskId == "106010" || config.TaskId == "305985" || config.TaskId == "305986" || config.TaskId == "305984" || config.TaskId == "305983" || config.TaskId == "102540" || config.TaskId == "102140" || config.TaskId == "102420" || config.TaskId == "102150" || config.TaskId == "102170" || config.TaskId == "102110" || config.TaskId == "102130" || config.TaskId == "306220" || config.TaskId == "306230")
                    {
                        //for certain report that does not require for any filter condition
                    }
                    else if (config.TaskId == "304140" || config.TaskId == "304130" || config.TaskId == "304170" || config.TaskId == "304180" || config.TaskId == "304190" || config.TaskId == "304200")
                    {
                    }
                    // xx start 20210522
                    else if (config.TaskId == "304220")
                    {
                        if (transFilter == "")
                        {
                            config.AddSqlExtraCondition(" fldTransCode in (02,03,08,15)");
                        }
                    }
                    // xx end 20210522
                    else if (config.TaskId == "304770")
                    {
                        if (PayBranchFilter.Equals("") && dsstatus.Equals("S") && collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldMICRDS = 0 or fldImageDS = 0 Order by fldBankDesc");
                        }
                        else if (!PayBranchFilter.Equals("") && dsstatus.Equals("S") && !collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("PayBranchFilter = '" + PayBranchFilter + "' And fldPreBankCode = '" + collectingBank + "'  and fldMICRDS = 0 or fldImageDS = 0 Order by fldBankDesc");
                        }
                        else if (!PayBranchFilter.Equals("") && dsstatus.Equals("S") && collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("PayBranchFilter = '" + PayBranchFilter + "'  and fldMICRDS = 0 or fldImageDS = 0 Order by fldBankDesc");
                        }
                        else if (PayBranchFilter.Equals("") && dsstatus.Equals("S") && !collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldPreBankCode = '" + collectingBank + "' and fldMICRDS = 0 or fldImageDS = 0 Order by fldBankDesc");
                        }
                        else if (PayBranchFilter.Equals("") && dsstatus.Equals("F") && collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldMICRDS != 0 and fldImageDS != 0 Order by fldBankDesc");
                        }
                        else if (!PayBranchFilter.Equals("") && dsstatus.Equals("F") && !collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("PayBranchFilter = '" + PayBranchFilter + "' And fldPreBankCode = '" + collectingBank + "'  and fldMICRDS != 0 and fldImageDS != 0 Order by fldBankDesc");
                        }
                        else if (!PayBranchFilter.Equals("") && dsstatus.Equals("F") && collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("PayBranchFilter = '" + PayBranchFilter + "'  and fldMICRDS != 0 and fldImageDS != 0 Order by fldBankDesc");
                        }
                        else if (PayBranchFilter.Equals("") && dsstatus.Equals("F") && !collectingBank.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldPreBankCode = '" + collectingBank + "' and fldMICRDS != 0 and fldImageDS != 0 Order by fldBankDesc");
                        }
                    }
                    //else if (config.TaskId == "304890")
                    //{
                    //    if (fldBranchCode.Equals("") && ncf.Equals(""))
                    //    {
                    //        config.AddSqlExtraCondition("fldPreBankCode = '" + CurrentUser.Account.BankCode + "'");
                    //    }
                    //    if (!fldBranchCode.Equals("") && !ncf.Equals(""))
                    //    {
                    //        config.AddSqlExtraCondition("fldPreBankCode = '" + CurrentUser.Account.BankCode + "' and fldIssueStateCode + fldIssueBranchCode = '" + fldBranchCode + "' and fldNonConformance = '" + ncf + "'");
                    //    }
                    //    if (!fldBranchCode.Equals("") && ncf.Equals(""))
                    //    {
                    //        config.AddSqlExtraCondition("fldPreBankCode = '" + CurrentUser.Account.BankCode + "' and fldIssueStateCode + fldIssueBranchCode = '" + fldBranchCode + "'");
                    //    }
                    //    if (fldBranchCode.Equals("") && !ncf.Equals(""))
                    //    {
                    //        config.AddSqlExtraCondition("fldPreBankCode = '" + CurrentUser.Account.BankCode + "' and fldNonConformance = '" + ncf + "'");
                    //    }
                    //}
                    else if (config.TaskId == "304511")
                    {
                        //config.AddSqlExtraCondition("fldIssueBankCode = '" + CurrentUser.Account.BankCode + "'");
                    }
                    else if (config.TaskId == "304614")
                    {
                        if (!fldprebankcode.Equals(""))
                        {
                            config.AddSqlExtraCondition("fldPreBankCode = '" + fldprebankcode + "'");
                        }
                    }
                    // xx start 20210326
                    else if (config.TaskId == "305010" || config.TaskId == "305020" || config.TaskId == "304350"
                        || config.TaskId == "304351")
                    {
                        if (bankCodeForReport == "")
                        {

                        }
                        else
                        {
                            config.AddSqlExtraCondition("fldBankCode='" + bankCodeForReport + "'");
                        }

                    }
                    else if (config.TaskId == "304902")
                    {
                        if (fileType == "")
                        {

                        }
                        else
                        {
                            config.AddSqlExtraCondition("fldFileType ='" + fileType + "'");
                        }
                    }
                    // xx end
                    else if (config.TaskId == "304760" || config.TaskId == "304761" || config.TaskId == "304762" || config.TaskId == "304763" || config.TaskId == "304764" || config.TaskId == "304765" || config.TaskId == "304766")
                    {
                        if (reportPeriod == "Weekly")
                        {
                            DateTime now = DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                            DateTime StartDate = new DateTime();
                            DateTime EndDate = new DateTime();
                            if (now.DayOfWeek == DayOfWeek.Sunday)
                            {
                                StartDate = now.AddDays(1); EndDate = now.AddDays(5);
                            }
                            else if (now.DayOfWeek == DayOfWeek.Monday)
                            {
                                StartDate = now; EndDate = now.AddDays(4);
                            }
                            else if (now.DayOfWeek == DayOfWeek.Tuesday)
                            {
                                StartDate = now.AddDays(-1); EndDate = now.AddDays(3);
                            }
                            else if (now.DayOfWeek == DayOfWeek.Wednesday)
                            {
                                StartDate = now.AddDays(-2); EndDate = now.AddDays(2);
                            }
                            else if (now.DayOfWeek == DayOfWeek.Thursday)
                            {
                                StartDate = now.AddDays(-3); EndDate = now.AddDays(1);
                            }
                            else if (now.DayOfWeek == DayOfWeek.Friday)
                            {
                                StartDate = now.AddDays(-4); EndDate = now;
                            }
                            else if (now.DayOfWeek == DayOfWeek.Saturday)
                            {
                                StartDate = now.AddDays(-5); EndDate = now.AddDays(-1);
                            }
                            sDateCondition = "datediff(d,fldCleardate,'" + StartDate.ToString("yyyy-MM-dd") + "') <= 0  and datediff(d,fldCleardate,'" + EndDate.ToString("yyyy-MM-dd") + "') >= 0 ";
                            config.AddSqlExtraCondition("datediff(d, fldCleardate, '" + StartDate.ToString("yyyy-MM-dd") + "') <= 0  and datediff(d, fldCleardate, '" + EndDate.ToString("yyyy-MM-dd") + "') >= 0 ");
                            config.AddSqlOrderBy("fldUserDesc");
                            string date_str = StartDate.ToString("dddd, dd MMMM yyyy");
                            string date_end = EndDate.ToString("dddd, dd MMMM yyyy");
                            collection.Add("StartDate", date_str);
                            collection.Add("EndDate", date_end);
                            // xx start 20210510
                            //collection.Add("fldClearDate", clearDate);
                            // xx end 20210510
                            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));
                        }
                        else if (reportPeriod == "Monthly")
                        {
                            DateTime month = DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                            sDateCondition = "datediff(m,fldCleardate,'" + clearDate + "')=0";
                            config.AddSqlExtraCondition("datediff(m,fldCleardate,'" + clearDate + "')=0");
                            config.AddSqlOrderBy("fldUserDesc");
                            string date_month = month.ToString("MMMM yyyy");
                            collection.Add("MonthDate", date_month);
                            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));
                        }
                        else if (reportPeriod == "Yearly")
                        {
                            DateTime year = DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture);
                            sDateCondition = "datediff(year,fldCleardate,'" + clearDate + "') = 0";
                            //config.sql

                            config.AddSqlExtraCondition("datediff(year,fldCleardate,'" + clearDate + "') = 0");
                            config.AddSqlOrderBy("fldUserDesc");
                            string date_year = year.ToString("yyyy");
                            collection.Add("YearlyDate", date_year);
                            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));
                        }
                        else
                        {
                            sDateCondition = "datediff(d,fldCleardate,'" + clearDate + "')=0";
                            config.AddSqlOrderBy("fldUserDesc");
                            if (configTable.ViewOrTableName == "View_UserPerformanceReportDailyH" || configTable.ViewOrTableName == "View_UserPerformanceReportbyTime" || configTable.ViewOrTableName == "View_UserPerformanceReportbyTimeH")
                            {
                                config.AddSqlExtraCondition("datediff(d, fldCleardate, '" + clearDate + "') = 0");
                            }
                        }
                    }
                    else
                    {
                        //config.AddSqlExtraCondition("fldBankCode='" + CurrentUser.Account.BankCode + "'");
                        //if (bankCodeForReport == "" || bankCodeForReport is null)
                        //{

                        //}
                        //else
                        //{
                        //    config.AddSqlExtraCondition("fldBankCode='" + bankCodeForReport + "'");
                        //}
                        //config.AddSqlExtraCondition("fldBankCode='" + CurrentUser.Account.BankCode + "'");

                    }
                }

                // xx start
                //config.TaskId == "304250"
                if (config.TaskId == "304550" || config.TaskId == "304551" || config.TaskId == "305010" || config.TaskId == "305020" || config.TaskId == "304140" || config.TaskId == "304130" || config.TaskId == "304170" || config.TaskId == "304180" || config.TaskId == "304190" || config.TaskId == "304200" || config.TaskId == "304270")
                {
                    if (dbClearDate != clearDate)
                    {
                        configTable.ViewOrTableName = configTable.ViewOrTableName + "H";
                    }
                }

                // xx end
                //collection["fldClearDate"] = DateUtils.formatDateToSql(collection["fldClearDate"]);
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(config, configTable, collection);

                //20200227: BEGIN: LENDER: PRINT ALL SELECT QUERY.
                DataTable dt;
                string fldcleardate = "";
                //string HolidayDate = "";
                string mainCondition = !string.IsNullOrEmpty(sql.conditionAsSqlString) ? " AND " + sql.conditionAsSqlString : "";
                string lockCondition = !string.IsNullOrEmpty(config.SqlLockCondition) ? " AND (" + config.SqlLockCondition + ")" : "";
                string filtersSql = mainCondition + lockCondition;
                if (config.TaskId == "107020" || config.TaskId == "308110" || config.TaskId == "308140" || config.TaskId == "107030" || config.TaskId == "102220" || config.TaskId == "102230" || config.TaskId == "306220" || config.TaskId == "306230" || config.TaskId == "106010" || config.TaskId == "308150" || config.TaskId == "308170" || config.TaskId == "308180" || config.TaskId == "308190" || config.TaskId == "308200" || config.TaskId == "309100" || config.TaskId == "301120")
                {
                    if (sql.sqlParams.Count != 0)
                    {
                        foreach (var sqlParam in sql.sqlParams)
                        {
                            string a = sqlParam.Value.ToString();
                            string b = sqlParam.ParameterName.ToString();
                            //filtersSql=filtersSql.Replace(b,a);
                            if (b == "@fldClearDate")
                            {
                                fldcleardate = a;
                                a = "'" + a + "'";
                            }
                            if (b == "@fldBankCode")
                            {
                                a = "'" + a + "'";
                            }
                            //if (b == "@HolidayDate")
                            //{
                            //    HolidayDate = a;
                            //    a = "'" + a + "'";
                            //}
                            filtersSql = filtersSql.Replace(b, a);
                        }

                        filtersSql = filtersSql.Replace("%' + ", "%");
                        filtersSql = filtersSql.Replace(" + '%", "%");

                    }

                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                    sqlParameterNext.Add(new SqlParameter("@TaskId", config.TaskId));
                    sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
                    //sqlParameterNext.Add(new SqlParameter("@HolidayDate", HolidayDate));
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", CurrentUser.Account.UserId));
                    sqlParameterNext.Add(new SqlParameter("@condition", filtersSql));

                    dt = dbContext.GetRecordsAsDataTableSP("spcgReportListPage", sqlParameterNext.ToArray());
                    localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
                }// xx start 20210325
                else if (config.TaskId == "314970" || config.TaskId == "304201" || config.TaskId == "314560")
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));

                    if (config.TaskId == "314560")
                    {
                        sqlParameterNext.Add(new SqlParameter("@bankCode", bankCodeForReport));
                        sqlParameterNext.Add(new SqlParameter("@filterBranch", filterBranch));
                    }
                    dt = dbContext.GetRecordsAsDataTableSP(reportModel.dataSetName, sqlParameterNext.ToArray());
                }

                // xx 20210331


                else if (config.TaskId == "304320")
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    sqlParameterNext.Add(new SqlParameter("@bankCode", bankCodeForReport));
                    sqlParameterNext.Add(new SqlParameter("@upiStatus", collection["StatusUpi"]));
                    sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));
                    dt = dbContext.GetRecordsAsDataTableSP(reportModel.dataSetName, sqlParameterNext.ToArray());
                }

                // BRANCH REPORT config.TaskId == "304440" //config.TaskId == "304450" 
                else if (  config.TaskId == "304470" ||  config.TaskId == "304460" || config.TaskId == "304480"
                    || config.TaskId == "304451" || config.TaskId == "304490")
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));
                    sqlParameterNext.Add(new SqlParameter("@userId", CurrentUser.Account.UserId));
                    sqlParameterNext.Add(new SqlParameter("@bankCode", bankCodeForReport.Trim()));
                    if (config.TaskId == "304460")
                    {
                        sqlParameterNext.Add(new SqlParameter("@transCode", collection["fldtransCode"]));
                    }
                    dt = dbContext.GetRecordsAsDataTableSP(reportModel.dataSetName, sqlParameterNext.ToArray());
                }
                // xx end

                //20200227: BEGIN: LENDER: PRINT ALL SELECT QUERY.

                //if (config.TaskId == "107020")
                //{
                //    query = "SELECT hm.fldHubCode, hm.fldHubDesc, um.fldUserAbb FROM tblUserMaster AS um FULL OUTER JOIN tblHubUser AS hu ON um.fldUserId = hu.fldUserId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubCode = hu.fldHubCode WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " ORDER BY hm.fldHubCode ";
                //}
                //else if (config.TaskId == "107030")
                //{
                //    query = "SELECT hm.fldHubCode, hm.fldHubDesc, bm.fldBranchDesc  FROM tblInternalBranchMaster AS bm FULL OUTER JOIN tblHubBranch AS hb ON bm.fldBranchCode = hb.fldBranchId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubCode = hb.fldHubCode WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " ORDER BY hm.fldHubCode ";
                //}
                //else
                //{
                //     query = "SELECT " + sql.queriesAsSqlString + " FROM " + sql.tableName + " WHERE fldBankCode=" + CurrentUser.Account.BankCode;
                //}
                else
                {
                    //if (config.TaskId == "304550" || config.TaskId == "304551")
                    //{
                    //    if (hostStauts != "")
                    //    {
                    //        sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[fldRejectStatus1] = @fldRejectStatus1", "(fldRejectStatus1 = @fldRejectStatus1 or fldRejectStatus2 = @fldRejectStatus1 or fldRejectStatus3 = @fldRejectStatus1 or fldRejectStatus4 = @fldRejectStatus1) ");
                    //    }
                    //}

                    // xx start 20210517
                    if (config.TaskId == "304250")
                    {
                        if (branchFilter != "")
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[PayBranchFilter] = @PayBranchFilter", "PayBranchFilter in (" + branchFilter + ")");
                        }

                    }

                    //xx start 20210617
                    if (config.TaskId == "304290")
                    {
                        if (hostRejectView == "1")
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus1 = '1' or fldRejectStatus2 = '1') ");
                        }
                        else if (hostRejectView == "2")
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus3 = '1' or fldRejectStatus4 = '1') ");
                        }
                        else
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString + " AND (fldRejectStatus1 = '1' or fldRejectStatus2 = '1' or fldRejectStatus3 = '1' or fldRejectStatus4 = '1')";
                        }
                    }






                    // xx end 20210617
                    if (config.TaskId == "304550" || config.TaskId == "304551")
                    {
                        if (hostRejectView == "1")
                        {
                            if (hostStauts != null)
                            {
                                sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus1 in ( " + hostStauts + ") or fldRejectStatus2 in ( " + hostStauts + ")) ");
                            }
                            else
                            {
                                sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus1 is not null or fldRejectStatus2 is not null) ");
                            }
                        }
                        else if (hostRejectView == "2")
                        {
                            if (hostStauts != null)
                            {
                                sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus3 in ( " + hostStauts + ") or fldRejectStatus4 in ( " + hostStauts + ")) ");
                            }
                            else
                            {
                                sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus3 is not null or fldRejectStatus4 is not null) ");
                            }
                        }
                        else
                        {
                            if (hostStauts != null)
                            {
                                //sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus1 in ( " + hostStauts + ") or fldRejectStatus2 in ( " + hostStauts + ") or fldRejectStatus3 in ( " + hostStauts + ") or fldRejectStatus4 in ( " + hostStauts + ")) ");
                                //config.AddSqlExtraCondition("( fldRejectStatus1 in ( " + hostStauts + ") or fldRejectStatus2 in ( " + hostStauts + ") or fldRejectStatus3 in ( " + hostStauts + ") or fldRejectStatus4 in ( " + hostStauts + "))");
                                sql.conditionAsSqlString = sql.conditionAsSqlString + " AND ( fldRejectStatus1 in ( " + hostStauts + ") or fldRejectStatus2 in ( " + hostStauts + ") or fldRejectStatus3 in ( " + hostStauts + ") or fldRejectStatus4 in ( " + hostStauts + "))";
                            }
                            else
                            {
                                sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[HostRejectView] = @HostRejectView", "(fldRejectStatus1 is not null or fldRejectStatus2 is not null or fldRejectStatus3 is not null or fldRejectStatus4 is not null) ");
                            }
                            //sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("AND [HostRejectView] = @HostRejectView", "");
                        }
                    }
                    // xx end 20210517

                    // xx start 20210522
                    if (bankCodeForReport == "" || bankCodeForReport is null)
                    {
                        sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("AND fldBankCode=''", "");
                    }
                    // XX START 20210528
                    if (config.TaskId == "304771")
                    {
                        if (dsstatus.Equals("S"))
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString + " AND fldMICRDS = 0 and fldImageDS = 0";
                        }
                        else if (dsstatus.Equals("F"))
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString + " AND fldMICRDS <> 0 and fldImageDS <> 0";
                        }
                        //sql.conditionAsSqlString = sql.conditionAsSqlString + " group by fldamount,subTotalItem,fldPreBanktype,fldPreBankCode,fldBankDesc,fldMICRDS,fldImageDS,fldClearDate";
                    }
                    // XX end 20210528

                    // xx end 20210522
                    //if (transFilter == "" || transFilter is null)
                    //{
                    //    sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("TransFilter=''", "ci.fldTransCode in (02,03,08,15)");
                    //}

                    // xx start 20210602
                    if (config.TaskId == "304270")
                    {
                        if (status != "")
                        {
                            sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[Status] = @Status", status);
                        }
                    }
                    // xx end 20210602

                    if (config.TaskId == "304760" || config.TaskId == "304761" || config.TaskId == "304762" || config.TaskId == "304763" || config.TaskId == "304764" || config.TaskId == "304765" || config.TaskId == "304766")
                    {
                        //sql.orderBySql = "fldUserDesc";
                        if (reportPeriod == "Monthly" || reportPeriod == "Yearly" || reportPeriod == "Weekly")
                        {
                            /*sql.sql = " SELECT SUM(UserTot) as UserTot, fldUserDesc, Sum(Attendance) as Attendance, SUM(UserTot)/SUM(Attendance) as Average from View_UserPerformanceReportWeekly "
                                       + " where [fldClearDate] = @fldClearDate "
                                       + " group by UserTot, fldUserDesc, Attendance ";*/
                            sql.conditionAsSqlString = sql.conditionAsSqlString.Replace("[fldClearDate] = @fldClearDate", sDateCondition);
                            dt = dbContext.GetRecordsAsDataTable(sql.ToUserPerformance(), sql.sqlParams.ToArray());
                        }
                        else
                        {
                            dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());
                        }

                        //sql.sql = sql.sql.Replace("[fldClearDate] = @fldClearDate", sDateCondition);

                    }
                    else
                    {
                        dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());
                    }

                }
                // xx end

                //dt = dbContext.GetRecordsAsDataTable(sql.sql, sql.sqlParams.ToArray());
                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
                //20200227: END: LENDER:
            }

            string deviceInfo = string.Format(GetDeviceInfo(reportModel.orientation), reportType);
            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;
            //return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            byte[] temp1 = localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);


            localReport.DataSources.Clear();
            localReport.Dispose();
            localReport.ReleaseSandboxAppDomain();


            return temp1;


        }


        public byte[] renderReportBasedOnConfigForPage(ReportModel reportModel, FormCollection collection, string path,
            string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();
            collection.Add("ReportLogo", new Uri(CurrentUser.Account.LogoPath).AbsoluteUri);
            collection.Add("BankName", CurrentUser.Account.BankDesc);
            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));

            if (reportType == "Excel")
            {
                reportType = "Excelopenxml";
            }

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);

            if (configTable.ViewOrTableName.Equals(""))
            {
                configTable.ViewOrTableName = reportModel.viewId;
            }
            //Heavy load of Datasets load
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                if (reportModel.taskId == "102160" || reportModel.taskId == "102220" || reportModel.taskId == "102230" || reportModel.taskId == "102520" || reportModel.taskId == "102680"  || reportModel.taskId == "102670" || reportModel.taskId == "205131")
                {
                    config.AddSqlExtraCondition("fldBankCode='" + CurrentUser.Account.BankCode + "'");
                }
               
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructPaginatedResultQueryWithFilter(config, configTable, collection);
                //20200227: BEGIN: LENDER: PRINT ALL SELECT QUERY.
                DataTable dt;
                string page = DatabaseUtils.SanitizeString(collection["page"]);
                page = page == null ? "0" : page;
                if (config.TaskId == "107020" || config.TaskId == "107030" || config.TaskId == "102220" || config.TaskId == "102230" )
                {

                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    string ParameterName = "";



                    if (sql.sqlParams.Count == 0)
                    {
                        ParameterName = "";
                    }
                    else
                    {
                        foreach (var print in sql.sqlParams)
                        {
                            ParameterName = print.ParameterName;
                        }

                    }
                    if (config.TaskId == "107020" || config.TaskId == "107030")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldHubCode", collection["fldHubCode"]));
                        sqlParameterNext.Add(new SqlParameter("@fldHubDesc", collection["fldHubDesc"]));
                        sqlParameterNext.Add(new SqlParameter("@fldGroupId", ""));
                        sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", ""));
                    }
                    else if (config.TaskId == "102220" || config.TaskId == "102230")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldHubCode", ""));
                        sqlParameterNext.Add(new SqlParameter("@fldHubDesc", ""));
                        sqlParameterNext.Add(new SqlParameter("@fldGroupId", collection["fldGroupCode"]));
                        sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", collection["fldGroupDesc"]));
                    }
                    sqlParameterNext.Add(new SqlParameter("@PageNumber", page));
                    sqlParameterNext.Add(new SqlParameter("@Parameter", ParameterName));
                    sqlParameterNext.Add(new SqlParameter("@ParameterCount", sql.sqlParams.Count));
                    sqlParameterNext.Add(new SqlParameter("@TaskId", config.TaskId));
                    sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

                     dt = dbContext.GetRecordsAsDataTableSP("spcgReportList", sqlParameterNext.ToArray());
                    localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
                }
                    dt = dbContext.GetRecordsAsDataTable(sql.sql, sql.sqlParams.ToArray());
                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
                //if (config.TaskId == "107020")
                //{
                //    switch (sql.sqlParams.Count)
                //    {
                //        case 1:
                //            {
                //                foreach (var print in sql.sqlParams)
                //                {
                //                    if (print.ParameterName.Equals("@fldHubId"))
                //                    {
                //                        sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, um.fldUserAbb FROM tblUserMaster AS um FULL OUTER JOIN tblHubUser AS hu ON um.fldUserId = hu.fldUserId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hu.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubId LIKE '%'+@fldHubId+'%' ORDER BY hm.fldHubId";

                //                    }
                //                    else
                //                    {
                //                        sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, um.fldUserAbb FROM tblUserMaster AS um FULL OUTER JOIN tblHubUser AS hu ON um.fldUserId = hu.fldUserId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hu.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubDesc LIKE '%'+@fldHubDesc+'%' ORDER BY hm.fldHubId";

                //                    }
                //                }

                //                break;
                //            }
                //        case 2:
                //            {
                //                sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, um.fldUserAbb FROM tblUserMaster AS um FULL OUTER JOIN tblHubUser AS hu ON um.fldUserId = hu.fldUserId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hu.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubId LIKE '%'+@fldHubId+'%' AND hm.fldHubDesc LIKE '%'+@fldHubDesc+'%' ORDER BY hm.fldHubId";
                //                break;
                //            }
                //        default:
                //            {
                //                sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, um.fldUserAbb FROM tblUserMaster AS um FULL OUTER JOIN tblHubUser AS hu ON um.fldUserId = hu.fldUserId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hu.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + "  ORDER BY hm.fldHubId";

                //                break;
                //            }
                //    }

                //}
                //else if (config.TaskId == "107030")
                //{
                //    switch (sql.sqlParams.Count)
                //    {
                //        case 1:
                //            {
                //                foreach (var print in sql.sqlParams)
                //                {
                //                    if (print.ParameterName.Equals("@fldHubId"))
                //                    {
                //                        sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, bm.fldBranchDesc FROM tblInternalBranchMaster AS bm FULL OUTER JOIN tblHubBranch AS hb ON bm.fldBranchCode = hb.fldBranchId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hb.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubId LIKE '%'+@fldHubId+'%' ORDER BY hm.fldHubId";

                //                    }
                //                    else
                //                    {
                //                        sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, bm.fldBranchDesc FROM tblInternalBranchMaster AS bm FULL OUTER JOIN tblHubBranch AS hb ON bm.fldBranchCode = hb.fldBranchId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hb.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubDesc LIKE '%'+@fldHubDesc+'%' ORDER BY hm.fldHubId";

                //                    }
                //                }

                //                break;
                //            }
                //        case 2:
                //            {
                //                sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, bm.fldBranchDesc FROM tblInternalBranchMaster AS bm FULL OUTER JOIN tblHubBranch AS hb ON bm.fldBranchCode = hb.fldBranchId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hb.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " AND hm.fldHubId LIKE '%'+@fldHubId+'%' AND hm.fldHubDesc LIKE '%'+@fldHubDesc+'%' ORDER BY hm.fldHubId";
                //                break;
                //            }
                //        default:
                //            {
                //                sql.sql = "SELECT hm.fldHubId, hm.fldHubDesc, bm.fldBranchDesc FROM tblInternalBranchMaster AS bm FULL OUTER JOIN tblHubBranch AS hb ON bm.fldBranchCode = hb.fldBranchId FULL OUTER JOIN tblHubMaster AS hm ON hm.fldHubId = hb.fldHubId WHERE hm.fldBankCode= " + CurrentUser.Account.BankCode + " ORDER BY hm.fldHubId";

                //                break;
                //            }
                //    }


                //}



            }

            string deviceInfo = string.Format(GetDeviceInfo(reportModel.orientation), reportType);
            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;
            //return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            byte[] temp1 = localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            localReport.DataSources.Clear();
            localReport.Dispose();
            localReport.ReleaseSandboxAppDomain();

            return temp1;

        }

        public byte[] renderChequeReportWithImageBasedOnConfig(ReportModel reportModel, FormCollection collection,
            string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();
            


            //Add Cheque Image to report as parameter
            string imageFolder = collection["imageFolder"];
            string imageId = collection["imageId"];
            string frontImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "front", "bw" });
            string backImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "back", "bw" });
            string grayscaleImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "grayscale" });

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("GrayscaleChequeImage", grayscaleImagePath);            

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                DataTable dt;
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(config, configTable, collection);
                if (reportModel.taskId == "62010")
                {
                    dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectTop1All(), sql.sqlParams.ToArray());
                }
                else
                {
                    dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());
                }
                
                
                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
            }

            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;

            byte[] temp1 = localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            localReport.DataSources.Clear();
            localReport.Dispose();
            localReport.ReleaseSandboxAppDomain();

            return temp1;
        }

        public byte[] PrintChequeConfig(ReportModel reportModel, FormCollection collection,
                                        string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sqlQuery = "";
            string itemId = "";
            //string itemId = collection["imageItemId"]; //Commented by Michelle 20200604
            if (collection["hTaskId"] == "311113")
            {
                itemId = collection["current_fldInwardItemID"];
            }
            else
            {
                itemId = collection["fldInwardItemID"];
            }
            string uic = collection["current_flduic"].Trim();
            //string frontImagePath = "";
            //string backImagePath = "";
            //string grayfrontImagePath = "";
            //string graybackImagePath = "";

            //sqlQuery = "select frontimg,backimg,gfrontimg,gbackimg from view_printcheque_ocs where flditemid = @flditemid::bigint";
            //sqlQuery = "select fldGFrontIMGCode as fldgfrontimgbt, fldGBackIMGCode as fldgbackimgbt, fldFrontIMGCode as fldfrontimgbt, fldBackIMGCode as fldbackimgbt,fldUVIMGCode as flduvimgbt  from tblICSMICRImage where fldUIC = @flduic";
            //sqlParams.Add(new SqlParameter("@flduic", uic));

            //DataTable dtable = dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());

            //foreach (DataRow row in dtable.Rows)
            //{
            //    frontImagePath = Convert.ToBase64String((byte[])row["fldfrontimgbt"]);
            //    backImagePath = Convert.ToBase64String((byte[])row["fldbackimgbt"]);
            //    grayfrontImagePath = Convert.ToBase64String((byte[])row["fldgfrontimgbt"]);
                //graybackImagePath = Convert.ToBase64String((byte[])row["fldgbackimgbt"]);

            string imageFolder = collection["imageFolder"];
            string imageId = collection["imageId"];
            string frontImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "front", "bw" });
            string backImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "back", "bw" });
            string grayscaleImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "grayscale" });


            //FormCollection collection = new FormCollection();

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("GrayFrontChequeImage", grayscaleImagePath);
            //collection.Add("GrayBackChequeImage", graybackImagePath);
            collection["fldInwardItemID"] = itemId;

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                DataTable dt;
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(config, configTable, collection);
                dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());

                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
            }

            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;

            sqlParams.Clear();

            return localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        }

        public byte[] renderSignatureCIFReport(ReportModel reportModel, FormCollection collection,
            string path, string reportType, out string mimeType, string imageValue)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();

            collection.Add("Image", imageValue);

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                DataTable dt;
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(config, configTable, collection);

                dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());


                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
            }

            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;

            return localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        }


        private string getImagePathForCheque(string imageFolder, string imageId, List<string> states)
        {

            //List<ImageHelper.ImageInfo> lstimageInfo = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);

            
            ImageHelper.ImageInfo imageInfo = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);

            if (!System.IO.File.Exists(imageInfo.sourcePath))
            {
                return null;
            }
            if (!System.IO.File.Exists(imageInfo.destinationPath))
            {
                try
                {
                    imageHelper.convertImageFromTiff(imageInfo.sourcePath, imageInfo.destinationPath, 1, imageInfo.sizeScale, imageInfo.angle, imageInfo.filter);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            return imageInfo.destinationPath.Replace("\\", "/");
        }

        // xx 20210506 start
        private List<ReportParameter> convertFormCollectionToReportParams(ReportParameterInfoCollection availableReportParams, FormCollection collection)
        {
            List<ReportParameter> result = new List<ReportParameter>();
            foreach (ReportParameterInfo prm in availableReportParams)
            {
                if (prm.Name == "fldClearDate" || prm.Name == "from_fldClearDate" || prm.Name == "to_fldClearDate")
                {
                    if (!string.IsNullOrEmpty(collection[prm.Name]))
                    {
                        result.Add(new ReportParameter(prm.Name, DateUtils.formatDateToReportDate(collection[prm.Name].Replace(",",""))));
                    }
                }
                if (prm.Name == "fldCreateTimeStamp" || prm.Name == "from_fldCreateTimeStamp" || prm.Name == "to_fldCreateTimeStamp")
                {
                    if (!string.IsNullOrEmpty(collection[prm.Name]))
                    {
                        result.Add(new ReportParameter(prm.Name, DateUtils.formatDateToReportDate(collection[prm.Name])));
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(collection[prm.Name]))
                    {
                        result.Add(new ReportParameter(prm.Name, collection[prm.Name]));
                    }
                }
            }            
            return result;

        }
        // xx 20210506 end

        private Boolean HasData(string view, string todate, string fromdate)
        {
            DataTable ds = new DataTable();
            string strQuery;
            string resultTo = DateTime.ParseExact(todate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string resultFrom = DateTime.ParseExact(fromdate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            strQuery = "Select count(1) from " + view + " where datediff(d,fldcleardate,@todate)>=0 and datediff(d,fldcleardate,@fromdate)<=0";
            ds = dbContext.GetRecordsAsDataTable(strQuery, new[]
            {
            new SqlParameter("@todate", resultTo ) ,
            new SqlParameter("@fromdate", resultFrom )
               });
            if (ds.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // XX START
        public string getClearDate()
        {
            string dbClearDate = "";

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetClearDate", sqlParameterNext.ToArray());

            dbClearDate = Convert.ToDateTime(dt.Rows[0]["fldClearDate"]).ToString("yyyy-MM-dd");

            return dbClearDate;
        }
        // XX END
        //* Created By Ali *//
        public string getReportName()
        {
            string dbReportName = "";

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetReportName", sqlParameterNext.ToArray());

            dbReportName = Convert.ToString(dt.Rows[0]["fldBankDesc"]);

            return dbReportName;
        }

        // xx start 20210517
        public List<ReportModel> ListBankHostStatus()
        {
            DataTable dataTable = new DataTable();
            List<ReportModel> hostStatus = new List<ReportModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dataTable = dbContext.GetRecordsAsDataTableSP("spcgHostStatusMaster", sqlParameterNext.ToArray());

            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    ReportModel status = new ReportModel()
                    {
                        bankHostStatusID = row["fldStatusID"].ToString(),
                        bankHostStatusDesc = row["fldStatusDesc"].ToString()
                    };
                    hostStatus.Add(status);
                }
            }
            return hostStatus;
        }
        // xx end 20210517
    }
}