
--------------------------------------
--TASK MASTER (DASHBOARD - ONLINE USER)
--------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '309011'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 309011, 309010, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'', N'DASHBOARD', 1, 5, N'Online User', N'', N'Dashboard - Online User', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'')


--------------------------------------
--DASHBOARD CONFIG
--------------------------------------
DELETE FROM [tblDashboardConfig] WHERE [TaskId] = '309011'
INSERT INTO [tblDashboardConfig] (parentTaskId, taskId, databaseViewId,Title,DivId,DivWidth,WidgetType,IsEnabled,Ordering) VALUES ( 309010, 309011,	'View_OnlineUser','Online User','onlineWidget','7','PaginatedTable','Y', 1);
GO

--------------------------------------
--VIEW
--------------------------------------

DROP VIEW [dbo].[View_OnlineUser]
GO
CREATE VIEW [dbo].[View_OnlineUser]
AS
SELECT DISTINCT b.fldUserAbb, a.fldLastActiveTimeStamp 
FROM dbo.tblUserSessionTrack AS a LEFT OUTER JOIN dbo.tblUserMaster AS b ON a.fldUserId = b.fldUserId
WHERE 
DATEADD(mi, (select top 1 fldTimeOut from tblSecurityProfile) , fldLastActiveTimeStamp) >= GETDATE()
GO


----------------------------------------
--SEARCH PAGE CONFIG
----------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = '309011'
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309011', N'View_OnlineUser', N'fldUserAbb', NULL, NULL, N'', N'User', N'', NULL, N'Dashboard - Online user', 10, N'N', N'Y', N'N', N'Y', N'Y', 1, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309011', N'View_OnlineUser', N'fldLastActiveTimeStamp', NULL, NULL, N'', N'Online Time', N'', NULL, N'Dashboard - Online user', 10, N'N', N'Y', N'N', N'Y', N'Y', 1, N'029', N'029')
GO