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
using INCHEQS.Areas.ICS.Models.MICRImage;

namespace INCHEQS.Areas.ICS.Controllers.Utilities
{

    public class DayEndProcessController : BaseController
    {

        private readonly IDataHouseKeepDao datahouse;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly FileInformationService fileInformationService;
        private readonly ISystemProfileDao systemProfileDao;
        private readonly IDataProcessDao dataProcessDao;
        private IDayEndProcessDao dayendProcess;
        protected readonly IMICRImageDao micrImageDao;




        public DayEndProcessController(IDayEndProcessDao dayendProcess, IDataHouseKeepDao dataHouseKeep, IAuditTrailDao auditTrailDao, FileInformationService fileInformationService, ISystemProfileDao systemProfileDao, IDataProcessDao dataProcessDao, IMICRImageDao micrImageDao)
        {
            this.dayendProcess = dayendProcess;
            this.datahouse = dataHouseKeep;
            this.auditTrailDao = auditTrailDao;
            this.fileInformationService = fileInformationService;
            this.systemProfileDao = systemProfileDao;
            this.dataProcessDao = dataProcessDao;
            this.micrImageDao = micrImageDao;
        }



        [CustomAuthorize(TaskIds = TaskIds.DayEndProcess.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.PurgeAudit = datahouse.GetDataHouseKeep();
            ViewBag.ArchiveChequeInfo = dayendProcess.GetChequeInfo(CurrentUser.Account.BankCode);

            return View();
        }



        public ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.PurgeAudit = datahouse.GetDataHouseKeep();
            ViewBag.ArchiveChequeInfo = dayendProcess.GetChequeInfo(CurrentUser.Account.BankCode);
            TempData["Notice"] = null;
            return View();
        }

        [HttpPost]
        public JsonResult ChequeVerification(FormCollection col)
        {
            string notice = "";
            
            List<string> totoalcheques = dayendProcess.Validate(col, CurrentUser.Account.BankCode);
            TempData["Notice"] = totoalcheques;
            notice = totoalcheques[0];
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);

        }

        [CustomAuthorize(TaskIds = TaskIds.DayEndProcess.SAVECREATE)]
        [HttpPost]
        public JsonResult SaveCreate(FormCollection col)
        {
            string notice = "";
            string datetest = col["CIAchDate"].ToString();
            string a = datetest;
            List<string> errorMessages = new List<string>(); //dayendProcess.Validate(col, CurrentUser.Account.BankCode);
            string tempPath = systemProfileDao.GetValueFromSystemProfile("ChequeTempFolderPath", CurrentUser.Account.BankCode);

            //initialize parameters
            string processName = "ICSEOD";
            string otherTask = "'ICSImport','ICSExtractICL'";

            string taskId = TaskIds.DayEndProcess.SAVECREATE;

            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {
                //Check running process if not "" OR "4" then dont Do process
                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayTypeICS(processName, DateUtils.formatDateToSql(col["CIAchDate"]), CurrentUser.Account.BankCode);
                //Do process
                if (runningProcess)
                {

                    bool runningProcess2 = dataProcessDao.CheckRunningProcessBeforeEod(otherTask, col["CIAchDate"], CurrentUser.Account.BankCode);
                    if (runningProcess2)
                    {
                        //Add to data process
                        dataProcessDao.InsertToDataProcessICS(CurrentUser.Account.BankCode, processName, "ICSEOD", col["CIAchDate"], "", taskId, "", CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                        //Delete file in tmp folder
                        fileInformationService.DeleteDirectory(tempPath, true, tempPath);

                        auditTrailDao.Log("Day End Process - clear date : " + DateUtils.formatDateToSql(col["CIAchDate"]), CurrentUser.Account);
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
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }


        public virtual ActionResult ProgressBar(FormCollection col)
        {
            string progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            progressbarvalue = RecursiveProgressBar(progressbarvalue);
            //if (progressbarvalue == "2")
            //{
            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            ////    progressbarvalue = "20";
            //}
            //if (progressbarvalue == "3")
            //{


            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            //    progressbarvalue = "60";
            //}
            //if (progressbarvalue == "4")
            //{
            //    progressbarvalue = micrImageDao.BroadCastProgressBar("", "");
            //    progressbarvalue = "100";
            //}

            return Json(progressbarvalue);
        }
        public string RecursiveProgressBar(string progressbarvalue)
        {

            string processname = "ICSEOD";

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