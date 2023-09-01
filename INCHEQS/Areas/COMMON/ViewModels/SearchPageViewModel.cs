using INCHEQS.Models.SearchPageConfig;
using System.Collections.Generic;
using System.Linq;

namespace INCHEQS.Areas.COMMON.ViewModels {
    public class SearchPageViewModel
    {
        public string PageTitle { get; set; }
        public string TaskId { get; set; }
        public List<DataField> FormFields { get; set; }
        public void setDefaultValueForField(string fieldId , string fieldValue) {
            DataField dataField = (from data in FormFields where string.Compare(data.fieldId, fieldId) == 0 select data).FirstOrDefault();
            if (dataField != null) {
                dataField.fieldDefaultValue = fieldValue;
            }
        }
    }
}