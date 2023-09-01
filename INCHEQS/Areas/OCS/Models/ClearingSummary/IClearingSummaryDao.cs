using System;
using System.Collections.Generic;
using INCHEQS.DataAccessLayer;
using INCHEQS.DataAccessLayer.OCS;
using System.Data;
using System.Data.SqlClient;
using INCHEQS.Models.DbJoin;
using System.Data.SqlTypes;
using System.Text;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security;
namespace INCHEQS.Areas.OCS.Models.ClearingSummary
{
    public interface IClearingSummaryDao
    {

        KeyValuePair<string, List<SqlParameter>> GetSqlParameter(string sqlQuery, Dictionary<string, string> currentUserParams);

        //DataTable GetClearingBranchByUserDataTable(string strUserId, string strBankCode);

        //void GenerateNewBatches(SqlInt32 intLockUserId, DateTime dtClearingDate, string strClearingUIC, string strIgnoreIQA, ref string strErrMesssage, string strBankCode);

        //DataTable GetClearingAgentConfig(string strClearingAgent);

        //DataTable GetDistinctUniqueIdentifierForClearing(string strCurrency, string strBankCode, string strBranchCode, string strChequeType, string dtChequeDate, string strUI, string dtClearingDate, string strIgnoreIQA);

        //DataTable getClearingAgentConfigPerPBM(string strBankCode);

        //bool HasClearableItemByUI(string strUI, string dtClearingDate, string strIgnoreIQA, string strRepresent);

        //DataTable GetItemForClearing(string strCurrency, string strBankCode, string strBranchCode, string strChequeType, string dtChequeDate, string strUI,
        //                                    string dtClearingDate, string strIgnoreIQA, string strIQA, string strRepresent);

        //bool UpdateItemWithClearingBatch(string strUI, int intMaxItemPerBatch, string dtClearingDate, SqlInt32 intLockUserId, string dtLockTimeStamp,
        //                                        string strIgnoreIQA, string strRepresent, string strBankCode);

        //string[] GetNumbers(ClearingSummaryDao.EnumSequenceCode sequenceCode, Int64 bintTotalNumberNeed);

        ////' Get Set Of Min Max New Number(s) From Database
        //string[] GetMinMaxNewNumber(int intNumberID, Int64 bintTotalNumberNeed);

        ////' Get Max Length Return For Table / Column Sequance Number 
        //int ReturnMaxLength(ClearingSummaryDao.EnumSequenceCode sequenceCode);

        ////'Get the Sequence Number of Clearing Batch for Multi Entity
        //string[] GetNumbersForClearingBatch(ClearingSummaryDao.EnumSequenceCode sequenceCode, Int64 bintTotalNumberNeed, string strBankCode);

        //string[] GetMinMaxNewNumberForClearingBatch(int intNumberID, Int64 bintTotalNumberNeed, string strBankCode);

        ////'calling spc to add new clearing status
        //bool AddClearingStatus(SqlInt64 intDateBatch, string strClearingBatch, string strClearingAgent,
        //            string strUI, string strOriginBRSTN, string strTransType, string strCurrency,
        //            string strCurrentProcess, string dtProcessDateTime, string dtCompleteDateTime,
        //            SqlInt32 intErrorCode, string strErrorMsg, string strCTCSclient, string strCTCSclientMsg,
        //            SqlInt32 intCreateUserId, string dtCreateTimeStamp, SqlInt32 intUpdateUserId, string dtUpdateTimeStamp);

        ////'return current process code by current process enumeration
        //string GetCurrentProcessCode(ClearingSummaryDao.EnumCurrentProcessCode CurrentProcess);

        ////'calling spc to add new clearing outbox item
        ////'/********** Param SelectItemByUI: Add/insert new outbox by selecting items from given Batch Number **************/ 
        //bool AddClearingOutbox(string strUI, int intMaxItemPerBatch, SqlInt64 intDateBatch, string dtClearingDate,
        //                    string strIgnoreIQA, SqlInt32 intLockUserId, SqlInt32 intCreateUserId, string dtCreateTimeStamp,
        //                    SqlInt32 intUpdateUserId, string dtUpdateTimeStamp);

        ////(OVERLOADS for Complete status update) calling spc to update clearing status
        //bool UpdateClearingStatus_CompleteStatus(SqlInt64 intDateBatch, string strCurrentProcess, string dtCompleteDateTime, SqlInt32 intErrorCode, string strErrorMsg, SqlInt32 intUserId, string strTaskId, string strBankCode);

        ////'calling spc to update clearing status
        //bool UpdateClearingStatus(SqlInt64 intDateBatch, string strCurrentProcess, string dtProcessDateTime, string dtCompleteDateTime,
        //                                SqlInt32 intErrorCode, string strErrorMsg, string strCTCSclient, string strCTCSclientMsg,
        //                                string strCTCSStatusCode, string strCTCSItemPosition, string strCTCSItemCount,
        //                                string strCTCSImageCount, string strCTCSBatchAmount, string strCTCSStatusDesc,
        //                                SqlInt32 intUserId, string strTaskId, string strBankCode);

        //DataTable GetClearingStatus(string strClearingBatch, SqlInt64 intDateBatch, string strUI, string strClearingAgent, string strCurrentProcess,
        //                                    SqlInt32 intFilterCreateByUserId, string strBankCode, string strGWCClearingDate = "");

        //bool UnlockItemForClearing(SqlInt32 intLockUserId);
    }
}