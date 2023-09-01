using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Signature
{
    public class AccountInfo
    {
        public string accountNo { get; set; }
        public string imageNo { get; set; }
        public string accountName { get; set; }
        public string bankCode { get; set; }
        public string branchCode { get; set; }
        public string accountStatus { get; set; }
        public string accountType { get; set; }
        public string externalCustomer { get; set; }
        public string imageStatus { get; set; }
        public string imageDesc { get; set; }
        public string condition { get; set; }
        public string fldRequireSigNo { get; set; }
        public string checkedSignature { get; set; }
        public string sigGroup { get; set; }
        public byte Imagecode { get; set; }
        public string fldSigAmountFrom { get; set; }
        public string fldSigAmountLimit { get; set; }

        public string AcExpiryDate { get; set; }
        public string imageId { get; set; }
        public string ImageCode { get; set; }
        public string SignatureRules { get; set; }
        public string SignatureSeq { get; set; }
        public string ProductTypeCode { get; set; }
        public string AccEffective { get; set; }
        public string AccExpiry { get; set; }
        public string Nationality { get; set; }
        public string Relation { get; set; }

        public string groupName { get; set; }
        public string totalReq { get; set; }
        public string minAmount { get; set; }
        public int realRuleNo { get; set; }
        public string accNo { get; set; }
        public string accName { get; set; }
        public string status { get; set; }
        public string openDate { get; set; }
        public string effectiveDate { get; set; }
        public string[] arrCondition { get; set; }
        public int countCondition { get; set; }
        public string RuleNo { get; set; }
        public string ConditionTypeID { get; set; }
        public string SignID { get; set; }
        public string amount { get; set; }
        public string fldSigValidFrom { get; set; }
        public string fldSigValidTo { get; set; }

    }

    public class SignatureModel
    {
        public string fldProcessName { get; set; }
        public string fldSystemProfileCode { get; set; }
        public int fldDateSubString { get; set; }
        public int fldBankCodeSubString { get; set; }
        public string fldFileExt { get; set; }
        public string fldPosPayType { get; set; }
    }

    public class SignatureInfo
    {
        public string accountNo { get; set; }
        public string accountName { get; set; }

        public string accOpenDate { get; set; }
        public string accountStatus { get; set; }
        public string condition { get; set; }
        public string phoneNo { get; set; }
        public string notes { get; set; }
        public string occupation { get; set; }
        public string introducer { get; set; }
        public string nationality { get; set; }
        public string NRIC { get; set; }

        public string bankCode { get; set; }
        public string branchCode { get; set; }

        public string accountType { get; set; }
        public string externalCustomer { get; set; }
        public string imageId { get; set; }
        public string imageNo { get; set; }
        public string imageStatus { get; set; }
        public string imageDesc { get; set; }
        public string effectivedDate { get; set; }
        public string expiredDate { get; set; }
        public string approvalDate { get; set; }
        public string remark { get; set; }


        public string sigGroup { get; set; }

        public string fldRequireSigNo { get; set; }
        public string fldSigAmountFrom { get; set; }
        public string fldSigAmountLimit { get; set; }
        public string fldSigValidFrom { get; set; }
        public string fldSigValidTo { get; set; }

        public string checkedSignature { get; set; }

        public string icNumber { get; set; }
        public string opendate { get; set; }
        public string address { get; set; }
        public string introducerName { get; set; }
        public string country { get; set; }
        public byte[] sigImage { get; set; }
        public string image { get; set; }
        public string imageGroup { get; set; }

    }
}