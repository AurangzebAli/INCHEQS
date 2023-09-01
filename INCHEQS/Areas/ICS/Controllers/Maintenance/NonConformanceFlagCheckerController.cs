using INCHEQS.Areas.ICS.Models.NonConformanceFlag;

using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class NonConformanceFlagCheckerController : BaseController
    {
       
        private INonConformanceFlagDao ncFlagDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public NonConformanceFlagCheckerController(INonConformanceFlagDao ncFlagDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.ncFlagDao = ncFlagDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.NCFlagChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            /*edit by umar to filterStateCode 3/5/2018*/
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.NCFlagChecker.INDEX, "View_NCFChecker"),
            
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string ncfCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string ncfCode = "";
            if (string.IsNullOrEmpty(ncfCodeParam))
            {
                ncfCode = filter["fldNCFCode"].Trim();
            }
            else
            {
                ncfCode = ncfCodeParam;
            }


            ViewBag.ncft = ncFlagDao.GetNCFCodeTemp(ncfCode);
            ViewBag.ncf = ncFlagDao.GetNCFCode(ncfCode);


            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.NCFlagChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string ncfCode = arrResult.Remove(0, 1);

                        //Act based on task id
                        switch (status)
                        {
                            case "D":
                                string ActionDetails = MaintenanceAuditLogDao.NCFChecker_DeleteTemplate(ncfCode, "Approve");
                                auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                                ncFlagDao.DeleteNCFCode(ncfCode);
                                ncFlagDao.DeleteNCFCodeTemp(ncfCode);

                                break;

                            default:
                                if (status.Equals("A"))
                                {
                                    ncFlagDao.MoveNCFCodeFromTemp(ncfCode, status);

                                    string ActionDetails1 = MaintenanceAuditLogDao.NCFChecker_AddTemplate(ncfCode, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails1, sTaskId, CurrentUser.Account);
                                }
                                else if (status.Equals("U"))
                                {
                                    NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                                    ncFlagDao.MoveNCFCodeFromTemp(ncfCode, status);

                                    NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                                    string ActionDetails2 = MaintenanceAuditLogDao.NCFChecker_EditTemplate(ncfCode, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails2, sTaskId, CurrentUser.Account);
                                }
                                
                                ncFlagDao.DeleteNCFCodeTemp(ncfCode);
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
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

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.NCFlagChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string ncfCode = arrResult.Remove(0, 1);

                        if (status.Equals("A"))
                        {
                            string ActionDetails1 = MaintenanceAuditLogDao.NCFChecker_AddTemplate(ncfCode, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("U"))
                        {
                            NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                            NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCodeTemp(ncfCode);

                            string ActionDetails2 = MaintenanceAuditLogDao.NCFChecker_EditTemplate(ncfCode, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.NCFChecker_DeleteTemplate(ncfCode, "Reject");
                            auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        ncFlagDao.DeleteNCFCodeTemp(ncfCode);

                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
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

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Approve2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.NCFlagChecker.VERIFY;

                List<string> arrResults = new List<string>();

                string status = col["Status"];
                string ncfCode = col["NCFCode"];

                //Act based on task id
                switch (status)
                {
                    case "D":
                        string ActionDetails = MaintenanceAuditLogDao.NCFChecker_DeleteTemplate(ncfCode, "Approve");
                        auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                        ncFlagDao.DeleteNCFCode(ncfCode);
                        ncFlagDao.DeleteNCFCodeTemp(ncfCode);
                        break;
                    default:
                        if (status.Equals("A"))
                        {
                            ncFlagDao.MoveNCFCodeFromTemp(ncfCode, status);

                            string ActionDetails1 = MaintenanceAuditLogDao.NCFChecker_AddTemplate(ncfCode, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("U"))
                        {
                            NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                            ncFlagDao.MoveNCFCodeFromTemp(ncfCode, status);

                            NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                            string ActionDetails2 = MaintenanceAuditLogDao.NCFChecker_EditTemplate(ncfCode, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Non-Conformance Flag", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                        ncFlagDao.DeleteNCFCodeTemp(ncfCode);
                        break;
                        
                }

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlagChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Reject2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.NCFlagChecker.VERIFY;

                List<string> arrResults = new List<string>();

                string status = col["Status"];
                string ncfCode = col["NCFCode"];

                //Act based on task id
                if (status.Equals("A"))
                {
                    string ActionDetails1 = MaintenanceAuditLogDao.NCFChecker_AddTemplate(ncfCode, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails1, sTaskId, CurrentUser.Account);
                }
                else if (status.Equals("U"))
                {
                    NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(ncfCode);

                    NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCodeTemp(ncfCode);

                    string ActionDetails2 = MaintenanceAuditLogDao.NCFChecker_EditTemplate(ncfCode, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails2, sTaskId, CurrentUser.Account);
                }
                else if (status.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.NCFChecker_DeleteTemplate(ncfCode, "Reject");
                    auditTrailDao.SecurityLog("Reject Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);
                }

                ncFlagDao.DeleteNCFCodeTemp(ncfCode);

                TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}