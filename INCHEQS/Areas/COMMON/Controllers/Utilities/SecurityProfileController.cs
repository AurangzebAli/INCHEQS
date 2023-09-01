using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;


namespace INCHEQS.Areas.COMMON.Controllers.Utilities
{

    public class SecurityProfileController : BaseController
    {
        

        private readonly ISecurityProfileDao securityprofile;
        protected readonly ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        

        public SecurityProfileController(ISecurityProfileDao securityprofile, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            
            this.securityprofile = securityprofile;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }
        // GET: SecurityProfile
        [CustomAuthorize(TaskIds = TaskIds.SecurityProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SecurityProfile = securityprofile.GetSecurityProfile();
            ViewBag.SecurityProfileTemp = securityprofile.GetSecurityProfileTemp();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.SecurityProfile.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col)
        {
            CurrentUser.Account.Status = "U";
            CurrentUser.Account.TaskId = TaskIds.SecurityProfile.INDEX;
            string x = col["fldUserAuthMethod"];
            try
            {
                string sTaskId = TaskIds.SecurityProfile.UPDATE;

                string tableheader = "";
                if (x == "AD")
                {
                    tableheader = "(Active Directory) - Edit";
                }
                else if (x == "LP")
                {
                    tableheader = "(Local Profiler) - Edit";
                }
                
                string securityProfile = securityprofile.GetValueFromSecurityProfile("fldDualApproval", CurrentUser.Account.BankCode).Trim();

                List<string> errorMessages = securityprofile.ValidateSecurity(col);
                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if (securityprofile.CheckSecurityProfileTemp() == false)
                    {
                        if ("N".Equals(securityProfile))
                        {
                            SecurityProfileModel before = securityprofile.GetSecurityProfile();

                            securityprofile.UpdateSecurityMaster(col, CurrentUser.Account.UserId);
                            //fldSecurityId,fldUserAuthMethod,fldUserIdLengthMin,fldUserIdLengthMax,fldUserLoginAttempt,fldUserCNCR,fldUserSessionTimeOut,fldDualApproval,fldUserADDomain,fldUserAcctExpiry,
                            //fldUserAcctExpiryInt,fldUserPwdLengthMin,fldUserPwdLengthMax,fldUserPwdHisRA,fldUserPwdExpiry,fldUserPwdExpiryInt,fldUserPwdNotification,fldUserPwdNotificationInt,
                            //fldUserPwdExpAction,fldUserAuthMethodDesc,fldSecurityValue,fldPwdChangeTime
                            auditTrailDao.Log(" Before Update => Authentication Method :" + before.fldUserAuthMethod + ", User ID Length Min: " + before.fldUserIdLengthMin +"/Max: " + before.fldUserIdLengthMax +
                            ", User Maximum Login Attempt: " + before.fldUserLoginAttempt + ", Concurrent Connection :" +
                            before.fldUserCNCR + ", User Session Timeout :" + before.fldUserSessionTimeOut +"Mins*" + ", Dual Approval :" + before.fldDualApproval + ", User Account Expiry Interval: " + before.fldUserADDomain + before.fldUserAcctExpiry +
                            before.fldUserAcctExpiryInt + ", User Password Length Min: " + before.fldUserPwdLengthMin + "/Max: " + before.fldUserPwdLengthMax + ", Password History Reusable after : " +
                            before.fldUserPwdHisRA + ", User Password Expiry Interval : "+ before.fldUserPwdExpiry + before.fldUserPwdExpiryInt +
                            ",User Password Notification Interval : " + before.fldUserPwdNotification + before.fldUserPwdNotificationInt +
                            ", When Password Expired : " + before.fldUserPwdExpAction + before.fldUserAuthMethodDesc + before.fldSecurityValue +
                            ",User Password Change Sequence : "+before.fldPwdChangeTime, 
                            CurrentUser.Account);

                            SecurityProfileModel after = securityprofile.GetSecurityProfile();
                            auditTrailDao.Log(" After Update => Authentication Method :" + after.fldUserAuthMethod + ", User ID Length Min: " + after.fldUserIdLengthMin + "/Max: " + after.fldUserIdLengthMax +
                            ", User Maximum Login Attempt: " + after.fldUserLoginAttempt + ", Concurrent Connection :" +
                            after.fldUserCNCR + ", User Session Timeout :" + after.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after.fldDualApproval + ", User Account Expiry Interval: " + after.fldUserADDomain + after.fldUserAcctExpiry +
                            after.fldUserAcctExpiryInt + ", User Password Length Min: " + after.fldUserPwdLengthMin + "/Max: " + after.fldUserPwdLengthMax + ", Password History Reusable after : " +
                            after.fldUserPwdHisRA + ", User Password Expiry Interval : " + after.fldUserPwdExpiry + after.fldUserPwdExpiryInt +
                            ",User Password Notification Interval : " + after.fldUserPwdNotification + after.fldUserPwdNotificationInt +
                            ", When Password Expired : " + after.fldUserPwdExpAction + after.fldUserAuthMethodDesc + after.fldSecurityValue +
                            ",User Password Change Sequence : " + after.fldPwdChangeTime,
                            CurrentUser.Account);

                            string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                            auditTrailDao.SecurityLog("Edit Security Profile", ActionDetail, sTaskId, CurrentUser.Account);

                            TempData["Notice"] = Locale.RecordsuccesfullyUpdated;
                        }
                        else
                        {
                            securityprofile.CreateSecurityMasterTemp(col, CurrentUser.Account.UserId);
                            if (securityprofile.CheckSecurityProfileTemp() == true)
                            {
                                SecurityProfileModel before = securityprofile.GetSecurityProfile();
                                auditTrailDao.Log(" Before Update => Authentication Method :" + before.fldUserAuthMethod + ", User ID Length Min: " + before.fldUserIdLengthMin + "/Max: " + before.fldUserIdLengthMax +
                                ", User Maximum Login Attempt: " + before.fldUserLoginAttempt + ", Concurrent Connection :" +
                                before.fldUserCNCR + ", User Session Timeout :" + before.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + before.fldDualApproval + ", User Account Expiry Interval: " + before.fldUserADDomain + before.fldUserAcctExpiry +
                                before.fldUserAcctExpiryInt + ", User Password Length Min: " + before.fldUserPwdLengthMin + "/Max: " + before.fldUserPwdLengthMax + ", Password History Reusable after : " +
                                before.fldUserPwdHisRA + ", User Password Expiry Interval : " + before.fldUserPwdExpiry + before.fldUserPwdExpiryInt +
                                ",User Password Notification Interval : " + before.fldUserPwdNotification + before.fldUserPwdNotificationInt +
                                ", When Password Expired : " + before.fldUserPwdExpAction + before.fldUserAuthMethodDesc + before.fldSecurityValue +
                                ",User Password Change Sequence : " + before.fldPwdChangeTime,
                                CurrentUser.Account);
                                
                                
                                securityprofile.CreateSecurityProfileChecker("Security Profile", "Update", CurrentUser.Account.UserId);

                                SecurityProfileModel after = securityprofile.GetSecurityProfileTemp();
                                //auditTrailDao.Log(" After Update => Authentication Method :" + after.fldUserAuthMethod + ", User ID Length Min: " + after.fldUserIdLengthMin + "/Max: " + after.fldUserIdLengthMax +
                                //", User Maximum Login Attempt: " + after.fldUserLoginAttempt + ", Concurrent Connection :" +
                                //after.fldUserCNCR + ", User Session Timeout :" + after.fldUserSessionTimeOut + "Mins*" + ", Dual Approval :" + after.fldDualApproval + ", User Account Expiry Interval: " + after.fldUserADDomain + after.fldUserAcctExpiry +
                                //after.fldUserAcctExpiryInt + ", User Password Length Min: " + after.fldUserPwdLengthMin + "/Max: " + after.fldUserPwdLengthMax + ", Password History Reusable after : " +
                                //after.fldUserPwdHisRA + ", User Password Expiry Interval : " + after.fldUserPwdExpiry + after.fldUserPwdExpiryInt +
                                //",User Password Notification Interval : " + after.fldUserPwdNotification + after.fldUserPwdNotificationInt +
                                //", When Password Expired : " + after.fldUserPwdExpAction + after.fldUserAuthMethodDesc + after.fldSecurityValue +
                                //",User Password Change Sequence : " + after.fldPwdChangeTime,
                                //CurrentUser.Account);

                                string ActionDetail = SecurityAuditLogDao.SecurityProfile_EditTemplate(before, after, tableheader, col);
                                auditTrailDao.SecurityLog("Edit Security Profile", ActionDetail, sTaskId, CurrentUser.Account);
                            }
                            TempData["Notice"] = Locale.SecurityProfileSucessfullyAddedToUpdate;
                        }
                    }
                    else
                        {
                            TempData["Warning"] = Locale.SecurityProfileRecordPendingforApproval;
                        }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}
