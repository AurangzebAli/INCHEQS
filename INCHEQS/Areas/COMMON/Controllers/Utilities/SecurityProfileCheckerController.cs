using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.Users;
using INCHEQS.Security;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using INCHEQS.Common.Resources;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Utilities
{
    public class SecurityProfileCheckerController : Controller
    {
        private readonly ISecurityProfileDao securityProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        private readonly IUserDao userDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public SecurityProfileCheckerController(ISecurityProfileDao securityProfileDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, IUserDao userDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.securityProfileDao = securityProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.userDao = userDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.SecurityProfileChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.SecurityProfileChecker.INDEX, "View_SecurityProfileChecker", "", "", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);
            ViewBag.SecurityProfileTemp = securityProfileDao.GetSecurityProfileTemp();
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SecurityProfile(FormCollection collection)
        {
            ViewBag.SecurityProfileTemp = securityProfileDao.GetSecurityProfileTemp();
            ViewBag.SecurityProfile = securityProfileDao.GetSecurityProfile();
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            CurrentUser.Account.Status = "Y";
            CurrentUser.Account.TaskId = TaskIds.SecurityProfileChecker.INDEX;
            try
            {
                string sTaskId = TaskIds.SecurityProfileChecker.VERIFY;

                string x = col["fldUserAuthMethod"];
                string tableheader = "";

                if (x == "AD")
                {
                    tableheader = "(Active Directory) - Approve";
                }
                else if (x == "LP")
                {
                    tableheader = "(Local Profiler) - Approve";
                }

                List<string> arrResults = new List<string>();
                if ((col["deleteBox"]) != null)
                {
                    SecurityProfileModel after1 = securityProfileDao.GetSecurityProfileTemp();
                    auditTrailDao.Log(" After Update => Authentication Method :" + after1.fldUserAuthMethod + ", User ID Length Min: " + after1.fldUserIdLengthMin + "/Max: " + after1.fldUserIdLengthMax +
                    ", User Maximum Login Attempt: " + after1.fldUserLoginAttempt + ", Concurrent Connection :" +
                    after1.fldUserCNCR + ", User Session Timeout :" + after1.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after1.fldDualApproval + ", User Account Expiry Interval: " + after1.fldUserADDomain + after1.fldUserAcctExpiry +
                    after1.fldUserAcctExpiryInt + ", User Password Length Min: " + after1.fldUserPwdLengthMin + "/Max: " + after1.fldUserPwdLengthMax + ", Password History Reusable after : " +
                    after1.fldUserPwdHisRA + ", User Password Expiry Interval : " + after1.fldUserPwdExpiry + after1.fldUserPwdExpiryInt +
                    ",User Password Notification Interval : " + after1.fldUserPwdNotification + after1.fldUserPwdNotificationInt +
                    ", When Password Expired : " + after1.fldUserPwdExpAction + after1.fldUserAuthMethodDesc + after1.fldSecurityValue +
                    ",User Password Change Sequence : " + after1.fldPwdChangeTime,
                    CurrentUser.Account);
                    SecurityProfileModel before = securityProfileDao.GetSecurityProfile();

                    securityProfileDao.DeleteSecurityMaster();
                    securityProfileDao.MovetoSecurityMasterfromTemp();
                    securityProfileDao.DeleteSecurityMasterTemp();

                    SecurityProfileModel after = securityProfileDao.GetSecurityProfile();

                    string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                    auditTrailDao.SecurityLog("Approve Security Profile", ActionDetail, sTaskId, CurrentUser.Account);

                    TempData["Notice"] = "Record(s) successfully approved";

                    securityProfileDao.DeleteSecurityProfileChecker();
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

        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            CurrentUser.Account.Status = "R";
            CurrentUser.Account.TaskId = TaskIds.SecurityProfileChecker.INDEX;
            try
            {
                string sTaskId = TaskIds.SecurityProfileChecker.VERIFY;

                string x = col["fldUserAuthMethod"];
                string tableheader = "";

                if (x == "AD")
                {
                    tableheader = "(Active Directory) - Reject";
                }
                else if (x == "LP")
                {
                    tableheader = "(Local Profiler) - Reject";
                }

                List<string> arrResults = new List<string>();

                if ((col["deleteBox"]) != null)
                {
                    SecurityProfileModel after1 = securityProfileDao.GetSecurityProfileTemp();
                    auditTrailDao.Log(" After Update => Authentication Method :" + after1.fldUserAuthMethod + ", User ID Length Min: " + after1.fldUserIdLengthMin + "/Max: " + after1.fldUserIdLengthMax +
                    ", User Maximum Login Attempt: " + after1.fldUserLoginAttempt + ", Concurrent Connection :" +
                    after1.fldUserCNCR + ", User Session Timeout :" + after1.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after1.fldDualApproval + ", User Account Expiry Interval: " + after1.fldUserADDomain + after1.fldUserAcctExpiry +
                    after1.fldUserAcctExpiryInt + ", User Password Length Min: " + after1.fldUserPwdLengthMin + "/Max: " + after1.fldUserPwdLengthMax + ", Password History Reusable after : " +
                    after1.fldUserPwdHisRA + ", User Password Expiry Interval : " + after1.fldUserPwdExpiry + after1.fldUserPwdExpiryInt +
                    ",User Password Notification Interval : " + after1.fldUserPwdNotification + after1.fldUserPwdNotificationInt +
                    ", When Password Expired : " + after1.fldUserPwdExpAction + after1.fldUserAuthMethodDesc + after1.fldSecurityValue +
                    ",User Password Change Sequence : " + after1.fldPwdChangeTime,
                    CurrentUser.Account);
                    SecurityProfileModel before = securityProfileDao.GetSecurityProfile();

                    SecurityProfileModel after = securityProfileDao.GetSecurityProfileTemp();

                    string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                    auditTrailDao.SecurityLog("Reject Security Profile", ActionDetail, sTaskId, CurrentUser.Account);
                    
                    securityProfileDao.DeleteSecurityMasterTemp();
                    securityProfileDao.DeleteSecurityProfileChecker();
                    TempData["Notice"] = "Record(s) successfully rejected";
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
        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col)
        {
            CurrentUser.Account.Status = "Y";
            CurrentUser.Account.TaskId = TaskIds.SecurityProfileChecker.INDEX;
            try
            {
                string sTaskId = TaskIds.SecurityProfileChecker.VERIFY;

                string x = col["fldUserAuthMethod"];
                string tableheader = "";

                if (x == "AD")
                {
                    tableheader = "(Active Directory) - Approve";
                }
                else if (x == "LP")
                {
                    tableheader = "(Local Profiler) - Approve";
                }
                SecurityProfileModel after1 = securityProfileDao.GetSecurityProfileTemp();
                auditTrailDao.Log(" After Update => Authentication Method :" + after1.fldUserAuthMethod + ", User ID Length Min: " + after1.fldUserIdLengthMin + "/Max: " + after1.fldUserIdLengthMax +
                ", User Maximum Login Attempt: " + after1.fldUserLoginAttempt + ", Concurrent Connection :" +
                after1.fldUserCNCR + ", User Session Timeout :" + after1.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after1.fldDualApproval + ", User Account Expiry Interval: " + after1.fldUserADDomain + after1.fldUserAcctExpiry +
                after1.fldUserAcctExpiryInt + ", User Password Length Min: " + after1.fldUserPwdLengthMin + "/Max: " + after1.fldUserPwdLengthMax + ", Password History Reusable after : " +
                after1.fldUserPwdHisRA + ", User Password Expiry Interval : " + after1.fldUserPwdExpiry + after1.fldUserPwdExpiryInt +
                ",User Password Notification Interval : " + after1.fldUserPwdNotification + after1.fldUserPwdNotificationInt +
                ", When Password Expired : " + after1.fldUserPwdExpAction + after1.fldUserAuthMethodDesc + after1.fldSecurityValue +
                ",User Password Change Sequence : " + after1.fldPwdChangeTime,
                CurrentUser.Account);

                SecurityProfileModel before = securityProfileDao.GetSecurityProfile();

                securityProfileDao.DeleteSecurityMaster();
                securityProfileDao.MovetoSecurityMasterfromTemp();
                securityProfileDao.DeleteSecurityMasterTemp();

                SecurityProfileModel after = securityProfileDao.GetSecurityProfile();

                string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                auditTrailDao.SecurityLog("Approve Security Profile", ActionDetail, sTaskId, CurrentUser.Account);

                TempData["Notice"] = "Record(s) successfully approved";

                securityProfileDao.DeleteSecurityProfileChecker();
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.SecurityProfileChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col)
        {
            CurrentUser.Account.Status = "R";
            CurrentUser.Account.TaskId = TaskIds.SecurityProfileChecker.INDEX;
            try
            {
                string sTaskId = TaskIds.SecurityProfileChecker.VERIFY;

                string x = col["fldUserAuthMethod"];
                string tableheader = "";

                if (x == "AD")
                {
                    tableheader = "(Active Directory) - Reject";
                }
                else if (x == "LP")
                {
                    tableheader = "(Local Profiler) - Reject";
                }
                SecurityProfileModel after1 = securityProfileDao.GetSecurityProfileTemp();
                auditTrailDao.Log(" After Update => Authentication Method :" + after1.fldUserAuthMethod + ", User ID Length Min: " + after1.fldUserIdLengthMin + "/Max: " + after1.fldUserIdLengthMax +
                ", User Maximum Login Attempt: " + after1.fldUserLoginAttempt + ", Concurrent Connection :" +
                after1.fldUserCNCR + ", User Session Timeout :" + after1.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after1.fldDualApproval + ", User Account Expiry Interval: " + after1.fldUserADDomain + after1.fldUserAcctExpiry +
                after1.fldUserAcctExpiryInt + ", User Password Length Min: " + after1.fldUserPwdLengthMin + "/Max: " + after1.fldUserPwdLengthMax + ", Password History Reusable after : " +
                after1.fldUserPwdHisRA + ", User Password Expiry Interval : " + after1.fldUserPwdExpiry + after1.fldUserPwdExpiryInt +
                ",User Password Notification Interval : " + after1.fldUserPwdNotification + after1.fldUserPwdNotificationInt +
                ", When Password Expired : " + after1.fldUserPwdExpAction + after1.fldUserAuthMethodDesc + after1.fldSecurityValue +
                ",User Password Change Sequence : " + after1.fldPwdChangeTime,
                CurrentUser.Account);

                SecurityProfileModel before = securityProfileDao.GetSecurityProfile();

                SecurityProfileModel after = securityProfileDao.GetSecurityProfileTemp();

                securityProfileDao.DeleteSecurityMasterTemp();
                securityProfileDao.DeleteSecurityProfileChecker();

                string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                auditTrailDao.SecurityLog("Reject Security Profile", ActionDetail, sTaskId, CurrentUser.Account);

                TempData["Notice"] = "Record(s) successfully rejected";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

