using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON
{
    public class COMMONAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "COMMON";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "COMMON_default",
                "COMMON/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}