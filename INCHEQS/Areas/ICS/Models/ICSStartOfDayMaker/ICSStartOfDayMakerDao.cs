using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.Account;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VBUtilites;

namespace INCHEQS.Areas.ICS.Models.ICSStartOfDayMaker
{
    public class ICSStartOfDayMakerDao : IICSStartOfDayMakerDao
    {
        private readonly ApplicationDbContext dbContext;
        public ICSStartOfDayMakerDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public bool ClearDateExist()
        {
            bool result = false;
            DataTable ProcessDateDataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgICSProcessDate", sqlParameterNext.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
        public string getCurrentDate()
        {
            DataTable ProcessDateDataTable = new DataTable();
            string currentDate = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgICSProcessDate", sqlParameterNext.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                currentDate = ProcessDateDataTable.Rows[0]["fldProcessDate"].ToString();
                DateTime dt = Convert.ToDateTime(currentDate);
                currentDate = dt.ToString("yyyy-MM-dd");
            }
            else
            {
                currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            return currentDate;
        }

        public string getNextProcessDate()
        {
            string date = "";
            DateTime dateCompare;
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            DataTable processDate = new DataTable();
            ArrayList arrHoliday = new ArrayList();
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string compare = (DateTime.Now.AddDays(1)).ToString("yyyy-MM-dd");
            DateTime dateProcess = Convert.ToDateTime(getCurrentDate());
            if (dateProcess > DateTime.Now.AddDays(-1))
            {
                dateCompare = dateProcess.AddDays(1);
            }
            else
            {
                dateCompare = DateTime.Today;
            }

            // Get holiday
            arrHoliday.Clear();
            arrHoliday = getHoliday(dateCompare);

            bool blnGetAvailableDate = false;
            ArrayList arrAvailableDate = new ArrayList();

            // Get next available date
            while ((arrHoliday.Contains(dateCompare.Day)))
            {
                dateCompare = dateCompare.AddDays(1);
                // Check next process date whether it is in the same month with process date
                if (dateCompare.Month != dateProcess.Month && blnGetAvailableDate == false)
                {
                    blnGetAvailableDate = true;
                    arrHoliday.Clear();
                    arrHoliday = getHoliday(dateCompare);
                }
            }

            date = dateCompare.ToString("dd-MM-yyyy");
            return date;
        }

        public ArrayList getHoliday(DateTime dateTimeToday)
        {
            vbUtilities vbutility = new vbUtilities();
            // ********************* DECLARATION *********************
            ArrayList arrDay = new ArrayList();
            int intCounter;
            DataTable ProcessDateDataTable;
            // ********************* METHOD *********************
            // This store procedure will run 4 times to get specific data
            //			
            //	-- 1: For Recurring is 'N'
            // -- 2: For Recurring is 'Y' and Recurring Type is 'Yearly'
            // -- 3: For Recurring is 'Y' and Recurring Type is 'Monthly'
            // -- 4: For Recurring is 'Y' and Recurring Type is 'Weekly'
            // Return Value is only Day number
            // ********************* ---------1---------- *********************
            // ********************* Get all holiday date *********************
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@currentMonth", dateTimeToday.Month.ToString()));
            sqlParameterNext.Add(new SqlParameter("@currentYear", dateTimeToday.Year.ToString()));
            sqlParameterNext.Add(new SqlParameter("@Recurring", 1));
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgHoliday", sqlParameterNext.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                {
                    arrDay.Add(Convert.ToInt32(ProcessDateDataTable.Rows[intCounter]["fldHolidayDate"].ToString()));
                }
            }
            // ********************* ---------2---------- *********************
            // ********************* Get all yearly holiday *********************
            List<SqlParameter> sqlParameterNext1 = new List<SqlParameter>();
            sqlParameterNext1.Add(new SqlParameter("@currentMonth", dateTimeToday.Month.ToString()));
            sqlParameterNext1.Add(new SqlParameter("@currentYear", dateTimeToday.Year.ToString()));
            sqlParameterNext1.Add(new SqlParameter("@Recurring", 2));
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgHoliday", sqlParameterNext1.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                {
                    arrDay.Add(Convert.ToInt32(ProcessDateDataTable.Rows[intCounter]["fldHolidayDate"].ToString()));
                }
            }
            
            List<SqlParameter> sqlParameterNext3 = new List<SqlParameter>();
            sqlParameterNext3.Add(new SqlParameter("@currentMonth", dateTimeToday.Month.ToString()));
            sqlParameterNext3.Add(new SqlParameter("@currentYear", dateTimeToday.Year.ToString()));
            sqlParameterNext3.Add(new SqlParameter("@Recurring", 4));
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgHoliday", sqlParameterNext3.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                int intDayCounter;
                DateTime dateTimeCheck;
                DataRow[] objrows = ProcessDateDataTable.Select("fldDayMon = 'Y' and fldDayWed = 'Y'");
                for (intDayCounter = 1; intDayCounter <= DateTime.DaysInMonth(dateTimeToday.Year, dateTimeToday.Month); intDayCounter++)
                {
                    dateTimeCheck = vbutility.GetDateSerial(dateTimeToday, intDayCounter);
                    // Check on sunday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDaySun"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 1)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on monday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDayMon"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 2)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on tuesday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDayTue"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 3)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on wednesday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDayWed"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 4)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on thursday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDayThu"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 5)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on friday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDayFri"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 6)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                    // Check on saturday
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        if (ProcessDateDataTable.Rows[intCounter]["fldDaySat"].ToString().Equals("Y"))
                        {
                            if (vbutility.GetWeekday(dateTimeCheck) == 7)
                            {
                                arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                            }
                        }
                    }
                }
            }

            // ********************* Remove duplicate day in arraylist *********************
            ArrayList arrCheckDay = new ArrayList();
            for (intCounter = 0; intCounter <= arrDay.Count - 1; intCounter++)
            {
                if (!arrCheckDay.Contains(arrDay[intCounter]))
                {
                    arrCheckDay.Add(arrDay[intCounter]);
                }
            }
            arrCheckDay.Sort();
            arrDay = arrCheckDay;
            return arrDay;
        }

        public static int GetWeekNumber(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }
        public DateTime DateSerial(int year, int month, int day)
        {
            if (year < 0)
            {
                year = DateTime.Now.Year + year;
            }
            else if (year < 100)
            {
                year = 1930 + year;
            }
            DateTime dt = new DateTime(year, 1, 1);
            dt = dt.AddMonths(month - 1);
            dt = dt.AddDays(day - 1);

            return dt;
        }

        public void UpdateProcessDate(FormCollection col)
        {
            string stmt = "update tblProcessDate set fldStatus= @fldStatus,fldCloseBy=@fldCloseBy, fldCloseTimestamp=@fldCloseTimestamp where Datediff(d,fldProcessDate,@fldProcessDate)=0 ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldStatus","N"),
                new SqlParameter("@fldCloseBy", Convert.ToInt64(CurrentUser.Account.UserId)),
                new SqlParameter("@fldCloseTimestamp",Convert.ToDateTime(DateUtils.GetCurrentDatetime())),
                new SqlParameter("@fldProcessDate",Convert.ToDateTime(col["currentDate"]).ToString("yyyy-MM-dd"))
            });
        }
        public void CreateProcessDate(FormCollection col, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlProcessDate = new Dictionary<string, dynamic>();
            sqlProcessDate.Add("fldProcessDate", Convert.ToDateTime(col["processDate"]).ToString("yyyy-MM-dd"));
            sqlProcessDate.Add("fldStatus", "Y");
            sqlProcessDate.Add("fldActiveBy", Convert.ToInt64(currentUser.UserId));
            sqlProcessDate.Add("fldActiveTimestamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime()));
            sqlProcessDate.Add("fldCloseBy", Convert.ToInt64(currentUser.UserId));
            sqlProcessDate.Add("fldCloseTimestamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime()));
            sqlProcessDate.Add("fldBankCode", currentUser.BankCode);
            dbContext.ConstructAndExecuteInsertCommand("tblProcessDate", sqlProcessDate);
        }

        public bool PerfromStartofDay(FormCollection col, AccountModel currentUser)
        {
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@currentprocessdate", col["currentDate"]));
            sqlParameterNext.Add(new SqlParameter("@Nextprocessdate", col["processDate"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciICSStartofDay", sqlParameterNext.ToArray());
            return blnResult;
        }

        public DataTable GetHolidayDates(FormCollection col)
        {
            try
            {
                DataTable ds = new DataTable();
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                SqlParameterNext.Add(new SqlParameter("@currentMonth", DateTime.Now.Month.ToString()));
                SqlParameterNext.Add(new SqlParameter("@currentYear", DateTime.Now.Year.ToString()));
                SqlParameterNext.Add(new SqlParameter("@Recurring", 999));
                ds = dbContext.GetRecordsAsDataTableSP("spcgHoliday", SqlParameterNext.ToArray());
                return ds;
            }
            catch (Exception exception)
            {

                throw exception;
            }
        }

        public string getConfirmProcessDate()
        {
            DataTable confirmprocessdate = new DataTable();
            string confirmprocessDate = "";
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            confirmprocessdate = dbContext.GetRecordsAsDataTableSP("spcgConfirmProcessDate", sqlParameterNext.ToArray());
            if (confirmprocessdate.Rows.Count > 0)
            {
                confirmprocessDate = confirmprocessdate.Rows[0]["fldProcessDate"].ToString();
                DateTime dt = Convert.ToDateTime(confirmprocessDate);
                confirmprocessDate = dt.ToString("dd-MM-yyyy");
            }
            else
            {
                confirmprocessDate = null;
            }
            return confirmprocessDate;
        }

        public void CreateICSStartOfDayTemp(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", col["processDate"]));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@fldApproveStatus", "A"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));

            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciICSStartOfDayTemp", sqlParameterNext.ToArray());

        }
        
        public bool DeleteICSStartOfDayTemp(AccountModel user)
        {
            int intRowAffected;
            bool blnResult = false;
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@user", Convert.ToInt64(user.UserId)));
            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdInwardClearDateTemp", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }
            return blnResult;
        }

        public void MoveToInwardClearDateMasterFromTemp(FormCollection col, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

            sqlParameterNext.Add(new SqlParameter("@fldBankCode", currentUser.BankCode));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", col["processDate"]));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", "Y"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserID", Convert.ToInt64(currentUser.UserId)));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime())));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciInwardClearDateMasterFromTemp", sqlParameterNext.ToArray());
        }

        public List<string> ValidateICSSOD()
        {
            List<string> err = new List<string>();
            string CheckClearDateExist = "";
            CheckClearDateExist = getConfirmProcessDate();
            if ((CheckClearDateExist != null))
            {
                err.Add(Locale.ClearDateExist);
            }

            return err;

        }
    }
}