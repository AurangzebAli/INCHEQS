using INCHEQS.Areas.ICS.Models.ICSAPIDebitPosting;
using INCHEQS.Common;
using INCHEQS.DataAccessLayer;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.Account;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace INCHEQS.Areas.ICS.Models.DayEndSettlement
{
    public class DayEndSettlementDao:IDayEndSettlementDao
    {
        private readonly ApplicationDbContext dbContext;
        protected readonly ISystemProfileDao systemProfileDao;

        public DayEndSettlementDao(ApplicationDbContext dbContext, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.systemProfileDao = systemProfileDao;
        }

        public List<string> ValidateDayEndsettlement(AccountModel currentUser)
        {

            List<string> err = new List<string>();
            string processname = "DayEndSettlment";
            int fldStatus = 4;
            string sql = "Select * from tbldataprocess where fldProcessName" +
                "=@fldProcessName and fldStatus=@fldStatus";
            if (dbContext.CheckExist(sql, new[] { new SqlParameter("@fldProcessName", processname), new SqlParameter("@fldStatus", fldStatus) }))
            {
                err.Add("Already Settled");
            }
            else
            {
                err = new List<string>();
            }




            return err;
        }


        public void InsertDayEndSettlement(DayEndSettlementModel  dayEndSettlementModel,AccountModel currentUser)
        {
            Dictionary<string, dynamic> sqlHostValidation = new Dictionary<string, dynamic>();
            sqlHostValidation.Add("fldPosPayType", dayEndSettlementModel.posPayType);
            sqlHostValidation.Add("fldClearDate", dayEndSettlementModel.clearDate);
            sqlHostValidation.Add("fldStartTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldEndTime", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldProcessName", dayEndSettlementModel.Processname);
            sqlHostValidation.Add("fldBankCode", dayEndSettlementModel.bankCode);
            sqlHostValidation.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetimeForSql());
            sqlHostValidation.Add("fldCreateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldUpdateUserId", currentUser.UserId);
            sqlHostValidation.Add("fldStatus", dayEndSettlementModel.status);
            sqlHostValidation.Add("fldBatchID", 1);


            //Excute the command
            int count = dbContext.ConstructAndExecuteInsertCommand("tblDataProcess", sqlHostValidation);
            if (count == 1)
            {
                SqlParameter[] sqlparam = new SqlParameter[] {
                new SqlParameter("@_clearDate", dayEndSettlementModel.clearDate),
                new SqlParameter("@_createUserID", currentUser.UserId),
                new SqlParameter("@_Processname", dayEndSettlementModel.Processname),
                new SqlParameter("@_fldBatchId", 1),
                new SqlParameter("@_IsRetry1", "0"),
                new SqlParameter("@_ItemPerBatch", 50)

            };


            }
        }


        public string GetTableForDayEndSettlement(FormCollection col,AccountModel currentUser)
        {

            string Message = "";

            try
            {

                //   string cmd = "SELECT  Top(50) * FROM (  SELECT ROW_NUMBER() OVER (ORDER BY fldIssuingBank) AS RowNum, [fldProcessDate] ,[fldIssuingBank] ,[fldProcessName] ,[fldTriggeredBy] ,[fldTriggeredTime] ,[fldStartTime] ,[fldEndTime] ,[fldErrorMsg]  ,  '0' As GrandTotalAmount , '0' As TotalOutstanding , '0' As TotalImportItem , '0' As GrandTotalImportItem , '0' As GrandTotalFile , Count(*) OVER() As TotalRecords , ((COUNT(*)OVER()+50-1)/50) AS TotalPage FROM [View_MICRImageProcess] ) AS RowConstrainedResult ORDER BY RowNum ";
                DataTable dt = dbContext.GetRecordsAsDataTable(" select * from View_DayEndSettlement", null);
                DataRow[] dataRows = dt.Select("fldIssueStateCode <> '20'") ;
                DataSet dsDayEndSettlement = new DataSet("DayEndSettlement");

                if (dataRows.Length !=0)
                {
                    DataTable dtselectgl = dataRows.CopyToDataTable();

                    dsDayEndSettlement.Tables.Add(dtselectgl);
                }
                
                DataRow[] drIssueStateCode = dt.Select("fldIssueStateCode = '20'");
                if (drIssueStateCode.Length !=0)
                {
                    DataTable dtSelect20 = drIssueStateCode.CopyToDataTable();
                    dsDayEndSettlement.Tables.Add(dtSelect20);

                }

                for (int i = 0; i < dsDayEndSettlement.Tables.Count; i++)
                {
                    object totalPrice = dsDayEndSettlement.Tables[i].Compute("Sum(fldAmount)", string.Empty);
                    string accNo = "407130691";
                    FormCollection formCollection = new FormCollection();
                    formCollection.Add("fldIssueBranchCode", "239");
                    formCollection.Add("fldIssueStateCode", "40");
                    formCollection.Add("fldAmount", totalPrice.ToString());



                    APIFundsTransfer apiResponse = new APIFundsTransfer(dbContext);

                    Message = apiResponse.GETAPIReponse(formCollection, accNo, "chequeno");
                    if (Message == "Request Processed Successfully")
                    {

                    }
                }
                
                return Message;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{1} \n\n Generated SQL Statement: \n {0}","Error", ex.Message));
            }
        }

    }
}