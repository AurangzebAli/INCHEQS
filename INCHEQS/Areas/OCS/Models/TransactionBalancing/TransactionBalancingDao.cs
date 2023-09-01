using INCHEQS.Areas.OCS.Models.Clearing;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Common;
using INCHEQS.Common.Resources;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Models.Sequence;
using INCHEQS.Security.Account;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.Balancing
{
    public class TransactionBalancingDao : ITransactionBalancingDao
    {
        private readonly OCSDbContext dbContext;
        private readonly ICommonOutwardItemDao commonOutwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IClearingDao clearingDao;

        public TransactionBalancingDao(OCSDbContext dbContext, ISequenceDao sequenceDao, ICommonOutwardItemDao commonOutwardItemDao, IAuditTrailDao auditTrailDao, IClearingDao clearingDao)
        {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.commonOutwardItemDao = commonOutwardItemDao;
            this.auditTrailDao = auditTrailDao;
            this.clearingDao = clearingDao;
        }

        
        public void DoBalancing(FormCollection collection, AccountModel currentUser, string taskId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(collection["htoUpdateBalancing"].Trim()))
                {
                    if (collection["htoUpdateBalancing"].Contains("|"))
                    {
                        string[] TotalValues = collection["htoUpdateBalancing"].Split('|');
                        foreach (var item in TotalValues)
                        {
                            string[] ParamValues = item.Split(',');
                            UpdateBalancingAmount(collection, currentUser, ParamValues);
                        }
                        updateBalancingStatus(collection, currentUser);
                    }
                    else
                    {
                        string[] ParamValues = collection["htoUpdateBalancing"].Split(',');
                        UpdateBalancingAmount(collection, currentUser, ParamValues);
                        updateBalancingStatus(collection, currentUser);
                    }

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void Confirm(FormCollection col, AccountModel currentUser, string taskId)
        {
            try
            {
                string auditLogBefore = "";
                string auditLogAfter = "";
                string historyRemarks = "";
                string outwardRemarks = "";
                //int nextVerifySeq = commonOutwardItemDao.GetNextVerifySeqNo(col["fldItemId"]);
                //int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int changesCounter = 0;

                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldItemId", col["current_fldItemID"] } };
                //If no change value

                if (col["new_fldIssuerAccNo"] == col["current_fldIssuerAccNo"])
                {
                    sqlUpdateInfo.Add("fldIssuerAccNo", col["current_fldIssuerAccNo"]);
                    sqlChequeHistory.Add("fldIssuerAccNo", col["current_fldIssuerAccNo"]);
                    auditLogBefore += " [fldIssuerAccNo] : " + col["current_fldIssuerAccNo"];
                    auditLogAfter += " [fldIssuerAccNo] : " + col["new_fldIssuerAccNo"];
                    historyRemarks += "| fldIssuerAccNo. changed from " + col["current_fldIssuerAccNo"] + " to " + col["new_fldIssuerAccNo"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldSerial"] == col["current_fldSerial"])
                {
                    sqlUpdateInfo.Add("fldSerial", col["current_fldSerial"]);
                    sqlChequeHistory.Add("fldSerial", col["current_fldSerial"]);
                    auditLogBefore += " [fldSerial] : " + col["current_fldSerial"];
                    auditLogAfter += " [fldSerial] : " + col["new_fldSerial"];
                    historyRemarks += "| fldSerial. changed from " + col["current_fldSerial"] + " to " + col["new_fldSerial"] + ".";
                    changesCounter += 1;
                }

                if (col["new_fldTCCode"] == col["current_fldTCCode"])
                {
                    sqlUpdateInfo.Add("fldTCCode", col["current_fldTCCode"]);
                    sqlChequeHistory.Add("fldTCCode", col["current_fldTCCode"]);
                }
                if (col["new_fldstatecode"] == col["current_fldstatecode"])
                {
                    sqlUpdateInfo.Add("fldStateCode", col["current_fldStateCode"]);
                    sqlChequeHistory.Add("fldStateCode", col["current_fldStateCode"]);
                    auditLogBefore += " [fldStateCode] : " + col["current_fldStateCode"];
                    auditLogAfter += " [fldStateCode] : " + col["new_fldStateCode"];
                    historyRemarks += "| fldStateCode. changed from " + col["current_fldStateCode"] + " to " + col["new_fldBankCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldBankCode"] == col["current_fldBankCode"])
                {
                    sqlUpdateInfo.Add("fldBankCode", col["current_fldBankCode"]);
                    sqlChequeHistory.Add("fldBankCode", col["current_fldBankCode"]);
                    auditLogBefore += " [fldBankCode] : " + col["current_fldBankCode"];
                    auditLogAfter += " [fldBankCode] : " + col["new_fldBankCode"];
                    historyRemarks += "| fldBankCode. changed from " + col["current_fldBankCode"] + " to " + col["new_fldBankCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldbranchcode"] == col["current_fldbranchcode"])
                {
                    sqlUpdateInfo.Add("fldBranchCode", col["current_fldBranchCode"]);
                    sqlChequeHistory.Add("fldBranchCode", col["current_fldBranchCode"]);
                    auditLogBefore += " [fldBranchCode] : " + col["current_fldBranchCode"];
                    auditLogAfter += " [fldBranchCode] : " + col["new_fldBranchCode"];
                    historyRemarks += "| fldBranchCode. changed from " + col["current_fldBranchCode"] + " to " + col["new_fldBranchCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldCheckDigit"] == col["current_fldCheckDigit"])
                {
                    sqlUpdateInfo.Add("fldCheckDigit", col["current_fldCheckDigit"]);
                    sqlChequeHistory.Add("fldCheckDigit", col["current_fldCheckDigit"]);
                    auditLogBefore += " [fldCheckDigit] : " + col["current_fldCheckDigit"];
                    auditLogAfter += " [fldCheckDigit] : " + col["new_fldCheckDigit"];
                    historyRemarks += "| fldCheckDigit. changed from " + col["current_fldCheckDigit"] + " to " + col["new_fldCheckDigit"] + ".";
                    changesCounter += 1;
                }
                //if (col["new_fldAmount"] == col["current_fldAmount"] && col["current_fldAmount"] != null && col["current_fldAmount"] != "" && col["txtChequeAmount"] == col["current_fldamount"])
                //{
                //    sqlUpdateInfo.Add("fldAmount", Convert.ToInt64(col["current_fldAmount"].Trim().Replace(".", "").Replace(",", "")));
                //    sqlChequeHistory.Add("fldAmount", Convert.ToInt64(col["current_fldAmount"].Trim().Replace(".", "").Replace(",", "")));
                //}
                if (col["new_fldType"] == col["current_fldType"])
                {
                    sqlUpdateInfo.Add("fldType", col["current_fldType"]);
                    sqlChequeHistory.Add("fldType", col["current_fldType"]);
                    auditLogBefore += " [fldType] : " + col["current_fldType"];
                    auditLogAfter += " [fldType] : " + col["new_fldType"];
                    historyRemarks += "| fldType. changed from " + col["current_fldType"] + " to " + col["new_fldType"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldLocation"] == col["current_fldLocation"])
                {
                    sqlUpdateInfo.Add("fldLocation", col["current_fldLocation"]);
                    sqlChequeHistory.Add("fldLocation", col["current_fldLocation"]);
                    auditLogBefore += " [Location] : " + col["current_fldLocation"];
                    auditLogAfter += " [Location] : " + col["new_fldLocation"];
                    historyRemarks += "| Location. changed from " + col["current_fldLocation"] + " to " + col["new_fldLocation"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldclearingstatus"] == col["current_fldclearingstatus"])
                {
                    sqlUpdateInfo.Add("fldclearingstatus", col["current_fldclearingstatus"]);
                    sqlChequeHistory.Add("fldclearingstatus", col["current_fldclearingstatus"]);
                }
                //Begin query contruction based on changed value
                if (col["new_fldIssuerAccNo"] != col["current_fldIssuerAccNo"])
                {
                    sqlUpdateInfo.Add("fldIssuerAccNo", col["new_fldIssuerAccNo"]);
                    sqlChequeHistory.Add("fldOriIssuerAccNo", col["current_fldIssuerAccNo"]);
                    sqlChequeHistory.Add("fldIssuerAccNo", col["new_fldIssuerAccNo"]);

                    auditLogBefore += " [Account No] : " + col["current_fldIssuerAccNo"];
                    auditLogAfter += " [Account No] : " + col["new_fldIssuerAccNo"];
                    historyRemarks += "| Account No. changed from " + col["current_fldIssuerAccNo"] + " to " + col["new_fldIssuerAccNo"] + ".";
                    changesCounter += 1;

                }
                if (col["new_fldSerial"] != col["current_fldSerial"])
                {
                    sqlUpdateInfo.Add("fldSerial", col["new_fldSerial"]);
                    sqlChequeHistory.Add("fldOriSerial", col["current_fldSerial"]);
                    sqlChequeHistory.Add("fldSerial", col["new_fldSerial"]);

                    auditLogBefore += " [Cheque No] : " + col["current_fldSerial"];
                    auditLogAfter += " [Cheque No] : " + col["new_fldSerial"];
                    historyRemarks += "| Cheque No. changed from " + col["current_fldSerial"] + " to " + col["new_fldSerial"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldTCCode"] != col["current_fldTCCode"])
                {
                    sqlUpdateInfo.Add("fldTCCode", col["new_fldTCCode"]);

                    sqlChequeHistory.Add("fldOriTCCode", col["current_fldTCCode"]);

                    sqlChequeHistory.Add("fldTCCode", col["new_fldTCCode"]);

                    auditLogBefore += " [Trans Code] : " + col["current_fldTCCode"];
                    auditLogAfter += " [Trans Code] : " + col["new_fldTCCode"];
                    historyRemarks += "| Trans Code changed from " + col["current_fldTCCode"] + " to " + col["new_fldTCCode"] + ".";
                    changesCounter += 1;
                }

                if (col["new_fldStateCode"] != col["current_fldStateCode"])
                {
                    sqlUpdateInfo.Add("fldStateCode", col["new_fldStateCode"]);
                    sqlChequeHistory.Add("fldOriStateCode", col["current_fldStateCode"]);
                    sqlChequeHistory.Add("fldStateCode", col["new_fldStateCode"]);

                    auditLogBefore += " [State Code] : " + col["current_fldStateCode"];
                    auditLogAfter += " [State Code] : " + col["new_fldStateCode"];
                    historyRemarks += "| State Code changed from " + col["current_fldStateCode"] + " to " + col["new_fldStateCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldBankCode"] != col["current_fldBankCode"])
                {
                    sqlUpdateInfo.Add("fldBankCode", col["new_fldBankCode"]);
                    sqlChequeHistory.Add("fldOriBankCode", col["current_fldBankCode"]);
                    sqlChequeHistory.Add("fldBankCode", col["new_fldBankCode"]);

                    auditLogBefore += " [Bank Code] : " + col["current_fldBankCode"];
                    auditLogAfter += " [Bank Code] : " + col["new_fldBankCode"];
                    historyRemarks += "| Bank Code changed from " + col["current_fldBankCode"] + " to " + col["new_fldbankcode"] + ".";
                    changesCounter += 1;
                }

                if (col["new_fldBranchCode"] != col["current_fldBranchCode"])
                {
                    sqlUpdateInfo.Add("fldBranchCode", col["new_fldBranchCode"]);
                    sqlChequeHistory.Add("fldOriBranchCode", col["current_fldBranchCode"]);
                    sqlChequeHistory.Add("fldBranchCode", col["new_fldBranchCode"]);

                    auditLogBefore += " [Branch Code] : " + col["current_fldBranchCode"];
                    auditLogAfter += " [Branch Code] : " + col["new_fldBranchCode"];
                    historyRemarks += "| Branch Code changed from " + col["current_fldBranchCode"] + " to " + col["new_fldBranchCode"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldCheckDigit"] != col["current_fldCheckDigit"])
                {
                    sqlUpdateInfo.Add("fldCheckDigit", col["new_fldCheckDigit"]);
                    sqlChequeHistory.Add("fldOriCheckDigit", col["current_fldCheckDigit"]);
                    sqlChequeHistory.Add("fldCheckDigit", col["new_fldCheckDigit"]);

                    auditLogBefore += " [Check Digit] : " + col["current_fldCheckDigit"];
                    auditLogAfter += " [Check Digit] : " + col["new_fldCheckDigit"];
                    historyRemarks += "| Check Digit changed from " + col["current_fldCheckDigit"] + " to " + col["new_fldCheckDigit"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldType"] != col["current_fldType"])
                {
                    sqlUpdateInfo.Add("fldType", col["new_fldType"]);
                    sqlChequeHistory.Add("fldOriType", col["current_fldType"]);
                    sqlChequeHistory.Add("fldType", col["new_fldType"]);

                    auditLogBefore += " [Type] : " + col["current_fldType"];
                    auditLogAfter += " [Type] : " + col["new_fldType"];
                    historyRemarks += "| Type changed from " + col["current_fldType"] + " to " + col["new_fldType"] + ".";
                    changesCounter += 1;
                }
                if (col["new_fldLocation"] != col["current_fldLocation"])
                {
                    sqlUpdateInfo.Add("fldLocation", col["new_fldLocation"]);
                    sqlChequeHistory.Add("fldOriLocation", col["current_fldLocation"]);
                    sqlChequeHistory.Add("fldLocation", col["new_fldLocation"]);

                    auditLogBefore += " [Location] : " + col["current_fldLocation"];
                    auditLogAfter += " [Location] : " + col["new_fldLocation"];
                    historyRemarks += "| Location changed from " + col["current_fldLocation"] + " to " + col["new_fldLocation"] + ".";
                    changesCounter += 1;
                }
                //if (col["txtChequeAmount"] != col["current_fldAmount"])
                //{
                //    sqlUpdateInfo.Add("fldAmount", Convert.ToInt64(col["txtChequeAmount"].Trim().Replace(".", "").Replace(",", "")));
                //    sqlChequeHistory.Add("fldAmount", Convert.ToInt64(col["txtChequeAmount"].Trim().Replace(".", "").Replace(",", "")));

                //    auditLogBefore += " [Amount] : " + col["current_fldAmount"];
                //    auditLogAfter += " [Amount] : " + col["txtChequeAmount"];
                //    historyRemarks += "| Amount changed from " + col["current_fldAmount"] + " to " + col["txtChequeAmount"] + ".";
                //    changesCounter += 1;
                //}
                if (changesCounter == 0)
                {
                    historyRemarks += "Item has no changes.";
                }
                //Compulsory update for tblItemInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));
                sqlUpdateInfo.Add("fldupdatetimestamp", DateUtils.GetCurrentDatetimeForSql());
                sqlUpdateInfo.Add("fldreasoncode", "");
                sqlChequeHistory.Add("fldreasoncode", "");
                //sqlUpdateInfo.Add("fldItemType", "V");
                sqlUpdateInfo.Add("fldremark", col["textAreaRemarks"]);

                //Compulsory update for tblMICRRepair
                sqlChequeHistory.Add("flditemid", Convert.ToInt64(col["current_flditemid"]));
                //sqlChequeHistory.Add("flditemtype", "V");
                sqlChequeHistory.Add("fldremark", col["textAreaRemarks"]);
                sqlChequeHistory.Add("fldoriremark", col["current_fldremark"]);
                //sqlChequeHistory.Add("fldcreatetimestamp", Convert.ToDateTime(col["current_fldcreatetimestamp"]));
                sqlChequeHistory.Add("fldcreatetimestamp", DateTime.Now);
                sqlChequeHistory.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblmicrrepair", sqlChequeHistory);
                dbContext.ConstructAndExecuteUpdateCommand("tbliteminfo", sqlUpdateInfo, sqlCondition);

                //Add to audit trail
                //auditTrailDao.Log("Balancing - Edit Item Info - Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore, currentUser);
               // auditTrailDao.Log("Balancing - Edit Item Info - Item Id : " + col["current_flditemid"] + " After Update=> " + auditLogAfter, currentUser);
                //outwardRemarks = "Balancing - Item has been Confirmed with following Information Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore + " After Update=> " + auditLogAfter;
               // commonOutwardItemDao.AddOutwardItemHistory("C", col["current_fldtransno"], outwardRemarks, taskId, currentUser);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void Reject(FormCollection col, AccountModel currentUser, string taskId)
        {
            try
            {
                string auditLogBefore = "";
                string auditLogAfter = "";
                string historyRemarks = "";
                string outwardRemarks = "Cheque Rejected";
                //int nextVerifySeq = commonOutwardItemDao.GetNextVerifySeqNo(col["fldItemId"]);
                //int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int changesCounter = 0;


                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldTransNo", Convert.ToInt64(col["current_fldtransno"]) } };

                sqlChequeHistory.Add("fldissueraccno", col["current_fldissueraccno"]);
                sqlChequeHistory.Add("fldserial", col["current_fldserial"]);
                sqlChequeHistory.Add("fldtccode", col["current_fldtccode"]);
                sqlChequeHistory.Add("fldstatecode", col["current_fldstatecode"]);
                sqlChequeHistory.Add("fldbankcode", col["current_fldbankcode"]);
                sqlChequeHistory.Add("fldbranchcode", col["current_fldbranchcode"]);
                sqlChequeHistory.Add("fldcheckdigit", col["current_fldcheckdigit"]);
                if (col["current_fldamount"] != "")
                {
                    sqlChequeHistory.Add("fldamount", Convert.ToDouble(col["current_fldamount"]));
                }
                if (col["current_fldpvaccno"] != "")
                {
                    sqlChequeHistory.Add("fldpvaccno", col["current_fldpvaccno"]);
                }
                sqlChequeHistory.Add("fldtype", col["current_fldtype"]);
                sqlChequeHistory.Add("fldlocation", col["current_fldlocation"]);
                sqlChequeHistory.Add("fldcreatetimestamp", Convert.ToDateTime(col["current_fldcreatetimestamp"]));
                sqlChequeHistory.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));
                sqlChequeHistory.Add("fldreasoncode", "430");
                sqlChequeHistory.Add("flditemid", Convert.ToInt64(col["current_flditemid"]));

                sqlUpdateInfo.Add("fldreasoncode", "430");
                sqlUpdateInfo.Add("fldIntRejCode", "430");

                auditLogBefore += " [Reason Code] : " + "";
                auditLogAfter += " [Reason Code] : " + "430";
                historyRemarks += "| Reason Code changed from " + "" + " to " + "430" + ".";

                //Compulsory update for tblItemInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldUpdateUserId", currentUser.UserId);
                sqlUpdateInfo.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
                //sqlUpdateInfo.Add("fldItemType", "V");
                sqlUpdateInfo.Add("fldRemark", col["current_fldremark"]);

                if (!string.IsNullOrEmpty(col["current_fldremark"]))
                {
                    auditLogAfter += " [Remarks] : " + col["current_fldremark"];
                }
                //Compulsory update for tblMICRRepair
                //sqlChequeHistory.Add("fldItemId",col["current_fldItemID"]));
                ////sqlChequeHistory.Add("fldItemType", "V");
                //sqlChequeHistory.Add("fldRemark", col["textAreaRemarks"]);
                //sqlChequeHistory.Add("fldOriRemark", col["current_fldRemark"]);
                //sqlChequeHistory.Add("fldCreateTimeStamp", Convert.ToDateTime(col["current_fldCreateTimeStamp"]));

                //Excute the command
                dbContext.ConstructAndExecuteUpdateCommand("tblItemInfo", sqlUpdateInfo, sqlCondition);
                dbContext.ConstructAndExecuteInsertCommand("tblMICRrepair", sqlChequeHistory);
                //Add to audit trail
                //auditTrailDao.Log("Balancing - Edit Item Info - Item Id : " + col["current_fldItemID"] + " Before Update=> " + auditLogBefore, currentUser);
                //auditTrailDao.Log("Balancing - Edit Item Info - Item Id : " + col["current_fldItemID"] + " After Update=> " + auditLogAfter, currentUser);
               // outwardRemarks = "Balancing - Item has been Rejected. Trans No: " + col["current_fldtransno"] + " Before Update=> " + auditLogBefore + " After Update=> " + auditLogAfter;
               // commonOutwardItemDao.AddOutwardItemHistory("R", col["current_fldtransno"], outwardRemarks, taskId, currentUser);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<string> Validate(FormCollection col, AccountModel currentUser)
        {
            List<string> err = new List<string>();
            //Force initiate with null check, because this is shared validation

            if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldItemId"], col["fldupdatetimestamp"]) == true)
            {
                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }
            if ((col["hIntRejCode"] == null || col["hIntRejCode"] == "") && col["txtChequeAccountNumber"].Trim() == "")
            {
                err.Add("Please Key in Acc/Amount or select Reject Reason.");
            }
            return err;
        }

        public List<string> BranchCode(String BranchCode, String BankCode)
        {
            string stmt = "Select fldBranchCode from tblBranchMaster where" +
                " fldBranchCode=@BranchCode and fldBankCode = @BankCode";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@BranchCode", BranchCode), new SqlParameter("@BankCode", BankCode) });
            List<string> branchCodeAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                branchCodeAvailable.Add(row["fldBranchCode"].ToString());
            }
            return branchCodeAvailable;
            //return null;
        }
        public List<string> BankCode(String BankCode)
        {
            string stmt = "Select fldbankcode from tblbankmaster where" +
                " fldbankcode = @bankcode and fldactive ='Y'";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@bankcode", BankCode) });
            List<string> bankCodeAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                bankCodeAvailable.Add(row["fldbankcode"].ToString());
            }
            return bankCodeAvailable;
            //return null;
        }
        public List<string> LocationCode(String LocationCode)
        {
            string stmt = "Select fldlocationcode from tbllocationmaster where" +
                " fldlocationcode = @locationcode";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@locationcode", Regex.Replace(LocationCode.ToString().Trim(), @"\s+", "")) });
            List<string> locationCodeAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                locationCodeAvailable.Add(row["fldlocationcode"].ToString());
            }
            return locationCodeAvailable;
            //return null;

        }
        public void UpdateBalancingAmount(FormCollection collection, AccountModel currentUser , string[] arr)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@intItemID", Convert.ToInt64(arr[0].ToString())));
            sqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@intNewAmount", Convert.ToInt64(arr[2].ToString())));
            sqlParameterNext.Add(new SqlParameter("@intTransNo", Convert.ToInt64(collection["current_fldtransno"].Trim())));
            dbContext.GetRecordsAsDataTableSP("spcuWriteBalAmntEntry", sqlParameterNext.ToArray());
        }
        public void updateBalancingStatus(FormCollection collection, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@intUserID", Convert.ToInt32(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@intTransNo", Convert.ToInt64(collection["current_fldtransno"].Trim())));
            dbContext.GetRecordsAsDataTableSP("spcuItemUpdateBalancing", sqlParameterNext.ToArray());
        }
        
        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldReturnCode, fldReturnDesc FROM tblReasonInternal Order by Cast(fldReturnCode as Int) asc";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }
        public DataTable AIFMasterList(string AccNumber)
        {
            DataTable ds = new DataTable();
            string stmt = "select fldName1 from tblAifMaster where fldPVAccount = '" + AccNumber + "'";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }

        public DataTable CheckIndividualItemDetail(string ItemID)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldItemID", ItemID));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBalancingIndividualItemDetail", sqlParameterNext.ToArray());
            return resultTable;
        }

        public List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm)
        {
            List<string> err = new List<string>();
            if (CheckConfirm == "A")
            {
                List<string> branchCode = BranchCode(col["new_fldBranchCode"].ToString(), col["new_fldBankCode"].ToString());
                List<string> bankCode = BankCode(col["new_fldbankcode"].ToString());
                List<string> locationCode = LocationCode(col["new_fldstatecode"].ToString());

                if (clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "1" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "2" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "16" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "6" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "3")
                {
                    err.Add("Cannot perform action. Cheque has been submitted");
                }
                else
                {
                    if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldItemId"], col["fldupdatetimestamp"]) == true)
                    {
                        err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
                    }
                    if (col["new_fldtype"] != null)
                    {

                        if (col["new_fldtype"].ToString().Trim().Length != 1)
                        {
                            err.Add("Type code must be 1 digit");
                        }
                        else if (col["new_fldtype"].ToString().Trim() != "1" && col["new_fldtype"].ToString().Trim() != "2")
                        {
                            err.Add("Invalid Type. Please enter correct Type");
                        }
                    }
                    else
                    {
                        err.Add("Invalid Type. Please enter correct Type");
                    }
                    //if (Regex.Replace(col["txtChequeAmount"].ToString().Trim(), "[^a-zA-Z0-9_.]+", "").Replace(".", "").Length > 20)
                    //{
                    //    string longts = Regex.Replace(col["txtChequeAmount"].ToString().Trim(), "[^a-zA-Z0-9_.]+", "").Replace(".", "");
                    //    err.Add("Amount must not be more than 20 digits");
                    //}
                    //if (col["txtChequeAmount"] == "0.00" || col["txtChequeAmount"] == null || col["txtChequeAmount"] == "")
                    //{
                    //    err.Add("Invalid Amount. Please enter correct Amount");
                    //}
                    if (col["new_fldserial"] != null)
                    {
                        if (col["new_fldserial"].ToString().Trim().Length != 6)
                        {
                            err.Add("Cheque number must be 6 digits");

                        }

                        else if (col["new_fldSerial"].ToString().Trim().Equals("000000") && col["new_fldtype"].ToString().Trim() == "1")
                        {
                            err.Add("Invalid Cheque number. Please enter correct Cheque number");
                        }
                        if (Regex.IsMatch(col["new_fldserial"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Cheque number must be numeric only");
                        }
                    }
                    else
                    {
                        err.Add("Invalid Cheque number. Please enter correct Cheque number");
                    }
                    if (col["new_fldissueraccNo"] != null)
                    {
                        if (col["new_fldissueraccno"].ToString().Equals("00000000000000000000"))
                        {
                            err.Add("Invalid Issuer Account number. Please enter correct Issuer Account number");
                        }
                        else if (col["new_fldissueraccno"].ToString().Equals("") || col["new_fldissueraccno"].ToString().Trim().Length != 20)
                        {
                            err.Add("Issuer Account number must be 20 digits");

                        }
                        else if (Regex.IsMatch(col["new_fldissueraccno"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Issuer Account number must be numeric only");
                        }
                    }
                    else
                    {

                        err.Add("Invalid Issuer Account number. Please enter correct Issuer Account number");
                    }
                    if (col["new_fldcheckdigit"] != null)
                    {

                        //if (col["new_fldcheckdigit"] == "" || col["new_fldcheckdigit"].ToString().Trim().Length != 2)
                        //{
                        //    err.Add(" Check Digit must be 2 digits");

                        //}
                        if (Regex.IsMatch(col["new_fldcheckdigit"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Invalid Check Digit.Please enter correct Check Digit");
                        }
                        else if (col["new_fldcheckdigit"] == "" || col["new_fldcheckdigit"].ToString().Trim().Length != 2)
                        {
                            err.Add("Check Digit must be 2 digits");

                        }
                    }
                    else
                    {
                        err.Add(" Invalid Check Digit. Please enter correct Check Digit");
                    }

                    if (col["textAreaRemarks"].ToString().Trim().Length > 100)
                    {
                        err.Add(" Remarks must not be more than 100 characters");

                    }

                    if (col["new_fldbankCode"] != null)
                    {
                        if (col["new_fldbankCode"].ToString().Trim().Length != 3)
                        {
                            err.Add("Bank code must be 3 digits");
                        }
                        if (Regex.IsMatch(col["new_fldbankCode"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Bank Code must be numeric only");
                        }
                        else if (bankCode.Count.Equals(0))
                        {
                            err.Add("Invalid Bank Code. Please enter correct Bank Code");
                        }
                    }

                    else
                    {
                        err.Add("Invalid Bank Code. Please enter correct Bank Code");
                    }
                    if (col["new_fldbranchCode"] != null)
                    {
                        if (col["new_fldbranchcode"].ToString().Trim().Length != 4)
                        {
                            err.Add("Branch code must be 4 digits");
                        }
                        if (Regex.IsMatch(col["new_fldbranchcode"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Branch Code must be numeric only");
                        }
                        else if (branchCode.Count.Equals(0))
                        {
                            err.Add("Invalid Branch Code. Please enter correct Branch Code");
                        }
                    }
                    else
                    {
                        err.Add("Invalid Branch Code. Please enter correct Branch Code");
                    }
                    if (Regex.Replace(col["new_fldstatecode"].ToString().Trim(), @"\s+", "") != null)
                    {
                        if (col["new_fldstatecode"].ToString().Trim().Length != 1)
                        {
                            err.Add("Location code must be 1 digit");
                        }
                        if (Regex.IsMatch(col["new_fldstatecode"].ToString().Trim(), "[^0-9]"))
                        {
                            err.Add("Location code must be numeric only");
                        }
                        else if (locationCode.Count.Equals(0))
                        {
                            err.Add("Invalid Location Code. Please enter correct Location Code");
                        }
                    }

                    else
                    {
                        err.Add("Invalid Location Code. Please enter correct Location Code");
                    }
                }
            }
            else
            {
                if (clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "1" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "2" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "16" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "6" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "3")

                {
                    err.Add("Cannot perform action. Cheque has been submitted");
                }
                else
                {
                    if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldItemId"], col["fldupdatetimestamp"]) == true)
                    {
                        err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
                    }

                    //if (col["textAreaRemarks"].ToString().Trim().Length > 100)
                    //{
                    //    err.Add(" Remarks must not be more than 100 characters");

                    //}

                }
            }

            return err;
        }
    }
}