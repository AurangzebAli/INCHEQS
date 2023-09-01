using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.BranchSubmission
{
    public class BranchSubmissionDao : IBranchSubmissionDao
    {
        private readonly ApplicationDbContext dbContext;
        public BranchSubmissionDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
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

        public DataTable GetItemReadyForSubmission(string userid)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@intUserID", userid));
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetBranchItemReadyForSubmit", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetItemSubmitted(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchItemSubmitted", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetBranchEndOfDay(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgEODBranchByBranchID", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable DataEntryPendingItem(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgPendingBatchEntry", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable AuthorizationPendingItem(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgPendingAuthorization", SqlParameterNext.ToArray());
            return dt;
        }
        public bool BranchSubmission(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber,string userid)
        {
            try
            {
                CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "yyyyMMdd", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ScannerID", ScannerId));
                SqlParameterNext.Add(new SqlParameter("@BatcNo", Convert.ToInt64(BatchNumber)));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                dbContext.GetRecordsAsDataTableSP("spcuBranchItemSubmissionByBatch", SqlParameterNext.ToArray());
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public List<BranchSubmissionItemList> GetItemReadyForSubmissionList(string userid)
        {
            List<BranchSubmissionItemList> result = new List<BranchSubmissionItemList>();
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@intUserID", userid));
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetBranchItemReadyForSubmit", SqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    BranchSubmissionItemList History = new BranchSubmissionItemList();
                    History.fldCapturingBranch = item["fldCapturingBranch"].ToString();
                    History.fldBranchDesc = item["fldBranchDesc"].ToString();
                    History.CapturingBranch = item["CapturingBranch"].ToString();
                    History.CapturingDate = item["CapturingDate"].ToString();
                    History.fldBatchNumber = item["fldBatchNumber"].ToString();
                    History.fldCapturingMode = item["fldCapturingMode"].ToString();
                    History.TotalItem = item["TotalItem"].ToString();
                    History.TotalAmount = item["TotalAmount"].ToString();
                    History.fldscannerid = item["fldscannerid"].ToString(); 
                    result.Add(History);

                }
                return result;
            }
            return null;
        }
        public List<BranchSubmittedItemList> GetItemReadyForSubmittedList(string userid)
        {
            List<BranchSubmittedItemList> result = new List<BranchSubmittedItemList>();
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@intUserID", userid));
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetBranchSubmittedItems", SqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    BranchSubmittedItemList History = new BranchSubmittedItemList();
                    History.fldCapturingBranch = item["fldCapturingBranch"].ToString();
                    History.fldBranchDesc = item["fldBranchDesc"].ToString();
                    History.CapturingBranch = item["CapturingBranch"].ToString();
                    History.CapturingDate = item["CapturingDate"].ToString();
                    History.fldBatchNumber = item["fldBatchNumber"].ToString();
                    History.fldCapturingMode = item["fldCapturingMode"].ToString();
                    History.TotalItem = item["TotalItem"].ToString();
                    History.TotalAmount = item["TotalAmount"].ToString();
                    History.fldscannerid = item["fldscannerid"].ToString();
                    result.Add(History);

                }
                return result;
            }
            return null;
        }

        public DataTable ReturnItemDetails(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber, string userid)
        {
            try
            {
                DataTable ds = new DataTable();
                CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ScannerID", ScannerId));
                SqlParameterNext.Add(new SqlParameter("@BatcNo", Convert.ToInt64(BatchNumber)));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxBranchItemReadyForSubmit", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }


        public DataTable ReadyforBranchSubmission(FormCollection collection)
        {
            try
            {
                DataTable ds = new DataTable();
                string CapturingBranch = getBetween(collection["row_fldchkbox"], "cb", "si");
                string ScannerId = getBetween(collection["row_fldchkbox"], "si", "cd");
                string CapturingDate = getBetween(collection["row_fldchkbox"], "cd", "bn");
                string BatchNumber = getBetween(collection["row_fldchkbox"], "bn", "ed");
                //CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ScannerID", ScannerId));
                SqlParameterNext.Add(new SqlParameter("@BatcNo", Convert.ToInt64(BatchNumber)));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxBranchItemReadyForSubmit", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }


        public DataTable ReturnSubmittedDetails(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber, string userid)
        {
            try
            {
                DataTable ds = new DataTable();
                CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ScannerID", ScannerId));
                SqlParameterNext.Add(new SqlParameter("@BatcNo", Convert.ToInt64(BatchNumber)));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxBranchItemSubmitted", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public DataTable BranchSubmittedItems(FormCollection collection)
        {
            try
            {
                DataTable ds = new DataTable();
                string CapturingBranch = getBetween(collection["row_fldchkbox"], "cb", "si");
                string ScannerId = getBetween(collection["row_fldchkbox"], "si", "cd");
                string CapturingDate = getBetween(collection["row_fldchkbox"], "cd", "bn");
                string BatchNumber = getBetween(collection["row_fldchkbox"], "bn", "ed");
                //CapturingDate = DateTime.ParseExact(CapturingDate.Trim(), "dd-MM-yyyy", null).ToString("yyyy-MM-dd");
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@CapturingDate", CapturingDate));
                SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", CapturingBranch));
                SqlParameterNext.Add(new SqlParameter("@ScannerID", ScannerId));
                SqlParameterNext.Add(new SqlParameter("@BatcNo", Convert.ToInt64(BatchNumber)));
                SqlParameterNext.Add(new SqlParameter("@UserId", CurrentUser.Account.UserId));
                ds = dbContext.GetRecordsAsDataTableSP("spcgOutboxBranchItemSubmitted", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
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
        public void UpdateBranchItem(FormCollection collection, AccountModel currentUser, string capturebranch, string scannerid, string id, string capturedate)
        {
            
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerid));
            sqlParameterNext.Add(new SqlParameter("@fldBatchNo", id));
            sqlParameterNext.Add(new SqlParameter("@fldClearingStatus", "1"));
            sqlParameterNext.Add(new SqlParameter("@fldsubmituserid", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@strClearingBranch", capturebranch));
            sqlParameterNext.Add(new SqlParameter("@fldCapturingDate", capturedate));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchClearingItem", sqlParameterNext.ToArray());
          
            //string stmt = " Update tblItemInfo SET fldClearingStatus = 1,fldsubmittimestamp = '" + currentDate + "',fldsubmituserid = '" + currentUser.UserAbbr + "' where flditemid in " +
            //                    "(select flditemid " +
            //                    "FROM tblItemInitial ini " +
            //                    "INNER JOIN tblItemInfo info ON ini.fldItemInitialID = info.fldItemInitialID " +
            //                    "INNER JOIN tblbranchmaster mp ON ini.fldCapturingBranch = mp.fldlocationcode||mp.fldbankcode || mp.fldBranchCode " +
            //                    "WHERE(info.fldClearingStatus = 9) " +
            //                    "AND(COALESCE(info.fldReasonCode, N'') = '') " +
            //                    "AND(ini.fldCapturingBranch = '" + capturebranch + "') " +
            //                    "AND(ini.fldBatchNumber = '" + id + "') " +
            //                    "AND(ini.fldScannerID::text ='" + scannerid + "') " +
            //                    "AND to_char(ini.fldCapturingDate::date, 'YYYY-MM-DD') ='" + capturedate + "'); ";
            //dbContext.GetRecordsAsDataTable(stmt);
            //ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@capturebranch", ToString()),
            //  new SqlParameter("@scannerid", ToString()),
            //  new SqlParameter("@id", ToString()),
            //  new SqlParameter("@capturedate", ToString())
            // });

        }

    }
}