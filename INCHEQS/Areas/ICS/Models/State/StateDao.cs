using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.State {
    public class StateDao : IStateDao {

        private readonly ApplicationDbContext dbContext;
        public StateDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable getAllState() {
            DataTable ds = new DataTable();

            string stmt = "SELECT  fldStatecode,fldStateDesc FROM tblStatemaster";

            ds = dbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }

    //    public DataTable GetAllIssuingBankBranch()
    //    {
    //        DataTable ds = new DataTable();

    //    string stmt = "select fldBranchId,fldBranchDesc from tblInternalBranchMaster bm inner join tblBankMaster bmz on bm.fldBankcode = bmz.fldBankCode";

    //    ds = dbContext.GetRecordsAsDataTable(stmt);

    //        return ds;
    //}

        public DataTable GetAllIssuingBankBranch(AccountModel currentUser)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldUserId", currentUser.UserId));
            
            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetIssuingBranch", sqlParameterNext.ToArray());


            return resultTable;
        }

        // xx start 20210426
        public DataTable GetIssueBankBranch(string bankCode)
        {
            DataTable resultTable = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldIssueBankCode", bankCode.Trim()));

            resultTable = dbContext.GetRecordsAsDataTableSP("spcgGetIssueBankBranch", sqlParameterNext.ToArray());


            return resultTable;
        }
        // xx end 20210426

        public DataTable GetAllPresentingBankBranch()
        {
            DataTable ds = new DataTable();

            string stmt = "select fldBranchId, fldBranchDesc from tblBranchMaster bm inner join tblBankMaster bmz on bm.fldBankcode = bmz.fldBankCode where bm.fldActive = 'Y'";

            ds = dbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }


        public DataTable GetReturnReason()
        {
            DataTable ds = new DataTable();

            string stmt = "select fldRejectCode,fldRejectDesc,* from tblRejectMaster where fldActive = 'Y'";

            ds = dbContext.GetRecordsAsDataTable(stmt);

            return ds;
        }


    }
}