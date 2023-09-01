using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using System;
using INCHEQS.Models.Report.OCS;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security.User;
using INCHEQS.Models.Verification;
using INCHEQS.Security;
//using INCHEQS.Security.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.OCS.Models.ChequeDateAmountEntry;
using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeDateAmountEntry.Base.OutwardClearing;
using INCHEQS.Areas.OCS.Models.AuditTrailOCS;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeDateAmountEntry
{
    public class ChequeDateAmountEntryController : ChequeDateAmountEntryBaseController
    {


        private readonly IChequeDateAmountEntryDao ChequeDateAmountEntryDao;
        string taskIdParam;

        public ChequeDateAmountEntryController(IPageConfigDaoOCS pageConfigDao, IChequeDateAmountEntryDao ChequeDateAmountEntryDao, ICommonOutwardItemDao commonOutwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportServiceOCS reportService, IHostReturnReasonDao hostReturnReasonDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, IAuditTrailOCSDao auditTrailOCSDao)
            : base(pageConfigDao, commonOutwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, userDao, largeAmountDao, systemProfileDao,auditTrailOCSDao)
        {

            this.ChequeDateAmountEntryDao = ChequeDateAmountEntryDao;

            base.chequeVerificationPageHtml = "ChequeVerificationPage";
            base.searchPageHtml = "Index";
            base.searchResultPageHtml = "SearchResultPage";
        }

        protected override string initializeQueueTaskId()
        {
            taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return taskIdParam;
        }


        public async Task<ActionResult> ChequeDateAmountConfirm(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_flditemid"];
            string CheckConfirm = "A";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;


            //if ("Maker".Equals(gQueueSqlConfig.TaskRole))
            //{
            errorMessages = ChequeDateAmountEntryDao.Validate(collection, CurrentUser.Account, CheckConfirm);
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
                }

            }
            else
            {
                if (!String.IsNullOrEmpty(collection["flduic2"]))
                {
                    commonOutwardItemDao.DeleteTempGif(collection["flduic2"].Trim());
                }

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItem(collection["current_flditemid"], collection["current_fldtransno"]);

                ChequeDateAmountEntryDao.Confirm(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                AuditTrailOCSModel after = auditTrailOCSDao.CheckItem(collection["current_flditemid"], collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeAmount_Confirm(before, after, "Confirm", collection);
                auditTrailOCSDao.AuditTrailOCSLog("Cheque Date & Cheque Amount Entry", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Cheque Date & Cheque Amount Entry] : Confirmed Transaction ("+ collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);
                //check next available cheque
                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                    //result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }
                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);
                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;

            }
            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]) || errorMessages.Count > 0)
            {
                //List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(resultModel.allFields["fldItemId"]);
                //ViewBag.ChequeRepair = history;
                ViewBag.OutwardItemViewModel = resultModel;
                return View("ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("OutwardClearing/Base/_EmptyChequeVerification");
            }
        }


        public async Task<ActionResult> ChequeDateAmountReject(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            string CheckConfirm = "R";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;
            errorMessages = ChequeDateAmountEntryDao.Validate(collection, CurrentUser.Account, CheckConfirm);
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
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(collection["flduic2"]))
                {
                    commonOutwardItemDao.DeleteTempGif(collection["flduic2"].Trim());
                }

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItemReject(collection["current_flditemid"], collection["current_fldtransno"]);

                ChequeDateAmountEntryDao.Reject(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                AuditTrailOCSModel after = auditTrailOCSDao.CheckItemReject(collection["current_flditemid"], collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeAmount_Reject(before, after, "Reject", collection);
                auditTrailOCSDao.AuditTrailOCSLog("Cheque Date & Cheque Amount Entry", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Cheque Date & Cheque Amount Entry] : Reject Transaction (" + collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);
                //check next available cheque
                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }
                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);


                ;

                ViewBag.MinusRecordIndicator = true;
            }

            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]))
            {
                //List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(OutwardItemId);
                //ViewBag.ChequeRepair = history;
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