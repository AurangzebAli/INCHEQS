using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.MicrRepair
{
    public interface IMicrRepairDao
    {
        //void CheckerConfirm(FormCollection col, AccountModel currentUser);
        //void CheckerRepair(FormCollection col, AccountModel currentUser);
        void Confirm(FormCollection col, AccountModel currentUser, string taskId);
        void Reject(FormCollection col, AccountModel currentUser, string taskId);
        List<string> ValidateMicr(FormCollection col, AccountModel currentUser);
        List<string> BranchCode(string BranchCode, string BankCode);



    }
}