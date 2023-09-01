using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ScheduledTask {
    public interface IScheduledTaskDao {

        DataTable GetScheduledTaskName();
        void InsertScheduledTaskTimer(FormCollection col, AccountModel currentUser);
        DataTable GetScheduledTimer();
        void DeleteScheduledTask(string schedulId);
        DataTable GetScheduledHistory(string scheduledTaskName);
    }
}