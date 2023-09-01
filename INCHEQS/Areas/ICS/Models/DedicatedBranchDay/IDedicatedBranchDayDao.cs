using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;


namespace INCHEQS.Areas.ICS.Models.DedicatedBranchDay
{
    public interface IDedicatedBranchDayDao
    {
        DataTable ListAll();
        DedicatedBranchDayModel FilterCCU();
        DedicatedBranchDayModel getUserProfile(string userId);
        DedicatedBranchDayModel InsertUserDedicatedBranch(FormCollection col);
        List<DedicatedBranchDayModel> ListSelectedBranch(string userId,string clearDate);
        List<DedicatedBranchDayModel> ListAvailableBranch(string userId, string clearDate);
        List<string> ValidateUser(FormCollection col, string action);
        bool CheckUserExist(string userAbb);

    }
}