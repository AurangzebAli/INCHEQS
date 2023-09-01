using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace INCHEQS.Areas.OCS.Controllers.Utilities
{

    public class ClearDataProcessController : BaseController
    {

        private readonly OCSIDataProcessDao OCScleardataProcess;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
 
        public ClearDataProcessController(OCSIDataProcessDao ocscleardataProcess, IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao)
        {
            this.OCScleardataProcess = ocscleardataProcess;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
        }
        // GET: OCS/ClearDataProcess
        [CustomAuthorize(TaskIds = TaskIdsOCS.ClearDataProcess.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ClearDataProcess.INDEX));
            auditTrailDao.SecurityLog("Access OCS Clear Data Process", "", TaskIdsOCS.ClearDataProcess.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ClearDataProcess.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            string x = collection["fldProcessDate"];
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ClearDataProcess.INDEX, "View_ClearDataProcessOCS", null, "fldBankCode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            }), collection);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ClearDataProcess.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {

            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    OCScleardataProcess.DeleteProcessUsingCheckbox(arrResult);
                    TempData["Notice"] = Locale.SuccessfullyDeleted;
                    auditTrailDao.SecurityLog("OCS Clear Data Process - Primary Id: " + arrResult, "", TaskIdsOCS.ClearDataProcess.INDEX, CurrentUser.Account);
                }
            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;

            return RedirectToAction("Index");
        }

    }
}