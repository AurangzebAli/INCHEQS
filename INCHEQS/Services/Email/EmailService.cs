using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace INCHEQS.Models.Email {
    public class EmailService : IEmailService {

        public async void Send(string fromName, string fromEmail, string subject , string message, List<string> recipients) {
            var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var mailMessage = new MailMessage();
            foreach(string recipient in recipients) {
                mailMessage.To.Add(new MailAddress(recipient)); //replace with valid value
            }
            mailMessage.Subject = subject;
            mailMessage.Body = string.Format(body, fromName, fromEmail, message);
            mailMessage.IsBodyHtml = true;
            using (var smtp = new SmtpClient()) {
                await smtp.SendMailAsync(mailMessage);
            }
        }
    }
}