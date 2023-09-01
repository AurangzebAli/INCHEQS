using System.Data;
using System.Data.SqlClient;
using INCHEQS.Helpers;
//using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security;
using System.Collections.Generic;
using INCHEQS.Security.Account;
using System;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Models;
using INCHEQS.Common;
using INCHEQS.Resources;

namespace INCHEQS.Areas.OCS.Models.SystemCutOffTime
{
    public class SystemCutOffTimeDao : ISystemCutOffTimeDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;

        public SystemCutOffTimeDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }

        public void AddToSystemCutOffTimeTempToDelete(string desc)
        {
            string strQuerySelect = "Select fldSystemCutOffId, fldDesc, fldTransactionType from tblSystemCutOffSetting WHERE fldDesc = @fldDesc ";
            string strQueryInsert = "Insert into tblSystemCutOffSettingTemp (fldSystemCutOffId, fldDesc, fldTransactionType, fldBankCode, fldApprovalStatus) " +
                                    "VALUES (@fldSystemCutOffId, @fldDesc, @fldTransactionType, @fldBankCode, @fldApprovalStatus)";

            DataTable dtSystemCutOffTimeTemp = new DataTable();

            dtSystemCutOffTimeTemp = this.ocsdbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldDesc", desc)
            });

            if (dtSystemCutOffTimeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtSystemCutOffTimeTemp.Rows[0];
                dbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldSystemCutOffId", drItem["fldSystemCutOffId"]),
                    new SqlParameter("@fldDesc", desc),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode),
                    new SqlParameter("@fldApprovalStatus", "D")
                });
            }
        }

        public void AddToSystemCutOffTimeTempToUpdate(FormCollection col, AccountModel currentUser)
        {                
            string cutOffTimeStart = col["startHour"] + ":" + col["startMin"];
            string cutOffTimeEnd = col["endHour"] + ":" + col["endMin"];

            Dictionary<string, dynamic> sqlSystemCutOffTime = new Dictionary<string, dynamic>();
            sqlSystemCutOffTime.Add("fldTransactionType", col["fldTransactionType"]);

            sqlSystemCutOffTime.Add("fldRecurring", col["fldRecurring"] ?? "N");

            sqlSystemCutOffTime.Add("fldStartTime", cutOffTimeStart);
            sqlSystemCutOffTime.Add("fldEndTime", cutOffTimeEnd);

            sqlSystemCutOffTime.Add("fldDayMon", col["fldDayMon"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayTue", col["fldDayTue"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayWed", col["fldDayWed"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayThu", col["fldDayThu"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayFri", col["fldDayFri"] ?? "N");
            sqlSystemCutOffTime.Add("fldDaySat", col["fldDayFri"] ?? "N");
            sqlSystemCutOffTime.Add("fldDaySun", col["fldDayFri"] ?? "N");

            sqlSystemCutOffTime.Add("fldDesc", col["fldDesc"]);
            sqlSystemCutOffTime.Add("fldUpdateUserId", currentUser.UserId);
            sqlSystemCutOffTime.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlSystemCutOffTime.Add("fldApprovalStatus", "U");
            sqlSystemCutOffTime.Add("fldBankCode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblSystemCutOffSettingTemp", sqlSystemCutOffTime);
        }

        public bool CheckExist(string desc)
        {
            string stmt = "select * from tblSystemCutOffSetting where fldDesc=@fldDesc ";
            return ocsdbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldDesc", desc)
            });
        }

        public bool CheckPendingApproval(string desc)
        {
            string stmt = "SELECT * FROM tblSystemCutOffSettingTemp where fldDesc=@fldDesc ";
            return dbContext.CheckExist(stmt, new[] {
                new SqlParameter("@fldDesc", desc)
            });
        }

        public void CreateSystemCutOffTime(string desc)
        {
            string strQuerySelect = "select fldTransactionType, fldRecurring, fldStartTime, fldEndTime, fldDayMon, fldDayTue, fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldDesc, " +
                                    "fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp, fldBankCode from tblSystemCutOffSettingTemp where fldDesc=@fldDesc ";
            string strQueryInsert = "Insert into tblSystemCutOffSetting(fldTransactionType, fldRecurring, fldStartTime, fldEndTime, fldDayMon, fldDayTue, fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldDesc, " +
                                    "fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp) " +
                                    "VALUES (@fldTransactionType, @fldRecurring, @fldStartTime, @fldEndTime, @fldDayMon, @fldDayTue, @fldDayWed, @fldDayThu, @fldDayFri, @fldDaySat, @fldDaySun, @fldDesc, " +
                                    "@fldCreateUserId, @fldCreateTimeStamp, @fldUpdateUserId, @fldUpdateTimeStamp) ";

            DataTable dtSystemCutOffTimeTemp = new DataTable();

            dtSystemCutOffTimeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldDesc", desc)
            });

            if (dtSystemCutOffTimeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtSystemCutOffTimeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryInsert, new[] {
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldRecurring", drItem["fldRecurring"]),
                    new SqlParameter("@fldStartTime", drItem["fldStartTime"]),
                    new SqlParameter("@fldEndTime", drItem["fldEndTime"]),
                    new SqlParameter("@fldDayMon", drItem["fldDayMon"]),
                    new SqlParameter("@fldDayTue", drItem["fldDayTue"]),
                    new SqlParameter("@fldDayWed", drItem["fldDayWed"]),
                    new SqlParameter("@fldDayThu", drItem["fldDayThu"]),
                    new SqlParameter("@fldDayFri", drItem["fldDayFri"]),
                    new SqlParameter("@fldDaySat", drItem["fldDaySat"]),
                    new SqlParameter("@fldDaySun", drItem["fldDaySun"]),
                    new SqlParameter("@fldDesc", drItem["fldDesc"]),
                    new SqlParameter("@fldCreateUserId", drItem["fldCreateUserId"]),
                    new SqlParameter("@fldCreateTimeStamp", drItem["fldCreateTimeStamp"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }

        public void CreateSystemCutOffTimeInTemp(FormCollection col, AccountModel currentUser)
        {
            string cutOffTimeStart = col["startHour"] + ":" + col["startMin"];
            string cutOffTimeEnd = col["endHour"] + ":" + col["endMin"];

            Dictionary<string, dynamic> sqlSystemCutOffTime = new Dictionary<string, dynamic>();
            sqlSystemCutOffTime.Add("fldTransactionType", col["fldTransactionType"]);

            sqlSystemCutOffTime.Add("fldRecurring", col["fldRecurring"] ?? "N");

            sqlSystemCutOffTime.Add("fldStartTime", cutOffTimeStart);
            sqlSystemCutOffTime.Add("fldEndTime", cutOffTimeEnd);

            sqlSystemCutOffTime.Add("fldDayMon", col["fldDayMon"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayTue", col["fldDayTue"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayWed", col["fldDayWed"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayThu", col["fldDayThu"] ?? "N");
            sqlSystemCutOffTime.Add("fldDayFri", col["fldDayFri"] ?? "N");
            sqlSystemCutOffTime.Add("fldDaySat", col["fldDayFri"] ?? "N");
            sqlSystemCutOffTime.Add("fldDaySun", col["fldDayFri"] ?? "N");

            sqlSystemCutOffTime.Add("fldDesc", col["fldDesc"]);

            sqlSystemCutOffTime.Add("fldCreateUserId", currentUser.UserId);
            sqlSystemCutOffTime.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlSystemCutOffTime.Add("fldUpdateUserId", currentUser.UserId);
            sqlSystemCutOffTime.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlSystemCutOffTime.Add("fldApprovalStatus", "A");
            sqlSystemCutOffTime.Add("fldBankCode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblSystemCutOffSettingTemp", sqlSystemCutOffTime);
        }

        public void DeleteSystemCutOffTime(string desc)
        {
            string stmt = "delete from tblSystemCutOffSetting where fldDesc=@fldDesc ";

            ocsdbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldDesc",desc)
            });
        }

        public void DeleteInSystemCutOffTimeTemp(string desc)
        {
            string stmt = "delete from tblSystemCutOffSettingTemp where fldDesc=@fldDesc ";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldDesc", desc)
            });
        }               

        public DataTable Find(string desc)
        {
            DataTable ds = new DataTable();
            string stmt = "SELECT fldSystemCutOffId, fldTransactionType, fldRecurring, fldStartTime, fldEndTime, fldDayMon, fldDayTue, fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldDesc FROM tblSystemCutOffSetting WHERE fldDesc = @fldDesc";
            ds = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldDesc", desc) });

            return ds;
        }

        public async Task<DataTable> FindAsync(string desc)
        {
            return await Task.Run(() => Find(desc));
        }

        public void UpdateSystemCutOffTime(string desc)
        {
            string strQuerySelect = "select fldTransactionType, fldRecurring, fldStartTime, fldEndTime, fldDayMon, fldDayTue, fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldDesc, " +
                                    "fldUpdateUserId, fldUpdateTimeStamp from tblSystemCutOffSettingTemp where fldDesc=@fldDesc ";
            string strQueryUpdate = "update tblSystemCutOffSetting SET fldTransactionType=@fldTransactionType, " +
                                    "fldRecurring=@fldRecurring, fldStartTime=@fldStartTime, fldEndTime=@fldEndTime, fldDayMon=@fldDayMon, fldDayTue=@fldDayTue, fldDayWed=@fldDayWed, fldDayThu=@fldDayThu, fldDayFri=@fldDayFri, fldDaySat=@fldDaySat, fldDaySun=@fldDaySun, " +
                                    "fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId where fldDesc=@fldDesc ";

            DataTable dtSystemCutOffTimeTemp = new DataTable();

            dtSystemCutOffTimeTemp = this.dbContext.GetRecordsAsDataTable(strQuerySelect, new SqlParameter[] {
                new SqlParameter("@fldDesc", desc)
            });

            if (dtSystemCutOffTimeTemp.Rows.Count > 0)
            {
                DataRow drItem = dtSystemCutOffTimeTemp.Rows[0];

                ocsdbContext.ExecuteNonQuery(strQueryUpdate, new[] {
                    new SqlParameter("@fldDesc", desc),
                    new SqlParameter("@fldTransactionType", drItem["fldTransactionType"]),
                    new SqlParameter("@fldRecurring", drItem["fldRecurring"]),
                    new SqlParameter("@fldStartTime", drItem["fldStartTime"]),
                    new SqlParameter("@fldEndTime", drItem["fldEndTime"]),
                    new SqlParameter("@fldDayMon", drItem["fldDayMon"]),
                    new SqlParameter("@fldDayTue", drItem["fldDayTue"]),
                    new SqlParameter("@fldDayWed", drItem["fldDayWed"]),
                    new SqlParameter("@fldDayThu", drItem["fldDayThu"]),
                    new SqlParameter("@fldDayFri", drItem["fldDayFri"]),
                    new SqlParameter("@fldDaySat", drItem["fldDaySat"]),
                    new SqlParameter("@fldDaySun", drItem["fldDaySun"]),
                    new SqlParameter("@fldUpdateUserId", drItem["fldUpdateUserId"]),
                    new SqlParameter("@fldUpdateTimeStamp", drItem["fldUpdateTimeStamp"])
                });
            }
        }
        
        public List<String> ValidateCreate(FormCollection col)
        {
            //Pending Locale Registration

            List<String> err = new List<String>();

            string strRecurring = col["fldRecurring"] ?? "empty";
            string strRecurringType = col["RecurringType"] ?? "empty";
            string strDayMon = col["fldDayMon"] ?? "N";
            string strDayTue = col["fldDayTue"] ?? "N";
            string strDayWed = col["fldDayWed"] ?? "N";
            string strDayThu = col["fldDayThu"] ?? "N";
            string strDayFri = col["fldDayFri"] ?? "N";
            string strDaySat = col["fldDaySat"] ?? "N";
            string strDaySun = col["fldDaySun"] ?? "N";

            if (col["fldDesc"].Equals(""))
            {
                err.Add(Locale.SystemCutOffTimeDescriptionCannotBeEmpty);
            }
            else
            {
                if (this.CheckExist(col["fldDesc"]))
                {
                    err.Add(Locale.SystemCutOffTimeAlreadyExists);
                }
            }

            if (col["fldTransactionType"].Equals(null))
            {
                err.Add(Locale.SystemCutOffTimeSelectTransactionType);
            }           
            if (strRecurring.Equals("empty"))
            {
                err.Add(Locale.SystemCutOffTimeSelectScheduleType);
            }
            else
            {
                if (strRecurringType.Equals("empty"))
                {
                    err.Add(Locale.SystemCutOffTimeSelectRecurringType);
                }
                else
                {
                    if (strDayMon.Equals("N") 
                        & strDayTue.Equals("N") 
                        & strDayWed.Equals("N") 
                        & strDayThu.Equals("N") 
                        & strDayFri.Equals("N") 
                        & strDaySat.Equals("N") 
                        & strDaySun.Equals("N")) {
                        err.Add(Locale.SystemCutOffTimeSelectDay);
                    }
                }
            }
            if (this.CheckPendingApproval(col["fldDesc"]))
            {                
                err.Add(Locale.SystemCutOffTimePendingApproval);
            }
            
            return err;
        }

        public List<String> ValidateUpdate(FormCollection col)
        {
            //Pending Locale Registration

            List<String> err = new List<String>();

            string strRecurring = col["fldRecurring"] ?? "empty";
            string strRecurringType = col["RecurringType"] ?? "empty";
            string strDayMon = col["fldDayMon"] ?? "N";
            string strDayTue = col["fldDayTue"] ?? "N";
            string strDayWed = col["fldDayWed"] ?? "N";
            string strDayThu = col["fldDayThu"] ?? "N";
            string strDayFri = col["fldDayFri"] ?? "N";
            string strDaySat = col["fldDaySat"] ?? "N";
            string strDaySun = col["fldDaySun"] ?? "N";

            if (col["fldTransactionType"].Equals(null))
            {
                err.Add(Locale.SystemCutOffTimeSelectTransactionType);
            }
            if (strRecurring.Equals("empty"))
            {
                err.Add(Locale.SystemCutOffTimeSelectScheduleType);
            }
            else
            {
                if (strRecurringType.Equals("empty"))
                {
                    err.Add(Locale.SystemCutOffTimeSelectRecurringType);
                }
                else
                {
                    if (strDayMon.Equals("N")
                        & strDayTue.Equals("N")
                        & strDayWed.Equals("N")
                        & strDayThu.Equals("N")
                        & strDayFri.Equals("N")
                        & strDaySat.Equals("N")
                        & strDaySun.Equals("N"))
                    {
                        err.Add(Locale.SystemCutOffTimeSelectDay);
                    }
                }
            }
            if (this.CheckPendingApproval(col["fldDesc"]))
            {
                err.Add(Locale.SystemCutOffTimePendingApproval);
            }

            return err;
        }
    }
}
