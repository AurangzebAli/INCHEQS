
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCHEQS.Areas.ICS.Models.OutwardReturnICL
{
    public interface IOutwardReturnICLDao
    {
        //DataTable GetHubBranches(string userId);
        string getBetween(string strSource, string strStart, string strEnd);
        void updateICSItemReadyForReturnICL(string clearDate,string issuingBankBranch); 
         OutwardReturnICLModel GetDataFromOutwardReturnICLConfig(string taskId);
        DataTable GetCenterItemReadyForReturnICLList(string SelectedRow);
        DataTable GetCenterItemForReturnedICLList(string SelectedRow);
    }
}
