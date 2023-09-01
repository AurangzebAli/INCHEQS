using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.LateReturnedMaintenance
{
    public class LateReturnedMaintenanceDao : ILateReturnedMaintenanceDao
    {
        private readonly ApplicationDbContext dbContext;
        public LateReturnedMaintenanceDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<LateReturnMaintenanceModal>> getLateReturnMaintenanceAsyn(FormCollection collection)
        {
            return await Task.Run(() => getLateReturnMaintenance(collection));
        }
        public List<LateReturnMaintenanceModal> GetClearDateforLateMaintenance()
        {
            List<LateReturnMaintenanceModal> lateReturnMaintenanceModal = new List<LateReturnMaintenanceModal>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            DataTable dsClearDate = dbContext.GetRecordsAsDataTableSP("spcgGetLateReturnClearDate", sqlParameterNext.ToArray());
            //string stmt = "select distinct top 1 a.fldClearDate,ClearDate from ( ";
            //stmt = stmt + "select convert(varchar(10),fldClearDate,120) as fldClearDate,fldClearDate as ClearDate from tblInwardItemInfo ";
            //stmt = stmt + "union ";
            //stmt = stmt + "select convert(varchar(10),fldClearDate,120) as fldClearDate,fldClearDate as ClearDate from tblInwardItemInfoH ";
            //stmt = stmt + ") as a where cleardate not in  ( select top 1 fldClearDate as cleardate1 from tblinwardcleardate order by cleardate1 desc) order by ClearDate desc";
            //DataTable dsClearDate = dbContext.GetRecordsAsDataTable(stmt, null);
            foreach (DataRow row in dsClearDate.Rows)
            {
                LateReturnMaintenanceModal LRM = new LateReturnMaintenanceModal();
                LRM.fldClearDate = row["fldClearDate"].ToString();
                lateReturnMaintenanceModal.Add(LRM);
            }
            return lateReturnMaintenanceModal;
        }
        public bool CheckLateReturnExistMainTable(LateReturnMaintenanceModal col)
        {
            string stmt = "select * from tblInwardItemInfo where fldUIC =@fldUIC and fldcleardate=@fldcleardate and fldissuebankcode=@fldissuebankcode";
            return dbContext.CheckExist(stmt, new[] {
                  new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldClearDate",col.fldClearDate),
               new SqlParameter("@fldissuebankcode", col.fldBankCode),
            });
        }
        public bool CheckLateReturnExistHistoryTable(LateReturnMaintenanceModal col)
        {
            string stmt = "select * from tblInwardItemInfoH where fldUIC =@fldUIC and fldcleardate=@fldcleardate and fldissuebankcode=@fldissuebankcode";
            return dbContext.CheckExist(stmt, new[] {
                  new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldClearDate", col.fldClearDate),
                new SqlParameter("@fldissuebankcode", col.fldBankCode),
            });
        }
        public List<string> InsertLateReturnMaintenanceRecord(LateReturnMaintenanceModal col)
        {
            List<string> err = new List<string>();
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            if (CheckLateReturnExistMainTable(col) == false)
            {
                if (CheckLateReturnExistHistoryTable(col) == true)
                {
                    string stmt = "Update View_InwardItemH set fldUPIDate = getDate(), fldRejectCode = @fldRejectCode, fldRemarks = @fldRemarks, fldApprovalStatus = 'R'where fldUIC = @fldUIC and fldcleardate = @fldcleardate ;";
                    stmt = stmt + "Update View_InwardItemH set fldApprovalTimeStamp = getDate(),  fldApprovalUserId = @fldApprovalUserId where fldUIC = @fldUIC and fldcleardate = @fldcleardate;";
                    stmt = stmt + "Update View_InwardItemH set fldUpdateUserID = @fldApprovalUserId,  fldUpdateTimeStamp = getDate() where fldUIC = @fldUIC and fldcleardate = @fldcleardate;";
                    dbContext.ExecuteNonQuery(stmt, new[] {
                        new SqlParameter("@fldRejectCode", col.fldrejectCode),
                        new SqlParameter("@fldRemarks", col.fldRemarks),
                        new SqlParameter("@fldApprovalUserId", CurrentUser.Account.UserId),
                        new SqlParameter("@fldUIC", col.fldUIC),
                        new SqlParameter("@fldcleardate",col.fldClearDate)
                        //CurrentUser.Account.UserId,
                    });
                    insertInwarditem(col);
                    insertLatemaintenanceh(col);
                    InsertHistoryH(col);
                    err.Add("Record saved successfully");
                }
                else
                {
                    err.Add("No record found");
                }
            }
            else
            {
                
                string stmt = "Update tblInwardItemInfoStatus set fldUPIDate = getdate(), fldRejectCode = @fldRejectCode, fldRemarks = @fldRemarks, fldApprovalStatus = 'R', fldApprovalTimeStamp = getdate(), fldApprovalUserId = @fldApprovalUserId, fldUpdateUserID = @fldApprovalUserId, fldUpdateTimeStamp = getdate()  where fldinwarditemid in (select fldInwardItemID from tblInwardItemInfo where flduic = @fldUIC and fldcleardate = @fldcleardate)";
                dbContext.ExecuteNonQuery(stmt, new[] {
                        new SqlParameter("@fldRejectCode", col.fldrejectCode),
                        new SqlParameter("@fldRemarks", col.fldRemarks),
                        new SqlParameter("@fldApprovalUserId", CurrentUser.Account.UserId),
                        new SqlParameter("@fldUIC", col.fldUIC),
                        new SqlParameter("@fldcleardate",col.fldClearDate)
                    });
                insertLatemaintenance(col);
                InsertHistory(col);
                err.Add("Record updated successfully");

            }
            return err;
        }
        public void insertInwarditem(LateReturnMaintenanceModal col)
        {
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblInwardItemInfo"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;
            string stmt = "insert into tblinwarditeminfo ";
            stmt = stmt + "(fldInwardItemId, fldUIC, fldCheckDigit, fldOriCheckDigit, fldChequeSerialNo, fldOriChequeSerialNo, fldPreBankType, fldPreBankCode, fldPreStateCode, fldPreBranchCode, fldIssueBankType, fldIssueBankCode, fldOriIssueBankCode, fldIssueStateCode, fldOriIssueStateCode, fldIssueBranchCode, fldOriIssueBranchCode, fldIssueDigit, fldAccountNumber, fldOriAccountNumber, fldTransCode, fldOriTransCode, fldAmount, fldImageFolder, fldImageFileName, fldClearDate, fldCurrency, fldIQA, fldDocToFollow, fldImageIndicator, fldMICRDS, fldImageDS, fldCreateUserID, fldCreateTimeStamp, fldUpdateUserID, fldUpdateTimeStamp, fldNonConformance, fldNonConformance2,fldChequeType)";
            stmt = stmt + " Select '" + iNextNo + "', fldUIC, fldCheckDigit, fldOriCheckDigit, fldChequeSerialNo, fldOriChequeSerialNo, fldPreBankType, fldPreBankCode, fldPreStateCode, fldPreBranchCode, fldIssueBankType, fldIssueBankCode, fldOriIssueBankCode, fldIssueStateCode, fldOriIssueStateCode, fldIssueBranchCode, fldOriIssueBranchCode, fldIssueDigit, fldAccountNumber, fldOriAccountNumber, fldTransCode, fldOriTransCode, fldAmount, fldImageFolder, fldImageFileName, fldClearDate, fldCurrency, fldIQA, fldDocToFollow, fldImageIndicator, fldMICRDS, fldImageDS, @fldCreateUserID, getdate(), @fldUpdateUserID, getDate(), fldNonConformance, fldNonConformance2,fldChequeType from  tblinwarditeminfoH where fldUIC = @fldUIC and fldClearDate = @fldClearDate ; ";
            stmt = stmt + "insert into tblinwarditeminfostatus ";
            stmt = stmt + "(fldInwardItemID, fldHostAccountNo, fldHostDebit, fldAccountHolderName, fldRejectStatus1, fldRejectStatus2, fldRejectStatus3, fldRejectStatus4, fldRRstatus, fldNonConfirmStatus, fldNonConfirmUserID, fldNonConfirmUserClass, fldNonConfirmTimeStamp, fldModifiedFields, fldApprovalStatus, fldApprovalUserId, fldApprovalUserClass, fldApprovalTimeStamp, fldApprovalIndicator, fldAssignedUserId, fldCustomerConfirm, fldCharges, fldRejectCode, fldRemarks, fldUPIDate, fldUPIGenerated, fldCreateUserID, fldCreateTimeStamp, fldUpdateUserID, fldUpdateTimeStamp, fldAssignedQueue, fldReviewAll, fldAutoRejectRemarks, fldUnMatchRemarks, fldOriHostAccountNo)";
            stmt = stmt + "Select '" + iNextNo + "', fldHostAccountNo, fldHostDebit, fldAccountHolderName, fldRejectStatus1, fldRejectStatus2, fldRejectStatus3, fldRejectStatus4, fldRRstatus, fldNonConfirmStatus, fldNonConfirmUserID, fldNonConfirmUserClass, fldNonConfirmTimeStamp, fldModifiedFields, 'R', @fldApprovalUserId, fldApprovalUserClass, getdate(), 'Y', fldAssignedUserId, fldCustomerConfirm, @fldCharges, @fldRejectCode, @fldRemarks, getdate(), null, @fldCreateUserID, getdate(), @fldUpdateUserID, getDate(), fldAssignedQueue, fldReviewAll, fldAutoRejectRemarks, fldUnMatchRemarks, fldOriHostAccountNo from  tblinwarditeminfoStatusH where fldInwardItemId in ( select fldInwardItemid from tblinwarditeminfoH where fldUIC = @fldUIC and fldClearDate = @fldClearDate)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldCreateUserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUpdateUserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldcleardate",col.fldClearDate),
                new SqlParameter("@fldApprovalUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldCharges", col.fldCharges),
                new SqlParameter("@fldRejectCode", col.fldrejectCode),
                new SqlParameter("@fldRemarks", col.fldRemarks)
            });
        }

        public DataTable GetLateMaintenanceItems()
        {
            DataTable dt = new DataTable();
            //string stmt = " Select fldApprovalTimestamp,fldRejectStatus1,fldRejectStatus2,fldRejectStatus3,fldRejectStatus4,";
            //stmt = stmt + " ci.fldAmount, isNULL(um.fldUserAbb,'')[fldUserAbb], ci.fldClearDate,isNULL(fldupigenerated,'')[fldupigenerated],";
            //stmt = stmt + " fldApprovalStatus = case ci.fldApprovalStatus";
            //stmt = stmt + " when 'B' then 'Pending'";
            //stmt = stmt + " when 'P' then 'Pull Out' ";
            //stmt = stmt + " when 'R' then 'Rejected'";
            //stmt = stmt + " when 'A' then 'Approved'";
            //stmt = stmt + " else 'No Status'";
            //stmt = stmt + " end";
            //stmt = stmt + " ,ci.fldaccountNumber,ci.fldChequeSerialNo,ci.fldTranscode,ci.fldPreBankType,ci.fldPreBankCode,ci.fldPreStateCode + ci.fldPreBranchCode as fldPreBranchCode ";
            //stmt = stmt + " ,ci.fldIssueBankType,ci.fldIssueBankCode,ci.fldIssueStateCode + ci.fldIssueBranchCode as fldIssueBranchCode,ci.fldTranscode,ci.fldUIC ";
            //stmt = stmt + " from View_InwardItem ci ";
            //stmt = stmt + " Left Outer Join tblUserMaster um on um.fldUserId = ci.fldApprovalUserId";
            //stmt = stmt + " where ci.fldUIC = @fldUIC";
            //stmt = stmt + " and ci.fldcleardate= @fldClearDate";
            //stmt = stmt + " and ci.fldissuebankcode=@fldissuebankcode";
            //stmt = stmt + " union all ";
            //stmt = stmt + " Select fldApprovalTimestamp,fldRejectStatus1,fldRejectStatus2,fldRejectStatus3,fldRejectStatus4,";
            //stmt = stmt + " ci.fldAmount, isNULL(um.fldUserAbb,'')[fldUserAbb], ci.fldClearDate,isNULL(fldupigenerated,'')[fldupigenerated],";
            //stmt = stmt + " fldApprovalStatus = case ci.fldApprovalStatus";
            //stmt = stmt + " when 'B' then 'Pending'";
            //stmt = stmt + " when 'P' then 'Pull Out' ";
            //stmt = stmt + " when 'R' then 'Rejected'";
            //stmt = stmt + " when 'A' then 'Approved'";
            //stmt = stmt + " else 'No Status'";
            //stmt = stmt + " end";
            //stmt = stmt + " ,ci.fldaccountNumber,ci.fldChequeSerialNo,ci.fldTranscode,ci.fldPreBankType,ci.fldPreBankCode,ci.fldPreStateCode + ci.fldPreBranchCode as fldPreBranchCode ";
            //stmt = stmt + " ,ci.fldIssueBankType,ci.fldIssueBankCode,ci.fldIssueStateCode + ci.fldIssueBranchCode as fldIssueBranchCode,ci.fldTranscode,ci.fldUIC ";
            //stmt = stmt + " from View_InwardItemH ci ";
            //stmt = stmt + " Left Outer Join tblUserMaster um on um.fldUserId = ci.fldApprovalUserId";
            //stmt = stmt + " where ci.fldUIC = @fldUIC";
            //stmt = stmt + " and ci.fldcleardate=  @fldClearDate";
            //stmt = stmt + " and ci.fldissuebankcode=@fldissuebankcode";



            //dt = dbContext.GetRecordsAsDataTable(stmt, new[] {
            //    new SqlParameter("@fldUIC", col.fldUIC),
            //    new SqlParameter("@fldClearDate", col.fldClearDate),
            //   new SqlParameter("@fldissuebankcode", CurrentUser.Account.BankCode)
            //});
            string stmt = "SELECT * from View_LateReturnMaintenance WHERE 1=1";
            dt = dbContext.GetRecordsAsDataTable(stmt);
            return dt;
        }
        public List<string> ValidateSearch(LateReturnMaintenanceModal col)
        {
            List<string> err = new List<string>();
            DateTime ClearDate = new DateTime();
            DateTime CurrentDate = new DateTime();
            if (col.fldClearDate.Equals(""))
            {
                err.Add("Please select a Clearing Date");
            }
            if (!col.fldClearDate.Equals(""))
            {
                ClearDate = Convert.ToDateTime(col.fldClearDate).Date;
                CurrentDate = DateTime.Now.Date;
                if (ClearDate >= CurrentDate)
                {
                    err.Add("Cheque clearing date must be earlier than UPI date");
                }
                if (CurrentDate < ClearDate)
                {
                    err.Add("Clearing date cannot be later than today's date");
                }
            }
            if (col.fldUIC.Equals(""))
            {
                err.Add("Please key in UIC");
            }
            if (!col.fldUIC.Equals(""))
            {
                if (col.fldUIC.Length > 30)
                {
                    err.Add("UIC must be 30 characters");
                }
                if (col.fldUIC.Length < 30)
                {
                    err.Add("UIC must be 30 characters");
                }
            }
            if (col.fldrejectCode.Equals(0))
            {
                err.Add("Please Select Return Code");
            }
            if (col.fldrejectCode.Equals("081") && col.fldRemarks == null)
            {
                err.Add("Please key in Remarks");
            }
            if (col.fldrejectCode.Equals("000"))
            {
                err.Add("Return Code cannot be 000");
            }
            if (!col.fldCharges.Equals(""))
            {
                col.fldCharges = col.fldCharges.Replace(",", "");
                if (col.fldCharges.Length > 14)
                {
                    err.Add("Charges must be less than 100,000,000,000.00");
                }
            }

            return err;
        }
        public void insertLatemaintenance(LateReturnMaintenanceModal col)
        {
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblLateMaintenance"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;
            string stmt = "insert into tblLateMaintenance ";
            stmt = stmt + "Select '" + iNextNo + "',fldClearDate,  fldUIC, fldAccountNumber, fldChequeSerialNo, fldIssueBankType, fldIssueBankCode, fldIssueStateCode, fldIssueBranchCode,  fldPreBankType, fldPreBankCode, fldPreStateCode, ";
            stmt = stmt + "fldPreBranchCode,fldTransCode,fldCurrency, '',convert(float,fldAmount),";
            stmt = stmt + "@UserID,'R',getdate(),@fldRejectCode,@fldCharges,@fldRemarks,@UserID,getdate(),@UserID,getdate() ";
            stmt = stmt + "from  tblinwarditeminfo where fldUIC = @fldUIC and fldClearDate = @fldClearDate";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@UserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldcleardate",col.fldClearDate),
                new SqlParameter("@fldCharges", col.fldCharges),
                new SqlParameter("@fldRejectCode", col.fldrejectCode),
                new SqlParameter("@fldRemarks", col.fldRemarks)
            });
        }

        public void insertLatemaintenanceh(LateReturnMaintenanceModal col)
        {
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblLateMaintenance"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;
            string stmt = "insert into tblLateMaintenance ";
            stmt = stmt + "Select '" + iNextNo + "',fldClearDate,  fldUIC, fldAccountNumber, fldChequeSerialNo, fldIssueBankType, fldIssueBankCode, fldIssueStateCode, fldIssueBranchCode,  fldPreBankType, fldPreBankCode, fldPreStateCode, ";
            stmt = stmt + "fldPreBranchCode,fldTransCode,fldCurrency, fldChequeType, fldAmount, ";
            stmt = stmt + "@UserID,'R',getdate(),@fldRejectCode,@fldCharges,@fldRemarks,@UserID,getdate(),@UserID,getdate() ";
            stmt = stmt + "from  tblinwarditeminfoH where fldUIC = @fldUIC and fldClearDate = @fldClearDate";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@UserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldcleardate",col.fldClearDate),
                new SqlParameter("@fldCharges", col.fldCharges),
                new SqlParameter("@fldRejectCode", col.fldrejectCode),
                new SqlParameter("@fldRemarks", col.fldRemarks)
            });

        }
        public void InsertHistory(LateReturnMaintenanceModal col)
        {
            int sVerifySeq = 0;
            string sInwardItemID = "";
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            string stmt = "Select isnull(fldVerifySeq,'') as  fldVerifySeq From tblInwardItemHistory WHERE fldUIC = @fldUIC order by fldVerifySeq desc";
            DataTable VerifySeq = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldUIC", col.fldUIC) });
            if (VerifySeq.Rows.Count > 0)
            {
                foreach (DataRow item in VerifySeq.Rows)
                {
                    sVerifySeq = Convert.ToInt32(item["fldVerifySeq"].ToString());
                    sVerifySeq = sVerifySeq + 1;
                }
            }
            else
            {
                sVerifySeq = sVerifySeq + 1;
            }
            string stmt1 = "Select fldinwarditemid From tblinwarditeminfo where fldUIC = @fldUIC and fldClearDate = @fldClearDate";
            DataTable inwarditemid = dbContext.GetRecordsAsDataTable(stmt1, new[] {
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldClearDate", col.fldClearDate) });
            if (inwarditemid.Rows.Count > 0)
            {
                foreach (DataRow item in inwarditemid.Rows)
                {
                    sInwardItemID = item["fldinwarditemid"].ToString().Trim();
                }
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblInwardItemHistory"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;
            string stmt2 = "insert into tblInwardItemHistory ";
            stmt2 = stmt2 + " (fldActionStatusId,fldActionStatus,fldInwardItemID,fldUIC, fldRejectCode, fldRemarks,fldVerifySeq,fldCreateUserID,fldCreateTimeStamp)";
            stmt2 = stmt2 + "values('" + iNextNo + "','R','" + sInwardItemID + "',@fldUIC,@fldRejectCode,@fldRemarks," + sVerifySeq + ",@UserID,getdate())";
            dbContext.ExecuteNonQuery(stmt2, new[] {
                new SqlParameter("@UserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldRejectCode", col.fldrejectCode),
                new SqlParameter("@fldRemarks", col.fldRemarks)
            });
        }
        public void InsertHistoryH(LateReturnMaintenanceModal col)
        {
            int sVerifySeq = 0;
            string sInwardItemID = "";
            col.fldRemarks = col.fldRemarks.Replace("&", "%26");
            string stmt = "Select isnull(fldVerifySeq,'') as  fldVerifySeq From tblinwarditemhistoryh WHERE fldUIC = @fldUIC order by fldVerifySeq desc";
            DataTable VerifySeq = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldUIC", col.fldUIC) });
            if (VerifySeq.Rows.Count > 0)
            {
                foreach (DataRow item in VerifySeq.Rows)
                {
                    sVerifySeq = Convert.ToInt32(item["fldVerifySeq"].ToString());
                    sVerifySeq = sVerifySeq + 1;
                }
            }
            else
            {
                sVerifySeq = sVerifySeq + 1;
            }
            string stmt1 = "Select fldinwarditemid From tblinwarditeminfoh where fldUIC = @fldUIC and fldClearDate = @fldClearDate";
            DataTable inwarditemid = dbContext.GetRecordsAsDataTable(stmt1, new[] {
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldClearDate", col.fldClearDate) });
            if (inwarditemid.Rows.Count > 0)
            {
                foreach (DataRow item in inwarditemid.Rows)
                {
                    sInwardItemID = item["fldinwarditemid"].ToString().Trim();
                }
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblInwardItemHistory"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
            long iNextNo = (long)sqlParameterNext[1].Value;
            string stmt2 = "insert into tblInwardItemHistory ";
            stmt2 = stmt2 + " (fldActionStatusId,fldActionStatus,fldInwardItemID,fldUIC, fldRejectCode, fldRemarks,fldVerifySeq,fldCreateUserID,fldCreateTimeStamp)";
            stmt2 = stmt2 + "values('" + iNextNo + "','R','" + sInwardItemID + "',@fldUIC,@fldRejectCode,@fldRemarks,'" + sVerifySeq + "',@UserID,getdate())";
            dbContext.ExecuteNonQuery(stmt2, new[] {
                new SqlParameter("@UserID", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", col.fldUIC),
                new SqlParameter("@fldRejectCode", col.fldrejectCode),
                new SqlParameter("@fldRemarks", col.fldRemarks)
            });

        }
        public bool CheckExist(string UIC, string clearDate)
        {
            string stmt = "select * from tblInwardItemInfoH where fldUIC =@fldUIC and fldcleardate=@fldcleardate";
            return dbContext.CheckExist(stmt, new[] {
                  new SqlParameter("@fldUIC", UIC),
                new SqlParameter("@fldClearDate", clearDate)
            });
        }
        public void deleteUPIH(string UIC, string clearDate, string lateid)
        {
            string stmt = "delete from tblinwarditeminfostatus where fldinwarditemid in (select fldInwardItemID from tblInwardItemInfo where flduic =@fldUIC and fldcleardate =@fldcleardate) ";
            stmt = stmt + "delete from tblinwarditeminfo where flduic =@fldUIC and fldcleardate =@fldcleardate ";
            stmt = stmt + "delete from tblLateMaintenance where flduic =@fldUIC and fldcleardate =@fldcleardate and fldLateMaintenanceId =@lateid";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUIC", UIC),
                new SqlParameter("@fldcleardate",clearDate),
                new SqlParameter("@lateid",lateid)
            });
        }
        public void updateInwarditemH(string UIC, string clearDate)
        {
            string stmt = "Update tblInwardItemInfoStatusH set fldUPIDate = NULL, fldRejectCode = '000', fldRemarks ='', fldApprovalIndicator ='Y', fldApprovalStatus = 'A', fldApprovalTimeStamp = getdate(),  fldApprovalUserId = @fldApprovalUserId,fldUpdateUserID=@fldApprovalUserId, fldUpdateTimeStamp = getdate() where fldinwarditemid in (select fldInwardItemID from tblInwardItemInfoH where flduic =@fldUIC and fldcleardate =@fldcleardate)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldApprovalUserId", CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", UIC),
                new SqlParameter("@fldcleardate",clearDate)
                
            });
        }
        public void deleteUPI(string UIC, string clearDate, string lateid)
        {
            string stmt = "delete from tblLateMaintenance where flduic =@fldUIC and fldcleardate =@fldcleardate and fldLateMaintenanceId =@lateid";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldUIC", UIC),
                new SqlParameter("@fldcleardate",clearDate),
                new SqlParameter("@lateid",lateid)
            });
        }
        public void updateInwarditem(string UIC, string clearDate)
        {
            string stmt = "Update tblInwardItemInfoStatus set fldUPIDate = NULL, fldRejectCode = '000', fldRemarks ='',fldApprovalStatus = 'A', fldApprovalTimeStamp = getdate(),  fldApprovalUserId = @fldApprovalUserId,fldUpdateUserID=@fldApprovalUserId, fldUpdateTimeStamp = getdate()  where fldinwarditemid in (select fldInwardItemID from tblInwardItemInfo where flduic =@fldUIC and fldcleardate =@fldcleardate)";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldApprovalUserId",CurrentUser.Account.UserId),
                new SqlParameter("@fldUIC", UIC),
                new SqlParameter("@fldcleardate",clearDate)
            });
        }
        public List<LateReturnMaintenanceModal> getLateReturnMaintenance(FormCollection collection)
        {
            List<LateReturnMaintenanceModal> results = new List<LateReturnMaintenanceModal>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["fldUIC"].ToString().Trim()));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", collection["fldClearDate"].ToString().Trim()));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", collection["fldBankcode"].ToString().Trim()));
            DataTable dt = dbContext.GetRecordsAsDataTableSP("sp_GetLateReturnMaintenance", sqlParameterNext.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                LateReturnMaintenanceModal result = new LateReturnMaintenanceModal();
                result.fldClearDate = Convert.ToDateTime(row["fldClearDate"]).ToString("dd-MM-yyyy");
                result.fldUIC = collection["fldUIC"].ToString();
                result.fldAccountNo = row["fldaccountNumber"].ToString();
                result.fldChequeNo = row["fldChequeSerialNo"].ToString();
                result.fldAmount = Convert.ToDouble(row["fldAmount"].ToString()).ToString("N02");
                result.fldtranscode = row["fldTranscode"].ToString();
                result.fldApprovalUserId = row["fldUserAbb"].ToString();
                result.fldApprovalStatus = row["fldApprovalStatus"].ToString();
                result.fldApprovalTimestamp = row["fldApprovalTimestamp"].ToString();
                if (Convert.ToInt64(row["fldupigenerated"]) == 0)
                {
                    result.upiGenerated = "No";
                }
                else
                { 
                    result.upiGenerated = "Yes";
                }
                results.Add(result);
            }
            return results;
        }
    }
    public static class clsLateReturnMaintenance
    {
        public const string CREATE = "102672";
        public const string DELETE = "102674";
        public const string EDIT = "102673";
        public const string INDEX = "102675";
        public const string SAVECREATE = "102672";
        public const string UPDATE = "102673";
    }

}