using INCHEQS.Areas.ATV.Models.Truncation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.Truncation
{
    public class TruncationController : BaseController
    {
        public readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private ITruncationDao truncationDao;
        protected readonly ISearchPageService searchPageService;
        private readonly ISecurityProfileDao securityProfileDao;


        public TruncationController(IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISecurityProfileDao securityProfileDao, ITruncationDao truncationDao)
        {
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
            this.searchPageService = searchPageService;
            this.securityProfileDao = securityProfileDao;
            this.truncationDao = truncationDao;
        }


        [CustomAuthorize(TaskIds = TaskIdsHC.ATVTruncation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            try
            {
                ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsHC.ATVTruncation.INDEX));
                ViewBag.fldMaxAmount = truncationDao.getMaxAmount(ViewBag.SearchPage.FormFields[0].fieldDefaultValue);
            }
            catch (Exception ex)
            {
                string remarks = "[Truncation] : " + ex.Message;
                TempData["ErrorMsg"] = ex.Message;
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            return View();

        }


        [CustomAuthorize(TaskIds = TaskIdsHC.ATVTruncation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string remarks = "";
            try
            {
                remarks = "[Truncation] : Retrieve ATV Truncation ";
                ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsHC.ATVTruncation.INDEX, "View_Truncation", "fldCreatedTimeStamp DESC"), collection);
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            catch (Exception ex)
            {
                remarks = "[Truncation] : " + ex.Message;
                TempData["ErrorMsg"] = ex.Message;
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsHC.ATVTruncation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Calculate(FormCollection collection)
        {
            string remarks = "";
            try
            {
                int TotItemCount = 0;
                long totalfldAmount = 0;
                string fldClearDate = collection["fldClearDate"];
                string fldPercentage = collection["fldPercentage"];
                string fldStatisticRange = collection["fldStatisticRange"];
                string fldTruncateChequeAmountMin = collection["fldTruncateChequeAmountMin"];
                string fldTruncateChequeAmountMax = collection["fldTruncateChequeAmountMax"];

                TruncationModel TotalRecModel = truncationDao.GetTotalRec(fldClearDate);
                TruncationModel TotalPendingRecModel = truncationDao.GetTotPending(fldClearDate);

                if (TotalRecModel.fldTotInward > 0)
                {
                    TotItemCount = Convert.ToInt32(Math.Round(double.Parse(fldPercentage) / 100 * TotalRecModel.fldTotInward));
                }

                List<TruncationModel> listModel = truncationDao.CalCulateRecord(fldClearDate, TotItemCount.ToString());
                for (int i = 0; i < listModel.Count; i++)
                {
                    totalfldAmount = totalfldAmount + Int64.Parse(listModel[i].fldAmount);
                }

                ViewBag.fldClearDate = fldClearDate;
                ViewBag.TotalRecModel = TotalRecModel;
                ViewBag.TotalPendingRecModel = TotalPendingRecModel;
                ViewBag.fldPercentage = fldPercentage;
                ViewBag.totalfldAmount = totalfldAmount.ToString();
                ViewBag.fldStatisticRange = fldStatisticRange;
                ViewBag.Model = listModel;

                if (listModel.Count > 0)
                {
                    ViewBag.fldMinAmount = listModel[0].fldAmount;
                    ViewBag.fldMaxAmount = listModel[listModel.Count - 1].fldAmount;
                }
                else
                {
                    ViewBag.fldMinAmount = "0";
                    ViewBag.fldMaxAmount = "0";
                }

                remarks = "[Truncation] : Auto Approve Threshold - Calculate Statistic ";
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            catch (Exception ex)
            {
                remarks = "[Truncation] : " + ex.Message;
                TempData["ErrorMsg"] = ex.Message;
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            return PartialView("_Calculate");
        }


        [CustomAuthorize(TaskIds = TaskIdsHC.ATVTruncation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult Truncation(FormCollection collection)
        {
            string remarks = "";
            try
            {
                string fldClearDate = collection["fldClearDate"];
                string fldPercentage = collection["fldPercentage"];
                string fldStatisticRange = collection["fldStatisticRange"];
                string fldTruncateChequeAmountMin = collection["fldTruncateChequeAmountMin"];
                string fldTruncateChequeAmountMax = collection["fldTruncateChequeAmountMax"];

                truncationDao.Truncate(fldClearDate, fldTruncateChequeAmountMin, fldTruncateChequeAmountMax);

                remarks = "[Truncation] - Update truncate cheque successfully.";
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");

            }
            catch (Exception ex)
            {
                remarks = "[Truncation] : " + ex.Message;
                TempData["ErrorMsg"] = ex.Message;
                auditTrailDao.InsertAuditLog(Int32.Parse(CurrentUser.Account.UserId), CurrentUser.Account.BankCode, TaskIdsHC.ATVTruncation.INDEX, remarks, "", "General");
            }
            return RedirectToAction("Index");
        }
    }
}