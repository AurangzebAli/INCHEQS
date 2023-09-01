using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Resources;

namespace INCHEQS.Areas.ICS.Models.LargeAmount
{
    public class ICSLargeAmountDao : ICSILargeAmountDao
    {
        private readonly ApplicationDbContext dbContext;

        public string BigAmount { get; private set; }

        public ICSLargeAmountDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataTable GetLargeAmountLimit()
        {
            DataTable dataTable = new DataTable();
            
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            dataTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimit", sqlParameters.ToArray());

            return dataTable;
        }

        public string SetLargeAmountLimit()
        {
            DataTable dataTable = new DataTable();
            string amount="";
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            dataTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimit", sqlParameters.ToArray());

            if (dataTable.Rows.Count > 0)
            {
                amount= dataTable.Rows[0]["fldLargeAmt"].ToString();
            }
            else
            {
                amount = "0.00";
            }
            return amount;
        }


        public void UpdateLargeAmountLimit(string Amount)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@fldAmount", Amount));
            sqlParameters.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuLargeAmountLimit", sqlParameters.ToArray());
            
        }

        public void CreateLargeAmountLimit(string Amount, string strUpdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAmount", Amount));
            //sqlParameterNext.Add(new SqlParameter("@fldAmountId", "1"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdate));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdate));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "U"));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciLargeAmountLimit", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        public List<string> Validate(FormCollection col)
        {
            List<string> strs = new List<string>();
            if (col["txtAmount"].Equals(""))
            {
                strs.Add(Locale.LargeAmountCannotBlank);
            }
            return strs;
        }

        //GET MENU TITLE OF TASK ID
        public string GetPageTitle(string TaskId)
        {
            string PageTitle = "";
            try
            {
                DataTable dtHubTemp = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
                dtHubTemp = dbContext.GetRecordsAsDataTableSP("spcgGetPageTitle", sqlParameterNext.ToArray());

                if (dtHubTemp.Rows.Count > 0)
                {
                    DataRow drItem = dtHubTemp.Rows[0];
                    PageTitle = drItem["fldPageTitle"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return PageTitle;
        }

        public bool NoChangesinLargeAmountLimit(FormCollection col, string Amount)
        {
            int counter = 0;
            bool strs = false;

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@fldAmount", Amount));
            sqlParameters.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

            DataTable dataTable = new DataTable();

            dataTable = dbContext.GetRecordsAsDataTableSP("spcgNoLargeAmountLimitChanges", sqlParameters.ToArray());

            if (dataTable.Rows.Count > 0)
            {
                DataRow drItem = dataTable.Rows[0];
                if (Amount.Equals(drItem["fldLargeAmt"].ToString()))
                {
                    counter++;
                }
                if (counter == 1)
                {
                    strs = true;
                }
            }
            return strs;
        }

        public bool CheckLargeAmountLimitinTempExist(string Amount)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@fldAmount", Amount));
            sqlParameters.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimitinTempExist", sqlParameters.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckLargeAmountLimitExist(string Amount)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@fldAmount", Amount));
            sqlParameters.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimitExist", sqlParameters.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void InsertLargeAmountLimitTemp(string Amount, string strUpdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAmount", Amount));
            //sqlParameterNext.Add(new SqlParameter("@fldAmountId", "1"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdate));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdate));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "U"));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciLargeAmountLimitTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        public LargeAmountModel GetLargeAmountLimit(string strBankCode)
        {
            LargeAmountModel lmodel = new LargeAmountModel();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));

            try
            {
                DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimit", sqlParameterNext.ToArray());
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    DataRow item = recordsAsDataTable.Rows[0];
                    lmodel.fldAmount = item["fldLargeAmt"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return lmodel;
        }


        //Large Amount Limit Checker
        public void UpdateLargeAmountLimitTemp(string strBankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuLargeAmountLimitTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteLargeAmountLimitTemp(string strBankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdLargeAmountLimitTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void UpdateLargeAmountLimitFromTemp(string strBankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuLargeAmountLimitFromTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        public void CreateLargeAmountLimitFromTemp(string strBankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));

            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciLargeAmountLimitFromTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }


}