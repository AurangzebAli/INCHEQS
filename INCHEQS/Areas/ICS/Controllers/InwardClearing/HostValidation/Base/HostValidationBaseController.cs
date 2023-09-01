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
using INCHEQS.Areas.ICS.ViewModels;
using INCHEQS.Areas.ICS.Models.HostValidation;
using INCHEQS.Areas.ICS.Models.MICRImage;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using FormCollection = System.Web.Mvc.FormCollection;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostValidation
{
    public abstract class HostValidationBaseController : BaseController
    {

        protected readonly IPageConfigDao pageConfigDao;
        protected readonly ICSIDataProcessDao cleardataProcess;
        protected readonly IFileManagerDao fileManagerDao;
        protected readonly IAuditTrailDao auditTrailDao;
        protected readonly ISearchPageService searchPageService;
        protected readonly ISystemProfileDao systemProfileDao;
        protected readonly IGenerateUPIDao generateUPIDao;
        protected readonly IHostValidationDao hostValidationDao;
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

        public HostValidationBaseController(IPageConfigDao pageConfigDao, ICSIDataProcessDao cleardataProcess, IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostValidationDao hostValidationDao, IMICRImageDao micrImageDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.cleardataProcess = cleardataProcess;
            this.fileManagerDao = fileManagerDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.hostValidationDao = hostValidationDao;
            this.micrImageDao = micrImageDao;
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
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsICS.HostValidation.INDEX, "View_HostValidation", "", "", new[] {
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
            string processName = "HostValidation";
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
            InwardItemViewModel inwardItemViewModel = new InwardItemViewModel();
            inwardItemViewModel.posPayType = "HostValidation";
            inwardItemViewModel.clearDate = collection["fldClearDate"];
            inwardItemViewModel.clearDate = DateTime.ParseExact(inwardItemViewModel.clearDate, "dd-MM-yyyy", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
            inwardItemViewModel.startDate = DateTime.Now.ToString("dd-MM-yyyy");
            inwardItemViewModel.endDate = DateTime.Now.ToString("dd-MM-yyyy");
            inwardItemViewModel.Processname = "HostValidation";
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
                    List<string> errorMessages = hostValidationDao.ValidateHostValidation(CurrentUser.Account);
                    if (errorMessages.Count > 0)
                    {
                        ErrorMsg = "AlreadyHosted";


                    }
                    else
                    {
                        hostValidationDao.InsertHostValidation(inwardItemViewModel, CurrentUser.Account);

                    }




                    //Insert new data process
                    //generateUPIDao.updateChequeType21ForUPI(clearDateDDMMYYYY);
                    //cleardataProcess.InsertToDataProcessICSUPI(CurrentUser.Account.BankCode, processName, posPayType, clearDateDDMMYYYY, reUpload, taskId, batchId, filename, CurrentUser.Account.UserId, CurrentUser.Account.UserId, filetype, fldClearingType,fldStateCode); 

                    auditTrailDao.Log("HostValidation - Date : " + clearDateDDMMYYYY + ". TaskId :" + TaskIdsICS.HostValidation.INDEX, CurrentUser.Account);
                    //TempData["Notice"] = "Generate UPI File has been triggered";
                    notice = "Host Validation";
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
                systemProfileDao.Log("HostValidationBaseController/Generate error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }

            return Json(new { notice = notice, ErrorMsg = ErrorMsg }, JsonRequestBehavior.AllowGet);


        }



        public virtual ActionResult ProgressBar(FormCollection col)
        {
            string progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            progressbarvalue = RecursiveProgressBar(progressbarvalue);

            return Json(progressbarvalue);
        }
        public string RecursiveProgressBar(string progressbarvalue)
        {

            string processname = "HostValidation";

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



        public ActionResult ChequeGenuine(FormCollection col)
        {
            try
            {
                List<HostValidationModel> list = new List<HostValidationModel>();
                HostValidationModel hostValidationModel = new HostValidationModel();
                string jsonstring = col["chk_genuineCheckvalue_"];
                int indexofbracket = jsonstring.IndexOf(']');
                indexofbracket++;
                jsonstring = jsonstring.Substring(0, indexofbracket);
                //var model = JsonConvert.DeserializeObject<HostValidationModel>(col["chk_genuineCheckvalue_"]);
                 list = JsonConvert.DeserializeObject<List<HostValidationModel>>(jsonstring);
                

                foreach (var arrResult in list)
                {
                    //string value = arrResult + "+ 1";
                    hostValidationModel.IsGenuine = arrResult.IsGenuine;// value;
                    hostValidationModel.Cheque = arrResult.Cheque;

                    hostValidationDao.UpdateGenuineness(hostValidationModel, CurrentUser.Account);
                }



                indexofbracket = 0;

                //foreach (var item in collection)
                //{

                //}

                //foreach (var key in col.AllKeys)
                //{
                //    var value = col[key];
                //    if (key.StartsWith("chk_genuineCheckvalue_"))
                //    {
                //        //Extract checque number..
                //        string chequeNumber = key.Split(new string[] { "_" }, StringSplitOptions.None)[2];
                //        bool isChecked = col[key] == "on";
                //    }
                //}

                //string x = col["checkBox"];
                //if ((col["checkBox"]) != null)
                //{
                //    List<string> arrResults = col["checkBox"].Split(',').ToList();
                //    foreach (string arrResult in arrResults)
                //    {
                //        //string value = arrResult + "+ 1";
                //        inwardItemViewModel.Genuiness = arrResult;// value;
                //        hostValidationDao.UpdateGenuineness(inwardItemViewModel, CurrentUser.Account);
                //    }

                //}
            }
            catch (Exception ex)
            {

                throw ex;
            }


            return RedirectToAction("Index");
        }

    }
}