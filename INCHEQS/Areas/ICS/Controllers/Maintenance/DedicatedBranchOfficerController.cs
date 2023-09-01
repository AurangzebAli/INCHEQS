
using INCHEQS.Areas.ICS.Models.DedicatedBranchOfficer;
using INCHEQS.Areas.ICS.Models.DedicatedBranchDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class DedicatedBranchOfficerController : Controller
    {
        private iDedicatedBranchOfficerDao dedicatedBranchOfficerDao;
        private IPageConfigDao pageConfigDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IUserDao userDao;
        private IDedicatedBranchDayDao DedicatedBranchDayDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;

        public DedicatedBranchOfficerController(iDedicatedBranchOfficerDao DedicatedBranchOfficerDao, IPageConfigDao pageConfigDao, ISecurityProfileDao securityProfileDao, IDedicatedBranchDayDao DedicatedBranchDayDao, IUserDao userDao, ISystemProfileDao systemProfileDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao)
        {
            this.dedicatedBranchOfficerDao = DedicatedBranchOfficerDao;
            this.DedicatedBranchDayDao = DedicatedBranchDayDao;
            this.pageConfigDao = pageConfigDao;
            this.securityProfileDao = securityProfileDao;
            this.searchPageService = searchPageService;
            this.auditTrailDao = auditTrailDao;
            this.userDao = userDao;
            this.systemProfileDao = systemProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchOfficer.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.DedicatedBranchOfficer.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchOfficer.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            try
            {
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.DedicatedBranchOfficer.INDEX, "View_DedicatedBranchDate", "fldUserAbb", "fldBankCode=@fldBankCode and userType='CCU' and fldAdminFlag<>'Verifier'", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
                collection);
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchOfficer.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string id = null)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            if (!string.IsNullOrEmpty(id))
            {
                ViewBag.User = userDao.GetUser(id);
            }
            else
            {
                ViewBag.User = userDao.GetUser(filter["fldUserId"]);
            }
            ViewBag.ClearDate = userDao.GetClearingDate();
            ViewBag.UserType2 = DedicatedBranchDayDao.getUserProfile(@ViewBag.User.fldUserAbb);
            ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
            ViewBag.City = userDao.ListCity();
            ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode,@ViewBag.User.fldUserAbb);
            ViewBag.GetOfficer = userDao.GetOfficerId(@ViewBag.User.fldUserAbb);
            ViewBag.BranchAvailableList = dedicatedBranchOfficerDao.ListAvailableVerifier(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            ViewBag.BranchSelectedList = dedicatedBranchOfficerDao.ListSelectedVerifier(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            ViewBag.VerificationClass = userDao.ListVerificationClass();
            ViewBag.Security = securityProfileDao.GetSecurityProfile();

            //Check if LoginAD "Y" to display password
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchOfficer.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col, string submit)
        {
            try
            {
                string user = "No user assigned";
                if (!String.IsNullOrEmpty(col["selectedTask"]))
                {
                    user = col["selectedTask"].ToString();
                }
                    UserModel before = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit Dedicated Branch Officer - Update => Officer Id:" + before.fldUserAbb + " User Desc : " + before.fldUserDesc , CurrentUser.Account);

                    dedicatedBranchOfficerDao.InsertUserDedicatedBranch(col);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;

                    UserModel after = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit Dedicated Branch Officer - Succesfully Update=> User Abb : " + after.fldUserAbb + " User Desc : " + after.fldUserDesc + " User assigned :" + user, CurrentUser.Account);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;
                

                return RedirectToAction("Edit", new { id = col["fldUserId"] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}