using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Concerns {
    public class InwardItemConcern {
        public static InwardItemViewModel NextChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection) {
            InwardItemViewModel verificationModel = new InwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0) {
                errors.Add("error", Locale.NoRecord);
            } else if (result["fldInwardItemId"].Equals(collection["fldInwardItemId"])) {
                
                if (pageSqlConfig.TaskId != "306530" && pageSqlConfig.TaskId != "306540" && pageSqlConfig.TaskId != "306550" && pageSqlConfig.TaskId != "306510" && pageSqlConfig.TaskId != "306520")
                {
                    errors.Add("warning", Locale.NoNextChequeAvailable);
                }
                
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }
        public static InwardItemViewModel ChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection, string message)
        {
            InwardItemViewModel verificationModel = new InwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("error", Locale.NoRecord);
            }
            else if (result["fldInwardItemId"].Equals(collection["fldInwardItemId"]))
            {

                if (pageSqlConfig.TaskId != "306530" && pageSqlConfig.TaskId != "306540" &&  pageSqlConfig.TaskId != "306510" && pageSqlConfig.TaskId != "306520")
                {
                    errors.Add("error", message);
                }

            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        public static InwardItemViewModel ChequePagePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result) {
            InwardItemViewModel verificationModel = new InwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (!(pageSqlConfig.TaskId == "301110" || pageSqlConfig.TaskId == "306540" || pageSqlConfig.TaskId == "306530" || pageSqlConfig.TaskId == "306510" || pageSqlConfig.TaskId == "301120" || pageSqlConfig.TaskId == "301130" || pageSqlConfig.TaskId == "309230"))
            {
                if (pageSqlConfig.TaskId != "308110" && pageSqlConfig.TaskId != "308170" && pageSqlConfig.TaskId != "308180" && pageSqlConfig.TaskId != "308190" && pageSqlConfig.TaskId != "308200")
                {
                    if (result["fldAssignedUserId"].ToString().Trim() != CurrentUser.Account.UserId && !(string.IsNullOrEmpty(result["fldAssignedUserId"].ToString().Trim())))
                    {
                        errors.Add("error", "Cheque is currently being locked by other user");

                    }
                }
                else
                {
                    if (result["fldAssignedUserIdPending"].ToString().Trim() != CurrentUser.Account.UserId && !(string.IsNullOrEmpty(result["fldAssignedUserIdPending"].ToString().Trim())))
                    {
                        errors.Add("error", "Cheque is currently being locked by other user");

                    }
                }
                
            }

            if (result.Count == 0) {
                errors.Add("notice", Locale.NoRecord);
            }
            
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            verificationModel.getFormatImageFolderPath();
            verificationModel.imageId = result["fldImageFileName"];
            verificationModel.pageTitle = pageSqlConfig.PageTitle;
            verificationModel.errorMessages = errors;

            return verificationModel;
        }

        public static InwardItemViewModel PrevChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection) {
            InwardItemViewModel verificationModel = new InwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (result.Count == 0) {
                errors.Add("warning",Locale.NoRecord);
            } else if (result["fldInwardItemId"].Equals(collection["fldInwardItemId"])) {
                //errors.Add("warning", Locale.NoPreviousChequeAvailable);
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        public static InwardItemViewModel InwardItemWithErrorMessages(QueueSqlConfig pageSqlConfig, Dictionary<string,string> result, List<string> errorMessages) {
            InwardItemViewModel verificationModel = new InwardItemViewModel();

            verificationModel.errorMessages.Add("error", errorMessages[0]);
    
            //verificationModel.errorMessages = errorMessages.ToDictionary(x => "error", x => x);
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            return verificationModel;
        }
        public static InwardItemViewModel InwardItemWithWarnings(PageSqlConfig pageSqlConfig, Dictionary<string, string> result, List<string> warnings) {
            InwardItemViewModel verificationModel = new InwardItemViewModel();
            verificationModel.errorMessages = warnings.ToDictionary(x => "warning", x => x);
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            return verificationModel;
        }


    }
}