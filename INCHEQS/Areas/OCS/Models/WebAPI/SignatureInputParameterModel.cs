using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.WebAPI
{
    public class SignatureInputParameterModel
    {
        public string AccountNumber { get; set; }
        public string SignatureRequire { get; set; }
    }
}