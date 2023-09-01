using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Password;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities {
    
    public class ChangePasswordController : BaseController {

        private readonly IPasswordDao changePassword;
        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;

        public ChangePasswordController(IPasswordDao changePassword , IUserDao userDao, IAuditTrailDao auditTrailDao) {
            this.changePassword = changePassword;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: ChangePassword
        [CustomAuthorize(TaskIds = TaskIds.ChangePassword.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
            //ViewBag.ChangePassword = changePassword.getPass(CurrentUser.Account.UserId);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ChangePassword.UPDATE)]
        [HttpPost()]
        public ActionResult Update(FormCollection col) {
            try {

                UserModel userModel = userDao.GetUser(CurrentUser.Account.UserId);
                List<string> errorMessages = changePassword.Validate(userModel.fldUserId, userModel.fldPassword , col["txtOldPasswd"], col["txtNewPasswd"], col["txtConfPasswd"]);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {
                    changePassword.UpdatePwd(userModel.fldUserId , userModel.fldPasswordExpDate, col["txtOldPasswd"], col["txtNewPasswd"]);
                    changePassword.AddPwdHistory(userModel.fldUserId , col["txtNewPasswd"]);
                    auditTrailDao.Log("Change Password - User ID : " + CurrentUser.Account.UserId, CurrentUser.Account);

                    TempData["Notice"] = Locale.PasswordChangesuccessfully;
                }


            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}