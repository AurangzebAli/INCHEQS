using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.Balancing
{
    public interface ITransactionBalancingDao
    {
        void Confirm(FormCollection col, AccountModel currentUser, string taskId);
        void DoBalancing(FormCollection col, AccountModel currentUser, string taskId);
        void Reject(FormCollection col, AccountModel currentUser, string taskId);
        List<string> Validate(FormCollection col, AccountModel currentUser);
        List<string> BranchCode(string BranchCode, string BankCode);
        void UpdateBalancingAmount(FormCollection col, AccountModel currentUser, string[] arr);
        void updateBalancingStatus(FormCollection col, AccountModel currentUser);
        DataTable ListAll();
        DataTable AIFMasterList(string AccNumber);
        List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm);

        DataTable CheckIndividualItemDetail(string ItemID);
    }
}
