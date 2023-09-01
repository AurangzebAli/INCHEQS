using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.ReturnCode
{
    public interface IReturnCodeDao
    {
        ReturnCodeModel GetReturnCode(string ReturnCode);
        ReturnCodeModel GetReturnCodeTemp(string ReturnCode);
        List<string> ValidateReturnCode(FormCollection col, string action);
        bool UpdateReturnCodeMaster(FormCollection col);
        void CreateReturnCodeMasterTemp(FormCollection col, string returnCode, string crtUser, string Action);
        bool DeleteReturnCodeMaster(string ReturnCode);
        bool CheckReturnCodeMasterTempById(string ReturnCode);
        bool CreateReturnCodeMaster(FormCollection col);
        void MoveToReturnCodeMasterFromTemp(string ReturnCode, string Action);
        bool DeleteReturnCodeMasterTemp(string ReturnCode);
        List<ReturnCodeModel> ListRejectType();
        List<ReturnCodeModel> ListRejectTypeForBranch();
        bool CheckValidateReturnCode(string returnCode);
        bool CheckValidateInternalReturnCode(string returnCode);
        void DeleteInRejectMasterTemp(string returnCode);
        List<ReturnCodeModel> FindAllRejectCodesDictionary();
        List<ReturnCodeModel> FindRejectCodesForBranchDictionary();
        Task<DataTable> GetAllRejectCodesAsync();
        Task<DataTable> GetAllRejectCodesWithoutInternalAsync();
        DataTable GetAllRejectCodesWithoutInternal();
        DataTable GetAllRejectCodes();
        DataTable Find(string RejectCode);
        DataTable FindWithoutInternalCode(string RejectCode);
        string getCharges(string rejectCode);
    }
}
