using INCHEQS.Common.Resources;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;
using INCHEQS.Security.Account;
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
using INCHEQS.Resources;
using INCHEQS.Common;
using System.Globalization; 

namespace INCHEQS.Areas.COMMON.Models.AccountProfile
{
    public class AccountProfileDao : IAccountProfileDao
    {
        private readonly ApplicationDbContext dbContext;

        public AccountProfileDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<AccountProfileModel> ListAccountType()
        {
            DataTable resultTable = new DataTable();
            List<AccountProfileModel> accountTypeList = new List<AccountProfileModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountTypeForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    AccountProfileModel account = new AccountProfileModel();
                    account.accountType = row["fldAccountType"].ToString();
                    account.accountTypeDesc = row["fldAccountTypeDesc"].ToString();
                    accountTypeList.Add(account);
                }
            }
            return accountTypeList;
        }

        public List<AccountProfileModel> ListAccountStatus()
        {
            DataTable resultTable = new DataTable();
            List<AccountProfileModel> accountStatusList = new List<AccountProfileModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountStatusForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    AccountProfileModel account = new AccountProfileModel();
                    account.accountStatus = row["fldAccountStatus"].ToString();
                    account.accountStatusDesc = row["fldAccountStatusDesc"].ToString();
                    accountStatusList.Add(account);
                }
            }
            return accountStatusList;
        }


        public List<AccountProfileModel> ListInternalBranchCode(string bankCode)
        {
            DataTable resultTable = new DataTable();
            List<AccountProfileModel> internalBranchCodeList = new List<AccountProfileModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgInternalBranchCodeForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    AccountProfileModel BranchID = new AccountProfileModel();
                    BranchID.BranchID = row["fldBranchCode"].ToString();
                    internalBranchCodeList.Add(BranchID);
                }
            }
            return internalBranchCodeList;
        }

        public List<AccountProfileModel> ListCountry()
        {
            DataTable resultTable = new DataTable();
            List<AccountProfileModel> countryList = new List<AccountProfileModel>();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            //sqlParameterNext.Add(new SqlParameter("@fldRejectCode", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgCountryForAccountProfile", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    AccountProfileModel country = new AccountProfileModel();
                    country.countryCode = row["fldCountryCode"].ToString();
                    country.countryDesc = row["fldCountryDesc"].ToString();
                    countryList.Add(country);
                }
            }
            return countryList;
        }

        public List<String> ValidateCreate(FormCollection col)
        {
            List<String> err = new List<String>();

            
            if (col["accountNumber"].Equals(""))
            {
                err.Add("Account Number cannot be empty");
            }
            else
            {
            bool IsAccountProfileExist = CheckAccountProfileById(col["accountNumber"]);
            if (IsAccountProfileExist == true)
            {
                err.Add(Resources.Locale.AccountProfileAlreadyExist);
            }

            bool IsAccountProfileTempExist = CheckAccountProfileTempById(col["accountNumber"]);
            if (IsAccountProfileTempExist == true)
            {
                err.Add(Resources.Locale.AccountProfileAlreadyCreatedinApproveChecker);

            }

            if (!(new Regex("^[0-9]+$")).IsMatch(col["accountNumber"]))
            {
                err.Add("Account Number must be numbers");
            }

            if (col["accountName"].Equals(""))
            {
                err.Add("Account Name cannot be empty");
            }
            if (col["accountType"].Equals(""))
            {
                err.Add("Account Type cannot be empty");
            }
            if (col["accountStatus"].Equals(""))
            {
                err.Add("Account Status cannot be empty");
            }
            }
            

            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["accountName"].Equals(""))
            {
                err.Add("Account Name Cannot Be Empty");
            }
            if (col["accountType"].Equals(""))
            {
                err.Add("Account Type Cannot Be Empty");
            }
            if (col["accountStatus"].Equals(""))
            {
                err.Add("Account Status Cannot Be Empty");
            }

            return err;
        }

        public bool CheckAccountProfileById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CheckAccountProfileTempById(string id)
        {
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool CreateAccountProfile(FormCollection col)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", col["accountNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountName", col["accountName"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountType", col["accountType"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountStatus", col["accountStatus"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchID", col["branchId"]));

            if (Regex.IsMatch(col["OpeningDate"].ToString().Trim(), @"(((0|1)[0-9]|2[0-9]|3[0-1])\-(0[1-9]|1[0-2])\-(\d\d))$"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldOpeningDate", col["OpeningDate"]));
            }
            else
            {
                String openDate = (DateTime.ParseExact(col["OpeningDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                sqlParameterNext.Add(new SqlParameter("@fldOpeningDate", openDate));
            }

            if (Regex.IsMatch(col["ClosingDate"].ToString().Trim(), @"(((0|1)[0-9]|2[0-9]|3[0-1])\-(0[1-9]|1[0-2])\-(\d\d))$"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldClosingDate", col["ClosingDate"]));
            }
            else
            {
                String openDate = (DateTime.ParseExact(col["ClosingDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                sqlParameterNext.Add(new SqlParameter("@fldClosingDate", openDate));
            }
            

            sqlParameterNext.Add(new SqlParameter("@fldCIFNumber", col["customerNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldContactNumber", col["ContactNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["emailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["postCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["city"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["country"]));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciAccountProfile", sqlParameterNext.ToArray());

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

        public bool CreateAccountProfileTemp(FormCollection col, string action)
        {
            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", col["accountNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountName", col["accountName"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountType", col["accountType"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountStatus", col["accountStatus"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchID", col["branchId"]));

            if (Regex.IsMatch(col["OpeningDate"].ToString().Trim(), @"(((0|1)[0-9]|2[0-9]|3[0-1])\-(0[1-9]|1[0-2])\-(\d\d))$"))
            {
                sqlParameterNext.Add(new SqlParameter("@fldOpeningDate", col["OpeningDate"]));
            }
            else
            {
                String openDate = (DateTime.ParseExact(col["OpeningDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                sqlParameterNext.Add(new SqlParameter("@fldOpeningDate", openDate));
            }

            if (Regex.IsMatch(col["ClosingDate"].ToString().Trim(), @"(((0|1)[0-9]|2[0-9]|3[0-1])\-(0[1-9]|1[0-2])\-(\d\d))$"))
            {

                sqlParameterNext.Add(new SqlParameter("@fldClosingDate", col["ClosingDate"]));
            }
            else
            {
                String openDate = (DateTime.ParseExact(col["ClosingDate"].ToString(), "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"));
                sqlParameterNext.Add(new SqlParameter("@fldClosingDate", openDate));
            }

            sqlParameterNext.Add(new SqlParameter("@fldCIFNumber", col["customerNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldContactNumber", col["ContactNumber"])); 
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["emailAddress"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["postCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["city"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["country"]));
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
                sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", col["CreateUserId"]));
                sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "U"));
            }


            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciAccountProfileTemp", sqlParameterNext.ToArray());

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

        public AccountProfileModel GetAccountProfileData(string id)
        {
            DataTable dataTable = new DataTable();
            AccountProfileModel accModal = new AccountProfileModel();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));

            dataTable = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileById", sqlParameterNext.ToArray());
            if (dataTable.Rows.Count > 0)
            {
                DataRow row = dataTable.Rows[0];
                accModal.accountNumber = row["fldAccountNumber"].ToString();
                accModal.accountName = row["fldAccountName"].ToString();

            }
            return accModal;

        }

        public AccountProfileModel GetAccountProfileDataById(string id)
        {
            AccountProfileModel AccountProfile = new AccountProfileModel();
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileDataById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                AccountProfile.bankCode = row["fldBankCode"].ToString();
                AccountProfile.accountNumber = row["fldAccountNumber"].ToString();
                AccountProfile.accountName = row["fldAccountName"].ToString();
                AccountProfile.accountType = row["fldAccountType"].ToString();
                AccountProfile.accountStatus = row["fldAccountStatus"].ToString();
                AccountProfile.BranchID = row["fldBranchID"].ToString();

                AccountProfile.OpeningDate = row["fldOpeningDate"].ToString();
                AccountProfile.ClosingDate = row["fldClosingDate"].ToString();

                AccountProfile.customerNumber = row["fldCIFNumber"].ToString();

                AccountProfile.contactNumber = row["fldContactNumber"].ToString();
                AccountProfile.emailAddress = row["fldEmailAddress"].ToString();

                AccountProfile.address1 = row["fldAddress1"].ToString();
                AccountProfile.address2 = row["fldAddress2"].ToString();
                AccountProfile.address3 = row["fldAddress3"].ToString();
                AccountProfile.postCode = row["fldPostCode"].ToString();
                AccountProfile.city = row["fldCity"].ToString();
                AccountProfile.countryCode = row["fldCountry"].ToString();
                AccountProfile.createUserId = row["fldCreateUserId"].ToString();
                AccountProfile.createTimeStamp = DateUtils.formatDateFromSql(row["fldCreateTimeStamp"].ToString());
                AccountProfile.updateUserId = row["fldUpdateUserId"].ToString();
                AccountProfile.updateTimeStamp = row["fldUpdateTimeStamp"].ToString();
                AccountProfile.accountIdForDelete = row["fldAccountIdForDelete"].ToString();
                //AccountProfile.reportTitle = row["ReportTitle"].ToString();


            }

            return AccountProfile;
        }

        public AccountProfileModel GetAccountProfileTempById(string id)
        {
            AccountProfileModel AccountProfile = new AccountProfileModel();
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgAccountProfileTempById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];

                AccountProfile.bankCode = row["fldBankCode"].ToString();
                AccountProfile.accountNumber = row["fldAccountNumber"].ToString();
                AccountProfile.accountName = row["fldAccountName"].ToString();
                AccountProfile.accountType = row["fldAccountType"].ToString();
                AccountProfile.accountStatus = row["fldAccountStatus"].ToString();
                AccountProfile.BranchID = row["fldBranchID"].ToString();

                AccountProfile.OpeningDate = row["fldOpeningDate"].ToString();
                AccountProfile.ClosingDate = row["fldClosingDate"].ToString();

                AccountProfile.customerNumber = row["fldCIFNumber"].ToString();

                AccountProfile.contactNumber = row["fldContactNumber"].ToString();
                AccountProfile.emailAddress = row["fldEmailAddress"].ToString();

                AccountProfile.address1 = row["fldAddress1"].ToString();
                AccountProfile.address2 = row["fldAddress2"].ToString();
                AccountProfile.address3 = row["fldAddress3"].ToString();
                AccountProfile.postCode = row["fldPostCode"].ToString();
                AccountProfile.city = row["fldCity"].ToString();
                AccountProfile.countryCode = row["fldCountry"].ToString();

                AccountProfile.status = row["fldStatus"].ToString();
            }

            return AccountProfile;
        }

        public bool UpdateAccountProfile(FormCollection col)
        {
            int intRowAffected;
            bool blnResult = false;


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", col["accountNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountName", col["accountName"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountType", col["accountType"]));
            sqlParameterNext.Add(new SqlParameter("@fldAccountStatus", col["accountStatus"]));
            sqlParameterNext.Add(new SqlParameter("@fldBranchId", col["branchId"]));
            
            sqlParameterNext.Add(new SqlParameter("@fldOpeningDate", col["OpeningDate"]));
            sqlParameterNext.Add(new SqlParameter("@fldClosingDate", col["ClosingDate"]));

            sqlParameterNext.Add(new SqlParameter("@fldCIFNumber", col["customerNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldContactNumber", col["ContactNumber"]));
            sqlParameterNext.Add(new SqlParameter("@fldEmailAddress", col["emailAddress"]));

            sqlParameterNext.Add(new SqlParameter("@fldAddress1", col["address1"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress2", col["address2"]));
            sqlParameterNext.Add(new SqlParameter("@fldAddress3", col["address3"]));
            sqlParameterNext.Add(new SqlParameter("@fldPostCode", col["postCode"]));
            sqlParameterNext.Add(new SqlParameter("@fldCity", col["city"]));
            sqlParameterNext.Add(new SqlParameter("@fldCountry", col["country"]));

            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));

            //sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", col["createTimeStamp"]));
            //sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", col["createUserId"]));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuAccountProfile", sqlParameterNext.ToArray());

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

        public bool DeleteAccountProfile(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdAccountProfile", sqlParameterNext.ToArray());

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

        public bool CreateAccountProfileTempToDelete(String id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciAccountProfileTempToDelete", sqlParameterNext.ToArray());

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

        public bool MoveToAccountProfileFromTemp(String id)
        {

            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciAccountProfileFromTemp", sqlParameterNext.ToArray());

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

        public bool UpdateAccountProfileById(string id)
        {
            //ReturnCodeModel bankCode = new ReturnCodeModel();
            bool Flag = false;
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcuAccountProfileById", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }

        public bool DeleteAccountProfileTemp(string id)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldAccountNumber", id));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdAccountProfileTemp", sqlParameterNext.ToArray());

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

        public DataTable getBranchDetails(AccountModel currentUser)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldbranchcode,fldbranchdesc,fldbranchid FROM tblInternalBranchMaster where fldbankcode = @fldbankcode and fldactive='Y' order by fldbranchcode";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldbankcode", currentUser.BankCode) });

            return ds;
        }
    }
}