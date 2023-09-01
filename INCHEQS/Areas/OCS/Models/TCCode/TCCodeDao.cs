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

namespace INCHEQS.Areas.OCS.Models.TCCode
{
    public class TCCodeDao : ITCCodeDao
    {


        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        public TCCodeDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public async Task<DataTable> ListAllAsync()
        {
            return await Task.Run(() => ListAll());
        }

        public async Task<DataTable> FindAsync(string TCCode)
        {
            return await Task.Run(() => Find(TCCode));
        }

        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT * FROM tblTCMaster WHERE fldActive = 'Y' And fldTCDesc like '%' AND fldTCCode Like '%' order by fldTCCode  ";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }

        public DataTable Find(string TCCode)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldTCCode,fldTCDesc, fldUpdateTimestamp FROM tblTCMaster WHERE fldTCCode = @tcCode";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@tcCode", TCCode) });

            return ds;
        }

        public void UpdateTCCodeInTemp(FormCollection col)
        {

            string stmt = "update tblTCMasterTemp set fldTCDesc=@fldTCDesc, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTCCode=@fldTCCode ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTCDesc",col["tcDesc"]),
                new SqlParameter("@fldUpdateTimestamp",DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldTCCode",col["tcCode"])
            });
        }

        public void UpdateTCCode(string tcCode)
        {
            string strQuerySelect = "select fldTCDesc, fldUpdateUserId, fldUpdateTimeStamp FROM tblTCMasterTemp where fldTCCode=@fldTCCode ";
            string strQueryUpdate = "update tblTCMaster set fldTCDesc=@fldTCDesc, fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldTCCode=@fldTCCode ";

            DataTable dtTCCodeTemp = new DataTable();

            dtTCCodeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTCCode", tcCode)
            });

            if (dtTCCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTCCodeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldTCDesc", drItem["fldTCDesc"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }

        public void DeleteTCCode(string tcCode)
        {
            string stmt = "delete from tblTCMaster where fldTCCode=@fldTCCode ";

            ocsdbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTCCode",tcCode)
            });
        }

        public void AddToTCCodeTempToDelete(string tcCode)
        {
            string strQuerySelect = "Select fldTCDesc from tblTCMaster WHERE fldTCCode = @fldTCCode ";
            string strQueryInsert = "Insert into tblTCMasterTemp (fldTCCode, fldTCDesc, fldBankCode) " +
                                    "VALUES (@fldTCCode, @fldTCDesc, @fldBankCode)";
            string strQueryUpdate = "Update tblTCMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTCCode=@fldTCCode ";

            DataTable dtTCCodeTemp = new DataTable();

            dtTCCodeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTCCode", tcCode)
            });

            if (dtTCCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTCCodeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldTCDesc", drItem["fldTCDesc"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldApproveStatus", "D")
                });
            }
        }

        public void AddToTCCodeTempToUpdate(string tcCode)
        {
            string strQuerySelect = "Select fldTCDesc from tblTCMaster WHERE fldTCCode = @fldTCCode ";
            string strQueryInsert = "Insert into tblTCMasterTemp (fldTCCode, fldTCDesc, fldBankCode) " +
                                    "VALUES (@fldTCCode, @fldTCDesc, @fldBankCode)";
            string strQueryUpdate = "Update tblTCMasterTemp SET fldApproveStatus=@fldApproveStatus WHERE fldTCCode=@fldTCCode ";

            DataTable dtTCCodeTemp = new DataTable();

            dtTCCodeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect,
                new SqlParameter[] { new SqlParameter("@fldTCCode", tcCode)
                });

            if (dtTCCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTCCodeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldTCDesc", drItem["fldTCDesc"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldApproveStatus", "U")
                });
            }
        }

        public void DeleteInTCCodeTemp(string tcCode)
        {

            string stmt = "delete from tblTCMasterTemp where fldTCCode=@fldTCCode ";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTCCode", tcCode)
            });
        }

        public void CreateTCCodeInTemp(FormCollection col, AccountModel currentUser)
        {

            Dictionary<string, dynamic> sqlTCCode = new Dictionary<string, dynamic>();
            sqlTCCode.Add("fldTCCode", col["tcCode"]);
            sqlTCCode.Add("fldTCDesc", col["tcDesc"]);
            sqlTCCode.Add("fldActive", "Y");
            sqlTCCode.Add("fldCreateUserId", currentUser.UserId);
            sqlTCCode.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTCCode.Add("fldUpdateUserId", currentUser.UserId);
            sqlTCCode.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlTCCode.Add("fldApproveStatus", "A");
            sqlTCCode.Add("fldBankCode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblTCMasterTemp", sqlTCCode);
        }

        public void CreateTCCode(string tcCode)
        {
            string strQuerySelect = "select fldTCDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode from tblTCMasterTemp where fldTCCode=@fldTCCode ";
            string strQueryInsert = "Insert into tblTCMaster(fldTCCode, fldTCDesc, fldActive, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp) " +
                                "VALUES (@fldTCCode, @fldTCDesc, @fldActive, @fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp) ";

            DataTable dtTCCodeTemp = new DataTable();

            dtTCCodeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldTCCode", tcCode)
            });

            if (dtTCCodeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtTCCodeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTCCode", tcCode),
                    new SqlParameter("@fldTCDesc", drItem["fldTCDesc"]),
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

            if (col["tcCode"].Equals(""))
            {
                err.Add(Locale.TCCodeCannotBeEmpty);
            }
            if (CheckExist(col["tcCode"]))
            {
                err.Add(Locale.TCCodeAlreadyExists);
            }
            if (col["tcDesc"].Equals(""))
            {
                err.Add(Locale.TCCodeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["tcCode"]))
            {
                err.Add(Locale.TCCodePendingApproval);
            }
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["tcDesc"].Equals(""))
            {
                err.Add(Locale.TCCodeDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["tcCode"]))
            {
                err.Add(Locale.TCCodePendingApproval);
            }
            return err;
        }

        public bool CheckExist(string tcCode)
        {
            string stmt = "select * from tblTCMaster where fldTCCode=@fldTCCode ";
            return ocsdbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTCCode", tcCode)
            });
        }

        public bool CheckPendingApproval(string tcCode)
        {
            string stmt = "select * from tblTCMasterTemp where fldTCCode=@fldTCCode ";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldTCCode", tcCode)
            });
        }
    }
}
