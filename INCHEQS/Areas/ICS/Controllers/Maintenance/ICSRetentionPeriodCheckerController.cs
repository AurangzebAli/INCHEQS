using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Common.Resources;
using INCHEQS.Areas.ICS.Models.ICSRetentionPeriod;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class ICSRetentionPeriodCheckerController : Controller
    {
        private readonly IICSRetentionPeriodDao ICSRetentionPeriodDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public ICSRetentionPeriodCheckerController(IICSRetentionPeriodDao ICSRetentionPeriodDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.ICSRetentionPeriodDao = ICSRetentionPeriodDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.ICSRetentionPeriodChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.ICSRetentionPeriodChecker.INDEX, "View_ICSRetentionPeriodChecker", "", "", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            ViewBag.ICSRetentionPeriodTemp = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult ICSRetentionPeriod(FormCollection collection)
        {
            ViewBag.ICSRetentionPeriodTemp = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();
            ViewBag.ICSRetentionPeriod = ICSRetentionPeriodDao.GetICSRetentionPeriod();
            ViewBag.ListInterval = ICSRetentionPeriodDao.ListIntervalType();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string sTaskId = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY;
            try
            {
                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                    ICSRetentionPeriodDao.DeleteICSRetentionPeriodMaster();
                    ICSRetentionPeriodDao.MovetoICSRetentionPeriodMasterfromTemp();
                    ICSRetentionPeriodDao.DeleteICSRetentionPeriodTemp();

                    ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                    string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriodChecker_EditTemplate(before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                    TempData["Notice"] = "Record(s) successfully approved";

                    ICSRetentionPeriodDao.DeleteICSRetentionPeriodChecker();
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

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string sTaskId = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY;
            try
            {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                    ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();

                    ICSRetentionPeriodDao.DeleteICSRetentionPeriodTemp();
                    ICSRetentionPeriodDao.DeleteICSRetentionPeriodChecker();
                    TempData["Notice"] = "Record(s) successfully rejected";

                    string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriodChecker_EditTemplate(before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);
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
        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            string sTaskId = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY;

            try
            {
                ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                ICSRetentionPeriodDao.DeleteICSRetentionPeriodMaster();
                ICSRetentionPeriodDao.MovetoICSRetentionPeriodMasterfromTemp();
                ICSRetentionPeriodDao.DeleteICSRetentionPeriodTemp();

                ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                TempData["Notice"] = "Record(s) successfully approved";

                ICSRetentionPeriodDao.DeleteICSRetentionPeriodChecker();

                string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriodChecker_EditTemplate(before, after, "Approve");
                auditTrailDao.SecurityLog("Approve ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            string sTaskId = TaskIdsICS.ICSRetentionPeriodChecker.VERIFY;

            try
            {
                ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();
            
                ICSRetentionPeriodDao.DeleteICSRetentionPeriodTemp();
                ICSRetentionPeriodDao.DeleteICSRetentionPeriodChecker();

                string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriodChecker_EditTemplate(before, after, "Reject");
                auditTrailDao.SecurityLog("Reject ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                TempData["Notice"] = "Record(s) successfully rejected";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

