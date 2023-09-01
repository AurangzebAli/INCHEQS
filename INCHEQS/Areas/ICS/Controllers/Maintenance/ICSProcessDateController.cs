using INCHEQS.Areas.ICS.Models.ICSProcessDate;
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
    public class ICSProcessDateController : BaseController
    {
        private IICSProcessDateDao ICSProcessDateDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ICSProcessDateController(IICSProcessDateDao ICSProcessDateDao, IAuditTrailDao auditTrailDao)
        {
            this.ICSProcessDateDao = ICSProcessDateDao;
            this.auditTrailDao = auditTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSProcessDate.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = ICSProcessDateDao.getCurrentDate();
            ViewBag.ProcessDate = ICSProcessDateDao.getNextProcessDate();
            auditTrailDao.SecurityLog("Access ICS Process Date (Start of Day) ", "", TaskIdsICS.ICSProcessDate.INDEX, CurrentUser.Account);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIdsICS.ICSProcessDate.INDEX)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                ICSProcessDateDao.PerfromStartofDay(col, CurrentUser.Account);
                TempData["Notice"] = "Successfully Added ICS Process Date!!!";
                auditTrailDao.SecurityLog("Add - ICS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsICS.ICSProcessDate.INDEX, CurrentUser.Account);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    
}