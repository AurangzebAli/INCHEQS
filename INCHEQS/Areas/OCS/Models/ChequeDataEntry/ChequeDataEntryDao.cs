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
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Areas.OCS.Models.Clearing;
using INCHEQS.Areas.OCS.Models.WebAPI;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using INCHEQS.OracleDataAccessLayer;
using Oracle.ManagedDataAccess.Client;

namespace INCHEQS.Areas.OCS.Models.ChequeDataEntry
{
    public class ChequeDataEntryDao : IChequeDataEntryDao
    {
        private readonly OCSDbContext dbContext;
        private readonly ApplicationOracleDbContext dbContextOracle;

        private readonly ICommonOutwardItemDao commonOutwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IClearingDao clearingDao;

        public ChequeDataEntryDao(OCSDbContext dbContext, ISequenceDao sequenceDao, ICommonOutwardItemDao commonOutwardItemDao, IAuditTrailDao auditTrailDao, IClearingDao clearingDao, ApplicationOracleDbContext dbContextOracle)
        {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.commonOutwardItemDao = commonOutwardItemDao;
            this.auditTrailDao = auditTrailDao;
            this.clearingDao = clearingDao;
            this.dbContextOracle = dbContextOracle;
        }

        public void PreAccountEntry(FormCollection col, AccountModel currentUser, string taskid)
        {
            string strChqAmount = string.Empty;
            decimal decOriginAmount = 0;

            string strVAcctNoList = string.Empty;
            string strOriginVAcctNoList = string.Empty;

            string[] arrVAcctNoList = null;
            string[] arrVAcctNoDetails = null;
            string[] arrOriginVAcctNoList = null;
            string[] arrOriginVAcctNoDetails = null;

            //Other parameter
            Int64 bintItemID = 0;
            string strVAcctNo = string.Empty;
            int intExistingVAcctNo = 0;
            string strDepAmount = string.Empty;
            string strPayeeName = string.Empty;
            string strPayeeAccStatus = string.Empty;
            

            string strOriginChqAmount = string.Empty;

            Int64 bintOriginItemID = 0;
            string strOriginVAcctNo = string.Empty;
            int intOriginExistingVAcctNo = 0;
            string strOriginDepAmount = string.Empty;

            string strAccNoList = string.Empty;
            string strDepAmountList = string.Empty; 
            string strPayeeNameList = string.Empty; 
            string strPayeeAccStatusList = string.Empty;
            string strChqAmountList = string.Empty;
            string strAccNoRemoveList = string.Empty;

            // #15 - Start - Declare a value hold conflicted flags
            string isConflictList = string.Empty;
            int isConflict = 0; // *** 0 means no conflicts occurred; 1 means conflicts occurred

            int i = 0;
            int j = 0;
            int k = 0;
            bool blnReturnVal = false;
            bool blnAccFound = false;


            //************************************************** 1.Declaration ********************************************************
            //1a. Get List Of Cheque Amount
            strChqAmount = col["hfdChqAmt"].ToString();
            if (strChqAmount == "")
            {
                strChqAmount = "NULL";
            }

            //1b. Get List Of Cheque Amount Original
            if (col["hfdCItemChqAmtList"].ToString() == "")
            {
                decOriginAmount = 0;
            }
            else
            {
                decOriginAmount = Convert.ToDecimal(col["hfdCItemChqAmtList"]);
            }

            //1c. Get List Of Account and Account Original 
            strVAcctNoList = col["hfdAccNo"];
            strOriginVAcctNoList = col["hfdVItemNoList"];
            // Get Array Of VAccount No + Flag
            // eg. [12345678+1]

            arrVAcctNoList = strVAcctNoList.Split(';');
            arrOriginVAcctNoList = strOriginVAcctNoList.Split(';');

            //************************************************** 2a.Construct Item Remove List ********************************************************
            for (k = 0; k < arrOriginVAcctNoList.Length; k++)
            {
                arrOriginVAcctNoDetails = arrOriginVAcctNoList[i].Split('+');
                bintOriginItemID = Convert.ToInt64(arrOriginVAcctNoDetails[0]);
                for (i = 0; i < arrVAcctNoList.Length; i++)
                {
                    arrVAcctNoDetails = arrVAcctNoList[i].Split('+');
                    bintItemID = Convert.ToInt64(arrVAcctNoDetails[0]);
                    intExistingVAcctNo = Convert.ToInt32(arrVAcctNoDetails[2]);
                    if (bintOriginItemID == bintItemID)
                    {
                        blnAccFound = true;
                        break;
                    }
                    arrVAcctNoDetails = null;
                    bintItemID = 0;
                    intExistingVAcctNo = 0;
                }
                if (!blnAccFound)
                {
                    if (strAccNoRemoveList == "")
                    {
                        strAccNoRemoveList = Convert.ToString(bintOriginItemID) + ";";
                    }
                    else
                    {
                        strAccNoRemoveList = strAccNoRemoveList + Convert.ToString(bintOriginItemID) + ";";
                    }
                    //strAccNoRemoveList = GetStringAppendedValue(strAccNoRemoveList, Convert.ToString(bintOriginItemID), ";");
                }
                blnAccFound = false;
                arrOriginVAcctNoDetails = null;
                bintOriginItemID = 0;
            }

            //************************************************** 2b.Construct Item Add List, and Update ********************************************************

            for (i = 0; i <= arrVAcctNoList.Length - 1; i++)
            {
                arrVAcctNoDetails = arrVAcctNoList[i].Split('+');
                bintItemID = Convert.ToInt64(arrVAcctNoDetails[0]);
                strVAcctNo = arrVAcctNoDetails[1];
                intExistingVAcctNo = Convert.ToInt32(arrVAcctNoDetails[2]);
                strDepAmount = arrVAcctNoDetails[9];
                strPayeeName = arrVAcctNoDetails[10];
                strPayeeAccStatus = arrVAcctNoDetails[11];

                if (strDepAmount == "")
                {
                    strDepAmount = "NULL";
                }

                if (arrVAcctNoDetails[8] != col["hfdBusinessType"].ToString())
                {
                    isConflict = 1;
                }
                else
                {
                    isConflict = 0;
                }

                switch (intExistingVAcctNo)
                {
                    case 0:
                        strDepAmount = strDepAmount.Replace(",", "").Replace(".", "");
                        if (strAccNoList == "")
                        {
                            strAccNoList = strVAcctNo + ";";
                        }
                        else
                        {
                            strAccNoList = strAccNoList + strVAcctNo + ";";

                        }
                        if (strDepAmountList == "")
                        {
                            strDepAmountList = Convert.ToString(strDepAmount) + ";";

                        }
                        else
                        {
                            strDepAmountList = strDepAmountList + Convert.ToString(strDepAmount) + ";";
                        }
                        if (strPayeeNameList == "")
                        {
                            strPayeeNameList = strPayeeName + ";";

                        }
                        else
                        {
                            strPayeeNameList = strPayeeNameList + strPayeeName + ";";
                        }
                        if (strPayeeAccStatusList == "")
                        {
                            strPayeeAccStatusList = strPayeeAccStatus + ";";

                        }
                        else
                        {
                            strPayeeAccStatusList = strPayeeAccStatusList + strPayeeAccStatus + ";";
                        }
                        if (isConflictList == "")
                        {
                            isConflictList = Convert.ToString(isConflict) + ";";
                        }
                        else
                        {
                            isConflictList = isConflictList + Convert.ToString(isConflict) + ";";

                        }
                        break;

                    case 1:
                        if (i <= arrOriginVAcctNoList.Length)
                        {
                            for (j = 0; j < arrOriginVAcctNoList.Length; j++)
                            {
                                arrOriginVAcctNoDetails = arrOriginVAcctNoList[j].Split('+');
                                bintOriginItemID = Convert.ToInt64(arrOriginVAcctNoDetails[0]);
                                strOriginVAcctNo = arrOriginVAcctNoDetails[1];
                                intOriginExistingVAcctNo = Convert.ToInt32(arrOriginVAcctNoDetails[2]);
                                strOriginDepAmount = arrOriginVAcctNoDetails[9];
                                if (arrVAcctNoDetails[8] != col["hfdBusinessType"].ToString())
                                {
                                    isConflict = 1;
                                }
                                // Same Item ID But Value Changed
                                if (bintItemID == bintOriginItemID)
                                {
                                    blnReturnVal = UpdateEntry(bintItemID, strVAcctNo, strDepAmount, strChqAmount, bintOriginItemID, strOriginVAcctNo, strOriginDepAmount, strOriginChqAmount, Convert.ToString(isConflict), currentUser, col, taskid, strPayeeName, strPayeeAccStatus);
                                    break;
                                }
                            }
                            if (blnReturnVal == false && i == arrVAcctNoList.Length)
                            {
                                break;
                            }
                        }
                        break;

                }
            }
            if (strAccNoList != "")
            {
                strAccNoList = strAccNoList.Remove(strAccNoList.Length - 1, 1);
            }
            if (strPayeeNameList != "")
            {
                strPayeeNameList = strPayeeNameList.Remove(strPayeeNameList.Length - 1, 1);
            }
            if (strDepAmountList != "")
            {
                strDepAmountList = strDepAmountList.Remove(strDepAmountList.Length - 1, 1);
            }
            if (isConflictList != "")
            {
                isConflictList = isConflictList.Remove(isConflictList.Length - 1, 1);
            }

            if (strAccNoRemoveList != "")
            {
                strAccNoRemoveList = strAccNoRemoveList.Remove(strAccNoRemoveList.Length - 1, 1);
            }
            //************************************************** 2c.Construct Item Add List, and Update ********************************************************
            if (!string.IsNullOrWhiteSpace(strAccNoList) || strAccNoList != "")
            {
                blnReturnVal = InsertAllEntry(strAccNoList, strDepAmountList, isConflictList, currentUser, col, taskid, strPayeeNameList, strPayeeAccStatusList);
            }

            if (!string.IsNullOrWhiteSpace(strAccNoRemoveList) || strAccNoRemoveList != "")
            {
                blnReturnVal = false; //DeleteAllEntry(strAccNoRemoveList)
            }
        }

        public bool UpdateEntry(Int64 bintItemID, string strVAcctNo, string strDepAmount, string strChqAmount, Int64 bintOriginItemID, string strOriginVAcctNo, string strOriginDepAmount, string strOriginChqAmount, string isConflict, AccountModel currentUser, FormCollection col, string taskid, string strPayeeName,string strPayeeAccStatus)
        {
            bool blnResult = false;
            try
            {

                decimal decDepAmount = 0;
                decimal decChqAmount = 0;
                decimal decOriginDepAmount = 0;
                decimal decOriginChqAmount = 0;
                string outwardRemarks = "";
                if (strDepAmount == "NULL")
                {
                    strDepAmount = "";
                }
                if (strOriginDepAmount == "NULL")
                {
                    strOriginDepAmount = "";
                }
                if (strChqAmount == "NULL")
                {
                    strChqAmount = "";
                }
                if (strOriginChqAmount == "NULL")
                {
                    strOriginChqAmount = "";
                }

                if (strDepAmount == "")
                {
                    decDepAmount = 0;
                }
                else
                {
                    decDepAmount = Convert.ToDecimal(strDepAmount.Replace(",", "").Replace(".", ""));
                }

                if (strOriginDepAmount == "")
                {
                    decOriginDepAmount = 0;
                }
                else
                {
                    decOriginDepAmount = Convert.ToDecimal(strOriginDepAmount.Replace(",", "").Replace(".", ""));
                }

                if (strChqAmount == "")
                {
                    decChqAmount = 0;
                }
                else
                {
                    decChqAmount = Convert.ToDecimal(strChqAmount);

                }

                if (strOriginChqAmount == "")
                {
                    decOriginChqAmount = 0;
                }
                else
                {
                    decOriginChqAmount = Convert.ToDecimal(strOriginChqAmount.Replace(",", "").Replace(".", ""));
                }

                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@bintItemID", bintItemID));
                sqlParameterNext.Add(new SqlParameter("@intUserID", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@strAccountNo", strVAcctNo));
                sqlParameterNext.Add(new SqlParameter("@intDepAmount", decDepAmount));
                sqlParameterNext.Add(new SqlParameter("@intChqAmount", strChqAmount.Replace(",", "").Replace(".", "")));
                sqlParameterNext.Add(new SqlParameter("@intTransNumber", col["current_fldtransno"]));
                sqlParameterNext.Add(new SqlParameter("@intReasonCode", ""));
                sqlParameterNext.Add(new SqlParameter("@intRemark", col["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@intIntRejCode", ""));
                sqlParameterNext.Add(new SqlParameter("@PayeeName", strPayeeName));
                //for Customer Account
                sqlParameterNext1.Add(new SqlParameter("@fldBankCode", currentUser.BankCode));
                sqlParameterNext1.Add(new SqlParameter("@fldAccountNumber", strVAcctNo.Trim()));
                sqlParameterNext1.Add(new SqlParameter("@fldAccountName", strPayeeName.Trim()));
                sqlParameterNext1.Add(new SqlParameter("@fldAccountStatus", strPayeeAccStatus.Trim()));
                sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
                sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
                dbContext.GetRecordsAsDataTableSP("spcuFrontEndDataEntry", sqlParameterNext.ToArray());
                dbContext.GetRecordsAsDataTableSP("spciCustomerAccount", sqlParameterNext1.ToArray());
                outwardRemarks = "Data Entry - Deposit Amount : " + strDepAmount + " Deposite Account : " + strVAcctNo;
                //auditTrailDao.Log(outwardRemarks, currentUser);
                //commonOutwardItemDao.AddOutwardItemHistory("C", col["current_fldtransno"], outwardRemarks, taskid, currentUser);
                blnResult = true;
            }
            catch (Exception)
            {
                blnResult = false;
                throw;
            }
            return blnResult;
        }

        public bool InsertAllEntry(string strAccNoList, string strDepAmtList, string isConflictList, AccountModel currentUser, FormCollection col, string taskid, string strPayeeNameparam,string strPayeeAccStatusParam)
        {
            bool blnResult = false;
            string[] arrAccountList = null;
            string[] arrDepAmtList = null;
            string[] arrPayeeNameList = null;
            string[] arrPayeeAccStatusList = null;

            Int64 intNewStartNumber = 0;
            string AccountNumber = "";
            string outwardRemarks = "";
            string Amount;
            string strPayeeName;
            string strPayeeAccStatus;
            try
            {
                arrAccountList = strAccNoList.Split(';');
                arrDepAmtList = strDepAmtList.Split(';');
                arrPayeeNameList = strPayeeNameparam.Split(';');
                arrPayeeAccStatusList = strPayeeAccStatusParam.Split(';');
                for (int i = 0; i <= arrAccountList.Length - 1; i++)
                {
                    DataTable resultTable = new DataTable();
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    sqlParameterNext.Add(new SqlParameter("@intColumnID", "1000"));
                    sqlParameterNext.Add(new SqlParameter("@intNumberMaxLength", "10"));
                    resultTable = dbContext.GetRecordsAsDataTableSP("spcuNewStartEndNumberByTypeWeb", sqlParameterNext.ToArray());
                    if (resultTable.Rows.Count > 0)
                    {
                        DataRow row = resultTable.Rows[0];
                        intNewStartNumber = Convert.ToInt64(row["intNewStartNumber"]);
                    }
                    AccountNumber = arrAccountList[i].ToString();
                    Amount = arrDepAmtList[i].ToString();
                    Amount = Amount.Replace(",", "").Replace(".", "");
                    strPayeeName = arrPayeeNameList[i].ToString();
                    strPayeeAccStatus = arrPayeeAccStatusList[i].ToString();
                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                    sqlParameterNext1.Add(new SqlParameter("@bintItemID", intNewStartNumber));
                    sqlParameterNext1.Add(new SqlParameter("@bintItemInitialID", col["hfdItemInitialID"]));
                    sqlParameterNext1.Add(new SqlParameter("@bintDepositAmount", Amount));
                    sqlParameterNext1.Add(new SqlParameter("@bintPVAccountNumber", AccountNumber));
                    sqlParameterNext1.Add(new SqlParameter("@bintCreateUserid", currentUser.UserId));
                    sqlParameterNext1.Add(new SqlParameter("@bintUpdateuserID", currentUser.UserId));
                    sqlParameterNext1.Add(new SqlParameter("@PayeeName", strPayeeName));
                    //for customer acc
                    sqlParameterNext2.Add(new SqlParameter("@fldBankCode", currentUser.BankCode));
                    sqlParameterNext2.Add(new SqlParameter("@fldAccountNumber", AccountNumber.Trim()));
                    sqlParameterNext2.Add(new SqlParameter("@fldAccountName", strPayeeName.Trim()));
                    sqlParameterNext2.Add(new SqlParameter("@fldAccountStatus", strPayeeAccStatus.Trim()));
                    sqlParameterNext2.Add(new SqlParameter("@fldCreateUserId", currentUser.UserId));
                    sqlParameterNext2.Add(new SqlParameter("@fldUpdateUserId", currentUser.UserId));
                    dbContext.GetRecordsAsDataTableSP("spciWriteAcctFrontEndDataEntry", sqlParameterNext1.ToArray());
                    dbContext.GetRecordsAsDataTableSP("spciCustomerAccount", sqlParameterNext2.ToArray());
                    outwardRemarks = "Data Entry - Auto Insert New V , Deposit Amount : " + arrDepAmtList[i].ToString() + " Deposite Account : " + AccountNumber;
                    //auditTrailDao.Log(outwardRemarks, currentUser);
                    //commonOutwardItemDao.AddOutwardItemHistory("C", col["current_fldtransno"], outwardRemarks, taskid, currentUser);
                    blnResult = true;
                }
            }
            catch (Exception)
            {
                blnResult = false;
                throw;
            }
            return blnResult;
        }

        public string GetStringAppendedValue(string strValList, string strVal, string strDelimiter)
        {
            string strValListRes = string.Empty;
            if (strValListRes == "")
            {
                strValList = strVal.ToString();
            }
            else
            {
                strValList = strValList + strDelimiter + strVal.ToString();
            }
            return strValListRes;
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
               // auditTrailDao.Log("Data Entry - Edit Item Info - Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore, currentUser);
                //auditTrailDao.Log("Data Entry - Edit Item Info - Item Id : " + col["current_flditemid"] + " After Update=> " + auditLogAfter, currentUser);
               // outwardRemarks = "Data Entry - Item has been Confirmed with following Information Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore + " After Update=> " + auditLogAfter;
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
                sqlUpdateInfo.Add("fldRemark", col["textAreaRemarks"]);

                if (!string.IsNullOrEmpty(col["textAreaRemarks"]))
                {
                    auditLogAfter += " [Remarks] : " + col["textAreaRemarks"];
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
                //auditTrailDao.Log("Data Entry - Edit Item Info - Item Id : " + col["current_fldItemID"] + " Before Update=> " + auditLogBefore, currentUser);
               // auditTrailDao.Log("Data Entry - Edit Item Info - Item Id : " + col["current_fldItemID"] + " After Update=> " + auditLogAfter, currentUser);
                //outwardRemarks = "Data Entry - Item has been Rejected. Trans No: " + col["current_fldtransno"] + " Before Update=> " + auditLogBefore + " After Update=> " + auditLogAfter;
                //commonOutwardItemDao.AddOutwardItemHistory("R", col["current_fldtransno"], outwardRemarks, taskId, currentUser);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<string> Validate(FormCollection col, AccountModel currentUser)
        {
            List<string> err = new List<string>();
            if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldItemId"], col["fldupdatetimestamp"]) == true)
            {
                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }
            //Force initiate with null check, because this is shared validation
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
        public void UpdateDataEntryAmountNAccount(FormCollection collection, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@bintItemID", Convert.ToInt64(collection["current_flditemid"].Trim())));
            sqlParameterNext.Add(new SqlParameter("@intUserID", currentUser.UserId));
            sqlParameterNext.Add(new SqlParameter("@strAccountNo", collection["txtChequeAccountNumber"].Trim()));
            if (collection["txtChequeAmount"].Trim() == "" || collection["txtChequeAmount"].Trim() == null)
            {
                sqlParameterNext.Add(new SqlParameter("@intDepAmount", ""));
                sqlParameterNext.Add(new SqlParameter("@intChqAmount", ""));
            }
            else
            {
                sqlParameterNext.Add(new SqlParameter("@intDepAmount", Convert.ToInt64(collection["txtChequeAmount"].Trim().Replace(",", "").Replace(".", ""))));
                sqlParameterNext.Add(new SqlParameter("@intChqAmount", Convert.ToInt64(collection["txtChequeAmount"].Trim().Replace(",", "").Replace(".", ""))));
            }
            if (!string.IsNullOrWhiteSpace(collection["hIntRejCode"].Trim()))
            {
                sqlParameterNext.Add(new SqlParameter("@intReasonCode", "430"));
                sqlParameterNext.Add(new SqlParameter("@intRemark", collection["hRemark"].Trim()));
                sqlParameterNext.Add(new SqlParameter("@intIntRejCode", collection["hIntRejCode"].Trim()));
            }
            else
            {
                sqlParameterNext.Add(new SqlParameter("@intReasonCode", ""));
                sqlParameterNext.Add(new SqlParameter("@intRemark", ""));
                sqlParameterNext.Add(new SqlParameter("@intIntRejCode", ""));
            }
            sqlParameterNext.Add(new SqlParameter("@intTransNumber", Convert.ToInt64(collection["current_fldtransno"].Trim())));
            dbContext.GetRecordsAsDataTableSP("spcuFrontEndDataEntry", sqlParameterNext.ToArray());
        }

        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldReturnCode, fldReturnDesc FROM tblReasonInternal Order by Cast(fldReturnCode as Int) asc";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }

        //GetOracleAccountInformation -MAB-MGR
        public DataTable GetOracleAccountInformation(string AccNumber)
        {
            DataTable ds = new DataTable();
            DataTable dt = new DataTable();
            string AccountName = string.Empty;
            string AccountStatus = string.Empty;
            string AccountDormant = string.Empty;
            string AccountFrozen = string.Empty;
            bool _checkOracleServerConnection = false;
            _checkOracleServerConnection = dbContextOracle.IsOracleServerConnected();
            if (_checkOracleServerConnection == true)
            {
                string stmt = "SELECT ACCOUNTNO,ACCNAME,CASE WHEN ACCSTATUS = 'O' then 'Open' WHEN ACCSTATUS = 'C' then 'Close' ELSE ACCSTATUS END AS ACCSTATUS, ACDORMANT, ACFROZEN FROM STVWS_CTS_ACC_STATUS WHERE ACCOUNTNO=:AccountNumber";
                dt = dbContextOracle.GetRecordsAsDataTable(stmt, new OracleParameter[] { new OracleParameter("AccountNumber", AccNumber.Trim()) });
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        AccountName = row["ACCNAME"].ToString();
                        AccountStatus = row["ACCSTATUS"].ToString();
                        AccountDormant = row["ACDORMANT"].ToString();
                        AccountFrozen = row["ACFROZEN"].ToString();
                    }
                    ds.Clear();
                    ds.Columns.Add("fldAccountName");
                    ds.Columns.Add("fldAccountStatus");
                    ds.Columns.Add("fldAccountDormant");
                    ds.Columns.Add("fldAccountFrozen");
                    object[] o = { AccountName, AccountStatus, AccountDormant, AccountFrozen };
                    ds.Rows.Add(o);
                }
                else
                {
                    ds.Clear();
                    ds.Columns.Add("fldAccountName");
                    ds.Columns.Add("fldAccountStatus");
                    ds.Columns.Add("fldAccountDormant");
                    ds.Columns.Add("fldAccountFrozen");
                    object[] o = { "Record Not found.", "-", "-", "-" };
                    ds.Rows.Add(o);
                }
            }
            else
            {

                ds.Clear();
                ds.Columns.Add("fldAccountName");
                ds.Columns.Add("fldAccountStatus");
                ds.Columns.Add("fldAccountDormant");
                ds.Columns.Add("fldAccountFrozen");
                object[] o = { "Failed to Connect Oracle Database.", "-", "-", "-" };
                ds.Rows.Add(o);


            }
            return ds;
        }

        //public DataTable AIFMasterList(string AccNumber)
        //{
        //    string APIHOSTADDRESS = string.Empty;
        //    string requesttokenAPIPath = string.Empty;
        //    string SignatureAPIPath = string.Empty;
        //    string TokenURL = string.Empty;
        //    string AccessToken = string.Empty;
        //    string SigURL = string.Empty;
        //    bool GotToken = false;
        //    DataTable ds = new DataTable();
        //    DataTable dt = new DataTable();
        //    DataTable dtApiHostAddress = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    SignatureInputParameterModel obj = new SignatureInputParameterModel();
        //    SignatureEnquiryModel result11 = null;
        //    string AccountName = string.Empty;
        //    string AccountStatus = string.Empty;
        //    //if (AccNumber.Length == 13)
        //    //{
        //        AccNumber = AccNumber.Substring(4, 9);
        //        //sqlParameterNext.Add(new SqlParameter("@GLcode", AccNumber.Trim()));
        //        //dt = dbContext.GetRecordsAsDataTableSP("spcgGLCode", sqlParameterNext.ToArray());
        //        string stmt = "select fldaccountName from tblAifMaster where fldaccountnumber ='" + AccNumber + "'";
        //        ds = dbContext.GetRecordsAsDataTable(stmt);
        //        if (dt.Rows.Count > 0)
        //        {
        //            AccountName = dt.Rows[0]["fldGLDesc"].ToString();
        //            ds.Clear();
        //            ds.Columns.Add("fldAccountName");
        //            ds.Columns.Add("fldAccountStatus");
        //            AccountStatus = "-";
        //            object[] o = { AccountName, AccountStatus };
        //            ds.Rows.Add(o);
        //        }
        //        else
        //        {
        //            ds.Clear();
        //            ds.Columns.Add("fldAccountName");
        //            ds.Columns.Add("fldAccountStatus");
        //            object[] o = { "Record Not found.", "-" };
        //            ds.Rows.Add(o);
        //        }

        //    return ds;
        //}

        public List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm)
        {
            List<string> err = new List<string>();


            if (CheckConfirm == "A")
            {
                if (clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "1" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "2" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "16" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "6" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "3")
                {
                    err.Add("Cannot perform action. Cheque has been submitted");
                }
                else
                {
                    if (col["current_flditemtype"] != "P")
                    {
                        List<string> branchCode = BranchCode(col["new_fldBranchCode"].ToString(), col["new_fldBankCode"].ToString());
                        List<string> bankCode = BankCode(col["new_fldbankcode"].ToString());
                        List<string> locationCode = LocationCode(col["new_fldstatecode"].ToString());

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

                    if (col["textAreaRemarks"].ToString().Trim().Length > 100)
                    {
                        err.Add(" Remarks must not be more than 100 characters");

                    }

                }
            }

            return err;
        }

        public string GetAPIRequestInfo(string Code)
        {
            string Result = string.Empty;
            DataTable dtApiHostAddress = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@RequestCode", Code.Trim()));
            dtApiHostAddress = dbContext.GetRecordsAsDataTableSP("spcgApiRequestTypeInfo", sqlParameterNext.ToArray());
            Result = dtApiHostAddress.Rows[0]["fldRequestPath"].ToString();
            if (Result.EndsWith(@"\") || Result.EndsWith("/"))
            {
                Result = Result.Remove(Result.Length - 1, 1);
            }
            return Result;
        }
        public string GETJWTToken(string URL, out bool GotKey)
        {
            string Result = string.Empty;
            GotKey = false;
            try
            {
                JWTTokenModel obj = new JWTTokenModel();
                JWTTokenModelResponse result11 = null;
                obj.client_secret = GetAPIRequestInfo("ClientSecret");//"asdasdasdasda";
                obj.client_id = GetAPIRequestInfo("ClientId"); //"ASDASDxczxxxz//zxc/zxasdas//##$%^^";

                using (var client = new HttpClient())
                {
                    HttpRequestMessage reqmsg = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(URL)
                    };
                    reqmsg.Content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/x-www-form-urlencoded");
                    HttpResponseMessage result = client.SendAsync(reqmsg).Result;
                    if (Convert.ToInt32(result.StatusCode) == 200)
                    {
                        using (HttpContent content = result.Content)
                        {
                            var json = content.ReadAsStringAsync().Result;
                            result11 = JsonConvert.DeserializeObject<JWTTokenModelResponse>(json);
                            Result = result11.access_token.ToString();
                            GotKey = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GotKey = false;
                Result = ex.InnerException.ToString();
                goto EndToken;
            }
            EndToken:
            return Result;
        }
        public bool CheckBranch(string BranchCode)
        {
            DataTable dt = new DataTable();
            bool result = false;
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@Branchcode", BranchCode.Trim()));
            dt = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", SqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                result = true;
            }
            return result;
        }
    }
}