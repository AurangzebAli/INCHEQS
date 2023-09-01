using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Security.Account;
using System.Collections.Generic;

namespace INCHEQS.Areas.ICS.Models.HostValidation
{
    public interface IHostValidationDao
    {
        void InsertHostValidation(InwardItemViewModel inwardItemViewModel, AccountModel currentUser);
        List<string> ValidateHostValidation(AccountModel currentUser);

        bool UpdateGenuineness(HostValidationModel inwardItemViewModel, AccountModel currentUser);
    }
}