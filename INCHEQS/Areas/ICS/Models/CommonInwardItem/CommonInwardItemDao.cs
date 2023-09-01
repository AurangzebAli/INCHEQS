using INCHEQS.Helpers;
using INCHEQS.Security;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.SystemProfile;
using System.Data.Common;
using INCHEQS.Models.Sequence;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Account;
//using static INCHEQS.Helpers.ImageHelper;
using System.IO;
//using INCHEQS.Security.Account;
using INCHEQS.Common;
using System.Globalization;
using INCHEQS.DataAccessLayer;
using static INCHEQS.Helpers.ImageHelper;
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace INCHEQS.Models.CommonInwardItem {
    public class CommonInwardItemDao : ICommonInwardItemDao {

        private readonly IPageConfigDao pageConfigDao;
        private readonly ISequenceDao sequenceDao;
        private readonly ApplicationDbContext dbContext;
        private readonly IAuditTrailDao auditTrailDao;
        ISystemProfileDao systemProfileDao;

        public CommonInwardItemDao(ISystemProfileDao systemProfileDao,ISequenceDao sequenceDao, IPageConfigDao pageConfigDao, ApplicationDbContext dbContext, IAuditTrailDao auditTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        public void LockThisCheque(string inwardItemId, AccountModel currentUser) {
            //Lock inwardItemForCurrentUser

          
            string sMySQL = "UPDATE tblInwardItemInfoStatus set fldAssignedUserId = @fldAssignedUserId where fldInwardItemId = @inwardItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", inwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });
          

        }

        public bool CheckStatus(string inwardItemId, AccountModel currentUser)
        {
            string userid;
           
            string sMySQL = "Select fldAssignedUserId from tblInwardItemInfoStatus where fldInwardItemId = @inwardItemId";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", inwardItemId)
                });
          
            if (dataResult.Rows.Count > 0)
            {
                userid = dataResult.Rows[0]["fldAssignedUserId"].ToString();
                if ((userid == "") || (userid == null))
                {
                    return true;
                }
            }
            else
            {
                userid = "";
                return true;
            }

            if (userid != currentUser.UserId)
            {
                return false;
            }
            else
            {
                return true;
            }
                
        }

        public DataTable GetTruncatedAmount()
        {
            DataTable dataTable = new DataTable();
            return this.dbContext.GetRecordsAsDataTable("SELECT fldTruncateMin, fldTruncateMax, fldTruncateRemark FROM tblSysSetting", null);
        }
        public bool CheckStatusBranch(string inwardItemId, AccountModel currentUser)
        {
            string userid;
            string sMySQL = "Select fldAssignedUserId from tblPendingInfo where fldInwardItemId = @inwardItemId";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", inwardItemId)
                });
            if (dataResult.Rows.Count > 0)
            {
                userid = dataResult.Rows[0]["fldAssignedUserId"].ToString();
                if ((userid == "") || (userid == null))
                {
                    return true;
                }
            }
            else
            {
                userid = "";
                return true;
            }

            if (userid != currentUser.UserId)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public void LockThisChequeHistory(string inwardItemId, AccountModel currentUser)
        {
            //Lock inwardItemForCurrentUser
            string sMySQL = "UPDATE tblInwardItemInfoStatusH set fldAssignedUserId = @fldAssignedUserId where fldInwardItemId = @inwardItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", inwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });

        }

        public Dictionary<string, string> LockAllCheque(QueueSqlConfig pageConfig, FormCollection collection , AccountModel currentUser , int numberOfItem) {

            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;

            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition +")": "";

            string sMySQL = string.Format("UPDATE top({0}) tblInwardItemInfoStatus SET fldAssignedUserId = @fldAssignedUserId, fldAssignedQueue=@fldAssignedQueue WHERE fldInwardItemId in ( SELECT TOP ({0}) fldInwardItemId FROM {1} WHERE ((fldAssignedUserId IS NULL OR fldAssignedUserId = @fldAssignedUserId) {4}) {2} {3} ) ", numberOfItem , sqlHolder.tableName,mainCondition, mainOrderBy, lockCondition);

            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            dbContext.ExecuteNonQuery(sMySQL, sqlParams.ToArray());
            //Return Null/Empty so that next cheque will be found as first item assigned           
            return FirstChequeFromTheResult(pageConfig, collection, currentUser);
        }



        public Dictionary<string, string> FindItemByInwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null) {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable=null;

                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious)) {
                if ("Next".Equals(nextOrPrevious)) {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                } else if ("Prev".Equals(nextOrPrevious)) {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}

            //return result;
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }

        //20210608 Azim Start
        public Dictionary<string, string> FindItemByInwardItemIdBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemBranch(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemBranch(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}

            //return result;
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }
        //20210608 Azim End

        public Dictionary<string, string> FindItemByInwardItemIdNOLOCK(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null,  string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;

            //ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    //resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                    resultInwardItemId = collection["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    //resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                    resultInwardItemId = collection["PreviousValue"].ToString();
                }
                else if ("Error".Equals(nextOrPrevious))
                {
                    resultInwardItemId = inwardItemId;
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}

            //return result;
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }

        public Dictionary<string, string> FindItemByInwardItemIdNOLOCKNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
      


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                    //resultInwardItemId = collection["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                    //resultInwardItemId = collection["PreviousValue"].ToString();
                }
                else if ("Error".Equals(nextOrPrevious))
                {
                    resultInwardItemId = inwardItemId;
                }
               
                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
              

            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}

            //return result;
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }

        public Dictionary<string, string> FindItemByInwardItemIdBranchNOLOCKNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemBranchNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
     


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                    //resultInwardItemId = collection["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                    //resultInwardItemId = collection["PreviousValue"].ToString();
                }
                else if ("Error".Equals(nextOrPrevious))
                {
                    resultInwardItemId = inwardItemId;
                }
               
                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemBranchNOLOCK(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
             

            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}

            //return result;
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }

        public Dictionary<string, string> FindInwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
     


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

               
                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
              

            }

            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}
            //return result;


        }

        public Dictionary<string, string> FindInwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
           

            sqlDetails = SearchPageHelper.ConstructSqlForSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, inwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
          
        


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }
              
                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
             

            }

            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

            //DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            //Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
            //        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            ////}
            //return result;


        }

        public Dictionary<string, string> FirstChequeFromTheResult(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser) {

            return FindItemByInwardItemId(pageConfig, collection, currentUser);
            //return FindInwardItemId(pageConfig, collection, currentUser);

        }
        

        public Dictionary<string, string> NextCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser) {

            return FindItemByInwardItemId(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
            //return FindInwardItemId(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
        }


        public Dictionary<string, string> PrevCheque(QueueSqlConfig pageConfig, FormCollection collection,AccountModel currentUser) {
           
            return FindItemByInwardItemId(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Prev");

        }

        public Dictionary<string, string> ErrorCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser) {

            return FindItemByInwardItemId(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Error");

        }

        public Dictionary<string, string> ErrorChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            return FindItemByInwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Error");

        }

        public Dictionary<string, string> ErrorChequeWithoutLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            return FindItemByInwardItemIdNOLOCK(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Error");

        }

        public int UnlockAllAssignedForUser(AccountModel currentUser) {
            string sMySQL = "update tblInwardItemInfoStatus set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId ;";
            sMySQL = sMySQL + "delete from  tblVerificationLock where fldAssignedUserId = @userId ;";
            return dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@userId", currentUser.UserId) });
        }

        public int UnlockAllAssignedForBranchUser(AccountModel currentUser)
        {
            string sMySQL = "update tblPendingInfo set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId ;";
            sMySQL = sMySQL + "update tblInwardItemInfoStatus set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId ;";
            sMySQL = sMySQL + "delete from  tblBranchVerificationLock where fldAssignedUserId = @userId or fldAssignedUserId IS NULL ;";
            return dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@userId", currentUser.UserId) });
        }

        public int UnlockAllAssignedForUserHistory(AccountModel currentUser)
        {
            string sMySQL = "update tblInwardItemInfoStatusH set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId";
            return dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@userId", currentUser.UserId) });
        }

        public int CheckMainTable(AccountModel currentUser, string date)
        {
            int countrow = 0;
            string sMySQL = "Select count(1) as totcount from tblinwardcleardate where datediff(d,fldCleardate,@fldCleardate)=0 and fldbankcode = @fldBankCode AND fldstatus = 'Y'";
            //string sMySQL = "Select count(1) as totcount from tblInwardItemInfo where datediff(d,fldCleardate,@fldCleardate)=0 and fldIssueBankCode = @fldBankCode";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                new SqlParameter("@fldBankCode", currentUser.BankCode),
                new SqlParameter("@fldCleardate", DateUtils.formatDateToSql(date))
            });

            if (dataResult.Rows.Count > 0)
            {
                DataRow row = dataResult.Rows[0];
                countrow = (int)row["totcount"];
                
            }
            if (countrow > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
            
        }

        public int CheckUserType(AccountModel currentUser)
        {
            string userType;
            string sMySQL = "Select fldUserType from tblUserMaster where fldUserId = @fldUserID";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                new SqlParameter("@fldUserID", currentUser.UserId)
            });
            if (dataResult.Rows.Count > 0)
            {
                userType = dataResult.Rows[0]["fldUserType"].ToString();
                if (userType == "Admin")
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }else
            {
                return 0;
            };
        }
        public int CheckMainTableInward(AccountModel currentUser, string inwarditemid)
        {
            int countrow = 0;
            string sMySQL = "Select count(1) as totcount from tblInwardItemInfo where fldInwardItemId=@fldInwardId";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                //new SqlParameter("@fldBankCode", currentUser.BankCode),
                new SqlParameter("@fldInwardId", inwarditemid)
            });

            if (dataResult.Rows.Count > 0)
            {
                DataRow row = dataResult.Rows[0];
                countrow = (int)row["totcount"];

            }
            if (countrow > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }

        // xx start 20210422
        public DataTable ChequeHistory(string inwardItemId) {
            return dbContext.GetRecordsAsDataTable("Select * from View_ChequeHistory where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp desc",
                new[] { new SqlParameter("@fldInwardItemId", inwardItemId) });
        }
        public DataTable IQAResult(string inwardItemId)
        {
            return dbContext.GetRecordsAsDataTable("Select * from View_IQAResultPopup where fldInwardItemId = @fldInwardItemId",
                new[] { new SqlParameter("@fldInwardItemId", inwardItemId) });
        }
        public DataTable RouteReason()
        {
            return dbContext.GetRecordsAsDataTable("Select * from View_RouteReason ORDER BY  cast(fldRejectCode as int) asc");
        }
        public DataTable ManuallyUpdateCheques(string inwardItemId, AccountModel currentUser, FormCollection collection, string accNo,string cheqNo,string Taskid)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", inwardItemId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", Taskid));

            DataTable dtManualMarked = dbContext.GetRecordsAsDataTableSP("sp_updateManuallyMarkedCheque", sqlParameterNext.ToArray());
            string Message = "";        
            
         
            return dtManualMarked;

        }
        public DataTable ManuallyUpdateReturnedCheques(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));

            
            return dbContext.GetRecordsAsDataTableSP("sp_updateManuallyReturnedMarkedCheque", sqlParameterNext.ToArray());

        }

        public DataTable RouteToAuthorizer(string inwardItemId, AccountModel currentUser, FormCollection collection, string accNo, string cheqNo, string Taskid)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", inwardItemId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", Taskid));

            DataTable dtManualMarked = dbContext.GetRecordsAsDataTableSP("sp_updateRouteToAuthorizer", sqlParameterNext.ToArray());
            string Message = "";


            return dtManualMarked;

        }

        
        


        public DataTable ChequeHistoryH(string inwardItemId)
        {
            return dbContext.GetRecordsAsDataTable("Select * from View_ChequeHistoryH where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp desc",
                new[] { new SqlParameter("@fldInwardItemId", inwardItemId) });
        }
        // xx end 20210422

        // shamil 20170616
        // function to delete temp gif
        public string DeleteTempGif(string imageId)
        {
            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);
            ImageInfo imageInfo = new ImageInfo();
            string tempImageFolder = string.Format("{0}{1}", tempPath, CurrentUser.Account.UserAbbr);
            if (Directory.Exists(tempImageFolder))
            {
                string[] fileList = Directory.GetFiles(tempImageFolder + "\\", "*" + imageId + "*");
                foreach (string file in fileList)
                {
                    File.Delete(file);
                }
            }
            return null;
        }

        public DataTable FindAllTransCode() {
            return dbContext.GetDataTableFromSqlWithParameter("Select * from tblTransCodeMaster");
        }

        public string CheckIfRecordUpdatedorDeletedForListing(string inwardItemId)
        {
            string result = "";
            string allowed;
            string updatedUserId;

            string sqlItem = "Select fldUpdateTimeStamp from tblInwardItemInfo where fldInwardItemId = @inwardItemId ";
            DataTable dataResultItem = dbContext.GetRecordsAsDataTable(sqlItem, new[] { new SqlParameter("@inwardItemId", inwardItemId) });

            string sqlStatus = "SELECT TOP (1) CASE WHEN DATEDIFF(Minute,fldUpdateTimeStamp,GETDATE()) >= 1 THEN 'Y' ELSE 'N' END AS Allowed, fldUpdateUserID FROM tblInwardItemInfoStatus where fldInwardItemId = @inwardItemId ";
            DataTable dataResultStatus = dbContext.GetRecordsAsDataTable(sqlStatus, new[] {new SqlParameter("@inwardItemId", inwardItemId)});

            //string sqlItemInBranch = "Select TOP 1 fldUpdateTimeStamp from tblInwardItemInfoStatus where fldInwardItemId = @inwardItemId ";


            if (dataResultItem.Rows.Count > 0)
            {
                if (dataResultStatus.Rows.Count > 0)
                {
                    updatedUserId = dataResultStatus.Rows[0]["fldUpdateUserID"].ToString();
                    

                    //DateTime.ParseExact(collection["row_fldcapturingdate"].Trim(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                    if (updatedUserId.Trim() == CurrentUser.Account.UserId)
                    {
                        result = "";
                    }
                    else
                    {
                        allowed = dataResultStatus.Rows[0]["Allowed"].ToString();

                        if (allowed.Trim() == "Y")
                        {
                            result = "";
                        }
                        else
                        {
                            result = "updated";
                        }
                    }
                }
                else
                {
                    result = "deleted";
                }
            }
            else
            {
                result = "deleted";
            }

            
            return result;
        }

        public bool CheckIfUPIGenerated(string inwardItemId)
        {
            string sql = "SELECT fldUPIGenerated FROM tblInwardItemInfoStatus where isnull(fldUPIGenerated,'0') != '0' and fldInwardItemId = @inwardItemId ";
            /*string infoStatusSql = "Select fldassigneduserid from tblinwarditeminfostatus where fldinwarditemid = @inwardItemId  " +
               "AND fldassigneduserid = @userid";*/
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@inwardItemId", inwardItemId) }))
            {
                return false;
            }
            return true;
        }

        public bool CheckIfRecordUpdatedOrDeleted(string inwardItemId) {
            string sql = "Select fldUpdateTimeStamp from tblInwardItemInfo where fldInwardItemId = @inwardItemId "; 
            string infoStatusSql = "Select fldassigneduserid from tblinwarditeminfostatus where fldinwarditemid = @inwardItemId  " +
               "AND fldassigneduserid = @userid";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@inwardItemId", inwardItemId) }) || dbContext.CheckExist(infoStatusSql, new[]{ new SqlParameter("@inwardItemId", inwardItemId) ,
                    new SqlParameter("@userid", Convert.ToInt32(CurrentUser.Account.UserId))})
                ) {

                return false;
            }
            return true;
        }

        public bool CheckIfRecordUpdatedOrDeletedBranch(string inwardItemId)
        {
            string sql = "Select fldUpdateTimeStamp from tblInwardItemInfo where fldInwardItemId = @inwardItemId ";
            string infoStatusSql = "Select fldassigneduserid from tblPendingInfo where fldinwarditemid = @inwardItemId  " +
               "AND fldassigneduserid = @userid";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@inwardItemId", inwardItemId) }) || dbContext.CheckExist(infoStatusSql, new[]{ new SqlParameter("@inwardItemId", inwardItemId) ,
                    new SqlParameter("@userid", Convert.ToInt32(CurrentUser.Account.UserId))})
                )
            {

                return false;
            }
            return true;
        }

        //add by shamil to validate check is lock by other user march 29 2017
        public bool CheckLockedCheck(string inwardItemId, string assigneduserid)
        {
            string sql = "Select fldInwardItemId from View_AppInwardItem where fldInwardItemId = @fldInwardItemId and fldAssignedUserId " +
                "=@fldAssignedUserId";
            if(dbContext.CheckExist(sql, new[]{ new SqlParameter("@fldInwardItemId", inwardItemId) ,
                    new SqlParameter("@fldAssignedUserId", assigneduserid)}))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //add by azim to validate check is lock by other user december 6 2021
        public bool CheckLockedCheck2(string inwardItemId, string assigneduserid)
        {
            string sql = "Select fldInwardItemId from View_AppInwardItem where fldInwardItemId = @fldInwardItemId and (fldAssignedUserId " +
                "=@fldAssignedUserId or ISNULL(fldAssignedUserId,'0') = '0') ";
            if (dbContext.CheckExist(sql, new[]{ new SqlParameter("@fldInwardItemId", inwardItemId) ,new SqlParameter("@fldAssignedUserId", assigneduserid)}))
            {
                return false;
            }
            else
            {
                
                return true;
            }
        }

        public string GetModifiedField(FormCollection collection) {
            string result = "";
            try {
                DataTable dataResult = dbContext.GetDataTableFromSqlWithParameter("Select fldModifiedFields from tblInwardItemInfoStatus where fldInwardItemId=@fldInwardItemId",
                Utils.ConvertFormCollectionToDictionary(collection));
                if (dataResult.Rows.Count > 0) {
                    result = dataResult.Rows[0]["fldModifiedFields"].ToString();
                }
            } catch (Exception ex) {
                throw ex;
            }
            return result;
        }

        public string GetBranchItemStatus(FormCollection collection) {
            string result = "";
            try {
                DataTable dataResult = dbContext.GetDataTableFromSqlWithParameter("Select fldApprovalStatus from tblPendingInfo where fldInwardItemId=@fldInwardItemId",
                Utils.ConvertFormCollectionToDictionary(collection));
                if (dataResult.Rows.Count > 0) {
                    result = dataResult.Rows[0]["fldApprovalStatus"].ToString();
                }
            } catch (Exception ex) {
                throw ex;
            }
            return result;
        }

        public bool CheckOldValueRejectReentry(string inwardItemId, string fieldName)
        {
            try
            {
                DataTable dataResult = dbContext.GetRecordsAsDataTable(String.Format("Select {0} from tblInwardItemInfo where fldInwardItemId=@fldInwardItemId", fieldName), new[] {
                    new SqlParameter("@fldInwardItemId", inwardItemId)
                });

                if (dataResult.Rows.Count > 0)
                {
                    string result = dataResult.Rows[0][fieldName].ToString();
                    if (StringUtils.Trim(result).Equals("")) {
                        return true;
                    }else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public int GetNextVerifySeqNo(string inwardItemId) {
            int result = 0;
            string stmt = "Select fldVerifySeq from tblInwardItemHistory where fldInwardItemId=@fldInwardItemId";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldInwardItemId", inwardItemId)
            });
            if (ds.Rows.Count > 0) {
                DataRow row = ds.Rows[0];
                result = StringUtils.convertToInt(row["fldVerifySeq"].ToString());
            }

            return result + 1;
        }

        public void InsertChequeHistory(FormCollection collection, string verifyAction, AccountModel currentUser, string taskId) {
            
            int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
            CurrentUser.Account.TaskId = taskId;
            //int nextVerifySeq = GetNextVerifySeqNo(collection["fldInwardItemId"]);

            Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();

            //Compulsory update for tblInwardItemHistory
            sqlChequeHistory.Add("fldQueue", taskId);
            sqlChequeHistory.Add("fldActionStatusId", nextHistorySecNo);
            sqlChequeHistory.Add("fldActionStatus", verifyAction);
            sqlChequeHistory.Add("fldUIC", collection["current_fldUIC"]);
            sqlChequeHistory.Add("fldInwardItemID", collection["fldInwardItemId"]);
            sqlChequeHistory.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlChequeHistory.Add("fldRemarks", string.IsNullOrEmpty(collection["textAreaRemarks"])? "Reject Reentry Manualy Marked": collection["textAreaRemarks"]);
            sqlChequeHistory.Add("fldTextAreaRemarks", string.IsNullOrEmpty(collection["textAreaRemarks"]) ? "Reject Reentry Manualy Marked" : collection["textAreaRemarks"]);
            sqlChequeHistory.Add("fldVerifySeq", "");
            sqlChequeHistory.Add("fldCreateUserID", currentUser.UserId);

            //If action is return / route , add reject code
            if ("R".Equals(verifyAction) || "B".Equals(verifyAction) || "J".Equals(verifyAction)) {
                sqlChequeHistory.Add("fldRejectCode", collection["txtReturnCode"]);
                sqlChequeHistory.Add("fldRejectCode2", collection["txtReturnCode2"]);
                sqlChequeHistory.Add("fldRejectCode3", collection["txtReturnCode3"]);


            }

            //Excute the command
            dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

            //Update sequence no
            //sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
            sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

            //Add to audit trail
            auditTrailDao.Log("Cheque Verification - Account Number: " + collection["current_fldAccountNumber"] + " Cheque Number: " + collection["current_fldChequeSerialNo"] + " UIC: " + collection["current_fldUIN"] + " Approval: " + verifyAction, CurrentUser.Account);
            //auditTrailDao.SecurityLog("Cheque Verification - Account Number: " + collection["current_fldAccountNumber"] + " Cheque Number: " + collection["current_fldChequeSerialNo"] + " UIC: " + collection["current_fldUIC"] + " Approval: " + verifyAction, "", taskId, CurrentUser.Account);
        }

        public void InsertChequeHistoryForApproveOrRejectAll(string verifyAction, AccountModel currentUser, string taskId, string rejectCode = "") {
            CurrentUser.Account.TaskId = taskId;
            string selectSql = "SELECT * FROM ";

            if ("A".Equals(verifyAction)) {
                selectSql += " View_ApproveAll";
            } else if ("R".Equals(verifyAction)) {
                selectSql += " View_ReturnAll";
            }

            DataTable dt = dbContext.GetRecordsAsDataTable(selectSql);

            foreach (DataRow row in dt.Rows) {
                
                int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int nextVerifySeq = GetNextVerifySeqNo(row["fldInwardItemId"].ToString());

                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();

                //Compulsory update for tblInwardItemHistory
                sqlChequeHistory.Add("fldQueue", taskId);
                sqlChequeHistory.Add("fldActionStatusId", nextHistorySecNo);
                sqlChequeHistory.Add("fldActionStatus", verifyAction);
                sqlChequeHistory.Add("fldUIC", row["fldUIC"].ToString());
                sqlChequeHistory.Add("fldInwardItemID", row["fldInwardItemId"].ToString());
                sqlChequeHistory.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlChequeHistory.Add("fldRemarks", "");
                sqlChequeHistory.Add("fldTextAreaRemarks", "");
                sqlChequeHistory.Add("fldVerifySeq", nextVerifySeq);
                sqlChequeHistory.Add("fldCreateUserID", currentUser.UserId);

                //If action is return / route , add reject code
                if ("R".Equals(verifyAction)) {
                    sqlChequeHistory.Add("fldRejectCode", rejectCode);
                }

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

                //Update sequence no
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

                //Add to audit trail
                auditTrailDao.Log("Cheque Verification - Account Number: " + row["fldAccountNumber"].ToString() + " Cheque Number: " + row["fldChequeSerialNo"].ToString() + " UIC: " + row["fldUIC"].ToString() + " Approval: " + verifyAction, CurrentUser.Account);
                //auditTrailDao.SecurityLog("Cheque Verification - Account Number: " + row["fldAccountNumber"].ToString() + " Cheque Number: " + row["fldChequeSerialNo"].ToString() + " UIC: " + row["fldUIC"].ToString() + " Approval: " + verifyAction, "", taskId, CurrentUser.Account);

            }


        }


        
            public string GetRejectCodeByRejectDesc(string rejectDesc) {
            string result = "";
            string stmt = "SELECT fldRejectCode from tblRejectMaster WHERE fldRejectDesc = @fldRejectDesc";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldRejectDesc", rejectDesc) });
            if (ds.Rows.Count > 0) {
                result = ds.Rows[0]["fldRejectCode"].ToString();
            }

            return result;
        }


        //Kai Hong 20170616
        //new function to new FirstChequeFromTheResult
        public Dictionary<string, string> LockAllChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem)
        {

            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;
            DataTable resultTable = new DataTable();
            string fldcleardate = "";
            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition + ")" : "";
            string lockcheq = mainCondition + lockCondition;
            //string sMySQL = string.Format("UPDATE top({0}) tblInwardItemInfoStatus SET fldAssignedUserId = @fldAssignedUserId, fldAssignedQueue=@fldAssignedQueue WHERE fldInwardItemId in ( SELECT TOP ({0}) fldInwardItemId FROM {1} WHERE ((fldAssignedUserId IS NULL OR fldAssignedUserId = @fldAssignedUserId) {4}) {2} {3} ) ", numberOfItem, sqlHolder.tableName, mainCondition, mainOrderBy, lockCondition);
            if (sqlParams.Count != 0)
            {
                foreach (var sqlParam in sqlParams)
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
					if (b == "@fldCapturingDate")
					{
						fldcleardate = a;
						a = "'" + a + "'";
					}
                    if (b == "@fldItemStatus")
                    {
                        a = "'" + a + "'";
                    }
                    lockcheq = lockcheq.Replace(b, a);

                }
                lockcheq = lockcheq.Replace("%' + ", "%");
                lockcheq = lockcheq.Replace(" + '%", "%");
            }
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", CurrentUser.Account.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
            sqlParameterNext.Add(new SqlParameter("@orderBy", mainOrderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", lockcheq));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@top", numberOfItem));

            if (pageConfig.TaskId == "306550")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockBranchConfirmationItem", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "306510")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockReviewInwardItem", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "318190" || pageConfig.TaskId == "318200" || pageConfig.TaskId == "318280" || pageConfig.TaskId == "318290" ||
                pageConfig.TaskId == "318350" || pageConfig.TaskId == "318360" || pageConfig.TaskId == "318390" || pageConfig.TaskId == "318400"
                 || pageConfig.TaskId == "318410")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuPPSLockInwardItem", sqlParameterNext.ToArray());
            }
            else
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockInwardItem", sqlParameterNext.ToArray());
            }
            //resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockInwardItem", sqlParameterNext.ToArray());
            //Return Null/Empty so that next cheque will be found as first item assigned           
            //return FirstChequeFromTheResultNew(pageConfig, collection, currentUser);
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            { 
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }
        }

        public Dictionary<string, string> LockChequeBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem)
        {
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;
            DataTable resultTable = new DataTable();
            string fldcleardate = "";
            string branchcode = "";
            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition + ")" : "";
            string lockcheq = mainCondition + lockCondition;
            if (sqlParams.Count != 0)
            {
                foreach (var sqlParam in sqlParams)
                {
                    string a = sqlParam.Value.ToString();
                    string b = sqlParam.ParameterName.ToString();
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
                    lockcheq = lockcheq.Replace(b, a);
                }
                lockcheq = lockcheq.Replace("%' + ", "%");
                lockcheq = lockcheq.Replace(" + '%", "%");
            }
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", collection["row_fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", CurrentUser.Account.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@branchcode", CurrentUser.Account.BranchCodes[0]));
            sqlParameterNext.Add(new SqlParameter("@orderBy", mainOrderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", lockcheq));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@top", 1));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockBranchInwardItemList", sqlParameterNext.ToArray());
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }
        }
  //bella 20201124
        public Dictionary<string, string> LockChequeVerification(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem)
        {

            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;
            DataTable resultTable = new DataTable();
            string fldcleardate = "";
            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition + ")" : "";
            string lockcheq = mainCondition + lockCondition;
            if (sqlParams.Count != 0)
            {
                foreach (var sqlParam in sqlParams)
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
                    lockcheq = lockcheq.Replace(b, a);

                }

                lockcheq = lockcheq.Replace("%' + ", "%");
                lockcheq = lockcheq.Replace(" + '%", "%");
            }
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", collection["row_fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@orderBy", mainOrderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", lockcheq));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@top", 1));


            resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockInwardItemList", sqlParameterNext.ToArray());
            //Return Null/Empty so that next cheque will be found as first item assigned           
            //return FirstChequeFromTheResultNew(pageConfig, collection, currentUser);
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }
        }
        public Dictionary<string, string> LockAllChequeBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem)
        {

            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;
            DataTable resultTable = new DataTable();
            string fldcleardate = "";
            string branchcode = "";
            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition + ")" : "";
            string lockcheq = mainCondition + lockCondition;
            //string sMySQL = string.Format("UPDATE top({0}) tblInwardItemInfoStatus SET fldAssignedUserId = @fldAssignedUserId, fldAssignedQueue=@fldAssignedQueue WHERE fldInwardItemId in ( SELECT TOP ({0}) fldInwardItemId FROM {1} WHERE ((fldAssignedUserId IS NULL OR fldAssignedUserId = @fldAssignedUserId) {4}) {2} {3} ) ", numberOfItem, sqlHolder.tableName, mainCondition, mainOrderBy, lockCondition);
            if (sqlParams.Count != 0)
            {
                foreach (var sqlParam in sqlParams)
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
                    lockcheq = lockcheq.Replace(b, a);

                }

                lockcheq = lockcheq.Replace("%' + ", "%");
                lockcheq = lockcheq.Replace(" + '%", "%");
            }
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", CurrentUser.Account.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@branchcode", CurrentUser.Account.BranchCodes[0].ToString().Trim()));
            sqlParameterNext.Add(new SqlParameter("@conBranchCode", CurrentUser.Account.BranchCodes[0].ToString().Trim()));
            sqlParameterNext.Add(new SqlParameter("@iBranchCode", CurrentUser.Account.BranchCodes[1].ToString().Trim()));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
            sqlParameterNext.Add(new SqlParameter("@orderBy", mainOrderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", lockcheq));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@top", numberOfItem));


           
           resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockBranchInwardItem", sqlParameterNext.ToArray());

           
            //Return Null/Empty so that next cheque will be found as first item assigned           
            //return FirstChequeFromTheResultNew(pageConfig, collection, currentUser);
            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }
        }

        //kai hong 20170616
        //new function to call new consturct sql function to get inward id 
        //purpose is to call function with simplified query because original query to complicated and to heavy
        public Dictionary<string, string> FindItemByInwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null)
        {
            string resultInwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = new DataTable();
            Dictionary<string, string> dicCurrentUser = pageConfig.CurrentUser;

            if (!string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultInwardItemId = collection["NextValue"].ToString();
                    // xx start 20210708
                    if (resultInwardItemId == "")
                    {
                        resultInwardItemId = collection["fldInwardItemId"].ToString();
                    }
                    // xx end 20210708
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultInwardItemId = collection["PreviousValue"].ToString();
                }
                else if ("Error".Equals(nextOrPrevious))
                {
                    resultInwardItemId = inwardItemId;
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                  sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNew(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, dicCurrentUser, resultInwardItemId);
                resultTable = dbContext.GetRecordsAsDataTableSP(sqlDetails.tableName, sqlDetails.sqlParams.ToArray());

            }
            else
            {
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemNew(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, dicCurrentUser, inwardItemId);
                resultTable = dbContext.GetRecordsAsDataTableSP(sqlDetails.tableName, sqlDetails.sqlParams.ToArray());
            }

            if (resultTable == null)
            {
                return null;
            }
            if (resultTable.Rows.Count == 0)
            {
                return null;
            }
            else
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }

        }

        // kai hong 20170616
        // new function to for simplified query
        public Dictionary<string, string> FirstChequeFromTheResultNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdNew(pageConfig, collection, currentUser);
        }

        // kai hong 20170616
        // new function to for simplified query next function
        public Dictionary<string, string> NextChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
        }

        //Azim 20210620
        public Dictionary<string, string> NextChequeNoLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdNOLOCK(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
        }

        //Azim 20210620
        public Dictionary<string, string> NextChequeNoLockBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdBranchNOLOCKNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
        }

        // kai hong 20170616
        // new function to for simplified query previous function
        public Dictionary<string, string> PrevChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Prev");
        }

        public Dictionary<string, string> PrevChequeNoLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdNOLOCK(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Prev");
        }

        public Dictionary<string, string> PrevChequeNoLockBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByInwardItemIdBranchNOLOCKNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Prev");
        }

        public void LockThisChequeBranch(string inwardItemId, AccountModel currentUser)
        {
            string sMySQL = "UPDATE tblPendingInfo set fldAssignedUserId = @fldAssignedUserId where fldInwardItemId = @inwardItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", inwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });
        }
        public DataTable SubmissionTime(string spickC)
        {
            DataTable ds = new DataTable();
            string stmtKL = "select  convert(char(8), fldKLCutOffTime, 108) as EndTime, convert(char(8), fldKLTimeFrom, 108) as StartTime from tblCutOffTime";
            string stmtJB = "select  convert(char(8), fldJBCutOffTime, 108) as EndTime, convert(char(8), fldJBTimeFrom, 108) as StartTime from tblCutOffTime";
            string stmtPP = "select  convert(char(8), fldPPCutOffTime, 108) as EndTime, convert(char(8), fldPPTimeFrom, 108) as StartTime from tblCutOffTime";
            string spick = "";
            DataTable results = new DataTable();
            if (spickC.Trim() == "BOP")
            {
                spick = stmtKL;
            }
            else if (spickC.Trim() == "JB")
            {
                spick = stmtJB;
            }
            else
            {
                spick = stmtPP;
            }
            return dbContext.GetRecordsAsDataTable(spick);
        }

        public DataTable GetMicr()
        {
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            dt = dbContext.GetRecordsAsDataTableSP("spcgGetMicrFormat", sqlParameterNext.ToArray());

            return dt;
        }
    }
}