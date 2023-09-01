using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.ScannerWorkStation;
using INCHEQS.Resources;
using INCHEQS.Security;
using log4net;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public class ScannerWorkStationCheckerController : BaseController
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ScannerWorkStationController));
        private IScannerWorkStationDao ScannerWorkStationDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public ScannerWorkStationCheckerController(IScannerWorkStationDao ScannerWorkStationDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.ScannerWorkStationDao = ScannerWorkStationDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ScannerWorkStationChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIdsOCS.ScannerWorkStationChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ScannerWorkStationChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {

//            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ScannerWorkStationChecker.INDEX, "View_ScannerWorkstationChecker", "fldscannerid", ""),
//collection);

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIdsOCS.ScannerWorkStationChecker.INDEX, "View_ScannerWorkstationChecker", "", "", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
            collection);

            return View();
        }

        //[CustomAuthorize(TaskIds = TaskIdsOCS.HolidayApprovedChecker.VERIFY)]
        [CustomAuthorize(TaskIds = TaskIdsOCS.ScannerWorkStationChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.ScannerWorkStationChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                                if (action.Equals("A"))
                                {
                                    ScannerWorkStationDao.CreateScannerWorkStationinMain(id);

                                    string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_AddTemplate(id, "Approve", "Approve");
                                    auditTrailDao.SecurityLog("Approve Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                                }
                                else if (action.Equals("D"))
                                {
                                    string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_DeleteTemplate(id, "Approve");
                                    auditTrailDao.SecurityLog("Approve Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);

                                    ScannerWorkStationModel objScannerWorkStation = ScannerWorkStationDao.GetScannerWorkStationData(id);
                                    ScannerWorkStationDao.DeleteScannerWorkStation(id);
                                    //auditTrailDao.Log("Deleted ScannerWorkStation from Main - Scanner Id :" + objScannerWorkStation.ScannerId + ", Scanner Type id : " + objScannerWorkStation.ScannerTypeId + ", Scanner Brand Name : " + objScannerWorkStation.ScannerBrandName + ", Scanner MAC Address 1 : " + objScannerWorkStation.MacAdd1 + ", Scanner MAC Address 2 : " + objScannerWorkStation.MacAdd2 + ", Scanner MAC Address 3 : " + objScannerWorkStation.MacAdd3 + ", Scanner Active : " + objScannerWorkStation.Active + ", Scanner Status : " + objScannerWorkStation.Status + ", Scanner UpdateBy : " + objScannerWorkStation.UpdateBy, CurrentUser.Account);

                                }
                                else if (action.Equals("U"))
                                {
                                    ScannerWorkStationModel before = ScannerWorkStationDao.GetScannerWorkStationData(id.ToString().PadLeft(3, '0'));
                                    //auditTrailDao.Log("ScannerWorkStation Created Before - Scanner Id :" + before.ScannerId + ", Scanner Type id : " + before.ScannerTypeId + ", Scanner Brand Name : " + before.ScannerBrandName + ", Scanner MAC Address 1 : " + before.MacAdd1 + ", Scanner MAC Address 2 : " + before.MacAdd2 + ", Scanner MAC Address 3 : " + before.MacAdd3 + ", Scanner Active : " + before.Active + ", Scanner Status : " + before.Status + ", Scanner UpdateBy : " + before.UpdateBy, CurrentUser.Account);

                                    ScannerWorkStationDao.UpdateScannerWorkStationToMain(id);

                                    ScannerWorkStationModel after = ScannerWorkStationDao.GetScannerWorkStationData(id.ToString().PadLeft(3, '0'));
                                    //auditTrailDao.Log("ScannerWorkStation Created After - Scanner Id :" + after.ScannerId + ", Scanner Type id : " + after.ScannerTypeId + ", Scanner Brand Name : " + after.ScannerBrandName + ", Scanner MAC Address 1 : " + after.MacAdd1 + ", Scanner MAC Address 2 : " + after.MacAdd2 + ", Scanner MAC Address 3 : " + after.MacAdd3 + ", Scanner Active : " + after.Active + ", Scanner Status : " + after.Status + ", Scanner UpdateBy : " + after.UpdateBy, CurrentUser.Account);

                                    string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_EditTemplate(id, before, after, "Approve");
                                    auditTrailDao.SecurityLog("Approve Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                                }

                                ScannerWorkStationDao.DeleteScannerWorkstationTemp(id);
                                //auditTrailDao.Log("Approve ScannerWorkStation - Task Assigment :" + taskId + " Scanner ID : " + id, CurrentUser.Account);
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyVerified;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIdsOCS.ScannerWorkStationChecker.VERIFY)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIdsOCS.ScannerWorkStationChecker.VERIFY;

                if ((col["deleteBox"]) != null)
                {
                    arrResults = col["deleteBox"].Split(',').ToList();

                    foreach (string arrResult in arrResults)
                    {
                        string action = arrResult.Substring(0, 1);
                        string taskId = arrResult.Substring(1, 6);
                        string id = arrResult.Remove(0, 7);

                        if (action.Equals("A"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            ScannerWorkStationModel before = ScannerWorkStationDao.GetScannerWorkStationData(id.ToString().PadLeft(3, '0'));

                            ScannerWorkStationModel after = MaintenanceAuditLogDao.GetScannerTempbyId(id.ToString().PadLeft(3, '0'));

                            string ActionDetails = MaintenanceAuditLogDao.TerminalScannerChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        ScannerWorkStationDao.DeleteScannerWorkstationTemp(id);
                        //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Scanner ID : " + id, CurrentUser.Account);
 
                    }
                    TempData["Notice"] = Locale.RecordsSuccsesfullyRejected;
                }
                else
                {
                    TempData["Warning"] = Locale.PleaseSelectARecord;
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}