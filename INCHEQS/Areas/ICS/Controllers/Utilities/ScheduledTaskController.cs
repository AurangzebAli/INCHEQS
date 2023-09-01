using INCHEQS.Areas.ICS.Models.ScheduledTask;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{
    public class ScheduledTaskController : BaseController{

        private readonly IAuditTrailDao auditTrailDao;
        private readonly IScheduledTaskDao scheduledTaskDao;
        public ScheduledTaskController(IScheduledTaskDao scheduledTaskDao, IAuditTrailDao auditTrailDao) {
            this.scheduledTaskDao = scheduledTaskDao;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: ICS/ScheduledTask
        [CustomAuthorize(TaskIds = TaskIds.TaskScheduler.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.ScheduledTask = scheduledTaskDao.GetScheduledTaskName();
            ViewBag.ScheduledTimer = scheduledTaskDao.GetScheduledTimer();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TaskScheduler.INDEX)]
        public ActionResult Create(FormCollection col) {

            if (col["processName"] != "") {
                scheduledTaskDao.InsertScheduledTaskTimer(col, CurrentUser.Account);
                TempData["Notice"] = Locale.SuccessfullycreatenewScheduledTask;
            }else {
                TempData["Warning"] = Locale.PleaseSelectProcess;
            }
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.TaskScheduler.INDEX)]
        public ActionResult Delete(string schedulId) {
            
                scheduledTaskDao.DeleteScheduledTask(schedulId);
                TempData["Notice"] = Locale.SuccessfullyDeleted;
            
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.TaskScheduler.INDEX)]
        public ActionResult ScheduledHistory(string historyId) {
            
            ViewBag.ScheduledHistory = scheduledTaskDao.GetScheduledHistory(historyId);

            return View();
        }
    }
}