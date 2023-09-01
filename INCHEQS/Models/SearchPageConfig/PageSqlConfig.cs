using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.SearchPageConfig
{

    public class PageSqlConfig
    {
        public string PageTitle { get; private set; }
        public string PrintReportParam { get; private set; }
        public string TaskId { get; private set; }
        public string ViewOrTableName { get; private set; }
        public string SqlOrderBy { get; private set; }
        public string SqlExtraCondition { get; private set; }
        public string SqlLockCondition { get; private set; }
        public string StoreProcedure { get; set; }
        public SqlParameter[] SqlExtraConditionParams { get; private set; }

        //Empty Constructor
        public PageSqlConfig(){ }

        //Constructor with Attributes
        public PageSqlConfig( string TaskId, string ViewOrTableName = null, string SqlOrderBy = null , string SqlExtraCondition = null, SqlParameter[] SqlExtraConditionParams = null , string pageTitle = "", string printReportParam = "" , string sqlLockCondition = "", string storeprocedure = "")
        {
            this.TaskId = TaskId;
            this.ViewOrTableName = ViewOrTableName;
            this.SqlOrderBy = SqlOrderBy;
            this.SqlExtraCondition = SqlExtraCondition;
            this.SqlExtraConditionParams = SqlExtraConditionParams;
            this.PageTitle = pageTitle;
            this.PrintReportParam = printReportParam;
            this.SqlLockCondition = sqlLockCondition;
            this.StoreProcedure = storeprocedure;
        }

        public PageSqlConfig SetReportParam(string printReportParam) {
            this.PrintReportParam = printReportParam;
            return this;
        }

        public PageSqlConfig SetSqlLockCondition(string sqlLockCondition) {
            this.SqlLockCondition = sqlLockCondition;
            return this;
        }

        public PageSqlConfig SetPageTitle(string pageTitle) {
            this.PageTitle = pageTitle;
            return this;
        }

        public PageSqlConfig SetTaskId(string taskId) {
            this.TaskId = taskId;
            return this;
        }

        public PageSqlConfig SetViewId(string ViewOrTableName) {
            this.ViewOrTableName = ViewOrTableName;
            return this;
        }

        public PageSqlConfig AddSqlOrderBy(string orderBy) {
            if (!string.IsNullOrEmpty(this.SqlOrderBy)) {
                this.SqlOrderBy = string.Format("{0} , {1}", this.SqlOrderBy, orderBy);
            } else {
                this.SqlOrderBy = orderBy;
            }
            return this;
        }


        public PageSqlConfig AddSqlExtraCondition(string SqlExtraCondition) {
            if (!string.IsNullOrEmpty(this.SqlExtraCondition)) {
                this.SqlExtraCondition = string.Format("{0} AND {1}", this.SqlExtraCondition, SqlExtraCondition);
            } else {
                this.SqlExtraCondition = SqlExtraCondition;
            }
            return this;
        }


        public PageSqlConfig AddSqlExtraConditionParams(SqlParameter[] SqlExtraConditionParams) {
            if (this.SqlExtraConditionParams != null) {
                List<SqlParameter> sqlParams = new List<SqlParameter>();
                sqlParams.AddRange(this.SqlExtraConditionParams);
                sqlParams.AddRange(SqlExtraConditionParams);
                this.SqlExtraConditionParams = sqlParams.ToArray();
            } else { 
                this.SqlExtraConditionParams = SqlExtraConditionParams;            
            }
            return this;
        }

    }
}