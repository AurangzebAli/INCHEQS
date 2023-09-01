--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_AppPendingDataMaker]
GO
CREATE VIEW [dbo].[View_AppPendingDataMaker]
AS
SELECT	*
FROM	dbo.View_AppPendingData
WHERE	fldApprovalStatusPending = 'B' OR fldApprovalStatusPending = 'K'
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Branch';
DECLARE @subMenu varchar(100) = 'BRANCH';
DECLARE @menuTitle varchar(100) = 'Pending Data Maker';
DECLARE @taskDesc varchar(100) = 'Branch - Pending Data Maker';
DECLARE @mainMenuOrder int = 2;
DECLARE @subMenuOrder int = 1;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_AppPendingDataMaker';
DECLARE @taskRole varchar(100) = 'Maker';
DECLARE @queueOrderBy varchar(100) = 'fldAccountDesc DESC, fldAmount DESC';
DECLARE @userParam varchar(100) = 'fldBranchCode IN (@currentUserBranchCodes)';
DECLARE @lockCondition varchar(100) = '';

DECLARE @parentTaskId varchar(100) = '308000';
DECLARE @taskId varchar(100) = '308110';
DECLARE @lock varchar(100) = '308111';
DECLARE @action varchar(100) = '308112';
DECLARE @approve varchar(100) = '308113';
DECLARE @return varchar(100) = '308114';
DECLARE @printAll varchar(100) = '308115';
DECLARE @printSum varchar(100) = '308116';
--------------------------------------------------------------
--Add to tblQueueConfig
--------------------------------------------------------------
DELETE FROM [tblQueueConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblQueueConfig] ([fldId], [fldTaskId], [fldQueueName], [fldQueueDesc], [fldViewName], [fldOrderBy], [fldUserParamCondition], [fldAllowedAction], [fldTaskRole], [fldLockCondition], [fldEnable], [fldEntityCode], [fldBankCode]) VALUES (14, @taskId, @menuTitle, @taskDesc, @view, @queueOrderBy, @userParam, @taskId+N','+@lock+N','+@action+N','+@approve+N','+@return+N','+@printAll+N','+@printSum , @taskRole, @lockCondition, N'Y', @bank, @bank)

--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/Branch?tId='+@taskId)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @lock
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @lock, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 1, 2, N'', N'', @taskDesc+N' (Lock Verification)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'LockVerification')


DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @action
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @action, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 1, 2, N'', N'', @taskDesc+N' (Verification Action)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'VerificationAction')


DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @approve
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @approve, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 1, 2, N'', N'', @taskDesc+N' (Branch Aprove)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'BranchApprove')


DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @return
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @return, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 1, 2, N'', N'', @taskDesc+N' (Branch Return)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'BranchReturn')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @printAll
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @printAll, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'',  @taskDesc+N' (Print All)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'PrintAll')


DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @printSum
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @printSum, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'',  @taskDesc+N' (Print Summary)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'PrintSum')


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAmount', NULL, NULL, N'Currency', N'Amount', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 9, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldLastUpdateUserNamePending', NULL, NULL, N'', N'Last Verify By', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 11, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldItemStatusPending', NULL, NULL, N'', N'Item Status', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 10, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldUIC', NULL, NULL, N'TextBox', N' UIN Number', N'', NULL, @taskDesc, 10, N'N', N'N', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N' Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL, @taskDesc, 10, N'N', N'Y', N'Y', N'Y', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAccountNumber', NULL, NULL, N'NumberBox', N' Account Number', NULL, NULL, @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldChequeSerialNo', NULL, NULL, N'NumberBox', N'Cheque Number', NULL, NULL, @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 3, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTransCode', NULL, NULL, N'SelectListWithAll', N'Trans Code', NULL, N'SELECT fldTransCode as "value", fldTransCode as "text" FROM tblTransMaster', @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 5, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'hostStatus', NULL, NULL, N'SelectListWithAll', N'Inward Status', NULL, N'SELECT 1 as "value" , ''w/ Host Status'' as "text" union select 2 as "value" , ''w/o Host Status'' as "text"', @taskDesc, 10, N'N', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldInwardItemId', NULL, NULL, N'', N'Inward Item ID', NULL, N'SELECT 1 as "value" , ''w/ Host Status'' as "text"', @taskDesc, 10, N'Y', N'N', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldrejectstatus1', NULL, NULL, N'SelectListWithAll', N' Host Reject Status', N'', N'SELECT fldStatusId as "value", fldStatusId as "text" FROM tblBankHostStatusMaster', @taskDesc, 10, N'N', N'Y', N'Y', N'Y', N'Y', 8, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectCodePending', NULL, NULL, N'', N'Return Reason', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 6, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldMinAmount', N'fldAmount >= @@param', NULL, N'NumberBox', N'Min Amount', NULL, N'', @taskDesc, 999, N'N', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldMaxAmount', N'fldAmount <= @@param', NULL, N'NumberBox', N'Max Amount', NULL, N'', @taskDesc, 999, N'N', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)


GO