using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.ScannerWorkStation
{
    public class ScannerWorkStationModel
    {
        public string ScannerTypeId { get; set; }
        public string ScannerBrandName { get; set; }
        public string ScannerBankName { get; set; }
        public string ScannerLocation { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string Location { get; set; }
        public string MacAdd1 { get; set; }
        public string MacAdd2 { get; set; }
        public string MacAdd3 { get; set; }
        public string Active { get; set; }
        public string Status { get; set; }
        public string UpdateBy { get; set; }
        public string ScannerId { get; set; }
        public string ScannerType { get; set; }
        public string BatchNo { get; set; }
        public string SeqNo { get; set; }
        public string CreateUserId { get; set; }
        public string CreateTimeStamp { get; set; }
        public string UpdateUserId { get; set; }
        public string UpdateTimeStamp { get; set; }
        public string ScannerIdForDelete { get; set; }
        public string ScannerTypeNumber { get; set; }
        public string ReportTitle { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }

    }
}