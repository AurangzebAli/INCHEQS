using INCHEQS.Models.SearchPageConfig;
using System.Data;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace INCHEQS.Models.Report.OCS
{
    public interface IReportServiceOCS
    {
        Task<DataTable> getReportBasedOnPageConfigAsync(PageSqlConfig pageSqlConfig, FormCollection collection);

        byte[] renderReportBasedOnConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);


        byte[] renderReportBasedOnConfigForPage(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] renderChequeReportWithImageBasedOnConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] ReturnChequeAdviceConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] PrintChequeConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        byte[] PrintChequeSearchConfig(ReportModel reportModel, FormCollection collection, string path, string reportType, out string mimeType);

        void ReturnChequeAdviceConfig(ReportModel reportModel, string itemId, string path, string reportType, out string mimeType);

        Task<ReportModel> GetReportConfigByTaskIdAsync(string taskId);

        Task<ReportModel> GetReportConfigAsync(PageSqlConfig pageSqlConfig);

        string CHistory(ReportModel reportModel, ConfigTable configTable, FormCollection collection, string bankcode);

    }
}