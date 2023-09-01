using INCHEQS.Areas.ICS.Models.HostFile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.HostFile {
    public interface IHostFileDao {
        DataTable GetOutwardReturnDetail(string clearDate, string dataFileName);
        HostFileModel GetDataFromHostFileConfig(string taskId);
        //DataTable getFilemanager(string clearDate, string taskId, string BankCode);
        void UpdateFTPSend(string fileName);
        void UpdateUPIbatchAck(string AckMessage);
        void OutwardReturnDetailRegen(string clearDate, string dataFileName);
    }
}