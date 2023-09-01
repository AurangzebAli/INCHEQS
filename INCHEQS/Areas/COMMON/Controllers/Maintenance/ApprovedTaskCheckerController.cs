//bank of Malaysia
//using INCHEQS.BNM.BankCode;
//using INCHEQS.BNM.BranchCode;
//using INCHEQS.BNM.ReturnCode;
//using INCHEQS.BNM.StateCode;

//bank of Philipine
//using INCHEQS.PCHC.BankCode;
//using INCHEQS.PCHC.BranchCode;
//using INCHEQS.PCHC.ReturnCode;
//using INCHEQS.PCHC.StateCode;

using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Models;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.InternalBranch.InternalBranch;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
//using INCHEQS.Areas.COMMON.Models.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;

using INCHEQS.Security.User;
//using INCHEQS.Security.Group;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using System.Data;

using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Areas.OCS.Models.TCCode;
using INCHEQS.Areas.OCS.Models.TransactionCode;
//using INCHEQS.Areas.OCS.Models.HubBranch;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance {
    public class ApprovedTaskCheckerController : BaseController {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IUserDao userDao;
        //private IGroupDao groupDao;
        private IGroupProfileDao groupDao;
        private IHostReturnReasonDao hostReturnReasonDao;
        //private IBankCodeDao bankCodeDao;
        //private IStateCodeDao stateCodeDao;
        //private IBranchCodeDao branchCodeDao;
        //private IReturnCodeDao returnCodeDao;
        private IInternalBranchDao internalBranchDao;
        private IVerificationLimitDao verificationLimitDao;
        private IPullOutReasonDao pullOutReasonDao;
        private readonly ApplicationDbContext dbContext;


        private IThresholdSettingDao thresholdSettingDao;

        private ITransactionTypeDao transactionTypeDao;
        private ITCCodeDao tcCodeDao;
        private ITransactionCodeDao transactionCodeDao;

        protected readonly ISearchPageService searchPageService;
        private ITaskDao taskDao;

        private IHubDao hubDao;
        private IHubBranchDao hubBranchDao;
        private ISystemCutOffTimeDao systemCutOffTimeDao;
        private ISystemProfileDao systemProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        //public ApprovedTaskCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IHubBranchDao hubBranchDao, IHubDao hubDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IBankCodeDao bankCodeDao, IHostReturnReasonDao hostReturnReasonDao, IStateCodeDao stateCodeDao, IBranchCodeDao branchCodeDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, IReturnCodeDao returnCodeDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, IGroupDao groupDao, ITaskDao taskDao,  ApplicationDbContext dbContext)

        public ApprovedTaskCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IHubBranchDao hubBranchDao, IHubDao hubDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IHostReturnReasonDao hostReturnReasonDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao,ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, IGroupProfileDao groupDao, ITaskDao taskDao, ApplicationDbContext dbContext, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.userDao = userDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            ////this.bankCodeDao = bankCodeDao;
            this.hostReturnReasonDao = hostReturnReasonDao;
            //this.stateCodeDao = stateCodeDao;
            //this.branchCodeDao = branchCodeDao;
            this.internalBranchDao = internalBranchDao;
            this.verificationLimitDao = verificationLimitDao;
            this.pullOutReasonDao = pullOutReasonDao;
            //this.returnCodeDao = returnCodeDao;
            this.transactionCodeDao = transactionCodeDao;
            this.transactionTypeDao = transactionTypeDao;
            this.tcCodeDao = tcCodeDao;
            this.thresholdSettingDao = thresholdSettingDao;
            this.groupDao = groupDao;
            this.taskDao = taskDao;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.systemCutOffTimeDao = systemCutOffTimeDao;
            this.systemProfileDao = systemProfileDao;
            this.dbContext = dbContext;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }
        
        [CustomAuthorize(TaskIds = TaskIds.ApprovedTaskChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ApprovedTaskChecker.INDEX));
            //ViewBag.Menutitle = GetMenuTitle("102230");
            ViewBag.PageTitle = groupDao.GetPageTitle(TaskIds.ApprovedTaskChecker.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedTaskChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ApprovedTaskChecker.INDEX, "View_ApprovedTaskChecker", "fldId", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedTaskChecker.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string groupIdParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string groupId = "";

            if (string.IsNullOrEmpty(groupIdParam))
            {
                groupId = filter["fldId"].Trim();
            }
            else
            {
                groupId = groupIdParam;
            }

            ViewBag.PageTitle = groupDao.GetPageTitle(TaskIds.ApprovedTaskChecker.EDIT);
            ViewBag.GroupChecker = groupDao.GetGroup(groupId, "Update");

            ViewBag.AvailableTaskChecker = taskDao.ListAvailableTaskInGroupTemp(groupId);
            ViewBag.SelectedTaskChecker = taskDao.ListSelectedTaskInGroupTemp(groupId);
            
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedTaskChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col) {
            try
            {

                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[5];
                string task = "";

                string staskid = TaskIds.ApprovedTaskChecker.VERIFY;
                int count = 0;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        //string action = arrResult.Substring(0, 1);
                        //string taskId = arrResult.Substring(1, 6);
                        //string id = arrResult.Remove(0, 7);

                        count++;
                        tmpArr = arrResult.Split(':');
                        string action = tmpArr[0].Trim().ToString();
                        string taskId = tmpArr[1].Trim().ToString();
                        string id = tmpArr[2].Trim().ToString();
                        if (tmpArr.Length > 3)
                        {
                            task = tmpArr[3].Trim().ToString();
                        }

                        //Act based on task id
                        switch (taskId)
                        {
                          
                            case TaskIds.Task.INDEX:
                                
                                if (action.Equals("S"))
                                {
                                    if ((taskDao.CheckGroupExist(id, task)))
                                    {
                                        taskDao.UpdateSelectedTaskIdApproval(CurrentUser.Account.UserId, id, task);
                                    }
                                    else
                                    {
                                        taskDao.InsertSelectedTaskIdApproval(CurrentUser.Account.UserId, id, task);
                                    }

                                    string beforeTask1 = "";
                                    List<TaskModel> beforeLists1 = taskDao.ListSelectedTaskInGroup(id);
                                    foreach (var beforelist1 in beforeLists1)
                                    {
                                        beforeTask1 = beforeTask1 + beforelist1.fldTaskDesc + "<br/>";
                                    }
                                    string afterTask1 = "";
                                    List<TaskModel> afterLists1 = taskDao.ListSelectedTaskInGroupTemp(id);
                                    foreach (var afterlist1 in afterLists1)
                                    {
                                        afterTask1 = afterTask1 + afterlist1.fldTaskDesc + "<br/>";
                                    }

                                    if (count == 1)
                                    {
                                        string ActionDetails = SecurityAuditLogDao.TaskAssignmentChecker_EditTemplate(beforeTask1, afterTask1, "Approve", "Approve", id);
                                        auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);
                                    }

                                }
                                else if (action.Equals("U"))
                                {
                                    string beforeTask2 = "";
                                    List<TaskModel> beforeLists2 = taskDao.ListSelectedTaskInGroup(id);
                                    foreach (var beforelist2 in beforeLists2)
                                    {
                                        beforeTask2 = beforeTask2 + beforelist2.fldTaskDesc + "<br/>";
                                    }

                                    taskDao.DeleteTaskNotSelectedApproval(id,task);

                                    string afterTask2 = "";
                                    List<TaskModel> afterLists2 = taskDao.ListSelectedTaskInGroupTemp(id);
                                    foreach (var afterlist2 in afterLists2)
                                    {
                                        afterTask2 = afterTask2 + afterlist2.fldTaskDesc + "<br/>";
                                    }

                                    if (count == 1)
                                    {
                                        string ActionDetails = SecurityAuditLogDao.TaskAssignmentChecker_EditTemplate(beforeTask2, afterTask2, "Approve", "Approve", id);
                                        auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);
                                    }
                                }

                                //auditTrailDao.Log("Approve - Update Task Assigment : " + id, CurrentUser.Account);
                                break;
                        }
                        taskDao.DeleteTaskTempAfterApproval(id,task);
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

        [CustomAuthorize(TaskIds = TaskIds.ApprovedTaskChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            int count = 0;
            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[5];
                string task = "";
                string staskid = TaskIds.ApprovedTaskChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        count++;
                        //string action = arrResult.Substring(0, 1);
                        //string taskId = arrResult.Substring(1, 6);
                        //string id = arrResult.Remove(0, 7);
                        tmpArr = arrResult.Split(':');
                        string action = tmpArr[0].Trim().ToString();
                        string taskId = tmpArr[1].Trim().ToString();
                        string id = tmpArr[2].Trim().ToString();
                        if (tmpArr.Length > 3)
                        {
                            task = tmpArr[3].Trim().ToString();
                        }
                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIds.Task.INDEX:
                                string beforeTask2 = "";
                                List<TaskModel> beforeLists = taskDao.ListSelectedTaskInGroup(id);
                                foreach (var beforelist2 in beforeLists)
                                {
                                    beforeTask2 = beforeTask2 + beforelist2.fldTaskDesc + "<br/>";
                                }
                                string afterTask1 = "";
                                List<TaskModel> afterLists1 = taskDao.ListSelectedTaskInGroupTemp(id);
                                foreach (var afterlist in afterLists1)
                                {
                                    afterTask1 = afterTask1 + afterlist.fldTaskDesc + "<br/>";
                                }
                                taskDao.UpdateRejectTaskIdApproval(CurrentUser.Account.UserId, id);
                                //auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
                                if (count == 1)
                                {
                                    string ActionDetails = SecurityAuditLogDao.TaskAssignmentChecker_EditTemplate(beforeTask2, afterTask1, "Reject", "Reject", id);
                                    auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);
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