﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.GenerateRepairedDebitFile
{
    public interface IGenerateRepairedDebitFileDao
    {
        DataTable GetHubBranches(string userId);
        void GenerateNewBatches(string bankcode, string processname, string fileext, string intUserId, string processdate, string Totalitems, string TotalAmount);
        void ReGenerateBatches(string bankcode,string previousbatch, string processname, string fileext, string intUserId, string processdate, string Totalitems, string TotalAmount);
        DataTable PostedItemHistory(FormCollection collection);
        DataTable ReadyItemForPostingHistory(FormCollection collection);
        string getBetween(string strSource, string strStart, string strEnd);

        GenerateRepairedDebitFileModel GetRepairedDebitFilePath(string processname, string bankcode);

        string GetFTPUserName();

        string GetFTPPassword();

        void UpdateDebitFileUploaded(FormCollection col, string uploadStatus);

        void UpdateDataProcess(string status, string processName, string remarks, string errMessage, string oriStatus, string bankCode);
    }
}
