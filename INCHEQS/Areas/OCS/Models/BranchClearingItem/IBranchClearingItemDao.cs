using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace INCHEQS.Areas.OCS.Models.BranchClearingItem
{
    public interface IBranchClearingItemDao
    {
        //string GetDataFromHubMaster();
        DataTable Find(string id);
        Task<DataTable> FindAsync(string id);
        void UpdateBranchItem(FormCollection col, AccountModel currentUser);

    }
}
