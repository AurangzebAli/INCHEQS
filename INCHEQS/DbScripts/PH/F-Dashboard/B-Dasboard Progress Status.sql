
--------------------------------------
--TASK MASTER (DASHBOARD - PROGRESS STATUS)
--------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '309012'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 309012, 309000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'', N'Dashboard', N'', 1, 5, N'Dashboard - Progress Status', N'', N'Dashboard - Progress Status', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'')
--------------------------------------
--DASHBOARD CONFIG
--------------------------------------
DELETE FROM [tblDashboardConfig] WHERE [TaskId] = '309012'
INSERT INTO [tblDashboardConfig] (parentTaskId, taskId, databaseViewId,Title,DivId,DivWidth,WidgetType,IsEnabled,Ordering) VALUES ( 309010, 309012,	'View_DashboardProgressStatus','Progress Status','progressStatusWidget','12','QueueTableTemplate','Y', 2);
GO


--------------------------------------
--VIEW
--------------------------------------

DROP VIEW [dbo].[View_DashboardProgressStatus]
GO
CREATE VIEW [dbo].[View_DashboardProgressStatus]
AS
SELECT * FROM View_ProgressStatus
WHERE fldClearDate = (select top 1 fldcleardate from tblinwardcleardate order by fldcleardate desc)
GO


----------------------------------------
--SEARCH PAGE CONFIG
----------------------------------------
DELETE FROM [tblSearchPageConfig] WHERE [TaskId] = '309012'
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'fldClearDate', NULL, NULL, N'Date', N'Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'Y', N'N', N'Y', N'Y', 1, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'BranchCheckerReturnOutstanding', NULL, NULL, N'', N'BranchCheckerReturnOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 13, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'QueueName', NULL, NULL, N'', N'Description', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 2, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'Approved', NULL, NULL, N'', N'Approved', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 3, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'Returned', NULL, NULL, N'', N'Returned', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 4, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'TotalCompleted', NULL, NULL, N'', N'TotalCompleted', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 5, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'ReadyToHost', NULL, NULL, N'', N'ReadyToHost', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 6, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'RejectReentryMakerOutstanding', NULL, NULL, N'', N'RejectReentryMakerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 7, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'RejectReentryCheckerOutstanding', NULL, NULL, N'', N'RejectReentryCheckerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 8, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'FirstVerificationOutstanding', NULL, NULL, N'', N'FirstVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 9, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'TotalItem', NULL, NULL, N'', N'TotalItem', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 14, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'SecondVerificationOutstanding', NULL, NULL, N'', N'SecondVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 10, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'ThirdVerificationOutstanding', NULL, NULL, N'', N'ThirdVerificationOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 10, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'BranchMakerOutstanding', NULL, NULL, N'', N'BranchMakerOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 11, N'029', N'029')
GO
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (N'309012', N'View_DashboardProgressStatus', N'BranchCheckerApproveOutstanding', NULL, NULL, N'', N'BranchCheckerApproveOutstanding', N'', NULL, N'Inward Clearing - Progress Status', 10, N'N', N'N', N'Y', N'Y', N'Y', 12, N'029', N'029')
GO
