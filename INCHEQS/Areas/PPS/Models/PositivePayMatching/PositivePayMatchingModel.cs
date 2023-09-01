using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.PPS.Models.PositivePayMatching
{
    public class PositivePayMatchingModel
    {
        public string minClearDate { get; set; }
        public string maxClearDate { get; set; }
        public string totalMatched { get; set; }
        public string totalUnMatched { get; set; }

        public string totalItem { get; set; }
        public string totalPayeeMatch { get; set; }
        public string totalDateMatch { get; set; }
        public string totalAmountMatch { get; set; }
        public string totalMICRMatch { get; set; }

        public string totalPayeeUnmatch { get; set; }
        public string totalDateUnmatch { get; set; }
        public string totalAmountUnmatch { get; set; }
        public string totalMICRUnmatch { get; set; }

        public Double PayeePercentage { get; set; }
        public Double DatePercentage { get; set; }
        public Double AmountPercentage { get; set; }
        public Double MICRPercentage { get; set; }
    }
}