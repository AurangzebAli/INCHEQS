using INCHEQS.Areas.COMMON.Models.CreditAccount;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class CreditAccountController : BaseController
    {
        private readonly ICreditAccountDao CreditAccountDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        public CreditAccountController(ICreditAccountDao CreditAccountDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao/*, ISecurityProfileDao securityProfileDao*/)
        {
            this.pageConfigDao = pageConfigDao;
            this.CreditAccountDao = CreditAccountDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }


        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.CreditAccount.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection col)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.CreditAccount.INDEX, "View_CreditAccount", "fldCreditAccountId"),
            col);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                ViewBag.Clearing = CreditAccountDao.ListBranch("");
                ViewBag.StateCode = CreditAccountDao.ListStateCode("");

                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                string accountid = "";
                ViewBag.AccountId = CreditAccountDao.GetAccountNumber(accountid);
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.CreditAccount.SAVECREATE;

                //StateCodeModel state = new StateCodeModel();
                //validate bank zone
                List<String> errorMessages = new List<string>();//StateCodeDao.ValidateStateCode(col, "Create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        //Create Process
                        CreditAccountDao.CreateAccountNumberMaster(col);

                        TempData["Notice"] = Locale.CreditAccountSuccessfullyCreated;

                        
                        //string ActionDetails = MaintenanceAuditLogDao.StateCode_AddTemplate(col["fldStateCode"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        //StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, null, "Create");

                        //TempData["Notice"] = Locale.StateCodeSuccessfullyAddedtoApprovedCreate;

                        //string ActionDetails = MaintenanceAuditLogDao.StateCode_AddTemplate(col["fldStateCode"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string CreditAccountId = null)
        {   
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                if (!string.IsNullOrEmpty(CreditAccountId))
                {
                    ViewBag.CreditAccount = CreditAccountDao.GetAccountNumber(CreditAccountId);
                }
                else
                {
                    if (col.AllKeys.Contains("fldCreditAccountId"))
                    {
                        ViewBag.CreditAccount = CreditAccountDao.GetAccountNumber(filter["fldCreditAccountId"]);
                    }
                    else
                    {
                        ViewBag.CreditAccount = CreditAccountDao.GetAccountNumber(filter["fldCreditAccountId"]);
                    }
                }
                
                    ViewBag.Clearing = CreditAccountDao.ListBranch(filter["fldClearingType"]);
                    ViewBag.StateCode = CreditAccountDao.ListStateCode(filter["fldStateCode"]);


                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.CreditAccount.UPDATE;

                //validate bank zone
                List<String> errorMessages = new List<string>();//StateCodeDao.ValidateStateCode(col, "Update");

                if ((errorMessages.Count > 0))
                {
                //    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        //CreditAccountModel before = CreditAccountDao.GetStateCode(col["fldStateCode"]);

                        //update Process
                        CreditAccountDao.UpdateCreditAccountMaster(col);
                        TempData["Notice"] = Locale.StateCodeSuccessfullyUpdated;

                        //StateCodeModel after = StateCodeDao.GetStateCode(col["fldStateCode"]);

                        //string ActionDetails = MaintenanceAuditLogDao.StateCode_EditTemplate(col["fldStateCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        //StateCodeModel before = StateCodeDao.GetStateCode(col["fldStateCode"]);

                        //StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, null, "Update");

                        //StateCodeModel after = StateCodeDao.GetStateCodeTemp(col["fldStateCode"]);

                        //TempData["Notice"] = Locale.StateCodeSuccessfullyAddedtotemp;

                        //string ActionDetails = MaintenanceAuditLogDao.StateCode_EditTemplate(col["fldStateCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [CustomAuthorize(TaskIds = TaskIds.CreditAccount.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.CreditAccount.DELETE;
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.StateCode_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete State Code", ActionDetails, sTaskId, CurrentUser.Account);

                            CreditAccountDao.DeleteCreditAccountMasterTemp(arrResult);

                            TempData["Notice"] = Locale.SuccessfullyDeleted;
                        }
                        else
                        {
                            //bool IsStateCodeTempExist = StateCodeDao.CheckStateCodeMasterTempById(arrResult);

                            //if (IsStateCodeTempExist == true)
                            //{
                            //    TempData["Warning"] = Locale.StateCodeAlreadyExiststoDeleteorUpdate;
                            //}
                            //else
                            //{
                            //    string ActionDetails = MaintenanceAuditLogDao.StateCode_DeleteTemplate(arrResult, "Delete");
                            //    auditTrailDao.SecurityLog("Delete State Code", ActionDetails, sTaskId, CurrentUser.Account);

                            ////    StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, arrResult, "Delete");
                            //    TempData["Notice"] = Locale.StateCodeVerifyDelete;
                            //}
                        }

                    }

                }
                else
                {
                    TempData["Warning"] = Locale.Nodatawasselected;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //_log.Error(ex);
                throw ex;
            }
        }



    }
}
