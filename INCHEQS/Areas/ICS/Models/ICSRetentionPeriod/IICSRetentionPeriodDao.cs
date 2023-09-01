using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ICSRetentionPeriod
{
    public interface IICSRetentionPeriodDao {

        ICSRetentionPeriodModel GetICSRetentionPeriod();
        ICSRetentionPeriodModel GetICSRetentionPeriodTemp();
        bool CheckMaxSession(string timeOut);
        List<string> ValidateICSRetentionPeriod(FormCollection col);
        string GetValueFromICSRetentionPeriodMaster(string retentionPeriodType, string type);
        string GetValueFromICSRetentionPeriodTemp(string retentionPeriodType, string type);
        void UpdateICSRetentionPeriod(FormCollection col);
        void CreateICSRetentionMasterTemp(FormCollection col);
        void CreateICSRetentionPeriodChecker(string ICSRetentionPeriod, string Update, string currentUser);
        void DeleteICSRetentionPeriodMaster();
        void MovetoICSRetentionPeriodMasterfromTemp();
        void DeleteICSRetentionPeriodChecker();
        void DeleteICSRetentionPeriodTemp();
        bool CheckICSRetentionPeriodTemp();
        List<ICSRetentionPeriodModel> ListIntervalType();
    }
}