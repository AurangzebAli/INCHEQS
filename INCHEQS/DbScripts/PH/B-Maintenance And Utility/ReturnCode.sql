--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_ReturnCode]
Go
CREATE VIEW [dbo].[View_ReturnCode]
AS
SELECT	*, (CASE WHEN fldCharges = 1 THEN 'No' ELSE 'Yes' END) as fldWaive,fldRejectCode as fldRejectIdForDelete,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_ReturnCode') GROUP BY ReportTitle) AS ReportTitle
FROM	tblRejectMaster
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'VERIFICATION';
DECLARE @menuTitle varchar(100) = 'Return Code';
DECLARE @taskDesc varchar(100) = 'Maintenance - Return Code';
DECLARE @taskDescNew varchar(100) = 'Maintenance - Return Code (New)';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Return Code (Edit)';
DECLARE @taskDescDelete varchar(100) = 'Maintenance - Return Code (Delete)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 10;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_ReturnCode';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102150';
DECLARE @new varchar(100) = '102151';
DECLARE @edit varchar(100) = '102152';
DECLARE @delete varchar(100) = '102153';

------------------------------------------------------------------------
-- Return Code
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ReturnCode')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @new
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @new, @taskId, NULL, N'B', N'', N'N', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescNew, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @taskId, NULL, N'B', N'', N'E', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescEdit, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @delete
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @delete, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescDelete, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)


--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]=@taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES 
(N'8', @taskId, @view, N'ReturnCodeDataset' ,N'~/Reports/ICS/ReturnCodePrint.rdlc', N'Return Code List',  N'ReturnCodeList', N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName],[PrintReportParam], [IsEnabled], [Ordering]) VALUES 
(N'8', @taskId, @view, N'ReturnCodeDataset' ,N'~/Reports/ICS/ReturnCodePrint.rdlc', N'Return Code List',  N'ReturnCodeList', N'page',N'Y', 1)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId=@taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectCode', NULL, NULL, N'TextBox', N'Return Code', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectDesc', NULL, NULL, N'TextBox', N'Return Code Description', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldType', NULL, NULL, N'TextBox', N'Return Code Type', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectIdForDelete', NULL, NULL, N'DeleteBox', N'Delete', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO