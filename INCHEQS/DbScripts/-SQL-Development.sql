
----------------------------------
-- MIRC IMPORT
----------------------------------
select * from tbldataprocess
select * from tblInwardClearingFile
select * from tblinwardcleardate
select * from tblInwardItemInfoStatus
select * from tblInwardItemInfo
select * from tblInwardClearingItem
select * from tblEHR
select * from tblBHR
select * from tblCDA
select * from tblIVT
select * from tblCCR
select * from View_GenerateDebitFile
--purge
delete from tbldataprocess
delete from tblMTFRecord
delete from tblMTFFile
delete from tblinwardcleardate
delete from tblInwardClearingFile
delete from tblInwardItemInfoStatus
delete from tblInwardItemInfo
delete from tblInwardClearingItem
delete from tblPendingInfo
delete from tblEHR
delete from tblBHR
delete from tblCDA
delete from tblIVT
delete from tblCCR
delete FROM tblFileManager
--PURGE
delete FROM tblUserSessionTrack
----------------------------------
-- UPDATE 5 Records REJECT REENTRY FOR DEVELOPEMENT
----------------------------------
UPDATE tblInwardItemInfo set fldAccountNumber = '000000000000' where fldInwardItemId IN (select top(1) fldInwardItemId from tblInwardItemInfo WHERE fldInwardItemId NOT IN(SELECT fldInwardItemId FROM View_RejectReentryMaker)) 
UPDATE tblInwardItemInfo set fldChequeSerialNo = '0000000000' where fldInwardItemId IN (select top(1) fldInwardItemId from tblInwardItemInfo WHERE fldInwardItemId NOT IN(SELECT fldInwardItemId FROM View_RejectReentryMaker)) 
UPDATE tblInwardItemInfoStatus set fldRejectstatus1 = '02' where fldInwardItemId IN (select top(1) fldInwardItemId from tblInwardItemInfo WHERE fldInwardItemId NOT IN(SELECT fldInwardItemId FROM View_RejectReentryMaker)) 
UPDATE tblInwardItemInfoStatus set fldRejectstatus1 = '04' where fldInwardItemId IN (select top(1) fldInwardItemId from tblInwardItemInfo WHERE fldInwardItemId NOT IN(SELECT fldInwardItemId FROM View_RejectReentryMaker)) 
UPDATE tblInwardItemInfoStatus set fldRejectstatus1 = '05' where fldInwardItemId IN (select top(1) fldInwardItemId from tblInwardItemInfo WHERE fldInwardItemId NOT IN(SELECT fldInwardItemId FROM View_RejectReentryMaker)) 
----------------------------------
-- VERIFICATION EDIT IN VIEW
----------------------------------
SELECT fldAccountNumber, fldTransCode, fldNonConformance, fldUpdateTimeStamp, fldApprovalStatus, fldApprovalUserId, fldApprovalUserClass, fldApprovalTimeStamp, 
fldApprovalIndicator, fldMSRejected
FROM  view_inwarditem
----------------------------------
-- NULL all item
----------------------------------
UPDATE tblInwardItemInfoStatus SET fldApprovalStatus = null
----------------------------------
-- 1 million test record
----------------------------------
;WITH q 
(n) AS (
   SELECT 1
   UNION ALL
   SELECT n + 1
   FROM   q
   WHERE  n < 1123456
)
INSERT INTO tblBankHostStatusMaster (fldStatusId, fldStatusDesc,fldEntityCode, fldBankCode, fldAutoReject,fldAutoPending)
select n,n,n,n,n,n from q
OPTION (MAXRECURSION 0)

----------------------------------
-- FLOW
----------------------------------
--Flow
--1. import ICL (mirc import)
--2. Service run (DownloadImport)
--3. Verify 
--	a. Manual (View_inwardItem)
--		i.		fldAccountNo cannot all zeros (0001757100010000)
--		ii.		fldTransCode (10)
--		iii.	fldNonConformance (0)
--		iv. 	fldUpdateTimeStamp (now)
--		v. 		fldApprovalStatus (A)
--		vi.		fldApprovalUserId (1)
--		vii.	fldApprovalUserClass (A)
--		viii.	fldApprovalTimestamp (now)
--		ix.		fldApprovalIndicator (Y)
--		x.		fldRejectCode1 -to- fldRejectCode6 (00)
--		xi.		fldMsRejected (0)
--	b. Manual Secritny - inside verification screen (Click Realize by maker)
--	c. Verification (first authorizer) (Click Realize by checker)
--	d. Verification (second authorizer) (Click Realize by second checker)
--4. Generate Debit File (after verification)
--5. Check generated file in (ICCS Data > Debit File)
--6. Load Bank Host file HREJ1
--7. Generate HREP1
--follow host file

----------------------------------
-- CHECK ACTIVE CONNECTION
----------------------------------

SELECT 
    DB_NAME(dbid) as DBName, 
    COUNT(dbid) as NumberOfConnections,
    loginame as LoginName
FROM
    sys.sysprocesses
WHERE 
    dbid > 0
GROUP BY 
    dbid, loginame
;

----------------------------------
-- ABL ROLLBACK
----------------------------------
ALTER DATABASE ABL_INCHEQS_ICS
SET OFFLINE WITH ROLLBACK IMMEDIATE

ALTER DATABASE ABL_INCHEQS_ICS
SET ONLINE

----------------------------------
-- CHANGE IMAGE PATH WITHOUT RUNNING SERVICE IMPORT
----------------------------------
update tblinwarditeminfo set fldimagefolder = REPLACE(fldimagefolder, 'D:\', 'C:\')

----------------------------------
-- INSERT FILE MANAGER SCRIPT FOR TESTING GENERATE HOST FILE
----------------------------------
DELETE from [tblFileManager]
INSERT [dbo].[tblFileManager] ([fldTaskId], [fldClearDate], [fldFilePath], [fldFileName], [fldCreateUserId], [fldCreateTimestamp], [fldRemarks], [fldSend],[fldBankCode]) VALUES (309120, N'2016-05-23 00:00:00.000', N'D:\\BankPBM\\HostFile', N'HREP1-001-20151112-01401-NC-02-00002', N'1', N'2016-05-23 00:00:00.000', N'Testing Remarks', N'N',N'029')
GO

INSERT [dbo].[tblFileManager] ([fldTaskId], [fldClearDate], [fldFilePath], [fldFileName], [fldCreateUserId], [fldCreateTimestamp], [fldRemarks], [fldSend],[fldBankCode]) VALUES (309121, N'2016-05-23 00:00:00.000', N'D:\\BankPBM\\HostFile', N'HREP1-001-20151112-01401-NC-02-00002', N'1', N'2016-05-23 00:00:00.000', N'Testing Remarks', N'N',N'029')
GO
