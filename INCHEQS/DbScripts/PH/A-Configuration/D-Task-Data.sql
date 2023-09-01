--**************************************************************************
--**************************************************************************
-- MAINTENANCE
--**************************************************************************
--**************************************************************************

------------------------------------------------------------------------
-- DECLARATION
------------------------------------------------------------------------
DECLARE @bank varchar(100) = '009';
------------------------------------------------------------------------
------------------------------------------------------------------------
------------------------------------------------------------------------
-- Delete all task from database 
------------------------------------------------------------------------
DELETE FROM tblTaskMaster
------------------------------------------------------------------------
-- Delete all assigned task and add TASK ASSIGMENT to admin
------------------------------------------------------------------------
DELETE FROM tblGroupTask
DBCC CHECKIDENT ('tblGroupTask', RESEED, 0);
INSERT tblGroupTask (fldGroupId,fldTaskId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApproved) values
('ADMIN',102230,'1',GETDATE(),'1',GETDATE(),'Y')
INSERT tblGroupTask (fldGroupId,fldTaskId,fldCreateUserId,fldCreateTimeStamp,fldUpdateUserId,fldUpdateTimeStamp,fldApproved) values
('ADMIN',102231,'1',GETDATE(),'1',GETDATE(),'Y')


------------------------------------------------------------------------
-- LargeAmount
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '102670'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 102670, 102000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Maintenance', N'', 9, 9, N'Large Amount Limit', N'', N'Maintenance - Large Amount', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/LargeAmount')

------------------------------------------------------------------------
-- Threshold Setting
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '102680'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 102680, 102000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Maintenance', N'', 9, 1, N'Threshold Setting', N'', N'Maintenance - Threshold Setting', N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ThresholdSetting')

DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '102682'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 102682, 102000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Maintenance', N'', 9, 1, N'Threshold Setting', N'', N'Maintenance - Threshold Setting (Edit)', N'Y', N'Y', N'', N'', 780, 400, N'', N'', N'', 1, N'')

DELETE FROM [tblThresholdSetting] WHERE [fldBankCode] = @bank
INSERT [dbo].[tblThresholdSetting] ([fldId], [fldType], [fldSequence], [fldTitle], [fldAmount], [fldEnable], [fldUpdateUserId], [fldUpdateTimeStamp], [fldEntityCode], [fldBankCode]) VALUES (1, N'A', 1, N'1st Level Amount', 5000, N'Y', 1, NULL, @bank, @bank)

INSERT [dbo].[tblThresholdSetting] ([fldId], [fldType], [fldSequence], [fldTitle], [fldAmount], [fldEnable], [fldUpdateUserId], [fldUpdateTimeStamp], [fldEntityCode], [fldBankCode]) VALUES (3, N'A', 2, N'2nd Level Amount', 249999.99, N'Y', 1, NULL, @bank, @bank)

INSERT [dbo].[tblThresholdSetting] ([fldId], [fldType], [fldSequence], [fldTitle], [fldAmount], [fldEnable], [fldUpdateUserId], [fldUpdateTimeStamp],[fldEntityCode], [fldBankCode]) VALUES (2, N'R', 1, N'1st Level Amount', 0, N'Y', 1, NULL, @bank, @bank)

INSERT [dbo].[tblThresholdSetting] ([fldId], [fldType], [fldSequence], [fldTitle], [fldAmount], [fldEnable], [fldUpdateUserId], [fldUpdateTimeStamp],[fldEntityCode], [fldBankCode]) VALUES (4, N'R', 2, N'2nd Level Amount', 0, N'Y', 1, NULL, @bank, @bank)

--**************************************************************************
--**************************************************************************
-- UTILITIES
--**************************************************************************
--**************************************************************************
------------------------------------------------------------------------
-- ClearUserSession
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205200'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205200, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 4, N'End User Session', N'', N'Utilities - End User Session', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ClearUserSession')
GO
------------------------------------------------------------------------
-- SecurityProfile
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205180'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205180, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 2, N'Security Profile', N'', N'Utilities - Security Profile', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/SecurityProfile')
GO
--Disable concurren connection
UPDATE tblSecurityProfile SET fldNCConnection = 0
--Add timout 30 minute
UPDATE tblSecurityProfile SET fldTimeOut = 30

------------------------------------------------------------------------
-- DataHouseKeep
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205210'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205210, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 3, N'Data HouseKeeping - Retention Period', N'', N'Utilities - Data HouseKeeping - Retention Period', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/DataHouseKeep')
GO
------------------------------------------------------------------------
-- ClearDataProcess
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205300'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205300, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 5, N'Clear Data Process', N'', N'Utilities - Clear Data Process', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ClearDataProcess')
GO
------------------------------------------------------------------------
-- ChangePassword
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205110'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205110, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 6, N'Change Password', N'', N'Utilities - Change Password', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ChangePassword')
GO
------------------------------------------------------------------------
-- DayEndProcess
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205220'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205220, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 7, N'Day End Process', N'', N'Utilities - Day End Process', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/DayEndProcess')
GO
------------------------------------------------------------------------
-- SendEmail
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205280'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205280, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 9, N'Send Email', N'', N'Utilities - Send Email', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/SendEmail')
GO
------------------------------------------------------------------------
-- ScheduledTask
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = '205420'
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', 205420, 205000, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', N'Utilities', N'', 8, 9, N'Scheduled Task', N'', N'Utilities - Scheduled Task', N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/ScheduledTask')
GO

------------------------------------------------------------------------
--Sequence No 
------------------------------------------------------------------------


------------------------------------------------------------------------
--END 
------------------------------------------------------------------------




GO