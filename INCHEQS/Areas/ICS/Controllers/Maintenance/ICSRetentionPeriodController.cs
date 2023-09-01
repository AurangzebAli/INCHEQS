using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.ICS.Models.ICSRetentionPeriod;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{

    public class ICSRetentionPeriodController : BaseController
    {
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IICSRetentionPeriodDao ICSRetentionPeriodDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public ICSRetentionPeriodController(IICSRetentionPeriodDao ICSRetentionPeriodDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.ICSRetentionPeriodDao = ICSRetentionPeriodDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        
        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriod.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.ICSRetentionPeriod = ICSRetentionPeriodDao.GetICSRetentionPeriod();
            ViewBag.ICSRetentionPeriodTemp = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();
            ViewBag.ListInterval = ICSRetentionPeriodDao.ListIntervalType();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSRetentionPeriod.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsICS.ICSRetentionPeriod.UPDATE;

                List<string> errorMessages = ICSRetentionPeriodDao.ValidateICSRetentionPeriod(col);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;

                }
                else
                {
                    if (ICSRetentionPeriodDao.CheckICSRetentionPeriodTemp() == false)
                    {
                        if ("N".Equals(systemProfile))
                        {
                            ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                            ICSRetentionPeriodDao.UpdateICSRetentionPeriod(col);

                            ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                            string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriod_EditTemplate(before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                        else
                        {
                            ICSRetentionPeriodDao.CreateICSRetentionMasterTemp(col);
                            if (ICSRetentionPeriodDao.CheckICSRetentionPeriodTemp() == true)
                            {
                                ICSRetentionPeriodModel before = ICSRetentionPeriodDao.GetICSRetentionPeriod();

                                ICSRetentionPeriodDao.CreateICSRetentionPeriodChecker("ICS Retention Period", "Update", CurrentUser.Account.UserId);

                                ICSRetentionPeriodModel after = ICSRetentionPeriodDao.GetICSRetentionPeriodTemp();

                                string ActionDetail = MaintenanceAuditLogDao.ICSRetentionPeriod_EditTemplate(before, after, "Edit");
                                auditTrailDao.SecurityLog("Edit ICS Retention Period", ActionDetail, sTaskId, CurrentUser.Account);
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
