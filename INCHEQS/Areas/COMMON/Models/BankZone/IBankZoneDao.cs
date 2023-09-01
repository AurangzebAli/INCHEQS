using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.BankZone
{
    public interface IBankZoneDao
    {
        
        BankZoneModel GetBankZone(string bankzonecode);
        List<string> ValidateBankZone(FormCollection col, string action, string bankcode);
        bool UpdateBankZoneMaster(FormCollection col);
        void CreateBankZoneMasterTemp(FormCollection col, string bankcode, string crtUser, string Action);
        bool DeleteBankZoneMaster(string bankzonecode, string bankcode);
        bool CheckBankZoneMasterTempById(string bankzonecode, string bankcode);
        bool CreateBankZoneMaster(FormCollection col, string bankcode);
        void MoveToBankZoneMasterFromTemp(string bankzonecode, string Action);
        bool DeleteBankZoneMasterTemp(string bankzonecode);
        BankZoneModel GetBankZoneTemp(string bankzonecode);
       
    }
}
