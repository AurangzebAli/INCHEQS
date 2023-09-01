using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
//using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.Message;
using log4net;
using System.Data;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class MessageController : BaseController
    {

        private readonly IMessageDao MessageDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        //private readonly ISecurityProfileDao securityProfileDao;
        //private static readonly ILog _log = LogManager.GetLogger(typeof(BankZoneController));
        public MessageController(IMessageDao MessageDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao/*, ISecurityProfileDao securityProfileDao*/)
        {
            this.pageConfigDao = pageConfigDao;
            this.MessageDao = MessageDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
            //this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.Message.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection col)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.Message.INDEX, "View_Message", "fldBroadcastMessage"),
            col);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string message = null)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                if (!string.IsNullOrEmpty(message))
                {
                    ViewBag.message = MessageDao.GetMessagebyId(message);
                }
                else
                {
                    if (col.AllKeys.Contains("fldBroadcastMessageId"))
                    {
                        ViewBag.message = MessageDao.GetMessagebyId(filter["fldBroadcastMessageId"]);
                    }
                    else
                    {
                        ViewBag.message = MessageDao.GetMessage(filter["fldBroadcastMessage"]);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try


            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.Message.INDEX;

                CurrentUser.Account.TaskId = sTaskId;

                //validate bank zone
                List<String> errorMessages = MessageDao.ValidateMessage(col, "Update");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        MessageModel before = MessageDao.GetMessagebyId(col["fldBroadcastMessageId"]);
                        auditTrailDao.Log("Edit Message - Before Update=> Message : " + before.fldBroadcastMessage, CurrentUser.Account);

                        //update Process
                        MessageDao.UpdateBroadcastMessageMaster(col);
                        TempData["Notice"] = Locale.RecordsuccesfullyUpdated;

                        MessageModel after = MessageDao.GetMessagebyId(col["fldBroadcastMessageId"]);
                        auditTrailDao.Log("Edit Message - After Update=> Message : " + after.fldBroadcastMessage, CurrentUser.Account);
                        //TempData["Notice"] = Locale.MessageSuccessfullyUpdated;

                        //string ActionDetails = MaintenanceAuditLogDao.Message_EditTemplate(col["fldBroadcastMessageId"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        //MessageModel before = MessageDao.GetMessagebyId(col["fldBroadcastMessageId"]);

                        MessageDao.CreateBroadcastMessageMasterTemp(col, CurrentUser.Account.UserId, null, "Update");

                        //MessageModel after = MessageDao.GetMessageTempbyId(col["fldBroadcastMessageId"]);

                        TempData["Notice"] = Locale.MessageSuccessfullyAddedtoApprovedUpdate;

                        auditTrailDao.Log("User Record Successfully Added to Temp Table for Check to Approve . Message : " + col["fldBroadcastMessage"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.Message_EditTemplate(col["fldBroadcastMessageId"], before, after, "Edit");
                        //auditTrailDao.SecurityLog("Edit Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Edit", new { message = col["fldBroadcastMessageId"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.Message.INDEX;

                CurrentUser.Account.TaskId = sTaskId;
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {

                        if ("N".Equals(systemProfile))
                        {
                            //string ActionDetails = MaintenanceAuditLogDao.Message_DeleteTemplate(arrResult, "Delete");
                            //auditTrailDao.SecurityLog("Delete Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);

                            MessageDao.DeleteBroadcastMessageMaster(arrResult);

                            TempData["Notice"] = Locale.RecordsuccesfullyDeleted;
                            auditTrailDao.Log("Delete - Message :  " + arrResult, CurrentUser.Account);

                        }
                        else
                        {
                            bool IsMessageTempExist = MessageDao.CheckBroadcastMessageMasterTempById(arrResult);

                            if (IsMessageTempExist == true)
                            {
                                TempData["Warning"] = Locale.MessageAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                //string ActionDetails = MaintenanceAuditLogDao.Message_DeleteTemplate(arrResult, "Delete");
                                //auditTrailDao.SecurityLog("Delete Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);

                                MessageDao.CreateBroadcastMessageMasterTemp(col, CurrentUser.Account.UserId, arrResult, "Delete");
                                TempData["Notice"] = Locale.MessageVerifyDelete;

                                auditTrailDao.Log("Add into Broadcast Message Temp table to Delete -  Message :  " + col["deleteBox"], CurrentUser.Account);
                            }
                        }

                    }

                }
                else
                {
                    TempData["Warning"] = Locale.Nodatawasselected;
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //_log.Error(ex);
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.Message.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.Message.INDEX;

                CurrentUser.Account.TaskId = sTaskId;

                MessageModel user = new MessageModel();
                //validate bank zone
                List<String> errorMessages = MessageDao.ValidateMessage(col, "Create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {

                        //Create Process
                        MessageDao.CreateBroadcastMessageMaster(col);

                        TempData["Notice"] = Locale.RecordsuccesfullyCreated;
                        auditTrailDao.Log("Add into Broadcast Message Master Table -  Message : " + col["fldBroadcastMessage"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.Message_AddTemplate(col["fldBroadcastMessage"], "Add", "N");
                        //auditTrailDao.SecurityLog("Add Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        MessageDao.CreateBroadcastMessageMasterTemp(col, CurrentUser.Account.UserId, null, "Create");
                        TempData["Notice"] = Locale.MessageSuccessfullyAddedtoApprovedCreate;
                        auditTrailDao.Log("Add into Broadcast Message Temporary Table - Message : " + col["fldBroadcastMessage"], CurrentUser.Account);

                        //string ActionDetails = MaintenanceAuditLogDao.Message_AddTemplate(col["fldBroadcastMessage"], "Add", "Y");
                        //auditTrailDao.SecurityLog("Add Broadcast Message", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}