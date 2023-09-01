using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.PPS.Models.StartBatchICR;
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
    public class StartBatchICRController : BaseController
    {
        protected readonly IStartBatchICRDao startBatchICRDao;

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

        public StartBatchICRController(IStartBatchICRDao startBatchICRDao, IDataProcessDao dataProcessDao, IAuditTrailDao auditTrailDao, ISystemProfileDao systemProfileDao)
        {
            this.startBatchICRDao = startBatchICRDao;
            this.dataProcessDao = dataProcessDao;
            this.auditTrailDao = auditTrailDao;
            this.systemProfileDao = systemProfileDao;
        }

        // GET: PPS/StartBatchICR
        public virtual ActionResult Index()
        {
            ViewBag.Status = startBatchICRDao.getOCRStatus();
            ViewBag.MachineId = startBatchICRDao.getMachineId();
            ViewBag.ProcessName = "KLInwardOCR";
            return View();
        }

        public virtual ActionResult SearchResultPage(FormCollection col)
        {
            ViewBag.MachineId = col["txtMachineID"].ToString();

            return View();
        }

        public async Task<JsonResult> percentage(String machineID)
        {
            //List<SignatureInfo> signatureList = await signatureDao.GetSignatureDetailsAsync(accountNo);
            return Json(await startBatchICRDao.percentage(machineID), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult StartBatchICR(FormCollection collection)
        {
            string notice = "";
            string batchId = "";
            string processName = "KLInwardOCR";
            string posPayType = "OCR";
            //string clearDate = collection["fldClearDate"];
            string reUpload = "N";
            string taskId = "318240";
            string clearDate = DateUtils.GetCurrentDate();

            try
            {
                
                //string fileExt = gSMSFileModel.fldFileExt;

                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayTypeICS(processName, clearDate, CurrentUser.Account.BankCode);
                if (runningProcess)
                {
                    clearDate = Convert.ToDateTime(clearDate).ToString("dd-MM-yyyy");
                    //Delete previous data process
                    dataProcessDao.DeleteDataProcessWithoutPosPayTypeICS(processName, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertSuccessToDataProcessICS(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                    auditTrailDao.Log("Start Batch ICR - Date : " + clearDate + ". TaskId :" + taskId, CurrentUser.Account);
                    notice = Locale.GeneratingData;
                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            }
            catch (Exception ex)
            {
                systemProfileDao.Log("Start Batch ICRController/Start Batch ICR error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }

            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult ViewBatchICR(FormCollection collection)
        {
            string processName = "KLInwardOCR";
            ViewBag.ICRProgress = startBatchICRDao.ICRProgressDetails(collection["txtMachineID"].ToString());
            ViewBag.StartEndTime = startBatchICRDao.getProcessStartEndTime(processName);
            ViewBag.CheckResult = startBatchICRDao.checkDataOCRTemp();
            return View("StartBatchICR");
        }

        public async Task<JsonResult> getProgressDetail(FormCollection collection)
        {
            List<StartBatchICRModel> progressDetail = await startBatchICRDao.GetProgressDetail(collection["txtMachineID"].ToString());
            return Json(progressDetail, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> upperStatus(FormCollection collection)
        {
            string processName = "KLInwardOCR";
            StartBatchICRModel progressDetail = await startBatchICRDao.GetUpperPartDetail(collection, processName);
            return Json(progressDetail, JsonRequestBehavior.AllowGet);
        }
    }
}