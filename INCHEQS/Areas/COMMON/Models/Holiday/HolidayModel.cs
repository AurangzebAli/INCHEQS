using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.Holiday
{
    public class HolidayModel
    {

        public string recurrType { get; internal set; }
        public string recurring { get; internal set; }
        public string Date { get; internal set; }
        public object Desc { get; internal set; }
        public string HolidayId { get; internal set; }
        public string recurrTypeYearly { get; internal set; }
        public string fldHolidayDate { get; internal set; }
        public string fldDayMon { get; internal set; }
        public string fldDayTue { get; internal set; }
        public string fldDayWed { get; internal set; }
        public string fldDayThu { get; internal set; }
        public string fldDayFri { get; internal set; }
        public string fldDaySat { get; internal set; }
        public string fldDaySun { get; internal set; }
        public string fldActiveHoliday { get; internal set; }
        public bool Activecheck { get; internal set; }
        public bool MonChecked { get; internal set; }
        public bool tueChecked { get; internal set; }
        public bool wedChecked { get; internal set; }
        public bool thuChecked { get; internal set; }
        public bool friChecked { get; internal set; }
        public bool satChecked { get; internal set; }
        public bool sunChecked { get; internal set; }

    }
}