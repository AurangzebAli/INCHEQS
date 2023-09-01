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

using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;


namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class InternalBranchCheckerController : BaseController
    {
        private IInternalBranchDao internalBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public InternalBranchCheckerController(IInternalBranchDao InternalBranch, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.internalBranchDao = InternalBranch;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.InternalBranchChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {


            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InternalBranchChecker.INDEX, "View_InternalBranchChecker", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult InternalBranch(FormCollection col, string intBranchCodeParam = "")
        {

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);


            string branchid = "";

            if (string.IsNullOrEmpty(intBranchCodeParam))
            {
                branchid = filter["fldInternalBranchId"].Trim();
            }
            else
            {
                branchid = intBranchCodeParam;
            }

            ViewBag.ClearingBranchIdConv = internalBranchDao.ListInternalBranch("C");
            ViewBag.ClearingBranchIdIslamic = internalBranchDao.ListInternalBranch("I");
            ViewBag.Country = internalBranchDao.ListCountry();
            //KBZ ViewBag.BankZone = internalBranchDao.ListBankZone();



            /*if ((branchid == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }*/

            DataTable dataTable = internalBranchDao.GetInternalBranchData(branchid); //Done
            DataTable dataTableTemp = internalBranchDao.GetInternalBranchDataTemp(branchid); //Done
            if ((dataTableTemp.Rows.Count > 0))
            {
                ViewBag.InternalBranchTemp = dataTableTemp.Rows[0];
                if (dataTable.Rows.Count > 0)
                {
                    ViewBag.InternalBranch = dataTable.Rows[0];
                }
                else
                {
                    ViewBag.InternalBranch = null;
                }


                if ((ViewBag.InternalBranchTemp["fldCBranchId"].ToString().Trim() == ""))
                {
                    ViewBag.SelfClearingConvDisabledTemp = "disabled";
                    ViewBag.SelfClearingConvTemp = "";
                    ViewBag.ConvTemp = "disabled";
                    ViewBag.CheckConvTemp = "";
                }
                else
                {
                    if (ViewBag.InternalBranchTemp["fldCClearingBranchId"].ToString() == ViewBag.InternalBranchTemp["fldCBranchId"].ToString())
                    {
                        ViewBag.SelfClearingConvTemp = "checked";
                        ViewBag.SelfClearingConvDisabledTemp = "disabled";
                    }
                    else
                    {
                        ViewBag.SelfClearingConvTemp = "";
                        ViewBag.SelfClearingConvDisabledTemp = "";
                    }
                    ViewBag.CheckConvTemp = "checked";
                    ViewBag.ConvTemp = "";
                }

                if ((ViewBag.InternalBranchTemp["fldIBranchId"].ToString().Trim() == ""))
                {
                    ViewBag.SelfClearingIslamicTemp = "";
                    ViewBag.SelfClearingIslamicDisabledTemp = "disabled";
                    ViewBag.IslamicTemp = "disabled";
                    ViewBag.CheckIslamicTemp = "";
                }
                else
                {
                    if (ViewBag.InternalBranchTemp["fldIClearingBranchId"].ToString() == ViewBag.InternalBranchTemp["fldIBranchId"].ToString())
                    {
                        ViewBag.SelfClearingIslamicTemp = "checked";
                        ViewBag.SelfClearingIslamicDisabledTemp = "disabled";
                    }
                    else
                    {
                        ViewBag.SelfClearingIslamicTemp = "";
                        ViewBag.SelfClearingIslamicDisabledTemp = "";
                    }
                    ViewBag.CheckIslamicTemp = "checked";
                    ViewBag.IslamicTemp = "";
                }
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchChecker.VERIFY;
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
                            internalBranchDao.MoveToInternalBranchFromTemp(id); //Done

                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);

                            internalBranchDao.DeleteInternalBranch(id); //Done
                        }
                        else if (action.Equals("U"))
                        {
                            InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                            internalBranchDao.UpdateInternalBranchById(id); //Done

                            InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_EditTemplate(id, before, after, "Approve");
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.VERIFY)]
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
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                            InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Temp");

                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_DeleteTemplate(id, "Reject");
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchChecker.VERIFY;
            try
            {
                string action = col["action"];
                string id = col["internalBranchID"];

                if (action.Equals("A"))
                {
                    internalBranchDao.MoveToInternalBranchFromTemp(id); //Done

                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);

                    internalBranchDao.DeleteInternalBranch(id); //Done
                }
                else if (action.Equals("U"))
                {
                    InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                    internalBranchDao.UpdateInternalBranchById(id); //Done

                    InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_EditTemplate(id, before, after, "Approve");
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            string taskId = TaskIdsOCS.InternalBranchCheckerKBZ.VERIFY;
            try
            {
                string id = col["internalBranchID"];
                string action = col["action"];

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    InternalBranchModel before = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Master");

                    InternalBranchModel after = MaintenanceAuditLogDao.GetInternalBranchDataById(id, "Temp");

                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Internal Branch Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.InternalBranchProfileChecker_DeleteTemplate(id, "Reject");
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