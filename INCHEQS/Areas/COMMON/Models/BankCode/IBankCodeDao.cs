using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.BankCode
{
	public interface IBankCodeDao
	{

		string condition();

		bool CreateInBankCodeTemp(FormCollection collection, string status);

		bool CreateInBankMaster(FormCollection collection);


		DataTable getBankCode(string bankCode,string bankType);

		Task<DataTable> getBankCodeAsync(string bankCode,string bankType);

        DataTable getBankCodeTemp(string bankCode);

        Task<DataTable> getBankCodeTempAsync(string bankCode);
        





		DataTable getBankType();

		Task<DataTable> getBankTypeAsync();


		List<string> ValidateCreate(FormCollection collection);

		Task<List<string>> ValidateCreateAsync(FormCollection collection);

		List<string> ValidateUpdate(FormCollection col);

		Task<List<string>> ValidateUpdateAsync(FormCollection col);

        BankCodeModel GetBankCodeData(string id,string bankType);
        bool UpdateBankCodeToMain(FormCollection col);
        bool CheckBankCodeDataTempById(String id);

        bool CreateBankCodeinMain(String id);
        bool DeleteBankCodeinTemp(String id);
        bool UpdateBankCodeToMainById(String id);
        bool DeleteInBankCode(String id);
        bool AddBankCodeinTemptoDelete(String id);
    }
}