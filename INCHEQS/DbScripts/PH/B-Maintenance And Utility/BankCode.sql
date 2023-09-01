--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_BankCode]
Go
CREATE VIEW [dbo].[View_BankCode]
AS
SELECT	*,fldBankCode as fldBankCodeIdForDelete,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_BankCode') GROUP BY ReportTitle) AS ReportTitle
FROM	tblbankmaster WHERE LEN(fldBankCode) < 3
GO 

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'BANK';
DECLARE @menuTitle varchar(100) = 'Bank Code';
DECLARE @taskDesc varchar(100) = 'Maintenance - Bank Code';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Bank Code (New)';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Bank Code (Edit)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Bank Code (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 20;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_BankCode';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102110';
DECLARE @new varchar(100) = '102111';
DECLARE @edit varchar(100) = '102112';
DECLARE @delete varchar(100) = '102113';

------------------------------------------------------------------------
-- BankCode
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/BankCode')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, @taskId, NULL, N'B', N'', N'N', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescNew, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @taskId, NULL, N'B', N'', N'E', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescEdit, N'', N' ', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescDelete, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]= @taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'11', @taskId, @view , N'BankCodeDataset' ,N'~/Reports/ICS/BankCodePrint.rdlc', N'Bank Code List',  N'BankCodeList',N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'11', @taskId, @view , N'BankCodeDataset' ,N'~/Reports/ICS/BankCodePrint.rdlc', N'Bank Code List',  N'BankCodeList',N'page',N'Y', 1)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBankCode', NULL, NULL, N'TextBox', N'Bank Code', NULL, N'', @taskDesc , 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBankDesc', NULL, NULL, N'TextBox', N'Bank Description', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldBankCodeIdForDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO