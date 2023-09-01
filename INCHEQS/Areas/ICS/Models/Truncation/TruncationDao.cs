using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.Truncation
{
    public class TruncationDao : ITruncationDao
    {
        protected readonly ApplicationDbContext dbContext;
        protected readonly IAuditTrailDao auditTrailDao;

        public TruncationDao(ApplicationDbContext dbContext, IAuditTrailDao auditTrailDao)
        {
            this.dbContext = dbContext;
            this.auditTrailDao = auditTrailDao;
        }

        public TruncationModel GetTotalRec(string fldClearDate)
        {
            TruncationModel model = new TruncationModel();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
            DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgATVTruncateGetTotalRec", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                model.fldTotInward = Int32.Parse(dt.Rows[0]["TotRec"].ToString());
                model.fldTotAmount = dt.Rows[0]["TotAmount"].ToString();
            }
            else
            {
                model.fldTotInward = 0;
                model.fldTotAmount = "0";
            }
            return model;
        }

        public TruncationModel GetTotPending(string fldClearDate)
        {
            TruncationModel model = new TruncationModel();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
            DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgATVTruncateGetTotalPendingRec", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                model.fldTotPending = Int32.Parse(dt.Rows[0]["TotRec"].ToString());
                model.fldTotAmountPending = dt.Rows[0]["TotAmountPending"].ToString();
            }
            else
            {
                model.fldTotPending = 0;
                model.fldTotAmountPending = "0";
            }
            return model;
        }

        public List<TruncationModel> CalCulateRecord(string fldClearDate, string fldTop)
        {
            List<TruncationModel> list = new List<TruncationModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTop", fldTop));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
            DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgATVTruncateCalculate", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    TruncationModel model = new TruncationModel();
                    model.fldIssueStateCode = row["fldIssueStateCode"].ToString();
                    model.fldInwardItemId = row["fldInwardItemId"].ToString();
                    model.fldAccountNumber = row["fldAccountNumber"].ToString();
                    model.fldChequeSerialNo = row["fldChequeSerialNo"].ToString();
                    model.fldUIC = row["fldUIC"].ToString();
                    model.fldRejectCode = row["fldRejectCode"].ToString();
                    model.fldAmount = row["fldAmount"].ToString();
                    list.Add(model);
                }
            }

            return list;
        }

        public void Truncate(string fldClearDate, string sTruncateMin, string sTruncateMax)
        {
            bool result = false;
            bool result2 = false;
            string fldTruncateMax = "";
            string fldATVuserId = GetATVUser();

            sTruncateMin = sTruncateMin.Replace(",", "").Replace(".", "");
            sTruncateMax = sTruncateMax.Replace(",", "").Replace(".", "");

            string sql = "Select * from tblSysSetting";
            DataTable dataTable = dbContext.GetRecordsAsDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                fldTruncateMax = dataTable.Rows[0]["fldTruncateMax"].ToString();
                result = true;
            }

            if (result)
            {
                if (Int64.Parse(fldTruncateMax) > Int64.Parse(sTruncateMax))
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    sqlParameterNext.Add(new SqlParameter("@_NewMaxMin", Int64.Parse(sTruncateMax)));
                    sqlParameterNext.Add(new SqlParameter("@_PreMaxAmt", Int64.Parse(fldTruncateMax)));
                    sqlParameterNext.Add(new SqlParameter("@_createdUserId", fldATVuserId));
                    sqlParameterNext.Add(new SqlParameter("@_clearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
                    dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "sp_InwardTruncationReset", sqlParameterNext.ToArray());
                }

                sql = "update tblSysSetting set "
                    + "fldTruncateMin = '" + Int64.Parse(sTruncateMin) + "',"
                    + "fldTruncateMax = '" + Int64.Parse(sTruncateMax) + "',"
                    + "fldUpdateUserID = " + CurrentUser.Account.UserId + " ,fldUpdateTimeStamp = GetDate()";
                dbContext.ExecuteNonQuery(sql);
                result2 = true;
            }

            if (result2)
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@_amountMin", Int64.Parse(sTruncateMin)));
                sqlParameterNext.Add(new SqlParameter("@_amountMax", Int64.Parse(sTruncateMax)));
                sqlParameterNext.Add(new SqlParameter("@_resetBeforeTruncate", "Y"));
                sqlParameterNext.Add(new SqlParameter("@_createdUserId", fldATVuserId));
                sqlParameterNext.Add(new SqlParameter("@_clearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "sp_InwardTruncation", sqlParameterNext.ToArray());
            }

        }

        public string GetATVUser()
        {
            string fldATVUserId = "";
            string sql = "Select * from tblUserMaster where fldUserAbb = 'ATVUSER'";
            DataTable dataTable = dbContext.GetRecordsAsDataTable(sql);
            if (dataTable.Rows.Count > 0)
            {
                fldATVUserId = dataTable.Rows[0]["fldUserId"].ToString();
            }

            return fldATVUserId;
        }

        public string getMaxAmount(string fldClearDate)
        {
            string fldMaxAmount = "0";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateUtils.formatDateToSqlyyyymmdd(fldClearDate)));
            DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgATVTruncateMaxAmount", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                fldMaxAmount = dt.Rows[0]["fldMaxAmount"].ToString();
            }
            return fldMaxAmount;
        }
    }
}