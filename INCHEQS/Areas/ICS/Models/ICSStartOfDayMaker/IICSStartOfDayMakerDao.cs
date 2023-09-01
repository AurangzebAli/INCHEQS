using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ICSStartOfDayMaker
{
    public interface IICSStartOfDayMakerDao
    {
        void UpdateProcessDate(FormCollection col);
        void CreateProcessDate(FormCollection col, AccountModel currentUser);
        string getCurrentDate();
        bool ClearDateExist();
        string getNextProcessDate();
        DataTable GetHolidayDates(FormCollection col);
        bool PerfromStartofDay(FormCollection col, AccountModel currentUser);
        string getConfirmProcessDate();
        void CreateICSStartOfDayTemp(FormCollection col, AccountModel currentUser);
        bool DeleteICSStartOfDayTemp(AccountModel user);
        void MoveToInwardClearDateMasterFromTemp(FormCollection col, AccountModel currentUser);
        List<string> ValidateICSSOD();
    }
}
