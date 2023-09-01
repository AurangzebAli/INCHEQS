--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_BranchCode]
Go
CREATE VIEW [dbo].[View_BranchCode]
AS
SELECT	*,fldBranchId as fldBranchIdForDelete,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_BranchCode') GROUP BY ReportTitle) AS ReportTitle
FROM	tblbranchmaster
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'BANK';
DECLARE @menuTitle varchar(100) = 'Branch Code';
DECLARE @taskDesc varchar(100) = 'Maintenance - Branch Code';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Branch Code (New)';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Branch Code (Edit)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Branch Code (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 21;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_BranchCode';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102340';
DECLARE @new varchar(100) = '102341';
DECLARE @edit varchar(100) = '102342';
DECLARE @delete varchar(100) = '102343';

------------------------------------------------------------------------
-- BranchCode
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/BranchCode')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, @taskId, NULL, N'B', N'', N'N', NULL, N'N', N'', N'', N'', N' ', 0, 0, N'', N' ', @taskDescNew, N'', N'N', N'', N'', 0, 0, N'', N' ', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @taskId, NULL, N'B', N'', N'E', NULL, N'N', N'', N'', N'', N' ', 0, 0, N'', N' ', @taskDescEdit, N'', N'N', N'', N'', 0, 0, N'', N' ', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N' ', 0, 0, N'', N' ', @taskDescDelete, N'', N'N', N'', N'', 0, 0, N'', N' ', N'', 1, NULL)



--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]= @taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES 
(N'10', @taskId, @view, N'BranchCodeDataset' ,N'~/Reports/ICS/BranchCodePrint.rdlc', N'Branch Code List',  N'BranchCodeList',N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES 
(N'10', @taskId, @view, N'BranchCodeDataset' ,N'~/Reports/ICS/BranchCodePrint.rdlc', N'Branch Code List',  N'BranchCodeList',N'page',N'Y', 1)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchId', NULL, NULL, N'TextBox', N'Branch Id', NULL, N'', @taskDesc, 999, N'Y', N'N', N'N', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldStateCode', NULL, NULL, N'TextBox', N'State Code', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchCode', NULL, NULL, N'TextBox', N'Branch Code', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchDesc', NULL, NULL, N'TextBox', N'Branch Description', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchIdForDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO