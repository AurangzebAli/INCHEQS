using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer.RPA;
using System.Threading.Tasks;
using INCHEQS.Areas.ICS.ViewModels;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using INCHEQS.Common;

namespace INCHEQS.Models.SearchPageConfig.RPA
{
    public class PageConfigDaoRPA : IPageConfigDaoRPA
    {
        private readonly RPADbContext dbContext;
        public PageConfigDaoRPA(RPADbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ResultPageViewModel> getResultListFromDatabaseViewAsync(PageSqlConfig sqlPageConfig, FormCollection collection)
        {
            return await Task.Run(() => getResultListFromDatabaseView(sqlPageConfig, collection));
        }

        public ResultPageViewModel getResultListFromDatabaseView(
          PageSqlConfig sqlPageConfig, FormCollection collection)
        {
            ResultPageViewModel resultPage = new ResultPageViewModel();


            string currentPage = collection["page"];
            if (string.IsNullOrEmpty(currentPage))
            {
                currentPage = "1";
            }

            ConfigTable configTable = GetConfigTable(sqlPageConfig.TaskId);
            DataTable resultTable = GetResultTable(sqlPageConfig, configTable, collection);

            resultPage = WrapResultPage(resultTable, configTable, Convert.ToInt32(currentPage));

            return resultPage;
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
                totalRecord = Convert.ToInt32(resultTable.Rows[0]["TotalRecords"]);
                totalOutstandingRecord = Convert.ToInt32(resultTable.Rows[0]["TotalOutstanding"]);
                totalPage = Convert.ToInt32(resultTable.Rows[0]["TotalPage"]);
                grandTotalAmount = Convert.ToDouble(resultTable.Rows[0]["GrandTotalAmount"]);
                totalImportItem = Convert.ToInt32(resultTable.Rows[0]["TotalImportItem"]);
                resultTable.Columns.Remove("TotalRecords");
                resultTable.Columns.Remove("RowNum");
                resultTable.Columns.Remove("TotalPage");
                resultTable.Columns.Remove("GrandTotalAmount");
                resultTable.Columns.Remove("TotalImportItem");
            }

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
            resultPage.Taskid = sTaskid;

            return resultPage;
        }

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

    }
}