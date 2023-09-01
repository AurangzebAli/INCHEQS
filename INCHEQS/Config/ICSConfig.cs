using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS {
    public class ICSConfig {
        public static PageSqlConfig QueueConfig {
            get {
                PageSqlConfig applicationSqlConfig = new PageSqlConfig();
                applicationSqlConfig.AddSqlOrderBy("fldAmount desc");
                applicationSqlConfig.AddSqlExtraCondition("fldIssueBankCode = @fldIssueBankCode");
                applicationSqlConfig.AddSqlExtraConditionParams(new[] {
                    new SqlParameter("@fldIssueBankCode",CurrentUser.Account.BankCode)
                });
                
                return applicationSqlConfig;
            }
        }
    }
}