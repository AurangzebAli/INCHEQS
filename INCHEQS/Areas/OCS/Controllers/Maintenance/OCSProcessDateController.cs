using INCHEQS.Areas.OCS.Models.OCSProcessDate;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public class OCSProcessDateController : Controller
    {
        // GET: OCS/OCSProcessDate

        private static readonly ILog _log = LogManager.GetLogger(typeof(OCSProcessDateController));
        private IOCSProcessDateDao OCSProcessDateDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public OCSProcessDateController(IOCSProcessDateDao OCSProcessDateDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.OCSProcessDateDao = OCSProcessDateDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSProcessDate.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = OCSProcessDateDao.getCurrentDate();
            ViewBag.ProcessDate = OCSProcessDateDao.getNextProcessDate();
            auditTrailDao.SecurityLog("Access OCS Process Date (Start of Day) ", "", TaskIdsOCS.OCSProcessDate.INDEX, CurrentUser.Account);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.OCSProcessDate.INDEX)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //OCSProcessDateDao.UpdateProcessDate(col);
               // OCSProcessDateDao.CreateProcessDate(col, CurrentUser.Account);
                OCSProcessDateDao.PerfromStartofDay(col, CurrentUser.Account);

                TempData["Notice"] = "Successfully Added OCS Process Date!!!";
                //auditTrailDao.Log("Add - Process Date : " + col["processDate"], CurrentUser.Account);
                auditTrailDao.SecurityLog("Add - OCS Process Date (Start of Day) : " + col["processDate"], "", TaskIdsOCS.OCSProcessDate.INDEX, CurrentUser.Account);
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}