using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
//using INCHEQS.Security.User;

using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
//using INCHEQS.Areas.ICS.Models.SystemProfile;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.BankCharges;
using log4net;
using System.Data;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{

    public class BankChargesController : BaseController
    {

        private readonly IBankChargesDao bankchargesDao;

        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        public BankChargesController(IBankChargesDao bankchargesDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.bankchargesDao = bankchargesDao;
     
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
        }
        
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.BankCharges.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.BankCharges.INDEX, "View_BankCharges", "fldProductCode"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.CREATE)] //Done
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            try
            {
                ViewBag.product = bankchargesDao.GetBankChargesTypeAndProductCodeList("tblProductMaster");
                ViewBag.bankcharges = bankchargesDao.GetBankChargesTypeAndProductCodeList("tblBankChargesTypeMaster");
                return View();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.SAVECREATE)] //Done
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            try
            {
                //Get value from system profile
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();

                //validate bank zone
                List<String> errorMessages = bankchargesDao.ValidateBankCharges(col, "Create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {

                        //Create Process
                        bankchargesDao.CreateBankCharges(col, CurrentUser.Account.BankCode); //Done
                        TempData["Notice"] = Locale.BankChargesSuccessfullyCreated;
                    }
                    else
                    {
                        bool IsBankChargesTempExist = bankchargesDao.ValidateExistingBankCharges(col["fldProductCode"], CurrentUser.Account.BankCode, col["fldBankChargesType"], col["fldChequeAmtMin"].Replace(",", ""), col["fldChequeAmtMax"].Replace(",", ""), "A", "temp");
                        if (IsBankChargesTempExist == true)
                        {
                            TempData["Warning"] = Locale.BankChargesExist;
                        }
                        else
                        {
                            bankchargesDao.CreateBankChargesTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Create", "", "", "", ""); //Done
                            TempData["Notice"] = Locale.BankChargesSuccessfullyAddedtoApprovedCreate;
                        }
                    }
                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.DELETE)] //Done
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                if (col != null & col["deleteBox"] != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string maxAmount = arrResult.Split(';').Last();
                        string otherAmount = arrResult.Split(';').First();
                        string bankcode = otherAmount.Substring(0, 3);
                        string productCode = otherAmount.Substring(3, 3);
                        string bankchargestype = otherAmount.Substring(6, 3);
                        string minAmount = otherAmount.Remove(0, 9);

                        if ("N".Equals(systemProfile))
                        {
                            bankchargesDao.DeleteBankCharges(productCode, bankcode, bankchargestype, minAmount, maxAmount); //Done
                            TempData["Notice"] = Locale.SuccessfullyDeleted;

                        }
                        else
                        {
                            bool IsBankChargesTempExist = bankchargesDao.CheckBankChargesTempById(productCode, bankcode, bankchargestype, minAmount, maxAmount); //Done

                            if (IsBankChargesTempExist == true)
                            {
                                TempData["Warning"] = Locale.BankChargesAlreadyExiststoDeleteorUpdate;
                            }
                            else
                            {
                                bankchargesDao.CreateBankChargesTemp(col, bankcode,CurrentUser.Account.UserId, "Delete", productCode, maxAmount, minAmount, bankchargestype); //Done
                                TempData["Notice"] = Locale.BankChargesVerifyDelete;
                            }
                        }

                    }

                }
                else

                    TempData["Warning"] = Locale.Nodatawasselected;

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Edit
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.EDIT)]    //Done
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection)
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);

                //ViewBag.bankcharges = bankchargesDao.GetBankChargesType(filter["fldBankChargesType"]);


                if (filter == null)
                {
                    ViewBag.productCode = collection["fldProductCode"];
                    ViewBag.bankChargesType = collection["fldBankChargesType"].Substring(0, 3);
                    ViewBag.currMaxAmount = collection["fldChequeAmtMax"].Replace(",", "");
                    ViewBag.currMinAmount = collection["fldChequeAmtMin"].Replace(",", "");
                    ViewBag.Amount = collection["fldBankChargesAmount"].Replace(",", "");
                    ViewBag.Rate = collection["fldBankChargesRate"].Replace(",", "");
                }
                else
                {
                    ViewBag.productCode = filter["fldProductCode"];
                    ViewBag.bankChargesType = filter["fldBankChargesType"].Substring(0, 3);
                    ViewBag.currMaxAmount = filter["fldChequeAmtMax"].Replace(",", "");
                    ViewBag.currMinAmount = filter["fldChequeAmtMin"].Replace(",", "");
                    ViewBag.Amount = filter["fldBankChargesAmt"].Replace(",", "");
                    ViewBag.Rate = filter["fldBankChargesRate"].Replace(",", "");
                }
                return View();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //Update
        [CustomAuthorize(TaskIds = TaskIdsOCS.BankCharges.UPDATE)] //Done
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            try
            {
                //get system profile value
                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim(); //Done

                //validate bank zone
                List<String> errorMessages = bankchargesDao.ValidateBankCharges(col, "Update"); //Done

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        //update Process
                        bankchargesDao.UpdateBankCharges(col); //done
                        TempData["Notice"] = Locale.BankChargesSuccessfullyUpdated;
                    }
                    else
                    {
                        bool IsBankChargesTempExist = bankchargesDao.ValidateExistingBankCharges(col["fldProductCode"], CurrentUser.Account.BankCode, col["fldBankChargesType"], col["fldChequeAmtMin"].Replace(",", ""), col["fldChequeAmtMax"].Replace(",", ""), "U", "temp");
                        if (IsBankChargesTempExist == true)
                        {
                            TempData["Warning"] = Locale.BankChargesExistChecker;
                        }
                        else
                        {
                            bankchargesDao.CreateBankChargesTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId, "Update", "", "", "", ""); //Done
                            TempData["Notice"] = Locale.BankChargesSuccessfullyAddedtoApprovedUpdate;
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [GenericFilter(AllowHttpGet = true)]
        public JsonResult GetBankChargesDesc(string type) {
            string description = bankchargesDao.GetBankChargesDesc(type);
            return Json(description, JsonRequestBehavior.AllowGet);
        }

    }
}