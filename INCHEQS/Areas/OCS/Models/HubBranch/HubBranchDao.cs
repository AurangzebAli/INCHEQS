using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Security.Resources;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Areas.COMMON.Models.InternalBranch;

namespace INCHEQS.Areas.OCS.Models.HubBranch
{
    public class HubBranchDao : IHubBranchDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;

        public HubBranchDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public void AddHubToHubMasterBranchTempToUpdate(FormCollection col, string strUpdate, string strBankCode)
        {
            string str = " INSERT INTO tblHubMasterBranchTemp (fldHubId,fldHubDesc,fldBankCode,fldApprovalStatus,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldHubDesc,@fldBankCode,@fldApprovalStatus,@fldUpdateUserId,@fldUpdateTimeStamp)";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", col["fldHubId"]),
                    new SqlParameter("@fldHubDesc", col["fldHubDesc"]),
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldApprovalStatus", "U"),
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void AddBranchToHubBranchTempToUpdate(string branchId, string hubId, string strUpdate, string strBankCode)
        {
            string str = " INSERT INTO tblHubBranchTemp (fldHubId,fldBranchId,fldBankCode,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldBranchId,@fldBankCode,@fldUpdateUserId,@fldUpdateTimeStamp) ";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldBranchId", branchId),
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public bool CheckBranchExistInHub(string hubId, string branchId)
        {
            bool flag;
            string str = " SELECT * FROM tblHubBranch WHERE fldBranchId=@fldBranchId AND fldHubId=@fldHubId";
            try
            {
                flag = this.ocsdbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId), new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public bool CheckBranchExistInHub(string hubId, string branchId, string strBankCode)
        {
            bool flag;
            string str = " SELECT * FROM tblHubBranch WHERE fldBranchId=@fldBranchId AND fldHubId=@fldHubId and fldBankCode = @fldBankCode";
            try
            {
                flag = this.ocsdbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldBranchId", branchId), new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public bool CheckPendingApproval(string hubId, string StrBankCode)
        {
            bool flag;
            string str = " SELECT * FROM tblHubMasterBranchTemp WHERE fldHubId=@fldHubId and fldBankCode=@fldBankCode";
            try
            {
                flag = this.dbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", StrBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public void DeleteAllBranchInHub(string hubId)
        {
            string str = " Delete from tblHubBranch where fldHubId=@fldHubId";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteAllBranchInHub(string hubId, string strBankCode)
        {
            string str = " Delete from tblHubBranch where fldHubId=@fldHubId and fldBankCode = @fldBankCode";
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) };
                this.ocsdbContext.ExecuteNonQuery(str, sqlParameter);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteAllBranchInHubTemp(string hubId)
        {
            string str = " DELETE FROM tblHubBranchTemp WHERE fldHubId=@fldHubId";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteAllBranchInHubTemp(string hubId, string strBankCode)
        {
            string str = " Delete from tblHubBranchTemp where fldHubId=@fldHubId and fldBankCode = @fldBankCode";
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) };
                this.ocsdbContext.ExecuteNonQuery(str, sqlParameter);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteInHubMasterBranchTemp(string HubId)
        {
            string str = " Delete from tblHubMasterBranchTemp where fldHubId=@fldHubId";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", HubId) });
        }

        public void DeleteBranchNotSelected(string hubId, string branchIds)
        {
            string[] strArrays = branchIds.Split(new char[] { ',' });
            if (strArrays.Length != 0)
            {
                string str = string.Concat("Delete from tblHubBranch where fldHubId=@fldHubId AND fldBranchId not in (", 
                    DatabaseUtils.getParameterizedStatementFromArray(strArrays, ""), ")");

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArrays, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldHubId", hubId));
                try
                {
                    this.dbContext.ExecuteNonQuery(str, sqlParametersFromArray.ToArray());
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        public void DeleteBranchNotSelected(string hubId, string branchIds, string strBankCode)
        {
            string[] strArrays = branchIds.Split(new char[] { ',' });
            if (strArrays.Length != 0)
            {
                string str = string.Concat("Delete from tblHubBranch where fldHubId=@fldHubId and fldBankCode = @fldBankCode AND fldBranchId not in (", 
                    DatabaseUtils.getParameterizedStatementFromArray(strArrays, ""), ")");

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArrays, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldHubId", hubId));
                sqlParametersFromArray.Add(new SqlParameter("@fldBankCode", strBankCode));
                try
                {
                    this.ocsdbContext.ExecuteNonQuery(str, sqlParametersFromArray.ToArray());
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
        }

        public void InsertBranchInHub(string hubId, string branchId, string strUpdate, string strUpdateId)
        {
            string str = " INSERT INTO tblHubBranch (fldHubId,fldBranchId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES (@fldHubId,@fldBranchId,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp) ";

            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldBranchId", branchId),
                    new SqlParameter("@fldCreateUserId", strUpdate),
                    new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldUpdateUserId", strUpdateId),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void InsertBranchInHub(string hubId, string branchId, string strUpdate, string strUpdateId, string strBankCode)
        {
            string strQueryInsert = " INSERT INTO tblHubBranch (fldHubId,fldBranchId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp, fldBankCode) VALUES (@fldHubId,@fldBranchId,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp,@fldBankCode) ";

            try
            {
                this.ocsdbContext.ExecuteNonQuery(strQueryInsert, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldBranchId", branchId),
                    new SqlParameter("@fldCreateUserId", strUpdate),
                    new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldUpdateUserId", strUpdateId),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldBankCode", strBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public List<InternalBranchModel> ListAvailableBranchInHub(string hubId, string strBankCode)
        {
            List<InternalBranchModel> branchModels = new List<InternalBranchModel>();
            DataTable dtHubBranch = new DataTable();
            List<string> branchIds = new List<string>();
            string strQuerySelect;

            try
            {
                strQuerySelect = "SELECT fldBranchId FROM tblHubBranch WHERE fldHubId = @fldHubId AND fldBankCode = @fldBankCode";
                dtHubBranch = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldHubId", hubId)
                });

                if (dtHubBranch.Rows.Count > 0)
                {
                    foreach (DataRow row in dtHubBranch.Rows)
                    {
                        branchIds.Add(row["fldBranchId"].ToString());
                    }
                }

                if (branchIds.Count == 0)
                {
                    branchIds.Add("");
                }
                string[] strArray = branchIds.ToArray();

                strQuerySelect = string.Concat(
                        "SELECT fldBranchCode,fldBranchAbb,fldBranchDesc FROM tblMapBranch WHERE fldBankCode = @fldBankCode ",
                        "AND fldBranchCode NOT IN (", DatabaseUtils.getParameterizedStatementFromArray(strArray, ""), ")"
                    );

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArray, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldBankCode", strBankCode));

                DataTable dataTable = new DataTable();
                dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, sqlParametersFromArray.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        InternalBranchModel branchModel = new InternalBranchModel()
                        {
                           // fldBranchCode = row["fldBranchCode"].ToString(),
                           // fldBranchAbb = row["fldBranchAbb"].ToString()
                        };
                        branchModels.Add(branchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return branchModels;
        }

        public List<InternalBranchModel> ListSelectedBranchInHub(string hubId, string strBankCode)
        {
            List<InternalBranchModel> branchModels = new List<InternalBranchModel>();
            DataTable dtHubBranch = new DataTable();
            List<string> branchIds = new List<string>();
            string strQuerySelect;

            try
            {
                strQuerySelect = "SELECT fldBranchId FROM tblHubBranch WHERE fldBankCode = @fldBankCode AND fldHubId = @fldHubId ";
                dtHubBranch = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldHubId", hubId)
                });

                if (dtHubBranch.Rows.Count > 0)
                {
                    foreach (DataRow row in dtHubBranch.Rows)
                    {
                        branchIds.Add(row["fldBranchId"].ToString());
                    }
                }

                if (branchIds.Count == 0)
                {
                    branchIds.Add("");
                }
                string[] strArray = branchIds.ToArray();

                strQuerySelect = string.Concat(
                        "SELECT fldBranchCode, fldBranchAbb FROM tblMapBranch WHERE fldBankCode = @fldBankCode ",
                        "AND fldBranchCode IN (", DatabaseUtils.getParameterizedStatementFromArray(strArray, ""), ")"
                    );

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArray, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldBankCode", strBankCode));

                DataTable dataTable = new DataTable();
                dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, sqlParametersFromArray.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        InternalBranchModel branchModel = new InternalBranchModel()
                        {
                            //fldBranchCode = row["fldBranchCode"].ToString(),
                            //fldBranchAbb = row["fldBranchAbb"].ToString()
                        };
                        branchModels.Add(branchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return branchModels;
        }

        public List<string> ListSelectedBranchInHubTemp(string hubId, string strBankCode)
        {
            List<string> branchIds = new List<string>();
            DataTable dtHubBranch = new DataTable();
            string strQuerySelect;
            try
            {
                strQuerySelect = "SELECT fldBranchId FROM tblHubBranchTemp WHERE fldHubId = @fldHubId AND fldBankCode = @fldBankCode ";

                DataTable dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldHubId", hubId)
                });

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        branchIds.Add(row["fldBranchId"].ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return branchIds;
        }

        public void UpdateHubMaster(string hubId)
        {
            string strQuerySelect = "SELECT fldUpdateUserId, fldUpdateTimeStamp FROM tblHubMasterBranchTemp WHERE fldHubId=@fldHubId ";
            DataTable dtHubTemp = new DataTable();

            dtHubTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldHubId", hubId)
            });

            if (dtHubTemp.Rows.Count > 0)
            {
                DataRow drItem = dtHubTemp.Rows[0];

                string strQueryUpdate = "Update tblHubMaster SET fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp WHERE fldHubId=@fldHubId ";

                this.ocsdbContext.ExecuteNonQuery(strQueryUpdate, new SqlParameter[] {
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"]),
                    new SqlParameter("@fldHubId", hubId) });
            }

            this.UpdateHubBranch(hubId);
        }

        public void UpdateHubBranch(string hubId)
        {
            List<string> branchIds = this.ListSelectedBranchInHubTemp(hubId, CurrentUser.Account.BankCode);

            if ((branchIds.Count) != 0)
            {
                foreach (string branchId in branchIds)
                {
                    this.DeleteBranchNotSelected(hubId, branchId, CurrentUser.Account.BankCode);
                }
                foreach (string branchId in branchIds)
                {
                    if ((this.CheckBranchExistInHub(hubId, branchId, CurrentUser.Account.BankCode)))
                    {
                        this.UpdateSelectedBranch(hubId, branchId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                    }
                    else
                    {
                        this.InsertBranchInHub(hubId, branchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                    }
                }
            }
            else
            {
                this.DeleteAllBranchInHub(hubId, CurrentUser.Account.BankCode);
            }
        }

        public void UpdateSelectedBranch(string hubId, string BranchId, string strUpdate)
        {
            string str = " update tblHubBranch set fldUpdateBranchId=@fldUpdateBranchId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldHubId=@fldHubId AND fldBranchId=@fldBranchId";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldBranchId", BranchId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void UpdateSelectedBranch(string hubId, string BranchId, string strUpdate, string strBankCode)
        {
            string str = " update tblHubBranch set fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldHubId=@fldHubId AND fldBranchId=@fldBranchId AND fldBankCode = @fldBankCode";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldBranchId", BranchId),
                    new SqlParameter("@fldBankCode", strBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}