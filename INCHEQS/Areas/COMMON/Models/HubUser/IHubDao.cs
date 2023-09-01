using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.HubUser
{
    public interface IHubDao
    {
        bool CheckUserExistInHub(string hubId, string userId, string strBankCode);

        bool DeleteInHubUserMaster(string HubId, string Action);
        bool DeleteInHubMasterTemp(string HubId);
        bool DeleteInHubUserTemp(string HubId);
        bool DeleteUserNotSelected(string hubId, string userIds, string strBankCode);

        void InsertUserInHubTemp(string hubCode, string userId, string strUpdate, string strUpdateId, string strBankCode, string strAction);
        void InsertUserInHub(string hubCode, string userId, string strUpdate, string strUpdateId, string strBankCode, string Action);

        List<UserModel> ListAvailableUserInHub(string strBankCode);
        List<UserModel> ListSelectedUserInHub(string hubId, string strBankCode);
        List<UserModel> ListSelectedUserInHubTempChecker(string hubCode, string strBankCode);
        bool UpdateHubMaster(FormCollection col, string userId);
        void UpdateHubUser(string hubId);
        bool UpdateSelectedUser(string hubId, string UserId, string strUpdate, string strBankCode);
        string GetMenuTitle(string TaskId);
        List<string> ValidateHubUser(FormCollection col, string action, string userId);
        HubModel CheckHubMasterByID(string HubID, string SearchFor);
        bool CheckHubMasterTempByID(string HubID, string SearchFor);
        void CreateHubMasterTemp(FormCollection col, string bankcode, string crtUser, string Action);
        void MoveToHubMasterFromTemp(string HubId, string Action);
        HubModel CheckHubMasterTempCheckerByID(string HubCode, string SearchFor);

        List<string> ListSelectedUserInHubTemp(string hubId, string strBankCode);
        List<UserModel> ListAvailableUserInHubChecker(string strBankCode);
        bool CheckHubUserExistInTemp(string hubcode, string userId, string Action);
    }
}