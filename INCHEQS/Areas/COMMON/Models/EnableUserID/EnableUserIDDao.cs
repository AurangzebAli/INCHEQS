using INCHEQS.Models;
using INCHEQS.Resources;
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
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using System.Text.RegularExpressions;
using INCHEQS.Security;

namespace INCHEQS.Areas.COMMON.Models.EnableUserID
{

    public class EnableUserIDDao : IEnableUserIDDao
    {
        private readonly ApplicationDbContext dbContext;
        //private readonly ISystemProfileDao systemProfileDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        public EnableUserIDDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            //this.systemProfileDao = systemProfileDao;
            //this.securityProfileDao = securityProfileDao;
        }

        public EnableUserIDModel GetEnableUserID(string UserID)
        {
            DataTable resultTable = new DataTable();
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            EnableUserIDModel enable = new EnableUserIDModel();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", UserID));
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgEnableUserID", sqlParameterNext.ToArray());

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                enable.fldUserAbb = row["fldUserAbb"].ToString();
                enable.fldUserDesc = row["fldUserDesc"].ToString();
                enable.fldLastLoginDate = row["fldLastLoginDate"].ToString();
            }
            else
            {
                enable = null;
            }
            return enable;
        }
    
        public bool UpdateEnableUserID(string UserId)
        {

            int intRowAffected;
            bool blnResult = false;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", UserId));

            intRowAffected = dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuEnableUserID", sqlParameterNext.ToArray());
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
    }
}
