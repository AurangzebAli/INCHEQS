using INCHEQS.Models.SearchPageConfig;
using System;
using System.Collections.Generic;
using System.Data;

namespace INCHEQS.Areas.ATV.ViewModels
{

    public class ResultPageViewModel
    {
        public DataTable TableHeaders { get; set; }
        public List<List<DataField>> TableData { get; set; }
        public int CurrentPage { get; set; }
        public int TotalRecordCount { get; set; }
        public int TotalOutstandingCount { get; set; }
        public int TotalPage { get; set; }
        public string PageTitle { get; set; }
        public string Taskid { get; set   ; } = "";
        public double GrandTotalAmount { get; set; }
        public DataTable ResultTable { get; set; }
        public double Amount { get; set; }
        public int TotalImportItem { get; set; }
        //public string RowActionUrl { get; set; }

    }

}