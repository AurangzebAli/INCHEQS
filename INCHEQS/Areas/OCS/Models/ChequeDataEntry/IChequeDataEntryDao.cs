using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ChequeDataEntry
{
    public interface IChequeDataEntryDao
    {
        void Confirm(FormCollection col, AccountModel currentUser, string taskId);
        void PreAccountEntry(FormCollection col, AccountModel currentUser, string taskid);
        void Reject(FormCollection col, AccountModel currentUser, string taskId);
        List<string> Validate(FormCollection col, AccountModel currentUser);
        List<string> BranchCode(string BranchCode, string BankCode);
        void UpdateDataEntryAmountNAccount(FormCollection col, AccountModel currentUser);
        List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm);
        DataTable ListAll();

        ////For Uat test
        //DataTable AIFMasterList(string AccNumber);

        //(GetOracleAccountInformation-MAB-MGR)
        DataTable GetOracleAccountInformation(string AccNumber);

        bool UpdateEntry(Int64 bintItemID, string strVAcctNo, string strDepAmount, string strChqAmount, Int64 bintOriginItemID, string strOriginVAcctNo, string strOriginDepAmount, string strOriginChqAmount, string isConflict, AccountModel currentUser, FormCollection col,string taskid,string strPayeeName, string strPayeeAccStatus);
        bool CheckBranch(string BranchCode);
    }
}
