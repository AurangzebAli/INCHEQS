using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.Branch
{
    public class BranchModel
    {
        public string fldBranchId { get; set; }
        public string fldBranchCode { get; set; }
        public string fldBranchDesc { get; set; }
        public string fldBankCode { get; set; }
        public string fldDisable { get; set; }
        public string fldCreateUserId { get; set; }
        public string fldCreateTimeStamp { get; set; }
        public string fldUpdateUserId { get; set; }
        public string fldUpdateTimeStamp { get; set; }
        public string fldBranchLevel { get; set; }

        public BranchModel()
        {
        }
    }
}