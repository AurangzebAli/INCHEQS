using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace INCHEQS.Models.Email {
    public interface IEmailService {

        void Send(string fromName, string fromEmail, string subject, string message, List<string> recipients);
    }
}