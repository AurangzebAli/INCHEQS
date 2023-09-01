using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Models.SearchPageConfig.Services;
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

namespace INCHEQS.Areas.OCS.Models.OCSAPIPosting
{
    public class OCSCreditPostingDao : IOCSCreditPostingDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;

        public OCSCreditPostingDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, ISearchPageService searchPageService)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
            this.searchPageService = searchPageService;
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
            dtProcessDate = dbContext.GetRecordsAsDataTableSP("spcgProcessDate", sqlParameterNext.ToArray());
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
            dtbPostingStatus = dbContext.GetRecordsAsDataTableSP("spcgPostingStatusLatestBatchID", SqlParameterNext.ToArray());
            if (dtbPostingStatus.Rows.Count < 1)
            {
                strPostingBatch = strPrefixToday + "00000";
            }
            else
            {
                if (dtbPostingStatus.Rows[0]["fldPostBatch"] == DBNull.Value)
                {
                    strPostingBatch = strPrefixToday + "00000";
                }
                else
                {
                    strPostingBatch = dtbPostingStatus.Rows[0]["fldPostBatch"].ToString();
                }
            }

            return strPostingBatch;
        }

        public void GenerateNewBatches(string bankcode, string intUserId, string processdate, string Totalitems, Int64 TotalAmount)
        {
            string strPostingMode = systemProfileDao.GetValueFromSystemProfile("PostingMode", CurrentUser.Account.BankCode).Trim();
            PostingStatusModel postingOBJ = new PostingStatusModel();
            postingOBJ.postBatch = Convert.ToInt64(GeneratePostingBatchID());
            postingOBJ.TotalItem = Convert.ToInt32(Totalitems);
            postingOBJ.TotalAmount = Convert.ToInt64(TotalAmount);
            if (strPostingMode == "FILE" || strPostingMode == "File" || strPostingMode == "file")
            {
                postingOBJ.currentProcess = "11";
            }
            else
            {
                postingOBJ.currentProcess = "31";
            }
            postingOBJ.processDateTime = DateTime.Now;
            postingOBJ.completeDateTime = DateTime.Now;
            postingOBJ.errorCode ="";
            postingOBJ.errorMsg = "";
            postingOBJ.FileGenerateFlag = "N";
            postingOBJ.regenerateFlag = "N";
            postingOBJ.uploadFlag = "N";
            postingOBJ.previousPostBatch = null;
            postingOBJ.createUserID = Convert.ToInt32(intUserId);
            postingOBJ.createTimeStamp = DateTime.Now;
            postingOBJ.updateUserID = Convert.ToInt32(intUserId);
            postingOBJ.updateTimeStamp = DateTime.Now;
            AddPostingStatus(postingOBJ, bankcode);
            if (strPostingMode == "FILE" || strPostingMode == "File" || strPostingMode == "file")
            {
                // Add Items in tblCPItemOCS in Case of posting mode = File
                AddFileBasePostingItems(postingOBJ, bankcode, processdate);
            }
            else
            {   // Add Items in tblCPItemOCS in Case of posting mode = API
                AddPostingItems(postingOBJ, bankcode, processdate);
            }
            
        }

        public void AddPostingStatus(PostingStatusModel postingOBJ, string bankcode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalItem", postingOBJ.TotalItem));
            sqlParameterNext.Add(new SqlParameter("@fldPostingTotalAmount", postingOBJ.TotalAmount));
            sqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", postingOBJ.currentProcess));
            sqlParameterNext.Add(new SqlParameter("@fldStartDatetime", postingOBJ.processDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldEndDatetime", postingOBJ.completeDateTime));
            sqlParameterNext.Add(new SqlParameter("@fldErrorCode", postingOBJ.errorCode));
            sqlParameterNext.Add(new SqlParameter("@fldErrorMsg", postingOBJ.errorMsg));
            sqlParameterNext.Add(new SqlParameter("@fldFileGenerateFlag", postingOBJ.FileGenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldRegenerateFlag", postingOBJ.regenerateFlag));
            sqlParameterNext.Add(new SqlParameter("@fldFileUploadFlag", postingOBJ.uploadFlag));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimestamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimestamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciCPStatusOCS", sqlParameterNext.ToArray());

        }

        public bool RegeneratePosting(FormCollection collection)
        {
            bool flag = false;
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            Int64 NewBatchNumber = Convert.ToInt64(filter["fldPostingBatch"].Trim());
            NewBatchNumber = NewBatchNumber + 1;
            int return_value = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciRegenerateCPItems", new[] {
                    new SqlParameter("@BankCode", filter["PresentingBankCode"].Trim()) ,
                    new SqlParameter("@fldPostingBatch", filter["fldPostingBatch"].Trim()),
                    new SqlParameter("@fldNewPostingBatch", NewBatchNumber),
                    new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId)
                });
            if (return_value == -1)
            {
                 flag = false;
            }
            else
            {
                flag = true;
            }
            return flag;
        }

        public bool checkBatchExist(Int64 datebatch)
        {
            string SQL = "select * from tblCPStatusOCS where fldPostingBatch=@fldPostingBatch";
            return dbContext.CheckExist(SQL, new[] {
                    new SqlParameter("@fldPostingBatch", datebatch) 
                });
        }
        public void AddPostingItems(PostingStatusModel postingOBJ,string bankcode, string processdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@BankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@PostBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", processdate));
            sqlParameterNext.Add(new SqlParameter("@createUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciCPItemOCS", sqlParameterNext.ToArray());

        }

        public void AddFileBasePostingItems(PostingStatusModel postingOBJ, string bankcode, string processdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@BankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@PostBatch", postingOBJ.postBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", processdate));
            sqlParameterNext.Add(new SqlParameter("@createUserId", postingOBJ.createUserID));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", postingOBJ.createTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", postingOBJ.updateUserID));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", postingOBJ.updateTimeStamp));
            dbContext.GetRecordsAsDataTableSP("spciCPFileBaseItemOCS", sqlParameterNext.ToArray());

        }
        public DataTable PostedItemHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", collection["row_fldPostingBatch"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", collection["row_PresentingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgPostedItems", sqlParameterNext.ToArray());
            //return dbContext.GetDataTableFromSqlWithParameter("Select * from View_ChequeHistory where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp",Utils.ConvertFormCollectionToDictionary(collection));
        }

        public DataTable PostedItemFileBaseHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldPostingBatch", collection["row_fldPostingBatch"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", collection["row_PresentingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgPostedItemsFileBase", sqlParameterNext.ToArray());
            //return dbContext.GetDataTableFromSqlWithParameter("Select * from View_ChequeHistory where fldInwardItemId = @fldInwardItemId order by fldCreateTimeStamp",Utils.ConvertFormCollectionToDictionary(collection));
        }
        public DataTable ReadyItemForPostingHistory(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string CaptureDate = DateTime.ParseExact(collection["row_fldcapturingdate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            sqlParameterNext.Add(new SqlParameter("@fldcapturingdate", CaptureDate));
            sqlParameterNext.Add(new SqlParameter("@PresentingBankCode", collection["row_PresentingBankCode"]));
            return dbContext.GetRecordsAsDataTableSP("spcgReadyItemForPostingDetail", sqlParameterNext.ToArray());
        }
    }
}