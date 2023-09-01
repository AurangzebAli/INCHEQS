using System;

namespace INCHEQS.Areas.OCS.Models.OCSRetentionPeriod
{
    public class OCSRetentionPeriodModel
    {
        public Int32 OCInt { get; set; }
        public string OCIntType { get; set; }
        public Int32 IRInt { get; set; }
        public string IRIntType { get; set; }
        public Int32 OCHistoryInt { get; set; }
        public string OCHistoryIntType { get; set; }
        public string IntType { get; set; }
    }
}