using System;
using System.Collections.Generic;
using System.Web.Mvc;


namespace INCHEQS.Areas.ICS.Models.TransCode {
    public interface ITransCodeDao
    {
        /*DataTable getAllState();

        DataTable GetAllIssuingBankBranch(AccountModel currentUser);
        DataTable GetAllPresentingBankBranch();
        DataTable GetReturnReason();*/
        void CreateTransCodeTemp(FormCollection col, string currentUser, string status, string transCode);
        void CreateTransCode(FormCollection col, string currentUser);
        void UpdateTransCode(FormCollection col, string currentUser);
        void DeleteTransCode(string transCode);
        void DeleteTransCodeTemp(string transCode);
        void MoveTransCodeFromTemp(string transCode, string status);
        List<TransCodeModel> GetTransCode(string transCode);
        List<TransCodeModel> GetTransCodeTemp(string transCode);
        bool CheckTransCodeExist(string transCode);
        bool CheckTransCodeTempExist(string transCode);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);

        void InsertToDataProcessICS(string bankCode, string processName, string posPayType, string clearingDate, string reUpload, string taskId, string batchId, string crtuserId, string upduserId, string fileName = "");
    }
}