﻿using INCHEQS.Helpers;
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
//using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using static INCHEQS.Helpers.ImageHelper;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.CommonOutwardItem
{
    public class CommonOutwardItemDao : ICommonOutwardItemDao
    {

        private readonly IPageConfigDaoOCS pageConfigDao;
        private readonly ISequenceDao sequenceDao;
        private readonly OCSDbContext dbContext;
        private readonly IAuditTrailDao auditTrailDao;
        ISystemProfileDao systemProfileDao;

        public CommonOutwardItemDao(ISystemProfileDao systemProfileDao, ISequenceDao sequenceDao, IPageConfigDaoOCS pageConfigDao, OCSDbContext dbContext, IAuditTrailDao auditTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        public void LockThisCheque(string OutwardItemId, AccountModel currentUser)
        {
            //Lock inwardItemForCurrentUser
            string sMySQL = "UPDATE tblItemInfo set fldLockUser = @fldAssignedUserId where fldItemId = @fldItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@fldItemId", OutwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });

        }
        public void LockThisChequebyTransNumber(string OutwardItemId, AccountModel currentUser)
        {
            //Lock inwardItemForCurrentUser
            string sMySQL = "UPDATE tblItemInfo set fldLockUser = @fldAssignedUserId where fldTransNo = @fldItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@fldItemId", OutwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });

        }
        
        public bool CheckStatus(string OutwardItemId, AccountModel currentUser)
        {
            string userid;
            string sMySQL = "Select fldlockuser from tblItemInfo where fldItemId = @fldItemId";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                    new SqlParameter("@fldItemId", OutwardItemId)
                });
            if (dataResult.Rows.Count > 0)
            {
                userid = dataResult.Rows[0]["fldlockuser"].ToString();
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

        public bool CheckStatusBalancing(string OutwardItemId, AccountModel currentUser)
        {
            string userid;
            string sMySQL = "Select fldlockuser from tblItemInfo where fldTransNo = @fldItemId";
            DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                    new SqlParameter("@fldItemId", OutwardItemId)
                });
            if (dataResult.Rows.Count > 0)
            {
                userid = dataResult.Rows[0]["fldlockuser"].ToString();
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

        public bool CheckStatusBranch(string OutwardItemId, AccountModel currentUser)
        {
            string userid;
            //string sMySQL = "Select fldAssignedUserId from tblPendingInfo where fldInwardItemId = @inwardItemId";
            //DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
            //        new SqlParameter("@inwardItemId", OutwardItemId)
            //    });
            //if (dataResult.Rows.Count > 0)
            //{
            //    userid = dataResult.Rows[0]["fldAssignedUserId"].ToString();
            //    if ((userid == "") || (userid == null))
            //    {
            //        return true;
            //    }
            //}
            //else
            //{
            //    userid = "";
            //    return true;
            //}

            //if (userid != currentUser.UserId)
            //{
            //    return false;
            //}
            //else
            //{
            return true;
            //}

        }

        public void LockThisChequeHistory(string OutwardItemId, AccountModel currentUser)
        {
            //Lock inwardItemForCurrentUser
            string sMySQL = "UPDATE tblInwardItemInfoStatusH set fldAssignedUserId = @fldAssignedUserId where fldInwardItemId = @inwardItemId";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@inwardItemId", OutwardItemId),
                    new SqlParameter("@fldAssignedUserId", currentUser.UserId)
                });

        }

        public Dictionary<string, string> LockAllCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem)
        {

            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            //Lock inwardItemForCurrentUser
            SearchPageHelper.SqlDetails sqlHolder = SearchPageHelper.ConstructSqlFromConfigTableSql(pageConfig.toPageSqlConfig(), configTable, collection);
            List<SqlParameter> sqlParams = sqlHolder.sqlParams;

            string mainCondition = !string.IsNullOrEmpty(sqlHolder.conditionAsSqlString) ? " AND " + sqlHolder.conditionAsSqlString : "";
            string mainOrderBy = !string.IsNullOrEmpty(sqlHolder.orderBySql) ? " ORDER BY " + sqlHolder.orderBySql : "";
            string lockCondition = !string.IsNullOrEmpty(pageConfig.SqlLockCondition) ? " AND (" + pageConfig.SqlLockCondition + ")" : "";

            string sMySQL = string.Format("UPDATE top({0}) tblItemInfo SET fldLockUser = @fldAssignedUserId WHERE flditemid in ( SELECT TOP ({0}) flditemid FROM {1} WHERE ((fldLockUser IS NULL OR fldLockUser = @fldAssignedUserId) {4}) {2} {3} ) ", numberOfItem, sqlHolder.tableName, mainCondition, mainOrderBy, lockCondition);

            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            dbContext.ExecuteNonQuery(sMySQL, sqlParams.ToArray());
            //Return Null/Empty so that next cheque will be found as first item assigned           
            return FirstChequeFromTheResult(pageConfig, collection, currentUser);
        }



        public Dictionary<string, string> FindItemByOutwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            string itemid = "";
            if (pageConfig.TaskId == "311117")
            {
                itemid = "fldTransNo";
            }
            else
            {
                itemid = "fldItemId";
            }
            sqlDetails = SearchPageHelper.ConstructSqlForItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            //if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious)) {
            // if ("Next".Equals(nextOrPrevious)) {
            //resultOutwardItemId = resultTable.Rows[0]["NextValue"].ToString();
            //} else if ("Prev".Equals(nextOrPrevious)) {
            //resultOutwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
            //}

            // //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
            //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, "fldItemID", currentUser, resultOutwardItemId);
            //resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            //}

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
        public Dictionary<string, string> FindItemByOutwardItemIdLocked(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            string itemid = "";
            if (pageConfig.TaskId == "311117")
            {
                itemid = "fldTransNo";
            }
            else
            {
                itemid = "fldItemId";
            }
            sqlDetails = SearchPageHelper.ConstructSqlForItemOCSLocked(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

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
        public Dictionary<string, string> FindOutwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;

            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultOutwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            }

            DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            //}
            return result;


        }

        public Dictionary<string, string> FindOutwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;

            sqlDetails = SearchPageHelper.ConstructSqlForSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());


            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForSearchItem(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultOutwardItemId);
                resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());

            }

            DataRow row = resultTable.AsEnumerable().FirstOrDefault();
            Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                    .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
            //}
            return result;


        }

        public Dictionary<string, string> FirstChequeFromTheResult(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            return FindItemByOutwardItemId(pageConfig, collection, currentUser);
            //return FindInwardItemId(pageConfig, collection, currentUser);

        }


        public Dictionary<string, string> NextCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            if (pageConfig.TaskId == "311117")
            {
                return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser, collection["current_fldTransNo"], "Next");
            }
            else
            {
                return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser, collection["current_fldItemId"], "Next");
            }
        }


        public Dictionary<string, string> PrevCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            if (pageConfig.TaskId == "311117")
            {
                return FindItemByOutwardItemIdPrev(pageConfig, collection, currentUser, collection["current_fldTransNo"], "Prev");
            }
            else
            {
                return FindItemByOutwardItemIdPrev(pageConfig, collection, currentUser, collection["current_fldItemId"], "Prev");
            }
            

        }

        public Dictionary<string, string> ErrorCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            return FindItemByOutwardItemId(pageConfig, collection, currentUser, collection["fldItemId"], "Error");

        }

        public Dictionary<string, string> ErrorChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {

            return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Error");

        }

        public int UnlockAllAssignedForUser(AccountModel currentUser)
        {
            string sMySQL = "update tblItemInfo set fldLockUser = null where fldLockUser = @userId ;";
            return dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@userId", currentUser.UserId) });
        }

        public int UnlockAllAssignedForBranchUser(AccountModel currentUser)
        {
            string sMySQL = "update tblPendingInfo set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId ;";
            sMySQL = sMySQL + "update tblInwardItemInfoStatus set fldAssignedUserId = null,fldAssignedQueue = null where fldAssignedUserId = @userId ;";
            sMySQL = sMySQL + "delete from  tblBranchVerificationLock where fldAssignedUserId = @userId ;";
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
            //string sMySQL = "Select count(1) as totcount from tblInwardItemInfo where datediff(d,fldCleardate,@fldCleardate)=0 and fldIssueBankCode = @fldBankCode";
            //DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
            //    new SqlParameter("@fldBankCode", currentUser.BankCode),
            //    new SqlParameter("@fldCleardate", DateUtils.formatDateToSql(date))
            //});

            //if (dataResult.Rows.Count > 0)
            //{
            //    DataRow row = dataResult.Rows[0];
            //    countrow = (int)row["totcount"];

            //}
            //if (countrow > 0)
            //{
            //    return 1;
            //}
            //else
            //{
            return 0;
            //}

        }

        public int CheckMainTableOutward(AccountModel currentUser, string OutwardItemId)
        {
            int countrow = 0;
            //string sMySQL = "Select count(1) as totcount from tblInwardItemInfo where fldInwardItemId=@fldInwardId and fldIssueBankCode = @fldBankCode";
            //DataTable dataResult = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
            //    new SqlParameter("@fldBankCode", currentUser.BankCode),
            //    new SqlParameter("@fldInwardId", OutwardItemId)
            //});

            //if (dataResult.Rows.Count > 0)
            //{
            //    DataRow row = dataResult.Rows[0];
            //    countrow = (int)row["totcount"];

            //}
            //if (countrow > 0)
            //{
            //    return 1;
            //}
            //else
            //{
            return 0;
            //}

        }

        public DataTable ChequeHistory(FormCollection collection)
        {
            return dbContext.GetDataTableFromSqlWithParameter("Select fldCheckDigit,fldSerial,fldBankCode,fldBranchCode,fldStateCode,fldIssuerAccNo,fldTCCode,fldamount from tblMICRRepair where flditemid = @fldItemId order by fldSequenceNo desc",
                Utils.ConvertFormCollectionToDictionary(collection));
        }


        //public DataTable ChequeRepair(string OutwardItemId)
        //{
        //    DataTable ds = new DataTable();
        //    string stmt = "Select fldOriCheckDigit,fldOriSerial,fldOriBankCode,fldOriBranchCode,fldOriStateCode,fldOriIssuerAccNo,fldOriTCCode from tblMICRRepair where fldItemID = @fldItemID order by fldSequenceNo desc";
        //    ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldItemID", OutwardItemId) });

        //    return ds;
        //}

        public DataTable ChequeHistoryH(FormCollection collection)
        {
            return dbContext.GetDataTableFromSqlWithParameter("Select * from View_ChequeHistoryH where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp",
                Utils.ConvertFormCollectionToDictionary(collection));
        }

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

        public DataTable FindAllTransCode()
        {
            return dbContext.GetDataTableFromSqlWithParameter("Select * from tblTransMaster");
        }


        public bool CheckIfRecordUpdatedOrDeleted(string OutwardItemId, string updateTimestamp)
        {
            string sql = "Select fldUpdateTimeStamp from tblItemInfo where fldItemId = @itemId " +
                "and isnull(fldUpdateTimeStamp,'') = ''";
            //string infoStatusSql = "Select fldUpdateTimeStamp from tblItemInfoStatus where fldItemId = @itemId  " +
            //    "AND fldUpdateTimeStamp = @fldUpdateTimestamp";
            string infoStatusSql = "Select fldLockUser from tblItemInfo where fldItemId = @itemid  " +
               "AND fldLockUser = @userid";
            if (dbContext.CheckExist(sql, new[]{ new SqlParameter("@itemId", OutwardItemId) ,
                    new SqlParameter("@fldUpdateTimestamp", updateTimestamp==null ? DBNull.Value : (object)DateUtils.formatTimeStampFromSql(updateTimestamp))})
                ||
                 //!dbContext.CheckExist(infoStatusSql, new[]{ new SqlParameter("@itemId", OutwardItemId) ,
                 //    new SqlParameter("@fldUpdateTimestamp", updateTimestamp==null ? DBNull.Value : (object)DateUtils.formatTimeStampFromSql(updateTimestamp)) })
                 dbContext.CheckExist(infoStatusSql, new[]{ new SqlParameter("@itemid", OutwardItemId) ,
                    new SqlParameter("@userid", Convert.ToInt32(CurrentUser.Account.UserId))})
                )
            {

                return false;
            }
            else
            {

                return true;
            }
        }
        //add by shamil to validate check is lock by other user march 29 2017
        public bool CheckLockedCheck(string OutwardItemId, string assigneduserid)
        {
            string sql = "Select fldInwardItemId from view_items where fldInwardItemId = @fldInwardItemId and fldAssignedUserId " + "=@fldAssignedUserId";
            if (dbContext.CheckExist(sql, new[]{ new SqlParameter("@fldInwardItemId", OutwardItemId) ,
                    new SqlParameter("@fldAssignedUserId", assigneduserid)}))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetModifiedField(FormCollection collection)
        {
            string result = "";
            try
            {
                DataTable dataResult = dbContext.GetDataTableFromSqlWithParameter("Select fldModifiedFields from tblInwardItemInfoStatus where fldInwardItemId=@fldInwardItemId",
                Utils.ConvertFormCollectionToDictionary(collection));
                if (dataResult.Rows.Count > 0)
                {
                    result = dataResult.Rows[0]["fldModifiedFields"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public string GetBranchItemStatus(FormCollection collection)
        {
            string result = "";
            try
            {
                DataTable dataResult = dbContext.GetDataTableFromSqlWithParameter("Select fldApprovalStatus from tblPendingInfo where fldInwardItemId=@fldInwardItemId",
                Utils.ConvertFormCollectionToDictionary(collection));
                if (dataResult.Rows.Count > 0)
                {
                    result = dataResult.Rows[0]["fldApprovalStatus"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public bool CheckOldValueRejectReentry(string OutwardItemId, string fieldName)
        {
            try
            {
                DataTable dataResult = dbContext.GetRecordsAsDataTable(String.Format("Select {0} from tblInwardItemInfo where fldInwardItemId=@fldInwardItemId", fieldName), new[] {
                    new SqlParameter("@fldInwardItemId", OutwardItemId)
                });

                if (dataResult.Rows.Count > 0)
                {
                    string result = dataResult.Rows[0][fieldName].ToString();
                    if (StringUtils.Trim(result).Equals(""))
                    {
                        return true;
                    }
                    else
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

        public int GetNextVerifySeqNo(string OutwardItemId)
        {
            int result = 0;
            string stmt = "Select fldVerifySeq from tblInwardItemHistory where fldInwardItemId=@fldInwardItemId";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldInwardItemId", OutwardItemId)
            });
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                result = StringUtils.convertToInt(row["fldVerifySeq"].ToString());
            }

            return result + 1;
        }

        public void InsertChequeHistory(FormCollection collection, string verifyAction, AccountModel currentUser, string taskId)
        {

            int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
            //int nextVerifySeq = GetNextVerifySeqNo(collection["fldInwardItemId"]);

            Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();

            //Compulsory update for tblInwardItemHistory
            sqlChequeHistory.Add("fldQueue", taskId);
            sqlChequeHistory.Add("fldActionStatusId", nextHistorySecNo);
            sqlChequeHistory.Add("fldActionStatus", verifyAction);
            sqlChequeHistory.Add("fldUIC", collection["current_fldUIC"]);
            sqlChequeHistory.Add("fldInwardItemID", collection["fldInwardItemId"]);
            sqlChequeHistory.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlChequeHistory.Add("fldRemarks", collection["remarkField"]);
            sqlChequeHistory.Add("fldTextAreaRemarks", collection["textAreaRemarks"]);
            sqlChequeHistory.Add("fldVerifySeq", "");
            sqlChequeHistory.Add("fldCreateUserID", currentUser.UserId);

            //If action is return / route , add reject code
            if ("R".Equals(verifyAction) || "B".Equals(verifyAction) || "J".Equals(verifyAction))
            {
                sqlChequeHistory.Add("fldRejectCode", collection["new_rejectCode"]);
            }

            //Excute the command
            dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

            //Update sequence no
            //sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
            sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

            //Add to audit trail
            //auditTrailDao.Log("Cheque Verification - Account Number: " + collection["current_fldAccountNumber"] + " Cheque Number: " + collection["current_fldChequeSerialNo"] + " UIC: " + collection["current_fldUIN"] + " Approval: " + verifyAction, CurrentUser.Account);
            auditTrailDao.SecurityLog("Cheque Verification - Account Number: " + collection["current_fldAccountNumber"] + " Cheque Number: " + collection["current_fldChequeSerialNo"] + " UIC: " + collection["current_fldUIN"] + " Approval: " + verifyAction, "", taskId, CurrentUser.Account);
        }

        public void InsertChequeHistoryForApproveOrRejectAll(string verifyAction, AccountModel currentUser, string taskId, string rejectCode = "")
        {

            string selectSql = "SELECT * FROM ";

            if ("A".Equals(verifyAction))
            {
                selectSql += " View_ApproveAll";
            }
            else if ("R".Equals(verifyAction))
            {
                selectSql += " View_ReturnAll";
            }

            DataTable dt = dbContext.GetRecordsAsDataTable(selectSql);

            foreach (DataRow row in dt.Rows)
            {

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
                if ("R".Equals(verifyAction))
                {
                    sqlChequeHistory.Add("fldRejectCode", rejectCode);
                }

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

                //Update sequence no
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

                //Add to audit trail
               // auditTrailDao.Log("Cheque Verification - Account Number: " + row["fldAccountNumber"].ToString() + " Cheque Number: " + row["fldChequeSerialNo"].ToString() + " UIC: " + row["fldUIC"].ToString() + " Approval: " + verifyAction, CurrentUser.Account);
                auditTrailDao.SecurityLog("Cheque Verification - Account Number: " + row["fldAccountNumber"].ToString() + " Cheque Number: " + row["fldChequeSerialNo"].ToString() + " UIC: " + row["fldUIC"].ToString() + " Approval: " + verifyAction, "", taskId, CurrentUser.Account);


            }


        }

        public string GetRejectCodeByRejectDesc(string rejectDesc)
        {
            string result = "";
            string stmt = "SELECT fldRejectCode from tblRejectMaster WHERE fldRejectDesc = @fldRejectDesc";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldRejectDesc", rejectDesc) });
            if (ds.Rows.Count > 0)
            {
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
            //string fldcleardate = "";
            DateTime fldcleardate;
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
                        //fldcleardate = a;
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
                        //fldcleardate = a;
                        a = "'" + a + "'";
                    }

                    lockcheq = lockcheq.Replace(b, a);

                }
                lockcheq = lockcheq.Replace("%'+", "%");
                lockcheq = lockcheq.Replace("+'%", "%");
            }
            string strBankCode = currentUser.BankCode;
            resultTable = dbContext.GetRecordsAsDataTable("select TOP 1 fldprocessdate from tblprocessdate where fldstatus = 'Y' and fldbankcode = " + strBankCode + "  order by fldprocessdate desc");
            //fldcleardate = (resultTable.Rows[0][0].ToString());
            fldcleardate = Convert.ToDateTime(resultTable.Rows[0][0].ToString());
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", CurrentUser.Account.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldcleardate));
            sqlParameterNext.Add(new SqlParameter("@orderBy", mainOrderBy));
            sqlParameterNext.Add(new SqlParameter("@condition", DBNull.Value)); //lockcheq//));
            sqlParameterNext.Add(new SqlParameter("@taskid", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@top", numberOfItem));

            //if (pageConfig.TaskId == "306550")
            //{
            //resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockBranchConfirmationItem", sqlParameterNext.ToArray());
            //}
            if (pageConfig.TaskId == "311100")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemchequeamount", sqlParameterNext.ToArray());
            }
            //Michelle
            else if (pageConfig.TaskId == "322200")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemchequedateamount", sqlParameterNext.ToArray());
            }
            //Michelle
            else if (pageConfig.TaskId == "311113")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemDataEntry", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "311117")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuItemLockBalAmntEntry", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "999960")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemchequeaccount", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "999970")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemchequeaccount", sqlParameterNext.ToArray());
            }
            else if (pageConfig.TaskId == "999100")
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spculockoutwarditemmicrrepair", sqlParameterNext.ToArray());
            }
            else
            {
                resultTable = dbContext.GetRecordsAsDataTableSP("spcuLockOutwardItem", sqlParameterNext.ToArray());
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
                lockcheq = lockcheq.Replace("%'+", "%");
                lockcheq = lockcheq.Replace("+'%", "%");
            }
            sqlParams.Add(new SqlParameter("@fldAssignedUserId", currentUser.UserId));
            sqlParams.Add(new SqlParameter("@fldAssignedQueue", pageConfig.TaskId));

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAssignedUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@currentUserBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@currentUserAbbr", CurrentUser.Account.UserAbbr));
            sqlParameterNext.Add(new SqlParameter("@branchcode", CurrentUser.Account.BranchCodes[0]));
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
        public Dictionary<string, string> FindItemByOutwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
            string OrderBy = "";
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;

            //string sMySQL = "update tbliteminfo set fldlockuser = @user where flditemid = (select top 1 flditemid from tbliteminfo where (fldLockUser IS NULL or fldLockUser = '' or fldLockUser = 0 ) and fldItemID > @id)";
            //dbContext.ExecuteNonQuery(sMySQL, new[] {
            //new SqlParameter("@id", OutwardItemId),
            //new SqlParameter("@user", currentUser.UserId)
            //});
            string itemid = "";
            if (pageConfig.TaskId == "311117")
            {
                itemid = "fldTransNo";
            }
            else
            {
                itemid = "fldItemId";
            }
            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }

                //sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItem2(pageConfig.toPageSqlConfig(), configTable, collection, "fldInwardItemId", currentUser, resultInwardItemId, OrderByNextPrev, nextOrPrevious);
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, resultOutwardItemId);
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
            if ((pageConfig.TaskId == "311100" || pageConfig.TaskId == "322200") && resultTable.Rows[0]["totalunverified"].ToString() == "0")
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
        public Dictionary<string, string> FindItemByOutwardItemIdPrev(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string OutwardItemId = null, string nextOrPrevious = null)
        {
            string resultOutwardItemId = "";
#pragma warning disable CS0219 // The variable 'OrderBy' is assigned but its value is never used
            string OrderBy = "";
#pragma warning restore CS0219 // The variable 'OrderBy' is assigned but its value is never used
            ConfigTable configTable = pageConfigDao.GetConfigTable(pageConfig.TaskId, pageConfig.ViewOrTableName);
            SearchPageHelper.SqlDetails sqlDetails;
            DataTable resultTable = null;
            string sMySQL = "update tbliteminfo set fldlockuser = @user where " +
                            "fldItemId = (select top 1 fldItemId from tbliteminfo where (fldLockUser IS NULL " +
                            "or fldLockUser = 0 ) and fldItemId < @id )";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@id", OutwardItemId),
                    new SqlParameter("@user", currentUser.UserId)
                });
            string itemid = "";
            if (pageConfig.TaskId == "311117")
            {
                itemid = "fldTransNo";
            }
            else
            {
                itemid = "fldItemId";
            }
            sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, OutwardItemId);
            resultTable = dbContext.GetRecordsAsDataTable(sqlDetails.sql, sqlDetails.sqlParams.ToArray());
            if (resultTable.Rows.Count > 0 && !string.IsNullOrEmpty(nextOrPrevious))
            {
                if ("Next".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["NextValue"].ToString();
                }
                else if ("Prev".Equals(nextOrPrevious))
                {
                    resultOutwardItemId = resultTable.Rows[0]["PreviousValue"].ToString();
                }
                sqlDetails = SearchPageHelper.ConstructSqlForNextAndPreviousItemOCS(pageConfig.toPageSqlConfig(), configTable, collection, itemid, currentUser, resultOutwardItemId);
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
        }

        // kai hong 20170616
        // new function to for simplified query
        public Dictionary<string, string> FirstChequeFromTheResultNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser);
        }

        // kai hong 20170616
        // new function to for simplified query next function
        public Dictionary<string, string> NextChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Next");
        }

        // kai hong 20170616
        // new function to for simplified query previous function
        public Dictionary<string, string> PrevChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser)
        {
            return FindItemByOutwardItemIdNew(pageConfig, collection, currentUser, collection["fldInwardItemId"], "Prev");
        }

        public async Task<List<ChequeRepairHistoryModel>> GetChequeHistoryAsync(string itemid)
        {
            return await Task.Run(() => GetChequeHistory(itemid));
        }

        public async Task<List<SearchHistoryModel>> GetSearchHistoryAsync(string itemid)
        {
            return await Task.Run(() => GetSearchHistory(itemid));
        }

        public List<SearchHistoryModel> GetSearchHistory(string itemid)
        {
            List<SearchHistoryModel> result = new List<SearchHistoryModel>();
            string stmt = "select ii.fldItemId,ii.fldItemInitialID,ii.fldItemType,ih.fldTransNo,ih.fldActionStatus,ih.fldRemarks,ih.fldCreateUserId,ih.fldcreatetimestamp from tbliteminfo ii join tblOutwardItemHistory ih on ii.fldtransno=ih.fldtransno where ii.flditemid=@id order by ii.fldItemID desc";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@id", itemid) });
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    SearchHistoryModel History = new SearchHistoryModel();
                    History.fldItemID = item["fldItemId"].ToString();
                    History.fldItemInitialID = item["fldItemInitialID"].ToString();
                    History.fldItemType = item["fldItemType"].ToString();
                    History.fldTransNo = item["fldTransNo"].ToString();
                    History.fldActionStatus = item["fldActionStatus"].ToString();
                    History.fldRemarks = item["fldRemarks"].ToString();
                    History.fldCreateUserId = item["fldCreateUserId"].ToString();
                    History.fldcreatetimestamp = item["fldcreatetimestamp"].ToString();
                    result.Add(History);
                }
                return result;
            }
            return null;
        }

        public async Task<List<BalancingHistoryModel>> GetBalancingHistoryAsync(string itemid)
        {
            return await Task.Run(() => GetBalancingHistory(itemid));
        }

        public Dictionary<string, string> GetBalancingItemsAsync(string itemid)
        {
            DataTable resultTable = new DataTable();
            string stmt = "select * from view_TransactionBalancingDetail where fldTransNo = @id order by fldItemID";
            resultTable = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@id", itemid) });
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.AsEnumerable().FirstOrDefault();
                Dictionary<string, string> result = row.Table.Columns.Cast<DataColumn>()
                        .ToDictionary(col => col.ColumnName, col => row[col.Ordinal].ToString());
                return result;
            }
            else
            {
                return null;
            }
        }

        public List<BalancingHistoryModel> GetBalancingHistory(string itemid)
        {
            List<BalancingHistoryModel> result = new List<BalancingHistoryModel>();
            string stmt = "select ISNULL(ini.flduic,'') as flduic,inf.fldItemId,inf.fldItemInitialID,inf.fldItemType,inf.fldBankCode,IsNull(inf.fldPVaccNo, '0') As fldPVaccNo,inf.fldRemark,isnull(inf.fldAmount,0) as fldAmount,case when inf.fldItemType in ('C') then 'Maker' else 'Checker' End as CheckerMaker,inf.fldSerial from tbliteminfo inf inner join tblIteminitial ini on inf.flditeminitialid = ini.flditeminitialid where inf.fldTransNo = @id order by inf.fldItemID desc";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@id", itemid) });
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    BalancingHistoryModel History = new BalancingHistoryModel();
                    History.fldItemID = item["fldItemId"].ToString();
                    History.fldItemInitialID = item["fldItemInitialID"].ToString();
                    History.fldItemType = item["fldItemType"].ToString();
                    History.fldBankCode = item["fldBankCode"].ToString();
                    History.fldPVaccNo = item["fldPVaccNo"].ToString();
                    if (item["fldAmount"] == DBNull.Value)
                    {
                        History.fldAmount = "0.00";
                    }
                    else
                    {
                        History.fldAmount = item["fldamount"].ToString();
                    }
                    History.CheckerMaker = item["CheckerMaker"].ToString();
                    History.fldSerial = item["fldSerial"].ToString();
                    History.flduic = item["flduic"].ToString();
                    result.Add(History);
                }
                return result;
            }
            return null;
        }
        public List<ChequeRepairHistoryModel> GetChequeHistory(string itemid)
        {
            List<ChequeRepairHistoryModel> result = new List<ChequeRepairHistoryModel>();
            string stmt = "SELECT mr.fldsequenceno, mr.fldItemId, mr.flditemtype, mr.fldcheckdigit, mr.fldoricheckdigit, mr.fldserial, mr.fldoriserial, mr.fldbankcode, mr.fldoribankcode, mr.fldstatecode, mr.fldoristatecode, mr.fldbranchcode, mr.fldoribranchcode, mr.fldissueraccno, mr.fldoriissueraccno, mr.fldtccode, mr.fldoritccode, mr.fldremark, mr.fldoriremark, mr.fldamount AS fldamount, mr.fldcreatetimestamp, mr.fldpvaccno, mr.fldorilocation, mr.fldlocation, mr.fldoritype,  mr.fldtype, case when mr.fldreasoncode = '430' then 'Rejected' else mr.fldreasoncode end as fldreasoncode, um.flduserabb, mr.fldpvaccno FROM tblmicrrepair mr " +
                " Join tbliteminfo itm ON itm.fldItemId = mr.fldItemId left outer join tblusermaster um on mr.fldupdateuserid = um.flduserid where mr.fldItemId = @id order by fldSequenceNo";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@id", itemid) });
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    ChequeRepairHistoryModel History = new ChequeRepairHistoryModel();
                    History.fldOriCheckDigit = item["fldOriCheckDigit"].ToString();
                    History.fldOriSerial = item["fldOriSerial"].ToString();
                    History.fldOriBankCode = item["fldOriBankCode"].ToString();
                    History.fldOriBranchCode = item["fldOriBranchCode"].ToString();
                    History.fldOriStateCode = item["fldOriStateCode"].ToString();
                    History.fldOriIssuerAccNo = item["fldOriIssuerAccNo"].ToString();
                    History.fldOriTCCode = item["fldOriTCCode"].ToString();
                    History.fldOriRemark = item["fldOriRemark"].ToString();
                    History.fldCheckDigit = item["fldCheckDigit"].ToString();
                    History.fldSerial = item["fldSerial"].ToString();
                    History.fldBankCode = item["fldBankCode"].ToString();
                    History.fldBranchCode = item["fldBranchCode"].ToString();
                    History.fldStateCode = item["fldStateCode"].ToString();
                    History.fldIssuerAccNo = item["fldIssuerAccNo"].ToString();
                    History.fldTCCode = item["fldTCCode"].ToString();
                    History.fldRemark = item["fldRemark"].ToString();
                    //History.fldAmount = item["fldamount"] == DBNull.Value ? 0 : Convert.ToDouble(item["fldamount"]);
                    if (item["fldamount"] == DBNull.Value)
                    {
                        History.fldAmount = "0.00";
                    }
                    else
                    {
                        History.fldAmount = item["fldamount"].ToString();
                    }
                    History.fldCreateTimeStamp = item["fldCreateTimeStamp"].ToString();
                    History.fldType = item["fldtype"].ToString();
                    History.fldLocation = item["fldlocation"].ToString();
                    History.fldReasonCode = item["fldreasoncode"].ToString();
                    History.fldUpdateUser = item["flduserabb"].ToString();
                    History.fldPayeeAccNo = item["fldpvaccno"].ToString();
                    result.Add(History);

                }
                return result;
            }
            return null;
        }
        public async Task<List<ChequeRepairHistorySearchModel>> GetChequeHistoryAsyncSearch(string itemid)
        {
            return await Task.Run(() => GetChequeHistorySearch(itemid));
        }
        public List<ChequeRepairHistorySearchModel> GetChequeHistorySearch(string itemid)
        {
            List<ChequeRepairHistorySearchModel> result = new List<ChequeRepairHistorySearchModel>();
            string stmt = "SELECT fldsequenceno, mr.fldItemId, mr.flditemtype, mr.fldcheckdigit, fldoricheckdigit, mr.fldserial,fldoriserial," +
                " mr.fldbankcode, fldoribankcode, mr.fldstatecode, fldoristatecode, mr.fldbranchcode,fldoribranchcode, mr.fldissueraccno, " +
                "fldoriissueraccno, mr.fldtccode, fldoritccode, mr.fldremark, fldoriremark, mr.fldamount::numeric(18, 2) AS fldamount," +
                " mr.fldpvaccno, fldorilocation, mr.fldlocation, fldoritype, mr.fldtype, itm.fldcreatetimestamp,um.flduserabb from tblmicrrepair " +
                "mr Join tbliteminfo itm ON itm.flditemid = mr.flditemid Join tblusermaster um ON um.flduserid = itm.fldupdateuserid where " +
                "mr.flditemid = @id::bigint order by fldsequenceno";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@id", itemid) });
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    ChequeRepairHistorySearchModel History = new ChequeRepairHistorySearchModel();
                    History.fldOriCheckDigit = item["fldoricheckdigit"].ToString();
                    History.fldOriSerial = item["fldoriserial"].ToString();
                    History.fldOriBankCode = item["fldoribankcode"].ToString();
                    History.fldOriBranchCode = item["fldoribranchcode"].ToString();
                    History.fldOriStateCode = item["fldoristatecode"].ToString();
                    History.fldOriIssuerAccNo = item["fldoriissueraccno"].ToString();
                    History.fldOriTCCode = item["fldoritccode"].ToString();
                    History.fldOriRemark = item["fldoriremark"].ToString();
                    History.fldCheckDigit = item["fldcheckdigit"].ToString();
                    History.fldSerial = item["fldserial"].ToString();
                    History.fldBankCode = item["fldbankcode"].ToString();
                    History.fldBranchCode = item["fldbranchcode"].ToString();
                    History.fldStateCode = item["fldstatecode"].ToString();
                    History.fldIssuerAccNo = item["fldissueraccno"].ToString();
                    History.fldTCCode = item["fldtccode"].ToString();
                    History.fldRemark = item["fldremark"].ToString();
                    //History.fldAmount = item["fldamount"] == DBNull.Value ? 0 : Convert.ToDouble(item["fldamount"]);
                    if (item["fldamount"] == DBNull.Value)
                    {
                        History.fldAmount = "0.00";
                    }
                    else
                    {
                        History.fldAmount = item["fldamount"].ToString();
                    }
                    History.fldcreatetimestamp = item["fldcreatetimestamp"].ToString();
                    History.fldType = item["fldtype"].ToString();
                    History.fldLocation = item["fldlocation"].ToString();
                    History.flduserabb = item["flduserabb"].ToString();
                    result.Add(History);

                }
                return result;
            }

            return null;
        }
        public Dictionary<string, string> GetStrListVirtualAcctNumberByItemInitialID(string initialid)
        {
            DataTable dtbItemInitalID = new DataTable();
            Dictionary<string, string> result = new Dictionary<string, string>();
            string strLstCChqAmt = "";
            string strLstVChqAmt = "";
            string strLstVAcctNo = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@bintItemInitialID", initialid));
            dtbItemInitalID = dbContext.GetRecordsAsDataTableSP("spcgItemInitialFrontEnd", sqlParameterNext.ToArray());
            if (dtbItemInitalID.Rows.Count > 0)
            {
                foreach (DataRow drItem in dtbItemInitalID.Rows)
                {

                    if (drItem["fldChqAmt"].ToString() == "0")
                    {
                        strLstCChqAmt = "0.00";
                    }
                    else
                    {
                        decimal fldChqAmt = Convert.ToDecimal(drItem["fldChqAmt"]);
                        //fldChqAmt = fldChqAmt / 100;
                        strLstCChqAmt = fldChqAmt.ToString();
                    }
                    if (drItem["fldDepAmt"].ToString() == "0")
                    {

                        strLstVChqAmt = "0.00";
                    }
                    else
                    {
                        decimal fldDepAmt = Convert.ToDecimal(drItem["fldDepAmt"]);
                        // fldDepAmt = fldDepAmt / 100;
                        strLstVChqAmt = fldDepAmt.ToString();
                    }
                    strLstVAcctNo = GetStringAppendedValue(strLstVAcctNo, drItem["fldItemID"].ToString() + "+" + drItem["fldPVaccNo"] + "+1" +
                                        "+" + drItem["fldItemType"] + "+" + drItem["fldReasonCode"] +
                                        "+" + drItem["fldRemark"] + "++++" + strLstVChqAmt, ";");

                    if (result.ContainsKey("hfdVItemNoList"))
                    {
                        result["hfdVItemNoList"] = strLstVAcctNo;
                    }
                    else
                    {
                        result.Add("hfdVItemNoList", strLstVAcctNo);
                    }
                    if (result.ContainsKey("hfdCItemChqAmtList"))
                    {
                        result["hfdCItemChqAmtList"] = strLstCChqAmt;
                    }
                    else
                    {
                        result.Add("hfdCItemChqAmtList", strLstCChqAmt);
                    }
                }

            }

            return result;
        }
        public string GetStringAppendedValue(string strValList, string strVal, string strDelimiter)
        {

            if (strValList == "")
            {
                strValList = strVal.ToString();
            }
            else
            {
                strValList = strValList + strDelimiter + strVal.ToString();
            }
            return strValList;

        }

        public void AddOutwardItemHistory(string fldActionStatus, string fldTransNo, string fldRemarks, string fldQueue, AccountModel currentUser)
        {
            string MSSQL = "INSERT INTO [dbo].[tblOutwardItemHistory]";
            MSSQL = MSSQL + " ([fldActionStatus] ,[fldTransNo],[fldRemarks],[fldCreateUserId],[fldQueue],fldCreateTimeStamp) ";
            MSSQL = MSSQL + " VALUES (@fldActionStatus,@fldTransNo,@fldRemarks,@fldCreateUserId,@fldQueue,@fldCreateTimeStamp) ";
            dbContext.ExecuteNonQuery(MSSQL, new[] {
                    new SqlParameter("@fldActionStatus", fldActionStatus),
                    new SqlParameter("@fldTransNo", fldTransNo),
                    new SqlParameter("@fldRemarks", fldRemarks),
                    new SqlParameter("@fldCreateUserId", currentUser.UserId),
                    new SqlParameter("@fldQueue", fldQueue),
                    new SqlParameter("@fldCreateTimeStamp", DateTime.Now)
                });
        }
    }
}