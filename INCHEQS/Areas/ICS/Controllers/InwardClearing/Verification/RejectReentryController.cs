using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.RejectReentry;
using INCHEQS.Models.Report;
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
using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Models.Signature;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Areas.ICS.Models.MICRImage;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.Verification {

    //RejectReentry?rrType=m
    //RejectReentry?rrType=c
    public class RejectReentryController : ICCSBaseController {


        private readonly IRejectReentryDao rejectReentryDao;
        string taskIdParam;

        public RejectReentryController(IPageConfigDao pageConfigDao, IRejectReentryDao rejectReentryDao , ICommonInwardItemDao commonInwardItemDao , ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao , IReportService reportService, IHostReturnReasonDao hostReturnReasonDao, IBankHostStatusKBZDao bankHostStatusKBZDao,IBankHostStatusDao bankHostStatusDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, ISignatureDao signatureDao, INonConformanceFlagDao nonConformanceFlagDao, IPullOutReasonDao pullOutReasonDao, IMICRImageDao micrImageDao) 
            : base(pageConfigDao , commonInwardItemDao, searchPageService, auditTrailDao , sequenceDao, verificationDao, reportService, hostReturnReasonDao, bankHostStatusKBZDao, bankHostStatusDao, userDao, largeAmountDao, systemProfileDao, signatureDao, nonConformanceFlagDao, pullOutReasonDao,micrImageDao) {

            this.rejectReentryDao = rejectReentryDao;

            base.chequeVerificationPageHtml = "ChequeVerificationPage";
            base.searchPageHtml = "InwardClearing/ICCSDefault/Index";
            base.searchResultPageHtml = "InwardClearing/ICCSDefault/SearchResultPage";
        }

        protected override string initializeQueueTaskId() {
            taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");           
            return taskIdParam;
        }

        //RRVerificationSave
        public async Task<ActionResult> RRConfirmMICR(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["current_fldInwardItemId"];
            Dictionary<string, string> result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            List<string> errorMessages = new List<string>();
            Dictionary<string, string> errors = new Dictionary<string, string>();

            int Checking = 0;
            ViewBag.TaskId = staskid;
            ViewBag.DataAction = collection["DataAction"];
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string Message = "";



            errorMessages = rejectReentryDao.Validate(collection, CurrentUser.Account);
            Checking = errorMessages.Count;

            //check if validate contain error
            if ((Checking > 0))
            {
                result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            }
            else
            {
                #region Commented By Ali Make  Maker to Checker

                //If fldCheckerMaker from tblTaskMaster is M = (Maker)
                if ("Maker".Equals(gQueueSqlConfig.TaskRole))
                {
                    Message = rejectReentryDao.CheckerConfirmNew(collection, CurrentUser.Account, gQueueSqlConfig.TaskId, "");


                    sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                    sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

                    //rejectReentryDao.MakerConfirm(collection, CurrentUser.Account, gQueueSqlConfig.TaskId);
                }
                //else if ("Checker".Equals(gQueueSqlConfig.TaskRole))
                //{
                //    rejectReentryDao.CheckerConfirmNew(collection, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //    sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                //    sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");
                //}
                #endregion
                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    // Azim Start 8 June 2021
                    if (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage")
                    {
                        result = commonInwardItemDao.ErrorChequeWithoutLock(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }

                    if (result == null)
                    {
                        //TempData["ErrorMsg"] = errorMessages;
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }

                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    
                    //Azim End
                    //return View("InwardClearing/Base/_EmptyChequeVerification");
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                ViewBag.InwardItemViewModel = resultModel;

                if (Message != "")
                {
                    errors.Add("error", Message);
                    resultModel.errorMessages = errors;
                }

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;


                //}


            }

            // if cheque available or contain error msg.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0 || errors.Count > 0)
            {

                ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                return View("ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
            /*}*/
        }


        public async Task<ActionResult> RepairMICR(FormCollection collection) {
            await initializeBeforeAction();
            string inwardItemId = collection["current_fldInwardItemId"];
            string updateTimestamp = collection["current_fldUpdateTimestamp"];
            Dictionary<string, string> result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
     //List<string> errorMessages = rejectReentryDao.Validate(collection, CurrentUser.Account);

            //check if validate contain error
            //if ((errorMessages.Count > 0)) {
            //    result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
            //    resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            //} else {

                //check next available cheque
                result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }

                //UpdateRecFn
                rejectReentryDao.CheckerRepair(collection, CurrentUser.Account);

                //Insert to cheque history
                commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.RejectReentryApprove, CurrentUser.Account, gQueueSqlConfig.TaskId); //F - History for checker ask to repair

                //Update sequence no
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemHistory"), "tblInwardItemHistory");

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
             //}

            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) /*|| errorMessages.Count > 0*/) {
                ViewBag.InwardItemViewModel = resultModel;
                return View("ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }


    }
}