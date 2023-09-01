using System.Data;
using System.Data.SqlClient;
using INCHEQS.Helpers;
//using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security;
using System.Collections.Generic;
using INCHEQS.Security.Account;
using System;
using INCHEQS.DataAccessLayer;
using INCHEQS.Models;
using INCHEQS.Common;

namespace INCHEQS.Models.TransactionType {
    public class TransactionTypeDao : ITransactionTypeDao {


        private readonly ApplicationDbContext dbContext;
        public TransactionTypeDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public async Task<DataTable> ListAllAsync() {
            return await Task.Run(() => ListAll());
        }

        public async Task<DataTable> FindAsync(string TransType) {
            return await Task.Run(() => Find(TransType));
        }


        public DataTable ListAll() {
            DataTable ds = new DataTable();
            string stmt = "SELECT * FROM tblCTCSTransactionType WHERE fldActive = 'Y' And fldTransactionDesc like '%' AND fldTransactionType Like '%' order by fldTransactionType  ";
            ds = dbContext.GetRecordsAsDataTable( stmt);

            return ds;
        }

        public DataTable Find(string TransType) {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldTransactionType,fldTransactionDesc, fldUpdateTimestamp FROM tblCTCSTransactionType WHERE fldTransactionType = @transType";
            ds = dbContext.GetRecordsAsDataTable( stmt, new[] { new SqlParameter("@transType", TransType) });

            return ds;
        }

        public void UpdateTransactionType(FormCollection col) {

            string stmt = "update tblCTCSTransactionType set fldTransactionDesc=@fldTransactionDesc, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTransactionType=@fldTransactionType";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionDesc",col["transDesc"]),
                new SqlParameter("@fldUpdateTimestamp",DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldTransactionType",col["transType"])
            });
        }

        public void DeleteTransactionType(string transactionType) {
            string stmt = "delete from tblCTCSTransactionType where fldTransactionType=@fldTransactionType";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionType",transactionType)
            });
        }

        public void AddToTransactionTypeTempToDelete(string transactionType) {
            string stmt = "Insert into tblCTCSTransactionTypeTemp (fldTransactionType, fldTransactionDesc) " +
                " Select fldTransactionType,fldTransactionDesc from tblCTCSTransactionType WHERE fldTransactionType=@fldTransactionType " +
                " Update tblCTCSTransactionTypeTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTransactionType=@fldTransactionType ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionType", transactionType),
                new SqlParameter("@fldApproveStatus", "D")
            });
        }

        public void DeleteInTransactionTypeTemp(string transactionType) {

            string stmt = "delete from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldTransactionType", transactionType) });
        }

        public void CreateTransactionTypeInTemp(FormCollection col, AccountModel currentUser) {

            Dictionary<string, dynamic> sqlTransactionType = new Dictionary<string, dynamic>();
            sqlTransactionType.Add("fldTransactionType", col["transType"]);
            sqlTransactionType.Add("fldTransactionDesc", col["transDesc"]);
            sqlTransactionType.Add("fldActive", "Y");
            sqlTransactionType.Add("fldCreateUserId", currentUser.UserId);
            sqlTransactionType.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionType.Add("fldUpdateUserId", currentUser.UserId);
            sqlTransactionType.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionType.Add("fldApproveStatus", "A");

            dbContext.ConstructAndExecuteInsertCommand("tblCTCSTransactionTypeTemp", sqlTransactionType);
        }

        public void CreateTransactionType(string transactionType) {

            string stmt = "Insert into tblCTCSTransactionType (fldTransactionType, fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp)" +
                "select fldTransactionType, fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldTransactionType", transactionType) });
        }

        public List<String> ValidateCreate(FormCollection col) {
            List<String> err = new List<String>();

            if (col["transType"].Equals("")) {
                err.Add("Transaction Type cannot be empty");
            }
            if (CheckExist(col["transType"])) {
                err.Add("Transaction Type already exist.");
            }
            return err;
        }

        public bool CheckExist(string transType) {
            string stmt = "select * from tblCTCSTransactionType where fldTransactionType=@fldTransactionType";
            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldTransactionType", transType) });
        }
    }
}
