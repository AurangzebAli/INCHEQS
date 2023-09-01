----------------------------------------------------------------------------------------
--User Master
----------------------------------------------------------------------------------------
ALTER TABLE tblUserMaster ADD fldCity varchar(50)
ALTER TABLE tblUserMaster ADD fldApproveStatus varchar(1)
GO
update tblUserMaster set fldApproveStatus = 'Y'
GO
----------------------------------------------------------------------------------------
--High Risk Account
----------------------------------------------------------------------------------------
ALTER TABLE tblHighRiskAccount ADD fldBankCode varchar(10)
ALTER TABLE tblHighRiskAccount ADD fldEntityCode varchar(10)
GO
----------------------------------------------------------------------------------------
--CMS Account Info
----------------------------------------------------------------------------------------
ALTER TABLE tblCMSAccountInfo ADD fldEntityCode varchar(10)
GO
----------------------------------------------------------------------------------------
--GL Replacement
----------------------------------------------------------------------------------------
ALTER TABLE tblGLReplacement ADD fldEntityCode varchar(10)
GO
----------------------------------------------------------------------------------------
--Update MapBranch Table
----------------------------------------------------------------------------------------
ALTER TABLE tblMapBranch ALTER COLUMN fldBranchAbb varchar(MAX)
GO
----------------------------------------------------------------------------------------
--Update tblInwardItemHistory Table
----------------------------------------------------------------------------------------
ALTER TABLE tblInwardItemHistory ALTER COLUMN fldRejectCode nvarchar(3) NULL
GO
----------------------------------------------------------------------------------------
--Security Profile
----------------------------------------------------------------------------------------
ALTER TABLE tblSecurityProfile ADD fldTimeOut int
ALTER TABLE tblSysProfile ADD fldRegisteredName nchar(50)
GO
update tblSysProfile set fldRegisteredName = 'MEGABANK'
GO
----------------------------------------------------------------------------------------
--Change assigned queue to int in tblInwardItemInfoStatus
----------------------------------------------------------------------------------------
ALTER TABLE tblInwardItemInfoStatus ALTER COLUMN fldAssignedQueue int
Go
----------------------------------------------------------------------------------------
--Add task mvcurl
----------------------------------------------------------------------------------------
ALTER TABLE tblTaskMaster ADD fldMvcUrl varchar(100)
Go
----------------------------------------------------------------------------------------
--Update tblinwarditemhistory
----------------------------------------------------------------------------------------
ALTER TABLE tblinwarditemhistory ALTER COLUMN fldRemarks varchar(MAX)
ALTER TABLE tblinwarditemhistory ADD fldTextAreaRemarks varchar(MAX)
GO
----------------------------------------------------------------------------------------
--Update tblDataProcess
----------------------------------------------------------------------------------------
ALTER TABLE tblDataProcess ALTER COLUMN fldTaskId int
----------------------------------------------------------------------------------------
--Update tblFileManager
----------------------------------------------------------------------------------------
ALTER TABLE tblFileManager ALTER COLUMN fldTaskId int
----------------------------------------------------------------------------------------
--Update tblMessage
----------------------------------------------------------------------------------------
ALTER TABLE tblMessage ALTER COLUMN fldSpickCode char(5)
----------------------------------------------------------------------------------------
--user master temp for approval
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblUserMasterTemp]
CREATE TABLE [dbo].[tblUserMasterTemp](
	[fldUserId] [int] NULL,
	[fldUserAbb] [varchar](100) NOT NULL,
	[fldUserDesc] [varchar](50) NOT NULL,
	[fldEmail] [varchar](50) NULL,
	[fldAppRight] [char](1) NOT NULL,
	[fldBankCode] [char](3) NULL,
	[fldSpickCode] [char](3) NOT NULL,
	[fldBPCSpick] [char](4) NULL,
	[fldAdminFlag] [char](1) NULL,
	[fldLoginIP1] [char](15) NOT NULL,
	[fldLoginIP2] [char](15) NULL,
	[fldChequeLimitID] [int] NULL,
	[fldBranchCode] [char](9) NULL,
	[fldBranchCode2] [char](5) NULL,
	[fldBranchCode3] [char](5) NULL,
	[fldAccessBranchFlag] [char](1) NULL,
	[fldSeeAllCheque] [char](1) NULL,
	[fldSeeOwnVerCheque] [char](1) NULL,
	[fldPassword] [nvarchar](100) NOT NULL,
	[fldFailLoginDate] [datetime] NULL,
	[fldLastLoginDate] [datetime] NULL,
	[fldPassLastUpdDate] [datetime] NULL,
	[fldPasswordExpDate] [datetime] NULL,
	[fldDisableLogin] [char](1) NOT NULL,
	[fldStartDltIdDate] [datetime] NULL,
	[fldCounter] [int] NOT NULL,
	[fldIDExpDate] [datetime] NULL,
	[fldIDExpStatus] [int] NULL,
	[fldAdminChequeFlag] [int] NULL,
	[fldVerifyChequeFlag] [tinyint] NOT NULL DEFAULT ((0)),
	[fldVerificationLimit] [int] NULL,
	[fldVerificationClass] [char](1) NULL,
	[fldRejectLimit] [float] NULL,
	[fldDeleted] [int] NULL,
	[fldCreateUserId] [int] NOT NULL,
	[fldCreateTimeStamp] [datetime] NOT NULL DEFAULT (getdate()),
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimeStamp] [datetime] NOT NULL DEFAULT (getdate()),
	[fldApproveBranchCode] [char](1) NULL,
	[fldReliefStaff] [char](1) NULL,
	[fldSwap] [char](1) NULL,
	[fldApproveStatus] [varchar](1) NULL,
	[fldCity] [varchar](50) NULL
)ON [PRIMARY]
GO
----------------------------------------------------------------------------------------
--Create Table for search page config
----------------------------------------------------------------------------------------
DROP TABLE tblSearchPageConfig
CREATE TABLE tblSearchPageConfig
(
	[TaskId] [varchar](10) NOT NULL,
	[DatabaseViewId] [varchar](200) NULL,
	[FieldId] [varchar](50) NULL,
	[FieldIdSqlCondition] [varchar](200) NULL,	
	[FieldIdOrderBy] [varchar](200) NULL,
	[FieldType] [varchar](50) NULL,
	[FieldLabel] [varchar](50) NULL,
	[FieldDefaultValue] [varchar](500) NULL,
	[ValueTextQueryForOption] [varchar](500) NULL,		
	[PageTitle] [varchar](100) NULL,
	[Length][int] NULL,
	[IsResultParam] [varchar](1) NULL,
	[IsFilter] [varchar](1) NULL,
	[IsResult] [varchar](1) NULL,
	[IsReadOnly] [varchar](1) NULL,
	[IsEnabled] [varchar](1) NULL,
	[Ordering] [int] NULL,
	[EntityCode] [int] NULL,
	[BankCode] [int] NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table & data for new threshold
----------------------------------------------------------------------------------------
DROP TABLE tblThresholdSetting
CREATE TABLE tblThresholdSetting
(
	fldId int NOT NULL,
	fldType varchar(50),
	fldSequence int,
	fldTitle varchar(255),
	fldAmount float,
	fldEnable varchar(1),
	fldUpdateUserId int,
	fldUpdateTimeStamp datetime,
	fldEntityCode varchar(20) NOT NULL,
	fldBankCode varchar(20) NOT NULL 
)ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for tblQueueConfig 
----------------------------------------------------------------------------------------
DROP TABLE tblQueueConfig
CREATE TABLE tblQueueConfig
(
	fldId int NOT NULL,
	fldTaskId int NOT NULL,
	fldQueueName varchar(255),
	fldQueueDesc varchar(255),
	fldViewName varchar(255) NOT NULL,
	fldOrderBy varchar(255),
	fldUserParamCondition varchar(255),
	fldAllowedAction varchar(255),
	fldTaskRole varchar(50),
	fldLockCondition varchar(255),
	fldEnable varchar(1),
	fldEntityCode varchar(20) NOT NULL,
	fldBankCode varchar(20) NOT NULL 
)ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for tblTaskBandLimit 
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblTaskBandLimit]
CREATE TABLE [dbo].[tblTaskBandLimit](
	[fldTaskId] [int] NOT NULL,
	[fldOnUsId] [int] NULL,
	[fldBandLBound] [float] NULL,
	[fldBandUBound] [float] NULL,
	[fldVolumnPercentage] [int] NULL,
	[fldIsLastBand] [char](1) NULL,
	[fldActive] [char](1) NULL,
	[fldEntityCode] [varchar](20) NOT NULL,
	[fldBankCode] [varchar](20) NOT NULL
)ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [[tblDashboardConfig]] 
----------------------------------------------------------------------------------------
DROP TABLE [tblDashboardConfig]
CREATE TABLE [dbo].[tblDashboardConfig](
	[ParentTaskId] [varchar](10) NULL,
	[TaskId] [varchar](10) NOT NULL,
	[DatabaseViewId] [varchar](50) NULL,	
	[Title] [varchar](100) NULL,
	[DivId] [varchar](20) NULL,	
	[DivWidth] [varchar](5) NULL,
	[widgetType] [varchar](20) NULL,
	[IsEnabled] [varchar](1) NULL,
	[Ordering] [int] NULL,
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblReportPageConfig] 
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblReportPageConfig]
CREATE TABLE [dbo].[tblReportPageConfig](
	[ReportId] [varchar](10) NULL,
	[TaskId] [varchar](10) NOT NULL,
	[DatabaseViewId] [varchar](50) NULL,
	[ReportPath] [varchar](100) NULL,
	[ReportTitle] [varchar](200) NULL,		
	[DataSetName] [varchar](50) NULL,	
	[exportFileName] [varchar](200) NULL,	
	[PrintReportParam] [varchar](50) NULL,		
	[Orientation] [varchar](50) NULL,	
	[IsEnabled] [varchar](1) NULL,
	[Ordering] [int] NULL,
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblHostFileConfig]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblHostFileConfig]
CREATE TABLE [dbo].[tblHostFileConfig](
	[fldTaskId] [varchar](10) NOT NULL,
	[fldSystemProfileCode] [varchar](200) NULL,
	[fldFileExt] [varchar](10) NULL,
	[fldHostFileDesc] [varchar](200) NULL,
	[fldProcessName] [varchar](100) NULL,
	[fldPosPayType] [varchar](50) NULL,
	[fldTaskRole] [varchar](50) NULL,
	[fldFTPFolder] [varchar](255) NULL,
	[fldEnable] [varchar](50) NULL,
	[fldEntityCode] [varchar](20) NOT NULL,
	[fldBankCode] [varchar](20) NOT NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblMICRImportConfig]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblMICRImportConfig]
CREATE TABLE [dbo].[tblMICRImportConfig](
	[fldId] [int] NULL,
	[fldTaskId] [varchar](10) NOT NULL,
	[fldSystemProfileCode] [varchar](200) NULL,
	[fldDateSubString] [int] NULL,
	[fldBankCodeSubString] [int] NULL,
	[fldFileExt] [varchar](10) NULL,
	[fldProcessName] [varchar](100) NULL,
	[fldPosPayType] [varchar](100) NULL,
	[fldEnable] [varchar](50) NULL,
	[fldEntityCode] [varchar](20) NOT NULL,
	[fldBankCode] [varchar](20) NOT NULL
) ON [PRIMARY]
GO
----------------------------------------------------------------------------------------
--Create table for [tblBankHostStatusMaster]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblBankHostStatusMaster]
CREATE TABLE [dbo].[tblBankHostStatusMaster](
	[fldPrimaryID] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[fldStatusID] [varchar](5) NOT NULL,
	[fldStatusDesc] [varchar](100) NOT NULL,
	[fldAutoPending] [int] NULL,
	[fldAutoReject] [int] NULL,
	[fldRejectCode] [char](10) NULL,
	[fldAction] [nchar](1) NULL,
	[fldForceDebit] [nchar](1) NULL,
	[fldForProtected] [nchar](1) NULL,
	[fldStatusType] [nchar](10) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldReprocess1] [nchar](1) NULL,
	[fldReprocess2] [nchar](1) NULL,
	[fldEntityCode] [varchar](20) NOT NULL,
	[fldBankCode] [varchar](20) NOT NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblChequeActivation]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblChequeActivation]
CREATE TABLE [dbo].[tblChequeActivation](
	[fldActivationId] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[fldKLActivation] [char](2) NULL,
	[fldJBActivation] [char](2) NULL,
	[fldPPActivation] [char](2) NULL,
	[fldBPCPPActivation] [char](2) NULL,
	[fldBPCJBActivation] [char](2) NULL,
	[fldBPCKLActivation] [char](2) NULL,
	[fldClearDate] [datetime] NOT NULL,
	[fldCreateUserId] [int] NOT NULL,
	[fldCreateTimestamp] [datetime] NOT NULL,
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimestamp] [datetime] NOT NULL,
	[fldBankCode] [nchar](3) NULL
 CONSTRAINT [PK_tblChequeActivation] PRIMARY KEY CLUSTERED 
(
	[fldActivationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblInwardItemSigHistoryImage]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblInwardItemSigHistoryImage]
CREATE TABLE [dbo].[tblInwardItemSigHistoryImage](
	[fldInwardItemId] [int] NULL,
	[fldImageNo] [varchar](5) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblScheduledHistory]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblScheduledHistory]
CREATE TABLE [dbo].[tblScheduledHistory](
	[fldScheduledHistoryID] [bigint] IDENTITY(1,1) NOT NULL,
	[fldScheduledTimeID] [bigint] NULL,
	[fldScheduledTaskName] [varchar](100) NULL,
	[fldScheduledProcessName] [varchar](50) NULL,
	[fldScheduledPosPayType] [varchar](50) NULL,
	[fldClearDate] [datetime] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldRemarks] [varchar](500) NULL,
 CONSTRAINT [PK_tblScheduledHistory] PRIMARY KEY CLUSTERED 
(
	[fldScheduledHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Create table for [tblScheduledTask]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblScheduledTask]
CREATE TABLE [dbo].[tblScheduledTask](
	[fldScheduledTaskID] [bigint] IDENTITY(1,1) NOT NULL,
	[fldScheduledTaskName] [nchar](100) NULL,
	[fldProcessName] [varchar](50) NULL,
	[fldPosPayType] [varchar](50) NULL,
	[fldIsShowed] [bit] NULL
) ON [PRIMARY]
Go


----------------------------------------------------------------------------------------
--Create table for [tblScheduledTimer]
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblScheduledTimer]
CREATE TABLE [dbo].[tblScheduledTimer](
	[fldScheduledTimeID] [bigint] IDENTITY(1,1) NOT NULL,
	[fldScheduledTaskName] [varchar](100) NULL,
	[fldScheduledProcessName] [varchar](50) NULL,
	[fldScheduledPosPayType] [varchar](50) NULL,
	[fldScheduledHours] [char](2) NULL,
	[fldScheduledMins] [char](2) NULL,
	[fldCreateUserId] [bigint] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldLastRunTimeStamp] [datetime] NULL,
) ON [PRIMARY]
GO
----------------------------------------------------------------------------------------
--Create table for [tblPullOutReason]
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblPullOutReason]
CREATE TABLE [dbo].[tblPullOutReason](
	[fldPullOutID] [int] NOT NULL,
	[fldPullOutReason] [varchar](100) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO
----------------------------------------------------------------------------------------
--bank master temp for approval
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblBankMasterTemp]
CREATE TABLE [dbo].[tblBankMasterTemp](
	[fldBankCode] [char](3) NOT NULL,
	[fldBankDesc] [varchar](100) NOT NULL,
	[fldBankIndicator] [char](2) NOT NULL,
	[fldBankShortName] [varchar](30) NULL,
	[fldParentBankCode] [char](2) NULL,
	[fldActive] [char](1) NOT NULL,
	[fldCreateUserId] [int] NOT NULL,
	[fldCreateTimeStamp] [datetime] NOT NULL,
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimeStamp] [datetime] NOT NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--bank host status master temp for approval
----------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblBankHostStatusMasterTemp]
CREATE TABLE [dbo].[tblBankHostStatusMasterTemp](
	[fldPrimaryID] [int] NULL,
	[fldStatusID] [varchar](5) NOT NULL,
	[fldStatusDesc] [varchar](100) NOT NULL,
	[fldAutoPending] [int] NULL,
	[fldAutoReject] [int] NULL,
	[fldRejectCode] [char](10) NULL,
	[fldAction] [nchar](1) NULL,
	[fldForceDebit] [nchar](1) NULL,
	[fldForProtected] [nchar](1) NULL,
	[fldStatusType] [nchar](10) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldReprocess1] [nchar](1) NULL,
	[fldReprocess2] [nchar](1) NULL,
	[fldEntityCode] [varchar](20) NOT NULL,
	[fldBankCode] [varchar](20) NOT NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--state master temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblStatemasterTemp]
CREATE TABLE [dbo].[tblStatemasterTemp](
	[fldStateCode] [char](2) NOT NULL,
	[fldStateDesc] [varchar](100) NOT NULL,
	[fldAccType] [char](1) NOT NULL,
	[fldCreateUserId] [int] NOT NULL,
	[fldCreateTimeStamp] [datetime] NOT NULL,
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimeStamp] [datetime] NOT NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--branch master temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblBranchMasterTemp]
CREATE TABLE [dbo].[tblBranchMasterTemp](
	[fldBranchId] [char](4) NULL,
	[fldBranchCode] [char](4) NULL,
	[fldBranchDesc] [varchar](100) NULL,
	[fldBankCode] [char](3)  NULL,
	[fldAccType] [char](2) NULL,
	[fldStateCode] [char](2)  NULL,
	[fldSpickCode] [char](3) NULL,
	[fldDisable] [char](1) NULL,
	[fldCreateUserId] [int]  NULL,
	[fldCreateTimeStamp] [datetime]  NULL,
	[fldUpdateUserId] [int]  NULL,
	[fldUpdateTimeStamp] [datetime]  NULL,
	[fldTel] [nchar](20) NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--Trans master temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblTransMasterTemp]
CREATE TABLE [dbo].[tblTransMasterTemp](
	[fldTransCode] [char](3) NOT NULL,
	[fldTransDesc] [varchar](120) NOT NULL,
	[fldCreateUserId] [int] NOT NULL,
	[fldCreateTimeStamp] [datetime] NOT NULL,
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimeStamp] [datetime] NOT NULL,
	[fldApproveStatus] [varchar](1) NULL
 ) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--map branch master temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblMapBranchTemp]
CREATE TABLE [dbo].[tblMapBranchTemp](
	[fldMapBranchId] [int] IDENTITY(1,1) NOT NULL,
	[fldAccountID] [int] NULL,
	[fldStateCode] [char](2) NULL,
	[fldStateDesc] [nvarchar](100) NULL,
	[fldSpickCode] [nchar](3) NULL,
	[fldBranchDesc] [nvarchar](100) NULL,
	[fldBranchCode] [char](4) NULL,
	[fldBranchAbb] [char](4) NULL,
	[fldIBranchCode] [char](5) NULL,
	[fldIBranchAbb] [char](4) NULL,
	[fldConBranchCode] [nvarchar](9) NULL,
	[fldIsBranchCode] [char](7) NULL,
	[fldIsBranchCode2] [char](5) NULL,
	[Address1] [nvarchar](100) NULL,
	[Address2] [nvarchar](100) NULL,
	[Address3] [nvarchar](100) NULL,
	[fldSubZone] [char](5) NULL,
	[fldAccountNumber] [char](12) NULL,
	[fldApprovedCode] [char](1) NULL,
	[fldEmailAddress] [nvarchar](100) NULL,
	[fldSOL] [char](4) NULL,
	[fldEntityCode] [varchar](20) NULL,
	[fldBankCode] [varchar](20) NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--verification limit temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblVerificationBatchSizeLimitTemp]
CREATE TABLE [dbo].[tblVerificationBatchSizeLimitTemp](
	[fldClass] [char](1) NOT NULL,
	[fld1stAmt] [float] NULL,
	[fld2ndAmt] [float] NULL,
	[fld1stType] [varchar](2) NULL,
	[fld2ndType] [varchar](2) NULL,
	[fldConcatenate] [char](10) NULL,
	[fldLimitDesc] [varchar](50) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--verification limit temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblVerificationBatchSizeLimitTemp]
CREATE TABLE [dbo].[tblVerificationBatchSizeLimitTemp](
	[fldClass] [char](1) NOT NULL,
	[fld1stAmt] [float] NULL,
	[fld2ndAmt] [float] NULL,
	[fld1stType] [varchar](2) NULL,
	[fld2ndType] [varchar](2) NULL,
	[fldConcatenate] [char](10) NULL,
	[fldLimitDesc] [varchar](50) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--pull out reason temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblPullOutReasonTemp]
CREATE TABLE [dbo].[tblPullOutReasonTemp](
	[fldPullOutID] [int] NULL,
	[fldPullOutReason] [varchar](100) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------
--reject master temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblRejectMasterTemp]
CREATE TABLE [dbo].[tblRejectMasterTemp](
	[fldRejectCode] [char](3) NOT NULL,
	[fldRejectDesc] [varchar](100) NOT NULL,
	[fldVerifyFlag] [char](1) NULL,
	[fldType] [varchar](100) NULL,
	[fldPriority] [int] NULL,
	[fldChargesRIE] [float] NULL,
	[fldChargesCR] [float] NULL,
	[fldCharges] [float] NULL,
	[fldPCHCCode] [nchar](3) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NOT NULL,
	[fldUpdateUserId] [int] NOT NULL,
	[fldUpdateTimeStamp] [datetime] NOT NULL,
	[fldCQType] [char](1) NULL,
	[fldUnposted] [tinyint] NULL,
	[fldTechnicalVerify] [nchar](1) NULL,
	[fldApproveStatus] [varchar](1) NULL
	) ON [PRIMARY]

GO

----------------------------------------------------------------------------------------
--Transaction Type Temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblCTCSTransactionTypeTemp]
CREATE TABLE [dbo].[tblCTCSTransactionTypeTemp](
	[fldTransactionType] [nchar](2) NULL,
	[fldTransactionDesc] [nvarchar](50) NULL,
	[fldActive] [nchar](1) NULL,
	[fldCreateUserId] [int] NULL,
	[fldCreateTimeStamp] [datetime] NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]

GO

----------------------------------------------------------------------------------------
--Threshold Setting Temp for approval
---------------------------------------------------------------------------------------
DROP TABLE [dbo].[tblThresholdSettingTemp]
CREATE TABLE [dbo].[tblThresholdSettingTemp](
	[fldId] [int] NULL,
	[fldType] [varchar](50) NULL,
	[fldSequence] [int] NULL,
	[fldTitle] [varchar](255) NULL,
	[fldAmount] [float] NULL,
	[fldEnable] [varchar](1) NULL,
	[fldUpdateUserId] [int] NULL,
	[fldUpdateTimeStamp] [datetime] NULL,
	[fldEntityCode] [varchar](20) NULL,
	[fldBankCode] [varchar](20) NULL,
	[fldApproveStatus] [varchar](1) NULL
) ON [PRIMARY]

GO
