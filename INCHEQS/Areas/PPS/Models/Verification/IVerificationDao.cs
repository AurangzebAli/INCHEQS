using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.PPS.Models.Verification
{
    public interface IVerificationDao
    {

        VerificationModel getChequeInfo(string inwardItemId,string uic);
        //List<VerificationModel> FindPayee(FormCollection col, string bankCode);
        Task<List<VerificationModel>> FindPayeeAsync(FormCollection col, string bankCode);
        List<VerificationModel> FindPayee(FormCollection col, string bankCode);

        void VerificationApprove(FormCollection col, AccountModel currentUser, string taskRole);
        void VerificationApproveNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        List<string> ValidateVerification(FormCollection col, AccountModel currentUser, string verifyAction, string taskid);

        void VerificationReturn(FormCollection col, AccountModel currentUser, string taskRole);
        void VerificationReturnNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationRoute(FormCollection col, AccountModel currentUser);
        void VerificationRouteNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction);
        void VerificationPullOut(FormCollection col, AccountModel currentUser);
        //void VerificationPullOutNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig);
        void InsertPullOutInfo(FormCollection collection, AccountModel currentUser);

        List<string> ValidateBranch(FormCollection col, AccountModel currentUser, string verifyAction);
        List<string> LockedCheck(FormCollection col, AccountModel currentUser);
        Boolean VerifyClassLimit(FormCollection col, AccountModel currentUser);
        void BranchApproveNew(FormCollection col, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean VerificationLimit);
        void BranchReturnNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean verificationlimit);

        double getVerificationAmountLimit(string status);

        List<VerificationModel> MatchingDetails(string inwardItemId, string matchingComponent);

    }
}