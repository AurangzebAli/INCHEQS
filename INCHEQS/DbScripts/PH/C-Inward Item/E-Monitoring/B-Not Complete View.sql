WITH MasterTable AS (
select a.fldClearDate,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldInwardItemId END
)AS RecordExist1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldApprovalStatus END
)AS Status1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldNonConfirmStatus END
)AS NonConfirm1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN p.fldApprovalStatusPending END
)AS BranchStatus1

FROM View_AppInwardItem a LEFT OUTER JOIN
View_AppPendingData p on a.fldInwardItemId = p.fldInwardItemId
)

SELECT
fldClearDate,
(SELECT tm.fldMenuTitle +' '+ STR(bl.fldBandLBound,12,2) +' < '+ STR(bl.fldBandUBound,12,2) FROM tblTaskMaster tm LEFT JOIN tblTaskBandLimit bl ON tm.fldTaskId = bl.fldTaskId WHERE tm.fldTaskId = '6450') AS QueueName,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' THEN 1 ELSE 0 END) as Approved,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'R' THEN 1 ELSE 0 END) as Returned,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' OR Status1 = 'R' THEN 1 ELSE 0 END) as TotalCompleted,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' OR Status1 = 'R' THEN 1 ELSE 0 END) as ReadyToHost,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryMaker WHERE fldInwardItemId IN (SELECT fldInwardItemId FROM MasterTable WHERE RecordExist1 IS NOT NULL)) as RejectReentryMakerOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'H' OR Status1 = 'J' THEN 1 ELSE 0 END) as RejectReentryCheckerOutstanding,

sum(case WHEN RecordExist1 IS NOT NULL AND (Status1 IS NULL) THEN 1 ELSE 0 END) as FirstVerificationOutstanding,

sum(case WHEN RecordExist1 IS NOT NULL AND NonConfirm1 = 'A' THEN 1 ELSE 0 END) as SecondVerificationOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'B' THEN 1 ELSE 0 END) as BranchMakerOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND BranchStatus1 = 'H' THEN 1 ELSE 0 END) as BranchCheckerApproveOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND BranchStatus1 = 'J' THEN 1 ELSE 0 END)BranchCheckerReturnOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL THEN 1 ELSE 0 END) AS TotalItem
FROM MasterTable 
group by fldClearDate

-------------------------------------------------------------------------------------------------------------------------------------------------
---by fadzuan---
--PROBLEM--
--1stVerificationOutstanding + RejectRenntryCheckerOutstanding
--TotalItem count

WITH MasterTable AS (
select a.fldClearDate,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldInwardItemId END
)AS RecordExist1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldApprovalStatus END
)AS Status1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN a.fldNonConfirmStatus END
)AS NonConfirm1,
(case when 
(a.fldAmount >= (SELECT b.fldBandLBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) AND 
(a.fldAmount <= (SELECT b.fldBandUBound FROM dbo.tblTaskBandLimit b WHERE b.fldTaskId = '6450')) 
THEN p.fldApprovalStatusPending END
)AS BranchStatus1

FROM View_AppInwardItem a LEFT OUTER JOIN
View_AppPendingData p on a.fldInwardItemId = p.fldInwardItemId
)

SELECT
fldClearDate,
(SELECT tm.fldMenuTitle +' '+ STR(bl.fldBandLBound,12,2) +' < '+ STR(bl.fldBandUBound,12,2) FROM tblTaskMaster tm LEFT JOIN tblTaskBandLimit bl ON tm.fldTaskId = bl.fldTaskId WHERE tm.fldTaskId = '6450') AS QueueName,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' THEN 1 ELSE 0 END) as Approved,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'R' THEN 1 ELSE 0 END) as Returned,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' OR Status1 = 'R' THEN 1 ELSE 0 END) as TotalCompleted,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'A' OR Status1 = 'R' THEN 1 ELSE 0 END) as ReadyToHost,

(SELECT Count(fldInwardItemId) FROM View_RejectReentryMaker WHERE fldInwardItemId IN (SELECT RecordExist1 FROM MasterTable WHERE RecordExist1 IS NOT NULL)) as RejectReentryMakerOutstanding,
(SELECT Count(fldInwardItemId) FROM View_RejectReentryChecker WHERE fldInwardItemId IN (SELECT RecordExist1 FROM MasterTable WHERE RecordExist1 IS NOT NULL)) as RejectReentryCheckerOutstanding,

(SELECT Count(fldInwardItemId) FROM View_Verification1st WHERE fldInwardItemId IN (SELECT RecordExist1 from MasterTable where RecordExist1 is not null)) as FirstVerificationOutstanding,


sum(case WHEN RecordExist1 IS NOT NULL AND NonConfirm1 = 'A' THEN 1 ELSE 0 END) as SecondVerificationOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND Status1 = 'B' THEN 1 ELSE 0 END) as BranchMakerOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND BranchStatus1 = 'H' THEN 1 ELSE 0 END) as BranchCheckerApproveOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL AND BranchStatus1 = 'J' THEN 1 ELSE 0 END)BranchCheckerReturnOutstanding,
sum(case WHEN RecordExist1 IS NOT NULL THEN 1 ELSE 0 END) AS TotalItem
FROM MasterTable 
group by fldClearDate

--------------------------------------------------------------------------------------------------------------------------------------------------





