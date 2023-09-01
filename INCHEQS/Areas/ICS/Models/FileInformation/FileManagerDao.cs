using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Models.FileInformation {
    public class FileManagerDao : IFileManagerDao{
        
        private readonly ApplicationDbContext dbContext;
        public FileManagerDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public void ClearRemarks(string fileName) {
            string stmt = "UPDATE tblFileManager SET [fldremarks] = NULL WHERE fldFileName = @fileName";
            
            dbContext.ExecuteNonQuery( stmt,new[] {
                new SqlParameter("@fileName", fileName)
            });
            
        }

        public bool CheckFileExist(AccountModel currentUser, string fileName) {

            string sql = "SELECT fldFilePath FROM tblFileManager WHERE fldFileName = @fldFileName AND fldBankCode = @fldBankCode";

            if (!dbContext.CheckExist(sql, new[]{
                new SqlParameter("@fldFileName", fileName) ,
                new SqlParameter("@fldBankCode", currentUser.BankCode)
                })) {
                return false;
            } else {
                return true;
            }
        }

        public void InsertToFileManager(AccountModel currentUser, string taskid, string path, string fileName, string clearDate) {

            Dictionary<string, dynamic> sqlFileManager = new Dictionary<string, dynamic>();
            sqlFileManager.Add("fldTaskId", taskid);
            sqlFileManager.Add("fldFilePath", path);
            sqlFileManager.Add("fldFileName", fileName);
            sqlFileManager.Add("fldClearDate", DateUtils.formatDateToSql(clearDate));
            sqlFileManager.Add("fldLoad", "N");
            sqlFileManager.Add("fldCreateUserId", currentUser.UserId);
            sqlFileManager.Add("fldCreateTimestamp", DateUtils.GetCurrentDatetimeForSql());
            sqlFileManager.Add("fldBankCode", currentUser.BankCode);
            
            dbContext.ConstructAndExecuteInsertCommand("tblFileManager", sqlFileManager);
        }

        public void InsertToFileManagerOcs(AccountModel currentUser, string path, string fileName, string clearDate)
        {
            Dictionary<string, dynamic> sqlFileManager = new Dictionary<string, dynamic>();
            sqlFileManager.Add("fldFilePath", path);
            sqlFileManager.Add("fldFileName", fileName);
            sqlFileManager.Add("fldClearDate", clearDate);
            sqlFileManager.Add("fldFileUploadFlag", "1");
            sqlFileManager.Add("fldCreateUserId", currentUser.UserId);
            sqlFileManager.Add("fldCreateTimestamp", DateUtils.GetCurrentDatetimeForSql());
            sqlFileManager.Add("fldBankCode", currentUser.BankCode);
            dbContext.ConstructAndExecuteInsertCommand("tblFileManagerOCS", sqlFileManager);
        }
        public void DeleteFileManagerOCS(string fileName, string filepath, string bankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                sqlParameterNext.Add(new SqlParameter("@fldFileName", fileName));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
                sqlParameterNext.Add(new SqlParameter("@fldfilepath", filepath));
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdFileManagerOCS", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public void DeleteFileManager(string fileName, string filepath, string bankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                sqlParameterNext.Add(new SqlParameter("@fldFileName", fileName));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
                sqlParameterNext.Add(new SqlParameter("@fldfilepath", filepath));
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdFileManagerICS", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
        public void UpdateFileManager(AccountModel currentUser, string path) {

            string sMySQL = "UPDATE tblFileManager SET fldCreateUserId = @fldCreateUserId, fldCreateTimestamp = getdate() WHERE CAST(fldFilePath as nvarchar(max)) =  @fldFilePath AND fldBankCode = @fldBankCode";

            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@fldCreateUserId", currentUser.UserId),
                    new SqlParameter("@fldFilePath", path),
                    new SqlParameter("@fldBankCode", currentUser.BankCode),
                });

        }
        
        public void UpdateFileManagerLoadFile(string fileName, string clearDate) {

            string deleteDataSql = "UPDATE tblFileManager SET fldLoad='Y' WHERE fldFileName=@fldFileName AND fldClearDate = @fldClearDate ";
            dbContext.ExecuteNonQuery(deleteDataSql, new[] {
            new SqlParameter("@fldClearDate", DateUtils.formatDateToSql(clearDate)),
            new SqlParameter("@fldFileName", fileName)
        });

        }

        public DataTable getBankDesc(string bankCode)
        {
            DataTable ds = new DataTable();
            string stmt = "select fldbankcode + ' - ' + fldbankdesc as fldbankdesc from tblbankmaster where fldbankcode=@bankCode";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@bankCode", bankCode) });
            return ds;
        }
    }
}