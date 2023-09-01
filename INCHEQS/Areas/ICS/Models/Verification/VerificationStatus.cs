using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.Verification {
    public class VerificationStatus {
        public string fldConBranchCode { get; internal set; }

        public static class ACTION {


            public const string VerificationApprove1stChecker = "C";
            public const string VerificationApprove = "A";
            public const string VerificationReturn = "R";
            public const string VerificationReturn1stChecker = "S";
            public const string VerificationRoute = "B";
            public const string VerificationPullOut = "P";
            public const string VerificationRepair = "L";
            public const string VerificationRouteCreditControl = "C";
            public const string VerificationBranchConfirm = null;
            public const string RejectReentryMaker = "D";
            public const string RejectReentryChecker = "E";
            public const string RejectReentryApprove = "F";
            public const string ReviewAction = "Rev";
            public const string Manual = "M";

            public const string BranchApproveChecker = "A";
            public const string BranchApproveMaker = "H";
            public const string BranchReturnChecker = "R";
            public const string BranchReturnMaker = "J";
            public const string BranchReferBackChecker = "K";
            public const string BranchLargeAmtApproveMaker = "M";
            public const string BranchLargeAmtApproveChecker = "A";
            public const string BranchLargeAmtRejectMaker = "O";
            public const string BranchLargeAmtRejectChecker = "R";
        }

    }
}