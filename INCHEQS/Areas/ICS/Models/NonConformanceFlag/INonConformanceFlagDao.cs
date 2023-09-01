using System;
using System.Collections.Generic;
using System.Web.Mvc;


namespace INCHEQS.Areas.ICS.Models.NonConformanceFlag {
    public interface INonConformanceFlagDao
    {
        /*DataTable getAllState();

        DataTable GetAllIssuingBankBranch(AccountModel currentUser);
        DataTable GetAllPresentingBankBranch();
        DataTable GetReturnReason();*/
        void CreateNCFCodeTemp(FormCollection col, string currentUser, string status, string ncfCode);
        void CreateNCFCode(FormCollection col, string currentUser);
        void UpdateNCFCode(FormCollection col, string currentUser);
        void DeleteNCFCode(string ncfCode);
        void DeleteNCFCodeTemp(string ncfCode);
        void MoveNCFCodeFromTemp(string ncfCode, string status);
        List<NonConformanceFlagModel> GetNCFCode(string ncfCode);
        List<NonConformanceFlagModel> GetNCFCodeTemp(string ncfCode);
        bool CheckNCFCodeExist(string ncfCode);
        bool CheckNCFCodeTempExist(string ncfCode);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);
    }
}