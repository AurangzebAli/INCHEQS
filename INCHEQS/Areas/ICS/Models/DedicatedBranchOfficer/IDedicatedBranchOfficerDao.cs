using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.DedicatedBranchOfficer
{
    public interface iDedicatedBranchOfficerDao
    {
        List<DedicatedBranchOfficerModel> ListAvailableVerifier(string userId, string clearDate);
        List<DedicatedBranchOfficerModel> ListSelectedVerifier(string userId, string clearDate);
        DedicatedBranchOfficerModel InsertUserDedicatedBranch(FormCollection col);
    }
}