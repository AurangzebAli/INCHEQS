using INCHEQS.Security.Account;
using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.CommonOutwardItem
{
    public interface ICommonOutwardItemDao {
        void LockThisCheque(string inwardItemId, AccountModel currentUser);
        void LockThisChequebyTransNumber(string TransNumber, AccountModel currentUser);
        void LockThisChequeHistory(string inwardItemId, AccountModel currentUser);
        Dictionary<string, string> LockAllCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> FindItemByOutwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        //int ConfirmVerify(FormCollection collection, string userId);
        Dictionary<string, string> FindItemByOutwardItemIdLocked(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);

        Dictionary<string, string> NextCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> PrevCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> ErrorCheque(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> ErrorChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        int UnlockAllAssignedForUser(AccountModel currentUser);
        int UnlockAllAssignedForBranchUser(AccountModel currentUser);
        int CheckMainTable(AccountModel currentUser,string date);
        int UnlockAllAssignedForUserHistory(AccountModel currentUser);
        string DeleteTempGif(string imageId);
        DataTable ChequeHistory(FormCollection collection);
        DataTable ChequeHistoryH(FormCollection collection);
        bool CheckIfRecordUpdatedOrDeleted(string inwardItemId, string updateTimestamp);
        bool CheckLockedCheck(string inwarditemId,string assignuser); //add by shamil to validate check is lock by other user march 29 2017
        bool CheckOldValueRejectReentry(string inwardItemId, string fieldName);
        string GetModifiedField(FormCollection collection);
        string GetBranchItemStatus(FormCollection collection);
        int GetNextVerifySeqNo(string inwardItemId);
        Dictionary<string, string> FirstChequeFromTheResult(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        void InsertChequeHistory(FormCollection col, string verifiyAction, AccountModel currentUser, string taskId);
        void InsertChequeHistoryForApproveOrRejectAll(string verifyAction, AccountModel currentUser, string taskId, string rejectCode = "");
        string GetRejectCodeByRejectDesc(string rejectDesc);
        Dictionary<string, string> GetStrListVirtualAcctNumberByItemInitialID(string InitialId);
        int CheckMainTableOutward(AccountModel currentUser, string inwarditemid);
        Dictionary<string, string> FindOutwardItemId(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindOutwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = null);
        Dictionary<string, string> FindItemByOutwardItemIdNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        Dictionary<string, string> FindItemByOutwardItemIdPrev(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, string inwardItemId = null, string nextOrPrevious = "Next");
        Dictionary<string, string> PrevChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> NextChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser);
        Dictionary<string, string> LockAllChequeNew(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        Dictionary<string, string> LockAllChequeBranch(QueueSqlConfig pageConfig, FormCollection collection, AccountModel currentUser, int numberOfItem);
        bool CheckStatus(string inwardItemId, AccountModel currentUser);
        bool CheckStatusBalancing(string inwardItemId, AccountModel currentUser);
        bool CheckStatusBranch(string inwardItemId, AccountModel currentUser);
        List<ChequeRepairHistoryModel> GetChequeHistory(string itemid);
        Task<List<ChequeRepairHistoryModel>> GetChequeHistoryAsync(string itemid);
        List<ChequeRepairHistorySearchModel> GetChequeHistorySearch(string itemid);
        Task<List<ChequeRepairHistorySearchModel>> GetChequeHistoryAsyncSearch(string itemid);
        Task<List<BalancingHistoryModel>> GetBalancingHistoryAsync(string itemid);

        Dictionary<string, string> GetBalancingItemsAsync(string itemid);
        Task<List<SearchHistoryModel>> GetSearchHistoryAsync(string itemid);
        void AddOutwardItemHistory(string fldActionStatus, string fldTransNo, string fldRemarks, string fldQueue, AccountModel currentUser);
        //Dictionary<string, string> FindOutwardItemId(QueueSqlConfig gQueueSqlConfig, FormCollection collection, AccountModel account, object inwardItemId, object p);
    }
}