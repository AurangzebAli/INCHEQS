using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.PPS.Models.StartBatchICR
{
    public class StartBatchICRModel
    {
        public string fldMachineID { get; set; }
        public Double totalCount { get; set; }
        public Double successCount { get; set; }
        public Double failCount { get; set; }
        public Double itemLeftCount { get; set; }

        public string startTime { get; set; }
        public string endTime { get; set; }

        public string fldChequeNumber { get; set; }
        public string fldAccountNumber { get; set; }
        public string fldStatus { get; set; }
        public string fldRemarks { get; set; }

    }
}