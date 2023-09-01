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
//using INCHEQS.Areas.ICS.Service;
using System.IO;
using System.Net;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Data.Common;
using System.Data.Entity;

namespace INCHEQS.Models.Signature
{
    public class SignatureDao : ISignatureDao
    {

        private readonly SDSDbContext sdsDbContext;
        private readonly ApplicationDbContext dbContext;
        public SignatureDao(SDSDbContext sdsDbContext, ApplicationDbContext dbContext)

        {
            this.sdsDbContext = sdsDbContext;
            this.dbContext = dbContext;
        }
        public async Task<DataTable> GetSignatureNoForAccountAsync(string accountNo)
        {
            return await Task.Run(() => GetSignatureNoForAccount(accountNo));
        }
        public async Task<List<AccountInfo>> GetSignatureDetailsAsync(string accountNo, string issuingBankBranch)
        {
            return await Task.Run(() => GetSignatureDetails(accountNo, issuingBankBranch));
        }
        public async Task<List<AccountInfo>> GetSignatureRulesAsync(string accountNo, string issuingBankBranch)
        {
            return await Task.Run(() => GetSignatureRules(accountNo, issuingBankBranch));
        }
        //public async Task<List<SignatureInfo>> GetCheckedSignatureAsync(string inwardItemId)
        //{
        //    return await Task.Run(() => GetCheckedSignature(inwardItemId));
        //}
        public async Task<List<AccountInfo>> GetImageDetailsAsync(string AccNo, string issuingBankBranch, string imageNo)
        {
            return await Task.Run(() => GetImageInfo(AccNo, issuingBankBranch, imageNo));
        }
        public async Task<List<AccountInfo>> GetSignatureRulesInfoListAsync(string accountNo, string issuingBankBranch)
        {
            return await Task.Run(() => GetSignatureRulesInfoList(accountNo));
        }
        //public async Task<DataTable> GetAIFMasterList(string AccNumber)
        //{
        //    return await Task.Run(() => AIFMasterList(AccNumber));
        //}


        public bool CheckValidateSignature(FormCollection collection, AccountModel currentUser)
        {
            string signatureBox = collection["signatureBox"].Trim().Replace(",", "|");
            String result = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@AccountNumber", collection["current_fldHostaccountNo"]));
            sqlParameterNext.Add(new SqlParameter("@arrImageNo", signatureBox));
            sqlParameterNext.Add(new SqlParameter("@AmountLimit", collection["current_fldAmount"]));
            sqlParameterNext.Add(new SqlParameter("@UserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@ClearDate", DateUtils.formatDateToSql(collection["fldClearDate"])));
            DataTable resultTable = null;
            resultTable = sdsDbContext.GetRecordsAsDataTableSP("sp_ValidateSignatureRule", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                result = resultTable.Rows[0]["Result"].ToString();
            }
            if (result != "0")
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        //    foreach (DataRow row in dataTable.Rows) {
        //        SignatureInfo signatureInfo = new SignatureInfo();
        //        signatureInfo.accountName = row["fldAccountName"].ToString();
        //        signatureInfo.accountNo = row["fldAccountNumber"].ToString();
        //        signatureInfo.accountStatus = row["fldAccountStatus"].ToString();
        //        signatureInfo.bankCode = row["fldBankCode"].ToString();
        //        signatureInfo.branchCode = row["fldBranchCode"].ToString();
        //        signatureInfo.accountType = row["fldAccountType"].ToString();
        //        signatureInfo.imageId = row["fldImageId"].ToString();
        //        signatureInfo.imageNo = row["fldImageNo"].ToString();
        //        signatureInfo.imageStatus = row["fldImageStatus"].ToString();
        //        signatureInfo.imageDesc = row["fldImageDesc"].ToString();
        //        signatureInfo.sigGroup = row["SigGroup"].ToString();
        //        result.Add(signatureInfo);
        //    }

        //    return result;
        //}
        //MBSB Enquery
        public List<AccountInfo> GetSignatureDetails(string accountNo, string issuingBankBranch)
        {
            //if digit 10, need to convert to internal account number
            //if (accountNo.Length.Equals(10))
            //{
            //    accountNo = GetAccConversion(accountNo, issuingBankBranch);
            //}

            List<AccountInfo> result = new List<AccountInfo>();
            String URL = "";


            //string mySQL = "  Select ca.fldAccountNumber as AccNo,ca.fldAccountName as AccName, ca.fldAccountStatus as Status, info.fldImageNo as ImageNo from tblImageInfo info inner join tblCustomerAccount ca on info.fldImageId=ca.fldAccountNumber WHERE (ca.fldAccountNumber = @fldAccountNo) ";
            string mySQL = "  SELECT a.fldImageId as accNo, a.fldImageNo as ImageNo, a.fldImageDesc as imageDesc, a.fldEffectivedDate as effdate, a.fldImageStatus as Status, b.GroupMember as GroupMember " +
                            " from tblImageInfo a WITH(NOLOCK) left join(SELECT DISTINCT fldsigGroupid as 'fldSignature', GroupMember = substring((SELECT ', ' + fldsigGroupid FROM tblImageGroup e2" +
                            " WHERE e2.fldimageno = e1.fldsigGroupid AND e2.fldImageId = @fldAccountNo GROUP BY fldsigGroupid FOR XML path(''), elements),2,500) " +
                            " FROM tblSigGroupMaster e1 where fldAccountNumber = @fldAccountNo and fldGroupIndicator = 'S' ) b ON a.fldImageNo = b.fldSignature where a.fldimageid = @fldAccountNo  ";
            DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(mySQL, new[] { new SqlParameter("@fldAccountNo", accountNo) });


            foreach (DataRow row in dataTable.Rows)
            {
                AccountInfo AccountInfo = new AccountInfo();
                AccountInfo.accountNo = row["AccNo"].ToString().Trim();
                //AccountInfo.accountName = row["AccName"].ToString();
                AccountInfo.accountStatus = row["Status"].ToString().Trim();
                //AccountInfo.accountType = row["fldaccounttype"].ToString();
                AccountInfo.imageId = accountNo;
                AccountInfo.imageNo = row["ImageNo"].ToString().Trim();
                //AccountInfo.imageStatus = row["fldImageStatus"].ToString();
                //AccountInfo.imageDesc = row["fldImageDesc"].ToString();
                AccountInfo.sigGroup = row["GroupMember"].ToString().Trim();
                //AccountInfo.bankCode = row["fldBankCode"].ToString();
                //AccountInfo.branchCode = row["fldBranchCode"].ToString();
                result.Add(AccountInfo);
            }
            return result;
        }
        public List<AccountInfo> GetSignatureRules(string accountNo, string issuingBankBranch)
        {

            //if digit 10, need to convert to internal account number
            //if (accountNo.Length.Equals(10))
            //{
            //    accountNo = GetAccConversion(accountNo, issuingBankBranch);
            //}

            List<AccountInfo> result = new List<AccountInfo>();
            //string mySQL = "  select fldAccountNumber as accNo , fldAccountName, fldAccountStatus,fldNoToSign,  " +
            //               "  convert(varchar(10), fldOpenedDate, 103) as fldOpenedDate, replace(fldCondition, '<', '!') as fldCondition,fldCountry " +
            //               "  FROM tblCustomerAccount WITH(NOLOCK) " +
            //               "  WHERE(fldAccountNumber = @fldAccountNumber) ";

            string mySQL = "  select fldAccountNumber as accNo , fldAccountName, fldAccountStatus,fldNoToSign,  " +
                           "  convert(varchar(10), fldOpenedDate, 103) as fldOpenedDate, fldCondition,fldCountry " +
                           "  FROM tblCustomerAccount WITH(NOLOCK) " +
                           "  WHERE(fldAccountNumber = @fldAccountNumber) ";
            DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(mySQL, new[] { new SqlParameter("@fldAccountNumber", accountNo) });

            foreach (DataRow row in dataTable.Rows)
            {
                AccountInfo signatureRule = new AccountInfo()
                {
                    accNo = row["accNo"].ToString(),
                    accName = row["fldAccountName"].ToString(),
                    accountStatus = row["fldAccountStatus"].ToString(),
                    fldRequireSigNo = row["fldNoToSign"].ToString(),
                    openDate = row["fldOpenedDate"].ToString(),
                    condition = row["fldCondition"].ToString(),
                    Nationality = row["fldCountry"].ToString(),

                };
                result.Add(signatureRule);
            }

            return result;


        }

        //public List<SignatureInfo> GetSignatureRules(string accountNo) {
        //    List<SignatureInfo> result = new List<SignatureInfo>();
        //    string mySQL = " WITH CTE AS (select" +
        //                    " 'G' as SigType  , a.fldSigGroupId, a.fldSigRuleId, isNULL(a.fldRequireSigNo, 0) as fldRequireSigNo , isnull(b.fldSigAmountLimit, 0) as fldSigAmountLimit, fldSigValidFrom, fldSigValidTo," +
        //                    " 'R' + CAST(DENSE_RANK() OVER(ORDER BY a.fldSigRuleId) as varchar(10)) AS condition, isnull(fldSigAmountFrom,0) as fldSigAmountFrom " +
        //                    " from tblSigGroupRule a inner join " +
        //                    " tblSigRule b on a.fldAccountNumber = b.fldAccountNumber AND a.fldSigRuleId = b.fldRuleId " +
        //                    " where a.fldAccountNumber = @fldAccountNumber " +
        //                    ") SELECT MAX(condition)condition, MAX(fldRequireSigNo) AS fldRequireSigNo, fldSigGroupId, MAX(fldSigAmountFrom) AS fldSigAmountFrom, MAX(fldSigAmountLimit) AS fldSigAmountLimit, MAX(fldSigValidFrom) AS fldSigValidFrom, MAX(fldSigValidTo) AS fldSigValidTo " +
        //                    " from CTE group by fldSigGroupId order by condition ; ";

        //    DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(mySQL, new[] { new SqlParameter("@fldAccountNumber", accountNo) });

        //    foreach (DataRow row in dataTable.Rows) {
        //        SignatureInfo signatureRule = new SignatureInfo();
        //        signatureRule.condition = row["condition"].ToString();
        //        signatureRule.fldRequireSigNo = row["fldRequireSigNo"].ToString();
        //        signatureRule.sigGroup = row["fldSigGroupId"].ToString();
        //        signatureRule.fldRequireSigNo = row["fldRequireSigNo"].ToString();
        //        signatureRule.fldSigAmountFrom = row["fldSigAmountFrom"].ToString();
        //        signatureRule.fldSigAmountLimit = row["fldSigAmountLimit"].ToString();
        //        signatureRule.fldSigValidFrom = DateUtils.formatDateFromSql(row["fldSigValidFrom"].ToString());
        //        signatureRule.fldSigValidTo = DateUtils.formatDateFromSql(row["fldSigValidTo"].ToString());
        //        result.Add(signatureRule);
        //    }

        //    return result;
        //}

        public List<AccountInfo> GetSignatureRulesInfo(string accountNo)
        {
            List<AccountInfo> signatureRulesInfo = new List<AccountInfo>();

            try
            {
                DataTable resultTable = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                string condition = "";
                int i = 0;

                //kathik - update to display signature information status in full description instead of initial. New SP is created.
                sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNo.Trim()));
                resultTable = sdsDbContext.GetRecordsAsDataTableSP("spcgSignatureInformation", sqlParameterNext.ToArray());
                if (resultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in resultTable.Rows)
                    {
                        AccountInfo SignatureRules = new AccountInfo();
                        SignatureRules.accNo = row["accNo"].ToString();
                        SignatureRules.accName = row["fldAccountName"].ToString();
                        SignatureRules.accountStatus = row["fldAccountStatus"].ToString();
                        SignatureRules.fldRequireSigNo = row["fldNoToSign"].ToString();
                        SignatureRules.openDate = row["fldOpenedDate"].ToString();
                        SignatureRules.effectiveDate = row["fldEffectivedDate"].ToString();
                        condition = row["fldCondition"].ToString();
                        SignatureRules.arrCondition = condition.Split('|');
                        SignatureRules.countCondition = SignatureRules.arrCondition.Count();

                        signatureRulesInfo.Add(SignatureRules);
                    }

                }


            }


            catch (Exception exception)
            {
                throw exception;
            }

            return signatureRulesInfo;
        }

        public List<AccountInfo> GetSignatureInformation(string accountNo)
            {
            //if digit 10, need to convert to internal account number
            //if (accountNo.Length.Equals(10))
            //{
            //    accountNo = GetAccConversion(accountNo, issueBankBranch);
            //}

            List<AccountInfo> signatureInformation = new List<AccountInfo>();

            //List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNo));

            try
            {
                string imgSQL = "  Select fldAccountNumber as 'AccNo',fldAccountName as 'AccName',CASE WHEN fldAccountStatus = 'V' THEN 'Validated' WHEN fldAccountStatus = 'I' THEN 'Invalidated' WHEN fldAccountStatus = 'C' THEN 'Closed' ELSE 'Unknown' END as Status  " +
                                "  FROM tblCustomerAccount " +
                                "  WHERE(fldAccountNumber = @fldAccountNumber)";

                DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(imgSQL, new[] { new SqlParameter("@fldAccountNumber", accountNo) });


                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        AccountInfo SignatureInfo = new AccountInfo()
                        {
                            accNo = row["AccNo"].ToString(),
                            accName = row["AccName"].ToString(),
                            status = row["Status"].ToString(),

                        };

                        signatureInformation.Add(SignatureInfo);

                    }
                }
            }


            catch (Exception exception)
            {
                throw exception;
            }

            return signatureInformation;
        }

        public DataTable GetSignatureNoForAccount(string accountNo)
        {
            DataTable dt = new DataTable();
            string imgSQL = "SELECT fldImageNo FROM tblimageInfo WHERE fldImageId = @fldAccountNo order by CAST(fldImageNo as Integer) ";
            dt = sdsDbContext.GetRecordsAsDataTable(imgSQL, new[] { new SqlParameter("@fldAccountNo", accountNo) });
            return dt;
        }

        /*public ReturnCodeModel GetReturnCode(string ReturnCode)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ReturnCodeModel returncode = new ReturnCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                returncode.fldRejectCode = row["fldRejectCode"].ToString();
                returncode.fldRejectDesc = row["fldRejectDesc"].ToString();
                returncode.fldRejectType = row["fldRejectType"].ToString();
                returncode.fldCharges = row["fldCharges"].ToString();
                returncode.fldCreateTimeStamp = row["fldCreateTimeStamp"].ToString();
                returncode.fldCreateUserId = row["fldCreateUserId"].ToString();
            }
            else
            {
                returncode = null;
            }
            return returncode;
        }*/

        public async Task<byte[]> GetSignatureImage(string accountNo, string imageNo)
        {
            byte[] byteArray = null;
            //string sql = " Select b.fldImageCode as imageSign from  tblCustomeraccount a inner join tblimageinfo  b ON a.fldaccountnumber = b.fldImageId " +
            //                "WHERE b.fldImageid = @fldAccountNo AND b.fldImageNo = @fldImageNo AND b.fldImageStatus = 'V' ";


            //byteArray = (byte[])await Task.Run(() => sdsDbContext.ExecuteScalar(CommandType.Text, sql, new[] { new SqlParameter("@fldImageNo", imageNo), new SqlParameter("@fldAccountNo", accountNo) }));

            string sql = " Select fldImageCode as imageSign from  tblImageInfo " +
                           "WHERE fldImageId = @fldAccountNo and fldImageNo = @fldImageNo";

            byteArray = (byte[])await Task.Run(() => sdsDbContext.ExecuteScalar(CommandType.Text, sql, new[] { new SqlParameter("@fldImageNo", imageNo), new SqlParameter("@fldAccountNo", accountNo) }));
            return byteArray;
        }

        public async Task<bool> ISSDSConnectionAvailable()
        {
            //string sql = "SELECT fldImageCode as imageSign FROM tblimageInfo WHERE fldImageid = @fldAccountNo AND fldImageNo = @fldImageNo ";


            //    string myXMLfile = @"D:\MBSB_ICS\SVS.xml";

            // System.IO.File.AppendAllText(@"D:\debug.txt", "myXMLfile-" + myXMLfile);




            return await Task.Run(() => sdsDbContext.IsServerConnected());
        }

        //bellaaaaaaaaaaa
        public List<AccountInfo> GetImageInfo(string AccNo, string issuingBankBranch, string ImageNo)
        {
            //if digit 10, need to convert to internal account number
            //if (AccNo.Length.Equals(10))
            //{
            //    AccNo = GetAccConversion(AccNo, issuingBankBranch);
            //}
            List<AccountInfo> result = new List<AccountInfo>();

            string imgSQL = " select fldImageId, fldImageGroup, fldimageDesc, fldEffectivedDate, fldImageStatus " +
                            " from tblImageInfo WITH (NOLOCK) " +
                            " WHERE fldImageId = @fldAccountNo and fldImageNo = @fldImageNo";

            DataTable dataTable = sdsDbContext.GetRecordsAsDataTable(imgSQL, new[] { new SqlParameter("@fldAccountNo", AccNo), new SqlParameter("@fldImageNo", ImageNo) });

            string groupSQL = "select a.fldSigGroupId" +
                              " from tblImageGroup a left join tblSigGroupMaster b on a.fldImageId = b.fldAccountNumber and a.fldSigGroupId = b.fldSigGroupId and b.fldGroupIndicator = 'G'" +
                              " where a.fldImageId = @fldAccountNo and a.fldImageNo = @fldImageNo";

            DataTable dataTable1 = sdsDbContext.GetRecordsAsDataTable(groupSQL, new[] { new SqlParameter("@fldAccountNo", AccNo), new SqlParameter("@fldImageNo", ImageNo) });


            foreach (DataRow row in dataTable.Rows)
            {
                AccountInfo imageInfo = new AccountInfo();
                string imageStatus = "";
                string imageGroup = "";
                imageInfo.imageId = row["fldImageId"].ToString();
                //imageInfo.groupName = row["fldImageGroup"].ToString();
                imageInfo.imageDesc = row["fldimageDesc"].ToString();
                imageStatus = row["fldImageStatus"].ToString().Trim();
                if (imageStatus == "V")
                {
                    imageInfo.imageStatus = "Valid";
                }
                else
                {
                    imageInfo.imageStatus = "Invalid";
                }
                //imageInfo.imageStatus = row["fldImageStatus"].ToString();
                imageInfo.AccEffective = row["fldEffectivedDate"].ToString();
                //imageInfo.sigGroup = row["imageGroup"].ToString();
                //imageInfo.imageId = row["ID"].ToString();
                //imageInfo.Nationality = row["Nationality"].ToString();
                //imageInfo.Relation = row["Relation"].ToString();
                foreach (DataRow rows in dataTable1.Rows)
                {
                    if (imageGroup != "")
                    {
                        imageGroup = imageGroup + ", ";
                    }
                    imageGroup = imageGroup + rows["fldSigGroupId"].ToString().Trim();
                }
                imageInfo.groupName = imageGroup;
                result.Add(imageInfo);

            }

            return result;
        }

        public List<AccountInfo> GetSignatureRulesInfoList(string accountNo)
        {
            List<AccountInfo> signatureRulesInfoList = new List<AccountInfo>();

            try
            {
                DataTable resultTable = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                string rowGroupId = "";
                string sGrpInd = "";
                string signID = "";

                int i = 0;
                string amount = "";

                //kathik - update to display signature information status in full description instead of initial. New SP is created.
                sqlParameterNext.Add(new SqlParameter("@accountNumber", accountNo.Trim()));
                resultTable = sdsDbContext.GetRecordsAsDataTableSP("spcgSinatureRuleInfo", sqlParameterNext.ToArray());
                if (resultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in resultTable.Rows)
                    {
                        AccountInfo SignatureRules = new AccountInfo();

                        DataTable resultTable1 = new DataTable();
                        List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();


                        SignatureRules.RuleNo = row["fldSigRuleId"].ToString();
                        SignatureRules.ConditionTypeID = row["fldStaticConditionId"].ToString();

                        sqlParameterNext1.Add(new SqlParameter("@fldSigRuleId", SignatureRules.RuleNo.Trim()));
                        resultTable1 = sdsDbContext.GetRecordsAsDataTableSP("spcgGetSigGroup", sqlParameterNext1.ToArray());
                        if (resultTable1.Rows.Count > 0)
                        {
                            int w = 0;
                            foreach (DataRow rows in resultTable1.Rows)
                            {
                                rowGroupId = rows["fldSigGroupId"].ToString();
                                sGrpInd = rows["fldGroupIndicator"].ToString();

                                if (sGrpInd == "G" && w == 0)
                                {
                                    signID = "1 of Group " + rowGroupId;
                                }
                                else if (sGrpInd == "G" && w > 0)
                                {
                                    signID = signID + ", 1 of Group " + rowGroupId;

                                }
                                else if (sGrpInd == "S" && w == 0)
                                {
                                    signID = rowGroupId;
                                }
                                else if (sGrpInd == "S" && w > 0)
                                {
                                    signID = signID + "," + rowGroupId;
                                }
                                w = w + 1;
                            }
                        }

                        SignatureRules.SignID = signID;
                        amount = row["fldSigAmountLimit"].ToString();
                        //SignatureRules.amount = row["fldSigAmountLimit"].ToString();
                        if (Convert.ToDouble(amount) < 0 || Convert.ToDouble(amount) == 0)
                        {
                            SignatureRules.amount = "No Limit";
                        }
                        else if (amount == "0.01")
                        {
                            SignatureRules.amount = "Co. Seal";
                        }
                        else
                        {
                            SignatureRules.amount = StringUtils.FormatCurrency(amount);
                        }
                        SignatureRules.fldSigValidFrom = row["fldSigValidFrom"].ToString();
                        SignatureRules.fldSigValidTo = row["fldSigValidTo"].ToString();

                        signatureRulesInfoList.Add(SignatureRules);
                    }

                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return signatureRulesInfoList;
        }
    }

}