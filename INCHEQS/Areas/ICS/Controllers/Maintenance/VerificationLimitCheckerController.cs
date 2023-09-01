﻿//bank of Malaysia
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
//using INCHEQS.Areas.OCS.Models.HubBranch;
using INCHEQS.Areas.OCS.Models.SystemCutOffTime;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.HubBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {
    public class VerificationLimitCheckerController : BaseController {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private IUserDao userDao;
        private IGroupDao groupDao;
  
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

        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public VerificationLimitCheckerController(ISystemProfileDao systemProfileDao, ISystemCutOffTimeDao systemCutOffTimeDao, IHubBranchDao hubBranchDao, IHubDao hubDao, IUserDao userDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IHostReturnReasonDao hostReturnReasonDao, IInternalBranchDao internalBranchDao, IVerificationLimitDao verificationLimitDao, IPullOutReasonDao pullOutReasonDao, ITransactionCodeDao transactionCodeDao, ITransactionTypeDao transactionTypeDao, ITCCodeDao tcCodeDao, IThresholdSettingDao thresholdSettingDao, IGroupDao groupDao, ITaskDao taskDao, ApplicationDbContext dbContext, IMaintenanceAuditLogDao MaintenanceAuditLogDao) {
            this.userDao = userDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            //this.bankCodeDao = bankCodeDao;
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
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            this.dbContext = dbContext;
        }
        
            [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimitChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.VerificationLimitChecker.INDEX));
            //ViewBag.Menutitle = GetMenuTitle("102230");

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimitChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.VerificationLimitChecker.INDEX, "View_VerificationLimitChecker", "actionStatus asc, fldId asc", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }
            

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimitChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col) {
            try {
                string sTaskId = TaskIdsOCS.VerificationLimitChecker.VERIFY;
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null) {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults) {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId) {
                          
                            case TaskIdsOCS.VerificationLimit.INDEX:
                                //if (formAction.Equals("Approve"))
                                //{
                                if (action.Equals("A"))
                                {
                                    verificationLimitDao.CreateInVerificationLimitMaster(id);
                                    //auditTrailDao.Log("Approve - Add Verification Limit : " + id, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    VerificationLimitModel before = MaintenanceAuditLogDao.GetVerificationLimitData(id);

                                    verificationLimitDao.DeleteInVerificationLimitMaster(id);
                                    verificationLimitDao.CreateInVerificationLimitMaster(id);
                                   
                                    VerificationLimitModel after = MaintenanceAuditLogDao.GetVerificationLimitData(id);

                                    //auditTrailDao.Log("Approve - Update Verification Limit  : " + id, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                    verificationLimitDao.DeleteInVerificationLimitMaster(id);
                                    //auditTrailDao.Log("Approve - Delete Verification Limit  : " + id, CurrentUser.Account);
                                }

                                
                                verificationLimitDao.DeleteInVerificationLimitMasterTemp(id);
                                //auditTrailDao.Log("Reject Update or Delete - Verification Limit  :" + id, CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.VerificationLimitChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIdsOCS.VerificationLimitChecker.VERIFY;
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
                            case TaskIdsOCS.VerificationLimit.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    VerificationLimitModel before = MaintenanceAuditLogDao.GetVerificationLimitData(id);
                                    VerificationLimitModel after = MaintenanceAuditLogDao.GetVerificationLimitDataTemp(id);
                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.VerificationLimitChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject Verification Limit", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                verificationLimitDao.DeleteInVerificationLimitMasterTemp(id);
                                //auditTrailDao.Log("Reject Update or Delete or Add - Verification Limit :" + id, CurrentUser.Account);
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