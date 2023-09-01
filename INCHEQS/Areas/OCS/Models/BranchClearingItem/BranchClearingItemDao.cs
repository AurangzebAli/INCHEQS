using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Security;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.BranchClearingItem
{
    public class BranchClearingItemDao : IBranchClearingItemDao
    {
        private readonly OCSDbContext ocsdbContext;
        public BranchClearingItemDao(OCSDbContext ocsdbContext)
        {
            this.ocsdbContext = ocsdbContext;
        }

        public async Task<DataTable> FindAsync(string id)
        {
            return await Task.Run(() => Find(id));
        }

        //public string GetDataFromHubMaster()
        //{
        //    string result = "";
        //    string i = "";
        //    string stmt = "SELECT fldBranchId FROM tblHubBranch as a left join tblHubUser as b ";
        //    stmt = stmt + "on a.fldHubCode = b.fldHubCode left join ";
        //    stmt = stmt + "tblHubMaster as c on a.fldHubCode = c.fldHubCode ";
        //    stmt = stmt + "where b.fldUserId = @fldUserID and c.fldActive = 'Y'";
        //    DataTable dt = ocsdbContext.GetRecordsAsDataTable(stmt, new[] {
        //        new SqlParameter("@fldUserID", CurrentUser.Account.UserId)
        //    });
        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow item in dt.Rows)
        //        {
        //            i = item["fldBranchId"].ToString();
        //            if (result.Equals(""))
        //            {
        //                result = i;
        //            }
        //            else
        //            {
        //                result = result + "," + i;
        //            }
        //        }
        //        result = result.Replace(",","','");
        //        return result;
        //    }

        //    return null;
        //}

        //public string GetDataFromMaster()
        //{
        //    string result = "";
        //    string stmt = "SELECT fldBranchId FROM tblHubBranch as a left join tblHubUser as b ";
        //    stmt = stmt + "on a.fldHubCode = b.fldHubCode left join ";
        //    stmt = stmt + "tblHubMaster as c on a.fldHubCode = c.fldHubCode ";
        //    stmt = stmt + "where b.fldUserId = @fldUserID and c.fldActive = 'Y'";
        //    DataTable dt = ocsdbContext.GetRecordsAsDataTable(stmt, new[] {
        //        new SqlParameter("@fldUserID", CurrentUser.Account.UserId)
        //    });
        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow item in dt.Rows)
        //        {
        //            result = item["fldBranchId"].ToString();
        //        }
        //        return result;
        //    }

        //    return null;
        //}

        public DataTable Find(string id)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldCapturingBranch,fldBranchDesc, CapturingBranch,CapturingDate,fldBatchNumber,fldCapturingMode,fldScannerID,fldARJBatchNumber,fldPostingMode,TotalItem,TotalAmount FROM View_BranchClearingItem WHERE fldItemInitialID = @Id";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@Id", id) });

            return ds;
        }

        public void UpdateBranchItem(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", col["this_fldCapturingBranch"]));
            sqlParameterNext.Add(new SqlParameter("@ScannerID", col["this_fldScannerID"]));
            sqlParameterNext.Add(new SqlParameter("@BatcNo", col["this_fldBatchNumber"]));
            sqlParameterNext.Add(new SqlParameter("@CapturingDate", col["this_CapturingDate"]));
            //Excute the command
            ocsdbContext.GetRecordsAsDataTableSP("spcuBranchItemSubmissionByBatch", sqlParameterNext.ToArray());

        }
  }
}