using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;

namespace INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ
{
    public interface IBankHostStatusKBZDao
    {

        string GetHostReturnReasonDesc(string statusId);
        //  DataTable GetBankHostStatusMaster(string fldBankHostStatusCode);
        bool DeleteInBankHostStatusMasterKBZ(string delete);
        bool UpdateHostStatusMasterKBZ(FormCollection col, AccountModel currentUser);
        bool AddBankHostCodeinKBZTemptoUpdate(FormCollection col, AccountModel currentUser);
        bool CreateBankHostStatusMasterKBZTemp(FormCollection col);
        bool CreateInBankHostStatusMasterKBZ(FormCollection col);
        List<BankHostStatusKBZModel> ListHostStatusAction();
        List<string> ValidateUpdate(FormCollection col);
        List<string> ValidateCreate(FormCollection col);
        bool AddtoBankHostStatusMasterKBZTempToDelete(string deleteId);
        void DeleteInBankHostStatusMasterTemp(string delete);
        BankHostStatusKBZModel GetHostReturnReasonModel(string statusId);
        BankHostStatusKBZModel GetBankHostStatusMasterKBZ(string fldBankHostStatusCode);
        bool CheckBankHostCodeDataTempKBZ(String fldBankHostStatusCode);
        bool CreateBankHostStatusCodeinMainKBZ(string fldBankHostStatusCode);
        BankHostStatusKBZModel GetBankHostStatusCodeData(string fldBankHostStatusCode);
        bool DeleteInBankHostStatusCodeKBZ(string fldBankHostStatusCode);
        bool UpdateBankHostStatusCodeToMainById(string fldBankHostStatusCode);
        bool DeleteBankHostStatusCodeinTempKBZ(string fldBankHostStatusCode);
        BankHostStatusKBZModel GetBankHostStatusMasterKBZTemp(string fldBankHostStatusCode);
        //BankHostReturnStatusKBZModel GetMABBankHostReturnStatus(string ItemId);
        List<BankHostReturnStatusKBZModel> GetMABBankHostReturnStatus(string ItemId);

    }
}