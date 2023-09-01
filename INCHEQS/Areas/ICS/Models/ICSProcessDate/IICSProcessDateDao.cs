﻿using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ICSProcessDate
{
    public interface IICSProcessDateDao
    {
        void UpdateProcessDate(FormCollection col);
        void CreateProcessDate(FormCollection col, AccountModel currentUser);
        string getCurrentDate();
        bool ClearDateExist();
        string getNextProcessDate();
        DataTable GetHolidayDates(FormCollection col);
        bool PerfromStartofDay(FormCollection col, AccountModel currentUser);
    }
}
