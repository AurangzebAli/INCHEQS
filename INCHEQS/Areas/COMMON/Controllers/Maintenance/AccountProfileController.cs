using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.AccountProfile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class AccountProfileController : BaseController
    {
        
        private IAccountProfileDao accountProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public AccountProfileController(IAccountProfileDao accountProfileDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.accountProfileDao = accountProfileDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        //[CustomAuthorize(TaskIds = TaskIds.ReturnCode.INDEX)]
        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.AccountProfile.INDEX));
            return View();
        }

        //[CustomAuthorize(TaskIds = TaskIds.AccountProfile.INDEX)]
        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.AccountProfile.INDEX, "View_AccountProfile", "fldAccountNumber"),
            collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string accountProfileParam = "")
        {
            ViewBag.AccountType = accountProfileDao.ListAccountType();
            ViewBag.AccountStatus = accountProfileDao.ListAccountStatus();
            ViewBag.InternalBranchCode = accountProfileDao.ListInternalBranchCode(CurrentUser.Account.BankCode);
            ViewBag.Country = accountProfileDao.ListCountry();
            ViewBag.BranchDetails = accountProfileDao.getBranchDetails(CurrentUser.Account);

            string accountNumber = "";

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            

            if (string.IsNullOrEmpty(accountProfileParam))
            {
                accountNumber = filter["fldAccountNumber"].Trim();

            }
            else
            {
                accountNumber = accountProfileParam;

            }

            if ((accountNumber == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.AccountProfile = accountProfileDao.GetAccountProfileDataById(accountNumber); 


             return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection collection)
        {
            ActionResult action;
            try
            {
                List<String> errorMessages = accountProfileDao.ValidateUpdate(collection);

                string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
                string sTaskId = TaskIdsOCS.AccountProfile.UPDATE;
                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    action = RedirectToAction("Edit", new { accountProfileParam = collection["accountNumber"] });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(collection["accountNumber"]);
                        //auditTrailDao.Log("Edit - Account Profile - Before Update => Account Number : " + before.accountNumber
                        //    + " Account Name : " + before.accountName + " Account Type : " + before.accountType
                        //    + " Account Status : " + before.accountStatus + " Branch ID : " + before.BranchID

                        //    + " Opening Date : " + before.OpeningDate + " Closing Date : " + before.ClosingDate

                        //    + " Customer Number : " + before.customerNumber + " Contact Number : " + before.contactNumber
                        //    + " Email Address : " + before.emailAddress + " Áddress 1 : " + before.address1
                        //    + " Áddress 2 : " + before.address2 + "  Áddress 3 : " + before.address3 + " PostCode : " + before.postCode
                        //    + " City : " + before.city + " Country : " + before.countryDesc, CurrentUser.Account);

                        accountProfileDao.UpdateAccountProfile(collection);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;

                        AccountProfileModel after = accountProfileDao.GetAccountProfileDataById(collection["accountNumber"]);
                        //auditTrailDao.Log("Edit - Account Profile - After Update => Account Number : " + after.accountNumber
                        //    + " Account Name : " + after.accountName + " Account Type : " + after.accountType
                        //    + " Account Status : " + after.accountStatus + " Branch ID : " + after.BranchID

                        //    + " Opening Date : " + after.OpeningDate + " Closing Date : " + after.ClosingDate

                        //    + " Customer Number : " + after.customerNumber + " Contact Number : " + after.contactNumber
                        //    + " Email Address : " + after.emailAddress + " Áddress 1 : " + after.address1
                        //    + " Áddress 2 : " + after.address2 + "  Áddress 3 : " + after.address3 + " PostCode : " + after.postCode
                        //    + " City : " + after.city + " Country : " + after.countryDesc, CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.AccountProfile_EditTemplate(collection["accountNumber"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsAccountProfileTempExist = accountProfileDao.CheckAccountProfileTempById(collection["accountNumber"]);

                        if (IsAccountProfileTempExist == true)
                        {
                            TempData["Warning"] = Locale.AccountProfileAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(collection["accountNumber"]);

                            accountProfileDao.CreateAccountProfileTemp(collection, "update");
                            TempData["Notice"] = Locale.AccountProfileUpdateVerify;

                            AccountProfileModel after = accountProfileDao.GetAccountProfileTempById(collection["accountNumber"]);

                            AccountProfileModel data = accountProfileDao.GetAccountProfileDataById(collection["accountNumber"]);
                            //auditTrailDao.Log("Add Account Peofile into Temporary record to Update => => Account Number : " + data.accountNumber
                            //    + " Account Name : " + data.accountName + " Account Type : " + data.accountType
                            //    + " Account Status : " + data.accountStatus + " Branch ID : " + data.BranchID

                            //    + " Opening Date : " + data.OpeningDate + " Closing Date : " + data.ClosingDate

                            //    + " Customer Number : " + data.customerNumber + " Contact Number : " + data.contactNumber
                            //    + " Email Address : " + data.emailAddress + " Áddress 1 : " + data.address1
                            //    + " Áddress 2 : " + data.address2 + "  Áddress 3 : " + data.address3
                            //    + " PostCode : " + data.postCode + " City : " + data.city + " Country : " + data.countryDesc, CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.AccountProfile_EditTemplate(collection["accountNumber"], before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return RedirectToAction("Edit", new { accountProfileParam = collection["accountNumber"] });
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()

        {
            ViewBag.AccountType = accountProfileDao.ListAccountType();
            ViewBag.AccountStatus = accountProfileDao.ListAccountStatus();
            ViewBag.InternalBranchCode = accountProfileDao.ListInternalBranchCode(CurrentUser.Account.BankCode);
            ViewBag.Country = accountProfileDao.ListCountry();
            ViewBag.BranchDetails = accountProfileDao.getBranchDetails(CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.SAVECREATE)]
        public ActionResult SaveCreate(FormCollection collection, string bankCode)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.AccountProfile.SAVECREATE;
            try
            {

                List<string> errorMessages = accountProfileDao.ValidateCreate(collection);

                if ((errorMessages.Count > 0))
                {

                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Create");

                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {

                        accountProfileDao.CreateAccountProfile(collection);

                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        //auditTrailDao.Log("Add into Account Profile Master Table: - Account Number : " + collection["accountNumber"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.AccountProfile_AddTemplate(collection["accountNumber"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {

                        accountProfileDao.CreateAccountProfileTemp(collection, "create");

                        TempData["Notice"] = Locale.AccountProfileCreateVerify;
                        //auditTrailDao.Log("Add into Account Profile Temporary Table: - Account Number : " + collection["accountNumber"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.AccountProfile_AddTemplate(collection["accountNumber"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfile.DELETE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection collection)
        {

            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIdsOCS.AccountProfile.DELETE;

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();

                foreach (string arrResult in arrResults)
                {

                    if ("N".Equals(systemProfile))
                    {
                        string ActionDetails = MaintenanceAuditLogDao.AccountProfile_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                        accountProfileDao.DeleteAccountProfile(arrResult);
                        TempData["Notice"] = Locale.SuccessfullyDeleted;
                        //auditTrailDao.Log("Delete - Account Profile table - Account Number :  " + collection["deleteBox"], CurrentUser.Account);

                    }
                    else
                    {
                        bool IsAccountProfileTempExist = accountProfileDao.CheckAccountProfileTempById(arrResult);

                        if (IsAccountProfileTempExist == true)
                        {
                            TempData["Warning"] = Locale.AccountProfileAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfile_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Account Profile", ActionDetails, sTaskId, CurrentUser.Account);
                            accountProfileDao.CreateAccountProfileTempToDelete(arrResult);
                            TempData["Notice"] = Locale.AccountProfileVerifyDelete;
                            //auditTrailDao.Log("Add into Account Profile Temp table to Delete -  Account Number :  " + collection["deleteBox"], CurrentUser.Account);
                        }
                    }

                }

            }
            else

                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }
}