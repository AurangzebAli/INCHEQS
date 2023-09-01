using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Common;
using INCHEQS.Resources;
using System.Web.Mvc;
using System.Text.RegularExpressions;


namespace INCHEQS.Areas.ICS.Models.NonConformanceFlag {
    public class NonConformanceFlagDao : INonConformanceFlagDao
    {

        private readonly ApplicationDbContext dbContext;
        public NonConformanceFlagDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<string>();
            //int result;

            if (CheckNCFCodeExist(col["NCFCode"]))
            {
                err.Add(Locale.NCFValueExist);

            }
            else
            {
                if (CheckNCFCodeTempExist(col["NCFCode"]))
                {
                    err.Add(Locale.NCFValueExistTemp);
                }
                else {
                    if (col["NCFCode"].Equals(""))
                    {
                        err.Add(Locale.NCFValueValidate);
                    }
                    else
                    {
                        Regex regex = new Regex("^[0-9]+$");

                        if (regex.IsMatch(col["NCFCode"]))
                        {

                        }
                        else
                        {
                            err.Add(Locale.NCFValueFormat);
                        }
                    }

                    if (col["NCFDesc"].Equals(""))
                    {
                        err.Add(Locale.NCFDescValidate);
                    }
                } 
            }
            
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<string>();
            //int result;

            if (col["NCFDesc"].Equals(""))
            {
                err.Add(Locale.NCFDescValidate);
            }

            return err;
        }

        public bool CheckNCFCodeExist(string ncfCode)
        {

            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCode", sqlParameterNext.ToArray());
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

        public bool CheckNCFCodeTempExist(string ncfCode)
        {

            bool blnResult;

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCodeTemp", sqlParameterNext.ToArray());
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

        public void CreateNCFCodeTemp(FormCollection col, string currentUser, string status, string ncfCode)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            if (status == "D")
            {
                sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
                sqlParameterNext.Add(new SqlParameter("@fldNCFDesc", ""));
            }
            else {
                sqlParameterNext.Add(new SqlParameter("@fldNCFCode", col["NCFCode"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldNCFDesc", col["NCFDesc"].ToString()));
            } 
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciNCFMasterTemp", sqlParameterNext.ToArray());
        }

        public void CreateNCFCode(FormCollection col, string currentUser)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", col["NCFCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldNCFDesc", col["NCFDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciNCFMaster", sqlParameterNext.ToArray());
        }

        public void UpdateNCFCode(FormCollection col, string currentUser)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", col["NCFCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldNCFDesc", col["NCFDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetime().ToString()));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuNCFMaster", sqlParameterNext.ToArray());
        }

        public void DeleteNCFCode(string ncfCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdNCFMaster", sqlParameterNext.ToArray()); //Done
        }

        public void DeleteNCFCodeTemp(string ncfCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdNCFMasterTemp", sqlParameterNext.ToArray()); //Done
        }

        public void MoveNCFCodeFromTemp(string ncfCode, string status)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", status));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciNCFMasterFromTemp", sqlParameterNext.ToArray()); //Done
        }

        public List<NonConformanceFlagModel> GetNCFCode(string ncfCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<NonConformanceFlagModel> fieldList = new List<NonConformanceFlagModel>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    NonConformanceFlagModel field = new NonConformanceFlagModel();
                    field.fldNCFCode = row["fldNCFCode"].ToString();
                    field.fldNCFDesc = row["fldNCFDesc"].ToString();
                    fieldList.Add(field);
                }
            }
            else
            {
                fieldList = null;
            }
            return fieldList;
        }

        public List<NonConformanceFlagModel> GetNCFCodeTemp(string ncfCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<NonConformanceFlagModel> fieldList = new List<NonConformanceFlagModel>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCodeTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    NonConformanceFlagModel field = new NonConformanceFlagModel();
                    field.fldNCFCode = row["fldNCFCode"].ToString();
                    field.fldNCFDesc = row["fldNCFDesc"].ToString();
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