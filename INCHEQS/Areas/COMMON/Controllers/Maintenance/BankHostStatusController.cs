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
using INCHEQS.Areas.COMMON.Models.BankHostStatus;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankHostStatusController : Controller
    {
        private readonly IBankHostStatusDao bankHostStatusDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected ISystemProfileDao systemProfileDao;

        public BankHostStatusController(IBankHostStatusDao bankHostStatusDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.bankHostStatusDao = bankHostStatusDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        // GET: ICS/HostReturnReason
        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankHostStatus.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankHostStatus.INDEX, "View_BankHostStatusMaster", "fldBankHostStatusCode"),
            collection);
            return View();
        }

        // GET: ReturnCode/Details?..
        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.EDIT)]
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

            ViewBag.BankHostStatus = bankHostStatusDao.GetBankHostStatusMaster(strStatusCode);
            ViewBag.HostStatusAction = bankHostStatusDao.ListHostStatusAction();
            ViewBag.RejectCode = bankHostStatusDao.ListHostRejectCode();
            //DataTable dataTable = bankHostStatusDao.GetBankHostStatusMaster(strStatusCode);
            //if ((dataTable.Rows.Count > 0))
            //{
            //    ViewBag.BankHostStatus = dataTable.Rows[0];
            //}
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            List<String> errorMessages = bankHostStatusDao.ValidateUpdate(collection);
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();

            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else {

                if ("N".Equals(systemProfile))
                {
                    BankHostStatusModel before = bankHostStatusDao.GetHostReturnReasonModel(collection["fldBankHoststatusCode"]);
                    //auditTrailDao.Log("Edit - Bank Host Status Master - Before Update => Status Id : " + before.fldBankHostStatusCode + ", Status Description : " + before.fldBankHostStatusDesc, CurrentUser.Account);

                    bankHostStatusDao.UpdateHostStatusMaster(collection, CurrentUser.Account);
                    TempData["notice"] = Locale.SuccessfullyUpdated;

                    BankHostStatusModel after = bankHostStatusDao.GetHostReturnReasonModel(collection["fldBankHoststatusCode"]);
                    //auditTrailDao.Log("Edit - Bank Host Status Master - After Update => Status Id : " + after.fldBankHostStatusCode + ", Status Description : " + after.fldBankHostStatusDesc, CurrentUser.Account);
                }
                else
                {

                    bool IsBankHostCodeExist = bankHostStatusDao.CheckBankHostCodeDataTemp(collection["fldBankHostStatusCode"]);

                    if (IsBankHostCodeExist == true)
                    {
                        TempData["Warning"] = Locale.BankHostCodeAlreadyExiststoDeleteorUpdate;
                    }
                    else
                    {

                        bankHostStatusDao.AddBankHostCodeinTemptoUpdate(collection, CurrentUser.Account);
                        TempData["notice"] = Locale.BankHostMasterUpdateVerify;
                    }
                    //auditTrailDao.Log("AddBank Bank Host Status Master Temporary Record - Status Id : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);

                }
            }
            return RedirectToAction("Edit", new { statusCodeParam = collection["fldBankHoststatusCode"] });
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            ViewBag.HostStatusAction = bankHostStatusDao.ListHostStatusAction();
            ViewBag.RejectCode = bankHostStatusDao.ListHostRejectCode();

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            List<String> errorMessages = bankHostStatusDao.ValidateCreate(collection);
         

            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {
                if ("N".Equals(systemProfile))
                {
                   // bankHostStatusDao.CreateBankHostStatusMasterTemp(collection, strAutoPending, strAutoReject, CurrentUser.Account);
                    bankHostStatusDao.CreateInBankHostStatusMaster(collection);
                    //bankHostStatusDao.DeleteInBankHostStatusMasterTemp(collection["statusCode"]);

                    TempData["notice"] = Locale.SuccessfullyCreated;
                    //auditTrailDao.Log("Add Bank Host Status Master - Status ID : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);

                }
                else
                {

                    bankHostStatusDao.CreateBankHostStatusMasterTemp(collection);
                    TempData["notice"] = Locale.BankHostMasterCreateVerify;

                    //auditTrailDao.Log("AddBank Host Status Master Temporary Record - Status Id : " + collection["fldBankHostStatusCode"] + ", Status Description : " + collection["fldBankHostStatusDesc"], CurrentUser.Account);
                }
            }
            return RedirectToAction("Create");
        }


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatus.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection collection)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            if (collection != null & collection["deleteBox"] != null)
            {
                
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        bankHostStatusDao.DeleteInBankHostStatusMaster(arrResult);
                        TempData["notice"] = Locale.SuccessfullyDeleted;
                    }
                    else
                    {
                        bool IsBankHostCodeTempExist = bankHostStatusDao.CheckBankHostCodeDataTemp(arrResult);
                        if (IsBankHostCodeTempExist == true)
                        {
                            TempData["Warning"] = Locale.BankHostCodeAlreadyExiststoDeleteorUpdate;
                        }
                        else { 
                            bankHostStatusDao.AddtoBankHostStatusMasterTempToDelete(arrResult);
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