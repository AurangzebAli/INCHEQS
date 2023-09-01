using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.HostReturnReason {
    public interface IHostReturnReasonDao {

        string GetHostReturnReasonDesc(string statusId);
        DataTable GetHostReturnReason(string statusId);
        void DeleteInBankHostStatusMaster(string delete);
        void UpdateHostReturnReason(FormCollection col, AccountModel currentUser);
        void CreateHostReturnReasonTemp(FormCollection col, string autoPending, string autoReject, AccountModel currentUser);
        void CreateInBankHostStatusMaster(string id);
        List<string> ValidateUpdate(FormCollection col);
        List<string> ValidateCreate(FormCollection col);
        void AddtoBankHostStatusMasterTempToDelete(string deleteId);
        void DeleteInBankHostStatusMasterTemp(string delete);
        HostReturnReasonModel GetHostReturnReasonModel(string statusId);

    }
}