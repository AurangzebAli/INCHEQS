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
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Models;
using INCHEQS.Common;
using INCHEQS.Resources;

namespace INCHEQS.Areas.OCS.Models.TransactionType
{
    public class TransactionTypeDao : ITransactionTypeDao
    {


        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        public TransactionTypeDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public async Task<DataTable> ListAllAsync()
        {
            return await Task.Run(() => ListAll());
        }

        public async Task<DataTable> FindAsync(string TransType)
        {
            return await Task.Run(() => Find(TransType));
        }


        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT * FROM tblCTCSTransactionType WHERE fldActive = 'Y' And fldTransactionDesc like '%' AND fldTransactionType Like '%' order by fldTransactionType  ";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }

        public List<TransactionTypeModel> ListTransactionTypes()
        {
            string str = "SELECT * FROM tblCTCSTransactionType WHERE fldActive = 'Y' And fldTransactionDesc like '%' AND fldTransactionType Like '%' order by fldTransactionType ";
            DataTable dataTable = new DataTable();
            List<TransactionTypeModel> transTypeModels = new List<TransactionTypeModel>();
            dataTable = this.ocsdbContext.GetRecordsAsDataTable(str);
            foreach (DataRow row in dataTable.Rows)
            {
                TransactionTypeModel transTypeModel = new TransactionTypeModel()
                {
                    fldTransactionType = row["fldTransactionType"].ToString(),
                    fldTransactionDesc = row["fldTransactionDesc"].ToString()
                };
                transTypeModels.Add(transTypeModel);
            }
            return transTypeModels;
        }

        public DataTable Find(string TransType)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldTransactionType,fldTransactionDesc, fldUpdateTimestamp FROM tblCTCSTransactionType WHERE fldTransactionType = @transType";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@transType", TransType) });

            return ds;
        }

        public void UpdateTransactionTypeInTemp(FormCollection col)
        {

            string stmt = "update tblCTCSTransactionTypeTemp set fldTransactionDesc=@fldTransactionDesc, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTransactionType=@fldTransactionType ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionDesc",col["transDesc"]),
                new SqlParameter("@fldUpdateTimestamp",DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldTransactionType",col["transType"])
            });
        }

        public void UpdateTransactionType(string transactionType)
        {
            string strQuerySelect = "select fldTransactionDesc, fldUpdateUserId, fldUpdateTimeStamp FROM tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType ";
            string strQueryUpdate = "update tblCTCSTransactionType set fldTransactionDesc=@fldTransactionDesc, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTransactionType=@fldTransactionType ";

            DataTable dtTransactionTypeTemp = new DataTable();

            dtTransactionTypeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionType", transactionType)
            });

            if (dtTransactionTypeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionTypeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }

        public void DeleteTransactionType(string transactionType)
        {
            string stmt = "delete from tblCTCSTransactionType where fldTransactionType=@fldTransactionType ";

            ocsdbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionType",transactionType)
            });
        }

        public void AddToTransactionTypeTempToDelete(string transactionType)
        {
            string strQuerySelect = "Select fldTransactionDesc from tblCTCSTransactionType WHERE fldTransactionType = @fldTransactionType ";
            string strQueryInsert = "Insert into tblCTCSTransactionTypeTemp (fldTransactionType, fldTransactionDesc, fldBankCode) " +
                                    "VALUES (@fldTransactionType, @fldTransactionDesc, @fldBankCode)";
            string strQueryUpdate = "Update tblCTCSTransactionTypeTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTransactionType=@fldTransactionType ";

            DataTable dtTransactionTypeTemp = new DataTable();

            dtTransactionTypeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionType", transactionType)
            });

            if (dtTransactionTypeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionTypeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldApproveStatus", "D")
                });
            }
        }

        public void AddToTransactionTypeTempToUpdate(string transactionType)
        {
            string strQuerySelect = "Select fldTransactionDesc from tblCTCSTransactionType WHERE fldTransactionType = @fldTransactionType ";
            string strQueryInsert = "Insert into tblCTCSTransactionTypeTemp (fldTransactionType, fldTransactionDesc, fldBankCode) " +
                                    "VALUES (@fldTransactionType, @fldTransactionDesc, @fldBankCode)";
            string strQueryUpdate = "Update tblCTCSTransactionTypeTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTransactionType=@fldTransactionType ";

            DataTable dtTransactionTypeTemp = new DataTable();

            dtTransactionTypeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect,
                new SqlParameter[] { new SqlParameter("@fldTransactionType", transactionType)
                });

            if (dtTransactionTypeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionTypeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldApproveStatus", "U")
                });
            }
        }

        public void DeleteInTransactionTypeTemp(string transactionType)
        {

            string stmt = "delete from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType ";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionType", transactionType)
            });
        }

        public void CreateTransactionTypeInTemp(FormCollection col, AccountModel currentUser)
        {

            Dictionary<string, dynamic> sqlTransactionType = new Dictionary<string, dynamic>();
            sqlTransactionType.Add("fldTransactionType", col["transType"]);
            sqlTransactionType.Add("fldTransactionDesc", col["transDesc"]);
            sqlTransactionType.Add("fldActive", "Y");
            sqlTransactionType.Add("fldCreateUserId", currentUser.UserId);
            sqlTransactionType.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionType.Add("fldUpdateUserId", currentUser.UserId);
            sqlTransactionType.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionType.Add("fldApproveStatus", "A");
            sqlTransactionType.Add("fldBankCode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblCTCSTransactionTypeTemp", sqlTransactionType);
        }

        public void CreateTransactionType(string transactionType)
        {
            string strQuerySelect = "select fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType ";
            string strQueryInsert = "Insert into tblCTCSTransactionType(fldTransactionType, fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp) " +
                                "VALUES (@fldTransactionType, @fldTransactionDesc, @fldActive, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp) ";

            DataTable dtTransactionTypeTemp = new DataTable();

            dtTransactionTypeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionType", transactionType)
            });

            if (dtTransactionTypeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionTypeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionType", transactionType),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldActive", drItem["fldActive"]),
                    new SqlParameter("@fldCreateUserId", drItem["fldCreateUserId"]),
                    new SqlParameter("@fldCreateTimeStamp", drItem["fldCreateTimeStamp"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["transType"].Equals(""))
            {
                err.Add(Locale.TransactionTypeCannotBeEmpty);
            }
            if (CheckExist(col["transType"]))
            {
                err.Add(Locale.TransactionTypeAlreadyExists);
            }
            if (col["transDesc"].Equals(""))
            {
                err.Add(Locale.TransactionTypeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["transType"]))
            {
                err.Add(Locale.TransactionTypePendingApproval);
            }
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["transDesc"].Equals(""))
            {
                err.Add(Locale.TransactionTypeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["transType"]))
            {
                err.Add(Locale.TransactionTypePendingApproval);
            }
            return err;
        }

        public bool CheckExist(string transType)
        {
            string stmt = "select * from tblCTCSTransactionType where fldTransactionType=@fldTransactionType ";
            return ocsdbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTransactionType", transType)
            });
        }

        public bool CheckPendingApproval(string transType)
        {
            string stmt = "select * from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType ";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTransactionType", transType)
            });
        }

    }
}
