using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Areas.ICS.Models.HostReturnReason {
    public class HostReturnReasonDao : IHostReturnReasonDao{
        private readonly ApplicationDbContext dbContext;
        public HostReturnReasonDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public string GetHostReturnReasonDesc(string statusId) {
            DataTable dt = new DataTable();
            string result = "";
            //string stmt = "SELECT * FROM tblBankHostStatusMaster where fldStatusID=@fldStatusID";
            string stmt = "SELECT * FROM tblBankHostStatusMaster where fldBankHostStatusCode=@fldBankHostStatusCode";
            dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusCode", statusId) });
            if (dt.Rows.Count > 0) {
                DataRow row = dt.Rows[0];
                result = row["fldStatusDesc"].ToString();
            }
            return result;
        }

        public DataTable GetHostReturnReason(string statusId) {
            DataTable dt = new DataTable();
            string stmt = "SELECT * FROM tblBankHostStatusMaster where fldStatusID=@fldStatusID";
            dt = dbContext.GetRecordsAsDataTable(stmt,new[] { new SqlParameter("@fldStatusID", statusId) });
            return dt;
        }

        public void UpdateHostReturnReason(FormCollection col,AccountModel currentUser) {

            string stmt = "Update tblBankHostStatusMaster set fldStatusDesc=@fldStatusDesc, fldAutoReject=@fldAutoReject, fldAutoPending=@fldAutoPending, fldUpdateTimestamp=@fldUpdateTimestamp, fldUpdateUserId=@fldUpdateUserId where fldStatusId=@fldStatusId";


            if (col["rejectAction"] == null) {
                col["rejectAction"] = "0";
            }
            if (col["pendingAction"] == null) {
                col["pendingAction"] = "0";
            } 
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldStatusDesc",col ["statusDesc"]),
                new SqlParameter("@fldAutoReject",col ["rejectAction"]),
                new SqlParameter("@fldAutoPending",col ["pendingAction"]),
                new SqlParameter("@fldUpdateTimestamp",DateUtils.GetCurrentDate()),
                new SqlParameter("@fldUpdateUserId",currentUser.UserId),
                new SqlParameter("@fldStatusId",col ["statusId"]),

            });
            
        }

        public void CreateHostReturnReasonTemp(FormCollection col, string autoPending, string autoReject,AccountModel currentUser) {
            
            Dictionary<string, dynamic> sqlHostReturnReason = new Dictionary<string, dynamic>();
            sqlHostReturnReason.Add("fldStatusId", col["statusId"]);
            sqlHostReturnReason.Add("fldStatusDesc", col["statusDesc"]);
            sqlHostReturnReason.Add("fldAutoReject", autoReject);
            sqlHostReturnReason.Add("fldAutoPending", autoPending);
            sqlHostReturnReason.Add("fldCreateUserId", currentUser.UserId);
            sqlHostReturnReason.Add("fldCreateTimeStamp", DateUtils.GetCurrentDate());
            sqlHostReturnReason.Add("fldUpdateUserId", currentUser.UserId);
            sqlHostReturnReason.Add("fldUpdateTimestamp", DateUtils.GetCurrentDate());
            sqlHostReturnReason.Add("fldEntityCode", currentUser.BankCode);
            sqlHostReturnReason.Add("fldBankCode", currentUser.BankCode);
            sqlHostReturnReason.Add("fldApproveStatus", "A");

            dbContext.ConstructAndExecuteInsertCommand("tblBankHostStatusMasterTemp", sqlHostReturnReason);
        }

        public void DeleteInBankHostStatusMaster(string statusId) {

            string stmt = "delete from tblBankHostStatusMaster  where fldStatusId=@fldStatusId";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });

        }

        public void DeleteInBankHostStatusMasterTemp(string statusId) {

            string stmt = "delete from tblBankHostStatusMasterTemp  where fldStatusId=@fldStatusId";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });

        }

        public List<string> ValidateUpdate(FormCollection col) {
            List<string> err = new List<string>();

            if (col["statusDesc"]=="") {
                err.Add(Locale.StatusDescCannotEmpty);
            }

            return err;
        }

        public List<string> ValidateCreate(FormCollection col) {
            List<string> err = new List<string>();

            if (col["statusId"] == "") {
                err.Add(Locale.StatusIdCannotEmpty);
            }
            if (col["statusDesc"] == "") {
                err.Add(Locale.StatusDescCannotEmpty);
            }
            if (CheckRecordExist(col["statusId"])) {
                err.Add(Locale.ReturnCodeAlreadyExist);
            }

            if (CheckRecordExistInTemp(col["statusId"])) {
                err.Add(Locale.ReturnCodeAlreadyCreatedToBeVerify);
            }
            return err;
        }

        public bool CheckRecordExist(string statusId) {
            string stmt = "select * from tblBankHostStatusMaster where fldStatusId=@fldStatusId";

            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });
        }

        public bool CheckRecordExistInTemp(string statusId) {
            string stmt = "select * from tblBankHostStatusMasterTemp where fldStatusId=@fldStatusId";

            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });
        }

        public void CreateInBankHostStatusMaster(string statusId) {

            string stmt = "Insert into tblBankHostStatusMaster (fldStatusId, fldStatusDesc, fldAutoReject, fldAutoPending, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimestamp, fldEntityCode, fldBankCode) "+
              " Select fldStatusId, fldStatusDesc, fldAutoReject, fldAutoPending, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimestamp, fldEntityCode, fldBankCode from tblBankHostStatusMasterTemp where fldStatusId=@fldStatusId";

            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldStatusId", statusId)});            
        }

        public void AddtoBankHostStatusMasterTempToDelete(string statusId) {

            string stmt = "Insert into tblBankHostStatusMasterTemp (fldStatusId, fldStatusDesc, fldAutoReject, fldAutoPending, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimestamp, fldEntityCode, fldBankCode)" +
                " Select fldStatusId, fldStatusDesc, fldAutoReject, fldAutoPending, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimestamp, fldEntityCode, fldBankCode from tblBankHostStatusMaster where fldStatusId=@fldStatusId" +
                " update tblBankHostStatusMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldStatusId=@fldStatusId";

            dbContext.ExecuteNonQuery(stmt, new[] {
            new SqlParameter("@fldStatusId",statusId),
            new SqlParameter("@fldApproveStatus","D")});
        }

        public HostReturnReasonModel GetHostReturnReasonModel(string statusId) {
            HostReturnReasonModel hostReturnReasonModel = new HostReturnReasonModel();
            DataTable ds = new DataTable();
            string stmt = "select * from tblBankHostStatusMaster where fldStatusId=@fldStatusId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldStatusId", statusId) });

            if (ds.Rows.Count > 0) {
                DataRow row = ds.Rows[0];
                hostReturnReasonModel.statusId = row["fldStatusId"].ToString();
                hostReturnReasonModel.statusDesc = row["fldStatusDesc"].ToString();
            }
            return hostReturnReasonModel;
        }

    }
}