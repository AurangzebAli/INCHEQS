using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.StateCode
{
    public interface IStateCodeDao
    {
        StateCodeModel GetStateCode(string StateCode);
        StateCodeModel GetStateCodeTemp(string StateCode);
        List<string> ValidateStateCode(FormCollection col, string action);
        bool UpdateStateCodeMaster(FormCollection col);
        void CreateStateCodeMasterTemp(FormCollection col, string crtUser, string StateCode, string Action);
        bool DeleteStateCodeMaster(string stateCode);
        bool CheckStateCodeMasterTempById(string stateCode);
        bool CreateStateCodeMaster(FormCollection col);
        void MoveToStateCodeMasterFromTemp(string stateCode, string Action);
        bool DeleteStateCodeMasterTemp(string stateCode);
    }
}
