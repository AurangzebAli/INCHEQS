using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
//using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.StateCode;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class StateCodeController : BaseController
    {

        private readonly IStateCodeDao StateCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public StateCodeController(IStateCodeDao StateCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao/*, ISecurityProfileDao securityProfileDao*/)
        {
            this.pageConfigDao = pageConfigDao;
            this.StateCodeDao = StateCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.StateCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection col)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.StateCode.INDEX, "View_StateCode", "fldStateCode"),
            col);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.StateCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string statecode = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                if (!string.IsNullOrEmpty(statecode))
                {
                    ViewBag.StateCode = StateCodeDao.GetStateCode(statecode);
                }
                else
                {
                    if (col.AllKeys.Contains("fldStateCode"))
                    {
                        ViewBag.StateCode = StateCodeDao.GetStateCode(filter["fldStateCode"]);
                    }
                    else
                    {
                        ViewBag.StateCode = StateCodeDao.GetStateCode(filter["fldStateCode"]);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIds.StateCode.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.StateCode.UPDATE;

                //validate bank zone
                List<String> errorMessages = StateCodeDao.ValidateStateCode(col, "Update");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        StateCodeModel before = StateCodeDao.GetStateCode(col["fldStateCode"]);

                        //update Process
                        StateCodeDao.UpdateStateCodeMaster(col);
                        TempData["Notice"] = Locale.StateCodeSuccessfullyUpdated;

                        StateCodeModel after = StateCodeDao.GetStateCode(col["fldStateCode"]);

                        string ActionDetails = MaintenanceAuditLogDao.StateCode_EditTemplate(col["fldStateCode"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        StateCodeModel before = StateCodeDao.GetStateCode(col["fldStateCode"]);

                        StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, null, "Update");

                        StateCodeModel after = StateCodeDao.GetStateCodeTemp(col["fldStateCode"]);

                        TempData["Notice"] = Locale.StateCodeSuccessfullyAddedtotemp;

                        string ActionDetails = MaintenanceAuditLogDao.StateCode_EditTemplate(col["fldStateCode"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.StateCode.DELETE;
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.StateCode_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete State Code", ActionDetails, sTaskId, CurrentUser.Account);

                            StateCodeDao.DeleteStateCodeMaster(arrResult);

                            TempData["Notice"] = Locale.SuccessfullyDeleted;
                        }
                        else
                        {
                            bool IsStateCodeTempExist = StateCodeDao.CheckStateCodeMasterTempById(arrResult);

                            if (IsStateCodeTempExist == true)
                            {
                                TempData["Warning"] = Locale.StateCodeAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                string ActionDetails = MaintenanceAuditLogDao.StateCode_DeleteTemplate(arrResult, "Delete");
                                auditTrailDao.SecurityLog("Delete State Code", ActionDetails, sTaskId, CurrentUser.Account);

                                StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, arrResult, "Delete");
                                TempData["Notice"] = Locale.StateCodeVerifyDelete;
                            }
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

        [CustomAuthorize(TaskIds = TaskIds.StateCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.StateCode.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.StateCode.SAVECREATE;

                StateCodeModel state = new StateCodeModel();
                //validate bank zone
                List<String> errorMessages = StateCodeDao.ValidateStateCode(col, "Create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        //Create Process
                        StateCodeDao.CreateStateCodeMaster(col);

                        TempData["Notice"] = Locale.StateCodeSuccessfullyCreated;

                        string ActionDetails = MaintenanceAuditLogDao.StateCode_AddTemplate(col["fldStateCode"], "Add", "N");
                        auditTrailDao.SecurityLog("Add State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        StateCodeDao.CreateStateCodeMasterTemp(col, CurrentUser.Account.UserId, null, "Create");

                        TempData["Notice"] = Locale.StateCodeSuccessfullyAddedtoApprovedCreate;

                        string ActionDetails = MaintenanceAuditLogDao.StateCode_AddTemplate(col["fldStateCode"], "Add", "N");
                        auditTrailDao.SecurityLog("Add State Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}