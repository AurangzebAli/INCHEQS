using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.SystemCutOffTime
{
    public interface ISystemCutOffTimeDao
    {
        void AddToSystemCutOffTimeTempToDelete(string desc);
        void AddToSystemCutOffTimeTempToUpdate(FormCollection col, AccountModel currentUser);
        bool CheckExist(string desc);
        bool CheckPendingApproval(string desc);
        void CreateSystemCutOffTime(string desc);
        void CreateSystemCutOffTimeInTemp(FormCollection col, AccountModel currentUser);
        void DeleteSystemCutOffTime(string desc);
        void DeleteInSystemCutOffTimeTemp(string desc);
        DataTable Find(string systemCutOffId);
        Task<DataTable> FindAsync(string systemCutOffId);
        void UpdateSystemCutOffTime(string desc);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);
    }
}
