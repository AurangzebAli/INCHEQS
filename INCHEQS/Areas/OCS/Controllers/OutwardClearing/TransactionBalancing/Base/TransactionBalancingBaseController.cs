using INCHEQS.Areas.OCS.Concerns;
using INCHEQS.Areas.OCS.Models.Balancing;
using INCHEQS.Areas.OCS.Models.ChequeDataEntry;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Areas.OCS.ViewModels;
using INCHEQS.Common.Resources;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Helpers;
using INCHEQS.Models.Report;
using INCHEQS.Models.Report.OCS;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Models.Verification;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.Balancing.Base
{
    public abstract class TransactionBalancingBaseController : BaseController
    {
        //Must be Injected in constructor by inheriting class. //Check sample DefaultICCSController
        protected readonly ICommonOutwardItemDao commonOutwardItemDao;
        protected readonly IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IVerificationDao verificationDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISequenceDao sequenceDao;
        protected readonly IReportServiceOCS reportService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ILargeAmountDao largeAmountDao;
        protected OutwardItemViewModel resultModel;
        private readonly ITransactionBalancingDao TransactionBalancingDao;
        //Readables by inheriting class
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }
        protected QueueSqlConfig gQueueSqlConfigSearch { get; private set; }

        //Must be implemented by inheriting class
        protected abstract string initializeQueueTaskId();

        //Overridables by inheriting class
        protected string chequeRetrieverPageHtml;
        protected string chequeVerificationPageHtml;
        protected string searchPageHtml;
        protected string searchResultPageHtml;
        protected string staskid;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;

        public TransactionBalancingBaseController(
            IPageConfigDaoOCS pageConfigDao,
            ICommonOutwardItemDao commonOutwardItemDao,
            ISearchPageService searchPageService,
            IAuditTrailDao auditTrailDao,
            IReportServiceOCS reportService,
            ILargeAmountDao largeAmountDao,
            ISystemProfileDao systemProfileDao,
            ITransactionBalancingDao TransactionBalancingDao
            )
                {
                    //Construct
                    this.chequeVerificationPageHtml = chequeVerificationPageHtml != null ? chequeVerificationPageHtml : "OutwardClearing/OCSDefault/TransactionBalancingVerificationPage";
                    this.chequeRetrieverPageHtml = chequeRetrieverPageHtml != null ? chequeRetrieverPageHtml : "OutwardClearing/OCSDefault/ChequeRetrieverPage";
                    this.searchPageHtml = searchPageHtml != null ? searchPageHtml : "OutwardClearing/OCSDefault/Index";
                    this.searchResultPageHtml = searchResultPageHtml != null ? searchResultPageHtml : "OutwardClearing/OCSDefault/SearchResultPage";

                    this.commonOutwardItemDao = commonOutwardItemDao;
                    this.pageConfigDao = pageConfigDao;
                    this.searchPageService = searchPageService;
                    this.auditTrailDao = auditTrailDao;
                    this.reportService = reportService;
                    this.largeAmountDao = largeAmountDao;
                    this.systemProfileDao = systemProfileDao;
                    this.TransactionBalancingDao = TransactionBalancingDao;

                }
        [NonAction]
        protected async Task<QueueSqlConfig> initializeBeforeAction(string taskId = null)
        {
            //Expose 'currentAction' to Children Controller so that it can be intercepted and add logics accordingly
            //currentAction is action URL accessed
            //currentFormCollection is FormCollection sent to URL accessed
            //The actions are:
            // - Index
            // - SearchResultPage
            // - TransactionBalancingVerificationPage
            // - Verification
            // - NextCheque
            // - PrevCheque
            // - ChequeHistory
            // - Print
            // - Close
            currentAction = currentAction != null ? currentAction : (string)ControllerContext.RouteData.Values["action"];
            currentFormCollection = new FormCollection(ControllerContext.HttpContext.Request.Form);
            staskid = initializeQueueTaskId();
            ViewBag.taskid = staskid;
            // add by Shamil 20161225
            // to add dedicated branch for user to CurrentAccount BranchCodes in task 1st,2nd,3rd verification
            // assign here because in tblQueueConfig get validation 
            // update by shamil/kai hong 20170616
            // to add dedicated branch for user to CurrentAccount BranchCodes in task new 1st,2nd,3rd verification
            if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "306550" || (staskid == "308110") || (staskid == "308120") || (staskid == "308150"))
            {
                //CurrentUser.Account.BranchHubCodes = verificationDao.VerificationCondition(CurrentUser.Account.UserAbbr, staskid).ToArray();
                gQueueSqlConfig = await pageConfigDao.GetQueueConfigAsyncNew(staskid, CurrentUser.Account);
            }
            else
            {
                gQueueSqlConfig = await pageConfigDao.GetQueueConfigAsync(staskid, CurrentUser.Account);
                gQueueSqlConfigSearch = await pageConfigDao.GetQueueConfigAsync(staskid, CurrentUser.Account);
            }
            //Initializ PageSqlConfig based on: 
            // - Inherited Controller initialization of initializeQueueTaskId()
            // - Request Query String of TaskId and ViewId

            //ViewBagForCheckerMaker
            ViewBag.TaskRole = gQueueSqlConfig.TaskRole;

            //ViewBagForAllowedAction
            ViewBag.AllowedAction = gQueueSqlConfig.AllowedActions;

            //ViewBag Exclusively for master verification
            ViewBag.DatabaseViewName = gQueueSqlConfig.ViewOrTableName;
            ViewBag.StoreProc = gQueueSqlConfig.StoreProcedure;
            //Set Page Title Name
            ViewBag.PageTitle = gQueueSqlConfig.PageTitle;

            //---- UNAUTHORIZED ACCESS  ----------------
            //Reject if User Does Not Have Access               
            try
            {
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, gQueueSqlConfig.TaskId);

                //Allow access only at index if user access by task id. 
                //THis can be configured in tblQueueConfig. 
                if (!"Index".Equals(currentAction)
                    & !"SearchResultPage".Equals(currentAction)
                    & !"TransactionBalancingVerificationPage".Equals(currentAction)
                    & !"ChequeRetrieverPage".Equals(currentAction)
                    & !"NextCheque".Equals(currentAction)
                    & !"PrevCheque".Equals(currentAction)
                    & !"ChequeHistory".Equals(currentAction)
                    & !"PrintCheque".Equals(currentAction)
                    & !"PrintChequeRetriever".Equals(currentAction)
                    & !"PrintAll".Equals(currentAction)
                    & !"PrintSummary".Equals(currentAction)
                    & !"Close".Equals(currentAction)
                    & !"ProceedApproveOrReturnAll".Equals(currentAction)
                    & !CurrentUser.HasAccessToUrlNotUnique(currentAction))
                {
                    RequestHelper.RejectAccessToLoginPage(ControllerContext);
                }
            }
            catch (HttpException ex)
            {
                systemProfileDao.Log("TransactionBalancingBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
            //------ UNAUTHORIZED ACCESS ---------------

            return gQueueSqlConfig;
        }

        // GET: OCS/ChequeDataEntryBase
        public virtual async Task<ActionResult> Index()
        {
            await initializeBeforeAction();
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, gQueueSqlConfigSearch.toPageSqlConfig());
            return View(searchPageHtml);
        }
        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection)
        {
            await initializeBeforeAction();
            //string sqlCondition = "";
            int checkitem = 1;
            // Image retrieval search
            if ((staskid == "301120") || (staskid == "301130"))
            {
                checkitem = commonOutwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldcleardate"]);
                if (checkitem == 0)
                {
                    gQueueSqlConfigSearch.ViewOrTableName = gQueueSqlConfigSearch.ViewOrTableName + "H";
                }
            }
            else
            {
                commonOutwardItemDao.UnlockAllAssignedForUser(CurrentUser.Account);
            }

            ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfigSearch, collection);
            return View(searchResultPageHtml);
        }

        public virtual async Task<ActionResult> TransactionBalancingVerificationPage(FormCollection collection)
        {
            await initializeBeforeAction();
            Boolean checkResult = true;
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string OutwardItemId = filter["fldItemID"];
            //Prompt check is locked by other user
            checkResult = commonOutwardItemDao.CheckStatus(OutwardItemId, CurrentUser.Account);
            //Lock this record for currentuser
            commonOutwardItemDao.LockThisCheque(OutwardItemId, CurrentUser.Account);
            Dictionary<string, string> result = commonOutwardItemDao.FindItemByOutwardItemId(gQueueSqlConfigSearch, collection, CurrentUser.Account, OutwardItemId);
            OutwardItemViewModel resultModel = OutwardItemConcern.ChequePagePopulateViewModelOCS(gQueueSqlConfigSearch, result);
            ViewBag.OutwardItemViewModel = resultModel;
            string OutwardTransNo = result["fldtransno"];
            List<BalancingHistoryModel> history = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
            ViewBag.BalancingHistory = history;
            return View(chequeVerificationPageHtml);
        }
        public virtual async Task<ActionResult> ChequeBalancingQueue(FormCollection collection)
        {
            systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": Start Balancing LockVerification", CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            await initializeBeforeAction();

            Dictionary<string, string> result = commonOutwardItemDao.LockAllChequeNew(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
            if (result == null)

            {
                systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End  Data Entry LockVerification", /*CurrentUser.Account.UserAbbr*/  CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                return View("OutwardClearing/Base/_EmptyChequeVerification");

            }
            else
            {
                OutwardItemViewModel resultModel = OutwardItemConcern.ChequePagePopulateViewModelOCS(gQueueSqlConfig, result);
                ViewBag.OutwardItemViewModel = resultModel;
                ViewBag.IQA = resultModel.getField("fldiqa").Trim();
                ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                ViewBag.MICRCorrection = resultModel.getField("fldmicrcorrectionind").Trim();
                ViewBag.MICRCrrectionDesc = resultModel.getField("micrcorrectiondesc").Trim();
                ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
            }

            string OutwardTransNo = result["fldtransno"];
            List<BalancingHistoryModel> history = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
            ViewBag.BalancingHistory = history;

            ViewBag.LockIndicator = true;
            systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End Balancing LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            return View(chequeVerificationPageHtml);
        }
        //ICCSNextRecord
        public virtual async Task<ActionResult> NextCheque(FormCollection collection)
        {
            await initializeBeforeAction();
            commonOutwardItemDao.DeleteTempGif(collection["flduic2"].Trim());
            // add by kai hong 20170616
            // New Verification,1st,2nd,3rd verification will call new simplified function
            if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "306550" || staskid == "308110" || staskid == "308120")
            {
                Dictionary<string, string> result = commonOutwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    OutwardItemViewModel resultModel = OutwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.OutwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                }

            }
            else
            {
                if (!string.IsNullOrWhiteSpace(collection["htoUpdateBalancing"].Trim()))
                {
                    if (collection["htoUpdateBalancing"].Contains("|"))
                    {
                        string[] TotalValues = collection["htoUpdateBalancing"].Split('|');
                        foreach (var item in TotalValues)
                        {
                            string[] ParamValues = item.Split(',');
                            TransactionBalancingDao.UpdateBalancingAmount(collection, CurrentUser.Account, ParamValues);
                            ViewBag.MinusRecordIndicator = true;

                        }
                    }
                    else
                    {
                        string[] ParamValues = collection["htoUpdateBalancing"].Split(',');
                        TransactionBalancingDao.UpdateBalancingAmount(collection, CurrentUser.Account, ParamValues);
                        ViewBag.MinusRecordIndicator = true;
                    }

                }
                Dictionary<string, string> result = commonOutwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null || result["totalunverified"] == "0")
                {
                    //update balancingstatus to 0 if no next item
                    if (!string.IsNullOrWhiteSpace(collection["htoUpdateBalancing"].Trim()))
                    {
                        TransactionBalancingDao.updateBalancingStatus(collection, CurrentUser.Account);
                    }
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    OutwardItemViewModel resultModel = OutwardItemConcern.NextChequePopulateViewModelOCS(gQueueSqlConfig, result, collection);
                    ViewBag.OutwardItemViewModel = resultModel;
                    string OutwardTransNo = result["fldtransno"];
                    List<BalancingHistoryModel> history = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                    ViewBag.BalancingHistory = history;
                }
                //update balancingstatus to 0
                if (!string.IsNullOrWhiteSpace(collection["htoUpdateBalancing"].Trim()))
                {
                    TransactionBalancingDao.updateBalancingStatus(collection, CurrentUser.Account);
                }
            }
            return View(chequeVerificationPageHtml);
        }

        //ICCSPrevRecord
        public virtual async Task<ActionResult> PrevCheque(FormCollection collection)
        {
            await initializeBeforeAction();
            // add by shamil 20170616
            // Delete temp gif when see next check
            commonOutwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            // add by kai hong 20170616
            // New Verification,1st,2nd,3rd,branch maker/checker verification will call new simplified function
            if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120")
            {
                Dictionary<string, string> result = commonOutwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    OutwardItemViewModel resultModel = OutwardItemConcern.PrevChequePopulateViewModelOCS(gQueueSqlConfig, result, collection);
                    ViewBag.OutwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                }

            }
            else
            {
                Dictionary<string, string> result = commonOutwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("OutwardClearing/Base/_EmptyChequeVerification");
                }
                else
                {
                    OutwardItemViewModel resultModel = OutwardItemConcern.PrevChequePopulateViewModelOCS(gQueueSqlConfig, result, collection);
                    ViewBag.OutwardItemViewModel = resultModel;
                    string OutwardTransNo = result["fldtransno"];
                    List<BalancingHistoryModel> history = await commonOutwardItemDao.GetBalancingHistoryAsync(OutwardTransNo);
                    ViewBag.BalancingHistory = history;
                }


            }
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            return View(chequeVerificationPageHtml);
        }

        ////ICCSHistory
        public virtual async Task<ActionResult> ChequeHistory(FormCollection collection)
        {
            await initializeBeforeAction();
            int checkitem = 1;
            if ((staskid == "301120") || (staskid == "301130"))
            {
                checkitem = commonOutwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldClearDate"]);
                if (checkitem == 0)
                {
                    ViewBag.ChequeHistory = commonOutwardItemDao.ChequeHistoryH(collection);
                    return PartialView("OutwardClearing/Modal/_ChequeHistoryPopup");
                }
                else
                {
                    ViewBag.ChequeHistory = commonOutwardItemDao.ChequeHistory(collection);
                    return PartialView("OutwardClearing/Modal/_ChequeHistoryPopup");
                }
            }
            else
            {
                ViewBag.ChequeHistory = commonOutwardItemDao.ChequeHistory(collection);
                return PartialView("OutwardClearing/Modal/_ChequeHistoryPopup");
            }

        }

        public virtual async Task<ActionResult> PrintCheque(FormCollection collection)
        {
            await initializeBeforeAction();

            ReportModel reportModel = await reportService.GetReportConfigByTaskIdAsync(TaskIdsOCS.PrintChequeOCS);
            string reportPath = reportModel.reportPath;

            //Search Report Path in Project Or In Any Folder specified
            if (!System.IO.File.Exists(reportPath))
            {
                reportPath = Server.MapPath(reportModel.reportPath);
                if (!System.IO.File.Exists(reportPath))
                {
                    return View("OutwardClearing/Modal/_EmptyPrint");
                }
            }

            string fileNameExtention = "pdf"; string mimeType;
            byte[] renderedBytes = reportService.PrintChequeConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);


            Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
           // auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
            auditTrailDao.SecurityLog("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", staskid, CurrentUser.Account);

            return File(renderedBytes, mimeType);
        }
        //Action Close can be defaulted by any action
        public virtual async Task<ActionResult> Close(FormCollection collection)
        {
            await initializeBeforeAction();

            // add by shamil 20170616
            // Delete temp gif when see next check
            if (!String.IsNullOrEmpty(collection["fldUIC2"]))
            {
                commonOutwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            }

            return PartialView("OutwardClearing/Modal/_ClosePopup");
        }

    }
}