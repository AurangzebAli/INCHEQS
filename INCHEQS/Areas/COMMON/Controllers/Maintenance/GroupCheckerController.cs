
using INCHEQS.Areas.COMMON.Models.StateCode;

using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Models;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;
using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Areas.OCS.Models.TCCode;
using INCHEQS.Areas.OCS.Models.TransactionCode;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.DataAccessLayer;


namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    
    public class GroupCheckerController : BaseController
    {

        private readonly IGroupProfileDao groupProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IUserDao userDao;
        private IHostReturnReasonDao hostReturnReasonDao;
        private IStateCodeDao stateCodeDao;
        private IBranchCodeDao branchCodeDao;
        private IReturnCodeDao returnCodeDao;
        private IInternalBranchDao internalBranchDao;
        private IVerificationLimitDao verificationLimitDao;
        private IPullOutReasonDao pullOutReasonDao;
        private IThresholdSettingDao thresholdSettingDao;
        private ITransactionTypeDao transactionTypeDao;
        private ITCCodeDao tcCodeDao;
        private ITransactionCodeDao transactionCodeDao;

        protected readonly ISearchPageService searchPageService;
        private ITaskDao taskDao;
        private ISystemCutOffTimeDao systemCutOffTimeDao;
        private ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ApplicationDbContext dbContext;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        public GroupCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService,  IHostReturnReasonDao hostReturnReasonDao, IStateCodeDao stateCodeDao, IBranchCodeDao branchCodeDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, IReturnCodeDao returnCodeDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, ITaskDao taskDao, ApplicationDbContext dbContext, IGroupProfileDao groupProfileDao, ISecurityAuditLogDao SecurityAuditLogDao, ISecurityProfileDao securityProfileDao)
        {
            this.userDao = userDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.hostReturnReasonDao = hostReturnReasonDao;
            this.stateCodeDao = stateCodeDao;
            this.branchCodeDao = branchCodeDao;
            this.internalBranchDao = internalBranchDao;
            this.verificationLimitDao = verificationLimitDao;
            this.pullOutReasonDao = pullOutReasonDao;
            this.returnCodeDao = returnCodeDao;
            this.transactionCodeDao = transactionCodeDao;
            this.transactionTypeDao = transactionTypeDao;
            this.tcCodeDao = tcCodeDao;
            this.thresholdSettingDao = thresholdSettingDao;
            this.groupProfileDao = groupProfileDao;
            this.taskDao = taskDao;
            this.systemCutOffTimeDao = systemCutOffTimeDao;
            this.systemProfileDao = systemProfileDao;
            this.dbContext = dbContext;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
            this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.GroupChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.GroupChecker.INDEX));
            ViewBag.PageTitle = groupProfileDao.GetPageTitle(TaskIds.GroupChecker.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GroupChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.GroupChecker.INDEX, "View_GroupChecker", "actionstatus asc, fldId asc", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GroupChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string groupIdParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string groupId = "";

            if (string.IsNullOrEmpty(groupIdParam))
            {
                groupId = filter["fldId"].Trim();
            }
            else
            {
                groupId = groupIdParam;
            }

            ViewBag.PageTitle = groupProfileDao.GetPageTitle(TaskIds.GroupChecker.INDEX);

            ViewBag.GroupChecker = groupProfileDao.GetGroupChecker(groupId, "Update");
            ViewBag.SelectedUserChecker = groupProfileDao.ListSelectedUserInGroupChecker(groupId, CurrentUser.Account.BankCode);
            ViewBag.AvailableUserChecker = groupProfileDao.ListAvailableUserInGroupChecker(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.GroupChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            int count = 0;
            CurrentUser.Account.Status = "Y";
            CurrentUser.Account.TaskId = TaskIds.GroupChecker.INDEX;
            string sTaskId = TaskIds.GroupChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];
                string astatus = "";
                string userid = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        count++;
                        tmpArr = arrResult.Split(':');
                        string actTask = tmpArr[0].Trim().ToString();
                        string action = actTask.Substring(0, 1);
                        string taskId = actTask.Substring(1, 6);  //tmpArr[1].Trim().ToString();
                        string id = actTask.Remove(0, 7); //tmpArr[1].Trim().ToString();
                        if (tmpArr.Length > 1)
                        {
                            astatus = tmpArr[1].Trim().ToString();
                            userid = tmpArr[2].Trim().ToString();
                        }

                        if (action.Equals("A"))
                        {
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroupChecker(id, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + "\n";
                            }
                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserIDTemp(id);
                            auditTrailDao.Log("Group Code: " + after.fldGroupId + " Group Desc: " + after.fldGroupDesc + " User : " + afterUser, CurrentUser.Account);

                            if (astatus != "" && userid != "")
                            {
                                groupProfileDao.MoveToGroupUserFromTemp(id, userid, "Create");
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYA");
                            }
                            if (groupProfileDao.CheckGroupMasterExistByID(id, "GroupCheckerAdd") == false)
                            {
                                groupProfileDao.MoveToGroupMasterFromTemp(id, "Create"); 
                            }
                            groupProfileDao.DeleteGroupMasterTemp(id, "Approve");  
                            //auditTrailDao.Log("Approve - Create Group ID : " + id, CurrentUser.Account);

                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_AddTemplate(afterUser, "Approve", "Approve", id);
                                auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }

                        }
                        else if (action.Equals("D"))
                        {
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(id, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + "\n";
                            }

                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserIDTemp(id);
                            auditTrailDao.Log("Group Code: " + after.fldGroupId + " Group Desc: " + after.fldGroupDesc + " User : " + afterUser, CurrentUser.Account);
                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_DeleteTemplate(afterUser, "Approve", "Approve", id);
                                auditTrailDao.SecurityLog("Delete Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }
                            if (astatus != "" && userid != "")
                            {
                                groupProfileDao.UpdateGroupUserTemp(id, userid);
                                groupProfileDao.DeleteGroupUserFromTemp(id,userid);
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYA");
                            }
                            groupProfileDao.UpdateGroupMasterTemp(id);
                            groupProfileDao.DeleteGroupMaster(id);
                            groupProfileDao.DeleteGroupMasterTemp(id, "Approve");                             
                            //auditTrailDao.Log("Approve - Delete Group ID : " + id, CurrentUser.Account);
                        }
                        else if (action.Equals("E"))
                        {
                            string beforeUser2 = "";
                            List<UserModel> beforeUserLists2 = groupProfileDao.ListSelectedUserInGroup(id, CurrentUser.Account.BankCode);
                            foreach (var beforeUserlist2 in beforeUserLists2)
                            {
                                beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                            }
                            GroupProfileModel before = SecurityAuditLogDao.CheckGroupMasterUserID(id, beforeUser2);
                            groupProfileDao.MoveToGroupMasterFromTemp(id, "Update");
                            groupProfileDao.DeleteGroupMasterTemp(id, "Approve"); 
                           // auditTrailDao.Log("Approve - Update Group ID : " + id, CurrentUser.Account);


                            if (groupProfileDao.CheckGroupUserExistInTemp(id, userid, "Delete") == true)
                            {
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "Delete");
                            }

                            if (groupProfileDao.CheckGroupUserExistInTemp(id, userid, "Add") == true) 
                            {
                                groupProfileDao.MoveToGroupUserFromTemp(id, userid, "Update");
                            }
                            else
                            {
                                groupProfileDao.MoveToGroupUserFromTemp(id, userid, "Edit");
                            }

                            groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYA");
                            groupProfileDao.MoveToGroupMasterFromTemp(id, "Update");
                            groupProfileDao.DeleteGroupMasterTemp(id, "Approve");

                            //GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserID(id, after);
                            string alreadyselecteduser2 = "";
                            List<UserModel> alreadyselecteduserLists2 = groupProfileDao.ListSelectedUserInGroup(id, CurrentUser.Account.BankCode);
                            foreach (var alreadyselecteduserList2 in alreadyselecteduserLists2)
                            {
                                alreadyselecteduser2 = alreadyselecteduser2 + alreadyselecteduserList2.fldUserAbb + ",";
                            }

                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserID(id, alreadyselecteduser2);

                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_EditTemplate(before, after, beforeUser2, alreadyselecteduser2, "Approve", "Approve", id);
                                auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }
                            auditTrailDao.Log("after Update => Group Code: " + after.fldGroupId + " Group Desc: " + after.fldGroupDesc + " User : " + alreadyselecteduser2, CurrentUser.Account);
                            // auditTrailDao.Log("Approve - Update Group ID : " + id, CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIds.GroupChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            int count = 0;
            CurrentUser.Account.Status = "R";
            CurrentUser.Account.TaskId = TaskIds.GroupChecker.INDEX;
            string sTaskId= TaskIds.GroupChecker.VERIFY;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            try
            {
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];
                string astatus = "";
                string userid = "";

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        count++;
                        tmpArr = arrResult.Split(':');
                        string actTask = tmpArr[0].Trim().ToString();
                        string action = actTask.Substring(0, 1);
                        string taskId = actTask.Substring(1, 6);  //tmpArr[1].Trim().ToString();
                        string id = actTask.Remove(0, 7); //tmpArr[1].Trim().ToString();
                        if (tmpArr.Length > 1)
                        {
                            astatus = tmpArr[1].Trim().ToString();
                            userid = tmpArr[2].Trim().ToString();
                        }

                        if (action.Equals("A"))
                        {
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroupChecker(id, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserIDTemp(id);
                            auditTrailDao.Log("Group Code: " + after.fldGroupId + " Group Desc: " + after.fldGroupDesc + " User : " + afterUser, CurrentUser.Account);
                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_AddTemplate(afterUser, "Reject", "Reject", id);
                                auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }
                            if (astatus != "" && userid != "")
                            {
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            }
                            groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            groupProfileDao.DeleteGroupMasterTemp(id, "Reject");
                        }
                        else if (action.Equals("D"))
                        {
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(id, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + "\n";
                            }
                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserID(id, afterUser);
                            auditTrailDao.Log("Group Code: " + after.fldGroupId + " Group Desc: " + after.fldGroupDesc + " User : " + afterUser, CurrentUser.Account);
                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_DeleteTemplate(afterUser, "Reject", "Reject", id);
                                auditTrailDao.SecurityLog("Delete Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }
                            if (astatus != "" && userid != "")
                            {
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            }
                            groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            groupProfileDao.DeleteGroupMasterTemp(id, "Reject");
                           
                        }
                        else if (action.Equals("E"))
                        {
                            string beforeUser2 = "";
                            List<UserModel> beforeUserLists2 = groupProfileDao.ListSelectedUserInGroup(id, CurrentUser.Account.BankCode);
                            foreach (var beforeUserlist2 in beforeUserLists2)
                            {
                                beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                            }
                            string alreadyselecteduser2 = "";
                            List<UserModel> alreadyselecteduserLists2 = groupProfileDao.ListSelectedUserInGroupChecker(id, CurrentUser.Account.BankCode);
                            foreach (var alreadyselecteduserList2 in alreadyselecteduserLists2)
                            {
                                alreadyselecteduser2 = alreadyselecteduser2 + alreadyselecteduserList2.fldUserAbb + ",";
                            }
                            GroupProfileModel before = SecurityAuditLogDao.CheckGroupMasterUserID(id, beforeUser2);
                            GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserIDTemp(id);
                            if (count == 1)
                            {
                                string ActionDetails = SecurityAuditLogDao.GroupProfileChecker_EditTemplate(before, after, beforeUser2, alreadyselecteduser2, "Reject", "Reject", id);
                                auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskId, CurrentUser.Account);
                            }
                            if (astatus != "" && userid != "")
                            {
                                groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            }
                            groupProfileDao.DeleteGroupUserTemp(id, userid, "VERIFYR");
                            groupProfileDao.DeleteGroupMasterTemp(id, "Reject");
                            auditTrailDao.Log("after Update => Group Code: " + before.fldGroupId + " Group Desc: " + before.fldGroupDesc + " User : " + alreadyselecteduser2, CurrentUser.Account);
                        }
                        
                        //auditTrailDao.Log("Reject Create ,Update or Delete - Group ID :" + id, CurrentUser.Account);
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