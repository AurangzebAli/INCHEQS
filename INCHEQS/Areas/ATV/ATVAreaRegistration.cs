using System.Web.Mvc;

namespace INCHEQS.Areas.ATV
{
    public class ATVAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ATV";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ATV_default",
                "ATV/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}