--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_MicrImageImportICL]
GO
CREATE VIEW [dbo].[View_MicrImageImportICL]
AS
SELECT 
IC.fldInwardClearingFileId, IC.fldInwardFileName, IC.fldTotalItemCount, IC.fldTotalAmount, IC.fldTotalImageCount, IC.fldImportTimeStamp, IC.fldCreateTimeStamp, IC.fldImportUserId, IC.fldCreateUserID, um.fldUserAbb,IC.fldClearDate, IC.fldTotalItemCount AS 'CountImportItem', IC.fldTotalItemCount - IC.fldTotalImageCount AS 'fldVariance'
FROM 
dbo.tblInwardClearingFile AS IC LEFT OUTER JOIN 
dbo.tblUserMaster AS um ON IC.fldCreateUserID = um.fldUserId
WHERE 
(IC.fldActive = 'Y') AND (RIGHT(IC.fldInwardFileName,3) = 'ICL')
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'MICR & IMAGE';
DECLARE @menuTitle varchar(100) = 'Import (ICL)';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - MICR & Image (ICL)';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 1;

DECLARE @view varchar(100) = 'View_MicrImageImportICL';
DECLARE @bank varchar(100) = '009';
DECLARE @taskRole varchar(100) = 'ICL';

DECLARE @parentTaskId varchar(100) = '306000';
DECLARE @taskId varchar(100) = '306010';
DECLARE @download varchar(100) = '306011';
DECLARE @import varchar(100) = '306012';
DECLARE @fileList varchar(100) = '306013';
DECLARE @history varchar(100) = '306014';
DECLARE @exception varchar(100) = '306015';

--Services Params
DECLARE @SystemProfileCode varchar(100) = 'InwardGatewayFolder';
DECLARE @DateSubString int = 2;
DECLARE @BankCodeSubString int = 26;
DECLARE @FileExt varchar(100) = '.ICL';
DECLARE @ProcessName varchar(100) = 'ICSImport';
DECLARE @PosPayType varchar(100) = 'Import';

--------------------------------------------------------------
--Add to tblQueueConfig
--------------------------------------------------------------
DELETE FROM [tblQueueConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblQueueConfig] ([fldId], [fldTaskId], [fldQueueName], [fldQueueDesc], [fldViewName], [fldOrderBy], [fldUserParamCondition], [fldAllowedAction], [fldTaskRole], [fldLockCondition], [fldEnable], [fldEntityCode], [fldBankCode]) VALUES (7, @taskId, @menuTitle, @taskDesc, @view, N'', N'', @taskId+N','+@download+N','+@import+N','+@fileList+N','+@history+N','+@exception, @taskRole, N'', N'Y', @bank , @bank)


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

--record status is 0 : wont be display in task list.. change record master to 1 if need to show
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @download
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @download, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Download & Import)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 0, N'DownloadImport')

DELETE FROM tblTaskMaster WHERE [fldTaskId] = @import
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @import, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Import)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'Import')

DELETE FROM tblTaskMaster WHERE [fldTaskId] = @fileList
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @fileList, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (File Listing)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'FileListing')

--record status is 0 : wont be display in task list.. change record master to 1 if need to show
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @history
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @history, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Download History)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 0, N'DownloadHistory')

DELETE FROM tblTaskMaster WHERE [fldTaskId] = @exception
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @exception, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDesc+N' (Exception File Listing)', N'N', N'N', N'', N'', 0, 0, N'', N'', N'', 1, N'ExceptionFileListing')


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', N'', @taskDesc, 0, N'*', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldInwardFileName', NULL, NULL, N'', N'File Name', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTotalItemCount', NULL, NULL, N'', N'Total Item', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 2, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTotalImageCount', NULL, NULL, N'', N'Total Image', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 3, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldVariance', NULL, NULL, N'', N'Variance', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 5, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTotalAmount', NULL, NULL, N'Currency', N'Amount', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 6, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldUserAbb', NULL, NULL, N'', N'Imported By', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 7, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldCreateTimeStamp', NULL, NULL, N'', N'Import Date', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 8, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'CountImportItem', NULL, NULL, N'', N'CountImportItem', NULL, N'', @taskDesc, 10, N'Y', N'N', N'N', N'N', N'Y', 9, @bank, @bank)

GO