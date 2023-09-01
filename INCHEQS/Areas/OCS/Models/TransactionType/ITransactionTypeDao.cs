using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.TransactionType {
    public interface ITransactionTypeDao {

        Task<DataTable> ListAllAsync();
        Task<DataTable> FindAsync(string TransType);
        void UpdateTransactionType(string transactionType);
        void DeleteTransactionType(string transactionType);
        void AddToTransactionTypeTempToDelete(string transactionType);
        void AddToTransactionTypeTempToUpdate(string transactionType);
        void UpdateTransactionTypeInTemp(FormCollection col);
        void DeleteInTransactionTypeTemp(string transactionType);
        DataTable ListAll();
        DataTable Find(string TransType);
        void CreateTransactionTypeInTemp(FormCollection col, AccountModel currentUser);
        void CreateTransactionType(string transactionType);
        List<String> ValidateCreate(FormCollection col);
        List<String> ValidateUpdate(FormCollection col);
        List<TransactionTypeModel> ListTransactionTypes();
        bool CheckPendingApproval(string transType);
    }
}
