using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Management;
using System.Text.RegularExpressions;
using INCHEQS.Resources;
using INCHEQS.Common;
using System.Data.Entity;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using ICSSecurity;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;

namespace INCHEQS.Areas.OCS.Models.BranchEndOfDay
{
    public class BranchEndOfDayDao : IBranchEndOfDayDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private string fldbranchid;

        public BranchEndOfDayDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, IAuditTrailDao auditTrailDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = auditTrailDao;
        }

        //public DataTable GetHubBranches(string userId)
        //{
        //    DataTable dt = new DataTable();
        //    string strQuery = "SELECT hubbranch.fldBranchId, dbo.tblInternalBranchMaster.fldHubBranch, " +
        //                            "hubbranch.fldBranchId + ' - ' + dbo.tblInternalBranchMaster.fldBranchDesc as BranchInfo " +
        //                        "FROM dbo.tblHubBranch AS hubbranch INNER JOIN " +
        //                            "dbo.tblHubUser AS hubuse ON hubuse.fldHubCode = hubbranch.fldHubCode INNER JOIN " +
        //                            "bo.tblInternalBranchMaster ON hubbranch.fldBranchId = dbo.tblInternalBranchMaster.fldBranchId " +
        //                        "WHERE (hubuse.fldUserId = @fldUserId) " +
        //                            "AND (dbo.tblInternalBranchMaster.fldHubBranch = N'Y') ";
        //    dt = dbContext.GetRecordsAsDataTable(strQuery, new[] { new SqlParameter("@fldUserId", userId) });
        //    return dt;
        //}

        
        public DataTable GetHubBranches(string userId)
        {
            DataTable dt = new DataTable();
            //string strQuery = "SELECT fldBranchCode, fldBranchDesc FROM tblHubUser hu " +
            //                "INNER JOIN tblHubBranch hb ON hb.fldHubCode = hu.fldHubCode " +
            //                "INNER JOIN tblMapBranch mb ON hb.fldbranchid = mb.fldBranchCode " +
            //                "WHERE hu.fldUserId = @fldUserId ";
            //dt = dbContext.GetRecordsAsDataTable(strQuery, new[] { new SqlParameter("@fldUserId", userId) });
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserHubBranches", SqlParameterNext.ToArray());
            return dt;
        }

        public List<string> GetHubBranchList(string userId)
        {
            List<string> hubbranchModels = new List<string>();
            
            DataTable dt = new DataTable();
            
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserHubBranches", SqlParameterNext.ToArray());
       
            if (dt.Rows.Count > 0)
            {
                
                foreach (DataRow row in dt.Rows)
                {

                    fldbranchid = row["fldbranchid"].ToString();
                   
                    hubbranchModels.Add(fldbranchid);
                }
                
            }
            return hubbranchModels;
        }

        public string GetProcessDate(string bankcode)
        {
            string ProcessDate = "";
            try
            {
                DataTable dtHubTemp = new DataTable();
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();

                SqlParameterNext.Add(new SqlParameter("@fldbankcode", bankcode));
                dtHubTemp = dbContext.GetRecordsAsDataTableSP("spcgEndDayBranchProcessDate", SqlParameterNext.ToArray());

                if (dtHubTemp.Rows.Count > 0)
                {
                    DataRow drItem = dtHubTemp.Rows[0];
                    ProcessDate = drItem["fldProcessDate"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return ProcessDate;

        }

        public DataTable GetItemReadyForSubmission(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            SqlParameterNext.Add(new SqlParameter("@Userid", CurrentUser.Account.UserId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchItemReadyForSubmit", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetItemSubmitted(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            SqlParameterNext.Add(new SqlParameter("@Userid", CurrentUser.Account.UserId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchItemSubmitted", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetHubItemReadyForSubmission(string userId, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@UserId", userId));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHubItemReadyForSubmit", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetHubItemSubmitted(string userId, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@UserId", userId));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHubItemSubmitted", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetBranchEndOfDay(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgEODBranchByBranchID", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable GetAllUserBranchEndOfDay(string strUserId, string strBankCode)
        {
            strUserId = CurrentUser.Account.UserId;

            DataTable dt = new DataTable();
            
                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                    SqlParameterNext.Add(new SqlParameter("@Userid", strUserId));
                    dt = dbContext.GetRecordsAsDataTableSP("spcgEODAllBranch", SqlParameterNext.ToArray());
            

            return dt;
        }

        public DataTable DataEntryPendingItem(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgPendingBatchEntry", SqlParameterNext.ToArray());
            return dt;
        }

        public DataTable AuthorizationPendingItem(string strBranchCode, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@CapturingBranchCode", strBranchCode));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgPendingAuthorization", SqlParameterNext.ToArray());
            return dt;
        }

     //   @bankCode nchar(3),
	    //@fldBranchID nchar(9),
	    //@fldAmount bigint,
     //   @fldDifference          bigint,
	    //@fldEODStatus nchar(1),
	    //@fldActive nchar(1),
	    //@fldCreatedBy int,
	    //@fldCreatedDateTime datetime

        public bool InsertBranchEndOfDay(string strBranchId, string strAmount, string strDifference, string strEODStatus, string strBankCode)
        {
            try
            {
                string Branchlst = "";
                if (strBranchId == "All")
                {
                    List<string> hubbranchLists = GetHubBranchList(CurrentUser.Account.UserId);

                    foreach (var branchIdlist in hubbranchLists)
                    {
                        if (Branchlst.Equals(""))
                        {
                            Branchlst = branchIdlist;
                        }
                        else
                        {
                            Branchlst = Branchlst + "," + branchIdlist;
                        }
                        List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                        SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                        SqlParameterNext.Add(new SqlParameter("@fldBranchID", branchIdlist.ToString()));
                        SqlParameterNext.Add(new SqlParameter("@fldAmount", Convert.ToDecimal(strAmount)));
                        SqlParameterNext.Add(new SqlParameter("@fldDifference", Convert.ToDecimal(strDifference)));
                        SqlParameterNext.Add(new SqlParameter("@fldEODStatus", strEODStatus));
                        SqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
                        SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", CurrentUser.Account.UserId));
                        SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
                        dbContext.GetRecordsAsDataTableSP("spciEODBranch", SqlParameterNext.ToArray());
                        
                    }

                }
                else
                {
                    Branchlst = strBranchId.ToString();
                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                    SqlParameterNext.Add(new SqlParameter("@fldBranchID", strBranchId.ToString()));
                    SqlParameterNext.Add(new SqlParameter("@fldAmount", Convert.ToDecimal(strAmount)));
                    SqlParameterNext.Add(new SqlParameter("@fldDifference", Convert.ToDecimal(strDifference)));
                    SqlParameterNext.Add(new SqlParameter("@fldEODStatus", strEODStatus));
                    SqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
                    SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", CurrentUser.Account.UserId));
                    SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
                    dbContext.GetRecordsAsDataTableSP("spciEODBranch", SqlParameterNext.ToArray());

                }
                Branchlst = Branchlst.TrimEnd(',');
                auditTrailDao.SecurityLog("OCS Branch End of Day. Performed Eod for Branche(s) : " + Branchlst + " with Amount : " + Convert.ToDecimal(strAmount), "", TaskIdsOCS.BranchEndOfDay.INDEX, CurrentUser.Account);
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }



        public DataTable ValidateUserLogin(string strusername, string strpassword, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            SqlParameterNext.Add(new SqlParameter("@flduserabb", strusername));
            SqlParameterNext.Add(new SqlParameter("@fldpassword", strpassword));
            dt = dbContext.GetRecordsAsDataTableSP("spcgValidateUser", SqlParameterNext.ToArray());
            return dt;
        }

        public bool DeleteBranchEndOfDay(string strBranchId, string strEODStatus, string strBankCode)
        {
            try
            {

                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                SqlParameterNext.Add(new SqlParameter("@fldBranchID", strBranchId));
                SqlParameterNext.Add(new SqlParameter("@fldEODStatus", strEODStatus));
                SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", CurrentUser.Account.UserId));
                SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
                dbContext.GetRecordsAsDataTableSP("spcdEODBranch", SqlParameterNext.ToArray());
                auditTrailDao.SecurityLog("OCS Branch End of Day Summary. Release Branch EOD for Branch : " + strBranchId ,"", TaskIdsOCS.BranchEndOfDaySummary.INDEX, CurrentUser.Account);
                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public string encryptPassword(string strpassword)
        {
            string str = "";
            try
            {
                EncryptDecrypt encryptDecrypt = new EncryptDecrypt()
                {
                    FilePath = systemProfileDao.GetDLLPath()
                };
                str = encryptDecrypt.EncryptString128Bit(strpassword);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str;
        }

        public bool InsertHubBranchEndOfDay(string strBranchId, string strEODStatus, string strBankCode)
        {
            strEODStatus = "Y";
            string Branchlst = "";
            try
            {
                if (strBranchId == "All")
                {
                    List<string> hubbranchLists = GetHubBranchList(CurrentUser.Account.UserId);

                    foreach (var branchIdlist in hubbranchLists)
                    {
                        if (Branchlst.Equals(""))
                        {
                            Branchlst = branchIdlist;
                        }
                        else
                        {
                            Branchlst = Branchlst + "," + branchIdlist;
                        }
                        List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                        SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                        SqlParameterNext.Add(new SqlParameter("@fldBranchID", branchIdlist.ToString()));
                        SqlParameterNext.Add(new SqlParameter("@fldEODStatus", strEODStatus));
                        SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", CurrentUser.Account.UserId));
                        SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
                        dbContext.GetRecordsAsDataTableSP("spciEODBranch", SqlParameterNext.ToArray());
                    }
                }
                else
                {
                    Branchlst = strBranchId.ToString();
                    List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                    SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
                    SqlParameterNext.Add(new SqlParameter("@fldBranchID", strBranchId.ToString()));
                    SqlParameterNext.Add(new SqlParameter("@fldEODStatus", strEODStatus));
                    SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", CurrentUser.Account.UserId));
                    SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
                    dbContext.GetRecordsAsDataTableSP("spciEODBranch", SqlParameterNext.ToArray());

                }
                Branchlst = Branchlst.TrimEnd(',');
                auditTrailDao.SecurityLog("OCS Branch End of Day. Performed Eod for Branche(s) : " + Branchlst , "", TaskIdsOCS.BranchEndOfDay.INDEX, CurrentUser.Account);

                return true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

    }
}