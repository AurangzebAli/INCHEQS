using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using INCHEQS.Security.Account;
using System.Web.Mvc;
using System.Data;
using INCHEQS.Security;
using INCHEQS.Common;
using System.Collections;
using System.Globalization;
using VBUtilites;

namespace INCHEQS.Areas.OCS.Models.OCSProcessDate
{
    public class OCSProcessDateDao : IOCSProcessDateDao
    {
        private readonly ApplicationDbContext dbContext;

        public OCSProcessDateDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        } 

        public void CreateProcessDate(FormCollection col, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlProcessDate = new Dictionary<string, dynamic>();
            sqlProcessDate.Add("fldProcessDate", Convert.ToDateTime(col["processDate"]));
            sqlProcessDate.Add("fldStatus", "Y");
            sqlProcessDate.Add("fldActiveBy", Convert.ToInt64(currentUser.UserId));
            sqlProcessDate.Add("fldActiveTimestamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime()));
            sqlProcessDate.Add("fldCloseBy", Convert.ToInt64(currentUser.UserId));
            sqlProcessDate.Add("fldCloseTimestamp", Convert.ToDateTime(DateUtils.GetCurrentDatetime()));
            sqlProcessDate.Add("fldBankCode", currentUser.BankCode);
            dbContext.ConstructAndExecuteInsertCommand("tblProcessDate", sqlProcessDate);
        }

        public string getCurrentDate()
        {
            DataTable ds = new DataTable();
            string currentDate = "";
            string stmt = "Select fldProcessDate From tblprocessdate Where fldStatus = 'Y' Order By fldProcessDate desc ";
            ds = dbContext.GetRecordsAsDataTable(stmt);

            if (ds.Rows.Count > 0)
            {
                currentDate = ds.Rows[0]["fldProcessDate"].ToString();
                DateTime dt = Convert.ToDateTime(currentDate);
                currentDate = dt.ToString("yyyy-MM-dd");

            }
            else
            {
                currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            //currentDate = DateTime.ParseExact(currentDate, "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("dd-MMMM-yyyy");
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


            //if (dateProcess > Convert.ToDateTime(today))
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", dateProcess.AddDays(1).ToString("yyyy-MM-dd")));
            //}
            //else if(dateProcess == Convert.ToDateTime(today))
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", Convert.ToDateTime(today).AddDays(1).ToString("yyyy-MM-dd")));
            //}
            //else if (dateProcess < Convert.ToDateTime(today))
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", today));
            //}
            //else
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", today));
            //}

            //if (dateProcess >= Convert.ToDateTime(compare))
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", getCurrentDate()));
            //}
            //else
            //{
            //    SqlParameterNext.Add(new SqlParameter("p_currentdate", today));
            //}

            ////Excute the command
            //processDate = dbContext.GetRecordsAsDataTableSP("sp_getnextocsprocessdate", SqlParameterNext.ToArray());

            //if (processDate.Rows.Count > 0)
            //{
            //    date = processDate.Rows[0]["processdate"].ToString();

            //}

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

            // ********************* ---------3---------- *********************
            // ********************* Get monthly table value *********************
            List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
            sqlParameterNext2.Add(new SqlParameter("@currentMonth", dateTimeToday.Month.ToString()));
            sqlParameterNext2.Add(new SqlParameter("@currentYear", dateTimeToday.Year.ToString()));
            sqlParameterNext2.Add(new SqlParameter("@Recurring", 3));
            ProcessDateDataTable = dbContext.GetRecordsAsDataTableSP("spcgHoliday", sqlParameterNext2.ToArray());
            if (ProcessDateDataTable.Rows.Count > 0)
            {
                DateTime dateTimeCheck;
                DateTime dateTimeFirstDayOfMonth;
                int intDayCounter;
                int intDayDiff;

                for (intDayCounter = 1; intDayCounter <= DateTime.DaysInMonth(dateTimeToday.Year, dateTimeToday.Month); intDayCounter++)
                {
                    for (intCounter = 0; intCounter <= ProcessDateDataTable.Rows.Count - 1; intCounter++)
                    {
                        // Set the first day of the month
                        dateTimeFirstDayOfMonth = vbutility.GetDateSerial(dateTimeToday, 1);//new DateTime(dateTimeToday.Year, dateTimeToday.Month, 1); //DateSerial(dateTimeToday.Year, dateTimeToday.Month, 1);

                        // Get current day
                        dateTimeCheck = vbutility.GetDateSerial(dateTimeToday, intDayCounter); // new DateTime(dateTimeToday.Year, dateTimeToday.Month, intDayCounter);

                        // Get number of week
                        intDayDiff = vbutility.Getnumberofweek(dateTimeCheck, dateTimeFirstDayOfMonth);

                        if (intDayDiff == Convert.ToInt32(ProcessDateDataTable.Rows[intCounter]["fldRecurrMonth"].ToString()))
                        {
                            if (ProcessDateDataTable.Rows[intCounter]["fldDayMon"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 2)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDayTue"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 3)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDayWed"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 4)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDayThu"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 5)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDayFri"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 6)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDaySat"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 7)
                                {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                            else if (ProcessDateDataTable.Rows[intCounter]["fldDaySun"].ToString().Equals("Y"))
                            {
                                if (vbutility.GetWeekday(dateTimeCheck) == 1)
            {
                                    arrDay.Add(Convert.ToInt32(dateTimeCheck.ToString("dd")));
                                }
                            }
                        }
                    }
                }
            }

            // ********************* ---------4----------- *********************
            // ********************* Get weekly table value *********************
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
            for (intCounter = 0; intCounter <= arrDay.Count-1 ; intCounter++)
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
            string stmt = "update tblProcessDate set fldStatus= @fldStatus,fldCloseBy=@fldCloseBy, fldCloseTimestamp=@fldCloseTimestamp where fldProcessDate = @fldProcessDate ";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldStatus","N"),
                new SqlParameter("@fldCloseBy", Convert.ToInt64(CurrentUser.Account.UserId)),
                new SqlParameter("@fldCloseTimestamp",Convert.ToDateTime(DateUtils.GetCurrentDatetime())),
                new SqlParameter("@fldProcessDate",Convert.ToDateTime(col["currentDate"]))
            });
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
    }
}