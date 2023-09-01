using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.CommonOutwardItem
{
    public class ChequeRepairHistoryModel
    {
        public string fldCreateTimeStamp { get; set; }
        public string fldAmount { get; set; }
        public string fldOriCheckDigit { get; set; }
        public string fldOriSerial { get; set; }
        public string fldOriBankCode { get; set; }
        public string fldOriStateCode { get; set; }
        public string fldOriBranchCode { get; set; }
        public string fldOriIssuerAccNo { get; set; }
        public string fldOriRemark { get; set; }
        public string fldOriTCCode { get; set; }
        public string fldCheckDigit { get; set; }
        public string fldSerial { get; set; }
        public string fldBankCode { get; set; }
        public string fldStateCode { get; set; }
        public string fldBranchCode { get; set; }
        public string fldIssuerAccNo { get; set; }
        public string fldRemark { get; set; }
        public string fldTCCode { get; set; }
        public string fldType { get; set; }
        public string fldLocation { get; set; }
        public string fldReasonCode { get; set; }
        public string fldUpdateUser { get; set; }
        public string fldPayeeAccNo { get; set; }
        public string fldclearingstatusdesc { get; set; }
        public string fldcreatetimestamp { get; set; }
        public string fldClearingStatus { get; set; }
    }

    public class BalancingHistoryModel
    {
        public string fldItemID { get; set; }
        public string fldItemInitialID { get; set; }
        public string fldItemType { get; set; }
        public string fldBankCode { get; set; }
        public string fldPVaccNo { get; set; }
        public string fldAmount { get; set; }
        public string CheckerMaker { get; set; }
        public string fldSerial { get; set; }
        public string flduic { get; set; }


    }
    public class SearchHistoryModel
    {
        public string fldItemID { get; set; }
        public string fldItemInitialID { get; set; }
        public string fldItemType { get; set; }
        public string fldTransNo { get; set; }
        public string fldActionStatus { get; set; }
        public string fldRemarks { get; set; }
        public string fldCreateUserId { get; set; }
        public string fldcreatetimestamp { get; set; }

    }
    public class ChequeRepairHistorySearchModel
    {
        //public string fldCreateTimeStamp { get; set; }
        public string fldAmount { get; set; }
        public string fldOriCheckDigit { get; set; }
        public string fldOriSerial { get; set; }
        public string fldOriBankCode { get; set; }
        public string fldOriStateCode { get; set; }
        public string fldOriBranchCode { get; set; }
        public string fldOriIssuerAccNo { get; set; }
        public string fldOriRemark { get; set; }
        public string fldOriTCCode { get; set; }
        public string fldCheckDigit { get; set; }
        public string fldSerial { get; set; }
        public string fldBankCode { get; set; }
        public string fldStateCode { get; set; }
        public string fldBranchCode { get; set; }
        public string fldIssuerAccNo { get; set; }
        public string fldRemark { get; set; }
        public string fldTCCode { get; set; }
        public string fldType { get; set; }
        public string fldLocation { get; set; }
        public string flduserabb { get; set; }
        public string fldclearingstatusdesc { get; set; }
        public string fldcreatetimestamp { get; set; }
        public string fldClearingStatus { get; set; }
        public string fldUpdateUser { get; set; }
        public string fldPayeeAccNo { get; set; }
        public string fldReasonCode { get; set; }
        public string fldCreateTimeStamp { get; set; }
    }

    public class BranchSubmissionItemList
    {

        public string fldCapturingBranch { get; set; }
        public string fldBranchDesc { get; set; }
        public string CapturingBranch { get; set; }
        public string CapturingDate { get; set; }
        public string fldBatchNumber { get; set; }
        public string fldCapturingMode { get; set; }
        public string TotalItem { get; set; }
        public string TotalAmount { get; set; }
        public string fldscannerid { get; set; }
        

    }
    public class clearingItemList
    {

        public string fldCapturingBranch { get; set; }
        public string fldBranchDesc { get; set; }
        public string CapturingBranch { get; set; }
        public string CapturingDate { get; set; }
        public string fldBatchNumber { get; set; }
        public string fldCapturingMode { get; set; }
        public string TotalItem { get; set; }
        public string TotalAmount { get; set; }
        public string fldscannerid { get; set; }


    }
    public class clearedItemList
    {

        public string fldCapturingBranch { get; set; }
        public string fldBranchDesc { get; set; }
        public string CapturingBranch { get; set; }
        public string CapturingDate { get; set; }
        public string fldBatchNumber { get; set; }
        public string fldCapturingMode { get; set; }
        public string TotalItem { get; set; }
        public string TotalAmount { get; set; }
        public string fldscannerid { get; set; }


    }
    public class BranchSubmittedItemList
    {

        public string fldCapturingBranch { get; set; }
        public string fldBranchDesc { get; set; }
        public string CapturingBranch { get; set; }
        public string CapturingDate { get; set; }
        public string fldBatchNumber { get; set; }
        public string fldCapturingMode { get; set; }
        public string TotalItem { get; set; }
        public string TotalAmount { get; set; }
        public string fldscannerid { get; set; }

    }

    public class OCSProgressStatus
    {

        public string TotalNormalCapturingItems { get; set; }
        public string TotalNormalCapturingAmount { get; set; }
        public string TotalDataEntryItems { get; set; }
        public string TotalAmountEntryItem { get; set; }
        public string TotalAmountEntryAmount { get; set; }
        public string TotalBalancingItem { get; set; }
        public string TotalBalancingAmount { get; set; }
        public string TotalMICRRepairItem { get; set; }
        public string TotalMICRRepairAmount { get; set; }
        public string TotalReadyforSubmit { get; set; }
        public string TotalSubmittedItem { get; set; }
        public string TotalApproved { get; set; }
        public string TotalClearedItem { get; set; }
        public string TotalPending { get; set; }
        public string TotalRejected { get; set; }

        //michelle
        public string TotalPercentCompletion { get; set; }
        public string TotalInwardReturn { get; set; }
        public string TotalNormalCheque { get; set; }
        public string TotalNonClearingCheque { get; set; }
        public string TotalItemCheque { get; set; }
        public string TotalDepositSlip { get; set; }
        public string TotalCheque { get; set; }
        public string TotalInwardReturnICL { get; set; }
        public string TotalIRMatch { get; set; }
        public string TotalIRUnmatch { get; set; }
        public string TotalPendingCreditPosting { get; set; }
        public string TotalCompleteCreditPosting { get; set; }
        public string TotalPendingDebitPosting { get; set; }
        public string TotalCompleteDebitPosting { get; set; }
        public string TotalPendingBranchClearingItem { get; set; }
        public string TotalCompleteBranchClearingItem { get; set; }
        public string TotalPendingGenOutwardICL { get; set; }
        public string TotalCompleteGenOutwardICL { get; set; }
        public string CaptureMode { get; set; }
        public string TotalChequeDateAmountEntryItem  { get; set; }
        public string TotalChequeDateAmountEntryAmount  { get; set; }

    }
}