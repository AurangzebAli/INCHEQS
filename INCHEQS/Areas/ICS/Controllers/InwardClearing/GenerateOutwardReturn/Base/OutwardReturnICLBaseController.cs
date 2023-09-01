using INCHEQS.Processes.DataProcess;
using INCHEQS.Areas.ICS.Models.OutwardReturnICL;
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


using System.Globalization;


namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.GenerateOutwardReturn
{
    public abstract class OutwardReturnICLBaseController : BaseController {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly IDataProcessDao dataProcessDao;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IOutwardReturnICLDao outwardReturnICLDao;


        //Readables by inheriting class
        protected PageSqlConfig pageSqlConfig { get; private set; }
        protected OutwardReturnICLModel outwardReturnICLModel { get; private set; }

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
            outwardReturnICLModel = outwardReturnICLDao.GetDataFromOutwardReturnICLConfig(pageSqlConfig.TaskId);

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

        public OutwardReturnICLBaseController(IPageConfigDao pageConfigDao, IDataProcessDao dataProcessDao, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao,IOutwardReturnICLDao outwardReturnICLDao) {
            this.pageConfigDao = pageConfigDao;
            this.dataProcessDao = dataProcessDao;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.outwardReturnICLDao = outwardReturnICLDao;
        }

       
        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateOutwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual async Task<ActionResult> Index() {
            initializeBeforeAction();

            ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            
            return View();
        }


      

        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateOutwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection) {
        //public virtual async Task<ActionResult> SearchResultPage(FormCollection collection)
        //{
            initializeBeforeAction();
           


            //return View();
            string t = collection["Type"];

            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (t.Equals("Ready"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult =  pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateOutwardReturnICL.INDEX, "View_GetItemsforClearingICS", "", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
              collection);
            }
            else if (t.Equals("Submitted"))
            {
                ViewBag.Type = t;
                ViewBag.SearchResult =  pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.GenerateOutwardReturnICL.Cleared, "View_GetClearedItemsICS", "", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            }
            return View();
        }

     
        [CustomAuthorize(TaskIds = TaskIdsICS.GenerateOutwardReturnICL.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        //public ActionResult Generate(FormCollection collection)
        //{
        //public virtual async Task<ActionResult> Generate(FormCollection collection)
        //{
        public JsonResult Generate(FormCollection collection)
        {
            initializeBeforeAction();
            string notice = "";
            string t = collection["Type"];
            if (t == null)
            {
                t = "Ready";
            }
            else
            {
                t = collection["Type"];
            }

            if (collection != null && collection["deleteBox"] != null)
            {
                if (t.Equals("Ready"))
                {
                    List<string> arrResults = collection["deleteBox"].Split(',').ToList();
                    foreach (string arrResult in arrResults)
                    {
                        string issuingBankBranch = outwardReturnICLDao.getBetween(arrResult, "cb", "si");
                        string clearDate = outwardReturnICLDao.getBetween(arrResult, "si", "cd");
                        
                        
                        outwardReturnICLDao.updateICSItemReadyForReturnICL(clearDate,issuingBankBranch);
                    }

                    try
                    {

                        string batchId = "";
                        string processName = outwardReturnICLModel.fldProcessName;
                        string posPayType = outwardReturnICLModel.fldPosPayType;
                        string clearDate = collection["fldClearDate"];
                        string clearDateDDMMYYYY = collection["row_fldClearDate"];
                        string clearDateyMMddyyyy = DateTime.ParseExact(clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("MM-dd-yyyy");
                        string fileExt = outwardReturnICLModel.fldFileExt;
                        string reUpload = "N";
                        string taskId = pageSqlConfig.TaskId;
                        string filename = "";
                        if (taskId == "309220")
                        {

                            if (collection.AllKeys.Contains("fldBatchNo"))
                            {
                                batchId = collection["fldBatchNo"];
                            }
                            else
                            {
                                batchId = "0";
                            }

                        }



                        bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayTypeICS(processName, clearDateyMMddyyyy, CurrentUser.Account.BankCode);
                        if (runningProcess)
                        {

                            //update fldupistatus = 1
                            //outwardReturnICLDao.updateICSItemReadyForReturnICL()


                            //Delete previous data process
                            dataProcessDao.DeleteDataProcessWithoutPosPayTypeICS(processName, CurrentUser.Account.BankCode);
                            //Insert new data process
                            dataProcessDao.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId,filename);

                            CurrentUser.Account.TaskId = TaskIdsICS.GenerateOutwardReturnICL.INDEX;
                            auditTrailDao.Log("Generate Outward Return File - Date : " + clearDate + ". TaskId :" + TaskIdsICS.GenerateOutwardReturnICL.INDEX, CurrentUser.Account);
                            //auditTrailDao.SecurityLog("Generate Outward Return File - Date : " + clearDate, "", TaskIdsICS.GenerateOutwardReturnICL.INDEX, CurrentUser.Account);

                            //TempData["Notice"] = Locale.GeneratingData;
                            notice = Locale.GeneratingData;
                        }
                        else
                        {
                            //TempData["Notice"] = Locale.CurrentlyRunningPleaseWaitAMoment;
                            notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                        }
                    }
                    catch (Exception ex)
                    {
                        systemProfileDao.Log("OutwardReturnICLBaseController/OutwardReturnICL error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                        throw ex;
                    }
                   
                }

            }
            else
            {
                //TempData["ErrorMsg"] = "No Data was selected";
                notice = "No Data was selected";
            }


            //ViewBag.SearchPage = await pageConfigDao.GetSearchFormModelFromConfigAsync(CurrentUser.Account, pageSqlConfig);
            //return View();
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);


        }
        public virtual ActionResult Download(FormCollection collection)
        {
            initializeBeforeAction();

            string filePath = collection["this_fldFilePath"];
            string fileName = collection["this_fldFileName"];
            string fullFileName = string.Format("{0}", filePath+fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(fullFileName);
                //Response.AddHeader("content-disposition", string.Format("attachment; filename={0}.{1}", fileName, ""));
                Response.AddHeader("content-disposition", string.Format("attachment; filename={0}", fileName));

                return File(fileBytes, MimeMapping.GetMimeMapping(fullFileName));
            }
            else
            {
                return null;
            }


        }




    }
}