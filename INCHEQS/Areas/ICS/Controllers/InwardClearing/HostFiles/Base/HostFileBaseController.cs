using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.HostFile;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.HostFile;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Resources;
using INCHEQS.Security;
//using INCHEQS.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostFiles
{
    public abstract class HostFileBaseController : BaseController {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IHostFileDao hostFileDao;


        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }
        protected HostFileModel gHostFileModel { get; private set; }

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
            gHostFileModel = hostFileDao.GetDataFromHostFileConfig(pageSqlConfig.TaskId);

            if (pageSqlConfig.TaskId == "309220") { 
                ReadGWCAck(currentFormCollection);
            }

            try {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            } catch (HttpException ex) {
                systemProfileDao.Log("HostFileBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }

        public HostFileBaseController(IPageConfigDao pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao) {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.hostFileDao = hostFileDao;
        }



        public virtual async Task<ActionResult> Index() {
            initializeBeforeAction();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            ViewBag.ProcessName = gHostFileModel.fldProcessName;
            ViewBag.FileTypeExt = gHostFileModel.fldFileExt;

            return View();
        }



        public virtual async Task<ActionResult> SearchResultPage(FormCollection collection) {
            initializeBeforeAction();
            ViewBag.SearchResult = await pageConfigDao.getResultListFromDatabaseViewAsync(pageSqlConfig, collection);
            ViewBag.TaskRole = gHostFileModel.fldTaskRole;

            return View();
        }


        public virtual ActionResult Download(FormCollection collection) {
            initializeBeforeAction();
            
            string filePath = collection["this_fldFilePath"];
            string fileName = collection["this_fldFileName"];
            string fullFileName = string.Format("{0}", filePath);
            if (System.IO.File.Exists(fullFileName)) {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, "txt"));
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            } else {
                return null;
            }


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

        public virtual JsonResult SendFTP(FormCollection collection) {
            initializeBeforeAction();

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string notice = "";

            string hostName = systemProfileDao.GetValueFromSystemProfile("FTPHostName", CurrentUser.Account.BankCode); ;
            string userName = systemProfileDao.GetValueFromSystemProfile("FTPUserName", CurrentUser.Account.BankCode); ;
            string password = systemProfileDao.GetValueFromSystemProfile("FTPPassword", CurrentUser.Account.BankCode); ;
            string fileName = filter["fldFileName"];
            string filePath = filter["fldFilePath"];
            string FTPFolder = gHostFileModel.fldFTPFolder;

            //int ftpResult = FTPService.FTPSendFile(hostName,userName, password, fileName, filePath, FTPFolder);
            //if(ftpResult == 0) {
            //    //update fldSend
            //    hostFileDao.UpdateFTPSend(filter["fldFileName"]);
            //    notice = "FTP Send SUCCESS";
            //}else {
            //    notice = "ERROR: FTP Send FAILED";
            //}

            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReadGWCAck(FormCollection collection)
        {
            String GWCpath = systemProfileDao.GetValueFromSystemProfile("OutwardReturnedGatewayFolder", CurrentUser.Account.BankCode);

            String FolderCompleted = GWCpath + @"Completed\";
            
            string[] fileEntries = Directory.GetFiles(FolderCompleted);
            foreach (string fileName in fileEntries) {
              String Resfilename =   fileName;
                FileInfo file = new FileInfo(fileName);

                    string[] lines = System.IO.File.ReadAllLines(Resfilename);

                    foreach (string line in lines)
                    {
                             hostFileDao.UpdateUPIbatchAck(line);
                    }
              

            }
            String FolderError = GWCpath + @"Error\";
            string[] fileEntries2 = Directory.GetFiles(FolderError);
            foreach (string fileName2 in fileEntries2)
            {
                String Resfilename2 = fileName2;
                FileInfo file = new FileInfo(fileName2);

                string[] lines = System.IO.File.ReadAllLines(Resfilename2);

                foreach (string line in lines)
                {
                    hostFileDao.UpdateUPIbatchAck(line);
                }
                //    } 


            }
            return null;

            //string fullFileName = string.Format("{0}", filePath);
            //if (System.IO.File.Exists(fullFileName))
            //{
            //    byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
            //    Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, "txt"));
            //    return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            //}
            //else
            //{
            //    return null;
            //}
        }
    }
}