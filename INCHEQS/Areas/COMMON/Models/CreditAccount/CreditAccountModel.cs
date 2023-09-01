using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.CreditAccount
{
    public class CreditAccountModel
    {
        public string fldCreditAccountId { get; set; }

        public string fldClearingValue { get; set; }

        public string fldClearingType { get; set; }
        public string fldStateCode { get; set; }
        public string fldStateDesc { get; set; }
        public string fldCreateTimeStamp { get; set; }
        public string fldCreditAccountNumber { get; set; }
        public string fldCreateUserId { get; set; }

    }
}