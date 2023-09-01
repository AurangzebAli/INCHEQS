using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Report;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security.User;
using INCHEQS.Models.Verification;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.Models.Signature;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using System.Data;
using System.Security.Permissions;
using INCHEQS.Areas.ICS.Models.MICRImage;
using System.Globalization;
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Models.RejectReentry;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.Verification
{
    public class VerificationController : ICCSBaseController
    {

        public VerificationController(IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportService reportService, IHostReturnReasonDao hostReturnReasonDao, IBankHostStatusKBZDao bankHostStatusKBZDao, IBankHostStatusDao bankHostStatusDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, ISignatureDao signatureDao, INonConformanceFlagDao nonConformanceFlagDao, IPullOutReasonDao pullOutReasonDao,
                IMICRImageDao micrImageDao) : base(pageConfigDao, commonInwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, bankHostStatusKBZDao, bankHostStatusDao, userDao, largeAmountDao, systemProfileDao, signatureDao, nonConformanceFlagDao, pullOutReasonDao, micrImageDao)
        {
        }

        protected override string initializeQueueTaskId()
        {
            return RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
        }

        public async Task<ActionResult> VerificationApprove(FormCollection collection)
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
            string verifyAction = VerificationStatus.ACTION.VerificationApprove;// A = Approve 
            Log(DateTime.Now + ":Start validate verification ", CurrentUser.Account.BankCode);
            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            Log(DateTime.Now + ":Finish validate verification ", CurrentUser.Account.BankCode);
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            //Dictionary<string, string> afterResult = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.TaskId = staskid;

            string manualmark = collection["ManualMark"];


            collection["fldIssueBranchCode"] = collection["fldIssueBranchCode"].Replace(",", "").Trim();

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                Log(DateTime.Now + ":Validate verification fail ", CurrentUser.Account.BankCode);
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
                if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210" || staskid == "306240" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                {


                    ViewBag.Message = verificationDao.VerificationApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                    if ((collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306520") || staskid == "306530" || staskid == "306540" || (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage" && staskid == "306510"))
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


                if (ViewBag.Message != "" && ViewBag.Message != "Request Processed Successfully")
                {
                    resultModel = InwardItemConcern.ChequePopulateViewModel(gQueueSqlConfig, result, collection, ViewBag.Message);
                    errorMessages = verificationDao.ValidateVerificationService(collection, CurrentUser.Account, verifyAction, staskid, ViewBag.Message);
                }
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
                    if (errorMessages.Count == 0)
                    {
                        commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                    }


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
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);

                    //Approve Process
                    verificationDao.VerificationApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

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
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/Base/_EmptyChequeVerification");


            }
        }

        public async Task<ActionResult> VerificationReturn(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationReturn;// R = Return

            if (collection["new_textRejectCode"].Trim().Length == 1)
            {
                collection["new_textRejectCode"] = "00" + collection["new_textRejectCode"];
            }
            else if (collection["new_textRejectCode"].Trim().Length == 2)
            {
                collection["new_textRejectCode"] = "0" + collection["new_textRejectCode"];
            }
            //List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            List<string> errorMessages = new List<string>();
            Dictionary<string, string> result;// = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.TaskId = staskid;
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
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306220" || staskid == "306230" || staskid == "306240" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                {

                    verificationDao.VerificationReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);


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
                    }
                }
                else
                {

                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }*/
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
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
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
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
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    //Return Process
                    verificationDao.VerificationReturn(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

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
                //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                ViewBag.Micr = commonInwardItemDao.GetMicr();

                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult> VerificationRoute(FormCollection collection, string rejectPending084 = null)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationRoute;// B = Route to branch 

            if (rejectPending084 == "084")
            {
                collection["new_textRejectCode"] = rejectPending084;
            }

            if (collection["new_textRejectCode"].Trim().Length == 1)
            {
                collection["new_textRejectCode"] = "00" + collection["new_textRejectCode"];
            }
            else if (collection["new_textRejectCode"].Trim().Length == 2)
            {
                collection["new_textRejectCode"] = "0" + collection["new_textRejectCode"];
            }


            //List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            List<string> errorMessages = new List<string>();
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.TaskId = staskid;
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
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520" || staskid == "306550")
                {
                    verificationDao.VerificationRouteNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
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
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                else
                {

                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    ////Route to branch process
                    //verificationDao.VerificationRoute(collection, CurrentUser.Account);

                    ////Insert to cheque history
                    //commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }*/
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
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
                    ViewBag.fldCyclecode = resultModel.getField("fldCyclecode");
                    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
                    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
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
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                    //Route to branch process

                    verificationDao.VerificationRoute(collection, CurrentUser.Account);

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
                //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                //return View("InwardClearing/Base/_EmptyChequeVerification");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }



        public async Task<ActionResult> VerificationPullOut(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationPullOut;// P = Pull Out Item

            //List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            List<string> errorMessages = new List<string>();
            Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            string otherPullOutReason = "";
            string pullOutReasonBox = "";
            ViewBag.TaskId = staskid;
            if (collection["pullOutReason"] != null)
            {
                otherPullOutReason = collection["pullOutReason"].ToString();
            }
            if (collection["pullOutReasonBox"] != null)
            {
                pullOutReasonBox = collection["pullOutReasonBox"].Trim().ToString();
            }


            string pullOutReason = "";

            if (otherPullOutReason != "" && pullOutReasonBox != null)
            {
                pullOutReason = pullOutReasonBox + ", " + otherPullOutReason;
            }
            else if (otherPullOutReason != "" && pullOutReasonBox == null)
            {
                pullOutReason = otherPullOutReason;
            }
            else if (otherPullOutReason == "" && pullOutReasonBox != null)
            {
                pullOutReason = pullOutReasonBox;
            }
            else
            {
                pullOutReason = "";
            }

            collection["textAreaRemarks"] = pullOutReason;

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            }
            else
            {
                //azfar
                if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "306210" || staskid == "306530" || staskid == "306540" || staskid == "307200")
                {
                    verificationDao.VerificationPullOutNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                    //Insert to cheque history
                    //commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    //Insert Pull Out Reason
                    verificationDao.InsertPullOutInfo(collection, CurrentUser.Account);
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }

                else if (gQueueSqlConfig.TaskId == "306550")
                {
                    verificationDao.BranchConfirmation(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                    //check next available cheque
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }

                }
                else if (gQueueSqlConfig.TaskId == "306510")
                {
                    verificationDao.VerificationPullOut(collection, CurrentUser.Account);

                    //Insert to cheque history
                    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    //Insert Pull Out Reason
                    verificationDao.InsertPullOutInfo(collection, CurrentUser.Account);
                    //check next available cheque
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                else if (gQueueSqlConfig.TaskId == "306220" || gQueueSqlConfig.TaskId == "306230")
                {
                    verificationDao.VerificationPullOut(collection, CurrentUser.Account);

                    //Insert to cheque history
                    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    //Insert Pull Out Reason
                    verificationDao.InsertPullOutInfo(collection, CurrentUser.Account);
                    //check next available cheque
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }

                }
                else
                {
                    //Pull out process here
                    verificationDao.VerificationPullOut(collection, CurrentUser.Account);

                    //Insert to cheque history
                    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                    //Insert Pull Out Reason
                    verificationDao.InsertPullOutInfo(collection, CurrentUser.Account);

                    //check next available cheque
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //if next cheque is not available.. check previous instead

                if (staskid != "306210" && staskid != "306220" && staskid != "306230" && staskid != "306520" && staskid != "306550")
                {
                    if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                        resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    }
                }
                if (gQueueSqlConfig.TaskId == "306550" || gQueueSqlConfig.TaskId == "306510")
                {
                    ViewBag.InwardItemViewModel = resultModel;
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
                }
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
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
                }
                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];



            }
            if ((resultModel.getField("NextValue") == "" && resultModel.getField("PreviousValue") == "") ||
                            (collection["NextValue"] == "" && collection["PreviousValue"] == "")) // if NON VERIRFICATION (just a single cheque)
            {
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
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
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
            }
            // VERIFICATION 
            else
            {
                int remaining = 0;
                if (errorMessages.Count > 0)
                {
                    remaining = Int32.Parse(resultModel.getField("TotalUnverified")) + 1;
                    resultModel.allFields["TotalUnverified"] = remaining.ToString();
                }
                else
                {
                    remaining = Int32.Parse(resultModel.getField("TotalUnverified"));
                }
                // if cheque available.. render cheque page
                if ((!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) && !(errorMessages.Count == 0 && collection["NextValue"] == ""))
                {

                    ViewBag.InwardItemViewModel = resultModel;
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
                    ViewBag.Micr = commonInwardItemDao.GetMicr();
                    //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                    //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                    return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
            }
        }


        public async Task<ActionResult> VerificationRepair(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationRepair;// E = Repair Item
            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
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
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
                {
                    //Repair process here
                    verificationDao.VerificationRepairNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                    //check next available cheque
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }

                }
                else
                {
                    //check next available cheque
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                {
                    result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }
                if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306220" || staskid == "306230" || staskid == "306530" || staskid == "306540" || staskid == "306510" || staskid == "306520")
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
                    //Repair process here
                    verificationDao.VerificationRepair(collection, CurrentUser.Account);

                    //Insert to cheque history
                    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                }
                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;

                // if cheque available.. render cheque page
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
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
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult> ReviewAction(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.ReviewAction;
            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            ViewBag.TaskId = staskid;
            ViewBag.DataAction = collection["DataAction"].ToString().Trim();
            Dictionary<string, string> result;
            if ((errorMessages.Count > 0))
            {
                if (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage")
                {
                    result = commonInwardItemDao.ErrorChequeWithoutLock(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                else
                {
                    result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                }

                //result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
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
                verificationDao.VerificationReview(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);

                if (staskid == "306510" && collection["DataAction"].ToString().Trim() == "ChequeVerificationPage")
                {
                    collection["fldInwardItemId"] = collection["NextValue"];
                    result = commonInwardItemDao.NextChequeNoLock(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                else
                {
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                }


                //result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                {
                    result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }*/
                ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
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
                //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            }

            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
            {
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
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
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            else
            {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }






        public virtual async Task<ActionResult> CreditPosting(FormCollection collection)
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
            Dictionary<string, string> result;
            string verifyAction = VerificationStatus.ACTION.VerificationReturn;// A = Approve 
            List<string> errorMessages = new List<string>();

            ViewBag.TaskId = staskid;

            //string taskid = gQueueSqlConfig.TaskId;
            collection.Add("fldTaskId", staskid);
            ViewBag.Title = "CreditPosting";



            if (!String.IsNullOrEmpty(collection["fldUIC2"]))
            {
                commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
            }
            //check next available cheque
            Log(DateTime.Now + ":Check available check ", CurrentUser.Account.BankCode);
            if (staskid == "306260")
            {

                ViewBag.Message = verificationDao.CreditPostingMarked(collection["fldInwardItemId"], collection);

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

            if (ViewBag.Message != "" && ViewBag.Message != "Request Processed Successfully")
            {
                resultModel = InwardItemConcern.ChequePopulateViewModel(gQueueSqlConfig, result, collection, ViewBag.Message);
                errorMessages = verificationDao.ValidateVerificationService(collection, CurrentUser.Account, verifyAction, staskid, ViewBag.Message);
            }
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
            if (errorMessages.Count == 0)
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
                //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);

                //Approve Process
                verificationDao.VerificationApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

                //Insert to cheque history
                commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
            }

            //Minus Record Indicator
            ViewBag.MinusRecordIndicator = true;
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];





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
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
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
        public virtual ActionResult PullOutReason(FormCollection collection)
        {
            //await initializeBeforeAction();
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

            //return PartialView("InwardClearing/Modal/_PulloutPopup");
            return PartialView("VerificationPullOut");
        }




    }
}