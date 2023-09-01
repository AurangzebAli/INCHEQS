using INCHEQS.Areas.ICS.Models.Verification;
using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Sequence;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data;
using INCHEQS.Areas.ICS.Models.ICSAPIHostValidation;
using Microsoft.AspNet.SignalR.Hosting;
using System.Collections.ObjectModel;

namespace INCHEQS.Models.RejectReentry {
    public class RejectReentryDao : IRejectReentryDao {

        private readonly ApplicationDbContext dbContext;
        private readonly ICommonInwardItemDao commonInwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;

        public RejectReentryDao(ApplicationDbContext dbContext, ISequenceDao sequenceDao, ICommonInwardItemDao commonInwardItemDao, IAuditTrailDao auditTrailDao) {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.commonInwardItemDao = commonInwardItemDao;
            this.auditTrailDao = auditTrailDao;
        }


        public void CheckerConfirm(FormCollection col, AccountModel currentUser) {
            try {

                string historyRemarks = "Approved by " + currentUser.UserAbbr;
                int nextVerifySeq = commonInwardItemDao.GetNextVerifySeqNo(col["fldInwardItemId"]);

                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", col["fldInwardItemId"] } };

                //Compulsory update for tblInwardItemInfo
                //TODO : this process is update and confirmed by checker
				sqlUpdateInfo.Add("fldAccountNumber", col["current_fldAccountNumber"]);
                //sqlUpdateInfo.Add("fldIssueChequeType", col["current_fldIssueChequeType"]);
                //sqlUpdateInfo.Add("fldIssueLocation", col["current_fldIssuelocation"]);
                sqlUpdateInfo.Add("fldChequeSerialNo", col["current_fldChequeSerialNo"]);
                sqlUpdateInfo.Add("fldTransCode", col["current_fldTransCode"]);
                sqlUpdateInfo.Add("fldIssueStateCode", col["current_fldIssueStateCode"]);
                sqlUpdateInfo.Add("fldIssueBankCode", col["current_fldIssueBankCode"]);
                sqlUpdateInfo.Add("fldIssueBranchCode", col["current_fldIssueBranchCode"]);
                sqlUpdateInfo.Add("fldCheckDigit", col["current_fldCheckDigit"]);
                sqlUpdateInfo.Add("fldUpdateUserID", currentUser.UserId);
                sqlUpdateInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

  //If no change value
                if (string.IsNullOrEmpty(col["current_fldOriAccountNumber"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriAccountNumber", col["current_fldOriAccountNumber"]);
                }
                if (string.IsNullOrEmpty(col["current_fldOriCheckDigit"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriCheckDigit", col["current_fldOriCheckDigit"]);
                }
                if (string.IsNullOrEmpty(col["current_fldOriChequeSerialNo"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriChequeSerialNo", col["current_fldOriChequeSerialNo"]);
                }
                if (string.IsNullOrEmpty(col["current_fldOriIssueBankCode"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriIssueBankCode", col["current_fldOriIssueBankCode"]);
                }
                if (string.IsNullOrEmpty(col["current_fldOriIssueBranchCode"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriIssueBranchCode", col["current_fldOriIssueBranchCode"]);
                }
                if (col["current_fldOriIssueStateCode"] != "")
                {
                    sqlUpdateInfo.Add("fldOriIssueStateCode", col["current_fldOriIssueStateCode"]);
                }
                if (col["current_fldOriTransCode"] != "")
                {
                    sqlUpdateInfo.Add("fldOriTransCode", col["current_fldOriTransCode"]);
                }

                /*if (string.IsNullOrEmpty(col["current_fldOriIssueChequeType"])!= true)
                {
                    sqlUpdateInfo.Add("fldOriIssueChequeType", col["current_fldOriIssueChequeType"]);
                }*/
                /*if (string.IsNullOrEmpty(col["current_fldOriIssuelocation"]) != true)
                {
                    sqlUpdateInfo.Add("fldOriIssuelocation", col["current_fldOriIssuelocation"]);
                }*/

                //Compulsory update for tblInwardItemInfoStatus
                sqlUpdateInfoStatus.Add("fldRRstatus", 1); // status NULL is going to verification screen
                sqlUpdateInfoStatus.Add("fldAssignedUserId", DBNull.Value);
                sqlUpdateInfoStatus.Add("fldNonConfirmStatus", DBNull.Value);
                sqlUpdateInfoStatus.Add("fldRemarks", col["textAreaRemarks"]);

                //Account conversion
                //DataTable resultTable = new DataTable();
                //List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                //sqlParameterNext.Add(new SqlParameter("@10AccNo", col["current_fldAccountNumber"]));
                //sqlParameterNext.Add(new SqlParameter("@BranchCode", col["current_fldIssueStateCode"] + col["current_fldIssueBranchCode"]));
                //sqlParameterNext.Add(new SqlParameter("@NewHostAccountNumber", DBNull.Value));
                //resultTable =  dbContext.GetRecordsAsDataTableSP("sp_MBSBAccNoConversion", sqlParameterNext.ToArray());
                

                //foreach (DataRow row in resultTable.Rows)
                //{
                //    if (row["NewHostAccountNumber"].ToString() != "")
                //    {
                //        sqlUpdateInfoStatus.Add("fldHostAccountNo", row["NewHostAccountNumber"].ToString());
                //    }
                //}

                //Excute the command
                dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfo", sqlUpdateInfo, sqlCondition);
                dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);
                //delete record from tblRejectReentryInfo
                string inwarditemid = col["fldInwardItemId"].ToString();
                string sMySQL = "delete from  tblRejectReentryInfo where fldInwardItemId = @inwarditemid ;";
                dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@inwarditemid", inwarditemid) });
                
			}
             catch (Exception ex) {

                throw ex;
            }

        }
        public void CheckerRepair(FormCollection col, AccountModel currentUser) {
            try {
                string inwarditemid = col["fldInwardItemId"].ToString();
                string historyRemarks = "Rejected by " + currentUser.UserAbbr; //TODO: get real remarks
                int nextVerifySeq = commonInwardItemDao.GetNextVerifySeqNo(col["fldInwardItemId"]);
                int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", col["fldInwardItemId"] } };

               
                

                //Compulsory update for tblInwardItemInfo
                //TODO : this process is update and confirmed by checker
                sqlUpdateInfo.Add("fldUpdateUserID", currentUser.UserId);
                sqlUpdateInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

                
                
                //Compulsory update for tblInwardItemInfoStatus
                sqlUpdateInfoStatus.Add("fldRRstatus", DBNull.Value); // status 1 is going to Reject reentry maker
                sqlUpdateInfoStatus.Add("fldAssignedUserId", DBNull.Value);
                //sqlUpdateInfoStatus.Add("fldRemarks", col["textAreaRemarks"]);

                //Excute the command
     			//delete record from tblRejectReentryInfo
                
                string sMySQL = "delete from  tblRejectReentryInfo where fldInwardItemId = @inwarditemid ;";
                

                dbContext.ExecuteNonQuery(sMySQL, new[] { new SqlParameter("@inwarditemid", inwarditemid) });
               
                dbContext.ConstructAndExecuteUpdateCommand("tblInwardItemInfoStatus", sqlUpdateInfoStatus, sqlCondition);

            } catch (Exception ex) {

                throw ex;
            }

        }
        public void MakerConfirm(FormCollection col, AccountModel currentUser, string taskId) {
            try {
                string auditLogBefore = "";
                string auditLogAfter = "";
                string historyRemarks = "";
                int nextVerifySeq = commonInwardItemDao.GetNextVerifySeqNo(col["fldInwardItemId"]);
                int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int changesCounter = 0;

                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
            

                Dictionary<string, dynamic> sqlUpdateInfoStatus = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldInwardItemId", col["fldInwardItemId"] } };

                if (col["new_fldchequeserialno"] is null)
                {
                    col["new_fldchequeserialno"] = col["current_fldchequeserialno"];
                }
                if (col["new_fldtranscode"] is null)
                {
                    col["new_fldtranscode"] = col["current_fldtranscode"];
                }

                if (col["new_fldIssueBankCode"] is null)
                {
                    col["new_fldIssueBankCode"] = col["current_fldIssueBankCode"];
                }

                if (col["new_fldIssueStateCode"] is null)
                {
                    col["new_fldIssueStateCode"] = col["current_fldIssueStateCode"];
                }

                if (col["new_fldIssueBranchCode"] is null)
                {
                    col["new_fldIssueBranchCode"] = col["current_fldIssueBranchCode"];
                }


                if (col["new_fldAccountNumber"] is null)
                {
                    col["new_fldAccountNumber"] = col["current_fldAccountNumber"];
                }

                //If no change value
                //if (col["new_fldAccountNumber"] == col["current_fldAccountNumber"])
                //{
                //    sqlUpdateInfo.Add("fldAccountNumber", col["current_fldAccountNumber"]);
                //}
                /*if (col["new_fldIssueChequeType"] == col["current_fldIssueChequeType"])
                {
                    sqlUpdateInfo.Add("fldIssueChequeType", col["current_fldIssueChequeType"]);
                }*/
                if (col["new_fldChequeSerialNo"] == col["current_fldChequeSerialNo"])
                {
                    sqlUpdateInfo.Add("fldChequeSerialNo", col["current_fldChequeSerialNo"]);
                }
                if (col["new_fldTransCode"] == col["current_fldTransCode"])
                {
                    sqlUpdateInfo.Add("fldTransCode", col["current_fldTransCode"]);
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
                /* if (col["new_fldIssuelocation"] == col["current_fldIssuelocation"])
                 {
                     sqlUpdateInfo.Add("fldIssueLocation", col["current_fldIssuelocation"]);
                 }*/
                //Begin query contruction based on changed value
                if (col["new_fldAccountNumber"] != col["current_fldAccountNumber"]) {
                    sqlUpdateInfo.Add("fldAccountNumber", col["new_fldAccountNumber"]);
                    //Check if old field has value.. if not then add original value
                    if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriAccountNumber")) {
                        sqlUpdateInfo.Add("fldOriAccountNumber", col["current_fldAccountNumber"]);
                    }
                    auditLogBefore += " [Account No] : " + col["current_fldAccountNumber"];
                    auditLogAfter += " [Account No] : " + col["new_fldAccountNumber"];
                    historyRemarks += "| Account No. changed from " + col["current_fldAccountNumber"] + " to " + col["new_fldAccountNumber"] + ".";
                    changesCounter += 1;
                } if (col["new_fldChequeSerialNo"] != col["current_fldChequeSerialNo"]) {
                    sqlUpdateInfo.Add("fldChequeSerialNo", col["new_fldChequeSerialNo"]);
                    //Check if old field has value.. if not then add original value
                    if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriChequeSerialNo"))
                    {
                        sqlUpdateInfo.Add("fldOriChequeSerialNo", col["current_fldChequeSerialNo"]);
                    }
                    auditLogBefore += " [Cheque No] : " + col["current_fldChequeSerialNo"];
                    auditLogAfter += " [Cheque No] : " + col["new_fldChequeSerialNo"];
                    historyRemarks += "| Cheque No. changed from " + col["current_fldChequeSerialNo"] + " to " + col["new_fldChequeSerialNo"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldTransCode"] != col["current_fldTransCode"]) {
                     sqlUpdateInfo.Add("fldTransCode", col["new_fldTransCode"]);
                     //Check if old field has value.. if not then add original value
                     if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriTransCode"))
                     {
                         sqlUpdateInfo.Add("fldOriTransCode", col["current_fldTransCode"]);
                     }
                     auditLogBefore += " [Trans Code] : " + col["current_fldTransCode"];
                     auditLogAfter += " [Trans Code] : " + col["new_fldTransCode"];
                     //historyRemarks += "| Trans Code changed from " + col["current_fldTransCode"] + " to " + col["new_fldTransCode"] + ".";
                     changesCounter += 1;
                 }

                if (col["new_fldIssueStateCode"] != col["current_fldIssueStateCode"]) {
                    sqlUpdateInfo.Add("fldIssueStateCode", col["new_fldIssueStateCode"]);
                    //Check if old field has value.. if not then add original value
                    if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueStateCode")) {
                        sqlUpdateInfo.Add("fldOriIssueStateCode", col["current_fldIssueStateCode"]);
                    }
                    auditLogBefore += " [State Code] : " + col["current_fldIssueStateCode"];
                    auditLogAfter += " [State Code] : " + col["new_fldIssueStateCode"];
                    //historyRemarks += "| State Code changed from " + col["current_fldIssueStateCode"] + " to " + col["new_fldIssueStateCode"] + ".";
                    changesCounter += 1;
                }

                if (col["new_fldIssueBankCode"] != col["current_fldIssueBankCode"]) {
                    sqlUpdateInfo.Add("fldIssueBankCode", col["new_fldIssueBankCode"]);
                    //Check if old field has value.. if not then add original value
                    if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueBankCode")) {
                        sqlUpdateInfo.Add("fldOriIssueBankCode", col["current_fldIssueBankCode"]);
                    }
                    auditLogBefore += " [Bank Code] : " + col["current_fldIssueBankCode"];
                    auditLogAfter += " [Bank Code] : " + col["new_fldIssueBankCode"];
                    //historyRemarks += "| Bank Code changed from " + col["current_fldIssueBankCode"] + " to " + col["new_fldIssueBankCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldIssueBranchCode"] != col["current_fldIssueBranchCode"]) {
                    sqlUpdateInfo.Add("fldIssueBranchCode", col["new_fldIssueBranchCode"]);
                    //Check if old field has value.. if not then add original value
                    if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriIssueBranchCode")) {
                        sqlUpdateInfo.Add("fldOriIssueBranchCode", col["current_fldIssueBranchCode"]);
                    }
                    auditLogBefore += " [Branch Code] : " + col["current_fldIssueBranchCode"];
                    auditLogAfter += " [Branch Code] : " + col["new_fldIssueBranchCode"];
                    //historyRemarks += "| Branch Code changed from " + col["current_fldIssueBranchCode"] + " to " + col["new_fldIssueBranchCode"] + ".";
                    changesCounter += 1;
                }
 				//if (col["new_fldCheckDigit"] != col["current_fldCheckDigit"]) {
     //               sqlUpdateInfo.Add("fldCheckDigit", col["new_fldCheckDigit"]);
     //               //Check if old field has value.. if not then add original value
     //               if (commonInwardItemDao.CheckOldValueRejectReentry(col["fldInwardItemId"], "fldOriCheckDigit")) {
     //                   sqlUpdateInfo.Add("fldOriCheckDigit", col["current_fldCheckDigit"]);
     //               }
     //               auditLogBefore += " [Check Digit] : " + col["current_fldCheckDigit"];
     //               auditLogAfter += " [Check Digit] : " + col["new_fldCheckDigit"];
     //               //historyRemarks += "| Check Digit changed from " + col["current_fldCheckDigit"] + " to " + col["new_fldCheckDigit"] + ".";
     //               changesCounter += 1;
     //           }
                
                if (changesCounter == 0) {
                    historyRemarks += "Item has no changes.";
                }
                //Compulsory update for tblRejectReentryInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldInwardItemId", col["fldInwardItemId"]);
                sqlUpdateInfo.Add("fldUpdateUserID", currentUser.UserId);
                sqlUpdateInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

                //Compulsory update for tblInwardItemInfoStatus
                sqlUpdateInfoStatus.Add("fldRRstatus", 0); // status 0 is going to Reject reentry checker
                sqlUpdateInfoStatus.Add("fldAssignedUserId", DBNull.Value);
                //sqlUpdateInfoStatus.Add("fldRemarks", col["textAreaRemarks"]);

                //Compulsory update for tblInwardItemHistory
                sqlChequeHistory.Add("fldActionStatusId", nextHistorySecNo);
                sqlChequeHistory.Add("fldActionStatus", VerificationStatus.ACTION.RejectReentryMaker); //D - history for maker confirm
                sqlChequeHistory.Add("fldUIC", col["current_fldUIC"]);
                sqlChequeHistory.Add("fldInwardItemID", col["fldInwardItemId"]);
                sqlChequeHistory.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                sqlChequeHistory.Add("fldRemarks", historyRemarks.TrimStart('|')); //trim for loop in front end
                //sqlChequeHistory.Add("fldTextAreaRemarks", col["textAreaRemarks"]);
                sqlChequeHistory.Add("fldVerifySeq", nextVerifySeq);
                sqlChequeHistory.Add("fldCreateUserID", currentUser.UserId);
                sqlChequeHistory.Add("fldQueue", taskId);

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblRejectReentryInfo", sqlUpdateInfo);
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
                //@fldTransCode
                //@fldIssueStateCode 
                //@fldIssueBankCode
            }
            catch (Exception ex) {

                throw ex;
            }

        }

        public string CheckerConfirmNew(FormCollection col, AccountModel currentUser, string taskId,string Message)
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

                if ( string.IsNullOrEmpty(col["new_fldtranscode"]))
                {
                    col["new_fldtranscode"] = col["current_fldtranscode"];
                }

                if  (string.IsNullOrEmpty(col["new_fldIssueBankCode"]) )
                {
                    col["new_fldIssueBankCode"] = col["current_fldIssueBankCode"];
                }

                if (string.IsNullOrEmpty(col["new_fldIssueStateCode"] ))
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
                Dictionary<string, string> Messagevalue = aPIHostValidation.GETAPIReponse(col, accNo, cheqNo,amount);
                if (Messagevalue["CCUandB"] == "B")
                {
                    

                    List<SqlParameter> sqlBranch = new List<SqlParameter>();
                    sqlBranch.Add(new SqlParameter("@_clearDate", Convert.ToDateTime(col["current_fldClearDate"]).ToString("dd-MMM-yyyy")));
                    sqlBranch.Add(new SqlParameter("@InwardItemID", col["current_fldInwardItemId1"]));
                    sqlBranch.Add(new SqlParameter("@ApprovalStatus", "B"));
                    sqlBranch.Add(new SqlParameter("@fldUpdateUserID", currentUser.UserId));
                    sqlBranch.Add(new SqlParameter("@fldInwardStatus", "BR"));
                    sqlBranch.Add(new SqlParameter("@fldRejectStatusMessage", "Balance:" + Messagevalue["Balance"].ToString() + ",NSF:" + Messagevalue["NSFStatus"] + ",CreditBlock:" + Messagevalue["CreditBlock"] + ",AccountClosed:" + Messagevalue["AccountClosed"] + ",StopPayment:" + Messagevalue["StopPayment"] + ",Dormant:" + Messagevalue["Dormant"] + ",OpenClose:" + Messagevalue["OpenClose"] + ",Frozen:" + Messagevalue["Frozen"] + ",Deceased:" + Messagevalue["Deceased"] +",ResidentStatus:" + Messagevalue["ResidentStatus"] + ",ChequeUsedStatus:" + Messagevalue["ChequeUsedStatus"]));
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
                else if(Messagevalue["CCUandB"] == "CCU")
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

        public List<string> Validate(FormCollection col, AccountModel currentUser) {
            List<string> err = new List<string>();


            //if (commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"], col["fldUpdateTimestamp"])) {
            //    err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            //}
            bool CheckRecordUpdatedOrDeleted;

            string DataAction = col["DataAction"].ToString().Trim();

            if (DataAction == "ChequeVerificationPage")
            {
                CheckRecordUpdatedOrDeleted = false;
            }
            else
            {
                CheckRecordUpdatedOrDeleted = commonInwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldInwardItemId"]);
            }

            if (CheckRecordUpdatedOrDeleted)
            {
                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }
            else
            {
                //Force initiate with null check, because this is shared validation
                if (col["new_fldChequeSerialNo"] != null )
                {

                    //if (col["new_fldChequeSerialNo"].Length != 6)
                    //{

                    //    err.Add("Cheque Number must be 6 digits");

                    //}
                    if (Regex.IsMatch(col["new_fldChequeSerialNo"].ToString(), "[^0-9]"))
                    {

                        err.Add("Input of Cheque Number must be numeric only");

                    }
                    //else if (col["new_fldChequeSerialNo"].ToString().Equals("000000"))
                    //{
                    //    err.Add("Invalid Cheque Number. Please enter correct Cheque Number");

                    //}
                    //else if (col["new_fldChequeSerialNo"].ToString().Equals(""))
                    //{
                    //    err.Add("Cheque Number must be 6 digits");
                    //}
                }

                if (col["new_fldAccountNumber"] != null)
                {
                    if (col["new_fldAccountNumber"].ToString().Equals("0000000000"))
                    {
                        err.Add("Invalid Issuer Account Number. Please enter correct Issuer Account Number");

                    }
                    //else if (col["new_fldAccountNumber"].Length != 10)
                    //{
                    //    err.Add("Issuer Account Number must be 10 digits");

                    //}
                    else if (Regex.IsMatch(col["new_fldAccountNumber"].ToString(), "[^0-9]"))
                    {
                        err.Add(Locale.InputofAccountNumbermustbenumericonly);
                    }

                }
                //if (string.IsNullOrEmpty(col["new_fldAccountNumber"]))
                //{

                //    err.Add("Invalid Account Number");
                //}
                //if (string.IsNullOrEmpty(col["new_fldChequeSerialNo"]))
                //{

                //    err.Add("Invalid Cheque Number");
                //}

                //if (string.IsNullOrEmpty(col["new_fldtranscode"]))
                //{
                //    err.Add("Invalid TC");

                //}

            }
            return err;
        }

        //tukar ke Sp
 		public List<string> VerificationBranchCode(String BranchCode, String BankCode)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", BranchCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", BankCode));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterByBranchCode", sqlParameterNext.ToArray());
            List<string> branchCodeAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                branchCodeAvailable.Add(row["fldCBranchCode"].ToString());
                branchCodeAvailable.Add(row["fldIBranchCode"].ToString());
            }
            return branchCodeAvailable;
            //return null;
        }

        public bool CheckValidateBankCode(string bankcode)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckValidateChequeType(string chequetype)
        {
            string sql = "Select fldChequeTypeCode FROM tblChequeTypeMaster where fldActive = 'Y' and fldChequeTypeCode = @chequetype";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@chequetype", chequetype) }))
            {
                return true;
            }
            return false;
        }

        public bool CheckValidateLocationCode(string locationcode, string branchcode)
        {
            string sql = "Select fldLocationCode FROM tblInternalBranchMaster where fldActive = 'Y' and fldLocationCode = @locationcode and fldBranchCode = @branchcode";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@locationcode", locationcode), new SqlParameter("@branchcode", branchcode) }))
            {
                return true;
            }
            return false;
        }

        public bool CheckValidateBranchCode(string branchcode)
        {
            string sql = "Select fldBranchCode FROM tblInternalBranchMaster where fldActive = 'Y' and fldBranchCode = @branchcode";
            if (!dbContext.CheckExist(sql, new[] { new SqlParameter("@branchcode", branchcode) }))
            {
                return true;
            }
            return false;
        }
    }
}