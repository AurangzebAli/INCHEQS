using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.HubBranchProfile {
    public class HubBranchProfileDao : IHubBranchProfileDao {

        private readonly ApplicationDbContext dbContext;
        public HubBranchProfileDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable ListAllHubBranch() {
            DataTable ds = new DataTable();

            string stmt = "SELECT * FROM tblHubMaster WHERE fldBankCode=@bankcode AND fldSpickCode=@pickcode";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@bankcode",CurrentUser.Account.BankCode),
                    new SqlParameter("@pickcode",CurrentUser.Account.SpickCode)
                });
            return ds;
        }
        public DataTable getHubBranch(string hubId) {
            DataTable ds = new DataTable();

            string stmt = "select * from tblHubMaster where fldHubId = @hubId";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@hubId",hubId)
                });

            return ds;
        }

        public DataTable getAvailableHubBranchList(string hubId) {
            DataTable ds = new DataTable();

            string stmt = "select fldBranchCode,fldBranchDesc,fldConBranchCode, fldStateDesc from tblMapBranch Where " +
                "fldBranchCode not in (select fldBranchCode from tblHubBranch where fldHubId=@hubid) " +
                "order by fldStateCode,fldBranchCode asc";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@hubid",hubId)
                });
            return ds;
        }
        public DataTable getSelectedHubBranchList(string hubId) {
            DataTable ds = new DataTable();

            string stmt = "select fldBranchCode,fldBranchDesc,fldConBranchCode, fldStateDesc from tblMapBranch Where " +
                "fldBranchCode IN (select fldBranchCode from tblHubBranch where fldHubId=@hubid) " +
                "order by fldStateCode,fldBranchCode asc";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
                    new SqlParameter("@hubid",hubId)
                });
            return ds;
        }

        public void DeleteNotSelected(string hubbranchid, string selectedbranches) {
            string[] aryText = selectedbranches.Split(',');
            if ((aryText.Length > 0)) {
                string stmt = "delete from tblHubBranch where fldBranchCode not in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") and fldHubId='" + hubbranchid + "'";

                dbContext.ExecuteNonQuery(stmt, DatabaseUtils.getSqlParametersFromArray(aryText).ToArray());
            }
        }

        public bool CheckHubBranchExistInGroup(string hubbranchid, string selectedbranches) {
            string stmt = "select * from tblHubBranch where fldBranchCode in (@selectedbranches) AND fldHubId=@hubid";

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
        public void InsertBranchInGroup(string hubbranchid, string selectedbranches) {
            string stmt = "insert into tblHubBranch(fldHubId,fldBranchCode,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp)" +
                            "Values(@hubbranchid, @selectedbranch, @userid , @createdate, @useridupdate , @updatedate)";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubbranchid", hubbranchid),
                new SqlParameter("@selectedbranch", selectedbranches),
                new SqlParameter("@userid", CurrentUser.Account.UserId),
                new SqlParameter("@createdate", DateTime.Now),
                new SqlParameter("@useridupdate", CurrentUser.Account.UserId),
                new SqlParameter("@updatedate", DateTime.Now)
            });
        }
        public void UpdateSelectedBranch(string hubbranchid, string selectedbranches) {
            string stmt = "update tblHubBranch set fldUpdateUserId=@UpdateUserid, fldUpdateTimeStamp=@updateDate where fldHubId=@hubId AND fldBranchCode=@branchCode";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@UpdateUserid", CurrentUser.Account.UserId),
                new SqlParameter("@updateDate", DateTime.Now),
                new SqlParameter("@hubId", hubbranchid),
                new SqlParameter("@branchCode", selectedbranches)
            });

        }
        public void DeleteAllHubBranch(string hubbranchId) {
            string stmt = " Delete from tblHubBranch where fldHubId=@hubID";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubID", hubbranchId)
            });
        }
        public DataTable UpdateHubMaster(FormCollection col) {
            DataTable ds=new DataTable();
            string stmt = "Update tblHubMaster SET fldHubDesc=@hubDesc,fldUpdateUserId=@updateUserId,fldUpdateTimeStamp=@updateDate "+
                " where fldHubId = @hubId ";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@hubDesc",col["hubDesc"]),
                new SqlParameter("@updateUserId",CurrentUser.Account.UserId),
                new SqlParameter("@updateDate",DateTime.Now),
                new SqlParameter("@hubId",col["hubId"])
            });
            return ds;
        }
        public DataTable getHubBranchList() {
            DataTable ds = new DataTable();

            string stmt = "select fldBranchCode,fldBranchDesc,fldConBranchCode, fldStateDesc from tblMapBranch "+
                "order by fldStateCode,fldBranchCode asc";

            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }
        public void CreateHubMaster(FormCollection col) {

            string stmt = "INSERT INTO tblHubMaster " +
                " (fldHubId,fldHubDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) " +
                " VALUES (@hubId,@hubDesc,@bankCode,@branchGroup, @SpickCode,@userId, @datenow,@updateUserId,@updateDate)";

            dbContext.ExecuteNonQuery(stmt,new[] {
                new SqlParameter("@hubId", col["hubId"]),
                new SqlParameter("@hubDesc",col["hubDesc"]),
                new SqlParameter("@bankCode", CurrentUser.Account.BankCode),
                new SqlParameter("@branchGroup",""),
                new SqlParameter("@SpickCode",CurrentUser.Account.SpickCode),
                new SqlParameter("@userId",CurrentUser.Account.UserId),
                new SqlParameter("@datenow",DateTime.Now),
                new SqlParameter("@updateUserId",CurrentUser.Account.UserId),
                new SqlParameter("@updateDate",DateTime.Now)
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
        public void DeleteHubMasterBranches(string deleteBranch) {
            string stmt = " delete from tblHubBranch where fldHubId =@deleteId";
            try {
                dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@deleteId", deleteBranch) });

            } catch (Exception ex) {
                throw ex;
            }
        }
        public List<string> Validate(FormCollection col) {

            List<string> err = new List<string>();

            if ((col["hubId"]).Equals("")) {
                err.Add(Locale.HubIDcannotbeblank);
            }
            if ((col["hubDesc"]).Equals("")) {
                err.Add(Locale.HubDescriptioncannotbeblank);
            }
            if (CheckIDexist(col["hubId"])) {
                err.Add(Locale.IdAlreadyExist);
            }

            return err;
        }
        public bool CheckIDexist(string hubId) {

            string stmt = "select fldHubId from tblHubMaster where fldHubId=@hubID";
            DataTable ds = new DataTable();

            ds=dbContext.GetRecordsAsDataTable(stmt,new[] { new SqlParameter("@hubID", hubId) });

            if (((ds.Rows.Count > 0))) {
                return true;
            } else {
                return false;
            }
        }
    }
}