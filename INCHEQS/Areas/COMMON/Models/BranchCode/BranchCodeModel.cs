using System;
using System.Runtime.CompilerServices;

namespace INCHEQS.Areas.COMMON.Models.BranchCode
{
	public class BranchCodeModel
	{
        //public string fldBankCode   {   get;  set;  }
        //public string fldBankDesc { get; set; }
        public string bankType { get; set; }
        public string businessType { get; set; }
        public string branchId { get; set; }
        public string locationCode    { get; set; }
        public string stateCode { get; set; }
        public string bankCode    { get; set; }
        public string bankDesc { get; set; }
        public string branchCode  { get; set; }
        public string branchDesc  { get; set; }
        public string active  { get; set; }
        public string branchType   { get; set; }
        public string createUserId    { get; set; }
        public string createTimeStamp     { get; set; }
        public string updateUserId    { get; set; }
        public string updateTimeStamp     { get; set; }
        public string bankCodeIdForDelete     { get; set; }
        public string reportTitle { get; set; }

    }
}