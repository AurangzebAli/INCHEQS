using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.PCHC.Resouce;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Globalization;
using INCHEQS.Security;
using INCHEQS.Common.Resources;


namespace INCHEQS.Areas.COMMON.Models.BranchCode
{
    public class BranchCodeDao : IBranchCodeDao
    {
        private readonly ApplicationDbContext dbContext;

        public SqlDbType branchId
        {
            get;
            private set;
        }

        public BranchCodeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddtoBranchMasterTempToDelete(string branchId)
        {
            string str = "Insert into tblBranchMasterTemp (fldBranchId,fldBranchCode,fldBranchDesc, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp)  Select fldBranchId,fldBranchCode,fldBranchDesc, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp from tblBranchMaster WHERE fldBranchId=@fldBranchId  Update tblBranchMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldBranchId=@fldBranchId ";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId), new SqlParameter("@fldApproveStatus", "D") });
        }

        //public bool CheckBranchCodeExist(FormCollection col)
        //{
        //	string str = col["txtBranchCode1"].Substring(0, 2);
        //	string str1 = " SELECT * FROM tblBranchMaster WHERE fldBranchCode=@branchcode and fldBankCode=@fldBankCode and fldStateCode=@fldStateCode";
        //	return this.dbContext.CheckExist(str1, new SqlParameter[] { new SqlParameter("@branchcode", col["branchCode"]), new SqlParameter("@fldBankCode", col["SelectBank"]), new SqlParameter("@fldStateCode", str) });
        //}

        public bool CheckBranchCodeExist(FormCollection col)
        {
            string branchCode = col["branchCode"];
            string bankCode = col["fldBankCode"];

            //string stateCode = branchCode.Substring(0, 2);
            string stmt = " SELECT * FROM tblBranchMaster WHERE fldBranchCode=@branchcode and fldBankCode=@bankCode";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@branchcode", col["branchCode"]),
                new SqlParameter("@bankCode", col["fldbankCode"]),

            });

        }

        //      public bool CheckBranchCodeExistInTemp(FormCollection col)
        //{
        //	string str = col["txtBranchCode1"].Substring(0, 2);
        //	string str1 = " SELECT * FROM tblBranchMasterTemp WHERE fldBranchCode=@branchcode and fldBankCode=@fldBankCode and fldStateCode=@fldStateCode";
        //	return this.dbContext.CheckExist(str1, new SqlParameter[] { new SqlParameter("@branchcode", col["branchCode"]), new SqlParameter("@fldBankCode", col["SelectBank"]), new SqlParameter("@fldStateCode", str) });
        //}

        public bool CheckBranchCodeExistInTemp(FormCollection col)
        {
            string branchCode = col["branchCode"];
            string bankCode = col["fldBankCode"];

            //string stateCode = branchCode.Substring(0, 2);
            string stmt = " SELECT * FROM tblBranchMasterTemp WHERE fldBranchCode=@branchcode and fldBankCode=@bankCode";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@branchcode", col["branchCode"]),
                new SqlParameter("@bankCode", col["fldBankCode"]),
                //new NpgsqlParameter("@fldStateCode", stateCode)
            });
        }

        public string condition()
        {
            return "len(fldBranchCode)=7";
        }

        public void CreateBranchMasterTemp(FormCollection col, string userId)
        {
            string str = "";
            string str1 = col["txtBranchCode1"].Substring(0, 2);
            str = (col["fldDisable"] != "Y" ? "N" : "Y");
            DataTable dataTable = new DataTable();
            if (this.dbContext.GetRecordsAsDataTable("Select * from tblBranchMasterTemp", null).Rows.Count != 0)
            {
                string str2 = "Insert into tblBranchMasterTemp (fldBranchId, fldStateCode, fldBranchCode, fldBranchDesc, fldDisable,fldCreateTimeStamp, fldCreateUserId, fldBankCode, fldApproveStatus) values ((select top 1 fldBranchId+1 from tblBranchMasterTemp order by fldCreateTimeStamp desc), @fldStateCode, @fldBranchCode, @fldBranchDesc, @fldDisable,@fldCreateTimeStamp, @fldCreateUserId, @fldBankCode, @fldApproveStatus)";
                this.dbContext.ExecuteNonQuery(str2, new SqlParameter[] { new SqlParameter("fldStateCode", str1), new SqlParameter("fldBranchCode", col["branchCode"]), new SqlParameter("fldBranchDesc", col["branchDesc"]), new SqlParameter("fldDisable", str), new SqlParameter("fldCreateTimeStamp", DateUtils.GetCurrentDatetime()), new SqlParameter("fldCreateUserId", userId), new SqlParameter("fldBankCode", col["SelectBank"]), new SqlParameter("fldApproveStatus", "A") });
            }
            else
            {
                string str3 = "Insert into tblBranchMasterTemp (fldBranchId, fldStateCode, fldBranchCode, fldBranchDesc, fldDisable,fldCreateTimeStamp, fldCreateUserId, fldBankCode, fldApproveStatus) values ((select top 1 fldBranchId+1 from tblBranchMaster order by fldCreateTimeStamp desc), @fldStateCode, @fldBranchCode, @fldBranchDesc, @fldDisable,@fldCreateTimeStamp, @fldCreateUserId, @fldBankCode, @fldApproveStatus)";
                this.dbContext.ExecuteNonQuery(str3, new SqlParameter[] { new SqlParameter("fldStateCode", str1), new SqlParameter("fldBranchCode", col["branchCode"]), new SqlParameter("fldBranchDesc", col["branchDesc"]), new SqlParameter("fldDisable", str), new SqlParameter("fldCreateTimeStamp", DateUtils.GetCurrentDatetime()), new SqlParameter("fldCreateUserId", userId), new SqlParameter("fldBankCode", col["SelectBank"]), new SqlParameter("fldApproveStatus", "A") });
            }
        }

        public void CreateInBranchMaster(string branchId)
        {
            string str = "Insert into tblBranchMaster(fldStateCode,fldBranchCode,fldBranchDesc,fldDisable,fldCreateTimeStamp,fldCreateUserId,fldBankCode) (Select fldStateCode,fldBranchCode,fldBranchDesc,fldDisable,fldCreateTimeStamp,fldCreateUserId,fldBankCode from tblBranchMasterTemp tblBankMasterTemp WHERE fldBranchCode = @fldBranchId) ";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId) });
        }

        public void DeleteInBranchMasters(string branchId)
        {
            string str = "delete from tblBranchMaster where fldBranchId=@fldBranchId";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId) });
        }

        public void DeleteInBranchMasterTemp(string branchId)
        {
            string str = "delete from tblBranchMasterTemp where fldBranchCode=@fldBranchId";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId) });
        }

        public List<BranchCodeModel> getBank()
        {
            DataTable dataTable = new DataTable();
            List<BranchCodeModel> branchCodeModels = new List<BranchCodeModel>();
            dataTable = this.dbContext.GetRecordsAsDataTable("select * from tblBankMaster", null);
            foreach (DataRow row in dataTable.Rows)
            {
                BranchCodeModel branchCodeModel = new BranchCodeModel()
                {
                    bankCode = row["fldBankCode"].ToString(),
                    bankDesc = row["fldBankDesc"].ToString()
                };
                branchCodeModels.Add(branchCodeModel);
            }
            return branchCodeModels;
        }

        public DataTable getBankType()
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", ""));
            return this.dbContext.GetRecordsAsDataTableSP("spcgBankType", sqlParameterNext.ToArray());
        }

        public DataTable getBranchCode(string branchId)
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            return this.dbContext.GetRecordsAsDataTableSP("spcgBranchMasterDataById", sqlParameterNext.ToArray());
        }

        public DataTable getBranchCodeFromCheckerView(string branchId)
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            return this.dbContext.GetRecordsAsDataTableSP("spcgBranchCodeFromCheckerView", sqlParameterNext.ToArray());
        }

        public DataTable ListAllBranchCode()
        {
            DataTable dataTable = new DataTable();
            return this.dbContext.GetRecordsAsDataTable("select * from tblbranchmaster WHERE 1=1 order by fldStateCode, fldBranchCode", null);
        }

        public void UpdateBranchMaster(FormCollection col, string userId)
        {
            string str = "";
            str = (col["fldDisable"] != "Y" ? "N" : "Y");
            string str1 = "update tblBranchMaster set fldBranchDesc=@fldBranchDesc, fldDisable=@fldDisable, fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldBranchId=@fldBranchId";
            this.dbContext.ExecuteNonQuery(str1, new SqlParameter[] { new SqlParameter("fldBranchId", col["fldBranchId"]), new SqlParameter("fldBranchDesc", col["branchDesc"]), new SqlParameter("fldDisable", str), new SqlParameter("fldUpdateUserId", userId), new SqlParameter("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime()) });
        }

        public async Task<List<string>> ValidateCreateAsync(FormCollection col)
        {
            List<string> strs = await Task.Run<List<string>>(() => this.ValidateCreate(col));
            return strs;
        }

        public bool CreateBranchMaster(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["stateCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["branchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", col["bankTypeId"]));
            sqlParameterNext.Add(new SqlParameter("@fldBusinessType", col["businessType"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMaster", sqlParameterNext.ToArray());

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

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["branchDesc"].Equals(""))
            {
                err.Add("Branch Description cannot be empty");
            }
            if (col["businessType"].Equals(""))
            {
                err.Add("Please select Business Type");
            }
            return err;
        }

        public BranchCodeModel GetBranchCodeDataById(string id)
        {
            BranchCodeModel branchCode = new BranchCodeModel();
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterDataById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                branchCode.branchId = row["fldBranchId"].ToString();
                branchCode.stateCode = row["fldStateCode"].ToString();
                branchCode.bankCode = row["fldBankCode"].ToString();
                branchCode.branchCode = row["fldBranchCode"].ToString();
                branchCode.branchDesc = row["fldBranchDesc"].ToString();
                branchCode.createUserId = row["fldCreateUserId"].ToString();
                branchCode.createTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                branchCode.updateUserId = row["fldUpdateUserId"].ToString();
                branchCode.updateTimeStamp = row["fldUpdateTimeStamp"].ToString();
                branchCode.bankType = row["fldBankType"].ToString();
                branchCode.businessType = row["fldBusinessType"].ToString();


            }

            return branchCode;

        }

        public bool UpdateBranchCode(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBusinessType", col["businessType"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchMaster", sqlParameterNext.ToArray());

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

        public bool CheckBranchCodeTempById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }
        public bool CreateBranchCodeTemp(FormCollection col,string action)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBusinessType", col["businessType"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            if (action.Equals("A"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldBankType", col["bankTypeId"]));
                sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["stateCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["branchCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", action));

            }
            else if (action.Equals("U"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldBankType", col["bankTypeId"]));
                sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["stateCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["bankCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["branchCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.ParseExact(col["createTimeStamp"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", action));

            }

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMasterTemp", sqlParameterNext.ToArray());

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

        public bool DeleteBranchCode(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBranchMaster", sqlParameterNext.ToArray());

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

        public bool CreateBranchCodeTempToDelete(String id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMasterTempToDelete", sqlParameterNext.ToArray());

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

        public bool MoveToBranchCodeFromTemp(String id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMasterFromTemp", sqlParameterNext.ToArray());

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

        public bool UpdateBranchCodeById(string id)
        {
           // ReturnCodeModel bankCode = new ReturnCodeModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuBranchMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool DeleteBranchCodeTemp(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBranchMasterTemp", sqlParameterNext.ToArray());

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

        public bool getHQ(FormCollection col)
        {

            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["branchId"].Substring(1,3)));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["branchId"].Substring(4, 4)));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHQ", sqlParameterNext.ToArray());


            if (col["chkHQ"] == "2")
            {
                Flag = false;
            }

            else
            {
                if (((resultTable.Rows.Count > 0)))
            {
                Flag = true;
            }
                else
                {
                    Flag = false;
                }

            }
            return Flag;

        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<String>();


            if (col["branchId"].Equals(""))
            {
                err.Add("Branch ID cannot be empty");
            }
            else
            {
                if (col["branchId"].Length != 8)
                {
                    err.Add("Branch ID must be 8 digits");
                }
                else
                {
                    bool IsBranchMasterExist = CheckBranchCodeById(col["branchId"]);
                    bool IsStateMasterExist = CheckStateCodeById(col["stateCode"]);

                    if (IsBranchMasterExist == true)
                    {
                        err.Add(Locale.BranchCodeAlreadyExist);
                    }

                    bool IsBranchMasterTempExist = CheckBranchCodeTempById(col["branchId"]);
                    if (IsBranchMasterTempExist == true)
                    {
                        err.Add(Locale.BranchCodealreadyCreatedinApproveChecker);
                    }
                    else
                    {
                        if (IsStateMasterExist == false)
                        {
                            err.Add("Invalid State Code");
                        }
                        if (!(new Regex("^[0-9]+$")).IsMatch(col["branchCode"]))
                        {
                            err.Add(Locale.BranchCodeNumber);
                        }
                        if (col["bankCode"].Equals(""))
                        {
                            err.Add("Invalid Bank Code");
                        }
                        if (col["branchCode"].Length != 3)
                        {
                            err.Add(Locale.BranchCodeLimit);
                        }
                        if (col["branchDesc"].Equals(""))
                        {
                            err.Add("Branch Description cannot be empty");
                        }
                        if (col["bankTypeId"].Equals(""))
                        {
                            err.Add(Locale.ChooseBankType);
                        }
                        if (col["businessType"].Equals(""))
                        {
                            err.Add("Please select Business Type");
                        }
                    }
                } 

                



            
            }
            
        
            return err;
        }

        public bool CheckBranchCodeById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckLocationCodeById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgLocationMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckStateCodeById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgStateMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }
    }
}