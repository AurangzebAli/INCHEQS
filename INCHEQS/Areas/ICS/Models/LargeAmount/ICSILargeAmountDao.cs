using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.LargeAmount
{
    public interface ICSILargeAmountDao
    {
        DataTable GetLargeAmountLimit();
        string SetLargeAmountLimit();
        void UpdateLargeAmountLimit(string Amount);
        void CreateLargeAmountLimit(string Amount, string strUpdate);
        List<string> Validate(FormCollection col);
        string GetPageTitle(string TaskId);
        bool NoChangesinLargeAmountLimit(FormCollection col, string Amount);
        bool CheckLargeAmountLimitinTempExist(string Amount);
        bool CheckLargeAmountLimitExist(string Amount);
        void InsertLargeAmountLimitTemp(string Amount, string strUpdate);
        LargeAmountModel GetLargeAmountLimit(string strBankCode);

        //Large Amount Limit Checker
        void UpdateLargeAmountLimitTemp(string strBankCode);
        void DeleteLargeAmountLimitTemp(string strBankCode);
        void UpdateLargeAmountLimitFromTemp(string strBankCode);
        void CreateLargeAmountLimitFromTemp(string strBankCode);
    }
}