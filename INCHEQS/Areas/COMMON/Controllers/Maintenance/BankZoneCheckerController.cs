using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankZone;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankZoneCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IBankZoneDao bankzoneDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankZoneCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IBankZoneDao bankzoneDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.bankzoneDao = bankzoneDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankZoneChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankZoneChecker.INDEX, "View_BankZoneChecker", "fldBankZoneCode"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult BankZone(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            ViewBag.BankZoneMaster = bankzoneDao.GetBankZone(filter["fldBankZoneCode"]);
            ViewBag.BankZoneMasterTemp = bankzoneDao.GetBankZoneTemp(filter["fldBankZoneCode"]);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.BankZoneChecker.VERIFY;
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
                            case TaskIdsOCS.BankZoneChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    bankzoneDao.MoveToBankZoneMasterFromTemp(id, "Create");
                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                    //Add Audit Trial
                                    bankzoneDao.DeleteBankZoneMaster(id, CurrentUser.Account.BankCode);
                                }
                                else if (action.Equals("U"))
                                {
                                    BankZoneModel before = bankzoneDao.GetBankZone(id);
                                    auditTrailDao.Log("Edit Bank Zone - Before Update=> Bank Zone Code : " + before.fldBankZoneCode + " Bank Zone Desc : " + before.fldBankZoneDesc, CurrentUser.Account);

                                    bankzoneDao.MoveToBankZoneMasterFromTemp(id, "Update");

                                    BankZoneModel after = bankzoneDao.GetBankZone(id);
                                    auditTrailDao.Log("Edit Bank Zone - After Update=> Bank Zone Code : " + after.fldBankZoneCode + " Bank Zone Desc : " + after.fldBankZoneDesc, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                bankzoneDao.DeleteBankZoneMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.BankZoneChecker.VERIFY;

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
                            case TaskIdsOCS.BankZoneChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    BankZoneModel before = bankzoneDao.GetBankZone(id);
                                    auditTrailDao.Log("Edit Bank Zone - Before Update=> Bank Zone Code : " + before.fldBankZoneCode + " Bank Zone Desc : " + before.fldBankZoneDesc, CurrentUser.Account);
                                    BankZoneModel after = bankzoneDao.GetBankZoneTemp(id);
                                    auditTrailDao.Log("Edit Bank Zone - After Update=> Bank Zone Code : " + after.fldBankZoneCode + " Bank Zone Desc : " + after.fldBankZoneDesc, CurrentUser.Account);
                                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                bankzoneDao.DeleteBankZoneMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["Action"];
                string id = col["fldBankZoneCode"];
                string sTaskId = TaskIdsOCS.BankZoneChecker.VERIFY;

                if (action.Equals("A"))
                {
                    //Add Audit Trial
                    bankzoneDao.MoveToBankZoneMasterFromTemp(id, "Create");
                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                    //Add Audit Trial
                    bankzoneDao.DeleteBankZoneMaster(id, CurrentUser.Account.BankCode);
                }
                else if (action.Equals("U"))
                {
                    BankZoneModel before = bankzoneDao.GetBankZone(id);
                    auditTrailDao.Log("Edit Bank Zone - Before Update=> Bank Zone Code : " + before.fldBankZoneCode + " Bank Zone Desc : " + before.fldBankZoneDesc, CurrentUser.Account);

                    bankzoneDao.MoveToBankZoneMasterFromTemp(id, "Update");

                    BankZoneModel after = bankzoneDao.GetBankZone(id);
                    auditTrailDao.Log("Edit Bank Zone - After Update=> Bank Zone Code : " + after.fldBankZoneCode + " Bank Zone Desc : " + after.fldBankZoneDesc, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                bankzoneDao.DeleteBankZoneMasterTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankZoneChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string id = col["fldBankZoneCode"];
                string sTaskId = TaskIdsOCS.BankZoneChecker.VERIFY;
                string action = col["Action"];
                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BankZoneModel before = bankzoneDao.GetBankZone(id);
                    auditTrailDao.Log("Edit Bank Zone - Before Update=> Bank Zone Code : " + before.fldBankZoneCode + " Bank Zone Desc : " + before.fldBankZoneDesc, CurrentUser.Account);
                    BankZoneModel after = bankzoneDao.GetBankZoneTemp(id);
                    auditTrailDao.Log("Edit Bank Zone - After Update=> Bank Zone Code : " + after.fldBankZoneCode + " Bank Zone Desc : " + after.fldBankZoneDesc, CurrentUser.Account);
                    string ActionDetails = MaintenanceAuditLogDao.BankZoneChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Zone Code", ActionDetails, sTaskId, CurrentUser.Account);
                }
                bankzoneDao.DeleteBankZoneMasterTemp(id);
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