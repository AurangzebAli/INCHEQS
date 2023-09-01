﻿using INCHEQS.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
//using INCHEQS.Models.Account;
using System.Web.Mvc;
using INCHEQS.Security;
//using System.Data.Entity;
using INCHEQS.Areas.OCS.ViewModels;
using System.Data.Common;
using INCHEQS.Models.SearchPageConfig;
using System.Threading.Tasks;
using INCHEQS.Security.Account;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.Clearing;
using INCHEQS.Areas.OCS.Models.ClearingSummary;



namespace INCHEQS.Models.SearchPageConfig
{
    public class PageConfigDaoOCS : IPageConfigDaoOCS
    {

        // private string strConn = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

        private readonly OCSDbContext dbContext;
        private readonly ClearingDao clearingDao;
        private readonly ClearingSummaryDao clearingSummaryDao;
        public PageConfigDaoOCS(OCSDbContext dbContext, ClearingDao clearingDao, ClearingSummaryDao clearingSummaryDao)
        {
            this.dbContext = dbContext;
            this.clearingDao = clearingDao;
            this.clearingSummaryDao = clearingSummaryDao;
        }


        public async Task<ResultPageViewModel> getResultListFromDatabaseViewAsync(PageSqlConfig sqlPageConfig, FormCollection collection)
        {
            return await Task.Run(() => getResultListFromDatabaseView(sqlPageConfig, collection));
        }

        public async Task<List<Dictionary<string, string>>> GetResultDashboardForDivIdAsync(string divId, PageSqlConfig pageSqlConfig, FormCollection collection)
        {
            return await Task.Run(() => GetResultDashboardForDivId(divId, pageSqlConfig, collection));
        }
        public async Task<List<Dictionary<string, string>>> updateQueueSessionAsync(string fldTaskId, string fldBankCode, string fldCapturingbranch, string fldScannerId, string fldBatchNumber, string fldUIC, string fldUserId)
        {
            return await Task.Run(() => updateQueueSession(fldTaskId, fldBankCode, fldCapturingbranch, fldScannerId, fldBatchNumber, fldUIC, fldUserId));
        }
        public async Task<List<Dictionary<string, string>>> updateQueueClearingSessionAsync(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate)
        {
            return await Task.Run(() => updateQueueClearingSession(scannerbatch, flduserid, flduic, userbankcode, fldscannerid, fldcapturingdate));
        }
        public async Task<List<Dictionary<string, string>>> updateQueueSuspendSessionAsync(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate)
        {
            return await Task.Run(() => updateQueueSuspendSession(scannerbatch, flduserid, flduic, userbankcode, fldscannerid, fldcapturingdate));
        }
        public async Task<SearchPageViewModel> GetSearchFormModelFromConfigAsync(AccountModel accountModel, PageSqlConfig pageSqlConfig)
        {
            return await Task.Run(() => GetSearchFormModelFromConfig(accountModel, pageSqlConfig));
        }

        public async Task<ConfigTable> GetConfigTableAsync(string taskId, string viewId = null)
        {
            return await Task.Run(() => GetConfigTable(taskId, viewId));
        }

        public async Task<SearchPageViewModel> GetPageConfigViewModelFromConfigAsync(PageSqlConfig pageSqlConfig)
        {
            return await Task.Run(() => GetPageConfigViewModelFromConfig(pageSqlConfig));
        }

        public async Task<QueueSqlConfig> GetQueueConfigAsync(string taskId, AccountModel account)
        {
            return await Task.Run(() => GetQueueConfig(taskId, account));
        }

        public async Task<QueueSqlConfig> GetQueueConfigAsyncNew(string taskId, AccountModel account)
        {
            return await Task.Run(() => GetQueueConfigNew(taskId, account));
        }

        public async Task<ResultPageViewModel> getQueueResultListFromDatabaseViewAsync(QueueSqlConfig queSqlConfig, FormCollection collection)
        {
            return await Task.Run(() => getQueueResultListFromDatabaseView(queSqlConfig, collection));
        }


        //This function gets Table-Formatted List from Database View
        public ResultPageViewModel getQueueResultListFromDatabaseView(QueueSqlConfig queSqlConfig, FormCollection collection)
        {
            string currentPage = collection["page"];
            if (string.IsNullOrEmpty(currentPage)) { currentPage = "1"; }
            ConfigTable configTable = GetConfigTable(queSqlConfig.TaskId);
            DataTable resultTable = GetQueueResultTable(queSqlConfig, configTable, collection);

            return WrapResultPage(resultTable, configTable, Convert.ToInt32(currentPage)); ;
        }

        public QueueSqlConfig GetQueueConfig(string taskId, AccountModel account)
        {
            DataTable dataResult = new DataTable();
            QueueSqlConfig pageSqlConfig = new QueueSqlConfig();
            Dictionary<string, string> currentUserParams = account.ToDictionary();

            string stmt = "Select * from [tblQueueConfig] where fldTaskId=@fldTaskId";
            dataResult = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldTaskId", taskId)
            });
            string sql = "";
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            if (dataResult.Rows.Count > 0)
            {
                //List<string> allowedAction = new List<string>(dataResult.Rows[0]["fldAllowdAction"].ToString().Split(','));    
                pageSqlConfig.TaskId = dataResult.Rows[0]["fldTaskId"].ToString();
                pageSqlConfig.ViewOrTableName = dataResult.Rows[0]["fldViewName"].ToString();
                pageSqlConfig.SqlOrderBy = dataResult.Rows[0]["fldOrderBy"].ToString();
                pageSqlConfig.PageTitle = dataResult.Rows[0]["fldQueueDesc"].ToString();
                pageSqlConfig.SqlLockCondition = dataResult.Rows[0]["fldLockCondition"].ToString();
                sql = dataResult.Rows[0]["fldUserParamCondition"].ToString();

                //Replace sql with currentuser parameters
                KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sql, currentUserParams);

                pageSqlConfig.SqlExtraCondition = holder.Key;
                pageSqlConfig.SqlExtraConditionParams = holder.Value.ToArray();
                pageSqlConfig.AllowedActions = new List<string>(dataResult.Rows[0]["fldAllowedAction"].ToString().Replace(" ", "").Split(','));
                pageSqlConfig.TaskRole = dataResult.Rows[0]["fldTaskRole"].ToString();
            }
            return pageSqlConfig;
        }

        public QueueSqlConfig GetQueueConfigNew(string taskId, AccountModel account)
        {
            DataTable dataResult = new DataTable();
            QueueSqlConfig pageSqlConfig = new QueueSqlConfig();
            Dictionary<string, string> currentUserParams = account.ToDictionary();

            string stmt = "Select * from [tblQueueConfig] where fldTaskId=@fldTaskId";
            dataResult = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldTaskId", taskId)
            });
            string sql = "";
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            if (dataResult.Rows.Count > 0)
            {
                //List<string> allowedAction = new List<string>(dataResult.Rows[0]["fldAllowdAction"].ToString().Split(','));    
                pageSqlConfig.TaskId = dataResult.Rows[0]["fldTaskId"].ToString();
                pageSqlConfig.ViewOrTableName = dataResult.Rows[0]["fldViewName"].ToString();
                pageSqlConfig.SqlOrderBy = dataResult.Rows[0]["fldOrderBy"].ToString();
                pageSqlConfig.PageTitle = dataResult.Rows[0]["fldQueueDesc"].ToString();
                pageSqlConfig.SqlLockCondition = dataResult.Rows[0]["fldLockCondition"].ToString();
                sql = dataResult.Rows[0]["fldUserParamCondition"].ToString();


                KeyValuePair<string, List<SqlParameter>> holder;
                if (pageSqlConfig.TaskId == "999930")
                {
                    holder = clearingDao.GetSqlParameter(sql, currentUserParams);
                }
                //else if (pageSqlConfig.TaskId == "999940") {
                //    holder = clearingSummaryDao.GetSqlParameter(sql, currentUserParams);
                //}
                else
                {
                    //Replace sql with currentuser parameters
                    holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sql, currentUserParams);
                }

                pageSqlConfig.StoreProcedure = dataResult.Rows[0]["fldSP"].ToString();
                pageSqlConfig.SqlExtraCondition = holder.Key;
                pageSqlConfig.SqlExtraConditionParams = holder.Value.ToArray();
                pageSqlConfig.AllowedActions = new List<string>(dataResult.Rows[0]["fldAllowedAction"].ToString().Replace(" ", "").Split(','));
                pageSqlConfig.TaskRole = dataResult.Rows[0]["fldTaskRole"].ToString();
                pageSqlConfig.CurrentUser = currentUserParams;
            }
            return pageSqlConfig;
        }

        private DataTable GetQueueResultTable(QueueSqlConfig quePageConfig, ConfigTable configTable, FormCollection collection)
        {
            PageSqlConfig sqlPageConfig = quePageConfig.toPageSqlConfig();
            Dictionary<string, string> dicCurrentUser = quePageConfig.CurrentUser;
            configTable.ViewOrTableName = quePageConfig.ViewOrTableName;
            //Find Me Container
            SearchPageHelper.SqlDetails sqlContainer;
            if ((sqlPageConfig.TaskId == "999930"))//|| (sqlPageConfig.TaskId == "999940")
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterClearing(sqlPageConfig, configTable, collection, dicCurrentUser);

                try
                {
                    return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }
            //if ( (sqlPageConfig.TaskId == "306910") || (sqlPageConfig.TaskId == "306920") || (sqlPageConfig.TaskId == "306930"))
            //{


            //    sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterNew(sqlPageConfig, configTable, collection, dicCurrentUser);

            //    try
            //    {
            //        return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
            //    }
            //}
            //else if ((sqlPageConfig.TaskId == "308130") || (sqlPageConfig.TaskId == "308140") || (sqlPageConfig.TaskId == "308150")  || (sqlPageConfig.TaskId == "308110") || (sqlPageConfig.TaskId == "308120") || (sqlPageConfig.TaskId == "306550"))
            //{
            //    sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterBranch(sqlPageConfig, configTable, collection, dicCurrentUser);

            //    try
            //    {
            //        return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
            //    }
            //}
            //else if (sqlPageConfig.TaskId == "306510" || sqlPageConfig.TaskId == "308210")
            //{
            //    sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterReviewMine(sqlPageConfig, configTable, collection, dicCurrentUser);

            //    try
            //    {
            //        return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
            //    }
            //}
            else
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilter(sqlPageConfig, configTable, collection);
                try
                {
                    return dbContext.GetRecordsAsDataTable(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }


        }



        //This function gets Table-Formatted List from Database View
        public ResultPageViewModel getResultListFromDatabaseView(
            PageSqlConfig sqlPageConfig, FormCollection collection)
        {
            ResultPageViewModel resultPage = new ResultPageViewModel();


            string currentPage = collection["page"];
            if (string.IsNullOrEmpty(currentPage))
            {
                currentPage = "1";
            }

            ConfigTable configTable = GetConfigTable(sqlPageConfig.TaskId, sqlPageConfig.ViewOrTableName);
            DataTable resultTable = GetResultTable(sqlPageConfig, configTable, collection);

            resultPage = WrapResultPage(resultTable, configTable, Convert.ToInt32(currentPage));

            return resultPage;
        }


        public SearchPageViewModel GetSearchFormModelFromConfig(AccountModel currentUserAccount, PageSqlConfig pageSqlConfig)
        {
            DataTable configTable = new DataTable();
            SearchPageViewModel searchPageView = new SearchPageViewModel();
            List<DataField> resultArray = new List<DataField>();
            string pageTitle = "";
            string bankcode = "";

            try
            {

                string stmt = "SELECT * FROM tblSearchPageConfig WHERE isEnabled = 'Y' And taskId = @taskId  And isFilter = 'Y' order by Ordering  ";
                configTable = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@taskId", pageSqlConfig.TaskId) });
                using (DbConnection con = dbContext.Database.Connection)
                {
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["ocs_connection_string"].ConnectionString;
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                    con.Open();

                    foreach (DataField dataField in ConvertConfigTableToConfigList(configTable))
                    {
                        pageTitle = dataField.pageTitle;
                        if (!string.IsNullOrEmpty(dataField.ValueTextQueryForOption))
                        {
                            if (dataField.ValueTextQueryForOption.Contains("@fldBankCode"))
                            {
                                dataField.ValueTextQueryForOption = dataField.ValueTextQueryForOption.Replace("@fldBankCode", currentUserAccount.BankCode);
                            }
                            //Michelle 20200602
                            else if (dataField.ValueTextQueryForOption.Contains("@fldUserId"))
                            {
                                dataField.ValueTextQueryForOption = dataField.ValueTextQueryForOption.Replace("@fldUserId", currentUserAccount.UserId);
                            }
                            //Michelle 20200602
                            dataField.keyValueFieldList = GetKeyValueFromSQLAndParams(con, dataField.ValueTextQueryForOption, currentUserAccount.ToDictionary());
                        }
                        if (!string.IsNullOrEmpty(dataField.fieldDefaultValue) & dataField.fieldDefaultValue.StartsWith("sql:"))
                        {
                            if (dataField.fieldType == "Date")
                            {
                                dataField.fieldDefaultValue = DateUtils.formatDateFromSql(ReplaceUserParamsConditionAndExecute(con, dataField.fieldDefaultValue, currentUserAccount.ToDictionary()).ToString());
                            }
                            else if (dataField.fieldType == "DateBranchEODSummary")
                            {
                                dataField.fieldDefaultValue = DateUtils.formatDateFromSql(ReplaceUserParamsConditionAndExecute(con, dataField.fieldDefaultValue, currentUserAccount.ToDictionary()).ToString());
                            }
                            else if (dataField.fieldType == "DateFromTo")
                            {
                                dataField.fieldDefaultValue = DateUtils.formatDateFromSql(ReplaceUserParamsConditionAndExecute(con, dataField.fieldDefaultValue, currentUserAccount.ToDictionary()).ToString());
                            }
                            else if (dataField.fieldType == "DateTime")
                            {
                                dataField.fieldDefaultValue = DateUtils.formatTimeStampFromSql(ReplaceUserParamsConditionAndExecute(con, dataField.fieldDefaultValue, currentUserAccount.ToDictionary()).ToString());
                            }
                            else
                            {
                                dataField.fieldDefaultValue = ReplaceUserParamsConditionAndExecute(con, dataField.fieldDefaultValue, currentUserAccount.ToDictionary());

                            }
                        }
                        resultArray.Add(dataField);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }

            searchPageView.FormFields = resultArray;
            searchPageView.PageTitle = pageTitle;
            return searchPageView;

        }


        //If record found for dashboardConfig, use table specified from dashboardConfig. Else, defaulted to "View_Database" as table
        public List<Dictionary<string, string>> GetResultDashboardForDivId(string divId, PageSqlConfig pageSqlConfig, FormCollection collection)
        {
            DataTable dt = new DataTable();

            string stmt = "SELECT * FROM [tblDashboardConfig] WHERE [isEnabled] = 'Y' AND divId = @divId";
            if (!string.IsNullOrEmpty(divId))
            {
                dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@divId", divId) });
                if (dt.Rows.Count > 0)
                {
                    pageSqlConfig.SetViewId(dt.Rows[0]["databaseViewId"].ToString());
                }
            }

            ConfigTable configTable = GetConfigTable(pageSqlConfig.TaskId, pageSqlConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlContainer = SearchPageHelper.ConstructSqlFromConfigTableSql(pageSqlConfig, configTable, collection);
            try
            {
                dt = dbContext.GetRecordsAsDataTable(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
            }

            return ConvertResultTableToListWithConfig(dt, configTable.dataTable);
        }



        public string ReplaceUserParamsConditionAndExecute(DbConnection con, string sqlQuery, Dictionary<string, string> currentUserParams)
        {
            DataTable resultTable = new DataTable();
            string result = "";
            try
            {
                using (DbCommand cmd = con.CreateCommand())
                {
                    KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sqlQuery, currentUserParams);

                    cmd.CommandText = holder.Key;
                    //cmd.CommandTimeout = 300;
                    cmd.Parameters.AddRange(holder.Value.ToArray());

                    using (DbDataReader sda = cmd.ExecuteReader())
                    {
                        resultTable.Load(sda);
                    }
                    if ((resultTable.Rows.Count > 0))
                    {
                        result = resultTable.Rows[0].ItemArray[0].ToString();
                    }
                }

            }
            catch (Exception ex)
            {

                throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlQuery, ex.Message));
            }

            return result;
        }



        private ArrayList GetKeyValueFromSQLAndParams(DbConnection oSqlConnection, string strSql, Dictionary<string, string> currentUserParams)
        {
            ArrayList resultArray = new ArrayList();

            //strSql = StringUtils.ReplaceParamsWithMap(strSql, params)
            try
            {
                using (DbCommand cmd = oSqlConnection.CreateCommand())
                {
                    DataTable dataTable = new DataTable();
                    KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(strSql, currentUserParams);

                    cmd.CommandText = holder.Key;
                    //cmd.CommandTimeout = 300;
                    cmd.Parameters.AddRange(holder.Value.ToArray());
                    using (DbDataReader sda = cmd.ExecuteReader())
                    {
                        dataTable.Load(sda);
                    }
                    foreach (DataRow row in dataTable.Rows)
                    {
                        ValueTextField keyValueField = new ValueTextField();
                        keyValueField.value = row["value"].ToString();
                        keyValueField.text = row["text"].ToString();
                        resultArray.Add(keyValueField);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", strSql, ex.Message));
            }

            return resultArray;
        }


        private ResultPageViewModel WrapResultPage(DataTable resultTable, ConfigTable configTableModel, int currentPage)
        {
            ResultPageViewModel resultPage = new ResultPageViewModel();
            string title = "";
            string sTaskid = "";
            ArrayList resultHeaders = new ArrayList();
            int totalRecord = 0;
            int totalOutstandingRecord = 0;
            int totalPage = 0;
            double grandTotalAmount = 0;
            int totalImportItem = 0;
            int grandtotalImportItem = 0;
            int grandtotalfile = 0;
            DataTable configTable = configTableModel.dataTable;
            if ((configTable.Rows.Count > 0))
            {
                title = configTable.Rows[0]["pageTitle"].ToString();
                sTaskid = configTable.Rows[0]["taskid"].ToString();
                //resultPage.RowActionUrl = configTable.Rows[0]["ResultClickUrl"].ToString();
            }

            foreach (DataRow row in configTable.Rows)
            {
                if ((string.Compare(row["isResult"].ToString(), "Y") == 0))
                {
                    resultHeaders.Add(row["fieldLabel"].ToString());
                }
            }

            if ((resultTable.Rows.Count > 0))
            {
                totalRecord = resultTable.Columns.Contains("TotalRecords") ? Convert.ToInt32(resultTable.Rows[0]["TotalRecords"]) : 0;
                totalOutstandingRecord = resultTable.Columns.Contains("TotalOutstanding") ? Convert.ToInt32(resultTable.Rows[0]["TotalOutstanding"]) : 0;
                totalPage = resultTable.Columns.Contains("TotalPage") ? Convert.ToInt32(resultTable.Rows[0]["TotalPage"]) : 0;
                grandTotalAmount = resultTable.Columns.Contains("GrandTotalAmount") ? (resultTable.Rows[0]["GrandTotalAmount"].ToString() != "" ? Convert.ToDouble(resultTable.Rows[0]["GrandTotalAmount"]) : 0) : 0;
                totalImportItem = resultTable.Columns.Contains("TotalImportItem") ? Convert.ToInt32(resultTable.Rows[0]["TotalImportItem"]) : 0;
                grandtotalImportItem = resultTable.Columns.Contains("GrandTotalImportItem") ? Convert.ToInt32(resultTable.Rows[0]["GrandTotalImportItem"]) : 0;
                grandtotalfile = resultTable.Columns.Contains("GrandTotalFile") ? Convert.ToInt32(resultTable.Rows[0]["GrandTotalFile"]) : 0;

                if (resultTable.Columns.Contains("TotalRecords")) resultTable.Columns.Remove("TotalRecords");
                if (resultTable.Columns.Contains("TotalOutstanding")) resultTable.Columns.Remove("RowNum");
                if (resultTable.Columns.Contains("TotalPage")) resultTable.Columns.Remove("TotalPage");
                if (resultTable.Columns.Contains("GrandTotalAmount")) resultTable.Columns.Remove("GrandTotalAmount");
                if (resultTable.Columns.Contains("TotalImportItem")) resultTable.Columns.Remove("TotalImportItem");
                if (resultTable.Columns.Contains("GrandTotalImportItem")) resultTable.Columns.Remove("GrandTotalImportItem");
                if (resultTable.Columns.Contains("GrandTotalFile")) resultTable.Columns.Remove("GrandTotalFile");
            }
            //700101
            resultPage.TotalRecordCount = totalRecord;
            resultPage.TotalOutstandingCount = totalOutstandingRecord;
            resultPage.TotalPage = totalPage;
            resultPage.TableData = CombineResultWithConfigData(resultTable, configTable);
            resultPage.ResultTable = resultTable;
            resultPage.TableHeaders = configTable;
            resultPage.PageTitle = title;
            resultPage.CurrentPage = currentPage;
            resultPage.GrandTotalAmount = grandTotalAmount;
            resultPage.TotalImportItem = totalImportItem;
            resultPage.GrandTotalImportItem = grandtotalImportItem;
            resultPage.GrandTotalFile = grandtotalfile;
            resultPage.Taskid = sTaskid;


            return resultPage;
        }

        //The purpose of this function is to transform rows in resultTable to hold the value configured in configTable. 
        //It is done through interating resultTable and find the configuration of each cell and construct result list that 
        //contains datafield With the value from the result table
        private List<List<DataField>> CombineResultWithConfigData(DataTable resultTable, DataTable configTable)
        {
            List<List<DataField>> resultList = new List<List<DataField>>();
            List<DataField> dataList = ConvertConfigTableToConfigList(configTable);
            List<DataField> dataListTemp = new List<DataField>();
            DataField dataField = new DataField();
            string taskid = "";
            if (dataList.Count != 0)
            {
                taskid = dataList[0].taskId;
            }
            
            foreach (DataRow row in resultTable.Rows)
            {
                foreach (DataColumn item in resultTable.Columns)
                {
                    dataField = new DataField();
                    dataField = (from data in dataList where string.Compare(data.fieldId, item.Caption) == 0 select data).FirstOrDefault();
                    if (dataField != null)
                    {
                        dataField = dataField.Clone();
                        if (dataField.fieldType == "Date")
                        {
                            if (taskid == "700104" || taskid == "700106" || taskid == "999950")
                            {
                                dataField.value = row[item.Ordinal].ToString();
                               
                            }
                            else
                            {
                                dataField.value = DateUtils.formatDateFromSql(row[item.Ordinal].ToString());
                            }
                            
                        }
                        else if (dataField.fieldType == "DateFromTo")
                        {
                            dataField.value = DateUtils.formatDateFromSql(row[item.Ordinal].ToString());
                        }
                        else if (dataField.fieldType == "DateTime")
                        {
                            dataField.value = DateUtils.formatTimeStampFromSql(row[item.Ordinal].ToString());
                        }
                        else
                        {
                            dataField.value = row[item.Ordinal].ToString();
                        }
                        dataListTemp.Add(dataField);
                    }
                }
                resultList.Add(dataListTemp);
                dataListTemp = new List<DataField>();
            }

            return resultList;

        }



        public List<Dictionary<string, string>> ConvertResultTableToListWithConfig(DataTable resultTable, DataTable configTable)
        {

            List<DataField> dataList = ConvertConfigTableToConfigList(configTable);


            List<Dictionary<string, string>> lst = resultTable.AsEnumerable()
                .Select(r => r.Table.Columns.Cast<DataColumn>()
                        .Select(c => new KeyValuePair<string, object>(c.ColumnName, r[c.Ordinal])
                       ).ToDictionary(z => (from data in dataList where z.Key.Equals(data.fieldId) select data).FirstOrDefault().fieldLabel, z => z.Value.ToString())
                ).ToList();

            return lst;
        }

        private List<DataField> ConvertConfigTableToConfigList(DataTable configTable)
        {
            List<DataField> resultArray = new List<DataField>();
            foreach (DataRow row in configTable.Rows)
            {
                DataField dataField = new DataField();
                dataField.taskId = row["taskId"].ToString();
                dataField.pageTitle = row["pageTitle"].ToString();

                dataField.databaseViewId = row["databaseViewId"].ToString();

                dataField.fieldId = row["fieldId"].ToString();
                dataField.fieldType = row["fieldType"].ToString();
                dataField.fieldLabel = row["fieldLabel"].ToString();
                dataField.fieldDefaultValue = row["fieldDefaultValue"].ToString();

                dataField.ValueTextQueryForOption = row["ValueTextQueryForOption"].ToString();
                dataField.length = Convert.ToInt32(row["length"]);

                dataField.isResultParameter = row["isResultParam"].ToString();
                dataField.isFilter = row["isFilter"].ToString();
                dataField.isResult = row["isResult"].ToString();
                dataField.isEnabled = row["isEnabled"].ToString();
                dataField.isReadOnly = row["isReadOnly"].ToString();

                dataField.ordering = (int)row["ordering"];

                resultArray.Add(dataField);
            }
            return resultArray;
        }


        public ConfigTable GetConfigTable(string taskId, string viewId = null)
        {

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            string stmt = "SELECT * FROM [tblSearchPageConfig] sp WHERE isEnabled = 'Y' And taskId = @taskId order by Ordering  ";
            sqlParams.Add(new SqlParameter("@taskId", taskId));
            DataTable configTable = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());
            string tableName = "";
            if (!string.IsNullOrEmpty(viewId))
            {
                tableName = viewId;
            }
            else if (configTable.Rows.Count > 0)
            {
                tableName = configTable.Rows[0]["DatabaseViewId"].ToString();
            }

            return new ConfigTable(configTable, tableName);
        }


        private DataTable GetResultTable(PageSqlConfig sqlPageConfig, ConfigTable configTable, FormCollection collection)
        {

            SearchPageHelper.SqlDetails sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilter(sqlPageConfig, configTable, collection);
            try
            {
                return dbContext.GetRecordsAsDataTable(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
            }
        }


        public SearchPageViewModel GetPageConfigViewModelFromConfig(PageSqlConfig pageSqlConfig)
        {
            //AccountModel currentUserAccount = CurrentUser.Account;
            SearchPageViewModel searchPageView = new SearchPageViewModel();
            string stmt = "SELECT top 1* FROM [tblSearchPageConfig] WHERE isEnabled = 'Y' And taskId = @taskId And isFilter = 'Y' order by Ordering  ";

            DataTable dt = dbContext.GetRecordsAsDataTable(stmt,
                new[] { new SqlParameter("@taskId", pageSqlConfig.TaskId)
                    //,new SqlParameter("@databaseViewId", pageSqlConfig.ViewOrTableName)
                });

            if (dt.Rows.Count > 0)
            {
                searchPageView.PageTitle = dt.Rows[0]["pageTitle"].ToString();
            }
            return searchPageView;

        }

        public List<Dictionary<string, string>> updateQueueSession(string fldTaskId, string fldBankCode, string fldCapturingbranch, string fldScannerId, string fldBatchNumber, string fldUIC, string fldUserId)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            string sMySQL, sMySQLDel = "";
            sMySQL = "Select * from tblQueueSession where fldUserId=@fldUserId and fldBankCode = @fldBankCode and fldTaskId = @fldTaskId";
            sqlParams.Add(new SqlParameter("@fldUserId", fldUserId));
            sqlParams.Add(new SqlParameter("@fldBankCode", fldBankCode));
            sqlParams.Add(new SqlParameter("@fldTaskId", fldTaskId));
            try
            {
                DataTable ds = dbContext.GetRecordsAsDataTable(sMySQL, sqlParams.ToArray());
                if (ds.Rows.Count > 0)
                {

                    //Clear session for other user with same bank code before reinsert, this is to avoid redundancy in queue listing
                    sMySQLDel = "Delete from tblQueueSession where flduserid=@flduserid and fldBankCode = @fldBankCode and fldTaskId = @fldTaskId";
                    dbContext.GetRecordsAsDataTable(sMySQLDel, new[] {
                         new SqlParameter("@flduserid", fldUserId),
                         new SqlParameter("@fldBankCode", fldBankCode),
                         new SqlParameter("@fldTaskId", fldTaskId)
                    });

                    sMySQL = "insert into tblQueueSession (fldTaskId, fldBankCode,fldCapturingbranch,fldScannerId,fldBatchNumber,fldUIC,fldUserId) " +
                                "values (@fldTaskId,@fldBankCode,@fldCapturingbranch,@fldScannerId,@fldBatchNumber,@fldUIC,@fldUserId)";
                            ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                             new SqlParameter("@fldTaskId", fldTaskId),
                            new SqlParameter("@fldBankCode", fldBankCode) ,
                            new SqlParameter("@fldCapturingbranch", fldCapturingbranch) ,
                            new SqlParameter("@fldScannerId", fldScannerId),
                            new SqlParameter("@fldBatchNumber", fldBatchNumber),
                            new SqlParameter("@fldUIC", fldUIC),
                            new SqlParameter("@fldUserId", fldUserId)

                    });
                }
                else
                {
                    sMySQL = "insert into tblQueueSession (fldTaskId, fldBankCode,fldCapturingbranch,fldScannerId,fldBatchNumber,fldUIC,fldUserId) " +
                                "values (@fldTaskId,@fldBankCode,@fldCapturingbranch,@fldScannerId,@fldBatchNumber,@fldUIC,@fldUserId)";
                    ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                             new SqlParameter("@fldTaskId", fldTaskId),
                            new SqlParameter("@fldBankCode", fldBankCode) ,
                            new SqlParameter("@fldCapturingbranch", fldCapturingbranch) ,
                            new SqlParameter("@fldScannerId", fldScannerId),
                            new SqlParameter("@fldBatchNumber", fldBatchNumber),
                            new SqlParameter("@fldUIC", fldUIC),
                            new SqlParameter("@fldUserId", fldUserId)

                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public List<Dictionary<string, string>> updateQueueClearingSession(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate)

        {


            List<SqlParameter> sqlParams = new List<SqlParameter>();
            string sMySQL, sMySQLDel = "";
            //sMySQL = "Select * from tblqueuesession where flduserid=@flduserid and fldtype = @fldtype and userbankcode = @userbankcode";
            sMySQL = "Select * from tblqueueclearingsession where  userbankcode = @userbankcode";
            //sqlParams.Add(new SqlParameter("flduserid", flduserid));
            //sqlParams.Add(new SqlParameter("fldtype", fldtype));
            sqlParams.Add(new SqlParameter("userbankcode", userbankcode));


            //sqlParams.Add(new SqlParameter("fldcapturingdate", fldcapturingdate));
            // sqlParams.Add(new SqlParameter("fldbankcode", fldbankcode));
            //sqlParams.Add(new SqlParameter("fldbranchcode", fldbranchcode));

            try
            {
                DataTable ds = dbContext.GetRecordsAsDataTable(sMySQL, sqlParams.ToArray());

                if (ds.Rows.Count > 0)
                {

                    //Clear session for other user with same bank code before reinsert, this is to avoid redundancy in queue listing
                    sMySQLDel = "Delete from tblqueueclearingsession  where  userbankcode = @userbankcode";
                    dbContext.GetRecordsAsDataTable(sMySQLDel, new[] {

                    new SqlParameter("@userbankcode", userbankcode)

                    });

                    sMySQL = "insert into tblqueueclearingsession (fldscannerbatch, flduserid,flduic,userbankcode,fldscannerid) " +
                                "values (@scannerbatch,@flduserid,@flduic,@userbankcode,@fldscannerid)";
                    ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                     new SqlParameter("@scannerbatch", scannerbatch),
                    new SqlParameter("@flduserid", flduserid) ,
                    new SqlParameter("@flduic", flduic) ,
                    new SqlParameter("@fldscannerid", fldscannerid),
                    new SqlParameter("@userbankcode", userbankcode)
                    });

                }

                else

                {

                    sMySQL = "insert into tblqueueclearingsession (fldscannerbatch, flduserid,flduic,userbankcode,fldscannerid) " +
                                "values (@scannerbatch,@flduserid,@flduic,@userbankcode,@fldscannerid)";
                    ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                     new SqlParameter("@scannerbatch", scannerbatch),
                    new SqlParameter("@flduserid", flduserid) ,
                    new SqlParameter("@flduic", flduic) ,
                    new SqlParameter("@fldscannerid", fldscannerid),
                    new SqlParameter("@userbankcode", userbankcode)
                    });
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }


            return null;
        }


        public List<Dictionary<string, string>> updateQueueSuspendSession(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            string sMySQL, sMySQLDel = "";
            //sMySQL = "Select * from tblqueuesession where flduserid=@flduserid and fldtype = @fldtype and userbankcode = @userbankcode";
            sMySQL = "Select * from tblqueueclearingsession where  userbankcode = @userbankcode";
            //sqlParams.Add(new SqlParameter("flduserid", flduserid));
            //sqlParams.Add(new SqlParameter("fldtype", fldtype));
            sqlParams.Add(new SqlParameter("userbankcode", userbankcode));
            //sqlParams.Add(new SqlParameter("fldcapturingdate", fldcapturingdate));
            // sqlParams.Add(new SqlParameter("fldbankcode", fldbankcode));
            //sqlParams.Add(new SqlParameter("fldbranchcode", fldbranchcode));
            try
            {
                DataTable ds = dbContext.GetRecordsAsDataTable(sMySQL, sqlParams.ToArray());

                if (ds.Rows.Count > 0)
                {

                    //Clear session for other user with same bank code before reinsert, this is to avoid redundancy in queue listing
                    sMySQLDel = "Delete from tblqueueclearingsession  where  userbankcode = @userbankcode";
                    dbContext.GetRecordsAsDataTable(sMySQLDel, new[] {

                    new SqlParameter("@userbankcode", userbankcode)

                    });

                    sMySQL = "insert into tblqueueclearingsession (fldscannerbatch, flduserid,flduic,userbankcode,fldscannerid) " +
                                "values (@scannerbatch,@flduserid,@flduic,@userbankcode,@fldscannerid)";
                    ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                     new SqlParameter("@scannerbatch", scannerbatch),
                    new SqlParameter("@flduserid", flduserid) ,
                    new SqlParameter("@flduic", flduic) ,
                    new SqlParameter("@fldscannerid", fldscannerid),
                    new SqlParameter("@userbankcode", userbankcode)
                    });

                }

                else

                {

                    sMySQL = "insert into tblqueueclearingsession (fldscannerbatch, flduserid,flduic,userbankcode,fldscannerid) " +
                                "values (@scannerbatch,@flduserid,@userbankcode,@fldscannerid)";
                    ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                     new SqlParameter("@scannerbatch", scannerbatch),
                    new SqlParameter("@flduserid", flduserid) ,
                    new SqlParameter("@flduic", flduic) ,
                    new SqlParameter("@fldscannerid", fldscannerid),
                    new SqlParameter("@userbankcode", userbankcode)
                    });
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

    }
}