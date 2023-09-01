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
using System.Net;
using System.IO;

namespace INCHEQS.Areas.ICS.Models.LoadBankHostFile2nd
{
    public class LoadBankHostFile2ndDao : ILoadBankHostFile2ndDao
    {

        private readonly ApplicationDbContext dbContext;


        public LoadBankHostFile2ndDao(ApplicationDbContext dbContext)
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
            return dbContext.GetRecordsAsDataTableSP("spcgLoadBankHostFiles2ndItems", sqlParameterNext.ToArray());
        }

        public string ListFolderPathFrom()
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "RepairedHostStatusFile"));
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
            sqlParameterNext.Add(new SqlParameter("@_InterfaceFileType ", "RepairedHostStatusFile"));
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


        public LoadBankHostFile2ndModel GetDataFromMICRImportConfig(string taskId, string bankcode)
        {
            LoadBankHostFile2ndModel loadNcfModel = new LoadBankHostFile2ndModel();
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

        public List<LoadBankHostFile2ndModel> GetLoadBankHostSummary(string clearDateDD)
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            List<LoadBankHostFile2ndModel> result = new List<LoadBankHostFile2ndModel>();
            sqlParameterNext.Add(new SqlParameter("@clearDate", clearDateDD));

            DataTable ds = new DataTable();
            ds = dbContext.GetRecordsAsDataTableSP("spcgGetLoad2ndBankHostSummary", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                foreach (DataRow item in ds.Rows)
                {
                    LoadBankHostFile2ndModel progressStatus = new LoadBankHostFile2ndModel();

                    progressStatus.totalInward = item["totalInward"].ToString();
                    progressStatus.totalHostItem = item["totalHostItem"].ToString();
                    progressStatus.totalUpdated = item["totalUpdated"].ToString();
                    progressStatus.totalError = item["totalError"].ToString();

                    result.Add(progressStatus);
                }
                return result;
            }
            return null;
        }


        public string GetFTPUserName()
        {
            DataTable dt = new DataTable();
            string ftpUsername = "";


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode", "FTPUserName"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                ftpUsername = row["fldSystemProfileValue"].ToString();

            }
            return ftpUsername;
        }

        public string GetFTPPassword()
        {
            DataTable dt = new DataTable();
            string ftpPassword = "";


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@profileCode", "FTPPassword"));
            dt = dbContext.GetRecordsAsDataTableSP("spcgSystemProfile", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                ftpPassword = row["fldSystemProfileValue"].ToString();

            }
            return ftpPassword;
        }

        public string GetInterfaceFileName(string interfaceFile)
        {
            DataTable dt = new DataTable();
            string interfaceFileName = "";


            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@interfaceFile", interfaceFile));
            dt = dbContext.GetRecordsAsDataTableSP("spcgGetInterfaceFileName", sqlParameterNext.ToArray());
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                interfaceFileName = row["fldInterfaceFileName"].ToString();

            }
            return interfaceFileName;
        }

        public bool DownloadFile(string url, string userid, string password, string fileDestPath, string FileName, ref string Message)
        {
            bool flag1 = false;
            try
            {
                FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(url + FileName);
                ftpWebRequest.Credentials = (ICredentials)new NetworkCredential(userid, password);
                ftpWebRequest.Method = "RETR";
                FtpWebResponse response1 = (FtpWebResponse)ftpWebRequest.GetResponse();
                Message += response1.StatusDescription;
                Stream responseStream = ftpWebRequest.GetResponse().GetResponseStream();
                int contentLength = checked((int)ftpWebRequest.ContentLength);
                byte[] numArray = new byte[checked(contentLength + 1)];
                int num1 = checked(contentLength - 1);
                int index = 0;
                while (index <= num1)
                {
                    numArray[index] = checked((byte)responseStream.ReadByte());
                    checked { ++index; }
                }
                byte[] buffer = new byte[1024];
                Stream stream = (Stream)System.IO.File.Create(fileDestPath + FileName);
                int count = 1;
                int num2 = 0;
                while (count >= 1)
                {
                    count = responseStream.Read(buffer, 0, 1024);
                    if (count > 0)
                    {
                        stream.Write(buffer, 0, count);

                        checked { num2 = num2 + count; }
                    }
                }
                stream.Close();
                responseStream.Close();
                FtpWebResponse response2 = (FtpWebResponse)ftpWebRequest.GetResponse();
                Message += response2.StatusDescription.ToString();
                response2.Close();
                flag1 = true;
            }
            catch (Exception ex)
            {

            }
            return flag1;
        }

    }
}