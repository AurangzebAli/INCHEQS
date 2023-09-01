using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.HubBranch
{
    public interface IHubBranchDao
    {
        void AddHubToHubMasterBranchTempToUpdate(FormCollection col, string strUpdate, string strBankCode);
        void AddBranchToHubBranchTempToUpdate(string branchId, string hubId, string strUpdate, string strBankCode);
        bool CheckBranchExistInHub(string hubId, string branchId);
        bool CheckBranchExistInHub(string hubId, string branchId, string strBankCode);
        bool CheckPendingApproval(string hubId, string StrBankCode);
        void DeleteAllBranchInHub(string hubId);
        void DeleteAllBranchInHub(string hubId, string strBankCode);
        void DeleteAllBranchInHubTemp(string hubId);
        void DeleteAllBranchInHubTemp(string hubId, string strBankCode);
        void DeleteInHubMasterBranchTemp(string HubId);
        void DeleteBranchNotSelected(string hubId, string branchIds);
        void DeleteBranchNotSelected(string hubId, string branchIds, string strBankCode);
        void InsertBranchInHub(string hubId, string branchId, string strUpdate, string strUpdateId);
        void InsertBranchInHub(string hubId, string branchId, string strUpdate, string strUpdateId, string strBankCode);
        List<InternalBranchModel> ListAvailableBranchInHub(string hubId, string strBankCode);
        List<InternalBranchModel> ListSelectedBranchInHub(string hubId, string strBankCode);
        List<string> ListSelectedBranchInHubTemp(string hubId, string strBankCode);
        void UpdateHubMaster(string hubId);
        void UpdateHubBranch(string hubId);
        void UpdateSelectedBranch(string hubId, string BranchId, string strUpdate);
        void UpdateSelectedBranch(string hubId, string BranchId, string strUpdate, string strBankCode);
    }
}