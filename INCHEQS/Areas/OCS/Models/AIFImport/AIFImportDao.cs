using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Common;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security.Account;
using INCHEQS.Security;

namespace INCHEQS.Areas.OCS.Models.AIFImport
{
    public class AIFImportDao : IAIFImportDao
    {
        private readonly OCSDbContext ocsdbContext;
        public AIFImportDao(OCSDbContext ocsdbContext)
        {
            this.ocsdbContext = ocsdbContext;
        }

        public async Task<List<AIFImportModel>> GetDataFromFileMasterAsync()
        {
            return await Task.Run(() => GetDataFromFileMaster());
        }

        public AIFImportModel GetDataFromHostFileConfig(string taskId,string bankcode)
        {
            AIFImportModel aifFileModel = new AIFImportModel();
            string stmt = "SELECT * FROM tblHostFileConfig WHERE fldTaskId=@fldTaskId";
            DataTable dt = ocsdbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                aifFileModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                aifFileModel.fldFileExt = row["fldFileExt"].ToString();
                aifFileModel.fldProcessName = row["fldProcessName"].ToString();
                aifFileModel.fldPosPayType = row["fldPosPayType"].ToString();
                return aifFileModel;
            }
            //string stmt = "SELECT * FROM View_AllFile WHERE fldTaskId=@fldTaskId and fldBankcode=@bankcode";
            //DataTable dt = ocsdbContext.GetRecordsAsDataTable(stmt, new[] {
            //    new SqlParameter("@fldTaskId", taskId),
            //    new SqlParameter("@bankcode", bankcode)
            //});
            //if (dt.Rows.Count > 0)
            //{
            //    DataRow row = dt.Rows[0];
            //    aifFileModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
            //    aifFileModel.fldDateSubString = Convert.ToInt32(row["fldDateSubString"]);
            //    aifFileModel.fldBankCodeSubString = Convert.ToInt32(row["fldBankCodeSubString"]);
            //    aifFileModel.fldFileExt = row["fldFileExt"].ToString();
            //    aifFileModel.fldProcessName = row["fldProcessName"].ToString();
            //    aifFileModel.fldPosPayType = row["fldPosPayType"].ToString();
            //    return aifFileModel;
            //}

            return null;
        }

        public List<AIFImportModel> GetDataFromFileMaster()
        {
            List<AIFImportModel> result = new List<AIFImportModel>();
            string stmt = "SELECT * FROM View_AllFile";
            DataTable dt = ocsdbContext.GetRecordsAsDataTable(stmt);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    AIFImportModel FileModel = new AIFImportModel();
                    FileModel.fldFileName = item["fldFileName"].ToString();
                        FileModel.fldFileType = item["fldFileType"].ToString();
                        FileModel.fldTaskId = item["fldTaskId"].ToString();
                        FileModel.fldBankCode = item["fldBankCode"].ToString();

                        result.Add(FileModel);                    

                }
                return result;
            }
                    
            return null;
        }

        public List<String> ValidateCheckBox(FormCollection col)
        {
            List<String> err = new List<String>();

            if (col["Type"].Equals(String.Empty))
            {
                err.Add("Please select atleast one checkbox");
            }
           
            return err;
        }
    }
}