
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
    public class TransCodeController : BaseController
    {
       
        private ITransCodeDao transCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public TransCodeController(ITransCodeDao transCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.pageConfigDao = pageConfigDao;
            this.transCodeDao = transCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.TransCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TransCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TransCode.INDEX, "View_TransCodeMaster"),
            
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.TransCode.SAVECREATE;
            CurrentUser.Account.TaskId = sTaskId;

            try
            {
                List<String> errorMessages = transCodeDao.ValidateCreate(collection); //done

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        transCodeDao.CreateTransCode(collection, CurrentUser.Account.UserId); //done

                        //string ActionDetails = MaintenanceAuditLogDao.TransCode_AddTemplate(collection["TransCode"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                        
                        auditTrailDao.Log("Add into Transaction Code Table Master -  Trans Code: " + collection["TransCode"] + ", Trans Desc: " + collection["TransCodeDesc"], CurrentUser.Account);

                    }
                    else
                    {
                        transCodeDao.CreateTransCodeTemp(collection, CurrentUser.Account.UserId, "A", ""); //done

                        //string ActionDetails = MaintenanceAuditLogDao.TransCode_AddTemplate(collection["TransCode"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.TransCodeCreateVerify;

                        auditTrailDao.Log("Add into Transaction Code Table Master -  Trans Code : " + collection["TransCode"] + ", Trans Desc: " + collection["TransCodeDesc"], CurrentUser.Account);

                    }
                    
                  }
                return RedirectToAction("Create");
               
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.EDIT)]
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

            ViewBag.transCode = transCodeDao.GetTransCode(transCode); //Done
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            try
            {
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIds.TransCode.SAVECREATE;
                CurrentUser.Account.TaskId = sTaskId;

                List<String> errorMessages = transCodeDao.ValidateUpdate(collection);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { transCodeParam = collection["TransCode"] });
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(collection["TransCode"]);
                        auditTrailDao.Log("Edit Trans Code - Before Update=> Trans Code: " + before.fldTransCode + ", Trans Desc: " + before.fldTransCodeDesc, CurrentUser.Account);

                        transCodeDao.UpdateTransCode(collection, CurrentUser.Account.UserId); //Done

                        TransCodeModel after = MaintenanceAuditLogDao.GetTransCode(collection["TransCode"]);
                        auditTrailDao.Log("Edit Trans Code - After Update=> Trans Code: " + after.fldTransCode + ", Trans Desc: " + after.fldTransCodeDesc, CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.TransCode_EditTemplate(collection["TransCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                    }
                    else
                    {

                        if (transCodeDao.CheckTransCodeTempExist(collection["TransCode"]))
                        {
                            TempData["ErrorMsg"] = Locale.TransCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            TransCodeModel before = MaintenanceAuditLogDao.GetTransCode(collection["TransCode"]);

                            transCodeDao.CreateTransCodeTemp(collection, CurrentUser.Account.UserId, "U", ""); //Done

                            TransCodeModel after = MaintenanceAuditLogDao.GetTransCodeTemp(collection["TransCode"]);

                            //string ActionDetails = MaintenanceAuditLogDao.TransCode_EditTemplate(collection["TransCode"], before, after, "Edit");
                            //auditTrailDao.SecurityLog("Edit Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.TransCodeUpdateVerify;

                            auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Trans Code : " + collection["TransCode"] + ", Trans Desc: " + collection["TransCodeDesc"], CurrentUser.Account);
                        }
                    }
                    //action = RedirectToAction("Index");
                    action = RedirectToAction("Edit", new { transCodeParam = collection["TransCode"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return action;
        }

        [CustomAuthorize(TaskIds = TaskIds.TransCode.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.TransCode.SAVECREATE;
            CurrentUser.Account.TaskId = sTaskId;

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        //string ActionDetails = MaintenanceAuditLogDao.TransCode_DeleteTemplate(arrResult, "Delete");
                        //auditTrailDao.SecurityLog("Delete Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                        transCodeDao.DeleteTransCode(arrResult); //Done
                        TempData["Notice"] = Locale.RecordsuccesfullyDeleted;
                        auditTrailDao.Log("Delete - Trans Code : " + arrResult, CurrentUser.Account);

                    }
                    else
                    {
                        if (transCodeDao.CheckTransCodeTempExist(arrResult))
                        {
                            TempData["ErrorMsg"] = Locale.TransCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            //string ActionDetails = MaintenanceAuditLogDao.TransCode_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Transaction Code", ActionDetails, sTaskId, CurrentUser.Account);

                            transCodeDao.CreateTransCodeTemp(collection, CurrentUser.Account.UserId, "D", arrResult);
                            TempData["Notice"] = Locale.TransCodeDeleteVerify;
                            auditTrailDao.Log("Delete - Trans Code : " + arrResult, CurrentUser.Account);
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