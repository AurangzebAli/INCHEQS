using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.BankChargesType
{
    public interface IBankChargesTypeDao
    {
        
        BankChargesTypeModel GetBankChargesType(string bankchargestype);
        List<string> ValidateBankChargesType(FormCollection col, string action, string bankcode);
        bool UpdateBankChargesTypeMaster(FormCollection col);
        void CreateBankChargesTypeMasterTemp(FormCollection col, string bankcode, string crtUser, string Action);
        bool DeleteBankChargesTypeMaster(string bankchargestype, string bankcode);
        bool CheckBankChargesTypeMasterTempById(string bankchargestype, string bankcode);
        bool CreateBankChargesTypeMaster(FormCollection col, string bankcode);
        void MoveToBankChargesTypeMasterFromTemp(string bankchargestype, string Action);
        bool DeleteBankChargesTypeMasterTemp(string bankchargestype);
        BankChargesTypeModel GetBankChargesTypeTemp(string bankchargestype);
       
    }
}
