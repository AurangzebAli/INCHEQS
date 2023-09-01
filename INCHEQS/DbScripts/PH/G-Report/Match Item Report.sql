--------------------------------------------------------------
--create view
--------------------------------------------------------------
Drop View [dbo].[View_MatchItemReport]
Go
CREATE VIEW [dbo].[View_MatchItemReport]
AS
SELECT /*mb.fldsol,*/ (ci.fldIssueStateCode + ci.fldIssueBankCode + ci.fldIssueBranchCode + ci.fldIssueDigit) AS fldBRSTN,ci.fldClearDate, 
			ci.fldChequeSerialNo, case when ci.fldChequeSerialNo <> ci.fldOriChequeSerialNo then  ci.fldOriChequeSerialNo else '' end as RepairedCheqNo, 
            ci.fldAccountNumber,  case when ci.fldAccountNumber <> ci.fldOriAccountNumber then  ci.fldOriAccountNumber else '' end as RepairedAcctNo, 
            ci.fldHostAccountNo as HostACNo, ci.fldAmount, ci.fldUnMatchRemarks, ci.fldtranscode 
            FROM View_AppInwardItem ci 
           	INNER JOIN tblMTFrecord mt   
			on ltrim(rtrim(mt.fldChequeNo)) = ltrim(rtrim(ci.fldChequeSerialNo))  
			and ltrim(rtrim(mt.fldBRSTN)) = ltrim(rtrim(ci.fldIssueStateCode + ci.fldIssueBankCode + ci.fldIssueBranchCode + ci.fldIssueDigit)) 
			and ltrim(rtrim(mt.fldAccountNumber)) = ltrim(rtrim(ci.fldAccountNumber)) 
			and ltrim(rtrim(mt.fldTransactionCode)) = ltrim(rtrim(ci.fldtranscode))
GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Reports';
DECLARE @subMenu varchar(100) = 'FREE ITEM''/''MISSING ITEM REPORT';
DECLARE @menuTitle varchar(100) = 'Generate Match Item Report';
DECLARE @taskDesc varchar(100) = 'Report - Generate Match Item Report';
DECLARE @mainMenuOrder int = 7;
DECLARE @subMenuOrder int = 63;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_MatchItemReport';
DECLARE @reportPath varchar(100) = '~//Reports/ICS/MatchItemReport.rdlc';
DECLARE @reportTitle varchar(100) = 'Match Item Report';
DECLARE @fileName varchar(100) = 'MatchItemRpt';
DECLARE @reportParam varchar(100) = '';
DECLARE @orientation varchar(100) = 'Landscape';

DECLARE @parentTaskId varchar(100) = '304000';
DECLARE @taskId varchar(100) = '304540';
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


GO

