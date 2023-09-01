using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.VerificationLimit
{
    public class VerificationLimitModel {

        public string verifyLimitClass { get; set; }
        public string verifyLimitDesc { get; set; }

        public float fld1stAmt { get; set; }
        public float fld2ndAmt { get; set; }

        public string fld1stType { get; set; }
        public string fld2ndType { get; set; }

        public string fldConcatenate { get; set; }
    }
}