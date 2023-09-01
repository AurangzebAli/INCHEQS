using INCHEQS.Areas.OCS.Models.BranchSubmission;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.BranchSubmissionNew
{
    public class BranchSubmissionNewController : Controller
    {
        private IPageConfigDaoOCS pageConfigDao;
        private readonly IBranchSubmissionDao BranchSubmissionDao;
        private readonly IAuditTrailDao auditTrailDao;

        public BranchSubmissionNewController(IPageConfigDaoOCS pageConfigDao, IBranchSubmissionDao BranchSubmissionDao, IAuditTrailDao audilTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.BranchSubmissionDao = BranchSubmissionDao;
            this.auditTrailDao = audilTrailDao;
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchClearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BranchClearing.INDEX));
            auditTrailDao.SecurityLog("Access Branch Clearing Item", "", TaskIdsOCS.BranchClearing.INDEX, CurrentUser.Account);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchClearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string t = collection["Type"];
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
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BranchClearing.INDEX, "View_BranchItemReadyForSubmit", "", "substring(fldCapturingBranch, 2, 3)= @fldUserBankCode AND fldcapturingbranch in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId = @fldUserID)", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId),
                    new SqlParameter("@fldUserBankCode", CurrentUser.Account.BankCode)}),

                collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BranchClearing.INDEX, "View_BranchItemSubmitted", "", "substring(fldCapturingBranch, 2, 3)= @fldUserBankCode AND fldcapturingbranch in (select distinct fldBranchId from tblHubBranch inner join tblHubUser on tblHubBranch.fldHubCode = tblHubUser.fldHubCode where tblHubUser.fldUserId = @fldUserID)", new[] {
                  new SqlParameter("@fldUserID", CurrentUser.Account.UserId),
                    new SqlParameter("@fldUserBankCode", CurrentUser.Account.BankCode)}),
                collection);

            }
            return View();

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchClearing.GENERATE)]
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
                        string capturebranch = BranchSubmissionDao.getBetween(arrResult, "cb", "si");
                        string scannerid = BranchSubmissionDao.getBetween(arrResult, "si", "cd");
                        string capturedate = BranchSubmissionDao.getBetween(arrResult, "cd", "bn");
                        string id = BranchSubmissionDao.getBetween(arrResult, "bn", "ed");
                        BranchSubmissionDao.UpdateBranchItem(collection, CurrentUser.Account, capturebranch, scannerid, id, capturedate);
                        auditTrailDao.SecurityLog("[Branch Clearing Item] : Submit Item(s) for Capturing Date : " + capturedate + " and Branch : " + capturebranch + " and Batch Number :" + id, "", TaskIdsOCS.BranchClearing.INDEX, CurrentUser.Account);
                        TempData["Notice"] = "Processing completed successfully";
                    }
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
        public ActionResult ReadyforBranchSubmission(FormCollection collection)
        {

            ViewBag.ReadyforBranchSubmission = BranchSubmissionDao.ReadyforBranchSubmission(collection);
            auditTrailDao.SecurityLog("[Branch Clearing Item] : View Ready Detail(s) ", "", TaskIdsOCS.BranchClearing.INDEX, CurrentUser.Account);
            return PartialView("Modal/_ReadyforBranchSubmissionPopup");

        }
        public ActionResult BranchSubmittedItems(FormCollection collection)
        {

            ViewBag.BranchSubmittedItems = BranchSubmissionDao.BranchSubmittedItems(collection);
            auditTrailDao.SecurityLog("[Branch Clearing Item] : View Submitted Detail(s) ", "", TaskIdsOCS.BranchClearing.INDEX, CurrentUser.Account);
            return PartialView("Modal/_BranchSubmittedItemsPopup");

        }
    }
}