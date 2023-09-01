using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Areas.COMMON.Models.Users;
using INCHEQS.Areas.COMMON.Models.Branch;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates
{
    public interface ISecurityAuditLogDao
    {
        // Clear User Session
        string UserSession_Template(string TableHeader, string user);

        // Change User Password         
        string ChangePassword_Template(ChangePasswordModel before, ChangePasswordModel after, string TableHeader, FormCollection User);
        ChangePasswordModel CheckUserMasterByID(string userId, string userAbb, string SearchFor);
        string decryptPassword(string encryptedPassword);

        // Security Profile
        string SecurityProfile_EditTemplate(SecurityProfileModel before, SecurityProfileModel after, string TableHeader, FormCollection Security);

        // Task Assignment
        string TaskAssignment_EditTemplate(string beforeTask2, string afterTask2, string TableHeader, string Action, FormCollection col);
        string TaskAssignmentChecker_EditTemplate(string beforeTask, string afterTask, string TableHeader, string Action, string id);

        //Hub User
        string HubUser_AddTemplate(FormCollection col, string afterUser2, string TableHeader);
        string HubUser_EditTemplate(HubModel before, HubModel after, string beforeUser2, string afterUser2, string TableHeader, FormCollection col);
        string HubUser_DeleteTemp(string beforeUser2, string TableHeader, string id);
        string HubUserChecker_AddTemplate(string afterUser2, string TableHeader, string Action, string id);
        string HubUserChecker_EditTemplate(HubModel before, HubModel after, string beforeUser2, string afterUser2, string TableHeader, string Action, string id);
        string HubUserChecker_DeleteTemplate(string beforeUser2, string TableHeader, string Action, string id);
        HubModel CheckHubMasterByIDTemp_Security(string HubCode, string SearchFor);

        //Hub Branch
        string HubBranch_EditTemplate(string beforeBranch1, string afterBranch1, string TableHeader, string Action, FormCollection col);
        string HubBranchChecker_EditTemplate(string beforeBranch1, string afterBranch1, string TableHeader, string Action, string id);
        List<BranchModel> ListSelectedBranchInHubTemp_Security(string hubCode, string strBankCode);

        //Group Profile
        string GroupProfile_AddTemplate(FormCollection col, string afterUser, string TableHeader, string Action);
        string GroupProfile_EditTemplate(GroupProfileModel before, GroupProfileModel after, string beforeUser2, string alreadyselecteduser2, string TableHeader, string Action, FormCollection col);
        string GroupProfile_DeleteTemplate(string afterUser, string TableHeader, string Action, string col);
        string GroupProfileChecker_AddTemplate(string afterUser, string TableHeader, string Action, string id);
        string GroupProfileChecker_DeleteTemplate(string afterUser, string TableHeader, string Action, string id);
        string GroupProfileChecker_EditTemplate(GroupProfileModel before, GroupProfileModel after, string beforeUser2, string alreadyselecteduser2, string TableHeader, string Action, string id);
        GroupProfileModel CheckGroupMasterUserID(string groupId);
        GroupProfileModel CheckGroupMasterUserIDTemp(string groupId);

        //User Profile
        string UserProfile_AddTemplate(FormCollection Newobj, FormCollection OldObj, string TableHeader, string Action);
        string UserProfile_EditTemplate(UserModel before, UserModel after, string TableHeader, FormCollection User);
        string UserProfile_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User);
        string UserProfileCheckerApp_AddTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User);
        string UserProfileCheckerApp_EditTemplate(UserModel before, UserModel after, string TableHeader, string User);
        string UserProfileCheckerApp_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User);
        string UserProfileCheckerRej_AddTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User);
        string UserProfileCheckerRej_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User);
        string UserProfileCheckerRej_EditTemplate(UserModel before, UserModel after, string TableHeader, string User);
        UserModel CheckUserMasterByTempID(string UserAbb, string UserID, string SearchFor);
    }
}
