using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.Signature
{
    public interface ISignatureDao
    {

      //  List<SignatureInfo> GetCheckedSignature(string inwardItemId);
        //List<AccountInfo> GetSignatureDetails(string accountNo);
        //List<SignatureInfo> GetSignatureRules(string accountNo);
        List<AccountInfo> GetSignatureRulesInfo(string accountNo);
        List<AccountInfo> GetSignatureInformation(string accountNo);
        DataTable GetSignatureNoForAccount(string accountNo);
        bool CheckValidateSignature(FormCollection collection, AccountModel currentUser);
        //  void InsertSignatureHistoryImage(FormCollection collection, AccountModel currentUser);
        //void DeleteSignatureHistoryImage(FormCollection collection);

        // Task<List<SignatureInfo>> GetCheckedSignatureAsync(string inwardItemId);
        Task<List<AccountInfo>> GetSignatureDetailsAsync(string accountNo, string issueBankBranch);
        Task<List<AccountInfo>> GetSignatureRulesAsync(string accountNo, string issueBankBranch);
        Task<DataTable> GetSignatureNoForAccountAsync(string accountNo);
        Task<List<AccountInfo>> GetImageDetailsAsync(string accountNo, string issuingBankBranch, string imageNo);

        Task<byte[]> GetSignatureImage(string accountNo, string imageNo);
        //byte[] GetSignatureImage2(string accountNo, string imageNo);
        Task<bool> ISSDSConnectionAvailable();
        List<AccountInfo> GetSignatureRulesInfoList(string accountNo);
        Task<List<AccountInfo>> GetSignatureRulesInfoListAsync(string accountNo, string issuingBankBranch);

    }
}
