using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.AuditTrailOCS
{
    public class AuditTrailOCSModel
    {
        public string fldCheckDigit { get; set; }
        public string fldType { get; set; }
        public string fldLocation { get; set; }
        public string fldBankCode { get; set; }
        public string fldBranchCode { get; set; }
        public string fldSerial { get; set; }
        public string fldIssuerAccNo { get; set; }
        public string fldChequeIssueDate { get; set; }
        public string fldAmount { get; set; }
        public string fldWaiveCharges { get; set; }
        public string fldRemark { get; set; }
        public string fldReasonCode { get; set; }
        public string fldPVaccNo { get; set; }
        public string fldOldAmount { get; set; }



    }
}