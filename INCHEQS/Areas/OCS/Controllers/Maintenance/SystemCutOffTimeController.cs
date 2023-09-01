using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
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
using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Common;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{

    public class SystemCutOffTimeController : BaseController
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SystemCutOffTimeController));
        private ISystemCutOffTimeDao systemCutOffTimeDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private ITransactionTypeDao transactiontypeDao;

        public SystemCutOffTimeController(ITransactionTypeDao transactiontypeDao, ISystemCutOffTimeDao systemCutOffTimeDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.systemCutOffTimeDao = systemCutOffTimeDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
            this.transactiontypeDao = transactiontypeDao;
        }

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Index()
        //{
        //    ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.SystemCutOffTime.INDEX));
        //    return View();
        //}   

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public virtual ActionResult SearchResultPage(FormCollection collection)
        //{
        //    ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.SystemCutOffTime.INDEX, "View_SystemCutOffTime", "fldDesc"),
        //    collection);
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.EDIT)]
        //[GenericFilter(AllowHttpGet = true)]
        //public async Task<ActionResult> Edit(FormCollection collection, string desc = null)
        //{
        //    Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
 
        //    if (!string.IsNullOrEmpty(desc))
        //    {
        //        ViewBag.SystemCutOffTimes = await systemCutOffTimeDao.FindAsync(desc);
        //    }
        //    else
        //    {
        //        ViewBag.SystemCutOffTimes = await systemCutOffTimeDao.FindAsync(filter["fldDesc"]);           
        //    }
        //    if (ViewBag.SystemCutOffTimes.Rows.Count > 0)
        //    {
        //        DateTime startTime = Convert.ToDateTime(ViewBag.SystemCutOffTimes.Rows[0]["fldStartTime"]);
        //        DateTime endTime = Convert.ToDateTime(ViewBag.SystemCutOffTimes.Rows[0]["fldEndTime"]);

        //        ViewBag.StartTimeHour = StringUtils.Mid(startTime.ToString("HH:mm"), 0, 2);
        //        ViewBag.StartTimeMin = StringUtils.Mid(startTime.ToString("HH:mm"), 3, 2);
        //        ViewBag.EndTimeHour = StringUtils.Mid(endTime.ToString("HH:mm"), 0, 2);
        //        ViewBag.EndTimeMin = StringUtils.Mid(endTime.ToString("HH:mm"), 3, 2);
        //    }

        //    ViewBag.TransactionTypes = transactiontypeDao.ListTransactionTypes();
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.UPDATE)]
        //public ActionResult Update(FormCollection col)
        //{
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("SystemCutOffTimeChecker", CurrentUser.Account.BankCode).Trim();
        //    try
        //    {
        //        List<string> errorMessages = systemCutOffTimeDao.ValidateUpdate(col);

        //        if ((errorMessages.Count > 0))
        //        {
        //            TempData["ErrorMsg"] = errorMessages;
        //            return RedirectToAction("Edit", new { desc = col["fldDesc"] });

        //        }
        //        else if ("N".Equals(systemProfile))
        //        {
        //            systemCutOffTimeDao.AddToSystemCutOffTimeTempToUpdate(col, CurrentUser.Account);
        //            systemCutOffTimeDao.UpdateSystemCutOffTime(col["fldDesc"]);
        //            systemCutOffTimeDao.DeleteInSystemCutOffTimeTemp(col["fldDesc"]);

        //            TempData["Notice"] = Locale.SystemCutOffTimeSuccessfullyUpdated;
        //            auditTrailDao.Log("Add - System Cut Off Time : " + col["fldDesc"] + ", Transaction Type : " + col["fldTransacationType"], CurrentUser.Account);
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            systemCutOffTimeDao.AddToSystemCutOffTimeTempToUpdate(col, CurrentUser.Account);
        //            TempData["Notice"] = Locale.SystemCutOffTimeAddedToTempForUpdate;
        //            auditTrailDao.Log("Add into Temporary Record to Update - System Cut Off Time : " + col["fldDesc"] + ", Transaction Type : " + col["fldTransacationType"], CurrentUser.Account);
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.CREATE)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.SAVECREATE)]
        //[HttpPost()]
        //public ActionResult SaveCreate(FormCollection col)
        //{
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("SystemCutOffTimeChecker", CurrentUser.Account.BankCode).Trim();
        //    try
        //    {
        //        List<string> errorMessages = systemCutOffTimeDao.ValidateCreate(col);

        //        if ((errorMessages.Count > 0))
        //        {
        //            TempData["ErrorMsg"] = errorMessages;

        //        }
        //        else if ("N".Equals(systemProfile))
        //        {
        //            systemCutOffTimeDao.CreateSystemCutOffTimeInTemp(col, CurrentUser.Account);
        //            systemCutOffTimeDao.CreateSystemCutOffTime(col["fldDesc"]);
        //            systemCutOffTimeDao.DeleteInSystemCutOffTimeTemp(col["fldDesc"]);

        //            TempData["Notice"] = Locale.SystemCutOffTimeSuccessfullyCreated;
        //            auditTrailDao.Log("Add - System Cut Off Time : " + col["fldDesc"] + ", Transaction Type : " + col["transType"], CurrentUser.Account);
        //        }
        //        else
        //        {
        //            systemCutOffTimeDao.CreateSystemCutOffTimeInTemp(col, CurrentUser.Account);
        //            TempData["Notice"] = Locale.SystemCutOffTimeAddedToTempForCreate;
        //            auditTrailDao.Log("Add into Temporary Record to Create - Description : " + col["fldDesc"] + ", Transaction Type : " + col["fldTransactionType"], CurrentUser.Account);
        //        }
        //        return RedirectToAction("Create");

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemCutOffTime.DELETE)]
        //[HttpPost()]
        //public ActionResult Delete(FormCollection collection)
        //{
        //    string systemProfile = systemProfileDao.GetValueFromSystemProfile("SystemCutOffTimeChecker", CurrentUser.Account.BankCode).Trim();
        //    if (collection != null & collection["deleteBox"] != null)
        //    {
        //        List<string> arrResults = collection["deleteBox"].Split(',').ToList();
        //        foreach (string arrResult in arrResults)
        //        {
        //            if (systemCutOffTimeDao.CheckPendingApproval(arrResult))
        //            {
        //                TempData["ErrorMsg"] = Locale.SystemCutOffTimePendingApproval;
        //            }
        //            else if ("N".Equals(systemProfile))
        //            {
        //                systemCutOffTimeDao.DeleteSystemCutOffTime(arrResult);
        //                TempData["Warning"] = Locale.SuccessfullyDeleted;
        //                auditTrailDao.Log("Delete - System Cut Off Time : " + collection["deleteBox"], CurrentUser.Account);
        //            }
        //            else
        //            {
        //                systemCutOffTimeDao.AddToSystemCutOffTimeTempToDelete(arrResult);
        //                TempData["Notice"] = Locale.SystemCutOffTimeAddedToTempForDelete;
        //                auditTrailDao.Log("Insert into Temporary Table to Delete - System Cut Off Time : " + collection["deleteBox"], CurrentUser.Account);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        TempData["Warning"] = Locale.Nodatawasselected;
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}
