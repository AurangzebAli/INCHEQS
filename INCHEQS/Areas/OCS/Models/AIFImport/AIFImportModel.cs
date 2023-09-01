using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.AIFImport
{
    public class AIFImportModel
    {
        public string fldProcessName { get; set; }
        public string fldSystemProfileCode { get; set; }
        public string fldFileExt { get; set; }
        public string fldPosPayType { get; set; }
        public string fldFileName { get; set; }
        public string fldFileType { get; set; }
        public string fldTaskId { get; set; }
        public string fldBankCode { get; set; }
    }
}