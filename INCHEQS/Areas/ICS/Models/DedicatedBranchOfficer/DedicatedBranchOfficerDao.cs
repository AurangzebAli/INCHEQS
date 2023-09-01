using INCHEQS.Areas.ICS.Models.DedicatedBranchOfficer;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Models;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;

namespace INCHEQS.Areas.ICS.Models.DedicatedBranchOfficer
{
    public class DedicatedBranchOfficerDao : iDedicatedBranchOfficerDao
    {

        private readonly ApplicationDbContext dbContext;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        public DedicatedBranchOfficerDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }

        public List<DedicatedBranchOfficerModel> ListSelectedVerifier(string userId, string clearDate)
        {
            DataTable cdt = new DataTable();
            string flag = "";
            string condition = "Select fldAdminFlag from tblUserMaster where fldUserAbb = '" + userId + "'";
            cdt = dbContext.GetRecordsAsDataTable(condition);
            foreach (DataRow row in cdt.Rows)
            {
                flag = row["fldAdminFlag"].ToString();
            }
            string fldClearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string stmt = "select b.flduserabb,a.fldAdminFlag,a.flduserDesc from tblusermaster a inner join tblDedicatedBranchOfficer b on a.fldUserAbb=b.fldOfficerAbb" +
                          " where fldAdminFlag = '"+ flag + "' and a.fldUserAbb = '" + userId + "'";
            DataTable ds = new DataTable();
            List<DedicatedBranchOfficerModel> branchSelected = new List<DedicatedBranchOfficerModel>();
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                DedicatedBranchOfficerModel branchSelectedList = new DedicatedBranchOfficerModel();
                branchSelectedList.fldOfficerId = row["fldAdminFlag"].ToString();
                branchSelectedList.fldUserId = row["fldUserAbb"].ToString();
                branchSelectedList.fldUserDesc = row["flduserDesc"].ToString();
                branchSelected.Add(branchSelectedList);
            }

            return branchSelected;
        }

        public List<DedicatedBranchOfficerModel> ListAvailableVerifier(string userId, string clearDate)
        {
            DataTable cdt = new DataTable();
            string flag = "";
            string condition = "Select fldAdminFlag from tblUserMaster where fldUserAbb = '" + userId + "'";
            cdt = dbContext.GetRecordsAsDataTable(condition);
            foreach (DataRow row in cdt.Rows)
            {
                flag = row["fldAdminFlag"].ToString();
            }

            string fldClearDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string stmt = " select  a.flduserabb,a.flduserDesc from tblusermaster a inner join tblDedicatedBranchUser b on a.flduserabb=b.fldUserAbb  where fldAdminFlag='N' and a.flduserabb not in (select flduserabb from tblDedicatedBranchOfficer where fldflag in ('S','Y'))";
            DataTable ds = new DataTable();
            List<DedicatedBranchOfficerModel> branchAvailable = new List<DedicatedBranchOfficerModel>();
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                DedicatedBranchOfficerModel branchAvailableLsit = new DedicatedBranchOfficerModel();
                branchAvailableLsit.fldUserId = row["flduserabb"].ToString();
                branchAvailableLsit.fldUserDesc = row["flduserDesc"].ToString();
                branchAvailable.Add(branchAvailableLsit);
            }
            return branchAvailable;
        }

        public DedicatedBranchOfficerModel InsertUserDedicatedBranch(FormCollection col)
        {
            dynamic bankCode = CurrentUser.Account.BankCode;
            DedicatedBranchOfficerModel user = new DedicatedBranchOfficerModel();
            string ccutypeVal = "";
            string ccutype = col["userType2"].ToString();
            if (ccutype == "Officer")
            {
                ccutypeVal = "Y";
            }
            if (ccutype == "Supervisor")
            {
                ccutypeVal = "S";
            }

            if (String.IsNullOrEmpty(col["selectedTask"]))
            {
                string converted = DateTime.ParseExact(col["fldIDExpDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                DataTable ds = new DataTable();
                string verifieruser = "";
                string stmt = "Select fldUserAbb from tblDedicatedBranchOfficer where fldOfficerAbb='" + col["fldUserAbb"].ToString() + "'";
                ds= dbContext.GetRecordsAsDataTable(stmt);
                foreach (DataRow rows in ds.Rows)
                {
                    verifieruser = rows["fldUserAbb"].ToString();
                    stmt = "Delete from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "' and fldbranchid in "+
                        "(select fldbranchid from tblDedicatedBranchDate where flduserid='"+ verifieruser + "')";
                    dbContext.GetRecordsAsDataTable(stmt);
                }
                stmt= "Delete from tblDedicatedBranchOfficer where fldOfficerAbb='"+ col["fldUserAbb"].ToString() + "'";
                dbContext.GetRecordsAsDataTable(stmt);

            }
            else
            {
                string[] branchArray = (col["selectedTask"].ToString()).Split(',');

                //cast date format dd-MM-yyyy to yyyyMMdd
                string converted = DateTime.ParseExact(col["fldIDExpDate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
                // delete from table tblDedicatedBranchDate when update branch for specific user
                DataTable ds = new DataTable();
                string verifieruser = "";
                string stmt = "Select fldUserAbb from tblDedicatedBranchOfficer where fldOfficerAbb='" + col["fldUserAbb"].ToString() + "'";
                ds = dbContext.GetRecordsAsDataTable(stmt);
                foreach (DataRow rows in ds.Rows)
                {
                    verifieruser = rows["fldUserAbb"].ToString();
                    stmt = "Delete from tblDedicatedBranchDate where fldUserid = '" + col["fldUserAbb"].ToString() + "' and fldbranchid in " +
                        "(select fldbranchid from tblDedicatedBranchDate where flduserid='" + verifieruser + "')";
                    dbContext.GetRecordsAsDataTable(stmt);
                }
                stmt = "Delete from tblDedicatedBranchOfficer where fldOfficerAbb='" + col["fldUserAbb"].ToString() + "'";
                dbContext.GetRecordsAsDataTable(stmt);
                // insert to tblDedicatedBranchDate loop by branch selected
                foreach (string branchArrayList in branchArray)
                {

                    stmt = "Select fldBranchId,fldClearDate from tblDedicatedBranchDate where fldUserId ='"+ branchArrayList.ToString() + "'";
                    ds = dbContext.GetRecordsAsDataTable(stmt);
                    foreach (DataRow rows in ds.Rows)
                    {

                        stmt = "Insert into tblDedicatedBranchDate" +
                        "(fldUserId,fldBranchId,fldClearDate,fldAdminFlag) VAlUES (@fldUserAbb, @fldBranchId,@fldIDExpDate,@fldAdminFlag)";
                        dbContext.ExecuteNonQuery(stmt, new[] {
                        new SqlParameter("@fldUserAbb", col["fldUserAbb"].ToString()),
                        new SqlParameter("@fldBranchId", rows["fldBranchId"].ToString()),
                        new SqlParameter("@fldIDExpDate", converted),
                        new SqlParameter("@fldAdminFlag",ccutypeVal)
                        });

                    }
                    stmt = "Insert into tblDedicatedBranchOfficer" +
                    "(fldUserAbb,fldOfficerAbb,fldFlag) VALUES (@fldUserAbb,@fldUserOfficer,@fldFlag)";
                    dbContext.ExecuteNonQuery(stmt, new[] {
                        new SqlParameter("@fldUserAbb", branchArrayList.ToString()),
                        new SqlParameter("@fldUserOfficer", col["fldUserAbb"].ToString()),
                        new SqlParameter("@fldFlag",ccutypeVal)
                        });
                }
            }
            return user;
        }
    }
}