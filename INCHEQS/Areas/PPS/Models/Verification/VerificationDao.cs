using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Areas.ICS.Models.VerificationLimit;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Common;
using System.Web.Mvc;
using INCHEQS.Security.Account;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Resources;
using INCHEQS.Models.Sequence;
using System.Linq;
using System.Threading.Tasks;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.ICS.ViewModels;

namespace INCHEQS.Areas.PPS.Models.Verification
{
    public class VerificationDao : IVerificationDao
    {
        private readonly IVerificationLimitDao verificationLimitDao;
        private readonly ICommonInwardItemDao commonInwardItemDao;
        //private readonly IThresholdSettingDao thresholdSettingDao;
        private readonly IBranchActivationDao branchActivationDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IReturnCodeDao returnCodeDao;


        private readonly ApplicationDbContext dbContext;
        public VerificationDao(ICommonInwardItemDao commonInwardItemDao, ApplicationDbContext dbContext, ISequenceDao sequenceDao, IVerificationLimitDao verificationLimitDao, IBranchActivationDao branchActivationDao, IReturnCodeDao returnCodeDao)
        {
            this.commonInwardItemDao = commonInwardItemDao;
            this.verificationLimitDao = verificationLimitDao;
            this.branchActivationDao = branchActivationDao;
            this.sequenceDao = sequenceDao;
            this.returnCodeDao = returnCodeDao;

            this.dbContext = dbContext;
        }

        //public VerificationDao(ISequenceDao sequenceDao, IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, IThresholdSettingDao thresholdSettingDao, IVerificationLimitDao verificationLimitDao, ApplicationDbContext dbContext, IBranchActivationDao branchActivationDao)
        //{
        //    this.pageConfigDao = pageConfigDao;
        //    this.commonInwardItemDao = commonInwardItemDao;
        //    this.sequenceDao = sequenceDao;
        //    this.thresholdSettingDao = thresholdSettingDao;
        //    this.verificationLimitDao = verificationLimitDao;
        //    this.dbContext = dbContext;
        //    this.branchActivationDao = branchActivationDao;
        //}

        public Dictionary<string, dynamic> ExtendInfoStatusNonConfirmField(Dictionary<string, dynamic> field, AccountModel currentUser)
        {
            field.Add("fldNonConfirmUserID", currentUser.UserId);
            field.Add("fldNonConfirmUserClass", currentUser.VerificationClass);
            field.Add("fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            //field.Add("fldApprovalStatus", DBNull.Value);
            field.Add("fldApprovalUserId", currentUser.UserId);
            field.Add("fldApprovalUserClass", DBNull.Value);
            field.Add("fldApprovalTimeStamp", DBNull.Value);
            return field;
        }

        public Dictionary<string, dynamic> ExtendInfoStatusApprovalField(Dictionary<string, dynamic> field, AccountModel currentUser)
        {
            field.Add("fldApprovalUserId", currentUser.UserId);
            field.Add("fldApprovalUserClass", currentUser.VerificationClass);
            field.Add("fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldNonConfirmStatus", DBNull.Value);
            field.Add("fldNonConfirmUserID", DBNull.Value);
            field.Add("fldNonConfirmUserClass", DBNull.Value);
            field.Add("fldNonConfirmTimeStamp", DBNull.Value);
            field.Add("fldHostDebit", 5);//TODO: Unknown process
            return field;
        }

        public Dictionary<string, dynamic> ExtendInfoStatusCommonField(Dictionary<string, dynamic> field, FormCollection collection)
        {
            field.Add("fldApprovalIndicator", "Y"); //TODO: unknown process
            field.Add("fldCharges", collection["fldCharges"].Replace(",", ""));
            field.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldRemarks", collection["textAreaRemarks"]);
            return field;
        }
        public Dictionary<string, dynamic> ExtendPendingInfoCommonField(Dictionary<string, dynamic> field, FormCollection collection, AccountModel currentUser)
        {
            field.Add("fldApprovalUserId", currentUser.UserId);
            field.Add("fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldUpdateUserID", currentUser.UserId);
            field.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldCharges", collection["fldCharges"].Replace(",", ""));
            field.Add("fldRemarks", collection["textAreaRemarks"]);
            return field;
        }

        public void VerificationReturnAll(AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            string sqlCondition = "fldInwardItemId IN (SELECT fldInwardItemId FROM View_ReturnAll)";

            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
            sqlUpdateInfoStatus.Add("fldApprovalIndicator", "Y"); //TODO: unknown process
            sqlUpdateInfoStatus.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlUpdateInfoStatus.Add("fldRemarks", "Return All");

            ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            dbContext.ConstructAndExecuteUpdateCommandWithStringCondition("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }
        

        public VerificationModel getChequeInfo(string inwardItemId,string uic)
        {

            VerificationModel verificationModel = new VerificationModel();
            DataTable ds = new DataTable();
            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            string stmt = "SELECT a.fldInwardItemId,ma.fldPercentMatch, a.fldModifiedFields, a.fldMatchResult, a.fldAmount,a.fldPreBankType, " +
            "a.fldIssueBankCode, a.fldIssueStateCode, a.fldIssueBranchCode, a.fldClearDate,  " +
            "a.fldPreBankCode, a.fldPreStateCode, a.fldPreBranchCode, a.fldUIC,a.fldNonConformance,a.fldNonConformance2, rm.fldRejectDesc, " +
            "nc.flddesc as fldNonDesc,nc1.flddesc as fldNonDesc1, i.flddesc as fldIQADesc, d.fldDocDesc ,a.fldHostDebit, " +
            "a.fldChequeSerialNo, a.fldAccountNumber, a.fldTransCode,a.fldCheckDigit,		 " +
            "payBrm.fldBranchDesc as PayBranchDesc, convert(varchar(50), a.fldUpdateTimeStamp, 120) as fldUpdateTimeStamp, a.fldRejectCode, isnull(a.fldRemarks, '') as fldRemarks, " +
            "a.fldHostAccountNo, a.fldApprovalStatus,a.fldSpickCode,a.fldCharges,  " +
            "a.fldRejectStatus1, a.fldRejectStatus2, a.fldRejectStatus3, a.fldRejectStatus4, a.fldImageFolder, a.fldImageFilename,fldocrPayee " +
            "FROM View_InwardItem a LEFT OUTER JOIN tblBranchMaster payBrm ON a.fldissuebanktype = payBrm.fldBankType and a.fldIssueBankCode = payBrm.fldBankCode AND a.fldIssueStateCode = payBrm.fldStateCode AND a.fldIssueBranchCode = payBrm.fldBranchCode LEFT JOIN " +
            "tblRejectMaster rm on a.fldRejectCode = rm.fldRejectCode LEFT OUTER JOIN " +
            "tblMatchInfo ma on a.fldInwardItemId = ma.fldInwardItemId LEFT OUTER JOIN " +
            "tblNonConformance nc WITH(NOLOCK) on a.fldNonConformance = nc.fldNonConformation LEFT OUTER JOIN " +
            "tblNonConformance nc1 WITH(NOLOCK) on a.fldNonConformance2 = nc1.fldNonConformation LEFT OUTER JOIN " +
            "tblIQAFlag i on a.fldIQA = i.fldIQAFlag LEFT OUTER JOIN " +
            "tblDocToFollow d on a.fldDocToFollow = d.fldDocFlag " +
            "WHERE a.fldInwardItemId = @fldInwardItemId ";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldInwardItemId", inwardItemId) });

            string stmt1 = "select * from tblOCRResult " +
                            "where fldUIC = @fldUIC";
            dt = dbContext.GetRecordsAsDataTable(stmt1, new[] { new SqlParameter("@fldUIC", uic) });



            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    if (row["fldFieldName"].ToString() == "MICR")
                    {
                        verificationModel.fldMicr = row["fldValue"].ToString();
                    }
                    else if (row["fldFieldName"].ToString() == "PAYEE")
                    {
                        verificationModel.fldPayee = row["fldValue"].ToString();
                    }
                    else if (row["fldFieldName"].ToString() == "PAYEE1")
                    {
                        verificationModel.fldPayee1 = row["fldValue"].ToString();
                    }
                    else if (row["fldFieldName"].ToString() == "PAYEE2")
                    {
                        verificationModel.fldPayee2 = row["fldValue"].ToString();
                    }
                    else if (row["fldFieldName"].ToString() == "VALUEDATE")
                    {
                        verificationModel.fldIssueDate = row["fldValue"].ToString();
                    }
                }
            }

            if (verificationModel.fldIssueDate == "" || verificationModel.fldIssueDate is null)
            {
                verificationModel.fldIssueDate = "NIL";
            }

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                verificationModel.fldInwardItemId = row["fldInwardItemId"].ToString();
                verificationModel.fldAccountNo = row["fldAccountNumber"].ToString();
                verificationModel.fldChequeNo = row["fldChequeSerialNo"].ToString();
                verificationModel.fldPercentMatch = row["fldPercentMatch"].ToString();
                verificationModel.fldCheckDigit = row["fldCheckDigit"].ToString();

                verificationModel.fldPayeeName = row["fldocrPayee"].ToString();
                verificationModel.fldAmount = row["fldAmount"].ToString();
                //verificationModel.fldIssueDate = row["fldIssueDate"].ToString();
                verificationModel.fldModifiedFields = row["fldModifiedFields"].ToString();
                verificationModel.fldPreBankType = row["fldPreBankType"].ToString();
                verificationModel.fldMatchResult = row["fldMatchResult"].ToString();
                verificationModel.fldIssueBankCode = row["fldIssueBankCode"].ToString();
                verificationModel.fldIssueStateCode = row["fldIssueStateCode"].ToString();
                verificationModel.fldIssueBranchCode = row["fldIssueBranchCode"].ToString();
                verificationModel.fldClearDate = Convert.ToDateTime(row["fldClearDate"]).ToString("dd-MM-yyyy");
                verificationModel.fldPreBankCode = row["fldPreBankCode"].ToString();
                verificationModel.fldPreStateCode = row["fldPreStateCode"].ToString();
                verificationModel.fldPreBranchCode = row["fldPreBranchCode"].ToString();
                verificationModel.fldUIC = row["fldUIC"].ToString();
                verificationModel.fldNonConformance = row["fldNonConformance"].ToString();
                verificationModel.fldNonConformance2 = row["fldNonConformance2"].ToString();
                verificationModel.fldRejectDesc = row["fldRejectDesc"].ToString();
                verificationModel.fldNonDesc = row["fldNonDesc"].ToString();
                verificationModel.fldNonDesc1 = row["fldNonDesc1"].ToString();
                verificationModel.fldIQADesc = row["fldIQADesc"].ToString();
                verificationModel.fldDocDesc = row["fldDocDesc"].ToString();
                verificationModel.fldHostDebit = row["fldHostDebit"].ToString();
                verificationModel.fldTransCode = row["fldTransCode"].ToString();
                verificationModel.PayBranchDesc = row["PayBranchDesc"].ToString();
                verificationModel.fldRejectCode = row["fldRejectCode"].ToString();
                verificationModel.fldRemarks = row["fldRemarks"].ToString();
                verificationModel.fldHostAccountNo = row["fldHostAccountNo"].ToString();
                verificationModel.fldApprovalStatus = row["fldApprovalStatus"].ToString();
                verificationModel.fldSpickCode = row["fldSpickCode"].ToString();
                verificationModel.fldCharges = row["fldCharges"].ToString();
                verificationModel.fldRejectStatus1 = row["fldRejectStatus1"].ToString();
                verificationModel.fldRejectStatus2 = row["fldRejectStatus2"].ToString();
                verificationModel.fldRejectStatus3 = row["fldRejectStatus3"].ToString();
                verificationModel.fldRejectStatus4 = row["fldRejectStatus4"].ToString();
                verificationModel.fldImageFolder = row["fldImageFolder"].ToString();
                verificationModel.fldImageFilename = row["fldImageFilename"].ToString();

            }

            string HostReject = "";
            string stmt2 = "select * from tblBankHostStatusMaster where fldStatusID in ('" + verificationModel.fldRejectStatus1 + "','" + verificationModel.fldRejectStatus2 + "','" + verificationModel.fldRejectStatus3 + "','" + verificationModel.fldRejectStatus4 + "')";
            dt1 = dbContext.GetRecordsAsDataTable(stmt2);

            if (dt1.Rows.Count > 0)
            {
                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    DataRow row = dt1.Rows[i];

                    HostReject = HostReject + row["fldStatusDesc"].ToString();

                    HostReject = HostReject + "(";

                    HostReject = HostReject + row["fldStatusID"].ToString();

                    HostReject = HostReject + ") ";

                    if (i < dt1.Rows.Count - 1)
                    {
                        HostReject = HostReject + ", ";
                    }



                }


            }
            if (HostReject == null || HostReject == "")
            {
                verificationModel.HostReject = "NIL";
            }
            else
            {
                verificationModel.HostReject = HostReject;
            }

            return verificationModel;
        }

        public async Task<List<VerificationModel>> FindPayeeAsync(FormCollection col, string bankCode)
        {
            return await Task.Run(() => FindPayee(col, bankCode));
        }

        public List<VerificationModel> FindPayee(FormCollection col, string bankCode)
        {
            DataTable ds = new DataTable();
            List<VerificationModel> payee = new List<VerificationModel>();
            string sCondition;
            string sConditionCust;
            string sSQL;
            string sSQLCust;
            string tblPayeeCheque;
            string tblchequeinfo;
            string sDormantStatus;
            string sDormantStatus2;
            bankCode = col["current_fldIssueBankCode"].ToString();

            if (bankCode == "32")
            {
                tblPayeeCheque = "tblPayeeCheque";
                tblchequeinfo = "View_InwardItem";
            }
            else
            {
                tblPayeeCheque = "tblPayeeChequeI";
                tblchequeinfo = "View_InwardItem";
            }


            sCondition = "where fldAccNo like '%" + col["current_fldAccountNumber"].ToString().Trim() + "%' and fldChequeNo like '%" + col["current_fldChequeSerialNo"].ToString().Trim() + "%' ";

            sConditionCust = "where pc.fldAccountNo like '%" + col["current_fldAccountNumber"].ToString().Trim() + "%' and pc.fldChequeNo like '%" + col["current_fldChequeSerialNo"].ToString().Trim() + "%'  ";

            if (col["IssueMonth"].ToString() != "")
            {
                sCondition = sCondition + " and month(fldIssueDate) = " + col["IssueMonth"].ToString() + " ";

                sConditionCust = sConditionCust + " and month(pc.fldIssuedDate) = " + col["IssueMonth"].ToString() + " ";
            }

            if (col["txtYear"].ToString().Trim() != "")
            {
                sCondition = sCondition + " and datediff(year, fldIssueDate, '" + col["txtYear"].ToString() + "') = 0 ";

                sConditionCust = sConditionCust + " and datediff(year, pc.fldIssuedDate, '" + col["txtYear"].ToString().Trim() + "') = 0 ";
            }

            sSQL = "select 'V'[ValidFlag],'-'[ValidDate],fldAccNo,fldPayeeName, Convert(varchar(50),fldIssueDate,101)[fldIssueDate], fldAmount, fldChequeNo, " +
            "fldStatus, fldTransCode, fldSpickCode, Convert(varchar(50),fldUpdateTimestamp,120)[fldUpdateTimestamp], isNull(fldStatusDesc, '') as fldStatusDesc,'1/1/1900'[fldStatusDesc2],'1/1/1900'[cleardate]  " +
            "from " + tblPayeeCheque + " left outer join tblDormantStatusMaster " +
            "on fldDormantStatus = fldStatusId ";


            sSQLCust = "select pc.fldValidation [ValidFlag],'-'[ValidDate],pc.fldAccountNo as [fldAccNo],isnull(pc.fldPayeeName,'')[fldPayeeName], Convert(varchar(50),pc.fldIssuedDate,101)[fldIssueDate], pc.fldAmount, pc.fldChequeNo, " +
                "pc.fldChequeStatus as [fldStatus], ci.fldTransCode, '' as [fldSpickCode], Convert(varchar(50),pc.fldUpdateTimestamp,120)[fldUpdateTimestamp], " +
                "ISNULL(CASE WHEN ci.fldPPSHostStatusDesc2 is not null THEN CONCAT(ISNULL(ci.fldPPSHostStatusDesc + ', ',''), ci.fldPPSHostStatusDesc2) else ci.fldPPSHostStatusDesc  END, 'NIL') as fldStatusDesc,fldStatusDesc2 = dbo.getChequeExpiryDate(pc.fldAccountNo,pc.fldChequeNo),ci.fldcleardate [cleardate] " +
                "from tblPayeeChequePPSCust pc left join " + tblchequeinfo + " ci on ci.fldAccountNumber = pc.fldaccountno and pc.fldchequeno = ci.fldchequeserialno ";

            string stmt = sSQL + sCondition + "UNION ALL " + sSQLCust + sConditionCust;
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                VerificationModel payees = new VerificationModel();
                payees.fldPayeeName = row["fldPayeeName"].ToString();
                payees.fldAccountNo = row["fldAccNo"].ToString();
                payees.fldIssueDate = Convert.ToDateTime(row["fldIssueDate"]).ToString("dd-MM-yyyy");
                payees.fldAmount = @StringUtils.FormatCurrency(row["fldAmount"].ToString());
                payees.fldChequeNo = row["fldChequeNo"].ToString();
                payees.fldTransCode = row["fldTransCode"].ToString();
                payees.fldStatus = row["fldStatus"].ToString();
                payees.fldHostStatus = row["fldStatusDesc"].ToString();


                if (row["ValidFlag"].ToString() == "V")
                {
                    payees.fldValid = "Valid";
                }
                else
                {
                    payees.fldValid = "Invalid";
                }
                payee.Add(payees);
            }
            return payee;
        }

        public List<string> ValidateVerification(FormCollection col, AccountModel currentUser, string verifyAction, string taskid)
        {

            List<string> err = new List<string>();
            VerificationLimitModel verificationLimit = verificationLimitDao.GetVerifyLimit(currentUser.VerificationClass);
            double itemAmount = Convert.ToDouble(col["current_fldAmount"]);

            BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            string UpdatedOrDelete = commonInwardItemDao.CheckIfRecordUpdatedorDeletedForListing(col["fldInwardItemId"]);

            if (commonInwardItemDao.CheckLockedCheck2(col["fldInwardItemId"], currentUser.UserId))
            {
                err.Add("Cheque is currently being locked by other user");
            }

            if (!"Rev".Equals(verifyAction))
            {
                if (commonInwardItemDao.CheckIfUPIGenerated(col["fldInwardItemId"]))
                {
                    err.Add("UPI already generated for this item. User cannot perform any changes!");
                }
            }


            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"]))
            //{
            //    err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            //}
            if (taskid.ToString().Trim() != "318410")
            {
                if (UpdatedOrDelete.Trim() != "" && UpdatedOrDelete.Trim() != null)
                {
                    if (UpdatedOrDelete.Trim() == "updated")
                    {
                        err.Add("This record has been updated by another user.");
                    }
                    else
                    {
                        err.Add("This record has been deleted by another user.");
                    }
                }
            }

            if (taskid.ToString().Trim() == "318410")
            {
                if (Check2ndVerification(col["fldInwardItemId"], currentUser.UserId))
                {
                    err.Add("Item cannot be verified by same user");
                }
            }

            //Compare user class with amount 
            //if ("".Equals(verificationLimit.fldConcatenate))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}
            //else if ("and".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) && StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}
            //else if ("or".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) || StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}

            //Check ccu activation and cut off time
            if ("0".Equals(chequeActivation.fldKLActivation))
            {
                err.Add(Locale.VerificationNotAllowed);
            }


            //Validation for Approve
            if ("A".Equals(verifyAction))
            {
                //put verification for approve here if needed
            }
            //Validation for Return
            if ("R".Equals(verifyAction))
            {
                if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
                {
                    err.Add(Locale.RejectCodeCannotEmpty);
                }
                if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
                {
                    err.Add("Internal Reject Code is only allowed when click Pending");
                }
                if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
                {
                    err.Add("Invalid Return Reason");
                }
            }
            //Validation for Route
            if ("B".Equals(verifyAction))
            {
                if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
                {
                    err.Add(Locale.RejectCodeCannotEmpty);
                }
                if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
                {
                    if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
                    {
                        //err.Add("Internal Reject Code is only allowed when click Pending");
                    }
                    else
                    {
                        err.Add("Invalid Return Reason");
                    }
                }
            }
            return err;
        }


        public void VerificationApprove(FormCollection collection, AccountModel currentUser, string taskRole)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            //Maker Condition
            if (taskRole.Equals("Maker"))
            {

                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            }
            //Checker 1 Condition
            else if (taskRole.Equals("Checker1"))
            {

                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            }
            //Last Checker COndition
            else if (taskRole.Equals("Checker"))
            {
                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
            }
            //Compulsory update for tblInwardItemInfoStatus
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //Since it approved, all reject code will be OK by desc
            sqlUpdateInfoStatus.Add("fldRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default"));
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void VerificationApproveNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            double amountLimit = getVerificationAmountLimit("A");

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            //Maker Condition
            if (pageConfig.TaskRole.Equals("Maker"))
            {
                if (itemAmount > amountLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                }
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                }


            }
            if (pageConfig.TaskRole.Equals("Maker1"))
            {

                //approve large amount

                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

            }

            //Checker 1 Condition
            else if (pageConfig.TaskRole.Equals("Checker1"))
            {


                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));

            }
            //Last Checker COndition
            else if (pageConfig.TaskRole.Equals("Checker"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
            }
            //Compulsory update for tblInwardItemInfoStatus
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalIndicator", "Y")); //TODO: unknown process
            sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            //sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //Since it approved, all reject code will be OK by desc
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default")));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            //sqlParameterNext.Add(new SqlParameter("fldActionStatusId", nextHistorySecNo);
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            //sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldVerifySeq", ""));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            //Excute the command
            dbContext.GetRecordsAsDataTableSP("sp_updateApprovedInwardItem", sqlParameterNext.ToArray());
            // dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }


        public void VerificationReturn(FormCollection collection, AccountModel currentUser, string taskRole)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            //Maker Condition
            if (taskRole.Equals("Maker"))
            {

                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            }
            //Checker 1 Condition
            if (taskRole.Equals("Checker1"))
            {
                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            }
            //Last Checker COndition
            else if (taskRole.Equals("Checker"))
            {
                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
            }
            //Compulsory update for tblInwardItemInfoStatus
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //update Reject Code
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void VerificationReturnNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            double amountLimit = getVerificationAmountLimit("R");
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string reject = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            reject = collection["new_textRejectCode"];
            cheqNo = collection["current_fldChequeSerialNo"];
            //Maker Condition
            if (pageConfig.TaskRole.Equals("Maker"))
            {
                if (itemAmount > amountLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                }
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                }


            }

            //Checker 1 Condition
            if (pageConfig.TaskRole.Equals("Checker1"))
            {

                //sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));

            }
            //Last Checker COndition
            else if (pageConfig.TaskRole.Equals("Checker"))
            {
                //sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
            }
            //Compulsory update for tblInwardItemInfoStatus
            //ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //update Reject Code
            //sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", reject));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            //Excute the command
            dbContext.GetRecordsAsDataTableSP("sp_updateReturnInwardItem", sqlParameterNext.ToArray());
            //dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void VerificationRoute(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRoute);
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);

            //Route : insert pending info
            Dictionary<string, dynamic> sqlInsertPendingInfo = new Dictionary<string, dynamic>();
            sqlInsertPendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute);
            sqlInsertPendingInfo.Add("fldPendingID", sequenceDao.GetNextSequenceNo("tblPendingInfo"));
            sqlInsertPendingInfo.Add("fldInwardItemId", collection["fldInwardItemId"]);
            sqlInsertPendingInfo.Add("fldCreateUserID", currentUser.UserId);
            sqlInsertPendingInfo.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlInsertPendingInfo.Add("fldRejectCode", collection["new_textRejectCode"]);

            ExtendPendingInfoCommonField(sqlInsertPendingInfo, collection, currentUser);
            //Excute the command
            dbContext.ConstructAndExecuteInsertCommand("tblPendingInfo", sqlInsertPendingInfo);
        }

        public void VerificationRouteNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);



            //if (pageConfig.TaskRole.Equals("Maker"))
            //{
            //        sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            //        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            //        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            //        sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            //        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRoute));
            //        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            //        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            //        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

            //}

            //else if (pageConfig.TaskRole.Equals("Checker1"))
            //{

            //    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            //    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            //    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            //    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            //    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRoute));
            //    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            //    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            //    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

            //}

            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

            sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["new_textRejectCode"]));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("sp_updatePendingInwardItem", sqlParameterNext.ToArray());
            //Excute the command
            //dbContext.ConstructAndExecuteInsertCommand("tblPendingInfo", sqlInsertPendingInfo);
        }

        public void VerificationPullOut(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            //Pull Out Field
            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationPullOut);
            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //Pull Out Reject Code
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        //public void VerificationPullOutNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig)
        //{
        //    Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
        //    Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

        //    double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);


        //    //Pull Out Field
        //    if (pageConfig.TaskRole.Equals("Maker"))
        //    {

        //            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationPullOut);
        //            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
        //            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);

        //    }
        //    else if (pageConfig.TaskRole.Equals("Checker1"))
        //    {

        //            sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationPullOut);
        //            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
        //            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);

        //    }
        //    //Pull Out Reject Code
        //    sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
        //    //Excute the command
        //    dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        //}

        public void InsertPullOutInfo(FormCollection collection, AccountModel currentUser)
        {

            List<string> arrResults = new List<string>();
            string otherPullOutReason = collection["pullOutReason"].ToString();

            if (otherPullOutReason != "")
            {
                arrResults.Add(otherPullOutReason);
            }

            if (collection["pullOutReasonBox"] != null)
            {
                arrResults = collection["pullOutReasonBox"].Trim().Split(',').ToList();
            }

            foreach (var arrResult in arrResults)
            {
                Dictionary<string, dynamic> sqlPullOutInfo = new Dictionary<string, dynamic>();

                int nextPullOutInfoSecNo = sequenceDao.GetNextSequenceNo("tblPullOutInfo");

                sqlPullOutInfo.Add("fldPullOutID", nextPullOutInfoSecNo);
                sqlPullOutInfo.Add("fldInwardItemId", collection["fldInwardItemId"]);
                sqlPullOutInfo.Add("fldReason", arrResult);
                sqlPullOutInfo.Add("fldCreateUserID", currentUser.UserId);
                sqlPullOutInfo.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlPullOutInfo.Add("fldUpdateUserID", currentUser.UserId);
                sqlPullOutInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblPullOutInfo", sqlPullOutInfo);

                //Update sequence no
                //sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblInwardItemInfo"), "tblInwardItemInfo");
                sequenceDao.UpdateSequenceNo(sequenceDao.GetNextSequenceNo("tblPullOutInfo"), "tblPullOutInfo");

                ////Add to audit trail
                //auditTrailDao.Log("Cheque Verification - Account Number: " + collection["current_fldAccountNumber"] + " Cheque Number: " + collection["current_fldChequeSerialNo"] + " UIC: " + collection["current_fldUIN"] + " Approval: " + verifyAction, CurrentUser.Account);

            }
        }

        public List<string> ValidateBranch(FormCollection col, AccountModel currentUser, string verifyAction)
        {
            List<string> err = new List<string>();
            //VerificationLimitModel verificationLimit = verificationLimitDao.GetVerifyLimit(currentUser.VerificationClass);
            double itemAmount = Convert.ToDouble(col["current_fldAmount"]);

            BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            //Comomon Validation
            if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"]))
            {
                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }

            ////Compare user class with amount 
            //if ("".Equals(verificationLimit.fldConcatenate))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}
            //else if ("and".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) && StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}
            //else if ("or".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //{
            //    if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) || StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //    {
            //        err.Add(Locale.UserClassNotAuthorizetoVerify);
            //    }
            //}

            //Check branch activation and cut off time
            if ("1".Equals(chequeActivation.fldBPCKLActivation))
            {
                if ("1".Equals(cutOffTime.fldActivation))
                {
                    if (DateTime.Now >= cutOffTime.fldKLCutOffTime)
                    {
                        err.Add(Locale.CutOffTimeActivatedBranchVerificationnotAllowed);
                    }
                }
            }
            else
            {
                err.Add(Locale.VerificationNotAllowed);
            }

            //Validation for Return Branch
            if ("J".Equals(verifyAction))
            {
                if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
                {
                    err.Add(Locale.RejectCodeCannotEmpty);
                }
            }

            return err;
        }

        public List<string> LockedCheck(FormCollection col, AccountModel currentUser)
        {
            List<string> err = new List<string>();
            if (commonInwardItemDao.CheckLockedCheck(col["fldInwardItemId"], currentUser.UserId))
            {

            }
            else
            {
                err.Add("This check is locked by other user");
            }
            return err;
        }

        public Boolean VerifyClassLimit(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            Boolean ind = false;

            sqlParameterNew.Add(new SqlParameter("@userClass", currentUser.VerificationClass));
            sqlParameterNew.Add(new SqlParameter("@Amount", col["current_fldAmount"]));
            DataTable ds = dbContext.GetRecordsAsDataTableSP("sp_checkVerificationLimit", sqlParameterNew.ToArray());

            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                if (row["result"].ToString() == "True")
                {
                    ind = true;
                }
                else
                {
                    ind = false;
                }
            }

            return ind;
        }

        public void BranchApproveNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean verificationlimit)
        {

            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];

            sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("DefaultDefault")));
            sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNew.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNew.Add(new SqlParameter("fldRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default")));
            sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("sp_updateApprovedPendingInwardItem", sqlParameterNew.ToArray());

        }

        public void BranchReturnNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean verificationlimit)
        {

            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            //Maker Condition
            //if (pageConfig.TaskRole.Equals("Maker") || verificationlimit == false)
            //{
            //    sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
            //    sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.BranchReturnMaker));
            //    sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            //    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            //    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            //    sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.BranchReturnMaker));
            //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //    sqlParameterNew.Add(new SqlParameter("@fldCharges", "0"));
            //    sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));

            //}
            ////Checker 1 Condition
            //else if (pageConfig.TaskRole.Equals("Checker"))
            //{

            sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
            sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));

            //}

            //history param
            sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("sp_updateReturnPendingInwardItem", sqlParameterNew.ToArray());

        }

        public double getVerificationAmountLimit(string status)
        {
            double verificationAmountLimit = 0;

            try
            {
                DataTable dataTable = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@status", status));
                dataTable = this.dbContext.GetRecordsAsDataTableSP("spcgVerificationAmountLimit", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    verificationAmountLimit = Convert.ToDouble(dataTable.Rows[0]["fldSystemSettingValue"].ToString());
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }

            return verificationAmountLimit;
        }

        public bool Check2ndVerification(string inwardItemId, string currentUser)
        {
            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemId", inwardItemId));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser));
            DataTable ds = dbContext.GetRecordsAsDataTableSP("spcgPPSVerification2nd", sqlParameterNew.ToArray());

            //ds = dbContext.GetRecordsAsDataTable(stmt);
            if (ds.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<VerificationModel> MatchingDetails(string inwardItemId,string matchingComponent)
        {
            List<VerificationModel> result = new List<VerificationModel>();
            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemId", inwardItemId));
            sqlParameterNew.Add(new SqlParameter("@MatchingComponent", matchingComponent));
            DataTable ds = dbContext.GetRecordsAsDataTableSP("spcgMatchingDetails", sqlParameterNew.ToArray());

            string[] micrArray = null;
            string scanPayeeName = "";
            string scanChequeDate = "";
            string scanAmount = "";
            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    

                    VerificationModel matchingDetails = new VerificationModel();
                    if (matchingComponent == "Scanning")
                    {
                        
                        if (item["fldFieldName"].ToString() == "MICR")
                        {
                            micrArray = item["fldValue"].ToString().Split('^');
                        }
                        else if (item["fldFieldName"].ToString() == "PAYEE")
                        {
                            scanPayeeName = item["fldValue"].ToString();
                        }
                        else if (item["fldFieldName"].ToString() == "VALUEDATE")
                        {
                            scanChequeDate = item["fldValue"].ToString();
                            scanChequeDate = scanChequeDate.Substring(0, 2) + "/" + scanChequeDate.Substring(2, 2) + "/20" + scanChequeDate.Substring(4, 2);
                        }
                        else if (item["fldFieldName"].ToString() == "COURTESYAMOUNT")
                        {
                            scanAmount = item["fldValue"].ToString();
                        }
                        if (item["fldFieldName"].ToString() == "MICR")
                        {
                            matchingDetails.ScanChequeNo = micrArray[2];
                            matchingDetails.ScanAccountNo = micrArray[5];
                            matchingDetails.ScanBankCode = micrArray[3];
                            matchingDetails.ScanStateCode = micrArray[4].Substring(0, 2);
                            matchingDetails.ScanBranchCode = micrArray[4].Substring(2, 3);
                        }
                        
                        matchingDetails.ScanChequeDate = scanChequeDate;
                        matchingDetails.ScanPayeeName = scanPayeeName;
                        matchingDetails.ScanChequeAmount = scanAmount;
                    }
                    else if (matchingComponent == "PPSFile")
                    {
                        matchingDetails.PPSChequeNo = item["fldChequeNo"].ToString();
                        matchingDetails.PPSAccountNo = item["fldAccountNo"].ToString();
                        matchingDetails.PPSBankCode = item["fldBankCode"].ToString();
                        matchingDetails.PPSStateCode = item["fldBranchCode"].ToString().Substring(0, 2);
                        matchingDetails.PPSBranchCode = item["fldBranchCode"].ToString().Substring(2, 3);
                        matchingDetails.PPSChequeDate = item["fldIssuedDate"].ToString();
                        matchingDetails.PPSPayeeName = item["fldPayeeName"].ToString();
                        matchingDetails.PPSChequeAmount = item["fldAmount"].ToString();
                    }
                    result.Add(matchingDetails);
                }
            }
            
            return result;

        }
    }
}