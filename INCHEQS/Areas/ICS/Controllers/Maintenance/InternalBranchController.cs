//using INCHEQS.Models.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.InternalBranch.InternalBranch;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using System.Linq;
//using INCHEQS.InternalBranch.InternalBranch;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
 
    public class InternalBranchController : BaseController {
        
        private IInternalBranchDao internalBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;

    public InternalBranchController(IInternalBranchDao InternalBranch, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
        this.internalBranchDao = InternalBranch;
        this.auditTrailDao = auditTrailDao;
        this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.InternalBranch.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.InternalBranch.INDEX, "View_InternalBranch", "fldSOL, fldStateCode, fldBranchCode", "fldBankCode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            }),
            collection);
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string fldConBranchCode = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string InternalBranchCode;
            if (string.IsNullOrEmpty(fldConBranchCode))
            {
                InternalBranchCode = filter["fldConBranchCode"].Trim();
            }
            else
            {
                InternalBranchCode = fldConBranchCode;
            }
            //string InternalBranchCode = filter["fldConBranchCode"];
            if ((InternalBranchCode == null)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } else {
                ViewBag.InternalBranch = internalBranchDao.FindInternalBranchCode(InternalBranchCode);
                return View();
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.UPDATE)]
        public ActionResult Update(FormCollection collection) {
            //string conBranchCode = collection[""] + collection[""] + collection[""];
            try
            {
                List<string> errorMessages = internalBranchDao.ValidateEdit(collection);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Edit", new { fldConBranchCode = collection["txtBranchCode2"] });
                }
                else
                {
                    InternalBranchModel before = internalBranchDao.FindInternalBranchCode(collection["txtBranchCode2"]);
                    auditTrailDao.Log("Edit Internal Branch - Before Update => ConBranchCode : " + before.fldConBranchCode + ", Description : " + before.fldBranchDesc, CurrentUser.Account);

                    internalBranchDao.UpdateInternalBranch(collection);
                    TempData["notice"] = Locale.SuccessfullyUpdated;

                    InternalBranchModel after = internalBranchDao.FindInternalBranchCode(collection["txtBranchCode2"]);
                    auditTrailDao.Log("Edit Internal Branch - Before Update => ConBranchCode : " + after.fldConBranchCode + ", Description : " + after.fldBranchDesc, CurrentUser.Account);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create(){
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection,string bankCode) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("InternalBranchChecker", CurrentUser.Account.BankCode).Trim();
            string conBranchCode = collection["txtBranchCode1"].Substring(0,2) + collection["bankDesc"] + collection["txtBranchCode2"];
            try {
                List<string> errorMessages = internalBranchDao.ValidateCreate(collection,bankCode);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");
                } else {
                    if ("N".Equals(systemProfile)) {

                        internalBranchDao.CreateInternalBranchTemp(collection, CurrentUser.Account.BankCode);
                        internalBranchDao.CreateInInternalBranch(conBranchCode);
                        internalBranchDao.DeleteInInternalBranchTemp(conBranchCode);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - Internal Branch Code : " + collection["txtBranchCode2"] + " Branch Desc : " + collection["txtBranchDesc"] + " ConBranchCode : " + CurrentUser.Account.BankCode + collection["txtBranchCode2"], CurrentUser.Account);

                    } else {
                        internalBranchDao.CreateInternalBranchTemp(collection, CurrentUser.Account.BankCode);

                        TempData["Notice"] = Locale.InternalBranchCreateVerify;
                        auditTrailDao.Log("Add Temporary Branch Code - Internal Branch Code : " + collection["txtBranchCode2"] + " Branch Desc : " + collection["txtBranchDesc"] + " ConBranchCode : " + CurrentUser.Account.BankCode + collection["txtBranchCode2"], CurrentUser.Account);

                    }
                    return RedirectToAction("Index");
                }
            } catch (Exception ex) {
                throw ex;
            }
            
        }

        [CustomAuthorize(TaskIds = TaskIds.InternalBranch.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("InternalBranchChecker", CurrentUser.Account.BankCode).Trim();
            if (col["deleteBox"] != null) {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults) {
                    if ("N".Equals(systemProfile)) {
                        internalBranchDao.DeleteInInternalBranch(arrResult);
                        TempData["Notice"] = Locale.SuccessfullyDeleted;
                    } else {
                        internalBranchDao.AddToMapBranchTempToDelete(arrResult);
                        TempData["Notice"] = Locale.InternalBranchDeleteVerify;
                    }
                }
                if ("N".Equals(systemProfile)) {
                    auditTrailDao.Log("Delete - Map Branch Code : " + col["deleteBox"], CurrentUser.Account);
                } else {
                    auditTrailDao.Log("Add temporary record to Delete - Map Branch Code : " + col["deleteBox"], CurrentUser.Account);
                }
            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            return RedirectToAction("Index");
        }

    }
    
}