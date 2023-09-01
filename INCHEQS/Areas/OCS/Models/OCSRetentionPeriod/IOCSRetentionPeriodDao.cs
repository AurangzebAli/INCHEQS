using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Models.OCSRetentionPeriod
{
    public interface IOCSRetentionPeriodDao {

        OCSRetentionPeriodModel GetOCSRetentionPeriod();
        OCSRetentionPeriodModel GetOCSRetentionPeriodTemp();
        bool CheckMaxSession(string timeOut);
        List<string> ValidateOCSRetentionPeriod(FormCollection col);
        string GetValueFromOCSRetentionPeriodMaster(string retentionPeriodType, string type);
        string GetValueFromOCSRetentionPeriodTemp(string retentionPeriodType, string type);
        void UpdateOCSRetentionPeriod(FormCollection col);
        void CreateOCSRetentionMasterTemp(FormCollection col);
        void CreateOCSRetentionPeriodChecker(string OCSRetentionPeriod, string Update, string currentUser);
        void DeleteOCSRetentionPeriodMaster();
        void MovetoOCSRetentionPeriodMasterfromTemp();
        void DeleteOCSRetentionPeriodChecker();
        void DeleteOCSRetentionPeriodTemp();
        bool CheckOCSRetentionPeriodTemp();
        List<OCSRetentionPeriodModel> ListIntervalType();
    }
}