using INCHEQS.Security.User;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.RationalBranch
{
    public interface IRationalBranchDao
    {
        string GetPageTitle(string TaskId);

        //DELETE
        bool DeleteRationalBranch(string branchId);

        //get menu title
        string GetMenuTitle(string TaskId);

        bool CheckRationalBranchTempExistByID(string branchId, string Action);

        //check if group user exist in groupusertemp table
        bool CheckSelectedBranchIdExistInTemp(string branchId, string selectedBranchId, string Action);

        //check if group user exist in groupuser table
        bool CheckRationalBranchExist(string branchId, string selectedBranchId);

        void CreateRationalBranchTemp(string branchid, string userId, string strUpdateId, string Action);

        //List of active merge
        List<RationalBranchModel> ListActiveRationalBranch(string strBankCode);

        //List of available branch for merge
        List<RationalBranchModel> ListAvailableRationalBranch(string strBankCode);
        void CreateRationalBranch(FormCollection col, string userId, string Action);
        List<RationalBranchModel> ListSelectedRationalBranch(string branchid, string strBankCode);

        RationalBranchModel GetBranch(string branchid, string Action);

        bool NoChangesBranch(FormCollection col, string branchid);
        bool NoChangesBranchSelected(FormCollection col, string branchid);

        void UpdateRationalBranchTable(string branchId, string seletedBranchId);

        //delete group user not selected
        void DeleteRationalBranchNotSelected(string branchId, string selectedBranchId);

        void MoveRationalBranchFromTemp(string branchId, string Action);
        void DeleteRationalBranchTemp(string branchId, string Action);
        void MoveRationalBranchFromTempRowByRow(string branchId, string selectedBrancdId, string Action);
        void DeleteRationalBranchTempRowByRow(string branchId, string selectedBrancdId, string Action);
        void DeleteRationalBranchFromTemp(string branchId);

        void UpdateRationalbranchTemp(string branchId);

        void CheckRationalBranchSeletedBranchHaveChildOrNot(string branchId, string selectedBrancdId);
      
        List<RationalBranchModel> ListSelectedRationalBranchChecker(string branchid);

        List<RationalBranchModel> ListSelectedRationalBranchCheckerWithAllTempRecord(string branchid);
        List<RationalBranchModel> ListAvailableRationalBranchChecker(string strBankCode);
    }
}
