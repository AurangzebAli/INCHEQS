using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.HubBranchProfile {
    public interface IHubBranchProfileDao {
        DataTable ListAllHubBranch();
        DataTable getHubBranch(string hubId);
        DataTable getAvailableHubBranchList(string hubId);
        DataTable getSelectedHubBranchList(string hubId);
        void DeleteNotSelected(string hubbranchid, string selectedbranches);
        bool CheckHubBranchExistInGroup(string hubbranchid, string selectedbranches);
        void InsertBranchInGroup(string hubbranchid, string selectedbranches);
        void UpdateSelectedBranch(string hubbranchid, string selectedbranches);
        void DeleteAllHubBranch(string hubbranchId);
        DataTable UpdateHubMaster(FormCollection col);
        DataTable getHubBranchList();
        void CreateHubMaster(FormCollection col);
        void DeleteHubMaster(string deleteMaster);
        void DeleteHubMasterBranches(string deleteBranch);
        List<string> Validate(FormCollection col);
    }
}