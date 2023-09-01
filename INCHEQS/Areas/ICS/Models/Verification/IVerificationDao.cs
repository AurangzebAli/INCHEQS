using INCHEQS.Security.Account;
using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.PPS.Models.Verification;
using INCHEQS.Areas.ICS.ViewModels;

namespace INCHEQS.Models.Verification {
    public interface IVerificationDao {
        void VerificationApproveAll(AccountModel currentUser);
        void VerificationReturnAll(AccountModel currentUser);
        void VerificationApprove(FormCollection col, AccountModel currentUser, string taskRole);
        string VerificationApproveNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationReturn(FormCollection col, AccountModel currentUser, string taskRole);
        void VerificationReturnNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationRoute(FormCollection col, AccountModel currentUser);
        void VerificationRouteNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationPullOut(FormCollection col, AccountModel currentUser);
        void VerificationRepair(FormCollection col, AccountModel currentUser);
        void VerificationRepairNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationReview(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void BranchApprove(FormCollection col, AccountModel currentUser, string taskRole);
        void BranchApproveNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean VerificationLimit);
        void BranchReturn(FormCollection col, AccountModel currentUser, string taskRole);
        void BranchReturnNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean verificationlimit);
        void BranchReferBack(FormCollection col, AccountModel currentUser);
        void BranchReferBackNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean VerificationLimit);
        void BranchConfirmation(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        Boolean VerifyClassLimit(FormCollection col, AccountModel currentUser);
        List<string> VerificationCondition(string Userid, string Taskid);
        List<string> ValidateVerification(FormCollection col, AccountModel currentUser, string verifyAction, string taskid);
        List<string> ValidateVerificationService(FormCollection col, AccountModel currentUser, string verifyAction, string taskid,string message);

        List<string> ValidateBranch(FormCollection col, AccountModel currentUser, string verifyAction);
        List<string> LockedCheck(FormCollection col, AccountModel currentUser);

        void VerificationPullOutNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void InsertPullOutInfo(FormCollection collection, AccountModel currentUser);
        void InsertHostValidation(InwardItemViewModel inwardItemViewModel, AccountModel currentUser);
        string CheckerConfirmNew(FormCollection col, AccountModel currentUser, string taskId, string Message);
        string CreditPostingMarked(string inwardItemId, FormCollection collection);

    }
}