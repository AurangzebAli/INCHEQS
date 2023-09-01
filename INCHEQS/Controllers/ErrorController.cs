using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index()
        {

            Response.StatusCode = 404;
            return View();
        }
        
        public ActionResult NotFound() {

            Response.StatusCode = 404;
            return View();
        }
    }
}