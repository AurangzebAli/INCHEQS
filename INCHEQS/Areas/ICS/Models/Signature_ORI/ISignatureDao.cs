using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.Signature {
    public interface ISignatureDao {

        List<SignatureInfo> GetCheckedSignature(string inwardItemId);
        List<SignatureInfo> GetSignatureDetails(string accountNo);
        List<SignatureInfo> GetSignatureRules(string accountNo);
        DataTable GetSignatureNoForAccount(string accountNo);
        bool CheckValidateSignature(FormCollection collection, AccountModel currentUser);
        void InsertSignatureHistoryImage(FormCollection collection, AccountModel currentUser);
        void DeleteSignatureHistoryImage(FormCollection collection);

        Task<List<SignatureInfo>> GetCheckedSignatureAsync(string inwardItemId);
        Task<List<SignatureInfo>> GetSignatureDetailsAsync(string accountNo);
        Task<List<SignatureInfo>> GetSignatureRulesAsync(string accountNo);
        Task<DataTable> GetSignatureNoForAccountAsync(string accountNo);

        Task<byte[]> GetSignatureImage(string accountNo, string imageNo);
        Task<bool> ISSDSConnectionAvailable();
    }
}
