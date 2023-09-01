using INCHEQS.DataAccessLayer;
//using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Areas.OCS.Models.BankBranchesOcs
{
    public class BankBranchesOcsDao : IBankBranchesOcsDao
    {
        private readonly ApplicationDbContext dbContext;

        public BankBranchesOcsDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public DataTable GetBankBranchesDataTable(string BankCode)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            if (!string.IsNullOrEmpty(BankCode))
            {
                strQuerySelect = "select fldbranchcode as value, (fldbranchcode || ' - ' || fldbranchdesc) as text from tblbranchmaster where fldbankcode=@fldbankcode and fldactive='Y' order by fldbranchcode asc";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbankcode", BankCode) });
            }
            else
            {
                dt = dbContext.GetDataTableFromSqlWithParameter("select fldbranchcode as value, (fldbranchcode || ' - ' || fldbranchdesc) as text from tblbranchmaster where fldactive='Y' order by fldbranchcode");
            }
            return dt;
        }
        public DataTable GetBankBranchesDataTableWorkStation(string BankCode)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            if (!string.IsNullOrEmpty(BankCode))
            {
                //strQuerySelect = "select fldbranchid || ' - ' || fldbranchdesc as value, (fldbranchcode || ' - ' || fldbranchdesc) as text from tblbranchmaster where fldbankcode=@fldbankcode and fldactive='Y' order by fldbranchcode asc " +
                //    "";

                strQuerySelect = "select CONCAT(CONCAT(fldbranchid,' - '),fldbranchdesc) as 'value',CONCAT(CONCAT(fldbranchcode, ' - '), fldbranchdesc) as 'text' from tblbranchmaster where fldbankcode = @fldbankcode and fldactive = 'Y' order by fldbranchcode asc " +
                    "";

                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbankcode", BankCode) });
            }
            else
            {
                dt = dbContext.GetDataTableFromSqlWithParameter("select fldbranchid || ' - ' || fldbranchdesc as value, (fldbranchcode || ' - ' || fldbranchdesc) as text from tblbranchmaster where fldactive='Y' order by fldbranchcode");
            }
            return dt;
        }
        public DataTable GetBankBranchDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            if (!string.IsNullOrEmpty(BranchId))
            {
                strQuerySelect = " SELECT CONCAT(CONCAT(lm.fldlocationcode,' - '),lm.fldlocationdesc)as fldlocationdesc,CONCAT(CONCAT(bm.fldbankcode, ' - '), bm.fldbankdesc) as fldbankdesc, " +
                                    "CONCAT(CONCAT(cm.fldbranchcode, ' - '), cm.fldbranchdesc) as fldbranchdesc " +
                                    "FROM tblbankmaster bm " +
                                    "inner join tblinternalbranchmaster cm on bm.fldbankcode = cm.fldbankcode " +
                                    "inner join tbllocationmaster lm on substring(cast (cm.fldbranchid as nvarchar), 1,1)= lm.fldlocationcode " +
                                    "where cm.fldbranchid = @fldbranchid ";

                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbranchid", BranchId) });
            }

            return dt;
        }

        public DataTable GetIslamicBankCodeDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            string bankCode = "";

            if (BranchId.Length >= 2)
            {
                bankCode = BranchId.Substring(0, 2).ToString().Trim();

                strQuerySelect = "SELECT CONCAT(CONCAT(fldbankcode,' - '),fldbankdesc) as fldbankdesc " +
                                   " from tblbankmaster " +
                                   " where fldbankcode = @fldbankcode and fldBankType = '03'";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbankcode", bankCode) });
            }

            return dt;
        }

        public DataTable GetConvBankCodeDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            string bankCode = "";

            if (BranchId.Length >= 2)
            {
                bankCode = BranchId.Substring(0, 3).ToString().Trim();

                strQuerySelect = "SELECT CONCAT(CONCAT(fldbankcode,' - '),fldbankdesc) as fldbankdesc " +
                                   " from tblbankmaster " +
                                   " where fldbankcode = @fldbankcode and fldBankType not in ('03')";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbankcode", bankCode) });
            }

            return dt;
        }

        public DataTable GetBankCodeDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            string bankCode = "";

            if (BranchId.Length >= 2)
            {
                bankCode = BranchId.Substring(0, 3).ToString().Trim();

                strQuerySelect = "SELECT CONCAT(CONCAT(fldbankcode,' - '),fldbankdesc) as fldbankdesc " +
                                   " from tblbankmaster " +
                                   " where fldbankcode = @fldbankcode ";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldbankcode", bankCode) });
            }

            return dt;
        }
        public DataTable GetStateCodeDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            string stateCode = "";

            if (BranchId.Length >= 4)
            {
                stateCode = BranchId.Substring(3, 2).ToString().Trim();
                strQuerySelect = "SELECT CONCAT(CONCAT(fldStateCode,' - '),fldStateDesc)as fldStateDesc " +
                                   " from tblStatemaster " +
                                   " where fldStateCode = @fldStateCode";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldStateCode", stateCode) });
            }

            return dt;
        }

        public DataTable GetLocCodeDetails(string BranchId)
        {
            DataTable dt = new DataTable();
            string strQuerySelect = "";
            string locCode = "";

            if (BranchId.Length >= 1)
            {
                locCode = BranchId.Substring(0, 1).ToString().Trim();
                strQuerySelect = "SELECT CONCAT(CONCAT(fldlocationcode,' - '),fldlocationdesc)as fldlocationdesc " +
                                   " from tbllocationmaster " +
                                   " where fldlocationcode = @fldlocationcode";
                dt = dbContext.GetRecordsAsDataTable(strQuerySelect, new[] { new SqlParameter("@fldlocationcode", locCode) });
            }

            return dt;
        }

    }
}