using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ChequeAccountEntry
{
    public interface IChequeAccountEntryDao
    {
        void Confirm(FormCollection col, AccountModel currentUser, string taskId);
        void Reject(FormCollection col, AccountModel currentUser, string taskId);
        List<string> Validate(FormCollection col, AccountModel currentUser);
        bool CheckAccountExist(string strAccountNumber);

    }
}