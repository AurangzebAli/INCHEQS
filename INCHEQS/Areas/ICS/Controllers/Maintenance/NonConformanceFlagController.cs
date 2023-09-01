
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;

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
    public class NonConformanceFlagController : BaseController
    {
       
        private INonConformanceFlagDao ncFlagDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public NonConformanceFlagController(INonConformanceFlagDao ncFlagDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.ncFlagDao = ncFlagDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.NCFlag.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.NCFlag.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.NCFlag.INDEX, "View_NCFMaster"),
            
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.NCFlag.INDEX;
            CurrentUser.Account.TaskId = sTaskId;
            try
            {
                List<String> errorMessages = ncFlagDao.ValidateCreate(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {

                        ncFlagDao.CreateNCFCode(collection, CurrentUser.Account.UserId);

                        //string ActionDetails = MaintenanceAuditLogDao.NCF_AddTemplate(collection["NCFCode"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;


                        auditTrailDao.Log("Add into Non-Conformance Table Master -  NCF Code : " + collection["NCFCode"] + ", NCF Desc: " + collection["NCFDesc"], CurrentUser.Account);

                    }
                    else
                    {
                        ncFlagDao.CreateNCFCodeTemp(collection, CurrentUser.Account.UserId, "A", "");

                        //string ActionDetails = MaintenanceAuditLogDao.NCF_AddTemplate(collection["NCFCode"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.NCFCreateVerify;

                        auditTrailDao.Log("Add into Non-Conformance Table Master -  NCF Code : " + collection["NCFCode"] + ", NCF Desc: " + collection["NCFDesc"], CurrentUser.Account);

                    }
                    
                  }
                return RedirectToAction("Create");
               
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string ncfCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string ncfCode = "";
            if (string.IsNullOrEmpty(ncfCodeParam))
            {
                ncfCode = filter["fldNCFCode"].Trim();
            }
            else
            {
                ncfCode = ncfCodeParam;
            }

            ViewBag.ncf = ncFlagDao.GetNCFCode(ncfCode);

           
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            try
            {
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.NCFlag.INDEX;
                CurrentUser.Account.TaskId = sTaskId;

                List<string> errorMessages = ncFlagDao.ValidateUpdate(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { ncfCodeParam = collection["NCFCode"] });
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(collection["NCFCode"]);
                        auditTrailDao.Log("Edit NCF Code - Before Update=> NCF Code: " + before.fldNCFCode + ", NCF Desc: " + before.fldNCFDesc, CurrentUser.Account);

                        ncFlagDao.UpdateNCFCode(collection, CurrentUser.Account.UserId);

                        NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCode(collection["NCFCode"]);
                        auditTrailDao.Log("Edit NCF Code - After Update=> NCF Code: " + after.fldNCFCode + ", NCF Desc: " + after.fldNCFDesc, CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.NCF_EditTemplate(collection["NCFCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                    }
                    else
                    {

                        if (ncFlagDao.CheckNCFCodeTempExist(collection["NCFCode"]))
                        {
                            TempData["ErrorMsg"] = Locale.NCFAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            NonConformanceFlagModel before = MaintenanceAuditLogDao.GetNCFCode(collection["NCFCode"]);

                            ncFlagDao.CreateNCFCodeTemp(collection, CurrentUser.Account.UserId, "U", "");

                            NonConformanceFlagModel after = MaintenanceAuditLogDao.GetNCFCodeTemp(collection["NCFCode"]);

                            //string ActionDetails = MaintenanceAuditLogDao.NCF_EditTemplate(collection["NCFCode"], before, after, "Edit");
                            //auditTrailDao.SecurityLog("Edit Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.NCFUpdateVerify;

                            auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . NCF Code: " + collection["NCFCode"] +", NCF Desc: " + collection["NCFDesc"], CurrentUser.Account);
                        }
                    }
                    action = RedirectToAction("Edit", new { ncfCodeParam = collection["NCFCode"] });
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return action;
        }

        [CustomAuthorize(TaskIds = TaskIds.NCFlag.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.NCFlag.INDEX;
            CurrentUser.Account.TaskId = sTaskId;

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        //string ActionDetails = MaintenanceAuditLogDao.NCF_DeleteTemplate(arrResult, "Delete");
                        //auditTrailDao.SecurityLog("Delete Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                        ncFlagDao.DeleteNCFCode(arrResult);
                        TempData["Notice"] = Locale.RecordsuccesfullyDeleted;
                        auditTrailDao.Log("Delete - NCF Code : " + arrResult, CurrentUser.Account);

                    }
                    else
                    {
                        if (ncFlagDao.CheckNCFCodeTempExist(arrResult))
                        {
                            TempData["ErrorMsg"] = Locale.NCFAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            //string ActionDetails = MaintenanceAuditLogDao.NCF_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Non-Conformance Flag", ActionDetails, sTaskId, CurrentUser.Account);

                            ncFlagDao.CreateNCFCodeTemp(collection, CurrentUser.Account.UserId, "D", arrResult);
                            TempData["Notice"] = Locale.NCFDeleteVerify;
                            auditTrailDao.Log("Delete - NCF Code : " + arrResult, CurrentUser.Account);
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