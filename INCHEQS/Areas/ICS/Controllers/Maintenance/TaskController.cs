using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.Group;
using INCHEQS.Security.AuditTrail;
//using INCHEQS.TaskAssignment;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance {

    
    public class TaskController : BaseController {
        
        private readonly ITaskDao taskDao;
        private readonly IGroupDao groupDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;

        public TaskController(ITaskDao taskDao, IGroupDao groupDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao) {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.taskDao = taskDao;
            this.groupDao = groupDao;
            this.systemProfileDao = systemProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIds.Task.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.Task.INDEX));
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.Task.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Task.INDEX, "View_Task","", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Task.EDIT)]
        [GenericFilter(AllowHttpGet=true)]
        public ActionResult Edit(FormCollection collection, string groupIdParam = "") {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string groupId = "";
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("TaskChecker", CurrentUser.Account.BankCode).Trim();

            //if (string.IsNullOrEmpty(groupIdParam)) {
            //    groupId = filter["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode;
            //} else {
            //groupId = groupIdParam;
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
            if ("N".Equals(systemProfile))
            {
                ViewBag.AvailableTask = taskDao.ListAvailableTaskInGroup(groupId);
                ViewBag.SelectedTask = taskDao.ListSelectedTaskInGroup(groupId);
            }
            else
            {
                ViewBag.AvailableTask = taskDao.ListAvailableTaskInGroupTemp(groupId);
                ViewBag.SelectedTask = taskDao.ListSelectedTaskInGroupTemp(groupId);
            }
 
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Task.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> taskIds = new List<string>();
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("TaskChecker", CurrentUser.Account.BankCode).Trim();

                //string groupId = col["fldGroupId"].Trim() + "_" + CurrentUser.Account.BankCode;
                string groupId = col["fldGroupId"].Trim(); // + "_" + CurrentUser.Account.BankCode;

                //Start Audit Trail
                string beforeTask = "";
                List<TaskModel> beforeLists = taskDao.ListSelectedTaskInGroup(groupId);

                if ("N".Equals(systemProfile))
                {
                    foreach (var beforelist in beforeLists)
                    {
                        beforeTask = beforeTask + beforelist.fldTaskId + ", ";
                    }
                    auditTrailDao.Log("Update Task Assignment , Before Update =>- Group ID: " + groupId + " Task : " + beforeTask, CurrentUser.Account);
                    //End Audit Trail

                    if ((col["selectedTask"]) != null)
                    {
                        taskDao.DeleteTaskNotSelected(groupId, col["selectedTask"]);
                        taskIds = col["selectedTask"].Split(',').ToList();
                        foreach (string taskId in taskIds)
                        {
                            if ((taskDao.CheckGroupExist(groupId, taskId)))
                            {
                                taskDao.UpdateSelectedTaskId(CurrentUser.Account.UserId, groupId, taskId);
                            }
                            else
                            {
                                taskDao.InsertSelectedTaskId(CurrentUser.Account.UserId,groupId, taskId);
                            }
                        }
                    }
                    else
                    {
                        taskDao.DeleteAllTaskInGroup(groupId);
                    }
                    //TempData["Notice"] = Locale.SuccessfullyUpdated;
                    TempData["Notice"] = Locale.TaskSuccessfullyUpdated;
                    //Start Audit Trail
                    string afterTask = "";
                    List<TaskModel> afterLists = taskDao.ListSelectedTaskInGroup(groupId);
                    foreach (var afterlist in afterLists)
                    {
                        afterTask = afterTask + afterlist.fldTaskId + ", ";
                    }
                    auditTrailDao.Log("Update Task Assignment , After Update =>- Group ID: " + groupId + " Task : " + afterTask, CurrentUser.Account);
                    //End Audit Trail
                }
                else
                {
                    foreach (var beforelist in beforeLists)
                    {
                        beforeTask = beforeTask + beforelist.fldTaskId + ", ";
                    }
                    auditTrailDao.Log("Update Task Assignment , Before Update =>- Group ID: " + groupId + " Task : " + beforeTask, CurrentUser.Account);
                    //End Audit Trail

                    if ((col["selectedTask"]) != null)
                    {
                        taskDao.DeleteTaskNotSelectedTemp(groupId, col["selectedTask"]);
                        taskIds = col["selectedTask"].Split(',').ToList();
                        foreach (string taskId in taskIds)
                        {
                            if ((taskDao.CheckGroupExist(groupId, taskId)))
                            {
                                taskDao.UpdateSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId);
                            }
                            else
                            {
                                taskDao.InsertSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId);
                            }
                        }
                    }
                    else
                    {
                        taskDao.DeleteAllTaskInGroupApproval(groupId);
                    }
                    TempData["Notice"] = Locale.TaskSuccessfullyUpdatedApproval;

                    //Start Audit Trail
                    string afterTask = "";
                    List<TaskModel> afterLists = taskDao.ListSelectedTaskInGroup(groupId);
                    foreach (var afterlist in afterLists)
                    {
                        afterTask = afterTask + afterlist.fldTaskId + ", ";
                    }
                    //auditTrailDao.Log("Update Task Assignment , After Update =>- Group ID: " + groupId + " Task : " + afterTask, CurrentUser.Account);
                    auditTrailDao.Log("Add into Temporary record to Update - Task Assignment - Group ID: " + groupId + " Task : " + afterTask, CurrentUser.Account);
                    //End Audit Trail
                }
                return RedirectToAction("Edit", new { groupIdParam = col["fldGroupId"].Trim() });
            } catch (Exception ex) {
                throw ex;
            }
        }
    }
}