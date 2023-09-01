using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.ReturnCode
{
    public class ReturnCodeModel
    {
        public string fldRejectCode { get; set; }
        public string fldRejectDesc { get; set; }
        public string fldRepresentable { get; set; }
        public string fldRejectType { get; set; }
        public string fldCharges { get; set; }
        public string fldApproveStatus { get; set; }
        public string fldCreateTimeStamp { get; set; }
        public string fldCreateUserId { get; set; }
        public string fldRejectTypeDesc { get; set; }
    }
}