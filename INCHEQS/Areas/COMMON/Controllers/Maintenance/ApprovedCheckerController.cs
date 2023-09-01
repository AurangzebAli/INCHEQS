﻿//bank of Malaysia
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

using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Areas.OCS.Models.TCCode;
using INCHEQS.Areas.OCS.Models.TransactionCode;
//using INCHEQS.Areas.OCS.Models.HubBranch;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.HubBranch;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance {
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
        private iHolidayDao holidayDao;

        public ApprovedCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IHubBranchDao hubBranchDao, IHubDao hubDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IBankCodeDao bankCodeDao, IHostReturnReasonDao hostReturnReasonDao, IStateCodeDao stateCodeDao, IBranchCodeDao branchCodeDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, IReturnCodeDao returnCodeDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, IGroupDao groupDao, ITaskDao taskDao, iHolidayDao holidayDao) {
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
            this.tcCodeDao = tcCodeDao;
            this.thresholdSettingDao = thresholdSettingDao;
            this.groupDao = groupDao;
            this.taskDao = taskDao;
            this.hubDao = hubDao;
            this.hubBranchDao = hubBranchDao;
            this.systemCutOffTimeDao = systemCutOffTimeDao;
            this.systemProfileDao = systemProfileDao;
            this.holidayDao = holidayDao;
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
                                        //auditTrailDao.Log("Approve - Create Group ID : " + id, CurrentUser.Account);
                                    }
                                    else if (action.Equals("D"))
                                    {       
                                        groupDao.DeleteAllUserInGroup(id);
                                        groupDao.DeleteAllTaskInGroup(id);
                                        groupDao.DeleteInGroupMasterTemp(id);
                                        groupDao.DeleteGroup(id);
                                        //auditTrailDao.Log("Approve - Delete Group ID : " + id, CurrentUser.Account);
                                    }
                                //}
                                groupDao.DeleteInGroupMasterTemp(id);
                                //auditTrailDao.Log("Reject Create or Delete - Group ID :" + id, CurrentUser.Account);
                                break;
                            case TaskIds.HubUserProfile.INDEX:
                                if (action.Equals("A"))
                                {
                                    //hubDao.CreateInHubMaster(id);
                                    //auditTrailDao.Log("Approve - Create Hub ID : " + id, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    //hubDao.DeleteAllUserInHub(id);
                                    //hubDao.DeleteHub(id);
                                    //auditTrailDao.Log("Approve - Delete Hub ID : " + id, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    //hubDao.UpdateHubMaster(id);
                                    //auditTrailDao.Log("Approve - Update Hub ID : " + id, CurrentUser.Account);

                                    string afterUser = "";
                                    List<UserModel> afterUserLists = hubDao.ListSelectedUserInHub(id, CurrentUser.Account.BankCode);
                                    foreach (var afterUserlist in afterUserLists)
                                    {
                                        afterUser = afterUser + afterUserlist.fldUserAbb + ',';
                                    }
                                    //auditTrailDao.Log("Update User In Hub , After Update => - Hub ID: " + id + " User : " + afterUser, CurrentUser.Account);
                                }
                                hubDao.DeleteInHubMasterTemp(id);
                                //hubDao.DeleteAllUserInHubTemp(id);                                
                                break;

                            case TaskIds.HubBranchProfile.INDEX:
                                if (action.Equals("U"))
                                {
                                //    //hubBranchDao.UpdateHubMaster(id);
                                //    auditTrailDao.Log("Approve - Update Hub ID : " + id, CurrentUser.Account);

                                //    string afterBranch = "";
                                //    List<InternalBranchModel> afterBranchLists = hubBranchDao.ListSelectedBranchInHub(id, CurrentUser.Account.BankCode);
                                //    foreach (var afterBranchlist in afterBranchLists)
                                //    {
                                //       // afterBranch = afterBranch + afterBranchlist.fldBranchCode + ',';
                                //    }
                                //    auditTrailDao.Log("Update Branch In Hub , After Update => - Hub ID: " + id + " Branch : " + afterBranch, CurrentUser.Account);
                                }
                                //hubBranchDao.DeleteInHubMasterBranchTemp(id);
                                //hubBranchDao.DeleteAllBranchInHubTemp(id);
                                break;

                            case TaskIds.BankCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        bankCodeDao.CreateInBankMaster(col);
                                        //auditTrailDao.Log("Approve - Create Bank Code : "+id,CurrentUser.Account);
                                    } else if (action.Equals("D")) {
                                       // bankCodeDao.DeleteInBankMaster(id);
                                        //auditTrailDao.Log("Approve - Delete Bank Code : " + id, CurrentUser.Account);
                                    }
                                //}
                               // bankCodeDao.DeleteInBankMasterTemp(id);
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
                                        //verificationLimitDao.CreateInVerificationBatchSizeLimit(id);
                                    } else if (action.Equals("D")) {
                                       // verificationLimitDao.DeleteInVerificationBatchSizeLimit(id);
                                    }
                                //}
                                //verificationLimitDao.DeleteInVerificationBatchSizeLimitTemp(id);
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
                                        //returnCodeDao.CreateInRejectMaster(id);
                                    } else if (action.Equals("D")) {
                                        //returnCodeDao.DeleteInRejectMaster(id);
                                    }
                                //}
                                returnCodeDao.DeleteInRejectMasterTemp(id);
                                break;

                            case TaskIds.TransactionCode.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        transactionCodeDao.CreateTransactionCode(id);
                                    } else if (action.Equals("D")) {
                                        transactionCodeDao.DeleteTransactionCode(id);
                                    } else if (action.Equals("U")) {
                                        transactionCodeDao.UpdateTransactionCode(id);
                                    }
                                //}
                                transactionCodeDao.DeleteInTransactionCodeTemp(id);
                                break;

                            case TaskIds.TransactionType.INDEX:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        transactionTypeDao.CreateTransactionType(id);
                                    } else if (action.Equals("D")) {
                                        transactionTypeDao.DeleteTransactionType(id);
                                    } else if (action.Equals("U")) {
                                        transactionTypeDao.UpdateTransactionType(id);
                                    }
                                //}
                                transactionTypeDao.DeleteInTransactionTypeTemp(id);
                                break;

                            //case TaskIds.TCCode.INDEX:
                            //    //if (formAction.Equals("Approve")) {
                            //    if (action.Equals("A"))
                            //    {
                            //        tcCodeDao.CreateTCCode(id);
                            //    }
                            //    else if (action.Equals("D"))
                            //    {
                            //        tcCodeDao.DeleteTCCode(id);
                            //    }
                            //    else if (action.Equals("U"))
                            //    {
                            //        tcCodeDao.UpdateTCCode(id);
                            //    }
                            //    //}
                            //    tcCodeDao.DeleteInTCCodeTemp(id);
                            //    break;
                            //case TaskIds.SystemCutOffTime.INDEX:
                            //    if (action.Equals("A"))
                            //    {
                            //        systemCutOffTimeDao.CreateSystemCutOffTime(id);
                            //    }
                            //    else if (action.Equals("D"))
                            //    {
                            //        systemCutOffTimeDao.DeleteSystemCutOffTime(id);
                            //    }
                            //    else if (action.Equals("U"))
                            //    {
                            //        systemCutOffTimeDao.UpdateSystemCutOffTime(id);
                            //    }
                            //    systemCutOffTimeDao.DeleteInSystemCutOffTimeTemp(id);
                            //    break;

                            case TaskIds.ThresholdSetting.MAIN:
                                //if (formAction.Equals("Approve")) {
                                    if (action.Equals("A")) {
                                        //thresholdSettingDao.CreateThresholdSetting(id);
                                    } else if (action.Equals("D")) {
                                        //thresholdSettingDao.DeleteInThresholdSetting(id);
                                    }
                                //}
                                //thresholdSettingDao.DeleteInThresholdSettingTemp(id);
                                break;
                            //case TaskIds.SystemProfile.INDEX:
                            //    //if (formAction.Equals("Approve")) {
                            //    if (action.Equals("U"))
                            //    {
                            //        systemProfileDao.UpdateSytemProfile(id, CurrentUser.Account.UserId);
                            //    }
                            //    //}
                            //    systemProfileDao.DeleteInSystemProfileTemp(id);
                            //    break;
                            //case TaskIds.Task.INDEX:
                            //    //if (formAction.Equals("Approve"))
                            //    //{
                            //        if (action.Equals("A") || action.Equals("S") || action.Equals("U"))
                            //        {
                            //            //taskDao.DeleteTaskNotSelectedApproval(id);
                            //            //taskDao.UpdateSelectedTaskIdApproval(CurrentUser.Account.UserId,id);
                            //            auditTrailDao.Log("Approve - Update Task Assigment : " + id, CurrentUser.Account);
                            //        }
                                
                            //    //}
                            //    taskDao.UpdateRejectTaskIdApproval(CurrentUser.Account.UserId,id);
                            //    auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
                            //    break;
                            case TaskIds.Holiday.INDEX:
                                if (action.Equals("A"))
                                {
                                    holidayDao.CreateHolidayinMain(id);
                                }
                                else if (action.Equals("D"))
                                {
                                    HolidayModel objHoliday = holidayDao.GetHolidayData(id);
                                    holidayDao.DeleteFromHolidayTable(id);
                                    //auditTrailDao.Log("Deleted Holiday from Main - Holiday Date :" + objHoliday.Date + ", Recurring : " + objHoliday.recurring + ", Recurring Type : " + objHoliday.recurrType + ", Description : " + objHoliday.Desc, CurrentUser.Account);

                                }
                                else if (action.Equals("U"))
                                {
                                    HolidayModel before = holidayDao.GetHolidayData(id);
                                    //auditTrailDao.Log("Holiday Calender Created Before - Holiday Date : " + before.Date + ", Recurring : " + before.recurring + ", Recurring Type : " + before.recurrType + ", Description : " + before.Desc, CurrentUser.Account);

                                    holidayDao.UpdateHolidayToMain(id);

                                    HolidayModel after = holidayDao.GetHolidayData(id);
                                    //auditTrailDao.Log("Holiday Calender Created After- Holiday Date : " + after.Date + ", Recurring : " + after.recurring + ", Recurring Type : " + after.recurrType + ", Description : " + after.Desc, CurrentUser.Account);

                                }
                                holidayDao.DeleteHolidayinTemp(id, Convert.ToInt32(arrResult.Length));
                                if (arrResult.Length < 15)
                                {
                                    //auditTrailDao.Log("Approve Holiday - Task Assigment :" + taskId + " Holiday ID : " + id, CurrentUser.Account);

                                }
                                else
                                {
                                    //auditTrailDao.Log("Approve Holiday - Task Assigment :" + taskId + " Holiday Date : " + id, CurrentUser.Account);
                                }
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
                                //auditTrailDao.Log("Reject Create or Delete - Group ID :" + id, CurrentUser.Account);
                                break;

                            case TaskIds.HubUserProfile.INDEX:
                                if (action.Equals("A"))
                                {
                                    //hubDao.DeleteAllUserInHub(id);
                                }
                                hubDao.DeleteInHubMasterTemp(id);
                                //hubDao.DeleteAllUserInHubTemp(id);
                                //auditTrailDao.Log("Reject Create, Update or Delete - Hub ID :" + id, CurrentUser.Account);
                                break;
                            case TaskIds.HubBranchProfile.INDEX:
                               // hubBranchDao.DeleteInHubMasterBranchTemp(id);
                                //hubBranchDao.DeleteAllBranchInHubTemp(id);
                                //auditTrailDao.Log("Reject Update - Hub ID :" + id, CurrentUser.Account);
                                break;
                            case TaskIds.BankCode.INDEX:
                                //bankCodeDao.DeleteInBankMasterTemp(id);
                                //auditTrailDao.Log("Reject Create or Delete - Bank Code :" + id, CurrentUser.Account);
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
                                //verificationLimitDao.DeleteInVerificationBatchSizeLimitTemp(id);
                                break;

                            case TaskIds.PullOutReason.INDEX:
                                pullOutReasonDao.DeleteInPullOutReasonTemp(id);
                                break;

                            case TaskIds.ReturnCode.INDEX:
                                returnCodeDao.DeleteInRejectMasterTemp(id);
                                break;

                            case TaskIds.TransactionCode.INDEX:
                                transactionCodeDao.DeleteInTransactionCodeTemp(id);
                                break;

                            case TaskIds.TransactionType.INDEX:
                                transactionTypeDao.DeleteInTransactionTypeTemp(id);
                                break;
                            //case TaskIds.TCCode.INDEX:
                            //    tcCodeDao.DeleteInTCCodeTemp(id);
                            //    break;
                            //case TaskIds.SystemCutOffTime.INDEX:
                            //    systemCutOffTimeDao.DeleteInSystemCutOffTimeTemp(id);
                            //    break;
                            case TaskIds.ThresholdSetting.MAIN:
                                //thresholdSettingDao.DeleteInThresholdSettingTemp(id);
                                break;
                            //case TaskIds.SystemProfile.INDEX:
                            //    systemProfileDao.DeleteInSystemProfileTemp(id);
                            //    break;
                            case TaskIds.Holiday.INDEX:
                                holidayDao.DeleteHolidayinTemp(id,Convert.ToInt32(arrResult.Length));
                                if (arrResult.Length < 15)
                                {
                                    //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + " Holiday ID : " + id, CurrentUser.Account);
                                }
                                else
                                {
                                     //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + " Holiday Date : " + id  , CurrentUser.Account);
                                }
                                break;
                            case TaskIds.Task.INDEX: 
                                taskDao.UpdateRejectTaskIdApproval(CurrentUser.Account.UserId,id);
                                //auditTrailDao.Log("Reject Update or Delete - Task Assigment :" + id, CurrentUser.Account);
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