using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.Hub
{
    public interface IHubDao
    {
        void AddHubToHubMasterTempToDelete(string id);
        void AddHubToHubMasterTempToUpdate(FormCollection col, string strUpdate, string strBankCode);
        void AddUserToHubUserTempToUpdate(string userId, string hubId, string strUpdate, string strBankCode);
        bool CheckHubExist(string hubId, string strBankCode);
        bool CheckPendingApproval(string hubId, string strBankCode);
        bool CheckUserExistInHub(string hubId, string userId);
        bool CheckUserExistInHub(string hubId, string userId, string strBankCode);
        void CreateHubMaster(FormCollection col, string strBankCode, string strUpdate, string strUpdateId);
        void CreateHubMasterTemp(FormCollection col, string strUpdate, string strBankCode, string strUpdateId);
        void CreateInHubMaster(string HubId);
        void DeleteAllUserInHub(string hubId);
        void DeleteAllUserInHub(string hubId, string strBankCode);
        void DeleteAllUserInHubTemp(string hubId);
        void DeleteHub(string hubId);
        void DeleteHub(string hubId, string strBankCode);
        void DeleteInHubMaster(string Hub);
        void DeleteInHubMasterTemp(string Hub);
        void DeleteUserNotSelected(string hubId, string userIds);
        void DeleteUserNotSelected(string hubId, string userIds, string strBankCode);
        HubModel GetHub(string hubId);
        HubModel GetHub(string hubId, string BankCode);
        string GetMaintenanceChecker();
        void InsertUserInHub(string hubId, string userId, string strUpdate, string strUpdateId);
        void InsertUserInHub(string hubId, string userId, string strUpdate, string strUpdateId, string strBankCode);
        List<UserModel> ListAvailableUserInHub(string strBankCode);
        List<UserModel> ListSelectedUserInHub(string hubId, string strBankCode);
        void UpdateHubMaster(string hubId);
        void UpdateHubUser(string hubId);
        void UpdateSelectedUser(string hubId, string UserId, string strUpdate);
        void UpdateSelectedUser(string hubId, string UserId, string strUpdate, string strBankCode);
        List<string> ValidateCreate(FormCollection col, string strBankCode);
        List<string> ValidateUpdate(FormCollection col, string strBankCode);
    }
}