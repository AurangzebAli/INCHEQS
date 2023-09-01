using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web.Routing;

namespace INCHEQS.Areas.ATV.Controller.ATV.Shared
{
    public class ImageController
    {
        // GET: Demo/GetImageFromByteArray  
        public ActionResult GetImageFromByteArray()
        {
            // Get image path  
            string imgPath = Server.MapPath("~/images/self-discipline.png");
            // Convert image to byte array  
            byte[] byteData = System.IO.File.ReadAllBytes(imgPath);
            //Convert byte arry to base64string   
            string imreBase64Data = Convert.ToBase64String(byteData);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            //Passing image data in viewbag to view  
            ViewBag.ImageData = imgDataURL;
            return View();
        }
    }
}