using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Helpers;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Report;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Models.Signature;
using INCHEQS.Models.Verification;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.User;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.BoPServices;
using System.Data;
using INCHEQS.Areas.ICS.Models.MICRImage;
using System.Globalization;
using Elmah;
using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Models.RejectReentry;

namespace INCHEQS.Areas.ICS.Controllers
{

    namespace InwardClearing
    {

        //[CustomAuthorize]
        public abstract class ICCSBaseController : BaseController {

            //Must be Injected in constructor by inheriting class. //Check sample DefaultICCSController
            protected readonly ICommonInwardItemDao commonInwardItemDao;
            protected readonly IPageConfigDao pageConfigDao;
            protected readonly ISearchPageService searchPageService;
            protected readonly IVerificationDao verificationDao;
            protected readonly IAuditTrailDao auditTrailDao;
            protected readonly ISequenceDao sequenceDao;
            protected readonly IReportService reportService;
            protected readonly IHostReturnReasonDao hostReturnReasonDao;
            protected readonly IBankHostStatusKBZDao bankHostStatusKBZDao;
            protected readonly IBankHostStatusDao bankHostStatusDao;
            protected readonly ISystemProfileDao systemProfileDao;
            protected readonly ILargeAmountDao largeAmountDao;
            protected readonly ISignatureDao signatureDao;
            protected readonly INonConformanceFlagDao nonConformanceFlagDao;
            protected readonly UserDao userDao;
            protected InwardItemViewModel resultModel;
            protected readonly IMICRImageDao micrImageDao;

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
            string Module = string.Empty;
            //Readables by inheriting class
            protected string currentAction;
            protected FormCollection currentFormCollection;
            protected readonly IPullOutReasonDao pullOutReasonDao;

            public ICCSBaseController(IPageConfigDao pageConfigDao,
                ICommonInwardItemDao commonInwardItemDao,
                ISearchPageService searchPageService,
                IAuditTrailDao auditTrailDao,
                ISequenceDao sequenceDao,
                IVerificationDao verificationDao,
                IReportService reportService,
                IHostReturnReasonDao hostReturnReasonDao,
                IBankHostStatusKBZDao bankHostStatusKBZDao,
                IBankHostStatusDao bankHostStatusDao,
                UserDao userDao,
                ILargeAmountDao largeAmountDao,
                ISystemProfileDao systemProfileDao,
                ISignatureDao signatureDao,
                INonConformanceFlagDao nonConformanceFlagDao,
                IPullOutReasonDao pullOutReasonDao
,
                IMICRImageDao micrImageDao
                )
            {
                //Construct
                this.chequeVerificationPageHtml = chequeVerificationPageHtml != null ?
                    chequeVerificationPageHtml : "InwardClearing/ICCSDefault/ChequeVerificationPage";

                this.chequeRetrieverPageHtml = chequeRetrieverPageHtml != null ?
                    chequeRetrieverPageHtml : "InwardClearing/ICCSDefault/ChequeRetrieverPage";

                this.searchPageHtml = searchPageHtml != null ?
                    searchPageHtml : "InwardClearing/ICCSDefault/Index";

                this.searchResultPageHtml = searchResultPageHtml != null ?
                    searchResultPageHtml : "InwardClearing/ICCSDefault/SearchResultPage";

                this.commonInwardItemDao = commonInwardItemDao;
                this.verificationDao = verificationDao;
                this.pageConfigDao = pageConfigDao;
                this.searchPageService = searchPageService;
                this.auditTrailDao = auditTrailDao;
                this.sequenceDao = sequenceDao;
                this.reportService = reportService;
                this.hostReturnReasonDao = hostReturnReasonDao;
                this.bankHostStatusKBZDao = bankHostStatusKBZDao;
                this.bankHostStatusDao = bankHostStatusDao;
                this.userDao = userDao;
                this.largeAmountDao = largeAmountDao;
                this.systemProfileDao = systemProfileDao;
                this.signatureDao = signatureDao;
                this.nonConformanceFlagDao = nonConformanceFlagDao;
                this.pullOutReasonDao = pullOutReasonDao;
                this.micrImageDao = micrImageDao;
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
                if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308130" || staskid == "308140" || staskid == "306550" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "308160" ||  staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200" || staskid == "309100" || staskid == "306520" || staskid == "306510" || staskid == "309230" )
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
                Module = gQueueSqlConfig.PageTitle;
                //---- UNAUTHORIZED ACCESS  ----------------
                //Reject if User Does Not Have Access               
                try {
                     RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, gQueueSqlConfig.TaskId);

                    //Allow access only at index if user access by task id. 
                    //THis can be configured in tblQueueConfig. 
                    if (!"Index".Equals(currentAction)
                        & !"SearchResultPage".Equals(currentAction)
                        & !"ChequeVerificationPage".Equals(currentAction)
                        & !"ChequeRetrieverPage".Equals(currentAction)
                        & !"CreditPosting".Equals(currentAction)
                        & !"NextCheque".Equals(currentAction)
                        & !"PrevCheque".Equals(currentAction)
                        & !"ChequeHistory".Equals(currentAction)
                        & !"ManuallyMarkedCheques".Equals(currentAction)
                        & !"ManualChequesReturned".Equals(currentAction)
                        & !"RoutetoAuthorizer".Equals(currentAction)
                        & !"IQAResult".Equals(currentAction)
                        & !"RouteReason".Equals(currentAction)
                        & !"ReturnReason".Equals(currentAction)
                        & !"PrintCheque".Equals(currentAction)
                        & !"PrintChequeRetriever".Equals(currentAction)
                        & !"PrintAll".Equals(currentAction)
                        & !"PrintSummary".Equals(currentAction)
                        & !"Close".Equals(currentAction)
                        & !"ProceedApproveOrReturnAll".Equals(currentAction)
                        & !"Generate".Equals(currentAction)

                        & !CurrentUser.HasAccessToUrlNotUnique(currentAction)) 
                    {
                        RequestHelper.RejectAccessToLoginPage(ControllerContext);
                    }
                } catch (HttpException ex) {
                    systemProfileDao.Log("ICCSBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    throw new HttpException(Locale.AccessDenied);
                }
                //------ UNAUTHORIZED ACCESS ---------------

                return gQueueSqlConfig;
            }


            [GenericFilter(AllowHttpGet = true)]
            public virtual async Task<ActionResult> Index() 
            {
                await initializeBeforeAction();

                CurrentUser.Account.TaskId = staskid;
                ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, gQueueSqlConfig.toPageSqlConfig());
                //auditTrailDao.SecurityLog("Access " + Module, "", staskid, CurrentUser.Account);
                auditTrailDao.Log("Access " + Module, CurrentUser.Account);
                return View(searchPageHtml);
            }

                public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
                await initializeBeforeAction();
                //string sqlCondition = "";
                int checkitem=1;
                // Image retrieval search
                 if ((staskid == "301120") || (staskid == "301130"))
                {

                    checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account,collection["fldClearDate"]);
                    if (checkitem == 0)
                    {
                        //commonInwardItemDao.UnlockAllAssignedForUserHistory(CurrentUser.Account);
                        gQueueSqlConfig.ViewOrTableName = gQueueSqlConfig.ViewOrTableName + "H";
                    }
                }
                else if (staskid == "308110" || staskid == "308120" || staskid == "308130" || staskid == "308140" || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                {
                    commonInwardItemDao.UnlockAllAssignedForBranchUser(CurrentUser.Account);
                }
                else
                {
                    commonInwardItemDao.UnlockAllAssignedForUser(CurrentUser.Account);
                }
                ViewBag.TaskId = staskid;
                    ViewBag.SearchResult = await pageConfigDao.getQueueResultListFromDatabaseViewAsync(gQueueSqlConfig, collection);
                return View(searchResultPageHtml);
            }

            //VerificationInfoCompare.asp
            public virtual async Task<ActionResult> ChequeVerificationPage(FormCollection collection) {
                await initializeBeforeAction();
                Boolean checkResult = true;
                Dictionary<string, string> result;
                Boolean checkResultBranch = true;
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                string inwardItemId = filter["fldInwardItemId"];
                string branchid = collection["fldIssueBankBranch"];
                //collection.Add("fldInwardItemId", inwardItemId);
                string taskid = gQueueSqlConfig.TaskId;
                Boolean unlock = true;
                CurrentUser.Account.TaskId = staskid;
                //auditTrailDao.SecurityLog("["+ M  odule + "] : Individual Item Pickup  (" + inwardItemId + ")", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : Individual Item Pickup  (" + inwardItemId + ")", CurrentUser.Account);
                //Prompt check is locked by other user
                checkResult = commonInwardItemDao.CheckStatus(inwardItemId, CurrentUser.Account);
                checkResultBranch = commonInwardItemDao.CheckStatusBranch(inwardItemId, CurrentUser.Account);
                ViewBag.TaskId = taskid;
                //Lock this record for currentuser

                //if (taskid == "308110" || taskid == "308140" || taskid == "308150" || taskid == "308160" || taskid == "308170" || taskid == "308180" || taskid == "308190" || taskid == "308200")
                //{
                //    chequeVerificationPageHtml = "InwardClearing/ICCSDefault/BranchChequeVerificationPage";
                //}

                
                if (taskid == "308110" || taskid == "308140" || taskid == "308150" || taskid == "308160" || taskid == "308170" || taskid == "308180" || taskid == "308190" || taskid == "308200")
                {
                    if (checkResultBranch == true)
                    {
                        ViewBag.Type = "Branch";
                        //For Affin, cheque not lock when user click item in listing
                        //commonInwardItemDao.LockChequeBranch(gQueueSqlConfig, collection, CurrentUser.Account, 1);

                        //commonInwardItemDao.LockThisCheque(inwardItemId, CurrentUser.Account);
                        //commonInwardItemDao.LockChequeVerification(gQueueSqlConfig, collection, CurrentUser.Account, 1);
                        //result = commonInwardItemDao.FindItemByInwardItemIdBranch(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        if (taskid == "308170" || taskid == "308180" || taskid == "308190" || taskid == "308200")
                        {
                            result = commonInwardItemDao.FindItemByInwardItemIdBranchNOLOCKNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        else
                        {
                            result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        
                        //chequeVerificationPageHtml = "InwardClearing/ICCSDefault/BranchChequeVerificationPage";
                    }
                    else
                    {
                        unlock = false;
                        if (taskid == "308170" || taskid == "308180" || taskid == "308190" || taskid == "308200")
                        {
                            result = commonInwardItemDao.FindItemByInwardItemIdBranchNOLOCKNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        else
                        {
                            
                            //result = commonInwardItemDao.FindItemByInwardItemIdNOLOCK(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                            //result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                            result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        
                    }
                }
                else
                {
                    if (checkResult == true)
                    {
                        //commonInwardItemDao.LockThisCheque(inwardItemId, CurrentUser.Account);
                        //For Affin, cheque not lock when user click item in listing
                        ViewBag.Type = "";
                        //commonInwardItemDao.LockChequeVerification(gQueueSqlConfig, collection, CurrentUser.Account, 1);
                        //result = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        if (taskid == "306530" || taskid == "306540" || taskid == "306550" || taskid == "306510" || taskid == "306520")
                        {
                            //collection["DataAction"] = 
                            result = commonInwardItemDao.FindItemByInwardItemIdNOLOCKNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        else
                        {
                            result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                            
                    }
                    else
                    {
                        unlock = false;
                        if (taskid == "306530" || taskid == "306540" || taskid == "306550" || taskid == "306510" || taskid == "306520")
                        {
                            //collection["DataAction"] = 
                            result = commonInwardItemDao.FindItemByInwardItemIdNOLOCKNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        else
                        {
                           
                            //result = commonInwardItemDao.FindItemByInwardItemIdNOLOCK(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                            result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                        }
                        
                    }
                }

                if (result == null)
                {
                    //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("InwardClearing/Base/_EmptyChequeVerificationReminder");
                }
                
                
                InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);

                ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
                ViewBag.DataAction = "ChequeVerificationPage";
                ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                
                if (taskid== "306550")
                {
                    string rej = resultModel.allFields["fldRejectCode"];
                    string rej2 = resultModel.allFields["fldRejectCode2"];
                    string rej3 = resultModel.allFields["fldRejectCode3"];
                    var rejcwithd = rej.Split('-');
                    var rejcwithd2 = rej2.Split('-');
                    var rejcwithd3 = rej3.Split('-');
                    ViewBag.RejectCode = rejcwithd[0];
                    ViewBag.RejectCode2 = rejcwithd2[0];

                    if (rejcwithd[0] != "")
                    {
                        ViewBag.fldReturnDescription1 = rejcwithd[1];
                    }
                    if (rejcwithd2[0] != "")
                    {

                        ViewBag.fldReturnDescription2 = rejcwithd[1];
                    }
                    ViewBag.RejectCode3 = rejcwithd3[0];

                    if (rejcwithd3[0] != "")
                    {
                        ViewBag.fldReturnDescription3 = rejcwithd3[1];

                    }


                }
                ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                ViewBag.LockCheck = checkResult.ToString();
                ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                ViewBag.LockCheckBranch = checkResultBranch.ToString();
                /** Ali
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                **/

                #region IQA Field
                ViewBag.UV = resultModel.getField("fldUV");
                ViewBag.QRCode = resultModel.getField("fldQRCodeMatch");
                ViewBag.MicrPresent = resultModel.getField("fldMICRpresent");
                ViewBag.STD = resultModel.getField("fldStandardCheque");
                ViewBag.Duplicate = resultModel.getField("fldInstrumentDuplicate");
                ViewBag.Capture = resultModel.getField("fldCapturedBy"); 
                ViewBag.Defered = resultModel.getField("fldDeferredCheque"); 
                ViewBag.Fraud = resultModel.getField("fldFraudChequeHistory ");
                ViewBag.UnderSizeImage = resultModel.getField("UnderSize Image");
                ViewBag.FoldedorTornDocumentCorners = resultModel.getField("Folded or Torn Document Corners");
                
                ViewBag.fldDocumentSkew = resultModel.getField("fldDocumentSkew");
                ViewBag.fldOversizeImage = resultModel.getField("fldOversizeImage");
                ViewBag.fldPiggybackImage = resultModel.getField("fldPiggybackImage");
                ViewBag.fldImageTooLight = resultModel.getField("fldImageTooLight");
                ViewBag.fldImageTooDark = resultModel.getField("fldImageTooDark");
                ViewBag.fldHorizontalStreaks = resultModel.getField("fldHorizontalStreaks");
                ViewBag.fldBelowMinimumCompressedImageSize = resultModel.getField("fldBelowMinimumCompressedImageSize");
                ViewBag.fldAboveMaximumCompressedImageSize = resultModel.getField("fldAboveMaximumCompressedImageSize");
                ViewBag.fldFrontRearDimensionMismatch = resultModel.getField("fldFrontRearDimensionMismatch");
                ViewBag.fldCarbonStrip = resultModel.getField("fldCarbonStrip");
                ViewBag.fldOutofFocus = resultModel.getField("fldOutofFocus");



                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.fldCyclecode = resultModel.getField("fldCyclecode");
                ViewBag.fldOriChequeSerialNo = resultModel.getField("fldOriChequeSerialNo");
                ViewBag.fldOriIssueBankCode = resultModel.getField("fldOriIssueBankCode");
                ViewBag.fldOriIssueBranchCode = resultModel.getField("fldOriIssueBranchCode");
                ViewBag.fldOriAccountNumber = resultModel.getField("fldOriAccountNumber");
                ViewBag.fldOriTransCode = resultModel.getField("fldOriTransCode");
                ViewBag.fldoriIssueStateCode = resultModel.getField("fldoriIssueStateCode");
                ViewBag.fldIssueBankCode   = resultModel.getField("fldIssueBankCode");
                ViewBag.fldRemarks= resultModel.getField("fldRemarks");
                ViewBag.fldRemarks1stLevel = resultModel.getField("fldRemarks1stLevel");
                ViewBag.fldRemarks2ndLevel = resultModel.getField("fldRemarks2ndLevel");
                ViewBag.fldIssueBankBranch = resultModel.getField("fldIssueBankBranch");
                ViewBag.fldBranchRemarks = resultModel.getField("fldBranchRemarks");
                ViewBag.BranchPendingRemarks = resultModel.getField("BranchPendingRemarks");
                ViewBag.fldBranchAuthorizerReferRemarks  = resultModel.getField("fldBranchAuthorizerReferRemarks");
                ViewBag.fldManual = resultModel.getField("fldManual");
                ViewBag.CCUActivation = resultModel.getField("CCUActivation");
                ViewBag.BranchActivation = resultModel.getField("BranchActivation");



                //*** Ali **//
                #endregion

                ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                ViewBag.getMinTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMin"];
                ViewBag.getMaxTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMax"];
                ViewBag.getRemarkTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateRemark"];
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(inwardItemId);
                
                #region  Created by: Ali Signature Service
                /* Signature Not Show on Reject Reentry */
                    if (staskid != "309100" && staskid != "306260")
                {
                    string s = "";
                    DataTable dt = new DataTable();
                    try
                    {
                        System.Data.DataSet dsResults = SignatureService.GetSignatureResult(result["fldAccountNumber"]);

                        s = dsResults.Tables[0].Rows[0]["SIG_TEXT"].ToString();
                        dt = dsResults.Tables[0];

                    }
                    catch (Exception ex)
                    {

                        s = ex.Message;
                    }
                    

                   
                //    //byte[] data = (byte[]) dsResults.Tables[0].Rows[0]["SIG_TEXT"];//.ToString();
                //    //byte[] data = System.Text.Encoding.ASCII.GetBytes(image);

                    //    //byte[] bytes = Convert.FromBase64String(s);
                    //    //Image image;
                    //    //using (MemoryStream ms = new MemoryStream(bytes))
                    //    //{
                    //    //    image = Image.FromStream(ms);
                    //    //}

                    //    //byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
                    //    //byte[] data = GetBytes(s);

                    //    //byte[] buffer = new byte[4096];
                    //    //Stream st= new MemoryStream(s);

                    //    //System.IO.MemoryStream m = new System.IO.MemoryStream();
                    //    //while (st.Read(buffer, 0, buffer.Length) > 0)
                    //    //{
                    //    //    m.Write(buffer, 0, buffer.Length);
                    //    //}
                    //    //imgView.Tag = m.ToArray();
                    //    //st.Close();
                    //    //m.Close();

                        ViewBag.Base64String = "data:image/png;base64," + s;
                        ViewBag.dtSSCard = dt;

                    }

                    #endregion

                return View(chequeVerificationPageHtml);
            }
            static byte[] GetBytes(string str)
            {
                byte[] bytes = new byte[str.Length * sizeof(char)];
                System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
                return bytes;
            }

            public void ProcessRequest(Object image)
            {
                Stream strm = new MemoryStream((byte[])image);
                byte[] buffer = new byte[4096];
                int byteSeq = strm.Read(buffer, 0, 4096);
                while (byteSeq > 0)
                {
                    
                    byteSeq = strm.Read(buffer, 0, 4096);
                }
            }

            public virtual async Task<ActionResult> ChequeRetrieverPage(FormCollection collection) {
                await initializeBeforeAction();

                string taskid = gQueueSqlConfig.TaskId;
                CurrentUser.Account.TaskId = taskid;
                

                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
                string inwardItemId = filter["fldInwardItemId"];
                
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
                else if (taskid == "309230")
                {
                    ViewBag.UPI = "Y";
                    result = commonInwardItemDao.FindInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId, null);
                }
                else
                {
                    result = commonInwardItemDao.FindItemByInwardItemIdNew(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId, null);

                }

                if (result == null)
                {
                    //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                    return View("InwardClearing/Base/_EmptyChequeVerificationReminder");
                }

                InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                ViewBag.TaskId = taskid;
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.HostStatus2 = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                ViewBag.pendingbranchremarks = resultModel.getField("pendingbranchremarks").Trim();
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                ViewBag.ImgRetrieve = "Y";
                //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(resultModel.allFields["fldInwardItemId"]);
                ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                
                ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                /*
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                */
                if ((staskid == "301120") || (staskid == "301130") || (staskid == "309230"))
                {
                    checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account, Convert.ToDateTime(resultModel.allFields["fldClearDate"]).ToString("dd-MM-yyyy"));
                    if (checkitem == 0)
                    {
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistoryH(resultModel.allFields["fldInwardItemId"]);
                    }
                    else
                    {
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                    }
                }
                auditTrailDao.Log("Search - Image Cheque:" + ViewBag.InwardItemViewModel.getField("fldChequeSerialNo") + ", Account No: " + ViewBag.InwardItemViewModel.getField("fldAccountNumber"), CurrentUser.Account);
                return View(chequeRetrieverPageHtml);
            }


            public virtual async Task<ActionResult> LockVerification(FormCollection collection) {
                //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": Start LockVerification", CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                await initializeBeforeAction();
                CurrentUser.Account.TaskId = staskid;
                //auditTrailDao.SecurityLog("["+ Module + "] : Start LockVerification Queue", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : Start LockVerification Queue", CurrentUser.Account);
                // add by kai hong 20170616
                // New Verification,1st,2nd,3rd,branch maker/checker verification will call new simplified function
                ViewBag.DataAction = "LockVerification";
                ViewBag.TaskId = staskid;
                if (staskid == "306910" || staskid == "306930" || staskid == "306920" || staskid == "306550" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "306510" || staskid == "309100" || staskid == "306520")
                {
                    Dictionary<string, string> result = commonInwardItemDao.LockAllChequeNew(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                    if (result == null)
                    {
                        systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification", /*CurrentUser.Account.UserAbbr*/  CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    else
                    {
                        InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                        ViewBag.InwardItemViewModel = resultModel;
                        ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                        ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                        ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                        //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(resultModel.allFields["fldInwardItemId"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                    }

                }
                else if ( staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308160" || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                {
                    Dictionary<string, string> result = commonInwardItemDao.LockAllChequeBranch(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                    if (result == null)
                    {
                        systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification", /*CurrentUser.Account.UserAbbr*/ CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    else
                    {
                        InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);

                        //chequeVerificationPageHtml = "InwardClearing/ICCSDefault/BranchChequeVerificationPage";
                        ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
                        ViewBag.InwardItemViewModel = resultModel;
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                        ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                        ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                        //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(resultModel.allFields["fldInwardItemId"]);
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                    }

                }
                else
                {
                    Dictionary<string, string> result = commonInwardItemDao.LockAllCheque(gQueueSqlConfig, collection, CurrentUser.Account, Convert.ToInt32(CurrentUser.Account.VerificationLimit));
                    if (result == null)
                    {
                        systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    else
                    {
                        InwardItemViewModel resultModel = InwardItemConcern.ChequePagePopulateViewModel(gQueueSqlConfig, result);
                        ViewBag.InwardItemViewModel = resultModel;
                        //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                        ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                        ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(resultModel.allFields["fldInwardItemId"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                    }

                }
                //Just initial total record
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                ViewBag.LockIndicator = true;
                //systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                //auditTrailDao.SecurityLog("[" + Module + "] : End LockVerification Queue", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : End LockVerification Queue", CurrentUser.Account);

                ViewBag.getMinTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMin"];
                ViewBag.getMaxTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMax"];
                ViewBag.getRemarkTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateRemark"];
                ViewBag.Micr = commonInwardItemDao.GetMicr();

                return View(chequeVerificationPageHtml);
            }

            //ICCSNextRecord
            public virtual async Task<ActionResult> NextCheque(FormCollection collection) {
                await initializeBeforeAction();
                CurrentUser.Account.TaskId = staskid;
                //string uic = collection["fldUIC2"];
                // add by shamil 20170616
                // Delete temp gif when see next check
                //auditTrailDao.SecurityLog("["+ Module + "] : Access Next Cheque ", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : Access Next Cheque ",  CurrentUser.Account);
                commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                // add by kai hong 20170616
                // New Verification,1st,2nd,3rd verification will call new simplified function
                // xx start 20210601
                Dictionary<string, string> result;
                ViewBag.TaskId = staskid;
                ViewBag.DataAction = collection["DataAction"];
                if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308160" || staskid == "306510"
                    || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200" || staskid == "309100" || staskid == "306520" || staskid == "306530" || staskid == "306540" || staskid == "306550")
                // xx end 20210601
                {
                    if (staskid == "306520" || staskid == "306530" || staskid == "306540" || staskid == "306550" || staskid == "306510")
                    {
                        if (staskid == "306510" || staskid == "306520")
                        {
                            if (ViewBag.DataAction == "ChequeVerificationPage")
                            {
                                result = commonInwardItemDao.NextChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                            }
                            else
                            {
                                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                            }
                        }
                        else
                        {
                            result = commonInwardItemDao.NextChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                        }
                        
                    }
                    else if (staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                    {
                        result = commonInwardItemDao.NextChequeNoLockBranch(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    
                    if (result == null)
                    {
                        systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    else
                    {
                        InwardItemViewModel resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                        if (staskid == "308110" || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                        {
                            //chequeVerificationPageHtml = "InwardClearing/ICCSDefault/BranchChequeVerificationPage";
                            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        }
                        else
                        {
                            ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        }
                        
                        ViewBag.InwardItemViewModel = resultModel;
                        //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
                        ViewBag.InwardItemViewModel = resultModel;
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        //ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        //ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ////ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                        ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                        //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(resultModel.allFields["fldInwardItemId"]);
                        //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                        //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                        collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                    }

                }
                else
                {
                    //commonInwardItemDao.LockThisCheque(collection["fldInwardItemId"], CurrentUser.Account);
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    InwardItemViewModel resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.InwardItemViewModel = resultModel;
                    //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                    ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                    ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                    ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                }
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                // xx start
                ViewBag.getMinTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMin"];
                ViewBag.getMaxTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMax"];
                ViewBag.getRemarkTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateRemark"];
                // xx end
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                return View(chequeVerificationPageHtml);
            }

            //ICCSPrevRecord
            public virtual async Task<ActionResult> PrevCheque(FormCollection collection) {
                await initializeBeforeAction();
                // add by shamil 20170616
                // Delete temp gif when see next check
                CurrentUser.Account.TaskId = staskid;
                //auditTrailDao.SecurityLog("[" + Module + "] : Access Previous Cheque ", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : Access Previous Cheque ",  CurrentUser.Account);
                commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                // add by kai hong 20170616
                // New Verification,1st,2nd,3rd,branch maker/checker verification will call new simplified function
                ViewBag.TaskId = staskid;
                ViewBag.DataAction = collection["DataAction"];
                // xx start 20210601 
                if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "306210" ||  staskid == "306230" || staskid == "306220" || staskid == "308130" || staskid == "308140" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308160" || staskid == "306510"
                    || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200" || staskid == "309100" || staskid == "306520" || staskid == "306550" || staskid == "306530" || staskid == "306540")
                // xx end 20210601
                {
                    Dictionary<string, string> result;
                    if (ViewBag.DataAction.ToString().Trim() == "ChequeVerificationPage")
                    {
                        if (staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                        {
                            result = commonInwardItemDao.PrevChequeNoLockBranch(gQueueSqlConfig, collection, CurrentUser.Account);
                        }
                        else
                        {
                            result = commonInwardItemDao.PrevChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                        }
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    
                    if (result == null)
                    {
                        systemProfileDao.Log(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff") + ": End LockVerification"/*, CurrentUser.Account.UserAbbr*/, CurrentUser.Account.UserAbbr, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    else
                    {
                        InwardItemViewModel resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);

                        if (staskid == "308110" || staskid == "308170" || staskid == "308180" || staskid == "308190" || staskid == "308200")
                        {
                            //chequeVerificationPageHtml = "InwardClearing/ICCSDefault/BranchChequeVerificationPage";
                            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        }
                        else
                        {
                            ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        }

                        ViewBag.InwardItemViewModel = resultModel;
                        //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
                        ViewBag.InwardItemViewModel = resultModel;
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        //ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        //ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                        ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                        //ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //    ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //    ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //    ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    }

                }
                else
                {
                    Dictionary<string, string> result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    InwardItemViewModel resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.InwardItemViewModel = resultModel;
                    //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                    ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                    ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
                    ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
                    ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    //collection.Add("fldInwardItemId", resultModel.allFields["fldInwardItemId"]);
                }
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                // xx start
                ViewBag.getMinTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMin"];
                ViewBag.getMaxTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateMax"];
                ViewBag.getRemarkTruncatedAmount = commonInwardItemDao.GetTruncatedAmount().Rows[0]["fldTruncateRemark"];
                // xx end
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["PreviousValue"].ToString());
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                return View(chequeVerificationPageHtml);
            }

            //ICCSHistory
            public virtual async Task<ActionResult> ChequeHistory(FormCollection collection) {
                await initializeBeforeAction();
                int checkitem = 1;
                if ((staskid == "301120") || (staskid == "301130"))
                {
                    checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldClearDate"]);
                    if (checkitem == 0)
                    {
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistoryH(collection["fldInwardItemId"].ToString());
                        return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                    }
                    else
                    {
                        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                        return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                    }
                }
                else
                {
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                    auditTrailDao.SecurityLog("[" + Module + "] : Retrieve audit log ", "", staskid, CurrentUser.Account);
                    return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                }

            }

            public virtual async Task<ActionResult> IQAResult(FormCollection collection)
            {
                await initializeBeforeAction();
                int checkitem = 1;


                ViewBag.IQAResult = commonInwardItemDao.IQAResult(collection["fldInwardItemId"].ToString());
                return PartialView("InwardClearing/Modal/_IQAResultPopup");
                //if ((staskid == "301120") || (staskid == "301130"))
                //{
                //    checkitem = commonInwardItemDao.CheckMainTable(CurrentUser.Account, collection["fldClearDate"]);
                //    if (checkitem == 0)
                //    {
                //        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistoryH(collection["fldInwardItemId"].ToString());
                //        return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                //    }
                //    else
                //    {
                //        ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                //        return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                //    }
                //}
                //else
                //{
                //    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["fldInwardItemId"].ToString());
                //    auditTrailDao.SecurityLog("[" + Module + "] : Retrieve audit log ", "", staskid, CurrentUser.Account);
                //    return PartialView("InwardClearing/Modal/_ChequeHistoryPopup");
                //}

            }

            public virtual async Task<ActionResult> RouteReason(FormCollection collection)
            {
                await initializeBeforeAction();
                int checkitem = 1;


                ViewBag.RouteReason = commonInwardItemDao.RouteReason();
                ChequeVerificationOnRouteAndReturReason(collection);

                return View("InwardClearing/Modal/_RouteReason");
            }
            public virtual async Task<ActionResult> ReturnReason(FormCollection collection)
            {
                await initializeBeforeAction();
                int checkitem = 1;

                ChequeVerificationOnRouteAndReturReason(collection);
                ViewBag.RouteReason = commonInwardItemDao.RouteReason();
                ViewBag.Title = "ReturnReason";
                return View("InwardClearing/Modal/_RouteReason");
            }

            public virtual async Task<ActionResult> ManualChequesReturned(FormCollection collection)
            {
                await initializeBeforeAction();
                int checkitem = 1;


                ViewBag.ManualMark = commonInwardItemDao.ManuallyUpdateReturnedCheques(collection);
                ViewBag.Title = "Returned";
                return View("InwardClearing/Base/_EmptyChequeVerification");
              
            }
            public async Task<ActionResult> ManuallyMarkedCheques(FormCollection collection)
            {
                await initializeBeforeAction();
                string inwardItemId = collection["fldInwardItemId"];
                /*Boolean checkResult = true;
                    Boolean checkResultBranch = true;
                checkResult = commonInwardItemDao.CheckStatus(inwardItemId, CurrentUser.Account);
                checkResultBranch = commonInwardItemDao.CheckStatusBranch(inwardItemId, CurrentUser.Account);*/

                collection["new_textRejectCode"] = "000";

                Log(DateTime.Now + ":Get inward item id :" + inwardItemId, CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item UIC :" + collection["fldUIC"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item check no :" + collection["fldChequeSerialNo"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item acc no :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item image name :" + collection["imageId"], CurrentUser.Account.BankCode);
                string verifyAction = VerificationStatus.ACTION.Manual;
                Log(DateTime.Now + ":Start validate verification ", CurrentUser.Account.BankCode);
                List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
                //List<string> errorMessages = new List<string>();
                Log(DateTime.Now + ":Finish validate verification ", CurrentUser.Account.BankCode);
                Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                                                  //Dictionary<string, string> afterResult = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                ViewBag.DataAction = collection["DataAction"];
                ViewBag.TaskId = staskid;

                string manualmark = collection["ManualMark"];

                //check if validate contain error
                if ((errorMessages.Count > 0))
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
                        TempData["ErrorMsg"] = errorMessages;
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
                }
                else
                {
                    if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                    {
                        commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                    }
                    //check next available cheque
                    Log(DateTime.Now + ":Check available check ", CurrentUser.Account.BankCode);
                    if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306240" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520"|| staskid == "309100")
                    {

                        ViewBag.ManualMark = commonInwardItemDao.ManuallyUpdateCheques(collection["fldInwardItemId"], CurrentUser.Account, collection, collection["fldAccountNumber"], collection["fldChequeSerialNo"], staskid);

                        if ((collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306520") || staskid == "306530" || staskid == "306540" || staskid == "306550" || (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306510"))
                        {
                            collection["fldInwardItemId"] = collection["NextValue"];
                            result = commonInwardItemDao.NextChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                        }
                        else
                        {
                            result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                        }

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
                            ViewBag.InwardItemViewModel = resultModel;
                            //Azim End
                            //return View("InwardClearing/Base/_EmptyChequeVerification");
                        }
                    }
                    else
                    {
                        result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                        if (result == null)
                        {
                            resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                            return View("InwardClearing/Base/_EmptyChequeVerification");
                        }

                    }

                    resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                    //if next cheque is not available.. check previous instead
                    /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                    {
                        Log(DateTime.Now + ":Check available check fail ", CurrentUser.Account.BankCode);
                        if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                        {
                            result = commonInwardItemDao.PrevChequeNew(gQueueSqlCon fig, collection, CurrentUser.Account);
                        }
                        else
                        {
                            result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                        } 
                        resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    }*/
                    if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                    {
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        //add new thing

                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                        commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    }
                    else
                    {
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        //add new thing
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                        
                        //Approve Process
                        //verificationDao.VerificationApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

                        //Insert to cheque history
                        commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                    }

                    //Minus Record Indicator
                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                }

                // if cheque available.. render cheque page
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                    ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                    ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator
                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    if (staskid == "309100")
                    {

                        return View("ChequeVerificationPage");
                    }
                    else
                    {

                        return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                    }

                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }



            }


            public async Task<ActionResult> RoutetoAuthorizer(FormCollection collection)
            {
                await initializeBeforeAction();
                string inwardItemId = collection["fldInwardItemId"];
                /*Boolean checkResult = true;
                    Boolean checkResultBranch = true;
                checkResult = commonInwardItemDao.CheckStatus(inwardItemId, CurrentUser.Account);
                checkResultBranch = commonInwardItemDao.CheckStatusBranch(inwardItemId, CurrentUser.Account);*/

                collection["new_textRejectCode"] = "000";

                Log(DateTime.Now + ":Get inward item id :" + inwardItemId, CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item UIC :" + collection["fldUIC"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item check no :" + collection["fldChequeSerialNo"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item acc no :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
                Log(DateTime.Now + ":Get inward item image name :" + collection["imageId"], CurrentUser.Account.BankCode);
                string verifyAction = VerificationStatus.ACTION.Manual;
                Log(DateTime.Now + ":Start validate verification ", CurrentUser.Account.BankCode);
                List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
                //List<string> errorMessages = new List<string>();
                Log(DateTime.Now + ":Finish validate verification ", CurrentUser.Account.BankCode);
                Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                                                  //Dictionary<string, string> afterResult = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
                ViewBag.DataAction = collection["DataAction"];
                ViewBag.TaskId = staskid;

                string manualmark = collection["ManualMark"];

                //check if validate contain error
                if ((errorMessages.Count > 0))
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
                        TempData["ErrorMsg"] = errorMessages;
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
                }
                else
                {
                    if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                    {
                        commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                    }
                    //check next available cheque
                    Log(DateTime.Now + ":Check available check ", CurrentUser.Account.BankCode);
                    if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306240" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520" || staskid == "309100")
                    {

                        ViewBag.RouteToAuthorizer = commonInwardItemDao.RouteToAuthorizer(collection["fldInwardItemId"], CurrentUser.Account, collection, collection["fldAccountNumber"], collection["fldChequeSerialNo"], staskid);

                        if ((collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306520") || staskid == "306530" || staskid == "306540" || staskid == "306550" || (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306510"))
                        {
                            collection["fldInwardItemId"] = collection["NextValue"];
                            result = commonInwardItemDao.NextChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                        }
                        else
                        {
                            result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                        }

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
                            ViewBag.InwardItemViewModel = resultModel;
                            //Azim End
                            //return View("InwardClearing/Base/_EmptyChequeVerification");
                        }
                    }
                    else
                    {
                        result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                        if (result == null)
                        {
                            resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                            return View("InwardClearing/Base/_EmptyChequeVerification");
                        }

                    }

                    resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                    //if next cheque is not available.. check previous instead
                    /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                    {
                        Log(DateTime.Now + ":Check available check fail ", CurrentUser.Account.BankCode);
                        if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                        {
                            result = commonInwardItemDao.PrevChequeNew(gQueueSqlCon fig, collection, CurrentUser.Account);
                        }
                        else
                        {
                            result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                        } 
                        resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    }*/
                    if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                    {
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                        ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        //add new thing

                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                        commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    }
                    else
                    {
                        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                        ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                        ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                        //add new thing
                        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                        //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                        ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                        ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                        ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                        ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                        ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                        ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                        ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                        ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);

                        //Approve Process
                        //verificationDao.VerificationApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

                        //Insert to cheque history
                        commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                    }

                    //Minus Record Indicator
                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                }

                // if cheque available.. render cheque page
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = bankHostStatusKBZDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
                    ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
                    ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
                    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator
                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                    ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    if (staskid == "309100")
                    {

                        return View("ChequeVerificationPage");
                    }
                    else
                    {

                        return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                    }

                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }



            }





            public void Log(string logMessage, string user)
            {
                if (CurrentUser.Account.Logindicator == "Y")
                {
                    string path = "";
                    path = CurrentUser.Account.LogPath;
                    if (String.IsNullOrEmpty(CurrentUser.Account.UserId))
                    {
                        path = path + @"\" + user + ".log";
                    }
                    else
                    {
                        path = path + @"\" + CurrentUser.Account.UserAbbr + ".log";
                    }
                    using (StreamWriter w = System.IO.File.AppendText(path))
                    {
                        w.WriteLine(logMessage);
                    }
                }
            }



            private void ChequeVerificationOnRouteAndReturReason(FormCollection collection)
            {
                
                ViewBag.new_textRejectCode = collection["new_textRejectCode"];
                ViewBag.DataAction = collection["DataAction"];
                ViewBag.fldUIC2 = collection["fldUIC2"];
                ViewBag.NextValue = collection["NextValue"];
                ViewBag.current_fldAccountNumber = collection["current_fldAccountNumber"];
                ViewBag.current_fldChequeSerialNo = collection["current_fldChequeSerialNo"];
                ViewBag.current_fldUIC = collection["current_fldUIC"];
                ViewBag.fldInwardItemId = collection["fldInwardItemId"];
                ViewBag.textAreaRemarks = collection["textAreaRemarks"];
                ViewBag.current_fldAmount = collection["current_fldAmount"];
                ViewBag.CCUActivation = collection["CCUActivation"];
                ViewBag.BranchActivation = collection["BranchActivation"];


                ViewBag.TaskId = staskid;

            }

            
            

            public virtual async Task<ActionResult> PrintCheque(FormCollection collection) {
                await initializeBeforeAction();

                CurrentUser.Account.TaskId = TaskIds.PrintCheque;
                auditTrailDao.Log("Print Cheque - Cheque Serial No: " + collection["fldChequeSerialNoLog"] + ", Account No: " + collection["fldAccountNumberLog"], CurrentUser.Account);
                ReportModel reportModel = await reportService.GetReportConfigByTaskIdAsync(TaskIds.PrintCheque);
                string reportPath = reportModel.reportPath;

                //Search Report Path in Project Or In Any Folder specified
                if (!System.IO.File.Exists(reportPath)) {
                    reportPath = Server.MapPath(reportModel.reportPath);
                    if (!System.IO.File.Exists(reportPath)) {
                        return View("InwardClearing/Modal/_EmptyPrint");
                    }
                }

                string fileNameExtention = "pdf"; string mimeType;
                //byte[] renderedBytes = reportService.renderChequeReportWithImageBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);
                byte[] renderedBytes = reportService.PrintChequeConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
                auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
                //auditTrailDao.SecurityLog("["+ Module + "] : Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", staskid, CurrentUser.Account);
                return File(renderedBytes, mimeType);
            }

            public virtual async Task<ActionResult> PrintChequeRetriever(FormCollection collection) {
                await initializeBeforeAction();

                CurrentUser.Account.TaskId = TaskIds.PrintChequeRetriever;
                auditTrailDao.Log("Print Cheque - Cheque Serial No: " + collection["fldChequeSerialNoLog"] + ", Account No: " + collection["fldAccountNumberLog"], CurrentUser.Account);
                ReportModel reportModel = await reportService.GetReportConfigByTaskIdAsync(TaskIds.PrintChequeRetriever);
                string reportPath = reportModel.reportPath;
                
                //Search Report Path in Project Or In Any Folder specified
                if (!System.IO.File.Exists(reportPath)) {
                    reportPath = Server.MapPath(reportModel.reportPath);
                    if (!System.IO.File.Exists(reportPath)) {
                        return View("InwardClearing/Modal/_EmptyPrint");
                    }
                }

                string fileNameExtention = "pdf"; string mimeType;
                //byte[] renderedBytes = reportService.renderChequeReportWithImageBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);
                byte[] renderedBytes = reportService.PrintChequeConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
                auditTrailDao.Log("Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
                //auditTrailDao.SecurityLog("["+ Module + "] Generate Report For Cheque '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", staskid, CurrentUser.Account);
                return File(renderedBytes, mimeType);
            }

            public virtual async Task<ActionResult> PrintAll(FormCollection collection) {
                await initializeBeforeAction();
                CurrentUser.Account.TaskId = staskid;
                //Azim Start
                ReportModel reportModel = await reportService.GetReportConfigAsync(gQueueSqlConfig.toPageSqlConfig(),"");
                //Azim End
                string reportPath = reportModel.reportPath;

                //Search Report Path in Project Or In Any Folder specified
                if (!System.IO.File.Exists(reportPath)) {
                    reportPath = Server.MapPath(reportModel.reportPath);
                    if (!System.IO.File.Exists(reportPath)) {
                        return View(chequeVerificationPageHtml);
                    }
                }

                string fileNameExtention = "pdf";
                string mimeType;
                byte[] renderedBytes = reportService.renderReportBasedOnConfig(reportModel, collection, reportPath, fileNameExtention, out mimeType);

                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", reportModel.extentionFilename, fileNameExtention));
                auditTrailDao.Log("Generate Report '" + reportModel.extentionFilename + "' in " + fileNameExtention, CurrentUser.Account);
                //auditTrailDao.SecurityLog("["+ Module + "] : Generate Report '" + reportModel.extentionFilename + "' in " + fileNameExtention, "", staskid, CurrentUser.Account);
                return File(renderedBytes, mimeType);
            }


            //Action Close can be defaulted by any action
            public virtual async Task<ActionResult> Close(FormCollection collection) {
                await initializeBeforeAction();
                CurrentUser.Account.TaskId = staskid;
                // add by shamil 20170616
                // Delete temp gif when see next check
                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }
                //auditTrailDao.SecurityLog("["+ Module + "] : Closed Verification", "", staskid, CurrentUser.Account);
                auditTrailDao.Log("[" + Module + "] : Closed Verification", CurrentUser.Account);
                return PartialView("InwardClearing/Modal/_ClosePopup");
            }


            public virtual async Task<ActionResult> ApproveAll(FormCollection collection) {
                await initializeBeforeAction();
                ViewBag.TaskRole = gQueueSqlConfig.TaskRole;
                return PartialView("InwardClearing/Modal/_PasswordPopup");
            }

            

            public virtual async Task<ActionResult> ReturnAll(FormCollection collection) {
                await initializeBeforeAction();
                ViewBag.TaskRole = gQueueSqlConfig.TaskRole;
                return PartialView("InwardClearing/Modal/_PasswordPopup");
            }

            public virtual async Task<JsonResult> ProceedApproveOrReturnAll(FormCollection collection) {
                await initializeBeforeAction();
                string notice = "";
                string password = collection["proceedPassword"];
                string userPassword = userDao.GetUser(CurrentUser.Account.UserId).fldPassword;

                if (password.Equals(userPassword)) {
                    if ("ApproveAll".Equals(gQueueSqlConfig.TaskRole)) {

                        //Insert to cheque history
                        commonInwardItemDao.InsertChequeHistoryForApproveOrRejectAll("A", CurrentUser.Account, gQueueSqlConfig.TaskId);
                        //Do process
                        verificationDao.VerificationApproveAll(CurrentUser.Account);
                        //Audit Trail
                        auditTrailDao.Log("Verification - Approve All", CurrentUser.Account);
                        //auditTrailDao.SecurityLog("Verification - Approve All", "", staskid, CurrentUser.Account);
                        notice = "Approve All Success";
                    } else if ("ReturnAll".Equals(gQueueSqlConfig.TaskRole)) {

                        if (!String.IsNullOrEmpty(collection["rejectCode"])) {
                            //Insert to cheque history
                            commonInwardItemDao.InsertChequeHistoryForApproveOrRejectAll("R", CurrentUser.Account, gQueueSqlConfig.TaskId, collection["rejectCode"]);
                            //Do process
                            verificationDao.VerificationReturnAll(CurrentUser.Account);
                            //Audit Trail
                            //auditTrailDao.Log("Verification - Return All", CurrentUser.Account);
                            auditTrailDao.SecurityLog("Verification - Return All", "", staskid, CurrentUser.Account);
                            notice = "Return All Success";

                        }else {
                            notice = "ERROR :Reject Code Required";
                        }
                    }
                }else {
                    notice = "ERROR : Wrong Password";
                }

                return Json(new {notice = notice}, JsonRequestBehavior.AllowGet);
            }


            public virtual async Task<ActionResult> Generate(FormCollection collection)
            {

                
                //await initializeBeforeAction();
                InwardItemViewModel inwardItemViewModel = new InwardItemViewModel();
                inwardItemViewModel.posPayType = "HostValidation";
                inwardItemViewModel.clearDate = collection["fldClearDate"];
                inwardItemViewModel.clearDate = DateTime.ParseExact(inwardItemViewModel.clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
                inwardItemViewModel.startDate = DateTime.Now.ToString("dd-MM-yyyy");
                inwardItemViewModel.endDate = DateTime.Now.ToString("dd-MM-yyyy");
                inwardItemViewModel.Processname = "HostValidation";
                inwardItemViewModel.bankCode = "083";
                inwardItemViewModel.status = "1";
                verificationDao.InsertHostValidation(inwardItemViewModel, CurrentUser.Account);

                return RedirectToAction("Index");

            }


            public virtual ActionResult ProgressBar(FormCollection col)
            {
                string progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                progressbarvalue = RecursiveProgressBar(progressbarvalue);
                //if (progressbarvalue == "2")
                //{
                //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                ////    progressbarvalue = "20";
                //}
                //if (progressbarvalue == "3")
                //{
                //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                //    progressbarvalue = "60";
                //}
                //if (progressbarvalue == "4")
                //{
                //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                //    progressbarvalue = "100";
                //}

                return Json(progressbarvalue);
            }



            public async Task<ActionResult> ConfirmMICR(FormCollection collection)
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



                errorMessages = new List<string>();//verificationDao.Validate(collection, CurrentUser.Account);
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
                        Message = verificationDao.CheckerConfirmNew(collection, CurrentUser.Account, gQueueSqlConfig.TaskId, "");


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
                        ViewBag.InwardItemViewModel = resultModel;
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
                    return View(chequeVerificationPageHtml);
                }
                // if not.. render empty cheque with close button
                else
                {
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
                /*}*/
            }



            public string RecursiveProgressBar(string progressbarvalue)
            {

                if (progressbarvalue == "")
                {
                    System.Threading.Thread.Sleep(1000);
                    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                    RecursiveProgressBar(progressbarvalue);
                }
                if (progressbarvalue == "1")
                {
                    System.Threading.Thread.Sleep(1000);
                    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                    progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);


                }
                if (progressbarvalue == "2")
                {
                    System.Threading.Thread.Sleep(1000);
                    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                    progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
                }
                if (progressbarvalue == "3")
                {
                    System.Threading.Thread.Sleep(1000);
                    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
                    progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
                }
                if (progressbarvalue == "4")
                {
                    progressbarvalue = "4";
                }

                return progressbarvalue;

            }




        }
    }
}