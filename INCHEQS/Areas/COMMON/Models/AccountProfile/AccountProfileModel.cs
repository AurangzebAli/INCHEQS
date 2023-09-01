using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.AccountProfile
{
    public class AccountProfileModel
    {
        public string bankCode { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string accountType { get; set; }
        public string accountTypeDesc { get; set; }
        public string accountStatus { get; set; }
        public string accountStatusDesc { get; set; }
        public string BranchID{ get; set; }
        public string contactNumber { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string postCode { get; set; }
        public string city { get; set; }
        public string countryCode { get; set; }
        public string countryDesc { get; set; }
        public string createUserId { get; set; }
        public string createTimeStamp { get; set; }
        public string updateUserId { get; set; }
        public string updateTimeStamp { get; set; }
        public string accountIdForDelete { get; set; }
        public string reportTitle { get; set; }
        public string emailAddress { get; set; }
        public string OpeningDate { get; set; }
        public string ClosingDate { get; set; }
        public string customerNumber { get; set; }
        public string location { get; set; }
        public string branchCode { get; set; }
        public string status { get; set; }


    }
}