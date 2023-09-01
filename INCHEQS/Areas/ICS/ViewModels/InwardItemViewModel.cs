using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.ViewModels {
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



        #region HostValidation Paramters
        public string posPayType { get; set; }
        public string clearDate { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string Processname { get; set; }
        public string bankCode { get; set; }
        public string status { get; set; }
        public string  Genuiness { get; set; }


        #endregion

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