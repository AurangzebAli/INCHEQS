using INCHEQS.Areas.ICS.Models.TransCode;

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
    public class TransCodeCheckerController : BaseController
    {
       
        private ITransCodeDao transCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public TransCodeCheckerController(ITransCodeDao transCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.transCodeDao = transCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TransCodeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            /*edit by umar to filterStateCode 3/5/2018*/
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TransCodeChecker.INDEX, "View_TransCodeMasterTemp"),
            
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string transCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string transCode = "";
            if (string.IsNullOrEmpty(transCodeParam))
            {
                transCode = filter["fldTransCode"].Trim();
            }
            else
            {
                transCode = transCodeParam;
            }


            ViewBag.transCodeTemp = transCodeDao.GetTransCodeTemp(transCode); //Done
            ViewBag.transCode = transCodeDao.GetTransCode(transCode); //Done


            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.TransCodeChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string transCode = arrResult.Remove(0, 1);

                        //Act based on task id
                        switch (status)
                        {
                            case "D":
                                string ActionDetails = MaintenanceAuditLogDao.TransCodeChecker_DeleteTemplate(transCode, "Approve");
                                auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                                transCodeDao.DeleteTransCode(transCode);
                                transCodeDao.DeleteTransCodeTemp(transCode);
                                break;

                            default:
                                if (status.Equals("A"))
                                {
                                    transCodeDao.MoveTransCodeFromTemp(transCode, status);

                                    string ActionDetails1 = MaintenanceAuditLogDao.TransCodeChecker_AddTemplate(transCode, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails1, sTaskId, CurrentUser.Account);
                                }
                                else if (status.Equals("U"))
                                {
                                    TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(transCode);
                                    
                                    transCodeDao.MoveTransCodeFromTemp(transCode, status);

                                    TransCodeModel after = MaintenanceAuditLogDao.GetTransCode(transCode);

                                    string ActionDetails2 = MaintenanceAuditLogDao.TransCodeChecker_EditTemplate(transCode, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails2, sTaskId, CurrentUser.Account);
                                }
                                transCodeDao.DeleteTransCodeTemp(transCode);
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

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.TransCodeChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string transCode = arrResult.Remove(0, 1);

                        if (status.Equals("A"))
                        {
                            string ActionDetails1 = MaintenanceAuditLogDao.TransCodeChecker_AddTemplate(transCode, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("U"))
                        {
                            TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(transCode);

                            TransCodeModel after = MaintenanceAuditLogDao.GetTransCodeTemp(transCode);

                            string ActionDetails2 = MaintenanceAuditLogDao.TransCodeChecker_EditTemplate(transCode, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.TransCodeChecker_DeleteTemplate(transCode, "Reject");
                            auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        transCodeDao.DeleteTransCodeTemp(transCode);

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

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Approve2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.TransCodeChecker.VERIFY;
                List<string> arrResults = new List<string>();

                string status = col["Status"];
                string transCode = col["TransCode"];

                //Act based on task id
                switch (status)
                {
                    case "D":
                        string ActionDetails = MaintenanceAuditLogDao.TransCodeChecker_DeleteTemplate(transCode, "Approve");
                        auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                        transCodeDao.DeleteTransCode(transCode);
                        transCodeDao.DeleteTransCodeTemp(transCode);
                        break;

                    default:
                        if (status.Equals("A"))
                        {
                            transCodeDao.MoveTransCodeFromTemp(transCode, status);

                            string ActionDetails1 = MaintenanceAuditLogDao.TransCodeChecker_AddTemplate(transCode, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("U"))
                        {
                            TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(transCode);

                            transCodeDao.MoveTransCodeFromTemp(transCode, status);

                            TransCodeModel after = MaintenanceAuditLogDao.GetTransCode(transCode);

                            string ActionDetails2 = MaintenanceAuditLogDao.TransCodeChecker_EditTemplate(transCode, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Transaction Code", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                        transCodeDao.DeleteTransCodeTemp(transCode);
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

        [CustomAuthorize(TaskIds = TaskIds.TransCodeChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult Reject2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.TransCodeChecker.VERIFY;

                List<string> arrResults = new List<string>();

                string status = col["Status"];
                string transCode = col["TransCode"];

                //Act based on task id
                if (status.Equals("A"))
                {
                    string ActionDetails1 = MaintenanceAuditLogDao.TransCodeChecker_AddTemplate(transCode, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails1, sTaskId, CurrentUser.Account);
                }
                else if (status.Equals("U"))
                {
                    TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(transCode);

                    TransCodeModel after = MaintenanceAuditLogDao.GetTransCodeTemp(transCode);

                    string ActionDetails2 = MaintenanceAuditLogDao.TransCodeChecker_EditTemplate(transCode, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails2, sTaskId, CurrentUser.Account);
                }
                else if (status.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.TransCodeChecker_DeleteTemplate(transCode, "Reject");
                    auditTrailDao.SecurityLog("Reject Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);
                }

                transCodeDao.DeleteTransCodeTemp(transCode);

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