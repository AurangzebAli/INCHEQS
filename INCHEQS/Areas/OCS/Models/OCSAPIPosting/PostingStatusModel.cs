using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.OCSAPIPosting
{
    public class PostingStatusModel
    {
        public DateTime completeDateTime { get; set; }
        public DateTime createTimeStamp { get; set; }
        public int createUserID { get; set; }
        public Int32 TotalItem { get; set; }
        public Int64 TotalAmount { get; set; }
        public string currentProcess { get; set; }
        public string errorCode { get; set; }
        public string errorMsg { get; set; }
        public long postBatch { get; set; }
        public string postingSession { get; set; }
        public Int64? previousPostBatch { get; set; }
        public DateTime processDateTime { get; set; }
        public string regenerateFlag { get; set; }
        public string FileGenerateFlag { get; set; }
        public DateTime updateTimeStamp { get; set; }
        public int updateUserID { get; set; }
        public string uploadFlag { get; set; }
    }
}