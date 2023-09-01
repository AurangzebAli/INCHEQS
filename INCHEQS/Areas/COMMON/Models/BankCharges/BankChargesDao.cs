using INCHEQS.Models;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using System.Text.RegularExpressions;
using INCHEQS.Security;

namespace INCHEQS.Areas.COMMON.Models.BankCharges
{

    public class BankChargesDao : IBankChargesDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public BankChargesDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }
        public List<BankChargesModel> GetBankChargesTypeAndProductCodeList(string tablename)
        {
            dynamic tableName = tablename;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<BankChargesModel> fieldList = new List<BankChargesModel>();
            sqlParameterNext.Add(new SqlParameter("@tablename", tableName));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTypeAndProductCode", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {

                //DataRow row = resultTable.Rows[0];
                if (tablename == "tblProductMaster")
                {
                    foreach (DataRow row in resultTable.Rows)
                    {
                        BankChargesModel field = new BankChargesModel();
                        field.fldProductCode = row["fldProductCode"].ToString();
                        fieldList.Add(field);
                    }
                }
                else if (tablename == "tblBankChargesTypeMaster")
                {
                    foreach (DataRow row in resultTable.Rows)
                    {
                        BankChargesModel field = new BankChargesModel();
                        field.fldBankChargesType = row["fldBankChargesType"].ToString();
                        fieldList.Add(field);
                    }
                }
            }
            else
            {
                fieldList = null;
            }
            return fieldList;
        }

        public string GetBankChargesDesc(string type) {
            string description;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", type));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesDesc", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                description = row["fldBankChargesDesc"].ToString();
            }
            else
            {
                description = null;
            }
            return description;
        }

        //Check Existing Record
        public bool ValidateExistingBankCharges(string productCode, string bankcode, string bankcharges, string minAmount, string maxAmount, string status, string tbltype) {
            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankcharges));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount.Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount.Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
            sqlParameterNext.Add(new SqlParameter("@tbltype", tbltype));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgExistingBankCharges", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0) { 

                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        //Validate BankCharges
        public List<string> ValidateBankCharges(FormCollection col, string action) //Done
        {

            List<string> err = new List<string>();

            if (action.Equals("Update"))
            {

                if (col.AllKeys.Contains("amount"))
                {
                    if (col["amount"].Equals("AmountRange"))
                    {
                        if (col["fldChequeAmtMax"].Equals(""))
                        {
                            err.Add(Locale.AmountMaxBlank);
                        }
                        

                        if (col["fldChequeAmtMin"].Equals(""))
                        {
                            err.Add(Locale.AmountMinBlank);
                        }
                        else
                        {
                            
                            if (float.Parse(col["fldChequeAmtMin"].Replace(",","")) < 0)
                        {
                            err.Add(Locale.AmountLessThanMin);
                        }
                           
                        }

                    } 
                    if (float.Parse(col["fldChequeAmtMax"].Replace(",", "")) <= float.Parse(col["fldChequeAmtMin"].Replace(",", "")))
                    {
                        err.Add(Locale.AmountMaxMoreThanMin);
                    }
                }
                else
                {
                    err.Add(Locale.ChooseChequeAmount);
                }

                if (col.AllKeys.Contains("bankcharges"))
                {
                    if (col["bankcharges"].Equals("ChargesAmount"))
                    {
                        if (col["fldBankChargesAmount"].Equals(""))
                        {
                            err.Add(Locale.AmountBlank);
                        }

                        

                    }
                    else
                    {
                        if (col["fldBankChargesRate"].Equals(""))
                        {
                            err.Add(Locale.RateBlank);
                        }

                      

                        if (float.Parse(col["fldBankChargesRate"]) > 100.00)
                        {
                            err.Add(Locale.RateMax);
                        }
                        if (float.Parse(col["fldBankChargesRate"]) < 0)
                        {
                            err.Add(Locale.RateMin);
                        }
                    }
                }
                else
                {
                    err.Add(Locale.ChooseBankCharges);
                }
                //Validate Existing
                /*bool validate = ValidateExistingBankCharges(col["fldProductCode"], CurrentUser.Account.BankCode, col["fldBankChargesType"], col["fldChequeAmtMin"], col["fldChequeAmtMax"], "", "");
                if (validate == true)
                {
                    err.Clear();
                    err.Add("Record Already Exist!, Please Enter Different Amount Range!");
                }*/



            }
            else if (action.Equals("Create"))
            {
                if (col["fldProductCode"].Equals("")) {
                    err.Add(Locale.ChooseProductCode);
                }

                if (col["fldBankChargesType"].Equals("")) {
                    err.Add(Locale.ChooseBankChargesType);
                }
                    if (col.AllKeys.Contains("amount"))
                {
                    if (col["amount"].Equals("AmountRange"))
                    {
                        if (col["fldChequeAmtMax"].Equals(""))
                        {
                            err.Add(Locale.AmountMinBlank);
                        }
                     

                        if (col["fldChequeAmtMin"].Equals(""))
                        {
                            err.Add(Locale.AmountMinBlank);
                        }
                        else
                        {
                            
                                if (float.Parse(col["fldChequeAmtMin"].Replace(",", "")) < 0)
                        {
                            err.Add(Locale.AmountLessThanMin);
                        }
                           
                    }

                        if (!(col["fldChequeAmtMin"].Equals("") && col["fldChequeAmtMax"].Equals("")))
                        {
                            if (float.Parse(col["fldChequeAmtMax"].Replace(",", "")) <= float.Parse(col["fldChequeAmtMin"].Replace(",", "")))
                    {
                        err.Add(Locale.AmountMaxMoreThanMin);
                    }
                }

                    }
 
                }
                else {
                    err.Add(Locale.ChooseChequeAmount);
                }

                if (col.AllKeys.Contains("bankcharges"))
                {
                    if (col["bankcharges"].Equals("ChargesAmount"))
                    {
                        if (col["fldBankChargesAmount"].Equals(""))
                        {
                            err.Add(Locale.AmountBlank);
                        }
                        

                        
                    }
                    else
                    {
                        if (col["fldBankChargesRate"].Equals(""))
                        {
                            err.Add(Locale.RateBlank);
                        }
                        else
                        {
                            
                                if (float.Parse(col["fldBankChargesRate"]) > 100.00)
                                {
                            err.Add(Locale.RateMax);
                        }
                        if (float.Parse(col["fldBankChargesRate"]) < 0)
                        {
                            err.Add(Locale.RateMin);
                        }
                         
                        }
                    }
                }
                else {
                    err.Add(Locale.ChooseBankCharges);
                }

                //Validate Existing Record
                bool validate = ValidateExistingBankCharges(col["fldProductCode"], CurrentUser.Account.BankCode, col["fldBankChargesType"],col["fldChequeAmtMin"].Replace(",", ""), col["fldChequeAmtMax"].Replace(",", ""), "","");
                if (validate == true)
                {
                    err.Clear();
                    err.Add(Locale.BankChargesExist);
                } 
                
 

            }
            return err;

        }

        public bool CreateBankCharges(FormCollection col, string bankcode)
        {

            int intRowAffected;
            bool blnResult = false;
            dynamic fldBankChargesRate = "";
            dynamic fldBankChargesAmount = "";
            if (col["fldBankChargesAmount"] != "")
            {
                fldBankChargesRate = "0";
                fldBankChargesAmount = col["fldBankChargesAmount"].Replace(",", "");
            }
            else
            {

                fldBankChargesRate = col["fldBankChargesRate"].Replace(",", "");
                fldBankChargesAmount = "0";
            }


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", col["fldProductCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", col["fldBankChargesType"]));

            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", col["fldChequeAmtMin"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", col["fldChequeAmtMax"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesAmount", fldBankChargesAmount));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesRate", fldBankChargesRate));

            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankCharges", sqlParameterNext.ToArray()); //Done
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

        public void CreateBankChargesTemp(FormCollection col, string bankcode, string crtUser, string Action, string productcode, string maxAmount, string minAmount, string bankchargestype) //done
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = bankcode;/*CurrentUser.Account.BankCode*/
            dynamic fldApproveStatus = "";
            dynamic fldBankChargesType = "";
            dynamic fldCreateUserId = "";
            dynamic fldBankChargesAmount = "";
            dynamic fldBankChargesRate = "";
            dynamic fldChequeAmtMin = "";
            dynamic fldChequeAmtMax = "";
            dynamic fldProductCode = "";
            dynamic currMaxAmount = "";
            dynamic currMinAmount = "";

            if (col["fldBankChargesAmount"] != null)
            {
                fldBankChargesAmount = col["fldBankChargesAmount"].Replace(",", "");
            }

            if (col["fldBankChargesRate"] != null)
            {
                fldBankChargesRate = col["fldBankChargesRate"];
            }


            if (Action == "Update")
            {
                Action = "Update";
                fldApproveStatus = "U";
                fldCreateUserId = crtUser;
                fldBankChargesType = col["fldBankChargesType"].ToString();
                fldChequeAmtMin = col["fldChequeAmtMin"].Replace(",", "");
                fldChequeAmtMax = col["fldChequeAmtMax"].Replace(",", "");
                fldProductCode = col["fldProductCode"];
                currMaxAmount = col["fldChequeAmtMaxCur"].Replace(",", "");
                currMinAmount = col["fldChequeAmtMinCur"].Replace(",", "");
            }
            else if (Action == "Create")
            {
                Action = "Create";
                fldApproveStatus = "A";
                fldBankChargesType = col["fldBankChargesType"].ToString();
                fldCreateUserId = crtUser;
                fldChequeAmtMin = col["fldChequeAmtMin"].Replace(",", "");
                fldChequeAmtMax = col["fldChequeAmtMax"].Replace(",", "");
                fldProductCode = col["fldProductCode"];
            }
            else {
                Action = "Delete";
                fldApproveStatus = "D";
                fldBankChargesType = bankchargestype;
                fldChequeAmtMin = minAmount;
                fldChequeAmtMax = maxAmount;
                fldProductCode = productcode;

            }

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", fldProductCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", fldBankChargesType));

            sqlParameterNext.Add(new SqlParameter("@currMinAmount", currMinAmount));
            sqlParameterNext.Add(new SqlParameter("@currMaxAmount", currMaxAmount));

            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", fldChequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", fldChequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesAmount", fldBankChargesAmount));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesRate", fldBankChargesRate));

            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", fldApproveStatus));

            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankChargesTemp", sqlParameterNext.ToArray()); //done

        }

        public bool DeleteBankCharges(string productCode, string bankcode, string bankchargestype, string minAmount, string maxAmount)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankCharges", sqlParameterNext.ToArray()); //Done

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

        public bool CheckBankChargesTempById(string productCode, string bankcode, string bankchargestype, string minAmount, string maxAmount) //Done
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTempById", sqlParameterNext.ToArray()); //Done

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UpdateBankCharges(FormCollection col) //done
        {

            int intRowAffected;
            bool blnResult = false;
            dynamic fldBankChargesRate = "";
            dynamic fldBankChargesAmount = "";
            if (col["fldBankChargesAmount"] != "")
            {
                fldBankChargesRate = "0";
                fldBankChargesAmount = col["fldBankChargesAmount"];
            }
            else {

                fldBankChargesRate = col["fldBankChargesRate"];
                fldBankChargesAmount = "0";
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", col["fldProductCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", col["fldBankChargesType"]));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", col["fldChequeAmtMin"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", col["fldChequeAmtMax"].Replace(",", "")));

            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMinCur", col["fldChequeAmtMinCur"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMaxCur", col["fldChequeAmtMaxCur"].Replace(",", "")));

            sqlParameterNext.Add(new SqlParameter("@fldBankChargesAmount", fldBankChargesAmount));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesRate", fldBankChargesRate));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBankCharges", sqlParameterNext.ToArray()); //done
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

        public void MoveToBankChargesFromTemp(string productCode, string bankCode, string bankcharges, string minAmount, string maxAmount, string Action)//Done
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankcharges));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankChargesFromTemp", sqlParameterNext.ToArray());//Done
        }

        public bool DeleteBankChargesTemp(string productCode, string bankcode, string bankcharges, string minAmount, string maxAmount) //Done
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankcharges));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankChargesTemp", sqlParameterNext.ToArray());

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

        public BankChargesModel GetBankCharges(string bankCode, string productCode, string bankChargesType, string minAmount, string maxAmount, string tblname) //DONE
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankChargesModel data = new BankChargesModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankChargesType));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesandTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                data.fldBankChargesType = row["fldBankChargesType"].ToString();
                data.fldProductCode = row["fldProductCode"].ToString();
                data.fldChequeAmtMin = row["fldChequeAmtMin"].ToString();
                data.fldChequeAmtMax = row["fldChequeAmtMax"].ToString();
                data.fldBankChargesAmount = row["fldBankChargesAmount"].ToString();
                data.fldBankChargesRate = row["fldBankChargesRate"].ToString();
                data.fldBankChargesDesc = row["fldBankChargesDesc"].ToString();

                if (tblname == "temp") {
                    data.fldApproveStatus = row["fldApproveStatus"].ToString();
                }
            }
            else
            {
                data = null;
            }
            return data;
        }
        /*public List<UserModel> ListBranch(string bankcode)
        {
            DataTable resultTable = new DataTable();
            List<UserModel> branchList = new List<UserModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchesForUserProfile", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel branch = new UserModel();
                    branch.fldConBranchCode = row["fldConBranchCode"].ToString();
                    branch.fldBranchDesc = row["fldBranchDesc"].ToString();
                    branchList.Add(branch);
                }
            }
            return branchList;
        }*/



        /*
        public BankChargesTypeModel GetBankChargesType(string bankchargestype) //DONE
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankChargesTypeModel type = new BankChargesTypeModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                type.fldBankChargesType = row["fldBankChargesType"].ToString();
                type.fldBankChargesDesc = row["fldBankChargesDesc"].ToString();
            }
            else
            {
                type = null;
            }
            return type;
        }

        public BankChargesTypeModel GetBankChargesTypeTemp(string bankchargestype) //Done
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankChargesTypeModel type = new BankChargesTypeModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTypeTemp", sqlParameterNext.ToArray()); //Done

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                type.fldBankChargesType = row["fldBankChargesType"].ToString();
                type.fldBankChargesDesc = row["fldBankChargesDesc"].ToString();
                type.fldApproveStatus = row["fldApproveStatus"].ToString();
            }
            else
            {
                type = null;
            }
            return type;
        }

        public bool UpdateBankChargesTypeMaster(FormCollection col) //done
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", col["fldBankChargesType"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesDesc", col["fldBankChargesDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBankChargesTypeMaster", sqlParameterNext.ToArray()); //done
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

        public void CreateBankChargesTypeMasterTemp(FormCollection col, string bankcode, string crtUser, string Action) //done
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = bankcode;/*CurrentUser.Account.BankCode
            dynamic fldApproveStatus = "";
            dynamic fldBankChargesType = "";
            dynamic fldCreateUserId = "";


            if (Action == "Update")
            {
                Action = "Update";
                fldApproveStatus = "U";
                fldCreateUserId = crtUser;
                fldBankChargesType = col["fldBankChargesType"].ToString();
            }
            else if (Action == "Create")
            {
                Action = "Create";
                fldApproveStatus = "A";
                fldBankChargesType = col["fldBankChargesType"].ToString();
                fldCreateUserId = crtUser;
            }
            else {
                Action = "Delete";
                fldApproveStatus = "D";
                fldBankChargesType = crtUser;
            }

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", fldBankChargesType));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesDesc", col["fldBankChargesDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", fldApproveStatus));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", fldCreateUserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", crtUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankChargesTypeMasterTemp", sqlParameterNext.ToArray()); //done

        }

        public bool DeleteBankChargesTypeMaster(string bankchargestype, string bankcode) //Done
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankChargesTypeMaster", sqlParameterNext.ToArray()); //Done
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

        public bool CheckBankChargesTypeMasterTempById(string bankchargestype, string bankcode) //Done
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTypeMasterTempById", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateBankChargesTypeMaster(FormCollection col, string bankcode) //Done
        {

            int intRowAffected;
            bool blnResult = false;


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", col["fldBankChargesType"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesDesc", col["fldBankChargesDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankChargesTypeMaster", sqlParameterNext.ToArray()); //Done
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

        public bool DeleteBankChargesTypeMasterTemp(string bankchargestype) //Done
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankChargesTypeMasterTemp", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }*/

    }
}
