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
using INCHEQS.Areas.COMMON.Models.Group;
using INCHEQS.Security.SecurityProfile;
using System.Data;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;
//using INCHEQS.TaskAssignment;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{


    public class TaskController : BaseController
    {

        private readonly ITaskDao taskDao;
        //private readonly IGroupDao groupDao;
        private readonly IGroupProfileDao groupDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public TaskController(ITaskDao taskDao, IGroupProfileDao groupDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.taskDao = taskDao;
            this.groupDao = groupDao;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.Task.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.Task.INDEX));
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.Task.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.Task.INDEX, "View_Task", "fldGroupCode", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
                    collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Task.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string groupIdParam = "")
         {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string groupId = "";
            //string systemProfile = systemProfileDao.GetValueFromSystemProfile("TaskChecker", CurrentUser.Account.BankCode).Trim();
            string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

            if (string.IsNullOrEmpty(groupIdParam))
            {
                groupId = filter["fldGroupCode"].Trim();
            }
            else
            {
                groupId = groupIdParam;
            }

            ViewBag.PageTitle = groupDao.GetPageTitle(TaskIds.Task.EDIT);//Michelle 20200520
            ViewBag.Group = groupDao.GetGroup(groupId, "Update");

            //if ("N".Equals(systemProfile))
            //{
            ViewBag.AvailableTask = taskDao.ListAvailableTaskInGroup(groupId);
            ViewBag.SelectedTask = taskDao.ListSelectedTaskInGroup(groupId);
            //}
            //else
            //{
            //    ViewBag.AvailableTask = taskDao.ListAvailableTaskInGroupTemp(groupId);
            //    ViewBag.SelectedTask = taskDao.ListSelectedTaskInGroupTemp(groupId);
            //}
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.Task.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            CurrentUser.Account.Status = "U";
            string sTaskID = TaskIds.Task.INDEX;
            if (CurrentUser.HasTask(sTaskID))
            {
                CurrentUser.Account.TaskId = sTaskID;
            }

            try
            {
                List<string> taskIds = new List<string>();
                //Get value from system profile
                
                //string systemProfile = systemProfileDao.GetValueFromSystemProfile("TaskChecker", CurrentUser.Account.BankCode).Trim();
                string securityProfile = securityProfileDao.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();


                string groupId = col["fldGroupCode"].Trim(); // + "_" + CurrentUser.Account.BankCode;
                string groupdesc = col["fldGroupDesc"].Trim();

                //Start Audit Trail
                string beforeTask = "";
                string beforeTask2 = "";
                List<TaskModel> beforeLists = taskDao.ListSelectedTaskInGroup(groupId);

                if ("N".Equals(securityProfile))
                {
                    //start audit trail
                    string afterTask3 = "";
                    List<TaskModel> afterLists3 = taskDao.ListSelectedTaskInGroup(groupId);
                    foreach (var afterlist3 in afterLists3)
                    {
                        afterTask3 = afterTask3 + afterlist3.fldTaskDesc;
                    }
                    auditTrailDao.Log("Before Update =>- Group ID: " + groupId + " Task : " + afterTask3, CurrentUser.Account);
                    //End Audit Trail



                    int counter = 0;
                    if ((col["selectedTask"]) != null)
                    {
                        DataTable dtHubTemps = new DataTable();
                        dtHubTemps = taskDao.ListSelectedTask(groupId);

                        taskIds = col["selectedTask"].Split(',').ToList();

                        foreach (string taskId in taskIds)
                        {
                            taskDao.DeleteTaskNotSelected(groupId, taskId);
                        }

                        foreach (DataRow dtRow in dtHubTemps.Rows) //loop check if has record with delete in selected task
                        {
                            string taskId = dtRow["fldTaskId"].ToString();
                            if (taskDao.checkfordelete2(groupId, taskId) == true) //if a record is not existing in selected task 
                            {
                                counter++; //will do if theres record that remove from the selected task
                            }
                        }

                        foreach (string taskId in taskIds)
                        {
                            if (taskDao.CheckifGroupTaskExist(groupId, taskId) == true)
                            {
                            }
                            else
                            {
                                if ((taskDao.CheckGroupExist(groupId, taskId)))
                                {
                                    //update available task if exist
                                    taskDao.UpdateSelectedTaskId(CurrentUser.Account.UserId, groupId, taskId);
                                    counter++;
                                }
                                else
                                {
                                    //insert task if not exist
                                    taskDao.InsertSelectedTaskId(CurrentUser.Account.UserId, groupId, taskId, CurrentUser.Account.BankCode);
                                    counter++;
                                }
                            }
                        }

                    }
                    else
                    {
                        //check if there are records to delete                 
                        DataTable dtHubTempss = new DataTable();
                        dtHubTempss = taskDao.ListSelectedTask(groupId);

                        if (dtHubTempss.Rows.Count != 0)
                        {
                            taskDao.DeleteAllTaskInGroup(groupId);
                            counter++;
                        }
                        else
                        {
                            counter = 0;
                        }

                    }

                    if (counter != 0) //if theres a process that successfully happen
                    {
                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                    }
                    else
                    {
                        TempData["ErrorMsg"] = Locale.NoChanges;
                    }



                    //Start Audit Trail
                    string afterTask1 = "";
                    List<TaskModel> afterLists1 = taskDao.ListSelectedTaskInGroup(groupId);
                    foreach (var afterlist in afterLists1)
                    {
                        afterTask1 = afterTask1 + afterlist.fldTaskDesc;
                    }
                    auditTrailDao.Log("After Update =>- Group ID: " + groupId + " Task : " + afterTask1, CurrentUser.Account);
                    //string ActionDetails = SecurityAuditLogDao.TaskAssignment_EditTemplate(beforeTask2, afterTask1, "Edit", "Update", col);
                    //auditTrailDao.SecurityLog("Edit Group", ActionDetails, sTaskID, CurrentUser.Account);
                    //End Audit Trail


                }

                else  //with approve task checker='Y'
                {
                    if (taskDao.CheckTaskExist(col["fldGroupCode"]) == false)
                    {
                        foreach (var beforelist in beforeLists)
                        {
                            beforeTask2 = beforelist.fldTaskDesc + ", ";
                        }
                        //auditTrailDao.Log("Update Task Assignment  A, Before Update =>- Group ID: " + groupId + " Task : " + beforeTask2, CurrentUser.Account);
                        //End Audit Trail

                        int counter = 0;
                        if ((col["selectedTask"]) != null)
                        {
                            //remove unselected task
                            taskIds = col["selectedTask"].Split(',').ToList();

                            foreach (string taskId in taskIds)
                            {
                                //if (taskDao.CheckifGroupTaskExist(groupId, taskId) == false)
                                //{
                                if ((taskDao.CheckGroupExist(groupId, taskId)))
                                {
                                    //update available task if exist set fldapproved = Y
                                    taskDao.InsertSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId, CurrentUser.Account.BankCode, "CreateA");
                                    counter++; //will do if theres record to update from the selected task
                                }
                                else
                                {
                                    //update available task if exist set fldapproved = S
                                    taskDao.InsertSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId, CurrentUser.Account.BankCode, "CreateA");
                                    counter++; //will do if theres record to insert to the selected task
                                }
                                //}
                            }

                            foreach (var beforelist in beforeLists)
                            {
                                string taskId = beforelist.fldTaskId;
                                if (taskDao.checkifexistinselectedtasktemp(groupId, taskId) == false)
                                {
                                    taskDao.InsertSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId, CurrentUser.Account.BankCode, "CreateD");
                                    counter++; //will do if theres record to delete to the selected task
                                }
                            }
                        }
                        else
                        {
                            foreach (var beforelist in beforeLists)
                            {
                                string taskId = beforelist.fldTaskId;
                                taskDao.InsertSelectedTaskIdTemp(CurrentUser.Account.UserId, groupId, taskId, CurrentUser.Account.BankCode, "CreateD");
                            }

                            DataTable dtHubTemps = new DataTable();
                            dtHubTemps = taskDao.ListSelectedTask(groupId);

                            if (dtHubTemps.Rows.Count != 0)
                            {
                                counter++;
                            }
                            else
                            {
                                counter = 0;
                            }

                        }


                        if (counter != 0) //if theres a process that successfully happen
                        {
                            TempData["Notice"] = Locale.TaskSuccessfullyUpdatedApproval;
                        }
                        else
                        {
                            TempData["ErrorMsg"] = Locale.NoChanges;
                        }

                    }
                    else
                    {
                        TempData["Warning"] = Locale.TaskPendingApproval;
                    }


                    //Start Audit Trail
                    string afterTask2 = "";
                    List<TaskModel> afterLists2 = taskDao.ListSelectedTaskInGroup(groupId);
                    foreach (var afterlist2 in afterLists2)
                    {
                        afterTask2 = afterTask2 + afterlist2.fldTaskDesc + ",";
                    }
                    auditTrailDao.Log("Before Update =>- Group ID: " + groupId + " Task : " + afterTask2, CurrentUser.Account);
                    //string ActionDetails = SecurityAuditLogDao.TaskAssignment_EditTemplate(beforeTask2, afterTask2, "Edit", "Update", col);
                    //auditTrailDao.SecurityLog("Edit Task", ActionDetails, sTaskID, CurrentUser.Account);
                    //End Audit Trail
                }

                return RedirectToAction("Edit", new { groupIdParam = col["fldGroupCode"].Trim() });
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}