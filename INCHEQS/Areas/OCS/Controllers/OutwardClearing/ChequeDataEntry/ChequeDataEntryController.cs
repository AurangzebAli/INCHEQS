using INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeDataEntry.Base;
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
using INCHEQS.Areas.OCS.Models.ChequeAmountEntry;
using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Areas.OCS.Models.ChequeDataEntry;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.ChequeDataEntry
{
    public class ChequeDataEntryController : ChequeDataEntryBaseController
    {
        private readonly IChequeDataEntryDao ChequeDataEntryDao;
        string taskIdParam;

        public ChequeDataEntryController(
            IPageConfigDaoOCS pageConfigDao, 
            ICommonOutwardItemDao commonOutwardItemDao, 
            ISearchPageService searchPageService, 
            IAuditTrailDao auditTrailDao, 
            IReportServiceOCS reportService,
            ILargeAmountDao largeAmountDao, 
            ISystemProfileDao systemProfileDao, 
            IChequeDataEntryDao ChequeDataEntryDao) 
            : base(pageConfigDao, commonOutwardItemDao, searchPageService, auditTrailDao, 
                  reportService, largeAmountDao, systemProfileDao, ChequeDataEntryDao)
        {

            this.ChequeDataEntryDao = ChequeDataEntryDao;
            base.chequeVerificationPageHtml = "ChequeDataEntryVerificationPage";
            base.searchPageHtml = "Index";
            base.searchResultPageHtml = "SearchResultPage";
        }

        protected override string initializeQueueTaskId()
        {
            taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return taskIdParam;
        }
        public async Task<ActionResult> ChequeAmountConfirm(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;


            //if ("Maker".Equals(gQueueSqlConfig.TaskRole))
            //{
            errorMessages = ChequeDataEntryDao.Validate(collection, CurrentUser.Account);
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

                //check next available cheque
                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }
                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);

                ChequeDataEntryDao.Confirm(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;

            }
            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemID"]) || errorMessages.Count > 0)
            {
                List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(resultModel.allFields["fldItemID"]);
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


        public async Task<ActionResult> ChequeDataEntryReject(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;
            errorMessages = ChequeDataEntryDao.Validate(collection, CurrentUser.Account);
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

                result = commonOutwardItemDao.NextCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
                }
                resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfigSearch, result, collection);


                ChequeDataEntryDao.Reject(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                ViewBag.MinusRecordIndicator = true;
            }
            


            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["flditemid"]))
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