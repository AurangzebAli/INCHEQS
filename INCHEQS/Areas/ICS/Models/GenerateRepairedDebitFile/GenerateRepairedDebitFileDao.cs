using INCHEQS.Areas.ICS.Models.GenerateRepairedDebitFile;
using INCHEQS.Common;
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

namespace INCHEQS.Areas.ICS.Models.GenerateRepairedDebitFile
{
    public class GenerateRepairedDebitFileDao : IGenerateRepairedDebitFileDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public GenerateRepairedDebitFileDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }
        public DataTable GetHubBranches(string userId)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserHubBranches", SqlParameterNext.ToArray());
            return dt;
        }

        public string GeneratePostingBatchID(string bankcode)
        {
            string strPostingBatchID = "";
            string strPrefix = "";
            string strTodayBatchCount = "";
            DateTime dtToday = GetProcessDate();
            strTodayBatchCount = GetTodayBatchCount(bankcode).ToString();

            strPrefix = dtToday.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            strTodayBatchCount = "000" + strTodayBatchCount;
            strTodayBatchCount = strTodayBatchCount.Substring(strTodayBatchCount.Length - 4, 4);
            strPostingBatchID = strPrefix + strTodayBatchCount;
            return strPostingBatchID;
        }
        public DateTime GetProcessDate()
        {
            string strProcessDate = "";
            DataTable dtProcessDate = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dtProcessDate = dbContext.GetRecordsAsDataTableSP("spcgProcessDateICS", sqlParameterNext.ToArray());
            strProcessDate = dtProcessDate.Rows[0]["fldProcessDate"].ToString();
            return Convert.ToDateTime(strProcessDate);
        }
        public Int64 GetTodayBatchCount(string bankcode)
        {
            Int64 intBatchCount = 0;
            string strPostBatch = GetLatestPostingBatchID(bankcode);
            strPostBatch = strPostBatch.Substring(8, 4);
            intBatchCount = Convert.ToInt64(strPostBatch);
            intBatchCount = intBatchCount + 1;
            return intBatchCount;
        }
        public string GetLatestPostingBatchID(string bankcode)
        {
            string strPostingBatch, strPrefixToday, strYear, strMonth, strDay;
            DataTable dtbPostingStatus;
            DateTime dtNow;
            dtNow = GetProcessDate();

            strPrefixToday = dtNow.ToString("yyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldPostBatch", strPrefixToday + "%"));
            SqlParameterNext.Add(new SqlParameter("@fldPostBankCode", bankcode + "%"));
            dtbPostingStatus = dbContext.GetRecordsAsDataTableSP("spcgDPICSRepairedItemPostingStatusLatestBatchID", SqlParameterNext.ToArray());
            if (dtbPostingStatus.Rows.Count < 1)
            {
                strPostingBatch = strPrefixToday + "00000";
            }
            else
            {
                if (dtbPostingStatus.Rows[0]["fldPostingBatch"] == DBNull.Value)
                {
                    strPostingBatch = strPrefixToday + "00000";
                }
                else
                {
                    strPostingBatch = dtbPostingStatus.Rows[0]["fldPostingBatch"].ToString();
                }
            }

            return strPostingBatch;
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
        public void GenerateNewBatches(string bankcode, string processname, string fileext, string intUserId, string processdate, string Totalitems, string TotalAmount)
        {
            GenerateRepairedDebitFileModel postingOBJ = new GenerateRepairedDebitFileModel();
            postingOBJ.postBatch = Convert.ToInt64(GeneratePostingBatchID(bankcode));
            postingOBJ.TotalItem = Convert.ToInt32(Totalitems);
            postingOBJ.TotalAmount = Convert.ToInt64(TotalAmount.Trim().Replace(".", ""));
            postingOBJ.fileExt = fileext;
            postingOBJ.filePath = GetFileDestPath(processname,bankcode);
            postingOBJ.currentProcess = "1";
            postingOBJ.processDateTime = DateTime.Now;
            postingOBJ.completeDateTime = DateTime.Now;
            postingOBJ.remarks = "Pending File Generating";
            postingOBJ.errorMsg = "";
            postingOBJ.FileGenerateFlag = "N";
            postingOBJ.regenerateFlag = "N";
            postingOBJ.uploadFlag = "N";
            postingOBJ.previousPostBatch = 0;
            postingOBJ.createUserID = Convert.ToInt32(intUserId);
            postingOBJ.createTimeStamp = DateTime.Now;
            postingOBJ.updateUserID = Convert.ToInt32(intUserId);
            postingOBJ.updateTimeStamp = DateTime.Now;
            AddICSDPPostingStatus(postingOBJ, bankcode, processdate);
            AddICSDPPostingItems(postingOBJ, bankcode, processdate);
        }

        public void ReGenerateBatches(string bankcode, string previousbatch , string processname, string fileext, string intUserId, string processdate, string Totalitems, string TotalAmount)
        {
            GenerateRepairedDebitFileModel postingOBJ = new GenerateRepairedDebitFileModel();
            postingOBJ.postBatch = Convert.ToInt64(GeneratePostingBatchID(bankcode));
            postingOBJ.TotalItem = Convert.ToInt32(Totalitems);
            postingOBJ.TotalAmount = Convert.ToInt64(TotalAmount.Trim().Replace(".", ""));
            postingOBJ.fileExt = fileext;
            postingOBJ.filePath = GetFileDestPath(processname, bankcode);
            postingOBJ.currentProcess = "1";
            postingOBJ.processDateTime = DateTime.Now;
            postingOBJ.completeDateTime = DateTime.Now;
            postingOBJ.remarks = "Pending File Generating";
            postingOBJ.errorMsg = "";
            postingOBJ.FileGenerateFlag = "N";
            postingOBJ.regenerateFlag = "N";
            postingOBJ.uploadFlag = "N";
            postingOBJ.previousPostBatch = Convert.ToInt64(previousbatch);
            postingOBJ.createUserID = Convert.ToInt32(intUserId);
            postingOBJ.createTimeStamp = DateTime.Now;
            postingOBJ.updateUserID = Convert.ToInt32(intUserId);
            postingOBJ.updateTimeStamp = DateTime.Now;
            AddICSDPPostingStatus(postingOBJ, bankcode, processdate);
            AddICSDPPostingItems(postingOBJ, bankcode, processdate);
        }

        public void AddICSDPPostingStatus(GenerateRepairedDebitFileModel postingOBJ, string bankcode, string processdate)
        {
            string postFileDate = processdate.Replace("-","");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldClearingDate", processdate));
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalItem", postingOBJ.TotalItem));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalAmount", postingOBJ.TotalAmount));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", postingOBJ.currentProcess));
            sqlParameterNext.Add(new SqlParameter("@fldStartTime", postingOBJ.processDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldEndTime", postingOBJ.completeDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", postingOBJ.remarks));
            sqlParameterNext.Add(new SqlParameter("@fldErrMessage", postingOBJ.errorMsg));
            sqlParameterNext.Add(new SqlParameter("@fldFileGenerateFlag", postingOBJ.FileGenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldRegenerateFlag", postingOBJ.regenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldFileUploadFlag", postingOBJ.uploadFlag));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimestamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimestamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciRepairedDPStatusICS", sqlParameterNext.ToArray());

        }
        public void AddICSDPPostingItems(GenerateRepairedDebitFileModel postingOBJ, string bankcode, string processdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@BankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@PostBatch", postingOBJ.postBatch));
             sqlParameterNext.Add(new SqlParameter("@clearingDate", processdate));
            //sqlParameterNext.Add(new SqlParameter("@PreviousPostBatch", postingOBJ.previousPostBatch));
            sqlParameterNext.Add(new SqlParameter("@createUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciRepairedDPItemICS", sqlParameterNext.ToArray());

        }

        public string GetFileDestPath(string processname,string bankcode)
        {
            DataTable dt = new DataTable();
            string filePath = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType", processname));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", bankcode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());

            if(dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                filePath = row["fldInterfaceFileDestPath"].ToString();

            }
            return filePath;
        }

        public DataTable PostedItemHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", collection["row_fldPostingBatch"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", "0001"));
            return dbContext.GetRecordsAsDataTableSP("spcgRepairedDPICSPostedItems", sqlParameterNext.ToArray());
            //return dbContext.GetDataTableFromSqlWithParameter("Select * from View_ChequeHistory where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp",Utils.ConvertFormCollectionToDictionary(collection));
        }
       
        public DataTable ReadyItemForPostingHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string CaptureDate = DateTime.ParseExact(collection["row_fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            //string CaptureDate = collection["row_fldClearDate"].ToString();
            sqlParameterNext.Add(new SqlParameter("@row_fldclearingdate", CaptureDate));
            sqlParameterNext.Add(new SqlParameter("@IssuingBankCode", ""));
            return dbContext.GetRecordsAsDataTableSP("spcgRepairedDPICSReadyItemForPostingDetail", sqlParameterNext.ToArray());
        }

        public GenerateRepairedDebitFileModel GetRepairedDebitFilePath(string processname, string bankcode)
        {
            DataTable dt = new DataTable();
            string filePath = "";

            GenerateRepairedDebitFileModel postingStatusModel = new GenerateRepairedDebitFileModel();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType", processname));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", bankcode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                postingStatusModel.fldFileSource = row["fldInterfaceFileSourcePath"].ToString();
                postingStatusModel.fldFileDestination = row["fldInterfaceFileDestPath"].ToString();
                postingStatusModel.fldFileName = row["fldInterfaceFileName"].ToString();

            }
            return postingStatusModel;
        }

        public string GetFTPUserName()
        {
            DataTable dt = new DataTable();
            string ftpUsername = "";


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode", "FTPUserName"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                ftpUsername = row["fldSystemProfileValue"].ToString();

            }
            return ftpUsername;
        }

        public string GetFTPPassword()
        {
            DataTable dt = new DataTable();
            string ftpPassword = "";


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode", "FTPPassword"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                ftpPassword = row["fldSystemProfileValue"].ToString();

            }
            return ftpPassword;
        }

        public void UpdateDebitFileUploaded(FormCollection col, string uploadStatus)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@uploadStatus", uploadStatus));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", DateTime.ParseExact(col["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")));
            dbContext.GetRecordsAsDataTableSP("spcuRepairedDebitFileUpload", sqlParameterNext.ToArray());

        }

        public void UpdateDataProcess(string status, string processName, string remarks, string errMessage, string oriStatus, string bankCode)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStatus", status));
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", remarks));
            sqlParameterNext.Add(new SqlParameter("@fldErrMessage", errMessage));
            sqlParameterNext.Add(new SqlParameter("@fldStatusOri", oriStatus));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dbContext.GetRecordsAsDataTableSP("spcuDataProcessICS", sqlParameterNext.ToArray());

        }
    }
}