--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_ThresholdSetting]
Go
CREATE VIEW [dbo].[View_ThresholdSetting]
AS
SELECT	*,fldId as fldIdforDelete,
(CASE	when fldType = 'A' then 'Approve'
		when fldType = 'R' then 'Reject'
END)as thresholdType,
(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_ThresholdSetting') GROUP BY ReportTitle) AS ReportTitle
FROM	tblThresholdSetting
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'VERIFICATION';
DECLARE @menuTitle varchar(100) = 'Threshold Setting';
DECLARE @taskDesc varchar(100) = 'Maintenance - Threshold Setting';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Threshold Setting (Edit)';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Threshold Setting (New)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Threshold Setting (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 27;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_ThresholdSetting';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102680';
DECLARE @edit varchar(100) = '102682';
DECLARE @new varchar(100) = '102681';
DECLARE @delete varchar(100) = '102683';
------------------------------------------------------------------------
-- Threshold Setting
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ThresholdSetting')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @taskId, NULL, N'B', N'', N'E', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescEdit, N'', N' ', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, @taskId, NULL, N'B', N'', N'N', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescNew, N'', N' ', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescDelete, N'', N' ', N'', N'', 0, 0, N'', N'', N'', 1, NULL)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldId', NULL, NULL, N'TextBox', N'Id', NULL, N'', @taskDesc , 999, N'Y', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'thresholdType', NULL, NULL, N'TextBox', N'Type', NULL, N'', @taskDesc , 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTitle', NULL, NULL, N'TextBox', N'Title', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAmount', NULL, NULL, N'TextBox', N'Amount', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldIdforDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO