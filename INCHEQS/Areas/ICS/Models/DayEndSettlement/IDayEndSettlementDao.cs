using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.DayEndSettlement
{
    public interface IDayEndSettlementDao
    {

        void InsertDayEndSettlement(DayEndSettlementModel inwardItemViewModel, AccountModel currentUser);
        List<string> ValidateDayEndsettlement(AccountModel currentUser);

        string GetTableForDayEndSettlement(FormCollection col,AccountModel currentUser);

    }
}
