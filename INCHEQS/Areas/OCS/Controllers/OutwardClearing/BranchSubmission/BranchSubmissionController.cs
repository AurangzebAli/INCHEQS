using INCHEQS.Areas.OCS.Models.BranchSubmission;
using INCHEQS.Areas.OCS.Models.CommonOutwardItem;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using INCHEQS.TaskAssignment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing.BranchSubmission
{
    public class BranchSubmissionController : Controller
    {
        private readonly IBranchSubmissionDao BranchSubmissionDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;

        public BranchSubmissionController(IBranchSubmissionDao BranchSubmissionDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.BranchSubmissionDao = BranchSubmissionDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }
        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchClearing.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            List<BranchSubmissionItemList> ItemsForSubmission = BranchSubmissionDao.GetItemReadyForSubmissionList(CurrentUser.Account.UserId);
            List<BranchSubmittedItemList> ItemsForSubmitted = BranchSubmissionDao.GetItemReadyForSubmittedList(CurrentUser.Account.UserId);
            ViewBag.ItemsForSubmission = ItemsForSubmission;
            ViewBag.ItemsForSubmitted = ItemsForSubmitted;
            return View();
        }
    }
}