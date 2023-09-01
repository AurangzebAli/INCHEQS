
--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'VERIFICATION';
DECLARE @menuTitle varchar(100) = 'SOA Service';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - SOA Service';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 19;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_AppInwardItem';

DECLARE @systemProfileCode varchar(100) = '';
DECLARE @fileExt varchar(100) = '';
DECLARE @processName varchar(100) = 'ICSSOAServices';
DECLARE @posPayType varchar(100) = '';
DECLARE @FTPFolder varchar(100) = '';
DECLARE @taskRole varchar(100) = '';

DECLARE @parentTaskId varchar(100) = '309000';
DECLARE @taskId varchar(100) = '309510';

--------------------------------------------------------------
--Add to [tblHostFileConfig]
--------------------------------------------------------------
DELETE FROM [tblHostFileConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblHostFileConfig] ([fldTaskId], [fldSystemProfileCode],[fldFileExt], [fldHostFileDesc], [fldProcessName], [fldPosPayType],[fldTaskRole],[fldFTPFolder], [fldEnable],[fldEntityCode],[fldBankCode]) VALUES (@taskId, @systemProfileCode, @fileExt, @taskDesc, @processName, @posPayType,@taskRole,@FTPFolder, N'Y',@bank, @bank)


--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page',@mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/ServiceActivate?tId='+@taskId)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', N'', @taskDesc, 0, N'*', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)