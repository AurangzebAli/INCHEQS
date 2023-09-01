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
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;
namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class ReturnCodeController : BaseController
    {

        private readonly IReturnCodeDao ReturnCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public ReturnCodeController(IReturnCodeDao ReturnCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao/*, ISecurityProfileDao securityProfileDao*/)
        {
            this.pageConfigDao = pageConfigDao;
            this.ReturnCodeDao = ReturnCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            //this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ReturnCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ReturnCode.INDEX, "View_ReturnCode", "fldRejectCode"),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string ReturnCode = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                if (!string.IsNullOrEmpty(ReturnCode))
                {
                    ViewBag.returncode = ReturnCodeDao.GetReturnCode(ReturnCode);
                }
                else
                {

                    ViewBag.returncode = ReturnCodeDao.GetReturnCode(filter["fldRejectCode"]);
                }
                ViewBag.RejectType = ReturnCodeDao.ListRejectType();
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.ReturnCode.INDEX;

                CurrentUser.Account.TaskId = sTaskId;

                //validate return code
                List<String> errorMessages = ReturnCodeDao.ValidateReturnCode(col, "Update");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(col["fldRejectCode"]);
                        auditTrailDao.Log("Edit Return Code - Before Update=> Return Code : " + before.fldRejectCode + " Return Desc : " + before.fldRejectDesc, CurrentUser.Account);

                        //update Process
                        ReturnCodeDao.UpdateReturnCodeMaster(col);
                        TempData["Notice"] = Locale.ReturnCodeUpdated;

                        INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCode(col["fldRejectCode"]);
                        auditTrailDao.Log("Edit Return Code - After Update=> Return Code : " + after.fldRejectCode + " Return Desc : " + after.fldRejectDesc, CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.RetCode_EditTemplate(col["fldRejectCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        //INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before = ReturnCodeDao.GetReturnCode(col["fldRejectCode"]);

                        ReturnCodeDao.CreateReturnCodeMasterTemp(col, null, CurrentUser.Account.UserId, "Update");

                        //INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after = ReturnCodeDao.GetReturnCodeTemp(col["fldRejectCode"]);

                        TempData["Notice"] = Locale.ReturnCodeUpdateVerify;
                        auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Return Code : " + col["fldRejectCode"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.RetCode_EditTemplate(col["fldRejectCode"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Edit",new { ReturnCode = col["fldRejectCode"]});
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.ReturnCode.INDEX;

                CurrentUser.Account.TaskId = sTaskId;

                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            //string ActionDetails = MaintenanceAuditLogDao.RetCode_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Return Code", ActionDetails, sTaskId, CurrentUser.Account);

                            ReturnCodeDao.DeleteReturnCodeMaster(arrResult);

                            TempData["Notice"] = Locale.SuccessfullyDeleted;
                            auditTrailDao.Log("Delete - Return Code table - Return Code :  " + col["deleteBox"], CurrentUser.Account);

                        }
                        else
                        {
                            bool IsAccountProfileTempExist = ReturnCodeDao.CheckReturnCodeMasterTempById(arrResult);

                            if (IsAccountProfileTempExist == true)
                            {
                                TempData["Warning"] = Locale.ReturnCodeAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                //string ActionDetails = MaintenanceAuditLogDao.RetCode_DeleteTemplate(arrResult, "Delete");
                                //auditTrailDao.SecurityLog("Delete Return Code", ActionDetails, sTaskId, CurrentUser.Account);

                                ReturnCodeDao.CreateReturnCodeMasterTemp(col, arrResult, CurrentUser.Account.UserId, "Delete");

                                TempData["Notice"] = Locale.ReturnCodeVerifyDelete;
                                auditTrailDao.Log("Add into Return Code Temp table to Delete - Return Code :  " + col["deleteBox"], CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                ViewBag.RejectType = ReturnCodeDao.ListRejectType();
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ReturnCode.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.ReturnCode.INDEX;

                CurrentUser.Account.TaskId = sTaskId;

                //ReturnCodeModel user = new ReturnCodeModel();
                //validate bank zone
                List<String> errorMessages = ReturnCodeDao.ValidateReturnCode(col, "Create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {

                        //Create Process
                        ReturnCodeDao.CreateReturnCodeMaster(col);

                        TempData["Notice"] = Locale.ReturnCodeSuccessfullyCreated;
                        auditTrailDao.Log("Add  Return Code : " + col["fldRejectCode"] + " Return Desc : " + col["fldRejectDesc"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.RetCode_AddTemplate(col["fldRejectCode"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Return Code", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        ReturnCodeDao.CreateReturnCodeMasterTemp(col, null, CurrentUser.Account.UserId, "Create");

                        TempData["Notice"] = Locale.ReturnCodeCreateVerify;
                        auditTrailDao.Log("Add into Return Code Table - Return Code : " + col["fldRejectCode"] + " Return Code Desc : " + col["fldReturnDesc"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.RetCode_AddTemplate(col["fldRejectCode"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Return Code", ActionDetails, sTaskId, CurrentUser.Account);
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