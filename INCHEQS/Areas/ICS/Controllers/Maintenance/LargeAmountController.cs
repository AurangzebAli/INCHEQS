using INCHEQS.Areas.ICS.Models.LargeAmount;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;


namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
    public class LargeAmountController : BaseController {

        private ICSILargeAmountDao largeAmountDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IPageConfigDaoICS pageConfigDao;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        public LargeAmountController(ICSILargeAmountDao largeAmountDao, IAuditTrailDao auditTrailDao, IPageConfigDaoICS pageConfigDao, ISystemProfileDao systemProfileDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {

            this.largeAmountDao = largeAmountDao;
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.systemProfileDao = systemProfileDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmount.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.LargeAmount.INDEX));
            ViewBag.PageTitle = largeAmountDao.GetPageTitle(TaskIdsICS.LargeAmount.INDEX);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmount.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.LargeAmount.INDEX, "View_LargeAmountLimit", null, "fldBankCode =@fldBankCode", new[] { new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmount.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit()
        {
            //ViewBag.BigAmount = largeAmountDao.GetLargeAmountLimit();
            ViewBag.BigAmount = largeAmountDao.SetLargeAmountLimit();
            if (largeAmountDao.CheckLargeAmountLimitExist(CurrentUser.Account.BankCode) == true)
            {
                ViewBag.PageTitle = largeAmountDao.GetPageTitle(TaskIdsICS.LargeAmount.UPDATE);
            }
            else
            {
                string pagetitle = largeAmountDao.GetPageTitle(TaskIdsICS.LargeAmount.UPDATE);
                ViewBag.PageTitle = pagetitle.Replace("(Edit)", "(Add)");
            }
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.LargeAmount.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col) {           

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsICS.LargeAmount.UPDATE;
            List<string> errorMessages = largeAmountDao.Validate(col);
            string largeAmount = col["txtAmount"].Replace(",","");

            if ((errorMessages.Count > 0)) {
                TempData["ErrorMsg"] = errorMessages;
            } else {

                if ("N".Equals(systemProfile))
                {
                    if (largeAmountDao.NoChangesinLargeAmountLimit(col, largeAmount) == true)
                    {
                        TempData["ErrorMsg"] = Locale.NoChanges;
                        goto Done;
                    }
                    else
                    {
                        LargeAmountModel before = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);

                        //auditTrailDao.Log("Edit Large Amount - Before Update=> Amount :" + before.fldAmount, CurrentUser.Account);

                        if (largeAmountDao.CheckLargeAmountLimitExist(CurrentUser.Account.BankCode) == true)
                        {
                            largeAmountDao.UpdateLargeAmountLimit(largeAmount);
                        }
                        else
                        {
                            largeAmountDao.CreateLargeAmountLimit(largeAmount, CurrentUser.Account.UserId);
                        }

                        LargeAmountModel after = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);

                        //auditTrailDao.Log("Edit Large Amount - After Update=> Amount :" + after.fldAmount, CurrentUser.Account);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;
                        string ActionDetails = MaintenanceAuditLogDao.LargeAmountLimit_EditTemplate(CurrentUser.Account.BankCode, before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Large Amount Limit", ActionDetails, sTaskId, CurrentUser.Account);
                    }

                }
                else
                {
                    if (largeAmountDao.CheckLargeAmountLimitinTempExist(CurrentUser.Account.BankCode) == true)
                    {
                        TempData["Warning"] = Locale.LargeAmountPendingforApproval;
                        goto Done;
                    }
                    else
                    {
                        if (largeAmountDao.NoChangesinLargeAmountLimit(col, largeAmount) == true)
                        {
                            TempData["ErrorMsg"] = Locale.NoChanges;
                            goto Done;
                        }
                        else
                        {
                            LargeAmountModel before = largeAmountDao.GetLargeAmountLimit(CurrentUser.Account.BankCode);

                           // auditTrailDao.Log("Edit Large Amount - Before Update=> Amount :" + before.fldAmount, CurrentUser.Account);

                            largeAmountDao.InsertLargeAmountLimitTemp(largeAmount,CurrentUser.Account.UserId);

                            //auditTrailDao.Log("Add into Temporary record to Update - Amount :" + largeAmount, CurrentUser.Account);
                            TempData["Notice"] = Locale.LargeAmountUpdateVerify;

                            //auditTrailDao.Log("Edit Large Amount - After Update=> Amount :" + largeAmount, CurrentUser.Account);

                            LargeAmountModel after = MaintenanceAuditLogDao.GetLargeAmountTemp(CurrentUser.Account.BankCode);
                            string ActionDetails = MaintenanceAuditLogDao.LargeAmountLimit_EditTemplate(CurrentUser.Account.BankCode, before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit Large Amount Limit", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }
                }
            }
            Done:
            return RedirectToAction("Edit");
        }

    }
}