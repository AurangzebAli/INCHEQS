using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.Sequence;
using System.Collections.Generic;
using INCHEQS.DataAccessLayer;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;

namespace INCHEQS.Areas.OCS.Models.ChequeImage  
{
    public class OCSChequeImageDao : IOCSChequeImageDao
    {

        private readonly ApplicationDbContext dbContext;
        private readonly ISequenceDao sequenceDao;
        private readonly IAuditTrailDao auditTrailDao;

        public OCSChequeImageDao(ApplicationDbContext dbContext, ISequenceDao sequenceDao, IAuditTrailDao auditTrailDao)
        {
            this.dbContext = dbContext;
            this.sequenceDao = sequenceDao;
            this.auditTrailDao = auditTrailDao;
        }


        public DataTable GetImageByte(string uic)
        {

            DataTable ds = new DataTable();
            string stmt = "select fldGFrontIMGCode as fldgfrontimgbt, fldGBackIMGCode as fldgbackimgbt, fldFrontIMGCode as fldfrontimgbt, fldBackIMGCode as fldbackimgbt,fldUVIMGCode as flduvimgbt  from tbliteminitial where fldUIC = @flduic";
            List <SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@flduic", uic.Trim()));
            ds = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());

            return ds;
        }

        public DataTable GetImageByte(string uic, FormCollection col)
        {
            List<string> states = col["imageState"].Split(',').ToList();
            DataTable ds = new DataTable();
            string mySQL = "";
            if (states != null)
            {
                if (states.Contains("bw") && states.Contains("front"))
                {
                    mySQL = " OMI.fldFrontIMGCode as fldImageCode ";
                }
                else if (states.Contains("bw") && states.Contains("back"))
                {
                    mySQL = " OMI.fldBackIMGCode as fldImageCode ";
                }
                else if (states.Contains("greyscale") && states.Contains("front"))
                {
                    mySQL = " OMI.fldGFrontIMGCode as fldImageCode ";
                }
                else if (states.Contains("greyscale") && states.Contains("back"))
                {
                    mySQL = " OMI.fldGBackIMGCode as fldImageCode ";
                }
                else if (states.Contains("uv"))
                {
                    mySQL = " OMI.fldUVIMGCode as fldImageCode ";
                }
                else
                {
                    mySQL = " OMI.fldGFrontIMGCode as fldImageCode ";
                }
            }
            else
            {
                mySQL = " OMI.fldGFrontIMGCode as fldImageCode ";
            }

            string stmt = "SELECT " + mySQL + " From tblOCSMICRImage OMI INNER JOIN tbliteminitial ini ON OMI.fldUIC = ini.fldUIC ";
            stmt = stmt + " WHERE  OMI.fldUIC = @flduic";
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            sqlParams.Add(new SqlParameter("@flduic", uic.Trim()));
            ds = dbContext.GetRecordsAsDataTable(stmt, sqlParams.ToArray());

            return ds;
        }
    }
}