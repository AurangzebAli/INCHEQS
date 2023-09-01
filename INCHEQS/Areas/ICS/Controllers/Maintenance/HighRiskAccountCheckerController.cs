using INCHEQS.Areas.ICS.Models.HighRiskAccount;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
//using INCHEQS.Models.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class HighRiskAccountCheckerController : BaseController{

        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private readonly ISystemProfileDao systemProfileDao;
        private IHighRiskAccountDao highRiskAccountDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public HighRiskAccountCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHighRiskAccountDao highRiskAccountDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.highRiskAccountDao = highRiskAccountDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccountChecker.INDEX)]
        [GenericFilter(AllowHttpGet =true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.HighRiskAccountChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccountChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.HighRiskAccountChecker.INDEX, "View_HighRiskAccount"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccountChecker.VERIFY)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.HighRiskAccountChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string highRiskAcc = arrResult.Remove(0, 1);

                        //Act based on task id
                        switch (status)
                        {
                            case "D":
                                string ActionDetails = MaintenanceAuditLogDao.HighRiskAccountChecker_DeleteTemplate(highRiskAcc, "Approve");
                                auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                                highRiskAccountDao.DeleteFromHighRiskAccount(highRiskAcc);
                                highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
                                break;

                            default:
                                if (status.Equals("A"))
                                {
                                    highRiskAccountDao.MoveHighRiskAccoountFromTemp(highRiskAcc, status);

                                    string ActionDetails1 = MaintenanceAuditLogDao.HighRiskAccountChecker_AddTemplate(highRiskAcc, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails1, sTaskId, CurrentUser.Account);
                                }
                                else if (status.Equals("U"))
                                {
                                    HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Master");

                                    highRiskAccountDao.MoveHighRiskAccoountFromTemp(highRiskAcc, status);

                                    HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Master");

                                    string ActionDetails2 = MaintenanceAuditLogDao.HighRiskAccountChecker_EditTemplate(highRiskAcc, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails2, sTaskId, CurrentUser.Account);
                                }
                                highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
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

        public ActionResult Approve2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.HighRiskAccountChecker.VERIFY;
                string status = col["Status"];
                string highRiskAcc = col["fldHighRiskAccount"];

                //Act based on task id
                switch (status)
                {
                    case "D":

                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccountChecker_DeleteTemplate(highRiskAcc, "Approve");
                        auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                        highRiskAccountDao.DeleteFromHighRiskAccount(highRiskAcc);
                        highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
                        break;

                    default:
                        if (status.Equals("A"))
                        {
                            highRiskAccountDao.MoveHighRiskAccoountFromTemp(highRiskAcc, status);

                            string ActionDetails1 = MaintenanceAuditLogDao.HighRiskAccountChecker_AddTemplate(highRiskAcc, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                        else if (status.Equals("U"))
                        {
                            HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Master");

                            highRiskAccountDao.MoveHighRiskAccoountFromTemp(highRiskAcc, status);

                            HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Master");

                            string ActionDetails2 = MaintenanceAuditLogDao.HighRiskAccountChecker_EditTemplate(highRiskAcc, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve High Risk Account", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                        highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
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


        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccountChecker.VERIFY)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.HighRiskAccountChecker.VERIFY;

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string status = arrResult.Substring(0, 1);
                        string highRiskAcc = arrResult.Remove(0, 1);

                        //Act based on task id
                        switch (status)
                        {
                            case "D":
                                string ActionDetails = MaintenanceAuditLogDao.HighRiskAccountChecker_DeleteTemplate(highRiskAcc, "Reject");
                                auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                                highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
                                break;

                            default:
                                if (status.Equals("A"))
                                {
                                    string ActionDetails1 = MaintenanceAuditLogDao.HighRiskAccountChecker_AddTemplate(highRiskAcc, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails1, sTaskId, CurrentUser.Account);
                                }
                                else if (status.Equals("U"))
                                {
                                    HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Master");
                                    
                                    HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(highRiskAcc, "Temp");

                                    string ActionDetails2 = MaintenanceAuditLogDao.HighRiskAccountChecker_EditTemplate(highRiskAcc, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails2, sTaskId, CurrentUser.Account);

                                }
                                highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
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

        public ActionResult Reject2(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIds.HighRiskAccountChecker.VERIFY;
                string status = col["Status"];
                string highRiskAcc = col["fldHighRiskAccount"];
                //Act based on task id
                switch (status)
                {
                   case "D":
                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccountChecker_DeleteTemplate(highRiskAcc, "Reject");
                        auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                        highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
                        break;

                    default:
                         if (status.Equals("A"))
                          {
                            string ActionDetails1 = MaintenanceAuditLogDao.HighRiskAccountChecker_AddTemplate(highRiskAcc, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails1, sTaskId, CurrentUser.Account);
                        }
                     else if (status.Equals("U"))
                        {
                            HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Master");

                            HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Temp");

                            string ActionDetails2 = MaintenanceAuditLogDao.HighRiskAccountChecker_EditTemplate(col["fldHighRiskAccount"], before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject High Risk Account", ActionDetails2, sTaskId, CurrentUser.Account);
                        }
                      highRiskAccountDao.DeleteHighRiskAccoountTemp(highRiskAcc);
                           break;
                }
                TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccountChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string HighRiskParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string HighRiskAccountId = "";
            if (string.IsNullOrEmpty(HighRiskParam))
            {
                HighRiskAccountId = filter["fldHighRiskAccount"].Trim();
            }
            else
            {
                HighRiskAccountId = HighRiskParam;
            }

            ViewBag.HighRiskAccount = highRiskAccountDao.GetHighRiskAccount(HighRiskAccountId);
            ViewBag.HighRiskAccountChecker = highRiskAccountDao.GetHighRiskAccountTemp(HighRiskAccountId);
            return View();
        }
    }
}