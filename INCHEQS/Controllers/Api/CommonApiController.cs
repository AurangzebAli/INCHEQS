using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.State;
//using INCHEQS.Models.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.UserSession;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.TransactionType;
using INCHEQS.Areas.OCS.Models.Capturing;
using INCHEQS.Areas.OCS.Models.ChequeDataEntry;
using INCHEQS.Areas.OCS.Models.BranchEndOfDay;
using INCHEQS.Areas.ICS.Models.BranchSubmission;
using INCHEQS.Areas.OCS.Models.BankBranchesOcs;
using INCHEQS.Areas.OCS.Models.ChequeImage;
using INCHEQS.Areas.ICS.Models.ChequeImage;
using System.Data;
using INCHEQS.Areas.OCS.Models.OutwardClearingICL;
using INCHEQS.Areas.OCS.Models.Balancing;
using INCHEQS.Areas.ICS.Models.OutwardReturnICL;
using INCHEQS.Models.Signature;
using INCHEQS.Areas.OCS.Models.OCSProcessDate;
using INCHEQS.Areas.COMMON.Models.BankHostStatus;
using INCHEQS.TaskAssignment;
namespace INCHEQS.Controllers.Api
{
    public class CommonApiController : BaseController {

        private readonly IStateDao stateDao;
        private readonly IUserSessionDao sessionDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ICommonInwardItemDao commonInwardItemDao;
        private readonly IBranchActivationDao branchActivationDao;
        private readonly IBranchEndOfDayDao branchEndOfDayDao;
        private readonly IBranchSubmissionDao BranchSubmissionDao;
        private readonly IBankBranchesOcsDao BankBranchesOcsDao;
        private readonly IOutwardClearingICLDao OutwardClearingICLDao;
        private readonly IOutwardReturnICLDao outwardReturnICLDao;
        private readonly ISignatureDao signatureDao;
        private readonly IOCSProcessDateDao OCSProcessDateDao;
        
        //OCS
        private readonly ITransactionTypeDao transactionTypeDao;
        private readonly IChequeDataEntryDao ChequeDataEntryDao;
        private readonly ICapturingDao capturingDao;
        private readonly IOCSChequeImageDao ocsChequeImageDao;
        private readonly IICSChequeImageDao icsChequeImageDao;
        private readonly ITransactionBalancingDao TransactionBalancingDao;
        private readonly IBankHostStatusDao bankHostStatusDao;

        public CommonApiController(IStateDao stateDao, IUserSessionDao sessionDao, IAuditTrailDao auditTrailDao, ICommonInwardItemDao commonInwardItemDao, IBranchActivationDao branchActivationDao, ITransactionTypeDao transactionTypeDao, ICapturingDao capturingDao, IChequeDataEntryDao ChequeDataEntryDao, IBranchEndOfDayDao branchEndOfDayDao, IBranchSubmissionDao BranchSubmissionDao, IBankBranchesOcsDao BankBranchesOcsDao, IOCSChequeImageDao ocsChequeImageDao, IICSChequeImageDao icsChequeImageDao,  IOutwardClearingICLDao OutwardClearingICLDao, ITransactionBalancingDao TransactionBalancingDao, IOutwardReturnICLDao outwardReturnICLDao, ISignatureDao signatureDao, IOCSProcessDateDao OCSProcessDateDao, IBankHostStatusDao bankHostStatusDao) {
            this.stateDao = stateDao;
            this.sessionDao = sessionDao;
            this.auditTrailDao = auditTrailDao;
            this.commonInwardItemDao = commonInwardItemDao;
            this.branchActivationDao = branchActivationDao;
            this.transactionTypeDao = transactionTypeDao;
            this.capturingDao = capturingDao;
            this.ChequeDataEntryDao = ChequeDataEntryDao;
            this.branchEndOfDayDao = branchEndOfDayDao; 
            this.BranchSubmissionDao = BranchSubmissionDao; 
            this.BankBranchesOcsDao = BankBranchesOcsDao;
            this.ocsChequeImageDao = ocsChequeImageDao;
            this.icsChequeImageDao = icsChequeImageDao;
            this.OutwardClearingICLDao = OutwardClearingICLDao;
            this.TransactionBalancingDao = TransactionBalancingDao;
            this.outwardReturnICLDao = outwardReturnICLDao;
            this.signatureDao = signatureDao;
            this.OCSProcessDateDao = OCSProcessDateDao;
            this.bankHostStatusDao = bankHostStatusDao;
        }


        //[CustomAuthorize(TaskIds = "all")]
        //public JsonResult TransactionCodeList()
        //{
        //    Dictionary<string, string> transCodes = transactionCodeDao.FindAllTransCode();
        //    return Json(transCodes, JsonRequestBehavior.AllowGet);                
        //}


        [CustomAuthorize(TaskIds = "all")]
        public JsonResult StateList() {
            List<Dictionary<string, string>> stateCodes = Utils.ConvertDataTableToList(stateDao.getAllState());
            return Json(stateCodes, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult IssuingBankBranch()
        {
            List<Dictionary<string, string>> IssuingBankBranchs = Utils.ConvertDataTableToList(stateDao.GetAllIssuingBankBranch(CurrentUser.Account));
            return Json(IssuingBankBranchs, JsonRequestBehavior.AllowGet);
        }

        // xx start 20210426
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult IssueBankBranch(string bankCode)
        {
            List<Dictionary<string, string>> IssueBankBranch = Utils.ConvertDataTableToList(stateDao.GetIssueBankBranch(bankCode));
            return Json(IssueBankBranch, JsonRequestBehavior.AllowGet);
        }
        // xx end 20210426
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult PresentingBankBranch()
        {
            List<Dictionary<string, string>> PresentingBankBranchs = Utils.ConvertDataTableToList(stateDao.GetAllPresentingBankBranch());
            return Json(PresentingBankBranchs, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult HostStatusList()
        {
            List<Dictionary<string, string>> hostStatus = Utils.ConvertDataTableToList(bankHostStatusDao.GetAllHostStatus());
            return Json(hostStatus, JsonRequestBehavior.AllowGet);
        }


        [CustomAuthorize(TaskIds = "all")]
        public JsonResult ReturnReason()
        {
            List<Dictionary<string, string>> ReturnReasons = Utils.ConvertDataTableToList(stateDao.GetReturnReason());
            return Json(ReturnReasons, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetMessage() {
            string message = branchActivationDao.GetMessage();
            Response.BufferOutput = false;
            return Json(message, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public void RemoveCurrentUserSession() {
            sessionDao.DeleteSessionForUser(CurrentUser.Account.UserId, HttpContext.Session.SessionID);
            commonInwardItemDao.UnlockAllAssignedForUser(CurrentUser.Account);
            //auditTrailDao.Log("User Session Cleared" + Utils.getRemoteAddress(Request), CurrentUser.Account);
            auditTrailDao.SecurityLog("User Session Cleared" + Utils.getRemoteAddress(Request), "", TaskIdsOCS.Logout, CurrentUser.Account);
            RequestHelper.RejectAccessToLoginPage(ControllerContext, Locale.UserSessionCleared);
        }

        [CustomAuthorize(TaskIds = "all")]
        public void KeepCurrentUserSession() {
            sessionDao.updateSessionTrack(CurrentUser.Account.UserId, HttpContext.Session.SessionID);
            //auditTrailDao.Log("User Session Continue" + Utils.getRemoteAddress(Request), CurrentUser.Account);
            auditTrailDao.SecurityLog("User Session Continue" + Utils.getRemoteAddress(Request), "", TaskIdsOCS.Login, CurrentUser.Account);
        }

        [CustomAuthorize(TaskIds = "all")]
        public void TransferSessionToOcs() {
            sessionDao.InsertSessionToOcs(CurrentUser.Account.UserId, HttpContext.Session.SessionID);
            //auditTrailDao.Log("User Session Transfered" + Utils.getRemoteAddress(Request), CurrentUser.Account);
            auditTrailDao.SecurityLog("User Session Transfered" + Utils.getRemoteAddress(Request), "", TaskIdsOCS.Login, CurrentUser.Account);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult TransactionTypeList()
        {
            List<Dictionary<string, string>> transactionTypes = Utils.ConvertDataTableToList(transactionTypeDao.ListAll());
            return Json(transactionTypes, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CapturingModeList()
        {
            List<Dictionary<string, string>> capturingModes = Utils.ConvertDataTableToList(capturingDao.GetCapturingModeDataTable());
            return Json(capturingModes, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CapturingTypeList(FormCollection col)
        {
            List<Dictionary<string, string>> capturingTypes = Utils.ConvertDataTableToList(capturingDao.GetCapturingTypeDataTable(col["id"]));
            return Json(capturingTypes, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CapturingInfoList(FormCollection col)
        {
            List<Dictionary<string, string>> capturingInfos = Utils.ConvertDataTableToList(capturingDao.GetCapturingInfo(col["intUserId"]));
            return Json(capturingInfos, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult PostingInfoList(FormCollection col)
        {
            List<Dictionary<string, string>> capturingInfos = Utils.ConvertDataTableToList(capturingDao.GetPostingModeInfo());
            return Json(capturingInfos, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult UpdateUICInfo(FormCollection col)
        {
            bool result = capturingDao.UpdateUICInfo(col["intScannerId"], col["intBatchNo"], col["intSeqNo"], col["intUserId"], col["intClearingBranch"]);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CheckBank(FormCollection col)
        {
            bool result = capturingDao.CheckBank(col["bankCode"]);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //[CustomAuthorize(TaskIds = "all")]
        //public void Scan(FormCollection col)
        //{
        //    capturingDao.UpdateUICInfo(col["intScannerId"], col["intBatchNo"], col["intSeqNo"], col["intUserId"]);
        //}

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetAndUpdateUICInfoIncSequence(FormCollection col)
        {
            List<Dictionary<string, string>> listUICInfo = Utils.ConvertDataTableToList(capturingDao.GetUICInfoDataTable(col["intScannerId"], col["intBranchId"]));
            //bool result = capturingDao.UpdateUICInfo(col["intScannerId"], col["intBatchNo"], col["intSeqNo"], col["intUserId"]);
            // capturingDao.UpdateUICInfoIncSequence(listUICInfo[0]["fldscannerid"], listUICInfo[0]["fldbranchid"], Convert.ToInt32(listUICInfo[0]["fldbatchno"]), Convert.ToInt32(listUICInfo[0]["fldseqno"]) + 1, CurrentUser.Account.UserId);
            return Json(listUICInfo, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult RetrieveScannerErr(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(capturingDao.GetScannerErrorDataTable(col["id"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetCapturingModeDesc(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(capturingDao.GetCapturingModeDetailsDataTable(col["strModeId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetSelfClearingBranchId(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(capturingDao.GetSelfClearingBranchId(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetCheckTypeDetails(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(capturingDao.GetCheckTypeDetailsDataTable(col["strTypeId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBankBranchesOcsWorkStationInformation(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetBankBranchesDataTableWorkStation(col["strBankCode"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBankBranchDetails(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetBankBranchDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [CustomAuthorize(TaskIds = "all")]
        public JsonResult FBIQA(FormCollection col)
        {
            FujiIQA_activeXdll.FujiIQA_activeXdll fbiqa = new FujiIQA_activeXdll.FujiIQA_activeXdll();

            string logFullFilePath = col["logFullFilePath"];
            string settingPath = col["settingPath"];

            string sChequePath = col["sChequePath"];
            string sFrontIMG = col["sFrontIMG"];
            string sBackIMG= col["sBackIMG"];
            string sGFrontIMG = col["sGFrontIMG"];
            string sGBackIMG = col["sGBackIMG"];

            fbiqa.logFullFilePath = logFullFilePath;

            fbiqa.settingPath = settingPath;
            fbiqa.callSetting();

            string res = fbiqa.callScan(sChequePath, sFrontIMG, sBackIMG, sGFrontIMG, sGBackIMG);

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult ReasonCodeInternalList()
        {
            List<Dictionary<string, string>> ReasonCode = Utils.ConvertDataTableToList(ChequeDataEntryDao.ListAll());
            return Json(ReasonCode, JsonRequestBehavior.AllowGet);
        }


        //MAB-MGR
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult MatchAIFMaster(string intAccNumber)
        {
            List<Dictionary<string, string>> AccountInfoResult = Utils.ConvertDataTableToList(ChequeDataEntryDao.GetOracleAccountInformation(intAccNumber)).ToList();
            return Json(AccountInfoResult, JsonRequestBehavior.AllowGet);
        }

        //    // For Uat testing
        //[CustomAuthorize(TaskIds = "all")]
        //public JsonResult MatchAIFMaster(string intAccNumber)
        //{
        //    List<Dictionary<string, string>> AifMasterList = Utils.ConvertDataTableToList(ChequeDataEntryDao.AIFMasterList(intAccNumber)).ToList();
        //    return Json(AifMasterList, JsonRequestBehavior.AllowGet);
        //}

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CheckBranchCode(FormCollection col)
        {
            bool result = ChequeDataEntryDao.CheckBranch(col["BranchCode"]);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult BranchList()
        {
            List<Dictionary<string, string>> branches = Utils.ConvertDataTableToList(branchEndOfDayDao.GetHubBranches(CurrentUser.Account.UserId));
            return Json(branches, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSValidateUser(FormCollection col)
        {
            List<Dictionary<string, string>> user = Utils.ConvertDataTableToList(branchEndOfDayDao.ValidateUserLogin(col["strusername"] , col["strpassword"], CurrentUser.Account.BankCode));
            return Json(user, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetItemReadyForSubmission(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetItemReadyForSubmission(col["strBranchCode"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetItemSubmitted(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetItemSubmitted(col["strBranchCode"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetHubItemReadyForSubmission(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetHubItemReadyForSubmission(CurrentUser.Account.UserId, CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetHubItemSubmitted(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetHubItemSubmitted(CurrentUser.Account.UserId, CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetBranchEndOfDay(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetBranchEndOfDay(col["strBranchCode"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetAllBranchEndOfDay(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.GetAllUserBranchEndOfDay(col["strUserId"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSDataEntryPendingItem(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.DataEntryPendingItem(col["strBranchCode"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBalancingIndividualItemDetail(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(TransactionBalancingDao.CheckIndividualItemDetail(col["intItemID"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSAuthorizationPendingItem(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(branchEndOfDayDao.AuthorizationPendingItem(col["strBranchCode"], CurrentUser.Account.BankCode));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSInsertBranchEndOfDay(FormCollection col)
        {
            bool result = branchEndOfDayDao.InsertHubBranchEndOfDay(col["strBranchId"], col["strEODStatus"], CurrentUser.Account.BankCode);
            //bool result = branchEndOfDayDao.InsertBranchEndOfDay(col["strBranchId"], col["strAmount"], col["strDifference"], col["strEODStatus"], CurrentUser.Account.BankCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]

        public JsonResult OCSEncryptedPassword(FormCollection col)
        {
            string result = branchEndOfDayDao.encryptPassword(col["strpassword"]);
            //Response.BufferOutput = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSDeleteBranchEndOfDay(FormCollection col)
        {
            bool result = branchEndOfDayDao.DeleteBranchEndOfDay(col["strBranchId"], col["strEODStatus"], CurrentUser.Account.BankCode);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSHubInsertBranchEndOfDay(FormCollection col)
        {
            bool result = branchEndOfDayDao.InsertHubBranchEndOfDay(col["strBranchId"], col["strEODStatus"], CurrentUser.Account.BankCode);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /*[CustomAuthorize(TaskIds = "all")]
        public JsonResult BranchSubmission(FormCollection col)
        {
            bool result = BranchSubmissionDao.BranchSubmission(col["strCapturingBranch"], col["strCapturingDate"], col["strScannerId"], col["strBatchNumber"], CurrentUser.Account.UserId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult BranchSubmissionDetails(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(BranchSubmissionDao.ReturnItemDetails(col["strCapturingBranch"], col["strCapturingDate"], col["strScannerId"], col["strBatchNumber"], CurrentUser.Account.UserId));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }*/
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CenterItemReadyForClearing(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(OutwardClearingICLDao.GetCenterItemReadyForClearingList(col["strSelectedRow"]));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CenterItemReadyForReturnICL(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(outwardReturnICLDao.GetCenterItemReadyForReturnICLList(col["strSelectedRow"]));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CenterItemForReturnedICL(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(outwardReturnICLDao.GetCenterItemForReturnedICLList(col["strSelectedRow"]));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult CenterClearedItems(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(OutwardClearingICLDao.GetCenterClearedItemList(col["strCapturingBranch"], col["strCapturingDate"],col["strClearingbatch"]));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }

        /*[CustomAuthorize(TaskIds = "all")]
        public JsonResult ReturnSubmittedDetails(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(BranchSubmissionDao.ReturnSubmittedDetails(col["strCapturingBranch"], col["strCapturingDate"], col["strScannerId"], col["strBatchNumber"], CurrentUser.Account.UserId));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }*/
        
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBranchBankCodeDetails(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetBankCodeDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBranchBankCodeDetailsIslamic(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetIslamicBankCodeDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBranchBankCodeDetailsConv(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetConvBankCodeDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBranchLocCodeDetails(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetLocCodeDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult GetBranchStateCodeDetails(FormCollection col)
        {
            List<Dictionary<string, string>> list = Utils.ConvertDataTableToList(BankBranchesOcsDao.GetStateCodeDetails(col["strBranchId"]));
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult OCSGetImageByte(FormCollection col)
        {

            //string taskIdParam = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");
            //if (taskIdParam == "311113" && col["imageState"] == "greyscale,front")
            //{
            //    col["imageState"] = "greyscale,back";
            //}

            List<string> states = col["imageState"].Split(',').ToList();

            string strImgRes = "";
            List<Dictionary<string, string>> checkimage = new List<Dictionary<string, string>>(); //= Utils.ConvertDataTableToList(chequeAmountEntryDao.GetImageByte(col["imageId"]));
            DataTable dt = ocsChequeImageDao.GetImageByte(col["imageId"], col);
            foreach (DataRow row in dt.Rows)
            {
                // Get the byte array from image file
                byte[] imgBytes = (byte[])row["fldImageCode"];

                //if (states != null)
                //{
                //    if (states.Contains("bw") && states.Contains("front"))
                //    {
                //        imgBytes = (byte[])row["fldfrontimgbt"];
                //    }
                //    else if (states.Contains("bw") && states.Contains("back"))
                //    {
                //        imgBytes = (byte[])row["fldbackimgbt"];
                //    }
                //    else if (states.Contains("greyscale") && states.Contains("front"))
                //    {
                //        imgBytes = (byte[])row["fldgfrontimgbt"];
                //    }
                //    else if (states.Contains("greyscale") && states.Contains("back"))
                //    {
                //        imgBytes = (byte[])row["fldgbackimgbt"];
                //    }
                //    else if (states.Contains("uv"))
                //    {
                //        imgBytes = (byte[])row["flduvimgbt"];
                //    }
                //}

                string imgString = Convert.ToBase64String(imgBytes);
                //Set the source with data:image/bmp
                strImgRes = String.Format("data:image/Bmp;base64,{0}", imgString);
                Dictionary<string, string> res = new Dictionary<string, string>() {
                    {"image", strImgRes}
                };

                checkimage.Add(res);
            }


            return Json(checkimage, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult ICSGetImageByte(FormCollection col)
        {
            List<string> states = col["imageState"].Split(',').ToList();

            string strImgRes = "";
            List<Dictionary<string, string>> checkimage = new List<Dictionary<string, string>>(); //= Utils.ConvertDataTableToList(chequeAmountEntryDao.GetImageByte(col["imageId"]));
            DataTable dt = icsChequeImageDao.GetImageByte(col["imageId"]);
            foreach (DataRow row in dt.Rows)
            {
                // Get the byte array from image file
                byte[] imgBytes = (byte[])row["fldgfrontimgbt"];

                if (states != null)
                {
                    if (states.Contains("bw") && states.Contains("front"))
                    {
                        imgBytes = (byte[])row["fldfrontimgbt"];
                    }
                    else if (states.Contains("bw") && states.Contains("back"))
                    {
                        imgBytes = (byte[])row["fldbackimgbt"];
                    }
                    else if (states.Contains("greyscale") && states.Contains("front"))
                    {
                        imgBytes = (byte[])row["fldgfrontimgbt"];
                    }
                    else if (states.Contains("greyscale") && states.Contains("back"))
                    {
                        imgBytes = (byte[])row["fldgfrontimgbt"];
                    }
                    else if (states.Contains("uv"))
                    {
                        imgBytes = (byte[])row["flduvimgbt"];
                    }
                }

                string imgString = Convert.ToBase64String(imgBytes);
                //Set the source with data:image/bmp
                strImgRes = String.Format("data:image/Bmp;base64,{0}", imgString);
                Dictionary<string, string> res = new Dictionary<string, string>() {
                    {"image", strImgRes}
                };

                checkimage.Add(res);
            }


            return Json(checkimage, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(TaskIds = "all")]
        public JsonResult ICSGetImage(FormCollection col)
        {
            List<string> states = col["imageState"].Split(',').ToList();

            //string strImgRes = "";
            List<Dictionary<string, string>> checkimage = new List<Dictionary<string, string>>(); //= Utils.ConvertDataTableToList(chequeAmountEntryDao.GetImageByte(col["imageId"]));
            //DataTable dt = icsChequeImageDao.GetImageByte(col["imageId"]);
            string imageId = col["imageId"].ToString().Trim();
            string imagePath = col["imageFolder"].ToString().Trim();
            //foreach (DataRow row in dt.Rows)
            //{

                // Get the byte array from image file
                string imageName = imagePath + imageId + ".jpg"; //grey default + .jpg

                if (states != null)
                {
                    if (states.Contains("bw") && states.Contains("front"))
                    {
                        imageName = imagePath + imageId + "1.tif"; //black front 1.tiff
                }
                    else if (states.Contains("bw") && states.Contains("back"))
                    {
                        imageName = imagePath + imageId + "2.tif"; //black back 2.tiff
                }
                    else if (states.Contains("greyscale") && states.Contains("front"))
                    {
                        imageName = imagePath + imageId + ".jpg"; //grey default + .jpg
                    }
                    else if (states.Contains("greyscale") && states.Contains("back"))
                    {
                        imageName = imagePath + imageId + ".jpg"; //grey default + .jpg
                    }
                    else if (states.Contains("uv"))
                    {
                        imageName = "D:\\\\AFFIN_ICS_v2\\\\INCHEQS_FILES\\\\Images\\\\Logo\\\\noimage.jpg"; //no image
                    }
                //}

                //string imgString = Convert.ToBase64String(imgBytes);
                //Set the source with data:image/bmp
                //strImgRes = String.Format("data:image/Bmp;base64,{0}", imgString);
                Dictionary<string, string> res = new Dictionary<string, string>() {
                    {"image", imageName}
                };

                checkimage.Add(res);
            }


            return Json(checkimage, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult AccountMaster(string intAccNumber)
        //{
        //    List<Dictionary<string, string>> MasterAccInfo = Utils.ConvertDataTableToList(signatureDao.GetOracleAccountInformation(intAccNumber, CurrentUser.Account.BankCode, CurrentUser.Account.UserId)).ToList();
        //    return Json(MasterAccInfo, JsonRequestBehavior.AllowGet);
        //}
        //public JsonResult SignatureMaster(string intAccNumber)
        //{
        //    List<Dictionary<string, string>> SignatureList = signatureDao.GetOracleSignatureInformation(intAccNumber);
        //    return Json(SignatureList, JsonRequestBehavior.AllowGet);
        //}
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult ReturnHolidayDates(FormCollection col)
        {
            List<Dictionary<string, string>> Items = Utils.ConvertDataTableToList(OCSProcessDateDao.GetHolidayDates(col));
            return Json(Items, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize(TaskIds = "all")]
        public JsonResult SubmissionTimes()
        {
            string spickC = CurrentUser.Account.SpickCode;
            if(spickC == "" || spickC == null)
            {
                spickC = "BOP";
            }
            DataTable submissionTimeTable = commonInwardItemDao.SubmissionTime(spickC);
            
            if (submissionTimeTable.Rows.Count>0)
            {
                var results1 = new { spickCode = spickC, startTime = submissionTimeTable.Rows[0]["StartTime"], endTime = submissionTimeTable.Rows[0]["EndTime"] };

                return Json(results1, JsonRequestBehavior.AllowGet);
            }
            var results = new { spickCode = spickC, startTime = "00:00:00", endTime = "00:00:00" };


            return Json(results, JsonRequestBehavior.AllowGet);

        }
    }
}