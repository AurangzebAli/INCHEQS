using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;

namespace INCHEQS.Areas.OCS.Models.Hub
{
    public class HubDao : IHubDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;

        public HubDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public void AddHubToHubMasterTempToDelete(string HubId)
        {
            string strQuerySelect = "SELECT fldHubId,fldHubDesc,fldBankCode,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp FROM tblHubMaster WHERE fldHubId=@fldHubId ";
            string strQueryInsert = "INSERT INTO tblHubMasterTemp (fldHubId,fldHubDesc,fldBankCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) " +
                                    "VALUES (@fldHubId,@fldHubDesc,@fldBankCode,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp)";
            string strQueryUpdate = "UPDATE tblHubMasterTemp SET fldApprovalStatus=@fldApprovalStatus WHERE fldHubId=@fldHubId ";

            DataTable dtHubTemp = new DataTable();

            dtHubTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldHubId", HubId)
            });

            if (dtHubTemp.Rows.Count > 0)
            {
                DataRow drItem = dtHubTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldHubId", HubId),
                    new SqlParameter("@fldHubDesc", drItem["fldHubDesc"]),
                    new SqlParameter("@fldBankCode", drItem["fldBankCode"]),
                    new SqlParameter("@fldCreateUserId", drItem["fldCreateUserId"]),
                    new SqlParameter("@fldCreateTimeStamp", drItem["fldCreateTimeStamp"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
                dbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldHubId", HubId),
                    new SqlParameter("@fldApprovalStatus", "D")
                });
            }
        }

        public void AddHubToHubMasterTempToUpdate(FormCollection col, string strUpdate, string strBankCode)
        {
            string str = " INSERT INTO tblHubMasterTemp (fldHubId,fldHubDesc,fldBankCode,fldApprovalStatus,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldHubDesc,@fldBankCode,@fldApprovalStatus,@fldUpdateUserId,@fldUpdateTimeStamp)";
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

        public void AddUserToHubUserTempToUpdate(string userId, string hubId, string strUpdate, string strBankCode)
        {
            string str = " INSERT INTO tblHubUserTemp (fldHubId,fldUserId,fldBankCode,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldUserId,@fldBankCode,@fldUpdateUserId,@fldUpdateTimeStamp) ";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldUserId", userId),
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public bool CheckHubExist(string hubId, string StrBankCode)
        {
            bool flag;
            string str = " SELECT * FROM tblHubMaster WHERE fldHubId=@fldHubId and fldBankCode=@fldBankCode";
            try
            {
                flag = this.ocsdbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", StrBankCode) });
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
            string str = " SELECT * FROM tblHubMasterTemp WHERE fldHubId=@fldHubId and fldBankCode=@fldBankCode";
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

        public bool CheckUserExistInHub(string hubId, string userId)
        {
            bool flag;
            string str = " SELECT * FROM tblHubUser WHERE fldUserId=@fldUserId AND fldHubId=@fldHubId";
            try
            {
                flag = this.dbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldUserId", userId), new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public bool CheckUserExistInHub(string hubId, string userId, string strBankCode)
        {
            bool flag;
            string str = " SELECT * FROM tblHubUser WHERE fldUserId=@fldUserId AND fldHubId=@fldHubId and fldBankCode = @fldBankCode";
            try
            {
                flag = this.ocsdbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldUserId", userId), new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public void CreateHubMaster(FormCollection col, string strBankCode, string strUpdate, string strUpdateId)
        {
            string str = " INSERT INTO tblHubMaster (fldHubId,fldHubDesc,fldBankCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldHubDesc,@fldBankCode,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp)";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId",col["fldHubId"]),
                    new SqlParameter("@fldHubDesc", col["fldHubDesc"]),
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldCreateUserId", strUpdate),
                    new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldUpdateUserId", strUpdateId),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now)
                });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void CreateHubMasterTemp(FormCollection col, string strUpdate, string strBankCode, string strUpdateId)
        {
            string str = " INSERT INTO tblHubMasterTemp (fldHubId,fldHubDesc,fldBankCode,fldApprovalStatus,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(@fldHubId,@fldHubDesc,@fldBankCode,@fldApprovalStatus,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp)";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", col["fldHubId"]),
                    new SqlParameter("@fldHubDesc", col["fldHubDesc"]),
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldApprovalStatus", "A"),
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

        public void CreateInHubMaster(string HubId)
        {
            
            string strQueryUpdate = "UPDATE tblHubMasterTemp SET fldApprovalStatus=@fldApprovalStatus WHERE fldHubId=@fldHubId ";
            this.dbContext.ExecuteNonQuery(strQueryUpdate, new SqlParameter[] {
                new SqlParameter("@fldApprovalStatus", "Y"),
                new SqlParameter("@fldHubId", HubId)
            });

            string strQuerySelect = "SELECT fldHubDesc,fldBankCode,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApprovalStatus FROM tblHubMasterTemp WHERE fldHubId = @fldHubId";
            DataTable dtHubTemp = new DataTable();
            dtHubTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldHubId", HubId)
            });

            if (dtHubTemp.Rows.Count > 0)
            {
                DataRow drItem = dtHubTemp.Rows[0];
                string strQueryInsert = "INSERT INTO tblHubMaster(fldHubId,fldHubDesc,fldBankCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApprovalStatus) VALUES (@fldHubId,@fldHubDesc,@fldBankCode,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp,@fldApprovalStatus) ";
                ocsdbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldHubId", HubId),
                    new SqlParameter("@fldHubDesc", drItem["fldHubDesc"]),
                    new SqlParameter("@fldBankCode", drItem["fldBankCode"]),
                    new SqlParameter("@fldCreateUserId", drItem["fldCreateUserId"]),
                    new SqlParameter("@fldCreateTimeStamp", drItem["fldCreateTimeStamp"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"]),
                    new SqlParameter("@fldApprovalStatus", drItem["fldApprovalStatus"])
                });
            }
        }

        public void DeleteAllUserInHub(string hubId)
        {
            string str = " Delete from tblHubUser where fldHubId=@fldHubId";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteAllUserInHub(string hubId, string strBankCode)
        {
            string str = " Delete from tblHubUser where fldHubId=@fldHubId and fldBankCode = @fldBankCode";
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

        public void DeleteAllUserInHubTemp(string hubId)
        {
            string str = " DELETE FROM tblHubUserTemp WHERE fldHubId=@fldHubId";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteHub(string hubId)
        {
            string str = " DELETE FROM tblHubMaster WHERE fldHubId=@fldHubId ";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteHub(string hubId, string strBankCode)
        {
            string str = " Delete from tblHubMaster where fldHubId=@fldHubId and fldBankCode = @fldBankCode";
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) };
                this.dbContext.ExecuteNonQuery(str, sqlParameter);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void DeleteInHubMaster(string HubId)
        {
            string str = " Delete from tblHubMaster where fldHubId=@fldHubId";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", HubId) });
        }

        public void DeleteInHubMasterTemp(string HubId)
        {
            string str = " Delete from tblHubMasterTemp where fldHubId=@fldHubId";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldHubId", HubId) });
        }

        public void DeleteUserNotSelected(string hubId, string userIds)
        {
            string[] strArrays = userIds.Split(new char[] { ',' });
            if (strArrays.Length != 0)
            {
                string str = string.Concat("Delete from tblHubUser where fldHubId=@fldHubId AND fldUserId not in (", DatabaseUtils.getParameterizedStatementFromArray(strArrays, ""), ")");
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

        public void DeleteUserNotSelected(string hubId, string userIds, string strBankCode)
        {
            string[] strArrays = userIds.Split(new char[] { ',' });
            if (strArrays.Length != 0)
            {
                string str = string.Concat("Delete from tblHubUser where fldHubId=@fldHubId and fldBankCode = @fldBankCode AND fldUserId not in (", DatabaseUtils.getParameterizedStatementFromArray(strArrays, ""), ")");
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

        public HubModel GetHub(string hubId)
        {
            string str = "Select * FROM tblHubMaster WHERE fldHubId=@fldHubId";
            HubModel hubModel = new HubModel();
            try
            {
                DataTable recordsAsDataTable = this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@fldHubId", hubId) });
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    DataRow item = recordsAsDataTable.Rows[0];
                    string str1 = item["fldHubId"].ToString().Trim();
                    hubModel.fldHubId = str1.Remove(str1.Length - 4);
                    hubModel.fldHubDesc = item["fldHubDesc"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return hubModel;
        }

        public HubModel GetHub(string hubId, string strBankCode)
        {
            string str = "Select * FROM tblHubMaster WHERE fldHubId=@fldHubId AND fldBankCode = @fldBankCode";
            HubModel hubModel = new HubModel();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] { new SqlParameter("@fldHubId", hubId), new SqlParameter("@fldBankCode", strBankCode) };
                DataTable recordsAsDataTable = this.ocsdbContext.GetRecordsAsDataTable(str, sqlParameter);
                if (recordsAsDataTable.Rows.Count > 0)
                {
                    DataRow item = recordsAsDataTable.Rows[0];
                    string str1 = item["fldHubId"].ToString().Trim();
                    hubModel.fldHubId = str1;
                    hubModel.fldHubDesc = item["fldHubDesc"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return hubModel;
        }

        public string GetMaintenanceChecker()
        {
            string str = "";
            DataTable dataTable = new DataTable();
            dataTable = this.dbContext.GetRecordsAsDataTable("Select fldSystemProfileValue from tblSystemProfile where fldSystemProfileCode = 'MaintenanceHubChecker'", null);
            if (dataTable.Rows.Count > 0)
            {
                str = dataTable.Rows[0]["fldSystemProfileValue"].ToString();
            }
            return str;
        }

        public void InsertUserInHub(string hubId, string userId, string strUpdate, string strUpdateId)
        {
            string str = " INSERT INTO tblHubUser (fldHubId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES (@fldHubId,@fldUserId,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp) ";
            UserModel userModel = new UserModel();
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldUserId", userId),
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

        public void InsertUserInHub(string hubId, string userId, string strUpdate, string strUpdateId, string strBankCode)
        {
            string strQueryInsert = " INSERT INTO tblHubUser (fldHubId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp, fldBankCode) VALUES (@fldHubId,@fldUserId,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId,@fldUpdateTimeStamp,@fldBankCode) ";
            UserModel userModel = new UserModel();
            try
            {
                this.ocsdbContext.ExecuteNonQuery(strQueryInsert, new SqlParameter[] {
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldUserId", userId),
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

        public DataTable ListAllHub(string strBankCode)
        {
            DataTable dataTable = new DataTable();
            string str = "SELECT * FROM tblHubMaster WHERE fldBankCode =@fldBankCode ";
            return this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@fldBankCode", strBankCode) });
        }
        
        public List<UserModel> ListAvailableUserInHub(string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable dtHubUser = new DataTable();
            List<string> userIds = new List<string>();
            string strQuerySelect;

            try
            {
                strQuerySelect = "SELECT fldUserId FROM tblHubUser WHERE fldBankCode = @fldBankCode";
                dtHubUser = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode)
                });

                if (dtHubUser.Rows.Count > 0)
                {
                    foreach (DataRow row in dtHubUser.Rows)
                    {
                        userIds.Add(row["fldUserId"].ToString());
                    }
                }

                if (userIds.Count == 0)
                {
                    userIds.Add("");
                }
                string[] strArray = userIds.ToArray();

                strQuerySelect = string.Concat(
                        "SELECT fldUserId,fldUserAbb,fldUserDesc FROM tblUserMaster WHERE fldBankCode = @fldBankCode ",
                        "AND fldUserId NOT IN (", DatabaseUtils.getParameterizedStatementFromArray(strArray, ""), ")",
                        "AND fldUserId NOT IN (SELECT fldUserId FROM tblHubUserTemp) ",
                        "AND fldApproveStatus = 'Y' "
                    );

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArray, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldBankCode", strBankCode));

                DataTable dataTable = new DataTable();
                dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, sqlParametersFromArray.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        UserModel userModel = new UserModel()
                        {
                            fldUserId = row["fldUserId"].ToString(),
                            fldUserAbb = row["fldUserAbb"].ToString()
                        };
                        userModels.Add(userModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return userModels;
        }

        public List<UserModel> ListSelectedUserInHub(string hubId, string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable dtHubUser = new DataTable();
            List<string> userIds = new List<string>();
            string strQuerySelect;

            try
            {                
                strQuerySelect = "SELECT fldUserId FROM tblHubUser WHERE fldBankCode = @fldBankCode AND fldHubId = @fldHubId ";
                dtHubUser = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldHubId", hubId)
                });

                if (dtHubUser.Rows.Count > 0)
                {
                    foreach (DataRow row in dtHubUser.Rows)
                    {
                        userIds.Add(row["fldUserId"].ToString());
                    }
                }

                if (userIds.Count == 0)
                {
                    userIds.Add("");
                }
                string[] strArray = userIds.ToArray();

                strQuerySelect = string.Concat(
                        "SELECT fldUserId, fldUserAbb FROM tblUserMaster WHERE fldBankCode = @fldBankCode ",
                        "AND fldUserId IN (", DatabaseUtils.getParameterizedStatementFromArray(strArray, ""), ")"
                    );

                List<SqlParameter> sqlParametersFromArray = DatabaseUtils.getSqlParametersFromArray(strArray, "");
                sqlParametersFromArray.Add(new SqlParameter("@fldBankCode", strBankCode));

                DataTable dataTable = new DataTable();
                dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, sqlParametersFromArray.ToArray());

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        UserModel userModel = new UserModel()
                        {
                            fldUserId = row["fldUserId"].ToString(),
                            fldUserAbb = row["fldUserAbb"].ToString()
                        };
                        userModels.Add(userModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return userModels;
        }

        public List<string> ListSelectedUserInHubTemp(string hubId, string strBankCode)
        {
            List<string> userIds = new List<string>();
            DataTable dtHubUser = new DataTable();
            string strQuerySelect;
            try
            {
                strQuerySelect = "SELECT fldUserId FROM tblHubUserTemp WHERE fldHubId = @fldHubId AND fldBankCode = @fldBankCode ";
                                
                DataTable dataTable = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                    new SqlParameter("@fldBankCode", strBankCode),
                    new SqlParameter("@fldHubId", hubId)
                });

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        userIds.Add(row["fldUserId"].ToString());
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return userIds;
        }

        public void UpdateHubMaster(string hubId)
        {
            string strQuerySelect = "SELECT fldHubDesc, fldUpdateUserId, fldUpdateTimeStamp FROM tblHubMasterTemp WHERE fldHubId=@fldHubId ";
            DataTable dtHubTemp = new DataTable();

            dtHubTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldHubId", hubId)
            });

            if (dtHubTemp.Rows.Count > 0)
            {
                DataRow drItem = dtHubTemp.Rows[0];

                string strQueryUpdate = "Update tblHubMaster SET fldHubDesc=@fldHubDesc, fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp WHERE fldHubId=@fldHubId ";

                this.ocsdbContext.ExecuteNonQuery(strQueryUpdate, new SqlParameter[] {
                    new SqlParameter("@fldHubDesc", drItem["fldHubDesc"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"]),
                    new SqlParameter("@fldHubId", hubId) });
            }

            this.UpdateHubUser(hubId);
        }

        public void UpdateHubUser(string hubId)
        {
            List<string> userIds = this.ListSelectedUserInHubTemp(hubId, CurrentUser.Account.BankCode);

            if ((userIds.Count) != 0)
            {
                foreach (string userId in userIds)
                {
                    this.DeleteUserNotSelected(hubId, userId, CurrentUser.Account.BankCode);
                }
                foreach (string userId in userIds)
                {
                    if ((this.CheckUserExistInHub(hubId, userId, CurrentUser.Account.BankCode)))
                    {
                        this.UpdateSelectedUser(hubId, userId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                    }
                    else
                    {
                        this.InsertUserInHub(hubId, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                    }
                }
            }
            else
            {
                this.DeleteAllUserInHub(hubId, CurrentUser.Account.BankCode);
            }
        }

        public void UpdateSelectedUser(string hubId, string UserId, string strUpdate)
        {
            string str = " update tblHubUser set fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldHubId=@fldHubId AND fldUserId=@fldUserId";
            try
            {
                this.dbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldUserId", UserId) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void UpdateSelectedUser(string hubId, string UserId, string strUpdate, string strBankCode)
        {
            string str = " update tblHubUser set fldUpdateUserId=@fldUpdateUserId, fldUpdateTimeStamp=@fldUpdateTimeStamp where fldHubId=@fldHubId AND fldUserId=@fldUserId AND fldBankCode = @fldBankCode";
            try
            {
                this.ocsdbContext.ExecuteNonQuery(str, new SqlParameter[] {
                    new SqlParameter("@fldUpdateUserId", strUpdate),
                    new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now),
                    new SqlParameter("@fldHubId", hubId),
                    new SqlParameter("@fldUserId", UserId),
                    new SqlParameter("@fldBankCode", strBankCode) });
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public List<string> ValidateCreate(FormCollection col, string strBankCode)
        {
            List<string> strs = new List<string>();
            if (col["fldHubId"].Equals(""))
            {
                strs.Add(Locale.HubIdCannotBeEmpty);
            }
            if (this.CheckHubExist(col["fldHubId"], strBankCode))
            {
                strs.Add(Locale.HubAlreadyExists);
            }
            if (col["fldHubId"].Length > 10)
            {
                strs.Add(Locale.HudIdAtLeastChars);
            }
            if (col["fldHubDesc"].Equals(""))
            {
                strs.Add(Locale.HubDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["fldHubId"],strBankCode))
            {
                strs.Add(Locale.HubPendingApproval);
            }
            return strs;
        }

        public List<string> ValidateUpdate(FormCollection col, string strBankCode)
        {
            List<string> strs = new List<string>();
            if (col["fldHubDesc"].Equals(""))
            {
                strs.Add(Locale.HubDescriptionCannotBeEmpty);
            }
            if (this.CheckPendingApproval(col["fldHubId"], strBankCode))
            {
                strs.Add(Locale.HubPendingApproval);
            }
            return strs;
        }

    }
}