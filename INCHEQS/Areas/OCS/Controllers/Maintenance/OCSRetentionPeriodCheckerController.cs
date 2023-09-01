using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Common.Resources;
using INCHEQS.Areas.OCS.Models.OCSRetentionPeriod;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public class OCSRetentionPeriodCheckerController : Controller
    {
        private readonly IOCSRetentionPeriodDao OCSRetentionPeriodDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public OCSRetentionPeriodCheckerController(IOCSRetentionPeriodDao OCSRetentionPeriodDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.OCSRetentionPeriodDao = OCSRetentionPeriodDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.OCSRetentionPeriodChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.OCSRetentionPeriodChecker.INDEX, "View_OCSRetentionPeriodChecker", "", "", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            ViewBag.OCSRetentionPeriodTemp = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult OCSRetentionPeriod(FormCollection collection)
        {
            ViewBag.OCSRetentionPeriodTemp = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();
            ViewBag.OCSRetentionPeriod = OCSRetentionPeriodDao.GetOCSRetentionPeriod();
            ViewBag.ListInterval = OCSRetentionPeriodDao.ListIntervalType();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY;
            try
            {
                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                    OCSRetentionPeriodDao.DeleteOCSRetentionPeriodMaster();
                    OCSRetentionPeriodDao.MovetoOCSRetentionPeriodMasterfromTemp();
                    OCSRetentionPeriodDao.DeleteOCSRetentionPeriodTemp();

                    OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                    string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriodChecker_EditTemplate(before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                    TempData["Notice"] = "Record(s) successfully approved";

                    OCSRetentionPeriodDao.DeleteOCSRetentionPeriodChecker();
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY;
            try
            {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                    OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();

                    OCSRetentionPeriodDao.DeleteOCSRetentionPeriodTemp();
                    OCSRetentionPeriodDao.DeleteOCSRetentionPeriodChecker();
                    TempData["Notice"] = "Record(s) successfully rejected";

                    string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriodChecker_EditTemplate(before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);
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
        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY;

            try
            {
                OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                OCSRetentionPeriodDao.DeleteOCSRetentionPeriodMaster();
                OCSRetentionPeriodDao.MovetoOCSRetentionPeriodMasterfromTemp();
                OCSRetentionPeriodDao.DeleteOCSRetentionPeriodTemp();

                OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                TempData["Notice"] = "Record(s) successfully approved";

                OCSRetentionPeriodDao.DeleteOCSRetentionPeriodChecker();

                string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriodChecker_EditTemplate(before, after, "Approve");
                auditTrailDao.SecurityLog("Approve OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.OCSRetentionPeriodChecker.VERIFY;

            try
            {
                OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();

                OCSRetentionPeriodDao.DeleteOCSRetentionPeriodTemp();
                OCSRetentionPeriodDao.DeleteOCSRetentionPeriodChecker();

                string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriodChecker_EditTemplate(before, after, "Reject");
                auditTrailDao.SecurityLog("Reject OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

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

