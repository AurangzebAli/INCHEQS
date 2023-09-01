using INCHEQS.Areas.ICS.Models.DedicatedBranchDay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;
using INCHEQS.Security.User;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.SecurityProfile;

namespace INCHEQS.Areas.ICS.Controllers.Maintenance
{
 
    public class DedicatedBranchDayController : BaseController
    {
        private IDedicatedBranchDayDao DedicatedBranchDayDao;
        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly ISecurityProfileDao securityProfileDao;

        public DedicatedBranchDayController(IDedicatedBranchDayDao DedicatedBranchDayDao, IPageConfigDao pageConfigDao, IUserDao userDao, IAuditTrailDao auditTrailDao,ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, ISecurityProfileDao securityProfileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.DedicatedBranchDayDao = DedicatedBranchDayDao;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.securityProfileDao = securityProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            //ViewBag.CreateDedicated = DedicatedBranchDayDao.ListAll();
            //return View();
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.DedicatedBranchDay.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult searchresultpage(FormCollection collection)
        {
            try
            {
                ViewBag.searchresult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.DedicatedBranchDay.INDEX, "view_dedicatedbranchdate", "flduserabb", "fldbankcode=@fldbankcode and usertype='ccu'", new[] {
                    new SqlParameter("@fldbankcode",CurrentUser.Account.BankCode)}),
                collection);
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection collection, string id = null)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            if (!string.IsNullOrEmpty(id))
            {
                ViewBag.User = userDao.GetUser(id);
            }
            else
            {
                ViewBag.User = userDao.getUserByAbbr(filter["fldUserAbb"],"");
            }
            ViewBag.ClearDate = userDao.GetClearingDate();
            ViewBag.UserType2 = DedicatedBranchDayDao.getUserProfile(@ViewBag.User.fldUserAbb);
            ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
            ViewBag.City = userDao.ListCity();
            ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode,@ViewBag.User.fldUserAbb);
            ViewBag.GetOfficer = userDao.GetOfficerId(@ViewBag.User.fldUserAbb);
            ViewBag.BranchAvailableList = DedicatedBranchDayDao.ListAvailableBranch(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            ViewBag.BranchSelectedList = DedicatedBranchDayDao.ListSelectedBranch(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            ViewBag.VerificationClass = userDao.ListVerificationClass();
            ViewBag.Security = securityProfileDao.GetSecurityProfile();

            //Check if LoginAD "Y" to display password
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
            return View();
        }


      
        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.UPDATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col,string submit)
        {
            try
            {
                string branch = "No branch assigned";
                if (!String.IsNullOrEmpty(col["selectedTask"]))
                {
                    branch = col["selectedTask"].ToString();
                }
                    UserModel before = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit Dedicated Branch - Update=> User Id : " + before.fldUserAbb, CurrentUser.Account);

                    DedicatedBranchDayDao.InsertUserDedicatedBranch(col);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;

                    UserModel after = userDao.GetUser(col["fldUserId"]);
                    auditTrailDao.Log("Edit Dedicated Branch - Successful Update=> User Id : " + after.fldUserAbb + " Assigned Branch : " + branch, CurrentUser.Account);
                    TempData["Notice"] = Locale.UserSuccessfullyUpdated;

                return RedirectToAction("Edit", new { id = col["fldUserId"] });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.DELETE)]
        [HttpPost]
        public ActionResult Delete(FormCollection col)
        {
            //Get value from system profile
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();
            try
            {

                if ((col["deleteBox"]) != null)
                {
                    List<string> arrResults = col["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        if (userDao.CheckUserExistInTempUser(arrResult) == false)
                        {
                            if ("N".Equals(systemProfile))
                            {
                                userDao.DeleteInUserMaster(arrResult);
                                TempData["Notice"] = Locale.SuccessfullyDeleted;
                            }
                            else
                            {
                                userDao.AddUserToUserMasterTempToDelete(arrResult);
                                TempData["Notice"] = Locale.UserSuccessfullyAddedtoApproved;
                            }
                        }
                    }

                    //audittrail
                    if ("N".Equals(systemProfile))
                    {
                        auditTrailDao.Log("Delete - Dedicated Branch: " + col["deleteBox"], CurrentUser.Account);
                    }
                    else
                    {
                        auditTrailDao.Log("Add into Temporary record to Delete - Dedicated Branch: " + col["deleteBox"], CurrentUser.Account);
                    }

                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("SearchResultPage");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Create()
        {
            String x = null;
            String y = null; 
            ViewBag.Bank = CurrentUser.Account;
            ViewBag.Branch = userDao.ListBranch(CurrentUser.Account.BankCode);
            ViewBag.Officer = userDao.ListOfficer(CurrentUser.Account.BankCode,CurrentUser.Account.UserAbbr);
            ViewBag.BranchAvailableList = userDao.ListAvailableBranch(x);
            ViewBag.BranchSelectedList = userDao.ListSelectedBranch(x,y);
            ViewBag.VerificationClass = userDao.ListVerificationClass();
            ViewBag.City = userDao.ListCity();

            //Check if LoginAD "Y" to display password
            ViewBag.EnableLoginAD = systemProfileDao.GetValueFromSystemProfile("LoginAD", CurrentUser.Account.BankCode).Trim();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.SAVECREATE)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCreate(FormCollection col)
        {
            //Get value from system profile
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("UserChecker", CurrentUser.Account.BankCode).Trim();

            try
            {
                UserModel user = new UserModel();
                List<string> errorMessages = userDao.ValidateUser(col, "Create",CurrentUser.Account.BankCode);

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        userDao.CreateUserToUserTemp(col,CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                        userDao.CreateInUserMaster(col["fldUserAbb"]);
                        userDao.DeleteInUserMasterTemp(col["fldUserAbb"]);
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        auditTrailDao.Log("Add - Dedicated Branch: " + col["fldUserAbb"], CurrentUser.Account);
                    }
                    else
                    {
                        userDao.CreateUserToUserTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);
                        userDao.InsertUserDedicatedBranch(col, CurrentUser.Account.BankCode);
                        TempData["Notice"] = Locale.UserSuccessfullyCreated;
                        auditTrailDao.Log("Add into Temporary record to Create - Dedicated Branch: " + col["fldUserAbb"], CurrentUser.Account);
                    }

                }
                return RedirectToAction("Create");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.DedicatedBranchDay.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Reload(FormCollection collection, string id = null)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            ViewBag.User = userDao.GetUser(filter["fldUserId"]);
            ViewBag.ClearDate = collection["fldIDExpDate"];
            ViewBag.BranchAvailableList = DedicatedBranchDayDao.ListAvailableBranch(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            ViewBag.BranchSelectedList = DedicatedBranchDayDao.ListSelectedBranch(@ViewBag.User.fldUserAbb, ViewBag.ClearDate);
            return View();
        }

    }
}