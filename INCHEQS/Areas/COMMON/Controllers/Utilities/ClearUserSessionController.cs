using INCHEQS.Security.AuditTrail;
using INCHEQS.TaskAssignment;
using INCHEQS.Security.UserSession;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates;

namespace INCHEQS.Areas.COMMON.Controllers.Utilities
{
    
    public class ClearUserSessionController : BaseController
    {

        private readonly IAuditTrailDao auditTrailDao;
        private readonly ISecurityAuditLogDao SecurityAuditLogDao;
        private readonly IUserSessionDao clearusersession;
        private IPageConfigDao pageConfigDao;
        public ClearUserSessionController(IUserSessionDao clearusersession, IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao, ISecurityAuditLogDao SecurityAuditLogDao)
        {
            this.clearusersession = clearusersession;
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
            this.SecurityAuditLogDao = SecurityAuditLogDao;
        }
        // GET: ClearUserSession
        [CustomAuthorize(TaskIds = TaskIds.ClearUserSession.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ClearUserSession.INDEX));
            auditTrailDao.SecurityLog("Access Clear User Session", "", TaskIds.ClearUserSession.INDEX, CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ClearUserSession.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ClearUserSession.INDEX, "View_ThresholdSetting", null, "fldBankCode=@fldBankCode", new[] {
                new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode )
            }), collection);
        
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ClearUserSession.DELETE)]
        [HttpPost()]
        public ActionResult Delete(FormCollection collection)
        {
            string sTaskID = "";
            sTaskID = TaskIds.ClearUserSession.DELETE;
            if (collection != null & collection["deleteBox"] != null)
            {
                List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    clearusersession.ClearSession(arrResult);


                    string ActionDetail = SecurityAuditLogDao.UserSession_Template("Clear User Session - Delete", arrResult);
                    auditTrailDao.SecurityLog("Delete User Session", ActionDetail, sTaskID, CurrentUser.Account);
                }
                TempData["Notice"] = Locale.Recordsdeleted;
                //auditTrailDao.Log("Clear user session - User Id: "+collection["chkdelete"], CurrentUser.Account);
            }
            else
                TempData["Warning"] = Locale.Nodatawasselected;
            
            return RedirectToAction("Index");
        }
    }
}