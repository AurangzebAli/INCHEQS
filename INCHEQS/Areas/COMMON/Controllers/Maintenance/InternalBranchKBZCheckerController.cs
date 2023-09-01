using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;

using INCHEQS.Areas.COMMON.Models.InternalBranchKBZ;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;


namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class InternalBranchKBZCheckerController : BaseController
    {
        private IInternalBranchKBZDao internalBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public InternalBranchKBZCheckerController(IInternalBranchKBZDao InternalBranch, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.internalBranchDao = InternalBranch;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.InternalBranchCheckerKBZ.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {


            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InternalBranchCheckerKBZ.INDEX, "View_InternalBranchChecker", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult InternalBranchKBZ(FormCollection col, string intBranchCodeParam = "")
        {

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);


            string branchid = "";

            if (string.IsNullOrEmpty(intBranchCodeParam))
            {
                branchid = filter["fldBranchId"].Trim();
            }
            else
            {
                branchid = intBranchCodeParam;
            }

            ViewBag.ClearingBranchId = internalBranchDao.ListInternalBranch(CurrentUser.Account.BankCode); //Done
            ViewBag.Country = internalBranchDao.ListCountry();
            ViewBag.BankZone = internalBranchDao.ListBankZone();



            if ((branchid == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DataTable dt = internalBranchDao.GetInternalBranchData(branchid); //Done
            DataTable dataTableTemp = internalBranchDao.GetInternalBranchDataTemp(branchid); //Done


            if ((dataTableTemp.Rows.Count > 0))
            {
                ViewBag.InternalBranchTemp = dataTableTemp.Rows[0];
                if (dt.Rows.Count > 0)
                {
                    ViewBag.InternalBranch = dt.Rows[0];
                }
                else
                {
                    ViewBag.InternalBranch = null;
                }

                if (ViewBag.InternalBranchTemp["fldActive"].ToString() == "Y")
                {
                    @ViewBag.ActiveTemp = "checked";
                }
                else
                {
                    @ViewBag.ActiveTemp = "";
                }

                if (ViewBag.InternalBranchTemp["fldClearingBranchId"].ToString() == ViewBag.InternalBranchTemp["fldBranchId"].ToString())
                {
                    @ViewBag.SelfClearingTemp = "checked";
                }
                else
                {
                    @ViewBag.SelfClearingTemp = "";

                }
                if (ViewBag.InternalBranchTemp["fldSubcenter"].ToString() == "Y")
                {
                    @ViewBag.SubcenterTemp = "checked";
                }
                else
                {
                    @ViewBag.SubcenterTemp = "";
                }


            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY;
            try
            {

                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        //string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 1);

                        if (action.Equals("A"))
                        {

                            internalBranchDao.MoveToInternalBranchFromTemp(id); //Done
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);

                            internalBranchDao.DeleteInternalBranch(id); //Done
                        }
                        else if (action.Equals("U"))
                        {
                            InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(id);

                            internalBranchDao.UpdateInternalBranchById(id); //Done

                            InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchData(id);
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_EditTemplate(id, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }

                        internalBranchDao.DeleteInternalBranchTemp(id); //Done


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

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY;
            try
            {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string id = arrResult.Remove(0, 1);
                        if (action.Equals("A"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(id);
                            InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchDataTemp(id);
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }

                        internalBranchDao.DeleteInternalBranchTemp(id); //Done

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

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY;
            try
            {
                string action = col["action"];
                string id = col["branchId"];

                if (action.Equals("A"))
                {

                    internalBranchDao.MoveToInternalBranchFromTemp(id); //Done
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);

                    internalBranchDao.DeleteInternalBranch(id); //Done
                }
                else if (action.Equals("U"))
                {
                    InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(id);
                    internalBranchDao.UpdateInternalBranchById(id); //Done
                    InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchData(id);
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }

                internalBranchDao.DeleteInternalBranchTemp(id); //Done


                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY;
            try
            {
                string action = col["action"];
                string id = col["branchId"];
                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(id);
                    InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchDataTemp(id);
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }

                internalBranchDao.DeleteInternalBranchTemp(id);
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