--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @bank varchar(100) = '009';
DECLARE @spikeCode varchar(100) = 'PHP';

------------------------------------------------------------------------
-- Delete and add user and task data from database 
------------------------------------------------------------------------
DELETE FROM tblUserMaster
DBCC CHECKIDENT ('tblUserMaster', RESEED, 0);
DELETE FROM tblGroupUser
DBCC CHECKIDENT ('tblGroupUser', RESEED, 0);
DELETE FROM tblGroupMaster
DELETE FROM tblUserPwdHistory
------------------------------------------------------------------------
-- ADD ADMIN
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'ADMIN', N'SUPERADMIN', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'ADMIN',N'SUPER ADMIN',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'ADMIN',1,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD OP ADMIN
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'OPADMIN', N'OPERATION ADMIN', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'OPADMIN',N'OPERATION ADMIN',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'OPADMIN',2,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD RRCHECKER1
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'RRCHECKER1', N'REJECT REENTRY CHECKER 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'RRCHECKER',N'REJECT REENTRY CHECKER',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'RRCHECKER',3,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD RRCHECKER1
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'RRMAKER1', N'REJECT REENTRY MAKER 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'RRMAKER',N'REJECT REENTRY MAKER',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'RRMAKER',4,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD SUPERVISIOR
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'CCUSUPERVISIOR1', N'CCU SUPERVISIOR 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'SPVISIOR',N'CCU SUPER VISIOR',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'SPVISIOR',5,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD VERIFIER 1
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'CCUVERIFIER1', N'CCU VERIFIER 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'VER1',N'VERIFIER 1',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'VER1',6,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD VERIFIER 2
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'CCUVERIFIER2', N'CCU VERIFIER 2', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'VER2',N'VERIFIER 2',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'VER2',7,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD VERIFIER 3
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'CCUVERIFIER3', N'CCU VERIFIER 3', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'VER3',N'VERIFIER 3',@bank,0,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'VER3',8,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD BRANCH USER
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'BRANCHUSER1', N'BRANCH USER 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'0102800', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'BRANCHUSER',N'BRANCH USER',@bank,1,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'BRANCHUSER',9,1,GETDATE(),1,GETDATE())
------------------------------------------------------------------------
-- ADD BRANCH OFFICER
------------------------------------------------------------------------
INSERT INTO tblUserMaster (fldUserAbb, fldUserDesc, fldEmail, fldAppRight, fldBankCode, fldBankCodei, fldSpickCode, fldAdminFlag, fldLoginIP1, fldPassword, fldDisableLogin, fldCounter, fldVerifyChequeFlag, fldBranchCode, fldVerificationLimit, fldCreateUserId, fldCreateTimeStamp, fldUpdateUserId, fldUpdateTimeStamp,fldPassLastUpdDate, fldCity, fldApproveStatus, fldApproveBranchCode, fldPasswordExpDate,fldVerificationClass,fldIDExpStatus,fldIDExpDate) VALUES (
N'BRANCHOFF1', N'BRANCH OFFICER 1', N'', 1, @bank, @bank, @spikeCode, N'Y', N'', N'4STJKZhIiKmJaXjCPB8hIw==', N'N', 0, 0, N'0102800', 50, 1, GETDATE(), 1, GETDATE(),GETDATE(), NULL, NULL, N'Y', N'3015-03-22 00:00:00.000',N'A',0,N'3015-03-22 00:00:00.000'
)
INSERT INTO tblGroupMaster (fldGroupId,fldGroupDesc,fldBankCode,fldBranchGroup, fldSpickCode,fldCreateUserId, fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) VALUES(N'BRANCHOFF',N'BRANCH OFFICER',@bank,1,@spikeCode,1,GETDATE(),1,GETDATE())
INSERT INTO tblGroupUser (fldGroupId,fldUserId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp) Values (
N'BRANCHOFF',10,1,GETDATE(),1,GETDATE())





GO