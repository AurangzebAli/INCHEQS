using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.BranchSubmission
{
    public interface IBranchSubmissionDao
    {
        DataTable GetHubBranches(string userId);
        DataTable GetItemReadyForSubmission(string userId);
        DataTable GetItemSubmitted(string strBranchCode, string strBankCode);
        DataTable GetBranchEndOfDay(string strBranchCode, string strBankCode);
        DataTable DataEntryPendingItem(string strBranchCode, string strBankCode);
        DataTable AuthorizationPendingItem(string strBranchCode, string strBankCode);
        DataTable ReadyforBranchSubmission(FormCollection collection);
        DataTable BranchSubmittedItems(FormCollection collection);
        bool BranchSubmission(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber, string userid);
        List<BranchSubmissionItemList> GetItemReadyForSubmissionList(string userId); 
        List<BranchSubmittedItemList> GetItemReadyForSubmittedList(string userId);
        DataTable ReturnItemDetails(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber, string userid); 
        DataTable ReturnSubmittedDetails(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber, string userid);
        string getBetween(string strSource, string strStart, string strEnd);
        void UpdateBranchItem(FormCollection collection, AccountModel currentUser, string capturebranch, string scannerid, string id, string capturedate);
    }
}
