using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;

namespace INCHEQS.Areas.RPA.Controller
{
    public class BotProfileController : BaseController
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;

        public BotProfileController(IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            //this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            //this.systemProfileDao = systemProfileDao;
           // this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.BotProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BotProfile.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BotProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BotProfile.INDEX, "View_ApprovedUser", "fldUserAbb", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }
    }
}