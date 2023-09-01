using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS
{
    //[RequireHttps]
    [GenericFilter]
    [CustomAuthorize(TaskIds = "all")]
    public abstract class BaseController : Controller
    {
    }
}