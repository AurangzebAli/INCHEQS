using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.Message;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class MessageCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IMessageDao MessageDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public MessageCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IMessageDao MessageDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.MessageDao = MessageDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.MessageChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.MessageChecker.INDEX, "View_MessageChecker", "fldBroadcastMessage"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Message(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            ViewBag.MessageMaster = MessageDao.GetMessage(filter["fldBroadcastMessage"]);
            ViewBag.MessageMasterTemp = MessageDao.GetMessageTemp(filter["fldBroadcastMessage"]);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.MessageChecker.VERIFY;
                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);
                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIdsOCS.MessageChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    MessageDao.MoveToBroadcastMessageMasterFromTemp(id, "Create");

                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);

                                    //Add Audit Trial
                                    MessageDao.DeleteBroadcastMessageMaster(id);
                                }
                                else if (action.Equals("U"))
                                {
                                    MessageModel before = MessageDao.GetMessagebyId(id);
                                    auditTrailDao.Log("Edit Broadcast Message - Before Update=> Message : " + before.fldBroadcastMessage, CurrentUser.Account);

                                    MessageDao.MoveToBroadcastMessageMasterFromTemp(id, "Update");

                                    MessageModel after = MessageDao.GetMessagebyId(id);
                                    auditTrailDao.Log("Edit Broadcast Message - After Update=> Message : " + after.fldBroadcastMessage, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                MessageDao.DeleteBroadcastMessageMasterTemp(id);
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.MessageChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIdsOCS.MessageChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    MessageModel before = MessageDao.GetMessagebyId(id);

                                    MessageModel after = MessageDao.GetMessageTempbyId(id);

                                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                MessageDao.DeleteBroadcastMessageMasterTemp(id);
                                break;
                        }
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["Action"];
                string id = col["fldBroadcastMessageId"];
                string sTaskId = TaskIdsOCS.MessageChecker.VERIFY;

                if (action.Equals("A"))
                {
                    //Add Audit Trial
                    MessageDao.MoveToBroadcastMessageMasterFromTemp(id, "Create");

                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);

                    //Add Audit Trial
                    MessageDao.DeleteBroadcastMessageMaster(id);
                }
                else if (action.Equals("U"))
                {
                    MessageModel before = MessageDao.GetMessagebyId(id);
                    auditTrailDao.Log("Edit Broadcast Message - Before Update=> Message : " + before.fldBroadcastMessage, CurrentUser.Account);
                    
                    MessageDao.MoveToBroadcastMessageMasterFromTemp(id, "Update");
                    
                    MessageModel after = MessageDao.GetMessagebyId(id);
                    auditTrailDao.Log("Edit Broadcast Message - After Update=> Message : " + after.fldBroadcastMessage, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                }
                MessageDao.DeleteBroadcastMessageMasterTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.MessageChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string id = col["fldBroadcastMessageId"];
                string sTaskId = TaskIdsOCS.MessageChecker.VERIFY;
                string action = col["Action"];

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    MessageModel before = MessageDao.GetMessagebyId(id);

                    MessageModel after = MessageDao.GetMessageTempbyId(id);

                    string ActionDetails = MaintenanceAuditLogDao.MessageChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                }
                MessageDao.DeleteBroadcastMessageMasterTemp(id);
                TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}