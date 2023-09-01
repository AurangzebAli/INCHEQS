using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using INCHEQS.Security.Account;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.Capturing
{
    public interface ICapturingDao
    {
        CapturingModel GetCaptureData();

        DataTable GetCapturingModeDataTable();

        DataTable GetCapturingTypeDataTable(string strModeId);

        DataTable GetCapturingRelationshipDataTable();

        DataTable GetCapturingModeDetailsDataTable(string strModeId);

        DataTable GetCheckTypeDetailsDataTable(string strTypeId);

        DataTable GetWorkstationScannerDataTable(string strMACAddress, string strUserId);
        DataTable GetCaptureDate();

        DataTable getCapturePageInfo(AccountModel currentUser);
        List<string> ValidateMacAdress(List<string> lstMacAddress, FormCollection col);
        List<string> GetMacAddress(AccountModel currentUser);
        DataTable GetScannerErrorDataTable(string strScannerTypeId);

        string GetProcessDate();

        DataTable GetCurrencyDataTable(string strCurrencyId);

        DataTable GetCompleteSeqNoDataTable(string strBankCode);

        DataTable GetUICInfoDataTable(string strScannerId, string strBranchId);

        DataTable GetScannerTuningDataTable(string strScannerTypeId);

        DataTable GetBranchEndOfDayDataTable(string strProcessDate, string strBranchId, string strBankCode);

        DataTable GetCenterEndOfDayDataTable(string strProcessDate, string strBankCode);

        string FormatProcessDate(int intDay, int intMonth, int intYear);

        bool UpdateUICInfo(string strScannerId, string strBatchNo, string strSeqNo, string strUserId, string strClearingBranch);
        DataTable GetCapturingInfo(string strUserId);
        DataTable GetPostingModeInfo();
        //bool UpdateUICInfoIncSequence(string strScannerId, string strBranchId, int intBatchNo, int intSeqNo, string strUserId);
        bool CheckBank(string bankCode);
        List<CapturingModel> GetCapturingBranchesInfo(string UserID);
        DataTable GetPostingModeDetailsDataTable(string strModeId);
        DataTable GetSelfClearingBranchId(string BranchId);
    }
}