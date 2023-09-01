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

namespace INCHEQS.Areas.OCS.Models.ProcessDate
{
    public class ProcessDateDao : IProcessDateDao
    {


        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        public ProcessDateDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
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

        public string getCurrentDate()
        {
            DataTable ds = new DataTable();
            string currentDate = "";
            string stmt = "Select top 1 convert(char(10),fldProcessDate,120) as fldProcessDate From tblProcessDate Where fldStatus = N'Y' Order By fldProcessDate desc";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt);

            if (ds.Rows.Count > 0)
            {
                currentDate = ds.Rows[0]["fldProcessDate"].ToString();

            }
            //currentDate = DateTime.ParseExact(currentDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd-MMMM-yyyy");
            return currentDate;
        }

        public string getNextProcessDate()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            DataTable processDate = new DataTable();
            string date = "";

            sqlParameterNext.Add(new SqlParameter("@CurrentDate", getCurrentDate()));
            //Excute the command
            processDate = ocsdbContext.GetRecordsAsDataTableSP("sp_GetNextProcessDate", sqlParameterNext.ToArray());

            if (processDate.Rows.Count > 0)
            {
                date = processDate.Rows[0]["ProcessDate"].ToString();

            }

            return date;
        }

        public DataTable Find(string TransType)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldTransactionType,fldTransactionDesc, fldUpdateTimestamp FROM tblCTCSTransactionType WHERE fldTransactionType = @transType";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@transType", TransType) });

            return ds;
        }

        public void UpdateProcessDate(FormCollection col)
        {

            string stmt = "update tblProcessDate set fldStatus= @fldStatus,fldCloseBy=@fldCloseBy, fldCloseTimestamp=@fldCloseTimestamp where fldProcessDate = @fldProcessDate ";

            ocsdbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldStatus","N"),
                new SqlParameter("@fldCloseBy", CurrentUser.Account.UserId),
                new SqlParameter("@fldCloseTimestamp",DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldProcessDate",col["currentDate"])
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
            string strQuerySelect = "Select fldTransactionDesc, fldBankCode from tblCTCSTransactionType WHERE fldTransactionType = @fldTransactionType ";
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

        public void CreateProcessDate(FormCollection col, AccountModel currentUser)
        {

            Dictionary<string, dynamic> sqlProcessDate = new Dictionary<string, dynamic>();
            sqlProcessDate.Add("fldProcessDate", col["processDate"]);
            sqlProcessDate.Add("fldStatus", "Y");
            sqlProcessDate.Add("fldActiveBy", currentUser.UserId);
            sqlProcessDate.Add("fldActiveTimestamp", DateUtils.GetCurrentDatetime());
            sqlProcessDate.Add("fldCloseBy", currentUser.UserId);
            sqlProcessDate.Add("fldCloseTimestamp", DateUtils.GetCurrentDatetime());

            ocsdbContext.ConstructAndExecuteInsertCommand("tblProcessDate", sqlProcessDate);
        }

        public void CreateTransactionType(string transactionType)
        {
            string strQuerySelect = "select fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode from tblCTCSTransactionTypeTemp where fldTransactionType=@fldTransactionType ";
            string strQueryInsert = "Insert into tblCTCSTransactionType(fldTransactionType, fldTransactionDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode) " +
                                "VALUES (@fldTransactionType, @fldTransactionDesc, @fldActive, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp, @fldBankCode) ";

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
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"]),
                    new SqlParameter("@fldBankCode", drItem["fldBankCode"])
                });
            }
        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["transType"].Equals(""))
            {
                err.Add("Transaction Type cannot be empty");
            }
            if (CheckExist(col["transType"]))
            {
                err.Add("Transaction Type already exist.");
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
    }
}
