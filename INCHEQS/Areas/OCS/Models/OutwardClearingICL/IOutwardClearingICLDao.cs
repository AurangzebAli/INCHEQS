using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.OutwardClearingICL
{
    public interface IOutwardClearingICLDao
    {
        DataTable GetHubBranches(string userId);
        DataTable ReadyforCenterClearing(FormCollection collection);
        DataTable CenterSubmittedItems(FormCollection collection);
        string getBetween(string strSource, string strStart, string strEnd);
        void AddToBatch(string intUserId, string strSelectedUIC, string processdate);
        DataTable GetCenterItemReadyForClearingList(string SelectedRow);
        DataTable GetCenterClearedItemList(string CapturingBranch, string CapturingDate, string clearingBatch);
    }
}
