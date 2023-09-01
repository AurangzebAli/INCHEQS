using INCHEQS.Areas.OCS.Models.ClearingItem;
using INCHEQS.Common.Resources;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ClearingItems
{
    public class ClearingItemsController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private IClearingItemDao IClearingItemDao;

        public ClearingItemsController(IPageConfigDao pageConfigDao, IClearingItemDao IClearingItemDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.IClearingItemDao = IClearingItemDao;
        }


       [CustomAuthorize(TaskIds = TaskIdsOCS.Clearing.ITEMCLEARING)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.Clearing.ITEMCLEARING));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Clearing.ITEMCLEARING)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string strUserBranch = "";
            DataTable dt = new DataTable();
            dt = IClearingItemDao.GetHubBranches(CurrentUser.Account.UserId);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {

                    if (strUserBranch == "")
                    {
                        strUserBranch = "'" + item["fldbranchid"].ToString().Trim() + "',";
                    }
                    else
                    {
                        strUserBranch = strUserBranch + "'" +  item["fldbranchid"].ToString().Trim() + "',";
                    }
                }
                strUserBranch = strUserBranch.Remove(strUserBranch.Length -1 , 1);
            }
            else
            {
                strUserBranch = "''";
            }
            //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.Clearing.ITEMCLEARING, "View_GetItemsforClearing", "fldCapturingBranch", "fldCapturingBranch=@fldCapturingBranch", new[] {
            //        new SqlParameter("@fldCapturingBranch",strUserBranch)}),
            //collection);
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.Clearing.ITEMCLEARING, "View_GetItemsforClearing_bak", "fldCapturingBranch", "fldCapturingBranch IN (" + strUserBranch + ") ", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
                collection);
            return View();
        }
        public ActionResult AddToBatch(FormCollection collection)
        {
            try
            {
                if (collection.AllKeys.Contains("deleteBox"))
                {
                    if (!string.IsNullOrWhiteSpace(collection["deleteBox"].Trim()))
                    {
                        if (collection["deleteBox"].Contains(","))
                        {
                            string[] TotalValues = collection["deleteBox"].Split(',');
                            foreach (var item in TotalValues)
                            {
                                string[] ParamValues   = item.Split('|');
                                var fldCapturingBranch = ParamValues[0];
                                var fldCapturingDate   = ParamValues[1];
                                var fldBatchNumber     = ParamValues[2];
                                var fldscannerid       = ParamValues[3];
                                IClearingItemDao.UpdateClearingStatusandInsertClearingAgent(fldCapturingBranch, fldCapturingDate, fldscannerid, fldBatchNumber);
                                TempData["Success"] = "Batch is Submitted for Clearing.";
                            }
                        }
                        else
                        {
                            string[] ParamValues = collection["deleteBox"].Split('|');
                            var fldCapturingBranch = ParamValues[0];
                            var fldCapturingDate = ParamValues[1];
                            var fldBatchNumber = ParamValues[2];
                            var fldscannerid = ParamValues[3];
                            IClearingItemDao.UpdateClearingStatusandInsertClearingAgent(fldCapturingBranch, fldCapturingDate, fldscannerid, fldBatchNumber);
                            TempData["Notice"] = "Batch is Submitted for Clearing.";
                        }

                    }
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