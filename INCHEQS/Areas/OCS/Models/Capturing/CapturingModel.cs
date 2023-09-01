using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
//using FujiIQA_activeXdll;

namespace INCHEQS.Areas.OCS.Models.Capturing
{
    public class CapturingModel
    {
        public DataTable CapturingModeDataTable { get; set; }
        public DataTable CapturingTypeDataTable { get; set; }
        public DataTable CapturingRelationDataTable { get; set; }
        //public DataTable CompleteSeqNoDataTable { get; set; }
        public DataTable WorkstationScannerDataTable { get; set; }      

        public string IQA0 { get; set; }
        public string IQA1 { get; set; }
        public string IQA2 { get; set; }
        public string ForceIQA { get; set; }
        public string AllowForceMICR { get; set; }

        public string ChequeStatus { get; set; }
        public string CICSMode { get; set; }
        public string ScannerOn { get; set; }
        public string ScannerId { get; set; }
        
        public string WorkstationBranchId { get; set; }
        public string CapBranchId { get; set; }
        public string ClrBranchId { get; set; }
        public string CapMode { get; set; }
        public string CapModeDesc { get; set; }
        public string ChqTypeId { get; set; }
        public string ChqType { get; set; }
        public string ChqTypeDesc { get; set; }
        public string SourceBranchId { get; set; }
        public string FloatDays { get; set; }
        public string NCFRequired { get; set; }
        public string ImmediateEntry { get; set; }
        public string EntryMode { get; set; }
        public string Priority { get; set; }
        public string EndorseAllignment { get; set; }
        public string TaskId { get; set; }
        public string BankType { get; set; }
        public string ZeroValue { get; set; }
        public string ScannerModel { get; set; }
        public DataTable ScannerErrorDataTable { get; set; }        

        public string DefaultCaptureCurrency { get; set; }
        public DataTable CurrencyDataTable { get; set; }
        public string CaptureCurrency { get; set; }

        public DataTable CompleteSeqNoDataTable { get; set; }
        public string CompletedSeqNo { get; set; }
        public string CapturingPath { get; set; }
        public string AutoCapture { get; set; }
        public string ShowBIRBOC { get; set; }
        public string ScrollableUI { get; set; }
        public string ShowTellerId { get; set; }
        public string SingleSlip { get; set; }
        public string USDMode { get; set; }
        public string MaxItemPerBatch { get; set; }

        public DataTable UICDataTable { get; set; }
        public string ServerBatchNo { get; set; }
        public int ServerSeqNo { get; set; }

        public string LockBoxKey { get; set; }
        public string OCRRequire { get; set; }
        public string LockBoxAccNo { get; set; }

        public string ScannerTypeId { get; set; }
        public DataTable ScannerTuningDataTable { get; set; }
        public string ScannerTuningXML { get; set; }
        public string ScannerErrorXML { get; set; }

        public string AllowCapTypAftCutOff { get; set; }

        public string ProcessDate { get; set; }
        public string ProcessDateHidden { get; set; }

        public DataTable EODDataTable { get; set; }

        public string UserBankCode { get; set; }
        public string UserBankDesc { get; set; }
        public string UserId { get; set; }

        public string BranchID { get; set; }
        public string BranchCode { get; set; }
        public string BranchDesc { get; set; }
        public string PostingMode { get; set; }
        public string PostingModeDesc { get; set; }
    }
}