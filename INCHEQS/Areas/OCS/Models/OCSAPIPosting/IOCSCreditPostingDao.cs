using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.OCSAPIPosting
{
    public interface IOCSCreditPostingDao
    {
        DataTable GetHubBranches(string userId);
        void GenerateNewBatches(string bankcode, string intUserId, string processdate, string Totalitems, Int64 TotalAmount);
        DataTable PostedItemHistory(FormCollection collection);
        DataTable PostedItemFileBaseHistory(FormCollection collection);
        DataTable ReadyItemForPostingHistory(FormCollection collection);
        bool RegeneratePosting(FormCollection collection);
        bool checkBatchExist(Int64 datebatch);
    }
}
