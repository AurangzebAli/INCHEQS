using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.LoadBankHostFile
{
    public class LoadBankHostFileModel
    {
        public string fldUIC { get; set; }
        public string fldIRItemInitialID { get; set; }
        public string fldisRepresentment { get; set; }
        public string fldFileName { get; set; }
        public string fldRejectDesc { get; set; }

        public DateTime completeDateTime { get; set; }
        public DateTime createTimeStamp { get; set; }
        public long MICRBatch { get; set; }
        public string currentProcess { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public DateTime processDateTime { get; set; }
        public DateTime updateTimeStamp { get; set; }
        public int updateUserID { get; set; }
        public int createUserID { get; set; }

        public DateTime fldStartDatetime { get; set; }
        public DateTime fldEndDateTime { get; set; }
        public string fldCurrentProcess { get; set; }
        public string fldMICRBatch { get; set; }
        //public string fldcurrentprocess { get; set; }

        public string fldProcessName { get; set; }
        public string fldSystemProfileCode { get; set; }
        public string fldSystemProfileCode2 { get; set; }
        public int fldDateSubString { get; set; }
        public int fldBankCodeSubString { get; set; }
        public int fldDateSubStringCompleted { get; set; }
        public int fldBankCodeSubStringCompleted { get; set; }
        public string fldFileExt { get; set; }
        public string fldPosPayType { get; set; }

        public string TotalItemLoadC { get; set; }
        public string TotalItemTaggedC { get; set; }
        public string TotalItemUntaggedC { get; set; }
        public string TotalInsufficientFundC { get; set; }
        public string Total1stPresentedChequeC { get; set; }
        public string TotalDataCorrectionsC { get; set; }
        public string TotalUnpostedC { get; set; }
        public string TotalRejectItemC { get; set; }
        public string TotalItemLoadI { get; set; }
        public string TotalItemTaggedI { get; set; }
        public string TotalItemUntaggedI { get; set; }
        public string TotalInsufficientFundI { get; set; }
        public string Total1stPresentedChequeI { get; set; }
        public string TotalDataCorrectionsI { get; set; }
        public string TotalUnpostedI { get; set; }
        public string TotalRejectItemI { get; set; }

        public string totalInwardRec { get; set; }
        public string totalBankHostItem { get; set; }
        public string totalRecUpdated { get; set; }
        public string totalAccountNoFound { get; set; }
        public string totalAccountTwoStatus { get; set; }
        public string totalError { get; set; }
       }
}