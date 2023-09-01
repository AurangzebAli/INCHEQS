using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.InternalBranchKBZ
{
    public interface IInternalBranchKBZDao
    {
        void AddToMapBranchTempToDelete(string branchCode);

        void CreateInInternalBranch(string branchCode);

        //void CreateInternalBranchTemp(FormCollection col, string currentBankCode);

        void DeleteInInternalBranch(string deleteids);

        void DeleteInInternalBranchTemp(string branchCode);

        InternalBranchKBZModel FindInternalBranchCode(string conBranchCode);

        DataTable GetInternalBranchDataTemp(string id);

        //void UpdateInternalBranch(FormCollection col);

        //List<string> ValidateCreate(FormCollection col, string bankCode);

        List<string> ValidateEdit(FormCollection col);

        DataTable GetInternalBranchData(string id);
        bool CreateInternalBranch(FormCollection col);
        bool CreateInternalBranchTemp(FormCollection col,String action);
        List<string> ValidateCreate(FormCollection col);
        bool CheckInternalBranchById(string id);
        bool CheckInternalBranchTempById(string id);
        //InternalBranchModel GetInternalBranchDataById(string id);
        List<string> ValidateUpdate(FormCollection col);
        bool UpdateInternalBranch(FormCollection col);
        bool DeleteInternalBranch(string id);
        bool CreateInternalBranchTempToDelete(String id);
        bool MoveToInternalBranchFromTemp(String id);
        bool UpdateInternalBranchById(string id);
        bool DeleteInternalBranchTemp(string id);
        List<InternalBranchKBZModel> ListInternalBranch(string userId);
        List<InternalBranchKBZModel> ListCountry();
        List<InternalBranchKBZModel> ListBankZone();
    }
}