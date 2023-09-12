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
using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using INCHEQS.Areas.ICS.Models.ICSAPIHostValidation;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security;
using Microsoft.ReportingServices.ReportProcessing.ExprHostObjectModel;

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
        private readonly IAuditTrailDao auditTrailDao;

        public VerificationDao(ISequenceDao sequenceDao, IPageConfigDao pageConfigDao, ICommonInwardItemDao commonInwardItemDao, IThresholdSettingDao thresholdSettingDao, IVerificationLimitDao verificationLimitDao, ApplicationDbContext dbContext, IBranchActivationDao branchActivationDao, IReturnCodeDao returnCodeDao, IBranchSubmissionDao branchSubmissionDao, IAuditTrailDao auditTrailDao)
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
            this.auditTrailDao = auditTrailDao;
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
            if (collection["fldCharges"] != null)
            {
                field.Add("fldCharges", collection["fldCharges"].Replace(",", ""));
            }
            else
            {
                field.Add("fldCharges", "");

            }
            field.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
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

        public string VerificationApproveNew(FormCollection collection, AccountModel currentUser, QueueSqlConfig pageConfig, string verifyAction)
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
            string Message = "";

            #region Previous Comment code
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
            #endregion


            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["current_fldRejectCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));

            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));

            sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", ""));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", ""));
            if (pageConfig.TaskId == "306920")
            {
                sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", "1"));

            }
            else
            {
                sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));

            }



            //Check if Maker is Zero
            if (string.IsNullOrEmpty(currentUser.fldZeroMaker))
            {
                currentUser.fldZeroMaker = "";
            }


            if (currentUser.fldZeroMaker == "Y")
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
                dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());


            }
            else
            {

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
                    APIFundsTransfer apiResponse = new APIFundsTransfer(dbContext);
                    Message = apiResponse.GETAPIReponse(collection, accNo, cheqNo);
                    if (Message == "Request Processed Successfully")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));

                        dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());

                    }



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
                        dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());
                    }
                    //if not, just approved it
                    else
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                        sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                        if (collection["current_fldNonConfirmStatus"] == "R")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));

                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", VerificationStatus.ACTION.VerificationReturn));
                        }
                        else
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationApprove));
                            sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
                            isRtService = true;

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
                        dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());


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
                        isRtService = true;




                    }
                }
            }

            #region Comment Code

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
            //sqlParameterNext.Add(new SqlParameter("fldActionStatusId", nextHistorySecNo);
            //sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql()));

            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextExtAreaRemarks", collection["textAreaExtRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldVerifySeq", ""));
            //sqlParameterNext.Add(new SqlParameter("@fldRemarks", ""));
            #endregion

            
            //Excute the command
            try
            {

                if (isRtService)
                {
                     APIFundsTransfer apiResponse = new APIFundsTransfer(dbContext);
                     Message = apiResponse.GETAPIReponse(collection, accNo,cheqNo);
                    if (Message == "Request Processed Successfully")
                    {
                        dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());

                    }
                }
                if (collection["current_fldNonConfirmStatus"] == "R" && pageConfig.TaskRole.Equals("Checker1"))
                {
                    dbContext.GetRecordsAsDataTableSP("spcuApprovedInwardItem", sqlParameterNext.ToArray());

                }

            }
            catch (Exception ex)
            {
               return "Exception"+ex.StackTrace+ ex.Message;
            }
            return Message;
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

            if (string.IsNullOrEmpty(currentUser.fldZeroMaker))
            {
                currentUser.fldZeroMaker = "";
            }

            if (currentUser.fldZeroMaker == "Y")
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
                sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));

                sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                
                if (collection["txtReturnCode"] == "801")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                }
                else if (collection["txtReturnCode2"] == "801")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                }
                else if (collection["txtReturnCode3"] == "801")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
                }
                else
                {
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                }


            }
            else
            {

                if (pageConfig.TaskRole.Equals("Checker2")) //2nd Level
                {
                    //sqlUpdateInfoStatus.Add("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn);
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", VerificationStatus.ACTION.VerificationReturn));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalUserClass", currentUser.VerificationClass));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmStatus", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserID", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmUserClass", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldNonConfirmTimeStamp", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", string.IsNullOrEmpty(collection["txtRemarks"]) ? "" : collection["txtRemarks"]));
                    sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                    sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));

                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", DBNull.Value));
                    //sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", DBNull.Value));
                    if (collection["txtReturnCode"] == "801")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                    }
                    else if (collection["txtReturnCode2"] == "801")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                    }
                    else if (collection["txtReturnCode3"] == "801")
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
                    }
                    else
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                    }




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
                        sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));


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
                        sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));


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
                        sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));

                        if (collection["txtReturnCode"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }
                        else if (collection["txtReturnCode2"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }
                        else if (collection["txtReturnCode3"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
                        }
                        else
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }


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
                        sqlParameterNext.Add(new SqlParameter("@fldRemarks1stLevel", DBNull.Value));
                        sqlParameterNext.Add(new SqlParameter("@fldRemarks2ndLevel", DBNull.Value));

                        sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
                        sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
                        sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));
                        /*sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                        sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));
                        */
                        sqlParameterNext.Add(new SqlParameter("@fldAuthorizer", DBNull.Value));


                        if (collection["txtReturnCode"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }
                        else if (collection["txtReturnCode2"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }
                        else if (collection["txtReturnCode3"] == "801")
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
                        }
                        else
                        {
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                            sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

                        }
                    }
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
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]== null ? "" : collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaRemarks"]));
            //sqlParameterNext.Add(new SqlParameter("@fldTextExtAreaRemarks", collection["textAreaExtRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            //Excute the command

            dbContext.GetRecordsAsDataTableSP("spcuReturnInwardItem", sqlParameterNext.ToArray());

        }

        public void VerificationRoute(FormCollection collection, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
            Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", collection["fldInwardItemId"] } };

            sqlUpdateInfoStatus.Add("fldApprovalStatus", VerificationStatus.ACTION.VerificationRoute);
            sqlUpdateInfoStatus.Add("fldRejectCode", collection["new_textRejectCode"]);
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
            if (collection["txtReturnCode"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }
            else if (collection["txtReturnCode2"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }
            else if (collection["txtReturnCode3"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
            }
            else
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }

            sqlParameterNext.Add(new SqlParameter("@fldTaskId", pageConfig.TaskId));
            sqlParameterNext.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNext.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]== null? "": collection["textAreaRemarks"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@accountNumber", accNo));
            sqlParameterNext.Add(new SqlParameter("@chequeNumber", cheqNo));
            sqlParameterNext.Add(new SqlParameter("@BankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@BranchPendingRemarks", ""));


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

        public string CreditPostingMarked(string inwardItemId, FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string accNo = collection["fldAccountNumberLog"];
            string cheqNo = collection["fldChequeSerialNoLog"];
            string DataAction = collection["fldTaskId"];
            sqlParameterNext.Add(new SqlParameter("@fldInwardItemId", inwardItemId));


            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", collection["txtReturnCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode2", collection["txtReturnCode2"]));
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode3", collection["txtReturnCode3"]));

            if (collection["txtReturnCode"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }
            else if (collection["txtReturnCode2"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));

                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }
            else if (collection["txtReturnCode3"] == "801")
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["textAreaRemarks"]));
            }
            else
            {
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription1", collection["txtReturnDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription2", collection["txtReturnDesc2"]));
                sqlParameterNext.Add(new SqlParameter("@fldReturnDescription3", collection["txtReturnDesc3"]));

            }
            APIFundsTransfer apiResponse = new APIFundsTransfer(dbContext);
            string Message = apiResponse.GETAPIReponse(collection, accNo, cheqNo);
            if (Message == "Request Processed Successfully")
            {
                DataTable dtCreditPosting = dbContext.GetRecordsAsDataTableSP("sp_updateCreditPostingMarked", sqlParameterNext.ToArray());
            }

            return Message;

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
            sqlHostValidation.Add("fldBatchID", 1);


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
            sqlUpdatePendingInfo.Add("fldApprovalStatus", VerificationStatus.ACTION.BranchReferBackChecker);
            sqlUpdatePendingInfo.Add("fldValidateFlag", "0");
            ExtendPendingInfoCommonField(sqlUpdatePendingInfo, collection, currentUser);
            dbContext.ConstructAndExecuteUpdateCommand("tblPendingInfo", sqlUpdatePendingInfo, sqlCondition);

            //Excute the command tblInwardItemInfoStatus
            sqlUpdateInfoStatus.Add("fldNonConfirmStatus", VerificationStatus.ACTION.BranchReferBackChecker);
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

            sqlParameterNew.Add(new SqlParameter("@fldPendingRejectCode", collection["new_textRejectCode"]));
            sqlParameterNew.Add(new SqlParameter("@fldPendingApprovalStatus", VerificationStatus.ACTION.BranchReferBackChecker));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalStatus", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalUserId", currentUser.UserId));
            //sqlParameterNew.Add(new SqlParameter("@fldApprovalUserClass", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldApprovalTimeStamp", DBNull.Value));
            sqlParameterNew.Add(new SqlParameter("@fldNonConfirmStatus", VerificationStatus.ACTION.BranchReferBackChecker));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserID", currentUser.UserId));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmUserClass", currentUser.VerificationClass));
            //sqlParameterNew.Add(new SqlParameter("@fldNonConfirmTimeStamp", DateUtils.GetCurrentDatetimeForSql()));
            sqlParameterNew.Add(new SqlParameter("@fldCharges", "0"));
            //sqlParameterNew.Add(new SqlParameter("fldRejectCode", collection["new_textRejectCode"]));

            //history param
            sqlParameterNew.Add(new SqlParameter("@fldQueue", pageConfig.TaskId));
            sqlParameterNew.Add(new SqlParameter("@fldActionStatus", verifyAction));
            sqlParameterNew.Add(new SqlParameter("@fldUIC", collection["current_fldUIC"]));
            sqlParameterNew.Add(new SqlParameter("@fldInwardItemID", collection["fldInwardItemId"]));
            sqlParameterNew.Add(new SqlParameter("@fldRemarks", collection["textAreaRemarks"]));
            sqlParameterNew.Add(new SqlParameter("@fldAlertReason", collection["textAreaRemarks"]));

            //sqlParameterNew.Add(new SqlParameter("@fldTextAreaRemarks", collection["textAreaExtRemarks"]));
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



            //BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"], col["current_fldUpdateTimeStamp"]))
            //if (commonInwardItemDao.CheckLockedCheck2(col["fldInwardItemId"], currentUser.UserId))
            //{
            //    err.Add("Cheque is currently being locked by other user");
            //}

            

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


        public List<string> ValidateVerificationService(FormCollection col, AccountModel currentUser, string verifyAction, string taskid,string message)
        {

            List<string> err = new List<string>();
            VerificationLimitModel verificationLimit = verificationLimitDao.GetVerifyLimit(currentUser.VerificationClass);
            double itemAmount = Convert.ToDouble(col["current_fldAmount"]);
            string DataAction = col["DataAction"].ToString().Trim();
            bool checkRecordUpdated;



            //BranchActivationModel cutOffTime = branchActivationDao.GetCutOffTime(col["fldClearDate"]);
            BranchActivationModel chequeActivation = branchActivationDao.GetChequeActivation(col["fldClearDate"]);

            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"], col["current_fldUpdateTimeStamp"]))
            if (commonInwardItemDao.CheckLockedCheck2(col["fldInwardItemId"], currentUser.UserId))
            {
                err.Add("Cheque is currently being locked by other user");
            }
            err.Add(message);

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

        public string CheckerConfirmNew(FormCollection col, AccountModel currentUser, string taskId, string Message)
        {
            try
            {
                string auditLogBefore = "";
                string auditLogAfter = "";
                string historyRemarks = "";
                string modifiedField = "";
                int nextVerifySeq = commonInwardItemDao.GetNextVerifySeqNo(col["fldInwardItemId"]);
                int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int changesCounter = 0;

                string accNo = "";
                string cheqNo = "";
                string amount = col["current_fldAmount"];
                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", col["fldInwardItemId"] } };


                sqlUpdateInfo.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlUpdateInfo.Add("fldCreateUserID", currentUser.UserId);
                if (string.IsNullOrEmpty(col["new_fldchequeserialno"]))
                {
                    col["new_fldchequeserialno"] = col["current_fldchequeserialno"];
                    accNo = col["new_fldchequeserialno"];
                }

                if (string.IsNullOrEmpty(col["new_fldtranscode"]))
                {
                    col["new_fldtranscode"] = col["current_fldtranscode"];
                }

                if (string.IsNullOrEmpty(col["new_fldIssueBankCode"]))
                {
                    col["new_fldIssueBankCode"] = col["current_fldIssueBankCode"];
                }

                if (string.IsNullOrEmpty(col["new_fldIssueStateCode"]))
                {
                    col["new_fldIssueStateCode"] = col["current_fldIssueStateCode"];
                }

                if (string.IsNullOrEmpty(col["new_fldIssueBranchCode"]))
                {
                    col["new_fldIssueBranchCode"] = col["current_fldIssueBranchCode"];
                }


                if (string.IsNullOrEmpty(col["new_fldAccountNumber"]))
                {
                    col["new_fldAccountNumber"] = col["current_fldAccountNumber"];
                    accNo = col["new_fldAccountNumber"];
                }


                //If no change value
                if (col["new_fldAccountNumber"] == col["current_fldAccountNumber"])
                {
                    sqlUpdateInfo.Add("fldAccountNumber", col["current_fldAccountNumber"]);
                    accNo = col["current_fldAccountNumber"];
                }
                /*if (col["new_fldIssueChequeType"] == col["current_fldIssueChequeType"])
                {
                    sqlUpdateInfo.Add("fldIssueChequeType", col["current_fldIssueChequeType"]);
                }*/
                if (col["new_fldchequeserialno"] == col["current_fldchequeserialno"])
                {

                    sqlUpdateInfo.Add("fldChequeSerialNo", col["new_fldchequeserialno"]);
                    cheqNo = col["new_fldchequeserialno"];

                }
                if (col["new_fldtranscode"] == col["current_fldtranscode"])
                {
                    sqlUpdateInfo.Add("fldTransCode", col["current_fldtranscode"]);
                }
                if (col["new_fldIssueStateCode"] == col["current_fldIssueStateCode"])
                {
                    sqlUpdateInfo.Add("fldIssueStateCode", col["current_fldIssueStateCode"]);
                }
                if (col["new_fldIssueBankCode"] == col["current_fldIssueBankCode"])
                {
                    sqlUpdateInfo.Add("fldIssueBankCode", col["current_fldIssueBankCode"]);
                }
                if (col["new_fldIssueBranchCode"] == col["current_fldIssueBranchCode"])
                {
                    sqlUpdateInfo.Add("fldIssueBranchCode", col["current_fldIssueBranchCode"]);
                }
                //if (col["new_fldCheckDigit"] == col["current_fldCheckDigit"])
                //{
                //    sqlUpdateInfo.Add("fldCheckDigit", col["current_fldCheckDigit"]);
                //}
                /*if (col["new_fldIssuelocation"] == col["current_fldIssuelocation"])
                {
                    sqlUpdateInfo.Add("fldIssueLocation", col["current_fldIssuelocation"]);
                }*/
                //Begin query contruction based on changed value
                if (col["new_fldAccountNumber"] != col["current_fldAccountNumber"])
                {
                    sqlUpdateInfo.Add("fldAccountNumber", col["new_fldAccountNumber"]);
                    accNo = col["new_fldAccountNumber"];
                    sqlUpdateInfoStatus.Add("fldHostAccountNo", col["new_fldAccountNumber"]);
                    sqlUpdateInfoStatus.Add("fldOriHostAccountNo", col["current_fldHostAccountNo"]);
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriAccountNumber"))
                    //{
                    sqlUpdateInfo.Add("fldOriAccountNumber", col["current_fldAccountNumber"]);
                    //}
                    auditLogBefore += " [Account No] : " + col["current_fldAccountNumber"];
                    auditLogAfter += " [Account No] : " + col["new_fldAccountNumber"];
                    historyRemarks += "";
                    //historyRemarks += "| Account No. changed from " + col["current_fldAccountNumber"] + " to " + col["new_fldAccountNumber"] + ".";
                    changesCounter += 1;
                    modifiedField = "AccNo";
                }
                if (col["new_fldchequeserialno"] != col["current_fldchequeserialno"])
                {
                    sqlUpdateInfo.Add("fldChequeSerialNo", col["new_fldchequeserialno"]);
                    cheqNo = col["new_fldchequeserialno"];
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriChequeSerialNo"))
                    //{
                    sqlUpdateInfo.Add("fldOriChequeSerialNo", col["current_fldchequeserialno"]);
                    //}
                    auditLogBefore += " [Cheque No] : " + col["current_fldchequeserialno"];
                    auditLogAfter += " [Cheque No] : " + col["new_fldchequeserialno"];
                    //historyRemarks += "| Cheque No. changed from " + col["current_fldChequeSerialNo"] + " to " + col["new_fldChequeSerialNo"] + ".";
                    //historyRemarks += "";
                    changesCounter += 1;
                    //modifiedField = modifiedField + ",ChequeNo"; 
                }
                if (col["new_fldtranscode"] != col["current_fldtranscode"])
                {
                    sqlUpdateInfo.Add("fldTransCode", col["new_fldtranscode"]);
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriTransCode"))
                    //{
                    sqlUpdateInfo.Add("fldOriTransCode", col["current_fldtranscode"]);
                    //}
                    auditLogBefore += " [Trans Code] : " + col["current_fldtranscode"];
                    auditLogAfter += " [Trans Code] : " + col["new_fldtranscode"];
                    //historyRemarks += "| Trans Code changed from " + col["current_fldTransCode"] + " to " + col["new_fldTransCode"] + ".";
                    //historyRemarks += "";
                    changesCounter += 1;
                    //modifiedField = modifiedField + ",ChequeNo";

                }

                if (col["new_fldIssueStateCode"] != col["current_fldIssueStateCode"])
                {
                    sqlUpdateInfo.Add("fldIssueStateCode", col["new_fldIssueStateCode"]);
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueStateCode"))
                    //{
                    sqlUpdateInfo.Add("fldOriIssueStateCode", col["current_fldIssueStateCode"]);
                    //}
                    auditLogBefore += " [State Code] : " + col["current_fldIssueStateCode"];
                    auditLogAfter += " [State Code] : " + col["new_fldIssueStateCode"];
                    //historyRemarks += "| State Code changed from " + col["current_fldIssueStateCode"] + " to " + col["new_fldIssueStateCode"] + ".";
                    changesCounter += 1;
                }

                if (col["new_fldIssueBankCode"] != col["current_fldIssueBankCode"])
                {
                    sqlUpdateInfo.Add("fldIssueBankCode", col["new_fldIssueBankCode"]);
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueBankCode"))
                    //{
                    sqlUpdateInfo.Add("fldOriIssueBankCode", col["current_fldIssueBankCode"]);

                    //}
                    auditLogBefore += " [Bank Code] : " + col["current_fldIssueBankCode"];
                    auditLogAfter += " [Bank Code] : " + col["new_fldIssueBankCode"];
                    //historyRemarks += "| Bank Code changed from " + col["current_fldIssueBankCode"] + " to " + col["new_fldIssueBankCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldIssueBranchCode"] != col["current_fldIssueBranchCode"])
                {
                    sqlUpdateInfo.Add("fldIssueBranchCode", col["new_fldIssueBranchCode"]);
                    //Check if old field has value.. if not then add original value
                    //if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueBranchCode"))
                    //{
                    sqlUpdateInfo.Add("fldOriIssueBranchCode", col["current_fldIssueBranchCode"]);

                    //}
                    auditLogBefore += " [Branch Code] : " + col["current_fldIssueBranchCode"];
                    auditLogAfter += " [Branch Code] : " + col["new_fldIssueBranchCode"];
                    //historyRemarks += "| Branch Code changed from " + col["current_fldIssueBranchCode"] + " to " + col["new_fldIssueBranchCode"] + ".";
                    changesCounter += 1;
                }

                if (changesCounter == 0)
                {
                    historyRemarks += "Item has no changes.";
                }
                //Compulsory update for tblRejectReentryInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldInwardItemId", col["fldInwardItemId"]);
                //sqlUpdateInfo.Add("fldUpdateUserID", currentUser.UserId);
                //sqlUpdateInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

                //Compulsory update for tblInwardItemInfoStatus
                sqlUpdateInfoStatus.Add("fldAssignedUserId", DBNull.Value);
                //sqlUpdateInfoStatus.Add("fldRemarks", col["textAreaRemarks"]);
                sqlUpdateInfoStatus.Add("fldModifiedFields", modifiedField.Trim().TrimStart(','));
                //Compulsory update for tblInwardItemHistory
                sqlChequeHistory.Add("fldActionStatusId", nextHistorySecNo);
                sqlChequeHistory.Add("fldActionStatus", VerificationStatus.ACTION.RejectReentryMaker); //D - history for maker confirm
                sqlChequeHistory.Add("fldUIC", col["current_fldUIC"]);
                sqlChequeHistory.Add("fldInwardItemID", col["fldInwardItemId"]);
                sqlChequeHistory.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlChequeHistory.Add("fldRemarks", historyRemarks.TrimStart('|')); //trim for loop in front end
                //sqlChequeHistory.Add("fldTextAreaRemarks", col["textAreaRemarks"]);
                sqlChequeHistory.Add("fldVerifySeq", DBNull.Value);
                sqlChequeHistory.Add("fldCreateUserID", currentUser.UserId);
                sqlChequeHistory.Add("fldQueue", taskId);
                //Excute the command
                sqlUpdateInfoStatus.Add("fldRRstatus", 1); // status NULL is going to verification screen

                APIHostValidation aPIHostValidation = new APIHostValidation(dbContext);
                Dictionary<string, string> Messagevalue = aPIHostValidation.GETAPIReponse(col, accNo, cheqNo, amount);
                if (Messagevalue["CCUandB"] == "B")
                {


                    List<SqlParameter> sqlBranch = new List<SqlParameter>();
                    sqlBranch.Add(new SqlParameter("@_clearDate", Convert.ToDateTime(col["current_fldClearDate"]).ToString("dd-MMM-yyyy")));
                    sqlBranch.Add(new SqlParameter("@InwardItemID", col["current_fldInwardItemId1"]));
                    sqlBranch.Add(new SqlParameter("@ApprovalStatus", "B"));
                    sqlBranch.Add(new SqlParameter("@fldUpdateUserID", currentUser.UserId));
                    sqlBranch.Add(new SqlParameter("@fldInwardStatus", "BR"));
                    sqlBranch.Add(new SqlParameter("@fldRejectStatusMessage", "Balance:" + Messagevalue["Balance"].ToString() + ",NSF:" + Messagevalue["NSFStatus"] + ",CreditBlock:" + Messagevalue["CreditBlock"] + ",AccountClosed:" + Messagevalue["AccountClosed"] + ",StopPayment:" + Messagevalue["StopPayment"] + ",Dormant:" + Messagevalue["Dormant"] + ",OpenClose:" + Messagevalue["OpenClose"] + ",Frozen:" + Messagevalue["Frozen"] + ",Deceased:" + Messagevalue["Deceased"] + ",ResidentStatus:" + Messagevalue["ResidentStatus"] + ",ChequeUsedStatus:" + Messagevalue["ChequeUsedStatus"]));
                    sqlBranch.Add(new SqlParameter("@fldHostValidationHttpCode", Messagevalue["fldHostValidationHttpCode"]));
                    dbContext.GetRecordsAsDataTableSP("sp_TransferItemstoBranchandCCU", sqlBranch.ToArray());

                    dbContext.ConstructAndExecuteInsertCommand("tblRejectReentryInfo", sqlUpdateInfo);
                    sqlUpdateInfo.Remove("fldInwardItemId");

                    dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfo", sqlUpdateInfo, sqlCondition);
                    dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
                    dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

                    //Update sequence no
                    sequenceDao.UpdateSequenceNo(nextHistorySecNo, "tblInwardItemHistory");
                    //Add to audit trail
                    CurrentUser.Account.TaskId = taskId;
                    auditTrailDao.Log("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " Before Update=> " + auditLogBefore, CurrentUser.Account);
                    auditTrailDao.Log("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " After Update=> " + auditLogAfter, CurrentUser.Account);

                    //auditTrailDao.SecurityLog("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " Before Update=> " + auditLogBefore, "", taskId, CurrentUser.Account);
                    //auditTrailDao.SecurityLog("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " After Update=> " + auditLogAfter, "", taskId, CurrentUser.Account);
                    return "";


                }
                else if (Messagevalue["CCUandB"] == "CCU")
                {
                    List<SqlParameter> sqlCCU = new List<SqlParameter>();
                    sqlCCU.Add(new SqlParameter("@_clearDate", Convert.ToDateTime(col["current_fldClearDate"]).ToString("dd-MMM-yyyy")));
                    sqlCCU.Add(new SqlParameter("@InwardItemID", col["current_fldInwardItemId1"]));
                    sqlCCU.Add(new SqlParameter("@ApprovalStatus", ""));
                    sqlCCU.Add(new SqlParameter("@fldUpdateUserID", currentUser.UserId));
                    sqlCCU.Add(new SqlParameter("@fldInwardStatus", "CCU"));
                    sqlCCU.Add(new SqlParameter("@fldRejectStatusMessage", "Balance:" + Messagevalue["Balance"].ToString() + ",NSF:" + Messagevalue["NSFStatus"] + ",CreditBlock:" + Messagevalue["CreditBlock"] + ",AccountClosed:" + Messagevalue["AccountClosed"] + ",StopPayment:" + Messagevalue["StopPayment"] + ",Dormant:" + Messagevalue["Dormant"] + ",OpenClose:" + Messagevalue["OpenClose"] + ",Frozen:" + Messagevalue["Frozen"] + ",Deceased:" + Messagevalue["Deceased"] + ",ResidentStatus:" + Messagevalue["ResidentStatus"] + ",ChequeUsedStatus:" + Messagevalue["ChequeUsedStatus"]));
                    sqlCCU.Add(new SqlParameter("@fldHostValidationHttpCode", Messagevalue["fldHostValidationHttpCode"]));
                    dbContext.GetRecordsAsDataTableSP("sp_TransferItemstoBranchandCCU", sqlCCU.ToArray());


                    dbContext.ConstructAndExecuteInsertCommand("tblRejectReentryInfo", sqlUpdateInfo);
                    sqlUpdateInfo.Remove("fldInwardItemId");



                    dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfo", sqlUpdateInfo, sqlCondition);
                    dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
                    dbContext.ConstructAndExecuteInsertCommand("tblInwardItemHistory", sqlChequeHistory);

                    //Update sequence no
                    sequenceDao.UpdateSequenceNo(nextHistorySecNo, "tblInwardItemHistory");
                    //Add to audit trail
                    CurrentUser.Account.TaskId = taskId;
                    auditTrailDao.Log("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " Before Update=> " + auditLogBefore, CurrentUser.Account);
                    auditTrailDao.Log("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " After Update=> " + auditLogAfter, CurrentUser.Account);

                    //auditTrailDao.SecurityLog("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " Before Update=> " + auditLogBefore, "", taskId, CurrentUser.Account);
                    //auditTrailDao.SecurityLog("Edit Item Info - Item Id : " + col["fldInwardItemId"] + " After Update=> " + auditLogAfter, "", taskId, CurrentUser.Account);
                    return "";


                }
                else
                {   //For RR
                    List<SqlParameter> sqlCCU = new List<SqlParameter>();
                    sqlCCU.Add(new SqlParameter("@_clearDate", Convert.ToDateTime(col["current_fldClearDate"]).ToString("dd-MMM-yyyy")));
                    sqlCCU.Add(new SqlParameter("@InwardItemID", Convert.ToInt32(col["current_fldInwardItemId1"])));
                    sqlCCU.Add(new SqlParameter("@ApprovalStatus", ""));
                    sqlCCU.Add(new SqlParameter("@fldUpdateUserID", currentUser.UserId));
                    sqlCCU.Add(new SqlParameter("@fldInwardStatus", "RR"));
                    sqlCCU.Add(new SqlParameter("@fldRejectStatusMessage", "000014-No Record Found"));
                    sqlCCU.Add(new SqlParameter("@fldHostValidationHttpCode", Messagevalue["CCUandB"]));
                    dbContext.GetRecordsAsDataTableSP("sp_TransferItemstoBranchandCCU", sqlCCU.ToArray());
                    return "000014-No Record Found";

                }



            }
            catch (Exception ex)
            {

                throw ex;
            }
            return "";
        }





    }
}