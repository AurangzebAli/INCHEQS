using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using INCHEQS.Security;
using System.Web.Mvc;
using static INCHEQS.TaskAssignment.TaskIds;

namespace INCHEQS.Areas.COMMON.Models.CreditAccount
{
    public class CreditAccountDao: ICreditAccountDao
    {
        private readonly ApplicationDbContext dbContext;
        public CreditAccountDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public CreditAccountModel GetAccountNumber(string AccountId)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            CreditAccountModel accountmodel = new CreditAccountModel();
            sqlParameterNext.Add(new SqlParameter("@fldCreditAccountId", AccountId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCreditAccount", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                accountmodel.fldCreditAccountId = row["fldCreditAccountId"].ToString();
                accountmodel.fldCreditAccountNumber= row["fldCreditAccountNumber"].ToString();
                accountmodel.fldClearingType = row["fldClearingType"].ToString();
                accountmodel.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                accountmodel.fldCreateUserId = row["fldCreateUserId"].ToString();
                accountmodel.fldStateCode= row["fldStateCode"].ToString();

            }
            else
            {
                accountmodel = null;
            }
            return accountmodel;
        }
        public bool CreateAccountNumberMaster(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCreditAccountNumber", col["fldAccountNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldClearingType", col["fldClearingType"]));
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["fldStateCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciCreditAccountMaster", sqlParameterNext.ToArray());
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
        public bool UpdateCreditAccountMaster(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["fldStateCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreditAccountId", col["fldCreditAccountId"]));
            sqlParameterNext.Add(new SqlParameter("@fldClearingType", col["fldClearingType"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuCreditAccountMaster", sqlParameterNext.ToArray());
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

        public bool DeleteCreditAccountMasterTemp(string Accountid)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCAccountid", Accountid));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdCreditAccountMaster", sqlParameterNext.ToArray());
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
        public List<CreditAccountModel> ListBranch(string Type)
        {
            DataTable resultTable = new DataTable();
            List<CreditAccountModel> clearingType = new List<CreditAccountModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearingNumber", Type));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgClearingType", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    CreditAccountModel creditaccount = new CreditAccountModel();
                    creditaccount.fldClearingValue = row["clearingValue"].ToString();
                    creditaccount.fldClearingType= row["clearingText"].ToString();
                    clearingType.Add(creditaccount);
                }
            }
            return clearingType;
        }
        public List<CreditAccountModel> ListStateCode(string StateCode)
        {
            DataTable resultTable = new DataTable();
            List<CreditAccountModel> StateCodes = new List<CreditAccountModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCreditAccountStateCode", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    CreditAccountModel creditaccount = new CreditAccountModel();
                    creditaccount.fldStateCode = row["fldStateCode"].ToString();
                    creditaccount.fldStateDesc = row["fldStateDesc"].ToString();

                    StateCodes.Add(creditaccount);
                }
            }
            return StateCodes;
        }



    }
}