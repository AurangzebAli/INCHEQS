using INCHEQS.Calendar.Holiday;
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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class HolidayController : BaseController
    {
        private iHolidayDao holidayDao;
        protected readonly ISearchPageService searchPageService;
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
 
        public HolidayController(IAuditTrailDao auditTrailDao, iHolidayDao holidayDao, ISearchPageService searchPageService, IPageConfigDao pageConfigDao)
        {
            this.auditTrailDao = auditTrailDao;
            this.holidayDao = holidayDao;
            this.searchPageService = searchPageService;
            this.pageConfigDao = pageConfigDao;
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
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Holiday.INDEX, "view_Holiday", "fldDesc,fldRecurring", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
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
            try
            {
                List<string> errorMessages = holidayDao.ValidateHoliday(col, "Create");
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    holidayDao.InsertHoliday(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                    return RedirectToAction("Index");
                }
                return RedirectToAction("Create");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Holiday.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            try
            {
                ViewBag.holiday = holidayDao.GetHolidayData(filter["fldHolidayId"]);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        //[CustomAuthorize(TaskIds = TaskIds.Holiday.EDIT)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Edit(FormCollection col, string holidayIdParam = "")
        //{
        //    Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
        //    //string holidayId = "";
        //    //if (string.IsNullOrEmpty(holidayIdParam))
        //    //{
        //    //    holidayId = filter["holidayId"].Trim();
        //    //}
        //    //else
        //    //{
        //    //    holidayId = holidayIdParam;
        //    //}

        //    9.CmsAccount = holidayDao.GetHolidayData(filter["fldHolidayId"]);
        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.Holiday.EDIT)]
        //public ActionResult Update(FormCollection col)
        //{ //return View();
        //    //more codes
        //    holidayDao.UpdateHolidayTable(col);
        //    return RedirectToAction("Index");
        //}
        [CustomAuthorize(TaskIds = TaskIds.Holiday.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            HolidayModel before = holidayDao.GetHolidayData(col["HolidayId"]);
            auditTrailDao.Log("Holiday Calender Created - Holiday Date : " + before.Date + ", Recurring : " + before.recurring + ", Recurring Type : " + before.recurrType + ", Description : " + before.Desc, CurrentUser.Account);
       
            holidayDao.UpdateHolidayTable(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
            TempData["Notice"] = Locale.SuccessfullyUpdated;

            HolidayModel after = holidayDao.GetHolidayData(col["HolidayId"]);
            auditTrailDao.Log("Holiday Calender Created - Holiday Date : " + after.Date + ", Recurring : " + after.recurring + ", Recurring Type : " + after.recurrType + ", Description : " + after.Desc, CurrentUser.Account);
            return RedirectToAction("Index");
            //return RedirectToAction("Edit", new { AccountNoParam = col["holidayId"] });
        }

        //[CustomAuthorize(TaskIds = TaskIds.Holiday.DELETE)]
        //[HttpPost]
        //public ActionResult Delete(FormCollection col)
        //{
        //    return View();
        //}
        [CustomAuthorize(TaskIds = TaskIds.Holiday.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col)
        {

            if (col != null & col["deleteBox"] != null)
            {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {

                    holidayDao.DeleteFromHolidayTable(arrResult);
                }
                TempData["Notice"] = Resources.Locale.SuccessfullyDeleted;
                auditTrailDao.Log("Holiday Calender Deleted - Holiday Date : " + col["Date"] + ", Recurring : " + col["recurring"] + ", Recurring Type : " + col["recurrType"] + ", Description : " + col["Desc"] , CurrentUser.Account);
                //auditTrailDao.Log("CMS Account Info Created - Holiday Date : " + after.Date + ", Recurring : " + after.recurring + ", Recurring Type : " + after.recurrType + ", Description : " + after.Desc, CurrentUser.Account);
            }
            else
            {
                TempData["Warning"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("SearchResultPage");
        }
    }
}