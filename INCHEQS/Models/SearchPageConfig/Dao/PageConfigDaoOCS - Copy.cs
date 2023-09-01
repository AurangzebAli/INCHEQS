using INCHEQS.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using Npgsql;
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
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.Clearing;
//using System.Data.SqlClient;
//using INCHEQS.Areas.OCS.Models.ClearingSummary;
using System.Data.SqlClient;




namespace INCHEQS.Models.SearchPageConfig
{
    public class PageConfigDaoOCS : IPageConfigDaoOCS
    {

        // private string strConn = ConfigurationManager.ConnectionStrings["default"].ConnectionString;

        private readonly ApplicationDbContext dbContext;
        private readonly ClearingDao clearingDao;
        //private readonly ClearingSummaryDao clearingSummaryDao;
        public PageConfigDaoOCS(ApplicationDbContext dbContext, ClearingDao clearingDao)
        {
            this.dbContext = dbContext;
            this.clearingDao = clearingDao;
            //this.clearingSummaryDao = clearingSummaryDao;
        }


        public async Task<ResultPageViewModel> getResultListFromDatabaseViewAsync(PageSqlConfig sqlPageConfig, FormCollection collection)
        {
            return await Task.Run(() => getResultListFromDatabaseView(sqlPageConfig, collection));
        }

        public async Task<List<Dictionary<string, string>>> GetResultDashboardForDivIdAsync(string divId, PageSqlConfig pageSqlConfig, FormCollection collection)
        {
            return await Task.Run(() => GetResultDashboardForDivId(divId, pageSqlConfig, collection));
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

            string stmt = "Select * from tblqueueconfig where fldtaskid=@fldtaskid";
            dataResult = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldtaskid", taskId)
            });
            string sql = "";
            List<SqlParameter> SqlParameters = new List<SqlParameter>();

            if (dataResult.Rows.Count > 0)
            {
                //List<string> allowedAction = new List<string>(dataResult.Rows[0]["fldAllowdAction"].ToString().Split(','));    
                pageSqlConfig.TaskId = dataResult.Rows[0]["fldtaskid"].ToString();
                pageSqlConfig.ViewOrTableName = dataResult.Rows[0]["fldviewname"].ToString();
                pageSqlConfig.SqlOrderBy = dataResult.Rows[0]["fldorderby"].ToString();
                pageSqlConfig.PageTitle = dataResult.Rows[0]["fldqueuedesc"].ToString();
                pageSqlConfig.SqlLockCondition = dataResult.Rows[0]["fldlockcondition"].ToString();
                sql = dataResult.Rows[0]["flduserparamcondition"].ToString();

                //Replace sql with currentuser parameters
                KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sql, currentUserParams);

                pageSqlConfig.SqlExtraCondition = holder.Key;
                pageSqlConfig.SqlExtraConditionParams = holder.Value.ToArray();
                pageSqlConfig.AllowedActions = new List<string>(dataResult.Rows[0]["fldallowedaction"].ToString().Replace(" ", "").Split(','));
                pageSqlConfig.TaskRole = dataResult.Rows[0]["fldtaskrole"].ToString();
            }
            return pageSqlConfig;
        }

        public QueueSqlConfig GetQueueConfigNew(string taskId, AccountModel account)
        {
            DataTable dataResult = new DataTable();
            QueueSqlConfig pageSqlConfig = new QueueSqlConfig();
            Dictionary<string, string> currentUserParams = account.ToDictionary();

            string stmt = "Select * from tblqueueconfig where fldtaskid=@fldtaskid";
            dataResult = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldtaskid", taskId)
            });
            string sql = "";
            List<SqlParameter> SqlParameters = new List<SqlParameter>();

            if (dataResult.Rows.Count > 0)
            {
                //List<string> allowedAction = new List<string>(dataResult.Rows[0]["fldAllowdAction"].ToString().Split(','));    
                pageSqlConfig.TaskId = dataResult.Rows[0]["fldtaskid"].ToString();
                pageSqlConfig.ViewOrTableName = dataResult.Rows[0]["fldviewname"].ToString();
                pageSqlConfig.SqlOrderBy = dataResult.Rows[0]["fldorderby"].ToString();
                pageSqlConfig.PageTitle = dataResult.Rows[0]["fldqueuedesc"].ToString();
                pageSqlConfig.SqlLockCondition = dataResult.Rows[0]["fldlockcondition"].ToString();
                sql = dataResult.Rows[0]["flduserparamcondition"].ToString();


                KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sql, currentUserParams);
                if (pageSqlConfig.TaskId == "999930")
                {
                    holder = clearingDao.GetSqlParameter(sql, currentUserParams);
                }
                //else if (pageSqlConfig.TaskId == "999940")
                //{
                //    holder = clearingDao.GetNpgsqlParameterForSummary(sql, currentUserParams);
                //}
                else
                {
                    //Replace sql with currentuser parameters
                    holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(sql, currentUserParams);
                }




                

                pageSqlConfig.StoreProcedure = dataResult.Rows[0]["fldsp"].ToString();
                pageSqlConfig.SqlExtraCondition = holder.Key;
                pageSqlConfig.SqlExtraConditionParams = holder.Value.ToArray();
                pageSqlConfig.AllowedActions = new List<string>(dataResult.Rows[0]["fldallowedaction"].ToString().Replace(" ", "").Split(','));
                pageSqlConfig.TaskRole = dataResult.Rows[0]["fldtaskrole"].ToString();
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
            if ((sqlPageConfig.TaskId == "999930") || (sqlPageConfig.TaskId == "999940"))
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
            if ((sqlPageConfig.TaskId == "306910") || (sqlPageConfig.TaskId == "306920") || (sqlPageConfig.TaskId == "306930"))
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterNew(sqlPageConfig, configTable, collection, dicCurrentUser);
                try
                {
                    return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }

            else if ((sqlPageConfig.TaskId == "308130") || (sqlPageConfig.TaskId == "308140") || (sqlPageConfig.TaskId == "308150") || (sqlPageConfig.TaskId == "308110") || (sqlPageConfig.TaskId == "308120") || (sqlPageConfig.TaskId == "306550"))
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterBranch(sqlPageConfig, configTable, collection, dicCurrentUser);

                try
                {
                    return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }
            else if (sqlPageConfig.TaskId == "306510" || sqlPageConfig.TaskId == "308210")
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterReviewMine(sqlPageConfig, configTable, collection, dicCurrentUser);
                try
                {
                    return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }
            else if (sqlPageConfig.TaskId == "700101")
            {
                sqlContainer = SearchPageHelper.ConstructPaginatedResultQueryWithFilterBranch(sqlPageConfig, configTable, collection, dicCurrentUser);

                try
                {
                    return dbContext.GetRecordsAsDataTableSP(sqlContainer.sql, sqlContainer.sqlParams.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}", sqlContainer.sql, ex.Message));
                }
            }

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
            string TaskId = pageSqlConfig.TaskId;

            if (TaskId == "700101")
            {
                pageTitle = "Outward Clearing - Branch Clearing Item";


            }
            try
            {

                string stmt = "SELECT * FROM tblsearchpageconfig WHERE isenabled = 'Y' And taskid = @taskid  And isfilter = 'Y' order by ordering  ";
                configTable = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@taskid", pageSqlConfig.TaskId) });
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["psql_connection_string"].ConnectionString;
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
                            if (dataField.ValueTextQueryForOption.Contains("@fldbankcode"))
                            {
                                dataField.ValueTextQueryForOption = dataField.ValueTextQueryForOption.Replace("@fldbankcode", currentUserAccount.BankCode);
                            }
                            dataField.keyValueFieldList = GetKeyValueFromSQLAndParams(con, dataField.ValueTextQueryForOption, currentUserAccount.ToDictionary());
                        }
                        if (!string.IsNullOrEmpty(dataField.fieldDefaultValue) & dataField.fieldDefaultValue.StartsWith("sql:"))
                        {
                            if (dataField.fieldType == "Date")
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

            string stmt = "SELECT * FROM tbldashboardconfig WHERE isenabled = 'Y' AND divid = @divid";
            if (!string.IsNullOrEmpty(divId))
            {
                dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@divid", divId) });
                if (dt.Rows.Count > 0)
                {
                    pageSqlConfig.SetViewId(dt.Rows[0]["databaseviewid"].ToString());
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


        private ArrayList GetKeyValueFromSQLAndParams(SqlConnection oSqlConnection, string strSql, Dictionary<string, string> currentUserParams)
        {
            ArrayList resultArray = new ArrayList();

            // strSql = StringUtils.ReplaceParamsWithMap(strSql, params)
            try
            {
                using (SqlCommand cmd = oSqlConnection.CreateCommand())
                {
                    DataTable dataTable = new DataTable();
                    KeyValuePair<string, List<SqlParameter>> holder = SearchPageHelper.GetSqlParameterFromSqlAndCurrentUserAccount(strSql, currentUserParams);

                    cmd.CommandText = holder.Key;
                    //cmd.CommandTimeout = 300;
                    SqlParameter[] pars = holder.Value.ToArray();
                    foreach (SqlParameter a in pars)
                    {
                        cmd.Parameters.Add(a);
                    }
                    using (SqlDataReader sda = cmd.ExecuteReader())
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
            ArrayList resultHeaders = new ArrayList();
            int totalRecord = 0;
            int totalOutstandingRecord = 0;
            int totalPage = 0;
            double grandTotalAmount = 0;
            int grandTotalItem = 0;
            DataTable configTable = configTableModel.dataTable;
            if ((configTable.Rows.Count > 0))
            {
                title = configTable.Rows[0]["pagetitle"].ToString();
                //resultPage.RowActionUrl = configTable.Rows[0]["ResultClickUrl"].ToString();
            }

            foreach (DataRow row in configTable.Rows)
            {
                if ((string.Compare(row["isresult"].ToString(), "Y") == 0))
                {
                    resultHeaders.Add(row["fieldlabel"].ToString());
                }
            }

            if ((resultTable.Rows.Count > 0))
            {
                totalRecord = resultTable.Columns.Contains("TotalRecords") ? Convert.ToInt32(resultTable.Rows[0]["TotalRecords"]) : 0;
                totalOutstandingRecord = resultTable.Columns.Contains("TotalOutstanding") ? Convert.ToInt32(resultTable.Rows[0]["TotalOutstanding"]) : 0;
                totalPage = resultTable.Columns.Contains("TotalPage") ? Convert.ToInt32(resultTable.Rows[0]["TotalPage"]) : 0;
                grandTotalAmount = resultTable.Columns.Contains("GrandTotalAmount") ? Convert.ToDouble(resultTable.Rows[0]["GrandTotalAmount"]) : 0;
                grandTotalItem = resultTable.Columns.Contains("GrandTotalItem") ? Convert.ToInt32(resultTable.Rows[0]["GrandTotalItem"]) : 0;

                if (resultTable.Columns.Contains("TotalRecords")) resultTable.Columns.Remove("TotalRecords");
                if (resultTable.Columns.Contains("TotalOutstanding")) resultTable.Columns.Remove("RowNum");
                if (resultTable.Columns.Contains("TotalPage")) resultTable.Columns.Remove("TotalPage");
                if (resultTable.Columns.Contains("GrandTotalAmount")) resultTable.Columns.Remove("GrandTotalAmount");
                if (resultTable.Columns.Contains("GrandTotalItem")) resultTable.Columns.Remove("GrandTotalItem");
            }

            resultPage.TotalRecordCount = totalRecord;
            resultPage.TotalOutstandingCount = totalOutstandingRecord;
            resultPage.TotalPage = totalPage;
            resultPage.TableData = CombineResultWithConfigData(resultTable, configTable);
            resultPage.ResultTable = resultTable;
            resultPage.TableHeaders = configTable;
            resultPage.PageTitle = title;
            resultPage.CurrentPage = currentPage;
            resultPage.OCSGrandTotalAmount = grandTotalAmount;
            resultPage.OCSGrandTotalItem = grandTotalItem;


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
                            dataField.value = DateUtils.formatDateFromSql(row[item.Ordinal].ToString());
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
                dataField.taskId = row["taskid"].ToString();
                dataField.pageTitle = row["pagetitle"].ToString();

                dataField.databaseViewId = row["databaseviewid"].ToString();

                dataField.fieldId = row["fieldid"].ToString();
                dataField.fieldType = row["fieldtype"].ToString();
                dataField.fieldLabel = row["fieldlabel"].ToString();
                dataField.fieldDefaultValue = row["fielddefaultvalue"].ToString();

                dataField.ValueTextQueryForOption = row["valuetextqueryforoption"].ToString();
                dataField.length = Convert.ToInt32(row["length"]);

                dataField.isResultParameter = row["isresultparam"].ToString();
                dataField.isFilter = row["isfilter"].ToString();
                dataField.isResult = row["isresult"].ToString();
                dataField.isEnabled = row["isenabled"].ToString();
                dataField.isReadOnly = row["isreadonly"].ToString();

                dataField.ordering = (int)row["ordering"];

                resultArray.Add(dataField);
            }
            return resultArray;
        }


        public ConfigTable GetConfigTable(string taskId, string viewId = null)
        {

            List<SqlParameter> sqlParams = new List<SqlParameter>();
            string stmt = "SELECT * FROM tblsearchpageconfig sp WHERE isenabled = 'Y' And taskid = @taskid order by ordering  ";
            sqlParams.Add(new SqlParameter("@taskid", taskId));
            DataTable configTable = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());
            string tableName = "";
            if (!string.IsNullOrEmpty(viewId))
            {
                tableName = viewId;
            }
            else if (configTable.Rows.Count > 0)
            {
                tableName = configTable.Rows[0]["databaseviewid"].ToString();
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
            string stmt = "SELECT * FROM tblsearchpageconfig WHERE isenabled = 'Y' And taskid = @taskid And isfilter = 'Y' order by ordering  LIMIT 1";

            DataTable dt = dbContext.GetRecordsAsDataTable(stmt,
                new[] { new SqlParameter("@taskid", pageSqlConfig.TaskId)
                    //,new SqlParameter("@databaseViewId", pageSqlConfig.ViewOrTableName)
                });

            if (dt.Rows.Count > 0)
            {
                searchPageView.PageTitle = dt.Rows[0]["pagetitle"].ToString();
            }
            return searchPageView;

        }

    }
}