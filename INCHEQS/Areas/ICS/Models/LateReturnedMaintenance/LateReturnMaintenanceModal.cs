using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.LateReturnedMaintenance
{
    public class LateReturnMaintenanceModal
    {

        public string fldClearDate { get; set; }
        public string fldUIC { get; set; }
        public string fldrejectCode { get; set; }
        public string fldRejectDesc { get; set; }
        public string fldCharges { get; set; }
        public string fldRemarks { get; set; }
        public string fldAccountNo { get; set; }
        public string fldChequeNo { get; set; }
        public string fldAmount { get; set; }
        public string fldtranscode { get; set; }
        public string fldApprovalUserId { get; set; }
        public string fldApprovalStatus { get; set; }
        public string fldApprovalTimestamp { get; set; }
        public string upiGenerated { get; set; }
        public string fldBankCode { get; set; }
    }
}