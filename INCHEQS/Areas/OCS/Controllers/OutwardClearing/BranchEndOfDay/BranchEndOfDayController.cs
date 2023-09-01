using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using System.Linq;
using INCHEQS.Areas.OCS.Models.BranchEndOfDay;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing
{

    public class BranchEndOfDayController : BaseController
    {
        private readonly IBranchEndOfDayDao branchEndOfDayDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISystemProfileDao systemProfileDao;

        public BranchEndOfDayController(IBranchEndOfDayDao branchEndOfDayDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.branchEndOfDayDao = branchEndOfDayDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.BranchEndOfDay.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.CurrentDate = branchEndOfDayDao.GetProcessDate(CurrentUser.Account.BankCode);
            auditTrailDao.SecurityLog("Access OCS Branch End of Day", "", TaskIdsOCS.BranchEndOfDay.INDEX, CurrentUser.Account);
            return View();
        }
        
    }
}