using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.WebAPI
{
    public class SignatureEnquiryModel
    {
        public List<Error> ErrorDetail { get; set; }
        public List<AccountDetails> Data { get; set; }
        public string KBZRefNo { get; set; }
    }
    public class AccountDetails
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string BankCode { get; set; }
        public string EffectiveDate { get; set; }
        public string AccountExpiry { get; set; }
        public string AccountStatus { get; set; }
        public string Condition { get; set; }
        public string NoOfSignature { get; set; }
        public List<ImageCode> Images { get; set; }
    }
    public class ImageCode
    {
        public string ImageNo { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ExpiryDate { get; set; }
        public string ImageCode64 { get; set; }

    }
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<Details> Details { get; set; }

    }
    public class Details
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

    }
}