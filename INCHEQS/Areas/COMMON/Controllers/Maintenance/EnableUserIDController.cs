using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.EnableUserID;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Areas.COMMON.Models.Users;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class EnableUserIDController : BaseController
    {

        private readonly IEnableUserIDDao EnableUserIDDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IUserDao userDao;

        public EnableUserIDController(IEnableUserIDDao EnableUserIDDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao, IAuditTrailDao auditTrailDao, IUserDao userDao/*, ISecurityProfileDao securityProfileDao*/)
        {
            this.pageConfigDao = pageConfigDao;
            this.EnableUserIDDao = EnableUserIDDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            this.auditTrailDao = auditTrailDao;
            this.userDao = userDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.EnableUserID.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.EnableUserID.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.EnableUserID.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection col)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.EnableUserID.INDEX, "View_EnableUserID", "fldUserAbb"),
            col);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.EnableUserID.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string EnableUserID = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                if (!string.IsNullOrEmpty(EnableUserID))
                {
                    ViewBag.enable = EnableUserIDDao.GetEnableUserID(EnableUserID);
                }
                else
                {

                    ViewBag.enable = EnableUserIDDao.GetEnableUserID(filter["fldIdForEnable"]);
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIds.EnableUserID.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                CurrentUser.Account.TaskId = TaskIds.EnableUserID.INDEX;
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        
                        string userId = arrResult;

                        EnableUserIDDao.UpdateEnableUserID(userId);
                        UserModel update = userDao.CheckUserMasterByID("", userId, "UserId");
                        auditTrailDao.Log("Enable User: " + update.fldUserAbb, CurrentUser.Account);
                    }

                    TempData["Notice"] = Locale.SuccessfullyEnabled;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}