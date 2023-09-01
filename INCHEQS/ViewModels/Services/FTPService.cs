
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using WinSCP;

namespace INCHEQS.Services {
  public class FTPService
    {
        private readonly string ftpUserName;
        private readonly string ftpPassword;
        private readonly string ftpRootPath;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(FTPService));

        public static int FTPSendFile(string hostName, string userName, string password, string fileName, string filePath, string FTPFolder) {

            try {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions {
                    Protocol = Protocol.Ftp,
                    HostName = hostName,
                    UserName = userName,
                    Password = password,

                    //For SFTP
                    //Protocol = Protocol.Sftp,
                    //SshHostKeyFingerprint = "ssh-rsa 2048 xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx:xx"
                };

                using (Session session = new Session()) {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(filePath, FTPFolder + fileName, false, transferOptions);


                    // Throw on any error
                    transferResult.Check();

                }

                return 0;
            } catch (Exception ex) {
                throw ex;
            }
        }


        public FTPService(string user, string password, string server)
        {
            this.ftpUserName = user;
            this.ftpPassword = password;
            this.ftpRootPath = server.TrimEnd('/') + "/";
        }

        public string UploadFile(string source)
        {
            Logger.DebugFormat("File For Upload: {0}，FTP Root Folder： {1}", source, ftpRootPath);
            string fileName = GenerateFileName(source);
            Logger.DebugFormat("Dynamically Generated File Name： {0}", fileName);
            var result = UploadFile(fileName, source);
            Logger.DebugFormat("Upload Successful! File Path returned：{0}", fileName);
            return fileName;
        }

        public byte[] DownloadFile(string source)
        {
            Logger.DebugFormat("Read files on FTP：{0}", source);
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRootPath + source);
            req.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.UseBinary = true;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                using (Stream ftpStream = response.GetResponseStream())
                using (var memoryStream = new MemoryStream())
                {
                    ftpStream.CopyTo(memoryStream);
                    response.Close();
                    req.Abort();
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.WarnFormat("An error occurred while reading the file！{0}", e.Message);
                req.Abort();
                return null;
            }
        }

        public bool DeleteFile(string source)
        {
            Logger.DebugFormat("Deleted files on FTP：{0}", source);
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRootPath + source);
            req.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            req.Method = WebRequestMethods.Ftp.DeleteFile;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                Logger.WarnFormat("An error occurred while deleting files！{0}", e.Message);
                req.Abort();
                return false;
            }
            req.Abort();
            Logger.Debug("File is successfully deleted！");
            return true;
        }

        #region private methods
        /// <summary>
        /// FTP upload files to a specified directory
        /// </summary>
        /// <param name="ftpFileName">Specified FTP directory that contains the full file name</param>
        /// <param name="source"></param>
        private bool UploadFile(string ftpFileName, string source)
        {
            CreateFtpDirectory(ftpFileName);
            FileInfo fi = new FileInfo(source);
            FileStream fs = fi.OpenRead();
            long length = fs.Length;
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRootPath + ftpFileName);
            req.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            req.Method = WebRequestMethods.Ftp.UploadFile;
            req.ContentLength = length;
            req.Timeout = 10 * 1000;
            try
            {
                Stream stream = req.GetRequestStream();
                int BufferLength = 4096;
                byte[] b = new byte[BufferLength];
                int i;
                while ((i = fs.Read(b, 0, BufferLength)) > 0)
                {
                    stream.Write(b, 0, i);
                }
                stream.Close();
                stream.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error("An error occurred while uploading files！", e);
                return false;
            }
            finally
            {
                fs.Close();
                req.Abort();
            }
            req.Abort();
            return true;
        }

        /// <summary>
        ///Generate file path and file name according to the current time. similar：
        /// 2014/06/17/guid.png
        /// </summary>
        /// <param name="source">Absolute path to the uploaded file</param>
        /// <returns></returns>
        private string GenerateFileName(string source)
        {
            var extension = Path.GetExtension(source);
            var now = DateTime.Now.ToString("yyyy/MM/dd");
            var guid = Guid.NewGuid().ToString();
            return now + "/" + guid + extension;
        }

        private void CreateFtpDirectory(string destFilePath)
        {
            string fullDir = FtpParseDirectory(destFilePath);
            string[] dirs = fullDir.Split('/');
            string curDir = "/";
            for (int i = 0; i < dirs.Length; i++)
            {
                string dir = dirs[i];
                if (dir != null && dir.Length > 0)
                {
                    try
                    {
                        curDir += dir + "/";
                        if (!CheckIfDirectoryExists(curDir))
                            MakeDirectory(curDir);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Error creating FTP directory！", e);
                    }
                }
            }
        }

        private static string FtpParseDirectory(string destFilePath)
        {
            return destFilePath.Substring(0, destFilePath.LastIndexOf("/"));
        }

        /// <summary>
        /// Check FTP server , specify the path exists
        /// </summary>
        /// <param name="localFile"></param>
        /// <returns></returns>
        private bool CheckIfDirectoryExists(string localFile)
        {
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRootPath + localFile);
            req.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            req.Method = WebRequestMethods.Ftp.ListDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                Logger.WarnFormat("FTP directory '{0}' does not exist! Error Message：{1}", localFile, e.Message);
                req.Abort();
                return false;
            }
            Logger.DebugFormat("FTP directory exists：{0}", localFile);
            req.Abort();
            return true;
        }

        /// <summary>
        /// Creates the specified directory on the FTP server
        /// </summary>
        /// <param name="localFile"></param>
        /// <returns></returns>
        private bool MakeDirectory(string localFile)
        {
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpRootPath + localFile);
            req.Credentials = new NetworkCredential(ftpUserName, ftpPassword);
            req.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                FtpWebResponse response = (FtpWebResponse)req.GetResponse();
                response.Close();
            }
            catch (Exception e)
            {
                Logger.WarnFormat("Create FTP directory： '{0}' Failure! Error Message：{ 1}", localFile, e.Message);
                req.Abort();
                return false;
            }
            Logger.DebugFormat("Create FTP directory： '{0}' Success！", localFile);
            req.Abort();
            return true;
        }
        #endregion
    }
}