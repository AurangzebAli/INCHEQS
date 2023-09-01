--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_InternalBranch]
Go
CREATE VIEW [dbo].[View_InternalBranch]
AS
SELECT	*,fldConBranchCode as fldInternalBranchIdForDelete,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_InternalBranch') GROUP BY ReportTitle) AS ReportTitle
FROM	tblMapBranch
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'BANK';
DECLARE @menuTitle varchar(100) = 'Internal Branch';
DECLARE @taskDesc varchar(100) = 'Maintenance - Internal Branch';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Internal Branch (New)';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Internal Branch (Edit)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Internal Branch (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 25;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_InternalBranch';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102690';
DECLARE @new varchar(100) = '102691';
DECLARE @edit varchar(100) = '102692';
DECLARE @delete varchar(100) = '102693';

------------------------------------------------------------------------
-- InternalBranch
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/InternalBranch')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, 102690, NULL, N'B', N'', N'N', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescNew, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, 102690, NULL, N'B', N'', N'E', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescEdit, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, 102690, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescDelete, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]= @taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'10', @taskId, @view, N'InternalBranchDataset' ,N'~/Reports/ICS/InternalBranchPrint.rdlc', N'Internal Branch List',  N'InternalBranchList',N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'10', @taskId, @view, N'InternalBranchDataset' ,N'~/Reports/ICS/InternalBranchPrint.rdlc', N'Internal Branch List',  N'InternalBranchList',N'page',N'Y', 1)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldConBranchCode', NULL, NULL, N'TextBox', N'Branch Code', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchDesc', NULL, NULL, N'TextBox', N'Branch Description', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldInternalBranchIdForDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO