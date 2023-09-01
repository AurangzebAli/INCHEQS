using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Signature {
    public class SignatureInfo {
        public string accountNo { get; set; }         
        public string accountName { get; set; }
        public string bankCode { get; set; }
        public string branchCode { get; set; }
        public string accountStatus { get; set; }
        public string accountType { get; set; }
        public string externalCustomer { get; set; }
        public string imageId { get; set; }
        public string imageNo { get; set; }
        public string imageStatus { get; set; }
        public string imageDesc { get; set; }

        public string sigGroup { get; set; }
        public string condition { get; set; }
        public string fldRequireSigNo { get; set; }
        public string fldSigAmountFrom { get; set; }
        public string fldSigAmountLimit { get; set; }
        public string fldSigValidFrom { get; set; }
        public string fldSigValidTo { get; set; }

        public string checkedSignature { get; set; }
    }
}