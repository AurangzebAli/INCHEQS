--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_GenerateOutwardReturn]
GO
CREATE VIEW [dbo].[View_GenerateOutwardReturn]
AS
SELECT 
count(1) as fldTotalItem, a.fldClearDate,a.fldDataFileName,a.fldAmount, a.fldBatchStatus,a.fldAckStatus,a.fldAckDescription

FROM 
tbloutwardreturnitem a left join tblAcknowledgement b on a.fldDataFileName = b.fldDataFileName

WHERE 
isnull(a.fldReGenBatchNumber, 0)= 0 

GROUP BY 
a.fldClearDate,a.fldDataFileName,a.fldAmount, a.fldBatchStatus,a.fldAckStatus,a.fldAckDescription


GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Inward Clearing';
DECLARE @subMenu varchar(100) = 'OUTWARD RETURN';
DECLARE @menuTitle varchar(100) = 'Generate Outward Return';
DECLARE @taskDesc varchar(100) = 'Inward Clearing - Generate Outward Return';
DECLARE @mainMenuOrder int = 1;
DECLARE @subMenuOrder int = 25;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_GenerateOutwardReturn';

DECLARE @systemProfileCode varchar(100) = '';
DECLARE @fileExt varchar(100) = '';
DECLARE @processName varchar(100) = 'ICSGenerateUPI';
DECLARE @posPayType varchar(100) = '';
DECLARE @FTPFolder varchar(100) = 'OutwardReturn/';
DECLARE @seqTableName varchar(100) = 'GenOutwardReturn';
DECLARE @taskRole varchar(100) = 'OutwardReturn';

DECLARE @parentTaskId varchar(100) = '309000';
DECLARE @taskId varchar(100) = '309220';

--------------------------------------------------------------
--Add to [tblHostFileConfig]
--------------------------------------------------------------
DELETE FROM [tblHostFileConfig] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblHostFileConfig] ([fldTaskId], [fldSystemProfileCode],[fldFileExt], [fldHostFileDesc], [fldProcessName], [fldPosPayType],[fldTaskRole],[fldFTPFolder], [fldEnable],[fldEntityCode],[fldBankCode]) VALUES (@taskId, @systemProfileCode, @fileExt, @taskDesc, @processName, @posPayType,@taskRole,@FTPFolder, N'Y',@bank, @bank)

--------------------------------------------------------------
--Add to [[tblSequenceMaster]]
--------------------------------------------------------------
DELETE FROM [tblSequenceMaster] WHERE [fldTableName] = @seqTableName
INSERT [dbo].[tblSequenceMaster] ([fldSequenceId], fldTableName,fldLastSeqId) VALUES ((select top 1 fldSequenceId from tblSequenceMaster order by fldSequenceId desc) + 1, @seqTableName, 1)

--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/GenerateDebitFile?tId='+@taskId)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'Date', N'Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', N'', @taskDesc, 0, N'Y', N'Y', N'N', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldDataFileName', NULL, NULL, N'', N'Data File Batch', NULL, N'', @taskDesc, 10, N'Y', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTotalItem', NULL, NULL, N'', N'Total Item', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAmount', NULL, NULL, N'Currency', N'Total Amount', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBatchStatus', NULL, NULL, N'', N'Status', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAckStatus', NULL, NULL, N'', N'Ack Status', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldAckDescription', NULL, NULL, N'', N'Ack Desc', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', 1, @bank, @bank)

GO
