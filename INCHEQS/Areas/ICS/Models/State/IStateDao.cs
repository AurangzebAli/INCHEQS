using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.State {
    public interface IStateDao {
        DataTable getAllState();

        DataTable GetAllIssuingBankBranch(AccountModel currentUser);
        DataTable GetAllPresentingBankBranch();
        DataTable GetReturnReason();
        DataTable GetIssueBankBranch(string bankCode);
    }
}