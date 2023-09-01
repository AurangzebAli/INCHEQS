using INCHEQS.Processes.DataProcess;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Resources;
using INCHEQS.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Models.OCSAIFFileUploadDao;
using INCHEQS.Areas.OCS.Models.OCSAIFFileUpload;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public abstract class AIFFileBaseController : BaseController {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IOCSAIFFileUploadDao AIFFileDao;


        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }
        protected OCSAIFFileUploadModel gAIFFileModel { get; private set; }

        //Must be implemented by inheriting class
        protected abstract PageSqlConfig setupPageSqlConfig();

        protected string searchPageHtml;
        protected string generateReportHtml;

        //Readables by inheriting class
        protected string currentAction;
        protected FormCollection currentFormCollection;

        /// <summary>
        /// This function should be called inside All Actions in ICCSBaseController and it's Inheritence Controller.
        /// This is to protect from UNAUTHORIZED ACCESS to the page through TASKID returned by setupPageSqlConfig().
        /// This function is important and have to be called or else, the page won't work
        /// Returns: PageSqlConfig set in setupPageSqlConfig();
        /// </summary>
        [NonAction]
        protected PageSqlConfig initializeBeforeAction() {

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
            gAIFFileModel = AIFFileDao.GetDataFromHostFileConfig(pageSqlConfig.TaskId);


            try {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            } catch (HttpException ex) {
                systemProfileDao.Log("AIFFileBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }

        public AIFFileBaseController(IPageConfigDao pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IOCSAIFFileUploadDao AIFFileDao) {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.AIFFileDao = AIFFileDao;
        }



        public virtual async Task<ActionResult> Index() {
            initializeBeforeAction();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.ProcessName = gAIFFileModel.fldProcessName;
            ViewBag.FileTypeExt = gAIFFileModel.fldFileExt;

            return View();
        }



        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
            initializeBeforeAction();
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);
            ViewBag.TaskRole = gAIFFileModel.fldTaskRole;

            return View();
        }

        
        public virtual ActionResult Action(FormCollection collection)
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
    }
}