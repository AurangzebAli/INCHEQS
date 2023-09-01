using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates
{
    public class ChangePasswordModel
    {
        internal IEnumerable<string> AllKeys;

        public string fldUserId { get; set; }
        public string fldUserAbb { get; set; }
        public string fldPassword { get; set; }
        
        
    }
}