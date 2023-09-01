using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.SearchPageConfig.Services {
    public interface ISearchPageService {
        Dictionary<string, string> GetFormFiltersFromRow(FormCollection formRowCollection);
    }
}