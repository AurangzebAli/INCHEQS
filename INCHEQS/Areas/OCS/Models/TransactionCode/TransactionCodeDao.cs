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

namespace INCHEQS.Areas.OCS.Models.TransactionCode
{
    public class TransactionCodeDao : ITransactionCodeDao
    {


        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        public TransactionCodeDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public async Task<DataTable> ListAllAsync()
        {
            return await Task.Run(() => ListAll());
        }

        public async Task<DataTable> FindAsync(string TransCode)
        {
            return await Task.Run(() => Find(TransCode));
        }


        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT * FROM tblCTCSTransactionMaster WHERE fldActive = 'Y' And fldTransactionDesc like '%' AND fldTransactionCode Like '%' order by fldTransactionCode  ";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }

        public DataTable Find(string TransCode)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldTransactionCode, fldTransactionDesc, fldTransactionType, fldUpdateTimestamp FROM tblCTCSTransactionMaster WHERE fldTransactionCode = @transCode";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@transCode", TransCode) });

            return ds;
        }

        public void UpdateTransactionCodeInTemp(FormCollection col)
        {

            string stmt = "update tblCTCSTransactionMasterTemp set fldTransactionDesc=@fldTransactionDesc, fldTransactionType=@fldTransactionType, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTransactionCode=@fldTransactionCode ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionDesc",col["transDesc"]),
                new SqlParameter("@fldTransactionType",col["transType"]),
                new SqlParameter("@fldUpdateTimestamp",DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldTransactionCode",col["transCode"])
            });
        }

        public void UpdateTransactionCode(string transactionCode)
        {
            string strQuerySelect = "select fldTransactionDesc, fldTransactionType, fldUpdateUserId, fldUpdateTimeStamp FROM tblCTCSTransactionMasterTemp where fldTransactionCode=@fldTransactionCode ";
            string strQueryUpdate = "update tblCTCSTransactionMaster set fldTransactionDesc=@fldTransactionDesc, fldTransactionType=@fldTransactionType, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTransactionCode=@fldTransactionCode ";

            DataTable dtTransactionCodeTemp = new DataTable();

            dtTransactionCodeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionCode", transactionCode)
            });

            if (dtTransactionCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionCodeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }

        public void DeleteTransactionCode(string transactionCode)
        {
            string stmt = "delete from tblCTCSTransactionMaster where fldTransactionCode=@fldTransactionCode ";

            ocsdbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionCode",transactionCode)
            });
        }

        public void AddToTransactionCodeTempToDelete(string transactionCode)
        {
            string strQuerySelect = "Select fldTransactionDesc,fldTransactionType from tblCTCSTransactionMaster WHERE fldTransactionCode = @fldTransactionCode ";
            string strQueryInsert = "Insert into tblCTCSTransactionMasterTemp (fldTransactionCode, fldTransactionDesc, fldTransactionType, fldBankCode) " +
                                    "VALUES (@fldTransactionCode, @fldTransactionDesc, @fldTransactionType, @fldBankCode)";
            string strQueryUpdate = "Update tblCTCSTransactionMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTransactionCode=@fldTransactionCode ";

            DataTable dtTransactionCodeTemp = new DataTable();

            dtTransactionCodeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionCode", transactionCode)
            });

            if (dtTransactionCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionCodeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldApproveStatus", "D")
                });
            }
        }

        public void AddToTransactionCodeTempToUpdate(string transactionCode)
        {
            string strQuerySelect = "Select fldTransactionDesc,fldTransactionType from tblCTCSTransactionMaster WHERE fldTransactionCode = @fldTransactionCode ";
            string strQueryInsert = "Insert into tblCTCSTransactionMasterTemp (fldTransactionCode, fldTransactionDesc, fldTransactionType, fldBankCode) " +
                                    "VALUES (@fldTransactionCode, @fldTransactionDesc, @fldTransactionType, @fldBankCode)";
            string strQueryUpdate = "Update tblCTCSTransactionMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTransactionCode=@fldTransactionCode ";

            DataTable dtTransactionCodeTemp = new DataTable();

            dtTransactionCodeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect,
                new SqlParameter[] { new SqlParameter("@fldTransactionCode", transactionCode)
                });

            if (dtTransactionCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionCodeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldApproveStatus", "U")
                });
            }
        }

        public void DeleteInTransactionCodeTemp(string transactionCode)
        {

            string stmt = "delete from tblCTCSTransactionMasterTemp where fldTransactionCode=@fldTransactionCode ";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTransactionCode", transactionCode)
            });
        }

        public void CreateTransactionCodeInTemp(FormCollection col, AccountModel currentUser)
        {

            Dictionary<string, dynamic> sqlTransactionCode = new Dictionary<string, dynamic>();
            sqlTransactionCode.Add("fldTransactionCode", col["transCode"]);
            sqlTransactionCode.Add("fldTransactionDesc", col["transDesc"]);
            sqlTransactionCode.Add("fldTransactionType", col["transType"]);
            sqlTransactionCode.Add("fldActive", "Y");
            sqlTransactionCode.Add("fldCreateUserId", currentUser.UserId);
            sqlTransactionCode.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionCode.Add("fldUpdateUserId", currentUser.UserId);
            sqlTransactionCode.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTransactionCode.Add("fldApproveStatus", "A");
            sqlTransactionCode.Add("fldBankCode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblCTCSTransactionMasterTemp", sqlTransactionCode);
        }

        public void CreateTransactionCode(string transactionCode)
        {
            string strQuerySelect = "select fldTransactionDesc, fldTransactionType, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode from tblCTCSTransactionMasterTemp where fldTransactionCode=@fldTransactionCode ";
            string strQueryInsert = "Insert into tblCTCSTransactionMaster(fldTransactionCode, fldTransactionDesc, fldTransactionType, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp) " +
                                "VALUES (@fldTransactionCode, @fldTransactionDesc,@fldTransactionType,  @fldActive, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp) ";

            DataTable dtTransactionCodeTemp = new DataTable();

            dtTransactionCodeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTransactionCode", transactionCode)
            });

            if (dtTransactionCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTransactionCodeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionCode", transactionCode),
                    new SqlParameter("@fldTransactionDesc", drItem["fldTransactionDesc"]),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
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

            if (col["transCode"].Equals(""))
            {
                err.Add(Locale.TransactionCodeCannotBeEmpty);
            }
            if (CheckExist(col["transCode"]))
            {
                err.Add(Locale.TransactionCodeAlreadyExists);
            }
            if (col["transDesc"].Equals(""))
            {
                err.Add(Locale.TransactionCodeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["transCode"]))
            {
                err.Add(Locale.TransactionCodePendingApproval);
            }
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["transDesc"].Equals(""))
            {
                err.Add(Locale.TransactionCodeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["transCode"]))
            {
                err.Add(Locale.TransactionCodePendingApproval);
            }
            return err;
        }

        public bool CheckExist(string transCode)
        {
            string stmt = "select * from tblCTCSTransactionMaster where fldTransactionCode=@fldTransactionCode ";
            return ocsdbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTransactionCode", transCode)
            });
        }

        public bool CheckPendingApproval(string transCode)
        {
            string stmt = "select * from tblCTCSTransactionMasterTemp where fldTransactionCode=@fldTransactionCode ";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTransactionCode", transCode)
            });
        }
    }
}
