using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace INCHEQS.Areas.ICS.Models.HighRiskAccount {
    public interface IHighRiskAccountDao {
        HighRiskAccountModel GetHighRiskAccount(string highRiskAccount);
        HighRiskAccountModel GetHighRiskAccountTemp(string highRiskAccount);
        DataTable GetInternalBranchCode();
        List<string> ValidateCreate(FormCollection col);
        void CreateHighRiskAccount(FormCollection col, AccountModel currentUser);
        void CreateHighRiskAccountTemp(FormCollection col, AccountModel currentUser, string status, string transCode);
        List<string> ValidateUpdate(FormCollection col);
        void DeleteFromHighRiskAccount(string highRiskAccount);
        void UpdateHighRiskAccount(FormCollection col);

        void DeleteHighRiskAccoountTemp(string highRiskAccount);

        void MoveHighRiskAccoountFromTemp(string highRiskAccount, string status);

        bool CheckHighRiskAccountTempExist(string highRiskAccount);
    }
}