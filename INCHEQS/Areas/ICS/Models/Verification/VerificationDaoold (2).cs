using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Common;
using INCHEQS.Security.Account;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.Sequence;
using INCHEQS.TaskAssignment;
using INCHEQS.ConfigVerification.ThresholdSetting;
using INCHEQS.Areas.ICS.Models.BranchSubmission;
//using INCHEQS.Models.VerificationLimit;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.ICS.ViewModels;
using static INCHEQS.TaskAssignment.TaskIds;
using System.Web.UI.WebControls;
using System.EnterpriseServices.Internal;
using System.Web.Razor.Editor;
using RestSharp;

namespace INCHEQS.Models.Verification
{
    public class VerificationDao : IVerificationDao
    {
        private readonly IPageConfigDao pageConfigDao;
        private readonly ICommonInwardItemDao commonInwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IThresholdSettingDao thresholdSettingDao;
        private readonly IVerificationLimitDao verificationLimitDao;
        private readonly IBranchActivationDao branchActivationDao;
        private readonly ApplicationDbContext dbContext;
        private readonly IReturnCodeDao returnCodeDao;
        private readonly IBranchSubmissionDao branchSubmissionDao;

        public VerificationDao(ISequenceDao sequenceDao, IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, IThresholdSettingDao thresholdSettingDao, IVerificationLimitDao verificationLimitDao, ApplicationDbContext dbContext, IBranchActivationDao branchActivationDao, IReturnCodeDao returnCodeDao, IBranchSubmissionDao branchSubmissionDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.commonInwardItemDao = commonInwardItemDao;
            this.sequenceDao = sequenceDao;
            this.thresholdSettingDao = thresholdSettingDao;
            this.verificationLimitDao = verificationLimitDao;
            this.dbContext = dbContext;
            this.branchActivationDao = branchActivationDao;
            this.returnCodeDao = returnCodeDao;
            this.branchSubmissionDao = branchSubmissionDao;


        }

        public Dictionary<string, dynamic> ExtendInfoStatusNonConfirmField(Dictionary<string, dynamic> field, AccountModel currentUser)
        {
            field.Add("fldNonConfirmUserID", currentUser.UserId);
            field.Add("fldNonConfirmUserClass", currentUser.VerificationClass);
            field.Add("fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldApprovalStatus", DBNull.Value);
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
            //field.Add("fldCharges", collection["fldCharges"]);//.Replace(",", "")
            //field.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldRemarks", collection["textAreaRemarks"]);
            //field.Add("fldExtRemarks", collection["textAreaExtRemarks"]);
            return field;
        }

        public Dictionary<string, dynamic> ExtendPendingInfoCommonField(Dictionary<string, dynamic> field, FormCollection collection, AccountModel currentUser)
        {
            field.Add("fldApprovalUserId", currentUser.UserId);
            field.Add("fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            field.Add("fldUpdateUserID", currentUser.UserId);
            field.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            if (!String.IsNullOrEmpty(collection["fldCharges"]))
            {
                field.Add("fldCharges", collection["fldCharges"].Replace(",", ""));

            }
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
        public void VerificationApproveAll(AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            string sqlCondition = "fldInwardItemId IN (SELECT fldInwardItemId FROM View_ApproveAll)";

            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
            sqlUpdateInfoStatus.Add("fldApprovalIndicator", "Y"); //TODO: unknown process
            sqlUpdateInfoStatus.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlUpdateInfoStatus.Add("fldRemarks", "Approve All");

            ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

            dbContext.ConstructAndExecuteUpdateCommandWithStringCondition("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }


        public string APIUsername { get; set; }
        public string APIPassword { get; set; }
        public string APIFundsTranferApiUrl { get; set; }
        public string APIFundsTransferAuth { get; set; }
        public string APITokenURL { get; set; }
        public string APITokenAuth { get; set; }
        public string APITokenGrantType { get; set; }
        public string APITokenScope { get; set; }

        public void VerificationApprove(FormCollection collection, AccountModel currentUser, string taskRole)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);

            //Maker Condition
            if (taskRole.Equals("Maker"))
            {
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 1, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount >= approveThresholdLimit)
                {
                    sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove);
                    ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
                }
                //if not, just approved it
                else
                {
                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
                }
            }
            //Checker 1 Condition
            else if (taskRole.Equals("Checker1"))
            {
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 2, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount >= approveThresholdLimit)
                {
                    sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove1stChecker);
                    ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
                }
                //if not, just approved it
                else
                {
                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
                }
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
            sqlUpdateInfoStatus.Add("fldRejectCode", "000");
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }
        private string GetAPIParam()
        {
            try
            {
                DataTable dtTokenURL = getAPIRequestInfo("TokenURLInfo");
                DataTable dtTokenAuth = getAPIRequestInfo("Authorization");
                DataTable dtTokenGrantType = getAPIRequestInfo("grant_type");
                DataTable dtTokenscope = getAPIRequestInfo("ScopeFinancial");
                DataTable dtFundsTransfer = getAPIRequestInfo("FundsTransfer");
                DataTable dtUserName = getAPIRequestInfo("username");
                DataTable dtPassword = getAPIRequestInfo("password");
                if (dtTokenURL.Rows.Count == 0
                    || dtTokenAuth.Rows.Count == 0
                    || dtTokenGrantType.Rows.Count == 0
                    || dtTokenscope.Rows.Count == 0
                    || dtFundsTransfer.Rows.Count == 0
                    || dtUserName.Rows.Count == 0
                    || dtPassword.Rows.Count == 0
                    )
                {
                    return "TokenURL/TokenAuth/TokenGrantType/TokenScope/FundsTransferURL/Username/Password cannot be found.";
                }
                else
                {
                    if (dtTokenURL.Rows.Count>0)
                    {

                        APITokenURL = dtTokenURL.Rows[0][0].ToString();
                    }
                    if (dtTokenAuth.Rows.Count > 0)
                    {

                        APITokenAuth = dtTokenAuth.Rows[0][0].ToString();
                    }
                    if (dtTokenGrantType.Rows.Count>0)
                    {

                        APITokenGrantType = dtTokenGrantType.Rows[0][0].ToString();
                    }
                    if (dtTokenscope.Rows.Count>0)
                    {
                        APITokenScope = dtTokenscope.Rows[0][0].ToString();
                    }
                    if (dtFundsTransfer.Rows.Count > 0)
                    {
                        APIFundsTranferApiUrl = dtFundsTransfer.Rows[0][0].ToString();

                    }
                    if (dtUserName.Rows.Count > 0)
                    {
                        APIUsername= dtUserName.Rows[0][0].ToString();

                    }
                    if (dtPassword.Rows.Count > 0)
                    {
                        APIPassword = dtPassword.Rows[0][0].ToString();

                    }

                }
                return "";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public DataTable getAPIRequestInfo(string Code)
        {
            DataTable dtResult = null/* TODO Change to default(_) if this is not a reference type */;
            try
            {
                bool boolResult = true;
                string strquery;
                strquery = "select fldrequestPath from tblWebApiRequestType where fldRequestName = '" + Code.Trim() + "'";
                dtResult = dbContext.GetRecordsAsDataTable(strquery);// cls_query.fct_sqlGetDataTable(conn, strquery, boolResult);
                if (boolResult == false)
                    dtResult = null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                string _strErrorMessage = "[getAPIRequestInfo]:" + ex.Message;
                dtResult = null/* TODO Change to default(_) if this is not a reference type */;
                throw ex;
            }
            return dtResult;
        }

        public void VerificationApproveNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            bool isRtService = false;
            /*if (pageConfig.TaskId == "308150" || pageConfig.TaskId == "308160")
            {
                if (pageConfig.TaskRole.Equals("Maker"))
                {
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.BranchLargeAmtApproveMaker));
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

             
                }
                //Checker 1 Condition
                else if (pageConfig.TaskRole.Equals("Checker1"))
                {

                        sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.BranchLargeAmtApproveChecker));
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

            }
            else
            {*/

            if (pageConfig.TaskRole.Equals("Checker2"))
            {

                if (collection["current_fldNonConfirmStatus"] == "S")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNext.Add(new SqlParameter("@fldActionStatus", VerificationStatus.ACTION.VerificationReturn));
                }
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldActionStatus", VerificationStatus.ACTION.VerificationApprove));
                }
                
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                //sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", collection["txtRemarks"]));
                //sqlParameterNext.Add(new SqlParameter("@fldRemarks", ""));
                //sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", ""));


            }
            else if (pageConfig.TaskRole.Equals("Checker1"))
            {
                double approveThresholdLimit = 0;
                if (collection["current_fldNonConfirmStatus"] != "R")
                {
                    approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 2, currentUser.BankCode);
                }
                else
                {
                    approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 2, currentUser.BankCode);
                }
                    //If amount more then threshold
                if (itemAmount > approveThresholdLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));

                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    if (collection["current_fldNonConfirmStatus"] == "R")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn1stChecker));
                        sqlParameterNext.Add(new SqlParameter("@fldActionStatus", VerificationStatus.ACTION.VerificationReturn));
                    }
                    else
                    {

                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove1stChecker));
                        sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                    }

                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", collection["txtRemarks"]));

                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));

                }
                //if not, just approved it
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    if (collection["current_fldNonConfirmStatus"] == "R")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn1stChecker));
                        sqlParameterNext.Add(new SqlParameter("@fldActionStatus", VerificationStatus.ACTION.VerificationReturn));
                    }
                    else
                    {

                        sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                        sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                    }

                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", collection["txtRemarks"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks", ""));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));
                }

            }
            else
            {
                //first level

                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 1, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount > approveThresholdLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["txtRemarks"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", ""));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));

                }
                //if not, just approved it
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
                    sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["txtRemarks"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", ""));
                    //sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));
                    isRtService= true;




                }
            }
            
            //Checker 1 Condition
            /*else if (pageConfig.TaskRole.Equals("Checker1"))
            {
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 2, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount >= approveThresholdLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove1stChecker));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                }
                //if not, just approved it
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
            }*/
            //Last Checker COndition
            /*else if (pageConfig.TaskRole.Equals("Checker"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
            }*/
        /*}*/
            //Compulsory update for tblInwardItemInfoStatus
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalIndicator", "Y")); //TODO: unknown process
            //sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            //sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //Since it approved, all reject code will be OK by desc
            //sqlParameterNext.Add(new SqlParameter("fldRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default")));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["current_fldRejectCode"]));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
            //sqlParameterNext.Add(new SqlParameter("fldActionStatusId", nextHistorySecNo);
            
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            //sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            
            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextExtAreaRemarks", collection["textAreaExtRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldVerifySeq", ""));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));

            sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));
            //sqlParameterNext.Add(new SqlParameter("@fldRemarks", ""));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", ""));

            //Excute the command
            dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());
            try
            {
                if (isRtService)
                {
                    GetAPIParam();
                    string vTokenApiEndPoint = APITokenURL;
                        //RestClient TokenAPIcall = new RestClient(vTokenApiEndPoint);
                        ////TokenAPIcall.Timeout = -1;
                        //RestRequest requestTokenCall = new RestRequest(Method.Post);
                        //requestTokenCall.AddHeader("Authorization", APITokenAuth);
                        //requestTokenCall.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                        //requestTokenCall.AddParameter("grant_type", APITokenGrantType);
                        //requestTokenCall.AddParameter("scope", APITokenScope);
                        //Task<RestResponse> resultTokenCall = TokenAPIcall.ExecuteAsync(requestTokenCall).Result;
                    
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 1, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount >= approveThresholdLimit)
                {
                    sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn);
                    ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
                }
                //if not, just approved it
                else
                {
                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
                }
            }
            //Checker 1 Condition
            if (taskRole.Equals("Checker1"))
            {
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 2, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount >= approveThresholdLimit)
                {
                    sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn1stChecker);
                    ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
                }
                //if not, just approved it
                else
                {
                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
                }
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
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string reject = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            reject = collection["new_textRejectCode"];
            cheqNo = collection["current_fldChequeSerialNo"];

            
            if (pageConfig.TaskRole.Equals("Checker2")) //2nd Level
            {
                //sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", collection["txtRemarks"]));

                sqlParameterNext.Add(new SqlParameter("@fldRejectCode", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", DBNull.Value));


            }
            else if (pageConfig.TaskRole.Equals("Checker1"))
            {
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 2, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount > approveThresholdLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", collection["txtRemarks"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", DBNull.Value));

                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", DBNull.Value));


                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));


                }
                //if not, just approved it
                else
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
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", collection["txtRemarks"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", DBNull.Value));

                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", DBNull.Value));


                }


            }
            else
            { //1st Level
                double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 1, currentUser.BankCode);
                //If amount more then threshold
                if (itemAmount > approveThresholdLimit)
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", DBNull.Value));
                    
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));


                }
                //if not, just approved it
                else
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
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", DBNull.Value));
                    
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                }
            }
                
                /*}*/
                //Checker 1 Condition
                    /*if (pageConfig.TaskRole.Equals("Checker1"))
                    {
                        double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("R", 2, currentUser.BankCode);
                        //If amount more then threshold
                        if (itemAmount >= approveThresholdLimit)
                        {
                            //sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn1stChecker);
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationReturn1stChecker));
                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                        }
                        //if not, just approved it
                        else
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
                    }*/
            /*}*/
            //Compulsory update for tblInwardItemInfoStatus
            //ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //update Reject Code
            //sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            //sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextExtAreaRemarks", collection["textAreaExtRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            //Excute the command
            
            dbContext.GetRecordsAsDataTableSP("spcuReturnInwardItem", sqlParameterNext.ToArray());
            //dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void VerificationRoute(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute);
            //sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            sqlUpdateInfoStatus.Add("fldNonConfirmUserID", DBNull.Value);
            sqlUpdateInfoStatus.Add("fldNonConfirmUserClass", DBNull.Value);
            sqlUpdateInfoStatus.Add("fldNonConfirmTimeStamp", DBNull.Value);
            sqlUpdateInfoStatus.Add("fldNonConfirmStatus", DBNull.Value);
            sqlUpdateInfoStatus.Add("fldApprovalUserId", currentUser.UserId);
            sqlUpdateInfoStatus.Add("fldApprovalUserClass", currentUser.VerificationClass);
            sqlUpdateInfoStatus.Add("fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlUpdateInfoStatus.Add("fldCreateUserID", currentUser.UserId);
            sqlUpdateInfoStatus.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlUpdateInfoStatus.Add("fldUpdateUserID", currentUser.UserId);
            sqlUpdateInfoStatus.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["txtReturnCode"]);
            sqlUpdateInfoStatus.Add("fldRejectCode2", collection["txtReturnCode2"]);
            sqlUpdateInfoStatus.Add("fldRejectCode3", collection["txtReturnCode3"]);
            sqlUpdateInfoStatus.Add("fldReturnDescription1", collection["txtReturnDesc"]);
            sqlUpdateInfoStatus.Add("fldReturnDescription2", collection["txtReturnDesc2"]);
            sqlUpdateInfoStatus.Add("fldReturnDescription3", collection["txtReturnDesc3"]);



            //ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
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
            sqlInsertPendingInfo.Add("fldRejectCode", collection["txtReturnCode"]);
            sqlInsertPendingInfo.Add("fldRejectCode2", collection["txtReturnCode2"]);
            sqlInsertPendingInfo.Add("fldRejectCode3", collection["txtReturnCode3"]);


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


                    /*if (collection["new_textRejectCode"].Trim().Equals("0") || collection["new_textRejectCode"].Trim().Equals("000") || collection["new_textRejectCode"].Trim().Equals("00") || collection["new_textRejectCode"].Trim().Equals(""))
            {*/
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
                sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
                sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
                sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
                sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
                sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
                
                dbContext.GetRecordsAsDataTableSP("spcuPendingInwardItem", sqlParameterNext.ToArray());
            /*}
            else
            {
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRoute));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaExtRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("spcuPendingInwardItem", sqlParameterNext.ToArray());
            //Excute the command
            //dbContext.ConstructAndExecuteInsertCommand("tblPendingInfo", sqlInsertPendingInfo);
            /*}*/
        }

        public void VerificationPullOutNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            double itemAmount = Convert.ToDouble(collection["current_fldAmount"]);
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];


            //1st Level Verification will approve directly, but if Rejected, it will go to 2nd Level Verification
            //1st Level Verification only can Approve/Reject. No Route.
            if (pageConfig.TaskId == "306220")
            {
                if (verifyAction == "A")
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
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                }
            }
            else
            {
                //Maker Condition
                if (pageConfig.TaskRole.Equals("Maker"))
                {
                    //double approveThresholdLimit = thresholdSettingDao.GetThresholdLimit("A", 1, currentUser.BankCode);
                    ////If amount more then threshold
                    //if (itemAmount >= approveThresholdLimit)
                    //{
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationApprove));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    //}
                    ////if not, just approved it
                    //else
                    //{
                    //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                    //sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    //sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    //sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    //sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                    //}
                }

                //Checker Condition
                else if (pageConfig.TaskRole.Equals("Checker"))
                {
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationPullOut));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                }
            }
            //Compulsory update for tblInwardItemInfoStatus
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalIndicator", "Y")); //TODO: unknown process
            //sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            //sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //Since it approved, all reject code will be OK by desc
            //sqlParameterNext.Add(new SqlParameter("fldRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default")));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", "000"));
            //history param
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
            //sqlParameterNext.Add(new SqlParameter("fldActionStatusId", nextHistorySecNo);
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            //sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextExtAreaRemarks", collection["textAreaExtRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldVerifySeq", ""));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            //Excute the command
            dbContext.GetRecordsAsDataTableSP("spcuPullOutInwardItem", sqlParameterNext.ToArray());
            // dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }
        public void InsertPullOutInfo(FormCollection collection, AccountModel currentUser)
        {

            List<string> arrResults = new List<string>();
            string otherPullOutReason = "";
            string pullOutReasonBox = "";
            if (collection["pullOutReason"] != null)
            {
                otherPullOutReason = collection["pullOutReason"].ToString();
            }
            if (collection["pullOutReasonBox"] != null)
            {
                pullOutReasonBox = collection["pullOutReasonBox"].Trim().ToString();
            }
            string pullOutReason = "";

            if (otherPullOutReason != "" && pullOutReasonBox != null)
            {
                pullOutReason = pullOutReasonBox + ", " + otherPullOutReason;
            }
            else if (otherPullOutReason != "" && pullOutReasonBox == null)
            {
                pullOutReason = otherPullOutReason;
            }
            else if (otherPullOutReason == "" && pullOutReasonBox != null)
            {
                pullOutReason = pullOutReasonBox;
            }
            else
            {
                pullOutReason = "";
            }

            //if (collection["pullOutReasonBox"] != null)
            //{
            //    arrResults = collection["pullOutReasonBox"].Trim().Split(',').ToList();
            //}

            //foreach (var arrResult in arrResults)
            //{
            Dictionary<string, dynamic> sqlPullOutInfo = new Dictionary<string, dynamic>();

            int nextPullOutInfoSecNo = sequenceDao.GetNextSequenceNo("tblPullOutInfo");

            sqlPullOutInfo.Add("fldPullOutID", nextPullOutInfoSecNo);
            sqlPullOutInfo.Add("fldInwardItemId", collection["fldInwardItemId"]);
            sqlPullOutInfo.Add("fldReason", pullOutReason);
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

        public void VerificationPullOut(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            //Pull Out Field
            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationPullOut);
            //ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //Pull Out Reject Code
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void BranchConfirmation(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            sqlParameterNext.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
            sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            
            dbContext.GetRecordsAsDataTableSP("sp_updateBranchConfirmation", sqlParameterNext.ToArray());
        }

        public void VerificationRepair(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            //Pull Out Field
            sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRepair);
            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            //Excute the command
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void VerificationRepairNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];
            //Pull Out Field
            //sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRepair);
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRepair));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNext.Add(new SqlParameter("@fldCharges", collection["fldCharges"].Replace(",", "")));
            sqlParameterNext.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));
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
            dbContext.GetRecordsAsDataTableSP("sp_updateRepairInwardItem", sqlParameterNext.ToArray());
            //Excute the command
            //dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void InsertHostValidation(InwardItemViewModel inwardItemViewModel, AccountModel currentUser)
        {



            Dictionary<string, dynamic> sqlHostValidation = new Dictionary<string, dynamic>();

            sqlHostValidation.Add("fldPosPayType", inwardItemViewModel.posPayType);
            sqlHostValidation.Add("fldClearDate", inwardItemViewModel.clearDate);
            sqlHostValidation.Add("fldStartTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldEndTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldProcessName", inwardItemViewModel.Processname);
            sqlHostValidation.Add("fldBankCode", inwardItemViewModel.bankCode);
            sqlHostValidation.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldCreateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldUpdateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldStatus", inwardItemViewModel.status);
            sqlHostValidation.Add("fldBatchID", 1 );


            //Excute the command
            int count = dbContext.ConstructAndExecuteInsertCommand("tblDataProcess", sqlHostValidation);
            if (count == 1)
            {
                SqlParameter[] sqlparam = new SqlParameter[] {
                new SqlParameter("@_clearDate", inwardItemViewModel.clearDate),
                new SqlParameter("@_createUserID", currentUser.UserId),
                new SqlParameter("@_Processname", inwardItemViewModel.Processname),
                new SqlParameter("@_fldBatchId", 1),
                new SqlParameter("@_IsRetry1", "0"),
                new SqlParameter("@_ItemPerBatch", 50)

            };
                //sqlparam[0] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);
                //sqlparam[1] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);
                //sqlparam[2] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);
                //sqlparam[3] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);
                //sqlparam[4] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);
                //sqlparam[5] = new SqlParameter("@_clearDate", inwardItemViewModel.clearDate);

                //DataTable dtPostingBatches = dbContext.GetRecordsAsDataTableSP("spcgHostValidationItems", sqlparam);
                //DataTable threadTable = new DataTable();
                //if (dtPostingBatches.Rows.Count>0)
                //{

                //    threadTable.Columns.Add("fldInwardItemId");
                //    threadTable.Columns.Add("fldChargeCode");
                //    threadTable.Columns.Add("fldCheqType");
                //    threadTable.Columns.Add("fldChequeSerialNo");
                //    threadTable.Columns.Add("fldClearingType");
                //    threadTable.Columns.Add("fldCommissionCode");
                //    threadTable.Columns.Add("fldClearingAccount");
                //    threadTable.Columns.Add("fldCreditCurrency");
                //    threadTable.Columns.Add("fldCreditTheirRef");
                //    threadTable.Columns.Add("fldHostAccountNumber");
                //    threadTable.Columns.Add("fldAmount");
                //    threadTable.Columns.Add("fldDebitCurrency");
                //    threadTable.Columns.Add("fldDebitTheirRef");
                //    threadTable.Columns.Add("fldClearDate");
                //    threadTable.Columns.Add("fldPresentingBank");
                //    threadTable.Columns.Add("fldRefNo");
                //    threadTable.Columns.Add("fldTransactionType");
                //    threadTable.Columns.Add("fldT24BranchCode");

                //    int numberOfRequests = dtPostingBatches.Rows.Count;
                //    for (int i = 0; i < numberOfRequests; i++)
                //    {
                //        DataRow row = dtPostingBatches.Rows[i];
                //        string vfldInwardItemId = row["fldInwardItemId"].ToString();
                //        string vChargeCode = row["fldChargeCode"].ToString();
                //        string vCheqType = row["fldCheqType"].ToString();
                //        string vChequeNumber = row["fldChequeSerialNo"].ToString();
                //        string vClearingType = row["fldClearingType"].ToString();
                //        string vCommissionCode = row["fldCommissionCode"].ToString();
                //        string vCreditAcctNo = row["fldClearingAccount"].ToString();
                //        string vCreditCurrency = row["fldCreditCurrency"].ToString();
                //        string vCreditTheirRef = row["fldCreditTheirRef"].ToString();
                //        string vDebitAcctNo = row["fldHostAccountNumber"].ToString();
                //        string vDebitAmount = row["fldAmount"].ToString();
                //        string vDebitCurrency = row["fldDebitCurrency"].ToString();
                //        string vDebitTheirRef = row["fldDebitTheirRef"].ToString();
                //        string vDebitValueDate = row["fldClearDate"].ToString();
                //        string vPresentingBank = row["fldPresentingBank"].ToString();
                //        string vRefNo = row["fldRefNo"].ToString();
                //        string vTransactionType = row["fldTransactionType"].ToString();
                //        string vfldT24BranchCode = row["fldT24BranchCode"].ToString();


                //    DataRow myrow = threadTable.NewRow();
                //    myrow["fldInwardItemId"] = vfldInwardItemId;
                //    myrow["fldChargeCode"] = vChargeCode;
                //    myrow["fldCheqType"] = vCheqType;
                //    myrow["fldChequeSerialNo"] = vChequeNumber;
                //    myrow["fldClearingType"] = vClearingType;
                //    myrow["fldCommissionCode"] = vCommissionCode;
                //    myrow["fldClearingAccount"] = vCreditAcctNo;
                //    myrow["fldCreditCurrency"] = vCreditCurrency;
                //    myrow["fldCreditTheirRef"] = vCreditTheirRef;
                //    myrow["fldHostAccountNumber"] = vDebitAcctNo;
                //    myrow["fldAmount"] = vDebitAmount;
                //    myrow["fldDebitCurrency"] = vDebitCurrency;
                //    myrow["fldDebitTheirRef"] = vDebitTheirRef;
                //    myrow["fldClearDate"] = vDebitValueDate;
                //    myrow["fldPresentingBank"] = vPresentingBank;
                //    myrow["fldRefNo"] = vRefNo;
                //    myrow["fldTransactionType"] = vTransactionType;
                //    myrow["fldT24BranchCode"] = vfldT24BranchCode;
                //    threadTable.Rows.Add(myrow)


                //    }




                }
            }


        
        // For Review
        public void VerificationReview(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];

            // Getting data to update
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            //Update SP
            dbContext.GetRecordsAsDataTableSP("spcuReviewInwardItem", sqlParameterNext.ToArray());
        }


        public void BranchApprove(FormCollection collection, AccountModel currentUser, string taskRole)
        {
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            
            //Maker Condition
            if (taskRole.Equals("Maker"))
            {
                sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.BranchApproveMaker);
                sqlUpdatePendingInfo.Add("fldRejectCode", "000");
                sqlUpdatePendingInfo.Add("fldValidateFlag", "0");

                sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.BranchApproveMaker);
                sqlUpdateInfoStatus.Add("fldRejectCode", "000");
                ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            }
            //Checker 1 Condition
            else if (taskRole.Equals("Checker"))
            {
                // initialize Approve the Approval or Rejection based on MAKER
                string itemStatus = commonInwardItemDao.GetBranchItemStatus(collection);
                if (itemStatus.Equals(VerificationStatus.ACTION.BranchApproveMaker))
                {

                    sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                    sqlUpdatePendingInfo.Add("fldRejectCode", "000");
                    sqlUpdatePendingInfo.Add("fldValidateFlag", "0");

                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove);
                    sqlUpdateInfoStatus.Add("fldRejectCode", "000");
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

                }
                else if (itemStatus.Equals(VerificationStatus.ACTION.BranchReturnMaker))
                {

                    sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                    sqlUpdatePendingInfo.Add("fldRejectCode", collection["new_textRejectCode"]);
                    sqlUpdatePendingInfo.Add("fldValidateFlag", "0");

                    sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                    sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
                    ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);

                }
            }

            //Excute the command tblPendingInfo
            ExtendPendingInfoCommonField(sqlUpdatePendingInfo, collection, currentUser);
            dbContext.ConstructAndExecuteUpdateCommand("tblPendingInfo", sqlUpdatePendingInfo, sqlCondition);

            //Excute the command tblInwardItemInfoStatus
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
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
            //Maker Condition
            if (pageConfig.TaskRole.Equals("Maker") || verificationlimit == false)
            {
                sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", "000"));
                sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.BranchApproveMaker));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.BranchApproveMaker));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));/*
                sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));*/
                //sqlParameterNew.Add(new SqlParameter("@fldCharges", collection["fldCharges"]));
                //sqlParameterNew.Add(new SqlParameter("fldRejectCode", "000"));
                sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
                sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
                sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
                sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
                sqlParameterNew.Add(new SqlParameter("@fldAlert", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldAlertReason", DBNull.Value));
                //sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaExtRemarks"]));
                sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["txtRemarks"]));

                sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
                sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
                sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
                sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
                dbContext.GetRecordsAsDataTableSP("spcuApprovedPendingInwardMaker", sqlParameterNew.ToArray());

            }
            //Checker 1 Condition
            else if (pageConfig.TaskRole.Equals("Checker"))
            {
                // initialize Approve the Approval or Rejection based on MAKER
                //string itemStatus = commonInwardItemDao.GetBranchItemStatus(collection);
                //if (itemStatus.Equals(VerificationStatus.ACTION.VerificationRoute) || itemStatus.Equals(VerificationStatus.ACTION.BranchApproveMaker))
                //{
                //    sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", commonInwardItemDao.GetRejectCodeByRejectDesc("Default")));
                //    sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                //    if (collection["RejectStatus1"] == "005" || collection["RejectStatus2"] == "005")
                //    {
                //        sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRouteCreditControl));
                //    }
                //    else
                //    {
                //        sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                //    }

                    sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", "000"));
                    sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationApprove)); 
                    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));

                /*
                sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("fldRejectCode", "000"));*/
                //sqlParameterNew.Add(new SqlParameter("@fldCharges", 0)); //collection["fldCharges"]


                    sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
                    sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
                    sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
                    sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
                    sqlParameterNew.Add(new SqlParameter("@fldAlert", "1"));
                    sqlParameterNew.Add(new SqlParameter("@fldAlertReason", DBNull.Value));//collection["fldReviewReason"]
                    
                    sqlParameterNew.Add(new SqlParameter("@fldBranchRemarks", collection["txtRemarks"]));
                    //sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaExtRemarks"]));
                    sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
                    sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
                    sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
                    sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
                    dbContext.GetRecordsAsDataTableSP("spcuApprovedPendingInwardChecker", sqlParameterNew.ToArray());

                //}
                //else if (itemStatus.Equals(VerificationStatus.ACTION.BranchReturnMaker))
                //{
                //    sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                //    sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                //    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                //    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                //    sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                //    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                //    sqlParameterNew.Add(new SqlParameter("@fldCharges", "0"));
                //    sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
                //    sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
                //    sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["remarkField"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
                //    sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
                //    sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
                //    sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
                //    sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
                //    dbContext.GetRecordsAsDataTableSP("sp_updateReturnPendingInwardItem", sqlParameterNew.ToArray());
                //}
            }

            //history param

        }


        public void BranchReturn(FormCollection collection, AccountModel currentUser, string taskRole)
        {
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            //Maker Condition
            if (taskRole.Equals("Maker"))
            {
                sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.BranchReturnMaker);
                sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.BranchReturnMaker);
                ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            }
            //Checker 1 Condition
            else if (taskRole.Equals("Checker"))
            {
                sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                ExtendInfoStatusApprovalField(sqlUpdateInfoStatus, currentUser);
            }

            //Excute the command tblPendingInfo
            sqlUpdatePendingInfo.Add("fldRejectCode", collection["new_textRejectCode"]);
            sqlUpdatePendingInfo.Add("fldValidateFlag", "0");
            ExtendPendingInfoCommonField(sqlUpdatePendingInfo, collection, currentUser);
            dbContext.ConstructAndExecuteUpdateCommand("tblPendingInfo", sqlUpdatePendingInfo, sqlCondition);

            //Excute the command tblInwardItemInfoStatus
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
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
            if (pageConfig.TaskRole.Equals("Maker") || verificationlimit == false)
            {
                sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
                sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                //sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                //sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldAlert", DBNull.Value));
                sqlParameterNew.Add(new SqlParameter("@fldAlertReason", DBNull.Value));
                //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.BranchReturnMaker));
                //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
                //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
                //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                sqlParameterNew.Add(new SqlParameter("@fldCharges", collection["fldCharges"]));
                //sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));

            }
            //Checker 1 Condition
            else if (pageConfig.TaskRole.Equals("Checker"))
            {

                    sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
                    sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                    //sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    //sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    /*sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));*/
                    sqlParameterNew.Add(new SqlParameter("@fldAlert", "1"));
                    sqlParameterNew.Add(new SqlParameter("@fldAlertReason", collection["fldReviewReason"]));
                    sqlParameterNew.Add(new SqlParameter("@fldCharges", collection["fldCharges"]));
                    //sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));

            }

            //history param
            sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
            //sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaExtRemarks"]));
            sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("spcuRejectedPendingInwardItem", sqlParameterNew.ToArray());

        }


        public void BranchReferBack(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            //Excute the command tblPendingInfo
            sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute);
            //sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.BranchReferBackChecker);
            sqlUpdatePendingInfo.Add("fldValidateFlag", "0");
            ExtendPendingInfoCommonField(sqlUpdatePendingInfo, collection, currentUser);
            dbContext.ConstructAndExecuteUpdateCommand("tblPendingInfo", sqlUpdatePendingInfo, sqlCondition);

            //Excute the command tblInwardItemInfoStatus
            //sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.BranchReferBackChecker);
            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute);
            ExtendInfoStatusNonConfirmField(sqlUpdateInfoStatus, currentUser);
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
            ExtendInfoStatusCommonField(sqlUpdateInfoStatus, collection);
            dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
        }

        public void BranchReferBackNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction, Boolean verificationlimit)
        {

            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            Dictionary<string, dynamic> sqlUpdatePendingInfo = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };
            string accNo = "";
            string cheqNo = "";
            accNo = collection["current_fldAccountNumber"];
            cheqNo = collection["current_fldChequeSerialNo"];

            sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", "002"));
            //sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.BranchReferBackChecker));
            sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.BranchReferBackChecker));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            //sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.VerificationRoute));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            //sqlParameterNew.Add(new SqlParameter("@fldCharges", "0"));
            //sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));


            sqlParameterNew.Add(new SqlParameter("@fldAlert", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldAlertReason", DBNull.Value));
            
            //history param
            sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["txtRemarks"]));
            //sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["txtRemarks"]));
            sqlParameterNew.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNew.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNew.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNew.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            dbContext.GetRecordsAsDataTableSP("sp_updateReferBackPendingInwardItem", sqlParameterNew.ToArray());

        }

        public List<string> ValidateVerification(FormCollection col, AccountModel currentUser, string verifyAction, string taskid)
        {

            List<string> err = new List<string>();
            VerificationLimitModel verificationLimit = verificationLimitDao.GetVerifyLimit(currentUser.VerificationClass);
            double itemAmount = Convert.ToDouble(col["current_fldAmount"]);
            string DataAction = col["DataAction"].ToString().Trim();
            bool checkRecordUpdated;


            //if(itemAmount < Convert.ToInt32(verificationLimit.fld1stAmt))
            //{
            //        err.Add("Cheque is greater than the Verification Limit");
                
            //}
            if (!(verificationLimit.fld2ndType == ""))
            {
                if (itemAmount <= Convert.ToInt32(verificationLimit.fld1stAmt)  || itemAmount >= Convert.ToInt32(verificationLimit.fld2ndAmt))
                {
                    err.Add("Cheque is greater than the Verification Limit");

                }


            }






            //BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"], col["current_fldUpdateTimeStamp"]))
                if (commonInwardItemDao.CheckLockedCheck2(col["fldInwardItemId"], currentUser.UserId))
                {
                    err.Add("Cheque is currently being locked by other user");
                }
        

            //if (!"Rev".Equals(verifyAction))
            //{
            //    if (commonInwardItemDao.CheckIfUPIGenerated(col["fldInwardItemId"]))
            //    {
            //        err.Add("UPI already generated for this item. User cannot perform any changes!");
            //    }
            //}


            //if (DataAction == "ChequeVerificationPage")
            //{
            //    string UpdatedOrDelete = commonInwardItemDao.CheckIfRecordUpdatedorDeletedForListing(col["fldInwardItemId"]);
            //    //string UpdatedOrDelete = "";
            //    if (UpdatedOrDelete.Trim() != "" && UpdatedOrDelete.Trim() != null)
            //    {
            //        if (taskid.ToString().Trim() == "306230")
            //        {
            //            if ("B".Equals(verifyAction))
            //            {

            //            }
            //            else
            //            {
            //                if (Check2ndVerification(col["fldInwardItemId"], currentUser.UserId))
            //                {
            //                    err.Add("Item cannot be verified by same user");
            //                }
            //                else
            //                {
            //                    //Compare user class with amount 
            //                    if ("".Equals(verificationLimit.fldConcatenate))
            //                    {
            //                        if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount))
            //                        {
            //                            err.Add(Locale.UserClassNotAuthorizetoVerify);
            //                        }
            //                    }
            //                    else if ("and".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //                    {
            //                        if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) && StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //                        {
            //                            err.Add(Locale.UserClassNotAuthorizetoVerify);
            //                        }
            //                    }
            //                    else if ("or".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            //                    {
            //                        if (StringUtils.Compare(verificationLimit.fld1stType, verificationLimit.fld1stAmt, itemAmount) || StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
            //                        {
            //                            err.Add(Locale.UserClassNotAuthorizetoVerify);
            //                        }
            //                    }
            //                }
            //            }

            //        }
            //        else
            //        {
            //            if (UpdatedOrDelete.Trim() == "updated")
            //            {
            //                err.Add("This record has been updated by another user.");
            //            }
            //            else
            //            {
            //                err.Add("This record has been deleted by another user.");
            //            }
            //        }

            //    }
            //    else
            //    {
            //        if (taskid.ToString().Trim() == "306230")
            //        {
            //            if ("B".Equals(verifyAction))
            //            {

            //            }
            //            else
            //            {
            //                if (Check2ndVerification(col["fldInwardItemId"], currentUser.UserId))
            //                {
            //                    err.Add("Item cannot be verified by same user");
            //                }
            //            }
            //        }

            //        //Validation for Review
            //        if ("Rev".Equals(verifyAction))
            //        {
            //            if ("0".Equals(chequeActivation.fldRVWKLActivation))
            //            {
            //                err.Add("Review are not allowed");
            //            }
            //        }
            //        else
            //        {
            //            //Check ccu activation and cut off time-- FOR MAB VERIFICATIOn
            //            if ("0".Equals(chequeActivation.fldKLActivation) || String.IsNullOrEmpty(chequeActivation.fldKLActivation))
            //            {
            //                err.Add(Locale.VerificationNotAllowed);
            //            }
            //        }

            //        //Validation for Approve
            //        if ("A".Equals(verifyAction))
            //        {
            //            //put verification for approve here if needed

            //            if (!col["new_textRejectCode"].Trim().Equals("000") && !col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add("Do not select Return Reason");
            //            }

            //        }
            //        //Validation for Return
            //        if ("R".Equals(verifyAction))
            //        {
            //            if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add(Locale.RejectCodeCannotEmpty);
            //            }
            //            if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                err.Add("Internal Reject Code is only allowed when click Pending");
            //            }
            //            if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                err.Add("Invalid Return Reason");
            //            }
            //        }
            //        //Validation for Route
            //        if ("B".Equals(verifyAction))
            //        {

            //            if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add(Locale.RejectCodeCannotEmpty);
            //            }
            //            if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
            //                {
            //                    //err.Add("Internal Reject Code is only allowed when click Pending");
            //                }
            //                else
            //                {
            //                    err.Add("Invalid Return Reason");
            //                }
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    if (taskid.ToString().Trim() == "306510")
            //    {
            //        checkRecordUpdated = false;
            //        string UpdatedOrDelete = commonInwardItemDao.CheckIfRecordUpdatedorDeletedForListing(col["fldInwardItemId"]);
            //        if ("Rev".Equals(verifyAction))
            //        {
            //            UpdatedOrDelete = "";
            //        }

            //        if (UpdatedOrDelete.Trim() != "" && UpdatedOrDelete.Trim() != null)
            //        {
            //            if (UpdatedOrDelete.Trim() == "updated")
            //            {
            //                err.Add("This record has been updated by another user.");
            //            }
            //            else
            //            {
            //                err.Add("This record has been deleted by another user.");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        checkRecordUpdated = commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"]);
            //    }

            //    if (checkRecordUpdated)
            //    {
            //        if ("Rev".Equals(verifyAction))
            //        {
            //            //Validation for Review
            //            if ("Rev".Equals(verifyAction))
            //            {
            //                if ("0".Equals(chequeActivation.fldRVWKLActivation))
            //                {
            //                    err.Add("Review are not allowed");
            //                }
            //            }
            //        }
            //        else
            //        {
            //            err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            //        }
            //    }
            //    else
            //    {
            //        if (taskid.ToString().Trim() == "306230")
            //        {
            //            if (Check2ndVerification(col["fldInwardItemId"], currentUser.UserId))
            //            {
            //                err.Add("Item cannot be verified by same user");
            //            }
            //        }

            //        //Validation for Review
            //        if ("Rev".Equals(verifyAction))
            //        {
            //            if ("0".Equals(chequeActivation.fldRVWKLActivation))
            //            {
            //                err.Add("Review are not allowed");
            //            }
            //        }
            //        else
            //        {
            //            //Check ccu activation and cut off time-- FOR MAB VERIFICATIOn
            //            if ("0".Equals(chequeActivation.fldKLActivation) || String.IsNullOrEmpty(chequeActivation.fldKLActivation))
            //            {
            //                err.Add(Locale.VerificationNotAllowed);
            //            }
            //        }

            //        //Validation for Approve
            //        if ("A".Equals(verifyAction))
            //        {
            //            //put verification for approve here if needed
            //            if (!col["new_textRejectCode"].Trim().Equals("000") && !col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add("Do not select Return Reason");
            //            }
            //        }
            //        //Validation for Return
            //        if ("R".Equals(verifyAction))
            //        {
            //            if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add(Locale.RejectCodeCannotEmpty);
            //            }
            //            if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                err.Add("Internal Reject Code is only allowed when click Pending");
            //            }
            //            if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                err.Add("Invalid Return Reason");
            //            }
            //        }
            //        //Validation for Route
            //        if ("B".Equals(verifyAction))
            //        {
            //            if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
            //            {
            //                err.Add(Locale.RejectCodeCannotEmpty);
            //            }
            //            if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
            //            {
            //                if (returnCodeDao.CheckValidateInternalReturnCode(col["new_textRejectCode"].Trim()))
            //                {
            //                    //err.Add("Internal Reject Code is only allowed when click Pending");
            //                }
            //                else
            //                {
            //                    err.Add("Invalid Return Reason");
            //                }
            //            }
            //        }
            //    }
            //}


            //remove by din because we already filter queue based on verification limit

            //Compare user class with amount 
            /*if ("".Equals(verificationLimit.fldConcatenate))
            {
                if (!StringUtils.Compare(verificationLimit.fld1stType, itemAmount, verificationLimit.fld1stAmt))
                {
                    err.Add(Locale.UserClassNotAuthorizetoVerify);
                }
            }
            else if ("and".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            {
                if (!StringUtils.Compare(verificationLimit.fld1stType, itemAmount, verificationLimit.fld1stAmt) || !StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
                {
                    err.Add(Locale.UserClassNotAuthorizetoVerify);
                }
            }
            else if ("or".Equals(verificationLimit.fldConcatenate.Trim().ToLower()))
            {
                if (!StringUtils.Compare(verificationLimit.fld1stType, itemAmount, verificationLimit.fld1stAmt) && !StringUtils.Compare(verificationLimit.fld2ndType, itemAmount, verificationLimit.fld2ndAmt))
                {
                    err.Add(Locale.UserClassNotAuthorizetoVerify);
                }
            }*/

            return err;
        }

        public List<string> LockedCheck(FormCollection col, AccountModel currentUser)
        {
            List<string> err = new List<string>();
            if (commonInwardItemDao.CheckLockedCheck(col["fldInwardItemId"], currentUser.UserId))
            {
                err.Add("This check is locked by other user");
            }
            else
            {
                
            }
            return err;
        }


        public List<string> ValidateBranch(FormCollection col, AccountModel currentUser, string verifyAction)
        {
            List<string> err = new List<string>();
            //VerificationLimitModel verificationLimit = verificationLimitDao.GetVerifyLimit(currentUser.VerificationClass);
            double itemAmount = Convert.ToDouble(col["current_fldAmount"]);
            string branchCode = col["fldIssueBankBranch"].ToString().Trim();
            string DataAction = col["DataAction"].ToString().Trim();
            bool checkUpdatedOrdDeleted;

            BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            if (DataAction == "ChequeVerificationPage")
            {
                checkUpdatedOrdDeleted = false;
            }
            else
            {
                checkUpdatedOrdDeleted = commonInwardItemDao.CheckIfRecordUpdatedOrDeletedBranch(col["fldInwardItemId"]);
            }

            //Comomon Validation
            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"], col["fldUpdateTimestamp"]))
           
            if (checkUpdatedOrdDeleted)
            {

                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }
            else
            {
                //Check Branch Submission

                if (branchSubmissionDao.CheckBranchSubmissionPerformed(branchCode))
                {
                    err.Add("Branch submission already performed. Any action cannot be performed anymore.");
                }
                else
                {
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

                    //Check branch activation and cut off time
                    /*if ("1".Equals(chequeActivation.fldBPCKLActivation))
                    {*/
                        if ("1".Equals(chequeActivation.fldBPCKLActivation))
                        {
                            if (DateTime.Now >= Convert.ToDateTime(cutOffTime.fldKLCutOffTime))
                            {
                                err.Add(Locale.CutOffTimeActivatedBranchVerificationnotAllowed);
                            }
                        }
                        else
                        {
                            err.Add(Locale.CutOffTimeActivatedBranchVerificationnotAllowed);
                        }
                    /*}
                    else
                    {
                        err.Add(Locale.VerificationNotAllowed);
                    }*/

                    if ("H".Equals(verifyAction) || "A".Equals(verifyAction))
                    {
                        //put verification for approve here if needed

                        if (!col["new_textRejectCode"].Trim().Equals("000") && !col["new_textRejectCode"].Trim().Equals(""))
                        {
                            err.Add("Do not select Return Reason");
                        }

                    }

                    //Validation for Return Branch
                    if ("J".Equals(verifyAction) || "K".Equals(verifyAction) || "R".Equals(verifyAction))
                    {
                        if (col["new_textRejectCode"].Trim().Equals("0") || col["new_textRejectCode"].Trim().Equals("000") || col["new_textRejectCode"].Trim().Equals("00") || col["new_textRejectCode"].Trim().Equals(""))
                        {
                            err.Add(Locale.RejectCodeCannotEmpty);
                        }
                        if (returnCodeDao.CheckValidateReturnCode(col["new_textRejectCode"].Trim()))
                        {
                            err.Add("Invalid Return Reason");
                        }
                    }

                    //Validate Review Reason
                    if (!string.IsNullOrEmpty(col["fldReviewStatus"]))
                    {
                        if (col["fldReviewStatus"].Trim().Equals("1"))
                        {
                            if (col["fldReviewReason"].Trim().Equals(""))
                            {
                                err.Add("Review Reason Cannot Empty");
                            }
                        }
                    }
                }
            }
            return err;
        }

        //add by shamil 20161225
        //list the branch from tblDedicatedBranch using user id
        public List<string> VerificationCondition(String UserId, String TaskId)
        {
            string stmt = "Select distinct (fldBranchId) as fldbranchcode from tbldedicatedbranchdate where" +
                " flduserid=@UserId";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@UserId", UserId) });
            List<string> branchAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                branchAvailable.Add(row["fldbranchcode"].ToString());
            }
            return branchAvailable;
            //return null;
        }

        public List<string> GetTruncatedAmount()
        {
            string stmt = "SELECT fldTruncateMin, fldTruncateMax, fldTruncateRemark FROM tblSysSetting";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt);
            List<string> truncatedAmount = new List<string>();
            foreach (DataRow row in ds.Rows)
            {
                truncatedAmount.Add(row["fldTruncateMin"].ToString());
                truncatedAmount.Add(row["fldTruncateMax"].ToString());
                truncatedAmount.Add(row["fldTruncateRemark"].ToString());
            }
            return truncatedAmount;
        }
        public Boolean VerifyClassLimit(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            Boolean ind = false;

            sqlParameterNew.Add(new SqlParameter("@userClass", currentUser.VerificationClass));
            sqlParameterNew.Add(new SqlParameter("@Amount", col["current_fldAmount"]));
            DataTable ds =  dbContext.GetRecordsAsDataTableSP("sp_checkVerificationLimit", sqlParameterNew.ToArray());

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

        public bool Check2ndVerification(string inwardItemId, string currentUser)
        {
            List<SqlParameter> sqlParameterNew = new List<SqlParameter>();
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemId", inwardItemId));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser));
            DataTable ds = dbContext.GetRecordsAsDataTableSP("spcgVerification2nd", sqlParameterNew.ToArray());

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






    }
}