using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.Branch;

namespace INCHEQS.Areas.COMMON.Models.HubBranch
{
    public interface IHubBranchDao
    {
        void CreateHubBranchMasterTemp(FormCollection col, string bankcode, string crtUser);

        List<BranchModel> ListSelectedBranchInHub(string hubId, string strBankCode);
        void AddBranchToHubBranchTempToUpdate(string branchId, string hubId, string strUpdate, string strBankCode, string strAction);
        bool CheckBranchExistInHub(string hubId, string branchId, string strBankCode);
        bool DeleteAllBranchInHub(string hubId, string bankCode);
        void DeleteAllBranchInHubTemp(string hubId);
        void DeleteAllBranchInHubTempById(string hubId, string branchid);
        bool DeleteBranchNotSelected(string hubId, string branchIds, string strBankCode);
        bool DeleteBranchNotSelectedApproval(string hubCode, string branchIds);
        void InsertBranchInHub(string hubId, string branchId, string strUpdate, string strUpdateId, string strBankCode);
        List<BranchModel> ListAvailableBranchInHub(string strBankCode, string hubCode);
        List<string> ListSelectedBranchInHubTemp(string hubId, string strBankCode);
        void UpdateHubBranch(string hubId);
        bool UpdateSelectedBranch(string hubId, string BranchId, string strUpdate, string strBankCode);
        bool UpdateHubMaster(FormCollection col, string userId);
        void MoveToHubMasterFromTemp(string HubId, string Action);
        bool CheckHubBranchMasterTempByID(string HubID, string SearchFor);
        List<string> ValidateHubBranch(FormCollection col, string action, string branchId, string bankCode);
        List<BranchModel> ListSelectedBranchInHubChecker(string hubCode, string strBankCode);
        List<BranchModel> ListAvailableBranchInHubChecker(string strBankCode);
        bool CheckHubBranchExistInTemp(string hubcode, string branchCode, string Action);
    }
}