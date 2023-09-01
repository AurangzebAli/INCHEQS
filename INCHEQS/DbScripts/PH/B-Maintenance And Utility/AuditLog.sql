--------------------------------------------------------------
--create view
--------------------------------------------------------------
DROP VIEW [dbo].[View_AuditLog]
Go
CREATE VIEW [dbo].[View_AuditLog]
AS
SELECT        TOP (100) PERCENT a.fldCreateTimeStamp, b.fldUserAbb, a.fldRemarks, b.fldBranchCode, c.fldGroupId
FROM            dbo.tblAuditTrail AS a INNER JOIN
                         dbo.tblUserMaster AS b ON a.fldUserId = b.fldUserId INNER JOIN
                         dbo.tblGroupUser AS c ON a.fldUserId = c.fldUserId
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Utilities';
DECLARE @subMenu varchar(100) = '';
DECLARE @menuTitle varchar(100) = 'Audit Log';
DECLARE @taskDesc varchar(100) = 'Utilities - Audit Log';
DECLARE @mainMenuOrder int = 8;
DECLARE @subMenuOrder int = 8;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_AuditLog';

DECLARE @parentTaskId varchar(100) = '205000';
DECLARE @taskId varchar(100) = '205130';

------------------------------------------------------------------------
-- TASK MENU
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page',@mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/AuditLog')

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId= @taskId
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldGroupId', NULL, NULL, N'SelectListWithAll', N'Group', NULL, N'SELECT DISTINCT fldGroupId as "value",fldGroupId as "text" FROM tblGroupUser order by fldGroupId asc', N'Audit Log', 10, N'N', N'Y', N'N', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldCreateTimeStamp', NULL, NULL, N'Date', N'Date', NULL, N'', N'Audit Log', 10, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRemarks', NULL, NULL, N'', N'Remarks', NULL, N'', @taskDesc, 10, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldUserAbb', NULL, NULL, N'SelectListWithAll', N'User ID', NULL, N'SELECT  fldUserAbb as "value", fldUserAbb as "text" FROM tblUserMaster order by fldUserAbb asc', @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)
GO
