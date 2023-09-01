using INCHEQS.Areas.COMMON.Models.Users;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeTransactionBalancing.Base;
using INCHEQS.Areas.OCS.Models.AuditTrailOCS;
using INCHEQS.Areas.OCS.Models.Balancing;
using INCHEQS.Areas.OCS.Models.ChequeAmountEntry;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Helpers;
using INCHEQS.Models.Report.OCS;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Models.Verification;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeTransactionBalancing
{
    public class ChequeTransactionBalancingController : ChequeTransactionBalancingBaseController
    {
        private readonly ITransactionBalancingDao TransactionBalancingDao;
        string taskIdParam;
        public ChequeTransactionBalancingController(IPageConfigDaoOCS pageConfigDao, ITransactionBalancingDao TransactionBalancingDao, ICommonOutwardItemDao commonOutwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportServiceOCS reportService, IHostReturnReasonDao hostReturnReasonDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, IAuditTrailOCSDao auditTrailOCSDao)
            : base(pageConfigDao,commonOutwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, userDao, largeAmountDao, systemProfileDao, TransactionBalancingDao, auditTrailOCSDao)
        {
            this.TransactionBalancingDao = TransactionBalancingDao;
            base.chequeVerificationPageHtml = "ChequeVerificationPage";
            base.searchPageHtml = "Index";
            base.searchResultPageHtml = "SearchResultPage";
        }

        protected override string initializeQueueTaskId()
        {
            taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return taskIdParam;
        }


        public async Task<ActionResult> ChequeBalancingConfirm(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_flditemid"];
            string CheckConfirm = "B";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;


            //if ("Maker".Equals(gQueueSqlConfig.TaskRole))
            //{
            errorMessages = TransactionBalancingDao.Validate(collection, CurrentUser.Account, CheckConfirm);
            Checking = errorMessages.Count;
            //}
            ////check if validate contain error
            if ((Checking > 0))
            {
                result = commonOutwardItemDao.ErrorCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    resultModel = OutwardItemConcern.OutwardItemWithErrorMessages(gQueueSqlConfigSearch, result, errorMessages);
                    string OutwardTransNo = result["fldTransNo"];
                    List<BalancingHistoryModel> history1 = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                    ViewBag.BalancingHistory = history1;
                }

            }
            else
            {
                if (!String.IsNullOrEmpty(collection["flduic2"]))
                {
                    commonOutwardItemDao.DeleteTempGif(collection["flduic2"].Trim());
                }

                //check next available cheque
                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }


                string beforeBalancingAmountV = "";
                List<AuditTrailOCSModel> beforeBalancingLists = auditTrailOCSDao.ListBalancing(collection["current_flditeminitialid"],collection["current_fldtransno"]);

                foreach (var beforeBalancinglist in beforeBalancingLists)
                {
                    beforeBalancingAmountV = beforeBalancingAmountV + beforeBalancinglist.fldAmount + ",";

                }

                string beforeBalancingAmountC = "";
                List<AuditTrailOCSModel> beforeBalancingListsC = auditTrailOCSDao.ListBalancingC(collection["current_flditemid"], collection["current_fldtransno"]);

                foreach (var beforeBalancinglist in beforeBalancingListsC)
                {
                    beforeBalancingAmountC = beforeBalancingAmountC + beforeBalancinglist.fldAmount + ",";
                }

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItemBalancing(collection["current_fldtransno"]);

                string TransNo = result["fldTransNo"];
                Dictionary<string, string> result1 = commonOutwardItemDao.GetBalancingItemsAsync(TransNo);
                result1.ToList().ForEach(x => result[x.Key] = x.Value);

                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);
                TransactionBalancingDao.DoBalancing(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                string OutwardTransNo = result["fldTransNo"];
                List<BalancingHistoryModel> history1 = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                ViewBag.BalancingHistory = history1;

                string afterBalancingAmountV = "";
                List<AuditTrailOCSModel> afterBalancingLists = auditTrailOCSDao.ListBalancing(collection["current_flditeminitialid"],collection["current_fldtransno"]);
                foreach (var afterBalancinglist in afterBalancingLists)
                {
                    afterBalancingAmountV = afterBalancingAmountV + afterBalancinglist.fldAmount + ",";
                }

                string afterBalancingAmountC = "";
                List<AuditTrailOCSModel> afterBalancingLists_C = auditTrailOCSDao.ListBalancingC(collection["current_flditemid"], collection["current_fldtransno"]);
                foreach (var afterBalancinglist in afterBalancingLists_C)
                {
                    afterBalancingAmountC = afterBalancingAmountC + afterBalancinglist.fldAmount + ",";
                }


                AuditTrailOCSModel after = auditTrailOCSDao.CheckItemBalancing(collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeBalancing_Confirm(before,after,beforeBalancingAmountV,afterBalancingAmountV,
                    beforeBalancingAmountC, afterBalancingAmountC,"Confirm", collection);

                auditTrailOCSDao.AuditTrailOCSLog("Amount Correction", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Balancing] : Confirmed Transaction (" + collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;

            }
            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]) || errorMessages.Count > 0)
            {
                List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(resultModel.allFields["fldItemId"]);
                ViewBag.ChequeRepair = history;
                ViewBag.OutwardItemViewModel = resultModel;
                return View("ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("OutwardClearing/Base/_EmptyChequeVerification");
            }
        }


        public async Task<ActionResult> ChequeBalancingReject(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            string CheckConfirm = "R";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;
            errorMessages = TransactionBalancingDao.Validate(collection, CurrentUser.Account, CheckConfirm);
            Checking = errorMessages.Count;

            if ((Checking > 0))
            {
                result = commonOutwardItemDao.ErrorCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    resultModel = OutwardItemConcern.OutwardItemWithErrorMessages(gQueueSqlConfigSearch, result, errorMessages);
                    string OutwardTransNo = result["fldTransNo"];
                    List<BalancingHistoryModel> history1 = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                    ViewBag.BalancingHistory = history1;
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(collection["flduic2"]))
                {
                    commonOutwardItemDao.DeleteTempGif(collection["flduic2"].Trim());
                }

                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItemRejectBalancing(collection["current_flditemid"], collection["current_fldtransno"]);

                string TransNo = result["fldTransNo"];
                Dictionary<string, string> result1 = commonOutwardItemDao.GetBalancingItemsAsync(TransNo);
                result1.ToList().ForEach(x => result[x.Key] = x.Value);
                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);
                TransactionBalancingDao.Reject(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);
                string OutwardTransNo = result["fldTransNo"];
                List<BalancingHistoryModel> history1 = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                ViewBag.BalancingHistory = history1;

                AuditTrailOCSModel after = auditTrailOCSDao.CheckItemRejectBalancing(collection["current_flditemid"], collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeBalancing_Reject(before, after, "Reject", collection);
                auditTrailOCSDao.AuditTrailOCSLog("Amount Correction", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Balancing] : Reject Transaction (" + collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);


                ViewBag.MinusRecordIndicator = true;
            }

            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]))
            {
                List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(OutwardItemId);
                ViewBag.ChequeRepair = history;
                ViewBag.OutwardItemViewModel = resultModel;
                return View("ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("OutwardClearing/Base/_EmptyChequeVerification");
            }
        }
    }
}