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
    public class HighRiskAccountController : BaseController{

        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private readonly ISystemProfileDao systemProfileDao;
        private IHighRiskAccountDao highRiskAccountDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public HighRiskAccountController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHighRiskAccountDao highRiskAccountDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.highRiskAccountDao = highRiskAccountDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.INDEX)]
        [GenericFilter(AllowHttpGet =true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.HighRiskAccount.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.HighRiskAccount.INDEX, "View_HighRiskAccount"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string HighRiskParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string HighRiskAccountId = "";
            if (string.IsNullOrEmpty(HighRiskParam)) {
                HighRiskAccountId = filter["fldHighRiskAccount"].Trim();
            } else {
                HighRiskAccountId = HighRiskParam;
            }

            ViewBag.HighRiskAccount = highRiskAccountDao.GetHighRiskAccount(HighRiskAccountId);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            ViewBag.InternalBranchCode = highRiskAccountDao.GetInternalBranchCode();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.CREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.HighRiskAccount.CREATE;
            List<string> errorMsg = highRiskAccountDao.ValidateCreate(col);
            try 
            {
                if (errorMsg.Count > 0)
                {
                TempData["ErrorMsg"] = errorMsg;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        highRiskAccountDao.CreateHighRiskAccount(col,CurrentUser.Account);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        //auditTrailDao.Log("Create High Risk Account - Account No : "+col["fldHighRiskAccount"],CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_AddTemplate(col["fldHighRiskAccount"], "Add", "N");
                        auditTrailDao.SecurityLog("Add High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        highRiskAccountDao.CreateHighRiskAccountTemp(col, CurrentUser.Account, "A", "");

                        TempData["Notice"] = Locale.HighRiskAccountCreateVerify;
                        //auditTrailDao.Log("Create High Risk Account TEMP - Account No : " + col["fldHighRiskAccount"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_AddTemplate(col["fldHighRiskAccount"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Index");
            } 
            catch(Exception ex) 
            { 
                throw ex; 
            }

        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) 
        {
			try 
            {
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.HighRiskAccount.UPDATE;
                List<String> errorMsg = highRiskAccountDao.ValidateUpdate(col);
                if (errorMsg.Count > 0)
                {
                TempData["ErrorMsg"] = errorMsg;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        //HighRiskAccountModel before = highRiskAccountDao.GetHighRiskAccount(col["fldHighRiskAccount"]);
                        //auditTrailDao.Log("Update High Risk Account => Before - High Risk Account : "+ before.fldHighRiskAccount + ", Amount : " + before.fldHighRiskAmount, CurrentUser.Account);

                        HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Master");

                        highRiskAccountDao.UpdateHighRiskAccount(col);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;

                        HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Master");

                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_EditTemplate(col["fldHighRiskAccount"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                        //HighRiskAccountModel after = highRiskAccountDao.GetHighRiskAccount(col["fldHighRiskAccount"]);
                        //auditTrailDao.Log("Update High Risk Account => Before - High Risk Account : " + after.fldHighRiskAccount+", Amount : "+after.fldHighRiskAmount, CurrentUser.Account);
                    }
                    else
                    {
                        if (highRiskAccountDao.CheckHighRiskAccountTempExist(col["fldHighRiskAccount"]))
                        {
                            TempData["ErrorMsg"] = Locale.HighRiskAccountAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            HighRiskAccountModel before = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Master");

                            highRiskAccountDao.CreateHighRiskAccountTemp(col, CurrentUser.Account, "U", "");
                            TempData["Notice"] = Locale.HighRiskAccountUpdateVerify;

                            HighRiskAccountModel after = MaintenanceAuditLogDao.GetHighRiskAccount(col["fldHighRiskAccount"], "Temp");

                            string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_EditTemplate(col["fldHighRiskAccount"], before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);


                            //auditTrailDao.Log("Update High Risk Account TEMP - Account No : " + col["fldHighRiskAccount"], CurrentUser.Account);
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.HighRiskAccount.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.HighRiskAccount.DELETE;
            if (col != null & col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {

                    if ("N".Equals(systemProfile))
                    {
                        string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);
                        
                        highRiskAccountDao.DeleteFromHighRiskAccount(arrResult);
                        TempData["Notice"] = Locale.SuccessfullyDeleted;
                }
                    else
                    {
                        if (highRiskAccountDao.CheckHighRiskAccountTempExist(arrResult))
                        {
                            TempData["ErrorMsg"] = Locale.HighRiskAccountAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            string ActionDetails = MaintenanceAuditLogDao.HighRiskAccount_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete High Risk Account", ActionDetails, sTaskId, CurrentUser.Account);

                            highRiskAccountDao.CreateHighRiskAccountTemp(col, CurrentUser.Account, "D", arrResult);
                            TempData["Notice"] = Locale.HighRiskAccountDeleteVerify;
                        }
                   }
                }                
            } else {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
        }
    }
}