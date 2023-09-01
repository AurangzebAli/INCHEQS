using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using System.Web.Mvc;
using System.Text;
using INCHEQS.Security;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.ChequeDateAmountEntry;
using INCHEQS.Security.Account;
using System.Data.SqlClient;
using System.Data;

namespace INCHEQS.Areas.OCS.Models.AuditTrailOCS
{
    public class AuditTrailOCSDao : IAuditTrailOCSDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IChequeDateAmountEntryDao ChequeDateAmountEntryDao;

        public AuditTrailOCSDao(ApplicationDbContext dbContext, IChequeDateAmountEntryDao ChequeDateAmountEntryDao)
        {
            this.dbContext = dbContext;
            this.ChequeDateAmountEntryDao = ChequeDateAmountEntryDao;

        }

        public void AuditTrailOCSLog(string ActionPerformed, string ActionDetails, string TaskId, string ItemID, string TransNo, AccountModel currentUserAccount = null)
        {
            Dictionary<string, dynamic> keyValue = new Dictionary<string, dynamic>();
            if (currentUserAccount != null)
            {
                keyValue.Add("fldBankCode", StringUtils.Trim(currentUserAccount.BankCode));
                keyValue.Add("fldTaskId", StringUtils.Trim(TaskId));
            }
            else
            {
                keyValue.Add("fldBankCode", "");
                keyValue.Add("fldTaskId", StringUtils.Trim(TaskId));
            }

            keyValue.Add("fldTransNo", StringUtils.Trim(TransNo));
            keyValue.Add("fldItemId", StringUtils.Trim(ItemID));

            keyValue.Add("fldActionPerformed", StringUtils.Trim(ActionPerformed));
            keyValue.Add("fldActionDetails", StringUtils.Trim(ActionDetails));
            keyValue.Add("fldCreateUserId", StringUtils.Trim(currentUserAccount.UserId));
            keyValue.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());

            dbContext.ConstructAndExecuteInsertCommand("tblAuditLogOCS", keyValue);
        }


        #region cheque amount
        public string ChequeAmount_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Cheque Date & Amount Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldcheckdigit" || key == "current_fldtype" || key == "new_fldlocation" || key == "new_fldbankcode" || key == "new_fldbranchcode" || key == "current_fldserial" || key == "new_fldissueraccNo" || key == "txtChequeDate" || key == "current_fldamount" || key == "chkWaive" || key == "NoWaive" || key == "current_fldremark")
                {
                    if (key == "current_fldcheckdigit")
                    {
                        if (before.fldCheckDigit == after.fldCheckDigit)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldCheckDigit;
                        afterEdit = after.fldCheckDigit;
                        ColName = "Check Digit";
                    }
                    else if (key == "current_fldtype")
                    {
                        if (before.fldType == after.fldType)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldType;
                        afterEdit = after.fldType;
                        ColName = "Type";
                    }
                    else if (key == "new_fldlocation")
                    {
                        if (before.fldLocation == after.fldLocation)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldLocation;
                        afterEdit = after.fldLocation;
                        ColName = "Location";
                    }
                    else if (key == "new_fldbankcode")
                    {
                        if (before.fldBankCode == after.fldBankCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldBankCode;
                        afterEdit = after.fldBankCode;
                        ColName = "Issuing Bank";
                    }
                    else if (key == "new_fldbranchcode")
                    {
                        if (before.fldBranchCode == after.fldBranchCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldBranchCode;
                        afterEdit = after.fldBranchCode;
                        ColName = "Issuing Branch";
                    }
                    else if (key == "current_fldserial")
                    {
                        if (before.fldSerial == after.fldSerial)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldSerial;
                        afterEdit = after.fldSerial;
                        ColName = "Cheque Number";
                    }
                    else if (key == "new_fldissueraccNo")
                    {
                        if (before.fldIssuerAccNo == after.fldIssuerAccNo)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldIssuerAccNo;
                        afterEdit = after.fldIssuerAccNo;
                        ColName = "Issuer Account Number";
                    }
                    else if (key == "txtChequeDate")
                    {
                        if (before.fldChequeIssueDate == after.fldChequeIssueDate)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldChequeIssueDate;
                        afterEdit = after.fldChequeIssueDate;
                        ColName = "Cheque Date";
                    }
                    else if (key == "current_fldamount")
                    {
                        if (before.fldAmount == after.fldAmount)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = Convert.ToDecimal(Convert.ToDecimal(before.fldAmount) / 100).ToString("#,##0.00");
                        afterEdit = Convert.ToDecimal(Convert.ToDecimal(after.fldAmount) / 100).ToString("#,##0.00");
                        ColName = "Cheque Amount";
                    }


                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }
                    else if (key == "chkWaive" && key == "NoWaive")
                    {
                        if (before.fldWaiveCharges == after.fldWaiveCharges)
                        {
                            Remarks = "No Changes";
                        }

                        else
                        {

                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldWaiveCharges;
                        afterEdit = after.fldWaiveCharges;
                        ColName = "Waive Charges";

                    }
                    else if (key == "NoWaive" && key != "chkWaive")
                    {
                        if (before.fldWaiveCharges == after.fldWaiveCharges)
                        {
                            Remarks = "No Changes";
                        }

                        else
                        {

                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldWaiveCharges;
                        afterEdit = after.fldWaiveCharges;
                        ColName = "Waive Charges";

                    }

                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion


        #region cheque amount reject
        public string ChequeAmount_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Cheque Date & Amount Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldreasoncode" || key == "current_fldremark")
                {
                    if (key == "current_fldreasoncode")
                    {
                        if (before.fldReasonCode == after.fldReasonCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldReasonCode;
                        afterEdit = after.fldReasonCode;
                        ColName = "Cheque Status";
                    }
                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }


                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region cheq amount
        public AuditTrailOCSModel CheckItem(string ItemId, string TransNo)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemAmountEntry", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                item.fldCheckDigit = row["fldCheckDigit"].ToString();
                item.fldType = row["fldType"].ToString();
                item.fldLocation = row["fldLocation"].ToString();
                item.fldBankCode = row["fldBankCode"].ToString();
                item.fldBranchCode = row["fldBranchCode"].ToString();
                item.fldSerial = row["fldSerial"].ToString();
                item.fldIssuerAccNo = row["fldIssuerAccNo"].ToString();
                item.fldChequeIssueDate = row["fldChequeIssueDate"].ToString();
                item.fldAmount = row["fldAmount"].ToString();
                item.fldWaiveCharges = row["fldWaiveCharges"].ToString();
                item.fldRemark = row["fldRemark"].ToString();

            }
            else
            {
                item = null;
            }
            return item;
        }

        public AuditTrailOCSModel CheckItemReject(string ItemId, string TransNo)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemRejectAmountEntry", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                item.fldReasonCode = row["fldReasonCode"].ToString();
                item.fldRemark = row["fldRemark"].ToString();

            }
            else
            {
                item = null;
            }
            return item;
        }

        #endregion


        #region  data entry
        public string ChequeDataEntry_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Data Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldpvaccno" || key == "current_fldamount" || key == "current_fldremark")
                {
                    if (key == "current_fldpvaccno")
                    {
                        if (before.fldPVaccNo == after.fldPVaccNo)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        //beforeEdit = Convert.ToDecimal(Convert.ToDecimal(before.fldPVaccNo) / 100).ToString("#,##0.00");
                        //afterEdit = Convert.ToDecimal(Convert.ToDecimal(after.fldPVaccNo) / 100).ToString("#,##0.00");
                        beforeEdit = before.fldPVaccNo;
                        afterEdit = after.fldPVaccNo;
                        ColName = "Creditor Account Number";
                    }
                    else if (key == "current_fldamount")
                    {
                        if (before.fldAmount == after.fldAmount)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = Convert.ToDecimal(Convert.ToDecimal(before.fldAmount) / 100).ToString("#,##0.00");
                        afterEdit = Convert.ToDecimal(Convert.ToDecimal(after.fldAmount) / 100).ToString("#,##0.00");
                        ColName = "Deposit Amount";
                    }
                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }


                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string ChequeDataEntry_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Data Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldreasoncode" || key == "current_fldremark")
                {
                    if (key == "current_fldreasoncode")
                    {
                        if (before.fldReasonCode == after.fldReasonCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldReasonCode;
                        afterEdit = after.fldReasonCode;
                        ColName = "Cheque Status";
                    }
                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }


                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        //string ItemId
        public AuditTrailOCSModel CheckItemDataEntry(string TransNo)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            //sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemDataEntry", sqlParameterNext.ToArray());

            //for(int i = 0; i < dt.Rows.Count; i++)
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    //DataRow row = resultTable.Rows[0];
                    item.fldAmount = row["fldAmount"].ToString();
                    item.fldPVaccNo = row["fldPVaccNo"].ToString();
                    item.fldRemark = row["fldRemark"].ToString();
                }
            }
            else
            {
                item = null;
            }
            return item;
        }


        //public List<AuditTrailOCSModel> CheckItemDataEntry(string TransNo)
        //{
        //    List<AuditTrailOCSModel> dataModels = new List<AuditTrailOCSModel>();

        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));

        //    try
        //    {
        //        DataTable dataTable = new DataTable();
        //        dataTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemDataEntry", sqlParameterNext.ToArray());
        //        if (dataTable.Rows.Count > 0)
        //        {
        //            foreach (DataRow row in dataTable.Rows)
        //            {
        //                AuditTrailOCSModel dataModel = new AuditTrailOCSModel()
        //                {
        //                    fldAmount = row["fldAmount"].ToString(),
        //                    fldPVaccNo = row["fldPVaccNo"].ToString(),
        //                    fldRemark = row["fldRemark"].ToString()

        //                };
        //                dataModels.Add(dataModel);
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        throw exception;
        //    }
        //    return dataModels;
        //}


        public AuditTrailOCSModel CheckItemDataEntryReject(string ItemId, string TransNo)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemRejectDataEntry", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                item.fldReasonCode = row["fldReasonCode"].ToString();
                item.fldRemark = row["fldRemark"].ToString();

            }
            else
            {
                item = null;
            }
            return item;
        }

        #endregion

        #region balancing
        public AuditTrailOCSModel CheckItemBalancing(string TransNo)
        {

            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            //sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemBalancing", sqlParameterNext.ToArray());

            //for(int i = 0; i < dt.Rows.Count; i++)
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    //DataRow row = resultTable.Rows[0];
                    //item.fldAmount = row["fldAmount"].ToString();
                    //item.fldOldAmount = row["fldOldAmount"].ToString();
                    item.fldRemark = row["fldRemark"].ToString();
                }
            }
            else
            {
                item = null;
            }
            return item;
        }


        public List<AuditTrailOCSModel> ListBalancing(string ItemId, string TransNo)
        {
            List<AuditTrailOCSModel> balancingModels = new List<AuditTrailOCSModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemBalancing_V", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        AuditTrailOCSModel balancingModel = new AuditTrailOCSModel()
                        {
                            fldAmount = string.Format("{0:c}", Convert.ToDecimal(row["fldAmount"].ToString()) / 100).Remove(0,1),
                            fldOldAmount = string.Format("{0:c}", Convert.ToDecimal(row["fldOldAmount"].ToString()) / 100).Remove(0, 1),
                            fldRemark = row["fldRemark"].ToString() 
                        
                        };
                        balancingModels.Add(balancingModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return balancingModels;
        }

        public List<AuditTrailOCSModel> ListBalancingC(string ItemId, string TransNo)
        {
            List<AuditTrailOCSModel> balancingModels = new List<AuditTrailOCSModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemBalancing_C", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        AuditTrailOCSModel balancingModel = new AuditTrailOCSModel()
                        {
                            fldAmount = string.Format("{0:c}", Convert.ToDecimal(row["fldAmount"].ToString()) / 100).Remove(0, 1),
                            fldOldAmount = string.Format("{0:c}", Convert.ToDecimal(row["fldOldAmount"].ToString()) / 100).Remove(0, 1),
                            fldRemark = row["fldRemark"].ToString()

                        };
                        balancingModels.Add(balancingModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return balancingModels;
        }

        //AuditTrailOCSModel before, AuditTrailOCSModel after
        public string ChequeBalancing_Confirm(AuditTrailOCSModel before, AuditTrailOCSModel after, string beforeBalancingAmountV, string afterBalancingAmountV,
           string beforeBalancingAmountC,string afterBalancingAmountC, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Balancing - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");
            //key == "current_fldremark" || 
            foreach (string key in item.AllKeys)
            {
                if (key == "hfdChqAmt" || key == "current_fldamount" || key == "current_fldremark")
                {
                    
                       if (key == "hfdChqAmt") { 
                        if (beforeBalancingAmountC == afterBalancingAmountC)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = beforeBalancingAmountC;
                        afterEdit = afterBalancingAmountC;
                        //beforeEdit = Convert.ToDecimal(Convert.ToDecimal(beforeBalancingAmountC) / 100).ToString("#,##0.00");
                        //afterEdit = Convert.ToDecimal(Convert.ToDecimal(afterBalancingAmountC) / 100).ToString("#,##0.00");
                        ColName = "Cheque Amount";
                    }
                        if (key == "current_fldamount")
                        {
                            if (beforeBalancingAmountV == afterBalancingAmountV)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }

                        beforeEdit = beforeBalancingAmountV; //Convert.ToDecimal(Convert.ToDecimal(beforeBalancingAmountV)).ToString("#,##0.00");
                        afterEdit = afterBalancingAmountV;//Convert.ToDecimal(Convert.ToDecimal(afterBalancingAmountV)).ToString("#,##0.00");
                            ColName = "Deposit Amount";
                        }

                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }

                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public AuditTrailOCSModel CheckItemRejectBalancing (string ItemId, string TransNo)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemRejectBalancing", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                item.fldReasonCode = row["fldReasonCode"].ToString();
                item.fldRemark = row["fldRemark"].ToString();

            }
            else
            {
                item = null;
            }
            return item;
        }

        public string ChequeBalancing_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Cheque Date & Amount Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldreasoncode" || key == "current_fldremark")
                {
                    if (key == "current_fldreasoncode")
                    {
                        if (before.fldReasonCode == after.fldReasonCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldReasonCode;
                        afterEdit = after.fldReasonCode;
                        ColName = "Cheque Status";
                    }
                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }


                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion


        #region search cheq
        public AuditTrailOCSModel CheckItemSearch(string ItemId, string TransNo)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            AuditTrailOCSModel item = new AuditTrailOCSModel();
            sqlParameterNext.Add(new SqlParameter("@fldItemId", ItemId));
            sqlParameterNext.Add(new SqlParameter("@fldTransNo", TransNo));
            //sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckItemRejectSearchCheque", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                item.fldReasonCode = row["fldReasonCode"].ToString();
                item.fldRemark = row["fldRemark"].ToString();

            }
            else
            {
                item = null;
            }
            return item;
        }


        public string ChequeSearch_Reject(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Cheque Date & Amount Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldreasoncode" || key == "current_fldremark")
                {
                    if (key == "current_fldreasoncode")
                    {
                        if (before.fldReasonCode == after.fldReasonCode)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldReasonCode;
                        afterEdit = after.fldReasonCode;
                        ColName = "Cheque Status";
                    }
                    else if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }


                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string ChequeSearch_Remark(AuditTrailOCSModel before, AuditTrailOCSModel after, string TableHeader, FormCollection item)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            //string chkWaive = "";
            //string Data2 = "";
            //string ClassName = "";
            //string BranchDesc = "";
            StringBuilder sb = new StringBuilder();



            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Cheque Date & Amount Entry - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in item.AllKeys)
            {
                if (key == "current_fldremark")
                {
                    if (key == "current_fldremark")
                    {
                        if (before.fldRemark == after.fldRemark)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldRemark;
                        afterEdit = after.fldRemark;
                        ColName = "Remarks";
                    }

                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>" + beforeEdit + "</td>");
                    sb.Append("<td>" + afterEdit + "</td>");
                    sb.Append("<td>" + Remarks + "</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }

            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

    }


}