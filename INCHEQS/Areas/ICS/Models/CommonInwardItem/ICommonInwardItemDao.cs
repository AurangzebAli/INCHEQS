using INCHEQS.Security.Account;
using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Security;

namespace INCHEQS.Models.CommonInwardItem {
    public interface ICommonInwardItemDao {
        void LockThisCheque(string inwardItemId, AccountModel currentUser);
        void LockThisChequeHistory(string inwardItemId, AccountModel currentUser);
        Dictionary<string, string> LockAllCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> FindItemByInwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindItemByInwardItemIdBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        Dictionary<string, string> FindItemByInwardItemIdNOLOCK(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        Dictionary<string, string> FindItemByInwardItemIdNOLOCKNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindItemByInwardItemIdBranchNOLOCKNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        //int ConfirmVerify(FormCollection collection, string userId);
        Dictionary<string, string> NextCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> NextChequeNoLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> NextChequeNoLockBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> PrevChequeNoLockBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> PrevCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> PrevChequeNoLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> ErrorCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> ErrorChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> ErrorChequeWithoutLock(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        int UnlockAllAssignedForUser(AccountModel currentUser);
        int UnlockAllAssignedForBranchUser(AccountModel currentUser);
        int CheckMainTable(AccountModel currentUser,string date);
        int CheckUserType(AccountModel currentUser);
        int UnlockAllAssignedForUserHistory(AccountModel currentUser);
        string DeleteTempGif(string imageId);
        DataTable ChequeHistory(string inwardItemId);
        DataTable ChequeHistoryH(string inwardItemId);
        //bool CheckIfRecordUpdatedOrDeleted(string inwardItemId, string updateTimestamp);
        string CheckIfRecordUpdatedorDeletedForListing(string inwardItemId);
        bool CheckIfRecordUpdatedOrDeleted(string inwardItemId);
        bool CheckIfUPIGenerated(string inwardItemId);
        bool CheckIfRecordUpdatedOrDeletedBranch(string inwardItemId);
        bool CheckLockedCheck(string inwarditemId,string assignuser); //add by shamil to validate check is lock by other user march 29 2017
        bool CheckLockedCheck2(string inwarditemId, string assignuser); //add by azim to validate check is lock by other user december 06 2021
        bool CheckOldValueRejectReentry(string inwardItemId, string fieldName);
        string GetModifiedField(FormCollection collection);
        string GetBranchItemStatus(FormCollection collection);
        int GetNextVerifySeqNo(string inwardItemId);
        Dictionary<string, string> FirstChequeFromTheResult(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        void InsertChequeHistory(FormCollection col, string verifiyAction, AccountModel currentUser, string taskId);
        void InsertChequeHistoryForApproveOrRejectAll(string verifyAction, AccountModel currentUser, string taskId, string rejectCode = "");
        string GetRejectCodeByRejectDesc(string rejectDesc);
        int CheckMainTableInward(AccountModel currentUser, string inwarditemid);
        Dictionary<string, string> FindInwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindInwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindItemByInwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        Dictionary<string, string> PrevChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> NextChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> LockAllChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> LockAllChequeBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> LockChequeBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> LockChequeVerification(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        bool CheckStatus(string inwardItemId, AccountModel currentUser);
        bool CheckStatusBranch(string inwardItemId, AccountModel currentUser);
        void LockThisChequeBranch(string inwardItemId, AccountModel currentUser);
        DataTable SubmissionTime(string spickC);
        DataTable GetTruncatedAmount();
        DataTable GetMicr();
        DataTable IQAResult(string inwardItemId);
        DataTable RouteReason();
        DataTable ManuallyUpdateCheques(string inwardItemId, AccountModel currentUser,FormCollection col,string accNo,string cheqNo, string Taskid);
        DataTable ManuallyUpdateReturnedCheques(FormCollection collection);
        DataTable RouteToAuthorizer(string inwardItemId, AccountModel currentUser, FormCollection col, string accNo, string cheqNo, string Taskid);

        
        

    }
}