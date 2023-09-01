using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;

namespace INCHEQS.Areas.COMMON.Models.BankHostStatus
{
    public interface IBankHostStatusDao
    {

        string GetHostReturnReasonDesc(string statusId);
        //  DataTable GetBankHostStatusMaster(string fldBankHostStatusCode);
        bool DeleteInBankHostStatusMaster(string delete);
        bool UpdateHostStatusMaster(FormCollection col, AccountModel currentUser);
        bool AddBankHostCodeinTemptoUpdate(FormCollection col, AccountModel currentUser);
        bool CreateBankHostStatusMasterTemp(FormCollection col);
        bool CreateInBankHostStatusMaster(FormCollection col);
        List<BankHostStatusModel> ListHostStatusAction();
        List<BankHostStatusModel> ListHostRejectCode();
        DataTable GetAllHostStatus();
        List<string> ValidateUpdate(FormCollection col);
        List<string> ValidateCreate(FormCollection col);
        bool AddtoBankHostStatusMasterTempToDelete(string deleteId);
        void DeleteInBankHostStatusMasterTemp(string delete);
        BankHostStatusModel GetHostReturnReasonModel(string statusId);
        BankHostStatusModel GetBankHostStatusMaster(string fldBankHostStatusCode);
        bool CheckBankHostCodeDataTemp(String fldBankHostStatusCode);
        bool CreateBankHostStatusCodeinMain(string fldBankHostStatusCode);
        BankHostStatusModel GetBankHostStatusCodeData(string fldBankHostStatusCode);
        bool DeleteInBankHostStatusCode(string fldBankHostStatusCode);
        bool UpdateBankHostStatusCodeToMainById(string fldBankHostStatusCode);
        bool DeleteBankHostStatusCodeinTemp(string fldBankHostStatusCode);
        BankHostStatusModel GetBankHostStatusMasterTemp(string fldBankHostStatusCode);

    }
}