using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.Account;

namespace INCHEQS.Areas.PPS.Models.PositivePayMatching
{
    public interface IPositivePayMatchingDao
    {
        PositivePayMatchingModel getMatchingCount(string bankCode);
        PositivePayMatchingModel getMatchingResult();
    }
}