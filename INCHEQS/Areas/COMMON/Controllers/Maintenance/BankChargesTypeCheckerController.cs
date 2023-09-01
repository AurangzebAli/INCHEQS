using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankChargesType;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankChargesTypeCheckerController : Controller
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IBankChargesTypeDao bankchargestypeDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankChargesTypeCheckerController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IBankChargesTypeDao bankchargestypeDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.bankchargestypeDao = bankchargestypeDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankChargesTypeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankChargesTypeChecker.INDEX, "View_BankChargesTypeChecker", "fldBankChargesType"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.INDEX)] //DOne
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult BankChargesType(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            ViewBag.BankChargesTypeMaster = bankchargestypeDao.GetBankChargesType(filter["fldBankChargesType"]);
            ViewBag.BankChargesTypeMasterTemp = bankchargestypeDao.GetBankChargesTypeTemp(filter["fldBankChargesType"]);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.BankChargesTypeChecker.VERIFY;
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
                            case TaskIdsOCS.BankChargesTypeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    //Add Audit Trial
                                    bankchargestypeDao.MoveToBankChargesTypeMasterFromTemp(id, "Create"); //Done
                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                    //Add Audit Trial
                                    bankchargestypeDao.DeleteBankChargesTypeMaster(id, CurrentUser.Account.BankCode); //Done
                                }
                                else if (action.Equals("U"))
                                {
                                    BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(id);
                                    //auditTrailDao.Log("Edit Bank Charges Type - Before Update=> Bank Charges Type : " + before.fldBankChargesType + " Bank Charges Desc : " + before.fldBankChargesDesc, CurrentUser.Account);

                                    bankchargestypeDao.MoveToBankChargesTypeMasterFromTemp(id, "Update"); //Done

                                    BankChargesTypeModel after = bankchargestypeDao.GetBankChargesType(id);
                                    //auditTrailDao.Log("Edit Bank Charges Type - After Update=> Bank Charges Type : " + after.fldBankChargesType + " Bank Charges Desc : " + after.fldBankChargesDesc, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                bankchargestypeDao.DeleteBankChargesTypeMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.BankChargesTypeChecker.VERIFY;

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
                            case TaskIdsOCS.BankChargesTypeChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_AddTemplate(id, "Reject", "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_DeleteTemplate(id, "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("U"))
                                {
                                    BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(id);
                                    BankChargesTypeModel after = bankchargestypeDao.GetBankChargesTypeTemp(id);
                                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_EditTemplate(id, before, after, "Reject");
                                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                bankchargestypeDao.DeleteBankChargesTypeMasterTemp(id);
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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["Action"];
                string id = col["fldBankChargesType"];
                string sTaskId = TaskIdsOCS.BankChargesTypeChecker.VERIFY;

                if (action.Equals("A"))
                {
                    //Add Audit Trial
                    bankchargestypeDao.MoveToBankChargesTypeMasterFromTemp(id, "Create");
                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                    //Add Audit Trial
                    bankchargestypeDao.DeleteBankChargesTypeMaster(id, CurrentUser.Account.BankCode);
                }
                else if (action.Equals("U"))
                {
                    BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(id);
                    //auditTrailDao.Log("Edit Bank Charges Type - Before Update=> Bank Charges Code : " + before.fldBankChargesType + " Bank Charges Desc : " + before.fldBankChargesDesc, CurrentUser.Account);

                    bankchargestypeDao.MoveToBankChargesTypeMasterFromTemp(id, "Update");

                    BankChargesTypeModel after = bankchargestypeDao.GetBankChargesType(id);
                    //auditTrailDao.Log("Edit Bank Charges - After Update=> Bank Charges Code : " + after.fldBankChargesType + " Bank Charges Desc : " + after.fldBankChargesDesc, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                }
                bankchargestypeDao.DeleteBankChargesTypeMasterTemp(id);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesTypeChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string id = col["fldBankChargesType"];
                string action = col["Action"];
                string sTaskId = TaskIdsOCS.BankChargesTypeChecker.VERIFY;
                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BankChargesTypeModel before = bankchargestypeDao.GetBankChargesType(id);
                    BankChargesTypeModel after = bankchargestypeDao.GetBankChargesTypeTemp(id);
                    string ActionDetails = MaintenanceAuditLogDao.BankChargesTypeChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Charges Type", ActionDetails, sTaskId, CurrentUser.Account);
                }
                bankchargestypeDao.DeleteBankChargesTypeMasterTemp(id);
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