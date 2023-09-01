using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.SearchPageConfig {
    public class QueueSqlConfig{

        public string PageTitle { get; set; }
        public string TaskId { get; set; }
        public string ViewOrTableName { get; set; }
        public string SqlOrderBy { get; set; }
        public string SqlExtraCondition { get; set; }
        public SqlParameter[] SqlExtraConditionParams { get; set; }
        public List<string> AllowedActions { get; set; }
        public string TaskRole { get; set; }
        public string PrintReportParam { get; set; }
        public string SqlLockCondition { get; set; }
        public string StoreProcedure { get; set; }
        public Dictionary<string, string> CurrentUser { get; set; }
        public PageSqlConfig toPageSqlConfig() {
            return new PageSqlConfig(this.TaskId, this.ViewOrTableName, this.SqlOrderBy,this.SqlExtraCondition, this.SqlExtraConditionParams, this.PageTitle,this.PrintReportParam, this.SqlLockCondition, this.StoreProcedure);
        }

        public bool isInAction(string action) {
            return this.AllowedActions.Contains(action);
        }

    }
}