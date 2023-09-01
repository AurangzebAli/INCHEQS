using INCHEQS.ActionFilters;
using System.Web.Mvc;

namespace INCHEQS {
    public class GlobalFiltersConfig {

        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new AuthorizeAttribute());
            filters.Add(new ExceptionHandlerAttribute());
        }
    }
}