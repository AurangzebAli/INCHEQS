using INCHEQS.Areas.ICS.Models.GenerateUPI;
using INCHEQS.Areas.ICS.Models.HostValidation;
using INCHEQS.Areas.ICS.Models.ICSDataProcess;
using INCHEQS.Areas.ICS.Models.MICRImage;
using INCHEQS.Models.FileInformation;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Security.SystemProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Helpers;
using INCHEQS.Security;
using INCHEQS.Common;
using INCHEQS.TaskAssignment;
using System.Data.SqlClient;
using System.Globalization;
using INCHEQS.Common.Resources;
using INCHEQS.Areas.ICS.Models.DayEndSettlement;
using System.Data;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.DayEndSettlement
{
    public abstract class DayEndSettlementBaseController : BaseController
    {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IGenerateUPIDao generateUPIDao;
        protected readonly IDayEndSettlementDao DayEndSettlementDao;
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

            ////Global constructor upi file model
            //outwardReturnICLModel = outwardReturnICLDao.GetDataFromOutwardReturnICLConfig(pageSqlConfig.TaskId);

            try
            {
                //Check for UNAUTHORIZED ACCESS
                //Reject if User Does Not Have Access               
                RequestHelper.RestrictAccessToUserBasedOnTaskId(ControllerContext, pageSqlConfig.TaskId);

                return pageSqlConfig;
            }
            catch (HttpException ex)
            {
                systemProfileDao.Log("OutwardReturnICLBaseController/initializeBeforeAction error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw new HttpException(Locale.AccessDenied);
            }
        }
        public DayEndSettlementBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IMICRImageDao micrImageDao,IDayEndSettlementDao DayEndSettlementDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.micrImageDao = micrImageDao;
            this.DayEndSettlementDao = DayEndSettlementDao;
        }


        [CustomAuthorize(TaskIds = TaskIdsICS.HostValidation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {

            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsICS.HostValidation.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.HostValidation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.DayEndSettlement.INDEX, "View_DayEndSettlement", "", "", new[] {
                    new SqlParameter("@fldUserID", CurrentUser.Account.UserId)}),
             collection);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsICS.HostValidation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual JsonResult Generate(FormCollection collection)
        {
            //initializeBeforeAction();

            Dictionary<string, string> errors = new Dictionary<string, string>();
            string processName = "DayEndSettlement";
            string reUpload = "N";
            string taskId = TaskIdsICS.HostValidation.INDEX;
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
            DayEndSettlementModel inwardItemViewModel = new DayEndSettlementModel();
            inwardItemViewModel.posPayType = "DayEndSettlement";
            inwardItemViewModel.clearDate = collection["fldClearDate"];
            inwardItemViewModel.clearDate = DateTime.ParseExact(inwardItemViewModel.clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
            inwardItemViewModel.startDate = DateTime.Now.ToString("dd-MM-yyyy");
            inwardItemViewModel.endDate = DateTime.Now.ToString("dd-MM-yyyy");
            inwardItemViewModel.Processname = "DayEndSettlement";
            inwardItemViewModel.bankCode = "083";
            inwardItemViewModel.status = "1";


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

                bool runningProcess = cleardataProcess.CheckRunningProcessICS(processName, inwardItemViewModel.posPayType, clearDate, CurrentUser.Account.BankCode);
                if (runningProcess)
                {
                    //await initializeBeforeAction();
                    List<string> errorMessages = DayEndSettlementDao.ValidateDayEndsettlement(CurrentUser.Account);
                    if (errorMessages.Count > 0)
                    {
                        ErrorMsg = "AlreadySettled";


                    }
                    else
                    {
                        //DayEndSettlementDao.InsertDayEndSettlement(inwardItemViewModel, CurrentUser.Account);
                        
                        notice =   DayEndSettlementDao.GetTableForDayEndSettlement(collection,CurrentUser.Account);

                    }




                    //Insert new data process
                    //generateUPIDao.updateChequeType21ForUPI(clearDateDDMMYYYY);
                    //cleardataProcess.InsertToDataProcessICSUPI(CurrentUser.Account.BankCode, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId, filetype, fldClearingType,fldStateCode); 

                    auditTrailDao.Log("DayEndSettlement - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.HostValidation.INDEX, CurrentUser.Account);
                    //TempData["Notice"] = "Generate UPI File has been triggered";
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
                systemProfileDao.Log("DayEndSettlement/Generate error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }

            return Json(new { notice = notice, ErrorMsg = ErrorMsg }, JsonRequestBehavior.AllowGet);


        }


    }
}