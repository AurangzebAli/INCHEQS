using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance
{
    public class ThresholdModel
    {
       //internal IEnumerable<string> AllKeys;

        public string thresholdType { get; set; }
        public string thresholdLevel { get; set; }
        public string thresholdAmount { get; set; }
        
        
    }
}