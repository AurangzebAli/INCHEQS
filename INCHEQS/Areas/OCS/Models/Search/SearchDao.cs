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


namespace INCHEQS.Areas.OCS.Models.Search
{
    public class SearchDao : ISearchDao
    {

        private readonly OCSDbContext dbContext;
        private readonly ICommonOutwardItemDao commonOutwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IClearingDao clearingDao;

        public SearchDao(OCSDbContext dbContext, ISequenceDao sequenceDao, ICommonOutwardItemDao commonOutwardItemDao, IAuditTrailDao auditTrailDao, IClearingDao clearingDao)
        {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.commonOutwardItemDao = commonOutwardItemDao;
            this.auditTrailDao = auditTrailDao;
            this.clearingDao = clearingDao;
        }


        public void Confirm(FormCollection col, AccountModel currentUser, string taskId)
        {
            try
            {
                string auditLogBefore = "";
                string auditLogAfter = "";
                string historyRemarks = "";
                //int nextVerifySeq = commonOutwardItemDao.GetNextVerifySeqNo(col["fldItemId"]);
                //int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");
                int changesCounter = 0;

                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlUpdateInfo1 = new Dictionary<string, dynamic>();

                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "fldItemID", col["current_fldItemID"] } };
                Dictionary<string, dynamic> sqlCondition1 = new Dictionary<string, dynamic>() { { "fldTransno", col["current_fldtransno"] } };

                //If no change value

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
                sqlUpdateInfo1.Add("fldremark", col["textAreaRemarks"]);

                //Compulsory update for tblMICRRepair
                //sqlChequeHistory.Add("flditemid", Convert.ToInt64(col["current_flditemid"]));
                //sqlChequeHistory.Add("flditemtype", "V");
                sqlChequeHistory.Add("fldremark", col["textAreaRemarks"]);
                sqlChequeHistory.Add("fldoriremark", col["current_fldremark"]);
                sqlChequeHistory.Add("fldcreatetimestamp", Convert.ToDateTime(col["current_fldcreatetimestamp"]));
                sqlChequeHistory.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));

                //Excute the command
                //dbContext.ConstructAndExecuteInsertCommand("tblmicrrepair", sqlChequeHistory);
                //dbContext.ConstructAndExecuteUpdateCommand("tbliteminfo", sqlUpdateInfo, sqlCondition);
                dbContext.ConstructAndExecuteUpdateCommand("tbliteminfo", sqlUpdateInfo1, sqlCondition1);

                //Add to audit trail
                auditTrailDao.Log("Edit Item Info - Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore, currentUser);
                auditTrailDao.Log("Edit Item Info - Item Id : " + col["current_flditemid"] + " After Update=> " + auditLogAfter, currentUser);

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
                auditTrailDao.Log("Cheque Amount Entry - Edit Item Info - Item Id : " + col["current_fldItemID"] + " Before Update=> " + auditLogBefore, currentUser);
                auditTrailDao.Log("Cheque Amount Entry - Edit Item Info - Item Id : " + col["current_fldItemID"] + " After Update=> " + auditLogAfter, currentUser);
                outwardRemarks = "Cheque Amount Entry - Item has been Rejected. Item Id: " + col["current_flditemid"] + " Before Update=> " + auditLogBefore + " After Update=> " + auditLogAfter;
                commonOutwardItemDao.AddOutwardItemHistory("R", col["current_fldtransno"], outwardRemarks, taskId, currentUser);


            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<string> Validate(FormCollection col, AccountModel currentUser, string CheckConfirm)
        {
       
            List<string> err = new List<string>();
            //List<string> branchCode = BranchCode(col["new_fldBranchCode"].ToString(), col["new_fldBankCode"].ToString());
            //List<string> bankCode = BankCode(col["new_fldbankcode"].ToString());
            ////List<string> locationCode = LocationCode(col["new_fldstatecode"].ToString());
            //List<string> locationCode = LocationCode(col["new_fldstatecode"].ToString(), col["new_fldbankcode"].ToString(), col["new_fldbranchcode"].ToString());

            if (CheckConfirm == "A")
            {
                if ( clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "2" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "16" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "6" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "3")
                {
                    err.Add("Cannot perform action. Cheque has been submitted");
                }
                else
                {
                    if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["fldItemId"], col["fldupdatetimestamp"]) == true)
                    {
                        err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
                    }
             
                }
            }
            else
            {
                if (clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "2" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "16" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "6" || clearingDao.GetCurrentStatus(col["fldItemId"].ToString()) == "3")

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

        public List<string> BranchCode(String BranchCode, String BankCode)
        {
            string stmt = "Select fldBranchCode from tblBranchMaster where" +
                " fldBranchCode=@BranchCode and fldBankCode = @BankCode and fldActive = 'Y'";
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

        public List<string> LocationCode(String LocationCode, String BankCode, String BranchCode)
        {
            string stmt = "select fldlocationcode from tblbranchmaster where fldbankcode = @fldbankcode and fldbranchcode = @fldbranchcode and fldlocationcode = @locationcode";
            DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@locationcode", Regex.Replace(LocationCode.ToString().Trim(), @"\s+", "")),
            new SqlParameter("@fldbankcode", BankCode),
            new SqlParameter("@fldbranchcode", BranchCode)
            });
            List<string> locationCodeAvailable = new List<string>();
            //ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                locationCodeAvailable.Add(row["fldlocationcode"].ToString());
            }
            return locationCodeAvailable;
            //return null;
        }

        public void UpdateRemarks(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@fldItemId", col["fldItemId"]));
                sqlParameterNext.Add(new SqlParameter("@fldremark", col["textAreaRemarks"]));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserid", currentUser.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuOCSChequeRemark", sqlParameterNext.ToArray());
        }
        
    }
}