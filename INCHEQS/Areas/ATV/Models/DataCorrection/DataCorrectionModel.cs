using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ATV.Models.DataCorrection
{
    public class DataCorrectionModel
    {
        public string itemId { get; set; }
        public string UIC { get; set; }
        public string accountNo { get; set; }
        public string chequeNo { get; set; }
        public string amount { get; set; } 
        public string imageDatePath { get; set; }
        public string dataMICRImage { get; set; }
        public string dataMICRAccount { get; set; }
        public Dictionary<string, string> allFields { get; set; }
        public Dictionary<string, string> errorMessages { get; set; }

        public string getImageDatePath()
        {
            return imageDatePath.Replace("\\\\", @"\").Replace("\\", "\\\\");
        }



    }
}