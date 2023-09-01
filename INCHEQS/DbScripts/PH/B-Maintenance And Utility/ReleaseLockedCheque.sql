--------------------------------------------------------------
--Declaration
--------------------------------------------------------------
DECLARE @mainMenu varchar(100) = 'Utilities';
DECLARE @subMenu varchar(100) = '';
DECLARE @menuTitle varchar(100) = 'Release Locked Cheque';
DECLARE @taskDesc varchar(100) = 'Utilities - Release Locked Cheque';
DECLARE @mainMenuOrder int = 8;
DECLARE @subMenuOrder int = 10;

DECLARE @bank varchar(100) = '009';

DECLARE @parentTaskId varchar(100) = '205000';
DECLARE @taskId varchar(100) = '205500';
DECLARE @mvcUrl varchar(100) = 'ReleaseLockedCheque';

------------------------------------------------------------------------
-- TASK MENU
------------------------------------------------------------------------
DELETE FROM [tblTaskMaster] WHERE [fldTaskId] = @taskId
INSERT [dbo].[tblTaskMaster] ([fldBranchIndicator], [fldTaskId], [fldParentTaskID], [fldSubMenuTaskID], [fldMenuType], [fldMenuDesc], [fldButtonType], [fldSeqNo], [fldSearchFlag], [fldEditPage], [fldTaskType], [fldMainMenu], [fldSubMenu], [fldMainSeq], [fldSubSeq], [fldMenuTitle], [fldPageTitle], [fldTaskDesc], [fldSearch], [fldAllowNew], [fldImgFileName], [fldEditFile], [fldWidth], [fldHeight], [fldListImage], [fldPageImage], [fldBrowsePage], [fldRecordStatus], [fldMvcUrl]) VALUES (N'H', @taskId, @parentTaskId, NULL, N'S', N'', N' ', NULL, N' ', N'', N'Menu Page', @mainMenu, @subMenu, @mainMenuOrder, @subMenuOrder, @menuTitle, N'', @taskDesc, N'N', N'N', N'', N'', 780, 400, N'', N'', N'', 1, N'ICS/'+@mvcUrl)

