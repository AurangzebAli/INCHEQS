using System;
using System.Runtime.CompilerServices;

namespace INCHEQS.Areas.COMMON.Models.InternalBranch
{
    public class InternalBranchModel
    {
        public string branchId { get; set; }
        public string locationCode { get; set; }
        public string bankCode { get; set; }
        public string bankDesc { get; set; }
        public string branchCode { get; set; }
        public string branchDesc { get; set; }
        public string internalBranchCode { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string address3 { get; set; }
        public string clearingBranchID { get; set; }
        public string selfClearing { get; set; }
        public string active { get; set; }
        public string createUserId { get; set; }
        public string createTimeStamp { get; set; }
        public string updateUserId { get; set; }
        public string updateTimeStamp { get; set; }
        public string bankCodeIdForDelete { get; set; }
        public string reportTitle { get; set; }
        public string countryCode { get; set; }
        public string countryDesc { get; set; }
        public string BankZoneCode { get; set; }

        public string postCode { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }

        public string fldInternalBranchId { get; set; }
        public string fldCBranchId { get; set; }
        public string fldCBranchDesc { get; set; }
        public string fldCInternalBranchCode { get; set; }
        public string fldCClearingBranchId { get; set; }
        public string fldIBranchId { get; set; }
        public string fldIBranchDesc { get; set; }
        public string fldIInternalBranchCode { get; set; }
        public string fldIClearingBranchId { get; set; }
        public string fldEmailAddress { get; set; }
        public string fldAddress1 { get; set; }
        public string fldAddress2 { get; set; }
        public string fldAddress3 { get; set; }
        public string fldPostCode { get; set; }
        public string fldCity { get; set; }
        public string fldActive { get; set; }
    }
}