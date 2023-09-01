using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Controllers.OutwardClearing.DataEntry.Base;
using INCHEQS.Areas.OCS.Models.AuditTrailOCS;
using INCHEQS.Areas.OCS.Models.ChequeDataEntry;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Helpers;
using INCHEQS.Models.Report.OCS;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.DataEntry
{
    public class DataEntryController : DataEntryBaseController
    {
        private readonly IChequeDataEntryDao ChequeDataEntryDao;
        string taskIdParam;
        
        public DataEntryController(
            IPageConfigDaoOCS pageConfigDao,
            ICommonOutwardItemDao commonOutwardItemDao,
            ISearchPageService searchPageService,
            IAuditTrailDao auditTrailDao,
            IReportServiceOCS reportService,
            ILargeAmountDao largeAmountDao,
            ISystemProfileDao systemProfileDao,
            IAuditTrailOCSDao auditTrailOCSDao,
            IChequeDataEntryDao ChequeDataEntryDao)
            : base(pageConfigDao, commonOutwardItemDao, searchPageService, auditTrailDao,
                  reportService, largeAmountDao, systemProfileDao, ChequeDataEntryDao, auditTrailOCSDao)
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
            string OutwardItemId = collection["current_flditemid"];
            string CheckConfirm = "B";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;

            errorMessages = ChequeDataEntryDao.Validate(collection, CurrentUser.Account, CheckConfirm);
            Checking = errorMessages.Count;

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
                    string iteminitialid = result["flditeminitialid"];
                    Dictionary<string, string> diciteminitialid = new Dictionary<string, string>();
                    //diciteminitialid = commonOutwardItemDao.GetStrListVirtualAcctNumberByItemInitialID(iteminitialid);
                    ViewBag.hfdVItemNoList = collection["hfdVItemNoList"];
                    ViewBag.hfdCItemChqAmtList = collection["hfdCItemChqAmtList"];
                    ViewBag.hfdAccNo = collection["hfdAccNo"];
                    ViewBag.hfdChqAmt = collection["hfdChqAmt"];
                    ViewBag.hfdItemInitialID = result["flditeminitialid"];
                    ViewBag.fldItemId = result["fldItemId"];
                    ViewBag.fldTransno = result["fldtransno"];
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

                string transno = result["fldtransno"];

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItemDataEntry(collection["current_fldtransno"]);
                //List<AuditTrailOCSModel> before = auditTrailOCSDao.CheckItemDataEntry(collection["current_fldtransno"]);

                //collection["current_flditemid"],
                //ChequeDataEntryDao.Confirm(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);
                ChequeDataEntryDao.PreAccountEntry(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                //List<AuditTrailOCSModel> after = auditTrailOCSDao.CheckItemDataEntry(collection["current_fldtransno"]);

                AuditTrailOCSModel after = auditTrailOCSDao.CheckItemDataEntry(collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeDataEntry_Confirm(before, after, "Confirm", collection);

                auditTrailOCSDao.AuditTrailOCSLog("Data Entry", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Creditor Account & Deposit Amount Entry] : Confirmed Transaction (" + collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);

                string iteminitialid = result["flditeminitialid"];
                Dictionary<string, string> diciteminitialid = new Dictionary<string, string>();
                diciteminitialid = commonOutwardItemDao.GetStrListVirtualAcctNumberByItemInitialID(iteminitialid);
                if (diciteminitialid.Count > 0)
                {
                    ViewBag.hfdVItemNoList = diciteminitialid["hfdVItemNoList"];
                    ViewBag.hfdCItemChqAmtList = diciteminitialid["hfdCItemChqAmtList"];
                }
                else
                {
                    ViewBag.hfdVItemNoList = "";
                    ViewBag.hfdCItemChqAmtList = "";
                }
                ViewBag.hfdAccNo = "";
                ViewBag.hfdChqAmt = "";
                ViewBag.hfdItemInitialID = result["flditeminitialid"];
                ViewBag.fldItemId = result["fldItemId"];
                ViewBag.fldTransno = result["fldtransno"];

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;

             



            }
            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]) || errorMessages.Count > 0)
            {
                //List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(resultModel.allFields["fldItemId"]);
                //ViewBag.ChequeRepair = history;
                ViewBag.OutwardItemViewModel = resultModel;
                return View("ChequeDataEntryVerificationPage");
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
            string CheckConfirm = "R";
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;
            errorMessages = ChequeDataEntryDao.Validate(collection, CurrentUser.Account, CheckConfirm);
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

                AuditTrailOCSModel before = auditTrailOCSDao.CheckItemReject(collection["current_flditemid"], collection["current_fldtransno"]);

                ChequeDataEntryDao.Reject(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

                AuditTrailOCSModel after = auditTrailOCSDao.CheckItemReject(collection["current_flditemid"], collection["current_fldtransno"]);
                string ActionDetail = auditTrailOCSDao.ChequeAmount_Reject(before, after, "Reject", collection);
                auditTrailOCSDao.AuditTrailOCSLog("Data Entry", ActionDetail, gQueueSqlConfigSearch.TaskId, collection["current_flditemid"], collection["current_fldtransno"], CurrentUser.Account);
                auditTrailDao.SecurityLog("[Creditor Account & Deposit Amount Entry] : Reject Transaction (" + collection["current_flditemid"] + ")", "", staskid, CurrentUser.Account);

                string iteminitialid = result["flditeminitialid"];
                Dictionary<string, string> diciteminitialid = new Dictionary<string, string>();
                diciteminitialid = commonOutwardItemDao.GetStrListVirtualAcctNumberByItemInitialID(iteminitialid);
                if (diciteminitialid.Count > 0)
                {
                    ViewBag.hfdVItemNoList = diciteminitialid["hfdVItemNoList"];
                    ViewBag.hfdCItemChqAmtList = diciteminitialid["hfdCItemChqAmtList"];
                }
                else
                {
                    ViewBag.hfdVItemNoList = "";
                    ViewBag.hfdCItemChqAmtList = "";
                }
                ViewBag.hfdAccNo = "";
                ViewBag.hfdChqAmt = "";
                ViewBag.hfdItemInitialID = result["flditeminitialid"];
                ViewBag.fldItemId = result["fldItemId"];
                ViewBag.fldTransno = result["fldtransno"];

                ViewBag.MinusRecordIndicator = true;
            }

            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemId"]))
            {
                //List<ChequeRepairHistoryModel> history = await commonOutwardItemDao.GetChequeHistoryAsync(OutwardItemId);
                //ViewBag.ChequeRepair = history;
                ViewBag.OutwardItemViewModel = resultModel;
                return View("ChequeDataEntryVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("OutwardClearing/Base/_EmptyChequeVerification");
            }
        }
    }
}