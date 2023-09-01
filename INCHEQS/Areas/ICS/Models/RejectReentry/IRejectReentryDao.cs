using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.RejectReentry {
    public interface IRejectReentryDao {
        void CheckerConfirm(FormCollection col, AccountModel currentUser);
        string CheckerConfirmNew(FormCollection col, AccountModel currentUser, string taskId,string Message);
        void CheckerRepair(FormCollection col, AccountModel currentUser);
        void MakerConfirm(FormCollection col, AccountModel currentUser, string taskId);
        List<string> Validate(FormCollection col, AccountModel currentUser);
		List<string> VerificationBranchCode(string BranchCode, string BankCode);


    }
}