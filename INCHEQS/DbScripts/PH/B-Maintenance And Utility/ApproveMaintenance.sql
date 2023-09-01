--------------------------------------------------------------
--Create View
--------------------------------------------------------------
DROP VIEW [dbo].View_ApprovedChecker
Go
CREATE VIEW [dbo].View_ApprovedChecker
AS
SELECT fldId, fldName, CASE 
	WHEN fldstatus = 'D' then 'Delete' 
	WHEN fldstatus = 'A' then 'Add'
	END as fldStatus, fldTaskDesc, fldIdForDelete
FROM (
    SELECT 
		CONVERT(VARCHAR, fldUserId) AS fldId,
		fldUserAbb AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/User') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/User')+fldUserAbb) as fldIdForDelete
	FROM tblUserMasterTemp

    UNION ALL

    SELECT 
		CONVERT(VARCHAR, fldStatusID) AS fldId,
		fldStatusDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/HostReturnReason') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/HostReturnReason')+CONVERT(VARCHAR, fldStatusID)) as fldIdForDelete
	FROM tblBankHostStatusMasterTemp

	 UNION ALL

    SELECT 
		CONVERT(VARCHAR, fldBankCode) AS fldId,
		fldBankDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/BankCode') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/BankCode')+CONVERT(VARCHAR, fldBankCode)) as fldIdForDelete
	FROM tblBankMasterTemp

	UNION ALL

    SELECT 
		CONVERT(VARCHAR, fldStateCode) AS fldId,
		fldStateDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/StateCode') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/StateCode')+CONVERT(VARCHAR, fldStateCode)) as fldIdForDelete
	FROM tblStatemasterTemp

	UNION ALL

    SELECT 
		CONVERT(VARCHAR, fldBranchId) AS fldId,
		fldBranchDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/BranchCode') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/BranchCode')+CONVERT(VARCHAR, fldBranchId)) as fldIdForDelete
	FROM tblBranchmasterTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldConBranchCode) AS fldId,
		fldBranchDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/InternalBranch') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/InternalBranch')+CONVERT(VARCHAR, fldConBranchCode)) as fldIdForDelete
	FROM tblMapBranchTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldClass) AS fldId,
		fldLimitDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/VerificationLimit') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/VerificationLimit')+CONVERT(VARCHAR, fldClass)) as fldIdForDelete
	FROM tblVerificationBatchSizeLimitTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldPullOutID) AS fldId,
		fldPullOutReason AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/PullOutReason') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/PullOutReason')+CONVERT(VARCHAR, fldPullOutID)) as fldIdForDelete
	FROM tblPullOutReasonTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldRejectCode) AS fldId,
		fldRejectDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/ReturnCode') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/ReturnCode')+fldRejectCode) as fldIdForDelete
	FROM tblRejectMasterTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldTransCode) AS fldId,
		fldTransDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/TransactionCode') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/TransactionCode')+fldTransCode) as fldIdForDelete
	FROM tblTransMasterTemp

	UNION ALL

     SELECT 
		CONVERT(VARCHAR, fldTransactionType) AS fldId,
		fldTransactionDesc AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/TransactionType') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/TransactionType')+fldTransactionType) as fldIdForDelete
	FROM tblCTCSTransactionTypetemp

	UNION ALL

	SELECT 
		CONVERT(VARCHAR, fldId) AS fldId,
		fldTitle AS fldName,
		fldApproveStatus AS fldStatus,
		(select fldTaskDesc from tblTaskMaster WHERE fldMvcUrl = 'ICS/ThresholdSetting') AS fldTaskDesc,
		(fldApproveStatus+(select CONVERT(VARCHAR,fldTaskId) from tblTaskMaster WHERE fldMvcUrl = 'ICS/ThresholdSetting')+CONVERT(VARCHAR,fldId)) as fldIdForDelete
	FROM tblThresholdSettingTemp

) tempTable
GROUP BY fldTaskDesc, fldId, fldName, fldStatus,fldIdForDelete



GO

--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Maintenance';
DECLARE @subMenu varchar(100) = 'SYSTEM AND PROFILE';
DECLARE @menuTitle varchar(100) = 'Approve Checker';
DECLARE @taskDesc varchar(100) = 'Maintenance - Approve Checker';
DECLARE @taskDescVerify varchar(100) = 'Maintenance - Approved Checker (Verify)';
DECLARE @mainMenuOrder int = 9;
DECLARE @subMenuOrder int = 1;

DECLARE @bank varchar(100) = '009';
DECLARE @view varchar(100) = 'View_ApprovedChecker';

DECLARE @parentTaskId varchar(100) = '102000';
DECLARE @taskId varchar(100) = '102180';
DECLARE @verify varchar(100) = '102184';
------------------------------------------------------------------------
-- User
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ApprovedChecker')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @verify
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @verify, @taskId, NULL, N'B', N'', N'D', NULL, N'N', N'', N'', N'', N'', 0, 0, N'', N'', @taskDescVerify, N'', N'N', N'', N'', 0, 0, N'', N'', N'', 1, NULL)

--------------------------------------------------------------
--Configure filter/result in tblSearchPageConfig
--------------------------------------------------------------
DELETE FROM [tblSearchPageConfig] where TaskId=@taskId

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldId', NULL, NULL, N'TextBox', N'User Id', NULL, N'', @taskDesc, 999, N'Y', N'N', N'N', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldName', NULL, NULL, N'TextBox', N'Name', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldTaskDesc', NULL, NULL, N'TextBox', N'Task Description', NULL, N'', @taskDesc, 999, N'N', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)


INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldStatus', NULL, NULL, N'TextBox', N'Status', NULL, N'', @taskDesc, 999, N'Y', N'Y', N'Y', N'N', N'Y', -1, @bank, @bank)

INSERT [dbo].[tblSearchPageConfig] ([TaskId], [DatabaseViewId], [FieldId], [FieldIdSqlCondition], [FieldIdOrderBy], [FieldType], [FieldLabel], [FieldDefaultValue], [ValueTextQueryForOption], [PageTitle], [Length], [IsResultParam], [IsFilter], [IsResult], [IsReadOnly], [IsEnabled], [Ordering], [EntityCode], [BankCode]) VALUES (@taskId, @view, N'fldIdForDelete', NULL, NULL, N'DeleteBox', N'Action', NULL, N'', @taskDesc, 999, N'N', N'N', N'Y', N'N', N'Y', -1, @bank, @bank)
GO