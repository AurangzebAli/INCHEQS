using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
namespace INCHEQS.Areas.ICS.Models.LoadBankHostFile2nd
{
    public interface ILoadBankHostFile2ndDao
    {
        LoadBankHostFile2ndModel GetDataFromMICRImportConfig(string taskId, string bankcode);
        void Update(string bankcode);

        
        DataTable InwardItemList(FormCollection collection);
        string ListFolderPathTo();
        string ListFolderPathFrom();

        List<LoadBankHostFile2ndModel> GetLoadBankHostSummary(string clearDateDD);

        string GetFTPUserName();
        string GetFTPPassword();

        bool DownloadFile(string url, string userid, string password, string fileDestPath, string FileName, ref string Message);
        string GetInterfaceFileName(string interfaceFile);
    }
}