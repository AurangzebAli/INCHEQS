using INCHEQS.Areas.ICS.Models.LargeAmount;
using INCHEQS.Security.AuditTrail;
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
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;


namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class LargeAmountCheckerController : BaseController
    {
        private ICSILargeAmountDao largeAmountDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IPageConfigDaoICS pageConfigDao;

        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;

        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        public LargeAmountCheckerController(ICSILargeAmountDao largeAmountDao, ISystemProfileDao systemProfileDao,IAuditTrailDao auditTrailDao, IPageConfigDaoICS pageConfigDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.largeAmountDao = largeAmountDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }        

        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmountChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LargeAmountChecker.INDEX));
            ViewBag.PageTitle = largeAmountDao.GetPageTitle(TaskIdsICS.LargeAmountChecker.INDEX);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmountChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LargeAmountChecker.INDEX, "View_ApprovedLargeAmountLimitChecker", "fldLargeAmt", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmountChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIdsICS.LargeAmountChecker.VERIFY;
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        tmpArr = arrResult.Split(':');
                        string action = tmpArr[0].Trim().ToString();
                        string taskId = tmpArr[1].Trim().ToString();
                        string id = tmpArr[2].Trim().ToString();

                        if (action.Equals("E") || action.Equals("U"))
                        {
                            LargeAmountModel before = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);
                            largeAmountDao.UpdateLargeAmountLimitTemp(CurrentUser.Account.BankCode);

                            if (largeAmountDao.CheckLargeAmountLimitExist(CurrentUser.Account.BankCode) == true)
                            {

                                largeAmountDao.UpdateLargeAmountLimitFromTemp(CurrentUser.Account.BankCode);
                            }
                            else
                            {
                                largeAmountDao.CreateLargeAmountLimitFromTemp(CurrentUser.Account.BankCode);
                            }

                            LargeAmountModel after = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);
                            largeAmountDao.DeleteLargeAmountLimitTemp(CurrentUser.Account.BankCode);
                            //auditTrailDao.Log("Approve- Update Large Amount Limit :" + id + " ", CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.LargeAmountLimitChecker_EditTemplate(CurrentUser.Account.BankCode, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Large Amount Limit", ActionDetails, sTaskId, CurrentUser.Account);
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

        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmountChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                string sTaskId = TaskIdsICS.LargeAmountChecker.VERIFY;
                List<string> arrResults = new List<string>();
                String[] tmpArr = new String[3];

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        tmpArr = arrResult.Split(':');
                        string action = tmpArr[0].Trim().ToString();
                        string taskId = tmpArr[1].Trim().ToString();
                        string id = tmpArr[2].Trim().ToString();

                        LargeAmountModel before = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);
                        LargeAmountModel after = MaintenanceAuditLogDao.GetLargeAmountTemp(CurrentUser.Account.BankCode);
                        largeAmountDao.UpdateLargeAmountLimitTemp(CurrentUser.Account.BankCode);
                        largeAmountDao.DeleteLargeAmountLimitTemp(CurrentUser.Account.BankCode);
                       // auditTrailDao.Log("Reject Update Large Amount Limit :" + id + " ", CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.LargeAmountLimit_EditTemplate(CurrentUser.Account.BankCode, before, after, "Reject");
                        auditTrailDao.SecurityLog("Reject Large Amount Limit", ActionDetails, sTaskId, CurrentUser.Account);

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
    }
}
