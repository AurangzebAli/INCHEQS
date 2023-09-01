using INCHEQS.Common;
//using INCHEQS.Common.Resources;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.PCHC.Resouce;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using INCHEQS.Resources;

namespace INCHEQS.Areas.COMMON.Models.InternalBranchKBZ
{
    public class InternalBranchKBZDao : IInternalBranchKBZDao
    {
        private readonly ApplicationDbContext dbContext;

        public InternalBranchKBZDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddToMapBranchTempToDelete(string conBranchCode)
        {
            string str = "Insert into tblMapBranchTemp (fldAccountID, fldBranchCode,fldIBranchCode, fldBranchDesc, fldStateCode, fldStateDesc,fldSpickCode, fldConBranchCode, fldIsBranchCode2, fldBranchAbb, fldEmailAddress, Address1, Address2, Address3)  Select fldAccountID, fldBranchCode,fldIBranchCode, fldBranchDesc, fldStateCode, fldStateDesc,fldSpickCode, fldConBranchCode, fldIsBranchCode2, fldBranchAbb, fldEmailAddress, Address1, Address2, Address3 from tblMapBranch WHERE fldConBranchCode=@fldConBranchCode  Update tblMapBranchTemp SET fldApproveStatus=@fldApproveStatus WHERE fldConBranchCode=@fldConBranchCode ";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldConBranchCode", conBranchCode), new SqlParameter("@fldApproveStatus", "D") });
        }

        public bool CheckIDexist(string conBranchCode)
        {
            bool flag;
            DataTable dataTable = new DataTable();
            string str = "select *from tblMapBranch where fldconBranchCode=@fldconBranchCode";
            flag = (this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@fldconBranchCode", conBranchCode) }).Rows.Count <= 0 ? false : true);
            return flag;
        }

        public bool CheckIDexistInMapBranchTemp(string conBranchCode)
        {
            string str = "select *from tblMapBranchTemp where fldconBranchCode=@fldconBranchCode";
            return this.dbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldconBranchCode", conBranchCode) });
        }

        public void CreateInInternalBranch(string conBranchCode)
        {
            string str = "INSERT INTO tblMapBranch (fldAccountID, fldBranchCode,fldIBranchCode, fldBranchDesc,fldStateCode, fldStateDesc,fldSpickCode, fldConBranchCode, fldIsBranchCode2, fldBranchAbb, fldEmailAddress, Address1, Address2, Address3,fldbankcode,fldentitycode,fldSOL) select fldAccountID, fldBranchCode,fldIBranchCode, fldBranchDesc, fldStateCode, fldStateDesc,fldSpickCode, fldConBranchCode, fldIsBranchCode2, fldBranchAbb, fldEmailAddress, Address1, Address2, Address3,fldbankcode,fldentitycode,fldSOL from tblMapBranchTemp where fldConBranchCode=@fldConBranchCode";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldConBranchCode", conBranchCode) });
        }

        public void DeleteInInternalBranch(string conBranchCode)
        {
            string str = "Delete from tblMapBranch WHERE fldconBranchCode=@fldconBranchCode";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldconBranchCode", conBranchCode) });
        }

        public void DeleteInInternalBranchTemp(string conBranchCode)
        {
            string str = "Delete from tblMapBranchTemp WHERE fldConBranchCode=@fldConBranchCode";
            this.dbContext.ExecuteNonQuery(str, new SqlParameter[] { new SqlParameter("@fldConBranchCode", conBranchCode) });
        }

        public InternalBranchKBZModel FindInternalBranchCode(string conBranchCode)
        {
            DataTable dataTable = new DataTable();
            InternalBranchKBZModel internalBranchModel = new InternalBranchKBZModel();
            string str = "SELECT a.fldStateCode,a.fldaccountid,a.fldaccountNumber,a.fldBranchCode,a.fldIBranchCode,a.fldBranchDesc, a.fldSpickCode,b.fldSpickDesc, c.fldStateCode, c.fldStateDesc, a.fldEmailAddress,a.Address1,a.Address2, a.Address3, a.fldConBranchCode, a.fldIsBranchCode2,a.fldBranchAbb,a.fldSOL FROM tblMapBranch a, tblSpickMaster b, tblStateMaster c WHERE a.fldConBranchCode =@fldConBranchCode AND a.fldStateCode = c.fldStateCode";
            dataTable = this.dbContext.GetRecordsAsDataTable(str, new SqlParameter[] { new SqlParameter("@fldConBranchCode", conBranchCode) });
            if (dataTable.Rows.Count > 0)
            {
                DataRow item = dataTable.Rows[0];

            }
            return internalBranchModel;
        }



        public List<string> ValidateEdit(FormCollection col)
        {
            List<string> strs = new List<string>();
            if (col["txtBranchDesc"].Trim().Equals(""))
            {
                strs.Add(Locale.Descriptioncannotbeblank);
            }
            return strs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------

        public DataTable GetInternalBranchData(string id) //done
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            return this.dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterDataById", sqlParameterNext.ToArray());

        }

        public DataTable GetInternalBranchDataTemp(string id)
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            return this.dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempById", sqlParameterNext.ToArray());

        }

        public bool CreateInternalBranch(FormCollection col)
        {
            string activ = "N";
            string fldSubcenter = "N";
            string clearingBranchId = col["ClearingBranchId"];

            if (col["chkActive"] != null)
            {
                activ = "Y";
            }

            if (col["chkSelfClearing"] != null)
            {
                //selfClearing = "Y";
                clearingBranchId = col["branchId"];
            }

            if (col["fldSubcenter"] != null)
                {
                fldSubcenter = "Y";
            }

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldlocationcode", col["LocationCode"].Substring(0, 1)));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["fldBankCode"].Substring(0, 3)));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["BranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["BranchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", col["fldBankZoneCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldSubcenter", fldSubcenter));

            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["InternalBranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["Address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["Address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["Address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));

            sqlParameterNext.Add(new SqlParameter("@fldClearingBranchId", clearingBranchId));
            sqlParameterNext.Add(new SqlParameter("@fldActive", activ));

            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciInternalBranchMaster", sqlParameterNext.ToArray());

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

        public bool CreateInternalBranchTemp(FormCollection col,string action) //Done
        {
            string activ = "N";
            string fldSubcenter = "N";
            string clearingBranchId = col["ClearingBranchId"];

            if (col["chkActive"] != null)
            {
                activ = "Y";
            }

            if (col["chkSelfClearing"] != null)
            {
                //selfClearing = "Y";
                clearingBranchId = col["branchId"];
            }

            if (col["fldSubcenter"] != null)
            {
                fldSubcenter = "Y";
            }

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldlocationcode", col["LocationCode"].Substring(0, 1)));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", col["fldBankCode"].Substring(0, 3)));
            sqlParameterNext.Add(new SqlParameter("@fldBranchCode", col["BranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["BranchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", col["fldBankZoneCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldSubcenter", fldSubcenter));

            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["InternalBranchCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["Address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["Address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["Address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));

            sqlParameterNext.Add(new SqlParameter("@fldClearingBranchId", clearingBranchId));
            sqlParameterNext.Add(new SqlParameter("@fldActive", activ));

            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            if (action.Equals("create"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
            }

            if (action.Equals("update"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Parse(col["createTimeStamp"])));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", col["CreateUserId"]));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
            }


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciInternalBranchMasterTemp", sqlParameterNext.ToArray());

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

        public List<string> ValidateCreate(FormCollection col)
        {
            List<string> err = new List<string>();

            if (col["branchId"].Equals(""))
            {
                err.Add("Branch ID cannot be empty");
            }
            else
            {

            bool IsInternalBranchMasterExist = CheckInternalBranchById(col["branchId"]);
            if (IsInternalBranchMasterExist == true)
            {
                err.Add(Locale.InternalBranchCodeAlreadyExist);
            }

            bool IsInternalBranchMasterTempExist = CheckInternalBranchTempById(col["branchId"]);
            if (IsInternalBranchMasterTempExist == true)
            {
                err.Add(Locale.InternalBranchAlreadyCreatedinApproveChecker);
                
            }

            if (!col["fldEmailAddress"].Equals(""))
            {
                    if (!(new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")).IsMatch(col["fldEmailAddress"]))
                    {
                        err.Add(Locale.EmailFormat);
                    }
            }
            

            if (!(new Regex("^[0-9]+$")).IsMatch(col["branchCode"]))
            {
                err.Add(Locale.BranchCodeNumber);
            }
            if (col["fldBankCode"].Equals(""))
            {
                err.Add(Locale.BankCodecannotbeblank);
            }
                if (col["branchId"].Substring(1,3) != CurrentUser.Account.BankCode)
                {
                    err.Add(Locale.OwnBankAllowed);
                }

            /*if (col["branchCode"].Length != 4)
            {
                err.Add("Branch Code must be equal to 4");
            }*/

            if (col["branchDesc"].Equals(""))
            {
                    err.Add(Locale.BankDesccannotbeblank);
            }
            if (col["address1"].Equals(""))
            {
                    err.Add(Locale.Address1cannotbeblank);
            }
            if (col["chkSelfClearing"] == null)
            {
                if (col["clearingBranchId"].Equals(""))
                {
                        err.Add(Locale.clearingBranchIdcannotbeblank);
                }
            }

            if (col["fldBankZoneCode"].Equals(""))
            {
                err.Add(Locale.BankZoneCodecannotbeblank);
            }

            if (!col["fldPostCode"].Equals(""))
            {
                if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldPostCode"]))
                {
                    err.Add(Locale.Postcodecannotbespecialchars);
                }
            }

                
            }

            return err;
        }

        public bool CheckInternalBranchById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckInternalBranchTempById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public List<string> ValidateUpdate(FormCollection col)
        {
            List<string> err = new List<string>();

            if (col["branchDesc"].Equals(""))
            {
                err.Add(Locale.BankDesccannotbeblank);
            }

            if (col["address1"].Equals(""))
            {
                err.Add(Locale.Address1cannotbeblank);
        }

            if (col["chkSelfClearing"] == null)
            {
                if (col["clearingBranchId"].Equals(""))
        {
                    err.Add(Locale.clearingBranchIdcannotbeblank);
                }
            }

            if (col["fldBankZoneCode"].Equals(""))
            {
                err.Add(Locale.BankZoneCodecannotbeblank);
            }

            if (!col["fldEmailAddress"].Equals(""))
            {
                if (!(new Regex(@"\A(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")).IsMatch(col["fldEmailAddress"]))
            {
                    err.Add(Locale.EmailFormat);
                }
            }

            if (!col["fldPostCode"].Equals(""))
            {
                if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldPostCode"]))
                {
                    err.Add(Locale.Postcodecannotbespecialchars);
                }
            }
            return err;
        }

        public bool UpdateInternalBranch(FormCollection col)
        {
            string activ = "N";
            string clearingBranchId = col["clearingBranchId"];
            string fldSubcenter = "N";

            if (col["chkActive"] != null)
            {
                activ = "Y";
            }

            if (col["chkSelfClearing"] != null)
            {
                //selfClearing = "Y";
                clearingBranchId = col["branchId"];
            }

            if (col["fldSubcenter"] != null)
            {
                fldSubcenter = "Y";
            }

          

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["BranchId"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchDesc", col["BranchDesc"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankZoneCode", col["fldBankZoneCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchCode", col["internalBranchCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));

            sqlParameterNext.Add(new SqlParameter("@fldClearingBranchId", clearingBranchId));
            sqlParameterNext.Add(new SqlParameter("@fldSubcenter", fldSubcenter));

            sqlParameterNext.Add(new SqlParameter("@fldActive", activ));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuInternalBranchMaster", sqlParameterNext.ToArray());

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

        public bool DeleteInternalBranch(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdInternalBranchMaster", sqlParameterNext.ToArray());

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

        public bool CreateInternalBranchTempToDelete(string id) //done
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciInternalBranchMasterTempToDelete", sqlParameterNext.ToArray());

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

        public bool MoveToInternalBranchFromTemp(string id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciInternalBranchMasterFromTemp", sqlParameterNext.ToArray());

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

        public bool UpdateInternalBranchById(string id) //Done
        {
            //ReturnCodeModel bankCode = new ReturnCodeModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuInternalBranchMasterById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool DeleteInternalBranchTemp(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdInternalBranchMasterTemp", sqlParameterNext.ToArray());

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

        public List<InternalBranchKBZModel> ListInternalBranch(string bankcode) //Done
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchKBZModel> branchList = new List<InternalBranchKBZModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchesForInternalBranch", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchKBZModel branch = new InternalBranchKBZModel();
                    branch.branchId = row["fldbranchId"].ToString();
                    branch.branchDesc = row["fldBranchDesc"].ToString();
                    branchList.Add(branch);
                }
            }
            return branchList;
        }

        public List<InternalBranchKBZModel> ListCountry()
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchKBZModel> countryList = new List<InternalBranchKBZModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCountryForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchKBZModel country = new InternalBranchKBZModel();
                    country.countryCode = row["fldCountryCode"].ToString();
                    country.countryDesc = row["fldCountryDesc"].ToString();
                    countryList.Add(country);
                }
            }
            return countryList;
        }
        public List<InternalBranchKBZModel> ListBankZone()
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchKBZModel> BankZoneList = new List<InternalBranchKBZModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankZoneCodeList", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchKBZModel BankZone = new InternalBranchKBZModel();
                    BankZone.BankZoneCode = row["fldBankZoneCode"].ToString();
                    BankZoneList.Add(BankZone);
                }
            }
            return BankZoneList;
        }
    }
}