using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.PPS.Models.StartBatchICR
{
    public interface IStartBatchICRDao
    {
        string getOCRStatus();
        List<StartBatchICRModel> getMachineId();
        Task<double> percentage(string machineID);
        StartBatchICRModel ICRProgressDetails(String machineID);
        StartBatchICRModel getProcessStartEndTime(String processName);
        List<StartBatchICRModel> getAllProgressDetails(String machineID);
        Task<List<StartBatchICRModel>> GetProgressDetail(String machineID);
        Task<StartBatchICRModel> GetUpperPartDetail(FormCollection collection, string processName);
        bool checkDataOCRTemp();
    }
}