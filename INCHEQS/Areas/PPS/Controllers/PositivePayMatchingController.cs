using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.PPS.Models.PositivePayMatching;
using INCHEQS.Common;
using INCHEQS.Helpers;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Resources;
using INCHEQS.Security;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;


namespace INCHEQS.Areas.PPS.Controllers
{
    public class PositivePayMatchingController : BaseController
    {
        protected readonly IPositivePayMatchingDao PositivePayMatchingDao;

        protected string currentAction;
        protected FormCollection currentFormCollection;
        protected readonly IDataProcessDao dataProcessDao;
        private readonly IAuditTrailDao auditTrailDao;
        protected readonly ISystemProfileDao systemProfileDao;

        protected PageSqlConfig pageSqlConfig { get; set; }
        //protected StartBatchICRModel gHostFileModel { get; private set; }

        protected PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId);
        }

        public PositivePayMatchingController(IPositivePayMatchingDao PositivePayMatchingDao, IDataProcessDao dataProcessDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.PositivePayMatchingDao = PositivePayMatchingDao;
            this.dataProcessDao = dataProcessDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        // GET: PPS/StartBatchICR
        public virtual ActionResult Index(FormCollection collection)
        {
            ViewBag.totalCount = PositivePayMatchingDao.getMatchingCount("all");
            ViewBag.ProcessName = "PPSMatching";
            return View();
        }

        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            if (collection["ddlBankCode"].ToString() != "")
            {
                ViewBag.totalCount = PositivePayMatchingDao.getMatchingCount(collection["ddlBankCode"].ToString());
            }
            return View();
        }

        public virtual ActionResult Search(FormCollection collection)
        {
            if (collection["ddlBankCode"].ToString() != "")
            {
                ViewBag.totalCount = PositivePayMatchingDao.getMatchingCount(collection["ddlBankCode"].ToString());
            }
            return View("Index");
        }


        public JsonResult StartMatching(FormCollection collection)
        {
            string notice = "";
            try
            {  
                string batchId = "";   
                string processName = "PPSMatching";
                string posPayType = "Matching";
                //string clearDate = collection["fldClearDate"];   
                string reUpload = "N"; 
                string taskId = "318180";  
                string clearDate = DateUtils.GetCurrentDate(); 
                
                //string fileExt = gSMSFileModel.fldFileExt;   

                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayTypeICS(processName, clearDate, CurrentUser.Account.BankCode);
                if (runningProcess)
                {
                    clearDate = Convert.ToDateTime(clearDate).ToString("dd-MM-yyyy");
                    //Delete previous data process
                    dataProcessDao.DeleteDataProcessWithoutPosPayTypeICS(processName, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                    auditTrailDao.Log("Positive Pay Matching - Date : " + clearDate + ". TaskId :" + taskId, CurrentUser.Account);
                    notice = Locale.GeneratingData;
                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            }
            catch (Exception ex)
            {
                systemProfileDao.Log("PositivePayMatchingController/Positive Pay Matching error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ViewMatchingResult()
        {
            ViewBag.totalMatchResult = PositivePayMatchingDao.getMatchingResult();
            return View();
        }

    }
}