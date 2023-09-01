using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.GenerateUPI
{
    public class GenerateUPIDao : IGenerateUPIDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public GenerateUPIDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        public GenerateUPIModel GetDataFromUPIConfig(string taskId)
        {
            GenerateUPIModel generateUPIModel = new GenerateUPIModel();
            string stmt = "SELECT * FROM tblHostFileConfig WHERE fldTaskId=@fldTaskId";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                generateUPIModel.fldProcessName = row["fldProcessName"].ToString();
                generateUPIModel.fldPosPayType = row["fldPosPayType"].ToString();
                generateUPIModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                generateUPIModel.fldFileExt = row["fldFileExt"].ToString();
                generateUPIModel.fldTaskRole = row["fldTaskRole"].ToString();

                return generateUPIModel;
            }

            return null;
        }

        public bool GetLateMaintenaceUPI()
        {
            bool Data;
            DataTable ds = new DataTable();
            /*DateTime cleardate1 = Convert.ToDateTime(clearDate);
            clearDate = cleardate1.ToString("yyyy-MM-dd");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));
            sqlParameterNext.Add(new SqlParameter("@issuingBankCode", issuingBankCode.Trim()));
            sqlParameterNext.Add(new SqlParameter("@returnType", returnType.Trim()));*/
            //ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionCount", SqlParameterNext.ToArray());
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankTypeId", ""));

            ds = dbContext.GetRecordsAsDataTableSP("spcgLateMaintenanceUPI", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                Data = true;
                //complete = row["Count"].ToString();
            }
            else
            {
                Data = false;
            }

            return Data;

           
        }

        public string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public void updateICSItemReadyForUPI(string clearDate, string issuingBankCode , string returnType)
        {

            DateTime cleardate1 = Convert.ToDateTime(clearDate);
            clearDate = cleardate1.ToString("yyyy-MM-dd");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));
            sqlParameterNext.Add(new SqlParameter("@issuingBankCode", issuingBankCode.Trim()));
            sqlParameterNext.Add(new SqlParameter("@returnType", returnType.Trim()));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuICSItemReadyForUPI", sqlParameterNext.ToArray());
        }

        public void updateChequeType21ForUPI(string clearDate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateTime.ParseExact(clearDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)));
            
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuUPIChequeType21", sqlParameterNext.ToArray());
        }


        public DataTable ReadyItemForPostingHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string CaptureDate = DateTime.ParseExact(collection["row_fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            sqlParameterNext.Add(new SqlParameter("@row_fldclearingdate", CaptureDate));
            sqlParameterNext.Add(new SqlParameter("@IssuingBankCode", collection["row_fldIssuingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgUPIReadyItemForPostingDetail", sqlParameterNext.ToArray());
        }

        public DataTable ReadyItemForLateReturnPostingHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string CaptureDate = DateTime.ParseExact(collection["row_fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            sqlParameterNext.Add(new SqlParameter("@row_fldclearingdate", CaptureDate));
            sqlParameterNext.Add(new SqlParameter("@IssuingBankCode", collection["row_fldIssuingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgUPIReadyLateReturnItemForPostingDetail", sqlParameterNext.ToArray());
        }

        public DataTable PostedItemHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldFileBatch", collection["row_fldFileBatch"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", collection["row_fldIssuingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgUPIPostedItems", sqlParameterNext.ToArray());
        }

    }
}