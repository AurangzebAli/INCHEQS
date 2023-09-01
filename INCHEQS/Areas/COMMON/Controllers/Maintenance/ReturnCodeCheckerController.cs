using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class ReturnCodeCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IReturnCodeDao ReturnCodeDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public ReturnCodeCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IReturnCodeDao ReturnCodeDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.ReturnCodeDao = ReturnCodeDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ReturnCodeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ReturnCodeChecker.INDEX, "View_ReturnCodeChecker", "fldRejectCode"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult ReturnCode(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            ViewBag.ReturnCodeMaster = ReturnCodeDao.GetReturnCode(filter["fldRejectCode"]);
            ViewBag.ReturnCodeMasterTemp = ReturnCodeDao.GetReturnCodeTemp(filter["fldRejectCode"]);
            ViewBag.RejectType = ReturnCodeDao.ListRejectType();
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.ReturnCodeChecker.VERIFY;

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
                            case TaskIdsOCS.ReturnCodeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    ReturnCodeDao.MoveToReturnCodeMasterFromTemp(id, "Create");

                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);

                                    //Add Audit Trial
                                    ReturnCodeDao.DeleteReturnCodeMaster(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(id);
                                    auditTrailDao.Log("Edit Return Code - Before Update=> Return Code : " + before.fldRejectCode + " Return Desc : " + before.fldRejectDesc, CurrentUser.Account);

                                    ReturnCodeDao.MoveToReturnCodeMasterFromTemp(id, "Update");

                                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCodeTemp(id);
                                    auditTrailDao.Log("Edit Return Code - After Update=> Return Code : " + after.fldRejectCode + " Return Desc : " + after.fldRejectDesc, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                ReturnCodeDao.DeleteReturnCodeMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.ReturnCodeChecker.VERIFY;

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
                            case TaskIdsOCS.ReturnCodeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(id);
                                    auditTrailDao.Log("Edit Return Code - Before Update=> Return Code : " + before.fldRejectCode + " Return Desc : " + before.fldRejectDesc, CurrentUser.Account);

                                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCodeTemp(id);
                                    auditTrailDao.Log("Edit Return Code - After Update=> Return Code : " + after.fldRejectCode + " Return Desc : " + after.fldRejectDesc, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                ReturnCodeDao.DeleteReturnCodeMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["Action"];
                string id = col["fldRejectCode"];
                string sTaskId = TaskIdsOCS.ReturnCodeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    //Add Audit Trial
                    ReturnCodeDao.MoveToReturnCodeMasterFromTemp(id, "Create");

                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);

                    //Add Audit Trial
                    ReturnCodeDao.DeleteReturnCodeMaster(id);
                }
                else if (action.Equals("U"))
                {
                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(id);
                    auditTrailDao.Log("Edit Return Code - Before Update=> Return Code : " + before.fldRejectCode + " Return Desc : " + before.fldRejectDesc, CurrentUser.Account);

                    ReturnCodeDao.MoveToReturnCodeMasterFromTemp(id, "Update");

                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCodeTemp(id);
                    auditTrailDao.Log("Edit Return Code - After Update=> Return Code : " + after.fldRejectCode + " Return Desc : " + after.fldRejectDesc, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                ReturnCodeDao.DeleteReturnCodeMasterTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string id = col["fldRejectCode"];
                string action = col["Action"];

                string sTaskId = TaskIdsOCS.ReturnCodeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(id);
                    auditTrailDao.Log("Edit Return Code - Before Update=> Return Code : " + before.fldRejectCode + " Return Desc : " + before.fldRejectDesc, CurrentUser.Account);

                    INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCodeTemp(id);
                    auditTrailDao.Log("Edit Return Code - After Update=> Return Code : " + after.fldRejectCode + " Return Desc : " + after.fldRejectDesc, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.RetCodeChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                ReturnCodeDao.DeleteReturnCodeMasterTemp(id);

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