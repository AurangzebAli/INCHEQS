using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using System.Collections.Generic;
using System.Data;


namespace INCHEQS.Areas.ICS.Models.ICSDataProcess
{ 
    public interface ICSIDataProcessDao
    {
        DataTable ListAll(string bankCode);

        DataTable GetProcessStatus(string clearDate, string posPayType, string bankCode);
        DataTable GetProcessStatusEOD(string clearDate, string posPayType, string bankCode);
        DataTable GetProcessStatusICL(string clearDate, string posPayTypes, string bankCode, string processName);
        DataTable GetProcessStatusECCS(string filetype, string clearDate, string posPayType, string bankCode);
        DataTable GenGif(string processName, string clearingDate, string bankCode);
        DataTable GetClearingType(string clearingType);
        List<string> GetFileNameFromFileManager(string taskId);
        void DeleteProcessUsingCheckbox(string processName);
        bool CheckRunningProcessICS(string processName, string posPayType, string clearingDate, string bankCode);
        bool CheckRunningProcessWithoutPosPayTypeICS(string processName, string clearingDate, string bankCode);
        bool CheckRunningProcessBeforeEod(string processName, string clearingDate, string bankCode);
        void InsertClearDate(string clearDate, int lastSequence, string bankCode);
        void DeleteDataProcessICS(string processName, string posPayType, string bankCode);
        void DeleteDataProcessWithoutPosPayTypeICS(string processName, string bankCode);
        void InsertToDataProcessICS(/*AccountModel*/string bankCode, string processName, string posPayType, string clearingDate, string reUpload, string taskId, string batchId, string crtuserId, string upduserId, string fileName = "");
        bool CheckProcessDateWithinRetentionPeriod(string clearingDate, int sProcess, string bankCode);
        bool CheckImportDataProcessICS(string processName, string posPayType, string clearDate, string bankCode);
        // xx start
        void DeleteDataProcessWithSystemTypeICS(string clearDate, string posPayType, string bankCode);
        bool CheckDataProcessExist(string processName, string posPayType, string clearDate, string bankCode);
        // xx end
        void InsertToDataProcessICSUPI(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskid, string batchId, string filename, string crtuserId, string upduserId,string filetype,string fldClearingType, string fldStateCode);
    }
}