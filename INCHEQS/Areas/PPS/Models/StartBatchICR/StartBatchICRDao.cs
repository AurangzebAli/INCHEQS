using INCHEQS.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;

using System.Web.Mvc;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace INCHEQS.Areas.PPS.Models.StartBatchICR
{
    public class StartBatchICRDao : IStartBatchICRDao
    {
        private readonly ApplicationDbContext dbContext;

        public StartBatchICRDao(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<StartBatchICRModel>> GetProgressDetail(String machineID)
        {
            return await Task.Run(() => getAllProgressDetails(machineID));
        }

        public async Task<StartBatchICRModel> GetUpperPartDetail(FormCollection collection, string processName)
        {
            return await Task.Run(() => getUpperPartDetail(collection, processName));
        }

        public string getOCRStatus()
        {
            string status = "";
            DataTable dt = new DataTable();
            string sql = "Select * from tblDataProcessICS where fldProcessName = 'KLInwardOCR'";
            dt = dbContext.GetRecordsAsDataTable(sql);

            if (dt.Rows.Count > 0)
            {

                status = dt.Rows[0]["fldStatus"].ToString();

            }

            if (status != "4")
            {
                if (status != "2")
                {
                    status = "Ready";
                }
                else
                {
                    status = "In Progress";
                }
            }
            else
            {
                status = "Done";
            }

            return status;
        }

        public List<StartBatchICRModel> getMachineId()
        {
            DataTable ds = new DataTable();
            List<StartBatchICRModel> batch = new List<StartBatchICRModel>();
            string stmt = "Select * from tblOCRMachine where fldRecordStatus = 1";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                StartBatchICRModel batchs = new StartBatchICRModel();
                batchs.fldMachineID = row["fldMachineID"].ToString();
                batch.Add(batchs);
            }
            return batch;


        }

        public async Task<double> percentage(String machineID)
        {
            double successCount = 0;
            double failCount = 0;
            double totalCount = 0;
            double percentageCompleted = 0;
            DataTable ds = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@vchProcessID", machineID));

            ds = dbContext.GetRecordsAsDataTableSP("sp_GetOCRPosProcessInfo", sqlParameterNext.ToArray());

            if(ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                successCount = Convert.ToDouble(row["iTotalSuccessRec"]);
                failCount = Convert.ToDouble(row["iTotalFailRec"]);
                totalCount = Convert.ToDouble(row["iTotalRec"]);
            }
            percentageCompleted = ((successCount + failCount) / totalCount) * 100;
            percentageCompleted = Math.Round(percentageCompleted,2);

            return await Task.Run(() => percentageCompleted);

        }

        public StartBatchICRModel ICRProgressDetails(String machineID)
        {
            double successCount = 0;
            double failCount = 0;
            double totalCount = 0;
            //double percentageCompleted = 0;
            DataTable ds = new DataTable();
            StartBatchICRModel batch = new StartBatchICRModel();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@vchProcessID", machineID));

            ds = dbContext.GetRecordsAsDataTableSP("sp_GetOCRPosProcessInfo", sqlParameterNext.ToArray());

            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                successCount = Convert.ToDouble(row["iTotalSuccessRec"]);
                failCount = Convert.ToDouble(row["iTotalFailRec"]);
                totalCount = Convert.ToDouble(row["iTotalRec"]);
            }
            //percentageCompleted = ((successCount + failCount) / totalCount) * 100;
            //percentageCompleted = Math.Round(percentageCompleted, 2);

            batch.fldMachineID = machineID;
            batch.successCount = successCount;
            batch.failCount = failCount;
            batch.totalCount = totalCount;
            batch.itemLeftCount = totalCount - failCount - successCount;

            return batch;

        }

        public StartBatchICRModel getProcessStartEndTime(String processName)
        {
            DataTable ds = new DataTable();
            StartBatchICRModel batch = new StartBatchICRModel();

            string stmt = "select * from tblDataProcessICS where fldProcessName = '" + processName + "'";
            ds = dbContext.GetRecordsAsDataTable(stmt);
                
            if (ds.Rows.Count > 0)
            {
                DataRow row = ds.Rows[0];
                batch.startTime = row["fldStartTime"].ToString();
                batch.endTime = row["fldEndTime"].ToString();
            }
            if (batch.endTime == "")
            {
                batch.endTime = "In Progress";
            }
            
            return batch;

        }

        public List<StartBatchICRModel> getAllProgressDetails(String machineID)
        {
            DataTable ds = new DataTable();
            List<StartBatchICRModel> batch = new List<StartBatchICRModel>();
            string stmt = "select fldChequeSerialNo, fldAccountNumber, fldStatus, fldRemarks " +
                          "from tblOCRTemp a " +
                          "inner join tblInwardItemInfo b on a.fldInwardItemId = b.fldInwardItemId " +
                          "where fldMachineId = '" + machineID + "' ";
            ds = dbContext.GetRecordsAsDataTable(stmt);
            foreach (DataRow row in ds.Rows)
            {
                StartBatchICRModel batchs = new StartBatchICRModel();
                batchs.fldChequeNumber = row["fldChequeSerialNo"].ToString();
                batchs.fldAccountNumber = row["fldAccountNumber"].ToString();
                if(row["fldStatus"].ToString() == "1")
                {
                    batchs.fldStatus = "In Progress";
                }
                else if (row["fldStatus"].ToString() == "2")
                {
                    batchs.fldStatus = "Completed";
                }
                else if (row["fldStatus"].ToString() == "-1")
                {
                    batchs.fldStatus = "Error";
                }
                batchs.fldRemarks = row["fldRemarks"].ToString();
                batch.Add(batchs);
            }
            return batch;
        }

        public StartBatchICRModel getUpperPartDetail(FormCollection collection, string processName)
        {
            StartBatchICRModel batch = new StartBatchICRModel();
            StartBatchICRModel batchs = new StartBatchICRModel();
            batch = ICRProgressDetails(collection["txtMachineID"].ToString());
            batchs = getProcessStartEndTime(processName);
            batch.startTime = batchs.startTime;
            batch.endTime = batchs.endTime;
            return batch;
        }

        public bool checkDataOCRTemp()
        {
            bool status = false;
            DataTable ds = new DataTable();
            StartBatchICRModel batch = new StartBatchICRModel();

            string stmt = "select count(*) As CountData from tblocrtemp ot inner join tblInwardItemInfo on ot.fldInwardItemId = tblInwardItemInfo.fldInwardItemId";
            ds = dbContext.GetRecordsAsDataTable(stmt);

            if (ds.Rows.Count > 0 && ds.Rows[0]["CountData"].ToString() != "0")
            {
                status = true;
            }
            return status;
        }
    }
}