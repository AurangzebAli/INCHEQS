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
using INCHEQS.Areas.COMMON.Models.HubUser;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Areas.COMMON.Models.Users;
using INCHEQS.Security;
using INCHEQS.Areas.COMMON.Models.Branch;
using INCHEQS.Common;

namespace INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates
{
    public class SecurityAuditLogDao : ISecurityAuditLogDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IPasswordDao changePassword;
        public SecurityAuditLogDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, IPasswordDao changePassword)
        {
            this.dbContext = dbContext;
            this.changePassword = changePassword;
            this.systemProfileDao = systemProfileDao;
        }

        #region ClearUserSession
        public string UserSession_Template(string TableHeader, string user)
        {
            string Result = "";
            int SerialNo = 1;
            //string Remarks = "";
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", user));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforUserSession", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserLoginId" || column.ToString() == "fldUserName" || column.ToString() == "row_fldLastLoginDate")
                        {
                            if (column.ToString() == "fldUserLoginId")
                            {
                                Data = row[column].ToString();
                                ColName = "User Login ID";
                                //Remarks = "Deleted";
                            }
                            else if (column.ToString() == "fldUserName")
                            {
                                Data = row[column].ToString();
                                ColName = "User Name";
                                //Remarks = "Deleted";
                            }
                            else if (column.ToString() == "row_fldLastLoginDate")
                            {
                                Data = row[column].ToString();
                                ColName = "Last Login Date Time";
                                //Remarks = "Deleted";
                            }

                            sb.Append("<tr>");
                            sb.Append("<td>" + SerialNo + "</td>");
                            sb.Append("<td>" + ColName + "</td>");
                            sb.Append("<td>" + Data + "</td>");
                            sb.Append("<td>-</td>");
                            sb.Append("<td> Deleted </td>");
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

        #region Change User Password
        public string ChangePassword_Template(ChangePasswordModel before, ChangePasswordModel after, string TableHeader, FormCollection User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data = "";
            string Data2 = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", User["fldUserId"]));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforChangePassword", sqlParameterNext.ToArray());

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldPassword" || column.ToString() == "fldNewPassword" ||
                        column.ToString() == "fldConfPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";

                                ColName = "User Login ID";
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                Data = row[column].ToString();
                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Old Password";
                            }
                            else if (column.ToString() == "fldNewPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;
                                ColName = "New Password";
                            }


                            else if (column.ToString() == "fldConfPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Confirm New Password";
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

        #region FunctionsforChangePassword
        public ChangePasswordModel CheckUserMasterByID(string userId, string userAbb, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ChangePasswordModel password = new ChangePasswordModel();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetUserPassword_Change", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                password.fldUserAbb = row["fldUserAbb"].ToString();
                password.fldUserId = row["fldUserId"].ToString();
                password.fldPassword = row["fldPassword"].ToString();
                password.fldPassword = decryptPassword(password.fldPassword);

            }
            else
            {
                password = null;
            }
            return password;
        }

        public string decryptPassword(string encryptedPassword)
        {
            string result = "";
            try
            {
                ICSSecurity.EncryptDecrypt encryptDecrypt = new ICSSecurity.EncryptDecrypt();
                encryptDecrypt.FilePath = systemProfileDao.GetDLLPath();
                result = encryptDecrypt.DecryptString128Bit(encryptedPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        #endregion

        #region Security Profile
        public string SecurityProfile_EditTemplate(SecurityProfileModel before, SecurityProfileModel after, string TableHeader, FormCollection Security)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Security Profile " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in Security.AllKeys)
            {
                if (key == "chkYes" || key == "fldUserIdLengthMin" || key == "fldUserAcctExpiry" || key == "fldUserLoginAttempt" || key == "fldUserCNCR" || key == "fldUserSessionTimeOut"
                    || key == "fldDualApproval" || key == "fldUserADDomain" || key == "fldUserPwdLengthMin" || key == "fldUserPwdHisRA" || key == "fldUserPwdExpiry"
                    || key == "fldUserPwdNotification" || key == "fldUserPwdExpAction")
                {
                    if (key == "chkYes")
                    {
                        if (before.fldUserAuthMethod == after.fldUserAuthMethod)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        if (before.fldUserAuthMethod == "AD")
                        {
                            if (after.fldUserAuthMethod == "AD")
                            {
                                after.fldUserAuthMethod = "Active Directory";
                            }
                            else if (after.fldUserAuthMethod == "LPr")
                            {
                                after.fldUserAuthMethod = "Local Profiler";
                            }
                            before.fldUserAuthMethod = "Active Directory";
                        }
                        else if (before.fldUserAuthMethod == "LP")
                        {
                            if (after.fldUserAuthMethod == "AD")
                            {
                                after.fldUserAuthMethod = "Active Directory";
                            }
                            else if (after.fldUserAuthMethod == "LP")
                            {
                                after.fldUserAuthMethod = "Local Profiler";
                            }
                            before.fldUserAuthMethod = "Local Profiler";
                        }
                        beforeEdit = before.fldUserAuthMethod;
                        afterEdit = after.fldUserAuthMethod;
                        ColName = "Authentication Method";
                    }
                    else if (key == "fldUserIdLengthMin")
                    {
                        if (before.fldUserIdLengthMin == after.fldUserIdLengthMin) /*&& before.fldUserIdLengthMax == after.fldUserIdLengthMax)*/
                        {
                            if (before.fldUserIdLengthMax == after.fldUserIdLengthMax)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "No Changes";
                        }
                        else
                        {
                            if (before.fldUserIdLengthMax == after.fldUserIdLengthMax)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "Value Edited";
                        }

                        beforeEdit = "Min : " + before.fldUserIdLengthMin.ToString() + ", Max : " + before.fldUserIdLengthMax;
                        afterEdit = "Min : " + after.fldUserIdLengthMin.ToString() + ", Max : " + after.fldUserIdLengthMax;
                        ColName = "User ID Length";
                    }
                    else if (key == "fldUserAcctExpiry")
                    {
                        if (before.fldUserAcctExpiry == after.fldUserAcctExpiry) /*&& before.fldUserAcctExpiryInt == after.fldUserAcctExpiryInt)*/
                        {
                            if (before.fldUserAcctExpiryInt == after.fldUserAcctExpiryInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "No Changes";
                        }
                        else
                        {
                            if (before.fldUserAcctExpiryInt == after.fldUserAcctExpiryInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldUserAcctExpiry.ToString() + " " + before.fldUserAcctExpiryInt;
                        afterEdit = after.fldUserAcctExpiry.ToString() + " " + after.fldUserAcctExpiryInt;
                        ColName = "User Account Expiry Interval";
                    }
                    else if (key == "fldUserLoginAttempt")
                    {
                        if (before.fldUserLoginAttempt == after.fldUserLoginAttempt)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserLoginAttempt.ToString() + " Time(s)";
                        afterEdit = after.fldUserLoginAttempt.ToString() + " Time(s)";
                        ColName = "User Maximum Login Attempt";
                    }
                    else if (key == "fldUserCNCR")
                    {
                        if (before.fldUserCNCR == after.fldUserCNCR)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserCNCR;
                        afterEdit = after.fldUserCNCR;
                        ColName = "Concurent Connection";
                    }
                    else if (key == "fldUserSessionTimeOut")
                    {
                        if (before.fldUserSessionTimeOut == after.fldUserSessionTimeOut)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserSessionTimeOut.ToString() + " Minute(s)";
                        afterEdit = after.fldUserSessionTimeOut.ToString() + " Minute(s)";
                        ColName = "User Session Timeout";
                    }
                    else if (key == "fldDualApproval")
                    {
                        if (before.fldDualApproval == after.fldDualApproval)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldDualApproval;
                        afterEdit = after.fldDualApproval;
                        ColName = "Dual Approval";
                    }
                    else if (key == "fldUserADDomain")
                    {
                        if (before.fldUserADDomain == after.fldUserADDomain)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserADDomain;
                        afterEdit = after.fldUserADDomain;
                        ColName = "AD Domain";
                    }
                    else if (key == "fldUserPwdLengthMin")
                    {
                        if (before.fldUserPwdLengthMin == after.fldUserPwdLengthMin)/* && before.fldUserPwdLengthMax == after.fldUserPwdLengthMax)*/
                        {
                            if (before.fldUserPwdLengthMax == after.fldUserPwdLengthMax)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "No Changes";
                        }
                        else
                        {
                            if (before.fldUserPwdLengthMax == after.fldUserPwdLengthMax)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "Value Edited";
                        }
                        beforeEdit = "Min : " + before.fldUserPwdLengthMin.ToString() + ", Max : " + before.fldUserPwdLengthMax.ToString();
                        afterEdit = "Min : " + after.fldUserPwdLengthMin.ToString() + ", Max : " + after.fldUserPwdLengthMax.ToString();
                        ColName = "User Password Length";
                    }
                    else if (key == "fldUserPwdHisRA")
                    {
                        if (before.fldUserPwdHisRA == after.fldUserPwdHisRA)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserPwdHisRA.ToString() + " Time(s)";
                        afterEdit = after.fldUserPwdHisRA.ToString() + " Time(s)";
                        ColName = "User Password History Reusable After";
                    }
                    else if (key == "fldUserPwdExpiry")
                    {
                        if (before.fldUserPwdExpiry == after.fldUserPwdExpiry) /*&& before.fldUserPwdExpiryInt == after.fldUserPwdExpiryInt)*/
                        {
                            if (before.fldUserPwdExpiryInt == after.fldUserPwdExpiryInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "No Changes";
                        }
                        else
                        {
                            if (before.fldUserPwdExpiryInt == after.fldUserPwdExpiryInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserPwdExpiry.ToString() + " " + before.fldUserPwdExpiryInt;
                        afterEdit = after.fldUserPwdExpiry.ToString() + " " + after.fldUserPwdExpiryInt;
                        ColName = "User Password Expiry Interval";
                    }
                    else if (key == "fldUserPwdNotification")
                    {
                        if (before.fldUserPwdNotification == after.fldUserPwdNotification)/* && before.fldUserPwdNotificationInt == after.fldUserPwdNotificationInt)*/
                        {
                            if (before.fldUserPwdNotificationInt == after.fldUserPwdNotificationInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "No Changes";
                        }
                        else
                        {
                            if (before.fldUserPwdNotificationInt == after.fldUserPwdNotificationInt)
                            {
                                Remarks = "No Changes";
                            }
                            else
                            {
                                Remarks = "Value Edited";
                            }
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserPwdNotification.ToString() + " " + before.fldUserPwdNotificationInt;
                        afterEdit = after.fldUserPwdNotification.ToString() + " " + after.fldUserPwdNotificationInt;
                        ColName = "User password Notification Interval";
                    }
                    else if (key == "fldUserPwdExpAction")
                    {
                        if (before.fldUserPwdExpAction == after.fldUserPwdExpAction)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        if (before.fldUserPwdExpAction == "F")
                        {
                            if (after.fldUserPwdExpAction == "F")
                            {
                                after.fldUserPwdExpAction = "Force User To Change Password";
                            }
                            else if (after.fldUserPwdExpAction == "D")
                            {
                                after.fldUserPwdExpAction = "Disable User";
                            }
                            before.fldUserPwdExpAction = "Force User To Change Password";
                        }
                        else if (before.fldUserPwdExpAction == "D")
                        {
                            if (after.fldUserPwdExpAction == "F")
                            {
                                after.fldUserPwdExpAction = "Force User To Change Password";
                            }
                            else if (after.fldUserPwdExpAction == "D")
                            {
                                after.fldUserPwdExpAction = "Disable User";
                            }
                            before.fldUserPwdExpAction = "Disable User";
                        }
                        beforeEdit = before.fldUserPwdExpAction;
                        afterEdit = after.fldUserPwdExpAction;
                        ColName = "When User password Expired";
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

        #region Task Assignment
        public string TaskAssignment_EditTemplate(string beforeTask2, string afterTask2, string TableHeader, string Action, FormCollection col)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Task Assignment - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            foreach (string key in col.AllKeys)
            {
                if (key == "fldGroupCode" || key == "fldGroupDesc" || key == "selectedTask")
                {
                    if (key == "fldGroupCode")
                    {
                        Remarks = "No Changes";
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        ColName = "Group Code";
                    }
                    else if (key == "fldGroupDesc")
                    {
                        Remarks = "No Changes";
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        ColName = "Group Description";
                    }
                    else if (key == "selectedTask")
                    {
                        if (beforeTask2 == afterTask2)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        if (beforeTask2 == "")
                        {
                            beforeEdit = "-";
                            afterEdit = afterTask2;
                        }
                        else
                        {
                            beforeEdit = beforeTask2;
                            afterEdit = afterTask2;
                        }
                        ColName = "Task Selected List";
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
        public string TaskAssignmentChecker_EditTemplate(string beforeTask, string afterTask, string TableHeader, string Action, string id)
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
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", id));

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Task Assignment (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgGroupProfile_Task", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgGroupProfile_Task", sqlParameterNext.ToArray());
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldGroupCode" || column.ToString() == "fldGroupDesc" || column.ToString() == "fldTaskId")
                        {
                            if (column.ToString() == "fldGroupCode")
                            {
                                ColName = "Group Code";
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldGroupDesc")
                            {
                                ColName = "Group Description";
                                Data = row[column].ToString();
                                beforeEdit = Data;
                                afterEdit = Data;
                                Remarks = "No Changes";
                            }
                            else if (column.ToString() == "fldTaskId")
                            {
                                if (beforeTask == afterTask)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (beforeTask == "")
                                {
                                    beforeEdit = "-";
                                    afterEdit = afterTask;
                                }
                                else
                                {
                                    beforeEdit = beforeTask;
                                    afterEdit = afterTask;
                                }
                                ColName = "Task Selected List";

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

        #region Hub User
        public string HubUser_AddTemplate(FormCollection col, string afterUser2, string TableHeader)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in col.AllKeys)
            {
                if (key == "fldHubCode" || key == "fldHubDesc" || key == "selectedUser")
                {
                    if (key == "fldHubCode")
                    {
                        Data = col[key].ToString();
                        ColName = "Hub Code";
                    }
                    else if (key == "fldHubDesc")
                    {
                        Data = col[key].ToString();
                        ColName = "Hub Description";
                    }
                    else if (key == "selectedUser")
                    {
                        Data = afterUser2;
                        ColName = "User Selected List";
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
        public string HubUser_EditTemplate(HubModel before, HubModel after, string beforeUser2, string afterUser2, string TableHeader, FormCollection col)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            foreach (string key in col.AllKeys)
            {
                if (key == "fldHubCode" || key == "fldHubDesc" || key == "selectedUser")
                {
                    if (key == "fldHubCode")
                    {
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        Remarks = "No Changes";
                        ColName = "Hub Code";
                    }
                    else if (key == "fldHubDesc")
                    {
                        if (before.fldHubDesc == after.fldHubDesc)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldHubDesc;
                        afterEdit = after.fldHubDesc;
                        ColName = "Hub Description";
                    }
                    else if (key == "selectedUser")
                    {
                        if (beforeUser2 == afterUser2)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = beforeUser2;
                        afterEdit = afterUser2;
                        ColName = "User Selected List";
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
        public string HubUser_DeleteTemp(string beforeUser2, string TableHeader, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", id));
            dt = dbContext.GetRecordsAsDataTableSP("spcgHubUser_Security", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHubCode" || column.ToString() == "fldHubDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldHubCode")
                            {
                                ColName = "Hub Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldHubDesc")
                            {
                                ColName = "Hub Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = beforeUser2;

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
        public string HubUserChecker_AddTemplate(string afterUser2, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUser_Security", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUserTemp_Security", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHubCode" || column.ToString() == "fldHubDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldHubCode")
                            {
                                ColName = "Hub Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldHubDesc")
                            {
                                ColName = "Hub Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = afterUser2;

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
        public string HubUserChecker_EditTemplate(HubModel before, HubModel after, string beforeUser2, string afterUser2, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUser_Security", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUserTemp_Security", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHubCode" || column.ToString() == "fldHubDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldHubCode")
                            {
                                beforeEdit = row[column].ToString();
                                afterEdit = row[column].ToString();
                                Remarks = "No Changes";
                                ColName = "Hub Code";
                            }
                            else if (column.ToString() == "fldHubDesc")
                            {
                                if (before.fldHubDesc == after.fldHubDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldHubDesc;
                                afterEdit = after.fldHubDesc;
                                ColName = "Hub Description";
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                if (beforeUser2 == afterUser2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = beforeUser2;
                                afterEdit = afterUser2;
                                ColName = "User Selected List";

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
        public string HubUserChecker_DeleteTemplate(string beforeUser2, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUser_Security", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubUserTemp_Security", sqlParameterNext.ToArray());
            }
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub User (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHubCode" || column.ToString() == "fldHubDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldHubCode")
                            {
                                ColName = "Hub Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldHubDesc")
                            {
                                ColName = "Hub Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = beforeUser2;

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

        #region FunctionforHubUser
        public HubModel CheckHubMasterByIDTemp_Security(string HubCode, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            HubModel hub = new HubModel();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubMasterbyIdTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                hub.fldHubId = row["fldHubCode"].ToString();
                hub.fldHubDesc = row["fldHubDesc"].ToString();
                hub.fldBankCode = row["fldBankCode"].ToString();
                hub.fldCreateuserId = row["fldCreateUserId"].ToString();
                hub.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                hub.fldUpdateUserId = row["fldUpdateUserId"].ToString();
                hub.fldUpdateTimeStamp = DateUtils.formatDateFromSql(row["fldUpdateTimeStamp"].ToString());


            }
            else
            {
                hub = null;
            }
            return hub;
        }
        #endregion

        #region Hub Branch
        public string HubBranch_EditTemplate(string beforeBranch1, string afterBranch1, string TableHeader, string Action, FormCollection col)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub Branch - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            foreach (string key in col.AllKeys)
            {
                if (key == "fldHubCode" || key == "fldHubDesc" || key == "selectedBranch")
                {
                    if (key == "fldHubCode")
                    {
                        Remarks = "No Changes";
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        ColName = "Hub Code";
                    }
                    else if (key == "fldHubDesc")
                    {
                        Remarks = "No Changes";
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        ColName = "Hub Description";
                    }
                    else if (key == "selectedBranch")
                    {
                        if (beforeBranch1 == afterBranch1)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        if (beforeBranch1 == "")
                        {
                            beforeEdit = "-";
                            afterEdit = afterBranch1;
                        }
                        else
                        {
                            beforeEdit = beforeBranch1;
                            afterEdit = afterBranch1;
                        }
                        ColName = "Hub Branch Selected List";
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
        public string HubBranchChecker_EditTemplate(string beforeBranch1, string afterBranch1, string TableHeader, string Action, string id)
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
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubMaster_Checker", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgHubMaster_Checker", sqlParameterNext.ToArray());
            }

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Hub Branch (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldHubCode" || column.ToString() == "fldHubDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldHubCode")
                            {
                                ColName = "Hub Code";
                                Remarks = "No Changes";
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;
                            }
                            else if (column.ToString() == "fldHubDesc")
                            {
                                ColName = "Hub Description";
                                Remarks = "No Changes";
                                Data = row[column].ToString();

                                beforeEdit = Data;
                                afterEdit = Data;
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "Hub Branch Selected List";
                                if (beforeBranch1 == afterBranch1)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                if (beforeBranch1 == "")
                                {
                                    beforeEdit = "-";
                                    afterEdit = afterBranch1;
                                }
                                else
                                {
                                    beforeEdit = beforeBranch1;
                                    afterEdit = afterBranch1;
                                }

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

        #region FunctionforHubBranch
        public List<BranchModel> ListSelectedBranchInHubTemp_Security(string hubCode, string strBankCode)
        {
            List<BranchModel> branchModels = new List<BranchModel>();
            DataTable dtHubBranch = new DataTable();
            List<string> branchIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            dtHubBranch = dbContext.GetRecordsAsDataTableSP("spcgHubBranchTemp", sqlParameterNext.ToArray());
            if (dtHubBranch.Rows.Count > 0)
            {
                foreach (DataRow row in dtHubBranch.Rows)
                {
                    BranchModel branchModel = new BranchModel()
                    {
                        fldBranchId = row["fldBranchId"].ToString(),
                        fldBranchCode = row["fldbranchCode"].ToString(),
                        fldBranchDesc = row["fldBranchDesc"].ToString()
                    };
                    branchModels.Add(branchModel);
                }
            }


            return branchModels;
        }
        #endregion

        #region GroupProfile
        public string GroupProfile_AddTemplate(FormCollection col, string afterUser, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in col.AllKeys)
            {
                if (key == "fldGroupCode" || key == "fldGroupDesc" || key == "selectedUser")
                {
                    if (key == "fldGroupCode")
                    {
                        Data = col[key].ToString();
                        ColName = "Group Code";
                    }
                    else if (key == "fldGroupDesc")
                    {
                        Data = col[key].ToString();
                        ColName = "Group Description";
                    }
                    else if (key == "selectedUser")
                    {
                        Data = afterUser;
                        ColName = "User Selected List";
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
        public string GroupProfile_EditTemplate(GroupProfileModel before, GroupProfileModel after, string beforeUser2, string alreadyselecteduser2, string TableHeader, string Action, FormCollection col)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            foreach (string key in col.AllKeys)
            {
                if (key == "fldGroupCode" || key == "fldGroupDesc" || key == "selectedUser")
                {
                    if (key == "fldGroupCode")
                    {
                        if (col[key] == col[key])
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = col[key];
                        afterEdit = col[key];
                        ColName = "Group Code";
                    }
                    else if (key == "fldGroupDesc")
                    {
                        if (before.fldGroupDesc == after.fldGroupDesc) /*(before.fldGroupDesc == after.fldGroupDesc)*/
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldGroupDesc;  /*before.fldGroupDesc;*/
                        afterEdit = after.fldGroupDesc;  /*after.fldGroupDesc;*/
                        ColName = "Group Description";
                    }
                    else if (key == "selectedUser")
                    {
                        if (beforeUser2 == alreadyselecteduser2) /*(before.fldUserAbb == after.fldUserAbb)*/
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = beforeUser2;  /*before.fldUserAbb;*/
                        afterEdit = alreadyselecteduser2; /*after.fldUserAbb;*/
                        ColName = "User Selected List";
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
        public string GroupProfile_DeleteTemplate(string afterUser, string TableHeader, string Action, string col)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", col));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfileTemp_Checker", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldGroupCode" || column.ToString() == "fldGroupDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldGroupCode")
                            {
                                ColName = "Group Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldGroupDesc")
                            {
                                ColName = "Group Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = afterUser;

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
        public string GroupProfileChecker_AddTemplate(string afterUser, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfile_Checker", sqlParameterNext.ToArray());
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfileTemp_Checker", sqlParameterNext.ToArray());
            }
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldGroupCode" || column.ToString() == "fldGroupDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldGroupCode")
                            {
                                ColName = "Group Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldGroupDesc")
                            {
                                ColName = "Group Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = afterUser;

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
        public string GroupProfileChecker_DeleteTemplate(string afterUser, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfile_Checker", sqlParameterNext.ToArray());
                Remarks = "Deleted";
            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfileTemp_Checker", sqlParameterNext.ToArray());
                Remarks = "Deleted";
            }
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldGroupCode" || column.ToString() == "fldGroupDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldGroupCode")
                            {
                                ColName = "Group Code";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldGroupDesc")
                            {
                                ColName = "Group Description";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                ColName = "User Selected List";
                                Data = afterUser;

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
        public string GroupProfileChecker_EditTemplate(GroupProfileModel before, GroupProfileModel after, string beforeUser2, string alreadyselecteduser2, string TableHeader, string Action, string id)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", id));

            if (Action == "Approve")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfile_Checker", sqlParameterNext.ToArray());

            }
            else if (Action == "Reject")
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfileTemp_Checker", sqlParameterNext.ToArray());

            }
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: Group Profile (Checker) - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldGroupCode" || column.ToString() == "fldGroupDesc" || column.ToString() == "fldUserId")
                        {
                            if (column.ToString() == "fldGroupCode")
                            {
                                Data = row[column].ToString();

                                beforeEdit = id;
                                afterEdit = id;
                                Remarks = "No Changes";
                                ColName = "Group Code";
                            }
                            else if (column.ToString() == "fldGroupDesc")
                            {
                                Data = row[column].ToString();

                                if (before.fldGroupDesc == after.fldGroupDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldGroupDesc;
                                afterEdit = after.fldGroupDesc;

                                ColName = "Group Description";
                            }
                            else if (column.ToString() == "fldUserId")
                            {
                                Data = row[column].ToString();
                                if (beforeUser2 == alreadyselecteduser2)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = beforeUser2;
                                afterEdit = alreadyselecteduser2;

                                ColName = "User Selected List";

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

        #region FunctionsforGroupProfile
        public GroupProfileModel CheckGroupMasterUserID(string groupId) 
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            GroupProfileModel group = new GroupProfileModel();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", groupId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfile", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                group.fldGroupId = row["fldGroupCode"].ToString();
                group.fldGroupDesc = row["fldGroupDesc"].ToString();
                group.fldUserId = row["fldUserId"].ToString();
                group.fldUserAbb = row["fldUserAbb"].ToString();
            }
            else
            {
                group = null;
            }
            return group;
        }
        public GroupProfileModel CheckGroupMasterUserIDTemp(string groupId)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            GroupProfileModel group = new GroupProfileModel();
            sqlParameterNext.Add(new SqlParameter("@fldGroupCode", groupId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserforGroupProfileTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                group.fldGroupId = row["fldGroupCode"].ToString();
                group.fldGroupDesc = row["fldGroupDesc"].ToString();
                group.fldUserId = row["fldUserId"].ToString();
                group.fldUserId = row["fldUserAbb"].ToString();
            }
            else
            {
                group = null;
            }
            return group;
        }
        #endregion

        #region User Profile
        public string UserProfile_AddTemplate(FormCollection Newobj, FormCollection OldObj, string TableHeader, string Action)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string DataDesc = "";
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", Newobj["fldUserAbb"]));

            if ("N".Equals(systemProfile))
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            }
            else
            {
                dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit_Reject", sqlParameterNext.ToArray());
            }
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            if (Action == "Create")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Approve")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Reject")
            {
                Remarks = "Newly Added";
            }

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();
                                DataDesc = row["fldBranchDesc"].ToString();

                                Data = Data + "-" + DataDesc;

                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";

                                Data = row[column].ToString();
                                DataDesc = row["fldLimitDesc"].ToString();

                                Data = Data + "-" + DataDesc;

                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "*********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "*********";
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
        public string UserProfile_EditTemplate(UserModel before, UserModel after, string TableHeader, FormCollection User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data = "";
            string Data2 = "";
            string ClassName = "";
            string BranchDesc = "";
            StringBuilder sb = new StringBuilder();

            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            foreach (string key in User.AllKeys)
            {
                if (key == "oldUserAbb" || key == "fldUserDesc" || key == "userType" || key == "fldBranchCode" || key == "fldVerificationClass_2" || key == "fldVerificationLimit_2" || key == "oldDisableLogin" || key == "passwordChecker" || key == "fldConfirmPassword")
                {
                    if (key == "oldUserAbb")
                    {
                        if (before.fldUserAbb == after.fldUserAbb)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserAbb;
                        afterEdit = after.fldUserAbb;
                        ColName = "User Login ID";
                    }
                    else if (key == "fldUserDesc")
                    {
                        if (before.fldUserDesc == after.fldUserDesc)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldUserDesc;
                        afterEdit = after.fldUserDesc;
                        ColName = "User Name";
                    }
                    else if (key == "userType")
                    {
                        if (before.userType == after.userType)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.userType;
                        afterEdit = after.userType;
                        ColName = "User Type";
                    }
                    else if (key == "fldBranchCode")
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

                        Data = before.fldBranchCode;

                        if (Data != "" && Data != null)
                        {
                            DataTable dt2 = new DataTable();
                            List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                            sqlParameterNext2.Add(new SqlParameter("@Branchcode", before.fldBranchCode.Trim()));
                            dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                            if (dt2.Rows.Count > 0)
                            {
                                BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                            }

                            Data = before.fldBranchCode.Trim() + "-" + BranchDesc;
                        }

                        afterEdit = after.fldBranchCode;

                        Data2 = after.fldBranchCode;

                        if (Data2 != "" && Data2 != null)
                        {
                            DataTable dt2 = new DataTable();
                            List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                            sqlParameterNext2.Add(new SqlParameter("@Branchcode", after.fldBranchCode.Trim()));
                            dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                            if (dt2.Rows.Count > 0)
                            {
                                BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                            }

                            Data2 = after.fldBranchCode + "-" + BranchDesc;
                        }

                        ColName = "Branch Code";

                        beforeEdit = Data;
                        afterEdit = Data2;

                    }
                    else if (key == "fldVerificationClass_2")
                    {
                        if (before.fldVerificationClass == after.fldVerificationClass)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }

                        beforeEdit = before.fldVerificationClass;
                        Data = before.fldVerificationClass;

                        if (Data != "" && Data != null)
                        {
                            DataTable dt1 = new DataTable();
                            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                            sqlParameterNext1.Add(new SqlParameter("@ClassName", before.fldVerificationClass.Trim()));
                            dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                            if (dt1.Rows.Count > 0)
                            {

                                ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                            }
                            Data = before.fldVerificationClass.Trim() + "-" + ClassName;
                        }

                        afterEdit = after.fldVerificationClass;
                        Data2 = after.fldVerificationClass;


                        if (Data2 != "" && Data2 != null)
                        {
                            DataTable dt1 = new DataTable();
                            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                            sqlParameterNext1.Add(new SqlParameter("@ClassName", after.fldVerificationClass.Trim()));
                            dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                            if (dt1.Rows.Count > 0)
                            {

                                ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                            }
                            Data2 = after.fldVerificationClass.Trim() + "-" + ClassName;
                        }

                        ColName = "Verification Class";

                        beforeEdit = Data;
                        afterEdit = Data2;
                    }
                    else if (key == "fldVerificationLimit_2")
                    {
                        if (before.fldVerificationLimit == after.fldVerificationLimit)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldVerificationLimit;
                        afterEdit = after.fldVerificationLimit;
                        ColName = "Verification Limit";
                    }
                    else if (key == "oldDisableLogin")
                    {
                        if (before.fldDisableLogin == after.fldDisableLogin)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        beforeEdit = before.fldDisableLogin;
                        afterEdit = after.fldDisableLogin;
                        ColName = "Disable Login";
                    }
                    else if (key == "passwordChecker")
                    {
                        if (before.fldPassword == after.fldPassword)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        Data = "**********";
                        Data2 = "**********";

                        beforeEdit = Data;
                        afterEdit = Data2;
                        ColName = "Password";
                    }
                    else if (key == "fldConfirmPassword")
                    {
                        if (before.fldPassword == after.fldPassword)
                        {
                            Remarks = "No Changes";
                        }
                        else
                        {
                            Remarks = "Value Edited";
                        }
                        Data = "**********";
                        Data2 = "**********";

                        beforeEdit = Data;
                        afterEdit = Data2;
                        ColName = "Confirm Password";
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
        public string UserProfile_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string ClassName = "";
            string BranchDesc = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Delete")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Approve")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Reject")
            {
                Remarks = "Deleted";
            }


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", Data.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = Data + "-" + BranchDesc;
                                }

                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", Data.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = Data.Trim() + "-" + ClassName;
                                }

                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "*********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "*********";
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
        public string UserProfileCheckerApp_AddTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            //string DataDesc = "";
            string ClassName = "";
            string BranchDesc = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Create")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Approve")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Reject")
            {
                Remarks = "Newly Added";
            }


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", Data.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = Data + "-" + BranchDesc;
                                }

                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";

                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", Data.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = Data.Trim() + "-" + ClassName;
                                }

                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "**********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "**********";
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
        public string UserProfileCheckerApp_EditTemplate(UserModel before, UserModel after, string TableHeader, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data2 = "";
            string ClassName = "";
            string BranchDesc = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                Data = row[column].ToString();

                                if (before.fldUserAbb == after.fldUserAbb)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldUserAbb;
                                afterEdit = after.fldUserAbb;

                                ColName = "User Login ID";
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                Data = row[column].ToString();

                                if (before.fldUserDesc == after.fldUserDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldUserDesc;
                                afterEdit = after.fldUserDesc;

                                ColName = "User Name";
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                Data = row[column].ToString();

                                if (before.userType == after.userType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.userType;
                                afterEdit = after.userType;

                                ColName = "User Type";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();

                                if (before.fldBranchCode == after.fldBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBranchCode;

                                Data = before.fldBranchCode;

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", before.fldBranchCode.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = before.fldBranchCode.Trim() + "-" + BranchDesc;
                                }

                                afterEdit = after.fldBranchCode;

                                Data2 = after.fldBranchCode;

                                if (Data2 != "" && Data2 != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", after.fldBranchCode.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data2 = after.fldBranchCode + "-" + BranchDesc;
                                }

                                ColName = "Branch Code";

                                beforeEdit = Data;
                                afterEdit = Data2;
                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                Data = row[column].ToString();

                                if (before.fldVerificationClass == after.fldVerificationClass)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }

                                beforeEdit = before.fldVerificationClass;
                                Data = before.fldVerificationClass;

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", before.fldVerificationClass.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = before.fldVerificationClass.Trim() + "-" + ClassName;
                                }

                                afterEdit = after.fldVerificationClass;
                                Data2 = after.fldVerificationClass;


                                if (Data2 != "" && Data2 != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", after.fldVerificationClass.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data2 = after.fldVerificationClass.Trim() + "-" + ClassName;
                                }

                                ColName = "Verification Class";

                                beforeEdit = Data;
                                afterEdit = Data2;
                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                Data = row[column].ToString();

                                if (before.fldVerificationLimit == after.fldVerificationLimit)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldVerificationLimit;
                                afterEdit = after.fldVerificationLimit;

                                ColName = "Verification Limit";
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                Data = row[column].ToString();

                                if (before.fldDisableLogin == after.fldDisableLogin)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldDisableLogin;
                                afterEdit = after.fldDisableLogin;

                                ColName = "Disable Login";
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Password";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Confirm Password";
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
        public string UserProfileCheckerApp_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string ClassName = "";
            string BranchDesc = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Delete")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Approve")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Reject")
            {
                Remarks = "Deleted";
            }


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", Data.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = Data + "-" + BranchDesc;
                                }

                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";

                                Data = row[column].ToString();

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", Data.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = Data.Trim() + "-" + ClassName;
                                }
                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "**********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "**********";
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
        public string UserProfileCheckerRej_AddTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string DataDesc = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit_Reject", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Create")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Approve")
            {
                Remarks = "Newly Added";
            }
            else if (Action == "Reject")
            {
                Remarks = "Newly Added";
            }


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();
                                DataDesc = row["fldBranchDesc"].ToString();

                                Data = Data + "-" + DataDesc;
                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";

                                Data = row[column].ToString();
                                DataDesc = row["fldLimitDesc"].ToString();

                                Data = Data + "-" + DataDesc;
                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "**********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "**********";
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
        public string UserProfileCheckerRej_DeleteTemplate(UserModel before, UserModel after, string TableHeader, string Action, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string ClassName = "";
            string BranchDesc = "";
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");

            if (Action == "Delete")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Approve")
            {
                Remarks = "Deleted";
            }
            else if (Action == "Reject")
            {
                Remarks = "Deleted";
            }


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                ColName = "User Login ID";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                ColName = "User Name";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                ColName = "User Type";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                ColName = "Branch Code";
                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", Data.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = Data + "-" + BranchDesc;
                                }

                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                ColName = "Verification Class";

                                Data = row[column].ToString();
                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", Data.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = Data.Trim() + "-" + ClassName;
                                }

                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                ColName = "Verification Limit";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                ColName = "Disable Login";
                                Data = row[column].ToString();
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                ColName = "Password";
                                Data = "**********";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                ColName = "Confirm Password";
                                Data = "**********";
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
        public string UserProfileCheckerRej_EditTemplate(UserModel before, UserModel after, string TableHeader, string User)
        {
            string Result = "";
            int SerialNo = 1;
            string Remarks = "";
            string ColName = "";
            string Data = "";
            string beforeEdit = "";
            string afterEdit = "";
            string Data2 = "";
            string ClassName = "";
            string BranchDesc = "";

            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", User));
            dt = dbContext.GetRecordsAsDataTableSP("spcgUserforSecurityAudit", sqlParameterNext.ToArray());
            //Table start.
            sb.Append("<table cellSpacing=0 cellPadding=1 border=1 width=100% height=95%>");
            //Adding HeaderRow.
            sb.Append("<tr><th colspan=5 height=20> Task Name: User Profile - " + TableHeader + " </th></tr>");
            //Add Rows
            sb.Append("<tr><td width=20px class=tblSubHdr>No</td><td width=100px class=tblSubHdr>Field Name</td><td width=80px class=tblSubHdr>Before Image</td><td width=80px class=tblSubHdr>After Image</td><td width=180px class=tblSubHdr>Remarks</td></tr>");


            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (column.ToString() == "fldUserAbb" || column.ToString() == "fldUserDesc" || column.ToString() == "fldUserType" || column.ToString() == "fldBranchCode" || column.ToString() == "fldVerificationClass" || column.ToString() == "fldVerificationLimit"
                       || column.ToString() == "fldDisableLogin" || column.ToString() == "fldPassword" || column.ToString() == "fldConfirmPassword")
                        {
                            if (column.ToString() == "fldUserAbb")
                            {
                                Data = row[column].ToString();

                                if (before.fldUserAbb == after.fldUserAbb)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldUserAbb;
                                afterEdit = after.fldUserAbb;

                                ColName = "User Login ID";
                            }
                            else if (column.ToString() == "fldUserDesc")
                            {
                                Data = row[column].ToString();

                                if (before.fldUserDesc == after.fldUserDesc)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldUserDesc;
                                afterEdit = after.fldUserDesc;

                                ColName = "User Name";
                            }
                            else if (column.ToString() == "fldUserType")
                            {
                                Data = row[column].ToString();

                                if (before.userType == after.userType)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.userType;
                                afterEdit = after.userType;

                                ColName = "User Type";
                            }
                            else if (column.ToString() == "fldBranchCode")
                            {
                                Data = row[column].ToString();

                                if (before.fldBranchCode == after.fldBranchCode)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldBranchCode;

                                Data = before.fldBranchCode;

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", before.fldBranchCode.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data = before.fldBranchCode.Trim() + "-" + BranchDesc;
                                }

                                afterEdit = after.fldBranchCode;

                                Data2 = after.fldBranchCode;

                                if (Data2 != "" && Data2 != null)
                                {
                                    DataTable dt2 = new DataTable();
                                    List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
                                    sqlParameterNext2.Add(new SqlParameter("@Branchcode", after.fldBranchCode.Trim()));
                                    dt2 = dbContext.GetRecordsAsDataTableSP("spcgValidBranchCode", sqlParameterNext2.ToArray());
                                    if (dt2.Rows.Count > 0)
                                    {
                                        BranchDesc = dt2.Rows[0]["fldBranchDesc"].ToString();
                                    }

                                    Data2 = after.fldBranchCode + "-" + BranchDesc;
                                }

                                ColName = "Branch Code";

                                beforeEdit = Data;
                                afterEdit = Data2;
                            }
                            else if (column.ToString() == "fldVerificationClass")
                            {
                                Data = row[column].ToString();

                                if (before.fldVerificationClass == after.fldVerificationClass)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldVerificationClass;
                                Data = before.fldVerificationClass;

                                if (Data != "" && Data != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", before.fldVerificationClass.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data = before.fldVerificationClass.Trim() + "-" + ClassName;
                                }

                                afterEdit = after.fldVerificationClass;
                                Data2 = after.fldVerificationClass;


                                if (Data2 != "" && Data2 != null)
                                {
                                    DataTable dt1 = new DataTable();
                                    List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                                    sqlParameterNext1.Add(new SqlParameter("@ClassName", after.fldVerificationClass.Trim()));
                                    dt1 = dbContext.GetRecordsAsDataTableSP("spcgVerificationLimitByClass", sqlParameterNext1.ToArray());
                                    if (dt1.Rows.Count > 0)
                                    {

                                        ClassName = dt1.Rows[0]["fldLimitDesc"].ToString();
                                    }
                                    Data2 = after.fldVerificationClass.Trim() + "-" + ClassName;
                                }

                                ColName = "Verification Class";

                                beforeEdit = Data;
                                afterEdit = Data2;

                            }
                            else if (column.ToString() == "fldVerificationLimit")
                            {
                                Data = row[column].ToString();

                                if (before.fldVerificationLimit == after.fldVerificationLimit)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldVerificationLimit;
                                afterEdit = after.fldVerificationLimit;

                                ColName = "Verification Limit";
                            }
                            else if (column.ToString() == "fldDisableLogin")
                            {
                                Data = row[column].ToString();

                                if (before.fldDisableLogin == after.fldDisableLogin)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                beforeEdit = before.fldDisableLogin;
                                afterEdit = after.fldDisableLogin;

                                ColName = "Disable Login";
                            }
                            else if (column.ToString() == "fldPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Password";
                            }
                            else if (column.ToString() == "fldConfirmPassword")
                            {
                                Data = row[column].ToString();

                                if (before.fldPassword == after.fldPassword)
                                {
                                    Remarks = "No Changes";
                                }
                                else
                                {
                                    Remarks = "Value Edited";
                                }
                                Data = "**********";
                                Data2 = "**********";

                                beforeEdit = Data;
                                afterEdit = Data2;

                                ColName = "Confirm Password";
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
        #region FunctionsforUserProfile
        public UserModel CheckUserMasterByTempID(string UserAbb, string UserID, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            UserModel user = new UserModel();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", UserID));
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", UserAbb));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserMasterTempById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                user.fldUserAbb = row["fldUserAbb"].ToString();
                user.fldUserId = row["fldUserId"].ToString();
                user.fldUserDesc = row["fldUserDesc"].ToString();
                user.fldPassword = row["fldPassword"].ToString();
                user.fldBankCode = row["fldBankCode"].ToString();
                user.fldBranchCode = row["fldBranchCode"].ToString();
                user.fldDisableLogin = row["fldDisableLogin"].ToString();
                user.fldFailLoginDate = DateUtils.formatDateFromSql(row["fldFailLoginDate"].ToString());
                user.fldPasswordExpDate = DateUtils.formatDateFromSql(row["fldPasswordExpDate"].ToString());
                user.fldIDExpDate = DateUtils.formatDateFromSql(row["fldIDExpDate"].ToString());
                user.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                user.userType = row["fldUserType"].ToString();
                user.fldPassword = decryptPassword(user.fldPassword);
                if (SearchFor == "System")
                {
                    user.fldVerificationClass = "";
                    user.fldVerificationLimit = "";
                }
                else
                {
                    user.fldVerificationClass = row["fldVerificationClass"].ToString();
                    user.fldVerificationLimit = row["fldVerificationLimit"].ToString();
                }
            }
            else
            {
                user = null;
            }
            return user;
        }
        #endregion
    }
}