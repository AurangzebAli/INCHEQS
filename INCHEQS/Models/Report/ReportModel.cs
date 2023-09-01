using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Report {
    public class ReportModel {
        public string reportId { get; set; }
        public string taskId { get; set; }
        public string viewId { get; set; }
        public List<PageSqlConfig> sqlConfigForDataSet { get; set; }
        public string reportPath { get; set; }
        public string reportTitle { get; set; }
        public string extentionFilename { get; set; }
        public string extensionType { get; set; }
        public string dataSetName { get; set; }
        public string printReportParam { get; set; }
        public string orientation { get; set; }

        public string bankHostStatusID { get; set; }
        public string bankHostStatusDesc { get; set; }

    }
}