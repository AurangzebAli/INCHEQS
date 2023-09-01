using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Models.EnableUserID
{
    public interface IEnableUserIDDao
    {
        EnableUserIDModel GetEnableUserID(string UserID);
        bool UpdateEnableUserID(string UserId);
    }
}
