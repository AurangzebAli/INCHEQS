//using INCHEQS.Models.VerificationLimit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;

namespace INCHEQS.Areas.ICS.Models.BranchSubmission
{
    public class BranchSubmissionDao : IBranchSubmissionDao
    {
        private readonly ApplicationDbContext dbContext;

        public BranchSubmissionDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<BranchSubmissionModel> GetBranchSubmission()
        {
            List<BranchSubmissionModel> branchSubmissionModel = new List<BranchSubmissionModel>();
            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmission", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    BranchSubmissionModel branchSubmission = new BranchSubmissionModel();

                    branchSubmission.fldCInternalBranchCode = item["fldCInternalBranchCode"].ToString();
                    branchSubmission.fldBranchCode = item["fldBranchCode"].ToString();
                    branchSubmission.Status = item["Status"].ToString();
                    branchSubmission.fldIdForEnable = item["fldIdforEnable"].ToString();
                    branchSubmission.fldStatus = item["fldStatus"].ToString();

                    branchSubmissionModel.Add(branchSubmission);
                }
            }

            return branchSubmissionModel;
        }

        public bool CheckBranchSubmissionPerformed(string branchCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", branchCode));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionPerformed", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public string GetCompleteCount()
        {
            string complete = "";
            DataTable ds = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();

            SqlParameterNext.Add(new SqlParameter("@Action", "Completed"));
            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionCount", SqlParameterNext.ToArray());
            
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                complete = row["Count"].ToString();
            }

            return complete;
        }

        public string GetIncompleteCount()
        {
            string incomplete = "";
            DataTable ds = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();

            SqlParameterNext.Add(new SqlParameter("@Action", "Incomplete"));
            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionCount", SqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                incomplete = row["Count"].ToString();
            }
            return incomplete;
        }

        public string GetTotalCount()
        {
            string total = "";
            DataTable ds = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();

            SqlParameterNext.Add(new SqlParameter("@Action", "Total"));
            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionCount", SqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                total = row["Count"].ToString();
            }
            return total;
        }

        public bool ValidateStatus(string fldCBranchId)
        {
            DataTable ds = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();

            SqlParameterNext.Add(new SqlParameter("@fldCBranchId", fldCBranchId));
            ds = dbContext.GetRecordsAsDataTableSP("spcgBranchSubmissionStatus", SqlParameterNext.ToArray());

            DataRow row = ds.Rows[0];

            string fldStatus = row["fldStatus"].ToString();

            if (fldStatus == "Incomplete")
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        public void updateStatus(string branchCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", branchCode));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchSubmissionStatus", sqlParameterNext.ToArray());
        }

        public void UpdateAll()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchSubmissionStatusAll", sqlParameterNext.ToArray());
        }
        public BranchSubmissionModel getBankInfo(string userId)
        {
            BranchSubmissionModel branchSubmissionModel = new BranchSubmissionModel();
            DataTable resultTable = new DataTable();
            string stmt = "select fldBranchCode,fldBranchCode2,fldBranchCode3 from tblUserMaster where fldUserId =" + userId;
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                branchSubmissionModel.gUserID = userId;
                branchSubmissionModel.gBranchCode = row["fldBranchCode"].ToString().Trim();
                branchSubmissionModel.gBranchCode2 = row["fldBranchCode2"].ToString().Trim();
                branchSubmissionModel.gBranchCode3 = row["fldBranchCode3"].ToString().Trim();
            }
            return branchSubmissionModel;
        }
        public string GetClearDate()
        {
            BranchSubmissionModel branchSubmissionModel = new BranchSubmissionModel();
            DataTable ds = new DataTable();
            string stmt = "select top 1 fldcleardate from tblinwardcleardate where fldStatus = 'Y' order by fldcleardate DESC";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                branchSubmissionModel.fldClearDate = Convert.ToDateTime(row["fldClearDate"].ToString()).ToString("dd-MM-yyyy");
            }
            else
            {
                branchSubmissionModel.fldClearDate = DateTime.Now.ToString("dd-MM-yyyy");
            }
            return branchSubmissionModel.fldClearDate;
        }
        public bool CheckBranchConfirm(string gBranchCode, string gBranchCode2, string gBranchCode3, string sRegion, string sSubmissionType)
        {
            string sSQL;
            string sField = "fldType";
            string sAddSQL = " and fldRegionKL = 1 ";
            string sSubType;
            sSQL = "select " + sField + " as type from tblBranchConfirm where (fldBranchCode in ('" + gBranchCode + "','" + gBranchCode3 + "') OR  fldBranchCode2 in ('" + gBranchCode + "','" + gBranchCode3 + "')) and fldtype = 'B;L;P;H;R;' " + sAddSQL;
                //"DateDiff(d, fldCreateTimeStamp, getdate()) = 0 and 
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(sSQL);
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                sSubType = row[0].ToString();
                if (sSubType.Contains(sSubmissionType)) //sSubmissionType is in col with type - sSubType
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CheckOfficer()
        {
            string MySql =
                "select fldverifychequeflag from tblusermaster where fldUserAbb = '" + CurrentUser.Account.UserAbbr + "'";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                if (row["fldverifychequeflag"].ToString().Trim() == "1")
                {
                    return true;//officer
                }
                else
                {
                    return false;//not officer
                }
            }
            else
            {
                return false;// not found
            }
        }
        public BranchSubmissionModel getCollapseBranch(string gBranchCode, string gBranchCode2, string gBranchCode3)
        {
            BranchSubmissionModel branchSubmissionModel = new BranchSubmissionModel();
            DataTable resultTable = new DataTable();
            string gBranch;
            string gBranch2;
            string mySQL = "";
            string cBranch = "";
            string cBranch2 = "";
            string myBranch;
            gBranch = gBranchCode + "," + gBranchCode2 + "," + gBranchCode3;
            gBranch2 = "'" + gBranchCode + "','" + gBranchCode2 + "','" + gBranchCode3 + "'";
            mySQL = "select right(fldCCollapseBranchId,5) as fldCCollapseBranchId,right(fldICollapseBranchId,5) as fldICollapseBranchId from tblRationalBranch where right(fldCMergeBranchId,5) = '" + gBranchCode + "' and right(fldIMergeBranchId,5) = '" + gBranchCode3 + "'";
            DataTable dt = dbContext.GetRecordsAsDataTable(mySQL);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    if (cBranch == "" & cBranch2 == "")
                    {
                        cBranch = gBranch + "," + row["fldCCollapseBranchId"].ToString().Trim() + "," + row["fldICollapseBranchId"].ToString().Trim() + "'";
                        cBranch2 = gBranch2 + ",'" + row["fldCCollapseBranchId"].ToString().Trim() + "','" + row["fldICollapseBranchId"].ToString().Trim() + "'";
                    }
                    else
                    {
                        cBranch = cBranch + "," + row["fldCCollapseBranchId"].ToString().Trim() + "," + row["fldICollapseBranchId"].ToString().Trim();
                        cBranch2 = cBranch2 + ",'" + row["fldCCollapseBranchId"].ToString().Trim() + "','" + row["fldICollapseBranchId"].ToString().Trim() + "'";
                    }
                }
            }
            else
            {
                cBranch = gBranch;
                cBranch2 = gBranch2;
            }
            branchSubmissionModel.cBranch = cBranch;
            branchSubmissionModel.cBranch2 = cBranch2;
            return branchSubmissionModel;
        }
        public bool CheckPreRegionRight(string sClearDate)
        {
            string sSQL;
            sSQL = "select top(1)* from tblCutOffTime where DateDiff(d, fldCreateTimeStamp,getdate()) = 0 And fldActivation = '1'";
            DataTable dt = dbContext.GetRecordsAsDataTable(sSQL);
            if (dt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckCutOffPeriod()
        {
            string sSpick = "fldKLcutofftime";
            string sSpickTimeFrom = "fldKLTimeFrom";
            string sStatus;
            string MySql =
                "select convert(varchar(10), GetDate(), 108)[TimeNow],convert(varchar(10)," + sSpick + ",108)[RegionCutOffTime] , " +
                "convert(varchar(10)," + sSpickTimeFrom + ",108)[RegionTimeFrom] ," + "Status = case when convert(varchar(10), GetDate(), 108) > convert(varchar(10),"
                + sSpickTimeFrom + ",108) " + "and convert(varchar(10), GetDate(), 108) < convert(varchar(10)," + sSpick + ",108) then 'IN' else 'OUT' end " +
                "From tblcutofftime where fldActivation = 1 ";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                sStatus = row["status"].ToString();
                if (sStatus == "IN")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CheckPendingData(string myBranch, string AddSQL)
        {
            string MySql = " Select pi.fldInwardItemId from tblPendingInfo pi WITH (NOLOCK)"
                            + " Inner Join view_AppPendingData ci WITH (NOLOCK) on ci.fldInwardItemId = pi.fldInwardItemId"
                            + myBranch + AddSQL.Replace("pi.", "ci.")
                            + " And pi.fldApprovalStatus ='B' "
                            + " And (ci.fldaccountnumber NOT IN (select fldaccountno from tblaccountinfo)  ) "
                            + "and (ci.fldaccountnumber NOT IN (select fldaccountno from tblCMSaccountinfo)  ) ";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool CheckPospayData(string myBranch, string AddSQL)
        {
            string MySql = " Select pi.fldInwardItemId from tblPendingInfo pi WITH (NOLOCK)"
                            + " Inner Join view_AppPendingData ci WITH (NOLOCK) on ci.fldInwardItemId = pi.fldInwardItemId"
                            + myBranch + AddSQL.Replace("pi.", "ci.")
                            + " And pi.fldApprovalStatus ='B'" + " and ci.fldAccountNumber in (select fldaccountNo from tblaccountInfo)";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool StartCopy(string myBranch, string AddSQL, string gBranchCode, string gBranchCode2)
        {
            bool sContinue = false;
            String MySql = " Select ci.fldIssuebankCode as fldBankCode,pi.fldRemarks,pi.fldRejectCode,pi.fldCharges,pi.fldApprovalStatus as fldActionStatus,ci.fldUIC, "
                            + " ci.fldprebankcode,ci.fldprestatecode,ci.fldprebranchcode,ci.fldtranscode,ci.fldAccountNumber,ci.fldChequeserialNo, "
                            + " pi.fldApprovalUserId, pi.fldApprovalTimeStamp as fldApprovalTimeStamp"
                            + " from tblpendinginfo pi WITH (NOLOCK) "
                            + " inner join view_AppPendingData ci (NOLOCK) on ci.fldInwarditemId = pi.fldInwarditemId"
                            + myBranch + AddSQL.Replace("pi.", "ci.")
                            + " And pi.fldApprovalStatus in ('R', 'A')"
                            + " and not (pi.fldApprovalStatus = 'A' "
                            + " and (ci.fldInwarditemId in (Select fldInwardItemId from tblRejectedBPCitem)"
                            + " or ci.fldInwarditemId in (Select fldInwardItemId from tblRejectedLargeAmtInfo)))"
                            + " and ci.fldIssueBankBranch in ('" + gBranchCode+ "','"+ gBranchCode2+"')";
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(MySql);
            if (ds.Rows.Count > 0)
            {
                foreach (DataRow row in ds.Rows)
                {
                    List<SqlParameter> sqlParametersBranchS1 = new List<SqlParameter>();
                    sqlParametersBranchS1.Add(new SqlParameter("@aBankCode", row["fldBankCode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aUIC", row["fldUIC"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aRemarks", row["fldRemarks"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aCharges", row["fldCharges"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aRejectCode", row["fldRejectCode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aActionStatus", row["fldActionStatus"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aPreBankCode", row["fldPrebankcode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aPreStateCode", row["fldPreStatecode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aPreBranchCode", row["fldPreBranchcode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aAccountNumber", row["fldAccountNumber"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aChequeNumber", row["fldchequeserialno"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aTransCode", row["fldTranscode"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aApprovalUserId", row["fldApprovalUserId"].ToString().Trim()));
                    sqlParametersBranchS1.Add(new SqlParameter("@aApprovalTimeStamp", row["fldApprovalTimeStamp"].ToString().Trim()));
                    dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchSubmission", sqlParametersBranchS1.ToArray());
                }
                sContinue = true;
            }
            else
            {
                sContinue = true;
            }
            if (sContinue)
            {
                MySql = "Select ci.fldIssueBankCode,ri.fldRemarks,ri.fldRejectCode,ri.fldCharges,ri.fldRejectedFlag,ci.fldUIC,"
                        + " ci.fldprebankcode,ci.fldprestatecode,ci.fldprebranchcode,ci.fldaccountnumber,ci.fldchequeserialno,ci.fldtranscode,"
                        + " ri.fldRejectUserId, Convert(char(50),ri.fldRejectTimeStamp,121)[fldRejectedTimeStamp]"
                        + " from tblRejectedLargeAmtInfo ri WITH (NOLOCK) "
                        + " inner join view_AppPendingData ci WITH (NOLOCK) on ci.fldInwardItemId = ri.fldInwardItemId"
                        + myBranch + AddSQL.Replace("pi.", "ci.")
                        + " And ri.fldRejectedFlag = 1 "
                        + " and not (ci.fldInwardItemId in (Select fldInwardItemId from tblPendingInfo where fldApprovalStatus = 'R'))";
                DataTable ds2 = new DataTable();
                ds2 = dbContext.GetRecordsAsDataTable(MySql);
                if (ds2.Rows.Count > 0)
                {
                    foreach (DataRow row in ds2.Rows)
                    {
                        List<SqlParameter> sqlParametersBranchS2 = new List<SqlParameter>();
                        sqlParametersBranchS2.Add(new SqlParameter("@aBankCode", row["fldIssueBankCode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aUIC", row["fldUIC"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aRemarks", row["fldRemarks"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aCharges", row["fldCharges"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aRejectCode", row["fldRejectCode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aActionStatus", row["fldApprovalstatus"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aPreBankCode", row["fldPrebankcode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aPreStateCode", row["fldPreStatecode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aPreBranchCode", row["fldPreBranchcode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aAccountNumber", row["fldAccountNumber"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aChequeNumber", row["fldchequeserialno"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aTransCode", row["fldTranscode"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aApprovalUserId", row["fldRejectUserId"].ToString().Trim()));
                        sqlParametersBranchS2.Add(new SqlParameter("@aApprovalTimeStamp", row["fldRejectedTimeStamp"].ToString().Trim()));
                        dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuBranchSubmission", sqlParametersBranchS2.ToArray());
                    }
                }
            }
            if (sContinue)
            {
                if (CheckExistBranch(gBranchCode, gBranchCode2))
                { //EXISTS = UPDATE
                    string sSQL;
                    string sSubmissionType = "B;L;P;H;R";
                    string UpdParam = " fldRegionKL =1,fldRegionJB =1,fldRegionPP =1";
                    sSQL = " Update tblBranchConfirm set " + UpdParam
                        + " ,fldType='" + sSubmissionType + ";'"
                        + " ,fldUpdateUserId=" + CurrentUser.Account.UserId + " , fldUpdateTimeStamp= getdate()"
                        + " Where fldBranchCode ='" + gBranchCode + "' and fldBranchCode2='" + gBranchCode2 + "'"; 
                    //and len(fldtype)=8 ";
                    dbContext.ExecuteNonQuery(sSQL);
                }
                else
                { //DOESN'T EXIST = INSERT 
                    string sSQL;
                    string sSubmissionType = "B;L;P;H;R";
                    string AddParam = "fldRegionKL,fldRegionJB,fldRegionPP,fldType,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) ";
                    string AddParamValue = ",1,1,1,'" + sSubmissionType + ";','" + CurrentUser.Account.UserId + "',getdate(),'" + CurrentUser.Account.UserId + "',getdate())";
                    sSQL = "insert into tblBranchConfirm (fldBranchCode,fldBranchCode2,fldBranchCode3," + AddParam +
                            "Values('" + gBranchCode + "','" + gBranchCode2 + "','" + null + "' " + AddParamValue;
                    dbContext.ExecuteNonQuery(sSQL);
                }
            }
            return sContinue;
        }

        public bool CheckExistBranch(string gBranchCode, string gBranchCode2)
        {
            string sSQL;
            string sSubType;
            sSQL = "select fldType as type  from tblBranchConfirm where " +
                //"DateDiff(d, fldCreateTimeStamp, getdate()) = 0"
                //+ " and " +
                //+ 
                " fldBranchCode = '" + gBranchCode + "' and fldBranchCode2 = '" + gBranchCode2
                + "' and (fldtype <> 'B;L;P;H;R;' and  fldtype <> 'S;A;E;'and  fldtype <> 'C;A;B;')  ";
            DataTable dt = dbContext.GetRecordsAsDataTable(sSQL);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                sSubType = row["type"].ToString().Trim();
                if (sSubType.Length == 8 | sSubType.Length == 4 | sSubType.Length == 10 | sSubType.Length == 6)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
