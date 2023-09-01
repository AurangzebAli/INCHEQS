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

namespace INCHEQS.Areas.ICS.Models.LoadBankHostFile
{
    public class LoadBankHostFileDao : ILoadBankHostFileDao
    {

        private readonly ApplicationDbContext dbContext;


        public LoadBankHostFileDao(ApplicationDbContext dbContext)
        {

            this.dbContext = dbContext;
        }

        public DataTable InwardItemList(FormCollection collection)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            string ProcessDate = DateTime.ParseExact(collection["row_fldprocessdate"], "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", ProcessDate));
            sqlParameterNext.Add(new SqlParameter("@fldFileName", collection["row_fldFileName"]));
            sqlParameterNext.Add(new SqlParameter("@fldBankHostFolderId", collection["row_fldBankHostFolderId"]));
            return dbContext.GetRecordsAsDataTableSP("spcgLoadBankHostFilesItems", sqlParameterNext.ToArray());
        }

        public string ListFolderPathFrom()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "HostStatusFile"));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", CurrentUser.Account.BankCode));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldInterfaceFileSourcePath"].ToString();
            }
            else
            {
                Path = "Path not found in DB";
            }
            return Path;
        }

        public string ListFolderPathTo()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "HostStatusFile"));
            sqlParameterNext.Add(new SqlParameter("@_BankCode", CurrentUser.Account.BankCode));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgInterfaceFileMasterICS", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldInterfaceFileDestPath"].ToString();
            }
            else
            {
                Path = "Local Path not found in DB";
            }
            return Path;
        }


        public LoadBankHostFileModel GetDataFromMICRImportConfig(string taskId, string bankcode)
        {
            LoadBankHostFileModel loadNcfModel = new LoadBankHostFileModel();
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

        public List<LoadBankHostFileModel> ReturnProgressStatusConventional(string clearDateDD)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<LoadBankHostFileModel> result = new List<LoadBankHostFileModel>();
            sqlParameterNext.Add(new SqlParameter("@fileName", DateTime.Parse(clearDateDD).ToString("yyyyMMdd")));
            sqlParameterNext.Add(new SqlParameter("@clearDate", DateTime.Parse(clearDateDD)));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgBankHostFileProgressStatus", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    LoadBankHostFileModel progressStatus = new LoadBankHostFileModel();

           
        
                    progressStatus.totalInwardRec = item["totalInwardRec"].ToString();
                    progressStatus.totalBankHostItem = item["totalBankHostItem"].ToString();
                    progressStatus.totalRecUpdated = item["totalRecUpdated"].ToString();
                    progressStatus.totalAccountNoFound = item["totalAccountNoFound"].ToString();
                    progressStatus.totalAccountTwoStatus = item["totalAccountTwoStatus"].ToString();
                    progressStatus.totalError = item["totalError"].ToString();
                    //progressStatus.TotalItemTaggedC = item["TotalItemTaggedC"].ToString();
                    //progressStatus.TotalItemUntaggedC = item["TotalItemUntaggedC"].ToString();
                    //progressStatus.TotalInsufficientFundC = item["TotalInsufficientFundC"].ToString();
                    //progressStatus.Total1stPresentedChequeC = item["Total1stPresentedChequeC"].ToString();
                    //progressStatus.TotalDataCorrectionsC = item["TotalDataCorrectionsC"].ToString();
                    //progressStatus.TotalUnpostedC = item["TotalUnpostedC"].ToString();
                    //progressStatus.TotalRejectItemC = item["TotalItemLoadC"].ToString();

                    //progressStatus.TotalItemLoadI = item["TotalItemLoadI"].ToString();
                    //progressStatus.TotalItemTaggedI = item["TotalItemTaggedI"].ToString();
                    //progressStatus.TotalItemUntaggedI = item["TotalItemUntaggedI"].ToString();
                    //progressStatus.TotalInsufficientFundI = item["TotalInsufficientFundI"].ToString();
                    //progressStatus.Total1stPresentedChequeI = item["Total1stPresentedChequeI"].ToString();
                    //progressStatus.TotalDataCorrectionsI = item["TotalDataCorrectionsI"].ToString();
                    //progressStatus.TotalUnpostedI = item["TotalUnpostedI"].ToString();
                    //progressStatus.TotalRejectItemI = item["TotalRejectItemI"].ToString();

                    //Michelle
                    result.Add(progressStatus);
                }
                return result;
            }
            return null;
        }

        public List<LoadBankHostFileModel> ReturnProgressStatusIslamic(string clearDateDD)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<LoadBankHostFileModel> result = new List<LoadBankHostFileModel>();
            sqlParameterNext.Add(new SqlParameter("@fileName", "IH"));
            sqlParameterNext.Add(new SqlParameter("@clearDate", DateTime.Parse(clearDateDD)));
            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgBankHostFileProgressStatus", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    LoadBankHostFileModel progressStatusIslam = new LoadBankHostFileModel();
                    
                    //progressStatus.TotalItemLoadC = item["TotalItemLoadC"].ToString();
                    //progressStatus.TotalItemTaggedC = item["TotalItemTaggedC"].ToString();
                    //progressStatus.TotalItemUntaggedC = item["TotalItemUntaggedC"].ToString();
                    //progressStatus.TotalInsufficientFundC = item["TotalInsufficientFundC"].ToString();
                    //progressStatus.Total1stPresentedChequeC = item["Total1stPresentedChequeC"].ToString();
                    //progressStatus.TotalDataCorrectionsC = item["TotalDataCorrectionsC"].ToString();
                    //progressStatus.TotalUnpostedC = item["TotalUnpostedC"].ToString();
                    //progressStatus.TotalRejectItemC = item["TotalItemLoadC"].ToString();

                    progressStatusIslam.TotalItemLoadI = item["TotalItemLoadC"].ToString();
                    progressStatusIslam.TotalItemTaggedI = item["TotalItemTaggedC"].ToString();
                    progressStatusIslam.TotalItemUntaggedI = item["TotalItemUntaggedC"].ToString();
                    progressStatusIslam.TotalInsufficientFundI = item["TotalInsufficientFundC"].ToString();
                    progressStatusIslam.Total1stPresentedChequeI = item["Total1stPresentedChequeC"].ToString();
                    progressStatusIslam.TotalDataCorrectionsI = item["TotalDataCorrectionsC"].ToString();
                    progressStatusIslam.TotalUnpostedI = item["TotalUnpostedC"].ToString();
                    progressStatusIslam.TotalRejectItemI = item["TotalItemLoadC"].ToString();

                    //Michelle
                    result.Add(progressStatusIslam);
                }
                return result;
            }
            return null;
        }

    }
}