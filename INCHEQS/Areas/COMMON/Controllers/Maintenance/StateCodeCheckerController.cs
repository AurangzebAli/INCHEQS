using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.StateCode;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class StateCodeCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IStateCodeDao StateCodeDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public StateCodeCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IStateCodeDao StateCodeDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.StateCodeDao = StateCodeDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.StateCodeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.StateCodeChecker.INDEX, "View_StateCodeChecker", "fldStateCode"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult StateCode(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            ViewBag.StateCodeMaster = StateCodeDao.GetStateCode(filter["fldStateCode"]);
            ViewBag.StateCodeMasterTemp = StateCodeDao.GetStateCodeTemp(filter["fldStateCode"]);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.StateCodeChecker.VERIFY;
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.StateCodeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    StateCodeDao.MoveToStateCodeMasterFromTemp(id, "Create");

                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);

                                    StateCodeDao.DeleteStateCodeMaster(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    StateCodeModel before = StateCodeDao.GetStateCode(id);

                                    StateCodeDao.MoveToStateCodeMasterFromTemp(id, "Update");

                                    StateCodeModel after = StateCodeDao.GetStateCode(id);

                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                StateCodeDao.DeleteStateCodeMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.StateCodeChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.StateCodeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    StateCodeModel before = StateCodeDao.GetStateCode(id);

                                    StateCodeModel after = StateCodeDao.GetStateCodeTemp(id);

                                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                StateCodeDao.DeleteStateCodeMasterTemp(id);
                                break;
                        }
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

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["Action"];
                string id = col["fldStateCode"];
                string sTaskId = TaskIds.StateCodeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    StateCodeDao.MoveToStateCodeMasterFromTemp(id, "Create");

                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);

                    StateCodeDao.DeleteStateCodeMaster(id);
                }
                else if (action.Equals("U"))
                {
                    StateCodeModel before = StateCodeDao.GetStateCode(id);

                    StateCodeDao.MoveToStateCodeMasterFromTemp(id, "Update");

                    StateCodeModel after = StateCodeDao.GetStateCode(id);

                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve State Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                StateCodeDao.DeleteStateCodeMasterTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string id = col["fldStateCode"];
                string sTaskId = TaskIds.StateCodeChecker.VERIFY;
                string action = col["Action"];

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    StateCodeModel before = StateCodeDao.GetStateCode(id);

                    StateCodeModel after = StateCodeDao.GetStateCodeTemp(id);

                    string ActionDetails = MaintenanceAuditLogDao.StateCodeChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject State Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                StateCodeDao.DeleteStateCodeMasterTemp(id);
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