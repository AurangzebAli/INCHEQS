using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.Security.Resources;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.SystemProfile;

namespace INCHEQS.Areas.OCS.Models.OCSRetentionPeriod
{
    public class OCSRetentionPeriodDao : IOCSRetentionPeriodDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public OCSRetentionPeriodDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        public OCSRetentionPeriodModel GetOCSRetentionPeriod()
        {
            OCSRetentionPeriodModel OCSRetentionPeriod = new OCSRetentionPeriodModel();

            OCSRetentionPeriod.OCInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodMaster("OC", "Interval")));
            OCSRetentionPeriod.OCIntType = GetValueFromOCSRetentionPeriodMaster("OC", "IntervalType");
            OCSRetentionPeriod.OCHistoryInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodMaster("OC History", "Interval")));
            OCSRetentionPeriod.OCHistoryIntType = GetValueFromOCSRetentionPeriodMaster("OC History", "IntervalType");
            OCSRetentionPeriod.IRInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodMaster("IR", "Interval")));
            OCSRetentionPeriod.IRIntType = GetValueFromOCSRetentionPeriodMaster("IR", "IntervalType");

            return OCSRetentionPeriod;
        }

        public OCSRetentionPeriodModel GetOCSRetentionPeriodTemp()
        {
            OCSRetentionPeriodModel OCSRetentionPeriodTemp = new OCSRetentionPeriodModel();

            OCSRetentionPeriodTemp.OCInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodTemp("OC", "Interval")));
            OCSRetentionPeriodTemp.OCIntType = GetValueFromOCSRetentionPeriodTemp("OC", "IntervalType");
            OCSRetentionPeriodTemp.OCHistoryInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodTemp("OC History", "Interval")));
            OCSRetentionPeriodTemp.OCHistoryIntType = GetValueFromOCSRetentionPeriodTemp("OC History", "IntervalType");
            OCSRetentionPeriodTemp.IRInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromOCSRetentionPeriodTemp("IR", "Interval")));
            OCSRetentionPeriodTemp.IRIntType = GetValueFromOCSRetentionPeriodTemp("IR", "IntervalType");

            return OCSRetentionPeriodTemp;
        }

        public bool CheckMaxSession(string timeOut)
        {
            bool status = false;
            //Get MaxSessionTimeOut setting from tblsystemprofile
            string maxtimeOut = systemProfileDao.GetValueFromSystemProfileWithoutCurrentUser("MaxSessionTimeOut");
            if (Convert.ToInt32(maxtimeOut) < Convert.ToInt32(timeOut))
            {
                status = true;
            }
            else
            {
                status = false;
            }
            return status;
        }

        public List<string> ValidateOCSRetentionPeriod(FormCollection col)
        {
            List<string> err = new List<string>();
            int result;

            if ((col["OCInt"].Equals("")))
            {
                err.Add(Locale.OutwardClearingRetentionPeriodCannotBeBlank);
            }
            else if (!(int.TryParse((col["OCInt"]), out result)))
            {
                err.Add(Locale.OutwardClearingRetentionPeriodMustBeNumeric);
            }
            else if ((col["OCInt"].Equals("0")))
            {
                err.Add(Locale.OutwardClearingRetentionPeriodCannotBeZero);
            }
            else if ((col["OCHistoryInt"].Equals("")))
            {
                err.Add(Locale.OutwardClearingHistoryRetentionPeriodCannotBeBlank);
            }
            else if (!(int.TryParse((col["OCHistoryInt"]), out result)))
            {
                err.Add(Locale.OutwardClearingHistoryRetentionPeriodMustBeNumeric);
            }
            else if ((col["OCHistoryInt"].Equals("0")))
            {
                err.Add(Locale.OutwardClearingHistoryRetentionPeriodCannotBeZero);
            }
            else if ((col["IRInt"].Equals("")))
            {
                err.Add(Locale.InwardReturnRetentionPeriodCannotBeBlank);
            }
            else if (!(int.TryParse((col["IRInt"]), out result)))
            {
                err.Add(Locale.InwardReturnRetentionPeriodMustBeNumeric);
            }
            else if ((col["IRInt"].Equals("0")))
            {
                err.Add(Locale.InwardReturnRetentionPeriodCannotBeZero);
            }
            else if ((col["OCIntType"].Equals("")))
            {
                err.Add(Locale.ChooseTheRightIntervalType);
            }
            else if ((col["OCHistoryIntType"].Equals("")))
            {
                err.Add(Locale.ChooseTheRightIntervalType);
            }
            else if ((col["IRIntType"].Equals("")))
            {
                err.Add(Locale.ChooseTheRightIntervalType);
            }

            return err;
        }

        public string GetValueFromOCSRetentionPeriodMaster(string retentionPeriodType, string type)
        {
            string result = "";
            try
            {
                DataTable dataResult = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldRetentionPeriodType", retentionPeriodType));
                sqlParameterNext.Add(new SqlParameter("@Type", type));
                dataResult = dbContext.GetRecordsAsDataTableSP("spcgValueFromOCSRetentionPeriodMaster", sqlParameterNext.ToArray());

                if (type == "Interval")
                {
                    if (dataResult.Rows.Count > 0)
                    {
                        result = dataResult.Rows[0]["fldRetentionPeriodInterval"].ToString();
                    }
                }
                else if (type == "IntervalType")
                {
                    if (dataResult.Rows.Count > 0)
                    {
                        result = dataResult.Rows[0]["fldRetentionPeriodIntervalType"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public string GetValueFromOCSRetentionPeriodTemp(string retentionPeriodType, string type)
        {
            string result = "";
            try
            {
                DataTable dataResult = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldRetentionPeriodType", retentionPeriodType));
                sqlParameterNext.Add(new SqlParameter("@Type", type));
                dataResult = dbContext.GetRecordsAsDataTableSP("spcgValueFromOCSRetentionPeriodTemp", sqlParameterNext.ToArray());

                if (type == "Interval")
                {
                    if (dataResult.Rows.Count > 0)
                    {
                        result = dataResult.Rows[0]["fldRetentionPeriodInterval"].ToString();
                    }
                }
                else if (type == "IntervalType")
                {
                    if (dataResult.Rows.Count > 0)
                    {
                        result = dataResult.Rows[0]["fldRetentionPeriodIntervalType"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public void UpdateOCSRetentionPeriod(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@OCInt", col["OCInt"]));
            sqlParameterNext.Add(new SqlParameter("@OCIntType", col["OCIntType"]));
            sqlParameterNext.Add(new SqlParameter("@OCHistoryInt", col["OCHistoryInt"]));
            sqlParameterNext.Add(new SqlParameter("@OCHistoryIntType", col["OCHistoryIntType"]));
            sqlParameterNext.Add(new SqlParameter("@IRInt", col["IRInt"]));
            sqlParameterNext.Add(new SqlParameter("@IRIntType", col["IRIntType"]));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuOCSRetentionPeriod", sqlParameterNext.ToArray());

        }

        public void CreateOCSRetentionMasterTemp(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@OCInt", col["OCInt"]));
            sqlParameterNext.Add(new SqlParameter("@OCIntType", col["OCIntType"]));
            sqlParameterNext.Add(new SqlParameter("@OCHistoryInt", col["OCHistoryInt"]));
            sqlParameterNext.Add(new SqlParameter("@OCHistoryIntType", col["OCHistoryIntType"]));
            sqlParameterNext.Add(new SqlParameter("@IRInt", col["IRInt"]));
            sqlParameterNext.Add(new SqlParameter("@IRIntType", col["IRIntType"]));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciOCSRetentionPeriodTemp", sqlParameterNext.ToArray());

        }

        public void CreateOCSRetentionPeriodChecker(string OCSRetentionPeriod, string Update, string currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTaskModule", OCSRetentionPeriod));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", Update));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciOCSRetentionPeriodChecker", sqlParameterNext.ToArray());

        }

        public void DeleteOCSRetentionPeriodMaster()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdOCSRetentionPeriodMaster", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void MovetoOCSRetentionPeriodMasterfromTemp()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciOCSRetentionPeriodMaster", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteOCSRetentionPeriodChecker()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdOCSRetentionPeriodChecker", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteOCSRetentionPeriodTemp()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdOCSRetentionPeriodTemp", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckOCSRetentionPeriodTemp()
        {
            bool strs = false;
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            dataTable = this.dbContext.GetRecordsAsDataTableSP("spcgOCSRetentionPeriodTemp", sqlParameterNext.ToArray());

            if (dataTable.Rows.Count > 0)
            {
                strs = true;


            }
            else
            {
                strs = false;

            }

            return strs;
        }

        public List<OCSRetentionPeriodModel> ListIntervalType()
        {
            DataTable resultTable = new DataTable();
            List<OCSRetentionPeriodModel> IntervalType = new List<OCSRetentionPeriodModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgListIntervalType", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    OCSRetentionPeriodModel type = new OCSRetentionPeriodModel();
                    type.IntType = row["fldIntervalType"].ToString();
                    IntervalType.Add(type);
                }
            }
            return IntervalType;
        }

    }
}
