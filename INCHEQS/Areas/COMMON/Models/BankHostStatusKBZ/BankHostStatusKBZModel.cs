namespace INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ
{
    public class BankHostStatusKBZModel
    {

        public string statusCode { get; set; }
        public string statusDesc { get; set; }

        public string hostActionCode { get; set; }
        public string hostActionDesc { get; set; }
        public string hostRejectDesc { get; set; }
        public string hostRejectCode { get; set; }

        public string fldBankHostStatusCode { get; set; }
        public string fldBankHostStatusDesc { get; set; }
        public string fldBankHostStatusAction { get; set; }
        public string fldrejectcode { get; set; }
        public string fldBankCode { get; set; }
        public string fldCreateTimeStamp { get; set; }
        public string fldCreateUserId { get; set; }
        public string fldUpdateTimeStamp { get; set; }
        public string fldUpdateUserId { get; set; }
        public string fldIdForDelete { get; set; }
        public string ReportTitle { get; set; }
    }
}