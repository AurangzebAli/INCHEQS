using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Areas.ICS.Models;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Common;
using INCHEQS.Security.Account;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.Sequence;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;
//using INCHEQS.Models.VerificationLimit;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using System.Globalization;

namespace INCHEQS.Areas.ICS.Models.AutoApproveThresholdModel
{
    public class AutoApproveThresholdDao : IAutoApproveThresholdDao
    {
        private readonly ApplicationDbContext dbContext;

        public AutoApproveThresholdDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public AutoApproveThresholdModel GetClearDate()
        {
            AutoApproveThresholdModel autoApproveThresholdModel = new AutoApproveThresholdModel();
            DataTable ds = new DataTable();
            string stmt = "select top 1 fldcleardate from tblinwardcleardate where fldStatus = 'Y' order by fldcleardate DESC";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];

                autoApproveThresholdModel.fldClearDate = Convert.ToDateTime(row["fldClearDate"].ToString()).ToString("dd-MM-yyyy");
                autoApproveThresholdModel.allowButton = "Y";
                //OG// autoApproveThresholdModel.fldClearDate =row["fldClearDate"].ToString();
                //OG// string[] dateonly = autoApproveThresholdModel.fldClearDate.Split(' ');
                //OG// return dateonly[0];


            }
            else
            {
                autoApproveThresholdModel.fldClearDate = DateTime.Now.ToString("dd-MM-yyyy");
                autoApproveThresholdModel.allowButton = "N";

            }
            return autoApproveThresholdModel;
        }

        public AutoApproveThresholdModel GetTotalInwardItem(FormCollection collection)
        {
            DataTable ds = new DataTable();
            AutoApproveThresholdModel autoapproveThreshold = new AutoApproveThresholdModel();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            
            
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));

            ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThreshold", sqlParameterNext.ToArray());
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
               
                autoapproveThreshold.TotalRecord = Convert.ToInt32(row["TotRec"]);

                if (autoapproveThreshold.TotalRecord==0)
                {
                    autoapproveThreshold.TotalAmount = 0;
                }
                else { autoapproveThreshold.TotalAmount = Convert.ToDouble(row["TotAmount"]); }
                
            }
            return autoapproveThreshold;
        }

        public AutoApproveThresholdModel GetTotalPendingInwardItem(FormCollection collection)
        {
            DataTable ds = new DataTable();
            AutoApproveThresholdModel autoapproveThreshold = new AutoApproveThresholdModel();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)
                        .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));
            sqlParameterNext.Add(new SqlParameter("@minAmount", collection["minAmount"]));
            sqlParameterNext.Add(new SqlParameter("@maxAmount", collection["maxAmount"]));

            ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdPending", sqlParameterNext.ToArray());
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                autoapproveThreshold.TotalPendingRecord = Convert.ToInt32(row["TotRec"]);
                if (autoapproveThreshold.TotalPendingRecord == 0){
                    autoapproveThreshold.TotalPendingAmount = 0;
                }
                else
                {
                    autoapproveThreshold.TotalPendingAmount = Convert.ToDouble(row["TotAmountPending"]);
                }
            }
            return autoapproveThreshold;
        }

        public List<String> ValidateForm(FormCollection collection)
        {
            List<String> err = new List<string>();
            if (collection["percentage"] == "")
            {
                err.Add("Please Enter Percentage");
            }
            return err;
        }

        public string GetFilteredAmount(FormCollection collection, int filteredPendingItem)
        {
            string filteredAmount = "";

            if (filteredPendingItem == 0)
            {
                filteredAmount = "0";
            }
            else
            {
                DataTable ds = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@pendingitem", filteredPendingItem));
                sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)
                             .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));

                ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdFiltered", sqlParameterNext.ToArray());
                if (ds.Rows.Count > 0)
                {
                    DataRow row = ds.Rows[0];
                    filteredAmount = row[0].ToString();
                }
            }
            
            return filteredAmount;
        }

        public string GetMaxAmount(FormCollection collection, int filteredPendingItem)
        {
            string maxAmount = "";

            if (filteredPendingItem == 0)
            {
                maxAmount = "0";
            }
            else
            {
                DataTable ds = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@pendingitem", filteredPendingItem));
                sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)
                             .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));

                ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdMax", sqlParameterNext.ToArray());
                if (ds.Rows.Count > 0)
                {
                    DataRow row = ds.Rows[0];
                    maxAmount = row[0].ToString();
                }
            }
            return maxAmount;
        }

        public string GetMinAmount(FormCollection collection, int filteredPendingItem)
        {
            string minAmount = "";
            if (filteredPendingItem == 0)
            {
                minAmount = "0";
            }
            else
            {
                DataTable ds = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@pendingitem", filteredPendingItem));
                sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)
                             .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));
                ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdMin", sqlParameterNext.ToArray());
                if (ds.Rows.Count > 0)
                {
                    DataRow row = ds.Rows[0];
                    minAmount = row[0].ToString();
                }
            }
            return minAmount;
        }

        public List<int> GetRangeList(FormCollection collection, string maxAmount) {
            List<int> RangeList = new List<int>();
            List<int> maxRangeString = new List<int>();
            int statisticRange = Convert.ToInt32(collection["statisticRange"]);

            for (int i = 0; Convert.ToDouble(maxAmount) > i; i += statisticRange) {
                int newStatisticRange = statisticRange + i;
                maxRangeString.Add(newStatisticRange);
            }
            return maxRangeString;
        }

        public List<Double> GetRangeItem(FormCollection collection, int filteredPendingItem)
        {
            List<Double> ItemPercentage = new List<Double>();
            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@pendingitem", filteredPendingItem));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(collection["fldClearDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture)
             .ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)))));

            ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdItemPercent", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                for (int i = 0; i < ds.Rows.Count; i++)
                {
                    DataRow row = ds.Rows[i];
                    ItemPercentage.Add(Convert.ToDouble(row["fldAmount"]));
                }
            }
            return ItemPercentage;
        }

        public void UpdateAutoApproveThreshold(FormCollection collection, string userid, string fldClearDate, string filteredPendingItem)
        {
            List<AutoApproveThresholdModel> aatList = new List<AutoApproveThresholdModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<SqlParameter> sqlParameterNextSeqNo = new List<SqlParameter>();
            string tableName = "tblInwardItemHistory";
            string lastSeqNo = "";
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(fldClearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)
 .ToString("dd/MMM/yyyy", CultureInfo.InvariantCulture)))));
            //sqlParameterNext.Add(new SqlParameter("@fldClearDate", fldClearDate));
            sqlParameterNext.Add(new SqlParameter("@pending", filteredPendingItem));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgAutoApproveThresholdSearch", sqlParameterNext.ToArray());

            foreach (DataRow row in ds.Rows)
            {
                AutoApproveThresholdModel aat = new AutoApproveThresholdModel();
                aat.fldInwardItemID = row["fldInwardItemId"].ToString();
                aat.fldUIC = row["fldUIC"].ToString();
                aat.fldRemarks = row["fldRemarks"].ToString();
                aatList.Add(aat);
            }
            //Update tblInwardItemInfoStatus
            List<SqlParameter> sqlParameterNextUpdate = new List<SqlParameter>();
            string approvalStatus = "A";
            string remarks = "Auto Approve Threshold";
            sqlParameterNextUpdate.Add(new SqlParameter("@fldApprovalStatus", approvalStatus));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldApprovalUserID", userid));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldApprovalTimeStamp", DateTime.Now));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldRemarks", remarks));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldUpdateUserId", userid));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNextUpdate.Add(new SqlParameter("@pendingitem", Convert.ToInt32(filteredPendingItem)));
            sqlParameterNextUpdate.Add(new SqlParameter("@fldClearDate", ((DateTime.ParseExact(fldClearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)
 .ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)))));

            //dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuAutoApproveThresholdApproval", sqlParameterNextUpdate.ToArray());

            sqlParameterNextSeqNo.Add(new SqlParameter("@TableName", tableName));
            DataTable dt = new DataTable();
            //OG// dt = dbContext.GetRecordsAsDataTableSP("spcgCTCSNextSeqNo", sqlParameterNextSeqNo.ToArray());
            dt = dbContext.GetRecordsAsDataTableSP("spcgCTCSNextSeqNoAAT", sqlParameterNextSeqNo.ToArray());
            
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                lastSeqNo = row["fldLastSeqId"].ToString();
            }
            int latestSeqNo = Convert.ToInt32(lastSeqNo);
            //insert into tblInwardItemHistory
            foreach (var item in aatList) { 

                List<SqlParameter> sqlParameterNextInsert = new List<SqlParameter>();
                sqlParameterNextInsert.Add(new SqlParameter("@fldActionStatusId", latestSeqNo.ToString()));
                sqlParameterNextInsert.Add(new SqlParameter("@fldInwardItemID", item.fldInwardItemID));
                sqlParameterNextInsert.Add(new SqlParameter("@fldUIC", item.fldUIC));
                sqlParameterNextInsert.Add(new SqlParameter("@fldRemarks", "Auto Approve Threshold"));
                sqlParameterNextInsert.Add(new SqlParameter("@fldCreateUserID", userid));
                sqlParameterNextInsert.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciAutoApproveThresholdInsert",sqlParameterNextInsert.ToArray());
                latestSeqNo += 1;

            }
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuAutoApproveThresholdApproval", sqlParameterNextUpdate.ToArray());

            string fldcleardate2 = (DateTime.ParseExact(fldClearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)
 .ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
            string sSQL = "insert into tblDataProcess (fldProcessName, fldStatus, fldClearDate, " +
                    "fldCreateUserId, fldCreateTimestamp, fldUpdateUserId, fldUpdateTimestamp, fldBankCode) " +
                    "values ('Auto Approve Threshold', 4 , '" + fldcleardate2 + "','"+ userid+"', Getdate() ,'"+ userid +"', " +
                    "Getdate(),'08')";
            dbContext.ExecuteNonQuery(sSQL);


        }

        public bool CheckAAT()
        {

            string MySql =
              "select * from tbldataprocess where fldProcessName = 'Auto Approve Threshold'";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                return true; 
            }
            else
            {
                return false;// not found
            }
        }
    }



}
