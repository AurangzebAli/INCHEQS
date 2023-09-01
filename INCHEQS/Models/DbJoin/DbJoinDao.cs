using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using System.Data;
using System.Data.SqlClient;

namespace INCHEQS.Models.DbJoin
{
    public class DbJoinDao : IDbJoinDao
    {
        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;

        public DbJoinDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
        }
        
        public DataTable GetCommonDatableFromOCS(string strQueryCommon, SqlParameter[] paramsCommon, string fldCommon, string strQueryOCS, SqlParameter[] paramsOCS, string fldOCS)
        {
            DataTable dtResult = new DataTable();
            DataTable dtOCS = new DataTable();
            DataTable dtCommon = new DataTable();

            //Step 1 : Select items from Common database
            dtCommon = this.dbContext.GetRecordsAsDataTable(strQueryCommon, paramsCommon);

            //Step 2 : Select items from OCS database
            dtOCS = this.ocsdbContext.GetRecordsAsDataTable(strQueryOCS, paramsOCS);

            //Step 3 : Join the two DataTable
            var query = (from common in dtCommon.AsEnumerable()
                         join ocs in dtOCS.AsEnumerable()
                         on common.Field<string>(fldCommon).Trim() equals ocs.Field<string>(fldOCS).Trim()
                         select common).ToList();

            //Step 4 : Convert to DataTable
            if (query.Count > 0)
            {
                dtResult = query.CopyToDataTable();
            }

            //Step 5 : Return DataTable result
            return dtResult;
        }
    }
}