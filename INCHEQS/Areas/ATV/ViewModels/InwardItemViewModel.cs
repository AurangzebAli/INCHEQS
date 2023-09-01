using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ATV.ViewModels {
    public class InwardItemViewModel {

        public string pageTitle { get; set; }
        public string inwardItemId { get; set; }
        public string amount { get; set; }
        public string imageFolderPath { get; set; }
        public string imageId { get; set; }
        public string UICNo { get; set; }

        public string chequeNo { get; set; }
        public string accountNo { get; set; }
        public string bankNo { get; set; }
        public string branchNo { get; set; }

        public Dictionary<string, string> allFields { get; set; }
        public Dictionary<string, string> errorMessages { get; set; }

        public string getFormatImageFolderPath() {
            return imageFolderPath.Replace("\\\\", @"\").Replace("\\", "\\\\");
        }

        public InwardItemViewModel() {
            errorMessages = new Dictionary<string, string>();
            allFields = new Dictionary<string, string>();
        }

        public string getField(string key) {
            if (allFields.ContainsKey(key)) {
                return allFields[key];
            } else {
                return "";
            }
        }

    }
}