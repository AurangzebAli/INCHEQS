using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using INCHEQS.Areas.OCS.Models.ProgressStatus;

using INCHEQS.Security;
using INCHEQS.Security.Account;

using INCHEQS.Areas.ICS.Models.ProgressStatus;
using System.Web.Mvc;
using INCHEQS.Common;

namespace INCHEQS.Areas.ICS.Models.ProgressStatus {
    public class ProgressStatusDao : IProgressStatusDao{
        
        private readonly ApplicationDbContext dbContext;
        public ProgressStatusDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public List<ProgressStatusModel> ReturnProgressStatus(FormCollection col)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<ProgressStatusModel> result = new List<ProgressStatusModel>();
            sqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(CurrentUser.Account.UserId)));
            sqlParameterNext.Add(new SqlParameter("@bankCode", col["bankCode"].ToString()));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgICSProgressStatus", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    ProgressStatusModel progressStatus = new ProgressStatusModel();

                    // REJECT REENTRY
                    progressStatus.Band0RR= item["Band0RR"].ToString();
                    progressStatus.Band1RR= item["Band1RR"].ToString();
                    progressStatus.Band2RR= item["Band2RR"].ToString();
                    progressStatus.Band3RR= item["Band3RR"].ToString();
                    progressStatus.Band4RR= item["Band4RR"].ToString();
                    progressStatus.Band5RR= item["Band5RR"].ToString();

                    progressStatus.Band0RRChecker = item["Band0RRChecker"].ToString();
                    progressStatus.Band1RRChecker = item["Band1RRChecker"].ToString();
                    progressStatus.Band2RRChecker = item["Band2RRChecker"].ToString();
                    progressStatus.Band3RRChecker = item["Band3RRChecker"].ToString();
                    progressStatus.Band4RRChecker = item["Band4RRChecker"].ToString();
                    progressStatus.Band5RRChecker = item["Band5RRChecker"].ToString();
                    //progressStatus.RRCompleted = item["RRCompleted"].ToString();
                    //progressStatus.RR1stLevel = item["RR1stLevel"].ToString();
                    //progressStatus.RR2ndLevel = item["RR2ndLevel"].ToString();
                    //progressStatus.RRTotal = item["RRTotal"].ToString();

                    // HOST REJECT
                    //progressStatus.HostRejectCompleted = item["HostRejectCompleted"].ToString();
                    //progressStatus.HostRejectCompletedApproved = item["HostRejectCompletedApproved"].ToString();
                    //progressStatus.HostRejectCompletedRejected = item["HostRejectCompletedRejected"].ToString();
                    //progressStatus.HostReject1stLevel = item["HostReject1stLevel"].ToString();
                    //progressStatus.HostReject2ndLevel = item["HostReject2ndLevel"].ToString();
                    //progressStatus.HostRejectTotal = item["HostRejectTotal"].ToString();

                    // PPS
                    progressStatus.PPSCompleted = item["PPSCompeleted"].ToString();
                    progressStatus.PPS1stLevel = item["PPS1stLevel"].ToString();
                    progressStatus.PPS2ndLevel = item["PPS2ndLevel"].ToString();
                    progressStatus.PPSTotal = item["PPSTotal"].ToString();

                    // BAND 0.00 - 500.00
                    progressStatus.Band0Completed = item["Band0Completed"].ToString();
                    progressStatus.Band0CompletedApproved = item["Band0CompletedApproved"].ToString();
                    progressStatus.Band0CompletedRejected = item["Band0CompletedRejected"].ToString();
                    progressStatus.Band0_1stLevel = item["Band0_1stLevel"].ToString();
                    progressStatus.Band0_2ndLevel = item["Band0_2ndLevel"].ToString();

                    progressStatus.Band0_3rdLevel = item["Band0_3rdLevel"].ToString();
                    progressStatus.Band0_4thLevel = item["Band0_4thLevel"].ToString();

                    progressStatus.Band0_PendingBranch = item["Band0_PendingBranch"].ToString();
                    progressStatus.Band0_PendingAuthorizer = item["Band0_PendingAuthorizer"].ToString();
                    
                    progressStatus.Band0PullOut = item["Band0PullOut"].ToString();
                    progressStatus.Band0PPS1stLevel = item["Band0PPS1stLevel"].ToString();
                    progressStatus.Band0PPS2ndLevel = item["Band0PPS2ndLevel"].ToString();
                    progressStatus.Band0Total = item["Band0Total"].ToString();

                    // BAND 1
                    progressStatus.Band1Completed = item["Band1Completed"].ToString();
                    progressStatus.Band1CompletedApproved = item["Band1CompletedApproved"].ToString();
                    progressStatus.Band1CompletedRejected = item["Band1CompletedRejected"].ToString();
                    progressStatus.Band1_1stLevel = item["Band1_1stLevel"].ToString();
                    progressStatus.Band1_2ndLevel = item["Band1_2ndLevel"].ToString();
                    progressStatus.Band1_3rdLevel = item["Band1_3rdLevel"].ToString();
                    progressStatus.Band1_4thLevel = item["Band1_4thLevel"].ToString();
                    progressStatus.Band1_PendingBranch = item["Band1_PendingBranch"].ToString();
                    progressStatus.Band1_PendingAuthorizer = item["Band1_PendingAuthorizer"].ToString();

                    progressStatus.Band1PullOut = item["Band1PullOut"].ToString();
                    progressStatus.Band1PPS1stLevel = item["Band1PPS1stLevel"].ToString();
                    progressStatus.Band1PPS2ndLevel = item["Band1PPS2ndLevel"].ToString();
                    progressStatus.Band1Total = item["Band1Total"].ToString();

                    // BAND 2
                    progressStatus.Band2Completed = item["Band2Completed"].ToString();
                    progressStatus.Band2CompletedApproved = item["Band2CompletedApproved"].ToString();
                    progressStatus.Band2CompletedRejected = item["Band2CompletedRejected"].ToString();
                    progressStatus.Band2_1stLevel = item["Band2_1stLevel"].ToString();
                    progressStatus.Band2_2ndLevel = item["Band2_2ndLevel"].ToString();
                    progressStatus.Band2_3rdLevel = item["Band2_3rdLevel"].ToString();
                    progressStatus.Band2_4thLevel = item["Band2_4thLevel"].ToString();

                    progressStatus.Band2_PendingBranch = item["Band2_PendingBranch"].ToString();
                    progressStatus.Band2_PendingAuthorizer = item["Band2_PendingAuthorizer"].ToString();

                    progressStatus.Band2PullOut = item["Band2PullOut"].ToString();
                    progressStatus.Band2PPS1stLevel = item["Band2PPS1stLevel"].ToString();
                    progressStatus.Band2PPS2ndLevel = item["Band2PPS2ndLevel"].ToString();
                    progressStatus.Band2Total = item["Band2Total"].ToString();

                    // BAND 3
                    progressStatus.Band3Completed = item["Band3Completed"].ToString();
                    progressStatus.Band3CompletedApproved = item["Band3CompletedApproved"].ToString();
                    progressStatus.Band3CompletedRejected = item["Band3CompletedRejected"].ToString();
                    progressStatus.Band3_1stLevel = item["Band3_1stLevel"].ToString();
                    progressStatus.Band3_2ndLevel = item["Band3_2ndLevel"].ToString();
                    progressStatus.Band3_3rdLevel = item["Band3_3rdLevel"].ToString();
                    progressStatus.Band3_4thLevel = item["Band3_4thLevel"].ToString();

                    progressStatus.Band3_PendingBranch = item["Band3_PendingBranch"].ToString();
                    progressStatus.Band3_PendingAuthorizer = item["Band3_PendingAuthorizer"].ToString();

                    progressStatus.Band3PullOut = item["Band3PullOut"].ToString();
                    progressStatus.Band3PPS1stLevel = item["Band3PPS1stLevel"].ToString();
                    progressStatus.Band3PPS2ndLevel = item["Band3PPS2ndLevel"].ToString();
                    progressStatus.Band3Total = item["Band3Total"].ToString();

                    // BAND 4
                    progressStatus.Band4Completed = item["Band4Completed"].ToString();
                    progressStatus.Band4CompletedApproved = item["Band4CompletedApproved"].ToString();
                    progressStatus.Band4CompletedRejected = item["Band4CompletedRejected"].ToString();
                    progressStatus.Band4_1stLevel = item["Band4_1stLevel"].ToString();
                    progressStatus.Band4_2ndLevel = item["Band4_2ndLevel"].ToString();
                    progressStatus.Band4_3rdLevel = item["Band4_3rdLevel"].ToString();
                    progressStatus.Band4_4thLevel = item["Band4_4thLevel"].ToString();


                    progressStatus.Band4_PendingBranch = item["Band4_PendingBranch"].ToString();
                    progressStatus.Band4_PendingAuthorizer = item["Band4_PendingAuthorizer"].ToString();

                    progressStatus.Band4PullOut = item["Band4PullOut"].ToString();
                    progressStatus.Band4PPS1stLevel = item["Band4PPS1stLevel"].ToString();
                    progressStatus.Band4PPS2ndLevel = item["Band4PPS2ndLevel"].ToString();
                    progressStatus.Band4Total = item["Band4Total"].ToString();

                    // BAND 5
                    progressStatus.Band5Completed = item["Band5Completed"].ToString();
                    progressStatus.Band5CompletedApproved = item["Band5CompletedApproved"].ToString();
                    progressStatus.Band5CompletedRejected = item["Band5CompletedRejected"].ToString();
                    progressStatus.Band5_1stLevel = item["Band5_1stLevel"].ToString();
                    progressStatus.Band5_2ndLevel = item["Band5_2ndLevel"].ToString();

                    progressStatus.Band5_3rdLevel = item["Band5_3rdLevel"].ToString();
                    progressStatus.Band5_4thLevel = item["Band5_4thLevel"].ToString();


                    progressStatus.Band5_PendingBranch = item["Band5_PendingBranch"].ToString();
                    progressStatus.Band5_PendingAuthorizer = item["Band5_PendingAuthorizer"].ToString();

                    progressStatus.Band5PullOut = item["Band5PullOut"].ToString();
                    progressStatus.Band5PPS1stLevel = item["Band5PPS1stLevel"].ToString();
                    progressStatus.Band5PPS2ndLevel = item["Band5PPS2ndLevel"].ToString();
                    progressStatus.Band5Total = item["Band5Total"].ToString();

                    result.Add(progressStatus);
                }
                return result;
            }
            return null;
        }
        public string GetUserType()
        {
            DataTable dtCapturingMode = new DataTable();
            string result = "";
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(CurrentUser.Account.UserId)));
            dtCapturingMode = dbContext.GetRecordsAsDataTableSP("spcgProgressUserType", SqlParameterNext.ToArray());
            if (dtCapturingMode.Rows.Count > 0)
            {
                DataRow row = dtCapturingMode.Rows[0];
                result = row["fldusertype"].ToString();
            }
            return result;
        }

        public List<ProgressStatusModel> ReturnFilterBranchProgressStatus(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<ProgressStatusModel> result = new List<ProgressStatusModel>();
            sqlParameterNext.Add(new SqlParameter("@clearDate", DateUtils.formatDateToSql(col["fldClearDate"])));
            sqlParameterNext.Add(new SqlParameter("@bankCode", col["bankCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldIssueBranchCode", col["fldIssueBranchCode"].ToString()));

            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgICSProgressStatusWithBranch", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    ProgressStatusModel progressStatus = new ProgressStatusModel();
                    progressStatus.Branch = item["BranchCode"].ToString() + " - " + item["BranchDesc"].ToString();
                    progressStatus.Completed = Convert.ToInt32(item["Completed"]);
                    progressStatus.Level1st = Convert.ToInt32(item["Level1st"]);
                    progressStatus.Level2nd = Convert.ToInt32(item["Level2nd"]);
                    progressStatus.Approved = Convert.ToInt32(item["Approved"]);
                    progressStatus.Rejected = Convert.ToInt32(item["Rejected"]);
                    progressStatus.Pending = Convert.ToInt32(item["Pending"]);
                    progressStatus.Total = Convert.ToInt32(item["Total"]);

                    result.Add(progressStatus);
                }
            }
            return result;
        }

        public List<ProgressStatusModel> ReturnFilterPPSBranchProgressStatus(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<ProgressStatusModel> result = new List<ProgressStatusModel>();
            sqlParameterNext.Add(new SqlParameter("@clearDate", DateUtils.formatDateToSql(col["fldClearDate"])));
            sqlParameterNext.Add(new SqlParameter("@bankCode", col["bankCode"].ToString()));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgICSProgressStatusWithPPSBranch", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    ProgressStatusModel progressStatus = new ProgressStatusModel();
                    progressStatus.PPSBranch = item["BranchCode"].ToString() + " - " + item["InternalBranchCode"].ToString() + " - " + item["BranchDesc"].ToString();
                    progressStatus.PPSBranchCompleted = Convert.ToInt32(item["Completed"]);
                    progressStatus.PPSBranchLevel1st = Convert.ToInt32(item["Level1st"]);
                    progressStatus.PPSBranchLevel2nd = Convert.ToInt32(item["Level2nd"]);
                    progressStatus.PPSBranchTotal = Convert.ToInt32(item["Total"]);

                    result.Add(progressStatus);
                }
            }
            return result;
        }


        public string getClearDate()
        {
            string clearDate = "";

            DataTable dtClearDate = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            dtClearDate = dbContext.GetRecordsAsDataTableSP("spcgGetClearDate", SqlParameterNext.ToArray());
            if (dtClearDate.Rows.Count > 0)
            {
                DataRow row = dtClearDate.Rows[0];
                clearDate = Convert.ToDateTime(row["fldClearDate"]).ToString("dd-MM-yyyy");
            }

            return clearDate;
        }
        //public ProgressStatusModel getReportForInwardItemQueue(string ViewId) {
        //    ProgressStatusModel model = new ProgressStatusModel();
        //    string sql = string.Format("SELECT * FROM {0} ", ViewId);
        //        using (DbDataReader dr = dbContext.ExecuteReader( CommandType.Text, sql, new[] { new SqlParameter("","") })) {

        //        }                   

        //    return model;
        //}
    }
}