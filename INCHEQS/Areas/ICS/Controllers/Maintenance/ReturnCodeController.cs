// bank of Malaysia
//using INCHEQS.BNM.ReturnCode;

// bank of Philipine
using INCHEQS.PCHC.ReturnCode;

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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    
    public class ReturnCodeController : BaseController {

        private IReturnCodeDao returnCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;

        public ReturnCodeController(IReturnCodeDao returnCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.returnCodeDao = returnCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ReturnCode.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ReturnCode.INDEX, "View_ReturnCode", "fldRejectCode"),
            collection);
            return View();
        }


        // GET: ReturnCode/Details?..
        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Details(FormCollection col,string returnCodeParam="") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string RejectCode = "";
            if (string.IsNullOrEmpty(returnCodeParam)) {
                RejectCode = filter["fldRejectCode"].Trim();
            } else {
                RejectCode = returnCodeParam;
            }
            DataTable dataTable = await returnCodeDao.FindAsync(RejectCode);

            if ((dataTable.Rows.Count > 0)) {
                ViewBag.ReturnCode = dataTable.Rows[0];
                ViewBag.RejectTypes = await returnCodeDao.FindAllRejectTypesAsync();

            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.UPDATE)]
        [HttpPost()]
        public async Task<ActionResult> Update(FormCollection collection) {

            try {
                List<String> errorMessages = await returnCodeDao.ValidateUpdateAsync(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Details",new { returnCodeParam = collection["txtRejectCode"].Trim() });
                } else {
                    ReturnCodeModel before = await returnCodeDao.getRejectAsync(collection["txtRejectCode"]);
                    auditTrailDao.Log("Edit tblRejectMaster - Before Update=> Return Code : " + before.rejectCode + " Return Desc : " + before.rejectDesc + " UpdateTimestamp : " + before.updateDate + " Type : " + before.rejectType + " UpdateID : " + before.updateId, CurrentUser.Account);

                    returnCodeDao.Update(collection["txtRejectDesc"], collection["txtType"], collection["txtUnposted"], CurrentUser.Account.UserId, /* collection["txtCharges"]*/"", collection["txtRejectCode"]);

                    ReturnCodeModel after = await returnCodeDao.getRejectAsync(collection["txtRejectCode"]);
                    auditTrailDao.Log("Edit tblRejectMaster - After Update=> Return Code : " + after.rejectCode + " Return Desc : " + after.rejectDesc + " UpdateTimestamp : " + after.updateDate + " Type : " + after.rejectType + " UpdateID : " + after.updateId, CurrentUser.Account);

                    TempData["Notice"] = Locale.ReturnCodeUpdated;
                    return RedirectToAction("Index");

                }
            } catch (Exception ex) {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.SAVECREATE)]
        [HttpPost()]
        public async Task<ActionResult> SaveCreate(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("ReturnCodeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<String> errorMessages = await returnCodeDao.ValidateCreateAsync(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                } else {
                    if ("N".Equals(systemProfile)) {
                        await returnCodeDao.CreateInRejectMasterTempAsync(collection, CurrentUser.Account.UserId);
                        returnCodeDao.CreateInRejectMaster(collection["txtRejectCode"]);
                        returnCodeDao.DeleteInRejectMasterTemp(collection["txtRejectCode"]);

                        TempData["Notice"] = Locale.ReturnReasonCodeSuccessfullyCreated;
                        auditTrailDao.Log("Add - RejectMaster table - Return Code :  " + collection["txtRejectCode"] + " Return Desc : " + collection["txtRejectDesc"], CurrentUser.Account);
                    } else {
                        await returnCodeDao.CreateInRejectMasterTempAsync(collection, CurrentUser.Account.UserId);

                        TempData["Notice"] = Locale.ReturnReasonCodeCreateVerify;
                        auditTrailDao.Log("Add record into RejectMasterTemp table -  Return Code :  " + collection["txtRejectCode"] + " Return Desc : " + collection["txtRejectDesc"], CurrentUser.Account);
                    }

                    
                    return RedirectToAction("Index");
                }
            } catch (Exception ex) {

                throw ex;
            }
            
        }

        // GET: TransactionCode
        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Create() {
            ViewBag.RejectTypes = await returnCodeDao.FindAllRejectTypesAsync();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ReturnCode.DELETE)]
        // TransactionCode/Delete
        [HttpPost()]
        public ActionResult Delete(FormCollection collection) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("ReturnCodeChecker", CurrentUser.Account.BankCode).Trim();
            try {
                if (collection != null & collection["deleteBox"] != null) {
                    List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults) {
                        if ("N".Equals(systemProfile)) {
                            returnCodeDao.DeleteInRejectMaster(arrResult);
                            TempData["Notice"] = Locale.ReturnCodesDeleted;
                        } else {
                            returnCodeDao.AddToRejectMasterTempToDelete(arrResult);
                            TempData["Notice"] = Locale.ReturnReasonCodeVerifyDelete;
                        }
                    }
                    if ("N".Equals(systemProfile)) {
                        auditTrailDao.Log("Delete - RejectMaster table - Return Code :  " + collection["deleteBox"], CurrentUser.Account);
                    } else {
                        auditTrailDao.Log("Add into RejectMasterTemp table to Delete -  Return Code :  " + collection["deleteBox"], CurrentUser.Account);
                    }
                } else {
                    TempData["warning"] = Locale.Norecordsselected;
                }
            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}
