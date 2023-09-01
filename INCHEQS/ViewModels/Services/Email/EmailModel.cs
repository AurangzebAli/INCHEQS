using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Email {
    public class EmailModel {
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string Message { get; set; }
    }
}