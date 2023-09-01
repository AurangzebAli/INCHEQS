using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;
namespace INCHEQS.Areas.PPS.Models.LoadPPSFile
{
    public interface ILoadPPSFileDao
    {
        
        string LoadPPSFilePath();
        
    }
}