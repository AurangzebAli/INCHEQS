//bank of Malaysia
using INCHEQS.BNM.BankCode;
using INCHEQS.BNM.BranchCode;
using INCHEQS.BNM.ReturnCode;
using INCHEQS.BNM.StateCode;

//bank of Philipine
//using INCHEQS.PCHC.BankCode;
//using INCHEQS.PCHC.BranchCode;
//using INCHEQS.PCHC.ReturnCode;
//using INCHEQS.PCHC.StateCode;

using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Models;
using INCHEQS.Security.AuditTrail;
using INCHEQS.InternalBranch.InternalBranch;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.Models.TransactionType;
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

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class ApprovedCheckerController : BaseController {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IUserDao userDao;
        private IGroupDao groupDao;
  
        private IHostReturnReasonDao hostReturnReasonDao;
        private IBankCodeDao bankCodeDao;
        private IStateCodeDao stateCodeDao;
        private IBranchCodeDao branchCodeDao;
        private IReturnCodeDao returnCodeDao;
        private IInternalBranchDao internalBranchDao;
        private IVerificationLimitDao verificationLimitDao;
        private IPullOutReasonDao pullOutReasonDao;
     
        private ITransactionCodeDao transactionCodeDao;
        private ITransactionTypeDao transactionTypeDao;
        private IThresholdSettingDao thresholdSettingDao;
        protected readonly ISearchPageService searchPageService;
        private ITaskDao taskDao;

        public ApprovedCheckerController(IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IBankCodeDao bankCodeDao, IHostReturnReasonDao hostReturnReasonDao, IStateCodeDao stateCodeDao, IBranchCodeDao branchCodeDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, IReturnCodeDao returnCodeDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, IThresholdSettingDao thresholdSettingDao, IGroupDao groupDao, ITaskDao taskDao) {
            this.userDao = userDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.bankCodeDao = bankCodeDao;
            this.hostReturnReasonDao = hostReturnReasonDao;
            this.stateCodeDao = stateCodeDao;
            this.branchCodeDao = branchCodeDao;
            this.internalBranchDao = internalBranchDao;
            this.verificationLimitDao = verificationLimitDao;
            this.pullOutReasonDao = pullOutReasonDao;
            this.returnCodeDao = returnCodeDao;
            this.transactionCodeDao = transactionCodeDao;
            this.transactionTypeDao = transactionTypeDao;
            this.thresholdSettingDao = thresholdSettingDao;
            this.groupDao = groupDao;
            this.taskDao = taskDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ApprovedChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ApprovedChecker.INDEX, "View_ApprovedChecker", "fldId", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }
        
        [CustomAuthorize(TaskIds = TaskIds.ApprovedChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col) {
            try {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null) {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults) {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId) {
                            case TaskIds.User.INDEX:
                                //if (formAction.Equals("Approve"))
                                //{
                                    if (action.Equals("A"))
                                    {
                                        userDao.CreateInUserMaster(id);
                                        ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
                                        if (ViewBag.GetDataTempUser.fldAdminFlag.Trim() == "N" && ViewBag.GetDataTempUser.fldCCUFlag.Trim() == "")
                                        {
                                            userDao.UpdateUserDedicatedBranch(id);
                                        }
                                        else
                                        {
                                            userDao.DeleteUSerDedicatedBranch(id);
                                        }
                                    }
                                    else if (action.Equals("D"))
                                    {
                                        userDao.DeleteInUserMaster(id);
                                        ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
                                        if (ViewBag.GetDataTempUser.fldAdminFlag.Trim() == "N" && ViewBag.GetDataTempUser.fldCCUFlag.Trim() == "")
                                        {
                                            userDao.DeleteDedicatedBranch(id);
                                        }
                                    }
                                //}
                                ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
                                userDao.DeleteUSerDedicatedBranch(id);

                                userDao.DeleteInUserMasterTemp(id);
                                break;
                            case TaskIds.Group.INDEX:
                                //if (formAction.Equals("Approve"))
                                //{
                                    if (action.Equals("A"))
                                    {
                                        groupDao.CreateInGroupMaster(id);
                                        auditTrailDao.Log("Approve - Create Group ID : " + id, CurrentUser.Account);
                                    }
                                    else if (action.Equals("D"))
                                    {       
                                        groupDao.DeleteAllUserInGroup(id);
                                        groupDao.DeleteAllTaskInGroup(id);
                                        groupDao.DeleteInGroupMasterTemp(id);
                                        groupDao.DeleteGroup(id);
                                        auditTrailDao.Log("Approve - Delete Group ID : " + id, CurrentUser.Account);
                                    }
                                //}
                                groupDao.DeleteInGroupMasterTemp(id);
                                auditTrailDao.Log("Reject Create or Delete - Group ID :" + id, CurrentUser.Account);
                                break;

                            case TaskIds.BankCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        bankCodeDao.CreateInBankMaster(id);
                                        auditTrailDao.Log("Approve - Create Bank Code : "+id,CurrentUser.Account);
                                    } else if (action.Equals("D")) {
                                        bankCodeDao.DeleteInBankMaster(id);
                                        auditTrailDao.Log("Approve - Delete Bank Code : " + id, CurrentUser.Account);
                                    }
                                //}
                                bankCodeDao.DeleteInBankMasterTemp(id);
                                auditTrailDao.Log("Reject Create or Delete - Bank Code :" + id, CurrentUser.Account);
                                break;

                            case TaskIds.HostReturnReason.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        hostReturnReasonDao.CreateInBankHostStatusMaster(id);
                                    } else if (action.Equals("D")) {
                                        hostReturnReasonDao.DeleteInBankHostStatusMaster(id);
                                    }
                                //}
                                hostReturnReasonDao.DeleteInBankHostStatusMasterTemp(id);
                                break;

                            case TaskIds.StateCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        stateCodeDao.CreateInStateMaster(id); 
                                    } else if (action.Equals("D")) {
                                        stateCodeDao.DeleteInStateMaster(id);
                                    }
                                //}
                                stateCodeDao.DeleteInStateMasterTemp(id);
                                break;

                            case TaskIds.BranchCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        branchCodeDao.CreateInBranchMaster(id);
                                    } else if (action.Equals("D")) {
                                        branchCodeDao.DeleteInBranchMasters(id);
                                    }
                                //}
                                branchCodeDao.DeleteInBranchMasterTemp(id);
                                break;

                            case TaskIds.InternalBranch.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        internalBranchDao.CreateInInternalBranch(id);
                                    } else if (action.Equals("D")) {
                                        internalBranchDao.DeleteInInternalBranch(id);
                                    }
                                //}
                                internalBranchDao.DeleteInInternalBranchTemp(id);
                                break;

                            case TaskIds.VerificationLimit.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        verificationLimitDao.CreateInVerificationBatchSizeLimit(id);
                                    } else if (action.Equals("D")) {
                                        verificationLimitDao.DeleteInVerificationBatchSizeLimit(id);
                                    }
                                //}
                                verificationLimitDao.DeleteInVerificationBatchSizeLimitTemp(id);
                                break;

                            case TaskIds.PullOutReason.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        pullOutReasonDao.CreateInPullOutReason(id);
                                    } else if (action.Equals("D")) {
                                        pullOutReasonDao.DeleteInPullOutReason(id);
                                    }
                                //}
                                pullOutReasonDao.DeleteInPullOutReasonTemp(id);
                                break;

                            case TaskIds.ReturnCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        returnCodeDao.CreateInRejectMaster(id);
                                    } else if (action.Equals("D")) {
                                        returnCodeDao.DeleteInRejectMaster(id);
                                    }
                                //}
                                returnCodeDao.DeleteInRejectMasterTemp(id);
                                break;

                            case TaskIds.TransactionCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        transactionCodeDao.CreateInTransMaster(id);
                                    } else if (action.Equals("D")) {
                                        transactionCodeDao.DeleteInTransMaster(id);
                                    }
                                //}
                                transactionCodeDao.DeleteInTransMasterTemp(id);
                                break;

                            case TaskIds.TransactionType.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        transactionTypeDao.CreateTransactionType(id);
                                    } else if (action.Equals("D")) {
                                        transactionTypeDao.DeleteTransactionType(id);
                                    }
                                //}
                                transactionTypeDao.DeleteInTransactionTypeTemp(id);
                                break;

                            case TaskIds.ThresholdSetting.MAIN:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        thresholdSettingDao.CreateThresholdSetting(id);
                                    } else if (action.Equals("D")) {
                                        thresholdSettingDao.DeleteInThresholdSetting(id);
                                    }
                                //}
                                thresholdSettingDao.DeleteInThresholdSettingTemp(id);
                                break;
                            case TaskIds.Task.INDEX:
                                //if (formAction.Equals("Approve"))
                                //{
                                    if (action.Equals("A") || action.Equals("S") || action.Equals("U"))
                                    {
                                        taskDao.DeleteTaskNotSelectedApproval(id);
                                        taskDao.UpdateSelectedTaskIdApproval(CurrentUser.Account.UserId,id);
                                        auditTrailDao.Log("Approve - Update Task Assigment : " + id, CurrentUser.Account);
                                    }
                                
                                //}
                                taskDao.UpdateRejectTaskIdApproval(CurrentUser.Account.UserId,id);
                                auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                } else {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.ApprovedChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
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
                            case TaskIds.User.INDEX:
                                ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
                                userDao.DeleteUSerDedicatedBranch(id);
                                userDao.DeleteInUserMasterTemp(id);
                                break;
                            case TaskIds.Group.INDEX:
                                groupDao.DeleteInGroupMasterTemp(id);
                                auditTrailDao.Log("Reject Create or Delete - Group ID :" + id, CurrentUser.Account);
                                break;

                            case TaskIds.BankCode.INDEX:
                                bankCodeDao.DeleteInBankMasterTemp(id);
                                auditTrailDao.Log("Reject Create or Delete - Bank Code :" + id, CurrentUser.Account);
                                break;

                            case TaskIds.HostReturnReason.INDEX:
                                hostReturnReasonDao.DeleteInBankHostStatusMasterTemp(id);
                                break;

                            case TaskIds.StateCode.INDEX:
                                stateCodeDao.DeleteInStateMasterTemp(id);
                                break;

                            case TaskIds.BranchCode.INDEX:
                                branchCodeDao.DeleteInBranchMasterTemp(id);
                                break;

                            case TaskIds.InternalBranch.INDEX:
                                internalBranchDao.DeleteInInternalBranchTemp(id);
                                break;

                            case TaskIds.VerificationLimit.INDEX:
                                verificationLimitDao.DeleteInVerificationBatchSizeLimitTemp(id);
                                break;

                            case TaskIds.PullOutReason.INDEX:
                                pullOutReasonDao.DeleteInPullOutReasonTemp(id);
                                break;

                            case TaskIds.ReturnCode.INDEX:
                                returnCodeDao.DeleteInRejectMasterTemp(id);
                                break;

                            case TaskIds.TransactionCode.INDEX:
                                transactionCodeDao.DeleteInTransMasterTemp(id);
                                break;

                            case TaskIds.TransactionType.INDEX:
                                transactionTypeDao.DeleteInTransactionTypeTemp(id);
                                break;

                            case TaskIds.ThresholdSetting.MAIN:
                                thresholdSettingDao.DeleteInThresholdSettingTemp(id);
                                break;
                            case TaskIds.Task.INDEX: 
                                taskDao.UpdateRejectTaskIdApproval(CurrentUser.Account.UserId,id);
                                auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
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
    }
    //20170301 Change from 1 method to reject and approve methods
    //[CustomAuthorize(TaskIds = TaskIds.ApprovedChecker.VERIFY)]
    //[HttpPost]
    //public ActionResult Verify(FormCollection col)
    //{
    //    try
    //    {
    //        string formAction = col["formAction"];
    //        List<string> arrResults = new List<string>();

    //        if ((col["deleteBox"]) != null)
    //        {
    //            arrResults = col["deleteBox"].Split(',').ToList();

    //            foreach (string arrResult in arrResults)
    //            {
    //                string action = arrResult.Substring(0, 1);
    //                string taskId = arrResult.Substring(1, 6);
    //                string id = arrResult.Remove(0, 7);

    //                //Act based on task id
    //                switch (taskId)
    //                {
    //                    case TaskIds.User.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                userDao.CreateInUserMaster(id);
    //                                ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
    //                                if (ViewBag.GetDataTempUser.fldAdminFlag.Trim() == "N" && ViewBag.GetDataTempUser.fldCCUFlag.Trim() == "")
    //                                {
    //                                    userDao.UpdateUserDedicatedBranch(id);
    //                                }
    //                                else
    //                                {
    //                                    userDao.DeleteUSerDedicatedBranch(id);
    //                                }
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                userDao.DeleteInUserMaster(id);
    //                                ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
    //                                if (ViewBag.GetDataTempUser.fldAdminFlag.Trim() == "N" && ViewBag.GetDataTempUser.fldCCUFlag.Trim() == "")
    //                                {
    //                                    userDao.DeleteDedicatedBranch(id);
    //                                }
    //                            }
    //                        }

    //                        ViewBag.GetDataTempUser = userDao.GetUserTemp(id);
    //                        userDao.DeleteUSerDedicatedBranch(id);

    //                        userDao.DeleteInUserMasterTemp(id);
    //                        break;
    //                    case TaskIds.Group.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                groupDao.CreateInGroupMaster(id);
    //                                auditTrailDao.Log("Approve - Create Group ID : " + id, CurrentUser.Account);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                groupDao.DeleteAllUserInGroup(id);
    //                                groupDao.DeleteAllTaskInGroup(id);
    //                                groupDao.DeleteInGroupMasterTemp(id);
    //                                groupDao.DeleteGroup(id);
    //                                auditTrailDao.Log("Approve - Delete Group ID : " + id, CurrentUser.Account);
    //                            }
    //                        }
    //                        groupDao.DeleteInGroupMasterTemp(id);
    //                        auditTrailDao.Log("Reject Create or Delete - Group ID :" + id, CurrentUser.Account);
    //                        break;

    //                    case TaskIds.BankCode.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                bankCodeDao.CreateInBankMaster(id);
    //                                auditTrailDao.Log("Approve - Create Bank Code : " + id, CurrentUser.Account);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                bankCodeDao.DeleteInBankMaster(id);
    //                                auditTrailDao.Log("Approve - Delete Bank Code : " + id, CurrentUser.Account);
    //                            }
    //                        }
    //                        bankCodeDao.DeleteInBankMasterTemp(id);
    //                        auditTrailDao.Log("Reject Create or Delete - Bank Code :" + id, CurrentUser.Account);
    //                        break;

    //                    case TaskIds.HostReturnReason.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                hostReturnReasonDao.CreateInBankHostStatusMaster(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                hostReturnReasonDao.DeleteInBankHostStatusMaster(id);
    //                            }
    //                        }
    //                        hostReturnReasonDao.DeleteInBankHostStatusMasterTemp(id);
    //                        break;

    //                    case TaskIds.StateCode.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                stateCodeDao.CreateInStateMaster(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                stateCodeDao.DeleteInStateMaster(id);
    //                            }
    //                        }
    //                        stateCodeDao.DeleteInStateMasterTemp(id);
    //                        break;

    //                    case TaskIds.BranchCode.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                branchCodeDao.CreateInBranchMaster(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                branchCodeDao.DeleteInBranchMasters(id);
    //                            }
    //                        }
    //                        branchCodeDao.DeleteInBranchMasterTemp(id);
    //                        break;

    //                    case TaskIds.InternalBranch.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                internalBranchDao.CreateInInternalBranch(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                internalBranchDao.DeleteInInternalBranch(id);
    //                            }
    //                        }
    //                        internalBranchDao.DeleteInInternalBranchTemp(id);
    //                        break;

    //                    case TaskIds.VerificationLimit.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                verificationLimitDao.CreateInVerificationBatchSizeLimit(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                verificationLimitDao.DeleteInVerificationBatchSizeLimit(id);
    //                            }
    //                        }
    //                        verificationLimitDao.DeleteInVerificationBatchSizeLimitTemp(id);
    //                        break;

    //                    case TaskIds.PullOutReason.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                pullOutReasonDao.CreateInPullOutReason(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                pullOutReasonDao.DeleteInPullOutReason(id);
    //                            }
    //                        }
    //                        pullOutReasonDao.DeleteInPullOutReasonTemp(id);
    //                        break;

    //                    case TaskIds.ReturnCode.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                returnCodeDao.CreateInRejectMaster(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                returnCodeDao.DeleteInRejectMaster(id);
    //                            }
    //                        }
    //                        returnCodeDao.DeleteInRejectMasterTemp(id);
    //                        break;

    //                    case TaskIds.TransactionCode.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                transactionCodeDao.CreateInTransMaster(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                transactionCodeDao.DeleteInTransMaster(id);
    //                            }
    //                        }
    //                        transactionCodeDao.DeleteInTransMasterTemp(id);
    //                        break;

    //                    case TaskIds.TransactionType.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                transactionTypeDao.CreateTransactionType(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                transactionTypeDao.DeleteTransactionType(id);
    //                            }
    //                        }
    //                        transactionTypeDao.DeleteInTransactionTypeTemp(id);
    //                        break;

    //                    case TaskIds.ThresholdSetting.MAIN:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A"))
    //                            {
    //                                thresholdSettingDao.CreateThresholdSetting(id);
    //                            }
    //                            else if (action.Equals("D"))
    //                            {
    //                                thresholdSettingDao.DeleteInThresholdSetting(id);
    //                            }
    //                        }
    //                        thresholdSettingDao.DeleteInThresholdSettingTemp(id);
    //                        break;
    //                    case TaskIds.Task.INDEX:
    //                        if (formAction.Equals("Approve"))
    //                        {
    //                            if (action.Equals("A") || action.Equals("S") || action.Equals("U"))
    //                            {
    //                                taskDao.DeleteTaskNotSelectedApproval(id);
    //                                taskDao.UpdateSelectedTaskIdApproval(id);
    //                                auditTrailDao.Log("Approve - Update Task Assigment : " + id, CurrentUser.Account);
    //                            }

    //                        }
    //                        taskDao.UpdateRejectTaskIdApproval(id);
    //                        auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
    //                        break;
    //                }
    //            }
    //            TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
    //        }
    //        else
    //        {
    //            TempData["Warning"] = Locale.PleaseSelectARecord;
    //        }
    //        return RedirectToAction("Index");
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }
    //}
}