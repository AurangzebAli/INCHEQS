using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;
using System.Data;
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{


    public class GroupController : BaseController
    {

        private readonly IGroupProfileDao groupProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ApplicationDbContext dbContext;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;


        public GroupController(IGroupProfileDao groupProfileDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ApplicationDbContext dbContext, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.dbContext = dbContext;
            this.groupProfileDao = groupProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

        }

        [CustomAuthorize(TaskIds = TaskIds.Group.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.Group.INDEX));
            ViewBag.PageTitle = groupProfileDao.GetPageTitle(TaskIds.Group.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Group.INDEX, "View_Group", "fldGroupCode", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string groupIdParam = "")
        {
            string staskid = TaskIds.Group.EDIT;
            if (CurrentUser.HasTask(staskid))
            {
                CurrentUser.Account.TaskId = staskid;
            }

            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string groupId = "";
            if (string.IsNullOrEmpty(groupIdParam))
            {
                groupId = filter["fldGroupCode"].Trim();
            }
            else
            {
                groupId = groupIdParam;
            }

            ViewBag.Group = groupProfileDao.GetGroup(groupId, "Update");
            ViewBag.SelectedUser = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
            ViewBag.AvailableUser = groupProfileDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);

            ViewBag.PageTitle = groupProfileDao.GetPageTitle(TaskIds.Group.EDIT);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            CurrentUser.Account.Status = "U";
            string staskid = TaskIds.Group.INDEX;
            if (CurrentUser.HasTask(staskid))
            {
                CurrentUser.Account.TaskId = staskid;
            }
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("GroupChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> userIds = new List<string>();
                string groupId = col["fldGroupCode"].Trim();
                string groupDesc= col["fldGroupDesc"].Trim();

                List<string> errorMessages = groupProfileDao.ValidateGroup(col, "Update", CurrentUser.Account.BankCode);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(securityProfile))
                    {
                        if ((groupProfileDao.NoChangesGroupSelectedUser(col, groupId) == true) && (groupProfileDao.NoChangesGroup(col, groupId) == true))
                        {
                            TempData["ErrorMsg"] = Locale.NoChanges;
                        }
                        else
                        {
                            groupProfileDao.UpdateGroupMasterTable(col, CurrentUser.Account.UserId);

                            //auditTrailDao.Log("Group Code: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);

                            if ((col["selectedUser"]) != null)
                            {
                                string beforeUser = "";
                                List<UserModel> beforeUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);

                                foreach (var beforeUserlist in beforeUserLists)
                                {
                                    beforeUser = beforeUser + beforeUserlist.fldUserAbb + ",";
                                }

                                //GroupProfileModel before = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, beforeUser);                               
                                //auditTrailDao.Log("Before Update => Group Code: " + groupId + " User : " + beforeUser, CurrentUser.Account);
                                

                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId1 in userIds)
                                {
                                    groupProfileDao.DeleteGroupUserNotSelected(groupId, userId1);
                                }

                                foreach (string userId2 in userIds)
                                {
                                    if (groupProfileDao.CheckGroupUserExist(groupId, userId2) == true)
                                    {
                                        groupProfileDao.UpdateGroupUserTable(groupId, userId2);
                                    }
                                    else
                                    {
                                        groupProfileDao.CreateGroupUser(col, userId2, "Update");
                                    }
                                }

                                string afterUser = "";
                                List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);

                                foreach (var afterUserlist in afterUserLists)
                                {
                                    afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                                }
                                //GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, afterUser);

                                //string ActionDetails = SecurityAuditLogDao.GroupProfile_EditTemplate(before, after, beforeUser, afterUser, "Edit", "Update", col);
                                //auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);

                                auditTrailDao.Log("Before Update => Group Code: " + groupId + " Group Desc: " + groupDesc + " User : " + beforeUser, CurrentUser.Account);

                            }
                            else
                            {
                                string beforeUser = "";
                                List<UserModel> beforeUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                foreach (var beforeUserlist in beforeUserLists)
                                {
                                    beforeUser = beforeUserlist.fldUserId;
                                    groupProfileDao.DeleteGroupUserSelected(groupId, beforeUser);
                                }

                                string afterUser = "";
                                List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                foreach (var afterUserlist in afterUserLists)
                                {
                                    afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                                }

                                auditTrailDao.Log("Before Update => Group Code: " + groupId + " Group Desc: " + groupDesc + " User : " + beforeUser, CurrentUser.Account);
                            }

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                    }
                    else //if systemprofile groupchecker=Y
                    {
                        if (groupProfileDao.CheckGroupMasterTempExistByID(groupId, "Update"))
                        {
                            TempData["Warning"] = Locale.GroupMasterPendingVerify;
                        }
                        else
                        {
                            //If there is no available user anymore.
                            string availableUser = "";
                            List<UserModel> availableUserLists = groupProfileDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in availableUserLists)
                            {
                                availableUser = availableUser + afterUserlist.fldUserAbb + ",";
                            }

                            if ((availableUser == "") && ((groupProfileDao.NoChangesGroup(col, groupId) == true) && (groupProfileDao.NoChangesGroupSelectedUser(col, groupId) == true)))
                            {
                                TempData["Warning"] = Locale.GroupUserEmpty;
                                TempData["ErrorMsg"] = Locale.NoChanges;
                                goto Done;
                            }
                            else if ((groupProfileDao.NoChangesGroupSelectedUser(col, groupId) == true) && (groupProfileDao.NoChangesGroup(col, groupId) == true))
                            {
                                TempData["ErrorMsg"] = Locale.NoChanges;
                            }
                            else
                            {
                                if ((col["selectedUser"]) != null)
                                {
                                    string beforeUser = "";
                                    List<UserModel> beforeUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist in beforeUserLists)
                                    {
                                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + ",";

                                    }

                                   //auditTrailDao.Log("Update User In Group , Before Update =>- Group Code: " + groupId + " User : " + beforeUser, CurrentUser.Account);

                                    userIds = col["selectedUser"].Split(',').ToList();

                                    foreach (string userId in userIds)
                                    {
                                        if (groupProfileDao.CheckGroupUserExistInTemp(groupId, userId, "Update"))
                                        {
                                            TempData["Warning"] = Locale.GroupUserStillPendingforApproval;
                                            goto Done;
                                        }
                                        else
                                        {
                                            groupProfileDao.CreateGroupUserTemp(groupId, userId, CurrentUser.Account.UserId, "CreateA");
                                        }
                                    }

                                    string alreadyselecteduser = "";
                                    List<UserModel> alreadyselecteduserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist in alreadyselecteduserLists)
                                    {
                                        alreadyselecteduser = beforeUserlist.fldUserId;
                                        if (groupProfileDao.CheckGroupUserExistInTemp(groupId, alreadyselecteduser, "Update") == false)
                                        {
                                            groupProfileDao.CreateGroupUserTemp(groupId, alreadyselecteduser, CurrentUser.Account.UserId, "CreateD");
                                        }

                                    }
                                    auditTrailDao.Log("Before Update => Group Code: " + groupId + " Group Desc: " + groupDesc + " User : " + beforeUser, CurrentUser.Account);
                                }
                                else
                                {
                                    string beforeUser = "";
                                    List<UserModel> beforeUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                    foreach (var beforeUserlist in beforeUserLists)
                                    {
                                        beforeUser = beforeUserlist.fldUserId;
                                        if (groupProfileDao.CheckGroupUserExistInTemp(groupId, beforeUser, "Update") == false)
                                        {
                                            groupProfileDao.CreateGroupUserTemp(groupId, beforeUser, CurrentUser.Account.UserId, "CreateD");
                                        }
                                    }
                                }

                                string beforeUser2 = "";
                                List<UserModel> beforeUserLists2 = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                                foreach (var beforeUserlist2 in beforeUserLists2)
                                {

                                    beforeUser2 = beforeUser2 + beforeUserlist2.fldUserAbb + "\n";
                                }

                                //GroupProfileModel before = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, beforeUser2);


                                groupProfileDao.CreateGroupMasterTemp(col, CurrentUser.Account.UserId, "Update");

                                string alreadyselecteduser2 = "";
                                List<UserModel> alreadyselecteduserLists2 = groupProfileDao.ListSelectedUserInGroupChecker(groupId, CurrentUser.Account.BankCode);
                                foreach (var alreadyselecteduserList2 in alreadyselecteduserLists2)
                                {
                                    alreadyselecteduser2 = alreadyselecteduser2 + alreadyselecteduserList2.fldUserAbb + "\n";
                                }

                                //GroupProfileModel after = SecurityAuditLogDao.CheckGroupMasterUserIDTemp(groupId);

                                //string ActionDetails = SecurityAuditLogDao.GroupProfile_EditTemplate(before, after, beforeUser2, alreadyselecteduser2, "Edit", "Update", col);
                                //auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);

                                TempData["Notice"] = Locale.GroupMasterUpdateVerify;
                                //auditTrailDao.Log("Add into Temporary record to Update - Group Code: " + col["fldGroupCode"], CurrentUser.Account);

                            }
                        }
                    }
                }

                Done:
                return RedirectToAction("Edit", new { groupIdParam = col["fldGroupCode"].Trim() });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [CustomAuthorize(TaskIds = TaskIds.Group.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            ViewBag.AvailableUser = groupProfileDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);
            ViewBag.PageTitle = groupProfileDao.GetPageTitle(TaskIds.Group.CREATE);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            CurrentUser.Account.Status = "A";
            string sTaskId = TaskIds.Group.INDEX;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("GroupChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> userIds = new List<string>();
                List<string> groupIds = new List<string>();
                string groupId = col["fldGroupCode"].Trim();
                string groupDesc = col["fldGroupDesc"].Trim();
                List<string> errorMessages = groupProfileDao.ValidateGroup(col, "Create", CurrentUser.Account.BankCode);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(securityProfile))
                    {
                        //auditTrailDao.Log("Add - Group Code: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);

                        if ((col["selectedUser"]) != null)
                        {
                            userIds = col["selectedUser"].Split(',').ToList();
                            foreach (string userId in userIds)
                            {
                                groupIds = groupId.Split(',').ToList();

                                groupProfileDao.CreateGroupUser(col, userId, "Create");
                            }


                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser, "Add", "Create");
                            //auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                            auditTrailDao.Log("Group Code: " + groupId + " Group Desc: " + groupDesc + " User : " + afterUser, CurrentUser.Account);

                        }
                        else {
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser, "Add", "Create");
                            //auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                            auditTrailDao.Log("Group Code: " + groupId + " Group Desc: " + groupDesc + " User : " + afterUser, CurrentUser.Account);
                        }

                        groupProfileDao.CreateGroupMaster(col, CurrentUser.Account.BankCode);
                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                    }
                    else //systemprofile='Y'
                    {
                        if (groupProfileDao.CheckGroupMasterTempExistByID(col["fldGroupCode"], "Create") == true)
                        {
                            TempData["Warning"] = Locale.GroupMasterPendingVerify;
                            goto Done;
                        }
                        else
                        {
                            if ((col["selectedUser"]) != null)
                            {
                                userIds = col["selectedUser"].Split(',').ToList();
                                foreach (string userId in userIds)
                                {
                                    if (groupProfileDao.CheckGroupUserExistInTemp(groupId, userId, "Update") == true)
                                    {
                                        TempData["Warning"] = Locale.GroupUserStillPendingforApproval;
                                        goto Done;
                                    }
                                    else
                                    {
                                        groupProfileDao.CreateGroupUserTemp(groupId, userId, CurrentUser.Account.UserId, "CreateA");
                                    }
                                }

                                string afterUser2 = "";
                                List<UserModel> afterUserLists2 = groupProfileDao.ListSelectedUserInGroupChecker(groupId, CurrentUser.Account.BankCode);
                                foreach (var afterUserlist2 in afterUserLists2)
                                {
                                    afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                                }
                                string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser2, "Add", "Create");
                                //auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);
                                auditTrailDao.Log("Group Code: " + groupId + "Group Desc: " + groupDesc + " User : " + afterUser2, CurrentUser.Account);


                            }
                            else {
                                string afterUser3 = "";
                                List<UserModel> afterUserLists3 = groupProfileDao.ListSelectedUserInGroupChecker(groupId, CurrentUser.Account.BankCode);
                                foreach (var afterUserlist3 in afterUserLists3)
                                {
                                    afterUser3 = afterUser3 + afterUserlist3.fldUserAbb + "\n";
                                }
                                //string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser3, "Add", "Create");
                                auditTrailDao.Log("Group Code: " + col["fldGroupCode"] + "Group Desc: " + col["fldGroupDesc"] + " User : " + afterUser3, CurrentUser.Account);
                            }
                            groupProfileDao.CreateGroupMasterTemp(col, CurrentUser.Account.UserId, "Create");
                            TempData["Notice"] = Locale.GroupMasterCreateVerify;
                            
                        }
                    }
                }
                Done:
                return RedirectToAction("Create", new { groupIdParam = col["fldGroupCode"].Trim() }); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.DELETE)]
        [HttpPost]
        
        public ActionResult Delete(FormCollection col)
        {
            CurrentUser.Account.Status = "D";
            string sTaskid = TaskIds.Group.INDEX;
            if (CurrentUser.HasTask(sTaskid))
            {
                CurrentUser.Account.TaskId = sTaskid;
            }
            
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("GroupChecker", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (var arrResult in arrResults)                    
                    if ("N".Equals(securityProfile))
                    {
                            string groupId = arrResult.Trim();
                            string afterUser2 = "";
                            List<UserModel> afterUserLists2 = groupProfileDao.ListSelectedUserInGroupChecker(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist2 in afterUserLists2)
                            {
                                afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            }

                            groupProfileDao.DeleteGroupUser(groupId);
                            groupProfileDao.DeleteGroupTask(groupId);
                            groupProfileDao.DeleteGroupMaster(groupId);

                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_DeleteTemplate(afterUser2, "Delete", "Delete", arrResult);
                            //auditTrailDao.SecurityLog("Delete Group", ActionDetails, sTaskid, CurrentUser.Account);

                            auditTrailDao.Log("Delete Group - Group Code: " + groupId, CurrentUser.Account);     
                            
                        TempData["Notice"] = Locale.RecordsuccesfullyDeleted;
                    }
                    else
                    {
                            string groupId = arrResult.Trim();
                            string afterUser2 = "";
                            List<UserModel> afterUserLists2 = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist2 in afterUserLists2)
                            {
                                afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            }

                            if (groupProfileDao.CheckGroupMasterTempExistByID(groupId, "Delete") == true)
                            {
                                TempData["Warning"] = Locale.GroupMasterPendingVerify;
                                goto Done;
                            }
                            else
                            {
                                groupProfileDao.CreateGroupUserTemp(groupId, "", CurrentUser.Account.UserId, "Delete");
                            }
                            
                            GroupProfileModel group = groupProfileDao.GetGroup(groupId, "Check");
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupProfileDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            GroupProfileModel getGroup = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, afterUser);
                            if (groupId != "") { 
                            auditTrailDao.Log("Group Code: " + groupId + " Group Desc : " + getGroup.fldGroupDesc + " User : " + afterUser, CurrentUser.Account);
                            }
                            groupProfileDao.CreateGroupMasterTempToDelete(groupId, CurrentUser.Account.UserId, "Delete");
                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_DeleteTemplate(afterUser2, "Delete", "Delete", arrResult);
                            //auditTrailDao.SecurityLog("Delete Group", ActionDetails, sTaskid, CurrentUser.Account);                       
                            TempData["Notice"] = Locale.GroupMasterDeleteVerify;
                        }
                   
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                Done:
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}