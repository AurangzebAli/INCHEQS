using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankCharges;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Resources;
using INCHEQS.Models.SearchPageConfig.Services;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankChargesCheckerController : Controller
    {
        private IPageConfigDao pageConfigDao;
        private readonly IBankChargesDao bankchargesDao;
        protected readonly ISearchPageService searchPageService;

        public BankChargesCheckerController(IPageConfigDao pageConfigDao, IBankChargesDao bankchargesDao, ISearchPageService searchPageService)
        {

            this.pageConfigDao = pageConfigDao;
            this.bankchargesDao = bankchargesDao;
            this.searchPageService = searchPageService;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankChargesChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankChargesChecker.INDEX, "View_BankChargesChecker", "fldBankChargesType"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.VERIFY)] //Done
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
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
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);
                        string productCode = id.Substring(0, 3);
                        string bankCode = id.Substring(3, 3);
                        string bankcharges = id.Substring(6, 3);
                        string maxAmount = id.Split(';').Last();
                        string minAmount = id.Split(';').First().Remove(0, 9);

                        /*string maxamount = float.Parse(maxAmount).ToString("R");
                        float minamount = float.Parse(minAmount);*/

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIdsOCS.BankChargesChecker.INDEX:
                                if (action.Equals("A"))
                                {
                                    bankchargesDao.MoveToBankChargesFromTemp(productCode, bankCode, bankcharges, minAmount, maxAmount, "Create"); //Done
                                }
                                else if (action.Equals("D"))
                                {
                                    bankchargesDao.DeleteBankCharges(productCode, bankCode, bankcharges, minAmount, maxAmount); //Done
                                }
                                else if (action.Equals("U"))
                                {
                                    bankchargesDao.MoveToBankChargesFromTemp(productCode, bankCode, bankcharges, minAmount, maxAmount, "Update"); //Done
                                }

                                bankchargesDao.DeleteBankChargesTemp(productCode, bankCode, bankcharges, minAmount, maxAmount);

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

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
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
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);
                        string productCode = id.Substring(0, 3);
                        string bankCode = id.Substring(3, 3);
                        string bankcharges = id.Substring(6, 3);
                        string maxAmount = id.Split(';').Last();
                        string minAmount = id.Split(';').First().Remove(0, 9);

                        //Act based on task id
                        switch (taskId)
                        {
                            case TaskIdsOCS.BankChargesChecker.INDEX:
                                bankchargesDao.DeleteBankChargesTemp(productCode, bankCode, bankcharges, minAmount, maxAmount);
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

     
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.INDEX)] //DOne
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult BankCharges(FormCollection col)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);

            string bankCode = CurrentUser.Account.BankCode;
            string productCode = filter["fldProductCode"];
            string bankChargesType = filter["fldBankChargesType"].Substring(0, 3);
            string maxAmount = filter["fldChequeAmtMax"].Replace(",", "");
            string minAmount = filter["fldChequeAmtMin"].Replace(",", "");
            /*string Amount = filter["fldBankChargesAmt"].Replace(",", "");
            string Rate = filter["fldBankChargesRate"].Replace(",", "");*/

            ViewBag.BankCharges = bankchargesDao.GetBankCharges(bankCode, productCode, bankChargesType, minAmount, maxAmount, "");
            ViewBag.BankChargesTemp = bankchargesDao.GetBankCharges(bankCode, productCode, bankChargesType, minAmount, maxAmount, "temp");

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            try
            {
                string action = col["action"];
                string productCode = col["fldProductCode"];
                string bankCode = CurrentUser.Account.BankCode;
                string bankcharges = col["fldBankChargesType"];
                string maxAmount = col["fldChequeAmtMax"];
                string minAmount = col["fldChequeAmtMin"];

                if (action.Equals("A"))
                {
                    bankchargesDao.MoveToBankChargesFromTemp(productCode, bankCode, bankcharges, minAmount, maxAmount, "Create"); //Done
                }
                else if (action.Equals("D"))
                {
                    bankchargesDao.DeleteBankCharges(productCode, bankCode, bankcharges, minAmount, maxAmount); //Done
                }
                else if (action.Equals("U"))
                {
                    bankchargesDao.MoveToBankChargesFromTemp(productCode, bankCode, bankcharges, minAmount, maxAmount, "Update"); //Done
                }

                bankchargesDao.DeleteBankChargesTemp(productCode, bankCode, bankcharges, minAmount, maxAmount);

                TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankChargesChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            try
            {
                string productCode = col["fldProductCode"];
                string bankCode = CurrentUser.Account.BankCode;
                string bankcharges = col["fldBankChargesType"];
                string maxAmount = col["fldChequeAmtMax"];
                string minAmount = col["fldChequeAmtMin"];
                bankchargesDao.DeleteBankChargesTemp(productCode, bankCode, bankcharges, minAmount, maxAmount);
                TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*
       */

    }
}