using INCHEQS.DataAccessLayer;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.BandQueueSetting {
    public class BandQueueSettingDao : IBandQueueSettingDao
    {
        private readonly ApplicationDbContext dbContext;
        public BandQueueSettingDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public BandQueueSettingModel GetBandQueueSetting(string taskId) {
            DataTable dt = new DataTable();
            BandQueueSettingModel bandQModel = new BandQueueSettingModel();

            string stmt = "Select bq.fldTaskId, isNULL(bq.fldBandLBound,'0')[fldBandLBound], IsNULL(bq.fldBandUBound,'0')[fldBandUBound],isNULL(bq.fldVolumnPercentage, 0)[fldVolumnPercentage], bq.fldIsLastBand, bq.fldActive, tm.fldMenuTitle from tblTaskBandLimit bq left join tblTaskMaster tm On bq.fldTaskId = tm.fldTaskId where bq.fldTaskId = @fldTaskId";

            dt = dbContext.GetRecordsAsDataTable(stmt,new[] { new SqlParameter("@fldTaskId",taskId.Trim())});

            if (dt.Rows.Count > 0) {
                DataRow row = dt.Rows[0];
                bandQModel.fldTaskId=row["fldTaskId"].ToString();
                bandQModel.fldMenuTitle = row["fldMenuTitle"].ToString();
                bandQModel.fldBandLBound = row["fldBandLBound"].ToString();
                bandQModel.fldBandUBound = row["fldBandUBound"].ToString();
                bandQModel.fldVolumnPercentage = row["fldVolumnPercentage"].ToString();
                bandQModel.fldActive = row["fldActive"].ToString();
            }
            return bandQModel;
        }

        public List<string> ValidateCreate(FormCollection col) {
            List <string> err= new List<string>();

            if (col["fldTaskId"].Equals("")) {
                err.Add("Task Id cannot be empty");
            }
            if (CheckExist(col["fldTaskId"])) {
                err.Add("Task Id already exist");
            }
            if (col["fldBandLBound"].Equals("")) {
                err.Add("Lower Bound cannot be empty");
            }
            if (col["fldBandUBound"].Equals("")) {
                err.Add("Upper Bound cannot be empty");
            }
            return err;
        }

        public bool CheckExist(string taskId) {
            string stmt = "select fldTaskId from tblTaskBandLimit where fldTaskId = @fldTaskId";
            return dbContext.CheckExist(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
        }

        public List<string> ValidateUpdate(FormCollection col) {
            List<string> err = new List<string>();

            if (col["fldBandLBound"].Equals("")) {
                err.Add("Lower Bound cannot be empty");
            }
            if (col["fldBandUBound"].Equals("")) {
                err.Add("Upper Bound cannot be empty");
            }
            return err;
        }

        public void CreateBandQueueSetting(FormCollection col, AccountModel currentUser) {
            Dictionary<string, dynamic> sqlBandQueue = new Dictionary<string, dynamic>();

            sqlBandQueue.Add("fldTaskId",col["fldTaskId"]);
            sqlBandQueue.Add("fldBandLBound", col["fldBandLBound"]);
            sqlBandQueue.Add("fldBandUBound", col["fldBandUBound"]);
            sqlBandQueue.Add("fldVolumnPercentage", col["fldVolumnPercentage"]);
            sqlBandQueue.Add("fldActive", col["fldActive"]);
            sqlBandQueue.Add("fldEntityCode", currentUser.BankCode);
            sqlBandQueue.Add("fldBankcode", currentUser.BankCode);

            dbContext.ConstructAndExecuteInsertCommand("tblTaskBandLimit", sqlBandQueue);
        }
        
        public void UpdateBandQueueSetting(FormCollection col) {
            string stmt = "update tblTaskBandLimit set fldBandLBound=@fldBandLBound,fldBandUBound= @fldBandUBound, fldVolumnPercentage = @fldVolumnPercentage, fldActive = @fldActive where fldTaskId=@fldTaskId";
            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldTaskId", col["fldTaskId"]),
                new SqlParameter("@fldBandLBound", col["fldBandLBound"]),
                new SqlParameter("@fldBandUBound", col["fldBandUBound"]),
                new SqlParameter("@fldVolumnPercentage", col["fldVolumnPercentage"]),
                new SqlParameter("@fldActive", col["fldActive"])
            });
        }

        public void DeleteFromBandQueueSetting(string taskId) {
            string stmt = "delete from tblTaskBandLimit where fldTaskId=@fldTaskId ";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldTaskId", taskId) });
        }
    }
}