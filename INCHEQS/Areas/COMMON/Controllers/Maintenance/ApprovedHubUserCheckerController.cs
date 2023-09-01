//bank of Malaysia
//using INCHEQS.BNM.BankCode;
//using INCHEQS.BNM.BranchCode;
//using INCHEQS.BNM.ReturnCode;
//using INCHEQS.BNM.StateCode;

//bank of Philipine
//using INCHEQS.PCHC.BankCode;
//using INCHEQS.PCHC.ReturnCode;
using INCHEQS.PCHC.StateCode;

using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Models;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;

using INCHEQS.Security.User;
using INCHEQS.Security.Group;
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
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.Security.SystemProfile;

//COMMON
using INCHEQS.Areas.COMMON.Models.HubUser;
//using INCHEQS.Areas.OCS.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class ApprovedHubUserCheckerController : BaseController
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IUserDao userDao;
        private IGroupDao groupDao;

        private IHostReturnReasonDao hostReturnReasonDao;
        private IBankCodeDao bankCodeDao;
        //private IStateCodeDao stateCodeDao;
        private IBranchCodeDao branchCodeDao;
        private IReturnCodeDao returnCodeDao;
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
        private readonly IHubBranchDao hubBranchDao;
        private ISystemCutOffTimeDao systemCutOffTimeDao;
        private ISystemProfileDao systemProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public ApprovedHubUserCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IHubDao hubDao, IHubBranchDao hubBranchDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IBankCodeDao bankCodeDao, IHostReturnReasonDao hostReturnReasonDao, IBranchCodeDao branchCodeDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, IReturnCodeDao returnCodeDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, IGroupDao groupDao, ITaskDao taskDao, ApplicationDbContext dbContext, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.userDao = userDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.bankCodeDao = bankCodeDao;
            this.hostReturnReasonDao = hostReturnReasonDao;
            //this.stateCodeDao = stateCodeDao;
            this.branchCodeDao = branchCodeDao;
            this.internalBranchDao = internalBranchDao;
            this.verificationLimitDao = verificationLimitDao;
            this.pullOutReasonDao = pullOutReasonDao;
            this.returnCodeDao = returnCodeDao;
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedHubUserChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ApprovedHubUserChecker.INDEX));
            ViewBag.Menutitle = hubDao.GetMenuTitle(TaskIdsOCS.ApprovedHubUserChecker.INDEX);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedHubUserChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ApprovedHubUserChecker.INDEX, "View_ApprovedHubUserChecker", "actionstatus asc", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedHubUserChecker.DETAILS)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Details(FormCollection collection, string hubIdParam = "")
        {
            string taskid = TaskIdsOCS.ApprovedHubUserChecker.DETAILS;
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

            ViewBag.Hub = hubDao.CheckHubMasterTempCheckerByID(hubCode, "HubCode");
            ViewBag.SelectedUser = hubDao.ListSelectedUserInHubTempChecker(hubCode, CurrentUser.Account.BankCode);
            ViewBag.AvailableUser = hubDao.ListAvailableUserInHubChecker(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ApprovedHubUserChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            string taskid = TaskIdsOCS.ApprovedHubUserChecker.VERIFY;
            if (CurrentUser.HasTask(taskid))
            {
                CurrentUser.Account.TaskId = taskid;
            }
            try
            {

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
                            case TaskIdsOCS.HubUserProfile.INDEX:

                                if (action.Equals("A"))
                                {
                                    string afterUser2 = "";
                                    List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(id, CurrentUser.Account.BankCode);
                                    foreach (var afterUserlist2 in afterUserLists2)
                                    {
                                        afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                    }
                                    hubDao.MoveToHubMasterFromTemp(id, "Create");

                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_AddTemplate(afterUser2, "Approve", "Approve", id);
                                    auditTrailDao.SecurityLog("Add Hub", ActionDetails, taskId, CurrentUser.Account);

                                }
                                else if (action.Equals("D"))
                                {
                                    string beforeUser2 = "";
                                    List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist2 in beforeUserLists2)
                                    {
                                        beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                    }
                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_DeleteTemplate(beforeUser2, "Approve", "Approve", id);
                                    auditTrailDao.SecurityLog("Delete Hub", ActionDetails, taskid, CurrentUser.Account);
                                    hubDao.DeleteInHubUserMaster(id, "Both");
                                    hubBranchDao.DeleteAllBranchInHubTemp(id);
                                    hubBranchDao.DeleteAllBranchInHub(id, CurrentUser.Account.BankCode);
                                }
                                else if (action.Equals("U"))
                                {
                                    HubModel beforeHub = hubDao.CheckHubMasterByID(id, "HubCode");
                                   // auditTrailDao.Log("Update Hub- Before Update=> Hub Code: " + beforeHub.fldHubId + " Hub Desc : " + beforeHub.fldHubDesc, CurrentUser.Account);

                                    string beforeUser = "";
                                    List<UserModel> beforeUserLists = hubDao.ListSelectedUserInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist in beforeUserLists)
                                    {
                                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + "\n";
                                    }

                                    //auditTrailDao.Log("Update User In Hub , Before Update =>- Hub Code: " + id + " User : " + beforeUser, CurrentUser.Account);

                                    hubDao.MoveToHubMasterFromTemp(id, "Update");
                                    hubDao.UpdateHubUser(id);


                                    HubModel afterHub = hubDao.CheckHubMasterByID(id, "HubCode");
                                   // auditTrailDao.Log("Update Hub- After Update=> Hub Code: " + afterHub.fldHubId + " Hub Desc : " + afterHub.fldHubDesc, CurrentUser.Account);

                                    string afterUser2 = "";
                                    List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(id, CurrentUser.Account.BankCode);
                                    foreach (var afterUserlist2 in afterUserLists2)
                                    {
                                        afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                    }
                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_EditTemplate(beforeHub, afterHub, beforeUser, afterUser2, "Approve", "Approve", id);
                                    auditTrailDao.SecurityLog("Edit Hub", ActionDetails, taskid, CurrentUser.Account);
                                    //auditTrailDao.Log("Update User In Hub , After Update => - Hub Code: " + id + " User : " + afterUser, CurrentUser.Account);
                                }

                                hubDao.DeleteInHubMasterTemp(id);
                                hubDao.DeleteInHubUserTemp(id);

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

        [CustomAuthorize(TaskIds =  TaskIdsOCS.ApprovedHubUserChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            string taskid = TaskIdsOCS.ApprovedHubUserChecker.VERIFY;
            if (CurrentUser.HasTask(taskid))
            {
                CurrentUser.Account.TaskId = taskid;
            }
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
                            case TaskIdsOCS.HubUserProfile.INDEX:
                                if (action.Equals("A"))
                                {
                                    string afterUser2 = "";
                                    List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(id, CurrentUser.Account.BankCode);
                                    foreach (var afterUserlist2 in afterUserLists2)
                                    {
                                        afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                    }

                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_AddTemplate(afterUser2, "Reject", "Reject", id);
                                    auditTrailDao.SecurityLog("Add Hub", ActionDetails, taskid, CurrentUser.Account);

                                }
                                else if (action.Equals("D"))
                                {
                                    string beforeUser2 = "";
                                    List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist2 in beforeUserLists2)
                                    {
                                        beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                    }

                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_DeleteTemplate(beforeUser2, "Reject", "Reject", id);
                                    auditTrailDao.SecurityLog("Delete Hub", ActionDetails, taskid, CurrentUser.Account);

                                }
                                else if (action.Equals("U"))
                                {
                                    HubModel beforeHub = hubDao.CheckHubMasterByID(id, "HubCode");

                                    string beforeUser2 = "";
                                    List<UserModel> beforeUserLists2 = hubDao.ListSelectedUserInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist2 in beforeUserLists2)
                                    {
                                        beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                    }

                                    HubModel afterHub = SecurityAuditLogDao.CheckHubMasterByIDTemp_Security(id, "HubCode");

                                    string afterUser2 = "";
                                    List<UserModel> afterUserLists2 = hubDao.ListSelectedUserInHubTempChecker(id, CurrentUser.Account.BankCode);
                                    foreach (var afterUserlist2 in afterUserLists2)
                                    {
                                        afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                    }

                                    string ActionDetails = SecurityAuditLogDao.HubUserChecker_EditTemplate(beforeHub, afterHub, beforeUser2, afterUser2, "Reject", "Reject", id);
                                    auditTrailDao.SecurityLog("Edit Hub", ActionDetails, taskid, CurrentUser.Account);

                                }
                                hubDao.DeleteInHubMasterTemp(id);
                                hubDao.DeleteInHubUserTemp(id);
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