using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Areas.ICS.Models.HighRiskAccount {
    public class HighRiskAccountDao : IHighRiskAccountDao{
        private readonly ApplicationDbContext dbContext;
        public HighRiskAccountDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public HighRiskAccountModel GetHighRiskAccount(string highRiskAccount) {
            DataTable ds = new DataTable();
            HighRiskAccountModel highRiskAccModel = new HighRiskAccountModel();

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetHighRiskAccount", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                highRiskAccModel.fldHighRiskAccount = row["fldHighRiskAccount"].ToString();
                highRiskAccModel.fldInternalBranchCode = row["fldInternalBranchCode"].ToString();
                highRiskAccModel.fldHighRiskAmount = row["fldHighRiskAmount"].ToString();
            }

            return highRiskAccModel;
        }

        public HighRiskAccountModel GetHighRiskAccountTemp(string highRiskAccount)
        {
            DataTable ds = new DataTable();
            HighRiskAccountModel highRiskAccModel = new HighRiskAccountModel();

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetHighRiskAccountTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                highRiskAccModel.fldHighRiskAccount = row["fldHighRiskAccount"].ToString();
                highRiskAccModel.fldInternalBranchCode = row["fldInternalBranchCode"].ToString();
                highRiskAccModel.fldHighRiskAmount = row["fldHighRiskAmount"].ToString();
                highRiskAccModel.fldApproveStatus = row["fldApproveStatus"].ToString();
            }

            return highRiskAccModel;
        }
        public DataTable GetInternalBranchCode()
        {

            HighRiskAccountModel highRiskAccModel = new HighRiskAccountModel();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            DataTable resultTable = new DataTable();
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetInternalBranchCode", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                highRiskAccModel.fldBranchCode = row["fldCInternalBranchCode"].ToString();
                highRiskAccModel.fldBranchDesc = row["fldCBranchDesc"].ToString();
                highRiskAccModel.fldBranchCodeI = row["fldIInternalBranchCode"].ToString();
                highRiskAccModel.fldBranchDescI = row["fldIBranchDesc"].ToString();
            }
                return resultTable;
        }

        public List<string> ValidateCreate(FormCollection col) {
            List<string> err = new List<string>();
            if (col["fldHighRiskAccount"].Equals("")) {
                err.Add(Locale.HighRiskAccountCannotBeEmpty);
            }
            if (CheckExist(col["fldHighRiskAccount"])) {
                err.Add(Locale.HighRiskAccountAlreadyExist);
            }

            return err;
        }

        public bool CheckExist(string accountNo) {

            bool blnResult;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", accountNo));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckExistHighRiskAccount", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {

                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public List<string> ValidateUpdate(FormCollection col) {
            List<string> err = new List<string>();
            if (col["fldHighRiskAccount"].Equals("")) {
                err.Add(Locale.HighRiskAccountCannotBeEmpty);
            }

            return err;
        }

        public void CreateHighRiskAccount(FormCollection col, AccountModel currentUser) {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", col["fldHighRiskAccount"]));
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["fldInternalBranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAmount", Double.Parse(col["fldHighRiskAmount"])));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime()));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHighRisKAccount", sqlParameterNext.ToArray()); //Done

        }

        public void CreateHighRiskAccountTemp(FormCollection col, AccountModel currentUser,string status, string highRiskAccount)
        {
            Dictionary<string, dynamic> sqlHighRiskAcc = new Dictionary<string, dynamic>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            if (status == "D")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));
                sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAmount", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHighRisKAccountTemp", sqlParameterNext.ToArray()); //Done
            }
            else if (status == "U") {
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", col["fldHighRiskAccount"]));
                sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAmount", Double.Parse(col["fldHighRiskAmount"])));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHighRisKAccountTemp", sqlParameterNext.ToArray()); //Done
            }
            else if (status == "A")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", col["fldHighRiskAccount"]));
                sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["fldInternalBranchCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldHighRiskAmount", Double.Parse(col["fldHighRiskAmount"])));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHighRisKAccountTemp", sqlParameterNext.ToArray()); //Done
            }

            
        }

        public void UpdateHighRiskAccount(FormCollection col) {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", col["fldHighRiskAccount"]));
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["fldInternalBranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAmount", col["fldHighRiskAmount"].Trim()));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHighRiskAccount", sqlParameterNext.ToArray()); //Done
        }

        public void DeleteFromHighRiskAccount(string highRiskAccount) {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHighRiskAccount", sqlParameterNext.ToArray()); //Done
        }

        public void DeleteHighRiskAccoountTemp(string highRiskAccount)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHighRiskAccountTemp", sqlParameterNext.ToArray()); //Done
        }

        
        public void MoveHighRiskAccoountFromTemp(string highRiskAccount, string status)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHighRisKAccountFromTemp", sqlParameterNext.ToArray()); 
        }

        public bool CheckHighRiskAccountTempExist(string highRiskAccount)
        {

            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", highRiskAccount));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgExisitingHighRiskAccountMasterTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {

                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
    }
}