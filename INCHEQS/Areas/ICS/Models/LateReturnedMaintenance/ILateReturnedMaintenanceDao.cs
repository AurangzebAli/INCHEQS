using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.LateReturnedMaintenance
{
    public interface ILateReturnedMaintenanceDao
    {
        List<LateReturnMaintenanceModal> GetClearDateforLateMaintenance();
        List<string> ValidateSearch(LateReturnMaintenanceModal col);
        DataTable GetLateMaintenanceItems();
        Boolean CheckLateReturnExistMainTable(LateReturnMaintenanceModal col);
        Boolean CheckLateReturnExistHistoryTable(LateReturnMaintenanceModal col);
        Boolean CheckExist(string UIC, string clearDate);
        List<string> InsertLateReturnMaintenanceRecord(LateReturnMaintenanceModal col);
        void insertInwarditem(LateReturnMaintenanceModal col);
        void InsertHistory(LateReturnMaintenanceModal col);
        void insertLatemaintenance(LateReturnMaintenanceModal col);
        void insertLatemaintenanceh(LateReturnMaintenanceModal col);
        void InsertHistoryH(LateReturnMaintenanceModal col);
        void deleteUPIH(string UIC, string clearDate, string lateid);
        void deleteUPI(string UIC, string clearDate, string lateid);
        void updateInwarditem(string UIC, string clearDate);
        void updateInwarditemH(string UIC, string clearDate);
        Task<List<LateReturnMaintenanceModal>> getLateReturnMaintenanceAsyn(FormCollection collection);
        List<LateReturnMaintenanceModal> getLateReturnMaintenance(FormCollection collection);
    }
}
