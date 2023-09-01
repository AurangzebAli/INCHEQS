//using INCHEQS.Areas.ICS.Models.Verification;
//using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Models.Sequence;
using INCHEQS.Resources;
//using INCHEQS.Security;
using System;
using System.Collections.Generic;
//using System.Data.Entity;
//using Npgsql;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.DataAccessLayer.OCS;
using System.Data.SqlClient;

namespace INCHEQS.Areas.OCS.Models.ChequeAccountEntry
{
    public class ChequeAccountEntryDao : IChequeAccountEntryDao
    {

        private readonly ApplicationDbContext dbContext;
        private readonly ICommonOutwardItemDao commonOutwardItemDao;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;

        public ChequeAccountEntryDao(ApplicationDbContext dbContext, ISequenceDao sequenceDao, ICommonOutwardItemDao commonOutwardItemDao, IAuditTrailDao auditTrailDao)
        {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.commonOutwardItemDao = commonOutwardItemDao;
            this.auditTrailDao = auditTrailDao;
        }


        public bool CheckAccountExist(string strAccountNumber)
        {
            string stmt = "select * from tblaifmaster where fldpvaccount=@fldpvaccount ";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldpvaccount", strAccountNumber)
            });
        }


        public List<string> Validate(FormCollection col, AccountModel currentUser)
        {
            List<string> err = new List<string>();
            //List<string> branchCode = VerificationBranchCode(col["new_fldbranchcode"].ToString(), col["new_fldbankcode"].ToString());

            if (commonOutwardItemDao.CheckIfRecordUpdatedOrDeleted(col["flditemid"], col["fldupdatetimestamp"]))
            {
                err.Add(Locale.Thisrecordhasbeendeletedorupdatedbyanotheruser);
            }

            //Force initiate with null check, because this is shared validation
            if (col["txtChequeAccount"] == null || col["txtChequeAccount"] == "")
            {
                err.Add("Invalid Account Number");
            }

            if (col["txtChequeAccount"] != null)
            {

                if (Regex.IsMatch(col["txtChequeAccount"].ToString(), "[^0-9]"))
                {
                    err.Add("Input of Account Number must be numeric only");
                }


                // else if (!CheckAccountExist(col["txtChequeAccount"]))
                // {
                //    err.Add("Invalid Account Number");
                //}
            }

            return err;
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
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "flditemid", Convert.ToInt64(col["current_flditemid"]) } };
                //If no change value


                //if (col["new_fldissueraccno"] == col["current_fldissueraccNo"])
                //{
                //    sqlUpdateInfo.Add("fldissueraccno", col["current_fldissueraccno"]);
                //    sqlChequeHistory.Add("fldissueraccno", col["current_fldissueraccno"]);
                //}
                //if (col["new_fldserial"] == col["current_fldserial"])
                //{
                //    sqlUpdateInfo.Add("fldserial", col["current_fldserial"]);
                //    sqlChequeHistory.Add("fldserial", col["current_fldserial"]);
                //}

                //if (col["new_fldtccode"] == col["current_fldtccode"])
                //{
                //    sqlUpdateInfo.Add("fldtccode", col["current_fldtccode"]);
                //    sqlChequeHistory.Add("fldtccode", col["current_fldtccode"]);
                //}
                //if (col["new_fldstatecode"] == col["current_fldstatecode"])
                //{
                //    sqlUpdateInfo.Add("fldstatecode", col["current_fldstatecode"]);
                //    sqlChequeHistory.Add("fldstatecode", col["current_fldstatecode"]);
                //}
                //if (col["new_fldbankcode"] == col["current_fldbankcode"])
                //{
                //    sqlUpdateInfo.Add("fldbankcode", col["current_fldbankcode"]);
                //    sqlChequeHistory.Add("fldbankcode", col["current_fldbankcode"]);
                //}
                //if (col["new_fldbranchcode"] == col["current_fldbranchcode"])
                //{
                //    sqlUpdateInfo.Add("fldbranchcode", col["current_fldbranchcode"]);
                //    sqlChequeHistory.Add("fldbranchcode", col["current_fldbranchcode"]);
                //}
                //if (col["new_fldcheckdigit"] == col["current_fldcheckdigit"])
                //{
                //    sqlUpdateInfo.Add("fldcheckdigit", col["current_fldcheckdigit"]);
                //    sqlChequeHistory.Add("fldcheckdigit", col["current_fldcheckdigit"]);
                //}
                //if (col["new_fldamount"] == col["current_fldamount"])
                //{
                //    sqlUpdateInfo.Add("fldamount", Convert.ToDouble(col["current_fldamount"]));
                //    sqlChequeHistory.Add("fldamount", Convert.ToDouble(col["current_fldamount"]));
                //}
                //if (col["new_fldtype"] == col["current_fldtype"])
                //{
                //    sqlUpdateInfo.Add("fldtype", col["current_fldtype"]);
                //    sqlChequeHistory.Add("fldtype", col["current_fldtype"]);
                //}
                //if (col["new_fldlocation"] == col["current_fldlocation"])
                //{
                //    sqlUpdateInfo.Add("fldlocation", col["current_fldlocation"]);
                //    sqlChequeHistory.Add("fldlocation", col["current_fldlocation"]);
                //}

                ////Begin query contruction based on changed value
                //if (col["new_fldissueraccno"] != col["current_fldissueraccno"])
                //{
                //    sqlUpdateInfo.Add("fldissueraccNo", col["new_fldissueraccno"]);
                //    sqlChequeHistory.Add("fldoriissueraccNo", col["current_fldissueraccno"]);
                //    sqlChequeHistory.Add("fldissueraccno", col["new_fldissueraccno"]);


                //    auditLogBefore += " [Account No] : " + col["current_fldissueraccno"];
                //    auditLogAfter += " [Account No] : " + col["new_fldissueraccno"];
                //    historyRemarks += "| Account No. changed from " + col["current_fldissueraccno"] + " to " + col["new_fldissueraccno"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldserial"] != col["current_fldserial"])
                //{
                //    sqlUpdateInfo.Add("fldserial", col["new_fldserial"]);
                //    sqlChequeHistory.Add("fldoriserial", col["current_fldserial"]);
                //    sqlChequeHistory.Add("fldserial", col["new_fldserial"]);

                //    auditLogBefore += " [Cheque No] : " + col["current_fldserial"];
                //    auditLogAfter += " [Cheque No] : " + col["new_fldserial"];
                //    historyRemarks += "| Cheque No. changed from " + col["current_fldserial"] + " to " + col["new_fldserial"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldtccode"] != col["current_fldtccode"])
                //{
                //    sqlUpdateInfo.Add("fldtccode", col["new_fldtccode"]);
                //    sqlChequeHistory.Add("fldoritccode", col["current_fldtccode"]);
                //    sqlChequeHistory.Add("fldtccode", col["new_fldtccode"]);

                //    auditLogBefore += " [Trans Code] : " + col["current_fldtccode"];
                //    auditLogAfter += " [Trans Code] : " + col["new_fldtccode"];
                //    historyRemarks += "| Trans Code changed from " + col["current_fldtccode"] + " to " + col["new_fldtccode"] + ".";
                //    changesCounter += 1;
                //}

                //if (col["new_fldstatecode"] != col["current_fldstatecode"])
                //{
                //    sqlUpdateInfo.Add("fldstatecode", col["new_fldstatecode"]);
                //    sqlChequeHistory.Add("fldoristatecode", col["current_fldstatecode"]);
                //    sqlChequeHistory.Add("fldstatecode", col["new_fldstatecode"]);

                //    auditLogBefore += " [State Code] : " + col["current_fldstatecode"];
                //    auditLogAfter += " [State Code] : " + col["new_fldstatecode"];
                //    historyRemarks += "| State Code changed from " + col["current_fldstatecode"] + " to " + col["new_fldstatecode"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldbankcode"] != col["current_fldbankcode"])
                //{
                //    sqlUpdateInfo.Add("fldBankCode", col["new_fldbankcode"]);
                //    sqlChequeHistory.Add("fldoribankcode", col["current_fldbankcode"]);
                //    sqlChequeHistory.Add("fldbankcode", col["new_fldbankcode"]);

                //    auditLogBefore += " [Bank Code] : " + col["current_fldbankcode"];
                //    auditLogAfter += " [Bank Code] : " + col["new_fldbankcode"];
                //    historyRemarks += "| Bank Code changed from " + col["current_fldbankcode"] + " to " + col["new_fldbankcode"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldbranchcode"] != col["current_fldbranchcode"])
                //{
                //    sqlUpdateInfo.Add("fldbranchcode", col["new_fldbranchcode"]);
                //    sqlChequeHistory.Add("fldoribranchcode", col["current_fldbranchcode"]);
                //    sqlChequeHistory.Add("fldbranchcode", col["new_fldbranchcode"]);

                //    auditLogBefore += " [Branch Code] : " + col["current_fldbranchcode"];
                //    auditLogAfter += " [Branch Code] : " + col["new_fldbranchcode"];
                //    historyRemarks += "| Branch Code changed from " + col["current_fldbranchcode"] + " to " + col["new_fldbranchcode"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldcheckdigit"] != col["current_fldcheckdigit"])
                //{
                //    sqlUpdateInfo.Add("fldcheckdigit", col["new_fldcheckdigit"]);
                //    sqlChequeHistory.Add("fldoricheckdigit", col["current_fldcheckdigit"]);
                //    sqlChequeHistory.Add("fldcheckdigit", col["new_fldcheckdigit"]);

                //    auditLogBefore += " [Check Digit] : " + col["current_fldcheckdigit"];
                //    auditLogAfter += " [Check Digit] : " + col["new_fldcheckdigit"];
                //    historyRemarks += "| Check Digit changed from " + col["current_fldcheckdigit"] + " to " + col["new_fldcheckdigit"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldtype"] != col["current_fldtype"])
                //{
                //    sqlUpdateInfo.Add("fldtype", col["new_fldtype"]);
                //    sqlChequeHistory.Add("fldoritype", col["current_fldtype"]);
                //    sqlChequeHistory.Add("fldtype", col["new_fldtype"]);

                //    auditLogBefore += " [Type] : " + col["current_fldtype"];
                //    auditLogAfter += " [Type] : " + col["new_fldtype"];
                //    historyRemarks += "| Type changed from " + col["current_fldtype"] + " to " + col["new_fldtype"] + ".";
                //    changesCounter += 1;
                //}
                //if (col["new_fldlocation"] != col["current_fldlocation"])
                //{
                //    sqlUpdateInfo.Add("fldlocation", col["new_fldlocation"]);
                //    sqlChequeHistory.Add("fldorilocation", col["current_fldlocation"]);
                //    sqlChequeHistory.Add("fldlocation", col["new_fldlocation"]);

                //    auditLogBefore += " [Location] : " + col["current_fldlocation"];
                //    auditLogAfter += " [Location] : " + col["new_fldlocation"];
                //    historyRemarks += "| Location changed from " + col["current_fldlocation"] + " to " + col["new_fldlocation"] + ".";
                //    changesCounter += 1;
                //}
                if (col["txtChequeAccount"] != col["current_fldpvaccno"])
                {
                    sqlUpdateInfo.Add("fldpvaccno", col["txtChequeAccount"]);
                    sqlChequeHistory.Add("fldpvaccno", col["txtChequeAccount"]);

                    auditLogBefore += " [PVaccNo] : " + col["current_fldpvaccno"];
                    auditLogAfter += " [PVaccNo] : " + col["txtChequeAccount"];
                    historyRemarks += "| PVaccNo changed from " + col["current_fldpvaccno"] + " to " + col["txtChequeAccount"] + ".";
                    changesCounter += 1;
                }
                if (changesCounter == 0)
                {
                    historyRemarks += "Item has no changes.";
                }
                //Compulsory update for tblItemInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));
                sqlUpdateInfo.Add("fldupdatetimestamp", DateUtils.GetCurrentDatetimeForSql());
                //sqlUpdateInfo.Add("fldItemType", "V");
                sqlUpdateInfo.Add("fldremark", col["textAreaRemarks"]);

                sqlUpdateInfo.Add("fldactive", "Y");
                sqlUpdateInfo.Add("fldreasoncode", "");
                sqlChequeHistory.Add("fldreasoncode", "");
                sqlUpdateInfo.Add("fldclearingstatus", 9);
                sqlUpdateInfo.Add("fldpostingmode", "0");

                //Compulsory update for tblMICRRepair
                sqlChequeHistory.Add("flditemid", Convert.ToInt64(col["current_flditemid"]));
                //sqlChequeHistory.Add("flditemtype", "V");
                sqlChequeHistory.Add("fldremark", col["textAreaRemarks"]);
                sqlChequeHistory.Add("fldoriremark", col["current_fldremark"]);
                sqlChequeHistory.Add("fldcreatetimestamp", Convert.ToDateTime(col["current_fldcreatetimestamp"]));
                sqlChequeHistory.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));

                //Excute the command
                dbContext.ConstructAndExecuteInsertCommand("tblmicrrepair", sqlChequeHistory);
                dbContext.ConstructAndExecuteUpdateCommand("tbliteminfo", sqlUpdateInfo, sqlCondition);

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
                //int nextVerifySeq = commonOutwardItemDao.GetNextVerifySeqNo(col["fldItemId"]);
                //int nextHistorySecNo = sequenceDao.GetNextSequenceNo("tblInwardItemHistory");


                Dictionary<string, dynamic> sqlUpdateInfo = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlChequeHistory = new Dictionary<string, dynamic>();
                Dictionary<string, dynamic> sqlCondition = new Dictionary<string, dynamic>() { { "flditemid", Convert.ToInt64(col["current_flditemid"]) } };

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

                auditLogBefore += " [Reason Code] : " + "";
                auditLogAfter += " [Reason Code] : " + "430";
                historyRemarks += "| Reason Code changed from " + "" + " to " + "430" + ".";

                //Compulsory update for tblItemInfo
                //TODO : this process is update and confirmed by maker
                sqlUpdateInfo.Add("fldupdateuserid", Convert.ToInt16(currentUser.UserId));
                sqlUpdateInfo.Add("fldupdatetimestamp", DateUtils.GetCurrentDatetimeForSql());

                sqlUpdateInfo.Add("fldremark", col["textAreaRemarks"]);

                //Excute the command
                dbContext.ConstructAndExecuteUpdateCommand("tbliteminfo", sqlUpdateInfo, sqlCondition);
                dbContext.ConstructAndExecuteInsertCommand("tblmicrrepair", sqlChequeHistory);

                //Add to audit trail
                auditTrailDao.Log("Edit Item Info - Item Id : " + col["current_flditemid"] + " Before Update=> " + auditLogBefore, currentUser);
                auditTrailDao.Log("Edit Item Info - Item Id : " + col["current_flditemid"] + " After Update=> " + auditLogAfter, currentUser);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }
}