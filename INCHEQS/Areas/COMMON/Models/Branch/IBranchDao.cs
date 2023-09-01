using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.Branch {
    public interface IBranchDao {

        void CreateBranchMasterTemp(FormCollection col, string bankcode, string crtUser, string Action);
        bool DeleteInBranchMasters(string BranchCode);
        bool UpdateBranchMaster(FormCollection col, string userId);
        List<string> ValidateBranch(FormCollection col, string action, string userId);
        bool CheckBranchMasterTempByID(string BranchCode, string BranchID, string SearchFor);
        void MoveToBranchMasterFromTemp(string branchCode, string Action);
        BranchModel CheckBranchMasterByID(string BranchCode, string BranchID, string SearchFor);
        bool DeleteInBranchMastersTemp(string BranchCode);
        string GetMenuTitle(string TaskId);
    }
}
