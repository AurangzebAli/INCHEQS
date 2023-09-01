using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.FileInformation {
    public interface IFileManagerDao {
        void ClearRemarks(string fileName);
        bool CheckFileExist(AccountModel currentUser, string path);
        void InsertToFileManager(AccountModel currentUser, string taskid, string path, string filename, string clearDate);
        void InsertToFileManagerOcs(AccountModel currentUser, string path, string filename, string clearDate);
        void UpdateFileManager(AccountModel currentUser, string fileName);
        void UpdateFileManagerLoadFile(string fileName, string clearDate);
        void DeleteFileManagerOCS(string fileName, string filepath, string bankCode);
        void DeleteFileManager(string fileName, string filepath, string bankCode);
        DataTable getBankDesc(string bankCode);
    }
}