using INCHEQS.Areas.OCS.Models.OCSReleaseLockedCheque;
using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using System.Collections.Generic;
using System.Linq;

namespace INCHEQS.Areas.OCS.Controllers.Utilities
{

    public class ReleaseLockedChequeController : BaseController
    {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly OCSIReleaseLockedChequeDao OCSreleaseLockedChequeDao;
        private IPageConfigDao pageConfigDao;
        public ReleaseLockedChequeController(OCSIReleaseLockedChequeDao ocsreleaseLockedChequeDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao)
        {
            this.OCSreleaseLockedChequeDao = ocsreleaseLockedChequeDao;
            this.auditTrailDao = auditTrailDao;
            this.pageConfigDao = pageConfigDao;
        }

        //GET: OCS/ReleaseLockedCheques
        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedChequeOCS.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {

            //ViewBag.LockedCheque = OCSreleaseLockedChequeDao.ListLockedCheque(CurrentUser.Account.BankCode);
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ReleaseLockedChequeOCS.INDEX));
            auditTrailDao.SecurityLog("Access Release OCS Locked Cheque", "", TaskIds.ReleaseLockedChequeOCS.INDEX, CurrentUser.Account);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedChequeOCS.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {



            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ReleaseLockedChequeOCS.INDEX, "view_releaselockedchequeocs", "fldbankcode", null, new[] {
             new System.Data.SqlClient.SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);


            return View();

        }

        [CustomAuthorize(TaskIds = TaskIds.ReleaseLockedChequeOCS.DELETE)]
        [HttpPost()]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Delete(FormCollection collection)
        {
            List<string> arrResults = new List<string>();

            if (collection != null & collection["deleteBox"] != null)
            {
                arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    OCSreleaseLockedChequeDao.DeleteProcessUsingCheckbox(arrResult);
                    OCSreleaseLockedChequeDao.UpdateReleaseLockedCheque(arrResult);
                }
                //OCSreleaseLockedChequeDao.DeleteProcessUsingCheckbox(collection["deleteBox"]);
                TempData["Notice"] = "Successfully Deleted";
                auditTrailDao.SecurityLog("Release OCS locked cheque - Item ID: " + collection["deleteBox"], "", TaskIds.ReleaseLockedChequeOCS.INDEX, CurrentUser.Account);
            }
            else
            {
                TempData["ErrorMsg"] = Locale.Nodatawasselected;
            }
            return RedirectToAction("Index");
            //return View();
        }
    }
}