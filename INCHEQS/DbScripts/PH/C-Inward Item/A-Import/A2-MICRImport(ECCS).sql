--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_MicrImageImportECCS]
GO
CREATE VIEW [dbo].[View_MicrImageImportECCS]
AS
SELECT 
fldMTFFileName, fldBranchCount, fldRecordCount,MF.fldClearDate,MF.fldCreateTimeStamp[fldCreateTimestamp],MF.fldCreateUserID[fldCreateUserID], flduserAbb, fldFileErrorDesc, fldFileType, fldReupload, fldRecordCount AS 'CountImportItem' , SUBSTRING(fldMTFFileName,10,12) AS fileType
FROM 
tblMTFFile MF LEFT OUTER JOIN 
tblusermaster um on MF.fldcreateuserid = um.flduserid
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'MICR & IMAGE';
DECLARE @menuTitle varchar(100) = 'Import (ECCS)';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - MICR & Image (ECCS)';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 2;

DECLARE @view varchar(100) = 'View_MicrImageImportECCS';
DECLARE @bank varchar(100) = '009';
DECLARE @taskRole varchar(100) = 'ECCS';

DECLARE @parentTaskId varchar(100) = '306000';
DECLARE @taskId varchar(100) = '306020';
DECLARE @import varchar(100) = '306021';
DECLARE @fileList varchar(100) = '306022';
DECLARE @history varchar(100) = '306023';

--Services Params
DECLARE @SystemProfileCode varchar(100) = 'ECCSFolder';
DECLARE @DateSubString int = 0;
DECLARE @BankCodeSubString int = 9;
DECLARE @FileExt varchar(100) = 'selectlist';
DECLARE @ProcessName varchar(100) = 'ICSLoadMTFFile';
DECLARE @PosPayType varchar(100) = 'filename';

--------------------------------------------------------------
--Add to tblQueueConfig
--------------------------------------------------------------
DELETE FROM [tblQueueConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblQueueConfig] ([fldId], [fldTaskId], [fldQueueName], [fldQueueDesc], [fldViewName], [fldOrderBy], [fldUserParamCondition], [fldAllowedAction], [fldTaskRole], [fldLockCondition], [fldEnable], [fldEntityCode], [fldBankCode]) VALUES (7, @taskId, @menuTitle, @taskDesc, @view, N'', N'',  @taskId+N','+@import+N','+@fileList+N','+@history, @taskRole, N'', N'Y', @bank , @bank)

--------------------------------------------------------------
--Add to [tblMICRImportConfig]
--[fldFolderName] must have data in [tblsystemprofile] to get a folder path
--------------------------------------------------------------
DELETE FROM [tblMICRImportConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblMICRImportConfig] ([fldId], [fldTaskId], [fldSystemProfileCode], [fldDateSubString], [fldBankCodeSubString], [fldFileExt], [fldProcessName], [fldPosPayType], [fldEnable],[fldEntityCode],[fldBankCode]) VALUES (1,@taskId,@SystemProfileCode, @DateSubString, @BankCodeSubString, @FileExt, @ProcessName, @PosPayType,N'Y',@bank,@bank)

--------------------------------------------------------------
--Update Task Master
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/MicrImageImport?tId='+@taskId)

DELETE FROM tblTaskMaster WHERE [fldTaskId] = @import
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @import, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Import)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'Import')

DELETE FROM tblTaskMaster WHERE [fldTaskId] = @fileList
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @fileList, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (File Listing)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'FileListing')

--record status is 0 : wont be display in task list.. change record master to 1 if need to show
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @history
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @history, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Download History)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 0, N'DownloadHistory')


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', N'', @taskDesc, 0, N'*', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fileType', NULL, NULL, N'SelectList', N'Source File', N'', N'SELECT ''029PHP00.MTF'' as "value" , ''029PHP00.MTF'' as "text" ,1 as "ordering" union SELECT ''029PHP00.AMF'' as "value" , ''029PHP00.AMF'' as "text" ,2 as "ordering" union SELECT ''029PHP00.DSF'' as "value" , ''029PHP00.DSF'' as "text" ,3 as "ordering" union SELECT ''029PHP00.REF'' as "value" , ''029PHP00.REF'' as "text" ,4 as "ordering" order by ordering', @taskDesc, 10, N'N', N'Y', N'N', N'Y', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldMTFFileName', NULL, NULL, N'', N'ECCS File Name', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBranchCount', NULL, NULL, N'', N'Branch', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 3, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRecordCount', NULL, NULL, N'', N'Item Count', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 4, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldUserAbb', NULL, NULL, N'', N'Download By', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 7, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldCreateTimeStamp', NULL, NULL, N'', N'Download Date', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 6, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'CountImportItem', NULL, NULL, N'', N'Import Item', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 5, @bank, @bank)
