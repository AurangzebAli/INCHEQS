using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data.SqlClient;

namespace INCHEQS.Areas.ICS.Models.ICSReleaseLockedCheque
{
    public class ICSReleaseLockedChequeDao : ICSIReleaseLockedChequeDao
    {
        private readonly ApplicationDbContext dbContext;
        public ICSReleaseLockedChequeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataTable ListLockedCheque(string bankCode)
        {
            DataTable ds = new DataTable();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@fldBankCode", bankCode));
            ds = dbContext.GetRecordsAsDataTableSP("spcgListLockedChequeICS", sqlParameters.ToArray());
            return ds;
        }

        public void DeleteProcessUsingCheckbox(string InwardItemId)
        {
            List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
            sqlParameters1.Add(new SqlParameter("@fldInwardItemId", InwardItemId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdReleaseLockedChequeICS", sqlParameters1.ToArray());
            
        }

        public void UpdateReleaseLockedCheque(string InwardItemId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@fldInwardItemId", InwardItemId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuReleaseLockedChequeICS", sqlParameters.ToArray());
        }
    }


}