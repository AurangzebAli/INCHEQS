using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.Users
{
    public interface IUserDao
    {
        bool CreateUserMaster(string userAbb);
        bool DeleteUserMasterTemp(string userAbb);
        bool DeleteUserMaster(string userAbb);
        bool UpdateUserMaster(FormCollection col, string updUser);
        bool UpdateUserMasterFromTemp(string userAbb);
        void CreateUserMasterTemp(FormCollection col, string bankcode, string crtUser, string Action);
        bool CheckUserMasterTempByID(string userId, string userAbb, string SearchFor);
        bool CheckUserPasswordHistory(string encryptedPassword, string action);
        UserModel CheckUserMasterByID(string UserAbb, string UserID , string SearchFor);
        UserModel CheckUserMasterInTempByID(string UserAbb, string UserID, string SearchFor);
        List<UserModel> ListBranch(string userId);
        List<UserModel> ListVerificationClass(string userId);
        List<string> ValidateUser(FormCollection col, string action, string userId);
        void MoveToUserMasterFromTemp(string userAbb, string Action);
        void ApproveInChecker(string userAbb, string Action);
        void RejectInChecker(string userAbb, string Action);
    }
}
