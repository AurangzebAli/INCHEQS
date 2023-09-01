using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
namespace INCHEQS.Areas.ICS.Models.LoadNcf
{
    public interface ILoadNcfDao
    {
        LoadNcfModel GetDataFromMICRImportConfig(string taskId, string bankcode);
        void Update(string bankcode);

        
        DataTable InwardItemList(FormCollection collection);
        string ListFolderPathTo();
        string ListFolderPathFrom();


    }
}