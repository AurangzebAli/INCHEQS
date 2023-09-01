
using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Security;
//using INCHEQS.Security.Account;
using INCHEQS.Common;
using System.Globalization;
using INCHEQS.DataAccessLayer;

namespace INCHEQS.Models.SearchPageConfig
{
    public class SearchPageHelper
    {

        private static Int32 pageSize = 10;

        public class SqlDetails
        {
            public List<SqlParameter> sqlParams { get; set; }
            public string sql { get; set; }
            public string queriesAsSqlString { get; set; }
            public string conditionAsSqlString { get; set; }
            public int pageSize { get; set; }
            public int currentPage { get; set; }
            public string paginationSql { get; set; }
            public string orderBySql { get; set; }
            public string tableName { get; set; }
            public SqlDetails() { }

            public SqlDetails(string sql, string tableName, SqlParameter[] sqlParamIn)
            {
                this.sql = sql;
                this.tableName = tableName;
                this.sqlParams = reinitializeParam(sqlParamIn);
            }

            public SqlDetails(string sql, string tableName, string queriesAsSqlString, string conditionAsSqlString, SqlParameter[] sqlParamIn, string orderBy)
            {
                this.sql = sql;
                this.tableName = tableName;
                this.queriesAsSqlString = queriesAsSqlString;
                this.conditionAsSqlString = conditionAsSqlString;
                this.sqlParams = reinitializeParam(sqlParamIn);
                this.orderBySql = orderBy;
            }

            public SqlDetails(string sql, string tableName, string queriesAsSqlString, string conditionAsSqlString, SqlParameter[] sqlParamIn, int pageSize,
                int currentPage, string paginationSql)
            {

                this.sql = sql;
                this.tableName = tableName;
                this.sqlParams = reinitializeParam(sqlParamIn);
                this.queriesAsSqlString = queriesAsSqlString;
                this.conditionAsSqlString = conditionAsSqlString;
                this.paginationSql = paginationSql;
            }

            public string ToSqlSelectAll()
            {
                string condition = string.IsNullOrEmpty(this.conditionAsSqlString) ? "" : string.Format(" WHERE {0} ", this.conditionAsSqlString);
                string orderBy = string.IsNullOrEmpty(this.orderBySql) ? "" : string.Format(" ORDER BY {0} ", this.orderBySql);
                return string.Format("SELECT * FROM {0} {1} {2}", this.tableName, condition, orderBy);
            }

            public string ToSqlSelectTop1All()
            {
                string condition = string.IsNullOrEmpty(this.conditionAsSqlString) ? "" : string.Format(" WHERE {0} ", this.conditionAsSqlString);
                string orderBy = string.IsNullOrEmpty(this.orderBySql) ? "" : string.Format(" ORDER BY {0} ", this.orderBySql);
                return string.Format("SELECT top 1 * FROM {0} {1} {2}", this.tableName, condition, orderBy);
            }

            public string ToUserPerformance()
            {
                string condition = string.IsNullOrEmpty(this.conditionAsSqlString) ? "" : string.Format(" WHERE {0} ", this.conditionAsSqlString);
                string orderBy = string.IsNullOrEmpty(this.orderBySql) ? "" : string.Format(" ORDER BY {0} ", this.orderBySql);
                //string groupby = " group by fldUserId, fldUserDesc "; 
                string groupby = " group by fldUserDesc "; /*comment by Ali*/
                return string.Format("SELECT getdate() as fldClearDate, SUM(UserTot) as UserTot, fldUserDesc, Sum(Attendance) as Attendance, SUM(UserTot)/SUM(Attendance) as Average FROM {0} {1} {2} {3}", this.tableName, condition, groupby, orderBy);
            }
        }
        public static SqlDetails ConstructSqlForNextAndPreviousItemNOLOCK(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            //SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);
            string DataAction;

            if (collection["DataAction"] is null)
            {
                DataAction = "";
            }
            else
            {
                DataAction = collection["DataAction"];
            }
  
            string sql;
            string taskId = pageConfig.TaskId.ToString().Trim();
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = sqlDetails.conditionAsSqlString;
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            /*if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }*/

            /*if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }*/

            if (DataAction == "ChequeVerificationPage")
            {
                if (taskId == "306230" || taskId == "306520" || taskId == "306530" || taskId == "306540" || taskId == "306550" || taskId == "309100")
                {
                    tableOrViewName = "View_Verification";
                    //conditionStr = "";
                }
                // xx start 20210708
                if (taskId == "318190" || taskId == "318210" || taskId == "318220" || taskId == "318290" || taskId == "318410")
                {
                    tableOrViewName = "View_InwardItem";
                }
                // xx end 20210708
                if (taskId == "306230" || taskId == "306220")
                {
                    //conditionStr = "";
                    tableOrViewName = "View_Verification";
                }

                if (taskId == "306520" || taskId == "306530" || taskId == "306540" || taskId == "306550")
                {
                    if (taskId != "306520")
                    {
                        tableOrViewName = sqlDetails.tableName;

                        if (conditionStr == null || conditionStr == "")
                        {
                            conditionStr = " ISNULL(fldApprovalUserId, '') != '' ";
                        }
                        else
                        {
                            conditionStr = conditionStr + " AND ISNULL(fldApprovalUserId, '') != '' ";
                        }
                    }
                    

                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT  rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3}) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum - 1" +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }
                else if (taskId == "306510")
                {
                    tableOrViewName = sqlDetails.tableName;

                    if (conditionStr == null || conditionStr == "")
                    {
                        conditionStr = string.Format(" ISNULL(fldInvolvedUserId, '') = {0} ", currentUser.UserId);
                    }
                    else
                    {
                        conditionStr = conditionStr + string.Format(" AND ISNULL(fldInvolvedUserId, '') = {0} ", currentUser.UserId);
                    }

                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT  rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} ) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} ) nex ON nex.rownum = CTE.rownum + (SELECT COUNT({0}) FROM {2} WHERE {0} = @fieldValue group by fldInvolvedUserId ) ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3}) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum + (SELECT COUNT({0}) FROM {2} WHERE {3} AND {0} = @fieldValue group by fldInvolvedUserId) WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }
                /*else if (taskId == "308170" || taskId == "308180" || taskId == "308190" || taskId == "308200")
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord,(SELECT COUNT(*) FROM {2} ) as TotalUnverified, * FROM {2} ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }*/
                else if (taskId == "308110")
                {
                    string IslamicBranchCode = currentUser.BranchCodes[1].ToString().Trim();
                    string ConventionalBranchCode = currentUser.BranchCodes[0].ToString().Trim();

                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6})) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6})) nex ON nex.rownum = CTE.rownum ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3} AND fldIssueBankBranch in ({5},{6}) ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3} AND fldIssueBankBranch in ({5},{6}) ) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3} AND fldIssueBankBranch in ({5},{6})) nex ON nex.rownum = CTE.rownum WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                }
                else
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }

                }
        
            }
            else
            {
                if (taskId == "306520" || taskId == "306530" || taskId == "306540" || taskId == "306550")
                {
                    if (taskId != "306520")
                    {
                        if (conditionStr == null || conditionStr == "")
                        {
                            conditionStr = " ISNULL(fldApprovalUserId, '') != '' ";
                        }
                        else
                        {
                            conditionStr = conditionStr + " AND ISNULL(fldApprovalUserId, '') != '' ";
                        }
                    }

                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                        "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                        "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3} ) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3} ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }
                else if (taskId == "306510")
                {
                    tableOrViewName = sqlDetails.tableName;

                    if (conditionStr == null || conditionStr == "")
                    {
                        conditionStr = string.Format(" ISNULL(fldInvolvedUserId, '') = {0} ", currentUser.UserId);
                    }
                    else
                    {
                        conditionStr = conditionStr + string.Format(" AND ISNULL(fldInvolvedUserId, '') = {0} ", currentUser.UserId);
                    }

                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT  rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} ) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} ) nex ON nex.rownum = CTE.rownum + (SELECT COUNT({0}) FROM {2} WHERE {0} = @fieldValue group by fldInvolvedUserId ) ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3}) As CTE " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT  rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum + (SELECT COUNT({0}) FROM {2} WHERE {3} AND {0} = @fieldValue group by fldInvolvedUserId ) WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }
                /*else if (taskId == "308170" || taskId == "308180" || taskId == "308190" || taskId == "308200")
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord,(SELECT COUNT(*) FROM {2} ) as TotalUnverified, * FROM {2} ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }*/
                else
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord,(SELECT COUNT(*) FROM {2} ) as TotalUnverified, * FROM {2} ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} WHERE {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} WHERE {3}) nex ON nex.rownum = CTE.rownum WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                    }
                }
            }

            if (!string.IsNullOrEmpty(fieldValue))
            {
                if (conditionStr.Trim() != "" && conditionStr.Trim() != null)
                {
                    
                        sql = string.Format("{0} AND CTE.{1} = @fieldValue ", sql, fieldId);
                        sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                    
                    
                }
                else
                {
                   
                        sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                        sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                    
                    
                }

            }

            /*if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }*/

            //sqlParameter.Add(new SqlParameter("@fldIssueBankCode", currentUser.BankCode));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousItemBranchNOLOCK(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            //SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);
            DataTable configTable = configTableModel.dataTable;
            ApplicationDbContext dbContext = new ApplicationDbContext();
            DataTable userBranch = null;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);
            string DataAction;

            if (collection["DataAction"] is null)
            {
                DataAction = "";
            }
            else
            {
                DataAction = collection["DataAction"];
            }

            string sql;
            string taskId = pageConfig.TaskId.ToString().Trim();
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;
            string IslamicBranchCode = currentUser.BranchCodes[1].ToString().Trim();
            string ConventionalBranchCode = currentUser.BranchCodes[0].ToString().Trim();

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = sqlDetails.conditionAsSqlString;

            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }

            /*userBranch = dbContext.GetRecordsAsDataTable("SELECT CONCAT('47',fldBranchCode3) as Islamic, CONCAT('32',fldBranchCode) as Conventional from tblUserMaster where fldUserId = @currentUserId",
                new[] {
                new SqlParameter("@currentUserId", CurrentUser.Account.UserId)
            });

            if (userBranch.Rows.Count > 0)
            {
                DataRow row = userBranch.Rows[0];
                IslamicBranchCode = row["Islamic"].ToString();
                ConventionalBranchCode = row["Conventional"].ToString();

            }
            else
            {
                IslamicBranchCode = "";
                ConventionalBranchCode = "";
            }*/

            /*if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }*/

            /*if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }*/

            if (taskId == "308190" || taskId == "308200")
            {
                string Verify;
                if (collection["NextVerifyBranch"] is null)
                {
                    Verify = "";
                }
                else
                {
                    Verify = collection["NextVerifyBranch"];
                }

                if (Verify == "Verify")
                {
                    /*if (conditionStr != null && conditionStr != "")
                    {
                        conditionStr = conditionStr + " And isnull(fldAlert,'')!='1' ";
                    }
                    else
                    {
                        conditionStr = "isnull(fldAlert,'')!='1' ";
                    }*/
                }
                
            }

            if (DataAction == "ChequeVerificationPage")
            {
                /*if (taskId == "306230" || taskId == "306520" || taskId == "306530" || taskId == "306540" || taskId == "306550" || taskId == "309100")
                {
                    tableOrViewName = "View_AppInwardItem";
                    conditionStr = "";
                }*/

                /*if (taskId == "306530" || taskId == "306540" || taskId == "306550")
                {
                    tableOrViewName = sqlDetails.tableName;

                    sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                    "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                    "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                    "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                }
                else*/ 
                if (taskId == "308170" || taskId == "308180" || taskId == "308190" || taskId == "308200")
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) prev ON prev.rownum = CTE.rownum - 1 " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) nex ON nex.rownum = CTE.rownum + 1", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                }
                else
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) ) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) nex ON nex.rownum = CTE.rownum ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 1 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) nex ON nex.rownum = CTE.rownum WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }

                }

            }
            else
            {
                /*if (taskId == "306530" || taskId == "306540" || taskId == "306550")
                {
                    sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                    "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord, 1 as TotalUnverified, * FROM {2} ) As CTE " +
                    "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) prev ON prev.rownum = CTE.rownum - 1 " +
                    "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                }
                else*/
                if (taskId == "308170" || taskId == "308180" || taskId == "308190" || taskId == "308200")
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) prev ON prev.rownum = CTE.rownum - 1 " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) nex ON nex.rownum = CTE.rownum + 1", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), 3 as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) nex ON nex.rownum = CTE.rownum + 1 WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                }
                else
                {
                    if (conditionStr == null || conditionStr == "")
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                        "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) ) As CTE " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) prev ON prev.rownum = CTE.rownum " +
                        "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) ) nex ON nex.rownum = CTE.rownum ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                    else
                    {
                        sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                       "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord, 1 as TotalUnverified, * FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) As CTE " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) prev ON prev.rownum = CTE.rownum " +
                       "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2} where fldIssueBankBranch in ({5},{6}) AND {3}) nex ON nex.rownum = CTE.rownum WHERE {3}", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr, IslamicBranchCode, ConventionalBranchCode);
                    }
                }
            }

            if (!string.IsNullOrEmpty(fieldValue))
            {
                if (conditionStr.Trim() != "" && conditionStr.Trim() != null)
                {
                    sql = string.Format("{0} AND CTE.{1} = @fieldValue AND fldIssueBankBranch in ({2},{3}) ", sql, fieldId, IslamicBranchCode, ConventionalBranchCode);
                    sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                }
                else
                {
                    sql = string.Format("{0} WHERE CTE.{1} = @fieldValue AND fldIssueBankBranch in ({2},{3}) ", sql, fieldId, IslamicBranchCode, ConventionalBranchCode);
                    sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                }

            }

            /*if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }*/

            //sqlParameter.Add(new SqlParameter("@fldIssueBankCode", currentUser.BankCode));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructPaginatedResultQueryWithFilter(PageSqlConfig sqlPageConfig,
            ConfigTable configTable,
            FormCollection collection)
        {
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(sqlPageConfig, configTable, collection);

            string tableOrViewName = sqlDetails.tableName;
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;

            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }

            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;

            string page = DatabaseUtils.SanitizeString(collection["page"]);
            page = page == null ? "0" : page;

            StringBuilder selectSql = new StringBuilder();
            selectSql.AppendFormat(" SELECT ROW_NUMBER() OVER (ORDER BY {1}) AS RowNum, {2} , ", pageSize, orderBy, fieldsSql);

            //Add Grand Total Amount if contain amount result or item status
            var amountField = "";
            var recordField = "";
            var fileField = "";
            var itemStatusField = "";
            var importItem = "";
            string[] splittedField = fieldsSql.Replace(" ", "").Split(',');
            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("amount") != -1)
                {
                    amountField = field;
                }
                else if (field.ToLower().IndexOf("itemstatus") != -1)
                {
                    itemStatusField = field;
                }
                else if (field.ToLower().IndexOf("countimportitem") != -1 || field.ToLower().IndexOf("totalitem") != -1)
                {
                    importItem = field;
                }
            }

            //Add total grand amount if exist
            if (!amountField.Equals(""))
            {
                selectSql.AppendFormat(" sum({0}) OVER() As GrandTotalAmount ,", amountField);
            }
            else
            {
                selectSql.Append(" '0' As GrandTotalAmount ,");
            }

            //Add total outstanding if exist
            if (!itemStatusField.Equals("") && !filtersSql.Equals(""))
            {
                if (sqlPageConfig.TaskId == "301110")
                {

                    selectSql.AppendFormat(" (select Count(*) FROM [{0}] WHERE fldItemStatus LIKE '%Outstanding%' OR fldItemStatus = 'Pending MICR Repair' AND {1} ) As TotalOutstanding ,", tableOrViewName, filtersSql);
                }
                else
                { 
                selectSql.AppendFormat(" (select Count(*) FROM [{0}] WHERE fldItemStatus = 'Outstanding' AND {1} ) As TotalOutstanding ,", tableOrViewName, filtersSql);
            }
            }
            else if (!itemStatusField.Equals(""))
            {
                if (sqlPageConfig.TaskId == "301110")
                {

                    selectSql.AppendFormat(" (select Count(*) FROM [{0}] WHERE fldItemStatus LIKE '%Outstanding%' OR fldItemStatus = 'Pending MICR Repair' AND {1} ) As TotalOutstanding ,", tableOrViewName);
                }
                else
                {
                selectSql.AppendFormat(" (select Count(*) FROM [{0}] WHERE fldItemStatus = 'Outstanding') As TotalOutstanding ,", tableOrViewName);
            }
            }
            else
            {
                selectSql.Append(" '0' As TotalOutstanding ,");
            }

            //Add total import item if exist
            if (!importItem.Equals(""))
            {
                selectSql.AppendFormat(" sum({0}) OVER() As TotalImportItem ,", importItem);
            }
            else
            {
                selectSql.Append(" '0' As TotalImportItem ,");
            }


            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("record") != -1)
                {
                    recordField = field;
                }
            }
             if (!recordField.Equals(""))
            {
                selectSql.AppendFormat(" sum({0}) OVER() As GrandTotalImportItem ,", recordField);
            }
            else
            {
                selectSql.Append(" '0' As GrandTotalImportItem ,");
            }
            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("file") != -1)
                {
                    fileField = field;
                }
            }
            if (!fileField.Equals(""))
            {
                selectSql.AppendFormat(" count({0}) OVER() As GrandTotalFile ,", fileField);
            }
            else
            {
                selectSql.Append(" '0' As GrandTotalFile ,");
            }

            //End

            if (sqlPageConfig.TaskId == "205132" || sqlPageConfig.TaskId == "306011")
            {
                selectSql.AppendFormat(" Count(*) OVER() As TotalRecords , ((COUNT(*)OVER()+{0}-1)/{0}) AS TotalPage FROM [{1}] ", "50", tableOrViewName);

            }
            else
            {
            selectSql.AppendFormat(" Count(*) OVER() As TotalRecords , ((COUNT(*)OVER()+{0}-1)/{0}) AS TotalPage FROM [{1}] ", pageSize, tableOrViewName);
            }

            if (!string.IsNullOrEmpty(filtersSql))
            {
                selectSql.AppendFormat("WHERE {0} ", filtersSql);
            }

            if (sqlPageConfig.TaskId == "205132" || sqlPageConfig.TaskId == "306011")
            {
                string paginationString = string.Format("SELECT  Top({0}) * FROM ( @selectSql ) AS RowConstrainedResult ORDER BY RowNum ", "50", page);
                string paginationSql = paginationString.Replace("@selectSql", selectSql.ToString());
                return new SqlDetails(paginationSql.ToString(), tableOrViewName,
                 fieldsSql.ToString(),
                filtersSql.ToString(), sqlParameter.ToArray(),
                pageSize, Convert.ToInt32(page),
                paginationString);
            }
            else if (sqlPageConfig.TaskId == "306280")
            {
                string paginationString = string.Format("SELECT * FROM ( @selectSql ) AS RowConstrainedResult ORDER BY RowNum ", pageSize, page);
                string paginationSql = paginationString.Replace("@selectSql", selectSql.ToString());

                return new SqlDetails(paginationSql.ToString(), tableOrViewName,
                     fieldsSql.ToString(),
                    filtersSql.ToString(), sqlParameter.ToArray(),
                    pageSize, Convert.ToInt32(page),
                    paginationString);
            }
            else
            {
            string paginationString = string.Format("SELECT  Top({0}) * FROM ( @selectSql ) AS RowConstrainedResult WHERE RowNum > (({1}-1)*{0}) ORDER BY RowNum ", pageSize, page);
            string paginationSql = paginationString.Replace("@selectSql", selectSql.ToString());

            return new SqlDetails(paginationSql.ToString(), tableOrViewName,
                 fieldsSql.ToString(),
                filtersSql.ToString(), sqlParameter.ToArray(),
                pageSize, Convert.ToInt32(page),
                paginationString);
            }

        }

        public static SqlDetails ConstructPaginatedResultQueryWithFilterNew(PageSqlConfig sqlPageConfig,
            ConfigTable configTable, FormCollection collection, Dictionary<string, string> dicCurrentUser)
        {

            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(sqlPageConfig, configTable, collection);

            string tableOrViewName = sqlDetails.tableName;
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;
            string fldcleardate = "";
            string fldbankcode = "";
            string flduserabb = "";

            if (dicCurrentUser != null)
            {
                fldbankcode = dicCurrentUser["currentUserBankCode"].ToString();
                flduserabb = dicCurrentUser["currentUserAbbr"].ToString();
            }
            List<SqlParameter> SqlParameterCondition = sqlDetails.sqlParams;

            string page = DatabaseUtils.SanitizeString(collection["page"]);
            page = page == null ? "0" : page;

            if (SqlParameterCondition.Count != 0)
            {
                foreach (var sqlParam in SqlParameterCondition)
                {
                    string a = sqlParam.Value.ToString();
                    string b = sqlParam.ParameterName.ToString();
                    //filtersSql=filtersSql.Replace(b,a);
                    if (b == "@fldClearDate")
                    {
                        fldcleardate = a;
                        a = "'" + a + "'";
                    }
                    if (b == "@fldItemStatus")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserAbbr")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserBankCode")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@fldCycleCode")
                    {
                        a = "'" + a + "'";
                    }
                    filtersSql = filtersSql.Replace(b, a);

                }
                filtersSql = filtersSql.Replace("%' + ", "%");
                filtersSql = filtersSql.Replace(" + '%", "%");
                filtersSql = " AND " + filtersSql;
            }

            //Add Grand Total Amount if contain amount result or item status
            var amountField = "";
            var itemStatusField = "";
            var importItem = "";
            string[] splittedField = fieldsSql.Replace(" ", "").Split(',');
            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("amount") != -1)
                {
                    amountField = field;
                }
                else if (field.ToLower().IndexOf("itemstatus") != -1)
                {
                    itemStatusField = field;
                }
                else if (field.ToLower().IndexOf("countimportitem") != -1 || field.ToLower().IndexOf("totalitem") != -1)
                {
                    importItem = field;
                }
            }


            //Add total grand amount if exist
            if (!amountField.Equals(""))
            {
                amountField = amountField;
            }
            else
            {
                amountField = "0";
            }

            List<SqlParameter> SqlParameter = new List<SqlParameter>();
            SqlParameter.Add(new SqlParameter("@vchTaskId", sqlPageConfig.TaskId));
            SqlParameter.Add(new SqlParameter("@condition", filtersSql));
            SqlParameter.Add(new SqlParameter("@orderBY", sqlDetails.orderBySql));
            SqlParameter.Add(new SqlParameter("@fldClearDate", fldcleardate));
            SqlParameter.Add(new SqlParameter("@currentUserAbbr", flduserabb));
            SqlParameter.Add(new SqlParameter("@currentUserBankCode", fldbankcode));
            SqlParameter.Add(new SqlParameter("@select", sqlDetails.queriesAsSqlString));
            SqlParameter.Add(new SqlParameter("@amount", amountField));
            SqlParameter.Add(new SqlParameter("@page", page));
            // SqlParameter = SqlParameter.Add(new SqlParameter("",""));

            return new SqlDetails(sqlPageConfig.StoreProcedure, "", SqlParameter.ToArray());

        }

        public static SqlDetails ConstructPaginatedResultQueryWithFilterClearing(PageSqlConfig sqlPageConfig,
            ConfigTable configTable, FormCollection collection, Dictionary<string, string> dicCurrentUser)
        {

            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(sqlPageConfig, configTable, collection);

            string tableOrViewName = sqlDetails.tableName;
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;
            string fldcleardate = "";
            string fldbankcode = "";
            string flduserabb = "";
            string fldbranchCodes = "";

            if (dicCurrentUser != null)
            {
                fldbankcode = dicCurrentUser["currentUserBankCode"].ToString();
                flduserabb = dicCurrentUser["currentUserAbbr"].ToString();
            }
            List<SqlParameter> sqlParameterCondition = sqlDetails.sqlParams;

            string page = DatabaseUtils.SanitizeString(collection["page"]);
            page = page == null ? "0" : page;

            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            if (sqlParameterCondition.Count != 0)
            {
                foreach (var sqlParam in sqlParameterCondition)
                {
                    sqlParameter.Add(new SqlParameter(sqlParam.ParameterName.ToString(), sqlParam.Value.ToString()));
                }
            }

            sqlParameter.Add(new SqlParameter("@orderBY", sqlDetails.orderBySql));
            sqlParameter.Add(new SqlParameter("@page", page));

            return new SqlDetails(sqlPageConfig.StoreProcedure, "", sqlParameter.ToArray());

        }

        public static SqlDetails ConstructPaginatedResultQueryWithFilterBranch(PageSqlConfig sqlPageConfig,
            ConfigTable configTable, FormCollection collection, Dictionary<string, string> dicCurrentUser)
        {

            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(sqlPageConfig, configTable, collection);

            string tableOrViewName = sqlDetails.tableName;
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;
            string fldcleardate = "";
            string fldbankcode = "";
            string flduserabb = "";
            string [] flbranchcode ;
            string iBranchCode = "";
            string convBranchCode = "";
            string iStateCode = "";
            if (dicCurrentUser != null)
            {
                fldbankcode = dicCurrentUser["currentUserBankCode"].ToString();
                flduserabb = dicCurrentUser["currentUserAbbr"].ToString();
                flbranchcode = dicCurrentUser["currentUserBranchCodes"].ToString().Trim().Split(',');
                convBranchCode = flbranchcode[0].ToString().Trim();
                iBranchCode = flbranchcode[1].ToString().Trim();
                iStateCode = dicCurrentUser["stateCode"].ToString();

            }
            List<SqlParameter> sqlParameterCondition = sqlDetails.sqlParams;

            string page = DatabaseUtils.SanitizeString(collection["page"]);
            page = page == null ? "0" : page;

            if (sqlParameterCondition.Count != 0)
            {
                foreach (var sqlParam in sqlParameterCondition)
                {
                    string a = sqlParam.Value.ToString();
                    string b = sqlParam.ParameterName.ToString();
                    //filtersSql=filtersSql.Replace(b,a);
                    if (b == "@fldClearDate")
                    {
                        fldcleardate = a;
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserAbbr")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserBankCode")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@fldrejectstatus1")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@fldItemStatus")
                    {
                        a = "'" + a + "'";
                    }

                    filtersSql = filtersSql.Replace(b, a);

                }
                filtersSql = filtersSql.Replace("%' + ", "%");
                filtersSql = filtersSql.Replace(" + '%", "%");
                filtersSql = " AND " + filtersSql;
            }

            //Add Grand Total Amount if contain amount result or item status
            var amountField = "";
            var itemStatusField = "";
            var importItem = "";
            string[] splittedField = fieldsSql.Replace(" ", "").Split(',');
            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("amount") != -1)
                {
                    amountField = field;
                }
                else if (field.ToLower().IndexOf("itemstatus") != -1)
                {
                    itemStatusField = field;
                }
                else if (field.ToLower().IndexOf("countimportitem") != -1 || field.ToLower().IndexOf("totalitem") != -1)
                {
                    importItem = field;
                }
            }


            //Add total grand amount if exist
            if (!amountField.Equals(""))
            {
                amountField = amountField;
            }
            else
            {
                amountField = "0";
            }

            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@vchTaskId", sqlPageConfig.TaskId));
            sqlParameter.Add(new SqlParameter("@condition", filtersSql));
            sqlParameter.Add(new SqlParameter("@orderBY", sqlDetails.orderBySql));
            sqlParameter.Add(new SqlParameter("@fldClearDate", fldcleardate));
            sqlParameter.Add(new SqlParameter("@currentUserAbbr", flduserabb));
            sqlParameter.Add(new SqlParameter("@currentUserBankCode", fldbankcode));
            sqlParameter.Add(new SqlParameter("@branchcode", ""));
            sqlParameter.Add(new SqlParameter("@iBranchCode", iBranchCode));
            sqlParameter.Add(new SqlParameter("@conBranchCode ", convBranchCode));
            sqlParameter.Add(new SqlParameter("@select", sqlDetails.queriesAsSqlString));
            sqlParameter.Add(new SqlParameter("@amount", amountField));
            sqlParameter.Add(new SqlParameter("@page", page));
            sqlParameter.Add(new SqlParameter("@stateCode", iStateCode));
            // sqlParameter = sqlParameter.Add(new SqlParameter("",""));

            return new SqlDetails(sqlPageConfig.StoreProcedure, "", sqlParameter.ToArray());

        }


        public static SqlDetails ConstructPaginatedResultQueryWithFilterReviewMine(PageSqlConfig sqlPageConfig,
    ConfigTable configTable, FormCollection collection, Dictionary<string, string> dicCurrentUser)
        {

            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(sqlPageConfig, configTable, collection);

            string tableOrViewName = sqlDetails.tableName;
            string filtersSql = sqlDetails.conditionAsSqlString;
            string fieldsSql = sqlDetails.queriesAsSqlString;
            string fldcleardate = "";
            //string fldbankcode = dicCurrentUser["currentUserBankCode"].ToString();
            //string flduserabb = dicCurrentUser["currentUserAbbr"].ToString();
            string userid = "";//dicCurrentUser["currentUserId"].ToString();

            string page = DatabaseUtils.SanitizeString(collection["page"]);
            page = page == null ? "0" : page;

            List<SqlParameter> sqlParameterCondition = sqlDetails.sqlParams;
            if (sqlParameterCondition.Count != 0)
            {
                foreach (var sqlParam in sqlParameterCondition)
                {
                    string a = sqlParam.Value.ToString();
                    string b = sqlParam.ParameterName.ToString();
                    //filtersSql=filtersSql.Replace(b,a);
                    if (b == "@fldClearDate")
                    {
                        fldcleardate = a;
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserId")
                    {
                        //a = "'" + a + "'";
                        userid = a;
                        filtersSql = filtersSql.Replace(b, "");
                        filtersSql = filtersSql.Replace("AND fldInvolvedUserId=", "");
                    }
                    if (b == "@currentUserBankCode")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@fldrejectstatus1")
                    {
                        a = "'" + a + "'";
                    }

                    filtersSql = filtersSql.Replace(b, a);

                }
                filtersSql = filtersSql.Replace("%' + ", "%");
                filtersSql = filtersSql.Replace(" + '%", "%");
                filtersSql = " AND " + filtersSql;
            }

            //Add Grand Total Amount if contain amount result or item status
            var amountField = "";
            var itemStatusField = "";
            var importItem = "";
            string[] splittedField = fieldsSql.Replace(" ", "").Split(',');
            foreach (var field in splittedField)
            {
                if (field.ToLower().IndexOf("amount") != -1)
                {
                    amountField = field;
                }
                else if (field.ToLower().IndexOf("itemstatus") != -1)
                {
                    itemStatusField = field;
                }
                else if (field.ToLower().IndexOf("countimportitem") != -1 || field.ToLower().IndexOf("totalitem") != -1)
                {
                    importItem = field;
                }
            }


            //Add total grand amount if exist
            if (!amountField.Equals(""))
            {
                amountField = amountField;
            }
            else
            {
                amountField = "0";
            }

            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            sqlParameter.Add(new SqlParameter("@vchTaskId", sqlPageConfig.TaskId));
            sqlParameter.Add(new SqlParameter("@condition", filtersSql));
            sqlParameter.Add(new SqlParameter("@orderBY", sqlDetails.orderBySql));
            sqlParameter.Add(new SqlParameter("@fldClearDate", fldcleardate));
            //sqlParameter.Add(new SqlParameter("@currentUserAbbr", flduserabb));
            //sqlParameter.Add(new SqlParameter("@currentUserBankCode", fldbankcode));
            sqlParameter.Add(new SqlParameter("@select", sqlDetails.queriesAsSqlString));
            sqlParameter.Add(new SqlParameter("@amount", amountField));
            sqlParameter.Add(new SqlParameter("@currentUserId", userid));
            sqlParameter.Add(new SqlParameter("@page", page));

            return new SqlDetails("spcgReviewMine", "", sqlParameter.ToArray());

        }
        public static SqlDetails ConstructSqlForNextAndPreviousItemOCS(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }
            string sql = "";
            if (pageConfig.TaskId == "311100" || pageConfig.TaskId == "311101" || pageConfig.TaskId == "311113" || pageConfig.TaskId == "322200")
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldcapturingbranch in ( select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId = @fldassigneduserid ) and fldlockuser = @fldassigneduserid {4} AND COALESCE(fldamount, 0) = 0 AND (COALESCE(fldreasoncode, '') = '' OR fldreasoncode <> '430') AND datediff(d,fldCapturingDate,CONVERT(date,@fldcapturingdate,103)) = 0 ) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
                
            }
            else if (pageConfig.TaskId == "999100" || pageConfig.TaskId == "999101")
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0) = 0 AND (COALESCE(fldreasoncode, '') = '' OR fldreasoncode <> '430' " +
                "and PATINDEX('%?%', fldCheckDigit) <> 0 OR PATINDEX('%?%', fldSerial) <> 0 OR PATINDEX('%?%',fldBankCode) <> 0 OR PATINDEX('%?%',fldStateCode) <> 0 OR PATINDEX('%?%', fldBranchCode) <> 0 OR PATINDEX('%?%', fldIssuerAccNo) <> 0 )) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0) = 0 AND (COALESCE(fldreasoncode, '') = '' OR fldreasoncode <> '430'" +
                "and PATINDEX('%?%', fldCheckDigit) <> 0 OR PATINDEX('%?%', fldSerial) <> 0 OR PATINDEX('%?%',fldBankCode) <> 0 OR PATINDEX('%?%',fldStateCode) <> 0 OR PATINDEX('%?%', fldBranchCode) <> 0 OR PATINDEX('%?%', fldIssuerAccNo) <> 0)) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0) = 0 AND (COALESCE(fldreasoncode, '') = '' OR fldreasoncode <> '430'" +
                "and PATINDEX('%?%', fldCheckDigit) <> 0 OR PATINDEX('%?%', fldSerial) <> 0 OR PATINDEX('%?%',fldBankCode) <> 0 OR PATINDEX('%?%',fldStateCode) <> 0 OR PATINDEX('%?%', fldBranchCode) <> 0 OR PATINDEX('%?%', fldIssuerAccNo) <> 0)) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }
            else if (pageConfig.TaskId == "999960" || pageConfig.TaskId == "999961")
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }
            else if (pageConfig.TaskId == "311117")
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
               "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4} and ISNULL(fldBalancedStatus,'') = '2') as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) As CTE " +
               "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) prev ON prev.rownum = CTE.rownum - 1 " +
               "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }
            else
            {
                 sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }
            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForItemOCS(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string sql = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            if (pageConfig.TaskId ==  "322200" || pageConfig.TaskId == "311100")
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4} AND COALESCE(fldamount, 0) = 0 AND (COALESCE(fldreasoncode, '') = '' OR fldreasoncode <> '430')) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }
            else
            {
                sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            }

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue",(fieldValue)));
            }
            //sql += "TOP ({1}) ";
            sqlParameter.Add(new SqlParameter("@fldassigneduserid",(currentUser.UserId)));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }
        public static SqlDetails ConstructSqlForItemOCSLocked(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} ) as totalunverified, * FROM {2} ) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  ) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2} ) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", Convert.ToDouble(fieldValue)));
            }
            //sql += "LIMIT 1 ";
            sqlParameter.Add(new SqlParameter("@fldassigneduserid", Convert.ToSingle(currentUser.UserId)));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousItemOCSAmountEntry(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) = 0::double precision AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) = 0::double precision AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) = 0::double precision AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR fldreasoncode <> '430'::bpchar)) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue",(fieldValue)));
            }
            sql += "LIMIT 1 ";
            sqlParameter.Add(new SqlParameter("@fldassigneduserid",(currentUser.UserId)));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousItemOCSAccountEntry(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
                "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldlockuser = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR itm.fldreasoncode <> '430'::bpchar)) As CTE " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR itm.fldreasoncode <> '430'::bpchar)) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldlockuser=@fldassigneduserid {3} AND COALESCE(fldamount, 0::double precision) <> 0::double precision AND COALESCE(fldpvaccno, ''::text::character varying)::text = ''::text AND (COALESCE(fldreasoncode, ''::bpchar) = ''::bpchar OR itm.fldreasoncode <> '430'::bpchar)) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue",(fieldValue)));
            }
            sql += "LIMIT 1 ";
            sqlParameter.Add(new SqlParameter("@fldassigneduserid",(currentUser.UserId)));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }


        public static SqlDetails ConstructSqlForNextAndPreviousItem(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" WHERE {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord,(SELECT COUNT(*) FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {4}) as TotalUnverified, * FROM {2} {3} ) As CTE " +
                "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}  {3} {4}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}  {3} {4}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }
            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousItemBranch(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format(" SELECT TOP 1 prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as RowNumber , CTE.TotalRecord , CTE.TotalUnverified ,  CTE.* FROM " +
                "(SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}), COUNT(*) over () as TotalRecord,(SELECT COUNT(*) FROM {2} WHERE fldAssignedUserIdPending = @fldAssignedUserId {4}) as TotalUnverified, * FROM {2}  WHERE fldAssignedUserIdPending=@fldAssignedUserId {3} ) As CTE " +
                "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}  WHERE fldAssignedUserIdPending=@fldAssignedUserId {3} {4}) prev ON prev.rownum = CTE.rownum - 1 " +
                "LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER (ORDER BY {1}) , {0} FROM {2}  WHERE fldAssignedUserIdPending=@fldAssignedUserId {3} {4}) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }
            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousSearchItem(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" WHERE {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" WHERE {0} ", pageConfig.SqlLockCondition);
            }

            //string sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
            //    "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} WHERE fldassigneduserid = @fldassigneduserid {4}) as totalunverified, * FROM {2}  WHERE fldassigneduserid=@fldassigneduserid {3} AND ISNULL(fldamount, 0) = 0 AND (ISNULL(fldrejectcode, '') = ''OR fldrejectcode <> '430')) As CTE " +
            //    "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldassigneduserid=@fldassigneduserid {3} AND ISNULL(fldamount, 0) = 0 AND (ISNULL(fldrejectcode, '') = '' OR fldrejectcode <> '430')) prev ON prev.rownum = CTE.rownum - 1 " +
            //    "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2}  WHERE fldassigneduserid=@fldassigneduserid {3} AND ISNULL(fldamount, 0) = 0 AND (ISNULL(fldrejectcode, '') = ''OR fldrejectcode <> '430')) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            // xx start 20210616
            string sql = string.Format(" SELECT prev.{0} PreviousValue, CTE.{0}, nex.{0} NextValue , CTE.rownum as rownumber , CTE.totalrecord , CTE.totalunverified ,  CTE.* FROM " +
               "(SELECT ROW_NUMBER() OVER(ORDER BY {1}) as rownum, COUNT(*) over () as totalrecord,(SELECT COUNT(*) FROM {2} {4}) as totalunverified, * FROM {2} {3} ) As CTE " +
               "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2} {3} ) prev ON prev.rownum = CTE.rownum - 1 " +
               "LEFT JOIN (SELECT ROW_NUMBER() OVER (ORDER BY {1}) as rownum , {0} FROM {2} {3} ) nex ON nex.rownum = CTE.rownum + 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
            // xx end 20210616
            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }
            sqlParameter.Add(new SqlParameter("@fldIssueBankCode", currentUser.BankCode));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForNextAndPreviousItemNew(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, Dictionary<string, string> dicCurrentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string inwarditemid = "";
            string conditionStr = "";
            string lockConditionStr = "";
            string fldcleardate = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(fieldValue))
            {
                inwarditemid = fieldValue;
            }
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            List<SqlParameter> sqlParameterCondition = sqlDetails.sqlParams;
            if (sqlParameterCondition.Count != 0)
            {
                foreach (var sqlParam in sqlParameterCondition)
                {
                    string a = sqlParam.Value.ToString();
                    string b = sqlParam.ParameterName.ToString();
                    //filtersSql=filtersSql.Replace(b,a);
                    if (b == "@fldClearDate")
                    {
                        fldcleardate = a;
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserAbbr")
                    {
                        a = "'" + a + "'";
                    }
                    if (b == "@currentUserBankCode")
                    {
                        a = "'" + a + "'";
                    }
                    conditionStr = conditionStr.Replace(b, a);

                }
                conditionStr = conditionStr.Replace("%'+", "%");
                conditionStr = conditionStr.Replace("+'%", "%");
                // conditionStr = " AND " + conditionStr;
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", inwarditemid));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", currentUser.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
            sqlParameterNext.Add(new SqlParameter("@orderBy", orderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", conditionStr));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            return new SqlDetails("", "spcgNextInwardItem", sqlParameterNext.ToArray());
        }

        public static SqlDetails ConstructSqlForSearchItem(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" {0} ", sqlDetails.conditionAsSqlString);
            }

            /*if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }*/

            /*string sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, 0 NextValue , 1 as RowNumber , 0 , 0 ,  CTE.* FROM " +
                " {2} As CTE WHERE fldIssueBankCode=@fldIssueBankCode {3}  ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);*/
            
            /* Ali change the previous and next value and get it from db */
            //string sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, 0 NextValue , 1 as RowNumber , 1 as TotalRecord , 1 as TotalUnverified ,   CTE.* FROM " +
            //" {2} As CTE WHERE {3}  ", fieldId, orderBy, tableOrViewName, conditionStr/*, lockConditionStr*/);

            string sql = string.Format(" SELECT TOP 1  CTE.{0},  1 as RowNumber , 1 as TotalRecord , 1 as TotalUnverified ,   CTE.* FROM " +
            " {2} As CTE WHERE {3}  ", fieldId, orderBy, tableOrViewName, conditionStr/*, lockConditionStr*/);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                if (conditionStr.Trim() != "" && conditionStr.Trim() != null)
                {
                    sql = string.Format("{0} AND CTE.{1} = @fieldValue ", sql, fieldId);
                    sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                }
                else
                {
                    sql = string.Format("{0} CTE.{1} = @fieldValue ", sql, fieldId);
                    sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                }
                
            }
            //sqlParameter.Add(new SqlParameter("@fldIssueBankCode", currentUser.BankCode));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }
        // kai hong 20170616
        // new function for construct item
        // simplified query
        // only can be applied to check that not yet approved or returned
        public static SqlDetails ConstructSqlForNextItem(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";

            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            string sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, 0 NextValue , CTE.rownum as RowNumber ,0 ,0 , " +
                                       "(select count(1) as totalrecord FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId AND [fldClearDate] = @fldClearDate {3} ) TotalRecord, " +
                                       "(select count(1) as pending FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId and isnull(fldapprovalstatus,'')='' {3} ) TotalUnverified, " +
                                       " CTE.* FROM " +
                                       "(SELECT rownum=0,* FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) as CTE ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }

            sql = string.Format("{0} ORDER BY fldAmount DESC, cte.fldinwarditemid DESC   ", sql, orderBy);


            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlForPreviousItem(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, string fieldId, AccountModel currentUser, string fieldValue = null, string OrderByDesc = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = OrderByDesc;
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            if (!string.IsNullOrEmpty(pageConfig.SqlLockCondition))
            {
                lockConditionStr = string.Format(" AND {0} ", pageConfig.SqlLockCondition);
            }

            string sql = string.Format("SELECT TOP 1 prev.{0} PreviousValue,CTE.fldInwardItemId FROM (SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}),* FROM {2} " +
            " WHERE fldAssignedUserId = @fldAssignedUserId {3}) As CTE LEFT JOIN (SELECT rownum = ROW_NUMBER() OVER(ORDER BY {1}) , {0} " +
            " FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3}) prev ON prev.rownum = CTE.rownum - 1 ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE CTE.{1} = @fieldValue ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
            }
            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));

            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }

        public static SqlDetails ConstructSqlFromConfigTableSql(PageSqlConfig sqlPageConfig, ConfigTable configTable,
            FormCollection collection)
        {
            StringBuilder fieldsSql = new StringBuilder();
            StringBuilder filtersSql = new StringBuilder();
            StringBuilder configOrderBy = new StringBuilder();
            List<SqlParameter> sqlParameter = new List<SqlParameter>();
            string tableOrViewName = configTable.ViewOrTableName;

            try
            {

                foreach (DataRow row in configTable.dataTable.Rows)
                {
                    string field = StringUtils.Trim(row["fieldId"].ToString());
                    if ((!string.IsNullOrEmpty(field)))
                    {

                        //Prepare SELECT clause
                        if ((string.Compare(row["isResult"].ToString(), "Y") == 0
                            | string.Compare(row["isResultParam"].ToString(), "Y") == 0))
                        {
                            fieldsSql.AppendFormat("[{0}] ,", field);
                        }

                        if (!string.IsNullOrEmpty(row["FieldIdOrderBy"].ToString()))
                        {
                            configOrderBy.AppendFormat(" {0} {1} ,", field, row["FieldIdOrderBy"].ToString());
                        }

                        //Prepare WHERE clause
                        if (string.Compare(row["isFilter"].ToString(), "Y") == 0
                            && string.Compare(row["FieldIdSqlCondition"].ToString(), "") != 0)
                        {
                            //@@self = @fldAccountNumber
                            string sqlCondition = row["FieldIdSqlCondition"].ToString();
                            sqlCondition = sqlCondition.Replace("@@param", "@" + field);

                            if (!string.IsNullOrEmpty(collection[field]))
                            {
                                if (string.Compare(row["FieldType"].ToString(), "Currency") == 0)
                                {
                                    filtersSql.AppendFormat(" {0}  AND", sqlCondition);
                                    sqlParameter.Add(new SqlParameter("@" + field, collection[field].Replace(",", "")));
                                }
                                else
                                {
                                    filtersSql.AppendFormat(" {0}  AND", sqlCondition);
                                    sqlParameter.Add(new SqlParameter("@" + field, collection[field]));
                                }
                                    
                            }
                        }
                        else if (string.Compare(row["isFilter"].ToString(), "Y") == 0)
                        {
                            if (!string.IsNullOrEmpty(collection[field]) |
                                !string.IsNullOrEmpty(collection["from_" + field]) |
                                !string.IsNullOrEmpty(collection["to_" + field]))
                            {
                                string fieldFilterParam = "@" + field;
                                string fieldFilterValue = "";
                                //    if (string.Compare(row["FieldType"].ToString(), "TextBox") == 0 |
                                //        string.Compare(row["FieldType"].ToString(), "NumberBox") == 0)
                                //    {

                                //        filtersSql.AppendFormat(" [{0}] LIKE '%'+{1}+'%' AND", field, fieldFilterParam);
                                //        fieldFilterValue = collection[field];

                                //    }
                                if (sqlPageConfig.TaskId == "305999"  && string.Compare(row["FieldType"].ToString(), "Date") == 0)
                                {

                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = (collection[field]);

                                }
                                if (string.Compare(row["fieldtype"].ToString(), "TextBox") == 0)
                                {

                                    filtersSql.AppendFormat(" {0} LIKE '%' + {1} + '%' AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field].Trim();

                                }
                                // filtersSql.AppendFormat(" {0}::integer = {1}::integer AND", field, fieldFilterParam);
                                else if (string.Compare(row["fieldtype"].ToString(), "NumberBox") == 0)
                                {
                                    filtersSql.AppendFormat(" {0} LIKE '%' + {1} + '%' AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field];
                                }
                                //Abdin 20201130
                                else if((/*sqlPageConfig.TaskId == "309300" ||*/ sqlPageConfig.TaskId == "309305" || sqlPageConfig.TaskId == "309123" || sqlPageConfig.TaskId == "309124" || sqlPageConfig.TaskId == "700101" || sqlPageConfig.TaskId == "700104" || sqlPageConfig.TaskId == "305989" || sqlPageConfig.TaskId == "999950") && string.Compare(row["FieldType"].ToString(), "Date") == 0)
                                {

                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field];

                                }
                                //Abdin 20201130

                                //20200520nicoforcleardataprocessocs
                                else if (sqlPageConfig.TaskId == "999980" && string.Compare(row["FieldType"].ToString(), "Date") == 0)
                                {

                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field];

                                }
                                //20200520nicoforcleardataprocessocs

                                //else if ((sqlPageConfig.TaskId == "205300" || sqlPageConfig.TaskId == "999840" || sqlPageConfig.TaskId == "999830") && string.Compare(row["FieldType"].ToString(), "Date") == 0)
                                //{

                                //    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);

                                //    fieldFilterValue = collection[field];

                                //}


                                else if ((/*sqlPageConfig.TaskId == "205300" ||*/ sqlPageConfig.TaskId == "999840" || sqlPageConfig.TaskId == "999830") && string.Compare(row["FieldType"].ToString(), "Date") == 0)
                                {

                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);

                                    fieldFilterValue = collection[field];

                                }


                                else if ((string.Compare(row["FieldType"].ToString(), "Date") == 0))
                                {

                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    if (sqlPageConfig.TaskId == "102675")
                                    {
                                        //fieldFilterValue = Convert.ToDateTime(collection[field]).ToString("yyyy-MM-dd");
                                        fieldFilterValue = DateTime.ParseExact(collection[field], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                                        
                                    }
                                    else if(sqlPageConfig.TaskId == "999850" || sqlPageConfig.TaskId == "309124")
                                    {
                                        fieldFilterValue = DateTime.ParseExact(collection[field], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");

                                    }
                                    else
                                    {
                                    fieldFilterValue = DateUtils.formatDateToSql(collection[field].Replace(",",""));
                                    }

                                }
                                else if ((string.Compare(row["FieldType"].ToString(), "DateBranchEODSummary") == 0))
                                {

                                    filtersSql.AppendFormat(" ([{0}] = {1} OR [{0}] IS NULL) AND", field, fieldFilterParam);
                                    fieldFilterValue = DateUtils.formatDateToSql(collection[field]);

                                }
                                else if (sqlPageConfig.TaskId == "205131" && string.Compare(row["fieldtype"].ToString(), "DateFromTo") == 0)
                                {
                                    string fieldFilterParam3 = "@from_" + field;
                                    string fieldFilterValue3 = "'"+DateUtils.formatDateToSqlyyyymmdd(collection["from_fldCreateTimeStamp1"])+"'";
                                    string fieldFilterParam2 = "@to_" + field;
                                    string fieldFilterValue2 = "'" + DateUtils.formatDateToSqlyyyymmdd(collection["to_fldCreateTimeStamp1"]) + "'";
                                    filtersSql.AppendFormat(" {0} between {1} AND {2} AND", field, fieldFilterValue3, fieldFilterValue2);
                                    sqlParameter.Add(new SqlParameter(fieldFilterParam3, fieldFilterValue3));
                                    sqlParameter.Add(new SqlParameter(fieldFilterParam2, fieldFilterValue2));
                                }
                                // xx edit 20210517
                                else if (string.Compare(row["FieldType"].ToString(), "DateFromTo") == 0)
                                {

                                    fieldFilterParam = "@from_" + field;
                                    //fieldFilterValue = DateUtils.formatDateToSql(collection["from_" + field]);
                                    fieldFilterValue = DateTime.ParseExact(collection["from_" + field], ConfigureSetting.GetDateFormat(), null).ToString("yyyy-MM-dd");
                                    

                                    string fieldFilterParam2 = "@to_" + field;
                                    string fieldFilterValue2 = DateTime.ParseExact(collection["to_" + field], ConfigureSetting.GetDateFormat(), null).ToString("yyyy-MM-dd");
                                    

                                    filtersSql.AppendFormat(" [{0}] >= {1} AND [{0}] <= {2} AND", field, fieldFilterParam, fieldFilterParam2);

                                    sqlParameter.Add(new SqlParameter(fieldFilterParam2, fieldFilterValue2));

                                }
                                // xx end 20210517
                                //Azim Start 20210608
                                else if (string.Compare(row["FieldType"].ToString(), "Currency") == 0)
                                {
                                    fieldFilterParam = fieldFilterParam.Replace(",", "");
                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field].Replace(",", ""); 
                                }
                                else if (string.Compare(row["FieldLabel"].ToString(), "Trans Code") == 0)
                                {
                                    fieldFilterParam = fieldFilterParam.Replace(",", "");
                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field].Replace(",", "");
                                }
                                //Azim End 20210608
                                else
                                {
                                    //filtersSql += " [" + field + "] = '" + fieldFilterValue + "' AND";
                                    filtersSql.AppendFormat(" [{0}] = {1} AND", field, fieldFilterParam);
                                    fieldFilterValue = collection[field];
                                }
                                sqlParameter.Add(new SqlParameter(fieldFilterParam, fieldFilterValue));
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            fieldsSql.Length = fieldsSql.ToString().LastIndexOf(",") > 0 ? fieldsSql.ToString().LastIndexOf(",") : 0;
            filtersSql.Length = filtersSql.ToString().LastIndexOf("AND") > 0 ? filtersSql.ToString().LastIndexOf("AND") : 0;
            configOrderBy.Length = configOrderBy.ToString().LastIndexOf(",") > 0 ? configOrderBy.ToString().LastIndexOf(",") : 0;

            if (fieldsSql.Length == 0) { fieldsSql.Append(" * "); }

            //Order By Construction
            //1. From orderByFromRequest -- From Pagination
            //2. From sqlPageConfig -- HardCode
            //3. From dbConfigOrdeByStr -- Form Database 

            string orderBy = "";
            string orderByFromRequest = string.Format(" {0} {1} ", DatabaseUtils.SanitizeString(collection["orderBy"]), DatabaseUtils.SanitizeString(collection["orderType"]));
            string dbConfigOrdeByStr = configOrderBy.ToString();

            if (!string.IsNullOrEmpty(StringUtils.Trim(dbConfigOrdeByStr)))
            {
                orderBy = dbConfigOrdeByStr;
            }
            if (!string.IsNullOrEmpty(StringUtils.Trim(sqlPageConfig.SqlOrderBy)))
            {
                orderBy = sqlPageConfig.SqlOrderBy;
            }
            if (!string.IsNullOrEmpty(StringUtils.Trim(orderByFromRequest)))
            {
                orderBy = orderByFromRequest;
            }

            string selectSql;
            //End
            //20200708
            if (sqlPageConfig.TaskId == "304792" || sqlPageConfig.TaskId == "304513" || sqlPageConfig.TaskId == "304791" || sqlPageConfig.TaskId == "304512" || sqlPageConfig.TaskId == "304601" || sqlPageConfig.TaskId == "304602" || sqlPageConfig.TaskId == "305989") {

                selectSql = string.Format("SELECT * FROM {1} ","", DatabaseUtils.SanitizeString(tableOrViewName));
            }
            else
            {
                selectSql = string.Format("SELECT {0} FROM {1} ", fieldsSql.ToString(), DatabaseUtils.SanitizeString(tableOrViewName));


            }
            var viewortablename = DatabaseUtils.SanitizeString(tableOrViewName);

            if (viewortablename == "view_ocsdashboardprogressstatus")
            {
                filtersSql.AppendFormat(sqlPageConfig.SqlExtraCondition);
                //selectSql += string.Format("WHERE fldcapturingdate::date = @fldcapturingdate::date::timestamp");
            }
            else
            {

            if (filtersSql.Length > 0 | !string.IsNullOrEmpty(sqlPageConfig.SqlExtraCondition))
            {
                if (filtersSql.Length > 0 & !string.IsNullOrEmpty(sqlPageConfig.SqlExtraCondition))
                {
                    filtersSql.AppendFormat(" AND {0} ", sqlPageConfig.SqlExtraCondition);
                }
                else if (filtersSql.Length == 0 & !string.IsNullOrEmpty(sqlPageConfig.SqlExtraCondition))
                {
                    filtersSql.AppendFormat(sqlPageConfig.SqlExtraCondition);
                }
                selectSql += string.Format("WHERE {0} ", filtersSql.ToString());
            }
            }
            //Add Custom SQL Parameters
            if (sqlPageConfig.SqlExtraConditionParams != null) { sqlParameter.AddRange(sqlPageConfig.SqlExtraConditionParams); }

            return new SqlDetails(selectSql, tableOrViewName, fieldsSql.ToString(), filtersSql.ToString(), sqlParameter.ToArray(), DatabaseUtils.SanitizeString(orderBy));
        }


        private static List<SqlParameter> reinitializeParam(SqlParameter[] sqlParamIn)
        {
            List<SqlParameter> reInitSqlParams = new List<SqlParameter>();
            foreach (SqlParameter p in sqlParamIn)
            {
                reInitSqlParams.Add(new SqlParameter(p.ParameterName, p.Value));
            }
            return reInitSqlParams;
        }

        public static KeyValuePair<string, List<SqlParameter>> GetSqlParameterFromSqlAndCurrentUserAccount(string sqlQuery, Dictionary<string, string> currentUserParams)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            foreach (KeyValuePair<string, string> pair in currentUserParams)
            {
                if ((sqlQuery.IndexOf("@" + pair.Key) >= 0))
                {
                    if ("currentUserBranchCodes".Equals(pair.Key))
                    {
                        string[] branchCodesArray = pair.Value.Split(',');
                        string branchCodesSql = DatabaseUtils.getParameterizedStatementFromArray(branchCodesArray, "currentUserBranchCodes");
                        sqlParams.AddRange(DatabaseUtils.getSqlParametersFromArray(branchCodesArray, "currentUserBranchCodes"));
                        sqlQuery = sqlQuery.Replace("@currentUserBranchCodes", branchCodesSql);
                    }
                    else if ("currentUserBranchHubCodes".Equals(pair.Key))
                    {
                        string[] branchCodesArray = pair.Value.Split(',');
                        string branchCodesSql = DatabaseUtils.getParameterizedStatementFromArray(branchCodesArray, "currentUserBranchHubCodes");
                        sqlParams.AddRange(DatabaseUtils.getSqlParametersFromArray(branchCodesArray, "currentUserBranchHubCodes"));
                        sqlQuery = sqlQuery.Replace("@currentUserBranchHubCodes", branchCodesSql);
                    }
                    else if ("currentUserTaskIds".Equals(pair.Key))
                    {
                        string[] branchCodesArray = pair.Value.Split(',');
                        string branchCodesSql = DatabaseUtils.getParameterizedStatementFromArray(branchCodesArray, "currentUserTaskIds");
                        sqlParams.AddRange(DatabaseUtils.getSqlParametersFromArray(branchCodesArray, "currentUserTaskIds"));
                        sqlQuery = sqlQuery.Replace("@currentUserTaskIds", branchCodesSql);
                    }
                    else if ("currentUserGroupIds".Equals(pair.Key))
                    {
                        string[] branchCodesArray = pair.Value.Split(',');
                        string branchCodesSql = DatabaseUtils.getParameterizedStatementFromArray(branchCodesArray, "currentUserGroupIds");
                        sqlParams.AddRange(DatabaseUtils.getSqlParametersFromArray(branchCodesArray, "currentUserGroupIds"));
                        sqlQuery = sqlQuery.Replace("@currentUserGroupIds", branchCodesSql);
                    }
                    else
                    {
                        sqlParams.Add(new SqlParameter("@" + pair.Key, pair.Value));
                    }
                }
            }
            return new KeyValuePair<string, List<SqlParameter>>(sqlQuery, sqlParams);
        }

        public static SqlDetails GoNext(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, DataTable resultTable, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            string AccountNumber = resultTable.Rows[0]["fldAccountNumber"].ToString();
            string Amount = resultTable.Rows[0]["fldAmount"].ToString();
            string sql = "";
            string UIC = resultTable.Rows[0]["fldUIC"].ToString();
            //collection["orderBy"]
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            // sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, fldinwarditemid as NextValue , CTE.rownum as RowNumber ,0 ,0 , 50 TotalRecord, 49 TotalUnverified,  CTE.* FROM " +
            //"(SELECT rownum=0,* FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) as CTE ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
            sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, fldinwarditemid as NextValue , CTE.rownum as RowNumber ,0 ,0 , " +
            "(select count(1) as totalrecord FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) TotalRecord, " +
            "(select count(1) as pending FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId AND isnull(fldapprovalstatus,'')='' {3} ) TotalUnverified, " +
            "CTE.* FROM " +
            "(SELECT rownum=0,* FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) as CTE ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE (fldamount < @fldAmount) ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                sqlParameter.Add(new SqlParameter("@fldAccountNumber", AccountNumber));
                sqlParameter.Add(new SqlParameter("@fldAmount", Amount));
                sqlParameter.Add(new SqlParameter("@fldUIC", UIC));
            }

            sql = string.Format("{0} ORDER BY fldAmount DESC, cte.fldinwarditemid DESC ", sql, orderBy);

            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }
        public static SqlDetails GoPrevious(PageSqlConfig pageConfig, ConfigTable configTableModel, FormCollection collection, DataTable resultTable, string fieldId, AccountModel currentUser, string fieldValue = null)
        {
            DataTable configTable = configTableModel.dataTable;
            SqlDetails sqlDetails = ConstructSqlFromConfigTableSql(pageConfig, configTableModel, collection);

            string tableOrViewName = sqlDetails.tableName;
            List<SqlParameter> sqlParameter = sqlDetails.sqlParams;
            string conditionStr = "";
            string lockConditionStr = "";
            string orderBy = "(SELECT NULL)";
            string AccountNumber = resultTable.Rows[0]["fldAccountNumber"].ToString();
            string Amount = resultTable.Rows[0]["fldAmount"].ToString();
            string UIC = resultTable.Rows[0]["fldUIC"].ToString();
            string sql = "";

            //collection["orderBy"]
            if (!string.IsNullOrEmpty(sqlDetails.orderBySql))
            {
                orderBy = string.Format("{0}", sqlDetails.orderBySql);
            }


            if (!string.IsNullOrEmpty(sqlDetails.conditionAsSqlString))
            {
                conditionStr = string.Format(" AND {0} ", sqlDetails.conditionAsSqlString);
            }

            // sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, fldinwarditemid as NextValue , CTE.rownum as RowNumber ,0 ,0 , 50 TotalRecord, 49 TotalUnverified,  CTE.* FROM " +
            //"(SELECT rownum=0,* FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) as CTE ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);
            sql = string.Format(" SELECT TOP 1 0 PreviousValue, CTE.{0}, fldinwarditemid as NextValue , CTE.rownum as RowNumber ,0 ,0 , " +
            "(select count(1) as totalrecord FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) TotalRecord, " +
            "(select count(1) as pending FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId AND isnull(fldapprovalstatus,'')='' {3} ) TotalUnverified, " +
            "CTE.* FROM " +
            "(SELECT rownum=0,* FROM {2} WHERE fldAssignedUserId = @fldAssignedUserId {3} ) as CTE ", fieldId, orderBy, tableOrViewName, conditionStr, lockConditionStr);

            if (!string.IsNullOrEmpty(fieldValue))
            {
                sql = string.Format("{0} WHERE (fldamount > @fldAmount) ", sql, fieldId);
                sqlParameter.Add(new SqlParameter("@fieldValue", fieldValue));
                sqlParameter.Add(new SqlParameter("@fldAccountNumber", AccountNumber));
                sqlParameter.Add(new SqlParameter("@fldAmount", Amount));
                sqlParameter.Add(new SqlParameter("@fldUIC", UIC));
            }

            sql = string.Format("{0} ORDER BY fldAmount ASC, cte.fldinwarditemid ASC ", sql, orderBy);

            sqlParameter.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            return new SqlDetails(sql, tableOrViewName, sqlParameter.ToArray());
        }
    }
}