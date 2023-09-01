using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Group;
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

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance {


    public class GroupController : BaseController {

        private readonly IGroupDao groupDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;

        public GroupController(IGroupDao groupDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.groupDao = groupDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.Group.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.Group.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Group.INDEX, "View_Group", "fldGroupId", "fldBankCode =@fldBankCode", new[] {
             new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string groupIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string groupId = "";
            //if (string.IsNullOrEmpty(groupIdParam)) {
            //    groupId = filter["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode;
            //} else {
            //    groupId = groupIdParam;
            //}
            if (string.IsNullOrEmpty(groupIdParam))
            {
                groupId = filter["fldGroupId"].Trim();
            }
            else
            {
                groupId = groupIdParam;
            }
          
            ViewBag.Group = groupDao.GetGroup(groupId, CurrentUser.Account.BankCode);
            ViewBag.SelectedUser = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
            ViewBag.AvailableUser = groupDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> userIds = new List<string>();
                //string groupId = col["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode;
                string groupId = col["fldGroupId"].Trim();
                GroupModel beforeGroup = groupDao.GetGroup(groupId, CurrentUser.Account.BankCode);
                auditTrailDao.Log("Update Group- Before Update=> Group ID: " + beforeGroup.fldGroupId + " Group Desc : " + beforeGroup.fldGroupDesc, CurrentUser.Account);
                //groupDao.UpdateGroupMaster(col, CurrentUser.Account.UserId);
                GroupModel afterGroup = groupDao.GetGroup(groupId, CurrentUser.Account.BankCode);
                auditTrailDao.Log("Update Group- After Update=> Group ID: " + afterGroup.fldGroupId + " Group Desc : " + afterGroup.fldGroupDesc, CurrentUser.Account);


                if ((col["selectedUser"]) != null) {
                    //Start Audit Trail
                    string beforeUser = "";
                    List<UserModel> beforeUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                    foreach (var beforeUserlist in beforeUserLists) {
                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Group , Before Update =>- Group ID: " + groupId + " User : " + beforeUser, CurrentUser.Account);
                    //End Audit Trail

                    //Update Process Start Here
                    //groupDao.DeleteUserNotSelected(groupId, col["selectedUser"]);
                    userIds = col["selectedUser"].Split(',').ToList();
                    foreach (string userId in userIds)
                    {
                        groupDao.DeleteUserNotSelected(groupId, userId, CurrentUser.Account.BankCode);
                    }
                    foreach (string userId in userIds) {
                        if ((groupDao.CheckUserExistInGroup(groupId, userId, CurrentUser.Account.BankCode))) {
                            groupDao.UpdateSelectedUser(groupId, userId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        } else {
                            groupDao.InsertUserInGroup(groupId, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                        }
                    }


                    //Start Audit Trail
                    string afterUser = "";
                    List<UserModel> afterUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                    foreach (var afterUserlist in afterUserLists) {
                        afterUser = afterUser + afterUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Group , After Update => - Group ID: " + groupId + " User : " + afterUser, CurrentUser.Account);
                    //End Audit Trail

                } else {
                    //Start Audit Trail
                    string beforeUser = "";
                    List<UserModel> beforeUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                    foreach (var beforeUserlist in beforeUserLists) {
                        beforeUser = beforeUser + beforeUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Group , Before Update =>- Group ID: " + groupId + " User : " + beforeUser, CurrentUser.Account);
                    //End Audit Trail

                    groupDao.DeleteAllUserInGroup(groupId, CurrentUser.Account.BankCode);

                    //Start Audit Trail
                    string afterUser = "";
                    List<UserModel> afterUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                    foreach (var afterUserlist in afterUserLists) {
                        afterUser = afterUser + afterUserlist.fldUserAbb + ',';
                    }
                    auditTrailDao.Log("Update User In Group , After Update => - Group ID: " + groupId + " User : " + afterUser, CurrentUser.Account);
                    //End Audit Trail
                }
                TempData["Notice"] = Locale.SuccessfullyUpdated;
                return RedirectToAction("Edit", new { groupIdParam = col["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode });
            } catch (Exception ex) {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create() {
            ViewBag.AvailableUser = groupDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col) {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("GroupChecker", CurrentUser.Account.BankCode).Trim();
            try {
                List<string> userIds = new List<string>();
                //string groupId = col["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode;
                string groupId = col["fldGroupId"].Trim();
                List<string> errorMessages = groupDao.ValidateGroup(col, CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    if ("N".Equals(systemProfile)) {
                        groupDao.CreateGroupMaster(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "", CurrentUser.Account.UserId);
                        auditTrailDao.Log("Add - Group ID: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);

                        if ((col["selectedUser"]) != null)
                        {
                            userIds = col["selectedUser"].Split(',').ToList();
                            foreach (string userId in userIds)
                            {
                                groupDao.InsertUserInGroup(groupId, userId,CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                            }

                            //Start Audit Trail
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            auditTrailDao.Log("Add User In Group , After Update => - Group ID: " + groupId + " User : " + afterUser, CurrentUser.Account);
                            //End Audit Trail
                        }
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                    }
                    else {
                        groupDao.CreateGroupMasterTemp(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode, "", CurrentUser.Account.UserId);
                    //    TempData["Notice"] = Locale.GroupMasterCreateVerify;
                    //    auditTrailDao.Log("Add - Group ID: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);
                        if ((col["selectedUser"]) != null)
                        {
                            userIds = col["selectedUser"].Split(',').ToList();
                            foreach (string userId in userIds)
                            {
                                groupDao.InsertUserInGroup(groupId, userId, CurrentUser.Account.UserId, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);
                            }

                    //        //Start Audit Trail
                            string afterUser = "";
                            List<UserModel> afterUserLists = groupDao.ListSelectedUserInGroup(groupId, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in afterUserLists)
                            {
                                afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            }
                            auditTrailDao.Log("Add User In Group , After Update => - Group ID: " + groupId + " User : " + afterUser, CurrentUser.Account);
                    //        //End Audit Trail
                    //    }
                    //    TempData["Notice"] = Locale.SuccessfullyCreated;
                    //}
                        }
                        TempData["Notice"] = Locale.GroupMasterCreateVerify;
                        auditTrailDao.Log("Add into Temporary record to Create - Group ID: " + col["fldGroupDesc"], CurrentUser.Account);
                    }

                    
                }
                return RedirectToAction("Create");
            } catch (Exception ex) {
                //using (StreamWriter mywrite = new StreamWriter("D:/test.txt", true))
                //{
                //    mywrite.WriteLine(ex.Message);
                //}
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.Group.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col) {
            try {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null) {
                    if ("N".Equals(groupDao.GetMaintenanceChecker().Trim())) {
                        /*arrResults = col["deleteBox"].Trim().Split(',').ToList();
                        foreach (var arrResult in arrResults) {
                            groupDao.DeleteAllUserInGroup(arrResult);
                            groupDao.DeleteAllTaskInGroup(arrResult);
                            groupDao.DeleteGroup(arrResult);
                            auditTrailDao.Log("Delete Group - Group ID: " + arrResult, CurrentUser.Account);
                        } */
                        arrResults = col["deleteBox"].Trim().Split(',').ToList();
                        foreach (var arrResult in arrResults) {
                            groupDao.DeleteAllUserInGroup(arrResult, CurrentUser.Account.BankCode);
                            groupDao.DeleteAllTaskInGroup(arrResult, CurrentUser.Account.BankCode);
                            groupDao.DeleteGroup(arrResult, CurrentUser.Account.BankCode);
                            auditTrailDao.Log("Delete Group - Group ID: " + arrResult, CurrentUser.Account);
                        }
                        TempData["Notice"] = Locale.SuccessfullyDeleted;

                    } else {
                        
                        arrResults = col["deleteBox"].Trim().Split(',').ToList();
                        foreach (var arrResult in arrResults)
                        {
                            groupDao.AddGroupToGroupMasterTempToDelete(arrResult);
                            auditTrailDao.Log("Add into Temporary record to Delete - Group Id: " + col["deleteBox"], CurrentUser.Account);
                        }                      
                        TempData["Notice"] = Locale.GroupSuccessfullyAddedtoApproved;
                    }
                } else {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }

                return RedirectToAction("Index");
            } catch (Exception ex) {
                throw ex;
            }
        }

    }
}