﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Management;
using System.Text.RegularExpressions;
using INCHEQS.Resources;
using INCHEQS.Common;
using System.Data.Entity;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security;

namespace INCHEQS.Areas.OCS.Models.BranchEndOfDaySummary
{
    public class BranchEndOfDaySummaryDao : IBranchEndOfDaySummaryDao
    {
        private readonly ApplicationDbContext dbContext;

        public BranchEndOfDaySummaryDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataTable GetCenterEndOfDay(string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcgEODCenter", SqlParameterNext.ToArray());
            return dt;
        }

        public bool InsertCenterEndOfDay(string strUserId, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            //SqlParameterNext.Add(new SqlParameter("@fldEODStatus", "1"));
            //SqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
            SqlParameterNext.Add(new SqlParameter("@fldEODStatus", "Y"));
            SqlParameterNext.Add(new SqlParameter("@fldCreatedBy", strUserId));
            SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
            SqlParameterNext.Add(new SqlParameter("@fldUpdatedBy", strUserId));
            SqlParameterNext.Add(new SqlParameter("@fldUpdatedDateTime", DateTime.Now));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spciEODCenter", SqlParameterNext.ToArray());
            return true;
        }

        public bool ReleaseBranchEndOfDay(string strBranchId, string strBankCode)
        {
            DataTable dt = new DataTable();
            List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
            SqlParameterNext.Add(new SqlParameter("@fldBranchId", strBranchId));
            SqlParameterNext.Add(new SqlParameter("@fldActive", "Y"));
            SqlParameterNext.Add(new SqlParameter("@fldCreatedDateTime", DateTime.Now));
            SqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            dt = dbContext.GetRecordsAsDataTableSP("spcdEODBranch", SqlParameterNext.ToArray());
            return true;
        }

        public bool CheckBranchNotYetEOD(string strBankCode)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgBranchNotYetEOD", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public DataTable GetPendingBranchesInfo(string strBankCode)
        {
                DataTable dt = new DataTable();
                List<SqlParameter> SqlParameterNext = new List<SqlParameter>();
                dt = dbContext.GetRecordsAsDataTableSP("spcgEODCenterReady", SqlParameterNext.ToArray());
                return dt;
        }
    }
}