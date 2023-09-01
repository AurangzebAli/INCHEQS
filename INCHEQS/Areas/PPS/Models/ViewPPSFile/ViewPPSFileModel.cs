using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.PPS.Models.ViewPPSFile
{
    public class ViewPPSFileModel
    {
        public string fldIssueDate { get; set; }
        public string fldAccNo { get; set; }
        public string fldChequeNo { get; set; }
        public string fldPayeeName { get; set; }
        public string fldAmount { get; set; }
        public string fldStatus { get; set; }
        public string validFlag { get; set; }

    }
}