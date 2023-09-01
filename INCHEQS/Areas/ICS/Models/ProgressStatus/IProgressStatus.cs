using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ProgressStatus
{
    public interface IProgressStatusDao
    {
        List<ProgressStatusModel> ReturnProgressStatus(FormCollection col);
        List<ProgressStatusModel> ReturnFilterBranchProgressStatus(FormCollection col);
        List<ProgressStatusModel> ReturnFilterPPSBranchProgressStatus(FormCollection col);
        string GetUserType();
        string getClearDate();
    }
}