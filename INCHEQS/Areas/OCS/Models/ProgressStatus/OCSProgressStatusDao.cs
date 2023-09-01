using INCHEQS.Areas.OCS.Models.ProgressStatus;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Security;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;

namespace INCHEQS.Models.ProgressStatus
{
    public class OCSProgressStatusDao : IOCSProgressStatusDao
    {

        private readonly ApplicationDbContext dbContext;
        public OCSProgressStatusDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public List<Areas.OCS.Models.CommonOutwardItem.OCSProgressStatus> ReturnProgressStatus()
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<Areas.OCS.Models.CommonOutwardItem.OCSProgressStatus> result = new List<Areas.OCS.Models.CommonOutwardItem.OCSProgressStatus>();
            sqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(CurrentUser.Account.UserId)));
            //sqlParameterNext.Add(new SqlParameter("@UserType", CurrentUser.Account.UserType));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgOCSProgressStatus", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    Areas.OCS.Models.CommonOutwardItem.OCSProgressStatus History = new Areas.OCS.Models.CommonOutwardItem.OCSProgressStatus();
                    History.TotalNormalCapturingItems = item["TotalNormalCapturingItems"].ToString();
                    History.TotalNormalCapturingAmount = item["TotalNormalCapturingAmount"].ToString();
                    History.TotalDataEntryItems = item["TotalDataEntryItems"].ToString();
                    History.TotalAmountEntryItem = item["TotalAmountEntryItem"].ToString();
                    History.TotalAmountEntryAmount = item["TotalAmountEntryAmount"].ToString();
                    History.TotalBalancingItem = item["TotalBalancingItem"].ToString();
                    History.TotalBalancingAmount = item["TotalBalancingAmount"].ToString();
                    History.TotalMICRRepairItem = item["TotalMICRRepairItem"].ToString();
                    History.TotalMICRRepairAmount = item["TotalMICRRepairAmount"].ToString(); 
                    History.TotalReadyforSubmit = item["TotalReadyforSubmit"].ToString(); 
                    History.TotalSubmittedItem = item["TotalSubmittedItem"].ToString();
                    History.TotalClearedItem = item["TotalClearedItem"].ToString();
                    History.TotalApproved = item["TotalApproved"].ToString();
                    History.TotalPending = item["TotalPending"].ToString();
                    History.TotalRejected = item["TotalRejected"].ToString();

                    //Michelle
                    History.TotalPercentCompletion = item["TotalPercentCompletion"].ToString();
                    History.TotalInwardReturn = item["TotalInwardReturn"].ToString();
                    History.TotalNormalCheque = item["TotalNormalCheque"].ToString();
                    History.TotalNonClearingCheque = item["TotalNonClearingCheque"].ToString();
                    History.TotalItemCheque = item["TotalItemCheque"].ToString();
                    History.TotalDepositSlip = item["TotalDepositSlip"].ToString();
                    History.TotalCheque = item["TotalCheque"].ToString();
                    History.TotalInwardReturnICL = item["TotalInwardReturnICL"].ToString();
                    History.TotalIRMatch = item["TotalIRMatch"].ToString();
                    History.TotalIRUnmatch = item["TotalIRUnmatch"].ToString();
                    History.TotalPendingCreditPosting = item["TotalPendingCreditPosting"].ToString();
                    History.TotalCompleteCreditPosting = item["TotalCompleteCreditPosting"].ToString();
                    History.TotalPendingDebitPosting = item["TotalPendingDebitPosting"].ToString();
                    History.TotalCompleteDebitPosting = item["TotalCompleteDebitPosting"].ToString();
                    History.TotalPendingGenOutwardICL = item["TotalPendingGenOutwardICL"].ToString();
                    History.TotalCompleteGenOutwardICL = item["TotalCompleteGenOutwardICL"].ToString();

                    History.CaptureMode = item["CaptureMode"].ToString();

                    History.TotalChequeDateAmountEntryItem = item["TotalChequeDateAmountEntryItem"].ToString();
                    History.TotalChequeDateAmountEntryAmount = item["TotalChequeDateAmountEntryAmount"].ToString();
                    //Michelle
                    result.Add(History);
                }
                return result;
            }
            return null;
        }
        public DataTable GetCapturingModeDataTable()
        {
            DataTable dtCapturingMode = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(CurrentUser.Account.UserId)));
            dtCapturingMode = dbContext.GetRecordsAsDataTableSP("spcgOCSProgressStatusCaptureModeCount", SqlParameterNext.ToArray());
            return dtCapturingMode;
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
        

    }
}
