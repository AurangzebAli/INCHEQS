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
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Common;
//using Npgsql;
using INCHEQS.DataAccessLayer;

namespace INCHEQS.Models.Report.OCS
{
    public class ReportServiceOCS : IReportServiceOCS
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

        private readonly IPageConfigDaoOCS pageDao;
        private readonly ApplicationDbContext dbContext;
        private readonly ImageHelper imageHelper;
        //////public ReportServiceOCS(IPageConfigDaoOCS pageConfigDao , OCSDbContext dbContext, ImageHelper imageHelper) {
        //////    pageDao = pageConfigDao;
        //////    this.dbContext = dbContext;
        //////    //this.imageHelper = imageHelper;
        //////}

        public ReportServiceOCS(IPageConfigDaoOCS pageConfigDao, ApplicationDbContext dbContext, ImageHelper imageHelper)
        {
            pageDao = pageConfigDao;
            this.dbContext = dbContext;
            this.imageHelper = imageHelper;
        }

        public async Task<ReportModel> GetReportConfigAsync(PageSqlConfig pageSqlConfig)
        {
            return await Task.Run(() => GetReportConfig(pageSqlConfig));
        }


        public async Task<ReportModel> GetReportConfigByTaskIdAsync(string taskId)
        {
            PageSqlConfig temp = new PageSqlConfig();
            temp.SetTaskId(taskId);
            return await Task.Run(() => GetReportConfig(temp));
        }

        public async Task<DataTable> getReportBasedOnPageConfigAsync(PageSqlConfig pageSqlConfig, FormCollection collection)
        {
            return await Task.Run(() => getReportBasedOnPageConfig(pageSqlConfig, collection));
        }

        public ReportModel GetReportConfig(PageSqlConfig pageSqlConfig)
        {
            ReportModel reportModel = new ReportModel();
            List<PageSqlConfig> tableAndViewNames = new List<PageSqlConfig>();
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sql = "";

            if (string.IsNullOrEmpty(pageSqlConfig.PrintReportParam))
            {
                sql = "SELECT * FROM tblreportpageconfig where taskid = @taskId ";
                sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
            }
            else
            {
                sql = "SELECT * FROM tblreportpageconfig WHERE taskid = @taskId AND printreportparam=@printreportparam ";
                sqlParams.Add(new SqlParameter("@taskId", pageSqlConfig.TaskId));
                sqlParams.Add(new SqlParameter("@printreportparam", pageSqlConfig.PrintReportParam));
            }

            DataTable dt = dbContext.GetRecordsAsDataTable(sql, sqlParams.ToArray());


            foreach (DataRow row in dt.Rows)
            {
                reportModel.reportId = row["reportid"].ToString();
                reportModel.reportPath = row["reportpath"].ToString();
                reportModel.reportTitle = row["reporttitle"].ToString();
                reportModel.taskId = row["taskid"].ToString();
                reportModel.viewId = row["databaseviewid"].ToString();
                reportModel.extentionFilename = row["exportfilename"].ToString();
                reportModel.dataSetName = row["datasetname"].ToString();
                reportModel.orientation = row["orientation"].ToString();
                tableAndViewNames.Add(new PageSqlConfig(row["taskid"].ToString(), row["databaseviewid"].ToString()));
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
            string fldcleardate = DateUtils.formatDateToSql(collection["fldcleardate"]);

            DataTable ds = new DataTable();
            string tableOrViewName = reportModel.viewId;
            string stmt = string.Format("SELECT fldcleardate FROM  {0} LIMIT 1", configTable.ViewOrTableName);
            stmt = stmt + " WHERE fldcleardate=@fldcleardate and fldbankcode =  @fldBankCode";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
               new SqlParameter("@tableorviewname", configTable.ViewOrTableName),
                new SqlParameter("@fldcleardate",fldcleardate),
                new SqlParameter("@fldbankcode", bankcode)
            });

            //if (ds.Rows.Count == 0)
            //{
            //    reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
            //    return reportModel.viewId + "h";

            //}
            //else
            //{
            reportModel.extentionFilename = "[" + fldcleardate + "]" + reportModel.extentionFilename;
            return reportModel.viewId;
            //}

        }

        public byte[] renderReportBasedOnConfig(ReportModel reportModel, FormCollection collection, string path,
            string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();
            collection.Add("ReportLogo", new Uri(CurrentUser.Account.LogoPath).AbsoluteUri);
            collection.Add("BankName", CurrentUser.Account.BankDesc);
            localReport.SetParameters(convertFormCollectionToReportParams(availableReportParams, collection));

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);

            if (reportType == "Excel")
            {
                reportType = "Excelopenxml";
            }

            if (configTable.ViewOrTableName.Equals(""))
            {
                configTable.ViewOrTableName = reportModel.viewId;
            }
            if (reportModel.taskId != "106010" && reportModel.taskId != "102150" && reportModel.taskId != "102110" && reportModel.taskId != "102130" && reportModel.taskId != "102340" && reportModel.taskId != "102380" && reportModel.taskId != "102160" && reportModel.taskId != "102220" && reportModel.taskId != "102230" && reportModel.taskId != "102520" && reportModel.taskId != "102680" && reportModel.taskId != "102570" && reportModel.taskId != "102690")
            {
                configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
            }
            //Heavy load of Datasets load
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                if (reportModel.taskId != "102150" && reportModel.taskId != "102110" && reportModel.taskId != "102130" && reportModel.taskId != "102340" && reportModel.taskId != "102380" && reportModel.taskId != "102570")
                {
                    if ((config.TaskId == "308110") || (config.TaskId == "308130"))
                    {
                        config.AddSqlExtraCondition("fldbankcode=" + CurrentUser.Account.BankCode + " and fldbranchcode=" + CurrentUser.Account.BranchCodes[0]);
                    }
                    else if ((config.TaskId == "308120") || (config.TaskId == "308140"))
                    {
                        config.AddSqlExtraCondition("fldbankcode=" + CurrentUser.Account.BankCode + " and fldbranchcode=" + CurrentUser.Account.BranchCodes[0]);
                    }

                    else if ((config.TaskId == "306220") || (config.TaskId == "306910"))
                    {
                        config.AddSqlExtraCondition("  fldbrstn in ( Select fldbranchid from tbldedicatedbranch where flduserid = '" + CurrentUser.Account.UserAbbr + "')");
                    }
                    else if ((config.TaskId == "306230") || (config.TaskId == "306920"))
                    {
                        config.AddSqlExtraCondition("  fldbrstn in ( Select fldbranchid from tbldedicatedbranch where fldofficerid = '" + CurrentUser.Account.UserAbbr + "')");
                    }
                    else if ((config.TaskId == "306240") || (config.TaskId == "306930"))
                    {
                        config.AddSqlExtraCondition("  fldbrstn not in ( Select fldbranchid from tbldedicatedbranch where fldofficerid = '" + CurrentUser.Account.UserAbbr + "')");
                    }
                    else if (config.TaskId == "306510" || config.TaskId == "308210")
                    {
                        config.AddSqlExtraCondition("fldissuebankcode= '" + CurrentUser.Account.BankCode + "' and fldInvolvedUserName = '" + CurrentUser.Account.UserAbbr + "'");
                    }
                    else if (config.TaskId == "304640" || config.TaskId == "304650" || config.TaskId == "304660" || config.TaskId == "304670" || config.TaskId == "304680")
                    {
                        config.AddSqlExtraCondition("fldbankcode=" + CurrentUser.Account.BankCode + " and fldbrstn=" + CurrentUser.Account.BranchCodes[0]);
                    }
                    else
                    {
                        config.AddSqlExtraCondition("fldbankcode='" + CurrentUser.Account.BankCode + "'");
                    }
                }
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructSqlFromConfigTableSql(config, configTable, collection);
                DataTable dt = dbContext.GetRecordsAsDataTable(sql.ToSqlSelectAll(), sql.sqlParams.ToArray());
                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
            }

            string deviceInfo = string.Format(GetDeviceInfo(reportModel.orientation), reportType);
            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;
            return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
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

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);

            if (configTable.ViewOrTableName.Equals(""))
            {
                configTable.ViewOrTableName = reportModel.viewId;
            }
            //Heavy load of Datasets load
            foreach (PageSqlConfig config in reportModel.sqlConfigForDataSet)
            {
                if (reportModel.taskId == "102160" || reportModel.taskId == "102220" || reportModel.taskId == "102230" || reportModel.taskId == "102520" || reportModel.taskId == "102680" || reportModel.taskId == "106010")
                {
                    config.AddSqlExtraCondition("fldbankcode='" + CurrentUser.Account.BankCode + "'");
                }
                SearchPageHelper.SqlDetails sql = SearchPageHelper.ConstructPaginatedResultQueryWithFilter(config, configTable, collection);
                DataTable dt = dbContext.GetRecordsAsDataTable(sql.sql, sql.sqlParams.ToArray());
                localReport.DataSources.Add(new ReportDataSource(reportModel.dataSetName, dt));
            }

            string deviceInfo = string.Format(GetDeviceInfo(reportModel.orientation), reportType);
            string encoding; string fileNameExtension; Warning[] warnings; string[] streams;
            return localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
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
            //string frontImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "front" , "bw" });
            //string backImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "back", "bw" });
            //string grayscaleImagePath = getImagePathForCheque(imageFolder, imageId, new List<string> { "grayscale" });


            string frontImagePath = "";
            string backImagePath = "";
            string grayscaleImagePath = "";

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("GrayscaleChequeImage", grayscaleImagePath);

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
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

            return localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        }


        public void ReturnChequeAdviceConfig(ReportModel reportModel, string itemId,
            string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;//"D:\\ACH\\Development\\INCHEQS_MVC_v4.2-all-2-allMY\\V16\\NPGSQL-V.9.7\\03-MVC_WEB\\02-INCHEQS\\02-Trunk\\0001.000.000.00\\INCHEQS\\Reports\\OCS\\ReturnChequeAdvice.rdlc";//
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sqlQuery = "";
            string imageFolder = "";
            string imageUIC = "";

            string frontImagePath = "";
            string backImagePath = "";

            sqlQuery = "select fldfoldername,flduic,fldirgfrontimg,fldirgbackimg from view_returnchequeadvice where fldiriteminitialid = @IRItemInitialId";
            sqlParams.Add(new SqlParameter("@irtemIinitialid", itemId));

            DataTable dtable = dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());

            foreach (DataRow row in dtable.Rows)
            {
                imageFolder = row["fldfoldername"].ToString();
                imageUIC = row["flduic"].ToString();

                frontImagePath = row["fldGFrontIMGCode"].ToString();
                backImagePath = row["fldGBackIMGCode"].ToString();
            }


            frontImagePath = imageFolder + "\\" + frontImagePath;
            backImagePath = imageFolder + '\\' + backImagePath;


            FormCollection collection = new FormCollection();

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("fldiriteminitialid", itemId);

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
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

            string reportFolder = imageFolder + '\\' + "InwardReturnDocument";
            if (!Directory.Exists(reportFolder))
            {
                Directory.CreateDirectory(reportFolder);
            }
            string savePath = reportFolder + "\\" + imageUIC + "." + reportType;
            byte[] renderedBytes = localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                stream.Write(renderedBytes, 0, renderedBytes.Length);
            }

            sqlParams.Clear();
            sqlQuery = "UPDATE tblinwardreturnitem SET fldirdgenflag=@fldIRDGenFlag, fldupdateuserid=@fldUpdateUserId, fldupdatetimestamp=@fldUpdateTimeStamp WHERE fldiriteminitialid = @fldIRItemInitialId";
            sqlParams.Add(new SqlParameter("@fldIRItemInitialId", itemId));
            sqlParams.Add(new SqlParameter("@fldIRDGenFlag", "Y"));
            sqlParams.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParams.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));

            dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());
        }

        public byte[] ReturnChequeAdviceConfig(ReportModel reportModel, FormCollection collection,
            string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            // localReport.ReportPath = "D:\\ACH\\Development\\INCHEQS_MVC_v4.2-all-2-allMY\\V16\\NPGSQL-V.9.7\\03-MVC_WEB\\02-INCHEQS\\02-Trunk\\0001.000.000.00\\INCHEQS\\Reports\\OCS\\ReturnChequeAdvice.rdlc";//path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            //string itemId = collection["Download"];
            string itemId = collection["this_fldiriteminitialid"].Trim();
            string sqlQuery = "";
            string imageFolder = "";
            string imageUIC = "";

            string frontImagePath = "";
            string backImagePath = "";

            sqlQuery = "select fldfoldername,flduic from view_returnchequeadvice where fldiriteminitialid = @IRItemInitialId";
            sqlParams.Add(new SqlParameter("@iriteminitialid", itemId.ToString()));

            DataTable dtable = dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());

            foreach (DataRow row in dtable.Rows)
            {
                imageFolder = row["fldfoldername"].ToString();
                imageUIC = row["flduic"].ToString();

                //frontImagePath = row["fldirgfrontimg"].ToString();
                //backImagePath = row["fldirgbackimg"].ToString();
            }


            string stmt = "select fldFrontIMGCode as fldFrontIMGCode, fldBackIMGCode as fldBackIMGCode, " +
                "fldGFrontIMGCode as fldGFrontIMGCode, fldGBackIMGCode as fldGBackIMGCode,fldUVIMGCode as fldUVIMGCode" +
                "  from tblinwardreturnimage where fldUIC = @flduic";
            List<SqlParameter> sqlParamsReport = new List<SqlParameter>();
            sqlParamsReport.Add(new SqlParameter("@flduic", imageUIC));
            dtable = dbContext.GetRecordsAsDataTable(stmt, sqlParamsReport.ToArray());
            foreach (DataRow row in dtable.Rows)
            {
                frontImagePath = Convert.ToBase64String((byte[])row["fldGFrontIMGCode"]);
                backImagePath = Convert.ToBase64String((byte[])row["fldBackIMGCode"]);
            }
            //frontImagePath = imageFolder + "\\" + frontImagePath;
            //backImagePath = imageFolder + "\\" + backImagePath;


            //FormCollection collection = new FormCollection();

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("fldiriteminitialid", itemId);

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
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

            sqlParams.Clear();
            sqlQuery = "UPDATE tblinwardreturnitem SET fldirdprintflag=@fldIRDPrintFlag, fldupdateuserid=@fldUpdateUserId, fldupdatetimestamp=@fldUpdateTimeStamp, fldirdprinttimestamp=@fldirdprinttimestamp  WHERE fldiriteminitialid = @fldIRItemInitialId";
            sqlParams.Add(new SqlParameter("@fldIRItemInitialId", itemId));
            sqlParams.Add(new SqlParameter("@fldIRDPrintFlag", "Y"));
            sqlParams.Add(new SqlParameter("@fldUpdateUserId", int.Parse(CurrentUser.Account.UserId)));
            sqlParams.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParams.Add(new SqlParameter("@fldirdprinttimestamp", DateTime.Now));

            dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());
            return localReport.Render(reportType, string.Format(GetDeviceInfo(reportModel.orientation), reportType), out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        }

        public byte[] PrintChequeConfig(ReportModel reportModel, FormCollection collection,
            string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();
            collection.Add("ReportLogo", new Uri(CurrentUser.Account.LogoPath).AbsoluteUri);

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sqlQuery = "";
            string itemId = "";
            //string itemId = collection["imageItemId"]; //Commented by Michelle 20200604
            if (collection["hTaskId"] == "311113")
            {
                itemId = collection["current_fldCItemId"];
            }
            else
            {
                itemId = collection["flditemid"];
            }
            string uic = collection["current_flduic"].Trim();
            string frontImagePath = "";
            string backImagePath = "";
            string grayfrontImagePath = "";
            string graybackImagePath = "";

            //sqlQuery = "select frontimg,backimg,gfrontimg,gbackimg from view_printcheque_ocs where flditemid = @flditemid::bigint";
            sqlQuery = "select fldGFrontIMGCode as fldgfrontimgbt, fldGBackIMGCode as fldgbackimgbt, fldFrontIMGCode as fldfrontimgbt, fldBackIMGCode as fldbackimgbt,fldUVIMGCode as flduvimgbt  from tblOCSMICRImage where fldUIC = @flduic";
            sqlParams.Add(new SqlParameter("@flduic", uic));

            DataTable dtable = dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());

            foreach (DataRow row in dtable.Rows)
            {
                frontImagePath = Convert.ToBase64String((byte[])row["fldfrontimgbt"]);
                backImagePath = Convert.ToBase64String((byte[])row["fldbackimgbt"]);
                grayfrontImagePath = Convert.ToBase64String((byte[])row["fldgfrontimgbt"]);
                graybackImagePath = Convert.ToBase64String((byte[])row["fldgbackimgbt"]);
            }


            //FormCollection collection = new FormCollection();

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("GrayFrontChequeImage", grayfrontImagePath);
            collection.Add("GrayBackChequeImage", graybackImagePath);
            collection["flditemid"] = itemId;

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
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

        public byte[] PrintChequeSearchConfig(ReportModel reportModel, FormCollection collection,
        string path, string reportType, out string mimeType)
        {

            LocalReport localReport = new LocalReport();
            localReport.ReportPath = path;
            localReport.EnableExternalImages = true;
            ReportParameterInfoCollection availableReportParams = localReport.GetParameters();

            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string sqlQuery = "";
            string itemId = collection["imageItemId"];
            string frontImagePath = "";
            string backImagePath = "";
            string grayfrontImagePath = "";
            string graybackImagePath = "";

            sqlQuery = "select frontimg,backimg,gfrontimg,gbackimg from view_printcheque_ocs where flditemid = @flditemid::bigint";
            sqlParams.Add(new SqlParameter("@flditemid", itemId));

            DataTable dtable = dbContext.GetRecordsAsDataTable(sqlQuery, sqlParams.ToArray());

            foreach (DataRow row in dtable.Rows)
            {
                frontImagePath = row["frontimg"].ToString();
                backImagePath = row["backimg"].ToString();
                grayfrontImagePath = row["gfrontimg"].ToString();
                graybackImagePath = row["gbackimg"].ToString();
            }


            //FormCollection collection = new FormCollection();

            collection.Add("FrontChequeImage", frontImagePath);
            collection.Add("BackChequeImage", backImagePath);
            collection.Add("GrayFrontChequeImage", grayfrontImagePath);
            collection.Add("GrayBackChequeImage", graybackImagePath);
            collection["flditemid"] = itemId;

            List<ReportParameter> reportParamters = convertFormCollectionToReportParams(availableReportParams, collection);
            localReport.SetParameters(reportParamters);

            ConfigTable configTable = pageDao.GetConfigTable(reportModel.taskId);
            //configTable.ViewOrTableName = CHistory(reportModel, configTable, collection, CurrentUser.Account.BankCode);
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



        private string getImagePathForCheque(string imageFolder, string imageId, List<string> states)
        {
            //List<ImageHelper.ImageInfo> lstimageInfo = imageHelper.constructFileNameBasedOnParameters(imageFolder, imageId, states, CurrentUser.Account.UserAbbr);


            //ImageHelper.ImageInfo imageInfo = lstimageInfo[0];
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


        private List<ReportParameter> convertFormCollectionToReportParams(ReportParameterInfoCollection availableReportParams, FormCollection collection)
        {
            List<ReportParameter> result = new List<ReportParameter>();
            foreach (ReportParameterInfo prm in availableReportParams)
            {
                if (!string.IsNullOrEmpty(collection[prm.Name]))
                {
                    result.Add(new ReportParameter(prm.Name, collection[prm.Name]));
                }
            }
            return result;

        }

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
    }
}