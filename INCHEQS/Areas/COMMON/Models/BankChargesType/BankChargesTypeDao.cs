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

namespace INCHEQS.Areas.COMMON.Models.BankChargesType
{

    public class BankChargesTypeDao : IBankChargesTypeDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public BankChargesTypeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }

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

        //Validate BankChargesType
        public List<string> ValidateBankChargesType(FormCollection col, string action, string bankcode) //Done
        {

            //string systemLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            //string domain = securityProfileDao.GetSecurityProfile().fldUserAuthMethod;
            //dynamic securityProfile = securityProfileDao.GetSecurityProfile();
            List<string> err = new List<string>();

            if ((action.Equals("Update")))
            {
                BankChargesTypeModel CheckExistingDescription = GetBankChargesType(col["fldBankChargesType"]);

                    if ((col["fldBankChargesDesc"].Equals("")))
                    {
                        err.Add(Locale.BankChargesTypeDesccannotbeblank);
                    }
                    else {
                        if ((col["fldBankChargesDesc"]).Equals(CheckExistingDescription.fldBankChargesDesc)) {
                            err.Add(Locale.BankChargesTypeDescNotUpdated);
                        }
                    }
            }
            else if ((action.Equals("Create")))
            {
                BankChargesTypeModel CheckBankChargesExist = GetBankChargesType(col["fldBankChargesType"]);
                if ((CheckBankChargesExist != null))
                {
                    err.Add(Locale.BankChargesTypeExist);
                }
                if ((col["fldBankChargesDesc"].Equals("")))
                {
                    err.Add(Locale.BankChargesTypeDesccannotbeblank);
                }
                if ((col["fldBankChargesType"].Equals("")))
                {
                    err.Add(Locale.BankChargesTypecannotbeblank);
                }
                else
                {
                if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldBankChargesType"]))
                {
                        err.Add(Locale.BankChargesTypecannotbespecialcharacters);
                }
                }
                
            }
                return err;

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
            dynamic bankCode = bankcode;/*CurrentUser.Account.BankCode*/
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

        public void MoveToBankChargesTypeMasterFromTemp(string bankchargestype, string Action)//Done
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankChargesTypeMasterFromTemp", sqlParameterNext.ToArray());//Done
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
        }

    }
}
