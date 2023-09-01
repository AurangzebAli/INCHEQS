using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class HolidayApprovedCheckerController : BaseController
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private iHolidayDao holidayDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public HolidayApprovedCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, iHolidayDao holidayDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.holidayDao = holidayDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HolidayApprovedChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.HolidayApprovedChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HolidayApprovedChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.HolidayApprovedChecker.INDEX, "View_HolidayApprovedChecker", "actionstatus ASC, fldName ASC", ""
                //, new[] {new SqlParameter()}
                ),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HolidayApprovedChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.HolidayApprovedChecker.VERIFY;
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.Holiday.INDEX:
                                if (action.Equals("A"))
                                {
                                    holidayDao.CreateHolidayinMain(id);
                                    HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDatabyId(id);
                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_AddTemplate(id, sHoliday, "", "Approve");
                                    auditTrailDao.SecurityLog("Approve Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    HolidayModel objHoliday = holidayDao.GetHolidayData(id);
                                    HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDatabyId(id);
                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_DeleteTemplate(id, sHoliday, "temp", "Approve");
                                    auditTrailDao.SecurityLog("Approve Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                    holidayDao.DeleteFromHolidayTable(id);
                                    //auditTrailDao.Log("Deleted Holiday from Main - Holiday Date :" + objHoliday.Date + ", Recurring : " + objHoliday.recurring + ", Recurring Type : " + objHoliday.recurrType + ", Description : " + objHoliday.Desc, CurrentUser.Account);

                                }
                                else if (action.Equals("U"))
                                {
                                    HolidayModel before = holidayDao.GetHolidayData(id);
                                    HolidayModel sBefore = MaintenanceAuditLogDao.GetHolidayDatabyId(id);

                                    //auditTrailDao.Log("Holiday Calender Created Before - Holiday Date : " + before.Date + ", Recurring : " + before.recurring + ", Recurring Type : " + before.recurrType + ", Description : " + before.Desc, CurrentUser.Account);

                                    holidayDao.UpdateHolidayToMain(id);

                                    HolidayModel after = holidayDao.GetHolidayData(id);
                                    HolidayModel sAfter = MaintenanceAuditLogDao.GetHolidayDatabyId(id);
                                    //auditTrailDao.Log("Holiday Calender Created After- Holiday Date : " + after.Date + ", Recurring : " + after.recurring + ", Recurring Type : " + after.recurrType + ", Description : " + after.Desc, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_EditTemplate(id, sBefore, sAfter, "", "Approve");
                                    auditTrailDao.SecurityLog("Approve Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                holidayDao.DeleteHolidayinTemp(id, Convert.ToInt32(arrResult.Length));
                                if (arrResult.Length < 15)
                                {
                                    //auditTrailDao.Log("Approve Holiday - Task Assigment :" + taskId + " Holiday ID : " + id, CurrentUser.Account);

                                }
                                else
                                {
                                    //auditTrailDao.Log("Approve Holiday - Task Assigment :" + taskId + " Holiday Date : " + id, CurrentUser.Account);
                                }
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.HolidayApprovedChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string sTaskId = TaskIdsOCS.HolidayApprovedChecker.VERIFY;
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.Holiday.INDEX:
                                if (action.Equals("A"))
                                {
                                    HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDataTempbyId(id);
                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_AddTemplate(id, sHoliday, "temp", "Reject");
                                    auditTrailDao.SecurityLog("Reject Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    HolidayModel sHoliday = MaintenanceAuditLogDao.GetHolidayDatabyId(id);
                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_DeleteTemplate(id, sHoliday, "temp", "Reject");
                                    auditTrailDao.SecurityLog("Rejcct Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    HolidayModel sBefore = MaintenanceAuditLogDao.GetHolidayDatabyId(id);
                                    HolidayModel sAfter = MaintenanceAuditLogDao.GetHolidayDataTempbyId(id);
                                    string ActionDetails = MaintenanceAuditLogDao.HolidayCalendarChecker_EditTemplate(id, sBefore, sAfter, "temp", "Reject");
                                    auditTrailDao.SecurityLog("Reject Holiday Calendar", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                holidayDao.DeleteHolidayinTemp(id, Convert.ToInt32(arrResult.Length));
                                if (arrResult.Length < 15)
                                {
                                    //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + " Holiday ID : " + id, CurrentUser.Account);
                                }
                                else
                                {
                                    //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + " Holiday Date : " + id, CurrentUser.Account);
                                }
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}