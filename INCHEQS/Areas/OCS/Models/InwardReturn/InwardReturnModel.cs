using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.InwardReturn
{
    public class InwardReturnModel
    {
        public string fldUIC { get; set; }
        public string fldIRItemInitialID { get; set; }
        public string fldisRepresentment { get; set; }
        public string fldFileName { get; set; }
        public string fldRejectDesc { get; set; }
    }
}