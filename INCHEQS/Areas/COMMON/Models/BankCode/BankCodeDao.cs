using INCHEQS.Common;
using INCHEQS.Common.Resources;
using INCHEQS.DataAccessLayer;
using INCHEQS.PCHC.Resouce;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using INCHEQS.Security;
using System.Web.Mvc;
using System.Collections;

namespace INCHEQS.Areas.COMMON.Models.BankCode
{
    public class BankCodeDao : IBankCodeDao
    {
        private readonly ApplicationDbContext dbContext;

        public BankCodeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public bool checkIdexist(string bankCode, string bankType)
        {
            bool flag;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            DataTable resultTable = new DataTable();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        public bool CheckTempIdExist(string bankCode)
        {
            bool flag;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            DataTable resultTable = new DataTable();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }

            return flag;
        }

        public string condition()
        {
            return "len(fldBankCode)=3";
        }


        public DataTable getBankCode(string bankCode,string bankType)
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            return this.dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());
        }

        public DataTable getBankCodeTemp(string bankCode)
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            return this.dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());
        }

        public async Task<DataTable> getBankCodeTempAsync(string bankCode)
        {
            DataTable dataTable = await Task.Run<DataTable>(() => this.getBankCodeTemp(bankCode));
            return dataTable;
        }

        public async Task<DataTable> getBankCodeAsync(string bankCode,string bankType)
        {
            DataTable dataTable = await Task.Run<DataTable>(() => this.getBankCode(bankCode, bankType));
            return dataTable;
        }

        public DataTable getBankType()
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", ""));
            return this.dbContext.GetRecordsAsDataTableSP("spcgBankType", sqlParameterNext.ToArray());
        }

        public async Task<DataTable> getBankTypeAsync()
        {
            DataTable dataTable = await Task.Run<DataTable>(() => this.getBankType());
            return dataTable;
        }



        public List<string> ValidateCreate(FormCollection collection)
        {
            List<string> strs = new List<string>();
            if (collection["bankCode"].Trim() == "")
            {
                strs.Add(Locale.BankCodecannotbeempty);
            }
            else
            {
                if (this.checkIdexist(collection["bankCode"],collection["bankTypeId"]))
                {
                    strs.Add(Locale.BankCodealreadyexist);
                    if (this.CheckTempIdExist(collection["bankCode"]))
                    {
                        strs.Add(Locale.BankCodealreadyCreatedinApproveChecker);
                    }
                    //else
                    //{

                    //    if (!(new Regex("^[0-9]+$")).IsMatch(collection["bankCode"]))
                    //    {
                    //        strs.Add(Locale.BankCodeFormat);
                    //    }
                    //    else
                    //    {
                    //    }
                    //    if (collection["bankAbbreviation"].Trim() == "")
                    //    {
                    //        strs.Add(Locale.BankAbbcannotbeempty);
                    //    }
                    //}
                }

                        if (!(new Regex("^[0-9]+$")).IsMatch(collection["bankCode"]))
                        {
                            strs.Add(Locale.BankCodeFormat);
                        }
                        else
                        {
                            if (collection["bankCode"].Length != 3)
                            {
                                strs.Add(Locale.BankCodeLimit);
                            }
                        }

                        if (collection["bankDesc"].Trim() == "")
                        {
                            strs.Add(Locale.BankDescriptioncannotbeempty);
                        }
                        if (collection["bankAbbreviation"].Trim() == "")
                        {
                            strs.Add(Locale.BankAbbcannotbeempty);
                }

            }

            return strs;
        }

        public async Task<List<string>> ValidateCreateAsync(FormCollection col)
        {
            List<string> strs = await Task.Run<List<string>>(() => this.ValidateCreate(col));
            return strs;
        }

        public List<string> ValidateUpdate(FormCollection collection)
        {
            int num;
            List<string> strs = new List<string>();
            if (collection["bankCode"].Trim() == "")
            {
                strs.Add(Locale.BankCodecannotbeempty);
            }
            if (collection["bankDesc"].Trim() == "")
            {
                strs.Add(Locale.BankDescriptioncannotbeempty);
            }
            if (!int.TryParse(collection["bankCode"], out num))
            {
                strs.Add(Locale.BankCodeLimit);
            }
            return strs;
        }

        public async Task<List<string>> ValidateUpdateAsync(FormCollection col)
        {
            List<string> strs = await Task.Run<List<string>>(() => this.ValidateUpdate(col));
            return strs;
        }

        public BankCodeModel GetBankCodeData(string id, string bankType)
        {
            BankCodeModel bankCode = new BankCodeModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                bankCode.fldBankCode = row["fldBankCode"].ToString();
                bankCode.fldBankDesc = row["fldBankDesc"].ToString();
                bankCode.fldBankAbb = row["fldBankAbb"].ToString();
                bankCode.fldBankType = row["fldBankType"].ToString();
                bankCode.fldCreateUserId = row["fldCreateUserId"].ToString();
                bankCode.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                bankCode.fldUpdateUserId = row["fldUpdateUserId"].ToString();
                bankCode.fldUpdateTimeStamp = row["fldUpdateTimeStamp"].ToString();

            }

            return bankCode;

        }

        public bool UpdateBankCodeToMain(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankDesc", col["bankDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankAbb", col["bankAbbreviation"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", col["bankTypeId"]));
            sqlParameterNext.Add(new SqlParameter("@fldOldBankTypeId", col["oldBankType"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBankCode", sqlParameterNext.ToArray());

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

        public bool CheckBankCodeDataTempById(string id)
        {
            BankCodeModel bankCode = new BankCodeModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CreateBankCodeinMain(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankCodeinMain", sqlParameterNext.ToArray());
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


        public bool DeleteBankCodeinTemp(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankCodeinTemp", sqlParameterNext.ToArray());
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

        public bool UpdateBankCodeToMainById(string id)
        {
            BankCodeModel bankCode = new BankCodeModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuBankCodeToMainById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CreateInBankMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankDesc", col["bankDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankAbb", col["bankAbbreviation"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", col["bankTypeId"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankCode", sqlParameterNext.ToArray());

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

        public bool CreateInBankCodeTemp(FormCollection col, string status)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankDesc", col["bankDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankAbb", col["bankAbbreviation"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", col["bankTypeId"]));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankCodeTemp", sqlParameterNext.ToArray());

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
        public bool DeleteInBankCode(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankCode", sqlParameterNext.ToArray());
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

        public bool AddBankCodeinTemptoDelete(String id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", id));
            sqlParameterNext.Add(new SqlParameter("@fldBankDesc", ""));
            sqlParameterNext.Add(new SqlParameter("@fldBankAbb", ""));
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", ""));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankCodeTemp", sqlParameterNext.ToArray());

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

    }
}