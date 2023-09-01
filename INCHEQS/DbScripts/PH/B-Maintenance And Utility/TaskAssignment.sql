--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_Task]
Go
CREATE VIEW [dbo].[View_Task]
AS
SELECT *,(SELECT ReportTitle FROM tblReportPageConfig WHERE (DatabaseViewId = 'View_Task') GROUP BY ReportTitle) AS ReportTitle
FROM tblGroupMaster
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'SYSTEM AND PROFILE';
DECLARE @menuTitle varchar(100) = 'Task Assignment';
DECLARE @taskDesc varchar(100) = 'Maintenance - Task Assignment';
DECLARE @taskDescEdit varchar(100) = 'Maintenance - Task Assignment (Edit)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 3;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_Task';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102230';
DECLARE @edit varchar(100) = '102231';

------------------------------------------------------------------------
-- Task Assignment
------------------------------------------------------------------------

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/Task')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @edit
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @edit, @parentTaskId, NULL, N'B', N'', N'E', NULL, N' ', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescEdit, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)


--------------------------------------------------------------
-- PRINT CONFIG 
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] WHERE [TaskId]= @taskId
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'7', @taskId, @view , N'TaskMasterDataset' ,N'~/Reports/ICS/TaskPrint.rdlc', N'Task Listing', N'Task Listing',N'all',N'Y', 1)

INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam],[IsEnabled], [Ordering]) VALUES 
(N'7', @taskId, @view , N'TaskMasterDataset' ,N'~/Reports/ICS/TaskPrint.rdlc', N'Task Listing', N'Task Listing',N'page',N'Y', 1)


--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldGroupId', NULL, NULL, N'TextBox', N'Group Id', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldGroupDesc', NULL, NULL, N'TextBox', N'Group Description', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)
GO