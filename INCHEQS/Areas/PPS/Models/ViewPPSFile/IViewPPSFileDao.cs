using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.PPS.Models.ViewPPSFile
{
    public interface IViewPPSFileDao
    {
        ViewPPSFileModel getPayeeDetail(string chequeNo, string accountNo);
    }
}