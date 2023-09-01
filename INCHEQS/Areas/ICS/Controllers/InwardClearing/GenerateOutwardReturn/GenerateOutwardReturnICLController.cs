using INCHEQS.Areas.ICS.Models.OutwardReturnICL;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig.Services;



namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.GenerateOutwardReturn
{
    
        
        public class GenerateOutwardReturnICLController : OutwardReturnICLBaseController
        {
            protected override PageSqlConfig setupPageSqlConfig()
            {
                string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

                return new PageSqlConfig(taskId);
            }



        public GenerateOutwardReturnICLController(IPageConfigDao pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IOutwardReturnICLDao outwardReturnICLDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, outwardReturnICLDao) { }
        

            //[CustomAuthorize(TaskIds = TaskIdsICS.GenerateOutwardReturnICL.INDEX)]
            //[GenericFilter(AllowHttpGet = true)]
            //public ActionResult Index()
            //{
            //    ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateOutwardReturnICL.INDEX));
            //    return View();
            //}

            //[CustomAuthorize(TaskIds = TaskIdsICS.GenerateOutwardReturnICL.INDEX)]
            //[GenericFilter(AllowHttpGet = true)]
            //public virtual ActionResult SearchResultPage(FormCollection collection)
            //{

            //    string t = collection["Type"];

            //    if (t == null)
            //    {
            //        t = "Ready";
            //    }
            //    else
            //    {
            //        t = collection["Type"];
            //    }

            //    if (t.Equals("Ready"))
            //    {
            //        ViewBag.Type = t;
            //        ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateOutwardReturnICL.INDEX, "View_GetItemsforClearingICS", "", "", new[] {
            //        new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
            //      collection);
            //    }
            //    else if (t.Equals("Submitted"))
            //    {
            //        ViewBag.Type = t;
            //        ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateOutwardReturnICL.Cleared, "View_GetClearedItemsICS", "", "", new[] {
            //        new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
            //     collection);
            //    }
            //    return View();

            //}


        }
    
       
}