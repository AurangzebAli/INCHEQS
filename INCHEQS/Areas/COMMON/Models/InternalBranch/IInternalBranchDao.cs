using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.InternalBranch
{
    public interface IInternalBranchDao
    {
        void AddToMapBranchTempToDelete(string branchCode);

        void CreateInInternalBranch(string branchCode);

        //void CreateInternalBranchTemp(FormCollection col, string currentBankCode);

        void DeleteInInternalBranch(string deleteids);

        void DeleteInInternalBranchTemp(string branchCode);

        InternalBranchModel FindInternalBranchCode(string conBranchCode);

        DataTable GetInternalBranchDataTemp(string id);

        //void UpdateInternalBranch(FormCollection col);

        //List<string> ValidateCreate(FormCollection col, string bankCode);

        List<string> ValidateEdit(FormCollection col);
        string GetInternalBranchID();
        DataTable GetInternalBranchData(string id);
        bool CreateInternalBranch(FormCollection col);
        bool CreateInternalBranchTemp(FormCollection col,String action);
        List<string> ValidateCreate(FormCollection col);
        bool CheckInternalBranchByIdandBranchId(string id, string branchId);
        bool CheckInternalBranchTempById(string id);
        //InternalBranchModel GetInternalBranchDataById(string id);
        List<string> ValidateUpdate(FormCollection col);
        bool UpdateInternalBranch(FormCollection col);
        bool DeleteInternalBranch(string id);
        bool CreateInternalBranchTempToDelete(String id);
        bool MoveToInternalBranchFromTemp(String id);
        bool UpdateInternalBranchById(string id);
        bool DeleteInternalBranchTemp(string id);
        List<InternalBranchModel> ListInternalBranch(string bankType);
        List<InternalBranchModel> ListCountry();
        List<InternalBranchModel> ListBankZone();
        bool CheckBranchMasterBankTypeById(string id, string bankType);
        bool CheckBankType(string bankcode, string banktypeid);
        bool CheckRelatedBank(string bankcode);
        bool CheckInternalBranchByBranchId(string id);
        bool CheckInternalBranchTempByBranchId(string id);
        bool CheckRationalBranchCollapse(string id, string branchType);
    }
}