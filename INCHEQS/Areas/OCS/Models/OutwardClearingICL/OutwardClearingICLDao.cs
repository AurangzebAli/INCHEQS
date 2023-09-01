using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.OutwardClearingICL
{
    public class OutwardClearingICLDao : IOutwardClearingICLDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public OutwardClearingICLDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        public DataTable GetHubBranches(string userId)
        {
            DataTable dt = new DataTable();
            //string strQuery = "SELECT fldBranchCode, fldBranchDesc FROM tblHubUser hu " +
            //                "INNER JOIN tblHubBranch hb ON hb.fldHubCode = hu.fldHubCode " +
            //                "INNER JOIN tblMapBranch mb ON hb.fldbranchid = mb.fldBranchCode " +
            //                "WHERE hu.fldUserId = @fldUserId ";
            //dt = dbContext.GetRecordsAsDataTable(strQuery, new[] { new SqlParameter("@fldUserId", userId) });
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserHubBranches", SqlParameterNext.ToArray());
            return dt;
        }
        public string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public DataTable ReadyforCenterClearing(FormCollection collection)
        {
            try
            {
                DataTable ds = new DataTable();
                string CapturingBranch = getBetween(collection["row_fldchkbox"], "cb", "si");
                string ScannerId = getBetween(collection["row_fldchkbox"], "si", "cd");
                string CapturingDate = getBetween(collection["row_fldchkbox"], "cd", "bn");
                string BatchNumber = getBetween(collection["row_fldchkbox"], "bn", "ed");
                string currency = getBetween(collection["row_fldchkbox"], "ed", "cr");
                string CapturingType = getBetween(collection["row_fldchkbox"], "cr", "ct");
                string hidUIC = CapturingDate + CapturingBranch + currency + CapturingType;
                string processdate = getBetween(collection["row_fldchkbox"], "cd", "bn");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxCenterItemReadyForClearing", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public DataTable GetCenterItemReadyForClearingList(string SelectedRow)
        {
            try
            {
                DataTable ds = new DataTable();
                string CapturingBranch = getBetween(SelectedRow, "cb", "si");
                string ScannerId = getBetween(SelectedRow, "si", "cd");
                string CapturingDate = getBetween(SelectedRow, "cd", "bn");
                string BatchNumber = getBetween(SelectedRow, "bn", "ed");
                string currency = getBetween(SelectedRow, "ed", "cr");
                string CapturingType = getBetween(SelectedRow, "cr", "ct");
                string hidUIC = CapturingDate + CapturingBranch + currency + CapturingType;
                string processdate = getBetween(SelectedRow, "cd", "bn");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxCenterItemReadyForClearing", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public DataTable CenterSubmittedItems(FormCollection collection)
        {
            try
            {
                DataTable ds = new DataTable();
                //clearingBatch = CapturingDate.Trim() + "" + clearingBatch.Trim();
                string CapturingDate = DateTime.ParseExact(collection["row_fldcapturingdate"].Trim(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                string CapturingBranch = collection["row_fldcapturingbranch"].Trim();
                string clearingBatch = collection["row_fldcapturingdate"].Trim() + collection["row_fldClearingBatch"].Trim();
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ClearingBatch", clearingBatch));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxClearedItem", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }
        public DataTable GetCenterClearedItemList(string CapturingDate, string CapturingBranch, string clearingBatch)
        {
            try
            {
                DataTable ds = new DataTable();
                clearingBatch = CapturingDate.Trim() +""+clearingBatch.Trim();
                CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ClearingBatch", clearingBatch));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxClearedItem", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }
        public void AddToBatch(string intUserId, string strSelectedUIC, string processdate)
        {
            GenerateNewBatches(intUserId, strSelectedUIC, processdate);

        }

        public DataTable GetDistinctUniqueIdentifierForClearing(string strCurrency, string strBankCode, string strBranchCode, string intUserId, string strChequeType, string dtChequeDate, string strUI,
            string dtClearingDate, string strIgnoreIQA)
        {
            DataTable ItemDataTable = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@currency", strCurrency));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            SqlParameterNext.Add(new SqlParameter("@branchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@userid", intUserId));
            SqlParameterNext.Add(new SqlParameter("@chequeType", strChequeType));
            SqlParameterNext.Add(new SqlParameter("@chequeDate", ""));
            SqlParameterNext.Add(new SqlParameter("@ui", strUI));
            SqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            SqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            ItemDataTable = dbContext.GetRecordsAsDataTableSP("spcgItemUIForClearingList", SqlParameterNext.ToArray());
            return ItemDataTable;

        }
        public void GenerateNewBatches(string intUserId, string strSelectedUIC, string processdate)
        {
            int intMaxItemPerBatch = 0;
            Int64 lngClearingBatchId = 0;
            int intFailedNextId = 0;
            Int64 intNewStartNumber = 0; 
            string lngDateBatch = ""; 
            bool blnThisUIItemExists = false;
            string strRepresent = "N";
            string strRepresentClearingUIC = "";
            DataTable ItemDataTable = new DataTable();
            intMaxItemPerBatch = Convert.ToInt32(systemProfileDao.GetValueFromSystemProfile("MaxItemPerClearingBatch", CurrentUser.Account.BankCode).Trim());
            ItemDataTable = GetDistinctUniqueIdentifierForClearing("", CurrentUser.Account.BankCode, "", intUserId, "", null, strSelectedUIC, processdate, "N");
            if (ItemDataTable.Rows.Count > 0)
            {
                foreach (DataRow drwItem in ItemDataTable.Rows)
                {
                    blnThisUIItemExists = true;
                    bool blnHasItem = HasClearableItemByUI("", CurrentUser.Account.BankCode, "", "", "", drwItem["ID"].ToString(), processdate, "N", "", strRepresent, intUserId);
                    if (!blnHasItem)
                    {
                        strRepresent = "Y";
                    }
                    while (blnThisUIItemExists)
                    {
                        lngClearingBatchId = 0;
                        intFailedNextId = 0;
                        strRepresentClearingUIC = drwItem["ID"].ToString();
                        while (lngClearingBatchId <= 0)
                        {
                            DataTable resultTable = new DataTable();
                            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                            sqlParameterNext.Add(new SqlParameter("intColumnID", "991"));
                            sqlParameterNext.Add(new SqlParameter("intNumberMaxLength", "16"));
                            resultTable = dbContext.GetRecordsAsDataTableSP("spcuNewStartEndNumberByTypeWeb", sqlParameterNext.ToArray());
                            if (resultTable.Rows.Count > 0)
                            {
                                DataRow row = resultTable.Rows[0];
                                lngClearingBatchId = Convert.ToInt64(row["intNewStartNumber"]);
                            }
                            if (lngClearingBatchId > 0)
                            {
                                string batchid = Convert.ToString(lngClearingBatchId);
                                batchid = batchid.PadLeft(6, '0');
                                lngDateBatch = Convert.ToInt64(drwItem["ClearingDate"]) +""+ Convert.ToInt64(batchid);
                                if (UpdateItemWithClearingBatchDateBatch(drwItem["ID"].ToString(), intMaxItemPerBatch, processdate, intUserId,DateTime.Now, "N", strRepresent, lngDateBatch) == true)
                                {
                                    if (AddClearingStatus(lngDateBatch, batchid,"CA1", strRepresentClearingUIC, "NEW", processdate, "","","","","", intUserId,DateTime.Now, intUserId,DateTime.Now) == true)
                                    {
                                        AddClearingOutbox(drwItem["ID"].ToString(), intMaxItemPerBatch, lngDateBatch, processdate, "N", intUserId, intUserId, DateTime.Now, intUserId, DateTime.Now);
                                        //ICLGeneratorTransfer(processdate, intUserId);
                                    }
                                }
                                break;
                            }
                            else
                            {
                                if (intFailedNextId == 500)
                                {
                                    UnlockItemForClearingNew(intUserId, lngDateBatch);
                                    break;
                                }
                                intFailedNextId += 1;
                            }
                        }
                        bool ItemExistforNewBatch = HasClearableItemByUI("", CurrentUser.Account.BankCode, "", "", "", drwItem["ID"].ToString(), processdate, "N", "", strRepresent, intUserId);
                        if (ItemExistforNewBatch == true)
                        {
                            blnThisUIItemExists = true;
                        }
                        else
                        {
                            blnThisUIItemExists = false;
                        }
                    }
                    UnlockItemForClearingNew(intUserId, lngDateBatch);
                    strRepresent = "N";
                }
            }
        }
        public void UnlockItemForClearingNew(string intUserId,string lngDateBatch)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@lockUser", intUserId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuItemUnlockForClearingNew", sqlParameterNext.ToArray());
        }

        public bool AddClearingOutbox(string strUI,int intMaxItemPerBatch, string intDateBatch,string dtClearingDate,string strIgnoreIQA,string intLockUserId, string intCreateUserId, DateTime dtCreateTimeStamp, string intUpdateUserId, DateTime dtUpdateTimeStamp)
        {
            bool blnResult = false;
            int intRowAffected = 0;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@topCount", intMaxItemPerBatch));
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            sqlParameterNext.Add(new SqlParameter("@lockUser", intLockUserId));
            sqlParameterNext.Add(new SqlParameter("@createUserId", intCreateUserId));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", dtCreateTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", intUpdateUserId));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", dtUpdateTimeStamp));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciOutboxByItemLockUser", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public void ICLGeneratorTransfer(string dtClearingDate, string intCreateUserId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@CapturingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@userId", intCreateUserId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spci_ICLGeneratorTransfer", sqlParameterNext.ToArray());
        }

        public bool AddClearingStatus(string intDateBatch, string strClearingBatch, string strClearingAgent,string strUI,
            string strCurrentProcess,string dtProcessDateTime, string dtCompleteDateTime,string intErrorCode,string strErrorMsg,string strCTCSclient,string strCTCSclientMsg,
            string intCreateUserId,DateTime dtCreateTimeStamp, string intUpdateUserId, DateTime dtUpdateTimeStamp
            )
        {
            bool blnResult = false;
            int intRowAffected = 0;
            dtProcessDateTime = DateTime.ParseExact(dtProcessDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingAgent", strClearingAgent));
            sqlParameterNext.Add(new SqlParameter("@clearingBatch", strClearingBatch));
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@currentProcess", strCurrentProcess));
            sqlParameterNext.Add(new SqlParameter("@errorCode", intErrorCode));
            sqlParameterNext.Add(new SqlParameter("@errorMsg", strErrorMsg));
            sqlParameterNext.Add(new SqlParameter("@CTCSclient", strCTCSclient));
            sqlParameterNext.Add(new SqlParameter("@CTCSclientMsg", strCTCSclientMsg));
            sqlParameterNext.Add(new SqlParameter("@createUserId", intCreateUserId));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", dtCreateTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", intUpdateUserId));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", dtUpdateTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@ProcessDate", dtProcessDateTime.ToString()));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciClearingStatus", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }


        public bool UpdateItemWithClearingBatchDateBatch(string strUI, int intMaxItemPerBatch, string dtClearingDate, string intLockUserId, DateTime dtLockTimeStamp,string strIgnoreIQA, string strRepresent, string lngDateBatch)
        {
            bool blnResult = false;
            int intRowAffected = 0;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@topCount", intMaxItemPerBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            sqlParameterNext.Add(new SqlParameter("@lockUser", intLockUserId));
            sqlParameterNext.Add(new SqlParameter("@lockTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@represent", strRepresent));
            sqlParameterNext.Add(new SqlParameter("@Datebatch", lngDateBatch));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuItemBatchForClearing", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public bool HasClearableItemByUI(string strCurrency, string strBankCode, string strBranchCode, string strChequeType, string dtChequeDate, string strUI, string dtClearingDate, string strIgnoreIQA,string strIQA, string strRepresent, string intUserId)
        {
            bool blnHasItem = false;
            DataTable ItemDataTable = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@currency", strCurrency));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            SqlParameterNext.Add(new SqlParameter("@branchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@chequeType", strChequeType));
            SqlParameterNext.Add(new SqlParameter("@chequeDate", dtChequeDate));
            SqlParameterNext.Add(new SqlParameter("@ui", strUI));
            SqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            SqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            SqlParameterNext.Add(new SqlParameter("@IQA", strIQA));
            SqlParameterNext.Add(new SqlParameter("@represent", strRepresent));
            ItemDataTable = dbContext.GetRecordsAsDataTableSP("spcgItemForClearingList", SqlParameterNext.ToArray());
            if (ItemDataTable.Rows.Count > 0)
            {
                blnHasItem = true;
            }
            else
            {
                blnHasItem = false;
            }
            return blnHasItem;
        }


    }
}