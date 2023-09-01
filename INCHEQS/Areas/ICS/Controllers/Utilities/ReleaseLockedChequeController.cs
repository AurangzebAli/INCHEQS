using INCHEQS.Areas.ICS.Models.ICSReleaseLockedCheque;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using System.Collections.Generic;
using System.Linq;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{

    public class ReleaseLockedChequeController : BaseController
    {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly ICSIReleaseLockedChequeDao releaseLockedChequeDao;
        private IPageConfigDao pageConfigDao;

        public ReleaseLockedChequeController(ICSIReleaseLockedChequeDao releaseLockedChequeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao)
        {
            this.releaseLockedChequeDao = releaseLockedChequeDao;
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
        }

        //GET: ReleaseLockedCheque
        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedCheque.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            CurrentUser.Account.TaskId = TaskIds.ReleaseLockedCheque.INDEX;
            //ViewBag.LockedCheque = releaseLockedChequeDao.ListLockedCheque(CurrentUser.Account.BankCode);
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ReleaseLockedCheque.INDEX));
            auditTrailDao.Log("Access Release ICS Locked Cheque", CurrentUser.Account);
            //auditTrailDao.SecurityLog("Access Release ICS Locked Cheque", "", TaskIds.ReleaseLockedCheque.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedCheque.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ReleaseLockedCheque.INDEX, "view_releaselockedcheque", "fldUIC", null, new[] {
             new System.Data.SqlClient.SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);


            return View();

        }

        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedCheque.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            CurrentUser.Account.TaskId = TaskIds.ReleaseLockedCheque.INDEX;
            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = new List<string>();

                arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    releaseLockedChequeDao.DeleteProcessUsingCheckbox(arrResult);
                    releaseLockedChequeDao.UpdateReleaseLockedCheque(arrResult);
                }

                TempData["Notice"] = Locale.SuccessfullyDeleted;
                //auditTrailDao.SecurityLog("Release ICS locked cheque - Inward Item ID: " + collection["deleteBox"], "", TaskIds.ReleaseLockedCheque.INDEX, CurrentUser.Account);
                auditTrailDao.Log("Release ICS locked cheque - Inward Item ID: " + collection["deleteBox"],CurrentUser.Account);
            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }
    }
}