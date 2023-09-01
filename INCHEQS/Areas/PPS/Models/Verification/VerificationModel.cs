using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.PPS.Models.Verification
{
    public class VerificationModel
    {
        public string fldInwardItemId { get; set; }
        public string fldAccountNo { get; set; }
        public string fldChequeNo { get; set; }
        public string fldPayeeName { get; set; }
        public string fldAmount { get; set; }
        public string fldCheckDigit { get; set; }
        public string fldIssueDate { get; set; }
        public string fldPercentMatch { get; set; }
        public string fldModifiedFields { get; set; }
        public string fldPreBankType { get; set; }
        public string fldMatchResult { get; set; }
        public string fldIssueBankCode { get; set; }
        public string fldIssueStateCode { get; set; }
        public string fldIssueBranchCode { get; set; }
        public string fldClearDate { get; set; }
        public string fldPreBankCode { get; set; }
        public string fldPreStateCode { get; set; }
        public string fldPreBranchCode { get; set; }
        public string fldUIC { get; set; }
        public string fldNonConformance { get; set; }
        public string fldNonConformance2 { get; set; }
        public string fldRejectDesc { get; set; }
        public string fldNonDesc { get; set; }
        public string fldNonDesc1 { get; set; }
        public string fldIQADesc { get; set; }
        public string fldDocDesc { get; set; }
        public string fldHostDebit { get; set; }
        public string fldTransCode { get; set; }
        public string PayBranchDesc { get; set; }
        public string fldRejectCode { get; set; }
        public string fldRemarks { get; set; }
        public string fldHostAccountNo { get; set; }
        public string fldApprovalStatus { get; set; }
        public string fldSpickCode { get; set; }
        public string fldCharges { get; set; }
        public string fldRejectStatus1 { get; set; }
        public string fldRejectStatus2 { get; set; }
        public string fldRejectStatus3 { get; set; }
        public string fldRejectStatus4 { get; set; }
        public string fldImageFolder { get; set; }
        public string fldImageFilename { get; set; }
        public string fldHostStatus { get; set; }
        public string fldStatus { get; set; }
        public string fldValid { get; set; }
        public string fldPayee { get; set; }
        public string fldPayee1 { get; set; }
        public string fldPayee2 { get; set; }
        public string fldMicr { get; set; }
        public string HostReject { get; set; }

        // PPS OCR RESULT
        public string ScanChequeNo { get; set; }
        public string ScanAccountNo { get; set; }
        public string ScanBankCode { get; set; }
        public string ScanStateCode { get; set; }
        public string ScanBranchCode { get; set; }
        public string ScanChequeDate { get; set; }
        public string ScanChequeAmount { get; set; }
        public string ScanPayeeName { get; set; }

        // PPS FILE RESULT
        public string PPSChequeNo { get; set; }
        public string PPSAccountNo { get; set; }
        public string PPSBankCode { get; set; }
        public string PPSStateCode { get; set; }
        public string PPSBranchCode { get; set; }
        public string PPSChequeDate { get; set; }
        public string PPSChequeAmount { get; set; }
        public string PPSPayeeName { get; set; }


    }
}