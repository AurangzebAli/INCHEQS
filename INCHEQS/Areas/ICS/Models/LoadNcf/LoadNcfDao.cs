using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Security.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using INCHEQS.Security;
using INCHEQS.Resources;
using System.Globalization;

namespace INCHEQS.Areas.ICS.Models.LoadNcf
{
    public class LoadNcfDao : ILoadNcfDao
    {

        private readonly ApplicationDbContext dbContext;


        public LoadNcfDao(ApplicationDbContext dbContext)
        {

            this.dbContext = dbContext;
        }

        public DataTable InwardItemList(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string ProcessDate = DateTime.ParseExact(collection["row_fldprocessdate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", ProcessDate));
            sqlParameterNext.Add(new SqlParameter("@fldFileName", collection["row_fldFileName"]));
            sqlParameterNext.Add(new SqlParameter("@fldNonConformanceFileId", collection["row_fldNonConformanceFileId"]));
            return dbContext.GetRecordsAsDataTableSP("spcgNcfItems", sqlParameterNext.ToArray());
        }

        public string ListFolderPathFrom()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "NCFFile"));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", CurrentUser.Account.BankCode));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldInterfaceFileSourcePath"].ToString();
            }
            else
            {
                Path = "MICR GWC Path not found in DB";
            }
            return Path;
        }

        public string ListFolderPathTo()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "NCFFile"));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", CurrentUser.Account.BankCode));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldInterfaceFileDestPath"].ToString();
            }
            else
            {
                Path = "NCF Local Path not found in DB";
            }
            return Path;
        }


        public LoadNcfModel GetDataFromMICRImportConfig(string taskId, string bankcode)
        {
            LoadNcfModel loadNcfModel = new LoadNcfModel();
            //string stmt = "SELECT * FROM tblMICRImportConfig WHERE fldTaskId=@fldTaskId and fldBankCode=@BankCode";
            string stmt = "SELECT * FROM tblMICRImportConfig WHERE fldTaskId=@fldTaskId ";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldTaskId", taskId)
                //,new SqlParameter("@BankCode", bankcode)
            });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                loadNcfModel.fldSystemProfileCode = row["fldSystemProfileCode"].ToString();
                loadNcfModel.fldSystemProfileCode2 = row["fldSystemProfileCode2"].ToString();
                loadNcfModel.fldDateSubString = Convert.ToInt32(row["fldDateSubString"]);
                loadNcfModel.fldBankCodeSubString = Convert.ToInt32(row["fldBankCodeSubString"]);
                loadNcfModel.fldDateSubStringCompleted = Convert.ToInt32(row["fldDateSubStringCompleted"]);
                loadNcfModel.fldBankCodeSubStringCompleted = Convert.ToInt32(row["fldBankCodeSubStringCompleted"]);
                loadNcfModel.fldFileExt = row["fldFileExt"].ToString();
                loadNcfModel.fldProcessName = row["fldProcessName"].ToString();
                loadNcfModel.fldPosPayType = row["fldPosPayType"].ToString();
                return loadNcfModel;
            }
            return null;
        }

        public void Update(string bankcode)
        {

            string stmt = "Update tblExtractServer set fldExtracted = 1 where fldBankCode=@fldBankCode and fldActive = 'Y'";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldBankCode",bankcode)
            });

        }


    }
}