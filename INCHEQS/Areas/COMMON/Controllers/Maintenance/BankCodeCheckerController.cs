// bank of Malaysia
//using INCHEQS.BNM.BankCode;

// bank of Philippines
//using INCHEQS.PCHC.BankCode;

//using INCHEQS.Areas.ICS.Models.SystemProfile;
//using INCHEQS.Models.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCode;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankCodeCheckerController : BaseController
    {

        private readonly ISystemProfileDao systemProfileDao;
        private IBankCodeDao bankCodeDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankCodeCheckerController(ISystemProfileDao systemProfileDao, IBankCodeDao bankCodeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.bankCodeDao = bankCodeDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankCodeChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankCodeChecker.INDEX, "View_BankCodeChecker", "", ""), collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Edit(FormCollection col, string bankCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);

            string bankcode = "";
            if (string.IsNullOrEmpty(bankCodeParam))
            {
                bankcode = filter["fldBankCode"].Trim();
            }
            else
            {
                bankcode = bankCodeParam;
            }
            DataTable dataTable = await bankCodeDao.getBankCodeAsync(bankcode);
            DataTable dataTable2 = await bankCodeDao.getBankCodeTempAsync(bankcode);

            if ((dataTable2.Rows.Count > 0))
            {
                ViewBag.BankCodeTemp = dataTable2.Rows[0];
                ViewBag.BankType = await bankCodeDao.getBankTypeAsync();
            }

            if ((dataTable.Rows.Count > 0))
            {
                ViewBag.BankCode = dataTable.Rows[0];
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [HttpPost]
        public ActionResult Approve(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string id = arrResult.Remove(0, 1);
                        string taskId = TaskIds.BankCodeChecker.INDEX;

                        if (action.Equals("A"))
                        {
                            bankCodeDao.CreateBankCodeinMain(id);

                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Code", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            BankCodeModel objBankCode = bankCodeDao.GetBankCodeData(id);

                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Code", ActionDetails, taskId, CurrentUser.Account);

                            bankCodeDao.DeleteInBankCode(id);

                            //auditTrailDao.Log("Deleted BankCode from Main - Bank Code :" + objBankCode.bankCode + ", Bank Desc: " + objBankCode.bankDesc + ", Bank Abbreviation : " + objBankCode.bankAbb + ",Bank Active ?  : "  + ", Bank Status : " + objBankCode.status + ", Bank UpdateBy : " + objBankCode.updateBy, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            BankCodeModel before = bankCodeDao.GetBankCodeData(id);
                            //auditTrailDao.Log("Deleted BankCode Created Before - Bank Code :" + before.bankCode + ", Bank Desc: " + before.bankDesc + ", Bank Abbreviation : " + before.bankAbb + ",Bank Active ?  : " + ", Bank Status : " + before.status + ", Bank UpdateBy : " + before.updateBy, CurrentUser.Account);

                            bankCodeDao.UpdateBankCodeToMainById(id);

                            BankCodeModel after = bankCodeDao.GetBankCodeData(id);
                            //auditTrailDao.Log("Deleted BankCode - Bank Code :" + after.bankCode + ", Bank Desc: " + after.bankDesc + ", Bank Abbreviation : " + after.bankAbb + ",Bank Active ?  : " + ", Bank Status : " + after.status + ", Bank UpdateBy : " + after.updateBy, CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_EditTemplate(id, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Code", ActionDetails, taskId, CurrentUser.Account);

                        }

                        bankCodeDao.DeleteBankCodeinTemp(id);
                        auditTrailDao.Log("Approve Bank Code - Task Assigment :" + taskId + " Bank Code : " + id, CurrentUser.Account);

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

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [HttpPost]
        public ActionResult Reject(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string id = arrResult.Remove(0, 1);
                        string taskId = TaskIds.BankCodeChecker.INDEX;

                        if (action.Equals("A"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            BankCodeModel before = bankCodeDao.GetBankCodeData(id);

                            BankCodeModel after = MaintenanceAuditLogDao.GetBankCodeDataTemp(id);

                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                        }
                        bankCodeDao.DeleteBankCodeinTemp(id);
                        //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Bank Code : " + id, CurrentUser.Account);

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

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [HttpPost]
        public ActionResult Approve2(FormCollection col)
        {
            try
            {
                string action = col["fldAction"];
                string id = col["bankCode"];
                string taskId = TaskIds.BankCodeChecker.INDEX;

                if (action.Equals("A"))
                {
                    bankCodeDao.CreateBankCodeinMain(id);

                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_AddTemplate(id, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Code", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    BankCodeModel objBankCode = bankCodeDao.GetBankCodeData(id);

                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_DeleteTemplate(id, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Code", ActionDetails, taskId, CurrentUser.Account);

                    bankCodeDao.DeleteInBankCode(id);

                    //auditTrailDao.Log("Deleted Bank Code from Main - Bank Code :" + objBankCode.bankCode + ", Bank Desc: " + objBankCode.bankDesc + ", Bank Abbreviation : " + objBankCode.bankAbb + ",Bank Active ?  : "  + ", Bank Status : " + objBankCode.status + ", Bank UpdateBy : " + objBankCode.updateBy, CurrentUser.Account);

                }
                else if (action.Equals("U"))
                {
                    BankCodeModel before = bankCodeDao.GetBankCodeData(id);
                    //auditTrailDao.Log("Deleted Bank Code Created Before - Bank Code :" + before.bankCode + ", Bank Desc: " + before.bankDesc + ", Bank Abbreviation : " + before.bankAbb + ",Bank Active ?  : " + ", Bank Status : " + before.status + ", Bank UpdateBy : " + before.updateBy, CurrentUser.Account);

                    bankCodeDao.UpdateBankCodeToMainById(id);

                    BankCodeModel after = bankCodeDao.GetBankCodeData(id);
                    //auditTrailDao.Log("Deleted Bank Code - Bank Code :" + after.bankCode + ", Bank Desc: " + after.bankDesc + ", Bank Abbreviation : " + after.bankAbb + ",Bank Active ?  : " + ", Bank Status : " + after.status + ", Bank UpdateBy : " + after.updateBy, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_EditTemplate(id, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Profile", ActionDetails, taskId, CurrentUser.Account);

                }

                bankCodeDao.DeleteBankCodeinTemp(id);


                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.BankCodeChecker.INDEX)]
        [HttpPost]
        public ActionResult Reject2(FormCollection col)
        {
            try
            {
                string action = col["fldAction"];
                string id = col["bankCode"];
                string taskId = TaskIds.BankCodeChecker.INDEX;

                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_AddTemplate(id, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BankCodeModel before = bankCodeDao.GetBankCodeData(id);
                    BankCodeModel after = MaintenanceAuditLogDao.GetBankCodeDataTemp(id);
                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_EditTemplate(id, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankProfileChecker_DeleteTemplate(id, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Code", ActionDetails, taskId, CurrentUser.Account);
                }
                bankCodeDao.DeleteBankCodeinTemp(id);
                auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Bank Code : " + id, CurrentUser.Account);


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