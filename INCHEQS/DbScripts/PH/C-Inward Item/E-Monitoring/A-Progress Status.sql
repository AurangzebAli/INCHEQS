
--------------------------------------
--VIEW
--------------------------------------

DROP VIEW [dbo].[View_ProgressStatus]
GO
CREATE VIEW [dbo].[View_ProgressStatus]
AS

WITH MasterTable AS (
select mas.fldClearDate,
band1.fldAmount AS Amount1, band1.fldApprovalStatus AS Status1, band1.fldNonConfirmStatus AS NonConfirm1,
band2.fldAmount AS Amount2, band2.fldApprovalStatus AS Status2, band2.fldNonConfirmStatus AS NonConfirm2
FROM View_AppInwardItem mas
LEFT OUTER JOIN View_Band1 band1 on mas.fldInwardItemId = band1.fldInwardItemId
LEFT OUTER JOIN View_Band2 band2 on mas.fldInwardItemId = band2.fldInwardItemId
)

SELECT
fldClearDate,
(SELECT tm.fldMenuTitle +', '+ STR(bl.fldBandLBound,12,2) +' < '+ STR(bl.fldBandUBound,12,2) FROM tblTaskMaster tm LEFT JOIN tblTaskBandLimit bl ON tm.fldTaskId = bl.fldTaskId WHERE tm.fldTaskId = '306310') AS QueueName,
sum(case WHEN Status1 = 'A' THEN 1 ELSE 0 END) as Approved,
sum(case WHEN Status1 = 'R' THEN 1 ELSE 0 END) as Returned,
(SELECT Count(fldInwardItemId) FROM View_Band1 WHERE fldApprovalStatus IN ('A','R')) as TotalCompleted,
sum(case WHEN Status1 = 'A' OR Status1 = 'R' THEN 1 ELSE 0 END) as ReadyToHost,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1)) as RejectReentryMakerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1)) as RejectReentryCheckerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_Verification1st WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1)) as FirstVerificationOutstanding,
sum(case WHEN NonConfirm1 = 'A' THEN 1 ELSE 0 END) as SecondVerificationOutstanding,
sum(case WHEN NonConfirm1 = 'C' THEN 1 ELSE 0 END) as ThirdVerificationOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1)) as BranchMakerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1) AND fldApprovalStatusPending = 'H') as BranchCheckerApproveOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1) AND fldApprovalStatusPending = 'J') as BranchCheckerReturnOutstanding,
(SELECT Count(fldInwardItemId)+(SELECT Count(fldInwardItemId) FROM View_AppPendingDataMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band1)) FROM View_Band1 where fldApprovalStatus is null) as TotalItem
FROM MasterTable
group by fldClearDate


UNION SELECT
fldClearDate,
(SELECT tm.fldMenuTitle +', '+ STR(bl.fldBandLBound,12,2) +' < '+ STR(bl.fldBandUBound,12,2) FROM tblTaskMaster tm LEFT JOIN tblTaskBandLimit bl ON tm.fldTaskId = bl.fldTaskId WHERE tm.fldTaskId = '306320') AS QueueName,
sum(case WHEN Status2 = 'A' THEN 1 ELSE 0 END) as Approved,
sum(case WHEN Status2 = 'R' THEN 1 ELSE 0 END) as Returned,
(SELECT Count(fldInwardItemId) FROM View_Band2 WHERE fldApprovalStatus IN ('A','R')) as TotalCompleted,
sum(case WHEN Status2 = 'A' OR Status2 = 'R' THEN 1 ELSE 0 END) as ReadyToHost,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2)) as RejectReentryMakerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2)) as RejectReentryCheckerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_Verification1st WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2)) as FirstVerificationOutstanding,
sum(case WHEN NonConfirm2 = 'A' THEN 1 ELSE 0 END) as SecondVerificationOutstanding,
sum(case WHEN NonConfirm1 = 'C' THEN 1 ELSE 0 END) as ThirdVerificationOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2)) as BranchMakerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2) AND fldApprovalStatusPending = 'H') as BranchCheckerApproveOutstanding,
(SELECT Count(fldInwardItemId) FROM View_AppPendingDataChecker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2) AND fldApprovalStatusPending = 'J') as BranchCheckerReturnOutstanding,
(SELECT Count(fldInwardItemId)+(SELECT Count(fldInwardItemId) FROM View_AppPendingDataMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId from View_Band2)) FROM View_Band2 where fldApprovalStatus is null) as TotalItem
FROM MasterTable
group by fldClearDate



GO



--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'MONITORING';
DECLARE @menuTitle varchar(100) = 'Progress Status';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - Progress Status';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 23;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_ProgressStatus';

DECLARE @parentTaskId varchar(100) = '303000';
DECLARE @taskId varchar(100) = '306360';
--------------------------------------
--TASK MASTER (INWARD CLEARING - PROGRESS STATUS)
--------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/ProgressStatus')


----------------------------------------
--SEARCH PAGE CONFIG
----------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'Y', N'N', N'Y', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'BranchCheckerReturnOutstanding', NULL, NULL, N'', N'BranchCheckerReturnOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 13, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'QueueName', NULL, NULL, N'', N'Description', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'Approved', NULL, NULL, N'', N'Approved', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 3, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'Returned', NULL, NULL, N'', N'Returned', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 4, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'TotalCompleted', NULL, NULL, N'', N'TotalCompleted', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 5, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'ReadyToHost', NULL, NULL, N'', N'ReadyToHost', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 6, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'RejectReentryMakerOutstanding', NULL, NULL, N'', N'RejectReentryMakerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 7, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'RejectReentryCheckerOutstanding', NULL, NULL, N'', N'RejectReentryCheckerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 8, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'FirstVerificationOutstanding', NULL, NULL, N'', N'FirstVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 9, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'TotalItem', NULL, NULL, N'', N'TotalItem', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 14, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'SecondVerificationOutstanding', NULL, NULL, N'', N'SecondVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 10, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'ThirdVerificationOutstanding', NULL, NULL, N'', N'ThirdVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 10, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'BranchMakerOutstanding', NULL, NULL, N'', N'BranchMakerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 11, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'BranchCheckerApproveOutstanding', NULL, NULL, N'', N'BranchCheckerApproveOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 12, @bank, @bank)


GO
