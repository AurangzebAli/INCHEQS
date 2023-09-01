using INCHEQS.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.GLReplacement {
    public interface IGLReplacementDao {
        GLReplacementModel GetGLReplacement(string glReplacementId);
        List<string> ValidateCreate(FormCollection col);
        List<string> ValidateUpdate(FormCollection col);
        void CreateGLReplacement(FormCollection col, AccountModel currentUser);
        void UpdateGLReplacement(FormCollection col);
        void DeleteFromGLReplacement(string glReplacementId);
        }
}