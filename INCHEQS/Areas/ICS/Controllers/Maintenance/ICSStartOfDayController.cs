using INCHEQS.Areas.ICS.Models.ICSStartOfDay;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class ICSStartOfDayController : BaseController
    {
        private IICSStartOfDayDao ICSStartOfDayDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ICSStartOfDayController(IICSStartOfDayDao ICSStartOfDayDao, IAuditTrailDao auditTrailDao)
        {
            this.ICSStartOfDayDao = ICSStartOfDayDao;
            this.auditTrailDao = auditTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDay.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = ICSStartOfDayDao.getCurrentDate();
            ViewBag.ProcessDate = ICSStartOfDayDao.getNextProcessDate();
            ViewBag.confirmprocessDate = ICSStartOfDayDao.getConfirmProcessDate();
            auditTrailDao.SecurityLog("Access ICS Process Date (Start of Day) ", "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDay.INDEX)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                ICSStartOfDayDao.PerfromStartofDay(col, CurrentUser.Account);
                TempData["Notice"] = "Successfully Added ICS Start of The Day !";
                auditTrailDao.SecurityLog("Add - ICS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDay.INDEX)]
        [HttpPost()]
        public ActionResult CreateTemp(FormCollection col)
        {
            try
            {
                ICSStartOfDayDao.CreateICSStartOfDayTemp(col, CurrentUser.Account);
                TempData["Notice"] = "Successfully Added ICS Start of The Day to Temp Table !";
                auditTrailDao.SecurityLog("Add - ICS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
}