using INCHEQS.Models.SearchPageConfig;
using System.Data;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace INCHEQS.Models.Report {
    public interface IReportService{
        Task<DataTable> getReportBasedOnPageConfigAsync(PageSqlConfig pageSqlConfig, FormCollection collection);

        byte[] renderReportBasedOnConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] renderReportBasedOnConfigForPage(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] renderChequeReportWithImageBasedOnConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] PrintChequeConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        Task<ReportModel> GetReportConfigByTaskIdAsync(string taskId);

        //Azim start
        Task<ReportModel> GetReportConfigAsync(PageSqlConfig pageSqlConfig, string reportType);
        //Azim End

        string CHistory(ReportModel reportModel, ConfigTable configTable, FormCollection collection,string bankcode);

        string checkClearDate(string pageSqlConfig, FormCollection collection);

        List<ReportModel> ListBankHostStatus();
    }
}