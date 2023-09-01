using INCHEQS.Areas.ICS.Concerns;
using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.ConfigVerification.LargeAmount;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.PPS.Models.Verification;
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
using INCHEQS.Areas.PPS.Controllers.Verification;
using INCHEQS.Areas.ICS.Models.PullOutReason;

namespace INCHEQS.Areas.PPS.Controllers.InwardClearing.Verification
{
    public class BranchController : ICCSBaseController
    {

        public BranchController(IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, ISearchPageService searchPageService, IAuditTrailDao auditTrailDao, ISequenceDao sequenceDao,
            INCHEQS.Areas.PPS.Models.Verification.IVerificationDao verificationDao, IReportService reportService, IHostReturnReasonDao hostReturnReasonDao, UserDao userDao, ILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao,IPullOutReasonDao pullOutReasonDao) : base(pageConfigDao, commonInwardItemDao, searchPageService, auditTrailDao, sequenceDao, verificationDao, reportService, hostReturnReasonDao, userDao, largeAmountDao, systemProfileDao, pullOutReasonDao) { }

        protected override string initializeQueueTaskId()
        {
            return RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
        }


        public async Task<ActionResult> BranchApprove(FormCollection collection)
        {
            await initializeBeforeAction();

            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationApprove;
            Log(DateTime.Now + ":Get inward item id :" + inwardItemId, CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward accountnumber :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward check no :" + collection["fldAccountNumber"], CurrentUser.Account.BankCode);
            Log(DateTime.Now + ":Get inward check no :" + collection["imageId"], CurrentUser.Account.BankCode);
            List<string> errorMessages = verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchApproveMaker);
            List<string> errorlockedcheck = verificationDao.LockedCheck(collection, CurrentUser.Account);
            Dictionary<string, string> result;//= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            Boolean VerifyClassInd = true; // verificationDao.VerifyClassLimit(collection, CurrentUser.Account);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                if (result == null)
                {
                    result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
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
                if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "308180" || staskid == "308110" || staskid == "308120")
                {
                    //Branch Approve Process
                    verificationDao.BranchApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationApprove, VerifyClassInd);

                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("PositivePay/Base/_EmptyChequeVerification");
                    }
                }
                else
                {
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                }

                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                ////if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{
                //    if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "308180" || staskid == "308110" || staskid == "308120")
                //    {
                //        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    else
                //    {
                //        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}

                if (staskid == "308150")
                {
                    //Branch Approve Process
                    verificationDao.VerificationApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                }


                if (staskid != "308140" && staskid != "308130" && staskid != "308160" && staskid != "308170" && staskid != "308110" && staskid != "308120" && staskid != "308150")
                {
                    //Branch Approve Process
                    verificationDao.BranchApproveNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationApprove, VerifyClassInd);
                }


                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];

            }



            ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
            ViewBag.TaskId = gQueueSqlConfig.TaskId;

            if (resultModel.getField("NextValue") == "" && resultModel.getField("PreviousValue") == "") // if NON VERIRFICATION (just a single cheque)
            {
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator

                    ViewBag.TotalUnverified = resultModel.getField("TotalUnverified").Trim();

                    ViewBag.MinusRecordIndicator = false;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    return View("PositivePay/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    return View("PositivePay/Base/_EmptyChequeVerification");
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
                if ((!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) && remaining > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator

                    ViewBag.TotalUnverified = resultModel.getField("TotalUnverified").Trim();

                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    return View("PositivePay/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
            }
        }

        public async Task<ActionResult> BranchReturn(FormCollection collection)
        {
            await initializeBeforeAction();
            string inwardItemId = collection["fldInwardItemId"];
            string verifyAction = VerificationStatus.ACTION.VerificationReturn;// R = Return 
            List<string> errorMessages = verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchReturnMaker);
            Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

            Boolean VerifyClassInd = true; //verificationDao.VerifyClassLimit(collection, CurrentUser.Account);

            //check if validate contain error
            if ((errorMessages.Count > 0))
            {
                if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "308110" || staskid == "308120")
                {
                    result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                    }
                }
                else
                {
                    result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
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
                if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "308110" || staskid == "308120")
                {
                    verificationDao.BranchReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationReturn, VerifyClassInd);

                    result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                    if (result == null)
                    {
                        return View("PositivePay/Base/_EmptyChequeVerification");
                    }
                }
                else
                {
                    result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                }
                resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

                ////if next cheque is not available.. check previous instead
                //if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
                //{
                //    if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308170" || staskid == "308110" || staskid == "308120")
                //    {
                //        result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    else
                //    {
                //        result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
                //    }
                //    resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
                //}
                if (staskid == "308150")
                {
                    //Branch Approve Process
                    verificationDao.VerificationReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, verifyAction);
                }

                if (staskid != "308140" && staskid != "308130" && staskid != "308160" && staskid != "308170" && staskid != "308110" && staskid != "308120"
                    && staskid != "308150")
                {

                    verificationDao.BranchReturnNew(collection, CurrentUser.Account, gQueueSqlConfig, VerificationStatus.ACTION.VerificationApprove, VerifyClassInd);
                }


                //Minus Record Indicator
                ViewBag.MinusRecordIndicator = true;
                ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];

            }
            ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
            ViewBag.TaskId = gQueueSqlConfig.TaskId;

            if (resultModel.getField("NextValue") == "" && resultModel.getField("PreviousValue") == "") // if NON VERIRFICATION (just a single cheque)
            {
                if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    //ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    //ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator

                    ViewBag.TotalUnverified = resultModel.getField("TotalUnverified").Trim();

                    ViewBag.MinusRecordIndicator = false;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    return View("PositivePay/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
            }
            // VERIFICATION 
            else
            {

                // if cheque available.. render cheque page
                if ((!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0) && Int32.Parse(resultModel.getField("TotalUnverified")) > 0)
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
                    ViewBag.IQADesc = resultModel.getField("flddesc").Trim();
                    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
                    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
                    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
                    //add new thing
                    //ViewBag.RejectStatus1 = resultModel.allFields["fldRejectStatus1"];
                    //ViewBag.RejectStatus2 = resultModel.allFields["fldRejectStatus2"];
                    //ViewBag.HostStatus = hostReturnReasonDao.GetHostReturnReasonDesc(resultModel.allFields["fldRejectStatus1"]);
                    //ViewBag.NCFDesc = resultModel.allFields["ncfdesc"];
                    //ViewBag.NCFDesc2 = resultModel.allFields["ncfdesc2"];
                    //ViewBag.StatusDesc = resultModel.allFields["fldStatusDesc"];
                    //ViewBag.StatusDesc2 = resultModel.allFields["fldStatusDesc2"];
                    ViewBag.NCFFlag = resultModel.getField("fldNonConformance").Trim();
                    ViewBag.NCFFlag2 = resultModel.getField("fldNonConformance2").Trim();
                    //Minus Record Indicator

                    ViewBag.TotalUnverified = resultModel.getField("TotalUnverified").Trim();

                    ViewBag.MinusRecordIndicator = true;
                    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
                    return View("PositivePay/ICCSDefault/ChequeVerificationPage");
                }
                // if not.. render empty cheque with close button
                else
                {
                    ViewBag.InwardItemViewModel = resultModel;
                    ViewBag.ChequeInfo = verificationDao.getChequeInfo(resultModel.allFields["fldInwardItemId"], resultModel.allFields["fldUIC"]);
                    return View("PositivePay/Base/_EmptyChequeVerification");
                }
            }
        }

        //public async Task<ActionResult> BranchReferBack(FormCollection collection)
        //{
        //    await initializeBeforeAction();
        //    string inwardItemId = collection["fldInwardItemId"];
        //    List<string> errorMessages = verificationDao.ValidateBranch(collection, CurrentUser.Account, VerificationStatus.ACTION.BranchReferBackChecker);
        //    Dictionary<string, string> result; //= commonInwardItemDao.FindItemByInwardItemId(gQueueSqlConfig, collection, CurrentUser.Account, inwardItemId);

        //    //check if validate contain error
        //    if ((errorMessages.Count > 0))
        //    {
        //        result = commonInwardItemDao.ErrorChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
        //        if (result == null)
        //        {
        //            result = commonInwardItemDao.ErrorCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //        }
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
        //        //check next available cheque
        //        if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308110" || staskid == "308120")
        //        {
        //            verificationDao.BranchReferBack(collection, CurrentUser.Account);
        //            commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReferBackChecker, CurrentUser.Account, gQueueSqlConfig.TaskId);
        //            result = commonInwardItemDao.NextChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
        //            if (result == null)
        //            {
        //                return View("InwardClearing/Base/_EmptyChequeVerification");
        //            }
        //        }
        //        else
        //        {
        //            result = commonInwardItemDao.NextCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //        }
        //        resultModel = InwardItemConcern.NextChequePopulateViewModel(gQueueSqlConfig, result, collection);

        //        //if next cheque is not available.. check previous instead
        //        if (inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]))
        //        {
        //            if (staskid == "308140" || staskid == "308130" || staskid == "308160" || staskid == "308110" || staskid == "308120")
        //            {
        //                result = commonInwardItemDao.PrevChequeNew(gQueueSqlConfig, collection, CurrentUser.Account);
        //            }
        //            else
        //            {
        //                result = commonInwardItemDao.PrevCheque(gQueueSqlConfig, collection, CurrentUser.Account);
        //            }
        //            resultModel = InwardItemConcern.PrevChequePopulateViewModel(gQueueSqlConfig, result, collection);
        //        }
        //        if (staskid != "308140" && staskid != "308130" && staskid != "308160" && staskid != "308110" && staskid != "308120")
        //        {
        //            verificationDao.BranchReferBack(collection, CurrentUser.Account);
        //            commonInwardItemDao.InsertChequeHistory(collection, VerificationStatus.ACTION.BranchReferBackChecker, CurrentUser.Account, gQueueSqlConfig.TaskId);
        //        }
        //    }
        //    ViewBag.IQA = resultModel.getField("fldIQA").Trim();
        //    ViewBag.IQADesc = resultModel.getField("fldDesc").Trim();
        //    ViewBag.MICRCorrection = resultModel.getField("fldMicrCorrectionInd").Trim();
        //    ViewBag.MICRCorrectionDesc = resultModel.getField("MICRCrrectionDesc").Trim();
        //    ViewBag.DLLStatus = resultModel.getField("fldDLLStatusDesc").Trim();
        //    ViewBag.MinusRecordIndicator = true;
        //    ViewBag.LargeAmount = largeAmountDao.GetLargeAmount().Rows[0]["fldAmount"];
        //    // if cheque available.. render cheque page
        //    if (!inwardItemId.Equals(resultModel.allFields["fldInwardItemId"]) || errorMessages.Count > 0)
        //    {
        //        ViewBag.InwardItemViewModel = resultModel;
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