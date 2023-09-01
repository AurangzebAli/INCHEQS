using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.DashboardConfig {
    public interface IDashboardConfigDao {
        DataTable List(string parentTaskId , List<string> childTaskIds);
        DataTable ProgressMonitoring(string parentTaskId, string TaskIds);
        DataTable OCSProgressMonitoring(string parentTaskId, string TaskIds);
        DataTable BranchProgressMonitoring(string parentTaskId, string TaskIds);
        DataTable GetResultForDivViewId(string divViewId, PageSqlConfig pageSqlConfig, FormCollection collection);
    }
}
