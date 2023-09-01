using INCHEQS.Security.Account;
using INCHEQS.Models.SearchPageConfig;
using System.Collections.Generic;
using System.Linq;

namespace INCHEQS.Helpers {
    public class ViewHelper {
        public static string DataFieldValue(List<DataField> dataFields, string fieldId) {
            return dataFields.Where(x => x.fieldId == fieldId).FirstOrDefault().value;
        }   

        public static bool IsAllowed(List<string> allowedActions ,params string[] actions) {
            if (allowedActions != null) {
                foreach (string action in actions){
                    if (allowedActions.Contains(action)) {
                        return true;
                    };
                }
                return false;
            } else {
                return false;
            }
        }

        public static bool HasUrlAccess(AccountModel currentUserAccount, List<string> allowedTaskIds, string strAction) {
            Dictionary<string, string> userTaskIds = currentUserAccount.TaskIds;
            foreach(string taskId in allowedTaskIds) {
                if (userTaskIds.ContainsKey(taskId)) {
                    if (userTaskIds[taskId].Equals(strAction)) {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}