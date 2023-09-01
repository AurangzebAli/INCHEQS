--**************************************************************************
--**************************************************************************
-- DIRECTORY CONFIGURATION
--**************************************************************************
--**************************************************************************


------------------------------------------------------------------------
-- DECLARATION
------------------------------------------------------------------------
DECLARE @bank varchar(100) = '009';
------------------------------------------------------------------------
------------------------------------------------------------------------

------------------------------------------------------------------------
-- UPDATE USING YOUR DIRECTORY
------------------------------------------------------------------------

UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\Inward' ,fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'InwardGatewayFolder'
UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\OutwardReturn',fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'OutwardReturnedGatewayFolder'
UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\IMG' ,fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'InwardImageFolder'
UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\ECCS' ,fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'ECCSFolder'
UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\ECCS' ,fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'ConsolidatedFolder'
UPDATE tblSystemProfile set fldSystemProfileValue = 'D:\BankPBM\UPI\'+@bank ,fldEntityId = @bank, fldBankCode = @bank where fldSystemProfileCode = 'OutwardReturnAgentFolder'

------------------------------------------------------------------------
--Inward Debit File Folder
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'InwardDebitFileFolder'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'InwardDebitFileFolder',N'Inward Debit File Folder',N'E:\INCHEQS4\BankPBM\InwardDebit',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)
------------------------------------------------------------------------
--Password DllPath
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'DllPath'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'DllPath',N'DLL And Other Path',N'D:\INCHEQS_ICS\dll\',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

------------------------------------------------------------------------
--Temporary cheque image
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeTempFolderPath'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeTempFolderPath',N'Cheque Image Temp Folder',N'D:\BankPBM\TempGif\',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

------------------------------------------------------------------------
--Active Directory (LDAP) configuration
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'LoginAD'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'LoginAD',N'Login using AD or NON AD',N'N',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--LDAP DOMAIN
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'DomainLDAP'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'DomainLDAP',N'LDAP Domain',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

------------------------------------------------------------------------
--FTP Service
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'FTPHostName'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'FTPHostName',N'FTP Host Name',N'localhost:14147',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'FTPUserName'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'FTPUserName',N'FTP User Name',N'ftpuser1',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'FTPPassword'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'FTPPassword',N'FTP Password',N'admin123',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

------------------------------------------------------------------------
--Cheque Image Format
------------------------------------------------------------------------
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeBlackWhiteFront'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeBlackWhiteFront',N'Cheque Black White Front Format',N'_BWF.tif',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeBlackWhiteBack'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeBlackWhiteBack',N'Cheque Black White Back Format',N'_BWB.tif',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeGreyscaleFront'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeGreyscaleFront',N'Cheque Greyscale Front Format',N'_GF.jpeg',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeGreyscaleBack'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeGreyscaleBack',N'Cheque Greyscale Back Format',N'_GB.jpeg',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ChequeGreyscaleUV'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ChequeGreyscaleUV',N'Cheque Greyscale UV Back Format',N'_GU.jpeg',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)
------------------------------------------------------------------------
--Maintenance Checker/Maker
------------------------------------------------------------------------

--User
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'UserChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'UserChecker',N'Maintenance - User Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--Bankcode
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'BankCodeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'BankCodeChecker',N'Maintenance - Bank Code Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--Group
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'GroupChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'GroupChecker',N'Maintenance - Group Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--HostReturnReason
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'HostReturnReasonChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'HostReturnReasonChecker',N'Maintenance - Host Return Reason Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--Statecode
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'StateCodeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'StateCodeChecker',N'Maintenance - State Code Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--BranchCode
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'BranchCodeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'BranchCodeChecker',N'Maintenance - Branch Code Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--TransactionCode
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'TransactionCodeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'TransactionCodeChecker',N'Maintenance - Transaction Code Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--TransactionType
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'TransactionTypeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'TransactionTypeChecker',N'Maintenance - Transaction Type Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--InternalBranch
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'InternalBranchChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'InternalBranchChecker',N'Maintenance - Internal Branch Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--VerificationLimit
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'VerificationLimitChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'VerificationLimitChecker',N'Maintenance - Verification Limit Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--PullOutReason
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'PullOutReasonChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'PullOutReasonChecker',N'Maintenance - Pull Out Reason Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--ReturnCode
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ReturnCodeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ReturnCodeChecker',N'Maintenance - Return Code Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--ThresholdSetting
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'ThresholdSettingChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'ThresholdSettingChecker',N'Maintenance - Threshold Setting Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)


--AccountMaintenance
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'AccountMaintenanceChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'AccountMaintenanceChecker',N'Maintenance - Account Maintenance Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

--AccountType
DELETE FROM [tblSystemProfile] WHERE [fldSystemProfileCode] = 'AccountTypeChecker'
INSERT [dbo].[tblSystemProfile] ([fldSystemProfileId],[fldSystemProfileCode],[fldSystemProfileDesc],[fldSystemProfileValue],[fldAllowEdit],[fldApproveUserId],[fldApproveTimeStamp],[fldUpdateUserId],[fldUpdateTimeStamp],[fldEntityId], [fldBankCode]) VALUES ((select top 1 fldSystemProfileId from tblsystemprofile order by fldSystemProfileId desc) + 1,N'AccountTypeChecker',N'Maintenance - Account Type Checker',N'Y',N'Y',N'1',GETDATE(),1,GETDATE(),@bank, @bank)

GO