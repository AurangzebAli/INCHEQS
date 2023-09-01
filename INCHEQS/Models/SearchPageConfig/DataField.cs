using System;
using System.Collections;

namespace INCHEQS.Models.SearchPageConfig
{

    public class DataField
    {

        public string taskId { get; set; }
        public string pageTitle { get; set; }

        public string databaseViewId { get; set; }
        public string queryId { get; set; }

        public string fieldId { get; set; }
        public string fieldType { get; set; }
        public string fieldLabel { get; set; }
        public string fieldDefaultValue { get; set; }
        public ArrayList keyValueFieldList { get; set; }

        public string ValueTextQueryForOption { get; set; }
        public int length { get; set; }


        public string isResultParameter { get; set; }
        public string isFilter { get; set; }
        public string isResult { get; set; }
        public string isEnabled { get; set; }
        public string isReadOnly { get; set; }

        public string value { get; set; }

        public Int32 ordering { get; set; }

        public DataField Clone()
        {
            return (DataField) this.MemberwiseClone();
        }

        public override string ToString() {
            return value;
        }

    }
}