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
using System.Collections;

namespace INCHEQS.Areas.COMMON.Models.ReturnCode
{

    public class ReturnCodeDao : IReturnCodeDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public ReturnCodeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }

        public ReturnCodeModel GetReturnCode(string ReturnCode)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ReturnCodeModel returncode = new ReturnCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                returncode.fldRejectCode = row["fldRejectCode"].ToString();
                returncode.fldRejectDesc = row["fldRejectDesc"].ToString();
                returncode.fldRepresentable = row["fldRepresentable"].ToString();
                returncode.fldRejectType = row["fldRejectType"].ToString();
                returncode.fldCharges = row["fldCharges"].ToString();
                returncode.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                returncode.fldCreateUserId = row["fldCreateUserId"].ToString();
            }
            else
            {
                returncode = null;
            }
            return returncode;
        }

        public ReturnCodeModel GetReturnCodeTemp(string ReturnCode)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ReturnCodeModel returncode = new ReturnCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgReturnCodeTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                returncode.fldRejectCode = row["fldRejectCode"].ToString();
                returncode.fldRejectDesc = row["fldRejectDesc"].ToString();
                returncode.fldRepresentable = row["fldRepresentable"].ToString();
                returncode.fldRejectType = row["fldRejectType"].ToString();
                returncode.fldCharges = row["fldCharges"].ToString();
                returncode.fldApproveStatus = row["fldApproveStatus"].ToString();
            }
            else
            {
                returncode = null;
            }
            return returncode;
        }

        //Validate ReturnCode
        public List<string> ValidateReturnCode(FormCollection col, string action)
        {
            List<string> err = new List<string>();
            string value = col["fldRepresentable"];

            if ((action.Equals("Update")))
            {
                if ((col["fldRejectDesc"].Equals("")))
                {
                    err.Add(Locale.ReturnDescCannotBeBlank);
                }
                if ((col["fldRejectType"].Equals("")))
                {
                    err.Add(Locale.ChooseRightReturnType);
                }
                if ((col["fldCharges"].Equals("")))
                {
                    err.Add(Locale.ChargesCannotBeBlank);
                }
                /*if ((col["fldRepresentable"] == null))
                {
                    err.Add(Locale.RepresentableCannotBeBlank);
                }*/
            }
            else if ((action.Equals("Create")))
            {
                ReturnCodeModel CheckUserExist = GetReturnCode(col["fldRejectCode"]);
                if ((CheckUserExist != null))
                {
                    err.Add(Locale.ReturnCodeAlreadyExist);
                }
                if ((col["fldRejectDesc"].Equals("")))
                {
                    err.Add(Locale.ReturnDescCannotBeBlank);
                }
                if ((col["fldRejectCode"].Equals("")))
                {
                    err.Add(Locale.ReturnCodeCannotBeBlank);
                }
                /*if ((col["fldRepresentable"] == null))
                {
                    err.Add(Locale.RepresentableCannotBeBlank);
                }*/
                if ((col["fldRejectType"].Equals("")))
                {
                    err.Add(Locale.ChooseRightReturnType);
                }
                if ((col["fldCharges"].Equals("")))
                {
                    err.Add(Locale.ChargesCannotBeBlank);
                }
                else {
                if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldRejectCode"]))
                {
                        err.Add(Locale.ReturnCodecannotbespecialcharacters);
                }
                }
               
            }
                return err;

        }

        public bool UpdateReturnCodeMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;
            string fldCharges = "";

            if (col["fldCharges"] != null)
            {
                fldCharges = col["fldCharges"].Replace(",", "");
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", col["fldRejectCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectDesc", col["fldRejectDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldRepresentable", ""));
            sqlParameterNext.Add(new SqlParameter("@fldRejectType", col["fldRejectType"]));
            sqlParameterNext.Add(new SqlParameter("@fldCharges", fldCharges));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuReturnCodeMaster", sqlParameterNext.ToArray());
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
        public void CreateReturnCodeMasterTemp(FormCollection col, string returnCode, string crtUser, string Action)
        {
            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
            string fldApproveStatus = "";
            string fldCreateUserId = "";
            string fldCreateTimeStamp = "";
            string fldCharges = "";

            if (col["fldCharges"] != null)
            {
                fldCharges = col["fldCharges"].Replace(",", "");
            }

            if (Action == "Update")
            {
                Action = "Update";
                fldApproveStatus = "U";

                ReturnCodeModel returncode = GetReturnCode(col["fldRejectCode"]);
                fldCreateTimeStamp = returncode.fldCreateTimeStamp;
                fldCreateUserId = returncode.fldCreateUserId;
            }
            else if (Action == "Create")
            {
                Action = "Create";
                fldApproveStatus = "A";
                fldCreateUserId = crtUser;
                fldCreateTimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
            }
            else
            {
                Action = "Delete";
                fldApproveStatus = "D";

                ReturnCodeModel returncode = GetReturnCode(returnCode);
                col["fldRejectCode"] = returncode.fldRejectCode;
                col["fldRejectDesc"] = returncode.fldRejectDesc;
                col["fldRepresentable"] = returncode.fldRepresentable;
                col["fldRejectType"] = returncode.fldRejectType;
                fldCharges = returncode.fldCharges;
                fldCreateTimeStamp = returncode.fldCreateTimeStamp;
                fldCreateUserId = returncode.fldCreateUserId;

                //if (col["fldCharges"] != null)
                //{
                //    fldCharges = col["fldCharges"].Replace(",", "");
                //}
            }

            sqlParameterNext1.Add(new SqlParameter("@fldRejectCode", col["fldRejectCode"]));
            sqlParameterNext1.Add(new SqlParameter("@fldRejectDesc", col["fldRejectDesc"].ToString()));
            sqlParameterNext1.Add(new SqlParameter("@fldRepresentable", col["fldRepresentable"].ToString()));
            sqlParameterNext1.Add(new SqlParameter("@fldRejectType", col["fldRejectType"]));
            sqlParameterNext1.Add(new SqlParameter("@fldCharges", fldCharges));
            sqlParameterNext1.Add(new SqlParameter("@fldApproveStatus", fldApproveStatus));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", fldCreateUserId));
            sqlParameterNext1.Add(new SqlParameter("@fldCreateTimeStamp", fldCreateTimeStamp));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", crtUser));
            sqlParameterNext1.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt")));
            sqlParameterNext1.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciReturnCodeMasterTemp", sqlParameterNext1.ToArray());

        }

        public bool DeleteReturnCodeMaster(string ReturnCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdReturnCodeMaster", sqlParameterNext.ToArray());
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

        public bool CheckReturnCodeMasterTempById(string ReturnCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgReturnCodeTempById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateReturnCodeMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;
            string fldCharges = "";

            if (col["fldCharges"] != null)
            {
                fldCharges = col["fldCharges"].Replace(",", "");
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", col["fldRejectCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectDesc", col["fldRejectDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldRepresentable", ""));
            sqlParameterNext.Add(new SqlParameter("@fldRejectType", col["fldRejectType"]));
            sqlParameterNext.Add(new SqlParameter("@fldCharges", fldCharges));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt")));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciReturnCodeMaster", sqlParameterNext.ToArray());
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

        public void MoveToReturnCodeMasterFromTemp(string ReturnCode, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciReturnCodeMasterFromTemp", sqlParameterNext.ToArray());
        }

        public bool DeleteReturnCodeMasterTemp(string ReturnCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdReturnCodeMasterTemp", sqlParameterNext.ToArray());
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

        public bool CheckValidateReturnCode(string returnCode)
        {
            string sql = "Select fldRejectCode , fldRejectDesc FROM tblRejectMaster where fldRejectCode = @fldRejectCode and fldRejectType not in ('12', '0')";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@fldRejectCode", returnCode) }))
            {
                return true;
            }
            return false;
        }

        public bool CheckValidateInternalReturnCode(string returnCode)
        {
            string sql = "Select fldRejectCode , fldRejectDesc FROM tblRejectMaster where fldRejectCode = @fldRejectCode and fldRejectType = '12'";
            if (dbContext.CheckExist(sql, new[] { new SqlParameter("@fldRejectCode", returnCode) }))
            {
                return true;
            }
            return false;
        }
        public void DeleteInRejectMasterTemp(string returnCode)
        {
            string str = "delete from tblRejectMasterTemp where fldRejectCode=@fldRejectCode";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldRejectCode", returnCode) });
        }
        // XX START 20210422
        public List<ReturnCodeModel> FindAllRejectCodesDictionary()
        {
            DataTable dataTable = new DataTable();
            dataTable = this.dbContext.GetRecordsAsDataTable("Select fldRejectCode , fldRejectDesc , fldRejectType FROM tblRejectMaster where fldRejectType <> '0' order by fldRejectCode ", null);
            List<ReturnCodeModel> RejectCode = new List<ReturnCodeModel>();
            foreach (DataRow row in dataTable.Rows)
            {
                ReturnCodeModel reject = new ReturnCodeModel();

                reject.fldRejectCode = row["fldRejectCode"].ToString();
                reject.fldRejectDesc = row["fldRejectDesc"].ToString();
                reject.fldRejectType = row["fldRejectType"].ToString();
                RejectCode.Add(reject);
            }
            return RejectCode;
        }
        // XX END 20210422

        
        public List<ReturnCodeModel> FindRejectCodesForBranchDictionary()
        {
            DataTable dataTable = new DataTable();
            dataTable = this.dbContext.GetRecordsAsDataTable("Select fldRejectCode , fldRejectDesc , fldRejectType FROM tblRejectMaster  where fldRejectType not in ('0','12') order by fldRejectCode", null);
            List<ReturnCodeModel> RejectCode = new List<ReturnCodeModel>();
            foreach (DataRow row in dataTable.Rows)
            {
                ReturnCodeModel reject = new ReturnCodeModel();

                reject.fldRejectCode = row["fldRejectCode"].ToString();
                reject.fldRejectDesc = row["fldRejectDesc"].ToString();
                reject.fldRejectType = row["fldRejectType"].ToString();
                RejectCode.Add(reject);
            }
            return RejectCode;
        }

        public List<ReturnCodeModel> ListRejectType()
        {
            DataTable resultTable = new DataTable();
            List<ReturnCodeModel> RejectType = new List<ReturnCodeModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgListRejectType", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    ReturnCodeModel type = new ReturnCodeModel();
                    type.fldRejectType = row["fldRejectType"].ToString();
                    type.fldRejectTypeDesc = row["fldRejectTypeDesc"].ToString();
                    if (type.fldRejectType.Trim() != "0")
                    {
                        RejectType.Add(type);
                    }
                }
            }
            return RejectType;
        }

        public List<ReturnCodeModel> ListRejectTypeForBranch()
        {
            DataTable resultTable = new DataTable();
            List<ReturnCodeModel> RejectType = new List<ReturnCodeModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgListRejectType", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    ReturnCodeModel type = new ReturnCodeModel();
                    type.fldRejectType = row["fldRejectType"].ToString();
                    type.fldRejectTypeDesc = row["fldRejectTypeDesc"].ToString();
                    if (type.fldRejectType.Trim() != "0" && type.fldRejectType.Trim() != "12")
                    {
                        RejectType.Add(type);
                    }  
                }
            }
            return RejectType;
        }

        public async Task<DataTable> GetAllRejectCodesAsync()
        {
            DataTable dataTable = await Task.Run<DataTable>(() => this.GetAllRejectCodes());
            return dataTable;
        }

        public async Task<DataTable> GetAllRejectCodesWithoutInternalAsync()
        {
            DataTable dataTable = await Task.Run<DataTable>(() => this.GetAllRejectCodesWithoutInternal());
            return dataTable;
        }

        public DataTable GetAllRejectCodes()
        {
            string str = "SELECT rm.fldRejectCode, rm.fldRejectDesc, rt.fldRejectType, rm.fldCharges FROM tblRejectMaster rm LEFT OUTER JOIN tblRejectTypeMaster rt ON rm.fldRejectType = rt.fldRejectType ORDER BY rm.fldRejectCode, rt.fldRejectSeq ";
            DataTable dataTable = new DataTable();
            ArrayList arrayLists = new ArrayList();
            return this.dbContext.GetRecordsAsDataTable(str, null);
        }

        public DataTable GetAllRejectCodesWithoutInternal()
        {
            string str = "SELECT rm.fldRejectCode, rm.fldRejectDesc, rt.fldRejectType, rm.fldCharges FROM tblRejectMaster rm LEFT OUTER JOIN tblRejectTypeMaster rt ON rm.fldRejectType = rt.fldRejectType where rm.fldRejectType <> '12' ORDER BY rm.fldRejectCode, rt.fldRejectSeq  ";
            DataTable dataTable = new DataTable();
            ArrayList arrayLists = new ArrayList();
            return this.dbContext.GetRecordsAsDataTable(str, null);
        }

        public DataTable Find(string RejectCode)
        {
            DataTable dataTable = new DataTable();
            string str = "Select fldRejectCode,fldRejectDesc FROM  tblRejectMaster WHERE fldRejectCode = @rejectcode";
            return this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@rejectcode", RejectCode) });
        }

        public DataTable FindWithoutInternalCode(string RejectCode)
        {
            DataTable dataTable = new DataTable();
            string str = "Select fldRejectCode,fldRejectDesc FROM  tblRejectMaster WHERE fldRejectCode = @rejectcode and fldRejectType <> '12'";
            return this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@rejectcode", RejectCode) });
        }

        public string getCharges(string rejectCode)
        {
            string charges = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@rejectCode", rejectCode));
            DataTable ds = dbContext.GetRecordsAsDataTableSP("spcgGetCharges", sqlParameterNext.ToArray());
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                charges = row["fldCharges"].ToString();
            }
            return charges;
        }
    }
}
