using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance {
    
    public class SystemProfileController : BaseController {

        private readonly ISystemProfileDao SystemProfile;

        private readonly IAuditTrailDao auditTrailDao;


        public SystemProfileController(ISystemProfileDao SystemProfile, IAuditTrailDao auditTrailDao) {
            this.SystemProfile = SystemProfile;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: SecurityProfile
        //[CustomAuthorize(TaskIds = TaskIds.SystemProfile.INDEX)]
        //[GenericFilter(AllowHttpGet = true)]
        //public ActionResult Index() {
        //    ViewBag.SystemProfile = SystemProfile.GetListOfSystemProfile(CurrentUser.Account.BankCode);


        //    return View();
        //}

        //[CustomAuthorize(TaskIds = TaskIds.SystemProfile.UPDATE)]
        //[HttpPost()]
        //public ActionResult Update(FormCollection col)
        //{
        //    try
        //    {
        //        List<string> errorMessages = SystemProfile.ValidateSecurity(col, CurrentUser.Account.BankCode);

        //        if ((errorMessages.Count > 0))
        //        {
        //            TempData["ErrorMsg"] = errorMessages;

        //        }
        //        else
        //        {
        //            SystemProfile.CreateSystemProfileTemp(col, CurrentUser.Account.BankCode, CurrentUser.Account.UserId);

        //            //Pending for locale
        //            TempData["Notice"] = "Record successfully transfer for Approval. Note that you cannot update again the Sysmtem Profile if there is pending Approval.";
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return RedirectToAction("Index");
        //}
    }
}
