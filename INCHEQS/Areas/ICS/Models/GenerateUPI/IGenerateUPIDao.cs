using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.GenerateUPI
{
    public interface IGenerateUPIDao
    {
       
        string getBetween(string strSource, string strStart, string strEnd);
        GenerateUPIModel GetDataFromUPIConfig(string taskId);
        void updateICSItemReadyForUPI(string clearDate, string issuingBankCode, string returnType);
        void updateChequeType21ForUPI(string clearDate);
        DataTable PostedItemHistory(FormCollection collection);
        DataTable ReadyItemForLateReturnPostingHistory(FormCollection collection);
        DataTable ReadyItemForPostingHistory(FormCollection collection);
        bool GetLateMaintenaceUPI();
    }
}
