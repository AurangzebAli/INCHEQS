using INCHEQS.Areas.OCS.Models.OCSAIFFileUpload;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.DataAccessLayer;

namespace INCHEQS.Models.OCSAIFFileUploadDao
{
    public class OCSAIFFileUploadDao : IOCSAIFFileUploadDao
    {

        private readonly ApplicationDbContext dbContext;
        public OCSAIFFileUploadDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }       

        public OCSAIFFileUploadModel GetDataFromHostFileConfig(string taskId) {
            OCSAIFFileUploadModel hostFileModel = new OCSAIFFileUploadModel();
            string stmt = "SELECT * FROM tblHostFileConfig WHERE fldTaskId=@fldTaskId";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
            if (dt.Rows.Count > 0) {
                DataRow row = dt.Rows[0];
                hostFileModel.fldProcessName = row["fldProcessName"].ToString();
                hostFileModel.fldHostFileDesc = row["fldHostFileDesc"].ToString();
                hostFileModel.fldPosPayType = row["fldPosPayType"].ToString();
                hostFileModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                hostFileModel.fldFileExt = row["fldFileExt"].ToString();
                hostFileModel.fldTaskRole = row["fldTaskRole"].ToString();
                hostFileModel.fldFTPFolder = row["fldFTPFolder"].ToString();
                return hostFileModel;
            }

            return null;
        }
              
    }
}