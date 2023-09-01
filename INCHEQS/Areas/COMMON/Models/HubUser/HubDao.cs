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

namespace INCHEQS.Areas.COMMON.Models.HubUser
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



        public bool CheckUserExistInHub(string hubCode, string userId, string strBankCode)
        {
            bool flag = false;
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@Action", "SelectwithID"));
            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubUser", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                flag = true;
                }




            return flag;
        }

 
        public bool DeleteUserNotSelected(string hubCode, string userIds, string strBankCode)
        {
            bool blnResult = false;
            string[] strArrays = userIds.Split(new char[] { ',' });
            if (strArrays.Length != 0)
            {
                int intRowAffected;
               
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userIds));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
                intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHubUsernotSelected", sqlParameterNext.ToArray());
                if (intRowAffected > 0)
                {
                    blnResult = true;
                }
                else
                {
                    blnResult = false;
                }
               
            }
            return blnResult;
        }

 
        public void InsertUserInHub(string hubCode, string userId, string strUpdate, string strUpdateId, string strBankCode, string Action)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = strBankCode/*CurrentUser.Account.BankCode*/;

            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdate));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));



            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHubUser", sqlParameterNext.ToArray());

        }



        public void InsertUserInHubTemp(string hubCode, string userId, string strUpdate, string strUpdateId, string strBankCode, string strAction)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = strBankCode/*CurrentUser.Account.BankCode*/;
            

            try
            {
                if (strAction == "Create")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(strUpdate)));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(strUpdateId)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
                    sqlParameterNext.Add(new SqlParameter("@Action", strAction));
                }
               
                else if (strAction == "Delete")
                {
                    sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(strUpdate)));
                    sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(strUpdateId)));
                    sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                    sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
                    sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            
                    sqlParameterNext.Add(new SqlParameter("@Action", strAction));
                }
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHubUserTemp", sqlParameterNext.ToArray());
            }
            catch (Exception exception)
            {
                throw exception;
            }
         
        }


        public List<UserModel> ListAvailableUserInHub(string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            
            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAvailableHubUser", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel userModel = new UserModel()
                    {
                        fldUserId = row["fldUserId"].ToString(),
                        fldUserAbb = row["fldUserAbb"].ToString()
                    };
                    userModels.Add(userModel);
                }
            }


            return userModels;
        }

        public List<UserModel> ListAvailableUserInHubTemp(string strBankCode,string userid)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", userid));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAvailableHubUserTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel userModel = new UserModel()
                    {
                        fldUserId = row["fldUserId"].ToString(),
                        fldUserAbb = row["fldUserAbb"].ToString()
                    };
                    userModels.Add(userModel);
                }
            }


            return userModels;
        }


        public List<UserModel> ListAvailableUserInHubChecker(string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAvailableHubUserChecker", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel userModel = new UserModel()
                    {
                        fldUserId = row["fldUserId"].ToString(),
                        fldUserAbb = row["fldUserAbb"].ToString()
                    };
                    userModels.Add(userModel);
                }
            }


            return userModels;
        }
        

    
        public bool UpdateHubMaster(FormCollection col, string userId)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", col["fldHubCode"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldHubDesc", col["fldHubDesc"].ToString()));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(userId)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHubMaster", sqlParameterNext.ToArray());
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
                        InsertUserInHub(hubId, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "Temp");
                    }
                }
            }
            else
            {
                this.DeleteInHubUserMaster(hubId, "HubUser");
            }
        }


        public bool UpdateSelectedUser(string hubCode, string UserId, string strUpdate, string strBankCode)
        {
          int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
            sqlParameterNext.Add(new SqlParameter("@fldUserId", UserId));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(strUpdate)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHubUser", sqlParameterNext.ToArray());
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
        public List<UserModel> ListSelectedUserInHub(string hubCode, string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@Action", "SelectAll"));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
                resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubUser", sqlParameterNext.ToArray());
                if (resultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in resultTable.Rows)
                    {
                       UserModel userModel = new UserModel()
                    {
                        fldUserId = row["fldUserId"].ToString(),
                        fldUserAbb = row["fldUserAbb"].ToString()
                    };
                    userModels.Add(userModel);
                 }
                }
         

            return userModels;
        }

        public List<UserModel> ListSelectedUserInHubTempChecker(string hubCode, string strBankCode)
        {
            List<UserModel> userModels = new List<UserModel>();
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgSelectedUserInHubUserChecker", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    UserModel userModel = new UserModel()
                    {
                        fldUserId = row["fldUserId"].ToString(),
                        fldUserAbb = row["fldUserAbb"].ToString()
                    };
                    userModels.Add(userModel);
                }
            }


            return userModels;
        }
        public List<string> ListSelectedUserInHubTemp(string hubCode, string strBankCode)
        {
            DataTable resultTable = new DataTable();
            List<string> userIds = new List<string>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubCode));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", strBankCode));
                resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubUserTemp", sqlParameterNext.ToArray());
                if (resultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in resultTable.Rows)
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
        public List<string> ValidateHubUser(FormCollection col, string action, string userId)
        {
            int counter = 0;
            bool counterval = false;
            List<string> userIds = new List<string>();
            List<string> strs = new List<string>();
            HubModel CheckHubExist = CheckHubMasterByID(col["fldHubCode"].Trim(), "HubCode");
            if ((action.Equals("Create")))
            {
                if (col["fldHubCode"].Equals(""))
                {
                    strs.Add(Locale.HubIDcannotbeblank);

                }
                else
                {
                    if ((CheckHubExist != null))
                    {
                        strs.Add(Locale.HubAlreadyExists);
                    }
                    if (col["fldHubCode"].Length > 10)
                    {
                        strs.Add(Locale.HudIdAtLeastChars);
                    }

                }
                if (col["fldHubDesc"].Equals(""))
                {
                    strs.Add(Locale.HubDescriptionCannotBeEmpty);
                }
                if (col["fldHubDesc"].Length > 51)
                {
                    strs.Add("Hub Description must be 50 characters.");
                }
            }
            else if ((action.Equals("Update")))
            {
                if (col["fldHubDesc"].Equals(""))
                {
                    strs.Add(Locale.HubDescriptionCannotBeEmpty);
                }
               
                
               else if ((CheckHubExist != null))
                {
                    if (col["fldHubDesc"].Equals(CheckHubExist.fldHubDesc.ToString()))
                    {
                        counter++;
                    }

                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                    DataTable resultTable = new DataTable();

                    sqlParameterNext.Add(new SqlParameter("@fldHubCode", col["fldHubCode"].Trim()));
                    sqlParameterNext.Add(new SqlParameter("@fldUserId", ""));
                    sqlParameterNext.Add(new SqlParameter("@Action", "SelectAll"));
                    sqlParameterNext.Add(new SqlParameter("@fldBankcode", CheckHubExist.fldBankCode));
                    resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubUser", sqlParameterNext.ToArray());
                    if ((col["selectedUser"]) != null)
                    {
                        userIds = col["selectedUser"].Split(',').ToList();
                        if (userIds.Count == resultTable.Rows.Count)
                        {
                            foreach (string user in userIds)
                            {
                                if (resultTable.Rows.Count > 0)
                                {
                                    foreach (DataRow row in resultTable.Rows)
                                    {
                                        if (row["fldUserId"].ToString().Equals(user))
                                        {
                                            counterval = true;
                                        }
                                        else
                                        {
                                            counterval = false;


                                        }
                                    }
                                }

                            }
                        }
                        else
                        {
                            counterval = false;

                                foreach (string user in userIds)
                                {
                                    if (CheckHubUserExistInTemp(col["fldHubCode"].Trim(), user, action) == true)
                                    {
                                    string userName = "";
                                    //List<UserModel> UserHubTemp = ListAvailableUserInHubTemp(CheckHubExist.fldBankCode, user);

                                    //foreach (var userHubTempList in UserHubTemp)
                                    //{
                                    //     userName = UserHubTemp.fldUserAbb;

                                    //    strs.Add("currently waiting for checker approval.");
                                    //    break;
                                    // }

                                    List<UserModel> UserHubTemp = ListAvailableUserInHubTemp(CheckHubExist.fldBankCode, user);
                                    foreach (var userlistTemp in UserHubTemp)
                                    {
                                        userName = userlistTemp.fldUserAbb;
                                        strs.Add(""+ userName +" currently waiting for checker approval.");
                                        break;
                                    }

                                }
                                }
                                
                        }

                    }
                    else
                    {
                        if (resultTable.Rows.Count == 0)
                        {
                            counterval = true;
                        }
                    }
                    if (counterval == true)
                    {
                        counter++;
                    }
                    if (counter == 2)
                    {
                        strs.Add(Locale.NoChanges);
                    }
                       
                    

                }
            }
                return strs;

        }
        public HubModel CheckHubMasterByID(string HubCode, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            HubModel hub = new HubModel();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubMasterbyId", sqlParameterNext.ToArray());
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
        public HubModel CheckHubMasterTempCheckerByID(string HubCode, string SearchFor)
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
        public bool CheckHubMasterTempByID(string HubCode, string SearchFor)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            HubModel branch = new HubModel();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            sqlParameterNext.Add(new SqlParameter("@SearchFor", SearchFor));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubMasterbyIdTemp", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CreateHubMasterTemp(FormCollection col, string bankcode, string crtUser, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            dynamic bankCode = bankcode/*CurrentUser.Account.BankCode*/;
            if (Action == "Create")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", col["fldHubCode"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldHubDesc", col["fldHubDesc"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
            if (Action == "Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", col["fldHubCode"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldHubDesc", col["fldHubDesc"].ToString()));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", bankcode));
                sqlParameterNext.Add(new SqlParameter("@fldHubDesc", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBankcode", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", ""));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", Convert.ToInt16(crtUser)));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHubMasterTemp", sqlParameterNext.ToArray());
        }
        public void MoveToHubMasterFromTemp(string HubCode, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHubMasterFromTemp", sqlParameterNext.ToArray());
        }

        
        public bool DeleteInHubMasterTemp(string HubCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHubMasterTemp", sqlParameterNext.ToArray());
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
        public bool DeleteInHubUserTemp(string HubCode)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHubUserTemp", sqlParameterNext.ToArray());
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
        public bool DeleteInHubUserMaster(string HubCode, string Action)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHubCode", HubCode));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHubUserMaster", sqlParameterNext.ToArray());
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

        public string GetMenuTitle(string TaskId)
        {
            string MenuTitle = "";
            try
            {
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
                DataTable resultTable = new DataTable();
                resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetTitle", sqlParameterNext.ToArray());

                if (resultTable.Rows.Count > 0)
                {
                    DataRow drItem = resultTable.Rows[0];
                    MenuTitle = drItem["fldMenuTitle"].ToString();
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return MenuTitle;

        }
        public bool CheckHubUserExistInTemp(string hubcode, string userId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            if (Action == "Add")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubcode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubcode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }
            else if (Action == "Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldHubCode", hubcode));
                sqlParameterNext.Add(new SqlParameter("@fldUserId", userId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", ""));
                sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            }

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgHubUserExistInTempbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}