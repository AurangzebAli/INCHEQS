using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Security;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;

namespace INCHEQS.Areas.COMMON.Models.BankHostStatus
{
    public class BankHostStatusDao : IBankHostStatusDao
    {
        private readonly ApplicationDbContext dbContext;
        public BankHostStatusDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public List<BankHostStatusModel> ListHostStatusAction()
        {
            DataTable resultTable = new DataTable();
            List<BankHostStatusModel> statusActionList = new List<BankHostStatusModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHostStatusMaster", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    BankHostStatusModel action = new BankHostStatusModel();
                    action.hostActionCode = row["fldBankHostStatusActionCode"].ToString();
                    action.hostActionDesc = row["fldBankHostStatusActionDesc"].ToString();
                    statusActionList.Add(action);
                }
            }
            return statusActionList;
        }

        public List<BankHostStatusModel> ListHostRejectCode()
        {
            DataTable resultTable = new DataTable();
            List<BankHostStatusModel> rejectCodeList = new List<BankHostStatusModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHostRejectCode", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    BankHostStatusModel rejectCode = new BankHostStatusModel();
                    rejectCode.hostRejectCode = row["fldRejectCode"].ToString();
                    rejectCode.hostRejectDesc = row["fldRejectDesc"].ToString();
                    rejectCodeList.Add(rejectCode);
                }
            }
            return rejectCodeList;
        }

        public DataTable GetAllHostStatus()
        {
            DataTable resultTable = new DataTable();
            
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
           

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHostStatusList", sqlParameterNext.ToArray());

           
            return resultTable;
        }

        public string GetHostReturnReasonDesc(string fldBankHostStatusCode)
        {
            DataTable dt = new DataTable();
            string result = "";
            string stmt = "SELECT * FROM tblBankHostStatusMaster where fldBankHostStatusCode=@fldBankHostStatusCode";
            dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                result = row["fldStatusDesc"].ToString();
            }
            return result;
        }

        //public DataTable GetBankHostStatusMaster(string fldBankHostStatusCode)
        //{
        //    string stmt = "SELECT * FROM tblBankHostStatusMaster where fldBankHostStatusCode=@fldBankHostStatusCode";
        //    dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode) });
        //    return dt;
        //}


        public BankHostStatusModel GetBankHostStatusMaster(string fldBankHostStatusCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankHostStatusModel bankHostStatus = new BankHostStatusModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMaster", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                bankHostStatus.fldBankHostStatusCode = row["fldBankHostStatusCode"].ToString();
                bankHostStatus.fldBankHostStatusDesc = row["fldBankHostStatusDesc"].ToString();
                bankHostStatus.fldBankHostStatusAction = row["fldBankHostStatusAction"].ToString();
                bankHostStatus.fldrejectcode = row["fldrejectcode"].ToString();
                bankHostStatus.fldBankCode = row["fldBankCode"].ToString();
                // bankHostStatus.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
            }

            return bankHostStatus;
        }


        public BankHostStatusModel GetBankHostStatusMasterTemp(string fldBankHostStatusCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankHostStatusModel bankHostStatus = new BankHostStatusModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                bankHostStatus.fldBankHostStatusCode = row["fldBankHostStatusCode"].ToString();
                bankHostStatus.fldBankHostStatusDesc = row["fldBankHostStatusDesc"].ToString();
                bankHostStatus.fldBankHostStatusAction = row["fldBankHostStatusAction"].ToString();
                bankHostStatus.fldrejectcode = row["fldrejectcode"].ToString();
                bankHostStatus.fldBankCode = row["fldBankCode"].ToString();
                // bankHostStatus.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
            }

            return bankHostStatus;
        }

        public bool UpdateHostStatusMaster(FormCollection col, AccountModel currentUser)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", col["fldBankHostStatusCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusDesc", col["fldBankHostStatusDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusAction", col["fldBankHostStatusAction"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", col["fldRejectCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBankHostStatusMaster", sqlParameterNext.ToArray());
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

        public bool AddBankHostCodeinTemptoUpdate(FormCollection col, AccountModel currentUser)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", col["fldBankHostStatusCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusDesc", col["fldBankHostStatusDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusAction", col["fldBankHostStatusAction"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", col["fldRejectCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankHostStatusMasterUpdateTemp", sqlParameterNext.ToArray());
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

        public bool CreateBankHostStatusMasterTemp(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", col["fldBankHostStatusCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusDesc", col["fldBankHostStatusDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusAction", col["fldBankHostStatusActionCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldrejectcode", col["fldrejectcode"]));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));


            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankHostStatusMasterTemp", sqlParameterNext.ToArray());

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

        public bool DeleteInBankHostStatusMaster(string fldBankHostStatusCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankHostStatusMaster", sqlParameterNext.ToArray());
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

        public void DeleteInBankHostStatusMasterTemp(string statusId)
        {

            string stmt = "delete from tblBankHostStatusMasterTemp  where fldStatusId=@fldStatusId";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });

        }

        public List<string> ValidateUpdate(FormCollection col)
        {
            List<string> err = new List<string>();

            if (col["fldBankHostStatusDesc"] == "")
            {
                err.Add(Locale.StatusDescCannotEmpty);
            }

            return err;
        }

        public List<string> ValidateCreate(FormCollection col)
        {
            List<string> err = new List<string>();

            if (col["fldBankHostStatusCode"] == "")
            {
                err.Add(Locale.StatusIdCannotEmpty);
            }
            if (col["fldBankHostStatusDesc"] == "")
            {
                err.Add(Locale.StatusDescCannotEmpty);
            }
            if (CheckRecordExist(col["fldBankHostStatusCode"]))
            {
                err.Add(Locale.ReturnCodeAlreadyExist);
            }

            //if (CheckRecordExistInTemp(col["fldBankHostStatusCode"]))
            //{
            //    err.Add(Locale.ReturnCodeAlreadyCreatedToBeVerify);
            //}
            return err;
        }

        public bool CheckRecordExist(string fldBankHostStatusCode)
        {
            string stmt = "select * from tblBankHostStatusMaster where fldRejectCode=@fldBankHostStatusCode";

            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode) });
        }

        public bool CheckRecordExistInTemp(string fldBankHostStatusCode)
        {
            string stmt = "select * from tblBankHostStatusMasterTemp where fldRejectCode=@fldBankHostStatusCode";

                return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode) });
        }

        public bool CreateInBankHostStatusMaster(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", col["fldBankHostStatusCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusDesc", col["fldBankHostStatusDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusAction", col["fldBankHostStatusActionCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldrejectcode", col["fldrejectcode"]));

            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankHostStatusMaster", sqlParameterNext.ToArray());

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

        public bool AddtoBankHostStatusMasterTempToDelete(string fldBankHostStatusCode)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
           
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode",fldBankHostStatusCode));
            
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankHostCodeinTemptoDelete", sqlParameterNext.ToArray());

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

        public BankHostStatusModel GetHostReturnReasonModel(string fldBankHostStatusCode)
        {
            BankHostStatusModel bankHostStatusCode = new BankHostStatusModel();
            DataTable ds = new DataTable();
            string stmt = "select * from tblBankHostStatusMaster where fldBankHostStatusCode=@fldBankHostStatusCode";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode) });

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                bankHostStatusCode.statusCode = row["fldBankHostStatusCode"].ToString();
                bankHostStatusCode.statusDesc = row["fldBankHostStatusDesc"].ToString();
            }
            return bankHostStatusCode;
        }

        public bool CheckBankHostCodeDataTemp(string fldBankHostStatusCode)
        {
            BankHostStatusModel bankCode = new BankHostStatusModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankHostCodeTempData", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }

            return Flag;

        }

        public bool CreateBankHostStatusCodeinMain(string fldBankHostStatusCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciBankHostStatusCodeinMain", sqlParameterNext.ToArray());
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


        public BankHostStatusModel GetBankHostStatusCodeData(string fldBankHostStatusCode)
        {
            BankHostStatusModel bankHostCode = new BankHostStatusModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", @fldBankHostStatusCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankHostCodeCodeDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                bankHostCode.fldBankHostStatusCode = row["fldBankHostStatusCode"].ToString();
                bankHostCode.fldBankHostStatusDesc = row["fldBankHostStatusDesc"].ToString();
                bankHostCode.fldBankHostStatusAction = row["fldBankHostStatusAction"].ToString();
                bankHostCode.fldrejectcode = row["fldrejectcode"].ToString();

                //bankHostCode.fldCreateUserId = row["fldCreateUserId"].ToString();
                //bankHostCode.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                //bankHostCode.fldUpdateUserId = row["fldUpdateUserId"].ToString();
                //bankHostCode.fldUpdateTimeStamp = row["fldUpdateTimeStamp"].ToString();
                //bankHostCode.fldIdForDelete = row["fldIdForDelete"].ToString();
                bankHostCode.ReportTitle = row["ReportTitle"].ToString();

            }

            return bankHostCode;

        }

        public bool DeleteInBankHostStatusCode(string fldBankHostStatusCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankHostStatusCode", sqlParameterNext.ToArray());
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


        public bool UpdateBankHostStatusCodeToMainById(string fldBankHostStatusCode)
        {
            BankHostStatusModel bankHostStatusCode = new BankHostStatusModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuBankHostStatusCodeToMainById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool DeleteBankHostStatusCodeinTemp(string fldBankHostStatusCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", fldBankHostStatusCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdBankHostStatusCodeinTemp", sqlParameterNext.ToArray());
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