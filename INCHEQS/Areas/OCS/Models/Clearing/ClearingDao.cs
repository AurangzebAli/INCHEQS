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

namespace INCHEQS.Areas.OCS.Models.Clearing
{
    public class ClearingDao : IClearingDao
    {
        public enum EnumSequenceCode : int {
            tblSystemLog = 1,
            tblAuditLog = 2,
            tblErrorLog = 3,
            tblDataImageConfig = 4,
            tblDataImageLog = 5,
            tblSecurityLog = 6,
            tblUserPwdHistory = 7,
            tblSecurityConfig = 8,
            tblReportRowColumnSetting = 9,
            fldItemID = 10,
            fldVItemID = 11,
            fldItemInitialID = 12,
            fldClearingBatch = 13,
            fldPostingBatch = 14,
            fldTransactionNo = 15
        }

        public enum EnumCurrentProcessCode : int {
            NewCreated = 0,
            ImageCopyToAgent = 1,
            ImageConversion = 2,
            PKIEncryption = 3,
            FileCopyToGateway = 4,
            BOTProcess = 5,
            CTMCompleted = 6,
            BOTError = 7,
            Rebatched = 8
        }

        private readonly ApplicationDbContext dbContext;
        private readonly OCSDbContext ocsdbContext;
        private readonly DbJoinDao dbJoinDao;
        private readonly ISystemProfileDao systemProfileDao;

        public ClearingDao(ApplicationDbContext dbContext, OCSDbContext ocsdbContext, DbJoinDao dbJoinDao, ISystemProfileDao systemProfileDao)
        {
            this.dbContext = dbContext;
            this.ocsdbContext = ocsdbContext;
            this.dbJoinDao = dbJoinDao;
            this.systemProfileDao = systemProfileDao;
        }

        public DataTable GetClearingBranchByUserDataTable(string strUserId, string strBankCode) {
            DataTable dt = new DataTable();
            string strQueryCommon = "SELECT CONCAT(RTrim(bm.fldStateCode), RTrim(bm.fldBankCode), RTrim(bm.fldBranchCode)) as BranchId, fldBranchDesc as BranchDesc FROM tblBranchMaster bm " +
                                    "WHERE fldBankCode = @fldBankCode ";

            SqlParameter[] paramsCommon = new SqlParameter[] {
                new SqlParameter("@fldBankCode", strBankCode)
            };


            string strQueryOCS = "SELECT fldBranchId FROM tblHubBranch hb " +
                                    "INNER JOIN tblHubUser hu ON hu.fldHubCode = hb.fldHubCode " +
                                    "INNER JOIN tblHubMaster hm ON hu.fldHubCode = hm.fldHubCode " +
                                    "WHERE hu.fldUserId = @fldUserId " +
                                    "AND hm.fldApprovalStatus = @fldApprovalStatus ";

            SqlParameter[] paramsOCS = new SqlParameter[] {
                new SqlParameter("@fldUserId", strUserId),
                new SqlParameter("@fldApprovalStatus", "Y")
            };

            dt = dbJoinDao.GetCommonDatableFromOCS(strQueryCommon, paramsCommon, "BranchId", strQueryOCS, paramsOCS, "fldBranchId");

            return dt;
        }

        public KeyValuePair<string, List<SqlParameter>> GetSqlParameter(string sqlQuery, Dictionary<string, string> currentUserParams)
        {
            List<SqlParameter> sqlParams = new List<SqlParameter>();

            string strUserId = currentUserParams["currentUserId"];
            string strBankCode = currentUserParams["currentUserBankCode"];
            string strUserBranch = "";

            DataTable dtBranches = GetClearingBranchByUserDataTable(strUserId, strBankCode);
            if (dtBranches.Rows.Count > 0) {
                foreach (DataRow row in dtBranches.Rows) {
                    if (strUserBranch != "") strUserBranch += ",";
                    strUserBranch += row["BranchId"].ToString();
                }
            }
            else
            {
                strUserBranch = "''";
            }

            sqlParams.Add(new SqlParameter("@currency", ""));
            sqlParams.Add(new SqlParameter("@bankCode", strBankCode));
            sqlParams.Add(new SqlParameter("@branchCode", strUserBranch));
            sqlParams.Add(new SqlParameter("@chequeType", ""));
            sqlParams.Add(new SqlParameter("@chequeDate", ""));
            sqlParams.Add(new SqlParameter("@ui", ""));
            sqlParams.Add(new SqlParameter("@clearingDate", ""));
            sqlParams.Add(new SqlParameter("@ignoreIQA", ""));

            return new KeyValuePair<string, List<SqlParameter>>(sqlQuery, sqlParams);

        }

        public void GenerateNewBatches(SqlInt32 intLockUserId, DateTime dtClearingDate, string strClearingUIC, string strIgnoreIQA, ref string strErrMesssage, string strBankCode) {
            DataTable ItemDataTable; DataTable AgentDataTable;//    Dim ItemDataTable As DataTable : Dim AgentDataTable As DataTable
            //DataRow drwItem;//    Dim drwItem As DataRow
            bool blnThisUIItemExists = false;//    Dim blnThisUIItemExists As Boolean = False
            long lngClearingBatchId = 0; long lngDateBatch = 0; int intMaxItemPerBatch = 0;//    Dim lngClearingBatchId As Long = 0 : Dim lngDateBatch As Long = 0 : Dim intMaxItemPerBatch As Integer = 0
            string strNextClearingAgent = ""; int intNextAgent = 0;//    Dim strNextClearingAgent As String = "" : Dim intNextAgent As Integer = 0

            //    Dim objDataEntryItemThai As New clsDataEntryItemThai
            //    AddHandler objDataEntryItemThai.evtError, AddressOf eventCall
            string strRepresent = "N";//    Dim strRepresent As String = "N"
            string strRepresentClearingUIC = "";//    Dim strRepresentClearingUIC As String = ""
            string strRepresentTransType = "";//    Dim strRepresentTransType As String = ""
            int intFailedNextId;//    Dim intFailedNextId As Integer

            //    'Get System Profile for MultiEntity Clearing Batch
            int intMultiEntityClearingBatch;//    Dim intMultiEntityClearingBatch As Integer
            int intMultiEntityDateBatch;//    Dim intMultiEntityDateBatch As Integer
            string intBankCode;//    Dim intBankCode As String
            //    Dim clsUser As New clsUserProfile

            //    'BEGIN MGI 20170421 Add variable to get the branch in the UIC
            DataTable dtClearingAgentConfigPerPBM;//    Dim dtClearingAgentConfigPerPBM As DataTable
            string[] arClearingAgent = new string[] { };//    Dim arClearingAgent() As String
            string[] arClearingConfig = new string[] { };//    Dim arClearingConfig() As String
            int intClearingCount = 0;//    Dim intClearingCount As Integer = 0


            //intReturnCode = 0;//    intReturnCode = 0

            int intErrorCode = 0; StringBuilder strErrorMsg = new StringBuilder(); StringBuilder strDisplayErrorMsg = new StringBuilder();//    Dim intErrorCode As Integer = 0 : Dim strErrorMsg As New StringBuilder : Dim strDisplayErrorMsg As New StringBuilder

            //    Try
            //        'Step 1: Get distinct [UI] Clearing Unique Identifier (ClearingDate+BankCode+BranchCode+Currency+ChequeType) order by !! PRIORITY
            //        'Step 2: Loop the CUI, gets running batch number from Number Manager
            //        'Step 3: While looping Step 2, gets all 'Clearable items' and update 'Lock User' flag
            //        'Step 4: Insert clearing batch and assign clearing agent to clear the outbox items
            //        'Step 5: Continuing from Step 3, insert the item ID to outbox ([SystemProfile.MaximumBatchCount] items per bucket), 
            //        '                    And update batch number to item INFO
            //        '  
            //        'Step 6: Connect to agent's map drive(File System IO) and create proper folder structures and do file copy of all images(At webform)
            //        'Step 7: Generate XML (At webform)
            //        'Commit Step 6 - Step 7

            //        'Step 8: Reset lock user flag ???

            //        '/************** Get System Profile and Clearing Agent Config *****************/
            intMaxItemPerBatch = Convert.ToInt32(systemProfileDao.GetValueFromSystemProfile("MaxItemPerClearingBatch", CurrentUser.Account.BankCode));//        intMaxItemPerBatch = CType(clsSystemProfile.GetSystemProfileValueByCode("MaxItemPerClearingBatch", intReturnCode), Integer)

            //        'Get the System Profile for Muli Entity
            intMultiEntityClearingBatch = Convert.ToInt32(systemProfileDao.GetValueFromSystemProfile("MultiEntitySeqClearingBatch", CurrentUser.Account.BankCode));//        intMultiEntityClearingBatch = CType(clsSystemProfile.GetSystemProfileValueByCode("MultiEntitySeqClearingBatch", intReturnCode), Integer)
            intMultiEntityDateBatch = Convert.ToInt32(systemProfileDao.GetValueFromSystemProfile("MultiEntityDateBatch", CurrentUser.Account.BankCode));//        intMultiEntityDateBatch = CType(clsSystemProfile.GetSystemProfileValueByCode("MultiEntityDateBatch", intReturnCode), Integer)

            string struserBankCode = CurrentUser.Account.BankCode;//        Dim struserBankCode As String = ""
            //        Dim userTable As DataTable
            //        userTable = clsUser.GetBankRecords(CInt(intLockUserId))
            //        struserBankCode = userTable.Rows.Item(0).Item("fldBankCode").ToString

            AgentDataTable = GetClearingAgentConfig("");//        AgentDataTable = clsClearingAgent.GetClearingAgentConfig("", intReturnCode)
            if (AgentDataTable.Rows.Count > 0) {//        If AgentDataTable.Rows.Count > 0 Then
                                                //            '/************** Step 1 *****************/
                ItemDataTable = GetDistinctUniqueIdentifierForClearing("", struserBankCode, "", "", "", strClearingUIC, dtClearingDate.ToString(), strIgnoreIQA).Copy();//            ItemDataTable = clsDataEntryItemThai.GetDistinctUniqueIdentifierForClearing("", struserBankCode, "", "", "", strClearingUIC, dtClearingDate.ToString, strIgnoreIQA,
                                                                                                                                                                        //                                Nothing, intReturnCode).Copy

                dtClearingAgentConfigPerPBM = getClearingAgentConfigPerPBM(struserBankCode);//            dtClearingAgentConfigPerPBM = getClearingAgentConfigPerPBM(struserBankCode, intReturnCode)

                intClearingCount = dtClearingAgentConfigPerPBM.Rows.Count;//            intClearingCount = dtClearingAgentConfigPerPBM.Rows.Count - 1

                Array.Resize(ref arClearingAgent, intClearingCount);//            ReDim arClearingAgent(intClearingCount)
                Array.Resize(ref arClearingConfig, intClearingCount);//            ReDim arClearingConfig(intClearingCount)

                for (int i = 0; i <= dtClearingAgentConfigPerPBM.Rows.Count - 1; i++)
                {//            For i As Integer = 0 To dtClearingAgentConfigPerPBM.Rows.Count - 1
                    DataRow row = dtClearingAgentConfigPerPBM.Rows[i];//                Dim row As DataRow = dtClearingAgentConfigPerPBM.Rows(i)
                    if (i != dtClearingAgentConfigPerPBM.Rows.Count)
                    {//                If i <> dtClearingAgentConfigPerPBM.Rows.Count Then
                        if ((i + 1) != dtClearingAgentConfigPerPBM.Rows.Count) {
                            DataRow nextRow = dtClearingAgentConfigPerPBM.Rows[i + 1];
                        }
                        //                    Dim nextRow As DataRow = dtClearingAgentConfigPerPBM(i + 1)
                        //                    'If row("fldClearingAgent").Equals(nextRow("fldClearingAgent")) Then
                        //                    'do something"
                        arClearingAgent[i] = dtClearingAgentConfigPerPBM.Rows[i]["fldClearingAgent"].ToString();//                    arClearingAgent(i) = dtClearingAgentConfigPerPBM.Rows(i).Item("fldClearingAgent").ToString
                                                                                                                //                    'If
                    }//                End If
                }//            Next

                foreach (DataRow drwItem in ItemDataTable.Rows) {//            For Each drwItem In ItemDataTable.Rows
                    blnThisUIItemExists = true;//                blnThisUIItemExists = True
                                               //                'BeginMod #06 If the items are only Representment Items, straight away generate representment batch
                    if (!HasClearableItemByUI(drwItem["ID"].ToString().Trim(), dtClearingDate.ToString(), strIgnoreIQA, strRepresent)) {//                If Not clsDataEntryItemThai.HasClearableItemByUI(drwItem.Item("ID").ToString.Trim, dtClearingDate.ToString, strIgnoreIQA, strRepresent, Nothing, intReturnCode) Then
                        strRepresent = "Y";//                    strRepresent = "Y"
                    }//                End If

                    while (blnThisUIItemExists) {//                While blnThisUIItemExists

                        strRepresentClearingUIC = Convert.ToString(drwItem["ID"]);//                    strRepresentClearingUIC = CType(drwItem.Item("ID"), String)



                        if (strRepresent == "Y") {//                    If strRepresent = "Y" Then
                            strRepresentTransType = systemProfileDao.GetValueFromSystemProfile("RepresentmentTransType", CurrentUser.Account.BankCode);//                        strRepresentTransType = clsSystemProfile.GetSystemProfileValueByCode("RepresentmentTransType", 0) '-- Mod 2007-11-22 wkCheah
                            if (strRepresentTransType == "") {//                        If strRepresentTransType = "" Then
                                strRepresentTransType = "21";//                            strRepresentTransType = "21"
                            }//                        End If
                            strRepresentClearingUIC = strRepresentTransType;//                        strRepresentClearingUIC = Mid(strRepresentClearingUIC, 1, 18) & strRepresentTransType
                        }//                    End If

                        //                    'strNextClearingAgent = AgentDataTable.Rows(intNextAgent).Item("fldClearingAgent").ToString : intNextAgent += 1
                        //                    'strNextClearingAgent = dtClearingAgentConfigByBranch.Rows(0)("fldClearingAgentConfig").ToString : intNextAgent += 1

                        //                    System.Threading.Thread.Sleep(intSleep)

                        //'/************** Step 3 *****************/
                        if (UpdateItemWithClearingBatch(Convert.ToString(drwItem["ID"]), intMaxItemPerBatch, dtClearingDate.ToString(), intLockUserId, DateTime.Now.ToString(), strIgnoreIQA, strRepresent, strBankCode)) {//                    If objDataEntryItemThai.UpdateItemWithClearingBatch(CType(drwItem.Item("ID"), String), intMaxItemPerBatch, dtClearingDate.ToString, intLockUserId, Now.ToString, _
                                                                                                                                                                                                              //                        strIgnoreIQA, strRepresent, Nothing, intReturnCode) Then

                            intFailedNextId = 1;//                        intFailedNextId = 1
                            lngClearingBatchId = 0;//                        lngClearingBatchId = 0
                            while (lngClearingBatchId <= 0) {//                        While lngClearingBatchId <= 0 '-- Mod 2008-01-08 wkCheah - This indicates the number manager not able to return next ID, it will go back to whileloop again

                                if (intMultiEntityClearingBatch == 0) {//                            If intMultiEntityClearingBatch = 0 Then
                                    lngClearingBatchId = Convert.ToInt64(GetNumbers(EnumSequenceCode.fldClearingBatch, 1));//                                lngClearingBatchId = CType(clsNumberManager.GetNumbers(clsNumberManager.EnumSequenceCode.fldClearingBatch, 1)(0), Long) '--Mod wkCheah 2007-12-26
                                }
                                else {//                            Else
                                    lngClearingBatchId = Convert.ToInt64(GetNumbersForClearingBatch(EnumSequenceCode.fldClearingBatch, 1, strBankCode)[0]);//                                lngClearingBatchId = CType(clsNumberManager.GetNumbersForClearingBatch(clsNumberManager.EnumSequenceCode.fldClearingBatch, 1, CInt(intLockUserId))(0), Long) '--Mod MGI 2016-07-224
                                }//                            End If


                                strNextClearingAgent = arClearingAgent[Convert.ToInt32(lngClearingBatchId) % arClearingAgent.Length];//                            strNextClearingAgent = arClearingAgent(CInt(lngClearingBatchId) Mod arClearingAgent.Length)
                                if (intNextAgent >= AgentDataTable.Rows.Count) {//                            If intNextAgent >= AgentDataTable.Rows.Count Then
                                    intNextAgent = 0;//                                intNextAgent = 0
                                }//                            End If


                                if (lngClearingBatchId > 0) {//                            If lngClearingBatchId > 0 Then

                                    //                                'lngDateBatch = CType(drwItem.Item("ClearingDate").ToString & lngClearingBatchId.ToString.PadLeft(6, CChar("0")), Long)

                                    intBankCode = strBankCode;//                                intBankCode = CType(clsUser.GetBankRecords(CInt(intLockUserId)), DataTable).Rows(0)("fldBankCode").ToString


                                    if (intMultiEntityDateBatch == 1)
                                    {//                                If intMultiEntityDateBatch = 1 Then
                                        lngDateBatch = Convert.ToInt64(drwItem["ClearingDate"].ToString() + lngClearingBatchId.ToString().PadLeft(6, '0') + intBankCode);//                                    lngDateBatch = CType(drwItem.Item("ClearingDate").ToString & lngClearingBatchId.ToString.PadLeft(6, CChar("0")) & intBankCode, Long)
                                    }
                                    else
                                    {//                                Else
                                        lngDateBatch = Convert.ToInt64(drwItem["ClearingDate"].ToString() + lngClearingBatchId.ToString().PadLeft(6, '0'));//                                    lngDateBatch = CType(drwItem.Item("ClearingDate").ToString & lngClearingBatchId.ToString.PadLeft(6, CChar("0")), Long)
                                    }                                                                                                                       //                                End If

                                    //'/************** Step 4 *****************/
                                    if (AddClearingStatus(lngDateBatch, lngClearingBatchId.ToString().PadLeft(10, '0'), strNextClearingAgent, strRepresentClearingUIC, drwItem["fldClearingBranch"].ToString().Trim(),
                                                            drwItem["fldClearingTransType"].ToString().Trim(), drwItem["fldCurrency"].ToString().Trim(),
                                                            GetCurrentProcessCode(EnumCurrentProcessCode.NewCreated), "", "", System.Data.SqlTypes.SqlInt32.Null, "", "", "", intLockUserId, DateTime.Now.ToString(), intLockUserId, DateTime.Now.ToString()))
                                    {//                                If AddClearingStatus(lngDateBatch, lngClearingBatchId.ToString.PadLeft(10, CChar("0")), strNextClearingAgent, _
                                     //                                              strRepresentClearingUIC, _
                                     //                                              drwItem.Item("fldClearingBranch").ToString.Trim, _
                                     //                                              drwItem.Item("fldClearingTransType").ToString.Trim, drwItem.Item("fldCurrency").ToString.Trim, _
                                     //                                              GetCurrentProcessCode(EnumCurrentProcessCode.NewCreated), "", "", _
                                     //                                              Nothing, "", "", "", _
                                     //                                              intLockUserId, Now.ToString, intLockUserId, Now.ToString, _
                                     //                                              Nothing, intReturnCode) Then

                                        //'/************** Step 5 *****************/
                                        AddClearingOutbox(Convert.ToString(drwItem["ID"]), intMaxItemPerBatch, lngDateBatch, dtClearingDate.ToString(), strIgnoreIQA,
                                                        intLockUserId, intLockUserId, DateTime.Now.ToString(), intLockUserId, DateTime.Now.ToString());//                                    AddClearingOutbox(CType(drwItem.Item("ID"), String), intMaxItemPerBatch, lngDateBatch, dtClearingDate.ToString, strIgnoreIQA, _
                                                                                                                                                       //                                       intLockUserId, intLockUserId, Now.ToString, intLockUserId, Now.ToString, Nothing, intReturnCode)

                                        //'/*********PHP Data Process**************/
                                        // 'Temporary Set 
                                        string strProcessName = "GenOutwardICL";//                                    Dim strProcessName As String = "GenOutwardICL"
                                        int intStatus = 1;//                                    Dim intStatus As Integer = 1
                                        string strCriticalMessage = "";//                                    Dim strCriticalMessage As String = ""

                                        //                                    'AddDataProcess(strProcessName, dtClearingDate.ToString, intStatus, CInt(intLockUserId), intReturnCode)
                                        //                                    'intLockUserId, intLockUserId, Now.ToString, intLockUserId, Now.ToString, Nothing, intReturnCode)


                                        //                                    ''/************************************************** Update Start status *****************************************************/
                                        //                                    'If UpdateClearingStatus_ProcessStart(lngDateBatch, GetCurrentProcessCode(EnumCurrentProcessCode.ImageCopyToAgent).ToString, _
                                        //                                    '                           Now.ToString, intLockUserId, "", intReturnCode) Then
                                        //                                    'End If

                                        //'/************************************************** Update Process and complete time status *****************************************************/
                                        if (!UpdateClearingStatus_CompleteStatus(lngDateBatch, GetCurrentProcessCode(EnumCurrentProcessCode.ImageCopyToAgent).ToString(),
                                                                                    DateTime.Now.ToString(), Convert.ToInt32(intErrorCode == 0 ? 0 : intErrorCode), strErrorMsg.ToString(), intLockUserId, "", strBankCode))
                                        {//                                    If Not UpdateClearingStatus_CompleteStatus(lngDateBatch, GetCurrentProcessCode(EnumCurrentProcessCode.ImageCopyToAgent).ToString, _
                                         //                                                Now.ToString, CType(IIf(intErrorCode = 0, 0, intErrorCode), Int32), strErrorMsg.ToString, intLockUserId, "", intReturnCode) Then
                                         //'/************** ERROR: Failed to update clearing batch status *************/
                                            strCriticalMessage = "Fail Update From New to CPY";//                                        strCriticalMessage = "Fail Update From New to CPY"
                                            UpdateClearingStatus_CompleteStatus(lngDateBatch, GetCurrentProcessCode(EnumCurrentProcessCode.ImageCopyToAgent).ToString(), DateTime.Now.ToString(), 5, strCriticalMessage, intLockUserId, "", strBankCode);//                                        clsClearingProcessThai.UpdateClearingStatus_CompleteStatus(lngDateBatch, GetCurrentProcessCode(clsClearingProcessThai.EnumCurrentProcessCode.ImageCopyToAgent).ToString,
                                                                                                                                                                                                                                            //                                        Now.ToString, 5, strCriticalMessage, intLockUserId, "", intReturnCode)

                                        }//                                    End If

                                    }//                                End If
                                    break;//                                Exit While
                                }
                                else {//                            Else
                                    if (intFailedNextId == 500) {//                                If intFailedNextId = 500 Then
                                        UnlockItemForClearing(intLockUserId);//                                    UnlockItemForClearing(intLockUserId, intReturnCode)
                                        break;//                                    Exit While
                                    }//                                End If
                                    intFailedNextId += 1;//                                intFailedNextId += 1
                                }//                            End If
                            }//                        End While
                        }//                    End If

                        //'Once completed on normal items, continue to process representment items
                        if (HasClearableItemByUI(drwItem["ID"].ToString().Trim(), dtClearingDate.ToString(), strIgnoreIQA, strRepresent)) {//                    If clsDataEntryItemThai.HasClearableItemByUI(drwItem.Item("ID").ToString.Trim, dtClearingDate.ToString, strIgnoreIQA, strRepresent, Nothing, intReturnCode) Then '#01
                            blnThisUIItemExists = true; }//                        blnThisUIItemExists = True
                        else if (strRepresent == "Y") {            //                    ElseIf strRepresent = "Y" Then
                            blnThisUIItemExists = false; }//                        blnThisUIItemExists = False
                        else {//                    Else
                            strRepresent = "Y";//                        strRepresent = "Y"
                            if (HasClearableItemByUI(drwItem["ID"].ToString().Trim(), dtClearingDate.ToString(), strIgnoreIQA, strRepresent)) {//                        If clsDataEntryItemThai.HasClearableItemByUI(drwItem.Item("ID").ToString.Trim, dtClearingDate.ToString, strIgnoreIQA, strRepresent, Nothing, intReturnCode) Then
                                blnThisUIItemExists = true; }//                            blnThisUIItemExists = True
                            else {//                        Else
                                blnThisUIItemExists = false;//                            blnThisUIItemExists = False
                            }//                        End If
                        }//                    End If
                    }//                End While
                    strRepresent = "N";//                strRepresent = "N"
                }//            Next
            }//        End If
                                               //    Catch ex As Exception
                                               //        intReturnCode = -1
                                               //        strErrMessage = ex.GetBaseException.Message
                                               //    Finally
                                               //        RemoveHandler objDataEntryItemThai.evtError, AddressOf eventCall
                                               //    End Try
        }


        public DataTable GetClearingAgentConfig(string strClearingAgent)
        {
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@clearingAgent", strClearingAgent));
            dt = ocsdbContext.GetRecordsAsDataTableSP("spcgClearingAgentConfig", sqlParameterNext.ToArray());

            return dt;
        }

        public DataTable GetDistinctUniqueIdentifierForClearing(string strCurrency, string strBankCode, string strBranchCode, string strChequeType, string dtChequeDate, string strUI, string dtClearingDate, string strIgnoreIQA)
        {
            DataTable ItemDataTable = new DataTable();
            DataColumn objChequeUICDateColumn;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@currency", strCurrency));
            sqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@branchCode", strBranchCode));
            sqlParameterNext.Add(new SqlParameter("@chequeType", strChequeType));
            sqlParameterNext.Add(new SqlParameter("@chequeDate", dtChequeDate));
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            ItemDataTable = ocsdbContext.GetRecordsAsDataTableSP("spcgItemUIForClearingListThai", sqlParameterNext.ToArray());

            objChequeUICDateColumn = new DataColumn("ChequeUICDate", typeof(string));
            objChequeUICDateColumn.DefaultValue = "";
            ItemDataTable.Columns.Add(objChequeUICDateColumn);

            ItemDataTable.Columns["ChequeUICDate"].Expression = "Substring(ID, 1, 8)";

            return ItemDataTable;
        }

        public DataTable getClearingAgentConfigPerPBM(string strBankCode)
        {
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@BankCode", strBankCode));
            dt = ocsdbContext.GetRecordsAsDataTableSP("spcgClearingConfigPerPBM", sqlParameterNext.ToArray());

            return dt;
        }

        public bool HasClearableItemByUI(string strUI, string dtClearingDate, string strIgnoreIQA, string strRepresent)
        {
            DataTable ItemDataTable = new DataTable();
            bool blnHasItem = false;

            string strBankCode;
            strBankCode = strUI.Substring(10, 3);

            ItemDataTable = GetItemForClearing("", strBankCode, "", "", "", strUI, dtClearingDate, strIgnoreIQA, "", strRepresent);

            if (ItemDataTable.Rows.Count > 0) {//If ItemDataTable.Rows.Count > 0 Then
                blnHasItem = true;//    blnHasItem = True
            }
            else {//Else
                blnHasItem = false;//    blnHasItem = False
            }//End If

            return blnHasItem;
        }

        public DataTable GetItemForClearing(string strCurrency, string strBankCode, string strBranchCode, string strChequeType, string dtChequeDate, string strUI,
                                            string dtClearingDate, string strIgnoreIQA, string strIQA, string strRepresent)
        {
            DataTable dt = new DataTable();

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@currency", strCurrency));
            sqlParameterNext.Add(new SqlParameter("@bankCode", strBankCode));
            sqlParameterNext.Add(new SqlParameter("@branchCode", strBranchCode));
            sqlParameterNext.Add(new SqlParameter("@chequeType", strChequeType));
            sqlParameterNext.Add(new SqlParameter("@chequeDate", dtChequeDate));
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            sqlParameterNext.Add(new SqlParameter("@IQA", strIQA));
            sqlParameterNext.Add(new SqlParameter("@represent", strRepresent));
            dt = ocsdbContext.GetRecordsAsDataTableSP("spcgItemForClearingListThai", sqlParameterNext.ToArray());

            return dt;
        }

        public bool UpdateItemWithClearingBatch(string strUI, int intMaxItemPerBatch, string dtClearingDate, SqlInt32 intLockUserId, string dtLockTimeStamp,
                                                string strIgnoreIQA, string strRepresent, string strBankCode)
        {
            bool blnResult = false;

            //if (intLockUserId.IsNull) {
            //    TempData["ErrorMsg"] = "User id cannot be null";
            //}

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("topCount", intMaxItemPerBatch));
            sqlParameterNext.Add(new SqlParameter("clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            sqlParameterNext.Add(new SqlParameter("@lockUser", intLockUserId));
            sqlParameterNext.Add(new SqlParameter("@lockTimeStamp", dtLockTimeStamp));
            sqlParameterNext.Add(new SqlParameter("@represent", strRepresent));
            sqlParameterNext.Add(new SqlParameter("@BankCode", strBankCode));
            ocsdbContext.GetRecordsAsDataTableSP("spcuItemBatchForClearingThai", sqlParameterNext.ToArray());
            blnResult = true;
            return blnResult;
        }

        public string[] GetNumbers(EnumSequenceCode sequenceCode, Int64 bintTotalNumberNeed)
        {
            string[] strArrReturn = {"", ""};//Dim strArrReturn() As String = { "", ""}

            //' Parameters Verification
            //If IsNothing(sequenceCode)Then Throw New INCHEQSException.clsBusinessException("sequenceCode cannot be NULL")
            //If IsNothing(bintTotalNumberNeed) Then Throw New INCHEQSException.clsBusinessException("bintTotalNumberNeed cannot be NULL")

            strArrReturn = GetMinMaxNewNumber(Convert.ToInt32(sequenceCode), bintTotalNumberNeed);

            return strArrReturn;
        }

        //' Get Set Of Min Max New Number(s) From Database
        public string[] GetMinMaxNewNumber(int intNumberID, Int64 bintTotalNumberNeed)
        {
            string[] strArrOutput = { "", "" };

            //' Parameters Verification
            //If IsNothing(intNumberID)Then Throw New INCHEQSException.clsBusinessException("intNumberID cannot be NULL")
            //If IsNothing(bintTotalNumberNeed) Then Throw New INCHEQSException.clsBusinessException("bintTotalNumberNeed cannot be NULL")

            List <SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@intColumnID", intNumberID));
            sqlParameterNext.Add(new SqlParameter("@intTotalNumbers", bintTotalNumberNeed));
            sqlParameterNext.Add(new SqlParameter("@intNumberMaxLength", ReturnMaxLength((EnumSequenceCode)(intNumberID))));

            sqlParameterNext.Add(new SqlParameter("@bintNewStartNumber", 1));
            sqlParameterNext[3].Direction = ParameterDirection.Output;
            sqlParameterNext.Add(new SqlParameter("@bintNewEndNumber", 1));
            sqlParameterNext[4].Direction = ParameterDirection.Output;

            ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuNewStartEndNumberByType", sqlParameterNext.ToArray());

            //' get return value from store procedure
            Int64 bintStartNumber = 0;//Dim bintStartNumber As Int64 = 0
            Int64 bintEndNumber = 0;//Dim bintEndNumber As Int64 = 0

            bintStartNumber = (long)sqlParameterNext[3].Value;
            bintEndNumber = (long)sqlParameterNext[4].Value;

            strArrOutput[0] = Convert.ToString(bintStartNumber);//strArrOutput(0) = CType(bintStartNumber, String)
            strArrOutput[1] = Convert.ToString(bintEndNumber);//strArrOutput(1) = CType(bintEndNumber, String)

            return strArrOutput;
        }

        //' Get Max Length Return For Table / Column Sequance Number 
        public int ReturnMaxLength(EnumSequenceCode sequenceCode)
        {
            //' Parameters Verification
            //If IsNothing(sequenceCode)Then Throw New INCHEQSException.clsBusinessException("sequenceCode cannot be NULL")

            switch (sequenceCode) {
                case EnumSequenceCode.fldItemID://Case EnumSequenceCode.fldItemID
                    return 16;//    Return 16

                case EnumSequenceCode.fldVItemID://Case EnumSequenceCode.fldVItemID
                    return 16;//    Return 16

                case EnumSequenceCode.fldItemInitialID://Case EnumSequenceCode.fldItemInitialID
                    return 16;//    Return 16

                case EnumSequenceCode.fldClearingBatch: //Case EnumSequenceCode.fldClearingBatch
                    return 16;//    Return 16

                case EnumSequenceCode.fldPostingBatch://Case EnumSequenceCode.fldPostingBatch
                    return 16; //    Return 16

                case EnumSequenceCode.fldTransactionNo://Case EnumSequenceCode.fldTransactionNo
                    return 16;//    Return 16

                default://Case Else
                    return 0;//    Throw New INCHEQSException.clsBusinessException("sequenceCode not valid")

            }
        }

        //'Get the Sequence Number of Clearing Batch for Multi Entity
        public string[] GetNumbersForClearingBatch(EnumSequenceCode sequenceCode, Int64 bintTotalNumberNeed, string strBankCode)
        {
            string[] strArrReturn = { "", "" };//Dim strArrReturn() As String = { "", ""}

            //' Parameters Verification
            //If IsNothing(sequenceCode)Then Throw New INCHEQSException.clsBusinessException("sequenceCode cannot be NULL")
            //If IsNothing(bintTotalNumberNeed) Then Throw New INCHEQSException.clsBusinessException("bintTotalNumberNeed cannot be NULL")

            strArrReturn = GetMinMaxNewNumberForClearingBatch(Convert.ToInt32(sequenceCode), bintTotalNumberNeed, strBankCode);

            return strArrReturn;
        }

        public string[] GetMinMaxNewNumberForClearingBatch(int intNumberID, Int64 bintTotalNumberNeed, string strBankCode)
        {
            string[] strArrOutput = { "", "" };

            //' Parameters Verification
            //If IsNothing(intNumberID)Then Throw New INCHEQSException.clsBusinessException("intNumberID cannot be NULL")
            //If IsNothing(bintTotalNumberNeed) Then Throw New INCHEQSException.clsBusinessException("bintTotalNumberNeed cannot be NULL")

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@intColumnID", intNumberID));
            sqlParameterNext.Add(new SqlParameter("@intTotalNumbers", bintTotalNumberNeed));
            sqlParameterNext.Add(new SqlParameter("@intNumberMaxLength", ReturnMaxLength((EnumSequenceCode)(intNumberID))));
            sqlParameterNext.Add(new SqlParameter("@BankCode", strBankCode));

            sqlParameterNext.Add(new SqlParameter("@bintNewStartNumber", 1));
            sqlParameterNext[4].Direction = ParameterDirection.Output;
            sqlParameterNext.Add(new SqlParameter("@bintNewEndNumber", 1));
            sqlParameterNext[5].Direction = ParameterDirection.Output;

            ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuNewStartEndNumberClearingBatch", sqlParameterNext.ToArray());
            
            //' get return value from store procedure
            Int64 bintStartNumber = 0;//Dim bintStartNumber As Int64 = 0
            Int64 bintEndNumber = 0;//Dim bintEndNumber As Int64 = 0

            bintStartNumber = Convert.ToInt64(sqlParameterNext[4].Value);
            bintEndNumber = Convert.ToInt64(sqlParameterNext[5].Value);

            strArrOutput[0] = Convert.ToString(bintStartNumber);//strArrOutput(0) = CType(bintStartNumber, String)
            strArrOutput[1] = Convert.ToString(bintEndNumber);//strArrOutput(1) = CType(bintEndNumber, String)

            return strArrOutput;
        }

        //'calling spc to add new clearing status
        public bool AddClearingStatus(SqlInt64 intDateBatch, string strClearingBatch, string strClearingAgent,
                    string strUI, string strOriginBRSTN, string strTransType, string strCurrency,
                    string strCurrentProcess, string dtProcessDateTime, string dtCompleteDateTime,
                    SqlInt32 intErrorCode, string strErrorMsg, string strCTCSclient, string strCTCSclientMsg,
                    SqlInt32 intCreateUserId, string dtCreateTimeStamp, SqlInt32 intUpdateUserId, string dtUpdateTimeStamp)
        {
            bool blnResult = false;//Dim blnResult As Boolean = False

            if (strClearingBatch.Length > 10) return false;//If strClearingBatch.Length > 10 Then
            if (strClearingAgent.Length > 30) return false;//If strClearingAgent.Length > 30 Then
            if (intDateBatch.IsNull) return false; //If intDateBatch.IsNull Then
            if (strUI.Length > 30) return false;//If strUI.Length > 30 Then
            if (intCreateUserId.IsNull) return false;//If intCreateUserId.IsNull Then

            //' Parameters Verification
            //If IsNothing(sequenceCode)Then Throw New INCHEQSException.clsBusinessException("sequenceCode cannot be NULL")
            //If IsNothing(bintTotalNumberNeed) Then Throw New INCHEQSException.clsBusinessException("bintTotalNumberNeed cannot be NULL")

            int intRowAffected;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("@clearingAgent", strClearingAgent));
            sqlParameterNext.Add(new SqlParameter("@clearingBatch", strClearingBatch));
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@currentProcess", strCurrentProcess));
            sqlParameterNext.Add(new SqlParameter("@processDateTime", dtProcessDateTime));
            sqlParameterNext.Add(new SqlParameter("@completeDateTime", dtCompleteDateTime));
            sqlParameterNext.Add(new SqlParameter("@errorCode", intErrorCode));
            sqlParameterNext.Add(new SqlParameter("@errorMsg", strErrorMsg));
            sqlParameterNext.Add(new SqlParameter("@CTCSclient", strCTCSclient));
            sqlParameterNext.Add(new SqlParameter("@CTCSclientMsg", strCTCSclientMsg));
            sqlParameterNext.Add(new SqlParameter("@createUserId", intCreateUserId));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", DateTime.Now.ToString()));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", intUpdateUserId));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", DateTime.Now.ToString()));

            intRowAffected = ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciClearingStatus", sqlParameterNext.ToArray());

            if (intRowAffected > 0) blnResult = true;

            return blnResult;
        }

        //'return current process code by current process enumeration
        public string GetCurrentProcessCode(EnumCurrentProcessCode CurrentProcess) {//Public Shared ReadOnly Property GetCurrentProcessCode(ByVal CurrentProcess As EnumCurrentProcessCode) As String
                                                                                    //    Get
            switch (CurrentProcess) {//        Select Case CurrentProcess
                case EnumCurrentProcessCode.NewCreated://            Case EnumCurrentProcessCode.NewCreated
                    return "NEW";//                Return "NEW"
                case EnumCurrentProcessCode.ImageCopyToAgent://            Case EnumCurrentProcessCode.ImageCopyToAgent
                    return "CPY";//                Return "CPY"
                case EnumCurrentProcessCode.ImageConversion://            Case EnumCurrentProcessCode.ImageConversion
                    return "IMG";//                Return "IMG"
                case EnumCurrentProcessCode.PKIEncryption://            Case EnumCurrentProcessCode.PKIEncryption
                    return "PKI";//                Return "PKI"
                case EnumCurrentProcessCode.FileCopyToGateway://            Case EnumCurrentProcessCode.FileCopyToGateway
                    return "GWC";//                Return "GWC"
                case EnumCurrentProcessCode.BOTProcess://            Case EnumCurrentProcessCode.BOTProcess
                    return "PRO";//                Return "PRO"
                case EnumCurrentProcessCode.CTMCompleted://            Case EnumCurrentProcessCode.CTMCompleted
                    return "CMP";//                Return "CMP"
                case EnumCurrentProcessCode.BOTError://            Case EnumCurrentProcessCode.BOTError
                    return "ERR";//                Return "ERR"
                case EnumCurrentProcessCode.Rebatched://            Case EnumCurrentProcessCode.Rebatched
                    return "REB";//                Return "REB"
                default://            Case Else
                    return "";//                Return ""
            }//        End Select
             //    End Get
        }//End Property

        //'calling spc to add new clearing outbox item
        //'/********** Param SelectItemByUI: Add/insert new outbox by selecting items from given Batch Number **************/ 
        public bool AddClearingOutbox(string strUI, int intMaxItemPerBatch, SqlInt64 intDateBatch, string dtClearingDate,
                            string strIgnoreIQA, SqlInt32 intLockUserId, SqlInt32 intCreateUserId, string dtCreateTimeStamp,
                            SqlInt32 intUpdateUserId, string dtUpdateTimeStamp)
        {
            bool blnResult = false;//Dim blnResult As Boolean = False

            if (intLockUserId.IsNull) {//If intLockUserId.IsNull Then
                                       //    RaiseEvent evtError(Me, "Lock user id cannot be null")
                return false;//    Exit Function
            }//End If
            if (intDateBatch.IsNull) {//If intDateBatch.IsNull Then
                                      //    RaiseEvent evtError(Me, "Date batch cannot be null")
                return false;//    Exit Function
            }//End If
            if (strUI.Length > 30) {//If strUI.Length > 30 Then
                                    //    RaiseEvent evtError(Me, "Clearing UIC cannot exceed 30 characters")
                return false;//    Exit Function
            }//End If
            if (intCreateUserId.IsNull) {//If intCreateUserId.IsNull Then
                                         //    RaiseEvent evtError(Me, "Create user id cannot be null")
                return false;//    Exit Function
            }//End If

            int intRowAffected;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@topCount", intMaxItemPerBatch));
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("clearingDate", dtClearingDate));
            sqlParameterNext.Add(new SqlParameter("@ignoreIQA", strIgnoreIQA));
            sqlParameterNext.Add(new SqlParameter("@lockUser", intLockUserId));
            sqlParameterNext.Add(new SqlParameter("@createUserId", intCreateUserId));
            sqlParameterNext.Add(new SqlParameter("@createTimeStamp", DateTime.Now.ToString()));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", intUpdateUserId));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", DateTime.Now.ToString()));

            intRowAffected = ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spciOutboxByItemLockUser", sqlParameterNext.ToArray());

            if (intRowAffected > 0) blnResult = true;

            return blnResult;
        }

        //(OVERLOADS for Complete status update) calling spc to update clearing status
        public bool UpdateClearingStatus_CompleteStatus(SqlInt64 intDateBatch, string strCurrentProcess, string dtCompleteDateTime, SqlInt32 intErrorCode, string strErrorMsg, SqlInt32 intUserId, string strTaskId, string strBankCode) {

            return UpdateClearingStatus(intDateBatch, strCurrentProcess, dtCompleteDateTime, dtCompleteDateTime, intErrorCode, strErrorMsg,
                                        "", "", "", "", "", "", "", "", intUserId, strTaskId, strBankCode);

        }

        //'calling spc to update clearing status
        public bool UpdateClearingStatus(SqlInt64 intDateBatch, string strCurrentProcess, string dtProcessDateTime, string dtCompleteDateTime,
                                        SqlInt32 intErrorCode, string strErrorMsg, string strCTCSclient, string strCTCSclientMsg,
                                        string strCTCSStatusCode, string strCTCSItemPosition, string strCTCSItemCount,
                                        string strCTCSImageCount, string strCTCSBatchAmount, string strCTCSStatusDesc,
                                        SqlInt32 intUserId, string strTaskId, string strBankCode) {

            bool blnResult = false;
            string strErrorMessage = "";

            DataTable ClearingBatchDataTable; DataRow objClearingRow; DateTime dtDateTime; //Dim ClearingBatchDataTable As New DataTable: Dim objClearingRow As DataRow = Nothing : Dim dtDateTime As DateTime = Nothing
            string strLogCurrentProcess;//Dim strLogCurrentProcess As String = ""

            if (intDateBatch.IsNull) {//If intDateBatch.IsNull Then
                                      //    RaiseEvent evtError(Me, "Date batch cannot be null")
                return false;//    Exit Function
            }//End If

            if (!string.IsNullOrEmpty(strCTCSclient)) {//If Not IsNothing(strCTCSclient) Then
                if (strCTCSclient.Length > 50) {//    If strCTCSclient.Length > 50 Then
                                                //        RaiseEvent evtError(Me, "CTCS client cannot exceed 50 characters")
                    return false;//        Exit Function
                }//    End If
            }//End If

            if (!string.IsNullOrEmpty(strErrorMsg)) {//If Not IsNothing(strErrorMsg) Then
                strErrorMsg = strErrorMsg.Replace(",","''");//    strErrorMsg = strErrorMsg.Replace("'", "''")
                if (strErrorMsg.Length > 1000) {//    If strErrorMsg.Length > 1000 Then
                    strErrorMsg = strErrorMsg.Substring(0, 1000) + "...";//        strErrorMsg = strErrorMsg.Substring(0, 1000) & "..."
                }//    End If
            }//End If

            if (!string.IsNullOrEmpty(strCTCSclientMsg)) {//If Not IsNothing(strCTCSclientMsg) Then
                strCTCSclientMsg = strCTCSclientMsg.Replace("'", "''");//    strCTCSclientMsg = strCTCSclientMsg.Replace("'", "''")
            }//End If

            if (!string.IsNullOrEmpty(strCTCSStatusDesc)) {//If Not IsNothing(strCTCSStatusDesc) Then
                strCTCSStatusDesc = strCTCSStatusDesc.Replace("'", "''");//    strCTCSStatusDesc = strCTCSStatusDesc.Replace("'", "''")
            }//End If

            ClearingBatchDataTable = GetClearingStatus("", intDateBatch, "", "", "", System.Data.SqlTypes.SqlInt32.Null, strBankCode);

            int intRowAffected;

            //DBAccessLayer.clsGenericDataAccess.AddParamSqlInt64(comm, "@dateBatch", intDateBatch)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "currentProcess", strCurrentProcess)
            //DBAccessLayer.clsGenericDataAccess.AddParamStrDateTime(comm, "processDateTime", dtProcessDateTime)
            //DBAccessLayer.clsGenericDataAccess.AddParamStrDateTime(comm, "completeDateTime", dtCompleteDateTime)
            //DBAccessLayer.clsGenericDataAccess.AddParamSqlInt32(comm, "@errorCode", intErrorCode)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@errorMsg", strErrorMsg)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSclient", strCTCSclient)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSclientMsg", strCTCSclientMsg)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSStatusCode", strCTCSStatusCode)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSItemPosition", strCTCSItemPosition)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSItemCount", strCTCSItemCount)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSImageCount", strCTCSImageCount)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSBatchAmount", strCTCSBatchAmount)
            //DBAccessLayer.clsGenericDataAccess.AddParamString(comm, "@CTCSStatusDesc", strCTCSStatusDesc)
            //DBAccessLayer.clsGenericDataAccess.AddParamSqlInt32(comm, "@updateUserId", intUserId)
            //DBAccessLayer.clsGenericDataAccess.AddParamStrDateTime(comm, "@updateTimeStamp", Now.ToString)

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("currentProcess", strCurrentProcess));
            sqlParameterNext.Add(new SqlParameter("processDateTime", dtProcessDateTime));
            sqlParameterNext.Add(new SqlParameter("completeDateTime", dtCompleteDateTime));
            sqlParameterNext.Add(new SqlParameter("@errorCode", intErrorCode));
            sqlParameterNext.Add(new SqlParameter("@errorMsg", strErrorMsg));
            sqlParameterNext.Add(new SqlParameter("@CTCSclient", strCTCSclient));
            sqlParameterNext.Add(new SqlParameter("@CTCSclientMsg", strCTCSclientMsg));
            sqlParameterNext.Add(new SqlParameter("@CTCSStatusCode", strCTCSStatusCode));
            sqlParameterNext.Add(new SqlParameter("@CTCSItemPosition", strCTCSItemPosition));
            sqlParameterNext.Add(new SqlParameter("@CTCSItemCount", strCTCSItemCount));
            sqlParameterNext.Add(new SqlParameter("@CTCSImageCount", strCTCSImageCount));
            sqlParameterNext.Add(new SqlParameter("@CTCSBatchAmount", strCTCSBatchAmount));
            sqlParameterNext.Add(new SqlParameter("@CTCSStatusDesc", strCTCSStatusDesc));
            sqlParameterNext.Add(new SqlParameter("@updateUserId", intUserId));
            sqlParameterNext.Add(new SqlParameter("@updateTimeStamp", DateTime.Now.ToString()));

            intRowAffected = ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuClearingStatus", sqlParameterNext.ToArray());

            if (intRowAffected > 0) blnResult = true;

            string strReferenceType = "I"; long intSystemLog = 0; int intLogReturnCode = 0;//Dim strReferenceType As String = "I" : Dim intSystemLog As Long = 0 : Dim intLogReturnCode As Integer = 0
            if (ClearingBatchDataTable != null) {//    If Not IsNothing(ClearingBatchDataTable) Then
                if (ClearingBatchDataTable.Rows.Count > 0) {//        If ClearingBatchDataTable.Rows.Count > 0 Then
                    strLogCurrentProcess = string.IsNullOrEmpty(strCurrentProcess) ? ClearingBatchDataTable.Rows[0]["fldCurrentProcess"].ToString() : strCurrentProcess;//            strLogCurrentProcess = IIf(IsNothing(strCurrentProcess), ClearingBatchDataTable.Rows(0).Item("fldCurrentProcess").ToString, strCurrentProcess).ToString
                }
                else {//        Else
                    strLogCurrentProcess = string.IsNullOrEmpty(strCurrentProcess) ? "" : strCurrentProcess;//            strLogCurrentProcess = IIf(IsNothing(strCurrentProcess), "", strCurrentProcess).ToString
                }//        End If
            } //    End If


            //    clsSystemLog.AddSystemLog(strTaskId, "Clearing status changed - current process code " & GetCurrentProcess(strLogCurrentProcess).ToString, _
            //        strReferenceType, intDateBatch.ToString, "", "", CType(intUserId, Int32), intSystemLog, intLogReturnCode)

            strReferenceType = "T";//    strReferenceType = "T"
                                   //    If intReturnCode = -1 Then
                                   //        clsErrorLog.AddErrorLog(intSystemLog, strReferenceType, strErrMessage, intDateBatch.ToString, CType(intUserId, Int32), intLogReturnCode)
                                   //    Else
                                   //if (ClearingBatchDataTable != null) {//        If Not IsNothing(ClearingBatchDataTable)Then
                                   //    if (ClearingBatchDataTable.Rows.Count > 0) {//            If ClearingBatchDataTable.Rows.Count > 0 Then
                                   //        objClearingRow = ClearingBatchDataTable.Rows[0];//                objClearingRow = ClearingBatchDataTable.Rows(0)
                                   ////                '---- log fldCurrentProcess ----'
                                   //        if (string.IsNullOrEmpty(strCurrentProcess)) {//                If Not IsNothing(strCurrentProcess) Then
                                   //            if (objClearingRow["fldCurrentProcess"].ToString() != strCurrentProcess) {//                    If objClearingRow.Item("fldCurrentProcess").ToString <> strCurrentProcess Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Current Process", _
                                   //                            objClearingRow.Item("fldCurrentProcess").ToString, strCurrentProcess.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If dtProcessDateTime <> "" And Not IsNothing(dtProcessDateTime) Then
                                   //                    If dtProcessDateTime.ToString <> New Date(1900, 1, 1).ToString Then
                                   //                        dtDateTime = CType(IIf(IsDBNull(objClearingRow.Item("fldProcessDateTime")), New Date(1900, 1, 1), objClearingRow.Item("fldProcessDateTime")), DateTime)
                                   //                        If DateTime.Compare(dtDateTime, DateTime.Parse(dtProcessDateTime)) <> 0 Then
                                   //                            clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Process DateTime", _
                                   //                                objClearingRow.Item("fldProcessDateTime").ToString, dtProcessDateTime.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                        End If
                                   //                    End If
                                   //                ElseIf Not IsDBNull(objClearingRow.Item("fldProcessDateTime")) Then
                                   //                    clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Process DateTime", _
                                   //                        objClearingRow.Item("fldProcessDateTime").ToString, "", CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                End If
                                   //                If dtCompleteDateTime <> "" And Not IsNothing(dtCompleteDateTime) Then
                                   //                    If dtCompleteDateTime.ToString <> New Date(1900, 1, 1).ToString Then
                                   //                        dtDateTime = CType(IIf(IsDBNull(objClearingRow.Item("fldCompleteDateTime")), New Date(1900, 1, 1), objClearingRow.Item("fldCompleteDateTime")), DateTime)
                                   //                        If DateTime.Compare(dtDateTime, DateTime.Parse(dtCompleteDateTime)) <> 0 Then
                                   //                            clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Complete DateTime", _
                                   //                                objClearingRow.Item("fldCompleteDateTime").ToString, dtCompleteDateTime.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                        End If
                                   //                    End If
                                   //                ElseIf Not IsDBNull(objClearingRow.Item("fldCompleteDateTime")) Then
                                   //                    clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Complete DateTime", _
                                   //                              objClearingRow.Item("fldCompleteDateTime").ToString, "", CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                End If
                                   //                If Not intErrorCode.IsNull Then
                                   //                    If CType(IIf(IsDBNull(objClearingRow.Item("fldErrorCode")), 0, objClearingRow.Item("fldErrorCode")), Integer) <> CType(IIf(IsNothing(intErrorCode), 0, intErrorCode), SqlInt32) Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Error Code", _
                                   //                            objClearingRow.Item("fldErrorCode").ToString, intErrorCode.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strErrorMsg) Then
                                   //                    If objClearingRow.Item("fldErrorMsg").ToString <> strErrorMsg.Replace("'", "''") Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "Error Message", _
                                   //                            objClearingRow.Item("fldErrorMsg").ToString, strErrorMsg.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSclient) Then
                                   //                    If objClearingRow.Item("fldCTCSclient").ToString <> strCTCSclient Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS client", _
                                   //                            objClearingRow.Item("fldCTCSclient").ToString, strCTCSclient.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSclientMsg) Then
                                   //                    If objClearingRow.Item("fldCTCSclientMsg").ToString <> strCTCSclientMsg Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS client Message", _
                                   //                            objClearingRow.Item("fldCTCSclientMsg").ToString, strCTCSclientMsg.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSStatusCode) Then
                                   //                    If objClearingRow.Item("fldCTCSStatusCode").ToString <> strCTCSStatusCode Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Status Code", _
                                   //                            objClearingRow.Item("fldCTCSStatusCode").ToString, strCTCSStatusCode.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSItemPosition) Then
                                   //                    If objClearingRow.Item("fldCTCSItemPosition").ToString <> strCTCSItemPosition Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Item Position", _
                                   //                            objClearingRow.Item("fldCTCSItemPosition").ToString, strCTCSItemPosition.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSItemCount) Then
                                   //                    If objClearingRow.Item("fldCTCSItemCount").ToString <> strCTCSItemCount Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Item Count", _
                                   //                            objClearingRow.Item("fldCTCSItemCount").ToString, strCTCSItemCount.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSImageCount) Then
                                   //                    If objClearingRow.Item("fldCTCSImageCount").ToString <> strCTCSImageCount Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Image Count", _
                                   //                            objClearingRow.Item("fldCTCSImageCount").ToString, strCTCSImageCount.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSBatchAmount) Then
                                   //                    If objClearingRow.Item("fldCTCSBatchAmount").ToString <> strCTCSBatchAmount Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Batch Amount", _
                                   //                            objClearingRow.Item("fldCTCSBatchAmount").ToString, strCTCSBatchAmount.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //                If Not IsNothing(strCTCSStatusDesc) Then
                                   //                    If objClearingRow.Item("fldCTCSStatusDesc").ToString <> strCTCSStatusDesc Then
                                   //                        clsDataImage.addDataImageLog(intSystemLog, strReferenceType, "CTCS Status Description", _
                                   //                            objClearingRow.Item("fldCTCSStatusDesc").ToString, strCTCSStatusDesc.ToString, CType(intUserId, Integer), intSystemLog, intLogReturnCode)
                                   //                    End If
                                   //                End If
                                   //            End If
                                   //        End If
                                   //    End If

            return blnResult;
        }

        public DataTable GetClearingStatus(string strClearingBatch, SqlInt64 intDateBatch, string strUI, string strClearingAgent, string strCurrentProcess,
                                            SqlInt32 intFilterCreateByUserId, string strBankCode, string strGWCClearingDate = "") {

            DataTable ClearingStatusDataTable;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@clearingBatch", strClearingBatch));
            sqlParameterNext.Add(new SqlParameter("@dateBatch", intDateBatch));
            sqlParameterNext.Add(new SqlParameter("@ui", strUI));
            sqlParameterNext.Add(new SqlParameter("@clearingAgent", strClearingAgent));
            sqlParameterNext.Add(new SqlParameter("@currentProcess", strCurrentProcess));
            sqlParameterNext.Add(new SqlParameter("@userId", intFilterCreateByUserId));
            sqlParameterNext.Add(new SqlParameter("@GWCClearingDate", strGWCClearingDate));
            sqlParameterNext.Add(new SqlParameter("@BankCode", strBankCode));

            ClearingStatusDataTable = ocsdbContext.GetRecordsAsDataTableSP("spcgClearingStatus", sqlParameterNext.ToArray());

            return ClearingStatusDataTable;
        }

        public bool UnlockItemForClearing(SqlInt32 intLockUserId) {
            bool blnResult = false;

            int intRowAffected;

            List<SqlParameter> sqlParameterNext = new List<SqlParameter>();
            sqlParameterNext.Add(new SqlParameter("@lockUser", intLockUserId));

            intRowAffected = ocsdbContext.ExecuteNonQuery(CommandType.StoredProcedure, "spcuItemUnlockForClearing", sqlParameterNext.ToArray());

            if (intRowAffected > 0) blnResult = true;

            return blnResult;
        }

        public String GetCurrentStatus(string itemID)
        {
            DataTable dt = new DataTable();
            string stmt = "SELECT fldclearingstatus from tbliteminfo " +
                                    "WHERE fldItemID = @fldItemID ";

            dt = dbContext.GetRecordsAsDataTable(stmt, new[] {
                new SqlParameter("@fldItemID", itemID)
            });
            string clearingStatus = dt.Rows[0]["fldclearingstatus"].ToString();
            return clearingStatus;


        }

    }
}