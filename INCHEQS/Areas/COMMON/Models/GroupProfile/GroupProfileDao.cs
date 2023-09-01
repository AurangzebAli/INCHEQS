using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;

namespace INCHEQS.Areas.COMMON.Models.Group
{
    public class GroupProfileDao : IGroupProfileDao
    {
        private readonly ApplicationDbContext dbContext;

        public GroupProfileDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public string GetPageTitle(string TaskId)
        {
            string PageTitle = "";
            try
            {
                DataTable dtHubTemp = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
                dtHubTemp = dbContext.GetRecordsAsDataTableSP("spcgGetPageTitle", sqlParameterNext.ToArray());

                if (dtHubTemp.Rows.Count > 0)
                {
                    DataRow drItem = dtHubTemp.Rows[0];
                    PageTitle = drItem["fldPageTitle"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return PageTitle;

        }

        //DELETE
        public bool DeleteGroupMaster(string groupId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupMaster", sqlParameterNext.ToArray());
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

        public bool DeleteGroupUser(string groupId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupUser", sqlParameterNext.ToArray());
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

        public bool DeleteGroupTask(string groupId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupTask", sqlParameterNext.ToArray());
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

        //GET MENU TITLE OF TASK ID
        public string GetMenuTitle(string TaskId)
        {
            string MenuTitle = "";
            try
            {
                DataTable dtHubTemp = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
                dtHubTemp = dbContext.GetRecordsAsDataTableSP("spcgGetTitle", sqlParameterNext.ToArray());

                if (dtHubTemp.Rows.Count > 0)
                {
                    DataRow drItem = dtHubTemp.Rows[0];
                    MenuTitle = drItem["fldMenuTitle"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return MenuTitle;

        }

        //check if group exist in groupmaster/groupmastertemp table
        public bool CheckGroupMasterExistByID(string groupId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGroupMasterExistbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckGroupMasterTempExistByID(string groupId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGroupMasterTempExistbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //check if group user exist in groupusertemp table
        public bool CheckGroupUserExistInTemp(string groupId, string userId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            if (Action == "Add")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }
            else if (Action=="Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }
            
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGroupUserExistInTempbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckGroupUserExist(string groupId, string userId)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGroupUserExist", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //validate group
        public List<string> ValidateGroup(FormCollection col, string Action, string groupId)
        {
            List<string> strs = new List<string>();

            if (Action.Equals("Create"))
            {
                if (CheckGroupMasterExistByID(col["fldGroupCode"], "Create"))
                {
                    strs.Add(Locale.GroupNameAlreadyTaken);
                }
                if (col["fldGroupCode"].Length > 15)
                {
                    strs.Add(Locale.GroupIdLength);
                }
                if (col["fldGroupCode"].Equals(""))
                {
                    strs.Add(Locale.GroupIDCannotBeEmpty);
                }
                if (col["fldGroupDesc"].Equals(""))
                {
                    strs.Add(Locale.GroupDescCannotBeEmpty);
                }
            }
            else if (Action.Equals("Update"))
            {
                if (col["fldGroupCode"].Equals(""))
                {
                    strs.Add(Locale.GroupIDCannotBeEmpty);
                }
                if (col["fldGroupDesc"].Equals(""))
                {
                    strs.Add(Locale.GroupDescCannotBeEmpty);
                }
            }

            return strs;

        }

        //Insert to GroupMasterTemp Table
        public void CreateGroupMasterTemp(FormCollection col, string strUpdateId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            if (Action == "Create")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", col["fldGroupDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
            
            else if (Action == "Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", col["fldGroupDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "E"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));

            }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupMasterTemp", sqlParameterNext.ToArray());
        }

        public void CreateGroupMasterTempToDelete(string groupId, string strUpdateId, string Action)
            {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

             if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));

            }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupMasterTempToDelete", sqlParameterNext.ToArray());
        }

        public void UpdateGroupMasterTemp(string groupId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuGroupMasterTemp", sqlParameterNext.ToArray());
        }

        public void CreateGroupUserTemp(string groupId, string userId, string strUpdateId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            if (Action == "CreateA")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            if (Action == "CreateD")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));

            }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupUserTemp", sqlParameterNext.ToArray());
        }

        //List of available users
        public List<UserModel> ListAvailableUserInGroup(string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));
            
            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListAvailableUserInGroup", sqlParameterNext.ToArray());
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

        public List<UserModel> ListSelectedUserInGroup(string groupId, string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListSelectedUserInGroup", sqlParameterNext.ToArray());
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

        //insert into GroupMaster Table
        public void CreateGroupMaster(FormCollection col, string strBankCode)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                        
            int num = 0;
            if (col["fldBranchGroup"] != null)
            {
                num = 1;
            }
            try
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", col["fldGroupDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
                sqlParameterNext.Add(new SqlParameter("@fldBranchGroup", (object)num));
                sqlParameterNext.Add(new SqlParameter("@fldSpickCode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupMaster", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }


        public void CreateGroupUser(FormCollection col, string userId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            if (Action.Equals("Create"))
            {            
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            else if (Action.Equals("Update"))
                {
                    sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                    sqlParameterNext.Add(new SqlParameter("@Action", Action));
                }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupUser", sqlParameterNext.ToArray());
        }

        public GroupProfileModel GetGroup(string groupId, string Action)
        {
            GroupProfileModel groupModel = new GroupProfileModel();
            
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgGroupMasterExistbyID", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                DataRow item = recordsAsDataTable.Rows[0];
                string str1 = item["fldGroupCode"].ToString().Trim();
                groupModel.fldGroupId = str1;
                groupModel.fldGroupDesc = item["fldGroupDesc"].ToString();
                //groupModel.fldBranchGroup = item["fldBranchGroup"].ToString();
            }

           
            return groupModel;
        }

        public GroupProfileModel GetGroupChecker(string groupId, string Action)
        {
            GroupProfileModel groupcheckerModel = new GroupProfileModel();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgGroupMasterTempExistbyID", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                DataRow item = recordsAsDataTable.Rows[0];
                string str1 = item["fldGroupCode"].ToString().Trim();
                groupcheckerModel.fldGroupId = str1;
                groupcheckerModel.fldGroupDesc = item["fldGroupDesc"].ToString();
            }


            return groupcheckerModel;
        }

        public bool NoChangesGroup(FormCollection col, string groupId)
        {
            int counter = 0;
            bool strs = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            
            DataTable dtGroupTemp = new DataTable();

            dtGroupTemp = dbContext.GetRecordsAsDataTableSP("spcgNoGroupMasterChanges", sqlParameterNext.ToArray());

            if (dtGroupTemp.Rows.Count > 0)
            {
                DataRow drItem = dtGroupTemp.Rows[0];
                if (col["fldGroupDesc"].Equals(drItem["fldGroupDesc"].ToString()))
                {
                    counter++;
                }
                if (counter == 1)
                {
                    strs = true;
                }
            }
            return strs;
        }

        public bool NoChangesGroupSelectedUser(FormCollection col, string groupId)
        {
            bool exit = false;
            int ctrS = 0;
            List<string> userIds = new List<string>();
            List<string> userAIds = new List<string>();
            string avUser = "";

            //check if there is added selected user from available
            List<UserModel> avUserLists = ListAvailableUserInGroup(CurrentUser.Account.BankCode);
            foreach (var beforeUserlist in avUserLists)
            {
                avUser = beforeUserlist.fldUserId;

                if ((col["selectedUser"]) != null)
                {
                    userAIds = col["selectedUser"].Split(',').ToList();

                    for (int i = 0; i < userAIds.Count; i++)
                    {
                        if (avUser.Equals(userAIds[i]))
                        {
                            return false;
                        }

                    }
                }
            }

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

            String userDb = "";

            DataTable dtGroupUserTemp = new DataTable();
            dtGroupUserTemp = dbContext.GetRecordsAsDataTableSP("spcgNoGroupUserChanges", sqlParameterNext.ToArray());

            if ((dtGroupUserTemp.Rows.Count > 0) && (col["selectedUser"]) != null)
            {
                userIds = col["selectedUser"].Split(',').ToList();

                foreach (DataRow data in dtGroupUserTemp.Rows)
                {
                    userDb = data["fldUserId"].ToString();

                    for (int i = 0; i < userIds.Count; i++)
                    {
                        if (i != ctrS)
                        {
                            return false;
                        }
                        if (userDb.Equals(userIds[ctrS]))
                        {
                            exit = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    ctrS = ctrS + 1;

                }
            }
            else if ((dtGroupUserTemp.Rows.Count > 0) && (col["selectedUser"]) == null)
            {
                exit = false;
            }
            else if ((avUserLists.Count > 0) && (col["selectedUser"]) == null)
            {
                exit = true;
            }
            else if ((dtGroupUserTemp.Rows.Count > 0) && ((col["selectedUser"]) != null) && (dtGroupUserTemp.Rows.Count < col["selectedUser"].Count()))
            {
                exit = false;
            }
            else if ((dtGroupUserTemp.Rows.Count <= 0) && (col["selectedUser"]) == null)
            {
                exit = true;
            }

            return exit;
        }

        //update groupmaster table
        public void UpdateGroupMasterTable(FormCollection col, string strUpdate)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            try
            {
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", col["fldGroupCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldGroupDesc", col["fldGroupDesc"]));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdate));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));

                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuGroupMaster", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public void UpdateGroupUserTable(string groupId, string userId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuGroupUser", sqlParameterNext.ToArray());
            
        }

        //delete group user not selected from tblgroupuser
        public void DeleteGroupUserNotSelected(string groupId, string userIds)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            
                sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldUserIds", userIds));
                try
                {
                    dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupUserNotSelected", sqlParameterNext.ToArray());
                }
                catch (Exception exception)
                {
                    throw exception;
                }
        }

        //delete group user from selected from tblgroupuser
        public void DeleteGroupUserSelected(string groupId, string userIds)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserIds", userIds));
            try
            {
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupUserSelected", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }


        public void MoveToGroupMasterFromTemp(string groupId, string Action)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupMasterFromTemp", sqlParameterNext.ToArray());

        }

        public void MoveToGroupUserFromTemp(string groupId, string userId, string Action)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciGroupUserFromTemp", sqlParameterNext.ToArray());
        }

        public void DeleteGroupUserTemp(string groupId, string userId, string Action) 
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupUserTemp", sqlParameterNext.ToArray());

        }

        public void DeleteGroupMasterTemp(string groupId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupMasterTemp", sqlParameterNext.ToArray());

        }

        public void DeleteGroupUserFromTemp(string groupId, string userId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdGroupUserFromTemp", sqlParameterNext.ToArray());

        }

        public void UpdateGroupUserTemp(string groupId, string userId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuGroupUserTemp", sqlParameterNext.ToArray());

        }

        public List<UserModel> ListSelectedUserInGroupChecker(string groupId, string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldGroupId", groupId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListSelectedUserInGroupChecker", sqlParameterNext.ToArray());
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

        public List<UserModel> ListAvailableUserInGroupChecker(string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListAvailableUserInGroupChecker", sqlParameterNext.ToArray());
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
    }
}