using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.ICSReleaseLockedCheque
{
    public interface ICSIReleaseLockedChequeDao
    {
        void DeleteProcessUsingCheckbox(string dataProcess);
        DataTable ListLockedCheque(string bankCode);
        void UpdateReleaseLockedCheque(string InwardItemId);
    }
}