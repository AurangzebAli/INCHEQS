using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Common;
using INCHEQS.Resources;
using System.Web.Mvc;
using System.Text.RegularExpressions;


namespace INCHEQS.Areas.ICS.Models.TransCode {
    public class TransCodeDao : ITransCodeDao
    {

        private readonly ApplicationDbContext dbContext;
        public TransCodeDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<string>();
            //int result;

            if (CheckTransCodeExist(col["TransCode"]))
            {
                //err.Add(Locale.TransCodeExist);
                err.Add("Transaction Code already exists");
            }
            else
            {
                if (CheckTransCodeTempExist(col["TransCode"]))
                {
                    //err.Add(Locale.TransCodeExistTemp);
                    err.Add("Transaction Code already exists");
                }
                else {
                    if (col["TransCode"].Equals(""))
                    {
                        //err.Add(Locale.TransCodeValidate);
                        err.Add("Transaction Code cannot be empty");
                    }
                    else
                    {
                        Regex regex = new Regex("^[0-9]+$");

                        if (regex.IsMatch(col["TransCode"]))
                        {

                        }
                        else
                        {
                            err.Add(Locale.TransCodeFormat);
                        }
                    }

                    if (col["TransCodeDesc"].Equals(""))
                    {
                        //err.Add(Locale.TransCodeDescValidate);
                        err.Add("Transaction Code Description cannot be empty");
                    }
                } 
            }
            
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<string>();
            //int result;

            if (col["TransCodeDesc"].Equals(""))
            {
                //err.Add(Locale.TransCodeDescValidate);
                err.Add("Transaction Code Description cannot be empty");
            }

            return err;
        }

        public bool CheckTransCodeExist(string transCode)
        {

            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMaster", sqlParameterNext.ToArray()); //done
            if (resultTable.Rows.Count > 0)
            {

                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public bool CheckTransCodeTempExist(string transCode)
        {

            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMasterTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {

                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public void CreateTransCodeTemp(FormCollection col, string currentUser, string status, string transCode)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            if (status == "D")
            {
                sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
                sqlParameterNext.Add(new SqlParameter("@fldTransCodeDesc", ""));
            }
            else {
                sqlParameterNext.Add(new SqlParameter("@fldTransCode", col["TransCode"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldTransCodeDesc", col["TransCodeDesc"].ToString()));
            } 
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciTransCodeMasterTemp", sqlParameterNext.ToArray());
        }

        public void CreateTransCode(FormCollection col, string currentUser)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldTransCode", col["TransCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldTransCodeDesc", col["TransCodeDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciTransCodeMaster", sqlParameterNext.ToArray());
        }

        public void UpdateTransCode(FormCollection col, string currentUser)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldTransCode", col["TransCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldTransCodeDesc", col["TransCodeDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuTransCodeMaster", sqlParameterNext.ToArray()); //DOne
        }

        public void DeleteTransCode(string transCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdTransCodeMaster", sqlParameterNext.ToArray()); //Done
        }

        public void DeleteTransCodeTemp(string transCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdTransCodeMasterTemp", sqlParameterNext.ToArray()); //Done
        }

        public void MoveTransCodeFromTemp(string transCode, string status)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciTransCodeMasterFromTemp", sqlParameterNext.ToArray()); //Done
        }

        public List<TransCodeModel> GetTransCode(string transCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<TransCodeModel> fieldList = new List<TransCodeModel>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    TransCodeModel field = new TransCodeModel();
                    field.fldTransCode = row["fldTransCode"].ToString();
                    field.fldTransCodeDesc = row["fldTransCodeDesc"].ToString();
                    fieldList.Add(field);
                }
            }
            else
            {
                fieldList = null;
            }
            return fieldList;
        }

        public void InsertToDataProcessICS(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskId, string batchId, string crtuserId, string upduserId, string filename = "")
        {



            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldPosPayType", posPayType));//@fldSystemType
            sqlParameterNext.Add(new SqlParameter("@fldStatus", 1));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", clearDate));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", crtuserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldStartTime", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldEndTime", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", upduserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));



            //sqlParameterNext.Add(new SqlParameter("@fldProductCode", "ICS"));
            //sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));



            //sqlParameterNext.Add(new SqlParameter("@fldTaskId", taskId));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessICS", sqlParameterNext.ToArray());
        }

        public List<TransCodeModel> GetTransCodeTemp(string transCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<TransCodeModel> fieldList = new List<TransCodeModel>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMasterTemp", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    TransCodeModel field = new TransCodeModel();
                    field.fldTransCode = row["fldTransCode"].ToString();
                    field.fldTransCodeDesc = row["fldTransCodeDesc"].ToString();
                    field.fldApproveStatus = row["fldApproveStatus"].ToString();
                    fieldList.Add(field);
                }
            }
            else
            {
                fieldList = null;
            }
            return fieldList;
        }

    }
}