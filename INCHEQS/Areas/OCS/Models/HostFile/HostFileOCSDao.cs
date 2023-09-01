using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Models.HostFile;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Areas.OCS.Models.HostFile {
    public class HostFileOCSDao : IHostFileOCSDao{

        private readonly ApplicationDbContext dbContext;
        public HostFileOCSDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable GetOutwardReturnDetail(string clearDate, string dataFileName) {
            DataTable ds = new DataTable();
            string stmt = " SELECT err.fldDataFileName , err.fldUIC, err.fldAccountNumber , err.fldChequeSerialNo , err.fldAmount , err.fldTransCode , err.fldReturnAmount , (CASE isnull(inward.fldApprovalStatus,'') WHEN 'E' THEN 'RE-BATCH' ELSE err.fldItemStatus END) as fldItemStatus ,  isnull(err.fldFileType,'1') as fldFileType , err.fldBatchNumber, ack.fldReason as fldRemark " +

            " FROM tbloutwardreturnitem  as err with(nolock) LEFT JOIN view_inwarditem as inward with(nolock) ON err.fldUIC = inward.fldUIC LEFT JOIN tblAckRejectedItems as ack with(nolock) on right(inward.fldPresentingBankItemSequenceNumber, len(ack.fldOriginalSeqNo)) = ack.fldOriginalSeqNo and ack.fldDataFileName = @ackfldDataFileName " +

            " WHERE datediff(d, err.fldClearDate,@fldClearDate)= 0 AND err.fldDataFileName = @errfldDataFileName ";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldCleardate", DateUtils.formatDateToFileDate(clearDate)),
                new SqlParameter("@ackfldDataFileName", dataFileName),
                new SqlParameter("@errfldDataFileName", dataFileName)
            });
            return ds;
        }

        //public DataTable getFilemanager(string clearDate, string taskId, string BankCode)
        //{
        //    DataTable ds = new DataTable();
        //    string stmt = " Select fldfilename, fldcreatetimestamp as ModifiedDate, fldCleardate , fldremarks from tblfilemanager" +
        //    " WHERE datediff(d, fldCleardate,@fldClearDate)= 0 AND fldTaskId = @fldTaskId AND fldBankcode = @fldBankcode";

        //    ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
        //        new SqlParameter("@fldClearDate", DateUtils.formatDateToFileDate(clearDate)),
        //        new SqlParameter("@fldTaskId", taskId),
        //        new SqlParameter("@fldBankcode", BankCode)
        //    });
        //    return ds;
        //}

        public void UpdateUPIbatchAck(string AckMessage)
        {
            string sMySQL = "UPDATE tblinwarditeminfostatus set fldUPIAckMessage = @AckMessage where fldBatchno = @BatchNo AND isNULL(fldupigenerated,'') <> '' AND fldApprovalStatus = 'R' ";


            dbContext.ExecuteNonQuery(sMySQL, new[] {
                new SqlParameter("@AckMessage ", AckMessage),
                new SqlParameter("@BatchNo", AckMessage.Substring(17,9))
                });

        }

        public void OutwardReturnDetailRegen(string clearDate, string dataFileName)
        {
            DataTable ds = new DataTable();
            string batchno = "";
            string sMySQL = "Select fldBatchNumber from tbloutwardreturnitem where fldClearDate=@fldcleardate and fldDataFileName=@fldDataFileName";
            ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                new SqlParameter("@fldcleardate", DateUtils.formatDateToFileDate(clearDate)),
                new SqlParameter("@fldDataFileName", dataFileName)
            });
            foreach (DataRow row in ds.Rows)
            {
                batchno = row["fldBatchNumber"].ToString().Trim();
            }
            sMySQL = "insert into tblDataProcess (fldProcessName, fldStatus, fldClearDate, fldStartTime, " +
                    "fldCreateUserId, fldCreateTimestamp, fldUpdateUserId, fldUpdateTimestamp, fldRemarks, fldBatchID, fldPosPayType, fldBankCode) " +
                    "values ('ICSGenerateUPI', 1, @fldCleardate, getDate(),@fldUserId, Getdate(), @fldUserId, " +
                    "Getdate(),'Generated Process has been started',@fldbatchno,'',@fldBankCode)";
            ds = dbContext.GetRecordsAsDataTable(sMySQL, new[] {
                new SqlParameter("@fldCleardate", DateUtils.formatDateToFileDate(clearDate)),
                new SqlParameter("@fldUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldbatchno", batchno),
                new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
            });
        }

        public HostFileOCSModel GetDataFromHostFileConfig(string taskId) {
            HostFileOCSModel hostFileModel = new HostFileOCSModel();
            string stmt = "SELECT * FROM tblHostFileConfig WHERE fldTaskId=@fldTaskId";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
            if (dt.Rows.Count > 0) {
                DataRow row = dt.Rows[0];
                hostFileModel.fldProcessName = row["fldProcessName"].ToString();
                hostFileModel.fldHostFileDesc = row["fldHostFileDesc"].ToString();
                hostFileModel.fldPosPayType = row["fldPosPayType"].ToString();
                hostFileModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                hostFileModel.fldFileExt = row["fldFileExt"].ToString();
                hostFileModel.fldTaskRole = row["fldTaskRole"].ToString();
                hostFileModel.fldFTPFolder = row["fldFTPFolder"].ToString();
                return hostFileModel;
            }

            return null;
        }

        public void UpdateFTPSend(string fileName) {
            string sMySQL = "UPDATE tblFileManager set fldSend = 'Y' where fldFileName = @fldFileName";
            dbContext.ExecuteNonQuery(sMySQL, new[] {
                    new SqlParameter("@fldFileName", fileName)
                });

        }

    }
}