using System.Web.Mvc;

namespace INCHEQS.Areas.OCS
{
    public class OCSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OCS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "OCS_default",
                "OCS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}