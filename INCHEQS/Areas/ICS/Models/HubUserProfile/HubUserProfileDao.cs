using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.HubUserProfile {
    public class HubUserProfileDao : IHubUserProfileDao{

        private readonly ApplicationDbContext dbContext;
        public HubUserProfileDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable ListAllHubUser() {
            DataTable ds = new DataTable();

            string stmt = "SELECT * FROM tblHubMaster WHERE fldBankCode=@bankCode AND fldSpickCode=@spickCode";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@bankCode",CurrentUser.Account.BankCode),
                    new SqlParameter("@spickCode",CurrentUser.Account.SpickCode)
                });
            return ds;
        }
        public DataTable getHubUser(string hubId) {
            DataTable ds = new DataTable();

            string stmt = "select * from tblHubMaster where fldHubId = @hubId";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@hubId",hubId)
                });

            return ds;
        }
        public DataTable getAvailableHubUserList() {
            DataTable ds = new DataTable();

            string stmt = "select fldUserId,fldUserAbb,fldUserDesc from tblUsermaster "+ 
                " where fldBankCode= @bankcode and fldUserId not in "+
                " (select fldUserId from tblHubUser)"+
                "and fldDeleted is null and fldBPCSPICK ='LAHORE' and fldBankCode = @bankid";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@bankcode",CurrentUser.Account.BankCode),
                    new SqlParameter("@bankid",CurrentUser.Account.BankCode)
                });
            return ds;
        }
        public DataTable getSelectedHubUserList(string hubId) {
            DataTable ds = new DataTable();

            string stmt = "Select fldUserId,fldUserAbb,fldUserDesc from tblUsermaster Where fldUserId in "+
                " (select b.fldUserId from tblHubUser b inner join tblUserMaster um on b.fldUserID = um.fldUserID "+
                "where b.fldHubId = @hubid and (um.fldDeleted <> '1' or um.fldDeleted is null)and fldBPCSPICK ='LAHORE' )"+
                "and fldBPCSPICK = 'LAHORE' and fldBankCode = @bankCode";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@hubid",hubId),
                new SqlParameter("@bankCode",CurrentUser.Account.BankCode)

                });
            return ds;
        }
        public DataTable UpdateHubMaster(FormCollection col) {
            DataTable ds=new DataTable();
            string stmt = "Update tblHubMaster"+
                " SET fldHubDesc = @hubDesc,"+
                " fldUpdateUserId = @updateUserId," +
                " fldUpdateTimeStamp = @updateTime," +
                " fldBranchGroup = @branchGroup " +
                " WHERE fldHubId = @hubId ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubDesc",col["hubDesc"]),
                new SqlParameter("@updateUserId",CurrentUser.Account.UserId),
                new SqlParameter("@updateTime",DateTime.Now),
                new SqlParameter("@branchGroup",""),
                new SqlParameter("@hubId",col["hubId"])
            });
            return ds;
        }
        public void DeleteNotSelected(string hubbranchid, string selectedbranches) {
            string[] aryText = selectedbranches.Split(',');
            if ((aryText.Length > 0)) {
                string stmt = "Delete from tblHubUser where flduserid not in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") and fldHubId='" + hubbranchid + "'";

                dbContext.ExecuteNonQuery(stmt, DatabaseUtils.getSqlParametersFromArray(aryText).ToArray());
            }
        }
        public bool CheckHubUserExistInGroup(string hubbranchid, string selectedbranches) {
            string stmt = "Select * from tblHubUser where fldUserId in (@selectedbranches) AND fldHubId=@hubid";

            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@selectedbranches", selectedbranches),
                new SqlParameter("@hubid", hubbranchid)
            });
            if (((ds.Rows.Count > 0))) {
                return true;
            } else {
                return false;
            }
        }
        public void UpdateSelectedUser(string hubbranchid, string selectedbranches) {
            string stmt = "update tblHubUser set fldUpdateUserId=@UpdateUserid, fldUpdateTimeStamp=@updateDate where fldHubId=@hubId AND fldUserId=@userId";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@UpdateUserid", CurrentUser.Account.UserId),
                new SqlParameter("@updateDate", DateTime.Now),
                new SqlParameter("@hubId", hubbranchid),
                new SqlParameter("@userId", selectedbranches)
            });

        }
        public void InsertUserInGroup(string hubbranchid, string selectedbranches) {
            string stmt = "insert into tblHubUser "+
                "(fldHubId,fldUserId, fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp)"+
                "Values (@hubid,@selectuserid,@createuserid,@createtime,@updateuser,@updatetime)";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubid", hubbranchid),
                new SqlParameter("@selectuserid", selectedbranches),
                new SqlParameter("@createuserid", CurrentUser.Account.UserId),
                new SqlParameter("@createtime", DateTime.Now),
                new SqlParameter("@updateuser", CurrentUser.Account.UserId),
                new SqlParameter("@updatetime", DateTime.Now)
            });
        }

        public void DeleteAllHubUser(string hubbranchId) {
            string stmt = " Delete from tblHubUser where fldHubId=@hubID";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubID", hubbranchId)
            });
        }

        public void DeleteHubMaster(string deleteMaster) {
            string stmt = " Delete from tblHubMaster WHERE fldHubId = @deleteIds";

            try {
                dbContext.ExecuteNonQuery(stmt, new[] {new SqlParameter("@deleteIds", deleteMaster)
                });
            } catch (Exception ex) {
                throw ex;
            }

        }
        public void DeleteHubMasterUsers(string deleteBranch) {
            string stmt = " delete from tblHubUser where fldHubId =@deleteId";
            try {
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@deleteId", deleteBranch) });

            } catch (Exception ex) {
                throw ex;
            }
        }
        public List<string> Validate(FormCollection col) {

            List<string> err = new List<string>();
            
            if ((col["hubDesc"]).Equals("")) {
                err.Add(Locale.HubDescriptioncannotbeblank);
            }
            return err;
        }
    }
}