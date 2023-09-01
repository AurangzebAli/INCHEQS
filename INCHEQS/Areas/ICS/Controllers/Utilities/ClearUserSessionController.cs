using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;


namespace INCHEQS.Areas.ICS.Controllers.Utilities {
    
    public class ClearUserSessionController : BaseController {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly IUserSessionDao clearusersession;
        public ClearUserSessionController(IUserSessionDao clearusersession, IAuditTrailDao auditTrailDao) {
            this.clearusersession = clearusersession;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: ClearUserSession
        [CustomAuthorize(TaskIds = TaskIds.ClearUserSession.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index() {
        
            ViewBag.ClearUserSession = clearusersession.ListAll(CurrentUser.Account.BankCode);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ClearUserSession.DELETE)]
        [HttpPost()]
        public ActionResult ClearSession(FormCollection collection) {

            if (collection != null & collection["chkdelete"] != null) {
                clearusersession.ClearSession(collection["chkdelete"]);
                TempData["Notice"] = Locale.Recordsdeleted;
                auditTrailDao.Log("Clear user session - User Id: "+collection["chkdelete"], CurrentUser.Account);
            } else
                TempData["Warning"] = Locale.Nodatawasselected;
            
            return RedirectToAction("Index");
        }
    }
}