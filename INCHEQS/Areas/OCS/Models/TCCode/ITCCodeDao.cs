using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.TCCode {
    public interface ITCCodeDao {

        Task<DataTable> ListAllAsync();
        Task<DataTable> FindAsync(string TCCode);
        void UpdateTCCode(string tcCode);
        void DeleteTCCode(string tcCode);
        void AddToTCCodeTempToDelete(string tcCode);
        void AddToTCCodeTempToUpdate(string tcCode);
        void UpdateTCCodeInTemp(FormCollection col);
        void DeleteInTCCodeTemp(string tcCode);
        DataTable ListAll();
        DataTable Find(string TCCode);
        void CreateTCCodeInTemp(FormCollection col, AccountModel currentUser);
        void CreateTCCode(string tcCode);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);
        bool CheckExist(string tcCode);
        bool CheckPendingApproval(string tcCode);
    }
}
