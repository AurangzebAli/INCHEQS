﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
//using INCHEQS.Helpers;
using INCHEQS.Processes.DataProcess;
//using INCHEQS.Processes.DataProcess;
//using INCHEQS.Security.Account;
//using INCHEQS.Security;
using System.Globalization;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using INCHEQS.Common;

//using INCHEQS.Helpers;

public class OCSDataProcessDao : OCSIDataProcessDao
{

    private readonly OCSDbContext OCSDbContext;

    public OCSDataProcessDao(OCSDbContext ocsdbContext)
    {
        this.OCSDbContext = ocsdbContext;
    }

    public List<string> GetFileNameFromFileManager(string TaskId)
    {
        OCSDataProcessModel dataProcess = new OCSDataProcessModel();
        List<string> resultList = new List<string>();

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldTaskId", TaskId));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgFileNameFromFileManager", sqlParameterNext.ToArray());


        foreach (DataRow row in dt.Rows)
        {
            string fileName = row["fldFileName"].ToString();
            resultList.Add(fileName);
        }
        return resultList;
    }

    public DataTable ListAll(/*AccountModel*/string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spgListAlltblDataProcessOCS", sqlParameterNext.ToArray());

        return dt;
    }


    public void DeleteProcessUsingCheckbox(string dataProcess)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@dataProcessId", dataProcess));
            this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcessOCS", sqlParameterNext.ToArray());


    }


    public void UpdateFileManagerID (string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuFileManagerID", sqlParameterNext.ToArray());
    }
    public void InsertClearDate(string clearDate, int lastSequence, string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcutblInwardClearDate", sqlParameterNext.ToArray());

        List<SqlParameter> sqlParameterNext2 = new List<SqlParameter>();
        sqlParameterNext2.Add(new SqlParameter("@fldClearDateID", lastSequence));
        sqlParameterNext2.Add(new SqlParameter("@fldClearDate", DateUtils.formatDateToSql(clearDate)));
        sqlParameterNext2.Add(new SqlParameter("@fldActiveStatus", "1"));
        sqlParameterNext2.Add(new SqlParameter("@fldBankCode", bankCode));
        this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcitblInwardClearDate", sqlParameterNext.ToArray());
    }

    public void DeleteDataProcess(string processName, string posPayType, string bankCode)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcessOCS2", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }

    }

    public void DeleteDataProcessWithoutPosPayType(string processName, string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcesswithoutSystemType", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }


    }

    public void DeleteDataProcessWithPosPayType(string processName, string posPayType, string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcesswithSystemType", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }

    }

    public void InsertToDataProcess(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskid, string batchId, string crtuserId, string upduserId, string filename = "")
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldStatus", 1));
        sqlParameterNext.Add(new SqlParameter("@fldProductCode", "OCS"));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", clearDate));
        sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", crtuserId));
        sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldStartTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldEndTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", upduserId));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
        this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessOCS", sqlParameterNext.ToArray());
    }

    public void InsertToDataProcessIRD(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskid, string batchId, string filename, string crtuserId, string upduserId)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldStatus", 1));
        sqlParameterNext.Add(new SqlParameter("@fldFileName", filename));
        sqlParameterNext.Add(new SqlParameter("@fldProductCode", "OCS"));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", clearDate));
        sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", crtuserId));
        sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldStartTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldEndTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", upduserId));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
        this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessIrdOCS", sqlParameterNext.ToArray());
    }

    public void UpdateToDataProcess(string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskId, string batchId, string crtuserId, string upduserId, string status, string oristatus, string remarks, string filename = "")
    {
        DateTime dtcleardate;
        dtcleardate = Convert.ToDateTime(clearDate);
        clearDate = dtcleardate.ToString("yyyy-MM-dd");
        //string stmt = "update tbldataprocessocs set fldStatus= @fldStatus, fldremarks = @fldremarks where fldprocessname = @fldProcessName and fldCleardate::Timestamp without time zone = @fldCleardate::Timestamp without time zone and fldstatus =@fldstatusori ";

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldStatus", Convert.ToInt16(status)));
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", clearDate));
            sqlParameterNext.Add(new SqlParameter("@fldRemarks", remarks));
            sqlParameterNext.Add(new SqlParameter("@fldstatusori", Convert.ToInt16(oristatus)));
            this.OCSDbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcutblDataProcessOCS", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }
    }




    public DataTable GetProcessStatus(string clearDate, string processName, string bankCode)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatus", sqlParameterNext.ToArray());

        return dt;

    }

    public DataTable GetProcessStatusEOD(string clearDate, string processName, string bankCode)
    {

        DataTable dt = new DataTable();
        if (String.IsNullOrEmpty(clearDate))
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
             dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatusEOD1", sqlParameterNext.ToArray());

        }
        else
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
            dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatusEOD2", sqlParameterNext.ToArray());

        }
    

        return dt;
    }

    public DataTable GetProcessStatusICL(string clearDate, string posPayType, string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldPosPayType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatusICL", sqlParameterNext.ToArray());

        return dt;
    }

    public DataTable GetProcessStatusIR_ICL(string clearDate, string systemType, string bankCode, string processName)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldPosPayType", systemType));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatusInwardICL", sqlParameterNext.ToArray());
        return dt;
    }
    public DataTable GetProcessStatusECCS(string filetype, string clearDate, string processName, string bankCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@filetype", filetype));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgProcessStatusECCS", sqlParameterNext.ToArray());

        return dt;
    }

    public DataTable GetClearingType(string clearingType)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldType", clearingType));
        DataTable dt = OCSDbContext.GetRecordsAsDataTableSP("spcgClearingType", sqlParameterNext.ToArray());

        return dt;
    }

    public bool CheckRunningProcess(string processName, string posPayType, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";

        if (processName == "ICSImport")
        {
         

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
             dt = OCSDbContext.GetRecordsAsDataTableSP("spcgCheckRunningProcessOCS1", sqlParameterNext.ToArray());
        }
        else
        {

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            dt = OCSDbContext.GetRecordsAsDataTableSP("spcgCheckRunningProcessOCS1", sqlParameterNext.ToArray());
        }



        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("") || result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        return false;
    }

    public bool CheckRunningProcessWithoutPosPayType(string processName, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", clearDate));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = OCSDbContext.GetRecordsAsDataTableSP("spcgRunningProcessWithoutSystemType", sqlParameterNext.ToArray());
        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("") || result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        return false;
    }

    public bool CheckRunningProcessBeforeEod(string processName, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = OCSDbContext.GetRecordsAsDataTableSP("spcgRunningProcessBeforeEOD", sqlParameterNext.ToArray());

        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("") || result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        return false;
    }

    public DataTable GenGif(string processName, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = OCSDbContext.GetRecordsAsDataTableSP("spcgGenGif", sqlParameterNext.ToArray());
        if (dt.Rows.Count > 0)
        {
            return dt;
        }
        else
        {
            return null;
        }

    }

    public bool CheckRunningProcessGenerateFile(string processName, string posPayType, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateUtils.formatDateToSql(clearDate)));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = OCSDbContext.GetRecordsAsDataTableSP("spcgRunningProcessGenerateFile", sqlParameterNext.ToArray());
        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("") || result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        return false;
    }
    public bool CheckProcessDateWithinRetentionPeriod(string sProcessingDate, int sProcess, string bankCode)
    {
        bool sContinue = false;
        int typeDayNumber = 0;
        int result;
        string archiveLog = "";
        string clearingDate = "";

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        DataTable ds = new DataTable();
        ds = OCSDbContext.GetRecordsAsDataTableSP("spcgTblAuitLogRetention", null);

        if (ds.Rows.Count > 0)
        {
            DataRow row = ds.Rows[0];
            archiveLog = row["fldAchAuditLog"].ToString();
        }

        DataTable configTable = OCSDbContext.GetRecordsAsDataTableSP("spcgtblInwardItemInfo", sqlParameterNext.ToArray());
        if (configTable.Rows.Count > 0)
        {
            clearingDate = configTable.Rows[0]["fldClearDate"].ToString();
        }
        else
        {
            clearingDate = DateTime.Today.ToString();
        }

        DateTime sProcessDate = DateTime.ParseExact(sProcessingDate, "dd-MM-yyyy", CultureInfo.InvariantCulture);
        DateTime todayDate = Convert.ToDateTime(clearingDate);

        if (sProcess == 0)
        {
            if (todayDate != sProcessDate)
            {
                sContinue = false;
            }
            else
            {
                sContinue = true;
            }
        }

        if (sProcess == 1)
        {
            string type = archiveLog.Substring(archiveLog.Length - 1);
            string num = archiveLog.Substring(0, archiveLog.Length - 1);
            //Mid(datahousekeep.archiveLog, 1, tblRes.Rows(0)("fldAuditLog").ToString.Length - 1)
            bool isNumeric = int.TryParse(num, out result);

            //DateTime sProcessDate = DateTime.Parse(sProcessingDate);


            if (isNumeric == false)
            {
                sContinue = false;
            }

            switch (type)
            {
                case "D":
                    typeDayNumber = 1;
                    break;
                case "M":
                    typeDayNumber = 30;
                    break;
                case "Y":
                    typeDayNumber = 365;
                    break;
            }
            int periorNumber = Int32.Parse(num);
            int rententionDay = 0;
            rententionDay = typeDayNumber * periorNumber;
            double diff = (todayDate - sProcessDate).TotalDays;

            if (diff > rententionDay)
            {
                sContinue = false;
            }
            else
            {
                sContinue = true;
            }

        }
        return sContinue;
    }


}