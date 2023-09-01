using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SecurityProfile;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities {
    
    public class SecurityProfileController : BaseController {

        private readonly ISecurityProfileDao securityprofile;

        private readonly IAuditTrailDao auditTrailDao;

        public SecurityProfileController(ISecurityProfileDao securityprofile , IAuditTrailDao auditTrailDao) {
            this.securityprofile = securityprofile;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: SecurityProfile
        [CustomAuthorize(TaskIds = TaskIds.SecurityProfile.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            ViewBag.SecurityProfile = securityprofile.GetSecurityProfile();

            return View();
        }

        [CustomAuthorize(TaskIds =TaskIds.SecurityProfile.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col) {
            try {
                List<string> errorMessages = securityprofile.ValidateSecurity(col);
                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;

                }
                else {
                    SecurityProfileModel before = securityprofile.GetSecurityProfile();
                    auditTrailDao.Log("Edit Security Profile - Before Change=> Password Expiry Interval : " + before.fldPwdExpInt + " Password Notification Interval :" + before.fldPwdNotificationInt + " User Account Expiry Interval :" + before.fldAcctExpInt + " User ID Length : MIN(" + before.fldIDLengthMin + ")MAX(" + before.fldIDLengthMax + ") User Password Length : MIN(" + before.fldPwdLengthMin + ")MAX(" + before.fldPwdLengthMax + ")" + " User Session Timeout:" + before.fldTimeOut + " Password History List Reusable after :" + before.fldPwdHisListRA + " Login Attempt:" + before.fldLoginAttempt, CurrentUser.Account);
                    securityprofile.UpdateSecurity(col,CurrentUser.Account.UserId);

                    SecurityProfileModel after = securityprofile.GetSecurityProfile();
                    auditTrailDao.Log("Edit Security Profile - After Change=> Password Expiry Interval : " + after.fldPwdExpInt + " Password Notification Interval :" + after.fldPwdNotificationInt + " User Account Expiry Interval :" + after.fldAcctExpInt + " User ID Length : MIN(" + after.fldIDLengthMin + ")MAX(" + after.fldIDLengthMax + ") User Password Length : MIN(" + after.fldPwdLengthMin + ")MAX(" + after.fldPwdLengthMax + ")" + " User Session Timeout:" + after.fldTimeOut + " Password History List Reusable after :" + after.fldPwdHisListRA + " Login Attempt:" + after.fldLoginAttempt, CurrentUser.Account);

                    TempData["Notice"] = Locale.Updaterecordsuccessfully;
                }


            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}
