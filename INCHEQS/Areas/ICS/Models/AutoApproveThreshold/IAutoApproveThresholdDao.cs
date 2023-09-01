using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.ICS.Models.AutoApproveThresholdModel{
    public interface IAutoApproveThresholdDao
    {
        AutoApproveThresholdModel GetTotalInwardItem(FormCollection collection);
        AutoApproveThresholdModel GetTotalPendingInwardItem(FormCollection collection);
        AutoApproveThresholdModel GetClearDate();
        List<String> ValidateForm(FormCollection collection);
        string GetFilteredAmount(FormCollection collection, int filteredPendingItem);
        string GetMaxAmount(FormCollection collection, int filteredPendingItem);
        string GetMinAmount(FormCollection collection, int filteredPendingItem);
        List<int> GetRangeList(FormCollection collection, string maxAmount);
        List<Double> GetRangeItem(FormCollection collection, int filteredPendingItem);
        //AutoApproveThresholdModel GetFilteredItems(FormCollection collection, int filteredPendingItem);
        void UpdateAutoApproveThreshold(FormCollection collection, string userid,string fldClearDate, string filteredPendingItem);


        bool CheckAAT();


    }
}