using INCHEQS.Common;
using INCHEQS.Helpers;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.DataAccessLayer;

using System.Globalization;



namespace INCHEQS.Models.FileInformation {
    public class FileInformationService {
        private readonly ApplicationDbContext dbContext;
        private IFileManagerDao fileManagerDao;
        public FileInformationService(ApplicationDbContext dbContext, IFileManagerDao fileManagerDao)
        {
            this.dbContext = dbContext;
            this.fileManagerDao = fileManagerDao;
        }

        public List<FileInformationModel> GetFileInFolder(string folderName, string selectedDate, int dateSubString, int bankSubString, string fileExt, string bankCode) {

            List<FileInformationModel> filesList = new List<FileInformationModel>();
            string[] filePath = Directory.GetFiles(folderName);
            string convertedDate = DateUtils.formatDateToFileDate(selectedDate);
            string fileDate = "";
            string bankCodeFromFile = "";
            int strDateLength = 8; //20161231 (8 char length)

            foreach (var item in filePath) {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                fileInformationModel.fileName = Path.GetFileName(item);
                if (fileInformationModel.fileName.Length >= dateSubString.ToString().Length + strDateLength) {
                    //Get date from file name
                    //fileDate = fileInformationModel.fileName.Substring(dateSubString, strDateLength);
                    fileDate = item.Substring(dateSubString, 8);
                    //Get bank code from file name (e.g 029.ICL)
                    //bankCodeFromFile = fileInformationModel.fileName.Substring(bankSubString, 3);
                    bankCodeFromFile = item.Substring(bankSubString, 3);

                }
                else {
                        fileDate = Locale.NoDateInFileName;
                     }

                //(fileDate.Equals(convertedDate) &&
                if  (bankCodeFromFile.Equals(bankCode) && fileExt.Equals(fileInfo.Extension) && convertedDate.Equals(fileDate)) {
                    fileInformationModel.fileSize = (Convert.ToDecimal(fileInfo.Length) / 1024).ToString("0.00");
                    DateTime creationTime = fileInfo.CreationTime;
                    fileInformationModel.fileTimeStamp = DateUtils.formatTimeStampFromSql(creationTime.ToString());
                    filesList.Add(fileInformationModel);
                }
            }
            return filesList;
        }
        public List<FileInformationModel> GetAllFileInFolder(string folderName, int bankSubString, string bankCode)
        {
            
            List<FileInformationModel> filesList = new List<FileInformationModel>();
            //string[] filePath = Directory.GetFiles(folderName);
            //string fileDate = "";
            //string bankCodeFromFile = "";
            string[] filePath = Directory.GetDirectories(folderName);

            foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                fileInformationModel.fileName = item.ToString();
                //if (fileInformationModel.fileName.Length >= bankSubString + 3) {
                //    //Get bank code from file name (e.g 029.ICL)
                //    bankCodeFromFile = fileInformationModel.fileName.Substring(bankSubString, 3);
                //} else { fileDate = Locale.NoDateInFileName; }
                //if (bankCodeFromFile.Equals(bankCode)) {

                fileInformationModel.fileSize = (Convert.ToDecimal(item.Length) / 1024).ToString("0.00");

                //   DateTime creationTime = fileInfo.CreationTime;
                //   fileInformationModel.fileTimeStamp = DateUtils.formatTimeStampFromSql(creationTime.ToString());

                fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                filesList.Add(fileInformationModel);
                //}
            }
            return filesList;
        }

        public List<FileInformationModel> GetAllFileInFolderICS(string folderNameFrom, string folderName, string completefolderName,string bankcode, int dateSubString, int bankSubString, int dateSubStringCompleted, int bankSubStringCompleted, string clearDate, string fileExt)
        {
            
            string filedate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

            List<FileInformationModel> filesList = new List<FileInformationModel>();
            string[] filePath = Directory.GetFiles(folderNameFrom);
            foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                fileInformationModel.fileName = item.ToString();
                fileInformationModel.fileNameOnly = Path.GetFileName(item.ToString());
                fileInformationModel.ICLfileDate = item.Substring(dateSubString, 8);
                fileInformationModel.ICLbankCode = item.Substring(bankSubString, 2);
                fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                fileInformationModel.fileSize = (Convert.ToDecimal(fileInfo.Length) / 1024).ToString("0.00");
                fileInformationModel.fileStatus = checkICLUploadStatus(item.ToString());
                fileInformationModel.fileClearDate = filedate;

                if (fileInformationModel.ICLbankCode.Equals(bankcode) &&  fileExt.Equals(fileInfo.Extension))
                {
                    filesList.Add(fileInformationModel);
                    //InsertFileListICS(fileInformationModel.fileNameOnly, fileInformationModel.fileName, fileInformationModel.ICLbankCode,fileInformationModel.fileSize, fileInformationModel.fileClearDate, fileInformationModel.fileTimeStamp);
                }
                
            }
          
            return filesList;

        }

        public List<FileInformationModel> GetAllFileInFolderICSDP(string folderName, int bankSubString, string bankCode, string UserId)
        {

            List<FileInformationModel> filesList = new List<FileInformationModel>();

            if (Directory.Exists(folderName))
            {


                string[] filePath = Directory.GetFiles(folderName);
                string fileDate = "";
                if (filePath != null)
                {
                    foreach (var item in filePath)
                    {
                        FileInformationModel fileInformationModel = new FileInformationModel();
                        FileInfo fileInfo = new FileInfo(item);
                        string exactFileName = Path.GetFileName(item.ToString());

                        fileInformationModel.fileName = item.ToString();
                        fileInformationModel.fileNameOnly = exactFileName;
                        fileInformationModel.filebankcode = exactFileName.Substring(2, 2);
                        fileInformationModel.fileBankType = exactFileName.Substring(0, 2);
                        fileDate = exactFileName.Substring(4, 8);
                        fileDate = DateTime.ParseExact(fileDate, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                        //long sizeFolder = Convert.ToInt64(fileInformationModel.fileSize) / 1024;
                        fileInformationModel.fileSize = (Convert.ToDecimal(fileInfo.Length.ToString()) / 1024).ToString("0.00");

                        fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                        filesList.Add(fileInformationModel);
                        InsertFileListICSDP(fileInformationModel.fileNameOnly, folderName, fileInformationModel.filebankcode, fileInformationModel.fileBankType, fileInformationModel.fileSize, fileDate, fileInformationModel.fileTimeStamp, "ICSGenerateDebitFile", UserId);
                    }
                }
                else
                {
                    filesList = null;
                }
            }
            else
            {
                filesList = null;
            }

            return filesList;

        }

        public long GetDirectorySize(string folderPath,string fileExt)
        {
           

            try
            {

                if (Directory.Exists(folderPath))
                {
                    DirectoryInfo di = new DirectoryInfo(folderPath);
                    return di.EnumerateFiles("*" + fileExt, SearchOption.AllDirectories).Sum(fi => fi.Length);
                }
                else
                {
                    return 0;
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
        }

        public int GetDirectoryCount(string folderPath, string fileExt)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*" + fileExt, SearchOption.AllDirectories).Count();
        }

        public List<FolderInformationModel>GetGWCFolderICS(string folderPath, string clearDate,string fileExt)
        {
            string folderDate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyyMMdd");
            string fullFolderPath = Path.Combine(folderPath.TrimEnd('\"'),folderDate);

            List<FolderInformationModel> folderList = new List<FolderInformationModel>();
            FolderInformationModel folderInformationModel = new FolderInformationModel();
            FileInfo fileInfo = new FileInfo(fullFolderPath);

            if (Directory.Exists(fullFolderPath))
            {
                folderInformationModel.folderName = Path.GetFileName(fullFolderPath.ToString());
                string strsizeFolder = GetDirectorySize(fullFolderPath, fileExt).ToString();
                long sizeFolder = Convert.ToInt64(strsizeFolder) / 1024;
                folderInformationModel.folderSize = sizeFolder.ToString();
                folderInformationModel.folderTimeStamp = fileInfo.CreationTime.ToString();
                folderList.Add(folderInformationModel);

            }
            return folderList;
        }

        //Get Destination Path (bella)
        public string getDestinationPath(string interfaceFile)
        {
            string Path = "";

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldInterfaceFile", interfaceFile));

            DataTable recordsAsDataTable = dbContext.GetRecordsAsDataTableSP("spcgDestinationPath", sqlParameterNext.ToArray());
            if (recordsAsDataTable.Rows.Count > 0)
            {
                Path = recordsAsDataTable.Rows[0]["fldInterfaceFIleDestPath"].ToString();
            }
            else
            {
                Path = "Destination Path not found in DB";
            }
            return Path;
        }

        public List<FileInformationModel> GetAllFileInFolderOCS(string folderName, string completefolderName, int bankSubString, string bankCode, string clearDate)
        {
            
            string dateClearing = DateTime.ParseExact(clearDate, "dd-mm-yyyy", CultureInfo.InvariantCulture).ToString("yyyymmdd");
            string filename = "";
            string filesize = "";
            string fileTimeStamp = "";

            List<FileInformationModel> filesList = new List<FileInformationModel>();
            //string[] filePath = Directory.GetFiles(folderName);
            string fileDate = "";
            string bankCodeFromFile = "";
            string dateFile = "";
            string dateFromFile = "";
            //string bankCodeDesc = "";
            string bankCodeDesc1 = "";
            string fileNameWith1 = "";
            string filecode = "";
            string fileClearDate1 = "";
            //string[] filePath = Directory.GetDirectories(folderName);
            string[] filePath = Directory.GetFiles(folderName);

            //DataTable bankCodeDesc = fileManagerDao.getBankDesc(bankCode);

            //string bankdesctry =null;
            //{
            //    string bankdesc = row["fldbankdesc"].ToString();
            //    bankdesctry = bankdesc;
            //}
     
           
                foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                fileInformationModel.fileName = item.ToString();
                //if (fileInformationModel.fileName.Length >= bankSubString + 3) {
                //    //Get bank code from file name (e.g 029.ICL)
                //    bankCodeFromFile = fileInformationModel.fileName.Substring(bankSubString, 3);
                //} else { fileDate = Locale.NoDateInFileName; }
                //if (bankCodeFromFile.Equals(bankCode)) {

                fileInformationModel.fileSize = (Convert.ToDecimal(fileInfo.Length) / 1024).ToString("0.00");
                filesize = fileInformationModel.fileSize;
                //   DateTime creationTime = fileInfo.CreationTime;
                //   fileInformationModel.fileTimeStamp = DateUtils.formatTimeStampFromSql(creationTime.ToString());
                //fileInformationModel.fileStatus = 

                //fileInformationModel.fileStatus = checkICLUploadStatus(item.ToString());
                fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                filesList.Add(fileInformationModel);
                //}
            }
            if (Directory.Exists(folderName + dateClearing))
            {

                DataTable bankCodeDesc = fileManagerDao.getBankDesc(bankCode);

                string bankdesctry = null;
                foreach (DataRow row in bankCodeDesc.Rows)
                {
                    string bankdesc = row["fldbankdesc"].ToString();
                    bankdesctry = bankdesc;
                }


                string[] completefilePath = Directory.GetFiles(folderName + dateClearing);
                foreach (var item in completefilePath)
                {
                    FileInformationModel fileInformationModel = new FileInformationModel();
                    FileInfo fileInfo = new FileInfo(item);
                    fileInformationModel.fileName = item.ToString();
                    filename = fileInformationModel.fileName;
                    string clearDate1 = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");


                    if (fileInformationModel.fileName.Length == filename.Length)
                    {
                        //Get bank code from file name (e.g 029.ICL)   strBankcodefrmICL = FileName.Substring(11, 3);
                        //fileInformationModel.fileName.Substring(84, 3);
                  
                        string filepath = fileInformationModel.fileName;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filepath);
                        bankCodeFromFile = fileNameWithoutExt.Substring(fileNameWithoutExt.Length - 3, 3);

                        string fileNameWith = Path.GetFileName(filepath);
                        fileInformationModel.fileNameOnly = fileNameWith;

                        fileNameWith1 = fileNameWith;

                        dateFile = fileNameWithoutExt.Substring(2, fileNameWithoutExt.Length - 2);
                        dateFromFile = dateFile.PadRight(8).Substring(0, 8).TrimEnd();
                    }
                    else { fileDate = Locale.NoDateInFileName; }
                    if (bankCodeFromFile.Equals(bankCode) && dateFromFile.Equals(dateClearing))
                    {

                      bankCodeDesc1 = bankdesctry;
                        //bankCodeDesc1 = = fileInfo.bankCode
                     //fileInformationModel.filebankcode = bankCodeFromFile;

                   filecode = bankCodeDesc1.PadRight(3).Substring(0, 3).TrimEnd();

                    if (filecode.Equals(bankCode)) 
                        {

                            bankCodeDesc1 = bankdesctry;
                            fileInformationModel.filebankcode1 = bankCodeDesc1;

                        }
                    else
                        {
                            
                        }

                      fileInformationModel.fileClearDate = clearDate1;
                      fileClearDate1 = fileInformationModel.fileClearDate;

                     fileInformationModel.fileSize = fileInfo.Length.ToString();
                     filesize = fileInformationModel.fileSize;
                    //   DateTime creationTime = fileInfo.CreationTime;
                    //   fileInformationModel.fileTimeStamp = DateUtils.formatTimeStampFromSql(creationTime.ToString());
                    //fileInformationModel.fileStatus = 

                    //fileInformationModel.fileStatus = checkICLUploadStatus(item.ToString());
                    fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString("dd-MM-yyyy HH:mm:ss"); 
                    fileTimeStamp = fileInformationModel.fileTimeStamp;
                    filesList.Add(fileInformationModel);
                    InsertFileList(fileNameWith1, filename, bankCode, filesize, fileClearDate1,fileTimeStamp);

                    }
                }
                
            }
            
            return filesList;
        }

        public string checkICLUploadStatus(string itemPath)
        {
            string status = "";
            string ICLFileName = Path.GetFileName(itemPath);
            string stmt = "select fldFlag from tblinwardclearingfile where fldinwardfilename = @fldinwardfilename";
            DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] {
              new SqlParameter("@fldinwardfilename",ICLFileName)
            });
            if (dt.Rows.Count > 0)
            {
                status = "Completed";
            }
            else
            {
                status = "Pending";
            }
            return status;
        }

         public void InsertFileList(string fileNameWith1, string filename, string bankCode, string filesize,string fileClearDate1, string fileTimeStamp)
                {
                    List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

                    sqlParameterNext.Add(new SqlParameter("@fldFileName", fileNameWith1));
                    sqlParameterNext.Add(new SqlParameter("@fldPath", filename));

                    sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
                    sqlParameterNext.Add(new SqlParameter("@fldfilesize", filesize));
                    sqlParameterNext.Add(new SqlParameter("@fldprocessdate", fileClearDate1));
                    sqlParameterNext.Add(new SqlParameter("@fldCreatetimestamp", fileTimeStamp));
                    dbContext.GetRecordsAsDataTableSP("spciInwardFileList", sqlParameterNext.ToArray());
                }

        public void InsertFileListICS(string fileNameWith1, string filename, string bankCode, string filesize, string fileClearDate1, string fileTimeStamp, string processName)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldFileName", fileNameWith1));
            sqlParameterNext.Add(new SqlParameter("@fldPath", filename));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldfilesize", filesize));
            sqlParameterNext.Add(new SqlParameter("@fldprocessdate", fileClearDate1));
            sqlParameterNext.Add(new SqlParameter("@fldCreatetimestamp", fileTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            dbContext.GetRecordsAsDataTableSP("spciICSInwardFileList", sqlParameterNext.ToArray());
        }

        public void InsertFileListICSDP(string fileNameWith1, string filename, string bankCode, string bankType, string filesize, string fileClearDate1, string fileTimeStamp, string processName, string userId)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldFileName", fileNameWith1));
            sqlParameterNext.Add(new SqlParameter("@fldPath", filename));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldBankType", bankType));
            sqlParameterNext.Add(new SqlParameter("@fldFileSize", filesize));
            sqlParameterNext.Add(new SqlParameter("@fldClearDate", fileClearDate1));
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldTotalItem", "0"));
            sqlParameterNext.Add(new SqlParameter("@fldTotalAmount", "0"));
            sqlParameterNext.Add(new SqlParameter("@fldCreateUserID", userId));
            sqlParameterNext.Add(new SqlParameter("@fldFileTimeStamp", fileTimeStamp));

            dbContext.GetRecordsAsDataTableSP("spciICSDPInwardFileList", sqlParameterNext.ToArray());
        }

        public void DeleteDirectory(string path) {
            foreach (string directory in Directory.GetDirectories(path)) {
                DeleteDirectory(directory);
            }

            try {
                Directory.Delete(path, true);
            } catch (IOException) {
                Directory.Delete(path, true);
            } catch (UnauthorizedAccessException) {
                Directory.Delete(path, true);
            }
        }

        public void DeleteDirectory(string path, bool recursive, string originalPath = "") {
            // Delete all files and sub-folders?
            if (recursive) {
                // Yep... Let's do this
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders) {
                    try {
                        DeleteDirectory(s, recursive);
                    } catch (Exception ex) {
                        ex.ToString();
                    }
                }
            }

            // Get all files of the folder
            var files = Directory.GetFiles(path);
            foreach (var f in files) {
                // Get the attributes of the file
                var attr = File.GetAttributes(f);

                // Is this file marked as 'read-only'?
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    // Yes... Remove the 'read-only' attribute, then
                    File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                }

                // Delete the file
                File.Delete(f);
            }

            // When we get here, all the files of the folder were
            // already deleted, so we just delete the empty folder
            try {
                if (originalPath.Equals("")) {
                    Directory.Delete(path);
                }
            } catch (Exception ex) {
                ex.ToString();
            }
        }
        // xx start 20210428
        public List<FileInformationModel> GetAllFileInFolderWithPath(string filename, int bankSubString, string bankCode)
        {
            List<FileInformationModel> filesList = new List<FileInformationModel>();
            string[] filePath = Directory.GetFiles(filename);
            foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                fileInformationModel.fileName = item.ToString();
                //fileInformationModel.fileSize = item.Length.ToString();

                //fileInformationModel.fileSize = fileInfo.Length.ToString();
                //long sizeFolder = Convert.ToInt64(fileInformationModel.fileSize) / 1000;
                //fileInformationModel.fileSize = sizeFolder.ToString();
                fileInformationModel.fileNameOnly = fileInfo.Name.ToString();
                fileInformationModel.fileSize = (Math.Round((Convert.ToDouble(fileInfo.Length) / 1000), 2)).ToString();
                fileInformationModel.fileTimeStamp = fileInfo.LastWriteTime.ToString("dd-MM-yyyy hh:mm:ss tt");

                filesList.Add(fileInformationModel);

            }
            return filesList;
        }
        // xx end 20210428
        // Azim start 20210518
        public List<FileInformationModel> GetAllFileInFolderWithPathLoadNCF(string filename, int bankSubString, string bankCode, string fileClearDate)
        {
            List<FileInformationModel> filesList = new List<FileInformationModel>();
            string[] filePath = Directory.GetFiles(filename);
            foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);
                if (fileInfo.Name.Trim().Length > 12)
                {
                    if ((fileInfo.Name.Trim().ToString().Substring(0, 8) == fileClearDate) && (fileInfo.Name.Trim().ToString().Substring(12, 3) == "NCF"))
                    {
                        fileInformationModel.fileName = fileInfo.Name.ToString();
                        fileInformationModel.fileSize = (Convert.ToDecimal(fileInfo.Length) / 1024).ToString("0.00");
                        fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                        filesList.Add(fileInformationModel);
                    }
                }
                
                
            }
            return filesList;
        }
        // Azim end 20210518
        public List<FileInformationModel> GetAllFileInFolderLoadDailyFile(string folderNameFrom, string bankcode, string clearDate, string fileExt)
        {

            string filedate = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            
            List<FileInformationModel> filesList = new List<FileInformationModel>();
            string[] filePath = Directory.GetFiles(folderNameFrom);
            foreach (var item in filePath)
            {
                FileInformationModel fileInformationModel = new FileInformationModel();
                FileInfo fileInfo = new FileInfo(item);

                if (Path.GetFileName(item.ToString()).ToString().Trim().Remove(0, 8) != "ErrorCode.ALL")
                {
                    fileInformationModel.fileNameOnly = Path.GetFileName(item.ToString());
                    fileInformationModel.fileName = item.ToString();
                    fileInformationModel.fileTimeStamp = fileInfo.CreationTime.ToString();
                    fileInformationModel.fileSize = fileInfo.Length.ToString();
                    long sizeFolder = Convert.ToInt64(fileInformationModel.fileSize) / 1024;
                    fileInformationModel.fileSize = sizeFolder.ToString();
                    fileInformationModel.fileClearDate = filedate;
                    filesList.Add(fileInformationModel);
                }
                

            }

            return filesList;

        }
    }

}