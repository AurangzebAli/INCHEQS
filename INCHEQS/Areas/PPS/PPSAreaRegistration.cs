using System.Web.Mvc;

namespace INCHEQS.Areas.PPS
{
    public class PPSAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PPS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PPS_default",
                "PPS/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}