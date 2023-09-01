using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.HostValidation
{
    public class HostValidationModel
    {
        public string Cheque { get; set; }
        public bool IsGenuine { get; set; }

    }
}