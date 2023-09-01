using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using INCHEQS.Models.Sequence;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer.SDS;

namespace INCHEQS.Models.Signature {
    public class SignatureDao : ISignatureDao {

        private readonly SDSDbContext sdsDbContext;
        private readonly ApplicationDbContext dbContext;

        public SignatureDao(SDSDbContext sdsDbContext, ApplicationDbContext dbContext) {
            this.sdsDbContext = sdsDbContext;
            this.dbContext = dbContext;
        }
        public async Task<List<SignatureInfo>> GetSignatureDetailsAsync(string accountNo) {
            return await Task.Run(() => GetSignatureDetails(accountNo));
        }
        public async Task<List<SignatureInfo>> GetSignatureRulesAsync(string accountNo) {
            return await Task.Run(() => GetSignatureRules(accountNo));
        }
        public async Task<List<SignatureInfo>> GetCheckedSignatureAsync(string inwardItemId) {
            return await Task.Run(() => GetCheckedSignature(inwardItemId));
        }
        public async Task<DataTable> GetSignatureNoForAccountAsync(string accountNo) {
            return await Task.Run(() => GetSignatureNoForAccount(accountNo));
        }


        public bool CheckValidateSignature(FormCollection collection, AccountModel currentUser) {
            //run stored procs
            string signatureBox = collection["signatureBox"].Replace(",", "|");

            int return_value = sdsDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "sp_ValidateSignatureRule", new[] {
                new SqlParameter("@AccountNumber", collection["current_fldAccountNumber"]) ,
                new SqlParameter("@arrImageNo", signatureBox),
                new SqlParameter("@AmountLimit", collection["current_fldAmount"]),
                new SqlParameter("@UserId", currentUser.UserId),
                new SqlParameter("@_ClearDate", DateUtils.formatDateToSql(collection["fldClearDate"]))
            });

            if (return_value == -1) {
                return false;
            }else {
                return true;
            }
        }

        public List<SignatureInfo> GetSignatureDetails(string accountNo) {
            List<SignatureInfo> result = new List<SignatureInfo>();
            string mySQL = "select " +

                " STUFF((SELECT ',' + CAST(c.fldSigGroupId AS VARCHAR(MAX)) FROM tblImageGroup c where b.fldImageNo = c.fldImageNo FOR XML PATH('')),1,1,'') SigGroup, "+
                " fldAccountName,fldAccountNumber,fldAccountStatus,fldBankCode,fldBranchCode,a.fldAccountType,fldImageId,fldImageNo,fldImageStatus, fldImageDesc " +
                " from tblCustomerAccount a inner join tblImageInfo b on a.fldAccountNumber = b.fldImageId WHERE a.fldAccountnumber = @fldAccountNo and a.fldAccountStatus = 'V'";

            DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(mySQL, new[] { new SqlParameter("@fldAccountNo", accountNo) });

            foreach (DataRow row in dataTable.Rows) {
                SignatureInfo signatureInfo = new SignatureInfo();
                signatureInfo.accountName = row["fldAccountName"].ToString();
                signatureInfo.accountNo = row["fldAccountNumber"].ToString();
                signatureInfo.accountStatus = row["fldAccountStatus"].ToString();
                signatureInfo.bankCode = row["fldBankCode"].ToString();
                signatureInfo.branchCode = row["fldBranchCode"].ToString();
                signatureInfo.accountType = row["fldAccountType"].ToString();
                signatureInfo.imageId = row["fldImageId"].ToString();
                signatureInfo.imageNo = row["fldImageNo"].ToString();
                signatureInfo.imageStatus = row["fldImageStatus"].ToString();
                signatureInfo.imageDesc = row["fldImageDesc"].ToString();
                signatureInfo.sigGroup = row["SigGroup"].ToString();
                result.Add(signatureInfo);
            }

            return result;
        }

        public List<SignatureInfo> GetSignatureRules(string accountNo) {
            List<SignatureInfo> result = new List<SignatureInfo>();
            string mySQL = " WITH CTE AS (select" +
                            " 'G' as SigType  , a.fldSigGroupId, a.fldSigRuleId, isNULL(a.fldRequireSigNo, 0) as fldRequireSigNo , isnull(b.fldSigAmountLimit, 0) as fldSigAmountLimit, fldSigValidFrom, fldSigValidTo," +
                            " 'R' + CAST(DENSE_RANK() OVER(ORDER BY a.fldSigRuleId) as varchar(10)) AS condition, isnull(fldSigAmountFrom,0) as fldSigAmountFrom " +
                            " from tblSigGroupRule a inner join " +
                            " tblSigRule b on a.fldAccountNumber = b.fldAccountNumber AND a.fldSigRuleId = b.fldRuleId " +
                            " where a.fldAccountNumber = @fldAccountNumber " +
                            ") SELECT MAX(condition)condition, MAX(fldRequireSigNo) AS fldRequireSigNo, fldSigGroupId, MAX(fldSigAmountFrom) AS fldSigAmountFrom, MAX(fldSigAmountLimit) AS fldSigAmountLimit, MAX(fldSigValidFrom) AS fldSigValidFrom, MAX(fldSigValidTo) AS fldSigValidTo " +
                            " from CTE group by fldSigGroupId order by condition ; ";

            DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(mySQL, new[] { new SqlParameter("@fldAccountNumber", accountNo) });

            foreach (DataRow row in dataTable.Rows) {
                SignatureInfo signatureRule = new SignatureInfo();
                signatureRule.condition = row["condition"].ToString();
                signatureRule.fldRequireSigNo = row["fldRequireSigNo"].ToString();
                signatureRule.sigGroup = row["fldSigGroupId"].ToString();
                signatureRule.fldRequireSigNo = row["fldRequireSigNo"].ToString();
                signatureRule.fldSigAmountFrom = row["fldSigAmountFrom"].ToString();
                signatureRule.fldSigAmountLimit = row["fldSigAmountLimit"].ToString();
                signatureRule.fldSigValidFrom = DateUtils.formatDateFromSql(row["fldSigValidFrom"].ToString());
                signatureRule.fldSigValidTo = DateUtils.formatDateFromSql(row["fldSigValidTo"].ToString());
                result.Add(signatureRule);
            }

            return result;
        }

        public DataTable GetSignatureNoForAccount(string accountNo) {
            DataTable dt = new DataTable();
            string imgSQL = "SELECT fldImageNo FROM tblimageInfo WHERE fldImageId = @fldAccountNo order by CAST(fldImageNo as Integer) ";
            dt = sdsDbContext.GetRecordsAsDataTable(imgSQL, new[] { new SqlParameter("@fldAccountNo", accountNo) });
            return dt;
        }

        public async Task<byte[]> GetSignatureImage(string accountNo, string imageNo) {
            byte[] byteArray = null;
            string sql = "SELECT fldImageCode as imageSign FROM tblimageInfo WHERE fldImageid = @fldAccountNo AND fldImageNo = @fldImageNo ";
            byteArray = (byte[]) await Task.Run(() => sdsDbContext.ExecuteScalar(CommandType.Text, sql, new[] { new SqlParameter("@fldImageNo", imageNo), new SqlParameter("@fldAccountNo", accountNo) }));            
            return byteArray;
        }

        public async Task<bool> ISSDSConnectionAvailable() {
            return await Task.Run(() => sdsDbContext.IsServerConnected());
        }

        public void InsertSignatureHistoryImage(FormCollection collection, AccountModel currentUser) {

            //Constuct sql to tblInwardItemSigHistory
            List<string> signatureBoxs = collection["signatureBox"].Split(',').ToList();

            foreach (var imageNo in signatureBoxs) {
                Dictionary<string, dynamic> sqlInwardItemSigHistoryImage = new Dictionary<string, dynamic>();
                sqlInwardItemSigHistoryImage.Add("fldInwardItemId", collection["fldInwardItemId"]);
                sqlInwardItemSigHistoryImage.Add("fldCreateUserId", currentUser.UserId);
                sqlInwardItemSigHistoryImage.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlInwardItemSigHistoryImage.Add("fldImageNo", imageNo);
                //Execute the command
                dbContext.ConstructAndExecuteInsertCommand("tblInwardItemSigHistoryImage", sqlInwardItemSigHistoryImage);
            }
        }

        public void DeleteSignatureHistoryImage(FormCollection collection) {
            //Delete inwardItemSigHistoryNo for item that belong to inwarditemid
            string deleteSql = "DELETE FROM tblInwardItemSigHistoryImage WHERE fldInwardItemId=@fldInwardItemId";
            dbContext.ExecuteNonQuery(deleteSql, new[] {
                new SqlParameter("@fldInwardItemId", collection["fldInwardItemId"])
            });
        }

        public List<SignatureInfo> GetCheckedSignature(string inwardItemId) {
            List<SignatureInfo> result = new List<SignatureInfo>();
            string mySQL = "SELECT fldImageNo FROM tblInwardItemSigHistoryImage WHERE fldInwardItemId=@fldInwardItemId";

            DataTable dataTable = dbContext.GetRecordsAsDataTable(mySQL, new[] {
                new SqlParameter("@fldInwardItemId", inwardItemId)
            });

            foreach (DataRow row in dataTable.Rows) {
                SignatureInfo image = new SignatureInfo();
                image.checkedSignature = row["fldImageNo"].ToString();
                result.Add(image);
            }

            return result;
        }
    }
}