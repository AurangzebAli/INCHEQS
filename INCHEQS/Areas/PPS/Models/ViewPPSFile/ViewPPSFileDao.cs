using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;

using System.Web.Mvc;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace INCHEQS.Areas.PPS.Models.ViewPPSFile
{
    public class ViewPPSFileDao : IViewPPSFileDao
    {
        private readonly ApplicationDbContext dbContext;

        public ViewPPSFileDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public ViewPPSFileModel getPayeeDetail(string chequeNo, string accountNo)
        {
            ViewPPSFileModel detail = new ViewPPSFileModel();

            DataTable ds = new DataTable();
            string stmt = "select * FROM view_ViewPositvePayFile " +
                          "where fldChequeNo = '" + chequeNo + "'" +
                          " AND fldAccNo = '" + accountNo + "' ";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            if (ds.Rows.Count > 0)
            {
                detail.fldIssueDate = ds.Rows[0]["fldIssueDate"].ToString();
                detail.fldAccNo = ds.Rows[0]["fldAccNo"].ToString();
                detail.fldChequeNo = ds.Rows[0]["fldChequeNo"].ToString();
                detail.fldPayeeName = ds.Rows[0]["fldPayeeName"].ToString();
                detail.fldAmount = ds.Rows[0]["fldAmount"].ToString();
                detail.fldStatus = ds.Rows[0]["fldStatus"].ToString();
                detail.validFlag = ds.Rows[0]["ValidFlag"].ToString();
            }

            return detail;
        }
    }
}