using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;

using System.Web.Mvc;
using System.Data.SqlClient;
using System.Threading.Tasks;
using INCHEQS.Security.Account;
using INCHEQS.Common;

namespace INCHEQS.Areas.PPS.Models.PositivePayMatching
{
    public class PositivePayMatchingDao : IPositivePayMatchingDao
    {
        private readonly ApplicationDbContext dbContext;

        public PositivePayMatchingDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public PositivePayMatchingModel getMatchingCount(string bankCode)
        {
            DataTable ds = new DataTable();
            PositivePayMatchingModel match = new PositivePayMatchingModel();
            string minClearDate = "";
            string maxClearDate = "";

            string stmt = "SELECT min(fldClearDate) minClearDate, max(fldClearDate) maxClearDate from tblInwardClearDate where fldStatus = 'Y' ";
            ds = dbContext.GetRecordsAsDataTable(stmt);

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                minClearDate = Convert.ToDateTime(row["minClearDate"]).ToString("dd-MM-yyyy");
                maxClearDate = row["maxClearDate"].ToString();
                match.minClearDate = minClearDate;
                match.maxClearDate = maxClearDate;
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@minClearDate", match.minClearDate));
            sqlParameterNext.Add(new SqlParameter("@maxClearDate", match.maxClearDate));
            sqlParameterNext.Add(new SqlParameter("@userBankCode", bankCode));

            ds = dbContext.GetRecordsAsDataTableSP("sp_GetMatchingCount", sqlParameterNext.ToArray());


            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                match.totalUnMatched = row["iTotalUnmatched"].ToString();
                match.totalMatched = row["iTotalMatched"].ToString();
            }
            return match;


        }
        
        public PositivePayMatchingModel getMatchingResult()
        {
            DataTable ds = new DataTable();
            PositivePayMatchingModel match = new PositivePayMatchingModel();
            

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@processDate", DateUtils.GetCurrentDate()));

            ds = dbContext.GetRecordsAsDataTableSP("sp_GetMatchMatchingResult", sqlParameterNext.ToArray());


            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                match.totalItem = row["totalItem"].ToString();
                match.totalPayeeMatch = row["totalPayeeMatch"].ToString();
                match.totalDateMatch = row["totalDateMatch"].ToString();
                match.totalAmountMatch = row["totalAmountMatch"].ToString();
                match.totalMICRMatch = row["totalMICRMatch"].ToString();
                match.totalPayeeUnmatch = row["totalPayeeUnmatch"].ToString();
                match.totalDateUnmatch = row["totalDateUnmatch"].ToString();
                match.totalAmountUnmatch = row["totalAmountUnmatch"].ToString();
                match.totalMICRUnmatch = row["totalMICRUnmatch"].ToString();

                match.PayeePercentage = Math.Round(Convert.ToDouble(row["PayeePercentage"]), 2);
                match.DatePercentage = Math.Round(Convert.ToDouble(row["DatePercentage"]), 2);
                match.AmountPercentage = Math.Round(Convert.ToDouble(row["AmountPercentage"]), 2);
                match.MICRPercentage = Math.Round(Convert.ToDouble(row["MICRPercentage"]), 2);
            }
            return match;


        }

    }
}