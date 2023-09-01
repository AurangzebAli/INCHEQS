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

namespace INCHEQS.Areas.COMMON.Models.StateCode
{

    public class StateCodeDao : IStateCodeDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public StateCodeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }

        public StateCodeModel GetStateCode(string StateCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            StateCodeModel statecode = new StateCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                statecode.fldStateCode = row["fldStateCode"].ToString();
                statecode.fldStateDesc = row["fldStateDesc"].ToString();
                statecode.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                statecode.fldCreateUserId = row["fldCreateUserId"].ToString();
            }
            else
            {
                statecode = null;
            }
            return statecode;
        }

        public StateCodeModel GetStateCodeTemp(string StateCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            StateCodeModel statecode = new StateCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgStateCodeTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                statecode.fldStateCode = row["fldStateCode"].ToString();
                statecode.fldStateDesc = row["fldStateDesc"].ToString();
                statecode.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                statecode.fldCreateUserId = row["fldCreateUserId"].ToString();
                statecode.fldApproveStatus = row["fldApproveStatus"].ToString();
            }
            else
            {
                statecode = null;
            }
            return statecode;
        }

        public List<string> ValidateStateCode(FormCollection col, string action)
        {
            List<string> err = new List<string>();

            if ((action.Equals("Update")))
            {
                if ((col["fldStateDesc"].Equals("")))
                {
                    err.Add(Locale.StateDescCannotBeBlank);
                }
            }
            else if ((action.Equals("Create")))
            {
                StateCodeModel CheckStateExist = GetStateCode(col["fldStateCode"]);
                if ((CheckStateExist != null))
                {
                    err.Add(Locale.StateDescExist);
                }
                if ((col["fldStateCode"].Equals("")))
                {
                    err.Add(Locale.StateDescCannotBeBlank);
                }
            }
            return err;

        }

        public bool UpdateStateCodeMaster(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["fldStateCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldStateDesc", col["fldStateDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuStateCodeMaster", sqlParameterNext.ToArray());
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

        public void CreateStateCodeMasterTemp(FormCollection col, string crtUser, string StateCode, string Action)
        {
            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
            string fldApproveStatus = "";
            string fldCreateUserId = "";
            string fldCreateTimeStamp = "";

            if (Action == "Update")
            {
                Action = "Update";
                fldApproveStatus = "U";

                StateCodeModel statecode = GetStateCode(col["fldStateCode"]);
                fldCreateTimeStamp = statecode.fldCreateTimeStamp;
                fldCreateUserId = statecode.fldCreateUserId;
            }
            else if (Action == "Create")
            {
                Action = "Create";
                fldApproveStatus = "A";
                fldCreateUserId = crtUser;
                fldCreateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            }
            else {
                Action = "Delete";
                fldApproveStatus = "D";

                StateCodeModel statecode = GetStateCode(StateCode);
                fldCreateTimeStamp = statecode.fldCreateTimeStamp;
                fldCreateUserId = statecode.fldCreateUserId;
                col["fldStateCode"] = statecode.fldStateCode;
                col["fldStateDesc"] = statecode.fldStateDesc;
            }

            string daf = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");

            sqlParameterNext1.Add(new SqlParameter("@fldStateCode", col["fldStateCode"]));
            sqlParameterNext1.Add(new SqlParameter("@fldStateDesc", col["fldStateDesc"]));
            sqlParameterNext1.Add(new SqlParameter("@fldApproveStatus", fldApproveStatus));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", fldCreateUserId));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateTimeStamp", fldCreateTimeStamp));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", crtUser));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateTimeStamp", daf));
            sqlParameterNext1.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciStateCodeMasterTemp", sqlParameterNext1.ToArray());

        }

        public bool DeleteStateCodeMaster(string stateCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", stateCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdStateCodeMaster", sqlParameterNext.ToArray());
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

        public bool CheckStateCodeMasterTempById(string stateCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", stateCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgStateCodeMasterTempById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateStateCodeMaster(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;
            
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", col["fldStateCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldStateDesc", col["fldStateDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciStateCodeMaster", sqlParameterNext.ToArray());
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

        public void MoveToStateCodeMasterFromTemp(string stateCode, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", stateCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciStateCodeMasterFromTemp", sqlParameterNext.ToArray());
        }

        public bool DeleteStateCodeMasterTemp(string stateCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", stateCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdStateCodeMasterTemp", sqlParameterNext.ToArray());
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
