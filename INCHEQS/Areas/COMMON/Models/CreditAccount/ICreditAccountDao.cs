using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.CreditAccount
{
    public interface ICreditAccountDao
    {
        CreditAccountModel GetAccountNumber(string AccountId);
        bool CreateAccountNumberMaster(FormCollection col);
        bool DeleteCreditAccountMasterTemp(string Accountid);
        List<CreditAccountModel> ListBranch(string Type);
        List<CreditAccountModel> ListStateCode(string StateCode);
        bool UpdateCreditAccountMaster(FormCollection col);
    }
}