using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using INCHEQS.Security;

using INCHEQS.Security.AuditTrail;
using INCHEQS.Resources;
using INCHEQS.TaskAssignment;
using INCHEQS.Models.SearchPageConfig;
using System.Data.SqlClient;
using INCHEQS.Models.SearchPageConfig.Services;

using INCHEQS.Security.SecurityProfile;
using INCHEQS.Security.SystemProfile;

using INCHEQS.Security.User;
using log4net;
using System.Data;
using System.Web;

using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.HostFile;
using INCHEQS.Helpers;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.HostFile;
using System.Threading.Tasks;

namespace INCHEQS.Areas.PPS.Controllers
{
    public class LoadCOFileController : BaseController
    {
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        private static readonly ILog _log = LogManager.GetLogger(typeof(LoadCOFileController));

        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IHostFileDao hostFileDao;


        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; set; }
        protected HostFileModel gHostFileModel { get; private set; }

        //Must be implemented by inheriting class
        //public abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;
        protected string generateReportHtml;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;

        protected PageSqlConfig setupPageSqlConfig()
        {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId);
        }

        public LoadCOFileController(IPageConfigDao pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.hostFileDao = hostFileDao;
        }

        [NonAction]
        protected PageSqlConfig initializeBeforeAction()
        {

            //Expose 'currentAction' to Children Controller so that it can be intercepted and add logics accordingly
            //currentAction is action URL accessed
            //currentFormCollection is FormCollection sent to URL accessed
            //The actions are:
            // - Index
            // - GenerateReport
            currentAction = currentAction != null ? currentAction : (string)ControllerContext.RouteData.Values["action"];
            currentFormCollection = new FormCollection(ControllerContext.HttpContext.Request.Form);


            //Initializ PageSqlConfig based on: 
            // - Inherited Controller initialization of setupPageSqlConfig()
            // - Request Query String of TaskId and ViewId
            pageSqlConfig = setupPageSqlConfig();

            //Global constructor host file model
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(pageSqlConfig.TaskId);

            try
            {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            }
            catch (HttpException ex)
            {
                systemProfileDao.Log("LoadCOFileController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }

        public virtual async Task<ActionResult> Index()
        {
            initializeBeforeAction();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.ProcessName = gHostFileModel.fldProcessName;
            ViewBag.FileTypeExt = gHostFileModel.fldFileExt;

            return View();
        }



        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection)
        {
            initializeBeforeAction();
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);

            return View();
        }

        public virtual ActionResult Download(FormCollection collection)
        {
            initializeBeforeAction();
            string filePath = collection["this_fldFilePath"];
            string fileName = collection["this_fldFileName"];
            string fullFileName = string.Format("{0}", filePath);
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, "txt"));
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }

        }

        public virtual JsonResult LoadCOFile(FormCollection collection)
        {
            initializeBeforeAction();

            string notice = "";
            string processName = gHostFileModel.fldProcessName;
            string posPayType = gHostFileModel.fldPosPayType;
            string clearDate = collection["fldClearDate"];
            string fileName = processName;
            string reUpload = "Y";
            string taskId = pageSqlConfig.TaskId;
            string batchId = "";

            //List<String> errorMessages = ValidateHostFile(collection);

            //if ((errorMessages.Count > 0)) {
            //    notice = "ERROR:<br/>" + string.Join("<br/>", errorMessages.ToArray());
            //} else {

            try
            {                                                                                                                                               
                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                if (runningProcess)
                {

                    //Delete previous data process
                    dataProcessDao.DeleteDataProcess(processName, fileName, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, fileName, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);
                    fileManagerDao.UpdateFileManagerLoadFile(fileName, clearDate);

                    auditTrailDao.Log("Load CO File (Details) - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                    notice = "Loading Data";

                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            }
            catch (Exception ex)
            {
                systemProfileDao.Log("LoadCOFileController/LoadCOFile error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }
            // }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }
    }
}