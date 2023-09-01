using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.ICS.Models.Truncation
{
    public interface ITruncationDao
    {
        TruncationModel GetTotalRec(string fldClearDate);
        TruncationModel GetTotPending(string fldClearDate);
        List<TruncationModel> CalCulateRecord(string fldClearDate, string fldTop);
        void Truncate(string fldClearDate, string sTruncateMin, string sTruncateMax);
        string GetATVUser();
        string getMaxAmount(string fldClearDate);
    }
}