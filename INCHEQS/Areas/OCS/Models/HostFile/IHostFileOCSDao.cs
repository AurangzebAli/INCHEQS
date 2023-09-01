using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.HostFile {
    public interface IHostFileOCSDao {
        DataTable GetOutwardReturnDetail(string clearDate, string dataFileName);
        HostFileOCSModel GetDataFromHostFileConfig(string taskId);
        //DataTable getFilemanager(string clearDate, string taskId, string BankCode);
        void UpdateFTPSend(string fileName);
        void UpdateUPIbatchAck(string AckMessage);
        void OutwardReturnDetailRegen(string clearDate, string dataFileName);
    }
}