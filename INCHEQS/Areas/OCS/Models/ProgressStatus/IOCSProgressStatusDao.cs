using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCHEQS.Areas.OCS.Models.ProgressStatus
{
   public interface IOCSProgressStatusDao
    {
        List<OCSProgressStatus> ReturnProgressStatus();
        DataTable GetCapturingModeDataTable();
        string GetUserType();

    }
}
