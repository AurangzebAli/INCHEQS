using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ChequeAmountEntry
{
    public interface IChequeAmountEntryDao
	{
        //void CheckerConfirm(FormCollection col, AccountModel currentUser);
        //void CheckerRepair(FormCollection col, AccountModel currentUser);
        void Confirm(FormCollection col, AccountModel currentUser, string taskId);
        void Reject(FormCollection col, AccountModel currentUser, string taskId);
        List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm);
        List<string> BranchCode(string BranchCode, string BankCode);



    }
}