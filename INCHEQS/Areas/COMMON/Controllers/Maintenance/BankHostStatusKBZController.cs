using INCHEQS.Areas.ICS.Models.HostReturnReason;
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
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankHostStatusKBZController : Controller
    {
        private readonly IBankHostStatusKBZDao bankHostStatusKBZDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankHostStatusKBZController(IBankHostStatusKBZDao bankHostStatusKBZDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.bankHostStatusKBZDao = bankHostStatusKBZDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }
        // GET: ICS/HostReturnReason
        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankHostStatusKBZ.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankHostStatusKBZ.INDEX, "View_BankHostStatusMasterMAB", "fldBankHostStatusCode"),
            collection);
            return View();
        }

        // GET: ReturnCode/Details?..
        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string statusCodeParam = "")
        {

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string strStatusCode = "";
            if (string.IsNullOrEmpty(statusCodeParam))
            {
                strStatusCode = filter["fldBankHostStatusCode"].Trim();
            }
            else
            {
                strStatusCode = statusCodeParam;
            }

            ViewBag.BankHostStatusKBZ = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(strStatusCode);
            ViewBag.HostStatusAction = bankHostStatusKBZDao.ListHostStatusAction();
            //DataTable dataTable = bankHostStatusKBZDao.GetBankHostStatusMaster(strStatusCode);
            //if ((dataTable.Rows.Count > 0))
            //{
            //    ViewBag.BankHostStatus = dataTable.Rows[0];
            //}
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            List<String> errorMessages = bankHostStatusKBZDao.ValidateUpdate(collection);
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BankHostStatusKBZ.UPDATE;

            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {

                if ("N".Equals(systemProfile))
                {
                    BankHostStatusKBZModel before = bankHostStatusKBZDao.GetHostReturnReasonModel(collection["fldBankHoststatusCode"]);
                    //auditTrailDao.Log("Edit - Bank Host Status Master - Before Update => Status Id : " + before.fldBankHostStatusCode + ", Status Description : " + before.fldBankHostStatusDesc, CurrentUser.Account);

                    BankHostStatusKBZModel beforeEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(collection["fldBankHoststatusCode"]);
                    bankHostStatusKBZDao.UpdateHostStatusMasterKBZ(collection, CurrentUser.Account);
                    TempData["notice"] = Locale.SuccessfullyUpdated;

                    BankHostStatusKBZModel after = bankHostStatusKBZDao.GetHostReturnReasonModel(collection["fldBankHoststatusCode"]);
                    //auditTrailDao.Log("Edit - Bank Host Status Master - After Update => Status Id : " + after.fldBankHostStatusCode + ", Status Description : " + after.fldBankHostStatusDesc, CurrentUser.Account);

                    BankHostStatusKBZModel afterEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(collection["fldBankHoststatusCode"]);
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_EditTemplate(collection["fldBankHostStatusCode"], beforeEdit, afterEdit, "Edit");
                    auditTrailDao.SecurityLog("Edit Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else
                {

                    bool IsBankHostCodeExist = bankHostStatusKBZDao.CheckBankHostCodeDataTempKBZ(collection["fldBankHostStatusCode"]);

                    if (IsBankHostCodeExist == true)
                    {
                        TempData["Warning"] = Locale.BankHostCodeAlreadyExiststoDeleteorUpdate;
                    }
                    else
                    {
                        BankHostStatusKBZModel beforeEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(collection["fldBankHoststatusCode"]);

                        bankHostStatusKBZDao.AddBankHostCodeinKBZTemptoUpdate(collection, CurrentUser.Account);
                        TempData["notice"] = Locale.BankHostMasterUpdateVerify;
                        BankHostStatusKBZModel afterEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZTemp(collection["fldBankHoststatusCode"]);
                        string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_EditTemplate(collection["fldBankHostStatusCode"], beforeEdit, afterEdit, "Edit");
                        auditTrailDao.SecurityLog("Edit Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    //auditTrailDao.Log("AddBank Bank Host Status Master Temporary Record - Status Id : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);

                }
            }
            return RedirectToAction("Edit", new { statusCodeParam = collection["fldBankHoststatusCode"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            ViewBag.HostStatusAction = bankHostStatusKBZDao.ListHostStatusAction();
            
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BankHostStatusKBZ.SAVECREATE;
            List<String> errorMessages = bankHostStatusKBZDao.ValidateCreate(collection);


            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {
                if ("N".Equals(systemProfile))
                {
                    // bankHostStatusKBZDao.CreateBankHostStatusMasterTemp(collection, strAutoPending, strAutoReject, CurrentUser.Account);
                    bankHostStatusKBZDao.CreateInBankHostStatusMasterKBZ(collection);
                    //bankHostStatusKBZDao.DeleteInBankHostStatusMasterTemp(collection["statusCode"]);

                    TempData["notice"] = Locale.SuccessfullyCreated;
                    //auditTrailDao.Log("Add Bank Host Status Master KBZ - Status ID : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_AddTemplate(collection["fldBankHostStatusCode"], "Add", "N");
                    auditTrailDao.SecurityLog("Add Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else
                {

                    bankHostStatusKBZDao.CreateBankHostStatusMasterKBZTemp(collection);
                    TempData["notice"] = Locale.BankHostMasterCreateVerify;

                    //auditTrailDao.Log("AddBank Host Status Master KBZ Temporary Record - Status Id : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_AddTemplate(collection["fldBankHostStatusCode"], "Add", "Y");
                    auditTrailDao.SecurityLog("Add Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
            }
            return RedirectToAction("Create");
        }


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZ.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.BankHostStatusKBZ.DELETE;
            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                        bankHostStatusKBZDao.DeleteInBankHostStatusMasterKBZ(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                    }
                    else
                    {
                        bool IsBankHostCodeTempExist = bankHostStatusKBZDao.CheckBankHostCodeDataTempKBZ(arrResult);
                        if (IsBankHostCodeTempExist == true)
                        {
                            TempData["Warning"] = Locale.BankHostCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                            bankHostStatusKBZDao.AddtoBankHostStatusMasterKBZTempToDelete(arrResult);
                            TempData["notice"] = Locale.BankHostMasterDeleteVerify;
                        }
                    }
                }

                if ("N".Equals(systemProfile))
                {
                    //auditTrailDao.Log("Delete Bank Host Status Master - Status Id: " + collection["deleteBox"], CurrentUser.Account);
                }
                else
                {
                    //auditTrailDao.Log("Add Bank Host Status Master into Temporary Record to Delete - Status Id: " + collection["deleteBox"], CurrentUser.Account);
                }

            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }

    }
}