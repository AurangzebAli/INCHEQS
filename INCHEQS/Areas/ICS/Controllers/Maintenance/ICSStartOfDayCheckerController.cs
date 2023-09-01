using INCHEQS.Areas.ICS.Models.ICSStartOfDayMaker;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
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
    public class ICSStartOfDayCheckerController : BaseController
    {
        private IICSStartOfDayMakerDao ICSStartOfDayMakerDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ICSStartOfDayCheckerController(IICSStartOfDayMakerDao ICSStartOfDayMakerDao, IAuditTrailDao auditTrailDao)
        {
            this.ICSStartOfDayMakerDao = ICSStartOfDayMakerDao;
            this.auditTrailDao = auditTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDayChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = ICSStartOfDayMakerDao.getCurrentDate();
            ViewBag.ProcessDate = ICSStartOfDayMakerDao.getNextProcessDate();
            ViewBag.confirmprocessDate = ICSStartOfDayMakerDao.getConfirmProcessDate();
            auditTrailDao.SecurityLog("Access ICS Process Date (Start of Day) - Checker ", "", TaskIdsICS.ICSStartOfDayChecker.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDayChecker.INDEX)]
        [HttpPost()]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                if(col["confirmprocessDate"] != "")
                {
                    ICSStartOfDayMakerDao.MoveToInwardClearDateMasterFromTemp(col, CurrentUser.Account);

                    ICSStartOfDayMakerDao.DeleteICSStartOfDayTemp(CurrentUser.Account);

                    auditTrailDao.SecurityLog("Add - ICS Process Date (Start of Day) - Checker : " + col["processDate"], "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);

                    TempData["Notice"] = Locale.SuccessfullyCreateICSStartOfDay;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseConfirmICSStartOfDay;
                }
                return RedirectToAction("Index");
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDayChecker.INDEX)]
        [HttpPost()]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                if (col["confirmprocessDate"] != "")
                {
                    ICSStartOfDayMakerDao.DeleteICSStartOfDayTemp(CurrentUser.Account);

                    TempData["Notice"] = Locale.SuccessfullyRejectedICSStartOfDay;

                    auditTrailDao.SecurityLog("Reject - ICS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseConfirmICSStartOfDay;
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