﻿using INCHEQS.Models;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using System.Text.RegularExpressions;
using INCHEQS.Security;

namespace INCHEQS.Areas.COMMON.Models.Users
{

    public class UserDao : IUserDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        public UserDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }

        public bool CreateUserMaster(string userAbb)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "Y"));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciUserMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public bool UpdateUserMasterFromTemp(string userAbb)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuUserMasterFromTemp", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public void MoveToUserMasterFromTemp(string userAbb,string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciUserMasterFromTemp", sqlParameterNext.ToArray());
        }
        public bool DeleteUserMasterTemp(string userAbb)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdUserMasterTemp", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public bool DeleteUserMaster(string userAbb)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdUserMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public void CreateUserMasterTemp(FormCollection col, string bankcode, string crtUser, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = bankcode/*CurrentUser.Account.BankCode*/;
            dynamic verificationclass = "";
            dynamic verificationlimit = "";
            if (Action == "Create")
            {
                //Initilize disable login checkbox with N if not checked
                string strFldDisableLogin = col["fldDisableLogin"];
                if (strFldDisableLogin == null)
                {
                    strFldDisableLogin = "N";
                }


                verificationclass = col["fldVerificationClass"];
                verificationlimit = col["fldVerificationLimit"];

                if ((col["userType"].Equals("branch")))
                {

                    verificationclass = "";
                }
                else
                {

                    verificationclass = col["fldVerificationClass"];
                }

                if ((col["userType"].Equals("System")))
                {
                    verificationclass = "";
                    verificationlimit = "";
                }




                //Initilize password expiry date based on security profile
                SecurityProfileModel securityProfile = securityProfileDao.GetSecurityProfile();
                string secPwdExp = securityProfile.fldUserPwdExpiry + securityProfile.fldUserPwdExpiryInt;
                DateTime passwordExpDate = FormatExpDate(secPwdExp);
                DateTime IDExpDate = DateUtils.AddDate(securityProfile.fldUserPwdExpiryInt, Convert.ToInt16(securityProfile.fldUserPwdExpiry), DateTime.Now);
                UserModel user = new UserModel();
                
                if ((col["userType"].Equals("System")))
                {
                    sqlParameterNext.Add(new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldUserDesc", col["fldUserDesc"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldBankcode", CurrentUser.Account.BankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldPassword", " "));
                    sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", strFldDisableLogin));
                    sqlParameterNext.Add(new SqlParameter("@fldCounter", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                    sqlParameterNext.Add(new SqlParameter("@fldPasswordExpDate", passwordExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpStatus", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpDate", IDExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalBankCode", bankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldUserType", col["userType"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchid", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", verificationclass));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", verificationlimit));
                    //Added after Combine
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@passwordChecker", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@Action", Action));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchuser", col["fldbranchuser"].ToString()));

                }
                else if ((col["UserType"].Equals("Admin")))
                {
                    sqlParameterNext.Add(new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldUserDesc", col["fldUserDesc"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldBankcode", CurrentUser.Account.BankCode));
                    if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword") /*&& col.AllKeys.Contains("chkboxPassword")*/)
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldPassword", encryptPassword(col["fldPassword"])));
                    }
                    else
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldPassword", " "));
                    }
                    sqlParameterNext.Add(new SqlParameter("@fldZeroMaker", col["fldZeroMaker"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", strFldDisableLogin));
                    sqlParameterNext.Add(new SqlParameter("@fldCounter", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                    sqlParameterNext.Add(new SqlParameter("@fldPasswordExpDate", passwordExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpStatus", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpDate", IDExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalBankCode", bankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldUserType", col["userType"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchid", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", verificationclass));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", verificationlimit));
                    //Added after Combine
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@passwordChecker", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@Action", Action));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchuser", col["fldbranchuser"].ToString()));
                }
                else if ((col["UserType"].Equals("branch")))
                {
                    sqlParameterNext.Add(new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldUserDesc", col["fldUserDesc"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldBankcode", CurrentUser.Account.BankCode));
                    if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword") /*&& col.AllKeys.Contains("chkboxPassword")*/)
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldPassword", encryptPassword(col["fldPassword"])));
                    }
                    else
                    {
                        sqlParameterNext.Add(new SqlParameter("@fldPassword", " "));
                    }
                    sqlParameterNext.Add(new SqlParameter("@fldZeroMaker", col["fldZeroMaker"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", strFldDisableLogin));
                    sqlParameterNext.Add(new SqlParameter("@fldCounter", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                    sqlParameterNext.Add(new SqlParameter("@fldPasswordExpDate", passwordExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpStatus", Convert.ToInt16("0")));
                    sqlParameterNext.Add(new SqlParameter("@fldIDExpDate", IDExpDate));
                    sqlParameterNext.Add(new SqlParameter("@fldApprovalBankCode", bankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldUserType", col["userType"].ToString()));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchid", col["fldBranchCode"]));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", verificationclass));
                    sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", verificationlimit));
                    //Added after Combine
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@passwordChecker", ""));
                    sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                    sqlParameterNext.Add(new SqlParameter("@Action", Action));
                    sqlParameterNext.Add(new SqlParameter("@fldbranchuser", col["fldbranchuser"].ToString()));
                }
            }
            else if (Action == "Update")
            {
                dynamic fldBranchCode = "";
                dynamic passwordChecker = "";
                dynamic encryptedPassword = "";

                if ((col["userType"].Equals("branch")))
                {
                    fldBranchCode = col["fldBranchCode"];
                }
                else
                {
                    fldBranchCode = "";
                }

                verificationclass = col["fldVerificationClass"];
                verificationlimit = col["fldVerificationLimit"];

                if ((col["userType"].Equals("branch")))
                {

                    verificationclass = "";
                }
                else
                {

                    verificationclass = col["fldVerificationClass"];
                }

                if ((col["userType"].Equals("System")))
                {
                    verificationclass = "";
                    verificationlimit = "";
                }

                if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword"))
                {
                    //Check Password if changed then update password and password update date
                    if (!(col["fldPassword"].Equals(col["passwordChecker"])))
                    {
                        passwordChecker = "1";
                        encryptedPassword = encryptPassword(col["fldPassword"]);
                    }
                }

                string strFldDisableLogin = col["fldDisableLogin"];
                if (strFldDisableLogin == null)
                {
                    strFldDisableLogin = "N";
                }
                string strfldZeroMaker = col["fldZeroMaker"];
                if (strfldZeroMaker == null)
                {
                    strfldZeroMaker = "N";
                }

                sqlParameterNext.Add(new SqlParameter("@fldUserId", col["fldUserId"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldUserDesc", col["fldUserDesc"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldUserType", col["userType"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldbranchid", fldBranchCode));
                sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", verificationclass));
                sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", verificationlimit));
                sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", strFldDisableLogin));
                sqlParameterNext.Add(new SqlParameter("@fldZeroMaker", strfldZeroMaker));

                sqlParameterNext.Add(new SqlParameter("@fldPassword", encryptedPassword));
                sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@passwordChecker", passwordChecker));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCounter", "0"));
                sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd")));
                //Added after Combine
                sqlParameterNext.Add(new SqlParameter("@fldUserAbb", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId",""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
                sqlParameterNext.Add(new SqlParameter("@fldPasswordExpDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldIDExpStatus", ""));
                sqlParameterNext.Add(new SqlParameter("@fldIDExpDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalBankCode", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldbranchuser", col["fldbranchuser"].ToString()));

            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldUserAbb", crtUser));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@fldUserDesc", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUserType", ""));
                sqlParameterNext.Add(new SqlParameter("@fldbranchid", ""));
                sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", ""));
                sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", ""));
                sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", ""));
                sqlParameterNext.Add(new SqlParameter("@fldZeroMaker", ""));

                sqlParameterNext.Add(new SqlParameter("@fldPassword", ""));
                sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@passwordChecker", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCounter", "0"));
                sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", 1));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldPasswordExpDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldIDExpStatus", ""));
                sqlParameterNext.Add(new SqlParameter("@fldIDExpDate", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalBankCode", DateTime.Now.ToString("yyyy-MM-dd")));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldbranchuser", ""));

            }
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciUserMasterTemp", sqlParameterNext.ToArray());
        }
        // xx end
        
        public bool UpdateUserMaster(FormCollection col, string updUser)
        {

            int intRowAffected;
            bool blnResult = false;
            dynamic fldBranchCode = "";
            dynamic passwordChecker = "";
            dynamic encryptedPassword = "";
            dynamic verificationclass = "";
            dynamic verificationlimit = "";

            verificationclass = col["fldVerificationClass"];
            verificationlimit = col["fldVerificationLimit"];

            if ((col["userType"].Equals("branch")))
            {
                fldBranchCode = col["fldBranchCode"];
                verificationclass = col["fldVerificationClass_2"];
            }
            else
            {
                fldBranchCode = "";
                verificationclass = col["fldVerificationClass"];
            }

            if ((col["userType"].Equals("System")))
            {
                verificationclass = col["fldVerificationClass_2"];
                verificationlimit = col["fldVerificationLimit_2"];
            }




            if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword"))
            {
                //Check Password if changed then update password and password update date
                if (!(col["fldPassword"].Equals(col["passwordChecker"])))
                {
                    passwordChecker = "1";
                    encryptedPassword = encryptPassword(col["fldPassword"]);
                }
            }

            string strFldDisableLogin = col["fldDisableLogin"];
            if (strFldDisableLogin == null)
            {
                strFldDisableLogin = "N";
            }
            // xx edit
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", col["fldUserId"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUserDesc", col["fldUserDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUserType", col["userType"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldbranchid", fldBranchCode));
            sqlParameterNext.Add(new SqlParameter("@fldVerificationClass", verificationclass));
            sqlParameterNext.Add(new SqlParameter("@fldVerificationLimit", verificationlimit));
            sqlParameterNext.Add(new SqlParameter("@fldDisableLogin", strFldDisableLogin));
            sqlParameterNext.Add(new SqlParameter("@fldPassword", encryptedPassword));
            sqlParameterNext.Add(new SqlParameter("@fldPassLastUpdDate", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@passwordChecker", passwordChecker));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCounter", "0"));
            sqlParameterNext.Add(new SqlParameter("@fldLastLoginDate", DateTime.Now.ToString("yyyy-MM-dd")));
            sqlParameterNext.Add(new SqlParameter("@fldbranchuser", col["fldbranchuser"].ToString()));
            // xx end

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuUserMaster", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }
        public UserModel CheckUserMasterByID(string UserAbb, string UserID, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            UserModel user = new UserModel();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", UserID));
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", UserAbb));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserMasterById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                user.fldUserAbb = row["fldUserAbb"].ToString();
                user.fldUserId = row["fldUserId"].ToString();
                user.fldUserDesc = row["fldUserDesc"].ToString();
                user.fldPassword = row["fldPassword"].ToString();
                user.fldBankCode = row["fldBankCode"].ToString();
                user.fldBankDesc = row["fldBankDesc"].ToString();
                user.fldBranchCode = row["fldBranchCode"].ToString();
                user.fldBranchCode3 = row["fldBranchCode3"].ToString();
                user.fldDisableLogin = row["fldDisableLogin"].ToString();
                user.fldFailLoginDate = DateUtils.formatDateFromSql(row["fldFailLoginDate"].ToString());
                user.fldPasswordExpDate = DateUtils.formatDateFromSql(row["fldPasswordExpDate"].ToString());
                user.fldIDExpDate = DateUtils.formatDateFromSql(row["fldIDExpDate"].ToString());
                user.fldCreateTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                user.userType = row["fldUserType"].ToString();
                user.fldPosition = row["fldPosition"].ToString().Trim();
                user.fldPassword = decryptPassword(user.fldPassword);
                user.fldZeroMaker = row["fldZeroMaker"].ToString().Trim();

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
        public UserModel CheckUserMasterInTempByID(string UserAbb, string UserID, string SearchFor)
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
                user.fldBranchCode3 = row["fldBranchCode3"].ToString();
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
        public bool CheckUserMasterTempByID(string userId, string userAbb, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserMasterTempById", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckUserPasswordHistory(string encryptedPassword, string userId)
        {
            bool rtnFlag = true;
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            int usablePassword = 0;
            sqlParameterNext.Add(new SqlParameter("@ReteriveRecord", usablePassword));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgUserPasswordHistory", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    if (row["fldPassword"].Equals(encryptedPassword))
                    {
                        rtnFlag = false;
                    }
                }
                rtnFlag = true;
            }
            return rtnFlag;
        }
        public List<string> ValidateUser(FormCollection col, string action, string userId)
        {

            //string systemLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            string domain = securityProfileDao.GetSecurityProfile().fldUserAuthMethod;
            dynamic securityProfile = securityProfileDao.GetSecurityProfile();
            List<string> err = new List<string>();

            if ((action.Equals("Create")))
            {
                UserModel CheckUserExist = CheckUserMasterByID(col["fldUserAbb"], "", "UserAbb");
                if ((CheckUserExist != null))
                {
                    err.Add(Locale.UserLoginAlreadyTaken);
                }
                //if (col.AllKeys.Contains("userType"))
                //{
                //    if (col["userType"].Equals("System"))
                //    {
                //        UserModel CheckSytemUserExists = CheckUserMasterByID(col["fldUserAbb"], "", "System");
                //        if (CheckSytemUserExists != null)
                //        {
                //            err.Add(Locale.NewSystemUserAlreadyExist);
                //        }
                //    }
                //}
                if ((col["fldUserAbb"].Equals("")))
                {
                    err.Add(Locale.UserLoginCannotbeblank);
                }
                else if ((col["fldUserAbb"].Length < securityProfile.fldUserIdLengthMin))
                {
                    err.Add(Locale.UserLoginCannotBeLessthan + securityProfile.fldUserIdLengthMin + Locale.character);
                }
                else if ((col["fldUserAbb"].Length > securityProfile.fldUserIdLengthMax))
                {
                    err.Add(Locale.UserLoginCannotBeMorethan + securityProfile.fldUserIdLengthMax + Locale.character);
                }
                //if (!(Regex.IsMatch(col["fldUserAbb"], "[A-Z].*[0-9]|[0-9].*[A-Z]"))) {
                //else if (!(Regex.IsMatch(col["fldUserAbb"], "/^(?=.*[0-9])(?=.*[a-zA-Z])([a-zA-Z0-9]+)$/")))
                //{
                //    err.Add(Locale.UserLoginmustbealphanumeric);
                //}
                if ((col["fldUserDesc"].Equals("")))
                {
                    err.Add(Locale.UserNamecannotbeblank);
                }
                if (col["userType"] != null)
                {

                    if (col["userType"].Equals("Admin"))
                    {
                        if ((col["fldVerificationClass"].Equals("")))
                        {
                            err.Add(Locale.VerificationClassCannotBeBlank);
                        }
                        if ((col["fldVerificationLimit"].Equals("")))
                        {
                            err.Add(Locale.VerificationLimitCannotBeBlank);
                        }
                        
                    }
                    if (col["userType"].Equals("branch"))
                    {
                        if ((col["fldVerificationLimit"].Equals("")))
                        {
                            err.Add(Locale.VerificationLimitCannotBeBlank);
                        }
                    }

                }

                if (!"AD".Equals(domain))
                {
                    if (col.AllKeys.Contains("userType"))
                    {
                        if (!col["userType"].Equals("System"))
                        {
                            //if (col.AllKeys.Contains("chkboxPassword"))
                            //{
                                if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword"))
                                {
                                    if ((col["fldPassword"].Equals("")))
                                    {
                                        err.Add(Locale.Passwordcannotbeblank);
                                    }
                                    else if ((col["fldPassword"].Length < securityProfile.fldUserPwdLengthMin))
                                    {
                                        err.Add(Locale.Passwordcannotbelessthan + securityProfile.fldUserPwdLengthMin + Locale.character);
                                    }
                                    else if ((col["fldPassword"].Length > securityProfile.fldUserPwdLengthMax))
                                    {
                                        err.Add(Locale.Passwordcannotbemorethan + securityProfile.fldUserPwdLengthMax + Locale.character);
                                    }
                                    //Modified 20220616
                                    //Not allowing space
                                    else if (!(Regex.IsMatch(col["fldPassword"], @"^[^ ]*([a-zA-Z\S*])+((\S*[a-zA-Z]+\S*\d+\S*)|(\S*\d+\S*[a-zA-Z]+\S*))$")))
                                    {
                                        err.Add(Locale.Passwordmustbealphanumeric);
                                    }//Check if Alphanumeric
                                    else if (!(Regex.IsMatch(col["fldPassword"], @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\W).*$")))
                                    {
                                        err.Add(Locale.Passwordmustbealphanumeric);
                                    }//End of Modified
                                    else if ((col["fldPassword"].Equals(col["fldUserAbb"])))
                                    {
                                        err.Add(Locale.PasswordcannotbesameasUserLogin);
                                    }
                                    else if ((col["fldPassword"].Equals(col["fldUserDesc"])))
                                    {
                                        err.Add(Locale.PasswordcannotbesameasUserName);
                                    }
                                    else if (!CheckUserPasswordHistory(encryptPassword(col["fldPassword"]), userId))
                                    {
                                        err.Add("Password cannot be used as previous password");
                                    }

                                    if ((col["fldConfirmPassword"].Equals("")))
                                    {
                                        err.Add("Confirm Password Cannot be empty");
                                    }
                                    else if (!col["fldConfirmPassword"].Equals(col["fldPassword"]))
                                    {
                                        err.Add("Password and Confirm Password is not Same");
                                    }
                                }
                                else
                                {
                                    err.Add("Password and Confirm Cannot be empty");
                                }
                           // }
                        }
                    }
                }
                if (col.AllKeys.Contains("userType"))
                {
                    if ((col["userType"].Equals("branch")))
                    {
                        if ((col["fldBranchCode"].Equals("")))
                        {
                            err.Add(Locale.Pleaseselectabranch);
                        }
                    }
                }
                else
                {
                    err.Add("Please Select User Type");
                }
            }
            //update
            else if ((action.Equals("Update")))
            {
                if ((col["fldUserDesc"].Equals("")))
                {
                    err.Add(Locale.UserNamecannotbeblank);
                }
                if (col["userType"].Equals("Admin"))
                {
                    if ((col["fldVerificationClass"].Equals("")))
                    {
                        err.Add(Locale.VerificationClassCannotBeBlank);
                    }
                    if ((col["fldVerificationLimit"].Equals("")))
                    {
                        err.Add(Locale.VerificationLimitCannotBeBlank);
                    }
                }
                if (col["userType"].Equals("branch"))
                {
                    if ((col["fldVerificationLimit"].Equals("")))
                    {
                        err.Add(Locale.VerificationLimitCannotBeBlank);
                    }
                }
                if (col.AllKeys.Contains("userType"))
                {
                    if (!col["userType"].Equals("System"))
                    {
                        if (col.AllKeys.Contains("chkboxPassword"))
                        {
                            if (col.AllKeys.Contains("fldPassword") && col.AllKeys.Contains("fldConfirmPassword"))
                            {
                                if ((col["fldPassword"].Equals("")))
                                {
                                    err.Add(Locale.Passwordcannotbeblank);
                                }
                                else if ((col["fldPassword"].Length < securityProfile.fldUserPwdLengthMin))
                                {
                                    err.Add(Locale.Passwordcannotbelessthan + securityProfile.fldUserPwdLengthMin + Locale.character);
                                }
                                else if ((col["fldPassword"].Length > securityProfile.fldUserPwdLengthMax))
                                {
                                    err.Add(Locale.Passwordcannotbemorethan + securityProfile.fldUserPwdLengthMax + Locale.character);
                                }
                                else if (!(Regex.IsMatch(col["fldPassword"], @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\W).*$")))
                                {
                                    err.Add(Locale.Passwordmustbealphanumeric);
                                }
                                else if ((col["fldPassword"].Equals(col["fldUserAbb"])))
                                {
                                    err.Add(Locale.PasswordcannotbesameasUserLogin);
                                }
                                else if ((col["fldPassword"].Equals(col["fldUserDesc"])))
                                {
                                    err.Add(Locale.PasswordcannotbesameasUserName);
                                }
                                else if (!CheckUserPasswordHistory(encryptPassword(col["fldPassword"]), userId))
                                {
                                    err.Add("Password cannot be used as previous password");
                                }

                                if ((col["fldConfirmPassword"].Equals("")))
                                {
                                    err.Add("Confirm Password Cannot be empty");
                                }
                                else if (!col["fldConfirmPassword"].Equals(col["fldPassword"]))
                                {
                                    err.Add("Password and Confirm Password is not Same");
                                }
                            }
                            else
                            {
                                err.Add("Password and Confirm Cannot be empty");
                            }
                        }
                    }
                }
                if (col.AllKeys.Contains("userType"))
                {
                    if ((col["userType"].Equals("branch")))
                    {
                        if ((col["fldBranchCode"].Equals("")))
                        {
                            err.Add(Locale.Pleaseselectabranch);
                        }
                    }
                }
                else
                {
                    err.Add("Please Select User Type");
                }

            }
            return err;
        }
        public List<UserModel> ListBranch(string bankcode)
        {
            DataTable resultTable = new DataTable();
            List<UserModel> branchList = new List<UserModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchesForUserProfile", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel branch = new UserModel();
                    branch.fldCBranchId = row["fldCBranchId"].ToString();
                    branch.fldBranchDesc = row["fldCBranchDesc"].ToString();
                    branch.fldIBranchId = row["fldIBranchId"].ToString();
                    branch.fldInternalBranchId = row["fldInternalBranchId"].ToString();
                    branchList.Add(branch);
                }
            }
            return branchList;
        }
        public List<UserModel> ListVerificationClass(string bankcode)
        {
            DataTable resultTable = new DataTable();
            List<UserModel> VerificationClassList = new List<UserModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgVerificationClass", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel VerificationClass = new UserModel();
                    VerificationClass.fldClass = row["fldClass"].ToString();
                    VerificationClass.fldLimitDesc = row["fldLimitDesc"].ToString();
                    VerificationClassList.Add(VerificationClass);
                }
            }
            return VerificationClassList;
        }
        public DateTime FormatExpDate(string expiryDate)
        {//99D
            DateTime result = new DateTime();
            string expiryDigit = new string(expiryDate.Where(char.IsDigit).ToArray());
            string expiryStr = StringUtils.UCase(StringUtils.Right(expiryDate.Trim(), 1).ToString());

            if (expiryStr == "D")
            {
                result = DateTime.Now.AddDays(Convert.ToDouble(expiryDigit));
            }
            else if (expiryStr == "M")
            {
                result = DateTime.Now.AddMonths(Convert.ToInt16(expiryDigit));
            }
            else if (expiryStr == "Y")
            {
                result = DateTime.Now.AddYears(Convert.ToInt16(expiryDigit));
            }
            return result;
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
        public string encryptPassword(string stringPassword)
        {
            string result = "";
            try
            {
                ICSSecurity.EncryptDecrypt encryptDecrypt = new ICSSecurity.EncryptDecrypt();
                encryptDecrypt.FilePath = systemProfileDao.GetDLLPath();
                result = encryptDecrypt.EncryptString128Bit(stringPassword);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        //Marlon 20220607
        public void ApproveInChecker(string userAbb, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciUserProfileChecker", sqlParameterNext.ToArray());
        }

        public void RejectInChecker(string userAbb, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserAbb", userAbb));           
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdUserMasterFromTemp", sqlParameterNext.ToArray());
        }


        //End 20220607
    }
}