using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.BandQueueSetting {
    public class BandQueueSettingModel {

        public string fldTaskId { get;set;}
        public string fldMenuTitle { get; set; }
        public string fldBandLBound { get; set; }
        public string fldBandUBound { get; set; }
        public string fldVolumnPercentage { get; set; }
        public string fldActive { get; set; }
           

    }
}