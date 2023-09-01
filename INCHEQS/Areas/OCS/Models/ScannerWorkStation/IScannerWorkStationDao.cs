using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.ScannerWorkStation
{
    public interface IScannerWorkStationDao
    {

        Task<DataTable> ListAllAsync();
        Task<DataTable> getScannerNameAsync();
        Task<DataTable> getBranchDetailsAsync(AccountModel currentUser);
        Task<DataTable> getBankDetailsAsync();
        Task<DataTable> getLocationDetailsAsync();
        Task<DataTable> FindAsync(string Scanner, string Type, string BranchId);
        Task<DataTable> getScannerBrandNameAsync(int ScannerTypeID);
        //void DeleteScannerWorkStation(string transactionType);
        //void UpdateScannerWorkStation(FormCollection col);
        DataTable ListAll();
        DataTable getScannerName();
        DataTable getBranchDetails(AccountModel currentUser);
        DataTable getBankDetails();
        DataTable getLocationDetails();
        //string getBetween();
        DataTable Find(string Scanner, string Type, string BranchId);
        void CreateScannerWorkStation(FormCollection col, AccountModel currentUser);
        DataTable getScannerBrandName(int ScannerTypeID);
        Task<ScannerWorkStationModel> getScannerAsync(string Scannner);
        List<String> Validate(FormCollection col, string action);
        string getScannerId();
        string getScannerIdFromTemp();
        string getBetween(string strSource, string strStart, string strEnd);

        bool CreateScannerWorkStationinMain(string id);
        ScannerWorkStationModel GetScannerWorkStationData(string id);
        bool DeleteScannerWorkStation(string id);

        bool UpdateScannerWorkStationToMain(string id);
        bool DeleteScannerWorkstationTemp(string value);
        bool UpdateScannerWorkStation(FormCollection col);
        bool CreateScannerWorkStationinTemptoUpdate(FormCollection col);
        bool CheckScannerWorkStationTempById(String id);
        bool CreateInScannerWorkStationTemp(FormCollection col);
        bool CheckScannerWorkStationTempExistByApproveStatus();
        bool CheckScannerWorkStation();
        bool CreateScannerWorkStationinTemptoDelete(String id);
        string GetMaxScannerID();
    }
}

