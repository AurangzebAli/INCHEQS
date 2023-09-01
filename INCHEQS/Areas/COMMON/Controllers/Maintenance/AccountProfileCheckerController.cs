using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.COMMON.Models.AccountProfile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class AccountProfileCheckerController : BaseController
    {

        private IAccountProfileDao accountProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public AccountProfileCheckerController(IAccountProfileDao accountProfileDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.accountProfileDao = accountProfileDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.AccountProfileChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            //ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.AccountProfileChecker.INDEX, "View_InternalBranch", "fldBankCode, fldBranchCode, fldlocationcode", "fldBankCode=@fldBankCode", new[] {
            //    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            //}),
            //collection); check also in branchChecker for this

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.AccountProfileChecker.INDEX, "View_AccountProfileChecker", ""), collection);
            return View();
        }

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

                        if (action.Equals("A"))
                        {
                            accountProfileDao.MoveToAccountProfileFromTemp(id);
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);

                            AccountProfileModel objAccountProfile = accountProfileDao.GetAccountProfileDataById(id);
                            accountProfileDao.DeleteAccountProfile(id);
                            //auditTrailDao.Log("Deleted Account Profile from Main => Account Number : " + objAccountProfile.accountNumber
                            //    + " Account Name : " + objAccountProfile.accountName + " Account Type : " + objAccountProfile.accountType
                            //    + " Account Status : " + objAccountProfile.accountStatus + " Branch ID : " + objAccountProfile.BranchID

                            //    + " Opening Date : " + objAccountProfile.OpeningDate + " Closing Date : " + objAccountProfile.ClosingDate
                            //    + " Customer Number : " + objAccountProfile.customerNumber + " Contact Number : " + objAccountProfile.contactNumber

                            //    + " Email Address : " + objAccountProfile.emailAddress
                            //    + " Áddress 1 : " + objAccountProfile.address1
                            //    + " Áddress 2 : " + objAccountProfile.address2 + "  Áddress 3 : " + objAccountProfile.address3
                            //    + " PostCode : " + objAccountProfile.postCode + " City : " + objAccountProfile.city + " Country : "
                            //    + objAccountProfile.countryDesc, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(id);
                            //auditTrailDao.Log("Deleted Account Profile Created Before => Account Number : " + before.accountNumber
                            //    + " Account Name : " + before.accountName + " Account Type : " + before.accountType
                            //    + " Account Status : " + before.accountStatus + " Branch ID : " + before.BranchID

                            //    + " Opening Date : " + before.OpeningDate + " Closing Date : " + before.ClosingDate
                            //    + " Customer Number : " + before.customerNumber + " Contact Number : " + before.contactNumber

                            //    + " Email Address : " + before.emailAddress
                            //    + " Áddress 1 : " + before.address1
                            //    + " Áddress 2 : " + before.address2 + "  Áddress 3 : " + before.address3
                            //    + " PostCode : " + before.postCode + " City : " + before.city + " Country : "
                            //    + before.countryDesc, CurrentUser.Account);

                            accountProfileDao.UpdateAccountProfileById(id);

                            AccountProfileModel after = accountProfileDao.GetAccountProfileDataById(id);
                            //auditTrailDao.Log("Deleted Account Profile After Create => Account Number : " + after.accountNumber
                            //    + " Account Name : " + after.accountName + " Account Type : " + after.accountType
                            //    + " Account Status : " + after.accountStatus + " Branch ID : " + after.BranchID

                            //    + " Opening Date : " + after.OpeningDate + " Closing Date : " + after.ClosingDate
                            //    + " Customer Number : " + after.customerNumber + " Contact Number : " + after.contactNumber

                            //    + " Email Address : " + after.emailAddress
                            //    + " Áddress 1 : " + after.address1
                            //    + " Áddress 2 : " + after.address2 + "  Áddress 3 : " + after.address3
                            //    + " PostCode : " + after.postCode + " City : " + after.city + " Country : "
                            //    + after.countryDesc, CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_EditTemplate(id, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);
                        }

                        accountProfileDao.DeleteAccountProfileTemp(id);
                        //auditTrailDao.Log("Approve Account Profile - Task Assigment :" + taskId + " Account Number : " + id, CurrentUser.Account);

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

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfileChecker.INDEX)]
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
                        if (action.Equals("A"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(id);
                            AccountProfileModel after = accountProfileDao.GetAccountProfileTempById(id);
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                        }

                        accountProfileDao.DeleteAccountProfileTemp(id);
                        //auditTrailDao.Log("Account Profile Update, Delete or Created New - Task Assigment :" + taskId + "Account Number : " + id, CurrentUser.Account);

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




        // CHANGES PAGE 
        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Changes(FormCollection col, string accountProfileParam = "")
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
            ViewBag.AccountProfileTemp = accountProfileDao.GetAccountProfileTempById(accountNumber);
            ViewBag.AccountProfile = accountProfileDao.GetAccountProfileDataById(accountNumber);


            return View();
        }

        [HttpPost]
        public ActionResult VerifyAChanges(FormCollection col)
        {
            try
            {
               string accountNumber = col["haccountNumber"].Trim();
                string taskId = TaskIdsOCS.AccountProfileChecker.INDEX;

                ViewBag.AccountProfileTemp = accountProfileDao.GetAccountProfileTempById(accountNumber);
                String action = ViewBag.AccountProfileTemp.status;



                if (action.Equals("Add"))
                {
                    accountProfileDao.MoveToAccountProfileFromTemp(accountNumber);
                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_AddTemplate(accountNumber, "Approve", "Approve");
                    auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("Delete"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_DeleteTemplate(accountNumber, "Approve");
                    auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);
                    AccountProfileModel objAccountProfile = accountProfileDao.GetAccountProfileDataById(accountNumber);
                    accountProfileDao.DeleteAccountProfile(accountNumber);
                    //auditTrailDao.Log("Deleted Account Profile from Main => Account Number : " + objAccountProfile.accountNumber
                    //    + " Account Name : " + objAccountProfile.accountName + " Account Type : " + objAccountProfile.accountType
                    //    + " Account Status : " + objAccountProfile.accountStatus + " Branch ID : " + objAccountProfile.BranchID

                    //    + " Opening Date : " + objAccountProfile.OpeningDate + " Closing Date : " + objAccountProfile.ClosingDate
                    //    + " Customer Number : " + objAccountProfile.customerNumber + " Contact Number : " + objAccountProfile.contactNumber

                    //    + " Email Address : " + objAccountProfile.emailAddress
                    //    + " Áddress 1 : " + objAccountProfile.address1
                    //    + " Áddress 2 : " + objAccountProfile.address2 + "  Áddress 3 : " + objAccountProfile.address3
                    //    + " PostCode : " + objAccountProfile.postCode + " City : " + objAccountProfile.city + " Country : "
                    //    + objAccountProfile.countryDesc, CurrentUser.Account);
                }
                else if (action.Equals("Update"))
                {
                    AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(accountNumber);
                    //auditTrailDao.Log("Deleted Account Profile Created Before => Account Number : " + before.accountNumber
                    //    + " Account Name : " + before.accountName + " Account Type : " + before.accountType
                    //    + " Account Status : " + before.accountStatus + " Branch ID : " + before.BranchID

                    //    + " Opening Date : " + before.OpeningDate + " Closing Date : " + before.ClosingDate
                    //    + " Customer Number : " + before.customerNumber + " Contact Number : " + before.contactNumber

                    //    + " Email Address : " + before.emailAddress
                    //    + " Áddress 1 : " + before.address1
                    //    + " Áddress 2 : " + before.address2 + "  Áddress 3 : " + before.address3
                    //    + " PostCode : " + before.postCode + " City : " + before.city + " Country : "
                    //    + before.countryDesc, CurrentUser.Account);

                    accountProfileDao.UpdateAccountProfileById(accountNumber);

                    AccountProfileModel after = accountProfileDao.GetAccountProfileDataById(accountNumber);
                    //auditTrailDao.Log("Deleted Account Profile After Create => Account Number : " + after.accountNumber
                    //    + " Account Name : " + after.accountName + " Account Type : " + after.accountType
                    //    + " Account Status : " + after.accountStatus + " Branch ID : " + after.BranchID

                    //    + " Opening Date : " + after.OpeningDate + " Closing Date : " + after.ClosingDate
                    //    + " Customer Number : " + after.customerNumber + " Contact Number : " + after.contactNumber

                    //    + " Email Address : " + after.emailAddress
                    //    + " Áddress 1 : " + after.address1
                    //    + " Áddress 2 : " + after.address2 + "  Áddress 3 : " + after.address3
                    //    + " PostCode : " + after.postCode + " City : " + after.city + " Country : "
                    //    + after.countryDesc, CurrentUser.Account);

                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_EditTemplate(accountNumber, before, after, "Approve");
                    auditTrailDao.SecurityLog("Approve Account Profile", ActionDetails, taskId, CurrentUser.Account);
                }

                accountProfileDao.DeleteAccountProfileTemp(accountNumber);
                //auditTrailDao.Log("Approve Account Profile - Task Assigment :" + TaskIdsOCS.AccountProfileChecker.INDEX + " Account Number : " + accountNumber, CurrentUser.Account);
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                throw ex;
            }
}

        [CustomAuthorize(TaskIds = TaskIdsOCS.AccountProfileChecker.INDEX)]
        [HttpPost]
        public ActionResult VerifyRChanges(FormCollection col)
        {
            try
            {
               string accountNumber = col["haccountNumber"].Trim();
                string taskId = TaskIdsOCS.AccountProfileChecker.INDEX;
                ViewBag.AccountProfileTemp = accountProfileDao.GetAccountProfileTempById(accountNumber);
                String action = ViewBag.AccountProfileTemp.status;
                if (action.Equals("Add"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_AddTemplate(accountNumber, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("Update"))
                {
                    AccountProfileModel before = accountProfileDao.GetAccountProfileDataById(accountNumber);
                    AccountProfileModel after = accountProfileDao.GetAccountProfileTempById(accountNumber);
                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_EditTemplate(accountNumber, before, after, "Reject");
                    auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                }
                else if (action.Equals("Delete"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.AccountProfileChecker_DeleteTemplate(accountNumber, "Reject");
                    auditTrailDao.SecurityLog("Reject Account Profile", ActionDetails, taskId, CurrentUser.Account);
                }
               accountProfileDao.DeleteAccountProfileTemp(accountNumber);
               //auditTrailDao.Log("Account Profile Update, Delete or Created New - Task Assigment :" + TaskIdsOCS.AccountProfileChecker.INDEX + "Account Number : " + accountNumber, CurrentUser.Account);
                
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