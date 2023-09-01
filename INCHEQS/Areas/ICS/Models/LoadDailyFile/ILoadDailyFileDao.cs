﻿using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
namespace INCHEQS.Areas.ICS.Models.LoadDailyFile
{
    public interface ILoadDailyFileDao
    {
        //List<LoadDailyFileModel> GetItemStatusListing(string BankCode, string cleardate, string status);
        DataTable GetErrorListFromICLException(string clearDate);
        LoadDailyFileModel GetDataFromLoadDailyFileConfig(string taskId, string bankcode, string foldername);
        void Update(string bankcode);
        //DataTable GetInwardReturnFileRecordWithStatus(string BankCode, string cleardate, string status);
        //DataTable GetInwardReturnItemForMatching(string BankCode, string cleardate, string status);
        //DataTable GetMatchItemID(string uic);
        //void UpdateInwardReturnItem(string InwardReturnItemId, string InwardReturnFileId, string ClearingDate, string cleardate, string UIC, string PresentingBankType, string PresentingBankCode,
        //string PresentingStateCode, string PresentingBranchCode, string CheckDigit, string Serial, string PayingBankcCode,
        //string PayingStateCode, string PayingBranchCode, string AccNo, string TCCode, string Amount, string ReturnCode, string ReturnCount, string ChequeType, string ReturnReason,
        //string ItemInitialId, string MatchFlag, string IRDFlag, string IRDGenFlag, string IRDPringFlag, string CreateUserId,
        //string CreateTimeStamp, string UpdateUserId, string UpdateTimeStamp, string IssuingBankType, string IssuingBankCode, string IssuingBankStateCode,
        //string IssuingBankBranchCode, string IQA, string NCFlag, string ImageIndicator, string DocumentToFollow, string Reason, string DSVeriStatus, string ForUIC, string ForFileId);
        //void PerformMatching(string UserId, string BankCode, string cleardate, string status);
        //void GenerateNewBatches(string bankcode, string intUserId, string processdate);
        //LoadDailyFileModel GetMICRStatus(string bankCode, string currentProcess, int ItemBatch);
        //List<string> GetInwardStatus(string bankCode, string currentProcess, int ItemBatch);

        //void updateMICRStatusProcessTime(string MICRBatch);
        //void updateMICRStatusCompleted(string MICRBatch);
        //void updateMICRStatusFail(string MICRBatch);
        //LoadDailyFileModel ListAvailableMICRStatusItem(string BankCode);

        //void InsertFileList(string fileName, string fileSize, string fileTimeStamp);

        DataTable LoadDailyFileItemList(string id, string type);
        string ListFolderPathTo(string foldername);
        string ListFolderPathToCompleted();
        string ListFolderPathFrom(string foldername, string clearDate);
        void run(string bankcode, string userid, string processDate, string processName, string fileType);


    }
}