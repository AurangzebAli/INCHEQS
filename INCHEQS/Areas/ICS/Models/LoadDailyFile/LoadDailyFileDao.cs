using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;
using System.Globalization;

namespace INCHEQS.Areas.ICS.Models.LoadDailyFile
{
    public class LoadDailyFileDao : ILoadDailyFileDao
    {

        private readonly ApplicationDbContext dbContext;


        public LoadDailyFileDao(ApplicationDbContext dbContext)
        {

            this.dbContext = dbContext;
        }

        //View the Match / Unmatch Item Item
        //public List<LoadDailyFileModel> GetItemStatusListing(string bankCode, string cleardate, string Status)
        //{
        //    DateTime dtcleardate;
        //    dtcleardate = Convert.ToDateTime(cleardate);
        //    cleardate = dtcleardate.ToString("yyyy-MM-dd");
        //    List<LoadDailyFileModel> ItemList = new List<LoadDailyFileModel>();
        //    DataTable dttblInwardItem = new DataTable();

        //    try
        //    {
        //        //Step 1 : Select items under InwardReturnItem and inwardreturnFile to get the list of items.
        //        string stmtIR = "Select ini.fldUIC, ini.fldIRItemInitialId, inf.fldFileName, inf.fldProcessDate, ini.fldreturnCode, " +
        //                        "rs.fldRejectDesc, 'N' AS isRepresentment " +
        //                        "FROM tblInwardReturnItem ini " +
        //                        "LEFT JOIN tblInwardReturnFile inf ON ini.fldInwardReturnFileId = inf.fldInwardReturnFileId " +
        //                        "LEFT JOIN tblRejectmaster AS rs ON ini.fldReturnCode = rs.fldRejectCode " +
        //                        "WHERE inf.fldProcessDate::Timestamp without time zone = @cleardate::Timestamp without time zone and ini.fldMatchFlag = @fldMatchFlag and ini.fldPresentingBankCode = @BankCode ";
        //        dttblInwardItem = dbContext.GetRecordsAsDataTable(stmtIR, new[] {
        //        new SqlParameter("@cleardate", cleardate),
        //        new SqlParameter("@fldMatchFlag", Status.ToString()),
        //        new SqlParameter("@BankCode", bankCode.ToString()),
        //        });




        //        //Step 2.1 : Pass the value to model from datatabel

        //        foreach (DataRow row in dttblInwardItem.Rows)
        //        {
        //            LoadDailyFileModel inwardreturnmodel = new LoadDailyFileModel()
        //            {
        //                fldUIC = row["fldUIC"].ToString(),
        //                fldIRItemInitialID = row["fldIRItemInitialId"].ToString(),
        //                fldRejectDesc = row["fldRejectDesc"].ToString(),
        //                fldisRepresentment = row["isRepresentment"].ToString(),
        //                fldFileName = row["fldFileName"].ToString()
        //            };
        //            //Step 2.2 : Pass the value of InwardReturnModel to Itemlist to display in Viewbag.
        //            ItemList.Add(inwardreturnmodel);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return ItemList;
        //}


        //View the Match / Unmatch Item Item
        //public DataTable GetInwardReturnFileRecordWithStatus(string bankCode, string cleardate, string Status)
        //{
        //    List<LoadDailyFileModel> ItemList = new List<LoadDailyFileModel>();
        //    DataTable dtGetInwardReturnFileRecordWithStatus = new DataTable();

        //    try
        //    {
        //        {
        //            //DataTable dt = new DataTable();

        //            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //            SqlParameterNext.Add(new SqlParameter("p_bankcode", bankCode));
        //            SqlParameterNext.Add(new SqlParameter("p_status", Status));
        //            SqlParameterNext.Add(new SqlParameter("p_dtcleardate", cleardate));
        //            dtGetInwardReturnFileRecordWithStatus = dbContext.GetRecordsAsDataTableSP("spcgInwardReturnFileRecordWithStatus ", SqlParameterNext.ToArray());


        //            return dtGetInwardReturnFileRecordWithStatus;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return dtGetInwardReturnFileRecordWithStatus;
        //}
        //public DataTable GetInwardReturnItemForMatching(string bankCode, string cleardate, string Status)
        //{
        //    List<LoadDailyFileModel> ItemList = new List<LoadDailyFileModel>();
        //    DataTable dtGetInwardReturnItemForMatching = new DataTable();

        //    try
        //    {
        //        {
        //            //DataTable dt = new DataTable();

        //            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //            SqlParameterNext.Add(new SqlParameter("p_bankcode", bankCode));
        //            SqlParameterNext.Add(new SqlParameter("p_status", Status));
        //            SqlParameterNext.Add(new SqlParameter("p_dtcleardate", cleardate));
        //            dtGetInwardReturnItemForMatching = dbContext.GetRecordsAsDataTableSP("spcgInwardReturnItemForMatching ", SqlParameterNext.ToArray());


        //            return dtGetInwardReturnItemForMatching;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return dtGetInwardReturnItemForMatching;
        //}
        //public DataTable GetMatchItemID(string uic)
        //{
        //    List<LoadDailyFileModel> ItemList = new List<LoadDailyFileModel>();
        //    DataTable dtGetMatchItemID = new DataTable();

        //    try
        //    {
        //        {
        //            //DataTable dt = new DataTable();

        //            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //            SqlParameterNext.Add(new SqlParameter("p_struic", uic));
        //            dtGetMatchItemID = dbContext.GetRecordsAsDataTableSP("spcgMatchItemID ", SqlParameterNext.ToArray());


        //            return dtGetMatchItemID;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //    return dtGetMatchItemID;
        //}
        //public void CreateDuplicateIRItems(string strIteminitialid, string CreateUserId)
        //{
        //    try
        //    {

        //        List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //        SqlParameterNext.Add(new SqlParameter("@p_flditeminitialid", strIteminitialid));
        //        SqlParameterNext.Add(new SqlParameter("@p_flduserid", CreateUserId));
        //        dbContext.GetRecordsAsDataTableSP("spciIRTblRecords", SqlParameterNext.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //}
        //public void UpdateInwardReturnFileRecord(String intFileId,
        //                            String strFileName, String strUPIDate,
        //                            String strPresentingBankType, String strPresentingBankCode,
        //                            String intTotalRecord, String intTotalAmount, String strStatus,
        //                            String strActive, String intImportUserId, String strImportTimeStamp,
        //                            String intMatchUserId, String strMatchTimeStamp, String intCreateUserId,
        //                            String strCreateTimeStamp, String intUpdateUserId, String strUpdateTimeStamp,
        //                            String intDownloadCount, String strFolderName, String strType, String strProcessDate,
        //                            String intUpdateReturnCode)
        //{
        //    try
        //    {
        //        List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //        SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnfileid", intFileId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldfilename", strFileName));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldupidate", strUPIDate));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbanktype", strPresentingBankType));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbankcode", strPresentingBankCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldtotalrecord", intTotalRecord));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldtotalamount", intTotalAmount));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldstatus", strStatus));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldactive", strActive));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldimportuserid", intImportUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldimporttimestamp", strImportTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldmatchuserid", intMatchUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldmatchtimestamp", strMatchTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldcreateuserid", intCreateUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldcreatetimestamp", strCreateTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldupdateuserid", intUpdateUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldupdatetimestamp", strUpdateTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_flddownloadcount", intDownloadCount));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldfoldername", strFolderName));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldtype", strType));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldprocessdate", strProcessDate));
        //        dbContext.GetRecordsAsDataTableSP("spcuUpdateInwardReturnFile", SqlParameterNext.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //}


        //public void UpdateInwardReturnItem(string InwardReturnItemId, string InwardReturnFileId, string ClearingDate, string cleardate, string UIC, string PresentingBankType,
        //string PresentingBankCode, string PresentingStateCode, string PresentingBranchCode, string CheckDigit, string Serial, string PayingBankcCode,
        //string PayingStateCode, string PayingBranchCode, string AccNo, string TCCode, string Amount, string ReturnCode, string ReturnCount, string ChequeType, string ReturnReason,
        //string ItemInitialId, string MatchFlag, string IRDFlag, string IRDGenFlag, string IRDPringFlag, string CreateUserId,
        //string CreateTimeStamp, string UpdateUserId, string UpdateTimeStamp, string IssuingBankType, string IssuingBankCode, string IssuingBankStateCode,
        //string IssuingBankBranchCode, string IQA, string NCFlag, string ImageIndicator, string DocumentToFollow, string Reason, string DSVeriStatus, string ForUIC, string ForFileId)
        //{

        //    try
        //    {
        //        List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //        SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnitemid", InwardReturnItemId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnfileid", InwardReturnFileId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldclearingdate", ClearingDate));
        //        SqlParameterNext.Add(new SqlParameter("@p_flduic", UIC));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbanktype", PresentingBankType));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbankcode", PresentingBankCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingstatecode", PresentingStateCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbranchcode", PresentingBranchCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldcheckdigit", CheckDigit));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldserial", Serial));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpayingbankccode", PayingBankcCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpayingstatecode", PayingStateCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldpayingbranchcode", PayingBranchCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldaccno", AccNo));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldtccode", TCCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldamount", Amount));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldreturncode", ReturnCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldreturncount", ReturnCount));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldchequetype", ChequeType));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldreturnreason", ReturnReason));
        //        SqlParameterNext.Add(new SqlParameter("@p_flditeminitialid", ItemInitialId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldmatchflag", MatchFlag));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldirdflag", IRDFlag));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldirdgenflag", IRDGenFlag));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldirdpringflag", IRDPringFlag));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldcreateuserid", CreateUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldcreatetimestamp", CreateTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldupdateuserid", UpdateUserId));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldupdatetimestamp", UpdateTimeStamp));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldissuingbanktype", IssuingBankType));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankcode", IssuingBankCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankstatecode", IssuingBankStateCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankbranchcode", IssuingBankBranchCode));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldiqa", IQA));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldncflag", NCFlag));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldimageindicator", ImageIndicator));
        //        SqlParameterNext.Add(new SqlParameter("@p_flddocumenttofollow", DocumentToFollow));
        //        SqlParameterNext.Add(new SqlParameter("@p_fldreason", Reason));
        //        SqlParameterNext.Add(new SqlParameter("@p_flddsveristatus", DSVeriStatus));
        //        SqlParameterNext.Add(new SqlParameter("@p_strforuic", ForUIC));
        //        SqlParameterNext.Add(new SqlParameter("@p_intforfileid", ForFileId));
        //        //Excute the command
        //        dbContext.GetRecordsAsDataTableSP("spcuUpdateInwardReturnItem", SqlParameterNext.ToArray());
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();
        //    }
        //}

        //public void PerformMatching(string userid, string bankCode, string cleardate, string Status)
        //{
        //    DataTable IRRecordWithStatusDataTable;
        //    DataTable InwardReturnItemForMatchingDataTable;
        //    DataTable GetMatchItemIDDataTable;
        //    DateTime dtcleardate;
        //    dtcleardate = Convert.ToDateTime(cleardate);
        //    cleardate = dtcleardate.ToString("yyyy-MM-dd");
        //    string strMatchFlag = "N";
        //    Boolean isUnMatch = false;
        //    String strStatus = "3";
        //    string intItemInitialId;
        //    string strAccNo;
        //    string strIRFileID;
        //    string strIRItemStatus;
        //    string strIRItemUIC;
        //    string strIRItemID;

        //    try
        //    {

        //        IRRecordWithStatusDataTable = GetInwardReturnFileRecordWithStatus(bankCode, cleardate, "1");
        //        if (IRRecordWithStatusDataTable.Rows.Count > 0)
        //        {
        //            InwardReturnItemForMatchingDataTable = GetInwardReturnItemForMatching(bankCode, cleardate, "1");
        //            if (InwardReturnItemForMatchingDataTable.Rows.Count > 0)
        //            {
        //                foreach (DataRow drwItem in InwardReturnItemForMatchingDataTable.Rows)
        //                {
        //                    strIRFileID = drwItem["fldinwardreturnfileid"].ToString().Trim();
        //                    strIRItemID = drwItem["fldinwardreturnitemid"].ToString().Trim();
        //                    strIRItemStatus = drwItem["fldstatus"].ToString().Trim();
        //                    strIRItemUIC = drwItem["flduic"].ToString().Trim();
        //                    GetMatchItemIDDataTable = GetMatchItemID(strIRItemUIC);

        //                    if (GetMatchItemIDDataTable.Rows.Count > 0)
        //                    {
        //                        foreach (DataRow drwItem1 in GetMatchItemIDDataTable.Rows)
        //                        {
        //                            strMatchFlag = drwItem1["p_strmatch"].ToString().Trim();
        //                            intItemInitialId = drwItem1["p_intiteminitialid"].ToString().Trim();
        //                            strAccNo = drwItem1["p_straccno"].ToString().Trim();

        //                            if (strMatchFlag == "N")
        //                            {
        //                                isUnMatch = true;
        //                            }

        //                            UpdateInwardReturnItem(
        //                                strIRItemID, strIRFileID, cleardate, cleardate, strIRItemUIC,
        //                                "",
        //                                "", "", "", "", "",
        //                                "", "", "", "", "",
        //                                "", "", "", "", "",
        //                                intItemInitialId, strMatchFlag, "Y", "", "",
        //                                userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "",
        //                                "", "", "", "", "",
        //                                "", "", "", "", "", ""
        //                                );

        //                            //strStatus to keep the match status of a file

        //                            if (isUnMatch == true)
        //                            {
        //                                strStatus = "4";
        //                                if (!string.IsNullOrEmpty(intItemInitialId))
        //                                {
        //                                    if (strAccNo == "")
        //                                    {
        //                                        CreateDuplicateIRItems(intItemInitialId, userid.ToString());
        //                                    }

        //                                }
        //                            }
        //                            else
        //                            {
        //                                CreateDuplicateIRItems(intItemInitialId, userid.ToString());
        //                            }

        //                            UpdateInwardReturnFileRecord(strIRFileID, "", "", "", "", "", "", strStatus, "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", "", "", "");

        //                        }
        //                    }

        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.ToString();

        //    }

        //}

        //public void GenerateNewBatches(string bankcode, string intUserId, string processdate)
        //{
        //    LoadDailyFileModel micrOBJ = new LoadDailyFileModel();
        //    micrOBJ.MICRBatch = Convert.ToInt64(GenerateMICRBatchID());
        //    micrOBJ.currentProcess = "1";
        //    micrOBJ.processDateTime = DateTime.Now;
        //    micrOBJ.completeDateTime = DateTime.Now;
        //    micrOBJ.errorCode = "";
        //    micrOBJ.errorMsg = "";
        //    micrOBJ.createUserID = Convert.ToInt32(intUserId);
        //    micrOBJ.createTimeStamp = DateTime.Now;
        //    micrOBJ.updateUserID = Convert.ToInt32(intUserId);
        //    micrOBJ.updateTimeStamp = DateTime.Now;
        //    AddMICRStatus(micrOBJ, bankcode);

        //}

        //public string GenerateMICRBatchID()
        //{
        //    string strPostingBatchID = "";
        //    string strPrefix = "";
        //    string strTodayBatchCount = "";
        //    DateTime dtToday = GetProcessDate();
        //    strTodayBatchCount = GetTodayBatchCount().ToString();

        //    strPrefix = dtToday.ToString("yyyMMdd", System.Globalization.CultureInfo.InvariantCulture);


        //    strTodayBatchCount = "000" + strTodayBatchCount;
        //    strTodayBatchCount = strTodayBatchCount.Substring(strTodayBatchCount.Length - 4, 4);
        //    strPostingBatchID = strPrefix + strTodayBatchCount;
        //    return strPostingBatchID;
        //}

        public DateTime GetProcessDate()
        {
            string strProcessDate = "";
            DataTable dtProcessDate = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dtProcessDate = dbContext.GetRecordsAsDataTableSP("spcgProcessDateICS", sqlParameterNext.ToArray());
            strProcessDate = dtProcessDate.Rows[0]["fldProcessDate"].ToString();
            return Convert.ToDateTime(strProcessDate);
        }

        //public Int64 GetTodayBatchCount()
        //{
        //    Int64 intBatchCount = 0;
        //    string strPostBatch = GetLatestMICRBatchID();
        //    strPostBatch = strPostBatch.Substring(8, 4);
        //    intBatchCount = Convert.ToInt64(strPostBatch);
        //    intBatchCount = intBatchCount + 1;
        //    return intBatchCount;
        //}

        //public string GetLatestMICRBatchID()
        //{
        //    string strMICRBatch, strPrefixToday, strYear, strMonth, strDay;
        //    DataTable dtbMICRStatus;
        //    DateTime dtNow = GetProcessDate();

        //    strPrefixToday = dtNow.ToString("yyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

        //    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //    SqlParameterNext.Add(new SqlParameter("@fldMICRBatch", strPrefixToday + "%"));
        //    dtbMICRStatus = dbContext.GetRecordsAsDataTableSP("spcgMICRStatusLatestBatchID", SqlParameterNext.ToArray());
        //    if (dtbMICRStatus.Rows.Count < 1)
        //    {
        //        strMICRBatch = strPrefixToday + "00000";
        //    }
        //    else
        //    {
        //        if (dtbMICRStatus.Rows[0]["fldMICRBatch"] == DBNull.Value)
        //        {
        //            strMICRBatch = strPrefixToday + "00000";
        //        }
        //        else
        //        {
        //            strMICRBatch = dtbMICRStatus.Rows[0]["fldMICRBatch"].ToString();
        //        }
        //    }

        //    return strMICRBatch;
        //}

        //public void AddMICRStatus(LoadDailyFileModel irdOBJ, string bankcode)
        //{
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
        //    sqlParameterNext.Add(new SqlParameter("@fldMICRBatch", irdOBJ.MICRBatch));
        //    //sqlParameterNext.Add(new SqlParameter("@fldPostingTotalItem", irdOBJ.TotalItem));
        //    //sqlParameterNext.Add(new SqlParameter("@fldPostingTotalAmount", irdOBJ.TotalAmount));
        //    sqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", irdOBJ.currentProcess));
        //    sqlParameterNext.Add(new SqlParameter("@fldStartDatetime", irdOBJ.processDateTime));
        //    sqlParameterNext.Add(new SqlParameter("@fldEndDatetime", irdOBJ.completeDateTime));
        //    sqlParameterNext.Add(new SqlParameter("@fldErrorCode", irdOBJ.errorCode));
        //    sqlParameterNext.Add(new SqlParameter("@fldErrorMsg", irdOBJ.errorMsg));
        //    //sqlParameterNext.Add(new SqlParameter("@fldFileGenerateFlag", postingOBJ.FileGenerateFlag));
        //    //sqlParameterNext.Add(new SqlParameter("@fldRegenerateFlag", postingOBJ.regenerateFlag));
        //    // sqlParameterNext.Add(new SqlParameter("@fldFileUploadFlag", postingOBJ.uploadFlag));
        //    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", irdOBJ.createUserID));
        //    sqlParameterNext.Add(new SqlParameter("@fldCreateTimestamp", irdOBJ.createTimeStamp));
        //    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", irdOBJ.updateUserID));
        //    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimestamp", irdOBJ.updateTimeStamp));
        //    dbContext.GetRecordsAsDataTableSP("spciMICRStatus", sqlParameterNext.ToArray());

        //}


        //public void InsertFileList(string fileName, string fileSize, string fileTimeStamp)
        //{
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldFileName", fileName));
        //    sqlParameterNext.Add(new SqlParameter("@fldfilesize", fileSize));
        //    sqlParameterNext.Add(new SqlParameter("@fldCreatetimestamp", fileTimeStamp));
        //    dbContext.GetRecordsAsDataTableSP("spciInwardFileList", sqlParameterNext.ToArray());
        //}



        //public LoadDailyFileModel GetMICRStatus(string bankCode, string currentProcess, int ItemBatch)
        //{
        //    LoadDailyFileModel mICRImageModel = new LoadDailyFileModel();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        //    sqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", currentProcess));
        //    sqlParameterNext.Add(new SqlParameter("@BatchTop", ItemBatch));

        //    DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgMICRStatus", sqlParameterNext.ToArray());

        //    if (dt.Rows.Count > 0)
        //    {
        //        DataRow row = dt.Rows[0];
        //        mICRImageModel.fldMICRBatch = row["fldMICRBatch"].ToString();
        //        mICRImageModel.fldCurrentProcess = row["fldCurrentProcess"].ToString();

        //        return mICRImageModel;
        //    }
        //    return null;
        //}


        //public List<string> GetInwardStatus(string bankCode, string currentProcess, int ItemBatch)
        //{
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        //    sqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", currentProcess));
        //    sqlParameterNext.Add(new SqlParameter("@BatchTop", ItemBatch));

        //    DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgMICRStatus", sqlParameterNext.ToArray());

        //    List<string> InwardStatus = new List<string>();
        //    foreach (DataRow row in recordsAsDataTable.Rows)
        //    {
        //        InwardStatus.Add(row["fldMICRBatch"].ToString());
        //        InwardStatus.Add(row["fldCurrentProcess"].ToString());
        //    }
        //    return InwardStatus;
        //    //return null;
        //}


        //public void updateMICRStatusProcessTime(string MICRBatch)
        //{


        //    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //    SqlParameterNext.Add(new SqlParameter("@fldMICRBatch", MICRBatch));
        //    SqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", "2"));
        //    dbContext.GetRecordsAsDataTableSP("spcuMICRStatus", SqlParameterNext.ToArray());

        //}

        //public void updateMICRStatusCompleted(string MICRBatch)
        //{

        //    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //    SqlParameterNext.Add(new SqlParameter("@fldMICRBatch", MICRBatch));
        //    SqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", "4"));
        //    dbContext.GetRecordsAsDataTableSP("spcuMICRStatus", SqlParameterNext.ToArray());

        //}

        //public void updateMICRStatusFail(string MICRBatch)
        //{

        //    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
        //    SqlParameterNext.Add(new SqlParameter("@fldMICRBatch", MICRBatch));
        //    SqlParameterNext.Add(new SqlParameter("@fldCurrentProcess", "3"));
        //    dbContext.GetRecordsAsDataTableSP("spcuMICRStatus", SqlParameterNext.ToArray());

        //}


        //public LoadDailyFileModel ListAvailableMICRStatusItem(string BankCode)
        //{
        //    LoadDailyFileModel micrstatusModels = new LoadDailyFileModel();

        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

        //    DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgListAvailableIRDStatus", sqlParameterNext.ToArray());
        //    if (recordsAsDataTable.Rows.Count > 0)
        //    {
        //        for (int i = 0; i < recordsAsDataTable.Rows.Count; i++)
        //        {
        //            DataRow item = recordsAsDataTable.Rows[0];
        //            micrstatusModels.fldCurrentProcess = item["fldcurrentprocess"].ToString();
        //            micrstatusModels.fldFileName = item["fldFileName"].ToString();
        //            //groupModel.fldBranchGroup = item["fldBranchGroup"].ToString();
        //        }
        //    }


        //    return micrstatusModels;
        //}


        public DataTable LoadDailyFileItemList(string id, string type)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            
            sqlParameterNext.Add(new SqlParameter("@FileId", id));
            sqlParameterNext.Add(new SqlParameter("@FileType", type));

            return dbContext.GetRecordsAsDataTableSP("spcgLoadDailyFileItem", sqlParameterNext.ToArray());
        }

        public string ListFolderPathFrom(string foldername, string clearDate)
        {
            string Path = "";
            if (foldername == "DDYF")
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@DirectoryType", "DDYF"));
                sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));

                DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgLoadDailyFilePathFrom", sqlParameterNext.ToArray());
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    Path = recordsAsDataTable.Rows[0]["fldpath"].ToString();
                }
                else
                {
                    Path = "MICR GWC Path not found in DB";
                }
            }
            else
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@DirectoryType", "DAHF"));
                sqlParameterNext.Add(new SqlParameter("@clearDate", clearDate));

                DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgLoadDailyFilePathFrom", sqlParameterNext.ToArray());
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    Path = recordsAsDataTable.Rows[0]["fldpath"].ToString();
                }
                else
                {
                    Path = "MICR GWC Path not found in DB";
                }
            }
            
            return Path;
        }

        public string ListFolderPathTo(string foldername)
        {
            string Path = "";
            if (foldername == "DDYF")
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@DirectoryType", "DDYF"));

                DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgLoadDailyFilePathTo", sqlParameterNext.ToArray());
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    Path = recordsAsDataTable.Rows[0]["fldpath"].ToString();
                }
                else
                {
                    Path = "MICR Local Path not found in DB";
                }
            }
            else
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@DirectoryType", "DAHF"));

                DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgLoadDailyFilePathTo", sqlParameterNext.ToArray());
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    Path = recordsAsDataTable.Rows[0]["fldpath"].ToString();
                }
                else
                {
                    Path = "MICR Local Path not found in DB";
                }
            }
                
            return Path;
        }

        public string ListFolderPathToCompleted()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@DirectoryType", "LocalCompleted"));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgMICRPath", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldpath"].ToString();
            }
            else
            {
                Path = "MICR Local Completed Path not found in DB";
            }
            return Path;
        }


        public LoadDailyFileModel GetDataFromLoadDailyFileConfig(string taskId, string bankcode, string foldername)
        {
            LoadDailyFileModel LoadDailyModel = new LoadDailyFileModel();
            //string stmt = "SELECT * FROM tblMICRImportConfig WHERE fldTaskId=@fldTaskId and fldBankCode=@BankCode";
            //string stmt = "SELECT * FROM tblMICRImportConfig WHERE fldTaskId=@fldTaskId ";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@foldername", foldername));

            DataTable dt = dbContext.GetRecordsAsDataTableSP("spcgloaddailyfileconfig", sqlParameterNext.ToArray());

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[0];
                    LoadDailyModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                    LoadDailyModel.fldSystemProfileCode2 = row["fldSystemProfileCode2"].ToString();
                    LoadDailyModel.fldDateSubString = Convert.ToInt32(row["fldDateSubString"]);
                    LoadDailyModel.fldBankCodeSubString = Convert.ToInt32(row["fldBankCodeSubString"]);
                    LoadDailyModel.fldDateSubStringCompleted = Convert.ToInt32(row["fldDateSubStringCompleted"]);
                    LoadDailyModel.fldBankCodeSubStringCompleted = Convert.ToInt32(row["fldBankCodeSubStringCompleted"]);
                    LoadDailyModel.fldFileExt = row["fldFileExt"].ToString();
                    LoadDailyModel.fldProcessName = row["fldProcessName"].ToString();
                    LoadDailyModel.fldPosPayType = row["fldPosPayType"].ToString();
                }
            }
                
            return LoadDailyModel;
        }

        public DataTable GetErrorListFromICLException(string clearDate)
        {
            DataTable ds = new DataTable();
            string stmt = "select * from tblICLexception where fldCleardate=@fldCleardate";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldCleardate", DateUtils.formatDateToFileDate(clearDate)) });
            return ds;
        }

        public void Update(string bankcode)
        {

            string stmt = "Update tblExtractServer set fldExtracted = 1 where fldBankCode=@fldBankCode and fldActive = 'Y'";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldBankCode",bankcode)
            });

        }

        public void run(string bankcode, string userid, string processDate, string processName, string fileType)
        {
            //string processDt = DateTime.Parse(processDate).ToString("yyyy-dd-MM");
            //string processDt = DateTime.ParseExact(processDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@bankcode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@processName", processName));
            sqlParameterNext.Add(new SqlParameter("@userid", userid));
            sqlParameterNext.Add(new SqlParameter("@fldProdCode", "ICS"));
            sqlParameterNext.Add(new SqlParameter("@fileType", fileType));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateTime.ParseExact(processDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessLDFICS", sqlParameterNext.ToArray());
            
        }


    }
}