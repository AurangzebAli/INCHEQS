using INCHEQS.Helpers;
using INCHEQS.Models;
using INCHEQS.Security.Account;
using INCHEQS.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.DataAccessLayer;
using INCHEQS.Common;

namespace INCHEQS.Areas.ICS.Models.GLReplacement {
    public class GLReplacementDao : IGLReplacementDao{

        private readonly ApplicationDbContext dbContext;

        public GLReplacementDao(ApplicationDbContext dbContext) {
            this.dbContext = dbContext;
        }

        public GLReplacementModel GetGLReplacement(string glReplacementId) {
            DataTable dt = new DataTable();
            GLReplacementModel glReplacementModel = new GLReplacementModel();
            string stmt = "select * from tblGLReplacement where fldGLReplacementID = @fldGLReplacementID";
            dt = dbContext.GetRecordsAsDataTable(stmt, new[] { new SqlParameter("@fldGLReplacementID", glReplacementId) });

            if (dt.Rows.Count > 0) {
                DataRow row = dt.Rows[0];
                glReplacementModel.fldGLAccountNumber = row["fldGLAccountNumber"].ToString().Trim();
                glReplacementModel.fldDescription = row["fldDescription"].ToString().Trim();
                glReplacementModel.fldGLReplacementID = row["fldGLReplacementID"].ToString().Trim();
            }
            return glReplacementModel;

        }

        public List<string> ValidateCreate(FormCollection col) {
            List<string> err = new List<string>();

            if (col["fldGLAccountNumber"].Trim().Equals("")) {
                err.Add(Locale.GLAccountNumberCannotEmpty);
            }
            return err;
        }

        public List<string> ValidateUpdate(FormCollection col) {
            List<string> err = new List<string>();

            if (col["fldGLAccountNumber"].Trim().Equals("")) {
                err.Add(Locale.GLAccountNumberCannotEmpty);
            }
            return err;
        }

        public void CreateGLReplacement(FormCollection col, AccountModel currentUser) {
            Dictionary<string, dynamic> sqlGLReplacement = new Dictionary<string, dynamic>();

            sqlGLReplacement.Add("fldGLAccountNumber", col["fldGLAccountNumber"]);
            sqlGLReplacement.Add("fldDescription", col["fldDescription"]);
            sqlGLReplacement.Add("fldCreateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlGLReplacement.Add("fldCreateUserId", currentUser.UserId);
            sqlGLReplacement.Add("fldUpdateTimeStamp", DateUtils.GetCurrentDatetime());
            sqlGLReplacement.Add("fldUpdateUserId", currentUser.UserId);
            sqlGLReplacement.Add("fldBankcode", currentUser.BankCode);
            sqlGLReplacement.Add("fldEntityCode", currentUser.BankCode);
            dbContext.ConstructAndExecuteInsertCommand("tblGLReplacement", sqlGLReplacement);
        }

        public void UpdateGLReplacement(FormCollection col) {
            string stmt = "update tblGLReplacement set fldGLAccountNumber= @fldGLAccountNumber,fldDescription= @fldDescription where fldGLReplacementID=@fldGLReplacementID";

            dbContext.ExecuteNonQuery(stmt, new[] {
                new SqlParameter("@fldGLReplacementID",col["fldGLReplacementID"].Trim()),
                new SqlParameter("@fldDescription",col["fldDescription"].Trim()),
                new SqlParameter("@fldGLAccountNumber",col["fldGLAccountNumber"].Trim()),

            });
        }

        public void DeleteFromGLReplacement(string glReplacementId) {
            string stmt = "delete from tblGLReplacement where  fldGLReplacementID=@fldGLReplacementID";
            dbContext.ExecuteNonQuery(stmt, new[] { new SqlParameter("@fldGLReplacementID", glReplacementId) });
        }

    }
}