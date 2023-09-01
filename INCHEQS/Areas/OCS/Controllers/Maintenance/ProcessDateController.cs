using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.ProcessDate;
using INCHEQS.Resources;
using INCHEQS.Security;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{

    public class ProcessDateController : BaseController
    {

        private static readonly ILog _log = LogManager.GetLogger(typeof(ProcessDateController));
        private IProcessDateDao ProcessDateDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public ProcessDateController(IProcessDateDao ProcessDateDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.ProcessDateDao = ProcessDateDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        [CustomAuthorize(TaskIds = "700001")]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = ProcessDateDao.getCurrentDate();
            ViewBag.ProcessDate = ProcessDateDao.getNextProcessDate();
            return View();
        }

        [CustomAuthorize(TaskIds = "700001")]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                ProcessDateDao.UpdateProcessDate(col);
                ProcessDateDao.CreateProcessDate(col, CurrentUser.Account);

                TempData["Notice"] = "Successfully Added Process Date!!!";
                auditTrailDao.Log("Add - Process Date : " + col["processDate"], CurrentUser.Account);

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
