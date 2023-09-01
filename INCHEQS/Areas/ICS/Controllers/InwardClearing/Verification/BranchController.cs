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
using INCHEQS.Models.Signature;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.PullOutReason;
using INCHEQS.Areas.ICS.Models.MICRImage;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.Verification {
    public class BranchController : ICCSBaseController {

        public BranchController(IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao, IVerificationDao verificationDao, IReportService reportService, IHostReturnReasonDao hostReturnReasonDao, IBankHostStatusKBZDao bankHostStatusKBZDao, IBankHostStatusDao bankHostStatusDao , UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao, ISignatureDao signatureDao, INonConformanceFlagDao nonConformanceFlagDao, IPullOutReasonDao pullOutReasonDao, IMICRImageDao micrImageDao) : base(pageConfigDao, commonInwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao , reportService, hostReturnReasonDao, bankHostStatusKBZDao, bankHostStatusDao, userDao, largeAmountDao, systemProfileDao, signatureDao, nonConformanceFlagDao, pullOutReasonDao, micrImageDao) 
        {
            
        }

        protected override string initializeQueueTaskId() {
            return RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
        }

        public async Task<ActionResult> BranchApprove(FormCollection collection) {
            await initializeBeforeAction();
            collection["new_textRejectCode"] = "000";
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationApprove;
            Log(DateTime.Now + ":Get inward item id :" + inwardItemId, CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward accountnumber :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward check no :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward check no :" + collection["imageId"], CurrentUser.Account.BankCode);
            List<string> errorMessages = new List<string>(); //verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchApproveMaker);
            //List<string> errorlockedcheck = verificationDao.LockedCheck(collection, CurrentUser.Account);
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            ViewBag.Type = "Branch";
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.TaskId = staskid;
            //Boolean VerifyClassInd =  verificationDao.VerifyClassLimit(collection, CurrentUser.Account);

            //check if validate contain error

            if (errorMessages.Count > 0)
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
            } else {

                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }


                //check next available cheque
                if (/*staskid == "308140" || staskid == "308130" ||*/ staskid == "308110" || staskid == "308120" || staskid == "308170" || staskid == "308190")
                {
                    //Branch Approve Process
                    //verificationDao.BranchApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationApprove, VerifyClassInd);
                    verificationDao.BranchApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationApprove, true);

                    if (staskid != "308190")
                    {
                        result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        collection["NextVerifyBranch"] = "Verify";
                        result = commonInwardItemDao.NextChequeNoLockBranch(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    

                    if (result == null)
                    {
                        // Azim Start 8 June 2021
                        //if (collection["DataAction"].ToString().Trim() == "ChequeVerificationPage")
                        //{
                        //    result = commonInwardItemDao.ErrorChequeWithoutLock(gQueueSqlConfig, collection, CurrentUser.Account);
                        //}
                        //else
                        //{
                        //    result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                        //}

                        if (result == null)
                        {
                            //TempData["ErrorMsg"] = errorMessages;
                            return View("InwardClearing/Base/_EmptyChequeVerification");
                        }

                        resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                        ViewBag.InwardItemViewModel = resultModel;
                    }
                }
                else if (staskid == "308150" || staskid == "308160")
                {
                    //Branch Large Amt Approve Process
                    //verifyAction = VerificationStatus.ACTION.BranchLargeAmtApproveMaker;// M = Branch Large Amt Approve Maker 
                    verificationDao.VerificationApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);

                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                else
                {
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120" || staskid == "308170" || staskid == "308190")
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }*/


                /*if (staskid != "308140" && staskid != "308130" && staskid != "308110" && staskid != "308120" && staskid !=  "308150" && staskid != "308160" &&
                    staskid != "308170" && staskid != "308190")
                {
                    //Branch Approve Process
                    verificationDao.BranchApprove(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);
                }*/




                //If fldCheckerMaker from tblQueueConfig is C = (Checker)
                //if ("Checker".Equals(gQueueSqlConfig.TaskRole)) {
                //    string verifyAction = VerificationStatus.ACTION.BranchApproveChecker;// G = Branch Approve Checker
                //    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //}
                ////If fldCheckerMaker from tblQueueConfig is M = (Maker)
                //else if ("Maker".Equals(gQueueSqlConfig.TaskRole)) {
                //    string verifyAction = VerificationStatus.ACTION.BranchApproveMaker;// H = Branch Approve Maker
                //    commonInwardItemDao.InsertChequeHistory(collection, verifyAction, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //}
            }
            //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            //ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
            //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
            //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
            //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
            ViewBag.MinusRecordIndicator = true;
            //ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];

            //add new thing
            ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
            ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
            //ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
            ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
            ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
            ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
            //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
            //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
            ViewBag.Micr = commonInwardItemDao.GetMicr();
            //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
            //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
            //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
            //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
            //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();

            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) {
                ViewBag.InwardItemViewModel = resultModel;

                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult> BranchReturn(FormCollection collection) {
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
            List<string> errorMessages = verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchReturnMaker);
            Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);
            ViewBag.Type = "Branch";
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.TaskId = staskid;
            //Boolean VerifyClassInd = verificationDao.VerifyClassLimit(collection, CurrentUser.Account);

            //check if validate contain error
            if ((errorMessages.Count > 0)) {
                if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308180" || staskid == "308200")
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
                else {
                    result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                    resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            } else {
                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }
                //check next available cheque
                if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120" || staskid == "308180" || staskid == "308200")
                {
                    verificationDao.BranchReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationReturn, true);

                    if (staskid != "308200")
                    {
                        result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        collection["NextVerifyBranch"] = "Verify";
                        result = commonInwardItemDao.NextChequeNoLockBranch(gQueueSqlConfig, collection, CurrentUser.Account);
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
                else if (staskid == "308150" || staskid == "308160")
                {
                    //Branch Approve Process
                    verificationDao.VerificationReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                else
                {
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                /*if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120" || staskid == "308150" || staskid == "308180" || staskid == "308200")
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }*/

                /*if (staskid != "308140" && staskid != "308130" && staskid != "308110" && staskid != "308120"
                    && staskid !=  "308150" && staskid != "308180" && staskid != "308200")
                {
                    verificationDao.BranchReturn(collection, CurrentUser.Account, gQueueSqlConfig.TaskRole);
                }*/



                //if ("Checker".Equals(gQueueSqlConfig.TaskRole)) {
                //    commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReturnChecker, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //}
                //else if ("Maker".Equals(gQueueSqlConfig.TaskRole)) {
                //    commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReturnMaker, CurrentUser.Account, gQueueSqlConfig.TaskId);
                //}
            }
            //ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            //ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
            //ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
            //ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
            //ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();

            ViewBag.MinusRecordIndicator = true;
            //ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
            ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
            //ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.HostStatus2 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus2"]);
            ViewBag.HostStatus3 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus3"]);
            ViewBag.HostStatus4 = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus4"]);
            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRulesList = signatureDao.GetSignatureRulesInfoList(resultModel.allFields["fldHostAccountNo"]);
            ViewBag.NCFDesc = resultModel.getField("ncfdesc").Trim();
            ViewBag.NCFDesc2 = resultModel.getField("ncfdesc2").Trim();
            ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
            ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
            ViewBag.DocToFollow = resultModel.getField("fldDocToFollow").Trim();
            ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
            ViewBag.Micr = commonInwardItemDao.GetMicr();
            //ViewBag.StatusDesc = resultModel.allFields["fldBankHostStatusDesc"];
            //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];

            //ViewBag.SignatureInfo = signatureDao.GetSignatureInformation(resultModel.allFields["fldHostAccountNo"]);
            //ViewBag.SignatureRules = signatureDao.GetSignatureRulesInfo(resultModel.allFields["fldHostAccountNo"]);

            //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult> BranchReferBack(FormCollection collection) {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            List<string> errorMessages = new List<string>();// verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchReferBackChecker);
            Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            //check if validate contain error
            if ((errorMessages.Count > 0))
                {
                result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return View("InwardClearing/Base/_EmptyChequeVerification");
                }
                resultModel = InwardItemConcern.InwardItemWithErrorMessages(gQueueSqlConfig, result, errorMessages);
            } else {
                if (!String.IsNullOrEmpty(collection["fldUIC2"]))
                {
                    commonInwardItemDao.DeleteTempGif(collection["fldUIC2"]);
                }
                //check next available cheque
                if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120")
                {
                    //verificationDao.BranchReferBack(collection, CurrentUser.Account);
                    verificationDao.BranchReferBackNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.BranchReferBackChecker, true);
                    //commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReferBackChecker, CurrentUser.Account, gQueueSqlConfig.TaskId);
                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("InwardClearing/Base/_EmptyChequeVerification");
                    }
                }
                else
                {
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                //if next cheque is not available.. check previous instead
                if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"])) {
                    if (staskid == "308140" || staskid == "308130" || staskid == "308110" || staskid == "308120")
                    {
                        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    else
                    {
                        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                }
                if (staskid != "308140" && staskid != "308130" && staskid != "308110" && staskid != "308120")
                {
                    //verificationDao.BranchReferBack(collection, CurrentUser.Account);
                    verificationDao.BranchReferBackNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.BranchReferBackChecker, true);
                    commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReferBackChecker, CurrentUser.Account, gQueueSqlConfig.TaskId);
                }
            }
            ViewBag.IQA = resultModel.getField("fldIQA").Trim();
            ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
            ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
            ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
            ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
            ViewBag.MinusRecordIndicator = true;
            ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
            //ViewBag.MABHostReturnStatus = bankHostStatusKBZDao.GetMABBankHostReturnStatus(collection["fldInwardItemId"]);
            ViewBag.HostStatus = bankHostStatusDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
            ViewBag.ChequeHistory = commonInwardItemDao.ChequeHistory(collection["NextValue"].ToString());
            // if cheque available.. render cheque page
            if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) {
                ViewBag.InwardItemViewModel = resultModel;
                return View("InwardClearing/ICCSDefault/ChequeVerificationPage");
            }
            // if not.. render empty cheque with close button
            else {
                return View("InwardClearing/Base/_EmptyChequeVerification");
            }
        }

        public async Task<ActionResult>  BranchRemarks(FormCollection collection)
        {

            ViewBag.new_textRejectCode = collection["new_textRejectCode"];
            ViewBag.DataAction = collection["DataAction"];
            ViewBag.fldUIC2 = collection["fldUIC2"];
            ViewBag.current_fldAccountNumber = collection["current_fldAccountNumber"];
            ViewBag.current_fldChequeSerialNo = collection["current_fldChequeSerialNo"];
            ViewBag.current_fldUIC = collection["current_fldUIC"];
            ViewBag.fldInwardItemId = collection["fldInwardItemId"];
            ViewBag.textAreaRemarks = collection["textAreaRemarks"];
            ViewBag.fldIssueBankBranch = collection["fldIssueBankBranch"];
            ViewBag.fldClearDate = collection["fldClearDate"];
            ViewBag.NextValue = collection["NextValue"];
            ViewBag.PreviousValue = collection["PreviousValue"];
            ViewBag.BranchActivation = collection["BranchActivation"];

            ViewBag.TaskId = staskid;


            return View("InwardClearing/Modal/_BranchRemarks");

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
    }
}