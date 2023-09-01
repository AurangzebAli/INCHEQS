using INCHEQS;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.EOD.DataHouseKeep;
using INCHEQS.Processes.DataProcess;
using INCHEQS.EOD.DayEndProcess;
using INCHEQS.Models.FileInformation;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using INCHEQS.Common;

namespace INCHEQS.Areas.OCS.Controllers.OutwardClearing
{

    public class DayEndProcessController : BaseController
    {

        private readonly IDataHouseKeepDao datahouse;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly FileInformationService fileInformationService;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly OCSIDataProcessDao dataProcessDao;

        private IDayEndProcessDao dayendProcess;
        public DayEndProcessController(IDayEndProcessDao dayendProcess, IDataHouseKeepDao dataHouseKeep, IAuditTrailDao auditTrailDao, FileInformationService fileInformationService, ISystemProfileDao systemProfileDao, OCSIDataProcessDao dataProcessDao)
        {
            this.dayendProcess = dayendProcess;
            this.datahouse = dataHouseKeep;
            this.auditTrailDao = auditTrailDao;
            this.fileInformationService = fileInformationService;
            this.systemProfileDao = systemProfileDao;
            this.dataProcessDao = dataProcessDao;
        }
        // GET: DayEndProcess

        [CustomAuthorize(TaskIds = TaskIds.DayEndProcessOCS.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.PurgeAudit = datahouse.GetDataHouseKeep();
            ViewBag.ArchiveChequeInfo = dayendProcess.GetChequeInfo(CurrentUser.Account.BankCode);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.DayEndProcessOCS.SAVECREATE)]
        [HttpPost]
        public ActionResult SaveCreate(FormCollection col)
        {
            string notice = "";
            List<string> errorMessages = dayendProcess.Validate(col, CurrentUser.Account.BankCode);
            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);

            //initialize parameters
            string processName = "incheqseod";
            string otherTask = "'ICSImport','ICSExtractICL'";

            string taskId = TaskIds.DayEndProcessOCS.SAVECREATE;

            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {

                //Check running process if not "" OR "4" then dont Do process
                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, col["CIAchDate"], CurrentUser.Account.BankCode);
                //Do process
                if (runningProcess)
                {

                    bool runningProcess2 = dataProcessDao.CheckRunningProcessBeforeEod(otherTask, col["CIAchDate"], CurrentUser.Account.BankCode);
                    if (runningProcess2)
                    {
                        //Add to data process
                        dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, "", col["CIAchDate"], "", taskId, "", CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                        //Delete file in tmp folder
                        fileInformationService.DeleteDirectory(tempPath, true, tempPath);

                        auditTrailDao.Log("Day End Process - clear date : " + DateUtils.formatTimeStampFromSql(col["CIAchDate"]), CurrentUser.Account);
                        notice = Locale.Endofdayprocesshasbeenstarted;
                    }
                    else
                    {
                        notice = Locale.TaskCurrentlyRunnig;
                    }

                }
                else
                {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }

                TempData["Notice"] = notice;
            }
            return RedirectToAction("Index");
        }
    }
}