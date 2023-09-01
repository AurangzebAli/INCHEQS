using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.ClearingItem
{
    public interface IClearingItemDao
    {
        DataTable GetHubBranches(string userId);
        bool UpdateClearingStatusandInsertClearingAgent(string CapturingBranch, string CapturingDate, string ScannerId, string BatchNumber);
    }
}
