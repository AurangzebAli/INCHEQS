using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;

namespace INCHEQS.Areas.ICS.Models.RationalBranch
{
    public class RationalBranchDao : IRationalBranchDao
    {
        private readonly ApplicationDbContext dbContext;

        public RationalBranchDao(ApplicationDbContext dbContext)
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

        public bool DeleteRationalBranch(string branchId)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdRationalBranch", sqlParameterNext.ToArray());
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

        public bool CheckRationalBranchTempExistByID(string branchId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgRationalBranchTempExistbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //check if selected Rational Branch id exist in rationalBranchTemo table
        public bool CheckSelectedBranchIdExistInTemp(string branchId, string selectedBranchId, string Action)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            if (Action == "Add")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "A"));               
            }
            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "D"));             
            }
            else if (Action=="Check")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", ""));
            }
            else if (Action=="Update")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
                sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "U"));
            }
            
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgSelectedRationalBranchExistInTempbyID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckRationalBranchExist(string branchId, string selectedBranchId)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgRationalBranchExist", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CreateRationalBranchTemp(string branchId, string selectedBranchId, string strUpdateId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<SqlParameter> branchIdSqlParameterNext = new List<SqlParameter>();
            List<SqlParameter> selectedBranchIdSqlParameterNext = new List<SqlParameter>();
            string IBranchId = "";
            string ISelectedBranchId = "";
            DataTable IBranchIdresultTable = new DataTable();
            DataTable ISelectedBranchIdresultTable = new DataTable();

            if (branchId != ""){
                branchIdSqlParameterNext.Add(new SqlParameter("@fldCBranchId", branchId));
                IBranchIdresultTable = dbContext.GetRecordsAsDataTableSP("spcgIslamRationalBranchId", branchIdSqlParameterNext.ToArray());
                if (IBranchIdresultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in IBranchIdresultTable.Rows)
                    {
                        IBranchId = row["fldIBranchId"].ToString();
                    }
                }
                else
                {
                    IBranchId = "";
                }
            }

            if (selectedBranchId != "")
            {
                selectedBranchIdSqlParameterNext.Add(new SqlParameter("@fldCBranchId", selectedBranchId));
                ISelectedBranchIdresultTable = dbContext.GetRecordsAsDataTableSP("spcgIslamRationalBranchId", selectedBranchIdSqlParameterNext.ToArray());

                if (ISelectedBranchIdresultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in ISelectedBranchIdresultTable.Rows)
                    {
                        ISelectedBranchId = row["fldIBranchId"].ToString();
                    }
                }
                else
                {
                    ISelectedBranchId = "";
                }
            }

            if (Action == "CreateA")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldICollapseBranchId", ISelectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            if (Action == "CreateD")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldICollapseBranchId", ISelectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            else if (Action == "Delete")
            {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldICollapseBranchId", ISelectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", strUpdateId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "D"));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));

            }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciRationalBranchTemp", sqlParameterNext.ToArray());
        }

        //List of Active branch
        public List<RationalBranchModel> ListActiveRationalBranch(string strBankCode)
        {
            //List<UserModel> userModels = new List<UserModel>();
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));
            
            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListActiveRationalBranch", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        //UserModel userModel = new UserModel()
                        RationalBranchModel rationalBranchModel = new RationalBranchModel()
                        {
                            fldCBranchId = row["fldCBranchId"].ToString(),
                            fldIBranchId = row["fldIBranchId"].ToString(),
                            fldCBranchDesc = row["fldCBranchDesc"].ToString()
                        };
                        RationalBranchModels.Add(rationalBranchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }

        //List of available branch for merge
        public List<RationalBranchModel> ListAvailableRationalBranch(string strBankCode)
        {
            //List<UserModel> userModels = new List<UserModel>();
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            //sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListAvailableRationalBranch", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        //UserModel userModel = new UserModel()
                        RationalBranchModel rationalBranchModel = new RationalBranchModel()
                        {
                            fldCBranchId = row["fldCBranchId"].ToString(),
                            fldIBranchId = row["fldIBranchId"].ToString(),
                            fldCBranchDesc = row["fldCBranchDesc"].ToString()
                        };
                        RationalBranchModels.Add(rationalBranchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }

        public List<RationalBranchModel> ListSelectedRationalBranch(string branchId, string IbranchId)
        {
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IbranchId));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListSelectedRationalBranch", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                            //UserModel userModel = new UserModel()
                            RationalBranchModel rationalBranchModel = new RationalBranchModel()
                            {
                                fldCBranchId = row["fldCCollapseBranchId"].ToString(),
                                fldIBranchId = row["fldICollapseBranchId"].ToString(),
                                fldCBranchDesc = row["fldCBranchDesc"].ToString()
                            };
                            RationalBranchModels.Add(rationalBranchModel);
                        } 
                   
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }


        public void CreateRationalBranch(FormCollection col, string selectedBranchId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<SqlParameter> branchIdSqlParameterNext = new List<SqlParameter>();
            List<SqlParameter> selectedBranchIdSqlParameterNext = new List<SqlParameter>();
            string IBranchId = "";
            string ISelectedBranchId = "";
            DataTable IBranchIdresultTable = new DataTable();
            DataTable ISelectedBranchIdresultTable = new DataTable();
            if (col["fldInternalBranchCode"] != "")
            {
                branchIdSqlParameterNext.Add(new SqlParameter("@fldCBranchId", col["fldInternalBranchCode"]));
                IBranchIdresultTable = dbContext.GetRecordsAsDataTableSP("spcgIslamRationalBranchId", branchIdSqlParameterNext.ToArray());
                if (IBranchIdresultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in IBranchIdresultTable.Rows)
                    {
                        IBranchId = row["fldIBranchId"].ToString();
                    }
                }
                else
                {
                    IBranchId = "";
                }
            }

            if (selectedBranchId != "")
            {
                selectedBranchIdSqlParameterNext.Add(new SqlParameter("@fldCBranchId", selectedBranchId));
                ISelectedBranchIdresultTable = dbContext.GetRecordsAsDataTableSP("spcgIslamRationalBranchId", selectedBranchIdSqlParameterNext.ToArray());

                if (ISelectedBranchIdresultTable.Rows.Count > 0)
                {
                    foreach (DataRow row in ISelectedBranchIdresultTable.Rows)
                    {
                        ISelectedBranchId = row["fldIBranchId"].ToString();
                    }
                }
                else
                {
                    ISelectedBranchId = "";
                }
            }
            if (Action.Equals("Create"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", col["fldInternalBranchCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldICollapseBranchId", ISelectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            else if (Action.Equals("Update"))
                {
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", col["fldInternalBranchCode"]));
                sqlParameterNext.Add(new SqlParameter("@fldIMergeBranchId", IBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldICollapseBranchId", ISelectedBranchId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", CurrentUser.Account.BankCode));
                sqlParameterNext.Add(new SqlParameter("@Action", Action));
            }

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciRationalBranch", sqlParameterNext.ToArray());
        }

        public RationalBranchModel GetBranch(string branchId, string Action)
        {
            RationalBranchModel rationalBranchModel = new RationalBranchModel();
            
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            //sqlParameterNext.Add(new SqlParameter("@Action", Action));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgMergeBankBranchDataById", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                DataRow item = recordsAsDataTable.Rows[0];
                string str1 = item["fldCBranchId"].ToString().Trim();
                rationalBranchModel.fldCBranchId = str1;
                rationalBranchModel.fldIBranchId = item["fldIBranchId"].ToString();
                rationalBranchModel.fldCBranchDesc = item["fldCBranchDesc"].ToString();
            }

           
            return rationalBranchModel;
        }


        public bool NoChangesBranch(FormCollection col, string id)
        {
            int counter = 0;
            bool strs = false;
            List<string> userIds = new List<string>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            string branchId = col["fldCBranchId"].Trim();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            DataTable dtRationalBranchTemp = new DataTable();
            dtRationalBranchTemp = dbContext.GetRecordsAsDataTableSP("spcgNoRationalBranchChanges", sqlParameterNext.ToArray());



            if ((dtRationalBranchTemp.Rows.Count > 0) && (col["selectedUser"]) != null)
            {
                userIds = col["selectedUser"].Split(',').ToList();

                foreach (DataRow data in dtRationalBranchTemp.Rows)
                {

                    for (int i = 0; i < userIds.Count; i++)
                    {
                        if (data[3].Equals(userIds[i]))
                        {
                            counter++;
                        }
                    }
                }
                if (dtRationalBranchTemp.Rows.Count == counter) {
                    return true;
                }
            }
            return strs;
        }

        public bool NoChangesBranchSelected(FormCollection col, string id)
        {
            bool exit = false;
            int ctrS = 0;
            List<string> userIds = new List<string>();
            List<string> userAIds = new List<string>();
            string avUser = "";

            //check if there is added selected user from available
            //get availble branch (avBranchLists) to compare with original selected branch (selectedUser)
            List<RationalBranchModel> avUserLists = ListAvailableRationalBranch(CurrentUser.Account.BankCode);
            foreach (var beforeUserlist in avUserLists)
            {
                avUser = beforeUserlist.fldCBranchId;

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
            string branchId = col["fldCBranchId"].Trim();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));

            DataTable dtRationalBranchTemp = new DataTable();
            dtRationalBranchTemp = dbContext.GetRecordsAsDataTableSP("spcgNoRationalBranchChanges", sqlParameterNext.ToArray());

            if ((dtRationalBranchTemp.Rows.Count > 0) && (col["selectedUser"]) != null)
            {
                userIds = col["selectedUser"].Split(',').ToList();

                foreach (DataRow data in dtRationalBranchTemp.Rows)
                {

                    for (int i = 0; i < userIds.Count; i++)
                    {
                        if (i != ctrS)
                        {
                            return false;
                        }
                    }
                    ctrS = ctrS + 1;

                }
            }
            else if ((dtRationalBranchTemp.Rows.Count > 0) && (col["selectedUser"]) == null)
            {
                exit = false;
            }
            else if ((avUserLists.Count > 0) && (col["selectedUser"]) == null)
            {
                //exit = true;
                exit = false;
            }
            else if ((dtRationalBranchTemp.Rows.Count > 0) && ((col["selectedUser"]) != null) && (dtRationalBranchTemp.Rows.Count < col["selectedUser"].Count()))
            {
                exit = false;
            }
            else if ((dtRationalBranchTemp.Rows.Count <= 0) && (col["selectedUser"]) == null)
            {
                exit = true;
            }

            return exit;
        }


        public void UpdateRationalBranchTable(string branchId, string selectedBranchId)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuRationalBranch", sqlParameterNext.ToArray());
            
        }

        public void DeleteRationalBranchNotSelected(string branchId, string selectedBranchId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            
                sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));                
                sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBranchId));
                try
                {
                    dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdRationalBranchNotSelected", sqlParameterNext.ToArray());
                }
                catch (Exception exception)
                {
                    throw exception;
                }
        }

        public void MoveRationalBranchFromTemp(string branchId,string Action)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciMoveRationalBranchFromTemp", sqlParameterNext.ToArray());
        }

        public void DeleteRationalBranchTemp(string branchId, string Action) 
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdRationalBranchTemp", sqlParameterNext.ToArray());

        }

        public void MoveRationalBranchFromTempRowByRow(string branchId, string selectedBrancdId, string Action)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBrancdId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciMoveRationalBranchFromTempRowByRow", sqlParameterNext.ToArray());
        }

        public void DeleteRationalBranchTempRowByRow(string branchId, string selectedBrancdId, string Action)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldCCollapseBranchId", selectedBrancdId));
            sqlParameterNext.Add(new SqlParameter("@Action", Action));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdRationalBranchTempRowByRow", sqlParameterNext.ToArray());

        }


        public void DeleteRationalBranchFromTemp(string branchId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdRationalBranchFromTemp", sqlParameterNext.ToArray());

        }

        public void UpdateRationalbranchTemp(string branchId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));
            sqlParameterNext.Add(new SqlParameter("@fldApprovalStatus", "Y"));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuRationalbranchTemp", sqlParameterNext.ToArray());

        }

        public List<RationalBranchModel> ListSelectedRationalBranchChecker(string branchId)
        {
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("[spcgListSelectedRationalBranchChecker]", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        RationalBranchModel rationalBranchModel = new RationalBranchModel()
                        {
                            fldCBranchId = row["fldCCollapseBranchId"].ToString(),
                            fldIBranchId = row["fldICollapseBranchId"].ToString(),
                            fldCBranchDesc = row["fldCBranchDesc"].ToString()
                        };
                        RationalBranchModels.Add(rationalBranchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }

        public List<RationalBranchModel> ListSelectedRationalBranchCheckerWithAllTempRecord(string branchId)
        {
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldCMergeBranchId", branchId));

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("[spcgListSelectedRationalBranchCheckerWithAllTempRecord]", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        RationalBranchModel rationalBranchModel = new RationalBranchModel()
                        {
                            fldCBranchId = row["fldCCollapseBranchId"].ToString(),
                            fldIBranchId = row["fldICollapseBranchId"].ToString(),
                            fldCBranchDesc = row["fldCBranchDesc"].ToString()
                        };
                        RationalBranchModels.Add(rationalBranchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }
        public List<RationalBranchModel> ListAvailableRationalBranchChecker(string strBankCode)
        {
            List<RationalBranchModel> RationalBranchModels = new List<RationalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            try
            {
                DataTable dataTable = new DataTable();
                dataTable = dbContext.GetRecordsAsDataTableSP("spcgListAvailableRationalBranchChecker", sqlParameterNext.ToArray());
                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        RationalBranchModel rationalBranchModel = new RationalBranchModel()
                        {
                            fldCBranchId = row["fldCBranchId"].ToString(),
                            fldIBranchId = row["fldIBranchId"].ToString(),
                            fldCBranchDesc = row["fldCBranchDesc"].ToString()
                        };
                        RationalBranchModels.Add(rationalBranchModel);
                    }
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return RationalBranchModels;
        }

        public void CheckRationalBranchSeletedBranchHaveChildOrNot(string branchId, string selectedBrancdId) {

            List<SqlParameter> selectedBranchIdSqlParameterNext = new List<SqlParameter>();
            string IBranchId = "";
            DataTable SelectedBranchIdresultTable = new DataTable();
            DataTable IBranchIdresultTable = new DataTable();
            List<SqlParameter> branchIdSqlParameterNext = new List<SqlParameter>();

            branchIdSqlParameterNext.Add(new SqlParameter("@fldCBranchId", branchId));
            IBranchIdresultTable = dbContext.GetRecordsAsDataTableSP("spcgIslamRationalBranchId", branchIdSqlParameterNext.ToArray());
            if (IBranchIdresultTable.Rows.Count > 0)
            {
                foreach (DataRow row in IBranchIdresultTable.Rows)
                {
                    IBranchId = row["fldIBranchId"].ToString();
                }
            }
            else
            {
                IBranchId = "";
            }

            if (selectedBrancdId != "")
            {
                
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                sqlParameterNext.Add(new SqlParameter("@selectedBranch", selectedBrancdId));
                sqlParameterNext.Add(new SqlParameter("@branchId", branchId));
                sqlParameterNext.Add(new SqlParameter("@IBranchId", IBranchId));

                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCheckRationalBranchSeletedBranchHaveChildOrNot", sqlParameterNext.ToArray());
            }
        }
    }
}