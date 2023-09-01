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
using System.Threading.Tasks;
using System.Web.Mvc;

using INCHEQS.Areas.COMMON.Models.InternalBranchKBZ;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
 
    public class InternalBranchKBZController : BaseController {
        
        private IInternalBranchKBZDao internalBranchDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        private readonly IAuditTrailDao auditTrailDao;

        public InternalBranchKBZController(IInternalBranchKBZDao InternalBranch, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao, IAuditTrailDao auditTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
        this.internalBranchDao = InternalBranch;

        this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            this.auditTrailDao = auditTrailDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.InternalBranchKBZ.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {


            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.InternalBranchKBZ.INDEX, "View_InternalBranch", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string intBranchCodeParam = "") {

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

            DataTable dataTable = internalBranchDao.GetInternalBranchData(branchid); //Done
           

            if ((dataTable.Rows.Count > 0))
            {
                ViewBag.InternalBranch = dataTable.Rows[0];

                    if (ViewBag.InternalBranch["fldActive"].ToString() == "Y")
            {
                    @ViewBag.Active = "checked";
            }
            else
            {
                    @ViewBag.Active = "";
            }

                    if (ViewBag.InternalBranch["fldClearingBranchId"].ToString() == ViewBag.InternalBranch["fldBranchId"].ToString())
                {
                    @ViewBag.SelfClearing = "checked";
                        @ViewBag.Disabled = "disabled";
                }
                else
                {
                    @ViewBag.SelfClearing = "";
                        @ViewBag.Disabled = "";
                    
                }
                    if (ViewBag.InternalBranch["fldSubcenter"].ToString() == "Y")
                    {
                        @ViewBag.Subcenter = "checked";
                    }
                    else
                    {
                        @ViewBag.Subcenter = "";

                    }


            }

                return View();
            }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            try
            {
                List<String> errorMessages = internalBranchDao.ValidateUpdate(collection);

                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.InternalBranchKBZ.UPDATE;

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { intBranchCodeParam = collection["branchId"].Trim() });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(collection["branchId"]);

                        internalBranchDao.UpdateInternalBranch(collection); //Done
                        InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchData(collection["branchId"]);
                        string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_EditTemplate(collection["branchId"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;


                    }
                    else
                    {
                        bool IsInternalBranchMasterTempExist = internalBranchDao.CheckInternalBranchTempById(collection["branchId"]);

                        if (IsInternalBranchMasterTempExist == true)
                        {
                            TempData["Warning"] = Locale.InternalBranchAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            InternalBranchKBZModel before = MaintenanceAuditLogDao.GetInternalBranchData(collection["branchId"]);
                            internalBranchDao.CreateInternalBranchTemp(collection, "update"); //Done
                            InternalBranchKBZModel after = MaintenanceAuditLogDao.GetInternalBranchDataTemp(collection["branchId"]);
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_EditTemplate(collection["branchId"], before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                            TempData["Notice"] = Locale.InternalBranchUpdateVerify;

                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return RedirectToAction("Edit", new { intBranchCodeParam = collection["branchId"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.CREATE)] //DOne
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create(){
            ViewBag.ClearingBranchId = internalBranchDao.ListInternalBranch(CurrentUser.Account.BankCode);
            ViewBag.Country = internalBranchDao.ListCountry();
            ViewBag.BankZone = internalBranchDao.ListBankZone();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection,string bankCode) {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.InternalBranchKBZ.SAVECREATE;

            try {

                List<string> errorMessages = internalBranchDao.ValidateCreate(collection);

                if ((errorMessages.Count > 0)) {

                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");

                } else {
                    if ("N".Equals(systemProfile)) {

                        internalBranchDao.CreateInternalBranch(collection); //Done

                        string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_AddTemplate(collection["branchId"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        TempData["Notice"] = Locale.SuccessfullyCreated;

                    } else {

                        internalBranchDao.CreateInternalBranchTemp(collection, "create"); //Done
                        string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_AddTemplate(collection["branchId"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.InternalBranchCreateVerify;

                    }
                    return RedirectToAction("Create");
                }
            } catch (Exception ex) {
                throw ex;
            }
            
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.InternalBranchKBZ.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.InternalBranchKBZ.DELETE;

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults)
                {

                    if ("N".Equals(systemProfile))
                    {
                        string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        internalBranchDao.DeleteInternalBranch(arrResult); //Done
                        TempData["Notice"] = Locale.SuccessfullyDeleted;

                    }
                    else
                    {
                        bool IsInternalBranchTempExist = internalBranchDao.CheckInternalBranchTempById(arrResult);

                        if (IsInternalBranchTempExist == true)
                        {
                            TempData["Warning"] = Locale.InternalBranchAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            internalBranchDao.CreateInternalBranchTempToDelete(arrResult); //Done
                            TempData["Notice"] = Locale.InternalBranchVerifyDelete;
  
                            string ActionDetails = MaintenanceAuditLogDao.InternalBranchKBZ_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Internal Branch Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }

                }

                }
            else

                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }
    
}