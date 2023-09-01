using INCHEQS.Models;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Areas.COMMON.Models.Holiday
{
    public class HolidayDao : iHolidayDao
    {
        private readonly ApplicationDbContext dbContext;
        public HolidayDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public SqlDbType HolidayId { get; private set; }
        public List<string> ValidateHoliday(FormCollection col, string action)
        {
            List<string> err = new List<string>();
            if ((col["fldHolidayDescription"].Equals("")))
            {
                err.Add(Locale.HolidayDescriptionCannotBlank);
            }
            if (!col.AllKeys.Contains("RecurringType") && col["OneTime"].ToString() != "OneTime")
            {
                err.Add("Please select the Recurring type");
            }
            if (col.AllKeys.Contains("RecurringType"))
            {
                if (col["RecurringType"].ToString() == "Week" && !col.AllKeys.Contains("Days"))
                {
                    err.Add("Please select Day Name");
                }
            }
            if (col["fldDate"] != null)
            {
            if (this.checkDateExist(col["fldDate"]))
            {
                err.Add("Holiday Date already exist");
            }
            if (this.checkDateExistTemp(col["fldDate"]))
            {
                err.Add("Holiday Date already exist");
            }
            }
            if (col["fldYearDate"] != null)
            {
                if (this.checkDateExist(col["fldYearDate"]))
                {
                    err.Add("Holiday Date already exist");
                }
                if (this.checkDateExistTemp(col["fldYearDate"]))
                {
                    err.Add("Holiday Date already exist");
                }
            } 
            
            return err;
        }
        public async Task<DataTable> FindAsync(string HolidayId)
        {
            return await Task.Run(() => Find(HolidayId));
        }
        public DataTable Find(string RejectCode)
        {

            DataTable ds = new DataTable();

            string stmt = "Select * FROM  tblHolidayMaster WHERE fldHolidayId = @HolidayId";

            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@HolidayId", HolidayId) });

            return ds;
        }

        public bool checkDateExist(string date)
        {
            bool flag = false;
            DataTable DT = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@HolidayDate", date));
            SqlParameterNext.Add(new SqlParameter("@Action","M"));
            DT = dbContext.GetRecordsAsDataTableSP("spcgDuplicateHoliday", SqlParameterNext.ToArray());
            if (DT.Rows.Count > 0)
            {
                flag = true;
            }
            //DateUtils.formatDateFromSql(date))

            //return this.dbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldHolidayDate", DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd")) });
            return flag;
        }

        public bool checkDateExistTemp(string date)
        {
            bool flag = false;
            DataTable DT = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@HolidayDate", date));
            SqlParameterNext.Add(new SqlParameter("@Action", "T"));
            DT = dbContext.GetRecordsAsDataTableSP("spcgDuplicateHoliday", SqlParameterNext.ToArray());
            if (DT.Rows.Count > 0)
            {
                flag = true;
            }
            return flag;
            //return this.dbContext.CheckExist(str, new SqlParameter[] { new SqlParameter("@fldHolidayDate", Convert.ToDateTime(date).ToString("yyyy-MM-dd")) });
        }

        public HolidayModel InsertHoliday(FormCollection col)
        {
            dynamic bankCode = CurrentUser.Account.BankCode;
            HolidayModel holiday = new HolidayModel();
            string Recurring = "";
            string RecurringType = "";
            string RecurringDay = "";
            string RecurringMonth = "0";
            string getDate = "";
            string mon = "N";
            string tue = "N";
            string wed = "N";
            string thu = "N";
            string fri = "N";
            string sat = "N";
            string sun = "N";
            string getDateDb = "";
            //string IsActiveHoliday = "";
            try
            {
                string OneTime = col["OneTime"].ToString();
                //if (!col.AllKeys.Contains("active"))
                //{
                //    IsActiveHoliday = "N";
                //}
                //else
                //{
                //    IsActiveHoliday = col["active"];
                //}
                if (OneTime == "OneTime")
                {
                    Recurring = "N";
                    getDate = col["fldDate"].ToString();
                }
                else
                {
                    Recurring = "Y";
                    RecurringType = col["RecurringType"].ToString();
                    if (RecurringType == "Week")
                    {
                        RecurringType = "W";
                        //RecurringDay = col["Days"].ToString();
                        string[] DayArray = (col["Days"].ToString()).Split(',');
                        foreach (string dayArrayList in DayArray)
                        {
                            if (dayArrayList == "1")
                            {
                                mon = "Y";
                            }
                            if (dayArrayList == "2")
                            {
                                tue = "Y";
                            }
                            if (dayArrayList == "3")
                            {
                                wed = "Y";
                            }
                            if (dayArrayList == "4")
                            {
                                thu = "Y";
                            }
                            if (dayArrayList == "5")
                            {
                                fri = "Y";
                            }
                            if (dayArrayList == "6")
                            {
                                sat = "Y";
                            }
                            if (dayArrayList == "7")
                            {
                                sun = "Y";
                            }
                        }
                    }
                    if (RecurringType == "Month")
                    {
                        RecurringType = "M";
                        RecurringMonth = col["dropdownWeek"].ToString();
                        RecurringDay = col["dropdownDay"].ToString();
                        if (RecurringDay == "1")
                        {
                            mon = "Y";
                        }
                        if (RecurringDay == "2")
                        {
                            tue = "Y";
                        }
                        if (RecurringDay == "3")
                        {
                            wed = "Y";
                        }
                        if (RecurringDay == "4")
                        {
                            thu = "Y";
                        }
                        if (RecurringDay == "5")
                        {
                            fri = "Y";
                        }
                        if (RecurringDay == "6")
                        {
                            sat = "Y";
                        }
                        if (RecurringDay == "7")
                        {
                            sun = "Y";
                        }
                    }
                    if (RecurringType == "Year")
                    {
                        RecurringType = "Y";
                        getDate = col["fldYearDate"].ToString();
                    }
                }
                if (col["OneTime"].ToString() == "OneTime" || RecurringType == "Y")
                {
                    getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    //getDateDb = Convert.ToDateTime(getDate).ToString("yyyy-MM-dd");
                }

                int intRowAffected;
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@TableName", "tblHolidayMaster"));
                sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
                sqlParameterNext[1].Direction = ParameterDirection.Output;
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
                long iNextNo = (long)sqlParameterNext[1].Value;


                string daf = DateTime.Now.ToString();
                //string stmt = "Insert into tblHolidayMaster " +
                //"(fldHolidayId,fldRecurring,fldRecurrType,fldHolidayDate,fldHolidayDesc,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId," +
                //"fldUpdateTimeStamp,fldDayMon,fldDayTue,fldDayWed,fldDayThu,fldDayFri,fldDaySat,fldDaySun)" +
                //"VALUES (@fldHolidayId,@fldRecurring,@fldRecurringType,@fldHolidayDate,@fldHolidayDesc,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId," +
                //"@fldUpdateTimeStamp,@fldDayMon,@fldDayTue,@fldDayWed,@fldDayThu,@fldDayFri,@fldDaySat,@fldDaySun)";
                //dbContext.ExecuteNonQuery(stmt, new[] {
                //    new SqlParameter("@fldHolidayId", iNextNo),
                //    new SqlParameter("@fldRecurring", Recurring),
                //    new SqlParameter("@fldRecurringType", RecurringType),
                //    //new SqlParameter("@RecurringMonth",RecurringMonth),
                //    new SqlParameter("@fldHolidayDate", getDateDb),
                //    new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]),
                //    new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId),
                //    new SqlParameter("@fldCreateTimeStamp", DateTime.Now),
                //    new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                //    new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                //    new SqlParameter("@fldDayMon", mon),
                //    new SqlParameter("@fldDayTue", tue),
                //    new SqlParameter("@fldDayWed", wed),
                //    new SqlParameter("@fldDayThu", thu),
                //    new SqlParameter("@fldDayFri", fri),
                //    new SqlParameter("@fldDaySat", sat),
                //    new SqlParameter("@fldDaySun", sun)
                //    //new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)

                List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                sqlParameterNext1.Add(new SqlParameter("@fldHolidayId", iNextNo));
                sqlParameterNext1.Add(new SqlParameter("@fldRecurring", Recurring));
                sqlParameterNext1.Add(new SqlParameter("@fldRecurringType", RecurringType));
                sqlParameterNext1.Add(new SqlParameter("@fldHolidayDate", getDateDb));
                sqlParameterNext1.Add(new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]));
                sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext1.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext1.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
                sqlParameterNext1.Add(new SqlParameter("@fldDayMon", mon));
                sqlParameterNext1.Add(new SqlParameter("@fldDayTue", tue));
                sqlParameterNext1.Add(new SqlParameter("@fldDayWed", wed));
                sqlParameterNext1.Add(new SqlParameter("@fldDayThu", thu));
                sqlParameterNext1.Add(new SqlParameter("@fldDayFri", fri));
                sqlParameterNext1.Add(new SqlParameter("@fldDaySat", sat));
                sqlParameterNext1.Add(new SqlParameter("@fldDaySun", sun));
                intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHolidayMaster", sqlParameterNext1.ToArray());
                return holiday;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public bool CheckHolidayExist(string id)
        {
            DataTable ds = new DataTable();
            bool Flag = false;
            string stmt = "Select * from tblHolidayMasterTemp where fldHolidayId=@fldHolidayId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldHolidayId", id) });
            if (ds.Rows.Count > 0)
            {
                Flag = true;
            }
            return Flag;
        }
        public HolidayModel GetHolidayData(string id)
        {
            HolidayModel holiday = new HolidayModel();
            DataTable ds = new DataTable();
            string stmt = "Select * from view_Holiday where fldHolidayId=@fldHolidayId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldHolidayId", id) });
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                if (row["fldHolidayDate"].ToString() == "")
                {
                    holiday.Date = "";
                }
                else
                {
                    holiday.Date = row["fldHolidayDate"].ToString();
                    //holiday.Date = DateTime.ParseExact(row["fldHolidayDate"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd-MM-yyyy");
                }
                holiday.Desc = row["fldHolidayDesc"].ToString();
                holiday.recurring = row["fldRecurring"].ToString();
                //holiday.fldActiveHoliday = row["fldActiveHoliday"].ToString();
                //if (row["fldActiveHoliday"].ToString() == "Yes")
                //{
                //    holiday.Activecheck = true;
                //    holiday.fldActiveHoliday = "Y";
                //}
                //else
                //{
                //    holiday.Activecheck = false;
                //    holiday.fldActiveHoliday = "N";
                //}
                if (holiday.recurring == "Yes")
                {
                    holiday.recurring = "Recurring";
                    holiday.recurrType = row["fldRecurrType"].ToString();
                    if (holiday.recurrType == "Month")
                    {
                        holiday.recurrType = "Month";
                    }
                    if (holiday.recurrType == "Week")
                    {
                        holiday.recurrType = "Week";

                        if (row["fldDayMon"].ToString() == "Y")
                        {
                            holiday.fldDayMon = "Y";
                            holiday.MonChecked = true;
                        }
                        else
                        {
                            holiday.fldDayMon = "N";
                            holiday.MonChecked = false;
                        }
                        if (row["fldDayTue"].ToString() == "Y")
                        {
                            holiday.fldDayTue = "Y";
                            holiday.tueChecked = true;
                        }
                        else
                        {
                            holiday.fldDayTue = "N";
                            holiday.tueChecked = false;
                        }
                        if (row["fldDayWed"].ToString() == "Y")
                        {
                            holiday.fldDayWed = "Y";
                            holiday.wedChecked = true;
                        }
                        else
                        {
                            holiday.fldDayWed = "N";
                            holiday.wedChecked = false;
                        }
                        if (row["fldDayThu"].ToString() == "Y")
                        {
                            holiday.fldDayThu = "Y";
                            holiday.thuChecked = true;
                        }
                        else
                        {
                            holiday.fldDayThu = "N";
                            holiday.thuChecked = false;
                        }
                        if (row["fldDayFri"].ToString() == "Y")
                        {
                            holiday.fldDayFri = "Y";
                            holiday.friChecked = true;
                        }
                        else
                        {
                            holiday.fldDayFri = "N";
                            holiday.friChecked = false;
                        }
                        if (row["fldDaySat"].ToString() == "Y")
                        {
                            holiday.fldDaySat = "Y";
                            holiday.satChecked= true;
                        }
                        else
                        {
                            holiday.fldDaySat = "N";
                            holiday.satChecked = false;
                        }
                        if (row["fldDaySun"].ToString() == "Y")
                        {
                            holiday.fldDaySun = "Y";
                            holiday.sunChecked = true;
                        }
                        else
                        {
                            holiday.fldDaySun = "N";
                            holiday.sunChecked = false;
                        }
                    }
                    if (holiday.recurrType == "Year")
                    {
                        holiday.recurrType = "Year";
                        holiday.recurrTypeYearly = "True";
                    }
                }
                else
                {
                    holiday.recurring = "N";
                    holiday.recurrType = "OneTime";
                }

                holiday.HolidayId = row["fldHolidayId"].ToString();
            }
            return holiday;
        }
        public void UpdateHolidayTable(FormCollection col)
        {
            //string IsActiveHoliday = "";
            //if (!col.AllKeys.Contains("active"))
            //{
            //    IsActiveHoliday = "N";
            //}
            //else
            //{
            //    IsActiveHoliday = col["active"];
            //}
            //string stmt = "UPDATE tblHolidayMaster SET fldHolidayDesc=@fldHolidayDesc,fldUpdateTimestamp=@fldUpdateTimestamp,fldUpdateUserId=@fldUpdateUserId Where fldHolidayId=@fldHolidayId";
            //dbContext.ExecuteNonQuery(stmt, new[] {
            //                new SqlParameter("@fldHolidayId", col["HolidayId"]),
            //                new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]),
            //                new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
            //                new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", col["HolidayId"]));
            sqlParameterNext.Add(new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@tblName", ""));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHolidayMasterTemp", sqlParameterNext.ToArray());

        }
        public void DeleteFromHolidayTable(string id)
        {
            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId",id));
            sqlParameterNext.Add(new SqlParameter("@fldHolidayDate", ""));
            sqlParameterNext.Add(new SqlParameter("@Count", ""));
            sqlParameterNext.Add(new SqlParameter("@tblName", ""));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHolidayMasterTemp", sqlParameterNext.ToArray());
            //string stmt = "delete from tblHolidayMaster where fldHolidayId=@fldHolidayId";
            //dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldHolidayId", id) });
        }
        public HolidayModel CreateHolidayToHolidayTemp(FormCollection col)
        {
            //dynamic bankCode = CurrentUser.Account.BankCode;
            HolidayModel holiday = new HolidayModel();
            string Recurring = "";
            string RecurringType = "";
            string RecurringDay = "";
            string RecurringMonth = "0";
            string getDate = "";
            string mon = "N";
            string tue = "N";
            string wed = "N";
            string thu = "N";
            string fri = "N";
            string sat = "N";
            string sun = "N";
            string getDateDb = "";
            //string IsActiveHoliday = "";
            try
            {
                string OneTime = col["OneTime"].ToString();
                //if (!col.AllKeys.Contains("active"))
                //{
                //    IsActiveHoliday = "N";
                //}
                //else
                //{
                //    IsActiveHoliday = col["active"];
                //}

                if (OneTime == "OneTime")
                {
                    Recurring = "N";
                    getDate = col["fldDate"].ToString();
                }
                else
                {
                    Recurring = "Y";
                    RecurringType = col["RecurringType"].ToString();
                    if (RecurringType == "Week")
                    {
                        RecurringType = "W";
                        getDateDb = DBNull.Value.ToString();
                        //RecurringDay = col["Days"].ToString();
                        string[] DayArray = (col["Days"].ToString()).Split(',');
                        foreach (string dayArrayList in DayArray)
                        {
                            if (dayArrayList == "1")
                            {
                                mon = "Y";
                            }
                            if (dayArrayList == "2")
                            {
                                tue = "Y";
                            }
                            if (dayArrayList == "3")
                            {
                                wed = "Y";
                            }
                            if (dayArrayList == "4")
                            {
                                thu = "Y";
                            }
                            if (dayArrayList == "5")
                            {
                                fri = "Y";
                            }
                            if (dayArrayList == "6")
                            {
                                sat = "Y";
                            }
                            if (dayArrayList == "7")
                            {
                                sun = "Y";
                            }
                        }

                    }
                    if (RecurringType == "Month")
                    {
                        RecurringType = "M";
                        RecurringMonth = col["dropdownWeek"].ToString();
                        RecurringDay = col["dropdownDay"].ToString();
                        if (RecurringDay == "1")
                        {
                            mon = "Y";
                        }
                        if (RecurringDay == "2")
                        {
                            tue = "Y";
                        }
                        if (RecurringDay == "3")
                        {
                            wed = "Y";
                        }
                        if (RecurringDay == "4")
                        {
                            thu = "Y";
                        }
                        if (RecurringDay == "5")
                        {
                            fri = "Y";
                        }
                        if (RecurringDay == "6")
                        {
                            sat = "Y";
                        }
                        if (RecurringDay == "7")
                        {
                            sun = "Y";
                        }
                    }
                    if (RecurringType == "Year")
                    {
                        RecurringType = "Y";
                        getDate = col["fldYearDate"].ToString();
                    }
                }
                if (col["OneTime"].ToString() == "OneTime" || RecurringType == "Y")
                {
                    getDateDb = DateTime.ParseExact(getDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                    //getDateDb = Convert.ToDateTime(getDate).ToString("yyyy-MM-dd");
                }

                int intRowAffected;
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@TableName", "tblHolidayMaster"));
                sqlParameterNext.Add(new SqlParameter("@NextNo", 0));
                sqlParameterNext[1].Direction = ParameterDirection.Output;
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcgCTCSNextSeqNo", sqlParameterNext.ToArray());
                long iNextNo = (long)sqlParameterNext[1].Value;

                string daf = DateTime.Now.ToString();
                long iNext = iNextNo;

                List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
                

                sqlParameterNext1.Add(new SqlParameter("@fldHolidayId", iNext));
                sqlParameterNext1.Add(new SqlParameter("@fldRecurring", Recurring));
                sqlParameterNext1.Add(new SqlParameter("@fldRecurringType", RecurringType));
                //sqlParameterNext1.Add(new SqlParameter("@RecurringMonth", RecurringMonth));
                sqlParameterNext1.Add(new SqlParameter("@fldHolidayDate", getDateDb));
                sqlParameterNext1.Add(new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]));
                sqlParameterNext1.Add(new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId));
                sqlParameterNext1.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
                sqlParameterNext1.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
                sqlParameterNext1.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
                sqlParameterNext1.Add(new SqlParameter("@fldDayMon", mon));
                sqlParameterNext1.Add(new SqlParameter("@fldDayTue", tue));
                sqlParameterNext1.Add(new SqlParameter("@fldDayWed", wed));
                sqlParameterNext1.Add(new SqlParameter("@fldDayThu", thu));
                sqlParameterNext1.Add(new SqlParameter("@fldDayFri", fri));
                sqlParameterNext1.Add(new SqlParameter("@fldDaySat", sat));
                sqlParameterNext1.Add(new SqlParameter("@fldDaySun", sun));
                //sqlParameterNext1.Add(new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode));


                intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHolidayTemp", sqlParameterNext1.ToArray());

                //string stmt = "Insert into tblHolidayMVCTemp " +
                //"(fldHolidayId,fldRecurring,fldRecurrType,fldRecurrMonth,fldHolidayDate,fldDesc,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId," +
                //"fldUpdateTimeStamp,fldConfirm,fldDelete,fldActiveHoliday,fldDayMon,fldDayTue,fldDayWed,fldDayThu,fldDayFri,fldDaySat,fldDaySun,fldBankCode,fldApproveStatus)" +
                //"VALUES (@fldHolidayId,@fldRecurring,@fldRecurringType,@RecurringMonth,@fldHolidayDate,@fldHolidayDesc,@fldCreateUserId,@fldCreateTimeStamp,@fldUpdateUserId," +
                //"@fldUpdateTimeStamp,'Y','N',@fldActiveHoliday,@fldDayMon,@fldDayTue,@fldDayWed,@fldDayThu,@fldDayFri,@fldDaySat,@fldDaySun,@fldBankCode,'A')";
                //dbContext.ExecuteNonQuery(stmt, new[] {
                //    new SqlParameter("@fldHolidayId", iNextNo),
                //    new SqlParameter("@fldRecurring", Recurring),
                //    new SqlParameter("@fldRecurringType", RecurringType),
                //    new SqlParameter("@RecurringMonth",RecurringMonth),
                //    new SqlParameter("@fldHolidayDate", getDateDb),
                //    new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]),
                //    new SqlParameter("@fldCreateUserId", CurrentUser.Account.UserId),
                //    new SqlParameter("@fldCreateTimeStamp", DateTime.Now),
                //    new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
                //    new SqlParameter("@fldUpdateTimeStamp", DateTime.Now),
                //    new SqlParameter("@fldActiveHoliday", IsActiveHoliday),
                //    new SqlParameter("@fldDayMon", mon),
                //    new SqlParameter("@fldDayTue", tue),
                //    new SqlParameter("@fldDayWed", wed),
                //    new SqlParameter("@fldDayThu", thu),
                //    new SqlParameter("@fldDayFri", fri),
                //    new SqlParameter("@fldDaySat", sat),
                //    new SqlParameter("@fldDaySun", sun),
                //    new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode)
                //});
                return holiday;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CreateHolidayinMain(string id)
        {
            //string stmt = "Insert into tblHolidayMaster(fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, ";
            //stmt = stmt + "fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, fldHolidayDesc, fldCreateUserId,";
            //stmt = stmt + "fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp) ";
            //stmt = stmt + "select fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, fldDayWed, fldDayThu,";
            //stmt = stmt + "fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, fldHolidayDesc, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, ";
            //stmt = stmt + "fldUpdateTimeStamp from tblHolidayMasterTemp ";
            //stmt = stmt + " where fldHolidayId=@fldHolidayId ";
            //dbContext.ExecuteNonQuery(stmt, new SqlParameter[]
            //{
            //    new SqlParameter("@fldHolidayId", id)

            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", id));
            sqlParameterNext.Add(new SqlParameter("@tblName", ""));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHolidayMasterTempbyId", sqlParameterNext.ToArray());
        }
        public void AddHolidayinTemptoDelete(string id)
        {
            //string stmt = "Insert into tblHolidayMasterTemp(fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, ";
            //stmt = stmt + "fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, fldHolidayDesc, fldCreateUserId,";
            //stmt = stmt + "fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldApproveStatus) ";
            //stmt = stmt + "select fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, fldDayWed, fldDayThu,";
            //stmt = stmt + "fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, fldHolidayDesc, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, ";
            //stmt = stmt + "fldUpdateTimeStamp, 'D' from tblHolidayMaster ";
            //stmt = stmt + " where fldHolidayId=@fldHolidayId";
            //dbContext.ExecuteNonQuery(stmt, new SqlParameter[]
            //{
            //    new SqlParameter("@fldHolidayId", id)

            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", id));
            sqlParameterNext.Add(new SqlParameter("@tblName", "temp"));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciHolidayMasterTempbyId", sqlParameterNext.ToArray());
        }
        public void AddHolidayinTemptoUpdate(FormCollection col)
        {
            //string IsActiveHoliday = "";
            //if (!col.AllKeys.Contains("active"))
            //{
            //    IsActiveHoliday = "N";
            //}
            //else
            //{
            //    IsActiveHoliday = col["active"];
            //}
            //string stmt = "Insert into tblHolidayMasterTemp(fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, ";
            //stmt = stmt + "fldDayWed, fldDayThu, fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, fldHolidayDesc, fldCreateUserId,";
            //stmt = stmt + "fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldApproveStatus) ";
            //stmt = stmt + "select fldHolidayId,fldRecurring, fldRecurrType, fldDayMon, fldDayTue, fldDayWed, fldDayThu,";
            //stmt = stmt + "fldDayFri, fldDaySat, fldDaySun, fldHolidayDate, @fldHolidayDesc, fldCreateUserId, fldCreateTimeStamp, @fldUpdateUserId, ";
            //stmt = stmt + "@fldUpdateTimeStamp, 'U' from tblHolidayMaster ";
            //stmt = stmt + " where fldHolidayId=@fldHolidayId";
            //dbContext.ExecuteNonQuery(stmt, new SqlParameter[]
            //{
            //    new SqlParameter("@fldHolidayId", col["HolidayId"]),
            //    new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]),
            //    //new SqlParameter("@fldActiveHoliday", IsActiveHoliday),
            //    new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId),
            //    new SqlParameter("@fldUpdateTimeStamp", DateTime.Now)
            //});
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", col["HolidayId"]));
            sqlParameterNext.Add(new SqlParameter("@fldHolidayDesc", col["fldHolidayDescription"]));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", CurrentUser.Account.UserId));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@tblName", "temp"));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHolidayMasterTemp", sqlParameterNext.ToArray());
        }
        public void DeleteHolidayinTemp(string Value, int count)
        {
            if (count < 15)
            {
                //string stmt = "delete from tblHolidayMasterTemp where fldHolidayId=@fldHolidayId";
                //dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldHolidayId", Value) });
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldHolidayId", Value));
                sqlParameterNext.Add(new SqlParameter("@fldHolidayDate", ""));
                sqlParameterNext.Add(new SqlParameter("@Count", count));
                sqlParameterNext.Add(new SqlParameter("@tblName", "temp"));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHolidayMasterTemp", sqlParameterNext.ToArray()); 
            }
            else
            {
                string getDateDb = "";
                //string stmt = "delete from tblHolidayMasterTemp where DATEDIFF(DAY,fldHolidayDate,@HolidayDate) = 0";
                getDateDb = DateTime.ParseExact(Value, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                //dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@HolidayDate", getDateDb) });
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldHolidayId", ""));
                sqlParameterNext.Add(new SqlParameter("@fldHolidayDate", getDateDb));
                sqlParameterNext.Add(new SqlParameter("@Count", count));
                sqlParameterNext.Add(new SqlParameter("@tblName", "temp"));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdHolidayMasterTemp", sqlParameterNext.ToArray());
            }

        }
        public void UpdateHolidayToMain(string id)
        {
            //string stmt = "update tblHolidayMaster set fldHolidayDesc = tblHolidayMasterTemp.fldHolidayDesc , fldUpdateUserId = tblHolidayMasterTemp.fldUpdateUserId , fldUpdateTimeStamp = tblHolidayMasterTemp.fldUpdateTimeStamp  from tblHolidayMaster inner join tblHolidayMasterTemp on tblHolidayMaster.fldHolidayId = tblHolidayMasterTemp.fldHolidayId Where tblHolidayMaster.fldHolidayId=@fldHolidayId";
            //dbContext.ExecuteNonQuery(stmt, new[] {
            //                new SqlParameter("@fldHolidayId", id)

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldHolidayId", id));
            sqlParameterNext.Add(new SqlParameter("@fldHolidayDesc", ""));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", ""));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", ""));
            sqlParameterNext.Add(new SqlParameter("@tblName", "checker"));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuHolidayMasterTemp", sqlParameterNext.ToArray());
        }

    }
}