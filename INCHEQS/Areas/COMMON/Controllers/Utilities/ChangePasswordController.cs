using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.Password;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.User;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;
using System.Data;
//using INCHEQS.Areas.COMMON.Models.Users;

namespace INCHEQS.Areas.COMMON.Controllers.Utilities {
    
    public class ChangePasswordController : BaseController {

        private readonly IPasswordDao changePassword;
        private readonly IUserDao userDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;

        public ChangePasswordController(IPasswordDao changePassword, IUserDao userDao, IAuditTrailDao auditTrailDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.changePassword = changePassword;
            this.userDao = userDao;
            this.auditTrailDao = auditTrailDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;

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

                string sTaskID = "";
                sTaskID = TaskIds.ChangePassword.UPDATE;

                UserModel userModel = userDao.GetUser(CurrentUser.Account.UserId);
                List<string> errorMessages = changePassword.Validate(userModel.fldUserId, userModel.fldPassword , col["txtOldPasswd"], col["txtNewPasswd"], col["txtConfPasswd"]);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else if (changePassword.GetTotalPwdChangeTime(CurrentUser.Account.UserId) == true)
                {
                    TempData["ErrorMsg"] = Locale.ExceededPwdChangeLimit;
                }
                else {
                    ChangePasswordModel before = SecurityAuditLogDao.CheckUserMasterByID(col["fldUserId"], col["fldUserAbb"], "userAbb");

                    changePassword.UpdatePwd(userModel.fldUserId , userModel.fldPasswordExpDate, col["txtOldPasswd"], col["txtNewPasswd"]);
                    changePassword.AddPwdHistory(userModel.fldUserId , col["txtNewPasswd"]);
                    //auditTrailDao.Log("Change Password - User ID : " + CurrentUser.Account.UserId, CurrentUser.Account);

                    ChangePasswordModel after = SecurityAuditLogDao.CheckUserMasterByID(col["fldUserId"], col["fldUserAbb"], "userAbb"); 


                    string ActionDetail = SecurityAuditLogDao.ChangePassword_Template(before, after, "Change Password - Edit", col);
                    auditTrailDao.SecurityLog("Change Password", ActionDetail, sTaskID, CurrentUser.Account);

                    TempData["Notice"] = Locale.PasswordChangesuccessfully;
                }


            } catch (Exception ex) {
                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}