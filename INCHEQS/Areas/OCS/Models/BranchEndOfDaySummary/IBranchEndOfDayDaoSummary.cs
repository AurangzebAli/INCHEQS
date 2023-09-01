using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace INCHEQS.Areas.OCS.Models.BranchEndOfDaySummary
{
    public interface IBranchEndOfDaySummaryDao
    {
        DataTable GetCenterEndOfDay(string strBankCode);
        DataTable GetPendingBranchesInfo(string strBankCode);
        bool InsertCenterEndOfDay(string strUserId, string strBankCode);
        bool CheckBranchNotYetEOD(string strBankCode);
    }
}

