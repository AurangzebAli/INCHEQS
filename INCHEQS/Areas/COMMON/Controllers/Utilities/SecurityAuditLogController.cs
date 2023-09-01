using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.Areas.COMMON.Controllers.Utilities
{
    public class SecurityAuditLogController : BaseController
    {
        private IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        public SecurityAuditLogController(IPageConfigDao pageConfigDao, IAuditTrailDao auditTrailDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.auditTrailDao = auditTrailDao;
        }

        // GET: AuditLog
        [CustomAuthorize(TaskIds = TaskIdsOCS.SecurityAuditLog.INDEX)]
        public async Task<ActionResult> Index()
        {
            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.SecurityAuditLog.INDEX, "View_SecurityAuditLog"));
            auditTrailDao.SecurityLog("Access Audit log", "", TaskIdsOCS.SecurityAuditLog.INDEX, CurrentUser.Account);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIdsOCS.SecurityAuditLog.INDEX)]
        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection)
        {
            if (collection["UserLoginId1"] == "")
            {
                ViewBag.collection3 = "All";
            }
            else
            {
                ViewBag.collection3 = collection["UserLoginId1"].Trim();
            }
            if (collection["ModuleAccessed1"] == "")
            {
                ViewBag.collection4 =  "All";
            }
            else
            {
                ViewBag.collection4 = collection["ModuleAccessed1"].Trim();
            }
            auditTrailDao.SecurityLog("Retrieve audit log(s) From : " + collection["from_fldCreateTimeStamp1"].Trim() + " To : " + collection["to_fldCreateTimeStamp1"].Trim() + " for User(s) :" + ViewBag.collection3 + " and Module(s) :" + ViewBag.collection4, "", TaskIdsOCS.SecurityAuditLog.INDEX, CurrentUser.Account);
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(new PageSqlConfig(TaskIdsOCS.SecurityAuditLog.INDEX, "View_SecurityAuditLog"
                , "fldCreateTimeStamp desc", "fldBankCode=@fldBankCode", new[] { new SqlParameter("@fldBankCode", CurrentUser.Account.BankCode) }
                ), collection);
            ViewBag.collection1 = collection["from_fldCreateTimeStamp1"];
            ViewBag.collection2 = collection["to_fldCreateTimeStamp1"];
            ViewBag.collection3 = collection["UserLoginId1"];
            ViewBag.collection4 = collection["ModuleAccessed1"];
            //auditTrailDao.Log("Retrieve audit log", CurrentUser.Account);
            return View();

        }
    }
}