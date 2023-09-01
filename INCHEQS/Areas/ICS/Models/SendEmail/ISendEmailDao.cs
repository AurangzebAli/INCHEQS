using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Models.SendEmail {
    public interface ISendEmailDao {
        List<string> GetAllUserEmailListOtherThan(string userId);
        string GetUserEmail(string userId);
        int InsertIntotblEmail(string subject, string message, AccountModel currentUserAccount);
        List<string> Validate(FormCollection col);

    }
}