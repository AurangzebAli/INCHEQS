
--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_RptLargeAmount]
GO
CREATE VIEW [dbo].[View_RptLargeAmount]
AS
SELECT        ci.fldInwardItemId, ci.fldUIC, ci.fldChequeSerialNo, ci.fldPreBankType, ci.fldPreBankCode, ci.fldPreStateCode, ci.fldPreBranchCode, ci.fldIssueBankType, ci.fldIssueBankCode, ci.fldIssueStateCode, 
                         ci.fldIssueBranchCode, ci.fldAccountNumber, ci.fldTransCode, ci.fldAmount, ci.fldImageFolder, ci.fldImageFileName, ci.fldClearDate, ci.fldSpickCode, ci.fldNonConformance, ci.fldNonConformance2, ci.fldIQA, 
                         ci.fldDocToFollow, ci.fldChequeType, ci.fldCreateUserID, ci.fldCreateTimeStamp, ci.fldCurrency, ci.fldCheckDigit, ci.fldDSStatus, ci.fldImageDS, ci.fldMICRDS, ci.fldDBatchID, ci.fldCBatchID, ci.fldPreDigit, 
                         ci.fldIssueDigit, ci.fldImageIndicator, ci.fldDIN, ci.fldUpdateUserID, ci.fldUpdateTimeStamp, ci.fldFromDP, ci.fldPrePayeeAccNo, ci.fldOriCheckDigit, ci.fldOriChequeSerialNo, ci.fldOriIssueBankCode, 
                         ci.fldOriIssueStateCode, ci.fldOriIssueBranchCode, ci.fldOriIssueDigit, ci.fldOriTransCode, ci.fldOriAccountNumber, ci.fldCICS, ci.fldPresentingBankItemSequenceNumber, ci.fldHostAccountNo, 
                         ci.fldAccountHolderName, ci.fldNonConfirmUserID, ci.fldNonConfirmUserClass, ci.fldNonConfirmTimeStamp, ci.fldModifiedFields, ci.fldMatchResult, ci.fldApprovalStatus, ci.fldApprovalUserId, 
                         ci.fldApprovalUserClass, ci.fldApprovalTimeStamp, ci.fldApprovalIndicator, ci.fldAssignedUserId, ci.fldCharges, ci.fldRejectStatus1, ci.fldRejectStatus2, ci.fldRejectStatus3, ci.fldRejectStatus4, ci.fldRejectCode, 
                         ci.fldRemarks, ci.fldUPIGenerated, ci.fldUPIDate, ci.fldHostDebit, ci.fldOldAccountNo, ci.fldOldChequeNo, ci.fldCustomerConfirm, ci.fldNonConfirmStatus, ci.fldRRstatus, ci.fldCompleted, ci.fldMatchStatus, 
                         ci.fldBatchNo, ci.fldBranchODReview, ci.fldODReview, ci.fldUpdateUserIDStatus, ci.fldUpdateTimeStampStatus, ci.fldMTFId, ci.fldAccountStatus, ci.fldHostFileName, ci.fldOldDigit, ci.fldOldAmount, 
                         ci.fldOldCheckDigit, ci.fldOldBranchCode, ci.fldOldStateCode, ci.fldOldBankCode, ci.fldOldTransCode, ci.fldSourceFile, ci.fldPrePayeeAccNoApproved, ci.fldFICMICExclude, ci.fldFICMICStatus, ci.fldORstatus, 
                         ci.fldQueueType, ci.fldOutwardBankUserID, ci.fldORAssignedUserId, ci.fldOutwardBankAcc, ci.fldAssignedQueue, ci.fldReviewAll, ci.fldUPIType, ci.fldAutoRejectRemarks, ci.fldUnMatchRemarks, 
                         ci.fldRejectStatus5, ci.fldRejectStatus6, ci.fldRejectStatus7, ci.fldSOAPrinted, ci.fldBranchCode, ci.fldPreBankDesc, ci.fldTransDesc, ci.fldUserAbb, ci.hoststatus, ci.fldItemStatus, ci.fldRejectDesc, 
                         ci.fldLastUpdateUserName, ci.fldLastUpdateUserId, tm.fldTransCode AS Expr1, tm.fldTransDesc AS Expr2, tm.fldCreateUserId AS Expr3, tm.fldCreateTimeStamp AS Expr4, tm.fldUpdateUserId AS Expr5, 
                         tm.fldUpdateTimeStamp AS Expr6, bm.fldBranchId, bm.fldBranchCode AS Expr7, bm.fldBranchDesc, bm.fldBankCode, bm.fldAccType, bm.fldStateCode, bm.fldSpickCode AS Expr8, bm.fldDisable, 
                         bm.fldCreateUserId AS Expr9, bm.fldCreateTimeStamp AS Expr10, bm.fldUpdateUserId AS Expr11, bm.fldUpdateTimeStamp AS Expr12, bm.fldTel
FROM            dbo.View_AppInwardItem AS ci LEFT OUTER JOIN
                         dbo.tblTransMaster AS tm ON ci.fldTransCode = tm.fldTransCode LEFT OUTER JOIN
                         dbo.tblBranchMaster AS bm ON ci.fldIssueBankType = bm.fldAccType AND bm.fldBranchCode = ci.fldIssueBranchCode AND bm.fldStateCode = ci.fldIssueStateCode AND bm.fldBankCode = ci.fldIssueBankCode

GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Reports';
DECLARE @subMenu varchar(100) = 'START OF DAY';
DECLARE @menuTitle varchar(100) = 'Large Amount Data';
DECLARE @taskDesc varchar(100) = 'Report - Large Amount Data';
DECLARE @mainMenuOrder int = 7;
DECLARE @subMenuOrder int = 63;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_RptLargeAmount';
DECLARE @reportPath varchar(100) = '~//Reports/ICS/LargeAmountReport.rdlc';
DECLARE @reportTitle varchar(100) = 'Large Amount';
DECLARE @fileName varchar(100) = 'LargeAmountRpt';
DECLARE @reportParam varchar(100) = '';
DECLARE @orientation varchar(100) = 'Landscape';

DECLARE @parentTaskId varchar(100) = '304000';
DECLARE @taskId varchar(100) = '304250';
--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/DefaultReport?tId='+@taskId)

--------------------------------------------------------------
--report config
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] where [TaskId]=@taskId
INSERT INTO [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [ReportPath], [ReportTitle], [DataSetName], [exportFileName],[PrintReportParam], [Orientation],  [IsEnabled], [Ordering]) VALUES ((select top 1 TaskId from tblReportPageConfig order by TaskId desc) + 1, @taskId, @view, @reportPath, @reportTitle, @view, @fileName, @reportParam, @orientation, N'Y', 1)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where [TaskId]=@taskId
INSERT INTO [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'DateFromTo', N'Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL, @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 1, @bank, @bank)


GO




