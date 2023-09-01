using System;

namespace INCHEQS.Areas.ICS.Models.ICSRetentionPeriod
{
    public class ICSRetentionPeriodModel
    {
        public Int32 ICInt { get; set; }
        public string ICIntType { get; set; }
        public Int32 ICHistoryInt { get; set; }
        public string ICHistoryIntType { get; set; }
        public string IntType { get; set; }
    }
}