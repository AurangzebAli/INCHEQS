using System.Web.Mvc;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.Security;
using INCHEQS.Models.FileInformation;
using System;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Processes.DataProcess;
using INCHEQS.Helpers;
//using INCHEQS.Areas.ICS.Models.DataProcess;
using INCHEQS.Resources;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Models.HostFile;
using System.Collections.Generic;
using INCHEQS.Areas.ICS.Models.HostFile;
using System.Data.SqlClient;

namespace INCHEQS.Areas.ICS.Controllers.InwardClearing.HostFiles {
    
    public class GenerateDebitFileController : HostFileBaseController {
        
        protected override PageSqlConfig setupPageSqlConfig() {
            string taskId = RequestHelper.PersistQueryStringForActions(ControllerContext, "tId");

            return new PageSqlConfig(taskId, "", "", "fldBankCode=@fldBankCode", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)});
        }

        public GenerateDebitFileController(IPageConfigDao pageConfigDao , 
            IDataProcessDao dataProcessDao , IFileManagerDao fileManagerDao, IAuditTrailDao auditTrailDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IHostFileDao hostFileDao) : base(pageConfigDao, dataProcessDao, fileManagerDao, auditTrailDao, searchPageService, systemProfileDao, hostFileDao) {
        }


        public JsonResult GenerateDebitFile(FormCollection collection) {
            initializeBeforeAction();

            string notice = "";
            try {
                string processName = gHostFileModel.fldProcessName;
                string posPayType = gHostFileModel.fldPosPayType; //collection["fldFileType"];
                string clearDate = collection["fldClearDate"];
                string reUpload = "N";
                string taskId = pageSqlConfig.TaskId;
                string batchId = "";
                string fileExt = gHostFileModel.fldFileExt;

                bool runningProcess = dataProcessDao.CheckRunningProcessWithoutPosPayType(processName, clearDate, CurrentUser.Account.BankCode);
                if (runningProcess) {
                    
                    //Delete previous data process
                    dataProcessDao.DeleteDataProcessWithoutPosPayType(processName, CurrentUser.Account.BankCode);
                    //Insert new data process
                    dataProcessDao.InsertToDataProcess(CurrentUser.Account.BankCode, processName, posPayType, clearDate, reUpload, taskId, batchId, CurrentUser.Account.UserId, CurrentUser.Account.UserId);

                    auditTrailDao.Log("Generate Debit File - Date : " + clearDate + ". TaskId :" + pageSqlConfig.TaskId, CurrentUser.Account);
                    notice = Locale.GeneratingData;
                } else {
                    notice = Locale.CurrentlyRunningPleaseWaitAMoment;
                }
            } catch (Exception ex) {
                systemProfileDao.Log("GenerateDebitFIleController/GenerateDebitFile error :" + ex.Message, CurrentUser.Account.Logindicator, CurrentUser.Account.LogPath, CurrentUser.Account.UserId, CurrentUser.Account.UserAbbr);
                throw ex;
            }
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        //public virtual ActionResult GetFilemanager(FormCollection collection)
        //{
        //    initializeBeforeAction();

        //    Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);

        //    // Show the file manager lsit
        //    ViewBag.HostFileResult = hostFileDao.getFilemanager(filter["fldClearDate"], pageSqlConfig.TaskId, CurrentUser.Account.BankCode);

        //    return View();
        //}


        public virtual JsonResult ClearRemark(FormCollection collection) {
            initializeBeforeAction();
            fileManagerDao.ClearRemarks(searchPageService.GetFormFiltersFromRow(collection)["fldFileName"]);
            string notice = Locale.RemarkCleared;
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult OutwardReturnDetail(FormCollection collection) {
            initializeBeforeAction();

            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);

            ViewBag.OutwardReturnDetail = hostFileDao.GetOutwardReturnDetail(filter["fldClearDate"], filter["fldDataFileName"]);
            return View();
        }
        public JsonResult OutwardReturnRegenerate(FormCollection collection)
        {
            initializeBeforeAction();
            string notice = "Outward ICL is regenerate";
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            hostFileDao.OutwardReturnDetailRegen(filter["fldClearDate"], filter["fldDataFileName"]);
            return Json(new { notice = notice }, JsonRequestBehavior.AllowGet);
        }

    }
}