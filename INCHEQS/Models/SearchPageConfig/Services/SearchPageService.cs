using INCHEQS.DataAccessLayer;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Models.SearchPageConfig.Services {

    public class SearchPageService : ISearchPageService{

        private ApplicationDbContext dbContext;

        public SearchPageService(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }
        
        public Dictionary<string,string> GetFormFiltersFromRow(FormCollection formRowCollection) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            char[] chars = new[] {'r','o','w','_' };
            foreach ( string key in formRowCollection.AllKeys) {
                result.Add(key.TrimStart(chars), formRowCollection[key]);
            }
            return result;
        }
        
    }
}