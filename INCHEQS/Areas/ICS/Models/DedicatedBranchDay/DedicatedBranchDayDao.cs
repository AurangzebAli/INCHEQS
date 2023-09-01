using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Resources;
using System.Web.Mvc;
using INCHEQS.Models;
using INCHEQS.Security;
using INCHEQS.Security.SystemProfile;
using System.Text.RegularExpressions;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Helpers;
using System.Globalization;
using INCHEQS.DataAccessLayer;

namespace INCHEQS.Areas.ICS.Models.DedicatedBranchDay
{
    public class DedicatedBranchDayDao : IDedicatedBranchDayDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        public DedicatedBranchDayDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }

        public DataTable ListAll()
        {
            DataTable ds = new DataTable();
            string 
            stmt = "SELECT * from tblUserMaster";

            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }

       public DedicatedBranchDayModel FilterCCU()
        {
            DedicatedBranchDayModel branch = new DedicatedBranchDayModel();
            string stmt = "Select *,CASE WHEN isNULL(fldBranchCode,'') ='' THEN 'CCU' ELSE 'Branch User' END as userType from tblUserMaster where isNull(fldBranchCode,'')=''";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in dt.Rows)
            {
                branch.userType = row["userType"].ToString();          
            }
            return branch;
        }


        public DedicatedBranchDayModel getUserProfile(String userId)
        {
            DedicatedBranchDayModel profile = new DedicatedBranchDayModel();
            String stmt = "Select *,Case When isNull(fldAdminFlag,'') = 'Y' then 'Officer' WHEN isNull(fldAdminFlag,'') = 'S' Then 'Supervisor' else 'Verifier' END as userType2 from tblUsermaster where fldUserAbb='" + userId + "'";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in dt.Rows)
            {
                profile.userType = row["userType2"].ToString();
            }
                return profile;
        }

        public DedicatedBranchDayModel InsertUserDedicatedBranch(FormCollection col)
        {
            dynamic bankCode = CurrentUser.Account.BankCode;
            DedicatedBranchDayModel user = new DedicatedBranchDayModel();
            string ccutypeVal = "";
            string officer = "";
            string AdminFlag = "";
            string ccutype = col["userType2"].ToString();
            if (ccutype == "Officer")
            {
                ccutypeVal = "Y";
            }
            if (ccutype == "Verifier")
            {
                ccutypeVal = "N";
            }
            if (ccutypeVal == "N")
            {
                string stm = "Select distinct fldOfficerAbb,b.fldAdminFlag from tblDedicatedBranchOfficer a inner join tblUsermaster b " +
                    "on a.fldOfficerAbb=b.fldUserAbb where a.fldUserAbb='" + col["fldUserAbb"].ToString() + "'";
                DataTable ds = new DataTable();
                ds = dbContext.GetRecordsAsDataTable(stm);
                foreach (DataRow row in ds.Rows)
                {
                    officer = row["fldOfficerAbb"].ToString();
                    AdminFlag = row["fldAdminFlag"].ToString();
                }
            }
            if (String.IsNullOrEmpty(col["selectedTask"]))
            {
                string converted = DateTime.ParseExact(col["fldIDExpDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                string stmt = "Delete from tblDedicatedBranchDate where fldUserId='" + officer.Trim() + "' and " +
                "fldBranchId in (select fldBranchId from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "')";
                dbContext.GetRecordsAsDataTable(stmt);
                stmt = "Delete from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "'";// and fldClearDate='" + converted + "'";
                dbContext.GetRecordsAsDataTable(stmt);
            }
            else
            {
                string[] branchArray = (col["selectedTask"].ToString()).Split(',');
                if (ccutypeVal == "N")
                {

                    string stm = "Delete from tblDedicatedBranchDate where fldUserId='"+ officer.Trim() + "' and "+
                        "fldBranchId in (select fldBranchId from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "')";
                    dbContext.GetRecordsAsDataTable(stm);

                }
                //cast date format dd-MM-yyyy to yyyyMMdd
                string converted = DateTime.ParseExact(col["fldIDExpDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                // delete from table tblDedicatedBranchDate when update branch for specific user
                string stmt = "Delete from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "'";
                dbContext.GetRecordsAsDataTable(stmt);
                // insert to tblDedicatedBranchDate loop by branch selected
                foreach (string branchArrayList in branchArray)
                {
                    stmt = "Insert into tblDedicatedBranchDate" +
                    "(fldUserId,fldBranchId,fldClearDate,fldAdminFlag) VAlUES (@fldUserAbb, @fldBranchId,@fldIDExpDate,@fldAdminFlag)";
                    dbContext.ExecuteNonQuery(stmt, new[] {
                    new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()),
                    new SqlParameter("@fldBranchId", branchArrayList.ToString()),
                    new SqlParameter("@fldIDExpDate", converted),
                    new SqlParameter("@fldAdminFlag",ccutypeVal)
                });

                    if ((ccutypeVal == "N") && (!string.IsNullOrEmpty(officer)))
                    {
                        stmt = "Insert into tblDedicatedBranchDate" +
                        "(fldUserId,fldBranchId,fldClearDate,fldAdminFlag) VAlUES (@fldUserAbb, @fldBranchId,@fldIDExpDate,@fldAdminFlag)";
                        dbContext.ExecuteNonQuery(stmt, new[] {
                        new SqlParameter("@fldUserAbb", officer.Trim()),
                        new SqlParameter("@fldBranchId", branchArrayList.ToString()),
                        new SqlParameter("@fldIDExpDate", converted),
                        new SqlParameter("@fldAdminFlag",AdminFlag.Trim())
                        });
                    }
                }

                // delete from table tblDedicatedBranchUser when update branch for specific user
                //stmt = "Delete from tblDedicatedBranchUser where fldUserAbb = '" + col["fldUserAbb"].ToString() + "'";
                //dbContext.GetRecordsAsDataTable(stmt);
                ////insert to tblDedicatedBranchUser for every update according user and date
                //string stmt2 = "Insert into tblDedicatedBranchUser" +
                //"(fldUserAbb,fldClearDate) VALUES (@fldUserAbb,@fldIDExpDate)";
                //dbContext.ExecuteNonQuery(stmt2, new[] {
                //new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()),
                //new SqlParameter("@fldIDExpDate", converted)
                //});
            }
            return user;
        }

        public List<DedicatedBranchDayModel> ListAvailableBranch(string userId, string clearDate)
        {
            DataTable cdt = new DataTable();
            string param = "";
            string condition = "Select fldAdminFlag from tblUserMaster where fldUserAbb = '"+ userId + "'";
            cdt = dbContext.GetRecordsAsDataTable(condition);
            foreach (DataRow row in cdt.Rows)
            {
                if (row["fldAdminFlag"].ToString()=="N")
                {
                    param = "Where fldAdminFlag='N'";
                }
                if (row["fldAdminFlag"].ToString() != "N")
                {
                    param = "Where fldUserId='"+ userId + "'";
                }
            }

            string fldClearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string stmt = "Select isNULL(fldConBranchCode,'')[fldConBranchCode], fldBranchDesc From tblMapBranch Where "+
                "fldBankCode=@fldBankCode and "+
                "fldConBranchCode not in (Select fldBranchId from tblDedicatedBranchDate) Order By fldBranchDesc ";
            DataTable ds = new DataTable();
            List<DedicatedBranchDayModel> branchAvailable = new List<DedicatedBranchDayModel>();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode) });
            foreach (DataRow row in ds.Rows)
            {
                DedicatedBranchDayModel branchAvailableLsit = new DedicatedBranchDayModel();
                branchAvailableLsit.fldConBranchCode = row["fldConBranchCode"].ToString();
                branchAvailableLsit.fldBranchDesc = row["fldBranchDesc"].ToString();
                branchAvailable.Add(branchAvailableLsit);
            }
            return branchAvailable;
        }

        public List<string> ValidateUser(FormCollection col, string action)
        {

            string systemLoginAD = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("LoginAD").Trim();
            dynamic securityProfile = securityProfileDao.GetSecurityProfile();
            List<string> err = new List<string>();

            if ((action.Equals("Create")))
            {
                if ((CheckUserExist(col["fldUserAbb"])))
                {
                    err.Add(Locale.UserAbbreviationalreadytaken);
                }
            }
            if ((col["fldUserAbb"].Equals("")))
            {
                err.Add(Locale.UserAbbreviationcannotbeblank);
            }
            if ((col["fldUserAbb"].Length < securityProfile.fldIDLengthMin))
            {
                err.Add(Locale.UserAbbreviationcannotbelessthan + securityProfile.fldIDLengthMin + Locale.character);
            }
            if ((col["fldUserAbb"].Length > securityProfile.fldIDLengthMax))
            {
                err.Add(Locale.UserAbbreviationcannotbemorethan + securityProfile.fldIDLengthMin + Locale.character);
            }
            if (!(Regex.IsMatch(col["fldUserAbb"], "[A-Z].*[0-9]|[0-9].*[A-Z]")))
            {
                err.Add(Locale.UserAbbreviationmustbealphanumeric);
            }


            if (!"Y".Equals(systemLoginAD))
            {
                if ((col["fldPassword"].Equals("")))
                {
                    err.Add(Locale.Passwordcannotbeblank);
                }
                else if ((col["fldPassword"].Length < securityProfile.fldPwdLengthMin))
                {
                    err.Add(Locale.Passwordcannotbelessthan + securityProfile.fldIDLengthMin + Locale.character);
                }
                else if ((col["fldPassword"].Length > securityProfile.fldIDLengthMax))
                {
                    err.Add(Locale.Passwordcannotbemorethan + securityProfile.fldIDLengthMax + Locale.character);
                }
                if (!(Regex.IsMatch(col["fldPassword"], "[a-z].*[0-9]|[0-9].*[a-z]")))
                {
                    err.Add(Locale.Passwordmustbealphanumeric);
                }
                if ((col["fldPassword"].Equals(col["fldUserAbb"])))
                {
                    err.Add(Locale.PasswordcannotbesameasUserAbbreviation);
                }
                if ((col["fldPassword"].Equals(col["fldUserDesc"])))
                {
                    err.Add(Locale.PasswordcannotbesameasUserDescription);
                }

            }

            return err;
        }

        public bool CheckUserExist(string userAbb)
        {

            string stmt = " SELECT * FROM tblUserMaster WHERE fldUserAbb=@fldUserAbb";
            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldUserAbb", userAbb) });

        }


        public List<DedicatedBranchDayModel> ListSelectedBranch(string userId,string clearDate)
        {
            string fldClearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string stmt = "Select isNULL(a.fldConBranchCode,'')[fldConBranchCode], fldBranchDesc,fldAdminFlag From tblMapBranch a " +
                          "inner join tblDedicatedBranchDate b on a.fldConBranchCode = b.fldBranchId " +
                          "Where b. fldUserId = '" + userId + "' /*and fldClearDate='" + fldClearDate + "'*/ Order By a.fldBranchDesc ";
            DataTable ds = new DataTable();
            List<DedicatedBranchDayModel> branchSelected = new List<DedicatedBranchDayModel>();
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                DedicatedBranchDayModel branchSelectedList = new DedicatedBranchDayModel();
                branchSelectedList.fldConBranchCode = row["fldConBranchCode"].ToString();
                branchSelectedList.fldBranchDesc = row["fldBranchDesc"].ToString();
                branchSelectedList.fldOfficerId = row["fldAdminFlag"].ToString();
                branchSelected.Add(branchSelectedList);
            }

            return branchSelected;
        }


    }
}