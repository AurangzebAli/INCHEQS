--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_Archive]
GO
CREATE VIEW [dbo].[View_Archive]
AS
Select *, CONVERT(CHAR(10),fldCreateTimestamp,110) + SUBSTRING(CONVERT(varchar,fldCreateTimestamp,0),12,8) as lastDate, 
isnull(fldRemarks,'') as Remarks, isnull(fldfilepath,'') as Filepath from tblFileManager WHERE fldfilename = 'archive'
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Utilities';
DECLARE @subMenu varchar(100) = '';
DECLARE @menuTitle varchar(100) = 'Import Archive File';
DECLARE @taskDesc varchar(100) = 'Utilities - Import Archive File';
DECLARE @mainMenuOrder int = 8;
DECLARE @subMenuOrder int = 10;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_Archive'; 

DECLARE @parentTaskId varchar(100) = '205000';
DECLARE @taskId varchar(100) = '205400';
DECLARE @mvcUrl varchar(100) = 'Archive';

------------------------------------------------------------------------
-- TASK MENU
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/'+@mvcUrl)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldfilename', NULL, NULL, N'TextBox', N'File Name', NULL, N'', N'Archive', 10, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'lastDate', NULL, NULL, N'Date', N'Cheque Clear Date', NULL, N'', N'', 10, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'Remarks', NULL, NULL, N'TextBox', N'Remarks', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO
