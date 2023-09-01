using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.ProgressStatus {
    public class ProgressStatusModel {
        
        // REJECT REENTRY
        public string Band0RR { get; set; }
        public string Band1RR { get; set; }
        public string Band2RR { get; set; }
        public string Band3RR { get; set; }
        public string Band4RR { get; set; }
        public string Band5RR { get; set; }

        public string Band0RRChecker { get; set; }
        public string Band1RRChecker { get; set; }
        public string Band2RRChecker { get; set; }
        public string Band3RRChecker { get; set; }
        public string Band4RRChecker { get; set; }
        public string Band5RRChecker { get; set; }
        public string RRCompleted { get; set; }
        public string RR1stLevel { get; set; }
        public string RR2ndLevel { get; set; }
        public string RRTotal { get; set; }

        // HOST REJECT
        public string HostRejectCompleted { get; set; }
        public string HostRejectCompletedApproved { get; set; }
        public string HostRejectCompletedRejected { get; set; }
        public string HostReject1stLevel { get; set; }
        public string HostReject2ndLevel { get; set; }
        public string HostRejectTotal { get; set; }

        // PPS
        public string PPSCompleted { get; set; }
        public string PPS1stLevel { get; set; }
        public string PPS2ndLevel { get; set; }
        public string PPSTotal { get; set; }

        // BAND 0.00 - 500.00
        public string Band0Completed { get; set; }
        public string Band0CompletedApproved { get; set; }
        public string Band0CompletedRejected { get; set; }
        public string Band0_1stLevel { get; set; }
        public string Band0_2ndLevel { get; set; }
        public string Band0_PendingBranch { get; set; }
        public string Band0Total { get; set; }
        public string Band0PullOut { get; set; }
        public string Band0PPS1stLevel { get; set; }
        public string Band0PPS2ndLevel { get; set; }

        // BAND 1
        public string Band1Completed { get; set; }
        public string Band1CompletedApproved { get; set; }
        public string Band1CompletedRejected { get; set; }
        public string Band1_1stLevel { get; set; }
        public string Band1_4thLevel { get; set; }
        public string Band1_PendingBranch { get; set; }
        public string Band1Total { get; set; }
        public string Band1PullOut { get; set; }

        public string Band1PPS1stLevel { get; set; }
        public string Band1PPS2ndLevel { get; set; }

        // BAND 2
        public string Band2Completed { get; set; }
        public string Band2CompletedApproved { get; set; }
        public string Band2CompletedRejected { get; set; }
        public string Band2_1stLevel { get; set; }
        public string Band2_2ndLevel { get; set; }
        public string Band2_PendingBranch { get; set; }
        public string Band2Total { get; set; }
        public string Band2PullOut { get; set; }
        public string Band2PPS1stLevel { get; set; }
        public string Band2PPS2ndLevel { get; set; }

        // BAND 3
        public string Band3Completed { get; set; }
        public string Band3CompletedApproved { get; set; }
        public string Band3CompletedRejected { get; set; }
        public string Band3_1stLevel { get; set; }
        public string Band3_2ndLevel { get; set; }
        public string Band3_PendingBranch { get; set; }
        public string Band3Total { get; set; }
        public string Band3PullOut { get; set; }
        public string Band3PPS1stLevel { get; set; }
        public string Band3PPS2ndLevel { get; set; }
        // BAND 4
        public string Band4Completed { get; set; }
        public string Band4CompletedApproved { get; set; }
        public string Band4CompletedRejected { get; set; }
        public string Band4_1stLevel { get; set; }
        public string Band4_2ndLevel { get; set; }
        public string Band4_PendingBranch { get; set; }
        public string Band4Total { get; set; }
        public string Band4PullOut { get; set; }
        public string Band4PPS1stLevel { get; set; }
        public string Band4PPS2ndLevel { get; set; }

        // BAND 5
        public string Band5Completed { get; set; }
        public string Band5CompletedApproved { get; set; }
        public string Band5CompletedRejected { get; set; }
        public string Band5_1stLevel { get; set; }
        public string Band5_2ndLevel { get; set; }
        public string Band5_PendingBranch { get; set; }
        public string Band5Total { get; set; }
        public string Band5PullOut { get; set; }
        public string Band5PPS1stLevel { get; set; }
        public string Band5PPS2ndLevel { get; set; }

        // BRANCH
        public string Branch { get; set; }
        public int Completed { get; set; }
        public int Level1st { get; set; }
        public int Level2nd { get; set; }
        public int Total { get; set; }

        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        

        // PPS BRANCH
        public string PPSBranch { get; set; }
        public int PPSBranchCompleted { get; set; }
        public int PPSBranchLevel1st { get; set; }
        public int PPSBranchLevel2nd { get; set; }
        public int PPSBranchTotal { get; set; }
        public string Band0_3rdLevel { get; internal set; }
        public string Band0_4thLevel { get; internal set; }
        public string Band1_3rdLevel { get; internal set; }
        public string Band2_3rdLevel { get; internal set; }
        public string Band2_4thLevel { get; internal set; }
        public string Band3_3rdLevel { get; internal set; }
        public string Band3_4thLevel { get; internal set; }
        public string Band4_3rdLevel { get; internal set; }
        public string Band4_4thLevel { get; internal set; }
        public string Band5_3rdLevel { get; internal set; }
        public string Band5_4thLevel { get; internal set; }
        public string Band1_2ndLevel { get; internal set; }
        public string Band1_PendingAuthorizer { get; internal set; }
        public string Band2_PendingAuthorizer { get; internal set; }
        public string Band3_PendingAuthorizer { get; internal set; }
        public string Band4_PendingAuthorizer { get; internal set; }
        public string Band5_PendingAuthorizer { get; internal set; }
        public string Band0_PendingAuthorizer { get; internal set; }
    }
}