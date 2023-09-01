using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Models.CommonInwardItem;
using INCHEQS.Security;
using INCHEQS.Security.Account;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using static INCHEQS.TaskAssignment.TaskIds;

namespace INCHEQS.Areas.ICS.Models.HostValidation
{
    public class HostValidationDao:IHostValidationDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;


        public HostValidationDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }
        public List<string> ValidateHostValidation( AccountModel currentUser)
        {

            List<string> err = new List<string>();
            string processname = "HostValidation";
            int fldStatus = 4;
            string sql = "Select * from tbldataprocess where fldProcessName" +
                "=@fldProcessName and fldStatus=@fldStatus";
            if (dbContext.CheckExist(sql, new[] { new SqlParameter("@fldProcessName", processname), new SqlParameter("@fldStatus", fldStatus) }))
            {
                err.Add("Already Hosted");
            }
            else
            {
                err = new List<string>();
            }


            

            return err;
        }

        public void InsertHostValidation(InwardItemViewModel inwardItemViewModel, AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlHostValidation = new Dictionary<string, dynamic>();
            sqlHostValidation.Add("fldPosPayType", inwardItemViewModel.posPayType);
            sqlHostValidation.Add("fldClearDate", inwardItemViewModel.clearDate);
            sqlHostValidation.Add("fldStartTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldEndTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldProcessName", inwardItemViewModel.Processname);
            sqlHostValidation.Add("fldBankCode", inwardItemViewModel.bankCode);
            sqlHostValidation.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldCreateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldUpdateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldStatus", inwardItemViewModel.status);
            sqlHostValidation.Add("fldBatchID", 1);


            //Excute the command
            int count = dbContext.ConstructAndExecuteInsertCommand("tblDataProcess", sqlHostValidation);
            if (count == 1)
            {
                SqlParameter[] sqlparam = new SqlParameter[] {
                new SqlParameter("@_clearDate", inwardItemViewModel.clearDate),
                new SqlParameter("@_createUserID", currentUser.UserId),
                new SqlParameter("@_Processname", inwardItemViewModel.Processname),
                new SqlParameter("@_fldBatchId", 1),
                new SqlParameter("@_IsRetry1", "0"),
                new SqlParameter("@_ItemPerBatch", 50)

            };
                

            }
        }



        public bool UpdateGenuineness(HostValidationModel inwardItemViewModel, AccountModel currentUser)
        {
            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            
            sqlParameterNext.Add(new SqlParameter("@fldchequeserialNoWithStatus", inwardItemViewModel.Cheque));
            sqlParameterNext.Add(new SqlParameter("@fldGenuine", inwardItemViewModel.IsGenuine));


            int intRowAffected;
            bool blnResult = false;

            intRowAffected = dbContext.ExecuteNonQuery(System.Data.CommandType.StoredProcedure, "spcuGenuiness", sqlParameterNext.ToArray());
            if (intRowAffected > 0)
            {
                blnResult = true;
            }
            else
            {
                blnResult = false;
            }

            return blnResult;


        }

    }
}