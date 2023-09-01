using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.PullOutReason {
    public class PullOutReasonDao : IPullOutReasonDao {
        private readonly ApplicationDbContext dbContext;
        public PullOutReasonDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }
        public DataTable ListAllPullOutReason() {
            DataTable ds = new DataTable();
            string stmt = "select* from tblPullOutReason";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }
        public DataTable getPullOutReason(string PullOutID) {
            DataTable ds = new DataTable();
            string stmt = "select* from tblPullOutReason where fldPullOutID=@pulloutid";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@pulloutid", PullOutID) });
            return ds;
        }
        public void Update(FormCollection collection, string userId) {

            string stmt = "update tblPullOutReason set fldPullOutReason=@pulloutreason,fldUpdateUserId=@updateuserid,fldUpdateTimeStamp=@updatetime where fldPullOutID=@pulloutid";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@pulloutreason",collection["pullOutDesc"]),
                new SqlParameter("@updateuserid",userId),
                new SqlParameter("@updatetime",DateTime.Now),
                new SqlParameter("@pulloutid",collection["pullOutReasonID"]),

            });
        }
        public void CreatePullOutReasonTemp(FormCollection collection, string userId) {
            string stmt = "insert into tblPullOutReasonTemp(fldpulloutId,fldPullOutReason,fldCreateUserId,fldCreateTimeStamp ,fldUpdateUserId,fldUpdateTimeStamp,fldApproveStatus)values (@fldpulloutId, @pulloutreason,@createuserid,@createtime,@updateuserid,@updatetime,@fldApproveStatus)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldpulloutId",collection["pullOutReasonID"]),
                new SqlParameter("@pulloutreason",collection["pullOutDesc"]),
                new SqlParameter("@createuserid",userId),
                new SqlParameter("@createtime",DateTime.Now),
                new SqlParameter("@updateuserid",userId),
                new SqlParameter("@updatetime",DateTime.Now),
                new SqlParameter("@fldApproveStatus","A")
            });
        }

        public void DeleteInPullOutReason(string pullOutID) {
            
                string stmt = "delete from tblPullOutReason where fldPullOutID=@fldPullOutID";
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldPullOutID", pullOutID) });
        }

        public List<string> Validate(FormCollection collection) {
            List<string> errorlist = new List<string>();
            if (collection["pullOutDesc"] == "") {
                errorlist.Add(Locale.PleasefillinPullOutDescription);
            }
            return errorlist;
        }

        public PullOutReasonModel getPullOutData(string pulloutid) { //for audit log
            PullOutReasonModel pullOutModel = new PullOutReasonModel();
            DataTable ds = new DataTable();
            string stmt = "select * from tblPullOutReason where fldPullOutID=@pulloutid";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@pulloutid", pulloutid) });

            if(ds.Rows.Count>0) {
                DataRow row = ds.Rows[0];
                pullOutModel.pullOutId = row["fldPullOutID"].ToString();
                pullOutModel.pullOutDesc = row["fldPullOutReason"].ToString();
            }
            return pullOutModel;
        }

        public void AddToPullOutReasonTempToDelete(string pulloutId) {

            string stmt = "insert into tblPullOutReasonTemp (fldPullOutId,fldPullOutReason,fldCreateUserId,fldCreateTimeStamp ,fldUpdateUserId,fldUpdateTimeStamp)"+
                " select fldPullOutId,fldPullOutReason,fldCreateUserId,fldCreateTimeStamp ,fldUpdateUserId,fldUpdateTimeStamp from tblPullOutReason where fldPullOutId=@fldPullOutId"+
                " update tblPullOutReasonTemp set fldApproveStatus=@fldApproveStatus where fldPullOutId=@fldPullOutId";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldApproveStatus","D"),
                new SqlParameter("@fldPullOutId",pulloutId)});
        }

        public void CreateInPullOutReason(string pulloutId) {

            string stmt = "insert into tblPullOutReason(fldPullOutId,fldPullOutReason,fldCreateUserId,fldCreateTimeStamp ,fldUpdateUserId,fldUpdateTimeStamp)" +
                " select fldPullOutId,fldPullOutReason,fldCreateUserId,fldCreateTimeStamp ,fldUpdateUserId,fldUpdateTimeStamp from tblPullOutReasonTemp where fldPullOutId=@fldPullOutId";

            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("fldPullOutId", pulloutId) });

        }

        public void DeleteInPullOutReasonTemp(string pullOutID) {
            string stmt = "delete from tblPullOutReasonTemp where fldPullOutID=@fldPullOutID";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldPullOutID", pullOutID) });
        }
    }
}