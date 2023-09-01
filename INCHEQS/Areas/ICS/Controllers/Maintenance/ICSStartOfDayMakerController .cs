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
    public class ICSStartOfDayMakerController : BaseController
    {
        private IICSStartOfDayMakerDao ICSStartOfDayMakerDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ICSStartOfDayMakerController(IICSStartOfDayMakerDao ICSStartOfDayMakerDao, IAuditTrailDao auditTrailDao)
        {
            this.ICSStartOfDayMakerDao = ICSStartOfDayMakerDao;
            this.auditTrailDao = auditTrailDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDayMaker1.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = ICSStartOfDayMakerDao.getCurrentDate();
            ViewBag.ProcessDate = ICSStartOfDayMakerDao.getNextProcessDate();
            ViewBag.confirmprocessDate = ICSStartOfDayMakerDao.getConfirmProcessDate();
            auditTrailDao.SecurityLog("Access ICS Process Date (Start of Day) ", "", TaskIdsICS.ICSStartOfDayMaker1.INDEX, CurrentUser.Account);
            return View();
        }

        
        [CustomAuthorize(TaskIds = TaskIdsICS.ICSStartOfDayMaker1.INDEX)]
        [HttpPost()]
        public ActionResult CreateTemp(FormCollection col)
        {
            try
            {
                List<String> errorMessages = ICSStartOfDayMakerDao.ValidateICSSOD();

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    ICSStartOfDayMakerDao.CreateICSStartOfDayTemp(col, CurrentUser.Account);
                    TempData["Notice"] = Locale.ICSStartOfDayUpdateVerified;
                    auditTrailDao.SecurityLog("Add to Temp Table - ICS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsICS.ICSStartOfDay.INDEX, CurrentUser.Account);
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