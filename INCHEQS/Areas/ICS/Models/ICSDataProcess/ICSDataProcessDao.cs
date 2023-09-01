using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;

public class ICSDataProcessDao : ICSIDataProcessDao
{

    private readonly ApplicationDbContext dbContext;

    public ICSDataProcessDao(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;

    }


    public void DeleteDataProcessICS(string processName, string posPayType, string bankCode)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcessICS", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }
    }

    // xx start
    public void DeleteDataProcessWithSystemTypeICS(string clearDate, string posPayType, string bankCode)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcessWithSystemType", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }
    }
    // xx end


    public void DeleteDataProcessWithoutPosPayTypeICS(string processName, string bankCode)
    {

        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        try
        {
            sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
            sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcdDataProcesswithoutSystemTypeICS", sqlParameterNext.ToArray());


        }
        catch (Exception exception)
        {
            throw exception;
        }

    }



    public void InsertToDataProcessICS(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskid, string batchId, string filename, string crtuserId, string upduserId)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();

        //clearDate = DateTime.Parse(clearDate).ToString("dd-MM-yy");
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldPosPayType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldStatus", 1));
        //sqlParameterNext.Add(new SqlParameter("@fldProductCode", "ICS"));
        sqlParameterNext.Add(new SqlParameter("@fldClearDate", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));
        sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", crtuserId));
        sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldStartTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldEndTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", upduserId));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        if (batchId.ToString().Trim() == "" || batchId.ToString().Trim() == null)
        {
            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessICS", sqlParameterNext.ToArray());
        }
        else
        {
            sqlParameterNext.Add(new SqlParameter("@fldBatchId", batchId.ToString()));
            this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessICSUPI", sqlParameterNext.ToArray());
        }

        
    }


    
    public bool CheckRunningProcessICS(string processName, string posPayType, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = dbContext.GetRecordsAsDataTableSP("spcgRunningProcess", sqlParameterNext.ToArray());
        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("") || result.Equals("3") || result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool CheckRunningProcessWithoutPosPayTypeICS(string processName, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));

        dt = dbContext.GetRecordsAsDataTableSP("spcgRunningProcessWithoutSystemTypeICS", sqlParameterNext.ToArray());
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
        string sql = "";
        sql = string.Format("SELECT fldStatus FROM tblDataProcessICS WHERE fldProcessName in ({0})  /*AND fldCleardate = @fldCleardate*/ AND fldBankCode=@fldBankCode and fldstatus<>'4' ", processName);

        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@fldCleardate", DateUtils.formatDateToSql(clearDate)));
        parameters.Add(new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/));
        dt = dbContext.GetRecordsAsDataTable(sql, parameters.ToArray());

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


    public DataTable GetProcessStatus(string clearDate, string processName, string bankCode)
    {

        DataTable dt = new DataTable();
        string sql = "SELECT ISNULL(fldRemarks,'') as fldRemarks, * FROM tblDataProcessICS WHERE fldProcessName = @fldProcessName AND fldClearDate = @fldClearDate and fldBankCode=@fldBankCode ORDER BY fldCreateTimestamp";

        dt = dbContext.GetRecordsAsDataTable(sql, new[] {
            new SqlParameter("@fldProcessName", processName),
            new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/),
            new SqlParameter("@fldClearDate" , DateUtils.formatDateToSql(clearDate))
        });

        return dt;
    }

    public DataTable GetProcessStatusEOD(string clearDate, string processName, string bankCode)
    {
        string sql;
        DataTable dt = new DataTable();
        if (String.IsNullOrEmpty(clearDate))
        {
            sql = "SELECT top 1 ISNULL(fldRemarks,'') as fldRemarks, * FROM tblDataProcessICS WHERE fldProcessName = 'ICSEOD' and fldBankCode=@fldBankCode ORDER BY fldCreateTimestamp desc";
        }
        else
        {
            sql = "SELECT top 1 ISNULL(fldRemarks,'') as fldRemarks, * FROM tblDataProcessICS WHERE fldProcessName = @fldProcessName /*AND fldClearDate = @fldClearDate*/ and fldBankCode=@fldBankCode ORDER BY fldCreateTimestamp desc";
        }
        dt = dbContext.GetRecordsAsDataTable(sql, new[] {
            new SqlParameter("@fldProcessName", processName),
            new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/),
            new SqlParameter("@fldClearDate" , DateUtils.formatDateToSql(clearDate))
        });

        return dt;
    }

    public DataTable GetProcessStatusICL(string clearDate, string posPayType, string bankCode, string processName)
    {

        DataTable dt = new DataTable();
        string sql = "SELECT ISNULL(fldRemarks,'') as fldRemarks, * FROM tblDataProcessICS WHERE fldPosPayType = @posPayType AND fldClearDate = @fldClearDate and fldBankCode=@fldBankCode ORDER BY fldCreateTimestamp";

        dt = dbContext.GetRecordsAsDataTable(sql, new[] {
            new SqlParameter("@posPayType" , posPayType),
            new SqlParameter("@fldBankCode" , bankCode/*CurrentUser.Account.BankCode*/),
            new SqlParameter("@fldClearDate" , DateUtils.formatDateToSql(clearDate))
        });

        return dt;
    }

    public DataTable GetProcessStatusECCS(string filetype, string clearDate, string processName, string bankCode)
    {

        DataTable dt = new DataTable();
        string sql = "SELECT ISNULL(fldRemarks,'') as fldRemarks, * FROM tblDataProcessICS WHERE fldProcessName = @fldProcessName AND fldClearDate = @fldClearDate AND RIGHT(RTRIM(fldPosPayType),12) = @filetype and fldBankCode=@fldBankCode ORDER BY fldCreateTimestamp";

        dt = dbContext.GetRecordsAsDataTable(sql, new[] {
            new SqlParameter("@fldProcessName", processName),
            new SqlParameter("@filetype", filetype),
            new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/),
            new SqlParameter("@fldClearDate" , DateUtils.formatDateToSql(clearDate))
        });

        return dt;
    }

    public DataTable GetClearingType(string clearingType)
    {
        DataTable dt = new DataTable();
        string sql = "SELECT fldClearingNumber as clearingValue, fldClearingDesc as clearingText FROM tblClearingType with (NOLOCK) WHERE fldType =@fldType";
        dt = dbContext.GetRecordsAsDataTable(sql, new[] { new SqlParameter("@fldType", clearingType) });

        return dt;
    }


    public DataTable GenGif(string processName, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        string sql = "";
        sql = string.Format("SELECT fldUIC,fldimagefolder FROM view_items WHERE fldcleardate = @fldCleardate and fldissuebankcode=@fldBankCode ");

        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@fldCleardate", DateUtils.formatDateToSql(clearDate)));
        parameters.Add(new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/));
        dt = dbContext.GetRecordsAsDataTable(sql, parameters.ToArray());

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
        string sql = "SELECT fldStatus FROM tblDataProcessICS WHERE fldProcessName = @fldProcessName AND fldCleardate = @fldCleardate AND fldPosPayType = @fldPosPayType AND fldBankCode=@fldBankCode ";

        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@fldProcessName", processName));
        parameters.Add(new SqlParameter("@fldCleardate", DateUtils.formatDateToSql(clearDate)));
        parameters.Add(new SqlParameter("@fldPosPayType", posPayType));
        parameters.Add(new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/));
        dt = dbContext.GetRecordsAsDataTable(sql, parameters.ToArray());

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
        string stmt = "Select * from tblAuditLogRetention";
        int typeDayNumber = 0;
        int result;
        string archiveLog = "";
        string clearingDate = "";
        string stmt2 = "Select top 1 fldcleardate from tblinwarditeminfo where fldissuebankcode = @fldBankCode order by fldcleardate desc";
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/));
        DataTable ds = new DataTable();
        ds = dbContext.GetRecordsAsDataTable(stmt);

        if (ds.Rows.Count > 0)
        {
            DataRow row = ds.Rows[0];
            archiveLog = row["fldAchAuditLog"].ToString();
        }


        DataTable configTable = dbContext.GetRecordsAsDataTable(stmt2, parameters.ToArray());
        if (configTable.Rows.Count > 0)
        {
            clearingDate = configTable.Rows[0]["fldcleardate"].ToString();
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

    public List<string> GetFileNameFromFileManager(string taskId)
    {
        ICSDataProcessModel dataProcess = new ICSDataProcessModel();
        List<string> resultList = new List<string>();

        string stmt = "SELECT fldFileName FROM tblFileManager WHERE fldTaskId=@fldTaskId";
        DataTable dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });

        foreach (DataRow row in dt.Rows)
        {
            string fileName = row["fldFileName"].ToString();
            resultList.Add(fileName);
        }
        return resultList;
    }

    public DataTable ListAll(string bankCode)
    {
        DataTable ds = new DataTable();
        string stmt = "select dp.fldPrimaryId,dp.fldProcessName,dp.fldStatus, convert(varchar,dp.fldstartTime,108) as fldStartTime, convert(varchar,dp.fldEndTime,108) as fldEndTime,um.fldUserAbb,convert(varchar,dp.fldClearDate,103) as fldClearDate from tblDataProcessICS dp, tblUserMaster um where dp.fldUpdateUserId = um.fldUserId and um.fldBankCode=@fldBankCode";

        ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldBankCode", bankCode/*currentUser.BankCode*/) });

        return ds;
    }

    public void DeleteProcessUsingCheckbox(string dataProcess)
    {
        string[] aryText = dataProcess.Split(',');
        if ((aryText.Length > 0))
        {
            string stmt = "delete from tblDataProcessICS where fldPrimaryId in (" + DatabaseUtils.getParameterizedStatementFromArray(aryText) + ")";

            dbContext.ExecuteNonQuery(stmt, DatabaseUtils.getSqlParametersFromArray(aryText).ToArray());

        }

    }


    public void InsertClearDate(string clearDate, int lastSequence, string bankCode)
    {

        string stmt = "Update tblInwardClearDate set fldActiveStatus='0' where fldBankCode=@fldBankCode";
        dbContext.ExecuteNonQuery(stmt, new[] {
            new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/)
        });

        Dictionary<string, dynamic> sqlMap = new Dictionary<string, dynamic>();
        sqlMap.Add("fldClearDateID", lastSequence);
        sqlMap.Add("fldClearDate", DateUtils.formatDateToSql(clearDate));
        sqlMap.Add("fldActiveStatus", "1");
        sqlMap.Add("fldBankCode", bankCode/*CurrentUser.Account.BankCode*/);

        dbContext.ConstructAndExecuteInsertCommand("tblInwardClearDate", sqlMap);
    }
    public bool CheckImportDataProcessICS(string processName, string posPayType, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", DateTime.Parse(clearDate)));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        dt = dbContext.GetRecordsAsDataTableSP("spcgCheckImportDataProcessProcessICS", sqlParameterNext.ToArray());
        if (dt.Rows.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckDataProcessExist(string processName, string posPayType, string clearDate, string bankCode)
    {
        DataTable dt = new DataTable();
        string result = "";
        string sql = "SELECT fldStatus FROM tblDataProcessICS WHERE fldProcessName = @fldProcessName AND fldProcessDate = @fldCleardate AND fldSystemType = @fldPosPayType AND fldBankCode=@fldBankCode ";

        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@fldProcessName", processName));
        parameters.Add(new SqlParameter("@fldCleardate", DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture)));
        parameters.Add(new SqlParameter("@fldPosPayType", posPayType));
        parameters.Add(new SqlParameter("@fldBankCode", bankCode/*CurrentUser.Account.BankCode*/));
        dt = dbContext.GetRecordsAsDataTable(sql, parameters.ToArray());

        if (dt.Rows.Count > 0)
        {
            result = dt.Rows[0]["fldStatus"].ToString();
        }

        if (result.Equals("4"))
        { //Result "" for initialize and "4" for complete
            return true;
        }
        return false;
    }

    public void InsertToDataProcessICSUPI(/*AccountModel*/string bankCode, string processName, string posPayType, string clearDate, string reUpload, string taskid, string batchId, string filename, string crtuserId, string upduserId,string filetype, string fldClearingType,  string fldStateCode)
    {
        List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
        sqlParameterNext.Add(new SqlParameter("@fldProcessName", processName));
        sqlParameterNext.Add(new SqlParameter("@fldSystemType", posPayType));
        sqlParameterNext.Add(new SqlParameter("@fldStatus", 1));
      //sqlParameterNext.Add(new SqlParameter("@fldProductCode", "ICS"));
        sqlParameterNext.Add(new SqlParameter("@fldProcessDate", clearDate));
        sqlParameterNext.Add(new SqlParameter("@fldCreateUserId", crtuserId));
        sqlParameterNext.Add(new SqlParameter("@fldCreateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldStartTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldEndTime", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateUserId", upduserId));
        sqlParameterNext.Add(new SqlParameter("@fldUpdateTimeStamp", DateTime.Now));
        sqlParameterNext.Add(new SqlParameter("@fldBankCode", bankCode));
        sqlParameterNext.Add(new SqlParameter("@fileTypeName", filetype));
        sqlParameterNext.Add(new SqlParameter("@fldClearingType", fldClearingType));
        sqlParameterNext.Add(new SqlParameter("@fldStateCode", fldStateCode));

        this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessInwardReturn", sqlParameterNext.ToArray());
        //this.dbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciDataProcessICSUPI", sqlParameterNext.ToArray());
    }
}