--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_PullOutReason]
Go
CREATE VIEW [dbo].[View_PullOutReason]
AS
SELECT	*,fldPullOutId as fldPullOutIdForDelete,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_PullOutReason') GROUP BY ReportTitle) AS ReportTitle
FROM	tblPullOutReason
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'VERIFICATION';
DECLARE @menuTitle varchar(100) = 'Pull Out Reason';
DECLARE @taskDesc varchar(100) = 'Maintenance - Pull Out Reason';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Pull Out Reason (New)';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Pull Out Reason (Edit)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Pull Out Reason (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 26;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_PullOutReason';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102390';
DECLARE @new varchar(100) = '102391';
DECLARE @edit varchar(100) = '102392';
DECLARE @delete varchar(100) = '102393';

------------------------------------------------------------------------
-- PullOutReason
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/PullOutReason')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, @taskId, NULL, N'B', N'', N'N', NULL, N'N', N' ', N' ', N' ', N' ', 0, 0, N' ', N' ', @taskDescNew, N' ', N'N', N'', N'', 0, 0, N' ', N' ', N' ', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @taskId, NULL, N'B', N'', N'E', NULL, N'N', N' ', N' ', N' ', N' ', 0, 0, N' ', N' ', @taskDescEdit, N' ', N'N', N'', N'', 0, 0, N' ', N' ', N' ', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N' ', N' ', N' ', N' ', 0, 0, N' ', N' ', @taskDescDelete, N' ', N'N', N'', N'', 0, 0, N' ', N' ', N' ', 1, NULL)

--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]= @taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'10', @taskId , @view, N'PullOutReasonDataset' ,N'~/Reports/ICS/PullOutReasonPrint.rdlc', N'Pull Out Reason List',  N'PullOutReasonList', N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'10', @taskId , @view, N'PullOutReasonDataset' ,N'~/Reports/ICS/PullOutReasonPrint.rdlc', N'Pull Out Reason List',  N'PullOutReasonList', N'page',N'Y', 1)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldPullOutId', NULL, NULL, N'TextBox', N'Pull Out Id', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldPullOutReason', NULL, NULL, N'TextBox', N'Pull Out Reason', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldPullOutIdForDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO