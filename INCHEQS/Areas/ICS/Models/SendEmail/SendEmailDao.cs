using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Models.SendEmail;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;

public class SendEmailDao : ISendEmailDao {

    private readonly ApplicationDbContext dbContext;
    public SendEmailDao(ApplicationDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public List<string> GetAllUserEmailListOtherThan(string userId) {//dropdown in ui
        string stmt = "SELECT fldEmail FROM tblusermaster where fldemail <> null or fldemail <> '' and flduserid not in (@userId)";

        DataTable ds = dbContext.GetRecordsAsDataTable( stmt , new[] { new SqlParameter("@userId",userId)});
        List<string> emails = new List<string>();
        foreach(DataRow row in ds.Rows) {
            emails.Add(row["fldEmail"].ToString());
        }
        return emails;
    }
    public string GetUserEmail(string userId) {//get current user email
        
        string stmt = "select fldEmail from tblusermaster where flduserid = @userId ";
        DataTable ds = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@userId", userId) } );
        if(ds.Rows.Count > 0) {
            return ds.Rows[0]["fldEmail"].ToString();
        }
        return "";
    }
    
    public int InsertIntotblEmail(string subject , string message , AccountModel currentUserAccount) {
        
        string stmt = "insert into tblEmail (fldEmailTitle, fldEmailContent, fldBranchCode, fldCreateUserID, fldCreateTimeStamp) " +
            " VALUES (@subject,@message, @branchcode, @userid, getdate())";
        try {

            return dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@subject", subject),
                new SqlParameter("@message", message),
                //new SqlParameter("@methodcode", CurrentUser.Account.SpickCode.ToString().Trim()),
                new SqlParameter("@branchcode", currentUserAccount.BranchCodes[0]),
                new SqlParameter("@userid", currentUserAccount.UserId)
            });
        }catch(Exception ex) {
            throw ex;
        }
    }

    public List<string> Validate(FormCollection col) {

        List<string> err = new List<string>();

        if (col["txtSubject"].Equals("")) {
            err.Add(Locale.Subjectcannotbeblank);
        }
        if (col["txtAreaBody"].Equals("")) {
            err.Add(Locale.Messagecannotbeblank);
        }
        return err;
    }
}
