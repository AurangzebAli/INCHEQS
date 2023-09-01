using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Security.Account;
using System.Web.Mvc;
using System.Data;

namespace INCHEQS.Areas.OCS.Models.OCSProcessDate
{
    public interface IOCSProcessDateDao
    {
        void UpdateProcessDate(FormCollection col);
        void CreateProcessDate(FormCollection col, AccountModel currentUser);
        string getCurrentDate();
        string getNextProcessDate();
        DataTable GetHolidayDates(FormCollection col);
    }
}