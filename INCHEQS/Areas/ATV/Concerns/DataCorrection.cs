using INCHEQS.Areas.ATV.Models.DataCorrection;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace INCHEQS.Areas.ATV.Concerns
{
    public class DataCorrection
    {
        public static DataCorrectionModel NextChequePopulateViewModel(QueueSqlConfig pageSqlConfig, Dictionary<string, string> result, FormCollection collection)
        {
            DataCorrectionModel verificationModel = new DataCorrectionModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            if (result.Count == 0)
            {
                errors.Add("error", Locale.NoRecord);
            }
            else if (result["fldInwardItemId"].Equals(collection["fldInwardItemId"]))
            {
                errors.Add("warning", Locale.NoNextChequeAvailable);
            }
            verificationModel.allFields = result;
            verificationModel.imageDatePath = result["fldCropDatePath"];
            verificationModel.errorMessages = errors;
            return verificationModel;
        }
    }
}