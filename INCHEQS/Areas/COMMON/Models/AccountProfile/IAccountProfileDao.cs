using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web.Mvc;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.COMMON.Models.AccountProfile
{
    public interface IAccountProfileDao
    {
        List<AccountProfileModel> ListAccountType();
        List<AccountProfileModel> ListAccountStatus();
        List<AccountProfileModel> ListInternalBranchCode(string bankCode);
        List<AccountProfileModel> ListCountry();
        List<String> ValidateCreate(FormCollection col);
        bool CheckAccountProfileById(string id);
        bool CheckAccountProfileTempById(string id);
        bool CreateAccountProfile(FormCollection col);
        bool CreateAccountProfileTemp(FormCollection col,string action);
        AccountProfileModel GetAccountProfileData(string id);
        List<String> ValidateUpdate(FormCollection col);
        AccountProfileModel GetAccountProfileDataById(string id);
        AccountProfileModel GetAccountProfileTempById(string id);
        bool UpdateAccountProfile(FormCollection col);
        bool DeleteAccountProfile(string id);
        bool CreateAccountProfileTempToDelete(String id);
        bool MoveToAccountProfileFromTemp(String id);
        bool UpdateAccountProfileById(string id);
        bool DeleteAccountProfileTemp(string id);
        DataTable getBranchDetails(AccountModel currentUser);


    }
}
