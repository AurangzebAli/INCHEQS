using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Models.SearchPageConfig;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace INCHEQS.Models.DashboardConfig {
    public class DashboardConfigDao : IDashboardConfigDao {

        private readonly ApplicationDbContext dbContext;
        public DashboardConfigDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }


        public DataTable List(string parentTaskId , List<string> childTaskIds) {

            DataTable ds = new DataTable();
            string stmt = "SELECT * FROM [tblDashboardConfig] WHERE [isEnabled] = 'Y' AND [ParentTaskId] = @parentTaskId";

            if(childTaskIds.Count > 0) { 
                stmt += string.Format(" AND [TaskId] IN ({0})" , DatabaseUtils.getParameterizedStatementFromArray(childTaskIds.ToArray()));
            }
            stmt += " ORDER BY [Ordering] ";

            List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFromArray(childTaskIds.ToArray());
            sqlParams.Add(new SqlParameter("@parentTaskId", parentTaskId));
            ds = dbContext.GetRecordsAsDataTable( stmt , sqlParams.ToArray());
            
            return ds;
        }

        public DataTable ProgressMonitoring(string parentTaskId, string TaskId)
        {

           // DataTable ds = new DataTable();
            string stmt = "SELECT divId, '" +  TaskId  + "' as taskId, widgetType, DivWidth FROM [tblDashboardConfig] WHERE Taskid = '309012' AND [ParentTaskId] = @parentTaskId";

           // if (childTaskIds.Count > 0)
           // {
            //    stmt += string.Format(" AND [TaskId] IN ({0})", DatabaseUtils.getParameterizedStatementFromArray(childTaskIds.ToArray()));
           // }
            stmt += " ORDER BY [Ordering] ";

            //List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFro;
            //sqlParams.Add(new SqlParameter("@parentTaskId", parentTaskId));
            //ds = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[]
            { new SqlParameter("@parentTaskId", parentTaskId)
            });

            return ds;
        }
        public DataTable OCSProgressMonitoring(string parentTaskId, string TaskId)
        {

            // DataTable ds = new DataTable();
            string stmt = "SELECT divId, '" + TaskId + "' as taskId, widgetType, DivWidth FROM tbldashboardconfig WHERE taskid = '306371' AND parenttaskid= @parentTaskId";

            // if (childTaskIds.Count > 0)
            // {
            //    stmt += string.Format(" AND [TaskId] IN ({0})", DatabaseUtils.getParameterizedStatementFromArray(childTaskIds.ToArray()));
            // }
            stmt += " ORDER BY Ordering ";

            //List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFro;
            //sqlParams.Add(new SqlParameter("@parentTaskId", parentTaskId));
            //ds = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[]
            { new SqlParameter("@parentTaskId", parentTaskId)
            });

            return ds;
        }


        public DataTable BranchProgressMonitoring(string parentTaskId, string TaskId)
        {

            // DataTable ds = new DataTable();
            string stmt = "SELECT divId, '" + TaskId + "' as taskId, widgetType, DivWidth FROM [tblDashboardConfig] WHERE Taskid = '309013' AND [ParentTaskId] = @parentTaskId";

            // if (childTaskIds.Count > 0)
            // {
            //    stmt += string.Format(" AND [TaskId] IN ({0})", DatabaseUtils.getParameterizedStatementFromArray(childTaskIds.ToArray()));
            // }
            stmt += " ORDER BY [Ordering] ";

            //List<SqlParameter> sqlParams = DatabaseUtils.getSqlParametersFro;
            //sqlParams.Add(new SqlParameter("@parentTaskId", parentTaskId));
            //ds = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[]
            { new SqlParameter("@parentTaskId", parentTaskId)
            });

            return ds;
        }

        public DataTable GetResultForDivViewId( string divViewId , PageSqlConfig pageSqlConfig, FormCollection collection) {
            DataTable dt = new DataTable();
            DataTable configTable = new DataTable();
            
            string stmt = "SELECT * FROM [tblDashboardConfig] WHERE [isEnabled] = 'Y' AND divId = @divId";
            dt = dbContext.GetRecordsAsDataTable( stmt, new[] { new SqlParameter("@divId" , divViewId) });
            if(dt.Rows.Count > 0) {
                dt = dbContext.GetRecordsAsDataTable( dt.Rows[0]["databaseViewId"].ToString());
            } else {
                dt = new DataTable();
            }    

            return dt;
        }
    }


}