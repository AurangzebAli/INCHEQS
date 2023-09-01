using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.Message
{
    public class MessageModel
    {
        public string fldBroadcastMessageId { get; set; }
        public string fldBroadcastMessage { get; set; }
        public string fldApproveStatus { get; set; }
        public string fldCreateTimeStamp { get; set; }
        public string fldCreateUserId { get; set; }
    }
}