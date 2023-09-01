//using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Areas.OCS.ViewModels;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Concerns
{
    public class OutwardItemConcern
    {
        public static OutwardItemViewModel NextChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("error", Locale.NoRecord);
           }
            if ((result["fldItemId"].Equals(collection["fldItemId"])))
            {
                errors.Add("warning", Locale.NoNextChequeAvailable);
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldpath"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        public static OutwardItemViewModel NextChequePopulateViewModelOCS(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("error", Locale.NoRecord);
            }
            else if ((result["fldItemId"].Equals(collection["fldItemId"])) && (result["NextValue"].Equals(""))) //michelle mod(!)
            {
                errors.Add("warning", Locale.NoNextChequeAvailable);
            }

            verificationModel.allFields = result;
            // verificationModel.imageFolderPath = result["fldGFrontIMG"];
            verificationModel.imageFolderPath = result["fldpath"];
            verificationModel.imageId = result["fldgfrontimg"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }


        public static OutwardItemViewModel ChequePagePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("notice", Locale.NoRecord);
            }

            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldimagefolder"];
            verificationModel.imageId = result["fldimagefilename"];
            verificationModel.pageTitle = pageSqlConfig.PageTitle;

            return verificationModel;
        }
        //20180821 OCS
        public static OutwardItemViewModel ChequePagePopulateViewModelOCS(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("notice", Locale.NoRecord);
            }
            if (result["fldreasoncode"].Equals("430"))
            {
                errors.Add("error", Locale.ChequeHasRejected);
            }
            if (result["fldlockuser"] != CurrentUser.Account.UserId)
            {
                errors.Add("error", "Cheque is currently being locked by other user");
                verificationModel.lockedCheque = true;
            }
            else
            {
                verificationModel.lockedCheque = false;
            }


            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldpath"];
            verificationModel.imageId = result["fldgfrontimg"];
            verificationModel.pageTitle = pageSqlConfig.PageTitle;
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        public static OutwardItemViewModel PrevChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (result.Count == 0)
            {
                errors.Add("warning", Locale.NoRecord);
            }
            else if (result["fldInwardItemId"].Equals(collection["fldInwardItemId"]))
            {
                errors.Add("warning", Locale.NoPreviousChequeAvailable);
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldImageFolder"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        public static OutwardItemViewModel PrevChequePopulateViewModelOCS(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (result.Count == 0)
            {
                errors.Add("warning", Locale.NoRecord);
            }
            else if (result["fldItemId"].Equals(collection["fldItemId"]))
            {
                errors.Add("warning", Locale.NoPreviousChequeAvailable);
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldpath"];
            verificationModel.imageId = result["fldgfrontimg"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }
        public static OutwardItemViewModel OutwardItemWithErrorMessages(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, List<string> errorMessages)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();

            verificationModel.errorMessages.Add("error", errorMessages[0]);

            //verificationModel.errorMessages = errorMessages.ToDictionary(x => "error", x => x);
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldpath"];
            return verificationModel;
        }
        public static OutwardItemViewModel OutwardItemWithWarnings(PageSqlConfig pageSqlConfig, Dictionary<string, string> result, List<string> warnings)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            verificationModel.errorMessages = warnings.ToDictionary(x => "warning", x => x);
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldimagefolder"];
            return verificationModel;
        }
       
        //Michelle 20200623
        public static OutwardItemViewModel UpdateRemarksChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            OutwardItemViewModel verificationModel = new OutwardItemViewModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (result.Count == 0)
            {
                errors.Add("warning", Locale.NoRecord);
            }
            else if (result["fldItemId"].Equals(collection["fldItemId"]))
            {
                errors.Add("warning", "Update Remarks Successfully!");
            }
            verificationModel.allFields = result;
            verificationModel.imageFolderPath = result["fldpath"];
            verificationModel.imageId = result["fldgfrontimg"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }

        //Michelle

    }
    }