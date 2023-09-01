using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ICSAPICreditPosting
{
    public interface IICSCreditPostingDao
    {
        DataTable GetHubBranches(string userId);
        void GenerateNewBatches(string bankcode, string intUserId, string processdate, string Totalitems, string TotalAmount);
        DataTable PostedItemHistory(FormCollection collection);
        DataTable ReadyItemForPostingHistory(FormCollection collection);


    }
}
