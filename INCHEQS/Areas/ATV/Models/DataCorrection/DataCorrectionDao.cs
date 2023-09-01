using INCHEQS.Areas.ATV.Models.DataCorrection;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Helpers;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Resources;
using System.Collections.Generic;
using System.Data;
//using System.Data.Entity;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace INCHEQS.Models.DataCorrectionDao
{
    public class DataCorrectionDao : IDataCorrectionDao {

        private readonly ApplicationDbContext dbContext;
        public DataCorrectionDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public DataTable getDataCorrectionList()
        {
            DataTable ds = new DataTable();
            string stmt = "select * from tblATVItemInfo WHERE fldProcessStatus = '5' order by fldItemId";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            return ds;
        }
        public string condition()
        {
            return " fldChequeDate = '000000'";
        }
        public DataCorrectionModel getDataCorrectionModel(string itemId)
        {
            DataCorrectionModel dataCorrectionModel = new DataCorrectionModel();
            DataTable ds = new DataTable();
            string stmt = "select * from tblATVInfo where flditemId=@flditemId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@flditemId", itemId) });

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                dataCorrectionModel.itemId = row["fldStateCode"].ToString();
                dataCorrectionModel.UIC = row["fldStateDesc"].ToString();
                dataCorrectionModel.accountNo = row["fldStateDesc"].ToString();
                dataCorrectionModel.chequeNo = row["fldStateDesc"].ToString();
            }
            return dataCorrectionModel;
        }
        public DataTable getPathCropImage(string itemId)
        {
            DataTable ds = new DataTable();
            string stmt = "select * from tblATVItemImage where fldItemId = @fldItemId";
            ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldItemId", itemId) });
            return ds;
        }

        public void Update(FormCollection col)
        {

            string stmt = "UPDATE tblATVItemImage set fldChequeDate=@fldChequeDate where fldItemId = @fldItemId";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("fldChequeDate", col["fldChequeDate"]),
            });
        }
    }
    
}

