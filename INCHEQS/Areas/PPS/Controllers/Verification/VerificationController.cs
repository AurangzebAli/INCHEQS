using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
//using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Report;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.Sequence;
using INCHEQS.Security.User;
using INCHEQS.Areas.PPS.Models.Verification;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.ConfigVerification.VerificationLimit;
using System.Data;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Models.Signature;

namespace INCHEQS.Areas.PPS.Controllers.Verification
{
    public class VerificationController : ICCSBaseController
    {

        public VerificationController(IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportService reportService, IHostReturnReasonDao hostReturnReasonDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, IPullOutReasonDao pullOutReasonDao, ISignatureDao signatureDao) :
            base(pageConfigDao, commonInwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, userDao, largeAmountDao, systemProfileDao, pullOutReasonDao, signatureDao)
        { }

        protected override string initializeQueueTaskId()
        {
            return RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
        }

        public async Task<ActionResult> VerificationApprove(FormCollection collection)
        {

            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
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

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                Log(DateTime.Now + ":Validate verification fail ", CurrentUser.Account.BankCode);
                //result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);

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
                    return View("PositivePay/Base/_EmptyChequeVerification");
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
                //if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210")
                //{
                //Approve Process//Insert to cheque history
                verificationDao.VerificationApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);

                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    //return View("PositivePay/Base/_EmptyChequeVerification");
                    
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
                        return View("PositivePay/Base/_EmptyChequeVerification");
                    }

                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                    ViewBag.InwardItemViewModel = resultModel;
                    //Azim End
                    //return View("InwardClearing/Base/_EmptyChequeVerification");
                    
                }
                //}
                //else
                //{
                //    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    if (result == null)
                //    {
                //        return View("PositivePay/Base/_EmptyChequeVerification");
                //    }

                //}

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                ////if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{
                //    Log(DateTime.Now + ":Check available check fail ", CurrentUser.Account.BankCode);
                //    if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210")
                //    {
                //        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    else
                //    {
                //        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}

                //if (staskid == "306910" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550" || staskid == "306210")
                //{
                //    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //    //add new thing
                //    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                //}
                //else
                //{
                //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                ////add new thing
                //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                ViewBag.DataAction = collection["DataAction"];

                //Approve Process
                //verificationDao.VerificationApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

                ////Insert to cheque history
                //commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //}

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
                //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //add new thing
                //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.TaskId = gQueueSqlConfig.TaskId;
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                return View("PositivePay/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.InwardItemViewModel = resultModel;
                return View("PositivePay/Base/_EmptyChequeVerification");
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

            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            Dictionary<string, string> result;// = commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                //result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);

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
                    return View("PositivePay/Base/_EmptyChequeVerification");

                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            }
            else
            {
                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }

                verificationDao.VerificationReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    //return View("PositivePay/Base/_EmptyChequeVerification");
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
                        return View("PositivePay/Base/_EmptyChequeVerification");
                    }
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{
                //    if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550")
                //    {
                //        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    else
                //    {
                //        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);
                ViewBag.DataAction = collection["DataAction"];
                //if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130" || staskid == "306550")
                //{
                //    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
                //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //    //add new thing
                //    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                //}
                //else
                //{
                //    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                //    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //    //add new thing
                //    ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //    ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //    ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //    ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //    ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //    ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                //    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                //    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                //    //Return Process
                //    verificationDao.VerificationReturn(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);

                //    //Insert to cheque history
                //    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

                //}
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
                //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                //add new thing
                //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.TaskId = gQueueSqlConfig.TaskId;
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                return View("PositivePay/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
                ViewBag.Micr = commonInwardItemDao.GetMicr();
                ViewBag.InwardItemViewModel = resultModel;
                return View("PositivePay/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult> VerificationRoute(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationRoute;// B = Route to branch

            if (collection["new_textRejectCode"].Trim().Length == 1)
            {
                collection["new_textRejectCode"] = "00" + collection["new_textRejectCode"];
            }
            else if (collection["new_textRejectCode"].Trim().Length == 2)
            {
                collection["new_textRejectCode"] = "0" + collection["new_textRejectCode"];
            }

            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                //result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);

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
                    return View("PositivePay/Base/_EmptyChequeVerification");
                    //if (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage")
                    //{
                    //    result = commonInwardItemDao.ErrorChequeWithoutLock(gQueueSqlConfig, collection, CurrentUser.Account);
                    //}
                    //else
                    //{
                    //    result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    //}

                    //if (result == null)
                    //{
                    //    //TempData["ErrorMsg"] = errorMessages;
                    //    return View("InwardClearing/Base/_EmptyChequeVerification");
                    //}
                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            }
            else
            {
                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }

                verificationDao.VerificationRouteNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    //return View("PositivePay/Base/_EmptyChequeVerification");
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
                        return View("PositivePay/Base/_EmptyChequeVerification");
                    }
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                ////if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{

                //    result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);

                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}


                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                ViewBag.TaskId = gQueueSqlConfig.TaskId;

                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];

            }
            ViewBag.DataAction = collection["DataAction"];
            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
            {
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.TaskId = gQueueSqlConfig.TaskId;
                ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(resultModel.allFields["fldInwardItemId"]);

                return View("PositivePay/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                ViewBag.InwardItemViewModel = resultModel;
                return View("PositivePay/Base/_EmptyChequeVerification");
            }
        }


        public async Task<ActionResult> VerificationPullOut(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationPullOut;// P = Pull Out Item
            List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction, staskid);
            Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
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
                result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);
                ////if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{
                //    result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}



                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                ViewBag.TaskId = gQueueSqlConfig.TaskId;
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);

            }

            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
            {
                ViewBag.InwardItemViewModel = resultModel;
                ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);

                return View("PositivePay/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else
            {
                ViewBag.InwardItemViewModel = resultModel;
                return View("PositivePay/Base/_EmptyChequeVerification");
            }
        }


        //public async Task<ActionResult> VerificationRepair(FormCollection collection)
        //{
        //    await initializeBeforeAction();
        //    string inwardItemId = collection["fldInwardItemId"];
        //    string verifyAction = VerificationStatus.ACTION.VerificationRepair;// E = Repair Item
        //    List<string> errorMessages = verificationDao.ValidateVerification(collection, CurrentUser.Account, verifyAction);
        //    Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

        //    //check if validate contain error
        //    if ((errorMessages.Count > 0))
        //    {
        //        result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //        if (result == null)
        //        {
        //            return View("InwardClearing/Base/_EmptyChequeVerification");
        //        }
        //        resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
        //    }
        //    else
        //    {
        //        if (!String.IsNullOrEmpty(collection["fldUIC2"]))
        //        {
        //            commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
        //        }
        //        if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130")
        //        {
        //            //Repair process here
        //            verificationDao.VerificationRepairNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
        //            //check next available cheque
        //            result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
        //            if (result == null)
        //            {
        //                return View("InwardClearing/Base/_EmptyChequeVerification");
        //            }

        //        }
        //        else
        //        {
        //            //check next available cheque
        //            result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //            if (result == null)
        //            {
        //                return View("InwardClearing/Base/_EmptyChequeVerification");
        //            }
        //        }

        //        resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

        //        //if next cheque is not available.. check previous instead
        //        if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
        //        {
        //            result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //            resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
        //        }
        //        if (staskid == "306910" || staskid == "306210" || staskid == "306920" || staskid == "306930" || staskid == "308140" || staskid == "308130")
        //        {
        //            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
        //            ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
        //            ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
        //            ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
        //            ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
        //            //add new thing
        //            ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
        //            ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
        //            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
        //            ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
        //            ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
        //            ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
        //            ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
        //            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
        //            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
        //        }
        //        else
        //        {
        //            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
        //            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
        //            ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
        //            ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
        //            ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
        //            //add new thing
        //            ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
        //            ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
        //            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
        //            ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
        //            ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
        //            ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
        //            ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
        //            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
        //            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
        //            //Repair process here
        //            verificationDao.VerificationRepair(collection, CurrentUser.Account);

        //            //Insert to cheque history
        //            commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);

        //        }
        //        //Minus Record Indicator
        //        ViewBag.MinusRecordIndicator = true;

        //        // if cheque available.. render cheque page
        //        if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
        //        {
        //            ViewBag.InwardItemViewModel = resultModel;
        //            return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
        //        }
        //        // if not.. render empty cheque with close button
        //        else
        //        {
        //            return View("InwardClearing/Base/_EmptyChequeVerification");
        //        }
        //    }

        //    // if cheque available.. render cheque page
        //    if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
        //    {
        //        ViewBag.InwardItemViewModel = resultModel;
        //        ViewBag.IQA = resultModel.getField("fldIQA").Trim();
        //        ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
        //        ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
        //        ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
        //        ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
        //        //add new thing
        //        ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
        //        ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
        //        ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
        //        ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
        //        ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
        //        ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
        //        ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
        //        ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
        //        ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
        //        //Minus Record Indicator
        //        ViewBag.MinusRecordIndicator = true;
        //        ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
        //        return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
        //    }
        //    // if not.. render empty cheque with close button
        //    else
        //    {
        //        return View("InwardClearing/Base/_EmptyChequeVerification");
        //    }
        //}

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



    }
}
