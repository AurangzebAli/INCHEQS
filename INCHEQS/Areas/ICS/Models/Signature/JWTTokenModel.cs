using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Signature
{
    public class JWTTokenModel
    {
        public string client_secret { get; set; }
        public string client_id { get; set; }
    }
    public class JWTTokenModelResponse
    {
        public string access_token { get; set; }
    }
}