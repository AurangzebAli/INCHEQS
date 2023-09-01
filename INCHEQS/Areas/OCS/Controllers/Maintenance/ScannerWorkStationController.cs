using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Areas.OCS.Models.ScannerWorkStation;
using INCHEQS.Resources;
using INCHEQS.Security;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;


namespace INCHEQS.Areas.OCS.Controllers.Maintenance
{
    public class ScannerWorkStationController : BaseController
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(ScannerWorkStationController));
        private IScannerWorkStationDao ScannerWorkStationDao;
        private IPageConfigDaoOCS pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private ISystemProfileDao systemProfileDao;
        private readonly IAuditTrailDao auditTrailDao;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;
        public ScannerWorkStationController(IScannerWorkStationDao ScannerWorkStationDao, IPageConfigDaoOCS pageConfigDao, ISearchPageService searchPageService, ISystemProfileDao systemProfileDao, IAuditTrailDao audilTrailDao, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.pageConfigDao = pageConfigDao;
            this.ScannerWorkStationDao = ScannerWorkStationDao;
            this.searchPageService = searchPageService;
            this.systemProfileDao = systemProfileDao;
            this.auditTrailDao = audilTrailDao;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.ScannerWorkStation.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            // ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ScannerWorkStation.INDEX, "view_scannerworkstation", "fldscannerid", ""),
            //collection);

            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.ScannerWorkStation.INDEX, "view_scannerworkstation", "fldscannerid", "", new[] {
                    new SqlParameter("@fldBankCode",CurrentUser.Account.BankCode)}),
           collection);

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.EDIT)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Details(FormCollection collection, string ScannerParam = null)
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(collection);
            string scanner = "";
            string scannerType = "";
            string BranchId = "";

            if (string.IsNullOrEmpty(ScannerParam))
            {
                scanner = filter["fldscannerid"].Trim();
                scannerType = filter["fldscannertypenumber"].Trim();
                BranchId = filter["fldbranchid"].Trim();
            }
            else
            {
                
                scanner = ScannerWorkStationDao.getBetween(ScannerParam, "cb", "si");
                scannerType = ScannerWorkStationDao.getBetween(ScannerParam, "si", "cd");
                BranchId = ScannerWorkStationDao.getBetween(ScannerParam, "cd", "ed");


            }

            DataTable result = await ScannerWorkStationDao.FindAsync(scanner, scannerType, BranchId);

            if (result.Rows.Count > 0)
            {
                ViewBag.Scanner = result.Rows[0];
                ViewBag.ScannerId = ViewBag.Scanner["fldscannerid"];
                ViewBag.ScannerType = ViewBag.Scanner["fldscannertypeid"];
                ViewBag.BranchId = ViewBag.Scanner["fldbranchid"];
                ViewBag.Mac1 = ViewBag.Scanner["fldmacaddress1"];
                ViewBag.Mac2 = ViewBag.Scanner["fldmacaddress2"];
                ViewBag.Mac3 = ViewBag.Scanner["fldmacaddress3"];

                if (ViewBag.Scanner["fldactive"] == "Y")
                {
                    @ViewBag.Active = "checked";
                }
                else
                {
                    @ViewBag.Active = "";
                }

                ViewBag.BankCode = ViewBag.Scanner["fldbankcode"];
                ViewBag.BranchCode = ViewBag.Scanner["fldbranchcode"];
                ViewBag.LocationCode = ViewBag.Scanner["fldlocationcode"];
                ViewBag.ScannerDesc = ViewBag.Scanner["fldDesc"];
                ViewBag.ScannerLocation = ViewBag.Scanner["fldlocationdesc"];
                ViewBag.ScannerBranchDesc = ViewBag.Scanner["fldbranchdesc"];
                ViewBag.ScannerName = await ScannerWorkStationDao.getScannerNameAsync();
                ViewBag.BankDetails = await ScannerWorkStationDao.getBankDetailsAsync();
                ViewBag.LocationDetails = await ScannerWorkStationDao.getLocationDetailsAsync();
                ViewBag.BranchDetails = await ScannerWorkStationDao.getBranchDetailsAsync(CurrentUser.Account);

                int ScannerTypeID = ViewBag.ScannerType;
                DataTable scannername = await ScannerWorkStationDao.getScannerBrandNameAsync(ScannerTypeID);

                if (scannername.Rows.Count > 0)
                {
                    ViewBag.ScannerBranchName = scannername.Rows[0]["fldscannertype"];
                }
            }

            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.UPDATE)]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ScannerWorkStation.UPDATE;

            try
            {

                List<String> errorMessages = ScannerWorkStationDao.Validate(col, "update");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                    return RedirectToAction("Details", new { ScannerParam = "cb" + col["scannerId"].ToString() + "si" + col["scannerType"].ToString() + "cd" + col["branchId"].ToString() + "ed" });

                }
                else
                {

                    if ("N".Equals(systemProfile))
                    {
                        ScannerWorkStationModel before = ScannerWorkStationDao.GetScannerWorkStationData(col["ScannerId"]);
                        //auditTrailDao.Log("Edit - WorkStation Scanner - Before Update => - Scanner Name : " + before.ScannerBrandName + ", Branch ID : " + before.BranchId + ", MAC Address1 : " + before.MacAdd1 + ", MAC Address2 : " + before.MacAdd2 + ", MAC Address3 : " + before.MacAdd3, CurrentUser.Account);

                        ScannerWorkStationDao.UpdateScannerWorkStation(col);
                        TempData["Notice"] = Locale.SuccessfullyUpdated;

                        ScannerWorkStationModel after = ScannerWorkStationDao.GetScannerWorkStationData(col["ScannerId"]);
                       // auditTrailDao.Log("Edit - WorkStation Scanner - After Update => - Scanner Name : " + after.ScannerBrandName + ", Branch ID : " + after.BranchId + ", MAC Address1 : " + after.MacAdd1 + ", MAC Address2 : " + after.MacAdd2 + ", MAC Address3 : " + after.MacAdd3, CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_EditTemplate(col["scannerId1"], before, after, "Edit");
                        auditTrailDao.SecurityLog("Edit Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsScannerWorkStationExist = ScannerWorkStationDao.CheckScannerWorkStationTempById(col["ScannerId"]);
                        if (IsScannerWorkStationExist == true)
                        {
                            TempData["Warning"] = Locale.ScannerWorkStationAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            ScannerWorkStationModel before = ScannerWorkStationDao.GetScannerWorkStationData(col["ScannerId"]);

                            //ScannerWorkStationModel objScannerWorkStationUpdated = ScannerWorkStationDao.GetScannerWorkStationData(col["ScannerId"]);
                            ScannerWorkStationDao.CreateScannerWorkStationinTemptoUpdate(col);

                            ScannerWorkStationModel after = MaintenanceAuditLogDao.GetScannerTempbyId(col["ScannerId"]);

                            TempData["Notice"] = Locale.ScannerWorkStationUpdateVerify;
                            //auditTrailDao.Log("Add ScannerWorkStation into Temporary record to Update - ScannerWorkStation Id : " + col["scannerId"] + ", Mac Address 1 : " + col["MacAdd1"] + ", Mac Address 2 : " + col["MacAdd2"] + ", Mac Address 3 : : " + col["MacAdd3"], CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_EditTemplate(col["scannerId"], before, after, "Edit");
                            auditTrailDao.SecurityLog("Edit Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                    }
                }
                //return RedirectToAction("DETAILS", new { returnCodeParam = col["scannerId"].Trim() /*+ "_" + CurrentUser.Account.BankCode*/ });
                //return RedirectToAction("Index");
                return RedirectToAction("Details", new { ScannerParam = "cb" + col["scannerId"].ToString() + "si" + col["scannerType"].ToString() + "cd" + col["branchId"].ToString() + "ed" });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.CREATE)]
        [GenericFilter(AllowHttpGet = true)]
        public async Task<ActionResult> Create()
        {

            //bool IsScannerWorkStationTempExist = ScannerWorkStationDao.CheckScannerWorkStationTempExistByApproveStatus();
            //bool IsScannerWorkStationExist = ScannerWorkStationDao.CheckScannerWorkStation();

            //if (IsScannerWorkStationTempExist == true)
            //{
            //    ViewBag.ScannerId = ScannerWorkStationDao.getScannerIdFromTemp();
            //}
            //else
            //{
            //    if (IsScannerWorkStationExist == true)
            //    {
            //        ViewBag.ScannerId = ScannerWorkStationDao.getScannerId();
            //    }
            //    else
            //    {
            //        ViewBag.ScannerId = "1";
            //    }

            //}

            ViewBag.ScannerId = ScannerWorkStationDao.GetMaxScannerID();
            ViewBag.ScannerName = await ScannerWorkStationDao.getScannerNameAsync();
            ViewBag.BankDetails = await ScannerWorkStationDao.getBankDetailsAsync();
            ViewBag.LocationDetails = await ScannerWorkStationDao.getLocationDetailsAsync();
            ViewBag.BranchDetails = await ScannerWorkStationDao.getBranchDetailsAsync(CurrentUser.Account);
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.SAVECREATE)]
        [HttpPost()]
        public ActionResult SaveCreate(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ScannerWorkStation.SAVECREATE;

            try
            {
                List<string> errorMessages = ScannerWorkStationDao.Validate(col, "create");

                if ((errorMessages.Count > 0))
                {
                    TempData["ErrorMsg"] = errorMessages;
                }
                else
                {
                    if ("N".Equals(systemProfile))
                    {
                        ScannerWorkStationDao.CreateScannerWorkStation(col, CurrentUser.Account);
                        TempData["Notice"] = Locale.SuccessfullyCreated;
                        //auditTrailDao.Log("Add - Scanner Name : " + col["scannerType"] + ", Branch ID : " + col["branchId"] + ", MAC Address1 : " + col["macAdd1"] + ", MAC Address2 : " + col["macAdd2"] + ", MAC Address3 : " + col["macAdd3"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_AddTemplate(col["scannerId1"], "Add", "N");
                        auditTrailDao.SecurityLog("Add Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                    else
                    {
                        ScannerWorkStationDao.CreateInScannerWorkStationTemp(col);
                        TempData["Notice"] = Locale.ScannerWorkStationCreateVerify;
                        //auditTrailDao.Log("Add into Temporary Record to Create - Scanner Name : " + col["scannerType"] + ", Branch ID : " + col["branchId"] + ", MAC Address1 : " + col["macAdd1"] + ", MAC Address2 : " + col["macAdd2"] + ", MAC Address3 : " + col["macAdd3"], CurrentUser.Account);

                        string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_AddTemplate(col["scannerId1"], "Add", "Y");
                        auditTrailDao.SecurityLog("Add Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);
                    }
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return RedirectToAction("Create");
        }

        [CustomAuthorize(TaskIds = TaskIds.ScannerWorkStation.DELETE)]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(FormCollection col)
        {
            string systemProfile = systemProfileDao.GetValueFromSystemProfile("DualApproval", CurrentUser.Account.BankCode).Trim();
            string sTaskId = TaskIds.ScannerWorkStation.DELETE;
            if (col != null & col["DeleteBox"] != null)
            {
                List<string> arrResults = col["deleteBox"].Split(',').ToList();
                foreach (string arrResult in arrResults)
                {
                    if ("N".Equals(systemProfile))
                    {
                        string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_DeleteTemplate(arrResult, "Delete");
                        auditTrailDao.SecurityLog("Delete Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);

                        ScannerWorkStationModel objScannerWorkStationDeleted = ScannerWorkStationDao.GetScannerWorkStationData(arrResult);
                        ScannerWorkStationDao.DeleteScannerWorkStation(arrResult);
                        TempData["Notice"] = Resources.Locale.SuccessfullyDeleted;
                        //auditTrailDao.Log("ScannerWorkStation Deleted - ScannerWorkStation scanner id : " + objScannerWorkStationDeleted.ScannerId + ", Mac Address 1 : " + objScannerWorkStationDeleted.MacAdd1 + ", Mac Address 2 : " + objScannerWorkStationDeleted.MacAdd2 + ", Mac Address 3 : : " + objScannerWorkStationDeleted.MacAdd3, CurrentUser.Account);
                    }
                    else
                    {
                        bool IsScannerWorkStationTempExist = ScannerWorkStationDao.CheckScannerWorkStationTempById(arrResult);
                        if (IsScannerWorkStationTempExist == true)
                        {
                            TempData["Warning"] = Locale.ScannerWorkStationAlreadyExiststoDeleteorUpdate;
                        }
                        else
                        {
                            string ActionDetails = MaintenanceAuditLogDao.TerminalScanner_DeleteTemplate(arrResult, "Delete");
                            auditTrailDao.SecurityLog("Delete Terminal Scanner", ActionDetails, sTaskId, CurrentUser.Account);

                            ScannerWorkStationModel objScannerWorkStationDeleted = ScannerWorkStationDao.GetScannerWorkStationData(arrResult);
                            ScannerWorkStationDao.CreateScannerWorkStationinTemptoDelete(arrResult);
                            TempData["Notice"] = Locale.ScannerWorkStationDeleteVerify;
                           // auditTrailDao.Log("Add ScannerWorkStation into Temporary record to Delete - ScannerWorkStation Id : " + objScannerWorkStationDeleted.ScannerId + ", Mac Address 1 : " + objScannerWorkStationDeleted.MacAdd1 + ", Mac Address 2 : " + objScannerWorkStationDeleted.MacAdd2 + ", Mac Address 3 : : " + objScannerWorkStationDeleted.MacAdd3, CurrentUser.Account);
                        }
                    }

                }
            }
            else
            {
                TempData["Warning"] = Locale.Nodatawasselected;
            }

            return RedirectToAction("Index");
        }
    }
}
