using INCHEQS.ConfigVerificationBranch.ReleaseBranchLockedCheque;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{
    public class ReleaseBranchLockedChequeController : BaseController
    {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly IReleaseBranchLockedChequeDao releaseBranchLockedChequeDao;

        public ReleaseBranchLockedChequeController(IReleaseBranchLockedChequeDao releaseBranchLockedChequeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao)
        {
            this.releaseBranchLockedChequeDao = releaseBranchLockedChequeDao;
            this.auditTrailDao = auditTrailDao;
        }

        //GET: ReleaseLockedCheque
        [CustomAuthorize(TaskIds = TaskIds.ReleaseBranchLockedCheque.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {

            ViewBag.LockedCheque = releaseBranchLockedChequeDao.ListLockedCheque(CurrentUser.Account.BankCode);
            return View();
        }



        [CustomAuthorize(TaskIds = TaskIds.ReleaseBranchLockedCheque.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {

            if (collection != null & collection["deleteBox"] != null)
            {
                releaseBranchLockedChequeDao.DeleteProcessUsingCheckbox(collection["deleteBox"]);
                TempData["Notice"] = Locale.SuccessfullyDeleted;
                //auditTrailDao.Log("Clear Data Process - Primary Id: " + collection["chkDelete"], CurrentUser.Account);
            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }
    }
}