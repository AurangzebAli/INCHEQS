using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.Security.Resources;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.SystemProfile;

namespace INCHEQS.Areas.ICS.Models.ICSRetentionPeriod
{
    public class ICSRetentionPeriodDao : IICSRetentionPeriodDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public ICSRetentionPeriodDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        public ICSRetentionPeriodModel GetICSRetentionPeriod()
        {
            ICSRetentionPeriodModel ICSRetentionPeriod = new ICSRetentionPeriodModel();

            ICSRetentionPeriod.ICInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromICSRetentionPeriodMaster("IC", "Interval")));
            ICSRetentionPeriod.ICIntType = GetValueFromICSRetentionPeriodMaster("IC", "IntervalType");
            ICSRetentionPeriod.ICHistoryInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromICSRetentionPeriodMaster("IC History", "Interval")));
            ICSRetentionPeriod.ICHistoryIntType = GetValueFromICSRetentionPeriodMaster("IC History", "IntervalType");
            
            return ICSRetentionPeriod;
        }

        public ICSRetentionPeriodModel GetICSRetentionPeriodTemp()
        {
            ICSRetentionPeriodModel ICSRetentionPeriodTemp = new ICSRetentionPeriodModel();

            ICSRetentionPeriodTemp.ICInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromICSRetentionPeriodTemp("IC", "Interval")));
            ICSRetentionPeriodTemp.ICIntType = GetValueFromICSRetentionPeriodTemp("IC", "IntervalType");
            ICSRetentionPeriodTemp.ICHistoryInt = StringUtils.convertToInt(StringUtils.Trim(GetValueFromICSRetentionPeriodTemp("IC History", "Interval")));
            ICSRetentionPeriodTemp.ICHistoryIntType = GetValueFromICSRetentionPeriodTemp("IC History", "IntervalType");
            
            return ICSRetentionPeriodTemp;
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

        public List<string> ValidateICSRetentionPeriod(FormCollection col)
        {
            List<string> err = new List<string>();
            int result;

            if ((col["ICInt"].Equals("")))
            {
                err.Add(Locale.InwardClearingRetentionPeriodCannotBeBlank);
            }
            else if (!(int.TryParse((col["ICInt"]), out result)))
            {
                err.Add(Locale.InwardClearingRetentionPeriodMustBeNumeric);
            }
            else if ((col["ICInt"].Equals("0")))
            {
                err.Add(Locale.InwardClearingRetentionPeriodCannotBeZero);
            }
            else if ((col["ICHistoryInt"].Equals("")))
            {
                err.Add(Locale.InwardClearingHistoryRetentionPeriodCannotBeBlank);
            }
            else if (!(int.TryParse((col["ICHistoryInt"]), out result)))
            {
                err.Add(Locale.InwardClearingHistoryRetentionPeriodMustBeNumeric);
            }
            else if ((col["ICHistoryInt"].Equals("0")))
            {
                err.Add(Locale.InwardClearingHistoryRetentionPeriodCannotBeZero);
            }
            else if ((col["ICIntType"].Equals("")))
            {
                err.Add(Locale.ChooseTheRightIntervalType);
            }
            else if ((col["ICHistoryIntType"].Equals("")))
            {
                err.Add(Locale.ChooseTheRightIntervalType);
            }

            return err;
        }

        public string GetValueFromICSRetentionPeriodMaster(string retentionPeriodType, string type)
        {
            string result = "";
            try
            {
                DataTable dataResult = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldRetentionPeriodType", retentionPeriodType));
                sqlParameterNext.Add(new SqlParameter("@Type", type));
                dataResult = dbContext.GetRecordsAsDataTableSP("spcgValueFromICSRetentionPeriodMaster", sqlParameterNext.ToArray());

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

        public string GetValueFromICSRetentionPeriodTemp(string retentionPeriodType, string type)
        {
            string result = "";
            try
            {
                DataTable dataResult = new DataTable();
                List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
                sqlParameterNext.Add(new SqlParameter("@fldRetentionPeriodType", retentionPeriodType));
                sqlParameterNext.Add(new SqlParameter("@Type", type));
                dataResult = dbContext.GetRecordsAsDataTableSP("spcgValueFromICSRetentionPeriodTemp", sqlParameterNext.ToArray());

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

        public void UpdateICSRetentionPeriod(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ICInt", col["ICInt"]));
            sqlParameterNext.Add(new SqlParameter("@ICIntType", col["ICIntType"]));
            sqlParameterNext.Add(new SqlParameter("@ICHistoryInt", col["ICHistoryInt"]));
            sqlParameterNext.Add(new SqlParameter("@ICHistoryIntType", col["ICHistoryIntType"]));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuICSRetentionPeriod", sqlParameterNext.ToArray());

        }

        public void CreateICSRetentionMasterTemp(FormCollection col)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ICInt", col["ICInt"]));
            sqlParameterNext.Add(new SqlParameter("@ICIntType", col["ICIntType"]));
            sqlParameterNext.Add(new SqlParameter("@ICHistoryInt", col["ICHistoryInt"]));
            sqlParameterNext.Add(new SqlParameter("@ICHistoryIntType", col["ICHistoryIntType"]));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciICSRetentionPeriodTemp", sqlParameterNext.ToArray());

        }

        public void CreateICSRetentionPeriodChecker(string ICSRetentionPeriod, string Update, string currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldTaskModule", ICSRetentionPeriod));
            sqlParameterNext.Add(new SqlParameter("@fldStatus", Update));
            sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", currentUser));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", (object)DateTime.Now));
            sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", currentUser));

            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciICSRetentionPeriodChecker", sqlParameterNext.ToArray());

        }

        public void DeleteICSRetentionPeriodMaster()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdICSRetentionPeriodMaster", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void MovetoICSRetentionPeriodMasterfromTemp()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciICSRetentionPeriodMaster", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteICSRetentionPeriodChecker()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdICSRetentionPeriodChecker", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteICSRetentionPeriodTemp()
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            try
            {
                this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdICSRetentionPeriodTemp", sqlParameterNext.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CheckICSRetentionPeriodTemp()
        {
            bool strs = false;
            DataTable dataTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();


            dataTable = this.dbContext.GetRecordsAsDataTableSP("spcgICSRetentionPeriodTemp", sqlParameterNext.ToArray());

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

        public List<ICSRetentionPeriodModel> ListIntervalType()
        {
            DataTable resultTable = new DataTable();
            List<ICSRetentionPeriodModel> IntervalType = new List<ICSRetentionPeriodModel>();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgListIntervalType", sqlParameterNext.ToArray());
            if (resultTable.Rows.Count > 0)
            {
                foreach (DataRow row in resultTable.Rows)
                {
                    ICSRetentionPeriodModel type = new ICSRetentionPeriodModel();
                    type.IntType = row["fldIntervalType"].ToString();
                    IntervalType.Add(type);
                }
            }
            return IntervalType;
        }

    }
}
