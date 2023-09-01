
--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_ReturnAll]
GO
CREATE VIEW [dbo].[View_ReturnAll]
AS
SELECT DISTINCT * FROM dbo.View_Verification WHERE fldApprovalStatus IS NULL
GO


--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'VERIFICATION';
DECLARE @menuTitle varchar(100) = 'Return All';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - Return All';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 18;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_ReturnAll';
DECLARE @taskRole varchar(100) = 'ReturnAll';
DECLARE @queueOrderBy varchar(100) = 'fldAccountDesc DESC, fldAmount DESC';
DECLARE @userParam varchar(100) = '';
DECLARE @lockCondition varchar(100) = '';

DECLARE @parentTaskId varchar(100) = '306000';
DECLARE @taskId varchar(100) = '306710';
DECLARE @approveAll varchar(100) = '306711';
DECLARE @printAll varchar(100) = '306712';
DECLARE @printSum varchar(100) = '306713';
--------------------------------------------------------------
--Add to [tblReportPageConfig] FOR Print
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId] = @taskId
INSERT INTO [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [ReportPath], [ReportTitle], [DataSetName], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES (N'3', @taskId, @view, N'~//Reports/ICS/ExtractedInwardData.rdlc', @taskDesc, N'ExtractedInwardDataSet', N'ExtractedInwardRpt', N'all', N'Y', 1)

INSERT INTO [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [ReportPath], [ReportTitle], [DataSetName], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES (N'3', @taskId, @view, N'~//Reports/ICS/ExtractedInwardDataSummary.rdlc', @taskDesc, N'ExtractedInwardDataSet', N'ExtractedInwardDataSummaryRpt', N'sum', N'Y', 1)

--------------------------------------------------------------
--Add to tblQueueConfig
--------------------------------------------------------------
DELETE FROM [tblQueueConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblQueueConfig] ([fldId], [fldTaskId], [fldQueueName], [fldQueueDesc], [fldViewName], [fldOrderBy], [fldUserParamCondition], [fldAllowedAction], [fldTaskRole], [fldLockCondition], [fldEnable], [fldEntityCode], [fldBankCode]) VALUES (1, @taskId, @menuTitle, @taskDesc, @view, @queueOrderBy, @userParam ,@taskId+N','+@approveAll+N','+@printAll+N','+@printSum , @taskRole ,@lockCondition , N'Y', @bank , @bank)


--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/Verification?tId='+@taskId)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @approveAll
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) 
VALUES (N'H', @approveAll, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Approve All)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'ReturnAll')

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

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldInwardItemId', NULL, NULL, N'', N'Inward Item ID', NULL, N'',  @taskDesc, 10, N'Y', N'N', N'N', N'N', N'Y', 0, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N' Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL,  @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldUIC', NULL, NULL, N'TextBox', N' UIN Number', N'', NULL,  @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAccountNumber', NULL, NULL, N'NumberBox', N' Account Number', NULL, NULL,  @taskDesc, 12, N'N', N'Y', N'Y', N'N', N'Y', 3, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldChequeSerialNo', NULL, NULL, N'NumberBox', N'Cheque Number', NULL, NULL,  @taskDesc, 12, N'N', N'Y', N'Y', N'N', N'Y', 4, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTransCode', NULL, NULL, N'SelectListWithAll', N'Trans Code', NULL, N'SELECT fldTransCode as "value", fldTransCode as "text" FROM tblTransMaster',  @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 5, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldMinAmount', N'fldAmount >= @@param', NULL, N'NumberBox', N'Min Amount', NULL, N'',  @taskDesc, 999, N'N', N'Y', N'N', N'N', N'Y', 5, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldMaxAmount', N'fldAmount <= @@param', NULL, N'NumberBox', N'Max Amount', NULL, N'',  @taskDesc, 999, N'N', N'Y', N'N', N'N', N'Y', 6, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectCode', NULL, NULL, N'', N'Return Reason', NULL, N'',  @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 7, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'hostStatus', NULL, NULL, N'SelectListWithAll', N'Host Status', NULL, N'SELECT 1 as "value" , ''w/ Host Status'' as "text" union select 2 as "value" , ''w/o Host Status'' as "text"',  @taskDesc, 10, N'N', N'Y', N'N', N'N', N'Y', 8, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldrejectstatus1', NULL, NULL, N'SelectListWithAll', N' Host Reject Status', N'', N'SELECT fldStatusId as "value", fldStatusId as "text" FROM tblBankHostStatusMaster', @taskDesc, 10, N'N', N'Y', N'Y', N'Y', N'Y', 9, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAmount', NULL, NULL, N'Currency', N'Amount', NULL, N'',  @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 10, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldItemStatus', NULL, NULL, N'', N'Item Status', NULL, N'',  @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 11, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldLastUpdateUserName', NULL, NULL, N'', N'Last Verify By', NULL, N'',  @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', 12, @bank, @bank)





GO