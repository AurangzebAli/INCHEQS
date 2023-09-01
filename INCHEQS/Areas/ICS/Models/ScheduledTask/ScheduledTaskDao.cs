using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.ScheduledTask {
    public class ScheduledTaskDao: IScheduledTaskDao {
        private readonly ApplicationDbContext dbContext;
        public ScheduledTaskDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable GetScheduledTaskName() {
            DataTable ds = new DataTable();

            string stmt = "SELECT fldScheduledTaskID,fldScheduledTaskName FROM tblScheduledTask";
            //SELECT fldScheduledTaskID,fldScheduledTaskName FROM tblScheduledTask WHERE fldIsShowed = 'True' //from MayBank
            ds = dbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }

        public DataTable GetScheduledTimer() {
            DataTable ds = new DataTable();

            string stmt = "SELECT fldScheduledTimeID, fldScheduledTaskName, fldScheduledHours, fldScheduledMins, (CASE isnull(fldLastRunTimeStamp,'') WHEN '' THEN '--' ELSE convert(varchar, fldLastRunTimeStamp, 120) END) as RunTime, fldScheduledTimeID FROM tblscheduledTimer ORDER by fldScheduledTaskName, fldScheduledHours, fldScheduledMins";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }

        public DataTable GetScheduledHistory(string scheduledTaskName) {
            DataTable ds = new DataTable();

            string stmt = "SELECT substring(convert(varchar,fldClearDate,120),1,10) as fldClearDate, convert(varchar,fldCreateTimeStamp,120) as fldCreateTimeStamp, fldRemarks  FROM tblScheduledHistory WHERE fldScheduledTaskName = @fldScheduledTaskName";

            ds = dbContext.GetRecordsAsDataTable(stmt,new[] { new SqlParameter("@fldScheduledTaskName", scheduledTaskName) });
            return ds;
        }

        public void InsertScheduledTaskTimer(FormCollection col,AccountModel currentUser) {

            string stmt = "INSERT INTO tblScheduledTimer (fldScheduledTaskName, fldScheduledProcessName, fldScheduledPosPayType, fldScheduledHours, fldScheduledMins, fldCreateUserId, fldCreateTimeStamp, fldLastRunTimeStamp)"+ 
              "SELECT Top 1 fldScheduledTaskName, fldProcessName, fldPosPayType, @fldScheduledHours, @fldScheduledMins, @fldCreateUserId, @fldCreateTimeStamp, NULL FROM tblScheduledTask WHERE fldScheduledTaskID = @fldScheduledTaskName";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldScheduledHours", col["hour"]),
                new SqlParameter("@fldScheduledMins", col["min"]),
                new SqlParameter("@fldCreateUserId", currentUser.UserId),
                new SqlParameter("@fldCreateTimeStamp", DateUtils.GetCurrentDatetime()),
                new SqlParameter("@fldScheduledTaskName", col["processName"]),
            });       
        }

        public void DeleteScheduledTask(string schedulId) {

            string stmt = "DELETE  FROM [tblScheduledTimer] WHERE [fldScheduledTimeID] = @fldScheduledTimeID";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldScheduledTimeID", schedulId) });
        }
    }
}