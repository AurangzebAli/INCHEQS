using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.AIFImport
{
    public interface IAIFImportDao
    {
        AIFImportModel GetDataFromHostFileConfig(string taskId,string bankcode);
        List<AIFImportModel> GetDataFromFileMaster();
        Task<List<AIFImportModel>> GetDataFromFileMasterAsync();
        List<String> ValidateCheckBox(FormCollection col);
    }
}