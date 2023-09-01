using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.ClearingItemsSummary
{
    public class ClearingItemsSummaryDao : IClearingItemsSummaryDao
    {
        private readonly ApplicationDbContext dbContext;
        public ClearingItemsSummaryDao(ApplicationDbContext dbContext)
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
    }
}