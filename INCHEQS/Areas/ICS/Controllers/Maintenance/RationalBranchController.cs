using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;

using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.DataAccessLayer;

using INCHEQS.Areas.ICS.Models.RationalBranch;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{


    public class RationalBranchController : BaseController
    {

        private readonly IRationalBranchDao RationalBranchDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ApplicationDbContext dbContext;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;


        public RationalBranchController(IRationalBranchDao RationalBranchDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ApplicationDbContext dbContext, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.dbContext = dbContext;
            this.RationalBranchDao = RationalBranchDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.RationalBranch.INDEX));
            ViewBag.PageTitle = RationalBranchDao.GetPageTitle(TaskIds.RationalBranch.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.RationalBranch.INDEX, "View_RationalBranch"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string branchIdParam = "", string IbranchIdParam = "")
        {
            string staskid = TaskIds.RationalBranch.EDIT;
            if (CurrentUser.HasTask(staskid))
            {
                CurrentUser.Account.TaskId = staskid;
            }

            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string branchId = "";
            string IbranchId = "";
            if (string.IsNullOrEmpty(branchIdParam))
            {
                branchId = filter["fldBranchId"].Trim();
                IbranchId = filter["fldIBranchId"].Trim();
            }
            else
            {
                branchId = branchIdParam;
                IbranchId = IbranchIdParam;
            }

            ViewBag.RationalBranch = RationalBranchDao.GetBranch(branchId, "Update");
            ViewBag.SelectedRationalBranch = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
            
            ViewBag.AvailableRationalBranch = RationalBranchDao.ListAvailableRationalBranch(CurrentUser.Account.BankCode);

            ViewBag.PageTitle = RationalBranchDao.GetPageTitle(TaskIds.RationalBranch.EDIT);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string staskid = TaskIds.RationalBranch.UPDATE;
            if (CurrentUser.HasTask(staskid))
            {
                CurrentUser.Account.TaskId = staskid;
            }

            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> selectedBranchIds = new List<string>();
                string branchId = col["fldCBranchId"].Trim();
                string IbranchId = col["fldIBranchId"].Trim();

                if ("N".Equals(securityProfile))
                    {
                        if ((RationalBranchDao.NoChangesBranchSelected(col, branchId) == true))
                        {
                            TempData["ErrorMsg"] = Locale.NoChanges;
                        }
                        else
                        {

                            //auditTrailDao.Log("Update - Group Code: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);

                            if ((col["selectedUser"]) != null)
                            {
                                string beforeUser = "";
                                List<RationalBranchModel> beforeUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);

                                //foreach (var beforeUserlist in beforeUserLists)
                                //{
                                //    beforeUser = beforeUser + beforeUserlist.fldUserAbb + ",";
                                //}

                                //RationalBranchModel before = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, beforeUser);

                                //auditTrailDao.Log("Update User In Group , Before Update => Group Code: " + groupId + " User : " + beforeUser, CurrentUser.Account);

                                selectedBranchIds = col["selectedUser"].Split(',').ToList();
                                foreach (string selectedBranchId1 in selectedBranchIds)
                                {
                                    RationalBranchDao.DeleteRationalBranchNotSelected(branchId, selectedBranchId1);
                                }

                                foreach (string selectedBranchId2 in selectedBranchIds)
                                {
                                    if (RationalBranchDao.CheckRationalBranchExist(branchId, selectedBranchId2) == true)
                                    {
                                        RationalBranchDao.UpdateRationalBranchTable(branchId, selectedBranchId2);
                                    }
                                    else
                                    {
                                        RationalBranchDao.CreateRationalBranch(col, selectedBranchId2, "Update");
                                    }
                                }

                                string afterUser = "";
                                List<RationalBranchModel> afterUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);

                                //foreach (var afterUserlist in afterUserLists)
                                //{
                                //    afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                                //}
                                //RationalBranchModel after = SecurityAuditLogDao.CheckGroupMasterUserID(groupId, afterUser);

                                //string ActionDetails = SecurityAuditLogDao.GroupProfile_EditTemplate(before, after, beforeUser, afterUser, "Edit", "Update", col);
                                //auditTrailDao.SecurityLog("Edit Group", ActionDetails, staskid, CurrentUser.Account);

                                //auditTrailDao.Log("Update User In Group , After Update => - Group Code: " + groupId + " User : " + afterUser, CurrentUser.Account);

                            }
                            else
                            {
                                string beforeUser = "";
                                List<RationalBranchModel> beforeUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
                                foreach (var beforeUserlist in beforeUserLists)
                                {
                                    beforeUser = beforeUserlist.fldCBranchId;
                                    RationalBranchDao.DeleteRationalBranchNotSelected(branchId, beforeUser);
                                }

                            }

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                    }
                    else //if systemprofile groupchecker=Y
                    {
                    //check branchId is pending for verify or not
                    if (RationalBranchDao.CheckRationalBranchTempExistByID(branchId, "Update"))
                        {
                            TempData["Warning"] = Locale.RationalBranchPendingVerify;
                        }
                        else
                        {
                            //If there is no available user anymore.
                            string availableUser = "";
                            List<RationalBranchModel> availableUserLists = RationalBranchDao.ListAvailableRationalBranch(CurrentUser.Account.BankCode);
                            //List<UserModel> availableUserLists = RationalBranchDao.ListAvailableUserInGroup(CurrentUser.Account.BankCode);
                            foreach (var afterUserlist in availableUserLists)
                            {
                                availableUser = availableUser + afterUserlist.fldCBranchId + ",";
                            }

                            if ((availableUser == "") && ((RationalBranchDao.NoChangesBranch(col, branchId) == true)))
                            {
                                TempData["Warning"] = Locale.AvailableRationalBranchEmpty;
                                TempData["ErrorMsg"] = Locale.NoChanges;
                                goto Done;
                            }
                            else if ((RationalBranchDao.NoChangesBranchSelected(col, branchId) == true))
                            {
                                TempData["ErrorMsg"] = Locale.NoChanges;
                            }
                            else
                            {
                                if ((col["selectedUser"]) != null)
                                {
                                    string beforeUser = "";
                                    //check original selected branch
                                    List<RationalBranchModel> beforeUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
                                    foreach (var beforeUserlist in beforeUserLists)
                                    {
                                        beforeUser = beforeUser + beforeUserlist.fldCBranchId + ",";

                                    }

                                // auditTrailDao.Log("Update User In Group , Before Update =>- Group Code: " + groupId + " User : " + beforeUser, CurrentUser.Account);

                                selectedBranchIds = col["selectedUser"].Split(',').ToList();

                                    foreach (string selectedBranchId in selectedBranchIds)
                                    {
                                        //check selected branchId is pending for verify or not(action is update)
                                        if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, selectedBranchId, "Check"))
                                        {
                                            TempData["Warning"] = Locale.RationalBranchStillPendingforApproval;
                                            goto Done;
                                        }
                                        //else
                                        //{
                                        //    RationalBranchDao.CreateRationalBranchTemp(branchId, selectedBranchId, CurrentUser.Account.UserId, "CreateA");
                                        //}
                                    }

                                foreach (string selectedBranchId in selectedBranchIds) {
                                    RationalBranchDao.CreateRationalBranchTemp(branchId, selectedBranchId, CurrentUser.Account.UserId, "CreateA");
                                }
								string alreadyselecteduser = "";
								List<RationalBranchModel> alreadyselecteduserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
								foreach (var beforeUserlist in alreadyselecteduserLists)
								{
									alreadyselecteduser = beforeUserlist.fldCBranchId;
									if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, alreadyselecteduser, "Check") == false)
									{
										RationalBranchDao.CreateRationalBranchTemp(branchId, alreadyselecteduser, CurrentUser.Account.UserId, "CreateD");
									}

								}
							}
                                else
                                {
                                    string beforeUser = "";
                                    List<RationalBranchModel> beforeUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
                                    foreach (var beforeUserlist in beforeUserLists)
                                    {
                                        beforeUser = beforeUserlist.fldCBranchId;
                                        if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, beforeUser, "Check") == false)
                                        {
                                            RationalBranchDao.CreateRationalBranchTemp(branchId, beforeUser, CurrentUser.Account.UserId, "CreateD");
                                        }
                                    }
                                }

                                string beforeUser2 = "";
                                List<RationalBranchModel> beforeUserLists2 = RationalBranchDao.ListSelectedRationalBranch(branchId, IbranchId);
                                foreach (var beforeUserlist2 in beforeUserLists2)
                                {

                                    beforeUser2 = beforeUser2 + beforeUserlist2.fldCBranchId + "\n";
                                }


                                TempData["Notice"] = Locale.RationalBranchUpdateVerify;
                                //auditTrailDao.Log("Add into Temporary record to Update - Group Code: " + col["fldGroupCode"], CurrentUser.Account);

                            }
                        }
                    }
                

                Done:
                return RedirectToAction("Edit", new { branchIdParam = col["fldCBranchId"].Trim(), IbranchIdParam  = col["fldIBranchId"].Trim() });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            ViewBag.AvailableRationalBranch = RationalBranchDao.ListAvailableRationalBranch(CurrentUser.Account.BankCode);
            ViewBag.ActiveRationalBranch = RationalBranchDao.ListActiveRationalBranch(CurrentUser.Account.BankCode);
            ViewBag.PageTitle = RationalBranchDao.GetPageTitle(TaskIds.RationalBranch.CREATE);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            string sTaskId = TaskIds.RationalBranch.SAVECREATE;
            if (CurrentUser.HasTask(sTaskId))
            {
                CurrentUser.Account.TaskId = sTaskId;
            }

            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();
            try
            {
                List<string> selectedBranchIds = new List<string>();
                List<string> groupIds = new List<string>();
                string branchId = col["fldInternalBranchCode"].Trim();

                    if ("N".Equals(securityProfile))
                    {
                        //auditTrailDao.Log("Add - Group Code: " + groupId + " Group Desc : " + col["fldGroupDesc"], CurrentUser.Account);

                        if ((col["selectedUser"]) != null)
                        {
                        selectedBranchIds = col["selectedUser"].Split(',').ToList();
                            foreach (string selectedBranchId in selectedBranchIds)
                            {
                                groupIds = branchId.Split(',').ToList();

                                RationalBranchDao.CreateRationalBranch(col, selectedBranchId, "Create");
                            }


                            //string afterUser = "";
                            //List<RationalBranchModel> afterUserLists = RationalBranchDao.ListSelectedRationalBranch(branchId, CurrentUser.Account.BankCode);
                            //foreach (var afterUserlist in afterUserLists)
                            //{
                            //    afterUser = afterUser + afterUserlist.fldUserAbb + ",";
                            //}

                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser, "Add", "Create");
                            //auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);


                           // auditTrailDao.Log("Add User In Group , After Update => - Group Code: " + groupId + " User : " + afterUser, CurrentUser.Account);

                        }

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                    }
                    else //systemprofile='Y'
                    {
                    //check branchId is pending for verify or not
                        if (RationalBranchDao.CheckRationalBranchTempExistByID(branchId, "Create") == true)
                        {
                            TempData["Warning"] = Locale.RationalBranchPendingVerify;
                            goto Done;
                        }
                        else
                        {
                        if ((col["selectedUser"]) != null)
                        {
                            selectedBranchIds = col["selectedUser"].Split(',').ToList();
                            foreach (string selectedBranchId in selectedBranchIds)
                            {
                                //check selected branchId is pending for verify or not (action is update)
                                if (RationalBranchDao.CheckSelectedBranchIdExistInTemp(branchId, selectedBranchId, "Check") == true)
                                {
                                    TempData["Warning"] = Locale.RationalBranchStillPendingforApproval;
                                    goto Done;
                                }
                                else
                                {
                                    RationalBranchDao.CreateRationalBranchTemp(branchId, selectedBranchId, CurrentUser.Account.UserId, "CreateA");
                                }
                            }

                            //string afterUser2 = "";
                            //List<RationalBranchModel> afterUserLists2 = RationalBranchDao.ListSelectedRationalBranchChecker(branchId);
                            //foreach (var afterUserlist2 in afterUserLists2)
                            //{
                            //    afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            //}

                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_AddTemplate(col, afterUser2, "Add", "Create");
                            //auditTrailDao.SecurityLog("Add Group", ActionDetails, sTaskId, CurrentUser.Account);

                            //auditTrailDao.Log("Add User In Group , After Update => - Group Code: " + groupId + " User : " + afterUser, CurrentUser.Account);

                            TempData["Notice"] = Locale.RationalBranchCreateVerify;
                        }
                        else {
                            TempData["Warning"] = Locale.NoChanges;
                        }

                        }
                    }
                
                Done:
                return RedirectToAction("Create", new { groupIdParam = col["fldInternalBranchCode"].Trim() }); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.RationalBranch.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col)
        {
            string sTaskid = TaskIds.RationalBranch.DELETE;
            if (CurrentUser.HasTask(sTaskid))
            {
                CurrentUser.Account.TaskId = sTaskid;
            }

            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            try
            {
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    if ("N".Equals(securityProfile))
                    {
                        arrResults = col["deleteBox"].Trim().Split(',').ToList();
                        foreach (var arrResult in arrResults)
                        {
                            string afterUser2 = "";
                            List<RationalBranchModel> afterUserLists2 = RationalBranchDao.ListSelectedRationalBranchCheckerWithAllTempRecord(arrResult);
                            foreach (var afterUserlist2 in afterUserLists2)
                            {
                                afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            }

                            RationalBranchDao.DeleteRationalBranch(arrResult);

                            //string ActionDetails = SecurityAuditLogDao.GroupProfile_DeleteTemplate(afterUser2, "Delete", "Delete", arrResult);
                            //auditTrailDao.SecurityLog("Delete Group", ActionDetails, sTaskid, CurrentUser.Account);
                            //auditTrailDao.Log("Delete Group - Group Code: " + arrResult, CurrentUser.Account);
                        }
                        TempData["Notice"] = Locale.RecordsuccesfullyDeleted;
                    }
                    else
                    {
                        arrResults = col["deleteBox"].Trim().Split(',').ToList();
                        foreach (var arrResult in arrResults)
                        {
                            string afterUser2 = "";
                            List<RationalBranchModel> afterUserLists2 = RationalBranchDao.ListSelectedRationalBranch(arrResult, CurrentUser.Account.BankCode);
                            foreach (var afterUserlist2 in afterUserLists2)
                            {
                                afterUser2 = afterUser2 + afterUserlist2.fldUserAbb + "\n";
                            }
                            //check branchId is pending for verify or not
                            if (RationalBranchDao.CheckRationalBranchTempExistByID(arrResult, "Delete") == true)
                            {
                                TempData["Warning"] = Locale.RationalBranchPendingVerify;
                                goto Done;
                            }
                            else
                            {
                                RationalBranchDao.CreateRationalBranchTemp(arrResult, "", CurrentUser.Account.UserId, "Delete");
                            }


                        }
                        //auditTrailDao.Log("Add into Temporary record to Delete - Group Code: " + col["deleteBox"], CurrentUser.Account);
                        TempData["Notice"] = Locale.RationalBranchDeleteVerify;
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