using INCHEQS.ConfigVerification.VerificationLimit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.ICS.Models.VerificationLimit {
    public interface IVerificationLimitDao {

        DataTable ListVerificationLimit();
        DataTable Find(string classId);
        void Update(FormCollection collection, string currentUserId);
        void CreateVerificationBatchSizeLimitTemp(FormCollection collection, string currentUserId);
        void DeleteInVerificationBatchSizeLimit(string delete);
        VerificationLimitModel GetVerifyLimit(string fldclass);
        bool checkClassExist(string fldClass);
        void DeleteInVerificationBatchSizeLimitTemp(string clasId);
        void CreateInVerificationBatchSizeLimit(string classId);
        List<string> Validate(FormCollection collection);
        void AddToVerificationBatchSizeLimitTempToDelete(string clasId);
        }
}