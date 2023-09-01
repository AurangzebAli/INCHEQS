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
using INCHEQS.Security.SystemProfile;
using System.Globalization;
using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class HolidayController : BaseController
    {
        private iHolidayDao holidayDao;
        protected readonly ISearchPageService searchPageService;
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public HolidayController(IAuditTrailDao auditTrailDao, iHolidayDao holidayDao, ISearchPageService searchPageService, IPageConfigDao pageConfigDao, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.auditTrailDao = auditTrailDao;
            this.holidayDao = holidayDao;
            this.searchPageService = searchPageService;
            this.pageConfigDao = pageConfigDao;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.Holiday.INDEX));
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.Holiday.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Holiday.INDEX, "view_Holiday", "fldHolidayDesc asc,fldRecurring asc", ""
                //,new[] { new SqlParameter() }
                ),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            string getDate = "";
            string getDateDb = "";
            string sTaskId = TaskIds.Holiday.SAVECREATE;
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("HolidayChecker", CurrentUser.Account.BankCode).Trim();
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> errorMessages = holidayDao.ValidateHoliday(col, "Create");
                //auditTrailDao.Log("validate one time - Holiday : " + col["fldDate"], CurrentUser.Account);
                //auditTrailDao.Log("validate yearly - Holiday : " + col["fldYearDate"], CurrentUser.Account);


                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    string OneTime = col["OneTime"].ToString();
                    if ("N".Equals(systemProfile))
                    {
                        //auditTrailDao.Log("calling  : holidayDao.InsertHoliday(col)", CurrentUser.Account);
                        holidayDao.InsertHoliday(col);
                        //auditTrailDao.Log("called  : holidayDao.InsertHoliday(col)", CurrentUser.Account);
                        #region AuditLog
                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                        if (OneTime == "OneTime")
                        {
                            getDate = col["fldDate"].ToString();
                            col["RecurringType"] = "Onetime";
                            //getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                        }
                        else
                        {
                            if (col.AllKeys.Contains("RecurringType"))
                            {
                                if (col["RecurringType"].ToString() == "Week")
                                {
                                    getDate = "";
                                }
                                else
                                {
                                    getDate = col["fldYearDate"].ToString();
                                    //getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                                }
                            }
                        }

                        //auditTrailDao.Log("Add - Holiday : " + getDate + " " + col["fldHolidayDescription"] + " , RecurringType  :" + col["RecurringType"].ToString(), CurrentUser.Account);

                        #endregion
                        string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_AddTemplate(col, getDate, "Add");
                        auditTrailDao.SecurityLog("Add Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        //auditTrailDao.Log("calling  : holidayDao.CreateHolidayToHolidayTemp(col)", CurrentUser.Account);
                        holidayDao.CreateHolidayToHolidayTemp(col);
                        //auditTrailDao.Log("called  : holidayDao.CreateHolidayToHolidayTemp(col)", CurrentUser.Account);

                        #region AuditLog
                        TempData["Notice"] = Locale.HolidayCreateVerify;
                        if (OneTime == "OneTime")
                        {
                            getDate = col["fldDate"].ToString();
                            col["RecurringType"] = "Onetime";
                            //getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                        }
                        else
                        {
                            if (col.AllKeys.Contains("RecurringType"))
                            {
                                if (col["RecurringType"].ToString() == "Week")
                                {
                                    getDate = "";
                                }
                                else
                                {
                                    getDate = col["fldYearDate"].ToString();
                                    //getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                                }
                            }
                        }
                        //auditTrailDao.Log("Add Holiday into Temporary record to Create - Holiday Date : " + getDate + " , Description :  " + col["fldHolidayDescription"] + " RecurringType  :" + col["RecurringType"].ToString(), CurrentUser.Account);

                        #endregion

                        string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_AddTemplate(col, getDate, "Add");
                        auditTrailDao.SecurityLog("Add Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                //auditTrailDao.Log("validate one time ex - Holiday : " + col["fldDate"], CurrentUser.Account);
                //auditTrailDao.Log("validate yearly ex - Holiday : " + col["fldYearDate"], CurrentUser.Account);
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string row_fldHolidayId = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string HolidayId = "";
            try
            {
                if (string.IsNullOrEmpty(row_fldHolidayId))
                {
                    HolidayId = filter["fldHolidayId"].Trim();
                }
                else
                {
                    HolidayId = row_fldHolidayId;
                }
                ViewBag.holiday = holidayDao.GetHolidayData(HolidayId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.Holiday.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.Holiday.UPDATE;
            string value = col["recurrTypeVal"];
            try
            {
                if ("N".Equals(systemProfile))
                {
                    HolidayModel before = holidayDao.GetHolidayData(col["HolidayId"]);
                    //auditTrailDao.Log("Holiday Calender Created - Holiday Date : " + before.Date + ", Recurring : " + before.recurring + ", Recurring Type : " + before.recurrType + ", Description : " + before.Desc, CurrentUser.Account);

                    holidayDao.UpdateHolidayTable(col);
                    TempData["Notice"] = Locale.RecordsuccesfullyUpdated;

                    HolidayModel after = holidayDao.GetHolidayData(col["HolidayId"]);
                    //auditTrailDao.Log("Holiday Calender Created - Holiday Date : " + after.Date + ", Recurring : " + after.recurring + ", Recurring Type : " + after.recurrType + ", Description : " + after.Desc, CurrentUser.Account);

                    HolidayModel after2 = MaintenanceAuditLogDao.GetHolidayDatabyId(col["HolidayId"]);
                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_EditTemplate(col["HolidayId"], before, after2, "", "Edit");
                    auditTrailDao.SecurityLog("Edit Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else
                {
                    bool IsHolidayExist = holidayDao.CheckHolidayExist(col["HolidayId"]);
                    if (IsHolidayExist == true)
                    {
                        TempData["warning"] = Locale.HolidayAlreadyExiststoDeleteorUpdate;
                    }
                    else
                    {
                        HolidayModel before = MaintenanceAuditLogDao.GetHolidayDatabyId(col["HolidayId"]);
                        holidayDao.AddHolidayinTemptoUpdate(col);
                        HolidayModel after = MaintenanceAuditLogDao.GetHolidayDataTempbyId(col["HolidayId"]);
                        TempData["Notice"] = Locale.HolidayUpdateVerify;
                        //auditTrailDao.Log("Add Holiday Calender in Temp Table to Update, Holiday ID : " + col["HolidayId"] + ", Description to Update : " + col["fldHolidayDescription"], CurrentUser.Account);
                        string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_EditTemplate(col["HolidayId"], before, after, "", "Edit");
                        auditTrailDao.SecurityLog("Edit Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }

                return RedirectToAction("Edit", new { row_fldHolidayId = col["HolidayId"].Trim() });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.Holiday.DELETE;
            if (col != null & col["deleteBox"] != null)
            {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        HolidayModel objHolidayDeleted = holidayDao.GetHolidayData(arrResult);
                        HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDatabyId(arrResult);
                        string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_DeleteTemplate(arrResult, sHoliday, "", "Delete");
                        auditTrailDao.SecurityLog("Delete Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                        holidayDao.DeleteFromHolidayTable(arrResult);
                        TempData["Notice"] = Resources.Locale.RecordsuccesfullyDeleted;
                        //auditTrailDao.Log("Holiday Calender Deleted - Holiday Date : " + objHolidayDeleted.Date + ", Recurring : " + objHolidayDeleted.recurring + ", Recurring Type : " + objHolidayDeleted.recurrType + ", Description : " + objHolidayDeleted.Desc, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsHolidayExist = holidayDao.CheckHolidayExist(arrResult);
                        if (IsHolidayExist == true)
                        {
                            TempData["warning"] = Locale.HolidayAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            HolidayModel objHolidayDeleted = holidayDao.GetHolidayData(arrResult);
                            HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDatabyId(arrResult);
                            holidayDao.AddHolidayinTemptoDelete(arrResult);
                            TempData["Notice"] = Locale.HolidayDeleteVerify;
                            //auditTrailDao.Log("Add Holiday into Temporary record to Delete - Holiday Date : " + objHolidayDeleted.Date + ", Recurring : " + objHolidayDeleted.recurring + ", Recurring Type : " + objHolidayDeleted.recurrType + ", Description : " + objHolidayDeleted.Desc, CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.HolidayCalendar_DeleteTemplate(arrResult, sHoliday, "temp", "Delete");
                            auditTrailDao.SecurityLog("Delete Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
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


    }
}