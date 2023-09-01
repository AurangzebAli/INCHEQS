--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_LoadHostHREJ]
GO
CREATE VIEW [dbo].[View_LoadHostHREJ]
AS
SELECT *,fldFileName AS fldFileNameForLoad
FROM tblFileManager where fldTaskId = '309121'
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'HOST FILE';
DECLARE @menuTitle varchar(100) = 'Load Bank Host Reject File';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - Load Host File (HREJ)';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 20;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_LoadHostHREJ';

DECLARE @systemProfileCode varchar(100) = 'HostFileHREJ';
DECLARE @fileExt varchar(100) = '.MTF';
DECLARE @processName varchar(100) = 'ICSLoadHostFile_Details';
DECLARE @posPayType varchar(100) = '';
DECLARE @FTPFolder varchar(100) = 'HostFile/';
DECLARE @taskRole varchar(100) = '';

DECLARE @parentTaskId varchar(100) = '309000';
DECLARE @taskId varchar(100) = '309121';

--------------------------------------------------------------
--Add to [tblHostFileConfig]
--------------------------------------------------------------
DELETE FROM [tblHostFileConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblHostFileConfig] ([fldTaskId], [fldSystemProfileCode],[fldFileExt], [fldHostFileDesc], [fldProcessName], [fldPosPayType],[fldTaskRole],[fldFTPFolder], [fldEnable],[fldEntityCode],[fldBankCode]) VALUES (@taskId, @systemProfileCode, @fileExt, @taskDesc, @processName, @posPayType,@taskRole,@FTPFolder, N'Y',@bank, @bank)

--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/LoadHostFile?tId='+@taskId)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldFileNameForLoad', NULL, NULL, N'RadioButton', N'Load File', N'', N'', @taskDesc, 0, N'N', N'N', N'Y', N'N', N'Y', 0, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldFilePath', NULL, NULL, N'', N'Folder Path', NULL, N'',  @taskDesc, 10, N'Y', N'N', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', N'', @taskDesc, 0, N'*', N'Y', N'N', N'Y', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldFileName', NULL, NULL, N'', N'File  Name', NULL, N'', @taskDesc, 10, N'Y', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldLoad', NULL, NULL, N'', N'File Loaded', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldCreateTimestamp', NULL, NULL, N'', N'Last Modified', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldSend', NULL, NULL, N'', N'FTP Status', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRemarks', NULL, NULL, N'', N'Remarks', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)
GO

