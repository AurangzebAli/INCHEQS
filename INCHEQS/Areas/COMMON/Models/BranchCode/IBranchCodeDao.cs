using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.BranchCode
{
	public interface IBranchCodeDao
	{
		void AddtoBranchMasterTempToDelete(string id);

		string condition();

		void CreateBranchMasterTemp(FormCollection col, string userId);

		void CreateInBranchMaster(string id);

		void DeleteInBranchMasters(string delete);

		void DeleteInBranchMasterTemp(string branchId);

		List<BranchCodeModel> getBank();

		DataTable getBranchCode(string branchid);

        DataTable getBranchCodeFromCheckerView(string branchid);

        DataTable getBankType();

        DataTable ListAllBranchCode();

		void UpdateBranchMaster(FormCollection col, string userId);

		//Task<List<string>> ValidateCreateAsync(FormCollection collection);

        bool CreateBranchMaster(FormCollection col);

        List<String> ValidateUpdate(FormCollection col);
        BranchCodeModel GetBranchCodeDataById(string id);
        bool UpdateBranchCode(FormCollection col);
        bool CheckBranchCodeTempById(string id);
        bool CreateBranchCodeTemp(FormCollection collection, string action);
        bool DeleteBranchCode(string id);
        bool CreateBranchCodeTempToDelete(String id);
        bool MoveToBranchCodeFromTemp(String id);
        bool UpdateBranchCodeById(string id);
        bool DeleteBranchCodeTemp(string id);
        List<String> ValidateCreate(FormCollection col);
        bool CheckBranchCodeById(string id);
    }
}