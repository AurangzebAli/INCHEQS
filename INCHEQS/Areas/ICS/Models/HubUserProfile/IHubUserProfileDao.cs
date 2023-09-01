using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.HubUserProfile {
    public interface IHubUserProfileDao {

        DataTable ListAllHubUser();
        DataTable getHubUser(string hubId);
        DataTable getAvailableHubUserList();
        DataTable getSelectedHubUserList(string hubId);
        DataTable UpdateHubMaster(FormCollection col);
        void DeleteNotSelected(string hubbranchid, string selectedbranches);
        void UpdateSelectedUser(string hubbranchid, string selectedbranches);
        bool CheckHubUserExistInGroup(string hubbranchid, string selectedbranches);
        void InsertUserInGroup(string hubbranchid, string selectedbranches);
        void DeleteAllHubUser(string hubbranchId);
        void DeleteHubMasterUsers(string deleteBranch);
        void DeleteHubMaster(string deleteMaster);
        List<string> Validate(FormCollection col);
    }
}