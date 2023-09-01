using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Report;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Areas.PPS.Models.Verification;
using System.Data;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Models.Signature;

namespace INCHEQS.Areas.PPS.Controllers.Verification
{

    //[CustomAuthorize]
    public abstract class ICCSBaseController : BaseController
    {

        //Must be Injected in constructor by inheriting class. //Check sample DefaultICCSController
        protected readonly ICommonInwardItemDao commonInwardItemDao;
        protected readonly IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly IVerificationDao verificationDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISequenceDao sequenceDao;
        protected readonly IReportService reportService;
        protected readonly IHostReturnReasonDao hostReturnReasonDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ILargeAmountDao largeAmountDao;
        protected readonly IPullOutReasonDao pullOutReasonDao;
        protected readonly UserDao userDao;
        protected readonly ISignatureDao signatureDao;
        protected InwardItemViewModel resultModel;

        //Readables by inheriting class
        protected QueueSqlConfig gQueueSqlConfig { get; private set; }

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

        public ICCSBaseController(IPageConfigDao pageConfigDao,
            ICommonInwardItemDao commonInwardItemDao,
            ISearchPageService searchPageService,
            IAuditTrailDao auditTrailDao,
            ISequenceDao sequenceDao,
            IVerificationDao verificationDao,
            IReportService reportService,
            IHostReturnReasonDao hostReturnReasonDao,
            UserDao userDao,
            ILargeAmountDao largeAmountDao,
            ISystemProfileDao systemProfileDao,
            IPullOutReasonDao pullOutReasonDao,
            ISignatureDao signatureDao
            )
        {
            //Construct
            this.chequeVerificationPageHtml = chequeVerificationPageHtml != null ?
                chequeVerificationPageHtml : "PositivePay/ICCSDefault/ChequeVerificationPage";

            this.chequeRetrieverPageHtml = chequeRetrieverPageHtml != null ?
                chequeRetrieverPageHtml : "PositivePay/ICCSDefault/ChequeRetrieverPage";

            this.searchPageHtml = searchPageHtml != null ?
                searchPageHtml : "PositivePay/ICCSDefault/Index";

            this.searchResultPageHtml = searchResultPageHtml != null ?
                searchResultPageHtml : "PositivePay/ICCSDefault/SearchResultPage";

            this.commonInwardItemDao = commonInwardItemDao;
            this.verificationDao = verificationDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.auditTrailDao = auditTrailDao;
            this.sequenceDao = sequenceDao;
            this.reportService = reportService;
            this.hostReturnReasonDao = hostReturnReasonDao;
            this.userDao = userDao;
            this.largeAmountDao = largeAmountDao;
            this.systemProfileDao = systemProfileDao;
            this.pullOutReasonDao = pullOutReasonDao;
            this.signatureDao = signatureDao;
        }

        /// <summary>
        /// This function should be called inside All Actions in ICCSBaseController and it's Inheritence Controller.
        /// This is to protect from UNAUTHORIZED ACCESS to the page through TASKID returned by setupPageSqlConfig().
        /// This function is important and have to be called or else, the page won't work
        /// Returns: PageSqlConfig set in setupPageSqlConfig();
        /// </summary>
        [NonAction]
        protected async Task<QueueSqlConfig> initializeBeforeAction(string taskId = null)
        {
            //Expose 'currentAction' to Children Controller so that it can be intercepted and add logics accordingly
            //currentAction is action URL accessed
            //currentFormCollection is FormCollection sent to URL accessed
            //The actions are:
            // - Index
            // - SearchResultPage
            // - ChequeVerificationPage
            // - Verification
            // - NextCheque
            // - PrevCheque
            // - ChequeHistory 
            // - Print
            // - Close
            currentAction = currentAction != null ? currentAction : (string)ControllerContext.RouteData.Values["action"];
            currentFormCollection = new FormCollection(ControllerContext.HttpContext.Request.Form);
            staskid = initializeQueueTaskId();

            // add by Shamil 20161225
            // to add dedicated branch for user to CurrentAccount BranchCodes in task 1st,2nd,3rd verification
            // assign here because in tblQueueConfig get validation 
            // update by shamil/kai hong 20170616
            // to add dedicated branch for user to CurrentAccount BranchCodes in task new 1st,2nd,3rd verification

            if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308160" || staskid == "308140" || staskid == "306550" || (staskid == "308110") || (staskid == "308120") || (staskid == "308150") || (staskid == "308250") || (staskid == "308260"))
            {
                //CurrentUser.Account.BranchHubCodes = verificationDao.VerificationCondition(CurrentUser.Account.UserAbbr, staskid).ToArray();
                gQueueSqlConfig = await pageConfigDao.GetQueueConfigAsyncNew(staskid, CurrentUser.Account);
            }
            else
            {
                //if (staskid == "306220" || staskid == "306230" || staskid == "306240")
                //{
                //    CurrentUser.Account.BranchHubCodes = verificationDao.VerificationCondition(CurrentUser.Account.UserAbbr, staskid).ToArray();
                //}
                gQueueSqlConfig = await pageConfigDao.GetQueueConfigAsync(staskid, CurrentUser.Account);

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
                    & !"ChequeVerificationPage".Equals(currentAction)
                    & !"ChequeRetrieverPage".Equals(currentAction)
                    & !"NextCheque".Equals(currentAction)
                    & !"PrevCheque".Equals(currentAction)
                    & !"ChequeHistory".Equals(currentAction)
                    & !"MatchingDetails".Equals(currentAction)
                    & !"PrintCheque".Equals(currentAction)
                    & !"PrintChequeRetriever".Equals(currentAction)
                    & !"PrintAll".Equals(currentAction)
                    & !"PrintSummary".Equals(currentAction)
                    & !"Close".Equals(currentAction)
                    & !"ProceedApproveOrReturnAll".Equals(currentAction)
                    & !"PullOutReason".Equals(currentAction)
                    & !"Find".Equals(currentAction)
                    & !CurrentUser.HasAccessToUrlNotUnique(currentAction))
                {
                    RequestHelper.RejectAccessToLoginPage(ControllerContext);
                }
            }
            catch (HttpException ex)
            {
                systemProfileDao.Log("ICCSBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
            //------ UNAUTHORIZED ACCESS ---------------

            return gQueueSqlConfig;
        }

        public virtual async Task<ActionResult> Index()
        {
            await initializeBeforeAction();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, gQueueSqlConfig.toPageSqlConfig());
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

                checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldClearDate"]);
                if (checkitem == 0)
                {
                    //commonInwardItemDao.UnlockAllAssignedForUserHistory(CurrentUser.Account);
                    gQueueSqlConfig.ViewOrTableName = gQueueSqlConfig.ViewOrTableName + "H";
                }
            }
            else if (staskid == "308110" || staskid == "308120" || staskid == "308130" || staskid == "308160" || (staskid == "308250") || (staskid == "308260") || staskid == "308140")
            {
                commonInwardItemDao.UnlockAllAssignedForBranchUser(CurrentUser.Account);
            }
            else
            {
                commonInwardItemDao.UnlockAllAssignedForUser(CurrentUser.Account);
            }

            ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);

            return View(searchResultPageHtml);
        }

        //VerificationInfoCompare.asp
        public virtual async Task<ActionResult> ChequeVerificationPage(FormCollection collection)
        {
            await initializeBeforeAction();
            Boolean checkResult = true;
            Dictionary<string, string> result;
            Boolean checkResultBranch = true;
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string inwardItemId = filter["fldInwardItemId"];
            string taskid = gQueueSqlConfig.TaskId;
            Boolean unlock = true;
            //Prompt check is locked by other user
            checkResult = commonInwardItemDao.CheckStatus(inwardItemId, CurrentUser.Account);
            checkResultBranch = commonInwardItemDao.CheckStatusBranch(inwardItemId, CurrentUser.Account);

            collection.Add("DataAction", "ChequeVerificationPage");
            
            //Lock this record for currentuser
            if (CurrentUser.Account.UserType == "Branch" && checkResultBranch == true)
            {
                commonInwardItemDao.LockThisChequeBranch(inwardItemId, CurrentUser.Account);
                result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            }
            else if (checkResult == true)
            {
                commonInwardItemDao.LockThisCheque(inwardItemId, CurrentUser.Account);
                result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            }
            else
            {
                unlock = false;
                result = commonInwardItemDao.FindItemByInwardItemIdNOLOCKNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            }

            if (result == null)
            {
                //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                return View("PositivePay/Base/_EmptyChequeVerificationReminder");
            }

            InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);

            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
            ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
            ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
            ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
            ViewBag.InwardItemViewModel = resultModel;
            //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
            //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            ViewBag.LockCheck = checkResult.ToString();
            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            ViewBag.LockCheckBranch = checkResultBranch.ToString();
            //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
            //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
            //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
            //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
            ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.Micr = commonInwardItemDao.GetMicr();
            ViewBag.ChequeInfo = verificationDao.getChequeInfo(inwardItemId, resultModel.allFields["fldUIC"]);
            ViewBag.TaskId = gQueueSqlConfig.TaskId;
            ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(inwardItemId);
            ViewBag.DataAction = "ChequeVerificationPage";

            //if (CurrentUser.Account.UserType == "Branch" && checkResultBranch == true)
            //{
            //    commonInwardItemDao.LockThisChequeBranch(inwardItemId, CurrentUser.Account);
            //}

            return View(chequeVerificationPageHtml);
        }

        //public virtual async Task<ActionResult> Find(FormCollection collection)
        //{
        //    await initializeBeforeAction();
        //    ViewBag.Payee = verificationDao.FindPayee(collection, collection["BankCode"]);
        //    await ChequeVerificationPage(collection);
        //    return View(chequeVerificationPageHtml);

        //}

        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue
            };
        }

        public async Task<JsonResult> Find(FormCollection collection)
        {
            List<VerificationModel> findPayee = await verificationDao.FindPayeeAsync(collection, collection["bankCode"]);
            return Json(findPayee, JsonRequestBehavior.AllowGet);
        }

        public virtual async Task<ActionResult> ChequeRetrieverPage(FormCollection collection)
        {
            await initializeBeforeAction();

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string inwardItemId = filter["fldInwardItemId"];
            string taskid = gQueueSqlConfig.TaskId;
            int checkitem = 1;
            Dictionary<string, string> result;
            //Lock this record for currentuser
            if ((taskid == "301120") || (taskid == "301130"))
            {
                checkitem = commonInwardItemDao.CheckMainTableInward(CurrentUser.Account, inwardItemId);
                if (checkitem == 0)
                {
                    commonInwardItemDao.LockThisChequeHistory(inwardItemId, CurrentUser.Account);
                    gQueueSqlConfig.ViewOrTableName = gQueueSqlConfig.ViewOrTableName + "H";
                    result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId, null);

                }
                else
                {
                    result = commonInwardItemDao.FindInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId, null);

                }

            }
            else
            {
                result = commonInwardItemDao.FindInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId, null);

            }

            if (result == null)
            {
                //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                return View("PositivePay/Base/_EmptyChequeVerificationReminder");
            }

            InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
            ViewBag.InwardItemViewModel = resultModel;
            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
            ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
            ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
            ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
            ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
            ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
            ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
            ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
            ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            ViewBag.ImgRetrieve = "Y";
            return View(chequeRetrieverPageHtml);
        }


        public virtual async Task<ActionResult> LockVerification(FormCollection collection)
        {
            systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": Start LockVerification", CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            await initializeBeforeAction();
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string inwardItemId = filter["fldInwardItemId"];
            ViewBag.DataAction = "LockVerification";
            // add by kai hong 20170616
            // New Verification,1st,2nd,3rd,branch maker/checker verification will call new simplified function
            if (staskid == "306910" || staskid == "306930" || staskid == "306920" || staskid == "306550" || staskid == "306210" || staskid == "318190" || staskid == "318200"
                 || staskid == "318280" || staskid == "318290" || staskid == "318350" || staskid == "318360" || staskid == "318390" || staskid == "318400" || staskid == "318410")
            {
                Dictionary<string, string> result = commonInwardItemDao.LockAllChequeNew(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification", /*CurrentUser.Account.UserAbbr*/  CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                else
                {
                    InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                    ViewBag.TaskId = gQueueSqlConfig.TaskId;
                }

            }
            else if (staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308160" || staskid == "308250" || staskid == "308150")
            {
                Dictionary<string, string> result = commonInwardItemDao.LockAllChequeBranch(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification", /*CurrentUser.Account.UserAbbr*/ CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                else
                {
                    InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                    ViewBag.TaskId = gQueueSqlConfig.TaskId;
                }

            }
            else
            {
                Dictionary<string, string> result = commonInwardItemDao.LockAllCheque(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                else
                {
                    InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();

                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                    ViewBag.TaskId = gQueueSqlConfig.TaskId;
                }

            }
            //Just initial total record

            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            ViewBag.LockIndicator = true;
            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.Micr = commonInwardItemDao.GetMicr();
            systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
            return View(chequeVerificationPageHtml);
        }

        //ICCSNextRecord
        public virtual async Task<ActionResult> NextCheque(FormCollection collection)
        {
            await initializeBeforeAction();
            //string uic = collection["fldUIC2"];
            // add by shamil 20170616
            // Delete temp gif when see next check
            commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            ViewBag.DataAction = collection["DataAction"];
            // add by kai hong 20170616
            // New Verification,1st,2nd,3rd verification will call new simplified function
            if (staskid == "308160" || staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "306550" || staskid == "308110" || staskid == "308120"
                || staskid == "318190" || staskid == "318200" || staskid == "318280" || staskid == "318290" || staskid == "318350" || staskid == "318360" || staskid == "318390" || staskid == "318410")
            {
                Dictionary<string, string> result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                else
                {
                    InwardItemViewModel resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                    //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    //ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                }

            }
            else
            {
                //commonInwardItemDao.LockThisCheque(collection["fldInwardItemId"], CurrentUser.Account);
                Dictionary<string, string> result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                InwardItemViewModel resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();

            }
            ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.Micr = commonInwardItemDao.GetMicr();
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            return View(chequeVerificationPageHtml);
        }

        //ICCSPrevRecord
        public virtual async Task<ActionResult> PrevCheque(FormCollection collection)
        {
            await initializeBeforeAction();
            // add by shamil 20170616
            // Delete temp gif when see next check
            commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            ViewBag.DataAction = collection["DataAction"];
            // add by kai hong 20170616
            // New Verification,1st,2nd,3rd,branch maker/checker verification will call new simplified function
            if (staskid == "308160" || staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120"
                || staskid == "318190" || staskid == "318200" || staskid == "318280" || staskid == "318290" || staskid == "318350" || staskid == "318360" || staskid == "318390" || staskid == "318210" || staskid == "318220" || staskid == "318410")
            {

                Dictionary<string, string> result;
                if (ViewBag.DataAction.ToString().Trim() == "ChequeVerificationPage")
                {
                    result = commonInwardItemDao.PrevChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                else
                {
                    result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                //Dictionary<string, string> result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                else
                {
                    InwardItemViewModel resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                    //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    //    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                    //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    //    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    //    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                }

            }
            else
            {
                Dictionary<string, string> result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                InwardItemViewModel resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            }
            ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.Micr = commonInwardItemDao.GetMicr();
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            return View(chequeVerificationPageHtml);
        }

        //ICCSHistory
        public virtual async Task<ActionResult> ChequeHistory(FormCollection collection)
        {
            await initializeBeforeAction();
            int checkitem = 1;
            if ((staskid == "301120") || (staskid == "301130"))
            {
                checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldClearDate"]);
                if (checkitem == 0)
                {
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistoryH(collection["fldInwardItemId"].ToString());
                    return PartialView("PositivePay/Modal/_ChequeHistoryPopup");
                }
                else
                {
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                    return PartialView("PositivePay/Modal/_ChequeHistoryPopup");
                }
            }
            else
            {
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                //auditTrailDao.SecurityLog("[" + Module + "] : Retrieve audit log ", "", staskid, CurrentUser.Account);
                return PartialView("PositivePay/Modal/_ChequeHistoryPopup");
            }

        }

        public virtual async Task<ActionResult> MatchingDetails(FormCollection collection)
        {
            await initializeBeforeAction();


            ViewBag.ScanningComponent = verificationDao.MatchingDetails(collection["fldInwardItemId"].ToString(), "Scanning");
            ViewBag.PPSFile = verificationDao.MatchingDetails(collection["fldInwardItemId"].ToString(), "PPSFile");

            return PartialView("PositivePay/Modal/_MatchingDetailsPopup");
        }


        public virtual async Task<ActionResult> PrintCheque(FormCollection collection)
        {
            await initializeBeforeAction();

            ReportModel reportModel = await reportService.GetReportConfigByTaskIdAsync(TaskIds.PrintChequePPS);
            string reportPath = reportModel.reportPath;

            //Search Report Path in Project Or In Any Folder specified
            if (!System.IO.File.Exists(reportPath))
            {
                reportPath = Server.MapPath(reportModel.reportPath);
                if (!System.IO.File.Exists(reportPath))
                {
                    return View("PositivePay/Modal/_EmptyPrint");
                }
            }

            string fileNameExtention = "pdf"; string mimeType;
            byte[] renderedBytes = reportService.renderChequeReportWithImageBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

            Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
            auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
            return File(renderedBytes, mimeType);
        }

        public virtual async Task<ActionResult> PrintChequeRetriever(FormCollection collection)
        {
            await initializeBeforeAction();

            ReportModel reportModel = await reportService.GetReportConfigByTaskIdAsync(TaskIds.PrintChequeRetriever);
            string reportPath = reportModel.reportPath;

            //Search Report Path in Project Or In Any Folder specified
            if (!System.IO.File.Exists(reportPath))
            {
                reportPath = Server.MapPath(reportModel.reportPath);
                if (!System.IO.File.Exists(reportPath))
                {
                    return View("PositivePay/Modal/_EmptyPrint");
                }
            }

            string fileNameExtention = "pdf"; string mimeType;
            byte[] renderedBytes = reportService.renderChequeReportWithImageBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

            Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
            auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
            return File(renderedBytes, mimeType);
        }

        //public virtual async Task<ActionResult> PrintAll(FormCollection collection)
        //{
        //    await initializeBeforeAction();

        //    ReportModel reportModel = await reportService.GetReportConfigAsync(gQueueSqlConfig.toPageSqlConfig());
        //    string reportPath = reportModel.reportPath;

        //    //Search Report Path in Project Or In Any Folder specified
        //    if (!System.IO.File.Exists(reportPath))
        //    {
        //        reportPath = Server.MapPath(reportModel.reportPath);
        //        if (!System.IO.File.Exists(reportPath))
        //        {
        //            return View(chequeVerificationPageHtml);
        //        }
        //    }

        //    string fileNameExtention = "pdf";
        //    string mimeType;
        //    byte[] renderedBytes = reportService.renderReportBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

        //    Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
        //    auditTrailDao.Log("Generate Report '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
        //    return File(renderedBytes, mimeType);
        //}


        //Action Close can be defaulted by any action
        public virtual async Task<ActionResult> Close(FormCollection collection)
        {
            await initializeBeforeAction();

            // add by shamil 20170616
            // Delete temp gif when see next check
            if (!String.IsNullOrEmpty(collection["fldUIC2"]))
            {
                commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            }

            return PartialView("PositivePay/Modal/_ClosePopup");
        }


        public virtual async Task<ActionResult> ApproveAll(FormCollection collection)
        {
            await initializeBeforeAction();
            ViewBag.TaskRole = gQueueSqlConfig.TaskRole;
            return PartialView("PositivePay/Modal/_PasswordPopup");
        }

        public virtual async Task<ActionResult> ReturnAll(FormCollection collection)
        {
            await initializeBeforeAction();
            ViewBag.TaskRole = gQueueSqlConfig.TaskRole;
            return PartialView("PositivePay/Modal/_PasswordPopup");
        }

        //public virtual async Task<JsonResult> ProceedApproveOrReturnAll(FormCollection collection) {
        //    await initializeBeforeAction();
        //    string notice = "";
        //    string password = collection["proceedPassword"];
        //    string userPassword = userDao.GetUser(CurrentUser.Account.UserId).fldPassword;

        //    if (password.Equals(userPassword)) {
        //        if ("ApproveAll".Equals(gQueueSqlConfig.TaskRole)) {

        //            //Insert to cheque history
        //            commonInwardItemDao.InsertChequeHistoryForApproveOrRejectAll("A", CurrentUser.Account, gQueueSqlConfig.TaskId);
        //            //Do process
        //            verificationDao.VerificationApproveAll(CurrentUser.Account);
        //            //Audit Trail
        //            auditTrailDao.Log("Verification - Approve All", CurrentUser.Account);
        //            notice = "Approve All Success";
        //        } else if ("ReturnAll".Equals(gQueueSqlConfig.TaskRole)) {

        //            if (!String.IsNullOrEmpty(collection["rejectCode"])) {
        //                //Insert to cheque history
        //                commonInwardItemDao.InsertChequeHistoryForApproveOrRejectAll("R", CurrentUser.Account, gQueueSqlConfig.TaskId, collection["rejectCode"]);
        //                //Do process
        //                verificationDao.VerificationReturnAll(CurrentUser.Account);
        //                //Audit Trail
        //                auditTrailDao.Log("Verification - Return All", CurrentUser.Account);
        //                notice = "Return All Success";

        //            }else {
        //                notice = "ERROR :Reject Code Required";
        //            }
        //        }
        //    }else {
        //        notice = "ERROR : Wrong Password";
        //    }

        //    return Json(new {notice = notice}, JsonRequestBehavior.AllowGet);
        //}


        public virtual async Task<ActionResult> PullOutReason(FormCollection collection)
        {
            await initializeBeforeAction();
            DataTable pullOutReaseonTable = pullOutReasonDao.ListAllPullOutReason();
            List<PullOutReasonModel> pullOutReasonModels = new List<PullOutReasonModel>();

            foreach (DataRow row in pullOutReaseonTable.Rows)
            {
                PullOutReasonModel pullOutReasonModel = new PullOutReasonModel()
                {
                    pullOutId = row["fldPullOutID"].ToString(),
                    pullOutDesc = row["fldPullOutReason"].ToString()
                };
                pullOutReasonModels.Add(pullOutReasonModel);
            }
            ViewBag.FormCollection = collection;
            ViewBag.PullOutReason = pullOutReasonModels;

            return PartialView("VerificationPullOut");
        }
    }
}