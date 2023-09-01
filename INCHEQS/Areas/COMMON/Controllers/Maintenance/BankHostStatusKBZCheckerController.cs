using INCHEQS.Areas.ICS.Models.HostReturnReason;
using INCHEQS.Security.SystemProfile;
using INCHEQS.Security.AuditTrail;
using INCHEQS.Models.SearchPageConfig;
using INCHEQS.Models.SearchPageConfig.Services;
using INCHEQS.TaskAssignment;
using INCHEQS.Resources;
using INCHEQS.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using INCHEQS.Areas.COMMON.Models.BankHostStatusKBZ;
using INCHEQS.Areas.COMMON.Models.SecurityAuditTemplates.Maintenance;

namespace INCHEQS.Areas.COMMON.Controllers.Maintenance
{
    public class BankHostStatusKBZCheckerController : BaseController
    {

        private readonly ISystemProfileDao systemProfileDao;
        private IBankHostStatusKBZDao bankHostStatusKBZDao;
        private readonly IAuditTrailDao auditTrailDao;
        private IPageConfigDao pageConfigDao;
        protected readonly ISearchPageService searchPageService;
        private readonly IMaintenanceAuditLogDao MaintenanceAuditLogDao;

        public BankHostStatusKBZCheckerController(ISystemProfileDao systemProfileDao, IBankHostStatusKBZDao bankHostStatusKBZDao, IAuditTrailDao auditTrailDao, IPageConfigDao pageConfigDao, ISearchPageService searchPageService, IMaintenanceAuditLogDao MaintenanceAuditLogDao)
        {
            this.systemProfileDao = systemProfileDao;
            this.pageConfigDao = pageConfigDao;
            this.bankHostStatusKBZDao = bankHostStatusKBZDao;
            this.auditTrailDao = auditTrailDao;
            this.searchPageService = searchPageService;
            this.MaintenanceAuditLogDao = MaintenanceAuditLogDao;
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Index()
        {
            ViewBag.SearchPage = pageConfigDao.GetSearchFormModelFromConfig(CurrentUser.Account, new PageSqlConfig(TaskIds.BankHostStatusKBZChecker.INDEX));
            return View();
        }

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public virtual ActionResult SearchResultPage(FormCollection collection)
        {
            ViewBag.SearchResult = pageConfigDao.getResultListFromDatabaseView(new PageSqlConfig(TaskIds.BankHostStatusKBZChecker.INDEX, "View_BankHostStatusMasterMABChecker", "fldBankHostStatusCode"),
            collection);
            return View();
        }


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [GenericFilter(AllowHttpGet = true)]
        public ActionResult Edit(FormCollection col, string statusCodeParam = "")
        {
            Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
            string strStatusCode = "";
            if (string.IsNullOrEmpty(statusCodeParam))
            {
                strStatusCode = filter["fldBankHostStatusCode"].Trim();
            }
            else
            {
                strStatusCode = statusCodeParam;
            }

            //ViewBag.PageTitle = bankHostStatusKBZDao.GetPageTitle(TaskIds.BankHostStatusChecker.INDEX);


            ViewBag.BankHostStatusKBZChecker = bankHostStatusKBZDao.GetBankHostStatusMasterKBZTemp(strStatusCode);
            ViewBag.BankHostStatusKBZ = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(strStatusCode);

            ViewBag.HostStatusAction = bankHostStatusKBZDao.ListHostStatusAction();
            return View();
        }


        [HttpPost]
        public ActionResult VerifyA(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.BankHostStatusKBZChecker.INDEX;

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
                            bankHostStatusKBZDao.CreateBankHostStatusCodeinMainKBZ(id);
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_AddTemplate(id, "Approve", "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            BankHostStatusKBZModel objBankHostCode = bankHostStatusKBZDao.GetBankHostStatusCodeData(id);
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(id, "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                            bankHostStatusKBZDao.DeleteInBankHostStatusCodeKBZ(id);
                            //auditTrailDao.Log("Deleted BankHostCode from Main - Bank Host Status Code :" + objBankHostCode.fldBankHostStatusCode + ", Bank Host Status Desc: " + objBankHostCode.fldBankHostStatusDesc + ", Bank Host Status Action : " + objBankHostCode.fldBankHostStatusAction + ",Reject Code " + objBankHostCode.fldrejectcode + ", UpdateBy : " + objBankHostCode.fldUpdateUserId, CurrentUser.Account);

                        }
                        else if (action.Equals("U"))
                        {
                            BankHostStatusKBZModel before = bankHostStatusKBZDao.GetBankHostStatusCodeData(id);
                            //auditTrailDao.Log("Deleted BankHostCode Created Before - Bank Host Status Code :" + before.fldBankHostStatusCode + ", Bank Host Status Desc: " + before.fldBankHostStatusDesc + ", Bank Host Status Action  : " + before.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + before.fldUpdateUserId, CurrentUser.Account);

                            bankHostStatusKBZDao.UpdateBankHostStatusCodeToMainById(id);

                            BankHostStatusKBZModel after = bankHostStatusKBZDao.GetBankHostStatusCodeData(id);
                            //auditTrailDao.Log("Deleted BankHostCode - Bank Host Status Code :" + after.fldBankHostStatusCode + ", Bank Host Status Desc: " + after.fldBankHostStatusDesc + ", Bank Host Status Action : " + after.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + after.fldUpdateUserId, CurrentUser.Account);

                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_EditTemplate(id, before, after, "Approve");
                            auditTrailDao.SecurityLog("Approve Bank Host Status KBZ", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        bankHostStatusKBZDao.DeleteBankHostStatusCodeinTempKBZ(id);
                        //auditTrailDao.Log("Approve BankHostCode - Task Assigment :" + taskId + " Bank HostStatus Code : " + id, CurrentUser.Account);

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

        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [HttpPost]
        public ActionResult VerifyR(FormCollection col)
        {
            try
            {
                //string formAction = col["formAction"];
                List<string> arrResults = new List<string>();
                string sTaskId = TaskIds.BankHostStatusKBZChecker.INDEX;

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
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_AddTemplate(id, "Reject", "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("D"))
                        {
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(id, "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                        }
                        else if (action.Equals("U"))
                        {
                            BankHostStatusKBZModel before = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(id);
                            BankHostStatusKBZModel after = bankHostStatusKBZDao.GetBankHostStatusMasterKBZTemp(id);
                            string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_EditTemplate(id, before, after, "Reject");
                            auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                        }

                        bankHostStatusKBZDao.DeleteBankHostStatusCodeinTempKBZ(id);
                        //auditTrailDao.Log("Reject Update, Delete or Created New - Task Assigment :" + taskId + "Bank Host Status Code : " + id, CurrentUser.Account);

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


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [HttpPost]
        public ActionResult VerifyA2(FormCollection col, string statusCodeParam = "")
        {

            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                string sTaskId = TaskIds.BankHostStatusKBZChecker.INDEX;
                string strStatusCode = "";
                if (string.IsNullOrEmpty(statusCodeParam))
                {
                    strStatusCode = filter["fldBankHostStatusCode"].Trim();
                }
                else
                {
                    strStatusCode = statusCodeParam;
                }
                ViewBag.BankHostStatusKBZTemp = MaintenanceAuditLogDao.GetStatus(strStatusCode);
                String action = ViewBag.BankHostStatusKBZTemp.statusCode;
                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_AddTemplate(strStatusCode, "Approve", "Approve2");
                    auditTrailDao.SecurityLog("Approve Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(strStatusCode, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BankHostStatusKBZModel beforeEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(strStatusCode);
                    BankHostStatusKBZModel afterEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZTemp(strStatusCode);
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_EditTemplate(strStatusCode, beforeEdit, afterEdit, "Approve");
                    auditTrailDao.SecurityLog("Approve Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }

                BankHostStatusKBZModel before = bankHostStatusKBZDao.GetBankHostStatusCodeData(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode Created Before - Bank Host Status Code :" + before.fldBankHostStatusCode + ", Bank Host Status Desc: " + before.fldBankHostStatusDesc + ", Bank Host Status Action  : " + before.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + before.fldUpdateUserId, CurrentUser.Account);

                bankHostStatusKBZDao.DeleteInBankHostStatusCodeKBZ(strStatusCode);
                bankHostStatusKBZDao.CreateBankHostStatusCodeinMainKBZ(strStatusCode);
                bankHostStatusKBZDao.DeleteBankHostStatusCodeinTempKBZ(strStatusCode);

                BankHostStatusKBZModel after = bankHostStatusKBZDao.GetBankHostStatusCodeData(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode - Bank Host Status Code :" + after.fldBankHostStatusCode + ", Bank Host Status Desc: " + after.fldBankHostStatusDesc + ", Bank Host Status Action : " + after.fldBankHostStatusAction + ",Reject Code " + before.fldrejectcode + ", UpdateBy : " + after.fldUpdateUserId, CurrentUser.Account);

                TempData["Notice"] = "Record(s) successfully approved";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        [CustomAuthorize(TaskIds = TaskIds.BankHostStatusKBZChecker.INDEX)]
        [HttpPost]
        public ActionResult VerifyR2(FormCollection col, string statusCodeParam = "")
        {
            try
            {
                Dictionary<string, string> filter = searchPageService.GetFormFiltersFromRow(col);
                string sTaskId = TaskIds.BankHostStatusKBZChecker.INDEX;
                string strStatusCode = "";
                if (string.IsNullOrEmpty(statusCodeParam))
                {
                    strStatusCode = filter["fldBankHostStatusCode"].Trim();
                }
                else
                {
                    strStatusCode = statusCodeParam;
                }
                ViewBag.BankHostStatusKBZTemp = MaintenanceAuditLogDao.GetStatus(strStatusCode);
                String action = ViewBag.BankHostStatusKBZTemp.statusCode;
                if (action.Equals("A"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_AddTemplate(strStatusCode, "Reject", "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("D"))
                {
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZ_DeleteTemplate(strStatusCode, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                else if (action.Equals("U"))
                {
                    BankHostStatusKBZModel beforeEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZ(strStatusCode);
                    BankHostStatusKBZModel afterEdit = bankHostStatusKBZDao.GetBankHostStatusMasterKBZTemp(strStatusCode);
                    string ActionDetails = MaintenanceAuditLogDao.BankHostStatusKBZChecker_EditTemplate(strStatusCode, beforeEdit, afterEdit, "Reject");
                    auditTrailDao.SecurityLog("Reject Bank Host Status", ActionDetails, sTaskId, CurrentUser.Account);
                }
                BankHostStatusKBZModel objBankHostCode = bankHostStatusKBZDao.GetBankHostStatusCodeData(strStatusCode);
                bankHostStatusKBZDao.DeleteBankHostStatusCodeinTempKBZ(strStatusCode);
                //auditTrailDao.Log("Deleted BankHostCode from Main - Bank Host Status Code :" + objBankHostCode.fldBankHostStatusCode + ", Bank Host Status Desc: " + objBankHostCode.fldBankHostStatusDesc + ", Bank Host Status Action : " + objBankHostCode.fldBankHostStatusAction + ",Reject Code " + objBankHostCode.fldrejectcode + ", UpdateBy : " + objBankHostCode.fldUpdateUserId, CurrentUser.Account);


                TempData["Notice"] = "Record(s) successfully rejected";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






    }
}