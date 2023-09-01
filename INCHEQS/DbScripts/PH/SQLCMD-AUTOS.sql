-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
--AUTO SCRIPT.. NEED TO CHANGE DIRECTORY
--USE SQLCMD MODE (SQL MANAGEMENT STUDIO > TOP MENU > QUERY > SQLCMDMODE)
-----------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------
--:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\A-Configuration\A-Initial TableData.sql"
--:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\A-Configuration\B-Directory Data.sql"
--:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\A-Configuration\C-User Data (sample).sql"
--:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\A-Configuration\D-Task-Data.sql"
--:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\A-Configuration\E-Views.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\ApproveMaintenance.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\Archive.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\AuditLog.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\BandQueueSetting.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\BankCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\BranchCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\CMSAccountInfo.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\GLReplacement.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\Group.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\HighRiskAccount.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\HostReturnReasonCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\InternalBranch.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\PullOutReason.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\ReleaseLockedCheque.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\ReturnCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\StateCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\TaskAssignment.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\ThresholdSetting.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\TransactionCode.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\TransactionType.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\User.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\B-Maintenance And Utility\VerificationLimit.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\A-Import\A1-MICRImport(ICL).sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\A-Import\A2-MICRImport(ECCS).sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\B-Reject Reentry\B1-RejectReentryMaker.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\B-Reject Reentry\B2-RejectReentryChecker.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\C1-Verification.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\C2-Verification 1st.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\C3-Verification 2nd.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\C4-Verification 3rd.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\D1-Band1.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\D2-Band2.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\D3-Band3.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\D4-Band4.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\E1-ReviewMine.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\E2-PullOutData.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\E3-ApprovedItem.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\E4-InwardRejected.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\E5-PendingBranchConfirmation.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\F1-ApproveAll.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\C-Verification\F2-ReturnAll.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\D-Host File\A1-Generate Debit File.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\D-Host File\A2-Load Host HREJ.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\D-Host File\A3-Generate HREP File.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\E-Monitoring\A-Progress Status.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\F-Branch Activation\A-BranchActivation.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\G-Outward Return\A1-Generate Outward Return.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\G-Outward Return\A2-Generate CTTUM File.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\C-Inward Item\H-Service Activate\A1-SOA Service.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\D-Branch\A-PendingDataMaker.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\D-Branch\B-PendingDataChecker.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\D-Branch\C-ReviewMineBranch.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\E-SearchCheques\SearchCheques.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\F-Dashboard\A-Dashboard Online User.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\F-Dashboard\B-Dasboard Progress Status.sql"

:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Free Item Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Host Status Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\InwardClearingChequeSummaryReport.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\LargeAmount.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Match Item Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Missing Item Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\NowListReport.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\PendingBranchDecisionReport.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\PrintCheque.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Rejected Inward Clearing Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Rejected Items Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\ReturnItemduetoTechnicalDefect.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\Summary of Return Item Report.sql"
:r "D:\incheqs.ics.v4\INCHEQS\DbScripts\PH\G-Report\VerificationSummaryReport.sql"
