

--------------------------------------------------------------
--create view
--------------------------------------------------------------
Drop View [dbo].[View_SummaryInwardClearingChequeReport]
Go
CREATE VIEW [dbo].[View_SummaryInwardClearingChequeReport]
AS
SELECT        TOP (100) PERCENT fldClearDate, SUM(TotRec) AS TotRec, SUM(TotAmo) AS TotAmo, fldIssueStateCode, fldIssueBranchCode, BranchCode, BranchDesc
FROM            (SELECT        ci.fldClearDate, COUNT(1) AS TotRec, SUM(ci.fldAmount) AS TotAmo, ci.fldIssueStateCode, ci.fldIssueBranchCode, ci.fldIssueStateCode + ci.fldIssueBranchCode AS BranchCode, 
                                                    br.fldBranchDesc AS BranchDesc
                          FROM            dbo.tblInwardItemInfo AS ci LEFT OUTER JOIN
                                                    dbo.tblBranchMaster AS br ON br.fldBankCode = ci.fldIssueBankCode AND br.fldStateCode = ci.fldIssueStateCode AND br.fldBranchCode = ci.fldIssueBranchCode
                          GROUP BY ci.fldClearDate, ci.fldIssueStateCode, ci.fldIssueBranchCode, br.fldBranchDesc
                          UNION
                          SELECT        ci.fldClearDate, COUNT(1) AS TotRec, SUM(ci.fldAmount) AS TotAmo, ci.fldIssueStateCode, ci.fldIssueBranchCode, ci.fldIssueStateCode + ci.fldIssueBranchCode AS BranchCode, 
                                                   br.fldBranchDesc AS BranchDesc
                          FROM            dbo.tblInwardItemInfoH AS ci LEFT OUTER JOIN
                                                   dbo.tblBranchMaster AS br ON br.fldBankCode = ci.fldIssueBankCode AND br.fldStateCode = ci.fldIssueStateCode AND br.fldBranchCode = ci.fldIssueBranchCode
                          GROUP BY ci.fldClearDate, ci.fldIssueStateCode, ci.fldIssueBranchCode, br.fldBranchDesc) AS tbl
GROUP BY fldClearDate, fldIssueStateCode, fldIssueBranchCode, BranchCode, BranchDesc
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Reports';
DECLARE @subMenu varchar(100) = 'INWARD CLEARING';
DECLARE @menuTitle varchar(100) = 'Summary of Total Inward Clearing Cheque Report';
DECLARE @taskDesc varchar(100) = 'Report - Summary of Total Inward Clearing Cheque Report';
DECLARE @mainMenuOrder int = 7;
DECLARE @subMenuOrder int = 63;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_SummaryInwardClearingChequeReport';
DECLARE @reportPath varchar(100) = '~//Reports/ICS/SummaryTotalInwardClearingChequeReport.rdlc';
DECLARE @reportTitle varchar(100) = 'SummaryInwardClearingCheque';
DECLARE @fileName varchar(100) = 'InwardClearingChequeRpt';
DECLARE @reportParam varchar(100) = '';
DECLARE @orientation varchar(100) = 'Landscape';

DECLARE @parentTaskId varchar(100) = '304000';
DECLARE @taskId varchar(100) = '304511';
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

INSERT INTO [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldIssueStateCode', NULL, NULL, N'SelectListWithAll', N'City', NULL, N'SELECT fldStateCode as "value", fldStateDesc as "text" FROM tblstatemaster', @taskDesc, 10, N'N', N'Y', N'Y', N'N', N'Y', 1, @bank, @bank)

GO
