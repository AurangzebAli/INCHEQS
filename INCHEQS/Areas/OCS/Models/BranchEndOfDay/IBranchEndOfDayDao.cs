﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace INCHEQS.Areas.OCS.Models.BranchEndOfDay
{
    public interface IBranchEndOfDayDao
    {
        DataTable GetHubBranches(string userId);
        DataTable GetItemReadyForSubmission(string strBranchCode, string strBankCode);
        DataTable GetItemSubmitted(string strBranchCode, string strBankCode);
        DataTable GetHubItemReadyForSubmission(string userId, string strBankCode);
        DataTable GetHubItemSubmitted(string userId, string strBankCode);
        DataTable GetBranchEndOfDay(string strBranchCode, string strBankCode);
        DataTable GetAllUserBranchEndOfDay(string strUserId, string strBankCode);
        DataTable DataEntryPendingItem(string strBranchCode, string strBankCode);
        DataTable AuthorizationPendingItem(string strBranchCode, string strBankCode);
        bool InsertBranchEndOfDay(string strBankCode, string strBranchId, string strAmount, string strDifference, string strEODStatus);
        bool DeleteBranchEndOfDay(string strBankCode, string strBranchId, string strEODStatus);
        string GetProcessDate(string bankcode);
        string encryptPassword(string stringPassword);
        DataTable ValidateUserLogin(string strusername, string strpassword, string strBankCode);
        List<string> GetHubBranchList(string userId);

        bool InsertHubBranchEndOfDay(string strBranchId, string strEODStatus, string strBankCode);

    }
}

