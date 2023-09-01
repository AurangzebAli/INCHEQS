using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BranchCodeCheckerController : BaseController
    {
        private IBranchCodeDao branchcodedao;
        private IBankCodeDao bankcodedao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BranchCodeCheckerController(IBranchCodeDao branchcodedao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IBankCodeDao bankCodeDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.branchcodedao = branchcodedao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.bankcodedao = bankCodeDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BranchCodeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {

            /*Edit by Umar to filter BranchCode */
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BranchCodeChecker.INDEX, "View_BranchCodeChecker", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);

            string branchid = filter["fldBranchId"].Trim();

            DataTable dataTable = branchcodedao.getBranchCodeFromCheckerView(branchid);
            DataTable dataTable1 = branchcodedao.getBranchCode(branchid);

            if ((dataTable.Rows.Count > 0))
            {
                ViewBag.BranchCodeTemp = dataTable.Rows[0];
            }
            if ((dataTable1.Rows.Count > 0))
            {
                ViewBag.BranchCode = dataTable1.Rows[0];
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.BranchCodeChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string id = arrResult.Remove(0, 1);

                        if (action.Equals("A"))
                        {
                            branchcodedao.MoveToBranchCodeFromTemp(id);

                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                            branchcodedao.DeleteBranchCode(id);    
                        }
                        else if (action.Equals("U"))
                        {
                            BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Master");

                            branchcodedao.UpdateBranchCodeById(id);

                            BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Master");

                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_EditTemplate(id, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        branchcodedao.DeleteBranchCodeTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.BranchCodeChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string id = arrResult.Remove(0, 1);

                        if (action.Equals("A"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Master");

                            BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Temp");

                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        branchcodedao.DeleteBranchCodeTemp(id);
                        
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

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult Approve2(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                string arrResult = col["branchId"].ToString();
                string action = arrResult.Substring(0, 1);
                string id = arrResult.Remove(0, 1);

                string sTaskId = TaskIds.BranchCodeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    branchcodedao.MoveToBranchCodeFromTemp(id);

                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                    branchcodedao.DeleteBranchCode(id);
                }
                else if (action.Equals("U"))
                {
                    BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Master");

                    branchcodedao.UpdateBranchCodeById(id);

                    BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Temp");

                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                }

                branchcodedao.DeleteBranchCodeTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchCodeChecker.VERIFY)]
        [HttpPost]
        public ActionResult Reject2(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];

                string arrResult = col["branchId"].ToString();
                string action = arrResult.Substring(0, 1);
                string id = arrResult.Remove(0, 1);
                string sTaskId = TaskIds.BranchCodeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BranchCodeModel before = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Master");
                    
                    BranchCodeModel after = MaintenanceAuditLogDao.GetBranchCodeDataById(id, "Temp");

                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BranchProfileChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                }
                branchcodedao.DeleteBranchCodeTemp(id);
                //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Branch Id : " + id, CurrentUser.Account);
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