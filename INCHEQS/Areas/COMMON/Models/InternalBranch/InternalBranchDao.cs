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

namespace INCHEQS.Areas.COMMON.Models.InternalBranch
{
    public class InternalBranchDao : IInternalBranchDao
    {
        private readonly ApplicationDbContext dbContext;

        public InternalBranchDao(ApplicationDbContext dbContext)
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

        public InternalBranchModel FindInternalBranchCode(string conBranchCode)
        {
            DataTable dataTable = new DataTable();
            InternalBranchModel internalBranchModel = new InternalBranchModel();
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

        public string GetInternalBranchID() //done
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@TableName", "tblInternalBranchMaster"));
            sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
            sqlParameterNext[1].Direction = ParameterDirection.Output;
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgNextSeqNo", sqlParameterNext.ToArray());
            string iNextNo = sqlParameterNext[1].Value.ToString();

            return iNextNo;
        }

        public DataTable GetInternalBranchData(string id) //done
        {
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));

            return this.dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterById", sqlParameterNext.ToArray());

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
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string branchIdIslamic, branchDescIslamic, internalBranchCodeIslamic;
            string branchIdConv, branchDescConv, internalBranchCodeConv;
            string active;
            string clearingBranchIdConv = col["ClearingBranchIdConv"];
            string clearingBranchIdIslamic = col["ClearingBranchIdIslamic"];
            string checkConv = col["checkConv"];
            string checkIslamic = col["checkIslamic"];
            string internalBranchID = col["internalBranchID"];
            string checkActive = col["fldActive"];

            if (checkActive == null)
            {
                active = "N";
            }
            else
            {
                active = "Y";
            }

            if (checkIslamic != null)
            {
                branchIdIslamic = col["branchIdIslamic"];
                branchDescIslamic = col["branchDescIslamic"];
                internalBranchCodeIslamic = col["internalBranchCodeIslamic"];
                if (col["chkSelfClearingIslamic"] != null)
                {
                    clearingBranchIdIslamic = col["branchIdIslamic"];
                }
            }
            else
            {
                branchIdIslamic = "";
                branchDescIslamic = "";
                internalBranchCodeIslamic = "";
                clearingBranchIdIslamic = "";
            }

            if (checkConv != null)
            {
                branchIdConv = col["branchIdConv"];
                branchDescConv = col["branchDescConv"];
                internalBranchCodeConv = col["internalBranchCodeConv"];

                if (col["chkSelfClearingConv"] != null)
                {
                    clearingBranchIdConv = col["branchIdConv"];
                }
            }
            else
            {
                branchIdConv = "";
                branchDescConv = "";
                internalBranchCodeConv = "";
                clearingBranchIdConv = "";
            }

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", internalBranchID));

            //Islamic
            sqlParameterNext.Add(new SqlParameter("@fldIBranchId", branchIdIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIBranchDesc", branchDescIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIInternalBranchCode", internalBranchCodeIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIClearingBranchId", clearingBranchIdIslamic));

            //Conv
            sqlParameterNext.Add(new SqlParameter("@fldCBranchId", branchIdConv));
            sqlParameterNext.Add(new SqlParameter("@fldCBranchDesc", branchDescConv));
            sqlParameterNext.Add(new SqlParameter("@fldCInternalBranchCode", internalBranchCodeConv));
            sqlParameterNext.Add(new SqlParameter("@fldCClearingBranchId", clearingBranchIdConv));
            //Common 
            /*Ali
            
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["Address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["Address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["Address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            
             */

            sqlParameterNext.Add(new SqlParameter("@fldActive", active));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));

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

        public bool CreateInternalBranchTemp(FormCollection col, string action) //Done
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string branchIdIslamic, branchDescIslamic, internalBranchCodeIslamic;
            string branchIdConv, branchDescConv, internalBranchCodeConv;
            string active;

            string clearingBranchIdConv = col["ClearingBranchIdConv"];
            string clearingBranchIdIslamic = col["ClearingBranchIdIslamic"];
            string checkConv = col["checkConv"];
            string checkIslamic = col["checkIslamic"];
            string internalBranchID = col["internalBranchID"];
            string checkActive = col["fldActive"];

            if (checkActive == null)
            {
                active = "N";
            }
            else
            {
                active = "Y";
            }
            if (checkIslamic != null)
            {
                branchIdIslamic = col["branchIdIslamic"];
                branchDescIslamic = col["branchDescIslamic"];
                internalBranchCodeIslamic = col["internalBranchCodeIslamic"];
                if (col["chkSelfClearingIslamic"] != null)
                {
                    clearingBranchIdIslamic = col["branchIdIslamic"];
                }
            }
            else {
                branchIdIslamic = "";
                branchDescIslamic = "";
                internalBranchCodeIslamic = "";
                clearingBranchIdIslamic = "";
            }

            if (checkConv != null)
            {
                branchIdConv = col["branchIdConv"];
                branchDescConv = col["branchDescConv"];
                internalBranchCodeConv = col["internalBranchCodeConv"];

                if (col["chkSelfClearingConv"] != null)
                {
                    clearingBranchIdConv = col["branchIdConv"];
                }
            }
            else
            {
                branchIdConv = "";
                branchDescConv = "";
                internalBranchCodeConv = "";
                clearingBranchIdConv = "";
            }

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", int.Parse(internalBranchID)));

            //Islamic
            sqlParameterNext.Add(new SqlParameter("@fldIBranchId", branchIdIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIBranchDesc", branchDescIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIInternalBranchCode", internalBranchCodeIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIClearingBranchId", clearingBranchIdIslamic));

            //Conv
            sqlParameterNext.Add(new SqlParameter("@fldCBranchId", branchIdConv));
            sqlParameterNext.Add(new SqlParameter("@fldCBranchDesc", branchDescConv));
            sqlParameterNext.Add(new SqlParameter("@fldCInternalBranchCode", internalBranchCodeConv));
            sqlParameterNext.Add(new SqlParameter("@fldCClearingBranchId", clearingBranchIdConv));

            //Common
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["Address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["Address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["Address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));
            sqlParameterNext.Add(new SqlParameter("@fldActive", active));
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
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", col["createUserId"]));
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

            if ((col["checkConv"]!=null) || (col["checkIslamic"]!=null))
            {

                if ((col["checkConv"] != null))
                {
                    if (col["branchIdConv"].Equals(""))
                    {
                        err.Add(Locale.ConventionalBranchIdcannotbeblank);
                    }
                    else
                    {
                        bool IsBranchMasterConvExist = CheckBranchMasterBankTypeById(col["branchIdConv"], "C");
                            if (IsBranchMasterConvExist == true)
                        {
                            bool IsConvBank = CheckBankType(col["branchIdconv"].Substring(0, 3), "C");
                            if (IsConvBank == true)
                            {
                                bool IsAffinBank = CheckRelatedBank(col["branchIdConv"].Substring(0, 3));

                                if (IsAffinBank == true)
                                {
                                    bool IsInternalBranchMasterConvExist = CheckInternalBranchByBranchId(col["branchIdConv"]);
                                    if (IsInternalBranchMasterConvExist == true)
                                    {
                                        err.Add(Locale.ConventionalBranchIdAlreadyExist);
                                    }
                                    else
                                    {
                                        bool IsInternalBranchMasterConvTempExist = CheckInternalBranchTempByBranchId(col["branchIdConv"]);
                                        if (IsInternalBranchMasterConvTempExist == true)
                                        {
                                            err.Add(Locale.ConventionalBranchIdAlreadyExistChecker);
                                        }
                                        else {
                                            if (col["fldActive"] != null)
                                            {
                                                bool IsBranchCollapse = CheckRationalBranchCollapse(col["branchIdConv"], "C");

                                                if (IsBranchCollapse == true)
                                                {
                                                    err.Add(Locale.ConventionalBranchIdCollapsed);
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    err.Add(Locale.BranchisnotrelatedtoAffinBankBerhad);
                                }
                            }
                            else
                            {
                                err.Add(Locale.BranchisnotConventionalBranch);
                            }

                        }
                        else
                        {
                            err.Add(Locale.ConventionalBranchisnotexistinBranchProfile);
                        }

                        if (col["branchDescConv"].Equals(""))
                        {
                            err.Add(Locale.ConventionalBranchDesccannotbeblank);
                        }
                        if (col["chkSelfClearingConv"] == null)
                        {
                            if (col["clearingBranchIdConv"].Equals(""))
                            {
                                err.Add(Locale.ConventionalclearingBranchIdcannotbeblank);
                            }
                        }
                    }    
                }

                if ((col["checkIslamic"] != null))
                {
                    if (col["branchIdIslamic"].Equals(""))
                    {
                        err.Add(Locale.IslamicBranchIdcannotbeblank);
                    }
                    else
                    {
                        bool IsBranchMasterIslamicExist = CheckBranchMasterBankTypeById(col["branchIdIslamic"], "I");
                        if (IsBranchMasterIslamicExist == true)
                        {
                            bool IsIslamicBank = CheckBankType(col["branchIdIslamic"].Substring(0, 2), "I");
                            if (IsIslamicBank == true)
                            {
                                bool IsAffinBank = CheckRelatedBank(col["branchIdIslamic"].Substring(0, 2));
                                if (IsAffinBank == true)
                                {
                                    bool IsInternalBranchMasterIslamicExist = CheckInternalBranchByBranchId(col["branchIdIslamic"]);
                                    if (IsInternalBranchMasterIslamicExist == true)
                                    {
                                        err.Add(Locale.IslamicBranchIdAlreadyExist);
                                    }
                                    else
                                    {
                                        bool IsInternalBranchMasterIslamicTempExist = CheckInternalBranchTempByBranchId(col["branchIdIslamic"]);
                                        if (IsInternalBranchMasterIslamicTempExist == true)
                                        {
                                            err.Add(Locale.IslamicBranchIdAlreadyExistChecker);
                                        }
                                        else
                                        {
                                            if (col["fldActive"] != null)
                                            {
                                                bool IsBranchCollapse = CheckRationalBranchCollapse(col["branchIdIslamic"], "I");

                                                if (IsBranchCollapse == true)
                                                {
                                                    err.Add(Locale.IslamicBranchIdCollapsed);
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                                else
                                {
                                    err.Add(Locale.BranchisnotrelatedtoAffinIslamicBankBerhad);
                                }
                            }
                            else
                            {
                                err.Add(Locale.BranchisnotIslamicBranch);
                            }
                        }
                        else
                        {
                            err.Add(Locale.IslamicBranchisnotexistinBranchProfile);
                        }

                        if (col["branchDescIslamic"].Equals(""))
                        {
                            err.Add(Locale.IslamicBranchDesccannotbeblank);
                        }
                        if (col["chkSelfClearingIslamic"] == null)
                        {
                            if (col["clearingBranchIdIslamic"].Equals(""))
                            {
                                err.Add(Locale.IslamicclearingBranchIdcannotbeblank);
                            }
                        }
                    }
                    
                }
                #region Ali Comment Code

                //if (!col["fldEmailAddress"].Equals(""))
                //{
                //    if (!(new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")).IsMatch(col["fldEmailAddress"]))
                //    {
                //        err.Add(Locale.EmailFormat);
                //    }
                //}
                //if (!col["fldPostCode"].Equals(""))
                //{
                //    if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldPostCode"]))
                //    {
                //        err.Add(Locale.Postcodecannotbespecialchars);
                //    }
                //}
                #endregion

            }
            else
            {
                err.Add(Locale.PleaseEnterConventionalorIslamicInternalBranchCode);
            }
            return err;
        }

        


        public bool CheckBranchMasterBankTypeById(string id, string bankType)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchMasterBankType", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckBankType(string bankcode, string banktype)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", banktype));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankMasterType", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckRelatedBank(string bankcode)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankcode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankMasterAffin", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }


        public bool CheckInternalBranchByIdandBranchId(string id, string branchId)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", id));
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", branchId));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterByIdandBranchId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckRationalBranchCollapse(string id, string branchType)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            sqlParameterNext.Add(new SqlParameter("@fldBranchType", branchType));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgRationalBranchCollapsedById", sqlParameterNext.ToArray());

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

        public bool CheckInternalBranchByBranchId(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterByBranchId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckInternalBranchTempByBranchId(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchMasterTempByBranchId", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public List<string> ValidateUpdate(FormCollection col)
        {
            List<string> err = new List<string>();

            if ((col["checkConv"] != null) || (col["checkIslamic"] != null))
            {

                if ((col["checkConv"] != null))
                {
                    if (col["branchIdConv"].Equals(""))
                    {
                        err.Add(Locale.ConventionalBranchIdcannotbeblank);
                    }
                    else
                    {
                        bool IsBranchMasterConvExist = CheckBranchMasterBankTypeById(col["branchIdConv"], "C");
                        if (IsBranchMasterConvExist == true)
                        {   
                            bool IsConvBank = CheckBankType(col["branchIdconv"].Substring(0, 3), "C");
                            if (IsConvBank == true)
                            {
                                bool IsAffinBank = CheckRelatedBank(col["branchIdConv"].Substring(0, 3));

                                if (IsAffinBank == true)
                                {
                                    bool IsInternalBranchMasterConvExist = CheckInternalBranchByBranchId(col["branchIdConv"]);
                                    if (IsInternalBranchMasterConvExist == true)
                                    {
                                        bool IsInternalBranchMasterIdForUpdate = CheckInternalBranchByIdandBranchId(col["internalBranchID"], col["branchIdConv"]);
                                        if (IsInternalBranchMasterIdForUpdate == true)
                                        {
                                            if (col["fldActive"] != null)
                                            {
                                                bool IsBranchCollapse = CheckRationalBranchCollapse(col["branchIdConv"], "C");

                                                if (IsBranchCollapse == true)
                                                {
                                                    err.Add(Locale.ConventionalBranchIdCollapsed);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            err.Add(Locale.ConventionalBranchIdAlreadyExist);
                                        }
                                    }
                                    else
                                    {
                                        bool IsInternalBranchMasterConvTempExist = CheckInternalBranchTempByBranchId(col["branchIdConv"]);
                                        if (IsInternalBranchMasterConvTempExist == true)
                                        {                
                                                err.Add(Locale.ConventionalBranchIdAlreadyExistChecker);
                                        }

                                    }
                                }
                                else
                                {
                                    err.Add(Locale.BranchisnotrelatedtoAffinBankBerhad);
                                }
                            }
                            else
                            {
                                err.Add(Locale.BranchisnotConventionalBranch);
                            }

                        }
                        else
                        {
                            err.Add(Locale.ConventionalBranchisnotexistinBranchProfile);
                        }

                        if (col["branchDescConv"].Equals(""))
                        {
                            err.Add(Locale.ConventionalBranchDesccannotbeblank);
                        }
                        if (col["chkSelfClearingConv"] == null)
                        {
                            if (col["clearingBranchIdConv"].Equals(""))
                            {
                                err.Add(Locale.ConventionalclearingBranchIdcannotbeblank);
                            }
                        }
                    }
                }

                if ((col["checkIslamic"] != null))
                {
                    if (col["branchIdIslamic"].Equals(""))
                    {
                        err.Add(Locale.IslamicBranchIdcannotbeblank);
                    }
                    else
                    {
                        bool IsBranchMasterIslamicExist = CheckBranchMasterBankTypeById(col["branchIdIslamic"], "I");
                        if (IsBranchMasterIslamicExist == true)
                        {
                            bool IsIslamicBank = CheckBankType(col["branchIdIslamic"].Substring(0, 2), "I");
                            if (IsIslamicBank == true)
                            {
                                bool IsAffinBank = CheckRelatedBank(col["branchIdIslamic"].Substring(0, 2));
                                if (IsAffinBank == true)
                                {
                                    bool IsInternalBranchMasterIslamicExist = CheckInternalBranchByBranchId(col["branchIdIslamic"]);
                                    if (IsInternalBranchMasterIslamicExist == true)
                                    {
                                        bool IsInternalBranchMasterIdForUpdate = CheckInternalBranchByIdandBranchId(col["internalBranchID"], col["branchIdIslamic"]);
                                        if (IsInternalBranchMasterIdForUpdate == true)
                                        {
                                            if (col["fldActive"] != null)
                                            {
                                                bool IsBranchCollapse = CheckRationalBranchCollapse(col["branchIdIslamic"], "I");

                                                if (IsBranchCollapse == true)
                                                {
                                                    err.Add(Locale.IslamicBranchIdCollapsed);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            err.Add(Locale.IslamicBranchIdAlreadyExist);
                                        }
                                    }
                                    else
                                    {
                                        bool IsInternalBranchMasterIslamicTempExist = CheckInternalBranchTempByBranchId(col["branchIdIslamic"]);
                                        if (IsInternalBranchMasterIslamicTempExist == true)
                                        {
                                            err.Add(Locale.IslamicBranchIdAlreadyExistChecker);
                                        }

                                    }
                                }
                                else
                                {
                                    err.Add(Locale.BranchisnotrelatedtoAffinIslamicBankBerhad);
                                }
                            }
                            else
                            {
                                err.Add(Locale.BranchisnotIslamicBranch);
                            }
                        }
                        else
                        {
                            err.Add(Locale.IslamicBranchisnotexistinBranchProfile);
                        }

                        if (col["branchDescIslamic"].Equals(""))
                        {
                            err.Add(Locale.IslamicBranchDesccannotbeblank);
                        }
                        if (col["chkSelfClearingIslamic"] == null)
                        {
                            if (col["clearingBranchIdIslamic"].Equals(""))
                            {
                                err.Add(Locale.IslamicclearingBranchIdcannotbeblank);
                            }
                        }
                    }

                }
                //Commented by Ali
                //if (!col["fldEmailAddress"].Equals(""))
                //{
                //    if (!(new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")).IsMatch(col["fldEmailAddress"]))
                //    {
                //        err.Add(Locale.EmailFormat);
                //    }
                //}
                //if (!col["fldPostCode"].Equals(""))
                //{
                //    if (!(new Regex("^[a-zA-Z0-9]+$")).IsMatch(col["fldPostCode"]))
                //    {
                //        err.Add(Locale.Postcodecannotbespecialchars);
                //    }
                //}

            }
            else
            {
                err.Add(Locale.PleaseEnterConventionalorIslamicInternalBranchCode);
            }
            return err;
        }

        public bool UpdateInternalBranch(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string branchIdIslamic, branchDescIslamic, internalBranchCodeIslamic;
            string branchIdConv, branchDescConv, internalBranchCodeConv;
            string active;

            string clearingBranchIdConv = col["ClearingBranchIdConv"];
            string clearingBranchIdIslamic = col["ClearingBranchIdIslamic"];
            string checkConv = col["checkConv"];
            string checkIslamic = col["checkIslamic"];
            string internalBranchID = col["internalBranchID"];
            string checkActive = col["fldActive"];

            if (checkActive == null)
            {
                active = "N";
            }
            else
            {
                active = "Y";
            }

            if (checkIslamic != null)
            {
                branchIdIslamic = col["branchIdIslamic"];
                branchDescIslamic = col["branchDescIslamic"];
                internalBranchCodeIslamic = col["internalBranchCodeIslamic"];
                if (col["chkSelfClearingIslamic"] != null)
                {
                    clearingBranchIdIslamic = col["branchIdIslamic"];
                }
            }
            else
            {
                branchIdIslamic = "";
                branchDescIslamic = "";
                internalBranchCodeIslamic = "";
                clearingBranchIdIslamic = "";
            }

            if (checkConv != null)
            {
                branchIdConv = col["branchIdConv"];
                branchDescConv = col["branchDescConv"];
                internalBranchCodeConv = col["internalBranchCodeConv"];

                if (col["chkSelfClearingConv"] != null)
                {
                    clearingBranchIdConv = col["branchIdConv"];
                }
            }
            else
            {
                branchIdConv = "";
                branchDescConv = "";
                internalBranchCodeConv = "";
                clearingBranchIdConv = "";
            }

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", internalBranchID));

            //Islamic
            sqlParameterNext.Add(new SqlParameter("@fldIBranchId", branchIdIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIBranchDesc", branchDescIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIInternalBranchCode", internalBranchCodeIslamic));
            sqlParameterNext.Add(new SqlParameter("@fldIClearingBranchId", clearingBranchIdIslamic));

            //Conv
            sqlParameterNext.Add(new SqlParameter("@fldCBranchId", branchIdConv));
            sqlParameterNext.Add(new SqlParameter("@fldCBranchDesc", branchDescConv));
            sqlParameterNext.Add(new SqlParameter("@fldCInternalBranchCode", internalBranchCodeConv));
            sqlParameterNext.Add(new SqlParameter("@fldCClearingBranchId", clearingBranchIdConv));

            //Common
            /*Ali
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["fldEmailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["Address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["Address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["Address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["fldPostCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["fldCity"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["fldCountry"]));
            */
            sqlParameterNext.Add(new SqlParameter("@fldActive", active));
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

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", id));
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

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", id));

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

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", id));

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

            sqlParameterNext.Add(new SqlParameter("@fldInternalBranchId", id));
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

        public List<InternalBranchModel> ListInternalBranch(string bankType) //Done
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchModel> branchList = new List<InternalBranchModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchesForInternalBranch", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchModel branch = new InternalBranchModel();
                    if (bankType == "C")
                    {
                        branch.branchId = row["fldCBranchId"].ToString();
                        branch.branchDesc = row["fldCBranchDesc"].ToString();
                    }

                    if (bankType == "I")
                    {
                        branch.branchId = row["fldIBranchId"].ToString();
                        branch.branchDesc = row["fldIBranchDesc"].ToString();
                    }
                    branchList.Add(branch);
                }
            }
            return branchList;
        }

        public List<InternalBranchModel> ListCountry()
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchModel> countryList = new List<InternalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCountryForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchModel country = new InternalBranchModel();
                    country.countryCode = row["fldCountryCode"].ToString();
                    country.countryDesc = row["fldCountryDesc"].ToString();
                    countryList.Add(country);
                }
            }
            return countryList;
        }
        public List<InternalBranchModel> ListBankZone()
        {
            DataTable resultTable = new DataTable();
            List<InternalBranchModel> BankZoneList = new List<InternalBranchModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBankZoneCodeList", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    InternalBranchModel BankZone = new InternalBranchModel();
                    BankZone.BankZoneCode = row["fldBankZoneCode"].ToString();
                    BankZoneList.Add(BankZone);
                }
            }
            return BankZoneList;
        }
    }
}