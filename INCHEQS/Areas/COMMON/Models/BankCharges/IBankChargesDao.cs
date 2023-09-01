using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.BankCharges
{
    public interface IBankChargesDao
    {
        List<BankChargesModel> GetBankChargesTypeAndProductCodeList(string tablename);
        string GetBankChargesDesc(string type);
        void CreateBankChargesTemp(FormCollection col, string bankcode, string crtUser, string Action, string productcode, string maxAmount, string minAmount, string bankchargestype);
        bool CreateBankCharges(FormCollection col, string bankcode);
        List<string> ValidateBankCharges(FormCollection col, string action);
        bool DeleteBankCharges(string productCode, string bankcode, string bankchargestype, string minAmount, string maxAmount);
        bool CheckBankChargesTempById(string productCode, string bankcode, string bankchargestype, string minAmount, string maxAmount);
        bool UpdateBankCharges(FormCollection col);
        void MoveToBankChargesFromTemp(string productCode, string bankCode, string bankcharges, string minAmount, string maxAmount, string Action);
        bool DeleteBankChargesTemp(string productCode, string bankcode, string bankcharges, string minAmount, string maxAmount);
        bool ValidateExistingBankCharges(string productCode, string bankcode, string bankcharges, string minAmount, string maxAmount, string status, string tbltype);
        BankChargesModel GetBankCharges(string bankCode, string productCode, string bankChargesType, string minAmount, string maxAmount, string tblname);

        /*
        BankChargesModel GetBankChargesTypeTemp(string bankchargestype);
        */

    }
}
