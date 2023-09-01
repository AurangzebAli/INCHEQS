using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting
{
    public class ICSDebitPostingDao : IICSDebitPostingDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public ICSDebitPostingDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
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

        public string GeneratePostingBatchID()
        {
            string strPostingBatchID = "";
            string strPrefix = "";
            string strTodayBatchCount = "";
            DateTime dtToday = GetProcessDate();
            strTodayBatchCount = GetTodayBatchCount().ToString();
            strPrefix = dtToday.Year.ToString();
            if (dtToday.Month < 10)
            {
                strPrefix = strPrefix + "0" + dtToday.Month;
            }
            else
            {
                strPrefix = strPrefix + dtToday.Month;
            }
            if (dtToday.Day < 10)
            {
                strPrefix = strPrefix + "0" + dtToday.Day;
            }
            else
            {
                strPrefix = strPrefix + dtToday.Day;
            }
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
        public Int64 GetTodayBatchCount()
        {
            Int64 intBatchCount = 0;
            string strPostBatch = GetLatestPostingBatchID();
            strPostBatch = strPostBatch.Substring(8, 4);
            intBatchCount = Convert.ToInt64(strPostBatch);
            intBatchCount = intBatchCount + 1;
            return intBatchCount;
        }
        public string GetLatestPostingBatchID()
        {
            string strPostingBatch, strPrefixToday, strYear, strMonth, strDay;
            DataTable dtbPostingStatus;
            DateTime dtNow;
            dtNow = GetProcessDate();
            strYear = dtNow.Year.ToString();
            if (dtNow.Month < 10)
            {
                strMonth = "0" + dtNow.Month.ToString();
            }
            else
            {
                strMonth = dtNow.Month.ToString();
            }
            if (dtNow.Day < 10)
            {
                strDay = "0" + dtNow.Day.ToString();
            }
            else
            {
                strDay = dtNow.Day.ToString();
            }

            strPrefixToday = strYear + strMonth + strDay;

            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldPostBatch", strPrefixToday + "%"));
            dtbPostingStatus = dbContext.GetRecordsAsDataTableSP("spcgDPICSPostingStatusLatestBatchID", SqlParameterNext.ToArray());
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

        public void GenerateNewBatches(string bankcode, string intUserId, string processdate, string Totalitems, string TotalAmount)
        {
            PostingStatusModel postingOBJ = new PostingStatusModel();
            postingOBJ.postBatch = Convert.ToInt64(GeneratePostingBatchID());
            postingOBJ.clearDate = processdate;
            postingOBJ.TotalItem = Convert.ToInt32(Totalitems);
            postingOBJ.TotalAmount = Convert.ToInt64(TotalAmount.Trim().Replace(".", ""));
            postingOBJ.currentProcess = "1";
            postingOBJ.processDateTime = DateTime.Now;
            postingOBJ.completeDateTime = DateTime.Now;
            postingOBJ.errorCode = "";
            postingOBJ.errorMsg = "";
            postingOBJ.FileGenerateFlag = "N";
            postingOBJ.regenerateFlag = "N";
            postingOBJ.uploadFlag = "N";
            postingOBJ.previousPostBatch = null;
            postingOBJ.createUserID = Convert.ToInt32(intUserId);
            postingOBJ.createTimeStamp = DateTime.Now;
            postingOBJ.updateUserID = Convert.ToInt32(intUserId);
            postingOBJ.updateTimeStamp = DateTime.Now;
            AddICSDPPostingStatus(postingOBJ, bankcode);
            AddICSDPPostingItems(postingOBJ, bankcode, processdate);
        }

        public void AddICSDPPostingStatus(PostingStatusModel postingOBJ, string bankcode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldClearingDate", postingOBJ.clearDate));
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalItem", postingOBJ.TotalItem));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalAmount", postingOBJ.TotalAmount));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", postingOBJ.currentProcess));
            sqlParameterNext.Add(new SqlParameter("@fldStartTime", postingOBJ.processDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldEndTime", postingOBJ.completeDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", postingOBJ.errorCode));
            sqlParameterNext.Add(new SqlParameter("@fldErrMessage", postingOBJ.errorMsg));
            sqlParameterNext.Add(new SqlParameter("@fldFileGenerateFlag", postingOBJ.FileGenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldRegenerateFlag", postingOBJ.regenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldFileUploadFlag", postingOBJ.uploadFlag));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimestamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimestamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciDPStatusICS", sqlParameterNext.ToArray());

        }
        public void AddICSDPPostingItems(PostingStatusModel postingOBJ, string bankcode, string processdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@BankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@PostBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", processdate));
            //sqlParameterNext.Add(new SqlParameter("@clearingDate", DateTime.ParseExact(processdate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@createUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciDPItemICS", sqlParameterNext.ToArray());

        }
        public DataTable PostedItemHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", collection["row_fldPostingBatch"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", collection["row_IssuingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgDPICSPostedItems", sqlParameterNext.ToArray());
            //return dbContext.GetDataTableFromSqlWithParameter("Select * from View_ChequeHistory where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp",Utils.ConvertFormCollectionToDictionary(collection));
        }
       
        public DataTable ReadyItemForPostingHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //DateTime CaptureDate = DateTime.ParseExact(collection["row_fldClearDate"], "MM-dd-yyyy", CultureInfo.InvariantCulture);
            DateTime CaptureDate = DateTime.ParseExact(collection["row_fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture);
            //string CaptureDate = collection["row_fldClearDate"].ToString();
            sqlParameterNext.Add(new SqlParameter("@row_fldclearingdate", DateTime.ParseExact(collection["row_fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)));
            sqlParameterNext.Add(new SqlParameter("@IssuingBankCode", (collection["row_IssuingBankCode"].Substring(0,2)).ToString()));
            return dbContext.GetRecordsAsDataTableSP("spcgDPICSReadyItemForPostingDetail", sqlParameterNext.ToArray());
        }

        public PostingStatusModel GetDebitFilePath(string processname, string bankcode)
        {
            DataTable dt = new DataTable();
            string filePath = "";

            PostingStatusModel postingStatusModel = new PostingStatusModel();

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

        public void UpdateDebitFileUploaded(FormCollection col,string uploadStatus)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@uploadStatus", uploadStatus));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", DateTime.ParseExact(col["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")));
            dbContext.GetRecordsAsDataTableSP("spcuDebitFileUpload", sqlParameterNext.ToArray());
            
        }

        public void UpdateDataProcess(string status, string processName, string remarks,string errMessage, string oriStatus, string bankCode)
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