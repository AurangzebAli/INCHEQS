using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ProcessDate
{
    public interface IProcessDateDao
    {
        void UpdateProcessDate(FormCollection col);
        void CreateProcessDate(FormCollection col, AccountModel currentUser);
        string getCurrentDate();
        string getNextProcessDate();
        Task<DataTable> ListAllAsync();
        Task<DataTable> FindAsync(string TransType);
        void UpdateTransactionType(string transactionType);
        void DeleteTransactionType(string transactionType);
        void AddToTransactionTypeTempToDelete(string transactionType);
        void AddToTransactionTypeTempToUpdate(string transactionType);        
        void DeleteInTransactionTypeTemp(string transactionType);
        DataTable ListAll();
        DataTable Find(string TransType);        
        void CreateTransactionType(string transactionType);
        List<String> ValidateCreate(FormCollection col);
    }
}
