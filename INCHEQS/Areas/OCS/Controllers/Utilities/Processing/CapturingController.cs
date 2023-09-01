using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Areas.OCS.Models.Capturing;
using System.Data;
using INCHEQS.Security.SystemProfile;
using System.Linq;

namespace INCHEQS.Areas.OCS.Controllers.Processing
{

    public class CapturingController : BaseController
    {
        private readonly ICapturingDao capturingDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;

        public CapturingController(ICapturingDao capturingDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.capturingDao = capturingDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Capturing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult FUJI()
        {
            return View("FUJI");
        }

        public ActionResult DCC()
        {
            return View("DCC");
        }

        public ActionResult Close(FormCollection collection)
        {
            return PartialView("Capturing/Modal/_ClosePopup");
        }

        public ActionResult Redirect(FormCollection col, string CapturingMode = null, string ChequeType = null, string ChequeStatus = null)
        {
            CapturingModel capturingModel = new CapturingModel();
            string strMacAddress = string.Join(";", capturingDao.GetMacAddress().ToArray());
            List<string> lstMacAddress = capturingDao.GetMacAddress();

            List<String> errorMessages = capturingDao.ValidateMacAdress(lstMacAddress);
            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
                return RedirectToAction("Index");
            }

            foreach (string macAdress in lstMacAddress) {
                capturingModel.WorkstationScannerDataTable = capturingDao.GetWorkstationScannerDataTable(macAdress, CurrentUser.Account.UserId);
                if (capturingModel.WorkstationScannerDataTable.Rows.Count > 0) {
                    strMacAddress = macAdress;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(CapturingMode) && !string.IsNullOrEmpty(ChequeType) && !string.IsNullOrEmpty(ChequeStatus))
            {
                capturingModel.CapMode = CapturingMode;
                capturingModel.ChqType = ChequeType;
                capturingModel.ChequeStatus = ChequeStatus;
            }
            else
            {
                capturingModel.CapMode = col["optCapturingMode"].ToString();
                capturingModel.ChqType = col["optChequeType"].ToString();
                capturingModel.ChequeStatus = col["radChequeStatus"].ToString();
            }

            capturingModel.UserId = CurrentUser.Account.UserId;

            capturingModel.IQA0 = systemProfileDao.GetValueFromSystemProfile("IQAValue0", CurrentUser.Account.BankCode);
            capturingModel.IQA1 = systemProfileDao.GetValueFromSystemProfile("IQAValue1", CurrentUser.Account.BankCode);
            capturingModel.IQA2 = systemProfileDao.GetValueFromSystemProfile("IQAValue2", CurrentUser.Account.BankCode);

            capturingModel.ForceIQA = systemProfileDao.GetValueFromSystemProfile("HideForceIQAPassButton", CurrentUser.Account.BankCode);
            capturingModel.AllowForceMICR = systemProfileDao.GetValueFromSystemProfile("AllowForceFailedMICR", CurrentUser.Account.BankCode);
         
            capturingModel.CICSMode = "";
            capturingModel.ScannerId = capturingModel.WorkstationScannerDataTable.Rows[0]["fldScannerId"].ToString();
            capturingModel.ScannerTypeId = capturingModel.WorkstationScannerDataTable.Rows[0]["fldScannerTypeId"].ToString();
            capturingModel.CapBranchId = capturingModel.WorkstationScannerDataTable.Rows[0]["fldBranchId"].ToString();
            capturingModel.ClrBranchId = capturingModel.WorkstationScannerDataTable.Rows[0]["fldBranchId"].ToString();
   
            capturingModel.CapModeDesc = capturingDao.GetCapturingModeDetailsDataTable(capturingModel.CapMode).Rows[0]["fldDescription"].ToString();
            capturingModel.ChqTypeId = capturingDao.GetCheckTypeDetailsDataTable(capturingModel.ChqType).Rows[0]["fldTypeId"].ToString();         
            capturingModel.ChqTypeDesc = capturingDao.GetCheckTypeDetailsDataTable(capturingModel.ChqType).Rows[0]["fldDescription"].ToString();
            capturingModel.SourceBranchId = "";
            capturingModel.FloatDays = "";
            capturingModel.NCFRequired = "";
            capturingModel.ImmediateEntry = capturingDao.GetCheckTypeDetailsDataTable(capturingModel.ChqType).Rows[0]["fldImmediateEntry"].ToString();
            capturingModel.EntryMode = capturingDao.GetCapturingModeDetailsDataTable(capturingModel.CapMode).Rows[0]["fldEntryMode"].ToString();
            capturingModel.Priority = capturingDao.GetCheckTypeDetailsDataTable(capturingModel.ChqType).Rows[0]["fldPriority"].ToString();
            capturingModel.EndorseAllignment = "0";
            capturingModel.TaskId = "";
            capturingModel.BankType = "";
            capturingModel.ZeroValue = "0";
            capturingModel.ScannerModel = capturingModel.WorkstationScannerDataTable.Rows[0]["fldScannerModel"].ToString();
            
            capturingModel.ScannerOn = "0";

            //-------------PROCESSING DATE
            string strProcessDate = capturingDao.GetProcessDate(CurrentUser.Account.BankCode);
            capturingModel.ProcessDateHidden = strProcessDate;
            capturingModel.ProcessDate = capturingDao.FormatProcessDate(int.Parse(strProcessDate.Substring(0,4)), int.Parse(strProcessDate.Substring(4,2)), int.Parse(strProcessDate.Substring(6,2)));

            //-------------CURRENCY
            capturingModel.DefaultCaptureCurrency = systemProfileDao.GetValueFromSystemProfile("CaptureCurrency", CurrentUser.Account.BankCode);
            capturingModel.CurrencyDataTable = capturingDao.GetCurrencyDataTable(null);
            string strCaptureCurrency = "";
            foreach (DataRow row in capturingModel.CurrencyDataTable.Rows) {
                if (strCaptureCurrency != "") {
                    strCaptureCurrency += ",";
                }
                strCaptureCurrency += row["fldCurrencyId"].ToString() + "#" + row["fldCurrencyCode"].ToString();
                if (row["fldCurrencyId"].ToString() == capturingModel.DefaultCaptureCurrency) {
                    strCaptureCurrency += "#" + "Y";
                }
            }
            capturingModel.CaptureCurrency = strCaptureCurrency;

            //-------------CAPTURING PATH
            capturingModel.CompleteSeqNoDataTable = capturingDao.GetCompleteSeqNoDataTable(CurrentUser.Account.BankCode);
            if (capturingModel.CompleteSeqNoDataTable.Rows.Count > 0) {
                int ctr = 0;
                foreach (DataRow row in capturingModel.CompleteSeqNoDataTable.Rows) {
                    string strProfileCode = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileCode"].ToString();
                    switch (strProfileCode) {
                        case "MaxItemPerBatch":
                            capturingModel.CompletedSeqNo = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            capturingModel.MaxItemPerBatch = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "CapturingPath":
                            capturingModel.CapturingPath = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "AutoCapture":
                            capturingModel.AutoCapture = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "ShowCapturingItemType":
                            capturingModel.ShowBIRBOC = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "CapturingScrollableUI":
                            capturingModel.ScrollableUI = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "ShowTellerId":
                            capturingModel.ShowTellerId = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "CapturingSingleSlip":
                            capturingModel.SingleSlip = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                        case "USDMode":
                            capturingModel.USDMode = capturingModel.CompleteSeqNoDataTable.Rows[ctr]["fldSystemProfileValue"].ToString();
                            break;
                    }
                    ctr++;
                }
            }

            //-------------BATCH NUMBER | SEQ NUMBER
            capturingModel.UICDataTable = capturingDao.GetUICInfoDataTable(capturingModel.ScannerId);
            if (capturingModel.UICDataTable.Rows.Count > 0) {
                capturingModel.ServerBatchNo = capturingModel.UICDataTable.Rows[0]["fldBatchNo"].ToString();
                capturingModel.ServerSeqNo = Convert.ToInt32(capturingModel.UICDataTable.Rows[0]["fldSeqNo"]) + 1;
            }

            //-------------LOCK BOX
            capturingModel.LockBoxKey = "";
            capturingModel.OCRRequire = "";
            capturingModel.LockBoxAccNo = "";

            //-------------SCANNER TUNING
            capturingModel.ScannerTuningDataTable = capturingDao.GetScannerTuningDataTable(capturingModel.ScannerTypeId);

            string strWriteXml = "<ScannerTuning>";
            strWriteXml += "<ScannerTypeId>" + capturingModel.ScannerTypeId + "</ScannerTypeId>";
            if (capturingModel.ScannerTuningDataTable.Rows.Count > 0) {
                for (int intCounter = 0; intCounter < capturingModel.ScannerTuningDataTable.Rows.Count; intCounter++) {
                    strWriteXml += "<ScannerTuningRecord>";
                    strWriteXml += "<ScannerTuningId><![CDATA[" + capturingModel.ScannerTuningDataTable.Rows[intCounter]["fldScannerTuningId"].ToString().Trim() + "]]></ScannerTuningId>";
                    strWriteXml += "<ScannerTuningCode><![CDATA[" + capturingModel.ScannerTuningDataTable.Rows[intCounter]["fldScannerTuningCode"].ToString().Trim() + "]]></ScannerTuningCode>";
                    strWriteXml += "<ScannerTuningvalue><![CDATA[" + capturingModel.ScannerTuningDataTable.Rows[intCounter]["fldScannerTuningvalue"].ToString().Trim() + "]]></ScannerTuningvalue>";
                    strWriteXml += "</ScannerTuningRecord>";                    
                }
            }
            strWriteXml += "</ScannerTuning>";
            capturingModel.ScannerTuningXML = strWriteXml;

            //-------------SCANNER ERROR
            strWriteXml = "";
            capturingModel.ScannerErrorDataTable = capturingDao.GetScannerErrorDataTable(capturingModel.ScannerTypeId);
            
            if (capturingModel.ScannerErrorDataTable.Rows.Count > 0) {
                for (int intCounter = 0; intCounter < capturingModel.ScannerErrorDataTable.Rows.Count; intCounter++) {
                    strWriteXml += "<ScannerErrorRecord>";
                    strWriteXml += "<ScannerErrorCode><![CDATA[" + capturingModel.ScannerErrorDataTable.Rows[intCounter]["fldScannerErrorCode"].ToString().Trim() + "]]></ScannerErrorCode>";
                    strWriteXml += "<ScannerErrorDesc><![CDATA[" + capturingModel.ScannerErrorDataTable.Rows[intCounter]["fldScannerErrorDesc"].ToString().Trim() + "]]></ScannerErrorDesc>";
                    strWriteXml += "</ScannerErrorRecord>";
                }
            }
            strWriteXml = "<ScannerError>" + strWriteXml + "</ScannerError>";
            capturingModel.ScannerErrorXML = strWriteXml;

            //-------------BANK CODE
            capturingModel.UserBankCode = CurrentUser.Account.BankCode;


            //-------------CHECKING
            capturingModel.AllowCapTypAftCutOff = systemProfileDao.GetValueFromSystemProfile("AllowCapTypAftCutOff", CurrentUser.Account.BankCode);
            if (capturingModel.AllowCapTypAftCutOff != "" && capturingModel.AllowCapTypAftCutOff.Split('|').ToList().Contains(capturingModel.ChqTypeId)) {
                return RedirectToAction("Index");
            }

            capturingModel.EODDataTable = capturingDao.GetBranchEndOfDayDataTable(capturingModel.ProcessDate,capturingModel.ClrBranchId, CurrentUser.Account.BankCode);
            if (capturingModel.EODDataTable.Rows.Count >  0) {
                TempData["ErrorMsg"] = "Branch end of day has been performed. Scanning is not allowed.";
                return RedirectToAction("Index");
            }

            capturingModel.EODDataTable = capturingDao.GetCenterEndOfDayDataTable(capturingModel.ProcessDate, CurrentUser.Account.BankCode);
            if (capturingModel.EODDataTable.Rows.Count > 0)
            {
                TempData["ErrorMsg"] = "HQ end of day already preformed. Scanning is not allowed.";
                return RedirectToAction("Index");
            }


            ViewBag.CapturingModel = capturingModel;

            switch (capturingModel.ScannerTypeId) {
                case "1":
                    return View("DCC");
                case "3":
                    return View("FUJI");
                default:
                    return View();
            }
            
        }
    }
}