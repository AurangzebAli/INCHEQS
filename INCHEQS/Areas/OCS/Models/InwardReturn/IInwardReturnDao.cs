using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.InwardReturn
{
    public interface IInwardReturnDao
    {
        List<InwardReturnModel> GetItemStatusListing(string BankCode, string cleardate, string status);

        DataTable GetInwardReturnFileRecordWithStatus(string BankCode, string cleardate, string status);
        DataTable GetInwardReturnItemForMatching(string BankCode, string cleardate, string status);
        DataTable GetMatchItemID(string uic);
        void UpdateInwardReturnItem(string InwardReturnItemId, string InwardReturnFileId, string ClearingDate, string cleardate, string UIC, string PresentingBankType, string PresentingBankCode,
            string PresentingStateCode, string PresentingBranchCode, string CheckDigit, string Serial, string PayingBankcCode,
            string PayingStateCode, string PayingBranchCode, string AccNo, string TCCode, string Amount, string ReturnCode, string ReturnCount, string ChequeType, string ReturnReason,
            string ItemInitialId, string MatchFlag, string IRDFlag, string IRDGenFlag, string IRDPringFlag, string CreateUserId,
            string CreateTimeStamp, string UpdateUserId, string UpdateTimeStamp, string IssuingBankType, string IssuingBankCode, string IssuingBankStateCode,
            string IssuingBankBranchCode, string IQA, string NCFlag, string ImageIndicator, string DocumentToFollow, string Reason, string DSVeriStatus, string ForUIC, string ForFileId);
        void PerformMatching(string UserId, string BankCode, string cleardate, string status);
    }
}