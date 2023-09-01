using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.OCS.Models.OCSRetentionPeriod;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{

    public class OCSRetentionPeriodController : BaseController
    {
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IOCSRetentionPeriodDao OCSRetentionPeriodDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public OCSRetentionPeriodController(IOCSRetentionPeriodDao OCSRetentionPeriodDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.OCSRetentionPeriodDao = OCSRetentionPeriodDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        
        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriod.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.OCSRetentionPeriod = OCSRetentionPeriodDao.GetOCSRetentionPeriod();
            ViewBag.OCSRetentionPeriodTemp = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();
            ViewBag.ListInterval = OCSRetentionPeriodDao.ListIntervalType();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSRetentionPeriod.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.OCSRetentionPeriod.UPDATE;

                List<string> errorMessages = OCSRetentionPeriodDao.ValidateOCSRetentionPeriod(col);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;

                }
                else
                {
                    if (OCSRetentionPeriodDao.CheckOCSRetentionPeriodTemp() == false)
                    {
                        if ("N".Equals(systemProfile))
                        {
                            OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                            OCSRetentionPeriodDao.UpdateOCSRetentionPeriod(col);

                            OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                            string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriod_EditTemplate(before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                        else
                        {
                            OCSRetentionPeriodDao.CreateOCSRetentionMasterTemp(col);
                            if (OCSRetentionPeriodDao.CheckOCSRetentionPeriodTemp() == true)
                            {
                                OCSRetentionPeriodModel before = OCSRetentionPeriodDao.GetOCSRetentionPeriod();

                                OCSRetentionPeriodDao.CreateOCSRetentionPeriodChecker("OCS Retention Period", "Update", CurrentUser.Account.UserId);

                                OCSRetentionPeriodModel after = OCSRetentionPeriodDao.GetOCSRetentionPeriodTemp();

                                string ActionDetail = MaintenanceAuditLogDao.OCSRetentionPeriod_EditTemplate(before, after, "Edit");
                                auditTrailDao.SecurityLog("Edit OCS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);
                            }
                            TempData["Notice"] = Locale.SecurityProfileSucessfullyAddedToUpdate;
                            

                        }
                    }

                    else
                    {
                        TempData["Warning"] = Locale.SecurityProfileRecordPendingforApproval;
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}
