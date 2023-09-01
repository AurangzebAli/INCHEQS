--**************************************************************************
--**************************************************************************
-- REQUIRED VIEW
--**************************************************************************
--**************************************************************************

------------------------------------------------------------------------
-- [View_ChequeHistory]
------------------------------------------------------------------------

DROP VIEW [dbo].[View_ChequeHistory]
GO
CREATE VIEW [dbo].[View_ChequeHistory]
AS
SELECT   DISTINCT     ch.fldRejectCode, rm.fldRejectDesc, um.fldUserAbb, ch.fldCreateUserId, ch.fldInwardItemId, ch.fldCreateTimeStamp,				ch.fldRemarks,ch.fldTextAreaRemarks,ch.fldActionStatus,

				(select fldMenuTitle from tblTaskMaster where fldTaskId = ch.fldQueue) Module,		

				CASE WHEN ch.fldActionStatus = 'A' THEN 'Approve' 
				WHEN ch.fldActionStatus = 'R' THEN 'Return'
				WHEN ch.fldActionStatus = 'B' THEN 'Route To Branch' 
				WHEN ch.fldActionStatus = 'D' THEN 'RR Maker' 
				WHEN ch.fldActionStatus = 'F' THEN 'RR Checker' 
				WHEN ch.fldActionStatus = 'E' THEN 'RR Approve' 
				WHEN ch.fldActionStatus = 'G' THEN 'Branch (Checker Approve)' 
				WHEN ch.fldActionStatus = 'H' THEN 'Branch (Maker Approve)' 
				WHEN ch.fldActionStatus = 'I' THEN 'Branch (Checker Return)'
				WHEN ch.fldActionStatus = 'J' THEN 'Branch (Maker Return)' 
				WHEN ch.fldActionStatus = 'K' THEN 'Branch (Checker Refer Back)' 
				WHEN ch.fldActionStatus = 'L' THEN 'RR From Verification' 
						
				END AS fldActionStatusDesc

FROM			dbo.tblInwardItemHistory AS ch WITH (NOLOCK) INNER JOIN
				dbo.tblUserMaster AS um ON ch.fldCreateUserId = um.fldUserId LEFT OUTER JOIN
				dbo.tblPullOutInfo AS po ON po.fldInwardItemId = ch.fldInwardItemId AND ch.fldActionStatus = 'P' LEFT OUTER JOIN
				dbo.tblRejectMaster AS rm ON rm.fldRejectCode = ch.fldRejectCode COLLATE SQL_Latin1_General_CP1_CS_AS

GO

------------------------------------------------------------------------
-- [[View_AppInwardItem]]
------------------------------------------------------------------------

DROP VIEW [dbo].[View_AppInwardItem]
GO
CREATE VIEW [dbo].[View_AppInwardItem]
AS
SELECT     DISTINCT  
				a.fldInwardItemId, a.fldUIC, a.fldChequeSerialNo, a.fldPreBankType, a.fldPreBankCode, a.fldPreStateCode, a.fldPreBranchCode, a.fldIssueBankType, a.fldIssueBankCode, a.fldIssueStateCode, a.fldIssueBranchCode, a.fldAccountNumber, a.fldTransCode, a.fldAmount, a.fldImageFolder, a.fldImageFileName, a.fldClearDate, a.fldSpickCode, a.fldNonConformance, a.fldNonConformance2, a.fldIQA, a.fldDocToFollow, a.fldChequeType, a.fldCreateUserID, a.fldCreateTimeStamp, a.fldCurrency, a.fldCheckDigit, a.fldDSStatus, a.fldImageDS, a.fldMICRDS, a.fldDBatchID, a.fldCBatchID, a.fldPreDigit, a.fldIssueDigit, a.fldImageIndicator, a.fldDIN, a.fldUpdateUserID, a.fldUpdateTimeStamp, a.fldFromDP, a.fldPrePayeeAccNo, a.fldOriCheckDigit, a.fldOriChequeSerialNo, a.fldOriIssueBankCode, a.fldOriIssueStateCode, a.fldOriIssueBranchCode, a.fldOriIssueDigit, a.fldOriTransCode, a.fldOriAccountNumber, a.fldCICS, a.fldPayBranchCode,a.fldPresentingBankItemSequenceNumber,a.fldIssueStateCode + a.fldIssueBankCode + a.fldIssueBranchCode AS fldBranchCode,
				 
				b.fldHostAccountNo, b.fldAccountHolderName, b.fldNonConfirmUserID, b.fldNonConfirmUserClass, b.fldNonConfirmTimeStamp, b.fldModifiedFields, b.fldMatchResult, b.fldApprovalStatus, b.fldApprovalUserId, b.fldApprovalUserClass, b.fldApprovalTimeStamp, b.fldApprovalIndicator, b.fldAssignedUserId, b.fldCharges, b.fldRejectStatus1, b.fldRejectStatus2, b.fldRejectStatus3, b.fldRejectStatus4, b.fldRejectCode, b.fldRemarks, b.fldUPIGenerated, b.fldUPIDate, b.fldHostDebit, b.fldOldAccountNo, b.fldOldChequeNo, b.fldCustomerConfirm, b.fldNonConfirmStatus, b.fldRRstatus, b.fldCompleted, b.fldMatchStatus, b.fldBatchNo, b.fldBranchODReview, b.fldODReview, b.fldUpdateUserID AS fldUpdateUserIDStatus, b.fldUpdateTimeStamp AS fldUpdateTimeStampStatus, b.fldMTFId, b.fldAccountStatus, b.fldHostFileName, b.fldOldDigit, b.fldOldAmount, b.fldOldCheckDigit, b.fldOldBranchCode, b.fldOldStateCode, b.fldOldBankCode, b.fldOldTransCode, b.fldSourceFile, b.fldPrePayeeAccNoApproved, b.fldFICMICExclude, b.fldFICMICStatus, b.fldORstatus, b.fldQueueType, b.fldOutwardBankUserID, b.fldORAssignedUserId, b.fldOutwardBankAcc, b.fldAssignedQueue, b.fldReviewAll, b.fldUPIType, b.fldAutoRejectRemarks, b.fldUnMatchRemarks, b.fldRejectStatus5, b.fldRejectStatus6, b.fldRejectStatus7, b.fldSOAPrinted, 

				bm.fldBankDesc AS fldPreBankDesc, 
				tm.fldTransDesc, 
				rm.fldRejectDesc, 
				um.fldUserAbb AS fldLastUpdateUserName, 
				um.fldUserId AS fldLastUpdateUserId,
				um.fldUserAbb,
				mb.fldStateDesc,
				
				CASE WHEN (b.fldrejectstatus1 <> '' AND b.fldrejectstatus1 <> 'NIL') OR
                (b.fldrejectstatus2 <> '' AND b.fldrejectstatus2 <> 'NIL') OR
                (b.fldrejectstatus3 <> '' AND b.fldrejectstatus3 <> 'NIL') OR
                (b.fldrejectstatus4 <> '' AND b.fldrejectstatus4 <> 'NIL') THEN '1' ELSE '2' END AS hoststatus, 

				CASE WHEN b.fldApprovalStatus = 'A' AND isNull(b.fldNonConfirmStatus, '') = '' THEN 'Approved' 
				WHEN b.fldApprovalStatus = 'R' AND isNull(b.fldNonConfirmStatus, '') = '' THEN 'Returned' 
				WHEN b.fldNonConfirmStatus = 'A' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Unauth. Approved' 
				WHEN b.fldNonConfirmStatus = 'R' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Unauth. Returned' 
				WHEN b.fldNonConfirmStatus = 'C' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Unauth. Approved' 
				WHEN b.fldNonConfirmStatus = 'S' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Unauth. Return' 
				WHEN b.fldNonConfirmStatus = 'P' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Pull Out' 
				WHEN b.fldNonConfirmStatus = 'B' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Branch Outstanding' 
				WHEN b.fldNonConfirmStatus = 'H' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Branch (Maker Approved)' 
				WHEN b.fldNonConfirmStatus = 'J' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Branch (Maker Returned)' 
				WHEN b.fldNonConfirmStatus = 'K' AND isNull(b.fldApprovalStatus, '') = '' THEN 'Branch (Checker Refered Back)'
				WHEN b.fldNonConfirmStatus = 'L' AND isNull(b.fldApprovalStatus, '') = '' THEN 'RR From Verification' 
				ELSE 'Outstanding' END AS fldItemStatus,

				CASE WHEN cms.fldAccountNo <> '' THEN 'CMS ACCOUNT'
				WHEN gl.fldAccountNumber <> '' THEN 'GL ACCOUNT'
				WHEN hr.fldHighRiskAccount <> '' THEN 'HIGH RISK ACCOUNT'
				END AS fldAccountDesc


FROM            dbo.tblInwardItemInfo AS a WITH (NOLOCK) INNER JOIN
                dbo.tblInwardItemInfoStatus AS b WITH (NOLOCK) ON a.fldInwardItemId = b.fldInwardItemID LEFT OUTER JOIN
                dbo.tblBankMaster AS bm WITH (NOLOCK) ON a.fldPreBankType = bm.fldBankIndicator AND a.fldPreBankCode = bm.fldBankCode LEFT OUTER JOIN
                dbo.tblTransMaster AS tm WITH (NOLOCK) ON a.fldTransCode = tm.fldTransCode LEFT OUTER JOIN
                dbo.tblUserMaster AS um WITH (NOLOCK) ON b.fldApprovalUserId = um.fldUserId LEFT OUTER JOIN
                dbo.tblRejectMaster AS rm WITH (NOLOCK) ON b.fldRejectCode = rm.fldRejectCode COLLATE SQL_Latin1_General_CP1_CS_AS LEFT OUTER JOIN
				dbo.tblMapBranch AS mb WITH(NOLOCK) ON a.fldIssueStateCode + a.fldIssueBankCode + a.fldIssueBranchCode = LEFT(mb.fldConBranchCode,8)  LEFT OUTER JOIN
				dbo.tblCMSAccountInfo AS cms WITH(NOLOCK) ON a.fldAccountNumber = cms.fldAccountNo LEFT OUTER JOIN
				dbo.tblGLReplacement AS gl WITH(NOLOCK) ON a.fldAccountNumber = gl.fldAccountNumber LEFT OUTER JOIN
				dbo.tblHighRiskAccount AS hr WITH(NOLOCK) ON a.fldAccountNumber = hr.fldHighRiskAccount

GO

--------------------------------------------------------------
--[View_Verification]
--------------------------------------------------------------
DROP VIEW [dbo].[View_Verification]
GO
CREATE VIEW [dbo].[View_Verification]
AS
SELECT DISTINCT * FROM  dbo.View_AppInwardItem
WHERE (ISNULL(fldRejectStatus1, '') <> '03') 
		AND (ISNULL(fldRejectStatus2, '') <> '03') 
		AND (ISNULL(fldRejectStatus3, '') <> '03') 
		AND (ISNULL(fldRejectStatus4, '') <> '03')
		AND (DATEDIFF(day, fldClearDate, (SELECT TOP (1) fldClearDate FROM dbo.tblInwardClearDate ORDER BY fldClearDate DESC)) = 0)
GO

------------------------------------------------------------------------
-- [View_AppPendingData]
------------------------------------------------------------------------
DROP VIEW [dbo].[View_AppPendingData]
GO
CREATE VIEW [dbo].[View_AppPendingData]
AS
SELECT  DISTINCT  a.*,
				
				b.fldPendingID, b.fldInwardItemId AS fldInwardItemIdPending, 
				b.fldApprovalStatus AS fldApprovalStatusPending, 
				b.fldCharges AS fldChargesPending, b.fldRejectCode AS fldRejectCodePending, b.fldRemarks AS fldRemarksPending, 
				b.fldApprovalUserID AS fldApprovalUserIDPending, b.fldApprovalTimeStamp AS fldApprovalTimeStampPending, 
				b.fldAssignedUserId AS fldAssignedUserIdPending, b.fldAlert, b.fldAlertReason, b.fldCreateUserID AS fldCreateUserIDPending, 
				b.fldCreateTimeStamp AS fldCreateTimeStampPending, b.fldUpdateUserID AS fldUpdateUserIDPending, (select fldUserAbb from tblUserMaster where fldUserId = b.fldUpdateUserID) AS fldLastUpdateUserNamePending, 
				b.fldUpdateTimeStamp AS fldUpdateTimeStampPending, 

                CASE WHEN b.fldApprovalStatus = 'G' THEN 'Branch (Checker Approve)' 
				WHEN b.fldApprovalStatus = 'H' THEN 'Branch (Maker Approve)' 
				WHEN b.fldApprovalStatus = 'I' THEN 'Branch (Checker Return)'
                WHEN b.fldApprovalStatus = 'J' THEN 'Branch (Maker Return)' 
                WHEN b.fldApprovalStatus = 'K' THEN 'Branch (Checker Reffered Back)' 
				ELSE 'Branch Outstanding' 
				END AS fldItemStatusPending

FROM            dbo.View_Verification AS a LEFT OUTER JOIN
                dbo.tblPendingInfo AS b ON b.fldInwardItemId = a.fldInwardItemId
WHERE        (b.fldInwardItemId IS NOT NULL)
GO

