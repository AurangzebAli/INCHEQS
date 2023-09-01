//using INCHEQS.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data.SqlClient;

namespace INCHEQS.Areas.OCS.Models.OCSReleaseLockedCheque
{
    public class OCSReleaseLockedChequeDao : OCSIReleaseLockedChequeDao
    {
        private readonly ApplicationDbContext dbContext;
        public OCSReleaseLockedChequeDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DataTable ListLockedCheque(string bankCode)
        {
            DataTable ds = new DataTable();
            //string stmt = "SELECT out.flditeminitialid,fldcapturingdate,flduic,fldserial,fldpvaccno,fldamount,usr.flduserabb FROM view_outwarditem out LEFT JOIN tblusermaster usr on out.fldlockuser = usr.flduserid WHERE /*coalesce(fldapprovalstatus, '') = '' AND*/ coalesce(flduserid,0) <> 0";
            //string stmt = "SELECT out.flditeminitialid,fldcapturingdate,flduic,fldserial,fldpvaccno,fldamount,usr.flduserabb,flduserid,fldlockuser" +
            //              " FROM view_outwarditem out LEFT JOIN tblusermaster usr on out.fldlockuser = usr.flduserid  WHERE  coalesce(fldlockuser,0) <> 0 and substring(fldcapturingbranch,2,3)= @bankCode order by flduic";

            //ds = dbContext.GetRecordsAsDataTable(stmt);
            //ds = dbContext.GetRecordsAsDataTable(stmt, new[] {
            //    new SqlParameter ("@bankCode", bankCode)
            //});
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            sqlParameters.Add(new SqlParameter("@bankCode", bankCode));
            ds = dbContext.GetRecordsAsDataTableSP("spcgListLockedChequeOCS", sqlParameters.ToArray());
            return ds;
        }

        public void DeleteProcessUsingCheckbox(string InwardItemId)
        {
            //string[] aryText = InwardItemId.Split(',');

            DataTable dt = new DataTable();
            //string stmt = "UPDATE tblInwardItemInfoStatus SET fldAssignedUserId = NULL WHERE fldAssignedUserId=@fldAssignedUserId";
            //dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldAssignedUserId", fldAssignedUserId) });

            //if ((aryText.Length > 0))
            //{
            //    for (int i = 0; i < aryText.Length; i++)
            //    {

            //        string stmt2 = "SELECT out.flditemid,out.fldlockuser" +
            //              " FROM view_outwarditem out where out.flditemid::text = @itemID";
                    
            //        dt = dbContext.GetRecordsAsDataTable(stmt2, new[] {
            //        new SqlParameter ("@itemID", aryText[i])
            //      });

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    sqlParameters.Add(new SqlParameter("@itemID", InwardItemId));
                    dt = dbContext.GetRecordsAsDataTableSP("spcgViewLockedChequeOCS", sqlParameters.ToArray());

                    string ardText = dt.Rows[0]["flditemid"].ToString();
                    string areText = dt.Rows[0]["fldlockuser"].ToString();
            //        string stmt3 = "Delete from tbldataentrylock WHERE flditemid::text = @flditemid and fldlockuser::text = @fldlockuser";
                    
            //        dbContext.ExecuteNonQuery(stmt3, new[] {
            //    new SqlParameter ("@flditemid", ardText),

            //        new SqlParameter("@fldlockuser", areText)
            //});
            //    }

                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                sqlParameters1.Add(new SqlParameter("@flditemid", ardText));
                sqlParameters1.Add(new SqlParameter("@fldlockuser", areText));
                dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdReleaseLockedChequeOCS", sqlParameters1.ToArray());


                //string stmt = "UPDATE tbliteminfo SET fldlockuser = NULL WHERE flditemid::text in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ") ;";
                //dbContext.ExecuteNonQuery(stmt, DatabaseUtils.getSqlParametersFromArray(aryText).ToArray());

            }

        public void UpdateReleaseLockedCheque(string InwardItemId)
        {
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@flditemid", InwardItemId));
            dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuReleaseLockedChequeOCS", sqlParameters.ToArray());
        }
    }

        
}