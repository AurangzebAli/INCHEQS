using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.BranchClearingItem
{
    public class BranchItemReadyModel
    {
        public int fldCapturingBranch { get; set; }
        public string fldBranchDesc { get; set; }
        public string CapturingBranch { get; set; }
        public int CapturingDate { get; set; }
        public int fldBatchNumber { get; set; }
        public string fldCapturingMode { get; set; }
        public int fldScannerID { get; set; }
        public string fldARJBatchNumber { get; set; }
        public string fldPostingMode { get; set; }
        public int TotalItem { get; set; }
        public int TotalAmount { get; set; }
    }

}