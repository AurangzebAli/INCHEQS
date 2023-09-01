using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
namespace INCHEQS.Areas.ICS.Models.LoadBankHostFile
{
    public interface ILoadBankHostFileDao
    {
        LoadBankHostFileModel GetDataFromMICRImportConfig(string taskId, string bankcode);
        void Update(string bankcode);

        
        DataTable InwardItemList(FormCollection collection);
        string ListFolderPathTo();
        string ListFolderPathFrom();

        List<LoadBankHostFileModel> ReturnProgressStatusConventional(string clearDateDD);
        List<LoadBankHostFileModel> ReturnProgressStatusIslamic(string clearDateDD);
    }
}