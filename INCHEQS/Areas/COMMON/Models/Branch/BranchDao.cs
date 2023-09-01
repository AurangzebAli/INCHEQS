
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using INCHEQS.Common;
using INCHEQS.Security.SystemProfile;

namespace INCHEQS.Areas.COMMON.Models.Branch
{
    public class BranchDao : IBranchDao
    {


        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;
        public BranchDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

      
        public void MoveToBranchMasterFromTemp(string branchCode, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", branchCode));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMasterFromTemp", sqlParameterNext.ToArray());
        }
        //BEGIN 20200404: LENDER ADD FUNCTION TO VALIDATE THE DATA.


        public List<string> ValidateBranch(FormCollection col, string action, string userId)
         {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("BranchCodeChecker", CurrentUser.Account.BankCode).Trim();
            List<string> strs = new List<string>();
            int counter = 0;
            BranchModel CheckBranchExist = CheckBranchMasterByID(col["branchCode"], "", "BranchCode");
            //BranchModel CheckBranchTempExist = CheckBranchMasterTempByID(col["branchCode"], "", "BranchCode");
            if ((action.Equals("Create")))
            {
                if (col["branchCode"].Equals(""))
                {
                    strs.Add("Branch Code is required");

                }
                else
                {
                    if ((CheckBranchExist != null))
                    {
                        strs.Add(Locale.BranchCodeAlreadyExist);
                    }
                       if (!(new Regex("^[0-9]+$")).IsMatch(col["branchCode"]))
                    {
                        strs.Add("Branch Code must be numbers");
                    }
                    if (col["branchCode"].Length != 5)
                    {
                        strs.Add(" Branch Code must be equal to 5");
                    }
                }
                if (col["branchDesc"].Equals(""))
                {
                    strs.Add("Branch Description is required");
                }

                if (!col.AllKeys.Contains("branchLevel"))
                {
                    strs.Add("Branch Level is required");
                }

            }
            else if ((action.Equals("Update")))
            {
                if (col["branchDesc"].Equals(""))
                {
                    strs.Add("Branch Description is required");
                }
                if (col["branchDesc"].Equals(CheckBranchExist.fldBranchDesc.ToString()))
                {
                    counter++;
                }
                if (col["branchLevel"].Equals(CheckBranchExist.fldBranchLevel.ToString()))
                {
                    counter++;
                }
                if (counter == 2)
                {
                    strs.Add(Locale.NoChanges);
                }

               
            }


                return strs;
        }
      
        
        public BranchModel CheckBranchMasterByID(string BranchCode, string BranchID, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BranchModel branch = new BranchModel();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", BranchID));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", BranchCode));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterbyId", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                branch.fldBranchId = row["fldBranchId"].ToString();
                branch.fldBranchCode = row["fldBranchCode"].ToString();
                branch.fldBranchDesc = row["fldBranchDesc"].ToString();
                branch.fldBankCode = row["fldBankCode"].ToString();
                branch.fldCreateUserId = row["fldCreateUserId"].ToString();
                branch.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                branch.fldUpdateUserId = row["fldUpdateUserId"].ToString();
                branch.fldUpdateTimeStamp = DateUtils.formatDateFromSql(row["fldUpdateTimeStamp"].ToString());
                branch.fldBranchLevel = row["fldBranchLevel"].ToString();

            }
            else
            {
                branch = null;
            }
            return branch;
        }
        public bool CheckBranchMasterTempByID(string BranchCode, string BranchID, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BranchModel branch = new BranchModel();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", BranchID));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", BranchCode));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterTempbyId", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CreateBranchMasterTemp(FormCollection col, string bankcode, string crtUser, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = bankcode/*CurrentUser.Account.BankCode*/;
            if (Action == "Create")
            {
                sqlParameterNext.Add(new SqlParameter("@fldBranchId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["branchCode"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@fldBranchLevel", col["branchLevel"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
            else if (Action == "Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["fldBranchId"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBranchCode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
                sqlParameterNext.Add(new SqlParameter("@fldBranchLevel", col["branchLevel"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldBranchId", bankcode));
                sqlParameterNext.Add(new SqlParameter("@fldBranchCode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@fldBranchLevel", ""));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBranchMasterTemp", sqlParameterNext.ToArray());
        }
        

     
        public bool UpdateBranchMaster(FormCollection col, string userId)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["fldBranchId"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["branchDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(userId)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldBranchLevel", col["branchLevel"].ToString()));
         

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
      
        public bool DeleteInBranchMasters(string BranchCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", BranchCode));
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

        public bool DeleteInBranchMastersTemp(string BranchCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", BranchCode));
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

        public string GetMenuTitle(string TaskId)
        {
            string MenuTitle = "";
            try
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
                DataTable resultTable = new DataTable();
                resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetTitle", sqlParameterNext.ToArray());

                if (resultTable.Rows.Count > 0)
                {
                    DataRow drItem = resultTable.Rows[0];
                    MenuTitle = drItem["fldMenuTitle"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return MenuTitle;

        }

    }
}
