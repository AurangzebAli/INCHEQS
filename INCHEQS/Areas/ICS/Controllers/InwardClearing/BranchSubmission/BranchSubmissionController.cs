using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Areas.ICS.Models.BranchSubmission;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.BranchSubmission
{
    public class BranchSubmissionController : BaseController
    {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IBranchSubmissionDao BranchSubmissionDao;

        

        public BranchSubmissionController(IPageConfigDao pageConfigDao, IBranchSubmissionDao BranchSubmissionDao, IAuditTrailDao auditTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.BranchSubmissionDao = BranchSubmissionDao;
            this.auditTrailDao = auditTrailDao;
        }

        protected string initializeQueueTaskId()
        {
            return RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
        }
        // GET: ICS/BranchActivation
       
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.BranchSubmission.INDEX));
            //if (taskid == "308220")
            //{
            //    ViewBag.LoadDate = BranchSubmissionDao.GetClearDate();
            //    return View("Branch_BranchSubmission/SearchResultPage");
            //}

            return View();
        }

        public ActionResult SearchResultPage(FormCollection collection)
        {
            //if (taskid == "308220")
            //{
            //    return View("Branch_BranchSubmission/SearchResultPage");
            //}
            //else
            //{
            //    ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(taskid, "View_BranchSubmission", "fldBranchCode"), collection);
            //    ViewBag.CompleteCount = BranchSubmissionDao.GetCompleteCount();
            //    ViewBag.IncompleteCount = BranchSubmissionDao.GetIncompleteCount();
            //    ViewBag.TotalCount = BranchSubmissionDao.GetTotalCount();
            //}

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.BranchSubmission.INDEX, "View_BranchSubmission", "fldStatus,fldBranchCode"), collection);
            ViewBag.CompleteCount = BranchSubmissionDao.GetCompleteCount();
            ViewBag.IncompleteCount = BranchSubmissionDao.GetIncompleteCount();
            ViewBag.TotalCount = BranchSubmissionDao.GetTotalCount();

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.BranchSubmission.INDEX)]
        [HttpPost]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                if ((col["deleteBox"]) != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();
                    int falsecounter = 0;
                    int truecounter = 0;

                    for (int i = 0; i < arrResults.Count; i++)
                    {
                        if (BranchSubmissionDao.ValidateStatus(arrResults[i]).Equals(false))
                        {
                            falsecounter++;
                        }
                        else
                        {
                            truecounter++;
                            BranchSubmissionDao.updateStatus(arrResults[i]);
                        }
                    }
                    if (truecounter == 0)
                    {
                        TempData["Notice"] = "Nothing has changed";
                    }
                    else
                    {
                        TempData["Notice"] = truecounter + " branch(es) updated to INCOMPLETE successfully";
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

        [CustomAuthorize(TaskIds = TaskIdsICS.BranchSubmission.INDEX)]
        [HttpPost]
        public ActionResult UpdateAll(FormCollection col)
        {
            try
            {
                BranchSubmissionDao.UpdateAll();
                TempData["Notice"] = "Update Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Confirm(FormCollection collection)
        {
            string sMessage = "";
            string sSubmissionType = "B";
            string sRegion = "KL";
            string sClearDate = collection["ClearingDate"];
            String sClearDateY = (DateTime.ParseExact(collection["ClearingDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
            String sClearDateM = (DateTime.ParseExact(collection["ClearingDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("MM-dd-yyyy"));
            bool startVerifiction = false;
            bool cutOffTimeActiveted = false;
            bool ICCSComplete = false;
            bool PospayComplete = false;
            bool isComplete = false;
            string auditMessage;
            string AddSQL;
            ViewBag.getBankInfo = BranchSubmissionDao.getBankInfo(CurrentUser.Account.UserId);
            if (BranchSubmissionDao.CheckOfficer())
            {
                sMessage = "Officers cannot perform Branch Submission.";
                auditTrailDao.Log(sMessage, CurrentUser.Account);
                ViewBag.MsgType = "Error";
                ViewBag.displayMsg = sMessage;
                return View("Branch_BranchSubmission/ConfirmPage");
            }
            if (BranchSubmissionDao.CheckBranchConfirm(ViewBag.getBankInfo.gBranchCode, ViewBag.getBankInfo.gBranchCode2, ViewBag.getBankInfo.gBranchCode3, sRegion, sSubmissionType))
            {
                sMessage = "Branch Submission already performed.";//(Region - " + sRegion + ",Submission Type - " + sSubmissionType + ")
                auditTrailDao.Log(sMessage, CurrentUser.Account);
                ViewBag.MsgType = "Error";
                ViewBag.displayMsg = sMessage;
                return View("Branch_BranchSubmission/ConfirmPage");
            }
            ViewBag.getCollapseBranch = BranchSubmissionDao.getCollapseBranch(ViewBag.getBankInfo.gBranchCode, ViewBag.getBankInfo.gBranchCode2, ViewBag.getBankInfo.gBranchCode3);
            if (BranchSubmissionDao.CheckPreRegionRight(sClearDateY)) { startVerifiction = true; }
            if (BranchSubmissionDao.CheckCutOffPeriod()) { cutOffTimeActiveted = true; }
            if (startVerifiction is true)
            {
                if (cutOffTimeActiveted is true)
                {
                    AddSQL = " and DateDiff(d, pi.fldClearDate ,'" + sClearDateM + "') = 0 and ci.fldChequeType in (11,14,21)";
                    string myBranch = " WHERE ci.fldIssueStateCode + ci.fldIssueBranchCode in ( " + ViewBag.getBankInfo.gBranchCode + ", " + ViewBag.getBankInfo.gBranchCode3 + ")";
                    ICCSComplete = BranchSubmissionDao.CheckPendingData(myBranch, AddSQL);
                    if (ICCSComplete.Equals(false))
                    {
                        sMessage = "Incomplete pending data record(s) (status is pending) was found";
                        auditTrailDao.Log(sMessage, CurrentUser.Account);
                    }
                    AddSQL = " and DateDiff(d, pi.fldClearDate ,'" + sClearDateY + "') = 0 and ci.fldChequeType in (11,14,21)";
                    PospayComplete = BranchSubmissionDao.CheckPospayData(myBranch, AddSQL);
                    if (PospayComplete.Equals(false))
                    {
                        sMessage = sMessage + "Incomplete pending Pospay data record(s) (Status is pending) was found";
                        auditTrailDao.Log(sMessage, CurrentUser.Account);
                    }
                    if (PospayComplete is true & ICCSComplete is true)
                    {
                        isComplete = true;
                        auditMessage = "ICCSComplete : True | PospayComplete : True ";
                        auditTrailDao.Log(auditMessage, CurrentUser.Account);
                    }
                    else
                    {
                        isComplete = false;
                        auditMessage = "Incomplete data found. Confirmation is not done from branch " + ViewBag.getBankInfo.gBranchCode + " , " + ViewBag.getBankInfo.gBranchCode3;
                        auditTrailDao.Log(auditMessage, CurrentUser.Account);
                        ViewBag.displayMsg = auditMessage;
                        ViewBag.MsgType = "Error";
                    }

                    if (isComplete)
                    {
                        if (BranchSubmissionDao.StartCopy(myBranch, AddSQL, ViewBag.getBankInfo.gBranchCode, ViewBag.getBankInfo.gBranchCode3) is true)
                        {
                            sMessage = "Submission from branch " + ViewBag.getBankInfo.gBranchCode + " , " + ViewBag.getBankInfo.gBranchCode3 + " confirmed successfully.";
                            auditTrailDao.Log(sMessage, CurrentUser.Account);
                            ViewBag.displayMsg = "Branch Submission has been performed successfully.";
                            ViewBag.MsgType = "Success";
                        }
                        else
                        {
                            sMessage = "Submission is not done successfully, please try again later";
                            ViewBag.displayMsg = sMessage;
                            ViewBag.MsgType = "Error";
                            auditTrailDao.Log(sMessage, CurrentUser.Account);
                        }
                    }
                    /*else
                    {
                        ViewBag.displayMsg = sMessage;
                        ViewBag.MsgType = "Error";
                        auditTrailDao.Log(sMessage, CurrentUser.Account);
                    }*/
                }
                else
                {
                    auditMessage = "Please refer to the submission time or contact CPC";
                    auditTrailDao.Log(auditMessage, CurrentUser.Account);
                    ViewBag.displayMsg = auditMessage;
                    ViewBag.MsgType = "Error";
                }
            }
            else
            {
                auditMessage = sRegion + "Region is not activate, Submission is not allowed. Please contact CPC";
                auditTrailDao.Log(auditMessage, CurrentUser.Account);
                ViewBag.displayMsg = auditMessage;
                ViewBag.MsgType = "Error";
            }
            return View("Branch_BranchSubmission/ConfirmPage");
        }
    }
}