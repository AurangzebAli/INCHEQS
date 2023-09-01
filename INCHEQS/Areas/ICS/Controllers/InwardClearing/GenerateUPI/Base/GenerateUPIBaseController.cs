using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Areas.ICS.Models.GenerateUPI;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using System.Linq;
using INCHEQS.Common;


using System.Globalization;
using INCHEQS.Areas.ICS.Models.MICRImage;


namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.GenerateUPI
{
    public abstract class GenerateUPIBaseController : BaseController {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IGenerateUPIDao generateUPIDao;
        protected readonly IMICRImageDao micrImageDao;


        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }
        //protected OutwardReturnICLModel outwardReturnICLModel { get; private set; }

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

            ////Global constructor upi file model
            //outwardReturnICLModel = outwardReturnICLDao.GetDataFromOutwardReturnICLConfig(pageSqlConfig.TaskId);

            try {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            } catch (HttpException ex) {
                systemProfileDao.Log("OutwardReturnICLBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }

        public GenerateUPIBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao,IGenerateUPIDao generateUPIDao,IMICRImageDao micrImageDao) {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.generateUPIDao = generateUPIDao;
            this.micrImageDao = micrImageDao;   
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateUPI.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            GenerateUPIModel gGenerateUPIModel = new GenerateUPIModel();
            gGenerateUPIModel = generateUPIDao.GetDataFromUPIConfig(TaskIdsICS.GenerateUPI.INDEX);
            //if (generateUPIDao.GetLateMaintenaceUPI())
            //{
            //    TempData["Warning"] = "There is Late Returned Item(s) not yet generated";
            //}
            /*else
            {
                TempData["Notice"] = "No Late Returned";
            }*/
            ViewBag.ProcessName = gGenerateUPIModel.fldProcessName; 
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.GenerateUPI.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateUPI.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateUPI.INDEX, "View_GenerateUPI", "fldBatchNo", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateUPI.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual JsonResult Generate(FormCollection collection)
        {
            //initializeBeforeAction();

            Dictionary<string, string> errors = new Dictionary<string, string>();
            GenerateUPIModel gGenerateUPIModel = new GenerateUPIModel();
            gGenerateUPIModel = generateUPIDao.GetDataFromUPIConfig(TaskIdsICS.GenerateUPI.INDEX);
            string processName = gGenerateUPIModel.fldProcessName;
            string posPayType = gGenerateUPIModel.fldPosPayType;
            string reUpload = "N";
            string taskId = TaskIdsICS.GenerateUPI.INDEX;
            string batchId = collection["fldBatchNo"];
            string bankcodeChkbox = "";
            string returnType = "";
            string totalItem = "";
            string totalAmount = "";
            // string clearDateDDMMYYYY = collection["row_fldClearDate"];
            string clearDate = collection["fldClearDate"];
            //string clearDateDDMMYYYY = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
            string clearDateDDMMYYYY = DateUtils.formatDateToSqlyyyymmdd(clearDate);
            string filename = "";
            string notice = "";
            string ErrorMsg = "";
            string filetype = collection["fileTypeName"];
            string fldClearingType = collection["fldClearingType"];
            string fldStateCode = collection["fldStateCode"];



            //if (batchId.Trim() == null || batchId.Trim() == "")
            //{
            //    batchId = "0";
            //}


            //string t = collection["Type"];

            //if (t == null)
            //{
            //    t = "Ready";
            //}
            //else
            //{
            //    t = collection["Type"];
            //}

            //if (collection != null && collection["deleteBox"] != null)
            //{
            //if (t.Equals("Ready") || t.Equals("ReadyLateReturn"))
            //{
            try
            {
                        //List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                        //foreach (string arrResult in arrResults)
                        //{
                            //bankcodeChkbox = generateUPIDao.getBetween(arrResult, "ae", "bf");
                            //returnType = generateUPIDao.getBetween(arrResult, "bf", "dh");

                            //To update flag status for items that has been pickup for upi processing
                            //generateUPIDao.updateICSItemReadyForUPI(clearDateDDMMYYYY, bankcodeChkbox,returnType);


                            bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, posPayType, clearDate, CurrentUser.Account.BankCode);
                            if (runningProcess)
                            {

                                //Insert new data process
                                generateUPIDao.updateChequeType21ForUPI(clearDateDDMMYYYY);
                                cleardataProcess.InsertToDataProcessICSUPI(CurrentUser.Account.BankCode, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId, filetype, fldClearingType,fldStateCode); 
                                
                                auditTrailDao.Log("Generate UPI - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.GenerateUPI.INDEX, CurrentUser.Account);
                                //TempData["Notice"] = "Generate UPI File has been triggered";
                                notice = "Generate UPI File has been triggered";
                            }
                            else
                            {
                            //TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                                ErrorMsg = Locale.CurrentlyRunningPleaseWaitAMoment;
                            }
                        //}

                    
                
                    }
                    catch (Exception ex)
                    {
                        systemProfileDao.Log("GenerateUPIBaseController/Generate error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        throw ex;
                    }

            //}

            //}
            //else
            //{
            //TempData["ErrorMsg"] = "No Data was selected";
            //}
            //return RedirectToAction("Index");
            return Json(new { notice = notice, ErrorMsg = ErrorMsg }, JsonRequestBehavior.AllowGet);


        }


        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateUPI.INDEX)]
        [HttpPost()]
        public virtual ActionResult Download(FormCollection collection)
        {
            //initializeBeforeAction();
            string taskId = TaskIdsICS.GenerateUPI.INDEX;
            GenerateUPIModel gGenerateUPIModel = new GenerateUPIModel();
            gGenerateUPIModel = generateUPIDao.GetDataFromUPIConfig(TaskIdsICS.GenerateUPI.INDEX);

            string filePath = collection["this_fldFilePath"];
            string fileName = collection["this_fldFileName"];
            string fullFileName = string.Concat(filePath,"\\",fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, gGenerateUPIModel.fldFileExt));
                auditTrailDao.Log("Generate UPI Download File : " + fullFileName + ". TaskId :" + TaskIdsICS.GenerateUPI.INDEX, CurrentUser.Account);
                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }


        }

        public ActionResult ReadyNormalItemForUPI(FormCollection collection)
        {

            ViewBag.PostedHistory = generateUPIDao.ReadyItemForPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostPopup");

        }
        public ActionResult ReadyLateReturnItemForUPI(FormCollection collection)
        {

            ViewBag.PostedHistory = generateUPIDao.ReadyItemForLateReturnPostingHistory(collection);
            return PartialView("Modal/_ReadyToPostLateReturnPopup");

        }
        public ActionResult GeneratedItemsForUPI(FormCollection collection)
        {

            ViewBag.PostedHistory = generateUPIDao.PostedItemHistory(collection);
            return PartialView("Modal/_PostedItemHistoryPopup");

        }


        public virtual ActionResult ProgressBar(FormCollection col)
        {
            string progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            progressbarvalue = RecursiveProgressBar(progressbarvalue);

            return Json(progressbarvalue);
        }
        public string RecursiveProgressBar(string progressbarvalue)
        {

            string processname = "ICSGenUPI";

            if (progressbarvalue == "")
            {
                System.Threading.Thread.Sleep(1000);
                progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                RecursiveProgressBar(progressbarvalue);
            }
            if (progressbarvalue == "1")
            {
                System.Threading.Thread.Sleep(1000);
                progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);


            }
            if (progressbarvalue == "2")
            {
                System.Threading.Thread.Sleep(1000);
                progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
            }
            if (progressbarvalue == "3")
            {
                System.Threading.Thread.Sleep(1000);
                progressbarvalue = micrImageDao.BroadCastProgressBar(processname, "");
                progressbarvalue = progressbarvalue == "4" ? "4" : RecursiveProgressBar(progressbarvalue);
            }
            if (progressbarvalue == "4")
            {
                progressbarvalue = "4";
            }

            return progressbarvalue;

        }


    }
}