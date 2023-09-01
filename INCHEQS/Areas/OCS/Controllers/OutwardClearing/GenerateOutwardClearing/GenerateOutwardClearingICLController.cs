using INCHEQS.Areas.OCS.Models.ClearingItem;
using INCHEQS.Areas.OCS.Models.OutwardClearingICL;
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

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.GenerateOutwardClearing
{
    public class GenerateOutwardClearingICLController : BaseController
    {
        private IPageConfigDaoOCS pageConfigDao;
        private readonly IOutwardClearingICLDao OutwardClearingICLDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IClearingItemDao IClearingItemDao;

        public GenerateOutwardClearingICLController(IPageConfigDaoOCS pageConfigDao, IOutwardClearingICLDao OutwardClearingICLDao, IAuditTrailDao audilTrailDao, IClearingItemDao IClearingItemDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.OutwardClearingICLDao = OutwardClearingICLDao;
            this.auditTrailDao = audilTrailDao;
            this.IClearingItemDao = IClearingItemDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateOutwardClearingICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.GenerateOutwardClearingICL.INDEX));
            auditTrailDao.SecurityLog("Access Generate Outward Clearing ICL ", "", TaskIdsOCS.GenerateOutwardClearingICL.INDEX, CurrentUser.Account);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateOutwardClearingICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string strUserBranch = "";
            string t = collection["Type"];
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
                        strUserBranch = strUserBranch + "'" + item["fldbranchid"].ToString().Trim() + "',";
                    }
                }
                strUserBranch = strUserBranch.Remove(strUserBranch.Length - 1, 1);
            }
            else
            {
                strUserBranch = "''";
            }

            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (t.Equals("Ready"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateOutwardClearingICL.INDEX, "View_GetItemsforClearing", "fldCapturingBranch", "fldCapturingBranch IN (" + strUserBranch + ") ", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.GenerateOutwardClearingICL.Cleared, "View_GetClearedItems", "fldCapturingBranch", "fldCapturingBranch IN (" + strUserBranch + ") AND fldprocessdate = '"+ collection["capturingdate"] +"' ", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();

        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.GenerateOutwardClearingICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Generate(FormCollection collection)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();

            string t = collection["Type"];
            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }
            if (collection != null && collection["deleteBox"] != null)
            {
                if (t.Equals("Ready"))
                {
                    List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        //e.g. cb010010001si1cd2019-12-11bn16
                        //cb10290002si004cd2020-04-17bn0002ed1cr01ct
                        string capturebranch =  OutwardClearingICLDao.getBetween(arrResult, "cb", "si");
                        //capturebranch = capturebranch.Substring(1, capturebranch.Length-1);
                        string scannerid = OutwardClearingICLDao.getBetween(arrResult, "si", "cd");
                        string capturedate = OutwardClearingICLDao.getBetween(arrResult, "cd", "bn").Replace("-","");
                        string batchNumber = OutwardClearingICLDao.getBetween(arrResult, "bn", "ed");
                        string currency = OutwardClearingICLDao.getBetween(arrResult, "ed", "cr");
                        string CapturingType = OutwardClearingICLDao.getBetween(arrResult, "cr", "ct");
                        string hidUIC = capturedate + capturebranch + currency + CapturingType;
                        string processdate = OutwardClearingICLDao.getBetween(arrResult, "cd", "bn");
                        OutwardClearingICLDao.AddToBatch(CurrentUser.Account.UserId, hidUIC, processdate);
                       // auditTrailDao.Log("Batch Submitted for Clearing Item. Batch info: " + hidUIC, CurrentUser.Account);
                        auditTrailDao.SecurityLog("[Outward Clearing File] Batch Submitted for Clearing Item. Batch info: " + hidUIC , "", TaskIdsOCS.GenerateOutwardClearingICL.INDEX, CurrentUser.Account);
                    }
                    TempData["Notice"] = "Batch Successfully Submitted for ICL Generation.";
                    //auditTrailDao.Log("Add - Branch Clearing Item - ", CurrentUser.Account);
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["warning"] = "No Data was selected";
            }
            return RedirectToAction("Index");
        }
        public virtual ActionResult Download(FormCollection collection)
        {
            string filePath = collection["this_fldFilePath"].Trim();
            string fileName = collection["this_fldFileName"].Trim();
            string fullFileName = string.Format("{0}", Path.Combine(filePath, fileName));
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                //Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, "txt"));
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));
                //auditTrailDao.Log("Outward Clearing File - Downloaded ICL File : " + fullFileName, CurrentUser.Account);
                auditTrailDao.SecurityLog("[Outward Clearing File] Downloaded ICL File : " + fullFileName, "", TaskIdsOCS.GenerateOutwardClearingICL.INDEX, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }
        }
        public ActionResult ReadyforCenterClearing(FormCollection collection)
        {

            ViewBag.ReadyforCenterClearing = OutwardClearingICLDao.ReadyforCenterClearing(collection);
            auditTrailDao.SecurityLog("[Outward Clearing File] View Detail(s) for Outward Clearing Ready item(s) : ", "", TaskIdsOCS.GenerateOutwardClearingICL.INDEX, CurrentUser.Account);
            return PartialView("Modal/_ReadyforCenterClearingPopup");

        }
        public ActionResult CenterSubmittedItems(FormCollection collection)
        {

            ViewBag.CenterSubmittedItems = OutwardClearingICLDao.CenterSubmittedItems(collection);
            auditTrailDao.SecurityLog("[Outward Clearing File] View Detail(s) for Outward Clearing Generated item(s) : ", "", TaskIdsOCS.GenerateOutwardClearingICL.INDEX, CurrentUser.Account);
            return PartialView("Modal/_CenterSubmittedItemsPopup");

        }

    }
}