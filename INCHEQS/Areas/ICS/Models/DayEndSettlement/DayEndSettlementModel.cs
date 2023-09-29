using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.DayEndSettlement
{
    public class DayEndSettlementModel
    {
        public string posPayType { get; set; }
        public string clearDate { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string Processname { get; set; }
        public string bankCode { get; set; }
        public string status { get; set; }

    }
}