﻿using INCHEQS.Areas.ICS.Models.HostReturnReason;
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
using INCHEQS.Areas.OCS.Models.MicrRepair;
using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.MicrRepair.Base
{


    public class MicrRepairController : MicrRepairBaseController
    {


        private readonly IMicrRepairDao MicrRepairDao;
        string taskIdParam;

        public MicrRepairController(IPageConfigDaoOCS pageConfigDao, IMicrRepairDao MicrRepairDao, ICommonOutwardItemDao commonOutwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportServiceOCS reportService, IHostReturnReasonDao hostReturnReasonDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao)
            : base(pageConfigDao, commonOutwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, userDao, largeAmountDao, systemProfileDao)
        {

            this.MicrRepairDao = MicrRepairDao;

            base.chequeVerificationPageHtml = "ChequeVerificationPage";
            base.searchPageHtml = "Index";
            base.searchResultPageHtml = "SearchResultPage";
        }

        protected override string initializeQueueTaskId()
        {
            taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            return taskIdParam;
        }


        public async Task<ActionResult> MicrRepairConfirm(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            List<string> errorMessages = new List<string>();
            int Checking = 0;


            //if ("Maker".Equals(gQueueSqlConfig.TaskRole))
            //{
            errorMessages = MicrRepairDao.ValidateMicr(collection, CurrentUser.Account);
            Checking = errorMessages.Count;
            //}
            ////check if validate contain error
            if ((Checking > 0))
            {
                result = commonOutwardItemDao.ErrorCheque(gQueueSqlConfigSearch, collection, CurrentUser.Account);
                resultModel = OutwardItemConcern.OutwardItemWithErrorMessages(gQueueSqlConfigSearch, result, errorMessages);
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

                MicrRepairDao.Confirm(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

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


        public async Task<ActionResult> MicrRepairReject(FormCollection collection)
        {
            await initializeBeforeAction();
            string OutwardItemId = collection["current_fldItemid"];
            Dictionary<string, string> result; //= commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, OutwardItemId);

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


            MicrRepairDao.Reject(collection, CurrentUser.Account, gQueueSqlConfigSearch.TaskId);

            ViewBag.MinusRecordIndicator = true;


            // if cheque available or contain error msg.. render cheque page
            if (!OutwardItemId.Equals(resultModel.allFields["fldItemID"]))
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