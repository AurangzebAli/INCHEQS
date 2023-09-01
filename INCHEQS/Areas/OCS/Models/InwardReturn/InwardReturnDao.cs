using INCHEQS.Common;
using INCHEQS.DataAccessLayer.OCS;
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



namespace INCHEQS.Areas.OCS.Models.InwardReturn
{
    public class InwardReturnDao : IInwardReturnDao
    {
        private readonly OCSDbContext ocsdbContext;
        private readonly ApplicationDbContext dbContext;


        public InwardReturnDao(OCSDbContext ocsdbContext, ApplicationDbContext dbContext)
        {
            this.ocsdbContext = ocsdbContext;
            this.dbContext = dbContext;
        }

        //View the Match / Unmatch Item Item
        public List<InwardReturnModel> GetItemStatusListing(string bankCode, string cleardate, string Status)
        {
            DateTime dtcleardate;
            dtcleardate = Convert.ToDateTime(cleardate);
            cleardate = dtcleardate.ToString("yyyy-MM-dd");
            List<InwardReturnModel> ItemList = new List<InwardReturnModel>();
            DataTable dttblInwardItem = new DataTable();

            try
            {
                //Step 1 : Select items under InwardReturnItem and inwardreturnFile to get the list of items.
                string stmtIR = "Select ini.fldUIC, ini.fldIRItemInitialId, inf.fldFileName, inf.fldProcessDate, ini.fldreturnCode, " +
                                "rs.fldRejectDesc, 'N' AS isRepresentment " +
                                "FROM tblInwardReturnItem ini " +
                                "LEFT JOIN tblInwardReturnFile inf ON ini.fldInwardReturnFileId = inf.fldInwardReturnFileId " +
                                "LEFT JOIN tblRejectmaster AS rs ON ini.fldReturnCode = rs.fldRejectCode " +
                                "WHERE inf.fldProcessDate::Timestamp without time zone = @cleardate::Timestamp without time zone and ini.fldMatchFlag = @fldMatchFlag and ini.fldPresentingBankCode = @BankCode ";
                dttblInwardItem = dbContext.GetRecordsAsDataTable(stmtIR, new[] {
                new SqlParameter("@cleardate", cleardate),
                new SqlParameter("@fldMatchFlag", Status.ToString()),
                new SqlParameter("@BankCode", bankCode.ToString()),
                });




                //Step 2.1 : Pass the value to model from datatabel

                foreach (DataRow row in dttblInwardItem.Rows)
                {
                    InwardReturnModel inwardreturnmodel = new InwardReturnModel()
                    {
                        fldUIC = row["fldUIC"].ToString(),
                        fldIRItemInitialID = row["fldIRItemInitialId"].ToString(),
                        fldRejectDesc = row["fldRejectDesc"].ToString(),
                        fldisRepresentment = row["isRepresentment"].ToString(),
                        fldFileName = row["fldFileName"].ToString()
                    };
                    //Step 2.2 : Pass the value of InwardReturnModel to Itemlist to display in Viewbag.
                    ItemList.Add(inwardreturnmodel);
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return ItemList;
        }


        //View the Match / Unmatch Item Item
        public DataTable GetInwardReturnFileRecordWithStatus(string bankCode, string cleardate, string Status)
        {
            List<InwardReturnModel> ItemList = new List<InwardReturnModel>();
            DataTable dtGetInwardReturnFileRecordWithStatus = new DataTable();

            try
            {
                {
                    //DataTable dt = new DataTable();

                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("p_bankcode", bankCode));
                    SqlParameterNext.Add(new SqlParameter("p_status", Status));
                    SqlParameterNext.Add(new SqlParameter("p_dtcleardate", cleardate));
                    dtGetInwardReturnFileRecordWithStatus = dbContext.GetRecordsAsDataTableSP("spcgInwardReturnFileRecordWithStatus ", SqlParameterNext.ToArray());


                    return dtGetInwardReturnFileRecordWithStatus;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return dtGetInwardReturnFileRecordWithStatus;
        }
        public DataTable GetInwardReturnItemForMatching(string bankCode, string cleardate, string Status)
        {
            List<InwardReturnModel> ItemList = new List<InwardReturnModel>();
            DataTable dtGetInwardReturnItemForMatching = new DataTable();

            try
            {
                {
                    //DataTable dt = new DataTable();

                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("p_bankcode", bankCode));
                    SqlParameterNext.Add(new SqlParameter("p_status", Status));
                    SqlParameterNext.Add(new SqlParameter("p_dtcleardate", cleardate));
                    dtGetInwardReturnItemForMatching = dbContext.GetRecordsAsDataTableSP("spcgInwardReturnItemForMatching ", SqlParameterNext.ToArray());


                    return dtGetInwardReturnItemForMatching;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return dtGetInwardReturnItemForMatching;
        }
        public DataTable GetMatchItemID(string uic)
        {
            List<InwardReturnModel> ItemList = new List<InwardReturnModel>();
            DataTable dtGetMatchItemID = new DataTable();

            try
            {
                {
                    //DataTable dt = new DataTable();

                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("p_struic", uic));
                    dtGetMatchItemID = dbContext.GetRecordsAsDataTableSP("spcgMatchItemID ", SqlParameterNext.ToArray());


                    return dtGetMatchItemID;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return dtGetMatchItemID;
        }
        public void CreateDuplicateIRItems(string strIteminitialid, string CreateUserId)
        {
            try
            {

                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@p_flditeminitialid", strIteminitialid));
                SqlParameterNext.Add(new SqlParameter("@p_flduserid", CreateUserId));
                dbContext.GetRecordsAsDataTableSP("spciIRTblRecords", SqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }
        public void UpdateInwardReturnFileRecord(String intFileId,
                                    String strFileName, String strUPIDate,
                                    String strPresentingBankType, String strPresentingBankCode,
                                    String intTotalRecord, String intTotalAmount, String strStatus,
                                    String strActive, String intImportUserId, String strImportTimeStamp,
                                    String intMatchUserId, String strMatchTimeStamp, String intCreateUserId,
                                    String strCreateTimeStamp, String intUpdateUserId, String strUpdateTimeStamp,
                                    String intDownloadCount, String strFolderName, String strType, String strProcessDate,
                                    String intUpdateReturnCode)
        {
            try
            {
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnfileid", intFileId));
                SqlParameterNext.Add(new SqlParameter("@p_fldfilename", strFileName));
                SqlParameterNext.Add(new SqlParameter("@p_fldupidate", strUPIDate));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbanktype", strPresentingBankType));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbankcode", strPresentingBankCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldtotalrecord", intTotalRecord));
                SqlParameterNext.Add(new SqlParameter("@p_fldtotalamount", intTotalAmount));
                SqlParameterNext.Add(new SqlParameter("@p_fldstatus", strStatus));
                SqlParameterNext.Add(new SqlParameter("@p_fldactive", strActive));
                SqlParameterNext.Add(new SqlParameter("@p_fldimportuserid", intImportUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldimporttimestamp", strImportTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_fldmatchuserid", intMatchUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldmatchtimestamp", strMatchTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_fldcreateuserid", intCreateUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldcreatetimestamp", strCreateTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_fldupdateuserid", intUpdateUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldupdatetimestamp", strUpdateTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_flddownloadcount", intDownloadCount));
                SqlParameterNext.Add(new SqlParameter("@p_fldfoldername", strFolderName));
                SqlParameterNext.Add(new SqlParameter("@p_fldtype", strType));
                SqlParameterNext.Add(new SqlParameter("@p_fldprocessdate", strProcessDate));
                dbContext.GetRecordsAsDataTableSP("spcuUpdateInwardReturnFile", SqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }


        public void UpdateInwardReturnItem(string InwardReturnItemId, string InwardReturnFileId, string ClearingDate, string cleardate, string UIC, string PresentingBankType,
        string PresentingBankCode, string PresentingStateCode, string PresentingBranchCode, string CheckDigit, string Serial, string PayingBankcCode,
        string PayingStateCode, string PayingBranchCode, string AccNo, string TCCode, string Amount, string ReturnCode, string ReturnCount, string ChequeType, string ReturnReason,
        string ItemInitialId, string MatchFlag, string IRDFlag, string IRDGenFlag, string IRDPringFlag, string CreateUserId,
        string CreateTimeStamp, string UpdateUserId, string UpdateTimeStamp, string IssuingBankType, string IssuingBankCode, string IssuingBankStateCode,
        string IssuingBankBranchCode, string IQA, string NCFlag, string ImageIndicator, string DocumentToFollow, string Reason, string DSVeriStatus, string ForUIC, string ForFileId)
        {

            try
            {
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnitemid", InwardReturnItemId));
                SqlParameterNext.Add(new SqlParameter("@p_fldinwardreturnfileid", InwardReturnFileId));
                SqlParameterNext.Add(new SqlParameter("@p_fldclearingdate", ClearingDate));
                SqlParameterNext.Add(new SqlParameter("@p_flduic", UIC));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbanktype", PresentingBankType));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbankcode", PresentingBankCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingstatecode", PresentingStateCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldpresentingbranchcode", PresentingBranchCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldcheckdigit", CheckDigit));
                SqlParameterNext.Add(new SqlParameter("@p_fldserial", Serial));
                SqlParameterNext.Add(new SqlParameter("@p_fldpayingbankccode", PayingBankcCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldpayingstatecode", PayingStateCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldpayingbranchcode", PayingBranchCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldaccno", AccNo));
                SqlParameterNext.Add(new SqlParameter("@p_fldtccode", TCCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldamount", Amount));
                SqlParameterNext.Add(new SqlParameter("@p_fldreturncode", ReturnCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldreturncount", ReturnCount));
                SqlParameterNext.Add(new SqlParameter("@p_fldchequetype", ChequeType));
                SqlParameterNext.Add(new SqlParameter("@p_fldreturnreason", ReturnReason));
                SqlParameterNext.Add(new SqlParameter("@p_flditeminitialid", ItemInitialId));
                SqlParameterNext.Add(new SqlParameter("@p_fldmatchflag", MatchFlag));
                SqlParameterNext.Add(new SqlParameter("@p_fldirdflag", IRDFlag));
                SqlParameterNext.Add(new SqlParameter("@p_fldirdgenflag", IRDGenFlag));
                SqlParameterNext.Add(new SqlParameter("@p_fldirdpringflag", IRDPringFlag));
                SqlParameterNext.Add(new SqlParameter("@p_fldcreateuserid", CreateUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldcreatetimestamp", CreateTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_fldupdateuserid", UpdateUserId));
                SqlParameterNext.Add(new SqlParameter("@p_fldupdatetimestamp", UpdateTimeStamp));
                SqlParameterNext.Add(new SqlParameter("@p_fldissuingbanktype", IssuingBankType));
                SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankcode", IssuingBankCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankstatecode", IssuingBankStateCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldissuingbankbranchcode", IssuingBankBranchCode));
                SqlParameterNext.Add(new SqlParameter("@p_fldiqa", IQA));
                SqlParameterNext.Add(new SqlParameter("@p_fldncflag", NCFlag));
                SqlParameterNext.Add(new SqlParameter("@p_fldimageindicator", ImageIndicator));
                SqlParameterNext.Add(new SqlParameter("@p_flddocumenttofollow", DocumentToFollow));
                SqlParameterNext.Add(new SqlParameter("@p_fldreason", Reason));
                SqlParameterNext.Add(new SqlParameter("@p_flddsveristatus", DSVeriStatus));
                SqlParameterNext.Add(new SqlParameter("@p_strforuic", ForUIC));
                SqlParameterNext.Add(new SqlParameter("@p_intforfileid", ForFileId));
                //Excute the command
                dbContext.GetRecordsAsDataTableSP("spcuUpdateInwardReturnItem", SqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public void PerformMatching(string userid, string bankCode, string cleardate, string Status)
        {
            DataTable IRRecordWithStatusDataTable;
            DataTable InwardReturnItemForMatchingDataTable;
            DataTable GetMatchItemIDDataTable;
            DateTime dtcleardate;
            dtcleardate = Convert.ToDateTime(cleardate);
            cleardate = dtcleardate.ToString("yyyy-MM-dd");
            string strMatchFlag = "N";
            Boolean isUnMatch = false;
            String strStatus = "3";
            string intItemInitialId;
            string strAccNo;
            string strIRFileID;
            string strIRItemStatus;
            string strIRItemUIC;
            string strIRItemID;
            
            try
            {

                IRRecordWithStatusDataTable = GetInwardReturnFileRecordWithStatus(bankCode, cleardate, "1");
                if (IRRecordWithStatusDataTable.Rows.Count > 0)
                {
                    InwardReturnItemForMatchingDataTable = GetInwardReturnItemForMatching(bankCode, cleardate, "1");
                    if (InwardReturnItemForMatchingDataTable.Rows.Count > 0)
                    {
                        foreach (DataRow drwItem in InwardReturnItemForMatchingDataTable.Rows)
                        {
                            strIRFileID = drwItem["fldinwardreturnfileid"].ToString().Trim();
                            strIRItemID = drwItem["fldinwardreturnitemid"].ToString().Trim();
                            strIRItemStatus = drwItem["fldstatus"].ToString().Trim();
                            strIRItemUIC = drwItem["flduic"].ToString().Trim();
                            GetMatchItemIDDataTable = GetMatchItemID(strIRItemUIC);

                            if (GetMatchItemIDDataTable.Rows.Count > 0)
                            {
                                foreach (DataRow drwItem1 in GetMatchItemIDDataTable.Rows)
                                {
                                    strMatchFlag = drwItem1["p_strmatch"].ToString().Trim();
                                    intItemInitialId = drwItem1["p_intiteminitialid"].ToString().Trim();
                                    strAccNo = drwItem1["p_straccno"].ToString().Trim();

                                    if (strMatchFlag == "N")
                                    {
                                        isUnMatch = true;
                                    }

                                    UpdateInwardReturnItem(
                                        strIRItemID, strIRFileID, cleardate, cleardate, strIRItemUIC,
                                        "",
                                        "", "", "", "", "",
                                        "", "", "", "", "",
                                        "", "", "", "", "",
                                        intItemInitialId, strMatchFlag, "Y", "", "",
                                        userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "",
                                        "", "", "", "", "",
                                        "", "", "", "", "", ""
                                        );

                                    //strStatus to keep the match status of a file

                                    if (isUnMatch == true)
                                    {
                                        strStatus = "4";
                                        if (!string.IsNullOrEmpty(intItemInitialId))
                                        {
                                            if (strAccNo == "")
                                            {
                                                CreateDuplicateIRItems(intItemInitialId, userid.ToString());
                                            }

                                        }
                                    }
                                    else
                                    {
                                        CreateDuplicateIRItems(intItemInitialId, userid.ToString());
                                    }

                                    UpdateInwardReturnFileRecord(strIRFileID, "", "", "", "", "", "", strStatus, "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", userid.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "", "", "", "", "");

                                }
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();

            }

        }
    }
}