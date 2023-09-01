using INCHEQS.ConfigVerificationBranch.BranchActivation;
using INCHEQS.Helpers;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Common;
using INCHEQS.Models.SearchPageConfig.Services;
using System.Collections.ObjectModel;
using ImageProcessor.Processors;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.ProgressMonitoring
{
    public class BranchActivationController : BaseController
    {

        private readonly IPageConfigDao pageConfigDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IBranchActivationDao branchActivationDao;
        protected readonly ISearchPageService searchPageService;
        public BranchActivationController(IPageConfigDao pageConfigDao, IBranchActivationDao branchActivationDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService)
        {
            this.pageConfigDao = pageConfigDao;
            this.branchActivationDao = branchActivationDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;

        }

        // GET: ICS/BranchActivation
        [CustomAuthorize(TaskIds = TaskIds.BranchActivation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BranchActivation.INDEX));
            ViewBag.GetMessage = branchActivationDao.GetMessage();
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchActivation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult SaveMessage(FormCollection col)
        {

            List<String> errorMessages = branchActivationDao.Validate(col);
            string spickCode = "KL";
            if ((errorMessages.Count > 0))
            {
                TempData["ErrorMsg"] = errorMessages;
            }
            else
            {
                if (col["txtMessage"] != null && col["txtMessage"] != "")
                {
                    branchActivationDao.InsertNewMessage(col["txtMessage"], CurrentUser.Account.UserId, CurrentUser.Account.BankCode, spickCode);
                    auditTrailDao.Log("Message : " + col["txtMessage"] + " - send to Branch.", CurrentUser.Account);

                    TempData["Notice"] = "Message added successfully";
                }
                if (col["MessageOption"] != "" && col["MessageOption"] != null)
                {
                    branchActivationDao.InsertNewMessage(col["MessageOption"], CurrentUser.Account.UserId, CurrentUser.Account.BankCode, spickCode);
                    auditTrailDao.Log("Message : " + col["MessageOption"] + " - send to Branch.", CurrentUser.Account);
                    TempData["Notice"] = "Message added successfully";
                }
            }
            return RedirectToAction("Index");
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchActivation.INDEX)]
        public ActionResult SearchResultPage(FormCollection collection)
        {
            string clearDate = collection["fldClearDate"];

            ViewBag.CutOfftime = branchActivationDao.GetCutOffTimes(clearDate);
            ViewBag.ChequeActivation = branchActivationDao.GetChequesActivation(clearDate);
            ViewBag.Message = branchActivationDao.GetMessageList();

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BranchActivation.INDEX)]
        public ActionResult Update(FormCollection col)
        {
            //string activation = col["cutOffActivate"];
            //string cutOffStartTime = DateUtils.formatDateToSql(col["fldClearDate"]) + " " + col["endHourStartTime"] + ":" + col["endMinStartTime"] + ":000";
            //string cutOffTime = DateTime.Now.ToString("dd/MMM/yyyy") + " " + col["endHourInterCityClearing"] + ":" + col["endMinInterCityClearing"] + ":000";
            //string cutOffTimeFrom = DateTime.Now.ToString("dd/MMM/yyyy") + " " + col["startHourInterCityClearing"] + ":" + col["startMinInterCityClearing"] + ":000";

            string clearDate = DateUtils.formatDateToSql(col["fldClearDate"]);

            List<CutofTime> cutofTimes = new List<CutofTime>();
            CutofTime cutofTime = new CutofTime();
            cutofTime.clsCutofTimeFrom = clearDate + " " + col["startHourInterCityClearing"] + ":" + col["startMinInterCityClearing"] + ":000";
            cutofTime.clsCutofTimeTo = clearDate + " " + col["endHourInterCityClearing"] + ":" + col["endMinInterCityClearing"] + ":000";
            cutofTime.clsActivate = col["cutOffActivateInterCityClearing"];
            cutofTime.ClearingType = "01";
            cutofTimes.Add(cutofTime);
            cutofTime = new CutofTime();
            cutofTime.clsCutofTimeFrom = clearDate + " " + col["startHourNormalClearing"] + ":" + col["startMinNormalClearing"] + ":000";
            cutofTime.clsCutofTimeTo = clearDate + " " + col["endHourNormalClearing"] + ":" + col["endMinNormalClearing"] + ":000";
            cutofTime.clsActivate = col["cutOffActivateNormalClearing"];
            cutofTime.ClearingType = "02";

            cutofTimes.Add(cutofTime);
            cutofTime = new CutofTime();
            cutofTime.clsCutofTimeFrom = clearDate + " " + col["startHourSameDayClearing"] + ":" + col["startMinSameDayClearing"] + ":000";
            cutofTime.clsCutofTimeTo = clearDate + " " + col["endHourSameDayClearing"] + ":" + col["endMinSameDayClearing"] + ":000";
            cutofTime.clsActivate = col["cutOffActivateSameDay"];
            cutofTime.ClearingType = "05";

            cutofTimes.Add(cutofTime);
            cutofTime = new CutofTime();
            cutofTime.clsCutofTimeFrom = clearDate + " " + col["startHourDollarClearing"] + ":" + col["startMinDollarClearing"] + ":000";
            cutofTime.clsCutofTimeTo = clearDate + " " + col["endHourDollarClearing"] + ":" + col["endMinDollarClearing"] + ":000";
            cutofTime.clsActivate = col["cutOffActivateDollarClearing"];
            cutofTime.ClearingType = "20";

            cutofTimes.Add(cutofTime);




            branchActivationDao.DeleteCheckActivation();
            branchActivationDao.InsertChequeActivation(col, CurrentUser.Account.UserId, CurrentUser.Account.BankCode);


            for (int i = 0; i < cutofTimes.Count(); i++)
            {
                branchActivationDao.InsertCutOffTime(cutofTimes[i].clsActivate, cutofTimes[i].clsCutofTimeFrom, cutofTimes[i].clsCutofTimeTo, CurrentUser.Account.UserId, cutofTimes[i].ClearingType);
            }

            TempData["Notice"] = Locale.SuccessfullyUpdated;


            return RedirectToAction("Index");

        }
        protected class CutofTime
        {
            public string clsCutofTimeTo{ get; set; }
            public string clsActivate{ get; set; }
            public string clsCutofTimeFrom{ get; set; }
            public string ClearingType{ get; set; }
        }
    }
}