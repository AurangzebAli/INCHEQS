using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.BankCharges
{
    public class BankChargesModel
    {
        public string fldBankChargesType { get; set; }
        public string fldBankChargesDesc { get; set; }
        public string fldApproveStatus { get; set; }
        public string fldBankCode { get; set; }
        public string fldProductCode { get; set; }
        public string fldChequeAmtMin { get; set; }
        public string fldChequeAmtMax { get; set; }
        public string fldBankChargesAmount { get; set; }
        public string fldBankChargesRate { get; set; }

    }
}