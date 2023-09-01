using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.TCCode;
using INCHEQS.Resources;
using INCHEQS.Security;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance {

    public class TCCodeController : BaseController {

        private static readonly ILog _log = LogManager.GetLogger(typeof(TCCodeController));
        private ITCCodeDao tccodeDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        public TCCodeController(ITCCodeDao tccodeDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao) {
            this.pageConfigDao = pageConfigDao;
            this.tccodeDao = tccodeDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
        }

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Index() {
        //    ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.TCCode.INDEX));
        //    return View();
        //}
        //[CustomAuthorize(TaskIds = TaskIds.TCCode.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public virtual ActionResult SearchResultPage(FormCollection collection) {
        //    ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.TCCode.INDEX, "View_TCCode", "fldTCCode"),
        //    collection);
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.DETAIL)]
        //[GenericFilter(AllowHttpGet = true)]
        //public async Task<ActionResult> Details(FormCollection collection, string TCCode = null) {
        //    Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
        //    if (!string.IsNullOrEmpty(TCCode)) {
        //        ViewBag.TCCodes = await tccodeDao.FindAsync(TCCode);
        //    } else {
        //        ViewBag.TCCodes = await tccodeDao.FindAsync(filter["fldTCCode"]);
        //    }
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.UPDATE)]
        //public ActionResult Update(FormCollection col) {
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("TCCodeChecker", CurrentUser.Account.BankCode).Trim();
        //    try
        //    {
        //        List<string> errorMessages = tccodeDao.ValidateUpdate(col);

        //        if ((errorMessages.Count > 0))
        //        {
        //            TempData["ErrorMsg"] = errorMessages;
        //            return RedirectToAction("Details", new { TCCode = col["tcCode"].Trim() });
        //        }
        //        else if ("N".Equals(systemProfile))
        //        {
        //            tccodeDao.AddToTCCodeTempToUpdate(col["tcCode"]);
        //            tccodeDao.UpdateTCCodeInTemp(col);
        //            tccodeDao.UpdateTCCode(col["tcCode"]);
        //            tccodeDao.DeleteInTCCodeTemp(col["tcCode"]);

        //            TempData["Notice"] = Locale.TCCodeSuccessfullyUpdated;
        //            auditTrailDao.Log("Add - TC Code : " + col["tcCode"] + ", TCCode Desc : " + col["transDesc"], CurrentUser.Account);
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            tccodeDao.AddToTCCodeTempToUpdate(col["tcCode"]);
        //            tccodeDao.UpdateTCCodeInTemp(col);
                    
        //            TempData["Notice"] = Locale.TCCodeAddedToTempForUpdate;
        //            auditTrailDao.Log("Add into Temporary Record to Update - TC Code : " + col["tcCode"] + ", TCCode Desc : " + col["transDesc"], CurrentUser.Account);
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.CREATE)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Create() {
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.SAVECREATE)]
        //[HttpPost()]
        //public ActionResult SaveCreate(FormCollection col) {
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("TCCodeChecker", CurrentUser.Account.BankCode).Trim();
        //    try {
        //        List<string> errorMessages = tccodeDao.ValidateCreate(col);

        //        if ((errorMessages.Count > 0)) {
        //            TempData["ErrorMsg"] = errorMessages;

        //        } else if ("N".Equals(systemProfile)) {
        //            tccodeDao.CreateTCCodeInTemp(col, CurrentUser.Account);
        //            tccodeDao.CreateTCCode(col["tcCode"]);
        //            tccodeDao.DeleteInTCCodeTemp(col["tcCode"]);

        //            TempData["Notice"] = Locale.TCCodeSuccessfullyCreated;
        //            auditTrailDao.Log("Add - TC Code : " + col["tcCode"] + ", TCCode Desc : " + col["transDesc"], CurrentUser.Account);
        //        } else {
        //            tccodeDao.CreateTCCodeInTemp(col, CurrentUser.Account);

        //            TempData["Notice"] =  Locale.TCCodeAddedToTempForCreate;
        //            auditTrailDao.Log("Add into Temporary Record to Create - TC Code : " + col["tcCode"] + ", TCCode Desc : " + col["transDesc"], CurrentUser.Account);
        //        }
        //        return RedirectToAction("Create") ;

        //    } catch (Exception ex) {
        //        throw ex;
        //    }
        //}

        //[CustomAuthorize(TaskIds = TaskIds.TCCode.DELETE)]
        //[HttpPost()]
        //public ActionResult Delete(FormCollection collection) {
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("TCCodeChecker", CurrentUser.Account.BankCode).Trim();
        //    if (collection != null & collection["deleteBox"] != null) {
        //        List<string> arrResults = collection["deleteBox"].Split(',').ToList();
        //        foreach (string arrResult in arrResults) {
        //            if (tccodeDao.CheckPendingApproval(arrResult))
        //            {
        //                TempData["ErrorMsg"] = Locale.TCCodePendingApproval;
        //            }
        //            else if ("N".Equals(systemProfile)) {
        //                tccodeDao.DeleteTCCode(arrResult);
        //                TempData["Warning"] = Locale.SuccessfullyDeleted;
        //                auditTrailDao.Log("Delete - TC Code : " + collection["deleteBox"], CurrentUser.Account);
        //            } else {
        //                tccodeDao.AddToTCCodeTempToDelete(arrResult);

        //                TempData["Notice"] =  Locale.TCCodeAddedToTempForDelete;
        //                auditTrailDao.Log("Insert into Temporary Table to Delete - TC Code : " + collection["deleteBox"], CurrentUser.Account);
        //            }
        //        }
        //    } else {
        //        TempData["Warning"] = Locale.Nodatawasselected;
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}
