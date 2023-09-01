using INCHEQS.Areas.OCS.ViewModels;
using INCHEQS.Security.Account;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.SearchPageConfig {
    public interface IPageConfigDaoOCS {

        Task<ResultPageViewModel> getResultListFromDatabaseViewAsync(PageSqlConfig sqlPageConfig, FormCollection collection);
        Task<List<Dictionary<string, string>>> GetResultDashboardForDivIdAsync(string divId, PageSqlConfig pageSqlConfig, FormCollection collection);
        Task<SearchPageViewModel> GetSearchFormModelFromConfigAsync(AccountModel currentUserAccount , PageSqlConfig pageSqlConfig);
        Task<ConfigTable> GetConfigTableAsync(string taskId, string viewId = null);
        Task<SearchPageViewModel> GetPageConfigViewModelFromConfigAsync(PageSqlConfig pageSqlConfig);
        Task<QueueSqlConfig> GetQueueConfigAsync(string taskId, AccountModel account);
        Task<QueueSqlConfig> GetQueueConfigAsyncNew(string taskId, AccountModel account);
        Task<ResultPageViewModel> getQueueResultListFromDatabaseViewAsync(QueueSqlConfig queSqlConfig, FormCollection collection);

        ResultPageViewModel getResultListFromDatabaseView(PageSqlConfig sqlPageConfig, FormCollection collection);
        List<Dictionary<string, string>> GetResultDashboardForDivId(string divId, PageSqlConfig pageSqlConfig, FormCollection collection);
        SearchPageViewModel GetSearchFormModelFromConfig(AccountModel currentUserAccount, PageSqlConfig pageSqlConfig);
        ConfigTable GetConfigTable(string taskId , string viewId = null);
        SearchPageViewModel GetPageConfigViewModelFromConfig(PageSqlConfig pageSqlConfig);
        QueueSqlConfig GetQueueConfig(string taskId, AccountModel account);
        QueueSqlConfig GetQueueConfigNew(string taskId, AccountModel account);
        ResultPageViewModel getQueueResultListFromDatabaseView(QueueSqlConfig queSqlConfig, FormCollection collection);
        Task<List<Dictionary<string, string>>> updateQueueSessionAsync(string fldTaskId, string fldBankCode, string fldCapturingbranch, string fldScannerId, string fldBatchNumber, string fldUIC, string fldUserId);
        Task<List<Dictionary<string, string>>> updateQueueClearingSessionAsync(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate);
        Task<List<Dictionary<string, string>>> updateQueueSuspendSessionAsync(string scannerbatch, string flduserid, string flduic, string userbankcode, string fldscannerid, string fldcapturingdate);

    }
}
