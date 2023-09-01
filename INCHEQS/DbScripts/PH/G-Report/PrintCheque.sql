--------------------------------------------------------------
--Print Cheque
--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].[View_PrintCheque]
GO
CREATE VIEW [dbo].[View_PrintCheque]
AS
SELECT	iw.fldClearDate, iw.fldUIC, iw.fldAccountNumber, iw.fldChequeSerialNo, iw.fldIssueBankCode + iw.fldIssueBranchCode AS payingBranch, bm.fldBankDesc, iw.fldAmount, iw.fldTransDesc, iw.fldRejectDesc,iw.fldRemarks, iw.fldInwardItemId
FROM	dbo.View_AppInwardItem AS iw LEFT OUTER JOIN dbo.tblBankMaster AS bm ON iw.fldPreBankCode = bm.fldBankCode
GO

--report config
DELETE FROM [tblReportPageConfig] where [TaskId]=62000
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam], [Orientation], [IsEnabled], [Ordering]) VALUES 
(N'3', N'62000', N'View_PrintCheque', N'View_PrintCheque' ,N'~/Reports/ICS/ChequePrint.rdlc', N'Print Cheque', N'Print Cheque', N'', N'Portrait', N'Y', 1)
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Reports';
DECLARE @subMenu varchar(100) = 'INWARD CLEARING';
DECLARE @menuTitle varchar(100) = 'Pending Branch Decision Report';
DECLARE @taskDesc varchar(100) = 'Report - Pending Branch Decision Report';
DECLARE @mainMenuOrder int = 7;
DECLARE @subMenuOrder int = 63;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_PendingBranchDecisionReport';
DECLARE @reportPath varchar(100) = '~//Reports/ICS/ChequePrint.rdlc';
DECLARE @reportTitle varchar(100) = 'Print Cheque';
DECLARE @fileName varchar(100) = 'PrintCheque';
DECLARE @reportParam varchar(100) = '';
DECLARE @orientation varchar(100) = 'Portrait';

DECLARE @parentTaskId varchar(100) = '304000';
DECLARE @taskId varchar(100) = '304560';
--------------------------------------------------------------
--Update Taskmaster
--------------------------------------------------------------
DELETE FROM tblTaskMaster WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, NULL, @taskDesc, N'N', N'N', N'', NULL, 780, 400, N'', N'', N'', 1, N'ICS/DefaultReport?tId='+@taskId)

--------------------------------------------------------------
--report config
--------------------------------------------------------------
DELETE FROM [tblReportPageConfig] where [TaskId]=@taskId
INSERT INTO [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [ReportPath], [ReportTitle], [DataSetName], [exportFileName],[PrintReportParam], [Orientation],  [IsEnabled], [Ordering]) VALUES ((select top 1 TaskId from tblReportPageConfig order by TaskId desc) + 1, @taskId, @view, @reportPath, @reportTitle, @view, @fileName, @reportParam, @orientation, N'Y', 1)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where [TaskId]=@taskId
INSERT INTO [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldClearDate', NULL, NULL, N'DateFromTo', N'Cheque Clearing Date', N'sql:select top 1 fldcleardate as "defaultValue" from tblinwardcleardate order by fldcleardate desc', NULL, @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 1, @bank, @bank)

INSERT INTO [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldRejectCode', NULL, NULL, N'SelectListWithAll', N'Return Reason', NULL, N'SELECT fldRejectCode as "value", fldRejectDesc as "text" FROM tblRejectMaster', @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 1, @bank, @bank)

GO
--search config
DELETE FROM [tblSearchPageConfig] where [TaskId]=62000
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES 
(N'62000', N'View_PrintCheque', N'fldInwardItemId', NULL, NULL, N'TextBox', N'Inward Item Id', N'', NULL, N'Report - Print Cheque', 10, N'N', N'Y', N'Y', NULL, N'Y', 1, N'029', N'029')
GO
--------------------------------------------------------------
--Print Cheque Retriever
--------------------------------------------------------------

--report config
DELETE FROM [tblReportPageConfig] where [TaskId]=62010
INSERT [dbo].[tblReportPageConfig] ([ReportId], [TaskId], [DatabaseViewId], [DataSetName], [ReportPath], [ReportTitle], [exportFileName], [PrintReportParam], [Orientation], [IsEnabled], [Ordering]) VALUES 
(N'3', N'62010', N'View_PrintCheque', N'View_PrintCheque' ,N'~/Reports/ICS/ChequeRetrieverPrint.rdlc', N'Print Cheque', N'Print Cheque', N'', N'Portrait', N'Y', 1)
GO

--search config
DELETE FROM [tblSearchPageConfig] where [TaskId]=62010
INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES 
(N'62010', N'View_PrintCheque', N'fldInwardItemId', NULL, NULL, N'TextBox', N'Inward Item Id', N'', NULL, N'Report - Print Cheque', 10, N'N', N'Y', N'Y', NULL, N'Y', 1, N'029', N'029')
GO
