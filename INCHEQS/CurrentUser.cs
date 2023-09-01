using INCHEQS.Security.Account;
using System;
using System.Web;

namespace INCHEQS.Security {
    public class CurrentUser {
        public static string userIdSessionVar = "incheqsUserId";
        public static AccountModel Account {
            get {
                if (HttpContext.Current == null) {
                    return null;
                }
                try {
                    var sessionVar = HttpContext.Current.Session[userIdSessionVar] as AccountModel;
                    if (sessionVar != null) {
                        return sessionVar;
                    }
                } catch (Exception ex) {
                    throw ex;
                }
                return new AccountModel();
            }
        }

        public static void SetAuthenticatedSession(AccountModel account) {
            HttpContext.Current.Session[userIdSessionVar] = account;
        }

        public static Boolean HasTask(string taskId) {
            return (Account.TaskIds.ContainsKey(taskId));
        }
        
        public static Boolean HasAccessToUrlNotUnique(string url) {
            return Account.TaskIds.ContainsValue(url);
        }        
    }
}