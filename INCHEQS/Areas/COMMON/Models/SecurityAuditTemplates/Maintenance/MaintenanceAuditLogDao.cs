using INCHEQS.DataAccessLayer;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.Password;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.BranchCode;
using INCHEQS.Areas.COMMON.Models.InternalBranchKBZ;
using INCHEQS.Areas.COMMON.Models.InternalBranch;
using INCHEQS.Areas.COMMON.Models.ReturnCode;
using INCHEQS.Areas.COMMON.Models.AccountProfile;
using INCHEQS.ConfigVerification.VerificationLimit;
using INCHEQS.Areas.ICS.Models.LargeAmount;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.BankZone;
using INCHEQS.Areas.COMMON.Models.BankChargesType;
using INCHEQS.Areas.COMMON.Models.BankCharges;
using INCHEQS.Areas.OCS.Models.ScannerWorkStation;
using System.Globalization;
using INCHEQS.Areas.COMMON.Models.Holiday;
using INCHEQS.Common;
using INCHEQS.Areas.OCS.Models.OCSRetentionPeriod;
using INCHEQS.Areas.ICS.Models.ICSRetentionPeriod;
using INCHEQS.Areas.COMMON.Models.Message;
using INCHEQS.Areas.ICS.Models.NonConformanceFlag;
using INCHEQS.Areas.ICS.Models.TransCode;
using INCHEQS.Areas.COMMON.Models.StateCode;
using INCHEQS.Areas.ICS.Models.HighRiskAccount;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance
{
    public class MaintenanceAuditLogDao : IMaintenanceAuditLogDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;
        public MaintenanceAuditLogDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, IPasswordDao changePassword)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        #region Bank Code/Profile
        public string BankProfile_AddTemplate(string bankCode, string TableHeader, string SystemProfile,string bankType)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                string BankType = "";
                                string BankTypeDesc = "";

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    Data = BankType + " - " + BankTypeDesc;
                                }

                                ColName = "Bank Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankProfile_EditTemplate(string bankCode, BankCodeModel before, BankCodeModel after, string TableHeader,string bankType)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                beforeEdit = bankCode;
                                afterEdit = bankCode;
                                Remarks = "No Changes";
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                if (before.fldBankDesc == after.fldBankDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBankDesc;
                                afterEdit = after.fldBankDesc;
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                if (before.fldBankType != "" && before.fldBankType != null)
                                {
                                    string BankType = "";
                                    string BankTypeDesc = "";

                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", before.fldBankType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    before.fldBankType = BankType + " - " + BankTypeDesc;
                                }

                                if (after.fldBankType != "" && after.fldBankType != null)
                                {
                                    string BankType = "";
                                    string BankTypeDesc = "";

                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", after.fldBankType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    after.fldBankType = BankType + " - " + BankTypeDesc;
                                }

                                if (before.fldBankType == after.fldBankType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBankType;
                                afterEdit = after.fldBankType;
                                ColName = "Bank Type";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankProfile_DeleteTemplate(string bankCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            string bankType = bankCode.Substring(Math.Max(bankCode.Length - 2, 0));
            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                string BankType = "";
                                string BankTypeDesc = "";

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    Data = BankType + " - " + BankTypeDesc;
                                }

                                ColName = "Bank Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankProfileChecker_AddTemplate(string bankCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                string BankType = "";
                                string BankTypeDesc = "";

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    Data = BankType + " - " + BankTypeDesc;
                                }

                                ColName = "Bank Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankProfileChecker_EditTemplate(string bankCode, BankCodeModel before, BankCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                beforeEdit = bankCode;
                                afterEdit = bankCode;
                                Remarks = "No Changes";
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                if (before.fldBankDesc == after.fldBankDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBankDesc;
                                afterEdit = after.fldBankDesc;
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                if (before.fldBankType != "" && before.fldBankType != null)
                                {
                                    string BankType = "";
                                    string BankTypeDesc = "";

                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", before.fldBankType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    before.fldBankType = BankType + " - " + BankTypeDesc;
                                }

                                if (after.fldBankType != "" && after.fldBankType != null)
                                {
                                    string BankType = "";
                                    string BankTypeDesc = "";

                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", after.fldBankType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    after.fldBankType = BankType + " - " + BankTypeDesc;
                                }

                                if (before.fldBankType == after.fldBankType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBankType;
                                afterEdit = after.fldBankType;
                                ColName = "Bank Type";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankProfileChecker_DeleteTemplate(string bankCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankCodeDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankCode" || column.ToString() == "fldBankDesc" || column.ToString() == "fldBankType")
                        {
                            if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldBankDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Description";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                string BankType = "";
                                string BankTypeDesc = "";

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldBankType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgBankTypeById", sqlParameterNext2.ToArray());
                                    if (dt.Rows.Count > 0)
                                    {
                                        BankType = dt2.Rows[0]["fldBankType"].ToString();
                                        BankTypeDesc = dt2.Rows[0]["fldBankTypeDesc"].ToString();
                                    }
                                    Data = BankType + " - " + BankTypeDesc;
                                }

                                ColName = "Bank Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Function For Bank Code/Profile New
        public BankCodeModel GetBankCodeDataTemp(string bankID)
        {
            BankCodeModel bankCode = new BankCodeModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankID));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankCodeTempDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                bankCode.fldBankCode = row["fldBankCode"].ToString();
                bankCode.fldBankDesc = row["fldBankDesc"].ToString();
                bankCode.fldBankType = row["fldBankType"].ToString();
            }

            return bankCode;

        }
        #endregion

        #region Branch Profile New

        public string BranchProfile_AddTemplate(string branchId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (SystemProfile.Equals("N"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                Data = row[column].ToString();
                                ColName = "Business Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BranchProfile_EditTemplate(string branchId, BranchCodeModel before, BranchCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                beforeEdit = branchId;
                                afterEdit = branchId;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                if (before.bankType == after.bankType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.bankType;
                                afterEdit = after.bankType;
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                if (before.bankCode == after.bankCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.bankCode;
                                afterEdit = after.bankCode;
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                if (before.stateCode == after.stateCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.stateCode;
                                afterEdit = after.stateCode;
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                if (before.branchCode == after.branchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchCode;
                                afterEdit = after.branchCode;
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                if (before.businessType == after.businessType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.businessType;
                                afterEdit = after.businessType;
                                ColName = "Business Type";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BranchProfile_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                Data = row[column].ToString();
                                ColName = "Business Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BranchProfileChecker_AddTemplate(string branchId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (Action.Equals("Approve"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                Data = row[column].ToString();
                                ColName = "Business Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BranchProfileChecker_EditTemplate(string branchId, BranchCodeModel before, BranchCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                beforeEdit = branchId;
                                afterEdit = branchId;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                if (before.bankType == after.bankType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.bankType;
                                afterEdit = after.bankType;
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                if (before.bankCode == after.bankCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.bankCode;
                                afterEdit = after.bankCode;
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                if (before.stateCode == after.stateCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.stateCode;
                                afterEdit = after.stateCode;
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                if (before.branchCode == after.branchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchCode;
                                afterEdit = after.branchCode;
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                if (before.businessType == after.businessType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.businessType;
                                afterEdit = after.businessType;
                                ColName = "Business Type";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BranchProfileChecker_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBankType" || column.ToString() == "fldBankCode" || column.ToString() == "fldStateCode" ||
                            column.ToString() == "fldBranchCode" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldBusinessType")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBankType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Type";
                            }
                            else if (column.ToString() == "fldBankCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Code";
                            }
                            else if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Code";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldBusinessType")
                            {
                                Data = row[column].ToString();
                                ColName = "Business Type";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Function Branch Profile New

        public BranchCodeModel GetBranchCodeDataById(string branchId, string action)
        {
            BranchCodeModel branchCode = new BranchCodeModel();
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (action == "Master")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }
            else if (action == "Temp")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgBranchProfile", sqlParameterNext.ToArray());
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                branchCode.branchId = row["fldBranchId"].ToString();
                branchCode.bankType = row["fldBankType"].ToString();
                branchCode.bankCode = row["fldBankCode"].ToString();
                branchCode.stateCode = row["fldStateCode"].ToString();
                branchCode.branchCode = row["fldBranchCode"].ToString();
                branchCode.branchDesc = row["fldBranchDesc"].ToString();
                branchCode.businessType = row["fldBusinessType"].ToString();
            }

            return branchCode;

        }

        #endregion

        #region Internal Branch Profile (KBZ)
        public string InternalBranchKBZ_AddTemplate(string branchId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempById", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }
                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchKBZ_EditTemplate(string branchId, InternalBranchKBZModel before, InternalBranchKBZModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                if (before.internalBranchCode == after.internalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.internalBranchCode;
                                afterEdit = after.internalBranchCode;
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                if (before.clearingBranchID == after.clearingBranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.clearingBranchID;
                                afterEdit = after.clearingBranchID;
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                if (before.BankZoneCode == after.BankZoneCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.BankZoneCode;
                                afterEdit = after.BankZoneCode;
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                if (before.subcenter == "Y")
                                {
                                    before.subcenter = "Yes";
                                }
                                else if (before.subcenter == "N")
                                {
                                    before.subcenter = "No";
                                }
                                if (after.subcenter == "Y")
                                {
                                    after.subcenter = "Yes";
                                }
                                else if (after.subcenter == "N")
                                {
                                    after.subcenter = "No";
                                }

                                if (before.subcenter == after.subcenter)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.subcenter;
                                afterEdit = after.subcenter;

                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.email == after.email)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.email;
                                afterEdit = after.email;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.country == after.country)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.country;
                                afterEdit = after.country;
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.active == "Y")
                                {
                                    before.active = "Yes";
                                }
                                else if (before.active == "N")
                                {
                                    before.active = "No";
                                }
                                if (after.active == "Y")
                                {
                                    after.active = "Yes";
                                }
                                else if (after.active == "N")
                                {
                                    after.active = "No";
                                }

                                if (before.active == after.active)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.active;
                                afterEdit = after.active;
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchKBZ_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }
                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchKBZChecker_AddTemplate(string branchId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempById", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }
                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchKBZChecker_EditTemplate(string branchId, InternalBranchKBZModel before, InternalBranchKBZModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                if (before.internalBranchCode == after.internalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.internalBranchCode;
                                afterEdit = after.internalBranchCode;
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                if (before.clearingBranchID == after.clearingBranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.clearingBranchID;
                                afterEdit = after.clearingBranchID;
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                if (before.BankZoneCode == after.BankZoneCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.BankZoneCode;
                                afterEdit = after.BankZoneCode;
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                if (before.subcenter == "Y")
                                {
                                    before.subcenter = "Yes";
                                }
                                else if (before.subcenter == "N")
                                {
                                    before.subcenter = "No";
                                }
                                if (after.subcenter == "Y")
                                {
                                    after.subcenter = "Yes";
                                }
                                else if (after.subcenter == "N")
                                {
                                    after.subcenter = "No";
                                }

                                if (before.subcenter == after.subcenter)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.subcenter;
                                afterEdit = after.subcenter;

                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.email == after.email)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.email;
                                afterEdit = after.email;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.country == after.country)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.country;
                                afterEdit = after.country;
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.active == "Y")
                                {
                                    before.active = "Yes";
                                }
                                else if (before.active == "N")
                                {
                                    before.active = "No";
                                }
                                if (after.active == "Y")
                                {
                                    after.active = "Yes";
                                }
                                else if (after.active == "N")
                                {
                                    after.active = "No";
                                }

                                if (before.active == after.active)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.active;
                                afterEdit = after.active;
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchKBZChecker_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldBankZoneCode" || column.ToString() == "fldSubcenter" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldCountry" ||
                            column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldSubcenter")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }
                                ColName = "Sub Center";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Functions for Internal Branch (KBZ)
        public InternalBranchKBZModel GetInternalBranchData(string id)
        {
            InternalBranchKBZModel internalBranch = new InternalBranchKBZModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                internalBranch.branchId = row["fldBranchId"].ToString();
                internalBranch.branchDesc = row["fldBranchDesc"].ToString();
                internalBranch.internalBranchCode = row["fldInternalBranchCode"].ToString();
                internalBranch.clearingBranchID = row["fldClearingBranchId"].ToString();
                internalBranch.BankZoneCode = row["fldBankZoneCode"].ToString();
                internalBranch.subcenter = row["fldSubcenter"].ToString();
                internalBranch.email = row["fldEmailAddress"].ToString();

                internalBranch.address1 = row["fldAddress1"].ToString();
                internalBranch.address2 = row["fldAddress2"].ToString();
                internalBranch.address3 = row["fldAddress3"].ToString();
                internalBranch.postCode = row["fldPostCode"].ToString();
                internalBranch.city = row["fldCity"].ToString();
                internalBranch.country = row["fldCountry"].ToString();

                internalBranch.active = row["fldActive"].ToString();
            }

            return internalBranch;

        }

        public InternalBranchKBZModel GetInternalBranchDataTemp(string id)
        {
            InternalBranchKBZModel internalBranch = new InternalBranchKBZModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                internalBranch.branchId = row["fldBranchId"].ToString();
                internalBranch.branchDesc = row["fldBranchDesc"].ToString();
                internalBranch.internalBranchCode = row["fldInternalBranchCode"].ToString();
                internalBranch.clearingBranchID = row["fldClearingBranchId"].ToString();
                internalBranch.BankZoneCode = row["fldBankZoneCode"].ToString();
                internalBranch.subcenter = row["fldSubcenter"].ToString();
                internalBranch.email = row["fldEmailAddress"].ToString();

                internalBranch.address1 = row["fldAddress1"].ToString();
                internalBranch.address2 = row["fldAddress2"].ToString();
                internalBranch.address3 = row["fldAddress3"].ToString();
                internalBranch.postCode = row["fldPostCode"].ToString();
                internalBranch.city = row["fldCity"].ToString();
                internalBranch.country = row["fldCountry"].ToString();

                internalBranch.active = row["fldActive"].ToString();
            }

            return internalBranch;

        }

        #endregion

        #region Internal Branch Profile MAB
        public string InternalBranch_AddTemplate(string branchId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempByIdMAB", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Email Address";

                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                                Remarks = "Newly Added";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranch_EditTemplate(string branchId, InternalBranchModel before, InternalBranchModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                if (before.internalBranchCode == after.internalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.internalBranchCode;
                                afterEdit = after.internalBranchCode;
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                if (before.clearingBranchID == after.clearingBranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.clearingBranchID;
                                afterEdit = after.clearingBranchID;
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.email == after.email)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.email == "" && after.email != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.email;
                                afterEdit = after.email;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.country == after.country)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.country;
                                afterEdit = after.country;
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.active == "Y")
                                {
                                    before.active = "Yes";
                                }
                                else if (before.active == "N")
                                {
                                    before.active = "No";
                                }
                                if (after.active == "Y")
                                {
                                    after.active = "Yes";
                                }
                                else if (after.active == "N")
                                {
                                    after.active = "No";
                                }

                                if (before.active == after.active)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.active;
                                afterEdit = after.active;
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranch_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";

                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchChecker_AddTemplate(string branchId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempByIdMAB", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Email Address";

                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                                Remarks = "Newly Added";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchChecker_EditTemplate(string branchId, InternalBranchModel before, InternalBranchModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                if (before.branchDesc == after.branchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.branchDesc;
                                afterEdit = after.branchDesc;
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                if (before.internalBranchCode == after.internalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.internalBranchCode;
                                afterEdit = after.internalBranchCode;
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                if (before.clearingBranchID == after.clearingBranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.clearingBranchID;
                                afterEdit = after.clearingBranchID;
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.email == after.email)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.email == "" && after.email != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.email;
                                afterEdit = after.email;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.country == after.country)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.country;
                                afterEdit = after.country;
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.active == "Y")
                                {
                                    before.active = "Yes";
                                }
                                else if (before.active == "N")
                                {
                                    before.active = "No";
                                }
                                if (after.active == "Y")
                                {
                                    after.active = "Yes";
                                }
                                else if (after.active == "N")
                                {
                                    after.active = "No";
                                }

                                if (before.active == after.active)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.active;
                                afterEdit = after.active;
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchChecker_DeleteTemplate(string branchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBranchId" || column.ToString() == "fldBranchDesc" || column.ToString() == "fldInternalBranchCode" ||
                            column.ToString() == "fldClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" ||
                            column.ToString() == "fldAddress2" || column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" ||
                            column.ToString() == "fldCity" || column.ToString() == "fldCountry" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch Description";
                            }
                            else if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";

                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post  Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();

                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else if (Data == "N")
                                {
                                    Data = "No";
                                }

                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Functions for Internal Branch Profile MAB
        public InternalBranchModel GetInternalBranchDataMAB(string id)
        {
            InternalBranchModel internalBranchMAB = new InternalBranchModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataByIdMAB", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                internalBranchMAB.branchId = row["fldBranchId"].ToString();
                internalBranchMAB.branchDesc = row["fldBranchDesc"].ToString();
                internalBranchMAB.internalBranchCode = row["fldInternalBranchCode"].ToString();
                internalBranchMAB.clearingBranchID = row["fldClearingBranchId"].ToString();
                internalBranchMAB.email = row["fldEmailAddress"].ToString();
                internalBranchMAB.address1 = row["fldAddress1"].ToString();
                internalBranchMAB.address2 = row["fldAddress2"].ToString();
                internalBranchMAB.address3 = row["fldAddress3"].ToString();
                internalBranchMAB.postCode = row["fldPostCode"].ToString();
                internalBranchMAB.city = row["fldCity"].ToString();
                internalBranchMAB.country = row["fldCountry"].ToString();

                internalBranchMAB.active = row["fldActive"].ToString();
            }

            return internalBranchMAB;

        }

        public InternalBranchModel GetInternalBranchDataTempMAB(string id)
        {
            InternalBranchModel internalBranchMAB = new InternalBranchModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempByIdMAB", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                internalBranchMAB.branchId = row["fldBranchId"].ToString();
                internalBranchMAB.branchDesc = row["fldBranchDesc"].ToString();
                internalBranchMAB.internalBranchCode = row["fldInternalBranchCode"].ToString();
                internalBranchMAB.clearingBranchID = row["fldClearingBranchId"].ToString();
                internalBranchMAB.email = row["fldEmailAddress"].ToString();
                internalBranchMAB.address1 = row["fldAddress1"].ToString();
                internalBranchMAB.address2 = row["fldAddress2"].ToString();
                internalBranchMAB.address3 = row["fldAddress3"].ToString();
                internalBranchMAB.postCode = row["fldPostCode"].ToString();
                internalBranchMAB.city = row["fldCity"].ToString();
                internalBranchMAB.country = row["fldCountry"].ToString();

                internalBranchMAB.active = row["fldActive"].ToString();
            }

            return internalBranchMAB;

        }
        #endregion

        #region Return Code
        //public string ReturnCode_AddTemplate(string rejectCode, string TableHeader, string SystemProfile)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Data = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));

        //    if (SystemProfile.Equals("N"))
        //    {
        //        dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());
        //    }
        //    else if (SystemProfile.Equals("Y"))
        //    {
        //        dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterTempById", sqlParameterNext.ToArray());
        //    }

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        Data = row[column].ToString();
        //                        if (Data == "Y" || Data == "Yes")
        //                        {
        //                            Data = "Yes";
        //                        }
        //                        else if (Data == "N" || Data == "No")
        //                        {
        //                            Data = "No";
        //                        }

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>-</td>");
        //                    sb.Append("<td>" + Data + "</td>");
        //                    sb.Append("<td>Newly Added</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        //public string ReturnCode_EditTemplate(string rejectCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Remarks = "";
        //    string beforeEdit = "";
        //    string afterEdit = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));
        //    dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        beforeEdit = rejectCode;
        //                        afterEdit = rejectCode;
        //                        Remarks = "No Changes";
        //                        ColName = "Reject Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        if (before.rejectDesc == after.rejectDesc)
        //                        {
        //                            Remarks = "No Changes";
        //                        }
        //                        else
        //                        {
        //                            Remarks = "Value Edited";
        //                        }
        //                        beforeEdit = before.rejectDesc;
        //                        afterEdit = after.rejectDesc;
        //                        ColName = "Reject Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        if (before.active == "Y" || before.active == "Yes")
        //                        {
        //                            before.active = "Yes";
        //                        }
        //                        else if (before.active == "N" || before.active == "No")
        //                        {
        //                            before.active = "No";
        //                        }
        //                        if (after.active == "Y" || after.active == "Yes")
        //                        {
        //                            after.active = "Yes";
        //                        }
        //                        else if (after.active == "N" || after.active == "No")
        //                        {
        //                            after.active = "No";
        //                        }

        //                        if (before.active == after.active)
        //                        {
        //                            Remarks = "No Changes";
        //                        }
        //                        else
        //                        {
        //                            Remarks = "Value Edited";
        //                        }
        //                        beforeEdit = before.active;
        //                        afterEdit = after.active;

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>" + beforeEdit + "</td>");
        //                    sb.Append("<td>" + afterEdit + "</td>");
        //                    sb.Append("<td>" + Remarks + "</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        //public string ReturnCode_DeleteTemplate(string rejectCode, string TableHeader)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Data = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));
        //    dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        Data = row[column].ToString();
        //                        if (Data == "Y" || Data == "Yes")
        //                        {
        //                            Data = "Yes";
        //                        }
        //                        else if (Data == "N" || Data == "No")
        //                        {
        //                            Data = "No";
        //                        }

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>" + Data + "</td>");
        //                    sb.Append("<td>-</td>");
        //                    sb.Append("<td>Deleted</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        //public string ReturnCodeChecker_AddTemplate(string rejectCode, string TableHeader, string Action)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Data = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));

        //    if (Action.Equals("Approve"))
        //    {
        //        dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());
        //    }
        //    else if (Action.Equals("Reject"))
        //    {
        //        dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterTempById", sqlParameterNext.ToArray());
        //    }

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        Data = row[column].ToString();
        //                        if (Data == "Y" || Data == "Yes")
        //                        {
        //                            Data = "Yes";
        //                        }
        //                        else if (Data == "N" || Data == "No")
        //                        {
        //                            Data = "No";
        //                        }

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>-</td>");
        //                    sb.Append("<td>" + Data + "</td>");
        //                    sb.Append("<td>Newly Added</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        //public string ReturnCodeChecker_EditTemplate(string rejectCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Remarks = "";
        //    string beforeEdit = "";
        //    string afterEdit = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));
        //    dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        beforeEdit = rejectCode;
        //                        afterEdit = rejectCode;
        //                        Remarks = "No Changes";
        //                        ColName = "Reject Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        if (before.rejectDesc == after.rejectDesc)
        //                        {
        //                            Remarks = "No Changes";
        //                        }
        //                        else
        //                        {
        //                            Remarks = "Value Edited";
        //                        }
        //                        beforeEdit = before.rejectDesc;
        //                        afterEdit = after.rejectDesc;
        //                        ColName = "Reject Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        if (before.active == "Y" || before.active == "Yes")
        //                        {
        //                            before.active = "Yes";
        //                        }
        //                        else if (before.active == "N" || before.active == "No")
        //                        {
        //                            before.active = "No";
        //                        }
        //                        if (after.active == "Y" || after.active == "Yes")
        //                        {
        //                            after.active = "Yes";
        //                        }
        //                        else if (after.active == "N" || after.active == "No")
        //                        {
        //                            after.active = "No";
        //                        }

        //                        if (before.active == after.active)
        //                        {
        //                            Remarks = "No Changes";
        //                        }
        //                        else
        //                        {
        //                            Remarks = "Value Edited";
        //                        }
        //                        beforeEdit = before.active;
        //                        afterEdit = after.active;

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>" + beforeEdit + "</td>");
        //                    sb.Append("<td>" + afterEdit + "</td>");
        //                    sb.Append("<td>" + Remarks + "</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        //public string ReturnCodeChecker_DeleteTemplate(string rejectCode, string TableHeader)
        //{
        //    string Result = "";
        //    int SerialNo = 1;
        //    string ColName = "";
        //    string Data = "";

        //    StringBuilder sb = new StringBuilder();

        //    DataTable dt = new DataTable();
        //    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        //    sqlParameterNext.Add(new SqlParameter("@fldRejectCode", rejectCode));
        //    dt = dbContext.GetRecordsAsDataTableSP("spcgRejectMasterById", sqlParameterNext.ToArray());

        //    //Table start.
        //    sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
        //    //Adding HeaderRow.
        //    sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
        //    //Add Rows
        //    sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

        //    if (dt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in dt.Rows)
        //        {
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldactive")
        //                {
        //                    if (column.ToString() == "fldRejectCode")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Code";
        //                    }
        //                    else if (column.ToString() == "fldRejectDesc")
        //                    {
        //                        Data = row[column].ToString();
        //                        ColName = "Return Description";
        //                    }
        //                    else if (column.ToString() == "fldactive")
        //                    {
        //                        Data = row[column].ToString();
        //                        if (Data == "Y" || Data == "Yes")
        //                        {
        //                            Data = "Yes";
        //                        }
        //                        else if (Data == "N" || Data == "No")
        //                        {
        //                            Data = "No";
        //                        }

        //                        ColName = "Active";
        //                    }
        //                    sb.Append("<tr>");
        //                    sb.Append("<td>" + SerialNo + "</td>");
        //                    sb.Append("<td>" + ColName + "</td>");
        //                    sb.Append("<td>" + Data + "</td>");
        //                    sb.Append("<td>-</td>");
        //                    sb.Append("<td>Deleted</td>");
        //                    sb.Append("</tr>");

        //                    SerialNo++;
        //                }

        //            }
        //        }
        //    }
        //    //Table end.
        //    sb.Append("</table>");
        //    Result = sb.ToString();

        //    return Result;
        //}

        #endregion

        #region Verification Threshold
        public string VerificationThreshold_AddTemplate(string bankCode, string type, string sequence, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckTempExist2", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                Data = row[column].ToString();
                                Data = Data + ".00";
                                ColName = "Threshold Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationThreshold_EditTemplate(string bankCode, string type, string sequence, ThresholdModel before, ThresholdModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                if (before.thresholdAmount == after.thresholdAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.thresholdAmount + ".00";
                                afterEdit = after.thresholdAmount + ".00";

                                ColName = "Threshold Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationThreshold_DeleteTemplate(string bankCode, string type, string sequence, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());
            
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                Data = row[column].ToString();
                                Data = Data + ".00";
                                ColName = "Threshold Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationThresholdChecker_AddTemplate(string bankCode, string type, string sequence, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckTempExist2", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                Data = row[column].ToString();
                                Data = Data + ".00";
                                ColName = "Threshold Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationThresholdChecker_EditTemplate(string bankCode, string type, string sequence, ThresholdModel before, ThresholdModel after, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgCheckTempExist2", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                if (before.thresholdAmount == after.thresholdAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.thresholdAmount + ".00";
                                afterEdit = after.thresholdAmount + ".00";

                                ColName = "Threshold Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationThresholdChecker_DeleteTemplate(string bankCode, string type, string sequence, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Threshold (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldVerificationThresholdType" || column.ToString() == "fldVerificationThresholdLevel" || column.ToString() == "fldVerificationThresholdAmt")
                        {
                            if (column.ToString() == "fldVerificationThresholdType")
                            {
                                Data = row[column].ToString();
                                if (Data == "A")
                                {
                                    Data = "Approve";
                                }
                                else if (Data == "R")
                                {
                                    Data = "Reject";
                                }
                                ColName = "Threshold Type";
                            }
                            else if (column.ToString() == "fldVerificationThresholdLevel")
                            {
                                Data = row[column].ToString();
                                ColName = "Threshold Level";
                            }
                            else if (column.ToString() == "fldVerificationThresholdAmt")
                            {
                                Data = row[column].ToString();
                                Data = Data + ".00";
                                ColName = "Threshold Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Functions for Verfication Threshold

        public ThresholdModel GetThresholdData(string type, string sequence, string bankCode)
        {
            ThresholdModel threshold = new ThresholdModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckExist2", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                threshold.thresholdType = row["fldVerificationThresholdType"].ToString();
                threshold.thresholdLevel = row["fldVerificationThresholdLevel"].ToString();
                threshold.thresholdAmount = row["fldVerificationThresholdAmt"].ToString();

            }

            return threshold;

        }

        public ThresholdModel GetThresholdDataTemp(string type, string sequence, string bankCode)
        {
            ThresholdModel threshold = new ThresholdModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdType", type));

            if (sequence.Equals("First Level Amount"))
            {
                sequence = "1";
            }
            else if (sequence.Equals("Second Level Amount"))
            {
                sequence = "2";
            }

            sqlParameterNext.Add(new SqlParameter("@fldVerificationThresholdSeq", sequence));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCheckTempExist2", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                threshold.thresholdType = row["fldVerificationThresholdType"].ToString();
                threshold.thresholdLevel = row["fldVerificationThresholdLevel"].ToString();
                threshold.thresholdAmount = row["fldVerificationThresholdAmt"].ToString();

            }

            return threshold;

        }

        #endregion

        #region Account Profile
        public string AccountProfile_AddTemplate(string accountNumber, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileTempById", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Name";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Type";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Status";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Opening Date";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Customer Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Contact Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                Remarks = "Newly Added";
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Postcode";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                                Remarks = "Newly Added";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string AccountProfile_EditTemplate(string accountNumber, AccountProfileModel before, AccountProfileModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));
            dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                ColName = "Account Number";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                if (before.accountName == after.accountName)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountName;
                                afterEdit = after.accountName;
                                ColName = "Account Name";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                if (before.accountType == after.accountType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountType;
                                afterEdit = after.accountType;
                                ColName = "Account Type";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                if (before.accountStatus == after.accountStatus)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountStatus;
                                afterEdit = after.accountStatus;
                                ColName = "Account Status";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                if (before.BranchID == after.BranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.BranchID;
                                afterEdit = after.BranchID;
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                if (before.OpeningDate == after.OpeningDate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.OpeningDate;
                                afterEdit = after.OpeningDate;
                                ColName = "Opening Date";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                if (before.ClosingDate == after.ClosingDate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.ClosingDate;
                                afterEdit = after.ClosingDate;
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                if (before.customerNumber == after.customerNumber)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.customerNumber;
                                afterEdit = after.customerNumber;
                                ColName = "Customer Number";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                if (before.contactNumber == after.contactNumber)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.contactNumber;
                                afterEdit = after.contactNumber;
                                ColName = "Contact Number";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.emailAddress == after.emailAddress)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.emailAddress;
                                afterEdit = after.emailAddress;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Postcode";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.countryCode == after.countryCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.countryCode;
                                afterEdit = after.countryCode;
                                ColName = "Country";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string AccountProfile_DeleteTemplate(string accountNumber, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";


            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));
            dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Name";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Type";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Status";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Opening Date";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Customer Number";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Contact Number";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Postcode";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string AccountProfileChecker_AddTemplate(string accountNumber, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileTempById", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Name";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Type";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Status";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Opening Date";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Customer Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Contact Number";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                Remarks = "Newly Added";
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Postcode";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                                Remarks = "Newly Added";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string AccountProfileChecker_EditTemplate(string accountNumber, AccountProfileModel before, AccountProfileModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));
            dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                ColName = "Account Number";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                if (before.accountName == after.accountName)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountName;
                                afterEdit = after.accountName;
                                ColName = "Account Name";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                if (before.accountType == after.accountType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountType;
                                afterEdit = after.accountType;
                                ColName = "Account Type";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                if (before.accountStatus == after.accountStatus)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.accountStatus;
                                afterEdit = after.accountStatus;
                                ColName = "Account Status";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                if (before.BranchID == after.BranchID)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.BranchID;
                                afterEdit = after.BranchID;
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                if (before.OpeningDate == after.OpeningDate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.OpeningDate;
                                afterEdit = after.OpeningDate;
                                ColName = "Opening Date";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                if (before.ClosingDate == after.ClosingDate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.ClosingDate;
                                afterEdit = after.ClosingDate;
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                if (before.customerNumber == after.customerNumber)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.customerNumber;
                                afterEdit = after.customerNumber;
                                ColName = "Customer Number";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                if (before.contactNumber == after.contactNumber)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.contactNumber;
                                afterEdit = after.contactNumber;
                                ColName = "Contact Number";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.emailAddress == after.emailAddress)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.emailAddress;
                                afterEdit = after.emailAddress;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.address1 == after.address1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address1;
                                afterEdit = after.address1;
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.address2 == after.address2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address2;
                                afterEdit = after.address2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.address3 == after.address3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.address3;
                                afterEdit = after.address3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.postCode == after.postCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.postCode;
                                afterEdit = after.postCode;
                                ColName = "Postcode";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.city == after.city)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.city;
                                afterEdit = after.city;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                if (before.countryCode == after.countryCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.countryCode;
                                afterEdit = after.countryCode;
                                ColName = "Country";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string AccountProfileChecker_DeleteTemplate(string accountNumber, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", accountNumber));
            dt = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Account Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldAccountNumber" || column.ToString() == "fldAccountName" || column.ToString() == "fldAccountType" ||
                            column.ToString() == "fldAccountStatus" || column.ToString() == "fldBranchId" || column.ToString() == "fldOpeningDate" ||
                            column.ToString() == "fldClosingDate" || column.ToString() == "fldCIFNumber" || column.ToString() == "fldContactNumber" ||
                            column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" ||
                            column.ToString() == "fldCountry")
                        {
                            if (column.ToString() == "fldAccountNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldAccountName")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Name";
                            }
                            else if (column.ToString() == "fldAccountType")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Type";
                            }
                            else if (column.ToString() == "fldAccountStatus")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Status";
                            }
                            else if (column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Branch ID";
                            }
                            else if (column.ToString() == "fldOpeningDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Opening Date";
                            }
                            else if (column.ToString() == "fldClosingDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Closing Date";
                            }
                            else if (column.ToString() == "fldCIFNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Customer Number";
                            }
                            else if (column.ToString() == "fldContactNumber")
                            {
                                Data = row[column].ToString();
                                ColName = "Contact Number";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Postcode";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldCountry")
                            {
                                Data = row[column].ToString();
                                ColName = "Country";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Verification Limit
        public string VerificationLimit_AddTemplate(string classId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string FirstType = "";
            string SecondType = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stAmt" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndAmt")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();
                                ColName = "Verification Class";
                            }
                            else if (column.ToString() == "fld1stAmt")
                            {
                                Data = row[column].ToString();
                                if (SystemProfile.Equals("N"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt2 = new DataTable();
                                        List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                        sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                        dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext2.ToArray());
                                        if (dt2.Rows.Count > 0)
                                        {
                                            FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                        }

                                        Data = "Amount " + FirstType + " " + Data + ".00 ";
                                    }
                                }
                                else if (SystemProfile.Equals("Y"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt2 = new DataTable();
                                        List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                        sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                        dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext2.ToArray());
                                        if (dt2.Rows.Count > 0)
                                        {
                                            FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                        }

                                        Data = "Amount " + FirstType + " " + Data + ".00 ";
                                    }
                                }
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                Data = row[column].ToString();
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndAmt")
                            {
                                Data = row[column].ToString();
                                if (SystemProfile.Equals("N"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt3 = new DataTable();
                                        List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                        sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                        dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext3.ToArray());
                                        if (dt3.Rows.Count > 0)
                                        {
                                            SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                        }

                                        Data = "Amount " + SecondType + " " + Data + ".00 ";
                                    }
                                }
                                else if (SystemProfile.Equals("Y"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt3 = new DataTable();
                                        List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                        sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                        dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext3.ToArray());
                                        if (dt3.Rows.Count > 0)
                                        {
                                            SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                        }

                                        Data = "Amount " + SecondType + " " + Data + ".00 ";
                                    }
                                }
                                ColName = "2nd Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationLimit_EditTemplate(string classId, VerificationLimitModel before, VerificationLimitModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stType" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndType")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                ColName = "Verification Class";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fld1stType")
                            {
                                if (before.fld1stType == after.fld1stType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fld1stType + ".00";
                                afterEdit = after.fld1stType + ".00";
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                if (before.fldConcatenate == after.fldConcatenate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldConcatenate;
                                afterEdit = after.fldConcatenate;
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndType")
                            {
                                if (before.fld2ndType == after.fld2ndType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fld2ndType + ".00";
                                afterEdit = after.fld2ndType + ".00";
                                ColName = "2nd Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationLimit_DeleteTemplate(string classId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string FirstType = "";
            string SecondType = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());


            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stAmt" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndAmt")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();
                                ColName = "Verification Class";
                            }
                            else if (column.ToString() == "fld1stAmt")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                    }

                                    Data = "Amount " + FirstType + " " + Data + ".00 ";
                                }
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                Data = row[column].ToString();
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndAmt")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt3 = new DataTable();
                                    List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                    sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                    dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext3.ToArray());
                                    if (dt3.Rows.Count > 0)
                                    {
                                        SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                    }

                                    Data = "Amount " + SecondType + " " + Data + ".00 ";
                                }
                                ColName = "2nd Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationLimitChecker_AddTemplate(string classId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string FirstType = "";
            string SecondType = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stAmt" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndAmt")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();
                                ColName = "Verification Class";
                            }
                            else if (column.ToString() == "fld1stAmt")
                            {
                                Data = row[column].ToString();
                                if (Action.Equals("Approve"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt2 = new DataTable();
                                        List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                        sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                        dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext2.ToArray());
                                        if (dt2.Rows.Count > 0)
                                        {
                                            FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                        }

                                        Data = "Amount " + FirstType + " " + Data + ".00 ";
                                    }
                                }
                                else if (Action.Equals("Reject"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt2 = new DataTable();
                                        List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                        sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                        dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext2.ToArray());
                                        if (dt2.Rows.Count > 0)
                                        {
                                            FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                        }

                                        Data = "Amount " + FirstType + " " + Data + ".00 ";
                                    }
                                }
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                Data = row[column].ToString();
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndAmt")
                            {
                                Data = row[column].ToString();
                                if (Action.Equals("Approve"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt3 = new DataTable();
                                        List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                        sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                        dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext3.ToArray());
                                        if (dt3.Rows.Count > 0)
                                        {
                                            SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                        }

                                        Data = "Amount " + SecondType + " " + Data + ".00 ";
                                    }
                                }
                                else if (Action.Equals("Reject"))
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        DataTable dt3 = new DataTable();
                                        List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                        sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                        dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext3.ToArray());
                                        if (dt3.Rows.Count > 0)
                                        {
                                            SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                        }

                                        Data = "Amount " + SecondType + " " + Data + ".00 ";
                                    }
                                }
                                ColName = "2nd Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationLimitChecker_EditTemplate(string classId, VerificationLimitModel before, VerificationLimitModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stType" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndType")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                ColName = "Verification Class";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fld1stType")
                            {
                                if (before.fld1stType == after.fld1stType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fld1stType + ".00";
                                afterEdit = after.fld1stType + ".00";
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                if (before.fldConcatenate == after.fldConcatenate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldConcatenate;
                                afterEdit = after.fldConcatenate;
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndType")
                            {
                                if (before.fld2ndType == after.fld2ndType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fld2ndType + ".00";
                                afterEdit = after.fld2ndType + ".00";
                                ColName = "2nd Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string VerificationLimitChecker_DeleteTemplate(string classId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string FirstType = "";
            string SecondType = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());


            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Verification Limit (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldClass" || column.ToString() == "fld1stAmt" || column.ToString() == "fldConcatenate" || column.ToString() == "fld2ndAmt")
                        {
                            if (column.ToString() == "fldClass")
                            {
                                Data = row[column].ToString();
                                ColName = "Verification Class";
                            }
                            else if (column.ToString() == "fld1stAmt")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldClass", classId));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        FirstType = dt2.Rows[0]["fld1stType"].ToString();
                                    }

                                    Data = "Amount " + FirstType + " " + Data + ".00 ";
                                }
                                ColName = "1st Amount";
                            }
                            else if (column.ToString() == "fldConcatenate")
                            {
                                Data = row[column].ToString();
                                ColName = "Concatenate";
                            }
                            else if (column.ToString() == "fld2ndAmt")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt3 = new DataTable();
                                    List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
                                    sqlParameterNext3.Add(new SqlParameter("@fldClass", classId));
                                    dt3 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext3.ToArray());
                                    if (dt3.Rows.Count > 0)
                                    {
                                        SecondType = dt3.Rows[0]["fld2ndType"].ToString();
                                    }

                                    Data = "Amount " + SecondType + " " + Data + ".00 ";
                                }
                                ColName = "2nd Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Functions for Verification Limit

        public VerificationLimitModel GetVerificationLimitData(string classId)
        {
            VerificationLimitModel verificationLimit = new VerificationLimitModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMaster", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                verificationLimit.fld1stType = "Amount " + row["fld1stType"].ToString() + " " + row["fld1stAmt"].ToString();
                verificationLimit.fldConcatenate = row["fldConcatenate"].ToString();
                verificationLimit.fld2ndType = "Amount " + row["fld2ndType"].ToString() + " " + row["fld2ndAmt"].ToString();
            }

            return verificationLimit;

        }
        public VerificationLimitModel GetVerificationLimitDataTemp(string classId)
        {
            VerificationLimitModel verificationLimit = new VerificationLimitModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldClass", classId));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitMasterTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                verificationLimit.fld1stType = "Amount " + row["fld1stType"].ToString() + " " + row["fld1stAmt"].ToString();
                verificationLimit.fldConcatenate = row["fldConcatenate"].ToString();
                verificationLimit.fld2ndType = "Amount " + row["fld2ndType"].ToString() + " " + row["fld2ndAmt"].ToString();
            }

            return verificationLimit;

        }

        #endregion

        #region Large Amount Limit
        public string LargeAmountLimit_EditTemplate(string bankCode, LargeAmountModel before, LargeAmountModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimit", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Large Amount Limit - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldLargeAmt")
                        {
                            if (column.ToString() == "fldLargeAmt")
                            {
                                if (before.fldAmount == after.fldAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAmount;
                                afterEdit = after.fldAmount;
                                ColName = "Large Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string LargeAmountLimitChecker_EditTemplate(string bankCode, LargeAmountModel before, LargeAmountModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimit", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Large Amount Limit (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldLargeAmt")
                        {
                            if (column.ToString() == "fldLargeAmt")
                            {
                                if (before.fldAmount == after.fldAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAmount;
                                afterEdit = after.fldAmount;
                                ColName = "Large Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Functions for Large Amount Limit
        public LargeAmountModel GetLargeAmountTemp(string bankCode)
        {
            LargeAmountModel LargeAmount = new LargeAmountModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgLargeAmountLimitTemp", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                LargeAmount.fldAmount = row["fldLargeAmt"].ToString();
            }

            return LargeAmount;

        }

        #endregion

        #region Bank Host Status KBZ
        public string BankHostStatusKBZ_AddTemplate(string bankhostCode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMABTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Description";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                Data = row[column].ToString();
                                if (Data != " " && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                    dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", Data) });

                                    if (dt2.Rows.Count > 0)
                                    {
                                        ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    }

                                    Data = Data + " - " + ActionDesc;
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Bank Host Status Action";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankHostStatusKBZ_EditTemplate(string bankhostCode, BankHostStatusKBZModel before, BankHostStatusKBZModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                ColName = "Bank Host Status Code";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                if (before.fldBankHostStatusDesc == after.fldBankHostStatusDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankHostStatusDesc;
                                afterEdit = after.fldBankHostStatusDesc;

                                ColName = "Bank Host Status Description";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                DataTable dt2 = new DataTable();
                                DataTable dt3 = new DataTable();
                                List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", before.fldBankHostStatusAction) });
                                dt3 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", after.fldBankHostStatusAction) });

                                if (dt2.Rows.Count > 0 && dt3.Rows.Count > 0)
                                {
                                    ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    before.fldBankHostStatusAction = before.fldBankHostStatusAction + " - " + ActionDesc;

                                    ActionDesc = dt3.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    after.fldBankHostStatusAction = after.fldBankHostStatusAction + " - " + ActionDesc;
                                }

                                if (before.fldBankHostStatusAction == after.fldBankHostStatusAction)
                                {
                                    Remarks = "No Changes";
                                }
                                else if (before.fldBankHostStatusAction != after.fldBankHostStatusAction)
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.fldBankHostStatusAction == " " && after.fldBankHostStatusAction != " ")
                                {
                                    Remarks = "Newly Added";
                                }

                                beforeEdit = before.fldBankHostStatusAction;
                                afterEdit = after.fldBankHostStatusAction;

                                ColName = "Bank Host Status Action";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankHostStatusKBZ_DeleteTemplate(string bankhostCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Code";
                                Remarks = "Deleted";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Description";
                                Remarks = "Deleted";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                Data = row[column].ToString();
                                if (Data != " " && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                    dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", Data) });

                                    if (dt2.Rows.Count > 0)
                                    {
                                        ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    }

                                    Data = Data + " - " + ActionDesc;
                                    Remarks = "Deleted";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Bank Host Status Action";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankHostStatusKBZChecker_AddTemplate(string bankhostCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject") || Action.Equals("Approve2"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMABTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Code";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Description";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                Data = row[column].ToString();
                                if (Data != " " && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                    dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", Data) });

                                    if (dt2.Rows.Count > 0)
                                    {
                                        ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    }

                                    Data = Data + " - " + ActionDesc;
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Bank Host Status Action";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankHostStatusKBZChecker_EditTemplate(string bankhostCode, BankHostStatusKBZModel before, BankHostStatusKBZModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                ColName = "Bank Host Status Code";
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                if (before.fldBankHostStatusDesc == after.fldBankHostStatusDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankHostStatusDesc;
                                afterEdit = after.fldBankHostStatusDesc;

                                ColName = "Bank Host Status Description";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                DataTable dt2 = new DataTable();
                                DataTable dt3 = new DataTable();
                                List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", before.fldBankHostStatusAction) });
                                dt3 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", after.fldBankHostStatusAction) });

                                if (dt2.Rows.Count > 0 && dt3.Rows.Count > 0)
                                {
                                    ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    before.fldBankHostStatusAction = before.fldBankHostStatusAction + " - " + ActionDesc;

                                    ActionDesc = dt3.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    after.fldBankHostStatusAction = after.fldBankHostStatusAction + " - " + ActionDesc;
                                }
                                //else if (dt3.Rows.Count > 0)
                                //{
                                //    ActionDesc = dt3.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                //    after.fldBankHostStatusAction = after.fldBankHostStatusAction + " - " + ActionDesc;
                                //}

                                if (before.fldBankHostStatusAction == after.fldBankHostStatusAction)
                                {
                                    Remarks = "No Changes";
                                }
                                else if (before.fldBankHostStatusAction != after.fldBankHostStatusAction)
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.fldBankHostStatusAction == " " && after.fldBankHostStatusAction != " ")
                                {
                                    Remarks = "Newly Added";
                                }

                                beforeEdit = before.fldBankHostStatusAction;
                                afterEdit = after.fldBankHostStatusAction;

                                ColName = "Bank Host Status Action";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankHostStatusKBZChecker_DeleteTemplate(string bankhostCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string ActionDesc = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusMasterMAB", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Host Status (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankHostStatusCode" || column.ToString() == "fldBankHostStatusDesc" || column.ToString() == "fldBankHostStatusAction")
                        {
                            if (column.ToString() == "fldBankHostStatusCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Code";
                                Remarks = "Deleted";
                            }
                            else if (column.ToString() == "fldBankHostStatusDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Host Status Description";
                                Remarks = "Deleted";
                            }
                            else if (column.ToString() == "fldBankHostStatusAction")
                            {
                                Data = row[column].ToString();
                                if (Data != " " && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    string stmt = "Select fldBankHostStatusActionCode, fldBankHostStatusActionDesc from tblBankHostStatusActionMaster where fldBankHostStatusActionCode=@fldBankHostStatusActionCode";
                                    dt2 = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankHostStatusActionCode", Data) });

                                    if (dt2.Rows.Count > 0)
                                    {
                                        ActionDesc = dt2.Rows[0]["fldBankHostStatusActionDesc"].ToString();
                                    }

                                    Data = Data + " - " + ActionDesc;
                                    Remarks = "Deleted";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "Bank Host Status Action";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;

        }
        #endregion

        #region Functions for Bank Host Status KBZ
        public BankHostStatusKBZModel GetStatus(string bankhostCode)
        {
            BankHostStatusKBZModel status = new BankHostStatusKBZModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankHostStatusCode", bankhostCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankHostStatusApprovalStatus", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                status.statusCode = row["fldApproveStatus"].ToString();
            }

            return status;

        }

        #endregion

        #region Bank Zone KBZ
        public string BankZone_AddTemplate(string bankzonecode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankZoneTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankZone_EditTemplate(string bankzonecode, BankZoneModel before, BankZoneModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                if(before.fldBankZoneDesc == after.fldBankZoneDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankZoneDesc;
                                afterEdit = after.fldBankZoneDesc;

                                ColName = "Bank Zone Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankZone_DeleteTemplate(string bankzonecode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());
            
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankZoneChecker_AddTemplate(string bankzonecode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankZoneTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankZoneChecker_EditTemplate(string bankzonecode, BankZoneModel before, BankZoneModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                if (before.fldBankZoneDesc == after.fldBankZoneDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankZoneDesc;
                                afterEdit = after.fldBankZoneDesc;

                                ColName = "Bank Zone Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankZoneChecker_DeleteTemplate(string bankzonecode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", bankzonecode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankZone", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Zone Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankZoneCode" || column.ToString() == "fldBankZoneDesc")
                        {
                            if (column.ToString() == "fldBankZoneCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Code";
                            }
                            else if (column.ToString() == "fldBankZoneDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Zone Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region KBZ Bank Charges Type
        public string BankChargesType_AddTemplate(string bankchargestype, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTypeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesType_EditTemplate(string bankchargestype, BankChargesTypeModel before, BankChargesTypeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                if (before.fldBankChargesDesc == after.fldBankChargesDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesDesc;
                                afterEdit = after.fldBankChargesDesc;

                                ColName = "Bank Charges Type Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesType_DeleteTemplate(string bankchargestype, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesTypeChecker_AddTemplate(string bankchargestype, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesTypeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesTypeChecker_EditTemplate(string bankchargestype, BankChargesTypeModel before, BankChargesTypeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                if (before.fldBankChargesDesc == after.fldBankChargesDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesDesc;
                                afterEdit = after.fldBankChargesDesc;

                                ColName = "Bank Charges Type Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesTypeChecker_DeleteTemplate(string bankchargestype, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesType", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges Type (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBankChargesType" || column.ToString() == "fldBankChargesDesc")
                        {
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldBankChargesDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Bank Charges
        public string BankCharges_AddTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" || 
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" || 
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Product Code";
                            }
                            else if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                Data = row[column].ToString() + "%";
                                ColName = "Bank Charges Rate";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankCharges_EditTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, BankChargesModel before, BankChargesModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" ||
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" ||
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Product Code";
                            }
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                if (before.fldChequeAmtMin == after.fldChequeAmtMin)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldChequeAmtMin;
                                afterEdit = after.fldChequeAmtMin;

                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                if (before.fldChequeAmtMax == after.fldChequeAmtMax)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldChequeAmtMax;
                                afterEdit = after.fldChequeAmtMax;

                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                if (before.fldBankChargesAmount == after.fldBankChargesAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesAmount;
                                afterEdit = after.fldBankChargesAmount;

                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                if (before.fldBankChargesRate == after.fldBankChargesRate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesRate + "%";
                                afterEdit = after.fldBankChargesRate + "%";

                                ColName = "Bank Charges Rate";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankCharges_DeleteTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());
           
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" ||
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" ||
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Product Code";
                            }
                            else if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                Data = row[column].ToString() + "%";
                                ColName = "Bank Charges Rate";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesChecker_AddTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" ||
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" ||
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Product Code";
                            }
                            else if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                Data = row[column].ToString() + "%";
                                ColName = "Bank Charges Rate";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesChecker_EditTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, BankChargesModel before, BankChargesModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" ||
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" ||
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Product Code";
                            }
                            if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                if (before.fldChequeAmtMin == after.fldChequeAmtMin)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldChequeAmtMin;
                                afterEdit = after.fldChequeAmtMin;

                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                if (before.fldChequeAmtMax == after.fldChequeAmtMax)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldChequeAmtMax;
                                afterEdit = after.fldChequeAmtMax;

                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                if (before.fldBankChargesAmount == after.fldBankChargesAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesAmount;
                                afterEdit = after.fldBankChargesAmount;

                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                if (before.fldBankChargesRate == after.fldBankChargesRate)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBankChargesRate + "%";
                                afterEdit = after.fldBankChargesRate + "%";

                                ColName = "Bank Charges Rate";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string BankChargesChecker_DeleteTemplate(string bankCode, string productCode, string bankchargestype, string chequeAmtMin, string chequeAmtMax, string tblName, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankchargestype));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", chequeAmtMin));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", chequeAmtMax));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblName));
            dt = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Bank Charges (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldProductCode" || column.ToString() == "fldBankChargesType" ||
                            column.ToString() == "fldChequeAmtMin" || column.ToString() == "fldChequeAmtMax" ||
                            column.ToString() == "fldBankChargesAmount" || column.ToString() == "fldBankChargesRate")
                        {
                            if (column.ToString() == "fldProductCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Product Code";
                            }
                            else if (column.ToString() == "fldBankChargesType")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Type";
                            }
                            else if (column.ToString() == "fldChequeAmtMin")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Minimum)";
                            }
                            else if (column.ToString() == "fldChequeAmtMax")
                            {
                                Data = row[column].ToString();
                                ColName = "Cheque Amount (Maximum)";
                            }
                            else if (column.ToString() == "fldBankChargesAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Bank Charges Amount";
                            }
                            else if (column.ToString() == "fldBankChargesRate")
                            {
                                Data = row[column].ToString() + "%";
                                ColName = "Bank Charges Rate";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Functions for Bank Charges

        public BankChargesModel GetBankChargesbyView(string bankCode, string productCode, string bankChargesType, string minAmount, string maxAmount, string tblname)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            BankChargesModel data = new BankChargesModel();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProductCode", productCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankChargesType", bankChargesType));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMin", minAmount));
            sqlParameterNext.Add(new SqlParameter("@fldChequeAmtMax", maxAmount));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankChargesbyView", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                data.fldBankChargesType = row["fldBankChargesType"].ToString();
                data.fldProductCode = row["fldProductCode"].ToString();
                data.fldChequeAmtMin = row["fldChequeAmtMin"].ToString();
                data.fldChequeAmtMax = row["fldChequeAmtMax"].ToString();
                data.fldBankChargesAmount = row["fldBankChargesAmount"].ToString();
                data.fldBankChargesRate = row["fldBankChargesRate"].ToString();
            }
            else
            {
                data = null;
            }
            return data;
        }

        #endregion

        #region Terminal Scanner

        public string TerminalScanner_AddTemplate(string scannerId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            if (SystemProfile.Equals("N"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerId.ToString().PadLeft(3, '0')));
                dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerId));
                dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationTempDatabyId", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldScannerId" || 
                            column.ToString() == "fldscannertypeid" || column.ToString() == "fldScannerTypeId" || 
                            column.ToString() == "fldbranchid" || column.ToString() == "fldBranchId" || 
                            column.ToString() == "fldmacaddress1" || column.ToString() == "fldMACAddress1" || 
                            column.ToString() == "fldmacaddress2" || column.ToString() == "fldMACAddress2" || 
                            column.ToString() == "fldmacaddress3" || column.ToString() == "fldMACAddress3")
                        {
                            if (column.ToString() == "fldscannerid" || column.ToString() == "fldScannerId")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldscannertypeid" || column.ToString() == "fldScannerTypeId")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldbranchid" || column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldmacaddress1" || column.ToString() == "fldMACAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldmacaddress2" || column.ToString() == "fldMACAddress2")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3" || column.ToString() == "fldMACAddress3")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "MAC Address 3";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TerminalScanner_EditTemplate(string scannerId, ScannerWorkStationModel before, ScannerWorkStationModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldscannerid", scannerId.ToString().PadLeft(3, '0')));
            dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldscannertypeid" ||
                            column.ToString() == "fldbranchid" || column.ToString() == "fldmacaddress1" ||
                            column.ToString() == "fldmacaddress2" || column.ToString() == "fldmacaddress3" )
                        {
                            if (column.ToString() == "fldscannerid")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldscannertypeid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldbranchid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldmacaddress1")
                            {
                                if (before.MacAdd1 == after.MacAdd1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.MacAdd1;
                                afterEdit = after.MacAdd1;
                                ColName = "MAC Address 1";
                            }
                            else if (column.ToString() == "fldmacaddress2")
                            {
                                if (before.MacAdd2 == after.MacAdd3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.MacAdd2 == "" && after.MacAdd2 != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.MacAdd2;
                                afterEdit = after.MacAdd2;
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3")
                            {
                                if (before.MacAdd3 == after.MacAdd3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.MacAdd3 == "" && after.MacAdd3 != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.MacAdd3;
                                afterEdit = after.MacAdd3;
                                ColName = "MAC Address 3";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TerminalScanner_DeleteTemplate(string scannerId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldscannerid", scannerId.ToString().PadLeft(3, '0')));
            dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());
            
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldscannertypeid" || column.ToString() == "fldbranchid" ||
                            column.ToString() == "fldmacaddress1" || column.ToString() == "fldmacaddress2" || column.ToString() == "fldmacaddress3")
                        {
                            if (column.ToString() == "fldscannerid")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";
                            }
                            else if (column.ToString() == "fldscannertypeid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";
                            }
                            else if (column.ToString() == "fldbranchid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";
                            }
                            else if (column.ToString() == "fldmacaddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 1";
                            }
                            else if (column.ToString() == "fldmacaddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 3";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TerminalScannerChecker_AddTemplate(string scannerId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            if (Action.Equals("Approve"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerId.ToString().PadLeft(3, '0')));
                dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerId));
                dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationTempDatabyId", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldScannerId" ||
                            column.ToString() == "fldscannertypeid" || column.ToString() == "fldScannerTypeId" ||
                            column.ToString() == "fldbranchid" || column.ToString() == "fldBranchId" ||
                            column.ToString() == "fldmacaddress1" || column.ToString() == "fldMACAddress1" ||
                            column.ToString() == "fldmacaddress2" || column.ToString() == "fldMACAddress2" ||
                            column.ToString() == "fldmacaddress3" || column.ToString() == "fldMACAddress3")
                        {
                            if (column.ToString() == "fldscannerid" || column.ToString() == "fldScannerId")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldscannertypeid" || column.ToString() == "fldScannerTypeId")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldbranchid" || column.ToString() == "fldBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldmacaddress1" || column.ToString() == "fldMACAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 1";
                                Remarks = "Newly Added";
                            }
                            else if (column.ToString() == "fldmacaddress2" || column.ToString() == "fldMACAddress2")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3" || column.ToString() == "fldMACAddress3")
                            {
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    Remarks = "Newly Added";
                                }
                                else
                                {
                                    Remarks = "";
                                }
                                ColName = "MAC Address 3";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>" + Remarks + "</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TerminalScannerChecker_EditTemplate(string scannerId, ScannerWorkStationModel before, ScannerWorkStationModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldscannerid", scannerId.ToString().PadLeft(3, '0')));
            dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldscannertypeid" ||
                            column.ToString() == "fldbranchid" || column.ToString() == "fldmacaddress1" ||
                            column.ToString() == "fldmacaddress2" || column.ToString() == "fldmacaddress3")
                        {
                            if (column.ToString() == "fldscannerid")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldscannertypeid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldbranchid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldmacaddress1")
                            {
                                if (before.MacAdd1 == after.MacAdd1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.MacAdd1;
                                afterEdit = after.MacAdd1;
                                ColName = "MAC Address 1";
                            }
                            else if (column.ToString() == "fldmacaddress2")
                            {
                                if (before.MacAdd2 == after.MacAdd3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.MacAdd2 == "" && after.MacAdd2 != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.MacAdd2;
                                afterEdit = after.MacAdd2;
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3")
                            {
                                if (before.MacAdd3 == after.MacAdd3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (before.MacAdd3 == "" && after.MacAdd3 != "")
                                {
                                    Remarks = "Newly Added";
                                }
                                beforeEdit = before.MacAdd3;
                                afterEdit = after.MacAdd3;
                                ColName = "MAC Address 3";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TerminalScannerChecker_DeleteTemplate(string scannerId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldscannerid", scannerId.ToString().PadLeft(3, '0')));
            dt = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationDatabyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Terminal Scanner (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldscannerid" || column.ToString() == "fldscannertypeid" || column.ToString() == "fldbranchid" ||
                            column.ToString() == "fldmacaddress1" || column.ToString() == "fldmacaddress2" || column.ToString() == "fldmacaddress3")
                        {
                            if (column.ToString() == "fldscannerid")
                            {
                                Data = row[column].ToString().PadLeft(3, '0');
                                ColName = "Scanner ID";
                            }
                            else if (column.ToString() == "fldscannertypeid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Type";
                            }
                            else if (column.ToString() == "fldbranchid")
                            {
                                Data = row[column].ToString();
                                ColName = "Scanner Bank Branch";
                            }
                            else if (column.ToString() == "fldmacaddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 1";
                            }
                            else if (column.ToString() == "fldmacaddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 2";
                            }
                            else if (column.ToString() == "fldmacaddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "MAC Address 3";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Functions for Terminal Scanner

        public ScannerWorkStationModel GetScannerTempbyId(string scannerId)
        {
            ScannerWorkStationModel scanner = new ScannerWorkStationModel();
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldScannerId", scannerId));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgScannerWorkStationTempDatabyId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                scanner.ScannerId = row["fldScannerId"].ToString();
                scanner.ScannerTypeId = row["fldScannerTypeId"].ToString();
                scanner.BranchId = row["fldBranchId"].ToString();
                scanner.MacAdd1 = row["fldMACAddress1"].ToString();
                scanner.MacAdd2 = row["fldMACAddress2"].ToString();
                scanner.MacAdd3 = row["fldMACAddress3"].ToString();
            }

            return scanner;

        }

        #endregion

        #region Holiday Calendar

        public string HolidayCalendar_AddTemplate(FormCollection col, string getDate, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Recurring = "";
            string Day = "";
            string DayName = "";
            List<string> arrDays = new List<string>();
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar  - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (col["RecurringType"].ToString() == "Week" || col["RecurringType"].ToString() == "Year")
            {
                Recurring = "Yes";
            }
            else
            {
                Recurring = "No";
            }

            if (col.AllKeys.Contains("Days"))
            {
                if (col["Days"] != null)
                {
                    arrDays = col["Days"].Split(',').ToList();

                    foreach (string arrDay in arrDays)
                    {
                        if (arrDay == "1")
                        {
                            DayName = "Monday";
                        }
                        else if (arrDay == "2")
                        {
                            DayName = "Tuesday";
                        }
                        else if (arrDay == "3")
                        {
                            DayName = "Wednesday";
                        }
                        else if (arrDay == "4")
                        {
                            DayName = "Thursday";
                        }
                        else if (arrDay == "5")
                        {
                            DayName = "Friday";
                        }
                        else if (arrDay == "6")
                        {
                            DayName = "Saturday";
                        }
                        else if (arrDay == "7")
                        {
                            DayName = "Sunday";
                        }

                        Day = Day + DayName + "</br>";
                    }

                }
            }

            foreach (string key in col.AllKeys)
            {
                if (key == "OneTime" || key == "fldHolidayDescription" || key == "RecurringType" || key == "Days" || key == "fldDate" || key == "fldYearDate")
                {
                    if (key == "OneTime")
                    {
                        Data = getDate;
                        ColName = "Holiday Date";
                    }
                    else if (key == "fldHolidayDescription")
                    {
                        Data = col[key];
                        ColName = "Description";
                    }
                    else if (key == "RecurringType")
                    {
                        if (col["RecurringType"].ToString() == "Week")
                        {
                            Data = "Weekly - " + Day;
                        }
                        else if (col["RecurringType"].ToString() == "Onetime")
                        {
                            Data = "One Time";
                        }
                        else if (col["RecurringType"].ToString() == "Year")
                        {
                            Data = "Yearly";
                        }
                        ColName = "Recurring Type";
                    }
                    else if (key == "Days" || key == "fldDate" || key == "fldYearDate")
                    {
                        Data = Recurring;
                        ColName = "Recurring";
                    }
                    sb.Append("<tr>");
                    sb.Append("<td>" + SerialNo + "</td>");
                    sb.Append("<td>" + ColName + "</td>");
                    sb.Append("<td>-</td>");
                    sb.Append("<td>" + Data + "</td>");
                    sb.Append("<td>Newly Added</td>");
                    sb.Append("</tr>");

                    SerialNo++;
                }
            }

            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HolidayCalendar_EditTemplate(string holidayId, HolidayModel before, HolidayModel after, string tblname, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Day = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", holidayId));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHolidayDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHolidayDate" || column.ToString() == "fldHolidayDesc" ||
                            column.ToString() == "fldRecurrType" || column.ToString() == "fldRecurring")
                        {
                            if (column.ToString() == "fldHolidayDate")
                            {
                                Data = row[column].ToString();
                                //Data = DateUtils.formatDateToSqlyyyymmdd(Data);
                                ColName = "Holiday Date";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldHolidayDesc")
                            {
                                if (before.Desc == after.Desc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.Desc.ToString();
                                afterEdit = after.Desc.ToString();
                                ColName = "Description";
                            }
                            if (column.ToString() == "fldRecurrType")
                            {
                                Data = row[column].ToString();
                                if (Data == " ")
                                {
                                    Data = "One Time";
                                }
                                else if (Data == "Y")
                                {
                                    Data = "Yearly";
                                }
                                else if (Data == "W")
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        if(after.fldDayMon == "Y")
                                        {
                                            Day = "Monday </br>";
                                        }
                                        if (after.fldDayTue == "Y")
                                        {
                                            Day = Day + "Tuesday </br>";
                                        }
                                        if (after.fldDayWed == "Y")
                                        {
                                            Day = Day + "Wednesday </br>";
                                        }
                                        if (after.fldDayThu == "Y")
                                        {
                                            Day = Day + "Thursday </br>";
                                        }
                                        if (after.fldDayFri == "Y")
                                        {
                                            Day = Day + "Friday </br>";
                                        }
                                        if (after.fldDaySat == "Y")
                                        {
                                            Day = Day + "Saturday </br>";
                                        }
                                        if (after.fldDaySun == "Y")
                                        {
                                            Day = Day + "Sunday </br>";
                                        }
                                        Data = "Weekly - " + Day;
                                    }
                                }
                                ColName = "Recurring Type";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }

                            if (column.ToString() == "fldRecurring")
                            {
                                Data = row[column].ToString();
                                if (Data == "N")
                                {
                                    Data = "No";
                                }
                                else
                                {
                                    Data = "Yes";
                                }

                                ColName = "Recurring";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HolidayCalendar_DeleteTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Day = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", holidayId));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHolidayDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHolidayDate" || column.ToString() == "fldHolidayDesc" ||
                            column.ToString() == "fldRecurrType" || column.ToString() == "fldRecurring")
                        {
                            if (column.ToString() == "fldHolidayDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Holiday Date";
                            }
                            else if (column.ToString() == "fldHolidayDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Description";
                            }
                            if (column.ToString() == "fldRecurrType")
                            {
                                Data = row[column].ToString();
                                if (Data == " ")
                                {
                                    Data = "One Time";
                                }
                                else if (Data == "Y")
                                {
                                    Data = "Yearly";
                                }
                                else if (Data == "W")
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        if (sHoliday.fldDayMon == "Y")
                                        {
                                            Day = Day + "Monday </br>";
                                        }
                                        if (sHoliday.fldDayTue == "Y")
                                        {
                                            Day = Day + "Tuesday </br>";
                                        }
                                        if (sHoliday.fldDayWed == "Y")
                                        {
                                            Day = Day + "Wednesday </br>";
                                        }
                                        if (sHoliday.fldDayThu == "Y")
                                        {
                                            Day = Day + "Thursday </br>";
                                        }
                                        if (sHoliday.fldDayFri == "Y")
                                        {
                                            Day = Day + "Friday </br>";
                                        }
                                        if (sHoliday.fldDaySat == "Y")
                                        {
                                            Day = Day + "Saturday </br>";
                                        }
                                        if (sHoliday.fldDaySun == "Y")
                                        {
                                            Day = Day + "Sunday </br>";
                                        }
                                        Data = "Weekly - " + Day;
                                    }
                                }
                                ColName = "Recurring Type";
                            }

                            if (column.ToString() == "fldRecurring")
                            {
                                Data = row[column].ToString();
                                if (Data == "N")
                                {
                                    Data = "No";
                                }
                                else
                                {
                                    Data = "Yes";
                                }

                                ColName = "Recurring";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HolidayCalendarChecker_AddTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Day = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", holidayId));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHolidayDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar (Checker)- " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHolidayDate" || column.ToString() == "fldHolidayDesc" ||
                            column.ToString() == "fldRecurrType" || column.ToString() == "fldRecurring")
                        {
                            if (column.ToString() == "fldHolidayDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Holiday Date";
                            }
                            else if (column.ToString() == "fldHolidayDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Description";
                            }
                            if (column.ToString() == "fldRecurrType")
                            {
                                Data = row[column].ToString();
                                if (Data == " ")
                                {
                                    Data = "One Time";
                                }
                                else if (Data == "Y")
                                {
                                    Data = "Yearly";
                                }
                                else if (Data == "W")
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        if (sHoliday.fldDayMon == "Y")
                                        {
                                            Day = Day + "Monday </br>";
                                        }
                                        if (sHoliday.fldDayTue == "Y")
                                        {
                                            Day = Day + "Tuesday </br>";
                                        }
                                        if (sHoliday.fldDayWed == "Y")
                                        {
                                            Day = Day + "Wednesday </br>";
                                        }
                                        if (sHoliday.fldDayThu == "Y")
                                        {
                                            Day = Day + "Thursday </br>";
                                        }
                                        if (sHoliday.fldDayFri == "Y")
                                        {
                                            Day = Day + "Friday </br>";
                                        }
                                        if (sHoliday.fldDaySat == "Y")
                                        {
                                            Day = Day + "Saturday </br>";
                                        }
                                        if (sHoliday.fldDaySun == "Y")
                                        {
                                            Day = Day + "Sunday </br>";
                                        }
                                        Data = "Weekly - " + Day;
                                    }
                                }
                                ColName = "Recurring Type";
                            }

                            if (column.ToString() == "fldRecurring")
                            {
                                Data = row[column].ToString();
                                if (Data == "N")
                                {
                                    Data = "No";
                                }
                                else
                                {
                                    Data = "Yes";
                                }

                                ColName = "Recurring";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HolidayCalendarChecker_EditTemplate(string holidayId, HolidayModel before, HolidayModel after, string tblname, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Day = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", holidayId));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHolidayDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHolidayDate" || column.ToString() == "fldHolidayDesc" ||
                            column.ToString() == "fldRecurrType" || column.ToString() == "fldRecurring")
                        {
                            if (column.ToString() == "fldHolidayDate")
                            {
                                Data = row[column].ToString();
                                //Data = DateUtils.formatDateToSqlyyyymmdd(Data);
                                ColName = "Holiday Date";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldHolidayDesc")
                            {
                                if (before.Desc == after.Desc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.Desc.ToString();
                                afterEdit = after.Desc.ToString();
                                ColName = "Description";
                            }
                            if (column.ToString() == "fldRecurrType")
                            {
                                Data = row[column].ToString();
                                if (Data == " ")
                                {
                                    Data = "One Time";
                                }
                                else if (Data == "Y")
                                {
                                    Data = "Yearly";
                                }
                                else if (Data == "W")
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        if (after.fldDayMon == "Y")
                                        {
                                            Day = "Monday </br>";
                                        }
                                        if (after.fldDayTue == "Y")
                                        {
                                            Day = Day + "Tuesday </br>";
                                        }
                                        if (after.fldDayWed == "Y")
                                        {
                                            Day = Day + "Wednesday </br>";
                                        }
                                        if (after.fldDayThu == "Y")
                                        {
                                            Day = Day + "Thursday </br>";
                                        }
                                        if (after.fldDayFri == "Y")
                                        {
                                            Day = Day + "Friday </br>";
                                        }
                                        if (after.fldDaySat == "Y")
                                        {
                                            Day = Day + "Saturday </br>";
                                        }
                                        if (after.fldDaySun == "Y")
                                        {
                                            Day = Day + "Sunday </br>";
                                        }
                                        Data = "Weekly - " + Day;
                                    }
                                }
                                ColName = "Recurring Type";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }

                            if (column.ToString() == "fldRecurring")
                            {
                                Data = row[column].ToString();
                                if (Data == "N")
                                {
                                    Data = "No";
                                }
                                else
                                {
                                    Data = "Yes";
                                }

                                ColName = "Recurring";

                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HolidayCalendarChecker_DeleteTemplate(string holidayId, HolidayModel sHoliday, string tblname, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string Day = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", holidayId));
            sqlParameterNext.Add(new SqlParameter("@tblname", tblname));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHolidayDataById", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Holiday Calendar (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHolidayDate" || column.ToString() == "fldHolidayDesc" ||
                            column.ToString() == "fldRecurrType" || column.ToString() == "fldRecurring")
                        {
                            if (column.ToString() == "fldHolidayDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Holiday Date";
                            }
                            else if (column.ToString() == "fldHolidayDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Description";
                            }
                            if (column.ToString() == "fldRecurrType")
                            {
                                Data = row[column].ToString();
                                if (Data == " ")
                                {
                                    Data = "One Time";
                                }
                                else if (Data == "Y")
                                {
                                    Data = "Yearly";
                                }
                                else if (Data == "W")
                                {
                                    if (Data != "" && Data != null)
                                    {
                                        if (sHoliday.fldDayMon == "Y")
                                        {
                                            Day = Day + "Monday </br>";
                                        }
                                        if (sHoliday.fldDayTue == "Y")
                                        {
                                            Day = Day + "Tuesday </br>";
                                        }
                                        if (sHoliday.fldDayWed == "Y")
                                        {
                                            Day = Day + "Wednesday </br>";
                                        }
                                        if (sHoliday.fldDayThu == "Y")
                                        {
                                            Day = Day + "Thursday </br>";
                                        }
                                        if (sHoliday.fldDayFri == "Y")
                                        {
                                            Day = Day + "Friday </br>";
                                        }
                                        if (sHoliday.fldDaySat == "Y")
                                        {
                                            Day = Day + "Saturday </br>";
                                        }
                                        if (sHoliday.fldDaySun == "Y")
                                        {
                                            Day = Day + "Sunday </br>";
                                        }
                                        Data = "Weekly - " + Day;
                                    }
                                }
                                ColName = "Recurring Type";
                            }

                            if (column.ToString() == "fldRecurring")
                            {
                                Data = row[column].ToString();
                                if (Data == "N")
                                {
                                    Data = "No";
                                }
                                else
                                {
                                    Data = "Yes";
                                }

                                ColName = "Recurring";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Function for Holiday Checker
        public HolidayModel GetHolidayDatabyId(string id)
        {
            HolidayModel holiday = new HolidayModel();
            DataTable ds = new DataTable();
            string stmt = "Select * from tblHolidayMaster where fldHolidayId=@fldHolidayId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldHolidayId", id) });
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                if (row["fldHolidayDate"].ToString() == "")
                {
                    holiday.Date = "";
                }
                else
                {
                    holiday.Date = row["fldHolidayDate"].ToString();
                    //holiday.Date = DateTime.ParseExact(row["fldHolidayDate"].ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                }
                holiday.Desc = row["fldHolidayDesc"].ToString();
                holiday.recurring = row["fldRecurring"].ToString();
                
                if (holiday.recurring == "Y")
                {
                    holiday.recurring = "Recurring";
                    holiday.recurrType = row["fldRecurrType"].ToString();
                    if (holiday.recurrType == "M")
                    {
                        holiday.recurrType = "M";
                    }
                    if (holiday.recurrType == "W")
                    {
                        holiday.recurrType = "W";

                        if (row["fldDayMon"].ToString() == "Y")
                        {
                            holiday.fldDayMon = "Y";
                            holiday.MonChecked = true;
                        }
                        else
                        {
                            holiday.fldDayMon = "N";
                            holiday.MonChecked = false;
                        }
                        if (row["fldDayTue"].ToString() == "Y")
                        {
                            holiday.fldDayTue = "Y";
                            holiday.tueChecked = true;
                        }
                        else
                        {
                            holiday.fldDayTue = "N";
                            holiday.tueChecked = false;
                        }
                        if (row["fldDayWed"].ToString() == "Y")
                        {
                            holiday.fldDayWed = "Y";
                            holiday.wedChecked = true;
                        }
                        else
                        {
                            holiday.fldDayWed = "N";
                            holiday.wedChecked = false;
                        }
                        if (row["fldDayThu"].ToString() == "Y")
                        {
                            holiday.fldDayThu = "Y";
                            holiday.thuChecked = true;
                        }
                        else
                        {
                            holiday.fldDayThu = "N";
                            holiday.thuChecked = false;
                        }
                        if (row["fldDayFri"].ToString() == "Y")
                        {
                            holiday.fldDayFri = "Y";
                            holiday.friChecked = true;
                        }
                        else
                        {
                            holiday.fldDayFri = "N";
                            holiday.friChecked = false;
                        }
                        if (row["fldDaySat"].ToString() == "Y")
                        {
                            holiday.fldDaySat = "Y";
                            holiday.satChecked = true;
                        }
                        else
                        {
                            holiday.fldDaySat = "N";
                            holiday.satChecked = false;
                        }
                        if (row["fldDaySun"].ToString() == "Y")
                        {
                            holiday.fldDaySun = "Y";
                            holiday.sunChecked = true;
                        }
                        else
                        {
                            holiday.fldDaySun = "N";
                            holiday.sunChecked = false;
                        }
                    }
                    if (holiday.recurrType == "Y")
                    {
                        holiday.recurrType = "Year";
                        holiday.recurrTypeYearly = "True";
                    }
                }
                else
                {
                    holiday.recurring = "N";
                    holiday.recurrType = "OneTime";
                }

                holiday.HolidayId = row["fldHolidayId"].ToString();
            }
            return holiday;
        }

        public HolidayModel GetHolidayDataTempbyId(string id)
        {
            HolidayModel holiday = new HolidayModel();
            DataTable ds = new DataTable();
            string stmt = "Select * from tblHolidayMasterTemp where fldHolidayId=@fldHolidayId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldHolidayId", id) });
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                if (row["fldHolidayDate"].ToString() == "")
                {
                    holiday.Date = "";
                }
                else
                {
                    holiday.Date = row["fldHolidayDate"].ToString();
                    //holiday.Date = DateTime.ParseExact(row["fldHolidayDate"].ToString(), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                }
                holiday.Desc = row["fldHolidayDesc"].ToString();
                holiday.recurring = row["fldRecurring"].ToString();
                
                if (holiday.recurring == "Y")
                {
                    holiday.recurring = "Recurring";
                    holiday.recurrType = row["fldRecurrType"].ToString();
                    if (holiday.recurrType == "M")
                    {
                        holiday.recurrType = "M";
                    }
                    if (holiday.recurrType == "W")
                    {
                        holiday.recurrType = "W";

                        if (row["fldDayMon"].ToString() == "Y")
                        {
                            holiday.fldDayMon = "Y";
                            holiday.MonChecked = true;
                        }
                        else
                        {
                            holiday.fldDayMon = "N";
                            holiday.MonChecked = false;
                        }
                        if (row["fldDayTue"].ToString() == "Y")
                        {
                            holiday.fldDayTue = "Y";
                            holiday.tueChecked = true;
                        }
                        else
                        {
                            holiday.fldDayTue = "N";
                            holiday.tueChecked = false;
                        }
                        if (row["fldDayWed"].ToString() == "Y")
                        {
                            holiday.fldDayWed = "Y";
                            holiday.wedChecked = true;
                        }
                        else
                        {
                            holiday.fldDayWed = "N";
                            holiday.wedChecked = false;
                        }
                        if (row["fldDayThu"].ToString() == "Y")
                        {
                            holiday.fldDayThu = "Y";
                            holiday.thuChecked = true;
                        }
                        else
                        {
                            holiday.fldDayThu = "N";
                            holiday.thuChecked = false;
                        }
                        if (row["fldDayFri"].ToString() == "Y")
                        {
                            holiday.fldDayFri = "Y";
                            holiday.friChecked = true;
                        }
                        else
                        {
                            holiday.fldDayFri = "N";
                            holiday.friChecked = false;
                        }
                        if (row["fldDaySat"].ToString() == "Y")
                        {
                            holiday.fldDaySat = "Y";
                            holiday.satChecked = true;
                        }
                        else
                        {
                            holiday.fldDaySat = "N";
                            holiday.satChecked = false;
                        }
                        if (row["fldDaySun"].ToString() == "Y")
                        {
                            holiday.fldDaySun = "Y";
                            holiday.sunChecked = true;
                        }
                        else
                        {
                            holiday.fldDaySun = "N";
                            holiday.sunChecked = false;
                        }
                    }
                    if (holiday.recurrType == "Y")
                    {
                        holiday.recurrType = "Year";
                        holiday.recurrTypeYearly = "True";
                    }
                }
                else
                {
                    holiday.recurring = "N";
                    holiday.recurrType = "OneTime";
                }

                holiday.HolidayId = row["fldHolidayId"].ToString();
            }
            return holiday;
        }

        #endregion

        #region OCS Retention Period
        public string OCSRetentionPeriod_EditTemplate(OCSRetentionPeriodModel before, OCSRetentionPeriodModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgOCSRetentionPeriodField", sqlParameterNext.ToArray());
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            sb.Append("<tr><th colspan=5 height=20> Task Name: OCS Retention Period - " + TableHeader + " </th></tr>");
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");
            if (dt.Rows.Count > 0)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ToString() == "OC" || column.ToString() == "OCHistory" || column.ToString() == "IR")
                    {
                        if (column.ToString() == "OC")
                        {
                            beforeEdit = before.OCInt.ToString() + " " + before.OCIntType.ToString();
                            afterEdit = after.OCInt.ToString() + " " + after.OCIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Outward Clearing Retention Period";
                        }
                        if (column.ToString() == "OCHistory")
                        {
                            beforeEdit = before.OCHistoryInt.ToString() + " " + before.OCHistoryIntType.ToString();
                            afterEdit = after.OCHistoryInt.ToString() + " " + after.OCHistoryIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Outward Clearing History Retention Period";
                        }
                        if (column.ToString() == "IR")
                        {
                            beforeEdit = before.IRInt.ToString() + " " + before.IRIntType.ToString();
                            afterEdit = after.IRInt.ToString() + " " + after.IRIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Return Retention Period";
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
            }
            sb.Append("</table>");
            Result = sb.ToString();
            return Result;
        }
        public string OCSRetentionPeriodChecker_EditTemplate(OCSRetentionPeriodModel before, OCSRetentionPeriodModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgOCSRetentionPeriodField", sqlParameterNext.ToArray());
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            sb.Append("<tr><th colspan=5 height=20> Task Name: OCS Retention Period (Checker) - " + TableHeader + " </th></tr>");
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");
            if (dt.Rows.Count > 0)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ToString() == "OC" || column.ToString() == "OCHistory" || column.ToString() == "IR")
                    {
                        if (column.ToString() == "OC")
                        {
                            beforeEdit = before.OCInt.ToString() + " " + before.OCIntType.ToString();
                            afterEdit = after.OCInt.ToString() + " " + after.OCIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Outward Clearing Retention Period";
                        }
                        if (column.ToString() == "OCHistory")
                        {
                            beforeEdit = before.OCHistoryInt.ToString() + " " + before.OCHistoryIntType.ToString();
                            afterEdit = after.OCHistoryInt.ToString() + " " + after.OCHistoryIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Outward Clearing History Retention Period";
                        }
                        if (column.ToString() == "IR")
                        {
                            beforeEdit = before.IRInt.ToString() + " " + before.IRIntType.ToString();
                            afterEdit = after.IRInt.ToString() + " " + after.IRIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Return Retention Period";
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
            }
            sb.Append("</table>");
            Result = sb.ToString();
            return Result;
        }
        #endregion

        #region ICS Retention Period Checker
        public string ICSRetentionPeriod_EditTemplate(ICSRetentionPeriodModel before, ICSRetentionPeriodModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgICSRetentionPeriodField", sqlParameterNext.ToArray());
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            sb.Append("<tr><th colspan=5 height=20> Task Name: ICS Retention Period - " + TableHeader + " </th></tr>");
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");
            if (dt.Rows.Count > 0)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ToString() == "IC" || column.ToString() == "ICHistory")
                    {
                        if (column.ToString() == "IC")
                        {
                            beforeEdit = before.ICInt.ToString() + " " + before.ICIntType.ToString();
                            afterEdit = after.ICInt.ToString() + " " + after.ICIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Clearing Retention Period";
                        }
                        if (column.ToString() == "ICHistory")
                        {
                            beforeEdit = before.ICHistoryInt.ToString() + " " + before.ICHistoryIntType.ToString();
                            afterEdit = after.ICHistoryInt.ToString() + " " + after.ICHistoryIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Clearing History Retention Period";
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
            }
            sb.Append("</table>");
            Result = sb.ToString();
            return Result;
        }
        public string ICSRetentionPeriodChecker_EditTemplate(ICSRetentionPeriodModel before, ICSRetentionPeriodModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dt = dbContext.GetRecordsAsDataTableSP("spcgICSRetentionPeriodField", sqlParameterNext.ToArray());
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            sb.Append("<tr><th colspan=5 height=20> Task Name: ICS Retention Period (Checker) - " + TableHeader + " </th></tr>");
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");
            if (dt.Rows.Count > 0)
            {
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ToString() == "IC" || column.ToString() == "ICHistory")
                    {
                        if (column.ToString() == "IC")
                        {
                            beforeEdit = before.ICInt.ToString() + " " + before.ICIntType.ToString();
                            afterEdit = after.ICInt.ToString() + " " + after.ICIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Clearing Retention Period";
                        }
                        if (column.ToString() == "ICHistory")
                        {
                            beforeEdit = before.ICHistoryInt.ToString() + " " + before.ICHistoryIntType.ToString();
                            afterEdit = after.ICHistoryInt.ToString() + " " + after.ICHistoryIntType.ToString();
                            if (beforeEdit == afterEdit)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            ColName = "Inward Clearing History Retention Period";
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
            }
            sb.Append("</table>");
            Result = sb.ToString();
            return Result;
        }
        #endregion

        #region Message
        public string Message_AddTemplate(string Message, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessage", Message));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgMessage", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgMessageTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message Maintenance - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                Data = row[column].ToString();
                                ColName = "Message";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string Message_EditTemplate(string MessageId, MessageModel before, MessageModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                if (before.fldBroadcastMessage == after.fldBroadcastMessage)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBroadcastMessage;
                                afterEdit = after.fldBroadcastMessage;

                                ColName = "Message";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string Message_DeleteTemplate(string MessageId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                Data = row[column].ToString();
                                ColName = "Message";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string MessageChecker_AddTemplate(string MessageId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgBroadcastMessageMasterTempById", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                Data = row[column].ToString();
                                ColName = "Message";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string MessageChecker_EditTemplate(string MessageId, MessageModel before, MessageModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                if (before.fldBroadcastMessage == after.fldBroadcastMessage)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldBroadcastMessage;
                                afterEdit = after.fldBroadcastMessage;

                                ColName = "Message Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string MessageChecker_DeleteTemplate(string MessageId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBroadcastMessageId", MessageId));
            dt = dbContext.GetRecordsAsDataTableSP("spcgMessagebyId", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Message (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldBroadcastMessage")
                        {
                            if (column.ToString() == "fldBroadcastMessage")
                            {
                                Data = row[column].ToString();
                                ColName = "Message";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }


        #endregion

        #region Non-Cnformance Flag
        public string NCF_AddTemplate(string NCFCode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string NCF_EditTemplate(string NCFCode, NonConformanceFlagModel before, NonConformanceFlagModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                if (before.fldNCFDesc == after.fldNCFDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldNCFDesc;
                                afterEdit = after.fldNCFDesc;

                                ColName = "Non-Confromance Flag Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string NCF_DeleteTemplate(string NCFCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string NCFChecker_AddTemplate(string NCFCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string NCFChecker_EditTemplate(string NCFCode, NonConformanceFlagModel before, NonConformanceFlagModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                if (before.fldNCFDesc == after.fldNCFDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldNCFDesc;
                                afterEdit = after.fldNCFDesc;

                                ColName = "Non-Conformance Flag Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string NCFChecker_DeleteTemplate(string NCFCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", NCFCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Non-Conformance Flag (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldNCFCode" || column.ToString() == "fldNCFDesc")
                        {
                            if (column.ToString() == "fldNCFCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Code";
                            }
                            else if (column.ToString() == "fldNCFDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Non-Conformance Flag Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Function for NCF
        public NonConformanceFlagModel GetNCFCode(string ncfCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            NonConformanceFlagModel field = new NonConformanceFlagModel();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgNCFMaster", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    field.fldNCFCode = row["fldNCFCode"].ToString();
                    field.fldNCFDesc = row["fldNCFDesc"].ToString();
                }
            }
            else
            {
                field = null;
            }
            return field;
        }

        public NonConformanceFlagModel GetNCFCodeTemp(string ncfCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            NonConformanceFlagModel field = new NonConformanceFlagModel();
            sqlParameterNext.Add(new SqlParameter("@fldNCFCode", ncfCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingNCFCodeTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    field.fldNCFCode = row["fldNCFCode"].ToString();
                    field.fldNCFDesc = row["fldNCFDesc"].ToString();
                    field.fldApproveStatus = row["fldApproveStatus"].ToString();
                }
            }
            else
            {
                field = null;
            }
            return field;
        }

        #endregion

        #region Transaction Code
        public string TransCode_AddTemplate(string TransCode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMasterTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TransCode_EditTemplate(string TransCode, TransCodeModel before, TransCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                if (before.fldTransCodeDesc == after.fldTransCodeDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldTransCodeDesc;
                                afterEdit = after.fldTransCodeDesc;

                                ColName = "Transaction Code Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TransCode_DeleteTemplate(string TransCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TransCodeChecker_AddTemplate(string TransCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMasterTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TransCodeChecker_EditTemplate(string TransCode, TransCodeModel before, TransCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                if (before.fldTransCodeDesc == after.fldTransCodeDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldTransCodeDesc;
                                afterEdit = after.fldTransCodeDesc;

                                ColName = "Transaction Code Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string TransCodeChecker_DeleteTemplate(string TransCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", TransCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Transaction Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldTransCode" || column.ToString() == "fldTransCodeDesc")
                        {
                            if (column.ToString() == "fldTransCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code";
                            }
                            else if (column.ToString() == "fldTransCodeDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Transaction Code Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Function Transaction Code
        public TransCodeModel GetTransCode(string transCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            TransCodeModel field = new TransCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgTransCodeMaster", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    field.fldTransCode = row["fldTransCode"].ToString();
                    field.fldTransCodeDesc = row["fldTransCodeDesc"].ToString();
                }
            }
            else
            {
                field = null;
            }
            return field;
        }

        public TransCodeModel GetTransCodeTemp(string transCode)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            TransCodeModel field = new TransCodeModel();
            sqlParameterNext.Add(new SqlParameter("@fldTransCode", transCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgexisitingTransCodeMasterTemp", sqlParameterNext.ToArray()); //Done
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    field.fldTransCode = row["fldTransCode"].ToString();
                    field.fldTransCodeDesc = row["fldTransCodeDesc"].ToString();
                    field.fldApproveStatus = row["fldApproveStatus"].ToString();
                }
            }
            else
            {
                field = null;
            }
            return field;
        }
        #endregion

        #region Return Code 2
        public string RetCode_AddTemplate(string ReturnCode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else
                                {
                                    Data = "No";
                                }
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        Data = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                Data = row[column].ToString();
                                ColName = "Charges (RM)";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string RetCode_EditTemplate(string ReturnCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                if (before.fldRejectDesc == after.fldRejectDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldRejectDesc;
                                afterEdit = after.fldRejectDesc;

                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                if (before.fldRepresentable == "Y")
                                {
                                    before.fldRepresentable = "Yes";
                                }
                                else
                                {
                                    before.fldRepresentable = "No";
                                }
                                if (after.fldRepresentable == "Y")
                                {
                                    after.fldRepresentable = "Yes";
                                }
                                else
                                {
                                    after.fldRepresentable = "No";
                                }
                                if (before.fldRepresentable == after.fldRepresentable)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldRepresentable;
                                afterEdit = after.fldRepresentable;
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                if (before.fldRejectType != "" && before.fldRejectType != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", before.fldRejectType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        before.fldRejectType = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                if (after.fldRejectType != "" && after.fldRejectType != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", after.fldRejectType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        after.fldRejectType = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                if (before.fldRejectType == after.fldRejectType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldRejectType;
                                afterEdit = after.fldRejectType;

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                if (before.fldCharges == after.fldCharges)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldCharges;
                                afterEdit = after.fldCharges;

                                ColName = "Charges (RM)";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string RetCode_DeleteTemplate(string ReturnCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else
                                {
                                    Data = "No";
                                }
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        Data = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                Data = row[column].ToString();
                                ColName = "Charges (RM)";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string RetCodeChecker_AddTemplate(string ReturnCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else
                                {
                                    Data = "No";
                                }
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        Data = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                Data = row[column].ToString();
                                ColName = "Charges (RM)";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string RetCodeChecker_EditTemplate(string ReturnCode, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel before, INCHEQS.Areas.COMMON.Models.ReturnCode.ReturnCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;

                                Remarks = "No Changes";
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                if (before.fldRejectDesc == after.fldRejectDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldRejectDesc;
                                afterEdit = after.fldRejectDesc;

                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                if (before.fldRepresentable == "Y")
                                {
                                    before.fldRepresentable = "Yes";
                                }
                                else
                                {
                                    before.fldRepresentable = "No";
                                }
                                if (after.fldRepresentable == "Y")
                                {
                                    after.fldRepresentable = "Yes";
                                }
                                else
                                {
                                    after.fldRepresentable = "No";
                                }
                                if (before.fldRepresentable == after.fldRepresentable)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldRepresentable;
                                afterEdit = after.fldRepresentable;
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                if (before.fldRejectType != "" && before.fldRejectType != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", before.fldRejectType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        before.fldRejectType = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                if (after.fldRejectType != "" && after.fldRejectType != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", after.fldRejectType));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        after.fldRejectType = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                if (before.fldRejectType == after.fldRejectType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldRejectType;
                                afterEdit = after.fldRejectType;

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                if (before.fldCharges == after.fldCharges)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldCharges;
                                afterEdit = after.fldCharges;

                                ColName = "Charges (RM)";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string RetCodeChecker_DeleteTemplate(string ReturnCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldRejectCode", ReturnCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgReturnCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Return Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldRejectCode" || column.ToString() == "fldRejectDesc" || column.ToString() == "fldRepresentable" || column.ToString() == "fldRejectType" || column.ToString() == "fldCharges")
                        {
                            if (column.ToString() == "fldRejectCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code";
                            }
                            else if (column.ToString() == "fldRejectDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Return Code Description";
                            }
                            else if (column.ToString() == "fldRepresentable")
                            {
                                Data = row[column].ToString();
                                if (Data == "Y")
                                {
                                    Data = "Yes";
                                }
                                else
                                {
                                    Data = "No";
                                }
                                ColName = "Representable";
                            }
                            else if (column.ToString() == "fldRejectType")
                            {
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@fldRejectType", Data));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgRejectType", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        Data = dt2.Rows[0]["fldRejectTypeDesc"].ToString();
                                    }
                                }

                                ColName = "Return Type";
                            }
                            else if (column.ToString() == "fldCharges")
                            {
                                Data = row[column].ToString();
                                ColName = "Charges (RM)";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region State Code

        public string StateCode_AddTemplate(string StateCode, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));

            if (SystemProfile.Equals("N"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgStateCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "State Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string StateCode_EditTemplate(string StateCode, StateCodeModel before, StateCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                if (before.fldStateCode == after.fldStateCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldStateCode;
                                afterEdit = after.fldStateCode;

                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                if (before.fldStateDesc == after.fldStateDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldStateDesc;
                                afterEdit = after.fldStateDesc;

                                ColName = "State Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string StateCode_DeleteTemplate(string StateCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "State Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string StateCodeChecker_AddTemplate(string StateCode, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));

            if (Action.Equals("Approve"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgStateCodeTemp", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "State Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string StateCodeChecker_EditTemplate(string StateCode, StateCodeModel before, StateCodeModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Remarks = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                if (before.fldStateCode == after.fldStateCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldStateCode;
                                afterEdit = after.fldStateCode;

                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                if (before.fldStateDesc == after.fldStateDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldStateDesc;
                                afterEdit = after.fldStateDesc;

                                ColName = "State Description";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string StateCodeChecker_DeleteTemplate(string StateCode, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", StateCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "State Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion
        #region Credit Account


        public string CreditAccountChecker_DeleteTemplate(string AccountNumber, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldStateCode", AccountNumber));
            dt = dbContext.GetRecordsAsDataTableSP("spcgStateCode", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: State Code (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldStateCode" || column.ToString() == "fldStateDesc")
                        {
                            if (column.ToString() == "fldStateCode")
                            {
                                Data = row[column].ToString();
                                ColName = "State Code";
                            }
                            if (column.ToString() == "fldStateDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "State Description";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Internal Branch
        public string InternalBranchProfile_AddTemplate(string IbranchId, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));

            if (SystemProfile.Equals("N"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();
                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchProfile_EditTemplate(string IbranchId, InternalBranchModel before, InternalBranchModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                beforeEdit = before.fldInternalBranchId;
                                afterEdit = after.fldInternalBranchId;
                                ColName = "Business Type";
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                if (before.fldCBranchId == after.fldCBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCBranchId;
                                afterEdit = after.fldCBranchId;
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                if (before.fldCBranchDesc == after.fldCBranchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCBranchDesc;
                                afterEdit = after.fldCBranchDesc;
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                if (before.fldCInternalBranchCode == after.fldCInternalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCInternalBranchCode;
                                afterEdit = after.fldCInternalBranchCode;
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                if (before.fldCClearingBranchId == after.fldCClearingBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCClearingBranchId;
                                afterEdit = after.fldCClearingBranchId;
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                if (before.fldIBranchId == after.fldIBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIBranchId;
                                afterEdit = after.fldIBranchId;
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                if (before.fldIBranchDesc == after.fldIBranchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIBranchDesc;
                                afterEdit = after.fldIBranchDesc;
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                if (before.fldIInternalBranchCode == after.fldIInternalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIInternalBranchCode;
                                afterEdit = after.fldIInternalBranchCode;
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                if (before.fldIClearingBranchId == after.fldIClearingBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIClearingBranchId;
                                afterEdit = after.fldIClearingBranchId;
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.fldEmailAddress == after.fldEmailAddress)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldEmailAddress;
                                afterEdit = after.fldEmailAddress;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.fldAddress1 == after.fldAddress1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress1;
                                afterEdit = after.fldAddress1;
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.fldAddress2 == after.fldAddress2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress2;
                                afterEdit = after.fldAddress2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.fldAddress3 == after.fldAddress3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress3;
                                afterEdit = after.fldAddress3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.fldPostCode == after.fldPostCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldPostCode;
                                afterEdit = after.fldPostCode;
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.fldPostCode == after.fldPostCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldPostCode;
                                afterEdit = after.fldPostCode;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.fldActive == after.fldActive)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldActive;
                                afterEdit = after.fldActive;
                                ColName = "City";
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchProfile_DeleteTemplate(string IbranchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();
                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchProfileChecker_AddTemplate(string IbranchId, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));

            if (Action.Equals("Approve"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();
                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchProfileChecker_EditTemplate(string IbranchId, InternalBranchModel before, InternalBranchModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                beforeEdit = before.fldInternalBranchId;
                                afterEdit = after.fldInternalBranchId;
                                ColName = "Business Type";
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                if (before.fldCBranchId == after.fldCBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCBranchId;
                                afterEdit = after.fldCBranchId;
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                if (before.fldCBranchDesc == after.fldCBranchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCBranchDesc;
                                afterEdit = after.fldCBranchDesc;
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                if (before.fldCInternalBranchCode == after.fldCInternalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCInternalBranchCode;
                                afterEdit = after.fldCInternalBranchCode;
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                if (before.fldCClearingBranchId == after.fldCClearingBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldCClearingBranchId;
                                afterEdit = after.fldCClearingBranchId;
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                if (before.fldIBranchId == after.fldIBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIBranchId;
                                afterEdit = after.fldIBranchId;
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                if (before.fldIBranchDesc == after.fldIBranchDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIBranchDesc;
                                afterEdit = after.fldIBranchDesc;
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                if (before.fldIInternalBranchCode == after.fldIInternalBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIInternalBranchCode;
                                afterEdit = after.fldIInternalBranchCode;
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                if (before.fldIClearingBranchId == after.fldIClearingBranchId)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldIClearingBranchId;
                                afterEdit = after.fldIClearingBranchId;
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                if (before.fldEmailAddress == after.fldEmailAddress)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldEmailAddress;
                                afterEdit = after.fldEmailAddress;
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                if (before.fldAddress1 == after.fldAddress1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress1;
                                afterEdit = after.fldAddress1;
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                if (before.fldAddress2 == after.fldAddress2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress2;
                                afterEdit = after.fldAddress2;
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                if (before.fldAddress3 == after.fldAddress3)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldAddress3;
                                afterEdit = after.fldAddress3;
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                if (before.fldPostCode == after.fldPostCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldPostCode;
                                afterEdit = after.fldPostCode;
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                if (before.fldPostCode == after.fldPostCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldPostCode;
                                afterEdit = after.fldPostCode;
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                if (before.fldActive == after.fldActive)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldActive;
                                afterEdit = after.fldActive;
                                ColName = "City";
                                ColName = "Active";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string InternalBranchProfileChecker_DeleteTemplate(string IbranchId, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Internal Branch Profile (Checker)- " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchId" || column.ToString() == "fldCBranchId" || column.ToString() == "fldCBranchDesc" || column.ToString() == "fldCInternalBranchCode" ||
                            column.ToString() == "fldCClearingBranchId" || column.ToString() == "fldIBranchId" || column.ToString() == "fldIBranchDesc" || column.ToString() == "fldIInternalBranchCode" ||
                            column.ToString() == "fldIClearingBranchId" || column.ToString() == "fldEmailAddress" || column.ToString() == "fldAddress1" || column.ToString() == "fldAddress2" ||
                            column.ToString() == "fldAddress3" || column.ToString() == "fldPostCode" || column.ToString() == "fldCity" || column.ToString() == "fldActive")
                        {
                            if (column.ToString() == "fldInternalBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch ID";
                            }
                            else if (column.ToString() == "fldCBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Branch Description";
                            }
                            else if (column.ToString() == "fldCInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Internal Branch Code";
                            }
                            else if (column.ToString() == "fldCClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Conventional Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch ID";
                            }
                            else if (column.ToString() == "fldIBranchDesc")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Branch Description";
                            }
                            else if (column.ToString() == "fldIInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Internal Branch Code";
                            }
                            else if (column.ToString() == "fldIClearingBranchId")
                            {
                                Data = row[column].ToString();
                                ColName = "Islamic Clearing Branch ID";
                            }
                            else if (column.ToString() == "fldEmailAddress")
                            {
                                Data = row[column].ToString();
                                ColName = "Email Address";
                            }
                            else if (column.ToString() == "fldAddress1")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 1";
                            }
                            else if (column.ToString() == "fldAddress2")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 2";
                            }
                            else if (column.ToString() == "fldAddress3")
                            {
                                Data = row[column].ToString();
                                ColName = "Address 3";
                            }
                            else if (column.ToString() == "fldPostCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Post Code";
                            }
                            else if (column.ToString() == "fldCity")
                            {
                                Data = row[column].ToString();
                                ColName = "City";
                            }
                            else if (column.ToString() == "fldActive")
                            {
                                Data = row[column].ToString();
                                ColName = "Active";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }
        #endregion

        #region Function for Internal Branch Profile
        public InternalBranchModel GetInternalBranchDataById(string IbranchId, string action)
        {
            InternalBranchModel IbranchCode = new InternalBranchModel();
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", IbranchId));

            if (action == "Master")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }
            else if (action == "Temp")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchProfile", sqlParameterNext.ToArray());
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                IbranchCode.fldInternalBranchId = row["fldInternalBranchId"].ToString();
                IbranchCode.fldCBranchId = row["fldCBranchId"].ToString();
                IbranchCode.fldCBranchDesc = row["fldCBranchDesc"].ToString();
                IbranchCode.fldCInternalBranchCode = row["fldCInternalBranchCode"].ToString();
                IbranchCode.fldCClearingBranchId = row["fldCClearingBranchId"].ToString();
                IbranchCode.fldIBranchId = row["fldIBranchId"].ToString();
                IbranchCode.fldIBranchDesc = row["fldIBranchDesc"].ToString();
                IbranchCode.fldIInternalBranchCode = row["fldIInternalBranchCode"].ToString();
                IbranchCode.fldIClearingBranchId = row["fldIClearingBranchId"].ToString();
                //IbranchCode.fldEmailAddress = row["fldEmailAddress"].ToString();
                //IbranchCode.fldAddress1 = row["fldAddress1"].ToString();
                //IbranchCode.fldAddress2 = row["fldAddress2"].ToString();
                //IbranchCode.fldAddress3 = row["fldAddress3"].ToString();
                //IbranchCode.fldPostCode = row["fldPostCode"].ToString();
                //IbranchCode.fldCity = row["fldCity"].ToString();
                IbranchCode.fldActive = row["fldActive"].ToString();

            }

            return IbranchCode;
        }

        #endregion
        #region High Risk Account
        public string HighRiskAccount_AddTemplate(string fldHighRiskAccount, string TableHeader, string SystemProfile)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));

            if (SystemProfile.Equals("N"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }
            else if (SystemProfile.Equals("Y"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HighRiskAccount_EditTemplate(string fldHighRiskAccount, HighRiskAccountModel before, HighRiskAccountModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                beforeEdit = before.fldInternalBranchCode;
                                afterEdit = after.fldInternalBranchCode;
                                Remarks = "No Changes";
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                beforeEdit = before.fldHighRiskAccount;
                                afterEdit = after.fldHighRiskAccount;
                                Remarks = "No Changes";
                                ColName = "High Risk Account";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                if (before.fldHighRiskAmount == after.fldHighRiskAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldHighRiskAmount;
                                afterEdit = after.fldHighRiskAmount;
                                ColName = "High Risk Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HighRiskAccount_DeleteTemplate(string fldHighRiskAccount, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HighRiskAccountChecker_AddTemplate(string fldHighRiskAccount, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));

            if (Action.Equals("Approve"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }
            else if (Action.Equals("Reject"))
            {
                sqlParameterNext.Add(new SqlParameter("@Action", "Temp"));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>Newly Added</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HighRiskAccountChecker_EditTemplate(string fldHighRiskAccount, HighRiskAccountModel before, HighRiskAccountModel after, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Remarks = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                beforeEdit = before.fldInternalBranchCode;
                                afterEdit = after.fldInternalBranchCode;
                                Remarks = "No Changes";
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                beforeEdit = before.fldHighRiskAccount;
                                afterEdit = after.fldHighRiskAccount;
                                Remarks = "No Changes";
                                ColName = "High Risk Account";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                if (before.fldHighRiskAmount == after.fldHighRiskAmount)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldHighRiskAmount;
                                afterEdit = after.fldHighRiskAmount;
                                ColName = "High Risk Amount";
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
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        public string HighRiskAccountChecker_DeleteTemplate(string fldHighRiskAccount, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));
            sqlParameterNext.Add(new SqlParameter("@Action", "Master"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: High Risk Account (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldInternalBranchCode" || column.ToString() == "fldHighRiskAccount" || column.ToString() == "fldHighRiskAmount")
                        {
                            if (column.ToString() == "fldInternalBranchCode")
                            {
                                Data = row[column].ToString();
                                ColName = "Internal Branch Code";
                            }
                            else if (column.ToString() == "fldHighRiskAccount")
                            {
                                Data = row[column].ToString();
                                ColName = "Account Number";
                            }
                            else if (column.ToString() == "fldHighRiskAmount")
                            {
                                Data = row[column].ToString();
                                ColName = "Amount";
                            }
                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td>Deleted</td>");
                            sb.Append("</tr>");

                            SerialNo++;
                        }

                    }
                }
            }
            //Table end.
            sb.Append("</table>");
            Result = sb.ToString();

            return Result;
        }

        #endregion

        #region Function High Risk Account
        public HighRiskAccountModel GetHighRiskAccount(string fldHighRiskAccount, string action)
        {
            HighRiskAccountModel IbranchCode = new HighRiskAccountModel();
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHighRiskAccount", fldHighRiskAccount));

            if (action == "Master")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }
            else if (action == "Temp")
            {
                sqlParameterNext.Add(new SqlParameter("@Action", action));
                dt = dbContext.GetRecordsAsDataTableSP("spcgHighRiskAccount", sqlParameterNext.ToArray());
            }

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                IbranchCode.fldInternalBranchCode = row["fldInternalBranchCode"].ToString();
                IbranchCode.fldHighRiskAccount = row["fldHighRiskAccount"].ToString();
                IbranchCode.fldHighRiskAmount = row["fldHighRiskAmount"].ToString();
            }

            return IbranchCode;

        }
        #endregion
    }
}