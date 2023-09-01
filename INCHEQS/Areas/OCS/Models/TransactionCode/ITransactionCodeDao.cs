using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.TransactionCode {
    public interface ITransactionCodeDao {

        Task<DataTable> ListAllAsync();
        Task<DataTable> FindAsync(string TransCode);
        void UpdateTransactionCode(string transactionCode);
        void DeleteTransactionCode(string transactionCode);
        void AddToTransactionCodeTempToDelete(string transactionCode);
        void AddToTransactionCodeTempToUpdate(string transactionCode);
        void UpdateTransactionCodeInTemp(FormCollection col);
        void DeleteInTransactionCodeTemp(string transactionCode);
        DataTable ListAll();
        DataTable Find(string TransCode);
        void CreateTransactionCodeInTemp(FormCollection col, AccountModel currentUser);
        void CreateTransactionCode(string transactionCode);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);
        bool CheckExist(string transCode);
        bool CheckPendingApproval(string transCode);
    }
}
