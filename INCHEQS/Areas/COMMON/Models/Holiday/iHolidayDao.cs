﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.Holiday{
    public interface iHolidayDao{
        HolidayModel InsertHoliday(FormCollection col);
        HolidayModel CreateHolidayToHolidayTemp(FormCollection col);
        HolidayModel GetHolidayData(string id);
        void UpdateHolidayTable(FormCollection col);
        List<string> ValidateHoliday(FormCollection col, string action);
        void DeleteFromHolidayTable(string id);
        void CreateHolidayinMain(string date);
        void DeleteHolidayinTemp(string Value, int count);
        void AddHolidayinTemptoDelete(string date);
        bool CheckHolidayExist(string id);
        void AddHolidayinTemptoUpdate(FormCollection col);
        void UpdateHolidayToMain(string id);
        bool checkDateExist(string bankCode);
        bool checkDateExistTemp(string bankCode);

    }
}