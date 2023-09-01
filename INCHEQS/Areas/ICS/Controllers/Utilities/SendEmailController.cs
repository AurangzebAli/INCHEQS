using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.Email;
using INCHEQS.Models.SendEmail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{
    public class SendEmailController : BaseController {
        private ISendEmailDao emailDao;
        private IEmailService emailService;

        public SendEmailController(ISendEmailDao email, IEmailService emailService) {
            this.emailDao = email;
            this.emailService = emailService;
        }
        // GET: SendEmail
        [CustomAuthorize(TaskIds = TaskIds.SendEmail.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.EmailLists = emailDao.GetAllUserEmailListOtherThan(CurrentUser.Account.UserId);            
            ViewBag.UserEmail = emailDao.GetUserEmail(CurrentUser.Account.UserId);//Get The latest email updated
            
            return View();
        }
        [CustomAuthorize(TaskIds = TaskIds.SendEmail.INDEX)]
        [HttpPost()]
        public ActionResult SendMessage(FormCollection collection) {

            try {
                List<string> errorMessages = emailDao.Validate(collection);

                if ((errorMessages.Count > 0)) {
                    TempData["ErrorMsg"] = errorMessages;
                } else {

                    string recipient = collection["drpEmailList"];
                    string subject = collection["txtSubject"];
                    string message = collection["textareavalue"];
                    //mailServer = gMailServer;
                    string allemail = collection["allEmaillist"];
                    string sendingMethod = collection["method"];
                    string fromEmail = emailDao.GetUserEmail(CurrentUser.Account.UserId);
                    List<string> recipients = new List<string>();

                    if (recipient == "A") {
                        recipients = emailDao.GetAllUserEmailListOtherThan(CurrentUser.Account.UserId);
                    } else {
                        recipients.Add(recipient);
                    }

                    try {
                        emailService.Send(CurrentUser.Account.UserAbbr, fromEmail, subject, message, recipients);
                        emailDao.InsertIntotblEmail(subject,message,CurrentUser.Account);                         
                        TempData["Notice"] = Locale.Successfull;
                    } catch (Exception) {
                        //AuditTrail.Log("");                     
                        TempData["Notice"] = Locale.Failtosendemail;
                    }
                }

            } catch (Exception ex) {

                throw ex;
            }
            return RedirectToAction("Index");
        }
    }
}