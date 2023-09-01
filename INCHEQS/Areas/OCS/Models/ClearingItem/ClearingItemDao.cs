using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.ClearingItem
{
    public class ClearingItemDao : IClearingItemDao
    {

        private readonly ApplicationDbContext dbContext;
        public ClearingItemDao(ApplicationDbContext dbContext)
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

        public bool UpdateClearingStatusandInsertClearingAgent(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber)
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
                dbContext.GetRecordsAsDataTableSP("spciClearingAgentConfig", SqlParameterNext.ToArray());
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}