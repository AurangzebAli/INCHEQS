using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

//COMMON
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.Branch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class ApprovedHubBranchCheckerController : BaseController
    {

        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private IHubDao hubDao;
        private readonly IHubBranchDao hubBranchDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;


        public ApprovedHubBranchCheckerController(IHubDao hubDao, IHubBranchDao hubBranchDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISecurityAuditLogDao SecurityAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.ApprovedHubBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsSDS.ApprovedHubBranchChecker.INDEX));
            ViewBag.Menutitle = hubDao.GetMenuTitle("107030");

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.ApprovedHubBranchChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsSDS.ApprovedHubBranchChecker.INDEX, "View_ApprovedHubBranchChecker", "fldId", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.ApprovedHubBranchChecker.DETAILS)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Details(FormCollection collection, string hubIdParam = "")
        {
            string taskid = TaskIdsSDS.ApprovedHubBranchChecker.DETAILS;
            if (CurrentUser.HasTask(taskid))
            {
                CurrentUser.Account.TaskId = taskid;
            }
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string hubCode = "";
            if (string.IsNullOrEmpty(hubIdParam))
            {
                hubCode = filter["fldId"].Trim();
            }
            else
            {
                hubCode = hubIdParam;
            }

            ViewBag.Hub = hubDao.CheckHubMasterByID(hubCode, "HubCode");
            ViewBag.SelectedBranch = hubBranchDao.ListSelectedBranchInHubChecker(hubCode, CurrentUser.Account.BankCode);
            ViewBag.AvailableBranch = hubBranchDao.ListAvailableBranchInHubChecker(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsSDS.ApprovedHubBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string sTaskID = TaskIdsSDS.ApprovedHubBranchChecker.VERIFY;
            int count = 0;
            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[5];
                string task = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        count++;
                        //tmpArr = arrResult.Split(':');
                        //string action = tmpArr[0].Trim().ToString();
                        //string taskId = tmpArr[1].Trim().ToString();
                        //string id = tmpArr[2].Trim().ToString();
                        //if (tmpArr.Length > 3)
                        //{
                        //    task = tmpArr[3].Trim().ToString();
                        //}

                        //Act based on task id
                        switch (taskId)
                        {


                            case TaskIdsSDS.HubBranchProfile.INDEX:


                                if (action.Equals("A"))
                                {
                                    string beforeBranch1 = "";
                                    List<BranchModel> beforeBranchLists1 = hubBranchDao.ListSelectedBranchInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeBranchlist in beforeBranchLists1)
                                    {
                                        beforeBranch1 = beforeBranch1 + beforeBranchlist.fldBranchCode + " - " + beforeBranchlist.fldBranchDesc + "\n";
                                    }
                                    //auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub Code: " + id + " Branch : " + beforeBranch, CurrentUser.Account);


                                    //if ((hubBranchDao.CheckBranchExistInHub(id, task, CurrentUser.Account.BankCode)))
                                    //{
                                    //    hubBranchDao.UpdateSelectedBranch(id, task, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                                    //}
                                    //else
                                    //{
                                    //    hubBranchDao.InsertBranchInHub(id, task, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                                    //}

                                    //hubBranchDao.UpdateHubMaster(col, CurrentUser.Account.UserId);


                                    hubBranchDao.MoveToHubMasterFromTemp(id, "Update");
                                    hubBranchDao.UpdateHubBranch(id);
                                    string afterBranch1 = "";
                                    List<BranchModel> afterHubLists1 = SecurityAuditLogDao.ListSelectedBranchInHubTemp_Security(id, CurrentUser.Account.BankCode);
                                    foreach (var afterBranchlist in afterHubLists1)
                                    {
                                        afterBranch1 = afterBranch1 + afterBranchlist.fldBranchCode + " - " + afterBranchlist.fldBranchDesc + "\n";
                                    }
                                    if (count == 1)
                                    {
                                        string ActionDetails = SecurityAuditLogDao.HubBranchChecker_EditTemplate(beforeBranch1, afterBranch1, "Approve", "Approve", id);
                                        auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);
                                    }
                                    
                                    //auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + id + " Branch : " + afterBranch, CurrentUser.Account);
                                }
                                if (action.Equals("U"))
                                {
                                    string beforeBranch1 = "";
                                    List<BranchModel> beforeBranchLists1 = hubBranchDao.ListSelectedBranchInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeBranchlist in beforeBranchLists1)
                                    {
                                        beforeBranch1 = beforeBranch1 + beforeBranchlist.fldBranchCode + " - " + beforeBranchlist.fldBranchDesc + "\n";
                                    }
                                    //auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub Code: " + id + " Branch : " + beforeBranch, CurrentUser.Account);

                                    //hubDao.MoveToHubMasterFromTemp(id, "Update");
                                    hubBranchDao.UpdateHubBranch(id);
                                    string afterBranch1 = "";
                                    List<BranchModel> afterHubLists1 = SecurityAuditLogDao.ListSelectedBranchInHubTemp_Security(id, CurrentUser.Account.BankCode);
                                    foreach (var afterBranchlist in afterHubLists1)
                                    {
                                        afterBranch1 = afterBranch1 + afterBranchlist.fldBranchCode + " - " + afterBranchlist.fldBranchDesc + "\n";
                                    }
                                    // auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + id + " Branch : " + afterBranch, CurrentUser.Account);

                                    if (count == 1)
                                    {
                                        string ActionDetails = SecurityAuditLogDao.HubBranchChecker_EditTemplate(beforeBranch1, afterBranch1, "Approve", "Approve", id);
                                        auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);
                                    }
                                    //auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + id + " Branch : " + afterBranch, CurrentUser.Account);
                                }
                                if (action.Equals("D"))
                                {
                                    string beforeBranch1 = "";
                                    List<BranchModel> beforeBranchLists1 = hubBranchDao.ListSelectedBranchInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeBranchlist in beforeBranchLists1)
                                    {
                                        beforeBranch1 = beforeBranch1 + beforeBranchlist.fldBranchCode + " - " + beforeBranchlist.fldBranchDesc + "\n";
                                    }
                                    //auditTrailDao.Log("Update Branch In Hub, Before Update =>- Hub Code: " + id + " Branch : " + beforeBranch, CurrentUser.Account);

                                    //hubDao.MoveToHubMasterFromTemp(id, "Update");
                                    hubBranchDao.DeleteBranchNotSelectedApproval(id, task);
                                    string afterBranch1 = "";
                                    List<BranchModel> afterHubLists1 = SecurityAuditLogDao.ListSelectedBranchInHubTemp_Security(id, CurrentUser.Account.BankCode);
                                    foreach (var afterBranchlist in afterHubLists1)
                                    {
                                        afterBranch1 = afterBranch1 + afterBranchlist.fldBranchCode + " - " + afterBranchlist.fldBranchDesc + "\n";
                                    }
                                    //auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + id + " Branch : " + afterBranch, CurrentUser.Account);

                                    if (count == 1)
                                    {
                                        string ActionDetails = SecurityAuditLogDao.HubBranchChecker_EditTemplate(beforeBranch1, afterBranch1, "Approve", "Approve", id);
                                        auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);
                                    }
                                    // auditTrailDao.Log("Update Branch In Hub , After Update => - Hub Code: " + id + " Branch : " + afterBranch, CurrentUser.Account);
                                }
                                //hubDao.DeleteInHubMasterTemp(id);

                                break;


                        }

                        hubBranchDao.DeleteAllBranchInHubTempById(id, task);
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

        [CustomAuthorize(TaskIds = TaskIdsSDS.ApprovedHubBranchChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string sTaskID = TaskIdsSDS.ApprovedHubBranchChecker.VERIFY;
            int count = 0;

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[5];
                string task = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        count++;
                        //tmpArr = arrResult.Split(':');
                        //string action = tmpArr[0].Trim().ToString();
                        //string taskId = tmpArr[1].Trim().ToString();
                        //string id = tmpArr[2].Trim().ToString();
                        //if (tmpArr.Length > 3)
                        //{
                        //    task = tmpArr[3].Trim().ToString();
                        //}

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIdsSDS.HubBranchProfile.INDEX:
                                string beforeBranch1 = "";
                                List<BranchModel> beforeBranchLists1 = hubBranchDao.ListSelectedBranchInHub(id, CurrentUser.Account.BankCode);
                                foreach (var beforeBranchlist in beforeBranchLists1)
                                {
                                    beforeBranch1 = beforeBranch1 + beforeBranchlist.fldBranchCode + " - " + beforeBranchlist.fldBranchDesc + "\n";
                                }
                                string afterBranch1 = "";
                                List<BranchModel> afterHubLists1 = SecurityAuditLogDao.ListSelectedBranchInHubTemp_Security(id, CurrentUser.Account.BankCode);
                                foreach (var afterBranchlist in afterHubLists1)
                                {
                                    afterBranch1 = afterBranch1 + afterBranchlist.fldBranchCode + " - " + afterBranchlist.fldBranchDesc + "\n";
                                }
                                //hubDao.DeleteInHubMasterTemp(id);
                                hubBranchDao.DeleteAllBranchInHubTemp(id);
                                if (count == 1)
                                {
                                    string ActionDetails = SecurityAuditLogDao.HubBranchChecker_EditTemplate(beforeBranch1, afterBranch1, "Reject", "Reject", col["row_fldId"].Trim().ToString());
                                    auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);
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