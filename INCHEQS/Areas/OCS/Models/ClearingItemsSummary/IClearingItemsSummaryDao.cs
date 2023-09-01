using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.ClearingItemsSummary
{
    public interface IClearingItemsSummaryDao
    {
        DataTable GetHubBranches(string userId);
    }
}
