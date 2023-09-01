using INCHEQS.Areas.OCS.Models.ClearingItem;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ClearingSummary
{
    public class ClearingItemsSummaryController : Controller
    {
        private IPageConfigDao pageConfigDao;

        public ClearingItemsSummaryController(IPageConfigDao pageConfigDao)
        {
            this.pageConfigDao = pageConfigDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ClearingSummary.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ClearingSummary.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ClearingSummary.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ClearingSummary.INDEX, "view_ClearingSummary", "fldDateBatch", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                collection);
            return View();
        }
    }
}