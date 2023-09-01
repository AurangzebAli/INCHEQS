using System.Collections.Generic;

namespace INCHEQS.Areas.ICS.Models.BranchSubmission
{
    public interface IBranchSubmissionDao
    {
        List<BranchSubmissionModel> GetBranchSubmission();
        string GetCompleteCount();
        string GetIncompleteCount();
        string GetTotalCount();
        bool ValidateStatus(string fldCBranchId);
        void updateStatus(string branchCode);
        void UpdateAll();
        BranchSubmissionModel getBankInfo(string userId);
        bool CheckBranchConfirm(string gBranchCode, string gBranchCode2, string gBranchCode3, string sRegion, string sSubmissionType);
        bool CheckOfficer();
        BranchSubmissionModel getCollapseBranch(string gBranchCode, string gBranchCode2, string gBranchCode3);
        bool CheckPreRegionRight(string sClearDate);
        bool CheckCutOffPeriod();
        bool CheckPendingData(string myBranch, string AddSQL);
        bool CheckPospayData(string myBranch, string AddSQL);
        bool StartCopy(string myBranch, string AddSQL, string gBranchCode, string gBranchCode2);
        bool CheckExistBranch(string gBranchCode, string gBranchCode2);
        bool CheckBranchSubmissionPerformed(string branchCode);
        string GetClearDate();
    }
}